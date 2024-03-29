#include "pch.h"
#include "WebrtcManager.h"
#include "WebrtcManager.g.cpp"

#include <winrt/Windows.Foundation.Collections.h>
#include <api/rtc_event_log/rtc_event_log_factory.h>
#include <modules/rtp_rtcp/source/rtp_utility.h>
#include <api/audio_codecs/builtin_audio_encoder_factory.h>
#include <api/video_codecs/builtin_video_decoder_factory.h>
#include <modules/audio_mixer/audio_mixer_impl.h>
#include <modules/video_coding/codecs/h264/win/h264_mf_factory.h>
#include <common_video/include/video_frame_buffer.h>
#include <sodium/crypto_secretbox.h>
#include <libyuv/convert.h>
#include <mfapi.h>
#include <winrt/Windows.Media.Core.h>
#include <mfidl.h>
#include <ppl.h>

#pragma comment(lib, "mfreadwrite")
#pragma comment(lib, "mfplat")
#pragma comment(lib, "mfuuid")

#include "AudioSinkAnalyzer.h"
#include "AudioSourceAnalyzer.h"
#include "StreamTransport.h"
#include <wrl/client.h>

const webrtc::SdpAudioFormat g_opusFormat = { "opus", 48000, 2, { {"stereo", "1"}, {"usedtx", "1"}, {"useinbandfec", "1" } } };

const std::vector<webrtc::RtpExtension> g_rtpExtensions = {
	{"urn:ietf:params:rtp-hdrext:ssrc-audio-level", 1},
	{"https://discord.com/#rtp-hdrext/2018-07-29/speaker", 9},
	{"urn:ietf:params:rtp-hdrext:sdes:mid", 10},
	{"urn:ietf:params:rtp-hdrext:sdes:rtp-stream-id", 10},
	{"urn:ietf:params:rtp-hdrext:sdes:repaired-rtp-stream-id", 12}
};

class EmptyConfig : public webrtc::WebRtcKeyValueConfig
{
	[[nodiscard]] std::string Lookup(absl::string_view) const override
	{
		return {};
	}
};

const EmptyConfig* g_trials = new EmptyConfig();

class Webrtc::SinkInterface : public rtc::VideoSinkInterface<webrtc::VideoFrame>
{
public:
	SinkInterface() = default;


	std::unique_ptr<webrtc::VideoFrame> sample = nullptr;

	winrt::Windows::Media::Core::MediaStreamSourceSampleRequest request = nullptr;
	winrt::Windows::Media::Core::MediaStreamSourceSampleRequestDeferral defferal = nullptr;
	std::mutex mutex;
	uint64_t first_render_time = 0;

	int last_width = 0;
	int last_height = 0;

	__declspec(noinline) void GenerateSample(winrt::Windows::Media::Core::MediaStreamSourceSampleRequest const& request)
	{
		/*if (g_queue.size() < 2) return;

		auto frame = std::move(g_queue.front());
		g_queue.pop();*/
		std::lock_guard guard(this->mutex);
		if (this->sample == nullptr)
		{
			this->defferal = request.GetDeferral();
			this->request = request;
			return;
		}
		ApplySample(request, *this->sample.release());
	}

	void ApplySample(winrt::Windows::Media::Core::MediaStreamSourceSampleRequest const& request, webrtc::VideoFrame const& frame)
	{
		if (0 == this->first_render_time) this->first_render_time = frame.render_time_ms() * 10000;

		const auto frame_buffer = frame.video_frame_buffer();
		winrt::com_ptr<IMFSample> sample;
		switch (frame_buffer->type())
		{
		case webrtc::VideoFrameBuffer::Type::kI420:
		default:
			const auto i420 = frame_buffer->ToI420();

			HRESULT hr = MFCreateSample(sample.put());
			if (FAILED(hr)) {
				return;
			}

			winrt::com_ptr<IMFMediaBuffer> mediaBuffer;
			hr = MFCreate2DMediaBuffer(
				static_cast<DWORD>(frame_buffer->width()),
				static_cast<DWORD>(frame_buffer->height()),
				cricket::FOURCC_NV12,
				FALSE,
				mediaBuffer.put()
			);
			if (FAILED(hr)) {
				return;
			}

			sample->AddBuffer(mediaBuffer.get());

			const winrt::com_ptr<IMF2DBuffer2> image_buffer = mediaBuffer.as<IMF2DBuffer2>();
			if (!image_buffer) {
				return;
			}

			BYTE* destRawData;
			BYTE* discard_buffer;
			LONG pitch;
			DWORD destMediaBufferSize;

			if (FAILED(image_buffer->Lock2DSize(MF2DBuffer_LockFlags_Write, &destRawData, &pitch, &discard_buffer, &destMediaBufferSize)))
			{
				return;
			}

			uint8_t* uv_dest = destRawData + (pitch * frame_buffer->height());
			libyuv::I420ToNV12(
				i420->DataY(), i420->StrideY(),
				i420->DataU(), i420->StrideU(),
				i420->DataV(), i420->StrideV(),
				destRawData,
				pitch,
				uv_dest,
				pitch,
				frame_buffer->width(),
				frame_buffer->height()
			);

			image_buffer->Unlock2D();

			if (const winrt::com_ptr<IMFAttributes> sample_attributes = sample.as<IMFAttributes>()) {
				sample_attributes->SetUINT32(MFSampleExtension_CleanPoint, TRUE);
				sample_attributes->SetUINT32(MFSampleExtension_Discontinuity, TRUE);
			}

			constexpr auto duration = static_cast<LONGLONG>((1.0 / 30) * 1000 * 1000 * 10);
			sample->SetSampleDuration(duration);

			sample->SetSampleTime(frame.render_time_ms() * 10000 + 45 - this->first_render_time);

			if ((frame_buffer->width() != this->last_width) || (frame_buffer->height() != this->last_height)) {
				this->last_width = frame_buffer->width();
				this->last_height = frame_buffer->height();
			}

			break;
		}

		Microsoft::WRL::ComPtr<IMFMediaStreamSourceSampleRequest> spRequest;
		const auto imf_request = request.as<IMFMediaStreamSourceSampleRequest>();
		imf_request->SetSample(sample.get());
	}
	__declspec(noinline) void OnFrame(const webrtc::VideoFrame& frame) override
	{
		//g_queue.push(frame);
		std::lock_guard guard(this->mutex);
		if (this->defferal != nullptr)
		{
			ApplySample(this->request, frame);
			this->defferal.Complete();
			this->defferal = nullptr;
			this->request = nullptr;
		}
		else
		{
			this->sample = std::make_unique<webrtc::VideoFrame>(frame);
		}
	}

	void OnDiscardedFrame() override
	{

	}

};

namespace winrt::Webrtc::implementation
{
	WebrtcManager::WebrtcManager() : task_queue_factory(webrtc::CreateDefaultTaskQueueFactory()),
	                                 task_queue(rtc::TaskQueue(
		                                 this->task_queue_factory->CreateTaskQueue(
			                                 "TaskQueue", webrtc::TaskQueueFactory::Priority::NORMAL)))
	{
	}

	WebrtcManager::WebrtcManager(hstring outputDeviceId, hstring inputDeviceId) : WebrtcManager()
	{
		this->output_device_id = to_string(outputDeviceId);
		this->input_device_id = to_string(inputDeviceId);
	}	

	void WebrtcManager::Create()
	{
		this->task_queue.PostTask([this]() {
			this->SetupCall();
		});
	}

	void WebrtcManager::SetupCall()
	{
		this->audio_send_transport = std::make_unique<::Webrtc::StreamTransport>(this);
		this->video_sink_interface = new ::Webrtc::SinkInterface();
		this->CreateCall();
		this->audio_send_stream = this->CreateAudioSendStream(this->ssrc, 120);
		this->call->SignalChannelNetworkState(webrtc::MediaType::AUDIO, webrtc::NetworkState::kNetworkUp);
		this->call->SignalChannelNetworkState(webrtc::MediaType::VIDEO, webrtc::NetworkState::kNetworkUp);
	}

	void WebrtcManager::Destroy()
	{
		this->task_queue.PostTask([this] {

			for (auto const& [key, stream] : this->audio_receive_streams)
			{
				this->call->DestroyAudioReceiveStream(stream);
			}

			for (auto const& [key, stream] : this->video_receive_streams)
			{
				this->call->DestroyVideoReceiveStream(stream);
			}
			
			if (this->audio_send_stream)
				this->call->DestroyAudioSendStream(this->audio_send_stream);

			if (this->udp_socket) {
				this->udp_socket.Close();
			}

			if (this->output_stream) {
				this->output_stream.Close();
			}

			this->audio_device = nullptr;
			this->audio_processing = nullptr;

			this->connected = false;

			this->ssrc_to_create.clear();
			this->audio_receive_streams.clear();
			this->video_receive_streams.clear();

			this->call = nullptr;
			this->audio_send_stream = nullptr;
			this->output_stream = nullptr;
			this->udp_socket = nullptr;
			this->audio_send_transport = nullptr;
			this->audio_processing = nullptr;
			this->audio_decoder_factory = nullptr;
			this->audio_encoder_factory = nullptr;
			this->video_decoder_factory = nullptr;
		});

	}

	void WebrtcManager::CreateCall()
	{
		this->audio_decoder_factory = webrtc::CreateBuiltinAudioDecoderFactory();
		this->audio_encoder_factory = webrtc::CreateBuiltinAudioEncoderFactory();

		this->video_decoder_factory = std::make_unique<webrtc::H264MFDecoderFactory>();

		this->audio_processing = webrtc::AudioProcessingBuilder()
			.SetCaptureAnalyzer(std::make_unique<::Webrtc::AudioSourceAnalyzer>(this))
			.SetRenderPreProcessing(std::make_unique<::Webrtc::AudioSinkAnalyzer>(this))
			.Create();

		{
			webrtc::AudioProcessing::Config config = this->audio_processing->GetConfig();
			config.voice_detection.enabled = true;

			//config.echo_canceller.enabled = true;

			//config.noise_suppression.enabled = true;
			//config.noise_suppression.level = webrtc::AudioProcessing::Config::NoiseSuppression::Level::kHigh;

			//config.gain_controller2.enabled = true;
			//config.gain_controller2.adaptive_digital.enabled = true;
			
			this->audio_processing->ApplyConfig(config);
		}

		this->audio_device = webrtc::AudioDeviceModule::Create(webrtc::AudioDeviceModule::kPlatformDefaultAudio, task_queue_factory.get());

		webrtc::AudioState::Config stateconfig;
		stateconfig.audio_processing = this->audio_processing;
		stateconfig.audio_device_module = this->audio_device;
		stateconfig.audio_mixer = webrtc::AudioMixerImpl::Create();

		const rtc::scoped_refptr<webrtc::AudioState> audio_state = webrtc::AudioState::Create(stateconfig);

		RTC_CHECK_EQ(0, this->audio_device->Init()) << "Failed to initialize the ADM.";

		if (this->audio_device->PlayoutDevices() > 0)
		{
			uint16_t deviceIndex = GetPlaybackDeviceIndexFromId(this->output_device_id);

			if (deviceIndex == -1) deviceIndex = 0;

			if (this->audio_device->SetPlayoutDevice(deviceIndex) != 0) {
				RTC_LOG(LS_ERROR) << "Unable to set playout device.";
				return;
			}
			if (this->audio_device->InitSpeaker() != 0) {
				RTC_LOG(LS_ERROR) << "Unable to access speaker.";
			}

			// Set number of channels
			bool available = false;
			if (this->audio_device->StereoPlayoutIsAvailable(&available) != 0) {
				RTC_LOG(LS_ERROR) << "Failed to query stereo playout.";
			}
			if (this->audio_device->SetStereoPlayout(available) != 0) {
				RTC_LOG(LS_ERROR) << "Failed to set stereo playout mode.";
			}
		}

		if (this->audio_device->RecordingDevices() > 0)
		{
			int deviceIndex = GetRecordingDeviceIndexFromId(input_device_id);

			if (deviceIndex == -1) deviceIndex = 0;

			if (this->audio_device->SetRecordingDevice(deviceIndex) != 0) {
				RTC_LOG(LS_ERROR) << "Unable to set recording device.";
				return;
			}
			if (this->audio_device->InitMicrophone() != 0) {
				RTC_LOG(LS_ERROR) << "Unable to access microphone.";
			}

			// Set number of channels
			bool available = false;
			if (this->audio_device->StereoRecordingIsAvailable(&available) != 0) {
				RTC_LOG(LS_ERROR) << "Failed to query stereo recording.";
			}
			if (this->audio_device->SetStereoRecording(available) != 0) {
				RTC_LOG(LS_ERROR) << "Failed to set stereo recording mode.";
			}
		}

		// webrtc::apm_helpers::Init(stateconfig.audio_processing);
		stateconfig.audio_processing->Initialize();
		
		this->audio_device->RegisterAudioCallback(audio_state->audio_transport());

		webrtc::Call::Config config(new webrtc::RtcEventLogNull());

		config.audio_state = audio_state;
		config.audio_processing = stateconfig.audio_processing;
		config.task_queue_factory = this->task_queue_factory.get();
		config.trials = g_trials;
				
		this->call = std::unique_ptr<webrtc::Call>(webrtc::Call::Create(config));
		this->connected = true;
		for (auto const& [ssrc, speaking] : this->ssrc_to_create)
		{
			if (speaking != 0)
			{
				this->audio_receive_streams[ssrc] = this->CreateAudioReceiveStream(this->ssrc, ssrc, 120);
			}
		}
	}

	WebrtcManager::~WebrtcManager()
	{
		this->Destroy();
	}

	webrtc::AudioSendStream* WebrtcManager::CreateAudioSendStream(uint32_t ssrc, uint8_t payload_type) const
	{
		webrtc::AudioSendStream::Config config{ this->audio_send_transport.get() };
		config.rtp.ssrc = ssrc;
		config.rtp.extensions = g_rtpExtensions;
		config.encoder_factory = this->audio_encoder_factory;
		config.send_codec_spec = webrtc::AudioSendStream::Config::SendCodecSpec(payload_type, g_opusFormat);

		webrtc::AudioSendStream* audioStream = this->call->CreateAudioSendStream(config);
		audioStream->Start();
		return audioStream;
	}

	webrtc::AudioReceiveStream* WebrtcManager::CreateAudioReceiveStream(uint32_t local_ssrc, uint32_t remote_ssrc, uint8_t payload_type) const
	{
		webrtc::AudioReceiveStream::Config config;
		config.rtp.local_ssrc = local_ssrc;
		config.rtp.remote_ssrc = remote_ssrc;
		config.rtp.extensions = g_rtpExtensions;
		config.decoder_factory = this->audio_decoder_factory;
		config.decoder_map = { { payload_type, g_opusFormat } };
		config.rtcp_send_transport = this->audio_send_transport.get();

		webrtc::AudioReceiveStream* audioStream = this->call->CreateAudioReceiveStream(config);
		audioStream->Start();

		return audioStream;
	}

	void WebrtcManager::GenerateSample(Windows::Media::Core::MediaStreamSourceSampleRequest const& request)
	{
		this->video_sink_interface->GenerateSample(request);
	}

	void WebrtcManager::SetVideoStream(UINT64 userId, UINT32 ssrc)
	{
		this->task_queue.PostTask([this, userId, ssrc]() {
			if(this->video_receive_streams.find(userId) == this->video_receive_streams.end())
			{
				this->video_receive_streams[userId] = this->CreateVideoReceiveStream(this->ssrc, ssrc);
			}
			else if(ssrc == 0)
			{
				this->call->DestroyVideoReceiveStream(this->video_receive_streams[userId]);
			}
			else
			{
				// TODO: investigate if this can happen and how to deal with it.
			}
		});
	}

	webrtc::VideoReceiveStream* WebrtcManager::CreateVideoReceiveStream(uint32_t local_ssrc, uint32_t remote_ssrc)
	{
		webrtc::VideoReceiveStream::Config config(this->audio_send_transport.get());
		config.rtp.local_ssrc = local_ssrc;
		config.rtp.remote_ssrc = remote_ssrc;
		config.rtp.extensions = g_rtpExtensions;
		
		webrtc::VideoReceiveStream::Decoder H264Decoder = webrtc::VideoReceiveStream::Decoder();
		H264Decoder.decoder_factory = this->video_decoder_factory.get();
		H264Decoder.video_format = webrtc::SdpVideoFormat("H264");
		H264Decoder.payload_type = 101;		

		webrtc::VideoReceiveStream::Decoder VP8Decoder = webrtc::VideoReceiveStream::Decoder();
		VP8Decoder.decoder_factory = this->video_decoder_factory.get();
		VP8Decoder.video_format = webrtc::SdpVideoFormat("VP8");
		VP8Decoder.payload_type = 103;

		webrtc::VideoReceiveStream::Decoder VP9Decoder = webrtc::VideoReceiveStream::Decoder();
		VP9Decoder.decoder_factory = this->video_decoder_factory.get();
		VP9Decoder.video_format = webrtc::SdpVideoFormat("VP9");
		VP9Decoder.payload_type = 105;

		config.decoders = { H264Decoder, VP8Decoder, VP9Decoder };
		config.renderer = this->video_sink_interface;

		webrtc::VideoReceiveStream* audioStream = this->call->CreateVideoReceiveStream(std::move(config));
		audioStream->Start();
		
		return audioStream;
	}

	void WebrtcManager::SetSpeaking(UINT32 ssrc, int speaking)
	{
		// Todo: handle priority speaker
		if (!this->connected)
		{
			this->ssrc_to_create[ssrc] = speaking;
		}
		else
		{
			const auto it = this->audio_receive_streams.find(ssrc);
			if (speaking != 0)
			{
				if (it == this->audio_receive_streams.end())
				{
					this->audio_receive_streams[ssrc] = nullptr;
					this->task_queue.PostTask([this, ssrc]() {
						this->audio_receive_streams[ssrc] = this->CreateAudioReceiveStream(this->ssrc, ssrc, 120);
					});
				}
				else
				{
					// How?
				}
			}
			else
			{
				if (it != audio_receive_streams.end())
				{
					auto stream = it->second;
					this->task_queue.PostTask([this, stream]() {
						this->call->DestroyAudioReceiveStream(stream);
					});
					audio_receive_streams.erase(it);
				}
			}
		}
	}

	void WebrtcManager::UpdateSpeaking(bool speaking)
	{
		if (!speaking)
		{
			// Not speaking
			if (this->is_speaking)
			{
				this->frame_count++;
				if (this->frame_count >= 50) {
					this->is_speaking = false;
					this->audio_send_transport->StopSend();
					this->speaking(false);
					this->frame_count = 0;
				}
			}
		}
		else
		{
			// Are Speaking
			if (!this->is_speaking)
			{
				this->is_speaking = true;
				this->audio_send_transport->StartSend();
				this->speaking(true);
			}
		}
	}

	uint16_t WebrtcManager::GetPlaybackDeviceIndexFromId(std::string deviceId) const
	{
		for (uint16_t i = 0; i < this->audio_device->PlayoutDevices(); ++i) {

			char name[webrtc::kAdmMaxDeviceNameSize];
			char guid[webrtc::kAdmMaxGuidSize];
			this->audio_device->PlayoutDeviceName(i, name, guid);
			if (deviceId == guid)
			{
				return i;
			}
		}
		return -1;
	}

	uint16_t WebrtcManager::GetRecordingDeviceIndexFromId(std::string deviceId) const
	{
		for (uint16_t i = 0; i < this->audio_device->RecordingDevices(); ++i) {

			char name[webrtc::kAdmMaxDeviceNameSize];
			char guid[webrtc::kAdmMaxGuidSize];
			this->audio_device->RecordingDeviceName(i, name, guid);
			if (deviceId == guid)
			{
				return i;
			}
		}
		return -1;
	}

	void WebrtcManager::SetPlaybackDevice(hstring deviceId) {
		this->output_device_id = to_string(deviceId);

		if (!this->audio_device) return;

		bool wasRecording = false;

		if (this->audio_device->SpeakerIsInitialized())
		{
			wasRecording = true;
			this->task_queue.PostTask([this]() {
				this->audio_device->StopPlayout();
			});
		}

		const int target_device_index = GetPlaybackDeviceIndexFromId(this->output_device_id);

		if (target_device_index > -1)
		{
			this->audio_device->SetPlayoutDevice(target_device_index);
		}

		if (wasRecording)
		{
			this->task_queue.PostTask([this]() {
				this->audio_device->InitPlayout();
				this->audio_device->StartPlayout();
			});
		}
	}

	void WebrtcManager::SetRecordingDevice(hstring deviceId) {
		this->input_device_id = to_string(deviceId);

		if (!this->audio_device) return;

		bool wasRecording = false;

		if (this->audio_device->MicrophoneIsInitialized())
		{
			wasRecording = true;
			this->task_queue.PostTask([this]() {
				this->audio_device->StopRecording();
			});
		}

		const int target_device_index = GetRecordingDeviceIndexFromId(this->input_device_id);

		if (target_device_index > -1)
		{
			this->audio_device->SetRecordingDevice(target_device_index);
		}

		if (wasRecording)
		{
			this->task_queue.PostTask([this]() {
				this->audio_device->InitRecording();
				this->audio_device->StartRecording();
			});
		}
	}

	void WebrtcManager::SetKey(array_view<const BYTE> key)
	{
		memcpy(this->key, key.begin(), 32);
	}

	void WebrtcManager::UpdateInBytes(Windows::Foundation::Collections::IVector<float> const& data) const
	{
		this->audioInData(data);
	}

	void WebrtcManager::UpdateOutBytes(Windows::Foundation::Collections::IVector<float> const& data) const
	{
		this->audioOutData(data);
	}

	winrt::fire_and_forget WebrtcManager::Connect(hstring ip, hstring port, UINT32 ssrc)
	{
		this->Create();
		this->has_got_ip = false;
		this->ssrc = ssrc;

		this->udp_socket = Windows::Networking::Sockets::DatagramSocket();
		(void)this->udp_socket.MessageReceived({ this, &WebrtcManager::OnMessageReceived });

		co_await this->udp_socket.ConnectAsync(Windows::Networking::HostName(ip), port);

		this->output_stream = Windows::Storage::Streams::DataWriter(this->udp_socket.OutputStream());
		this->SendSelectProtocol(ssrc);
	}

	void WebrtcManager::SendSelectProtocol(UINT32 ssrc) const
	{
		std::array<uint8_t, 74> packet = {0};
		packet[0] = 0;
		packet[1] = 1;
		packet[2] = 0;
		packet[3] = 70;

		packet[4] = (ssrc >> 24) & 0xFF;
		packet[5] = (ssrc >> 16) & 0xFF;
		packet[6] = (ssrc >> 8) & 0xFF;
		packet[7] = (ssrc >> 0) & 0xFF;
		this->output_stream.WriteBytes(packet);
		(void)this->output_stream.StoreAsync();
	}
	
	void WebrtcManager::OnMessageReceived(Windows::Networking::Sockets::DatagramSocket const& sender, Windows::Networking::Sockets::DatagramSocketMessageReceivedEventArgs const& args)
	{
		auto time = rtc::TimeMicros();
		const Windows::Storage::Streams::DataReader dr = args.GetDataReader();
		const unsigned int dataLength = dr.UnconsumedBufferLength();
		if (this->has_got_ip)
		{
			std::vector<BYTE> bytes = std::vector<BYTE>(dataLength);
			dr.ReadBytes(bytes);

			BYTE nonce[24] = { 0 };

			rtc::CopyOnWriteBuffer decrypted(dataLength - 16);


			int header_size;
			webrtc::MediaType mediaType;

			const webrtc::RtpUtility::RtpHeaderParser rtp_parser(bytes.data(), dataLength);
			if (rtp_parser.RTCP())
			{
				header_size = 8;
				mediaType = webrtc::MediaType::ANY;
			}
			else
			{
				header_size = 12;
				switch(bytes[1] & 127)
				{
				case 120:
					mediaType = webrtc::MediaType::AUDIO;
					break;
				case 101:
				case 103:
				case 105:
					mediaType = webrtc::MediaType::VIDEO;
					break;
				default:
					mediaType = webrtc::MediaType::ANY;
					break;
				}
			}

			std::memcpy(nonce, bytes.data(), header_size);
			std::memcpy(decrypted.data(), bytes.data(), header_size);

			crypto_secretbox_open_easy(decrypted.data() + header_size, bytes.data() + header_size, dataLength - header_size, nonce, this->key);

			this->task_queue.PostTask([this, mediaType, decrypted, time]{
				this->call->Receiver()->DeliverPacket(mediaType, decrypted, time);
			});

		}
		else {
			this->has_got_ip = true;
			if (dr.ReadInt16() == 0x2) {
				(void)dr.ReadInt16();
				(void)dr.ReadInt32();
				std::array<BYTE, 64> bytes{};
				dr.ReadBytes(bytes);

				const std::wstring ip(std::begin(bytes), std::end(bytes));
				const USHORT port = dr.ReadUInt16();
				this->ipAndPortObtained(hstring(ip), port);
			}
		}
	}
		
#pragma region Actions
	IpAndPortObtainedDelegate WebrtcManager::IpAndPortObtained() noexcept
	{
		return this->ipAndPortObtained;
	}
	AudioOutDataDelegate WebrtcManager::AudioOutData() noexcept
	{
		return this->audioOutData;
	}
	AudioInDataDelegate WebrtcManager::AudioInData() noexcept
	{
		return this->audioInData;
	}
	SpeakingDelegate WebrtcManager::Speaking() noexcept
	{
		return this->speaking;
	}

	void WebrtcManager::IpAndPortObtained(const IpAndPortObtainedDelegate value) noexcept
	{
		this->ipAndPortObtained = value;
	}
	void WebrtcManager::AudioOutData(const AudioOutDataDelegate value) noexcept
	{
		this->audioOutData = value;
	}
	void WebrtcManager::AudioInData(const AudioInDataDelegate value) noexcept
	{
		this->audioInData = value;
	}
	void WebrtcManager::Speaking(const SpeakingDelegate value) noexcept
	{
		this->speaking = value;
	}

#pragma endregion
}