﻿using Discord_UWP.LocalModels;
using Discord_UWP.Managers;
using Discord_UWP.SharedModels;
using Microsoft.Advertising.WinRT.UI;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Background;
using Windows.Media.SpeechSynthesis;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.Foundation.Metadata;
using Windows.Foundation;
using Windows.ApplicationModel.ExtendedExecution;
using System.Threading;

using Windows.Security.Credentials;
using System.Diagnostics;
using Midgard.Collections;
using Discord_UWP.Classes;
using Discord_UWP.MarkdownTextBlock;
using Discord_UWP.SimpleClasses;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Discord_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            if (!App.IsDesktop)
            {
                TitleBarHolder.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //sideDrawer.SetupInteraction(cmdBar);
            setupArgs = e.Parameter as string;
            App.SetupMainPage += Setup;
            //PCAd.Width = MobileAd.Width = Ad.Width = 300;
            //PCAd.Height = MobileAd.Height = Ad.Height = 50;
            PCAd.ApplicationId = MobileAd.ApplicationId = Ad.ApplicationId = "9nbrwj777c8r";
            PCAd.AdUnitId = MobileAd.AdUnitId = Ad.AdUnitId = "1100023969";
            base.OnNavigatedTo(e);
            sideDrawer.SetupInteraction();
            
        }
        ScrollViewer MessageScrollviewer;
        ItemsStackPanel messageStacker;
        string setupArgs = "";



        public async void Setup(object o, EventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                   () =>
                   {
                       //Reset everything, for when accounts are being switched
                       ServerList.Items.Clear();
                       //Setup UI
                       MediumTrigger.MinWindowWidth = Storage.Settings.RespUiM;
                       LargeTrigger.MinWindowWidth = Storage.Settings.RespUiL;
                       ExtraLargeTrigger.MinWindowWidth = Storage.Settings.RespUiXl;
                       TransitionCollection collection = new TransitionCollection();
                       NavigationThemeTransition theme = new NavigationThemeTransition();
                       var info = new DrillInNavigationTransitionInfo();
                       theme.DefaultNavigationTransitionInfo = info;
                       collection.Add(theme);
                       SubFrame.ContentTransitions = collection;

                       //Setup cinematic mode
                       if (App.CinematicMode)
                       {
                           cmdBar.Visibility = Visibility.Collapsed;
                           TitleBarHolder.Visibility = Visibility.Collapsed;
                           userButton.Padding = new Thickness(0, 0, 0, 48);
                           userButton.Height = 112;
                           //ServerList.Padding = new Thickness(0, 84, 0, 48);
                           //ChannelList.Padding = new Thickness(0, 84, 0, 48);
                           ServerScrollviewer.Margin = new Thickness(0, 84, 0, 48);
                           ChannelScrollviewer.Margin = new Thickness(0, 84, 0, 0);
                           MembersListView.Margin = new Thickness(0, 48, 0, 48);

                           CinematicChannelName.Visibility = Visibility.Visible;
                           CinematicGuildName.Visibility = Visibility.Visible;
                           friendPanel.Margin = new Thickness(0, 84, 0, 0);
                           MessageList.Padding = new Thickness(0, 84, 0, 0);
                           MessageArea.Margin = new Thickness(0);
                           CinematicMask1.Visibility = Visibility.Visible;
                           CinematicMask2.Visibility = Visibility.Visible;
                           ControllerHints.Visibility = Visibility.Visible;
                           Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
                           sideDrawer.DrawOpenedLeft += SideDrawer_DrawOpenedLeft;
                           sideDrawer.DrawOpenedRight += SideDrawer_DrawOpenedRight;
                           sideDrawer.DrawsClosed += SideDrawer_DrawsClosed;
                           SubFrame.FocusDisengaged += SubFrame_FocusDisengaged;
                           userButton.IsTabStop = false;
                           Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryResizeView(new Size(960,540));
                       } else
                       {
                           ServerScrollviewer.Margin = new Thickness(0, 0, 0, 0);
                       }

                       //Setup BackButton
                       SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                       SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
                       //Setup Controller input
                       Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
                       Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;
                       Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
                       //Setup MessageList infinite scroll

                       if (!Storage.Settings.CustomBG)
                       {
                           BackgroundImage.Visibility = Visibility.Collapsed;
                       }

                       if (App.DontLogin) return;

                       //Hook up the login Event
                       App.LoggingInHandler += App_LoggingInHandlerAsync;

                       UISize.CurrentStateChanged += UISize_CurrentStateChanged;
                       //Verify if a token exists, if not navigate to login page
                       if (App.LoggedIn() == false)
                       {
                           SubFrameNavigator(typeof(LogScreen));
                           return;
                       }
                       else
                       {
                           App_LoggingInHandlerAsync(null, null);
                       }
                   });
            //LocalState.SupportedGames = await RESTCalls.GetGamelist();
        }

        private void SubFrame_FocusDisengaged(Control sender, FocusDisengagedEventArgs args)
        {
            App.SubpageClose();
        }

        private void UISize_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {

            if (e.NewState == Large || e.NewState == ExtraLarge)
            {
                if (App.ShowAds)
                {
                    PCAd.Visibility = Visibility.Visible;
                    MobileAd.Visibility = Visibility.Collapsed;
                }

                if (content.Children.Contains(cmdBar))
                {
                    content.Children.Remove(cmdBar);
                    MessageAreaCMD.Children.Add(cmdBar);
                }
                if(e.NewState == Large)
                {
                    MemberToggle.Visibility = Visibility.Visible;
                    burgerButton.Visibility = Visibility.Collapsed;
                }
                else if(e.NewState == ExtraLarge)
                {
                    MemberToggle.Visibility = Visibility.Collapsed;
                    burgerButton.Visibility = Visibility.Collapsed;
                }
                cmdBar.Background = (Brush)Application.Current.Resources["AcrylicMessageBackground"];
                cmdBarShadow.Visibility = Visibility.Visible;
            }
            else
            {
                if (MessageAreaCMD.Children.Contains(cmdBar))
                {
                    MessageAreaCMD.Children.Remove(cmdBar);
                    content.Children.Add(cmdBar);
                }
                if (App.ShowAds)
                {
                    PCAd.Visibility = Visibility.Collapsed;
                    MobileAd.Visibility = Visibility.Visible;
                }
                MemberToggle.Visibility = Visibility.Visible;
                burgerButton.Visibility = Visibility.Visible;
               
                cmdBar.Background = (Brush)Application.Current.Resources["AcrylicCommandBarBackground"];
              //  cmdBarShadow.Visibility = Visibility.Collapsed;
            }
            if(!App.ShowAds)
            {
                
                PCAd.Visibility = Visibility.Collapsed;
                MobileAd.Visibility = Visibility.Collapsed;
            }
        }

        private void ServerScrollviewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            RefreshVisibilityIndicators();
        }

        bool prevshowabove = false;
        bool prevshowbelow = false;
        private void RefreshVisibilityIndicators()
        {
            bool showabove = false;
            bool showbelow = false;

            foreach (SimpleGuild sg in ServerList.Items)
            {
                if (sg.NotificationCount > 0)
                {
                    var pos = GetVisibilityPosition((ListViewItem)ServerList.ContainerFromItem(sg), ServerScrollviewer);
                    if (pos == VisibilityPosition.Above)
                        showabove = true;
                    else if (pos == VisibilityPosition.Below)
                        showbelow = true;
                }
            }
            if (showabove && !prevshowabove)
                NewAboveIndicator.Fade(0.8f, 200).Start();
            else if (prevshowabove != showabove)
                NewAboveIndicator.Fade(0, 200).Start();
            if (showbelow && !prevshowbelow)
                NewBelowIndicator.Fade(0.8f, 200).Start();
            else if (prevshowbelow != showbelow)
                NewBelowIndicator.Fade(0, 200).Start();

            prevshowbelow = showbelow;
            prevshowabove = showabove;
        }
        enum VisibilityPosition { Visible, Above, Below, Hidden };
        private VisibilityPosition GetVisibilityPosition(FrameworkElement element, FrameworkElement container)
        {
            if (element == null || container == null)
                return VisibilityPosition.Hidden;

            if (element.Visibility != Visibility.Visible)
                return VisibilityPosition.Hidden;

            Rect elementBounds = element.TransformToVisual(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            Rect containerBounds = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            if (elementBounds.Bottom < 4)
                return VisibilityPosition.Above;
            else if (elementBounds.Top > containerBounds.Bottom - 32)
                return VisibilityPosition.Below;
            else return VisibilityPosition.Visible;
        }

        Stack<Tuple<string, string>> navigationHistory = new Stack<Tuple<string, string>>();
        Tuple<string, string> currentPage = new Tuple<string, string>(null, null);

        private void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            if (SubFrame.Visibility == Visibility.Visible && App.shareop == null) //if the app was opened as a share target, disable back navigation
            {
                App.SubpageClose();
            } else
            {
                if (navigationHistory.Count > 0)
                {
                    var page = navigationHistory.Pop();
                    if (page.Item1 != null)
                    {
                        App.NavigateToGuildChannel(page.Item1, page.Item2, null, false, true);
                    }
                    else
                    {
                        if (page.Item2 != null)
                        {
                            App.NavigateToDMChannel(page.Item2, null, false, true);
                        } else
                        {
                            App.NavigateToDMChannel(null, null, false, true);
                        }
                    }
                }
            }
        }

        bool DisableLoadingMessages;
        bool AtBottom = false;
        bool AtTop = false;
        private void MessageScrollviewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (MessageList.Items.Count > 0)
            {
                double fromTop = MessageScrollviewer.VerticalOffset;
                double fromBottom = MessageScrollviewer.ScrollableHeight - fromTop;
                if (fromTop < 100 && !DisableLoadingMessages)
                    LoadOlderMessages();
                if (fromBottom < 200 && !DisableLoadingMessages)
                    LoadNewerMessages();
            }
        }

        public void SetupEvents()
        {
            //LogOut
            App.LogOutHandler += App_LogOutHandler;
            //Navigation
            App.NavigateToGuildHandler += App_NavigateToGuildHandler;
            App.NavigateToGuildChannelHandler += App_NavigateToGuildChannelHandler;
            App.NavigateToDMChannelHandler += App_NavigateToDMChannelHandler;
            //SubPages
            App.SubpageClosedHandler += App_SubpageClosedHandler;
            App.NavigateToBugReportHandler += App_NavigateToBugReportHandler;
            App.NavigateToChannelEditHandler += App_NavigateToChannelEditHandler;
            App.NavigateToCreateBanHandler += App_NavigateToCreateBanHandler;
            App.NavigateToCreateServerHandler += App_NavigateToCreateServerHandler;
            App.NavigateToDeleteChannelHandler += App_NavigateToDeleteChannelHandler;
            App.NavigateToDeleteServerHandler += App_NavigateToDeleteServerHandler;
            App.NavigateToGuildEditHandler += App_NavigateToGuildEditHandler;
            App.NavigateToJoinServerHandler += App_NavigateToJoinServerHandler;
            App.NavigateToLeaveServerHandler += App_NavigateToLeaveServerHandler;
            App.NavigateToNicknameEditHandler += App_NavigateToNicknameEditHandler;
            App.NavigateToProfileHandler += App_NavigateToProfileHandler;
            App.OpenAttachementHandler += App_OpenAttachementHandler;
            App.NavigateToChannelTopicHandler += App_NavigateToChannelTopicHandler;
            App.NavigateToCreateChannelHandler += App_NavigateToCreateChannelHandler;
            App.NavigateToSettingsHandler += App_NavigateToSettingsHandler;
            App.NavigateToAboutHandler += App_NavigateToAboutHandler;
            App.NavigateToAddServerHandler += App_NavigateToAddServerHandler;
            App.NavigateToMessageEditorHandler += App_NavigateToMessageEditorHandler;
            App.NavigateToIAPSHandler += App_NavigateToIAPSHandler;
            //Flyouts
            App.MenuHandler += App_MenuHandler;
            App.MentionHandler += App_MentionHandler;
            App.ShowMemberFlyoutHandler += App_ShowMemberFlyoutHandler;
            //Link
            App.LinkClicked += App_LinkClicked;
            //API
            App.CreateMessageHandler += App_CreateMessageHandler;
            App.DeleteMessageHandler += App_DeleteMessageHandler;
            App.FlashMentionHandler += App_FlashMentionHandler;
            typingCooldown.Tick += TypingCooldown_Tick;
            App.StartTypingHandler += App_StartTypingHandler;
            App.AddFriendHandler += App_AddFriendHandler;
            App.BlockUserHandler += App_BlockUserHandler;
            App.MarkMessageAsReadHandler += App_MarkMessageAsReadHandler;
            App.MarkChannelAsReadHandler += App_MarkChannelAsReadHandler;
            App.MarkGuildAsReadHandler += App_MarkGuildAsReadHandler;
            App.MuteChannelHandler += App_MuteChannelHandler;
            App.MuteGuildHandler += App_MuteGuildHandler;
            App.RemoveFriendHandler += App_RemoveFriendHandler;
            App.UpdatePresenceHandler += App_UpdatePresenceHandler;
            App.VoiceConnectHandler += App_VoiceConnectHandler;
            App.GuildSyncedHandler += App_GuildSyncedHandler;
            App.PresenceUpdatedHandler += App_PresenceUpdatedHandler;
            //DM
            App.DMCreatedHandler += App_DMCreatedHandler;
            App.DMDeletedHandler += App_DMDeletedHandler;
            App.DMUpdatePosHandler += App_DMUpdatePosHandler;
            //UpdateUI
            App.ReadyRecievedHandler += App_ReadyRecievedHandler;
            App.TypingHandler += App_TypingHandler;
            App.UpdateUnreadIndicatorsHandler += App_UpdateUnreadIndicatorsHandler;
            App.UserStatusChangedHandler += App_UserStatusChangedHandler;
            //UpdateUI-Messages
            App.MessageCreatedHandler += App_MessageCreatedHandler;
            App.MessageDeletedHandler += App_MessageDeletedHandler;
            App.MessageEditedHandler += App_MessageEditedHandler;
            //UpdateUI-Channels
            App.GuildChannelCreatedHandler += App_GuildChannelCreatedHandler;
            App.GuildChannelDeletedHandler += App_GuildChannelDeletedHandler;
            App.GuildChannelUpdatedHandler += App_GuildChannelUpdatedHandler;
            //UpdateUI-Guilds
            App.GuildCreatedHandler += App_GuildCreatedHandler;
            App.GuildDeletedHandler += App_GuildDeletedHandler;
            App.GuildUpdatedHandler += App_GuildUpdatedHandler;
            //UpdateUI-Members
            App.MembersUpdatedHandler += App_MembersUpdatedHandler;

            //Auto selects
            App.SelectGuildChannelHandler += App_SelectGuildChannelHandler;

            App.ToggleCOModeHandler += App_ToggleCOModeHandler;


            App.WentOffline += App_WentOffline;
        }

        private async void Gateway_GuildMemberRemoved(object sender, Gateway.GatewayEventArgs<GuildMemberRemove> e)
        {
            if (App.CurrentGuildId != e.EventData.guildId) return;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                memberscvs.Remove(FindMember(e.EventData.User.Id));
            });
        }

        private void Gateway_GuildMemberAdded(object sender, Gateway.GatewayEventArgs<GuildMemberAdd> e)
        {
            if (App.CurrentGuildId != e.EventData.guildId) return;
            var member = e.EventData;
            Member m = new Member(new GuildMember()
            {
                Deaf = e.EventData.Deaf,
                JoinedAt = e.EventData.JoinedAt,
                Mute = e.EventData.Mute,
                Nick = e.EventData.Nick,
                Roles = e.EventData.Roles,
                User = e.EventData.User
            });

            AddToMembersCvs(m);
        }
        private async void AddToMembersCvs(Member m, bool dm = false)
        {
            if (m.Raw.Roles != null)
            {
                m.Raw.Roles = m.Raw.Roles.TakeWhile(x => LocalState.Guilds[App.CurrentGuildId].roles.ContainsKey(x)).OrderByDescending(x => LocalState.Guilds[App.CurrentGuildId].roles[x].Position);
            }

            //Set it to first Hoist Role or everyone if null
            if (dm)
                m.MemberHoistRole = new HoistRole("MEMBERS", 0, "MEMBERS", 0, -1);
            else
                m.MemberHoistRole = MemberManager.GetRole(m.Raw.Roles.FirstOrDefault(x => LocalState.Guilds[App.CurrentGuildId].roles[x].Hoist), App.CurrentGuildId);

            if (LocalState.PresenceDict.ContainsKey(m.Raw.User.Id))
            {
                m.status = LocalState.PresenceDict[m.Raw.User.Id];
            }
            else
            {
                m.status = new Presence() { Status = "offline", Game = null };
            }
            if (m.Raw.Nick != null)
                m.DisplayName = m.Raw.Nick;
            else
                m.DisplayName = m.Raw.User.Username;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    if (memberscvs != null)
                        memberscvs.Add(m);
                });
        }
        private Member FindMember(string id)
        {
            if (memberscvs != null && memberscvs.RoleIndexer.ContainsKey(id))
            {
                var key = memberscvs.RoleIndexer[id];
                Grouping<HoistRole, Member> group = null;
                foreach (var g in memberscvs)
                    if (key.Id == g.Key.Id) group = g;
                if (group != null)
                {
                    return (group.FirstOrDefault(x => x.Raw.User.Id == id));
                }
                else
                    return null;
            }
            else
                return null;
        }
        private async void Gateway_GuildMemberUpdated(object sender, Gateway.GatewayEventArgs<GuildMemberUpdate> e)
        {
            if (App.CurrentGuildId != e.EventData.guildId) return;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var member = FindMember(e.EventData.User.Id);
                if (member == null) return;
                member.Raw.Nick = e.EventData.Nick;
                if (e.EventData.Nick != null)
                {
                    member.DisplayName = e.EventData.Nick;
                }
                else
                {
                    member.DisplayName = member.Raw.User.Username;
                }
                
                member.Raw.User = e.EventData.User;
                member.Raw.Roles = e.EventData.Roles;
                // member.Raw.Nick = e.EventData.Nick;
                var previoushoistrole = new HoistRole(member.MemberHoistRole.Id, member.MemberHoistRole.Position, member.MemberHoistRole.Name, member.MemberHoistRole.Membercount, member.MemberHoistRole.Brush);
                member.MemberHoistRole = MemberManager.GetRole(e.EventData.Roles.FirstOrDefault(x => LocalState.Guilds[App.CurrentGuildId].roles[x].Hoist), App.CurrentGuildId);
                if(!member.MemberHoistRole.Equals(previoushoistrole))
                {
                    memberscvs.ChangeKey(member, previoushoistrole, member.MemberHoistRole);
                    //memberscvs.Add(member);
                }
            });

        }

        private async void App_PresenceUpdatedHandler(object sender, App.PresenceUpdatedArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (App.CurrentGuildIsDM)
                {
                    foreach(SimpleChannel channel in ChannelList.Items)
                    {
                        if (channel.UserId != null && channel.UserId == e.UserId)
                            channel.UserStatus = e.Presence;
                    }
                }
                //if the memberscvs isn't null, and either the current guild is DMs or the currentguild isn't null and contains the member
                if (memberscvs != null && (App.CurrentGuildIsDM || (App.CurrentGuildId != null && LocalState.Guilds[App.CurrentGuildId].members.ContainsKey(e.UserId))))
                {
                    var member = FindMember(e.UserId);
                    if (member == null)
                    {
                        if (App.CurrentGuildId == null) return;
                        if (e.Presence.Status == "offline") return;
                        member = new Member(LocalState.Guilds[App.CurrentGuildId].members[e.UserId]);
                        member.MemberHoistRole = MemberManager.GetRole(member.Raw.Roles.FirstOrDefault(x => LocalState.Guilds[App.CurrentGuildId].roles[x].Hoist), App.CurrentGuildId);
                        if (!string.IsNullOrEmpty(member.Raw.Nick))
                            member.DisplayName = member.Raw.Nick;
                        else
                            member.DisplayName = member.Raw.User.Username;
                        member.status = e.Presence;
                        memberscvs.Add(member);
                    }
                    else
                    {
                        member.status = e.Presence;
                    }
                }
            });
         //   var member = memberscvs.Items.FirstOrDefault();
                
           // if (memberscvs.Item)
             //   ((Member)memberscvs[e.UserId]).status = e.Presence;
          /*  await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                   () =>
                   {
                       MembersCvs.Source = memberscvs;
                   });*/
        }

        private async void App_GuildUpdatedHandler(object sender, SharedModels.Guild e)
        {
            //update localstate guilds
            LocalState.Guilds[e.Id].Raw = e;
            //update icon
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                               () =>
                               {
                                   foreach (SimpleGuild guild in ServerList.Items)
                                   {
                                       if (guild.Id == e.Id)
                                       {
                                           if (!string.IsNullOrEmpty(e.Icon))
                                               guild.ImageURL = "https://discordapp.com/api/guilds/" + e.Id + "/icons/" + e.Icon + ".jpg";

                                           else
                                               guild.ImageURL = "empty";
                                       }
                                   }
                               });
        }

        private async void App_FlashMentionHandler(object sender, EventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                               () =>
                               {
                                   if (MentionFlasherStoryboard.GetCurrentState() != ClockState.Stopped)
                                       MentionFlasherStoryboard.Stop();
                                   MentionFlasherStoryboard.Begin();
                               });
            
        }

        private async void App_GuildDeletedHandler(object sender, App.GuildDeletedArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                               () =>
                               {
                                   foreach (SimpleGuild guild in ServerList.Items)
                                   {
                                       if (guild.Id == e.GuildId)
                                       {
                                           if (App.CurrentGuildId == e.GuildId)
                                               ServerList.SelectedIndex = 0;

                                           ServerList.Items.Remove(guild);
                                           if (LocalState.Guilds.ContainsKey(e.GuildId))
                                               LocalState.Guilds.Remove(e.GuildId);
                                           break;
                                       }
                                   }
                               });
        }
        private void App_MentionHandler(object sender, App.MentionArgs e)
        {
            if (MessageBox1.Text.Trim() == "")
                MessageBox1.Text = "@" + e.Username + "#" + e.Discriminator;
            else
                MessageBox1.Text = MessageBox1.Text + " @" + e.Username + "#" + e.Discriminator;
        }

        string autoselectchannel = "";
        string autoselectchannelcontent = null;
        private void App_SelectGuildChannelHandler(object sender, App.GuildChannelSelectArgs e)
        {
            string guildid = e.GuildId;
            string channelid = e.ChannelId;
            autoselectchannelcontent = e.MessageContent;
            if (guildid == "friendrequests")
            {
                friendPanel.NavigateToFriendRequests();
            }
            else
            {
                foreach (SimpleGuild g in ServerList.Items)
                {
                    if (g.Id == guildid)
                    {
                        autoselectchannelcontent = e.MessageContent;
                        autoselectchannel = channelid;
                        ServerSelectionWasClicked = true; //It wasn't actually, hehehe. Let me teach you a lesson in trickery, this is going down in history...
                        ServerList.SelectedItem = g;
                    }
                }
            }
        }

        public void ClearData()
        {
            LocalState.CurrentUser = new SharedModels.User();
            LocalState.DMs.Clear();
            LocalState.Friends.Clear();
            LocalState.Guilds.Clear();
            LocalState.GuildSettings.Clear();
            LocalState.Notes.Clear();
            LocalState.PresenceDict.Clear();
            LocalState.RPC.Clear();
            LocalState.Typers.Clear();
            LocalState.TyperTimers.Clear();
            LocalState.VoiceDict.Clear();

            //LogOut
            App.LogOutHandler -= App_LogOutHandler;
            //Navigation
            App.NavigateToGuildHandler -= App_NavigateToGuildHandler;
            App.NavigateToGuildChannelHandler -= App_NavigateToGuildChannelHandler;
            App.NavigateToDMChannelHandler -= App_NavigateToDMChannelHandler;
            //SubPages
            App.SubpageClosedHandler -= App_SubpageClosedHandler;
            App.NavigateToBugReportHandler -= App_NavigateToBugReportHandler;
            App.NavigateToChannelEditHandler -= App_NavigateToChannelEditHandler;
            App.NavigateToCreateBanHandler -= App_NavigateToCreateBanHandler;
            App.NavigateToCreateServerHandler -= App_NavigateToCreateServerHandler;
            App.NavigateToDeleteChannelHandler -= App_NavigateToDeleteChannelHandler;
            App.NavigateToDeleteServerHandler -= App_NavigateToDeleteServerHandler;
            App.NavigateToGuildEditHandler -= App_NavigateToGuildEditHandler;
            App.NavigateToJoinServerHandler -= App_NavigateToJoinServerHandler;
            App.NavigateToLeaveServerHandler -= App_NavigateToLeaveServerHandler;
            App.NavigateToNicknameEditHandler -= App_NavigateToNicknameEditHandler;
            App.NavigateToProfileHandler -= App_NavigateToProfileHandler;
            App.OpenAttachementHandler -= App_OpenAttachementHandler;
            App.NavigateToChannelTopicHandler -= App_NavigateToChannelTopicHandler;
            App.NavigateToCreateChannelHandler -= App_NavigateToCreateChannelHandler;
            App.NavigateToSettingsHandler -= App_NavigateToSettingsHandler;
            App.NavigateToAboutHandler -= App_NavigateToAboutHandler;
            App.NavigateToAddServerHandler -= App_NavigateToAddServerHandler;
            App.NavigateToMessageEditorHandler -= App_NavigateToMessageEditorHandler;
            App.NavigateToIAPSHandler -= App_NavigateToIAPSHandler;
            //Flyouts
            App.MenuHandler -= App_MenuHandler;
            App.MentionHandler -= App_MentionHandler;
            App.ShowMemberFlyoutHandler -= App_ShowMemberFlyoutHandler;
            //Link
            App.LinkClicked -= App_LinkClicked;
            //API
            App.CreateMessageHandler -= App_CreateMessageHandler;
            App.DeleteMessageHandler -= App_DeleteMessageHandler;
            App.FlashMentionHandler -= App_FlashMentionHandler;
            typingCooldown.Tick -= TypingCooldown_Tick;
            App.StartTypingHandler -= App_StartTypingHandler;
            App.AddFriendHandler -= App_AddFriendHandler;
            App.BlockUserHandler -= App_BlockUserHandler;
            App.MarkMessageAsReadHandler -= App_MarkMessageAsReadHandler;
            App.MarkChannelAsReadHandler -= App_MarkChannelAsReadHandler;
            App.MarkGuildAsReadHandler -= App_MarkGuildAsReadHandler;
            App.MuteChannelHandler -= App_MuteChannelHandler;
            App.MuteGuildHandler -= App_MuteGuildHandler;
            App.RemoveFriendHandler -= App_RemoveFriendHandler;
            App.UpdatePresenceHandler -= App_UpdatePresenceHandler;
            App.VoiceConnectHandler -= App_VoiceConnectHandler;
            App.GuildSyncedHandler -= App_GuildSyncedHandler;
            App.PresenceUpdatedHandler -= App_PresenceUpdatedHandler;
            //DM
            App.DMCreatedHandler -= App_DMCreatedHandler;
            App.DMDeletedHandler -= App_DMDeletedHandler;
            App.DMUpdatePosHandler -= App_DMUpdatePosHandler;
            //UpdateUI
            App.ReadyRecievedHandler -= App_ReadyRecievedHandler;
            App.TypingHandler -= App_TypingHandler;
            App.UpdateUnreadIndicatorsHandler -= App_UpdateUnreadIndicatorsHandler;
            App.UserStatusChangedHandler -= App_UserStatusChangedHandler;
            //UpdateUI-Messages
            App.MessageCreatedHandler -= App_MessageCreatedHandler;
            App.MessageDeletedHandler -= App_MessageDeletedHandler;
            App.MessageEditedHandler -= App_MessageEditedHandler;
            //UpdateUI-Channels
            App.GuildChannelCreatedHandler -= App_GuildChannelCreatedHandler;
            App.GuildChannelDeletedHandler -= App_GuildChannelDeletedHandler;
            App.GuildChannelUpdatedHandler -= App_GuildChannelUpdatedHandler;
            //UpdateUI-Guilds
            App.GuildCreatedHandler -= App_GuildCreatedHandler;
            App.GuildDeletedHandler -= App_GuildDeletedHandler;
            App.GuildUpdatedHandler -= App_GuildUpdatedHandler;
            //UpdateUI-Members
            App.MembersUpdatedHandler -= App_MembersUpdatedHandler;

            //Auto selects
            App.SelectGuildChannelHandler -= App_SelectGuildChannelHandler;

            App.ToggleCOModeHandler -= App_ToggleCOModeHandler;


            App.WentOffline -= App_WentOffline;
        }

        private void App_WentOffline(object sender, EventArgs e)
        {
            SubFrameNavigator(typeof(Offline));
        }

        private ExtendedExecutionSession session = null;
        private async void BeginExtendedExecution()
        {
            ClearExtendedExecution();

            var newSession = new ExtendedExecutionSession
            {
                Reason = ExtendedExecutionReason.Unspecified,
                Description = "Periodic update of live tile"
            };
            newSession.Revoked += SessionRevoked;
            ExtendedExecutionResult result = await newSession.RequestExtensionAsync();

            switch (result)
            {
                case ExtendedExecutionResult.Allowed:
                    session = newSession;
                    Console.WriteLine("Extened execution");
                    //periodicTimer = new Timer();
                    break;

                default:
                case ExtendedExecutionResult.Denied:
                    newSession.Dispose();
                    break;
            }
        }

        void ClearExtendedExecution()
        {
            if (session != null)
            {
                session.Revoked -= SessionRevoked;
                session.Dispose();
                session = null;
            }
        }

        private async void SessionRevoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (args.Reason)
                {
                    case ExtendedExecutionRevokedReason.Resumed:
                        break;

                    case ExtendedExecutionRevokedReason.SystemPolicy:
                        break;
                }

                ClearExtendedExecution();
            });
        }

        #region AppEvents

        #region LogIn
        private async void App_LoggingInHandlerAsync(object sender, EventArgs e)
        {
            Loading.Show(false);
            SubFrameMask.Opacity = 0;
            try
            {
                await RESTCalls.SetupToken();
            }
            catch
            {
                Page.Frame.Navigate(typeof(Offline));
            }
            var credentials = Storage.PasswordVault.FindAllByResource("Token");
            AccountView.Items.Clear();
            foreach(var cred in credentials)
            {
                if(cred.UserName != Storage.Settings.DefaultAccount)
                AccountView.Items.Add(cred);
            }
            if (App.IsMobile)
            {
                TitleBarHolder.Visibility = Visibility.Collapsed;
            }
            if (App.LoggedIn())
            {
                SetupEvents();
                if (Managers.GatewayManager.Gateway != null && App.IsOnline())
                {
                    GatewayManager.StartGateway();
                    Common.LoadEmojiDawg();
                    //Debug.Write(Windows.UI.Notifications.BadgeUpdateManager.GetTemplateContent(Windows.UI.Notifications.BadgeTemplateType.BadgeNumber).GetXml());
                    BeginExtendedExecution();
                    BackgroundTaskManager.TryRegisterBackgroundTask();
                    SubFrame.Visibility = Visibility.Collapsed;
                } else
                {
                    SubFrameNavigator(typeof(Offline));
                }

            } else
            {
                SubFrameNavigator(typeof(LogScreen));
            }
        }

        public class UserLogin {
            public string Token { get; set; }
            public string Name { get; set; }
            public SolidColorBrush Foreground { get; set; }
        }
        private void RefreshLoginList()
        {
            var tokens = Storage.PasswordVault.FindAllByResource("Token");
            List<UserLogin> users = new List<UserLogin>();
            foreach(var user in tokens)
            {
                UserLogin u = new UserLogin();
                u.Name = user.UserName;
                user.RetrievePassword();
                u.Token = user.Password;
                
            }
        }
        #endregion

        #region LogOut
        private void App_LogOutHandler(object sender, EventArgs e)
        {
            PasswordCredential creds;
            try
            {
                creds = Storage.PasswordVault.Retrieve("Token", LocalState.CurrentUser.Email);
            }
            catch 
            {
                creds = Storage.PasswordVault.Retrieve("Token", "logintoken");
            }
            
            Storage.PasswordVault.Remove(creds);

            ClearData();

            SubFrameNavigator(typeof(LogScreen));
        }
        #endregion

        #region Navigation
        private void SaveDraft()
        {
            if (App.CurrentChannelId != null)
            {
                if (MessageBox1.Text == "")
                {
                    if (LocalState.Drafts.ContainsKey(App.CurrentChannelId))
                    {
                        LocalState.Drafts.Remove(App.CurrentChannelId);
                    }
                }
                else
                {
                    if (LocalState.Drafts.ContainsKey(App.CurrentChannelId))
                    {
                        LocalState.Drafts[App.CurrentChannelId] = MessageBox1.Text;
                    }
                    else
                    {
                        LocalState.Drafts.Add(App.CurrentChannelId, MessageBox1.Text);
                    }
                }
            }
        }

        private void LoadDraft()
        {
            if (App.CurrentChannelId != null && LocalState.Drafts.ContainsKey(App.CurrentChannelId))
            {
                MessageBox1.Text = LocalState.Drafts[App.CurrentChannelId];
            } else
            {
                MessageBox1.Text = "";
            }
        }

        private async void App_NavigateToGuildHandler(object sender, App.GuildNavigationArgs e)
        {
            SaveDraft();
            if(memberscvs != null)
                memberscvs.Clean();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                             () =>
                             {

                                 (ServerList.SelectedItem as SimpleGuild).IsSelected = true;
                                 MembersCvs.Source = null;
                             });
            App.CurrentGuildIsDM = e.GuildId == "@me"; //Could combine...
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                 () =>
                 {
                     foreach (SimpleGuild guild in ServerList.Items)
                     {
                         if (guild.Id == e.GuildId)
                         {
                             ServerList.SelectedItem = guild;
                             guild.IsSelected = true;
                         }
                         else
                         {
                             guild.IsSelected = false;
                         }
                     }

                     if (e.GuildId != "@me")
                     {
                         MemberToggle.Visibility = Visibility.Visible;

                         App.CurrentGuildId = e.GuildId;
                         UserDetails.Visibility = Visibility.Collapsed;
                         MemberListFull.Visibility = Visibility.Visible;
                         RenderGuildChannels();
                         if (App.ShowAds)
                         {
                             Ad.Visibility = Visibility.Visible;
                         }

                     }
                     else
                     {
                         Ad.Visibility = Visibility.Collapsed;

                         App.CurrentGuildId = null;
                         MemberToggle.Visibility = Visibility.Collapsed;
                         RenderDMChannels();
                     }

                     if (App.CurrentGuildId == null)
                     {
                         string[] channels = new string[LocalState.DMs.Count];
                         for (int x = 0; x < LocalState.DMs.Count; x++)
                         {
                             channels[x] = LocalState.DMs.Values.ToList()[x].Id;
                         }
                         GatewayManager.Gateway.SubscribeToGuild(channels);
                     }
                     else
                     {
                         string[] channels = new string[LocalState.Guilds[App.CurrentGuildId].channels.Count];
                         channels[0] = App.CurrentGuildId;
                         for (int x = 1; x < LocalState.Guilds[App.CurrentGuildId].channels.Count; x++)
                         {
                             channels[x] = LocalState.Guilds[App.CurrentGuildId].channels.Values.ToList()[x].raw.Id;
                         }

                         GatewayManager.Gateway.SubscribeToGuild(channels);
                     }
                 });
            App.UpdateUnreadIndicators();
        }
        private void App_NavigateToGuildChannelHandler(object sender, App.GuildChannelNavigationArgs e)
        {
            SaveDraft();
            if (App.CurrentGuildId == e.GuildId)
            {
                Ad.Visibility = Visibility.Collapsed;
                if (!e.OnBack)
                {
                    navigationHistory.Push(currentPage);
                }

                App.CurrentChannelId = e.ChannelId;
                App.LastReadMsgId = LocalState.RPC.ContainsKey(e.ChannelId) ? LocalState.RPC[e.ChannelId].LastMessageId : null;
                RenderMessages();
                App.MarkChannelAsRead(e.ChannelId);
                currentPage = new Tuple<string, string>(App.CurrentGuildId, App.CurrentChannelId);

                if (e.OnBack)
                {
                    foreach (SimpleChannel chn in ChannelList.Items)
                    {
                        if (chn.Id == e.ChannelId)
                        {
                            lastChangeProgrammatic = true;
                            ChannelList.SelectedItem = chn;
                        }
                    }
                }
            }
            else //Out of guild navigation
            {
                if (!e.OnBack)
                {
                    navigationHistory.Push(currentPage);
                }

                foreach (SimpleGuild guild in ServerList.Items)
                {
                    if (guild.Id == e.GuildId)
                    {
                        ServerList.SelectedItem = guild;
                    }
                }
                foreach (SimpleChannel chn in ChannelList.Items)
                {
                    if (chn.Id == e.ChannelId)
                    {
                        lastChangeProgrammatic = true;
                        ChannelList.SelectedItem = chn;
                        chn.IsSelected = true;
                    }
                    else if(chn.Type != 2)
                    {
                        chn.IsSelected = false;
                    }
                }

                App.CurrentChannelId = e.ChannelId;
                App.LastReadMsgId = LocalState.RPC[e.ChannelId].LastMessageId;
                //RenderMessages();
                App.MarkChannelAsRead(e.ChannelId);
                currentPage = new Tuple<string, string>(App.CurrentGuildId, App.CurrentChannelId);
            }
            foreach (SimpleChannel chn in ChannelList.Items)
                if (chn.Id == e.ChannelId)
                    chn.IsSelected = true;
                else if(chn.Type != 2)
                    chn.IsSelected = false;
            UpdateTyping();
            LoadDraft();
        }
        private void App_NavigateToDMChannelHandler(object sender, App.DMChannelNavigationArgs e)
        {
            autoselectchannelcontent = null;
            SaveDraft();

            if (!e.OnBack)
            {
                navigationHistory.Push(currentPage);
            }

            if (App.CurrentGuildIsDM)
            {
                App.CurrentChannelId = e.ChannelId;
                if (LocalState.RPC.ContainsKey(e.ChannelId))
                    App.LastReadMsgId = LocalState.RPC[e.ChannelId].LastMessageId;
                else
                    App.LastReadMsgId = null;
            }
            else
            {
                ServerList.SelectedIndex = 0;
                App.CurrentChannelId = e.ChannelId;
                App.CurrentGuildIsDM = true;
                App.CurrentGuildId = null;
                if (LocalState.RPC.ContainsKey(e.ChannelId))
                    App.LastReadMsgId = LocalState.RPC[e.ChannelId].LastMessageId;
                else
                    App.LastReadMsgId = null;
                RenderDMChannels();
            }

            if (LocalState.DMs[e.ChannelId].Type == 1)
            {
                UserDetails.DisplayedMember = new GuildMember() { User = LocalState.DMs[e.ChannelId].Users.FirstOrDefault() };
                UserDetails.Visibility = Visibility.Visible;
                MemberListFull.Visibility = Visibility.Collapsed;
            }
            else
            {
                RenderGroupMembers();
                UserDetails.Visibility = Visibility.Collapsed;
                MemberListFull.Visibility = Visibility.Visible;
            }

            App.MarkChannelAsRead(e.ChannelId);
            currentPage = new Tuple<string, string>(App.CurrentGuildId, App.CurrentChannelId);

            if (e.Message != null && !e.Send)
            {
                MessageBox1.Text = e.Message;
            }
            else if (e.Send && e.Message != null)
            {
                App.CreateMessage(App.CurrentChannelId, e.Message);
            }

            if (e.OnBack)
            {
                foreach (SimpleChannel chn in ChannelList.Items)
                {
                    if (chn.Id == e.ChannelId)
                    {
                        lastChangeProgrammatic = true;
                        ChannelList.SelectedItem = chn;
                    }
                }
            }

            foreach (SimpleChannel chn in ChannelList.Items)
            {
                if (chn.Id == e.ChannelId)
                {
                    chn.IsSelected = true;
                    ChannelList.SelectedItem = chn;
                }
                else
                {
                    chn.IsSelected = false;
                }
            }
            UpdateTyping();

            RenderMessages();

            LoadDraft();
        }

        #endregion

        #region SubPages
        private async void SubFrameNavigator(Type page, object args = null)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                 () =>
                 {

                     if (Storage.Settings.ExpensiveRender)
                     {
                         content.Blur(2, 300).Start();
                     }
                     SubFrame.BackStack.Clear();
                     SubFrame.Navigate(page, args);
                     SubFrameMask.Fade(0.6f, 500, 0, 0).Start();
                     SubFrame.Visibility = Visibility.Visible;
                     //SubFrame.IsFocusEngagementEnabled = true;
                     SubFrame.Focus(FocusState.Keyboard);
                     //SubFrame.IsFocusEngaged = true;
                     //((Control)FocusManager.FindFirstFocusableElement(SubFrame)).Focus(FocusState.Keyboard);
                    
                 });
        }
        private async void App_SubpageClosedHandler(object sender, EventArgs e)
        {
            if (Storage.Settings.ExpensiveRender)
            {
                content.Blur(0, 600).Start();
            }
            else
            {
                content.Blur(0, 0).Start();
            }
            await SubFrameMask.Fade(0f, 300, 0, 0).StartAsync();
            SubFrame.IsFocusEngagementEnabled = false;
            SubFrame.IsFocusEngaged = false;
            SubFrame.Content = null;
        }

        private void App_NavigateToBugReportHandler(object sender, App.BugReportNavigationArgs e)
        {
            SubFrameNavigator(typeof(SubPages.BugReport), e.Exception);
        }
        private void App_NavigateToChannelEditHandler(object sender, App.ChannelEditNavigationArgs e)
        {
            SubFrameNavigator(typeof(SubPages.EditChannel), e.ChannelId);
        }
        private void App_NavigateToCreateBanHandler(object sender, App.CreateBanNavigationArgs e)
        {
            SubFrameNavigator(typeof(SubPages.DynamicSubPage), new SubPages.SubPageData()
            {
                Message = App.GetString("/Dialogs/VerifyBan") + LocalState.Guilds[App.CurrentGuildId].members[e.UserId].User.Username + "?",
                ConfirmMessage = App.GetString("/Dialogs/Ban"),
                SubMessage = "",
                StartText = "",
                PlaceHolderText = null,
                ConfirmRed = true,
                args = new Tuple<string, string, API.Guild.Models.CreateGuildBan>(e.UserId, App.CurrentGuildId, new API.Guild.Models.CreateGuildBan() { DeleteMessageDays = 0}),
                function = RESTCalls.CreateBan
            });
        }
        private void App_NavigateToCreateServerHandler(object sender, EventArgs e)
        {
            SubFrameNavigator(typeof(SubPages.DynamicSubPage), new SubPages.SubPageData()
            {
                Message = App.GetString("/Dialogs/CreateServer"),
                StartText = "",
                PlaceHolderText = App.GetString("/Dialogs/ServerName"),
                SubMessage = App.GetString("/Dialogs/ServerGuidelinesDesc1") + App.GetString("/Dialogs/ServerGuidelinesDesc2"),
                ConfirmMessage = App.GetString("/Dialogs/Create"),
                ConfirmRed = false,
                args = new List<object>(),
                function = RESTCalls.CreateGuild
            });
        }
        private void App_NavigateToDeleteChannelHandler(object sender, App.DeleteChannelNavigationArgs e)
        {
            SubFrameNavigator(typeof(SubPages.DynamicSubPage), new SubPages.SubPageData()
            {
                Message = App.CurrentGuildIsDM
                ? App.GetString("/Dialogs/CloseDMConfirm") + LocalState.DMs[e.ChannelId].Users.FirstOrDefault().Username + "?"
                : App.GetString("/Dialogs/VerifyDelete") + LocalState.Guilds[App.CurrentGuildId].channels[e.ChannelId].raw.Name + "?",
                SubMessage = "",
                StartText = "",
                PlaceHolderText = null,
                ConfirmMessage = App.GetString("/Dialogs/Delete"),
                ConfirmRed = true,
                args = e.ChannelId,
                function = RESTCalls.DeleteChannel
            });
        }
        private void App_NavigateToDeleteServerHandler(object sender, App.DeleteServerNavigationArgs e)
        {
            SubFrameNavigator(typeof(SubPages.DynamicSubPage), new SubPages.SubPageData()
            {
                Message = App.GetString("/Dialogs/VerifyDelete") + LocalState.Guilds[e.GuildId].Raw.Name + "?",
                ConfirmMessage = App.GetString("/Dialogs/Delete"),
                SubMessage = "",
                StartText = "",
                PlaceHolderText = null,
                ConfirmRed = true,
                args = e.GuildId,
                function = RESTCalls.DeleteGuild
            });
        }
        private void App_NavigateToGuildEditHandler(object sender, App.GuildEditNavigationArgs e)
        {
            SubFrameNavigator(typeof(SubPages.EditGuild), e.GuildId);
        }
        private void App_NavigateToJoinServerHandler(object sender, string e)
        {
            SubFrameNavigator(typeof(SubPages.JoinServer), e);
        }
        private void App_NavigateToLeaveServerHandler(object sender, App.LeaverServerNavigationArgs e)
        {
            SubFrameNavigator(typeof(SubPages.DynamicSubPage), new SubPages.SubPageData()
            {
                Message = App.GetString("/Dialogs/VerifyLeave") + LocalState.Guilds[e.GuildId].Raw.Name + "?",
                ConfirmMessage = App.GetString("/Dialogs/LeaveServer"),
                SubMessage = "",
                StartText = "",
                PlaceHolderText = null,
                ConfirmRed = true,
                args = e.GuildId,
                function = RESTCalls.LeaveServer
            });
        }
        private void App_NavigateToNicknameEditHandler(object sender, App.NicknameEditNavigationArgs e)
        {
            var member = LocalState.Guilds[App.CurrentGuildId].members[e.UserId];
            var pageData = new SubPages.SubPageData()
            {
                Message = App.GetString("/Dialogs/EditNickname"),
                ConfirmMessage = App.GetString("/Dialogs/Save"),
                SubMessage = "",
                StartText = member.Nick != null ? member.Nick : "",
                ConfirmRed = false,
                PlaceHolderText = member.User.Username,
                args = e.UserId == LocalState.CurrentUser.Id ? new List<object>() { App.CurrentGuildId } : new List<object>() { App.CurrentGuildId, e.UserId },
            };
            if (e.UserId == LocalState.CurrentUser.Id)
            {
                pageData.function = RESTCalls.ModifyCurrentUserNickname;
            } else
            {
                pageData.function = RESTCalls.ModifyGuildMemberNickname;
            }
            SubFrameNavigator(typeof(SubPages.DynamicSubPage), pageData);
        }
        private void App_NavigateToProfileHandler(object sender, App.ProfileNavigationArgs e)
        {
                SubFrameNavigator(typeof(SubPages.UserProfile), e.User);
        }
        private void App_OpenAttachementHandler(object sender, SharedModels.Attachment e)
        {
            SubFrameNavigator(typeof(SubPages.PreviewAttachement), e);
        }
        private void App_NavigateToChannelTopicHandler(object sender, App.ChannelTopicNavigationArgs e)
        {
            SubFrameNavigator(typeof(SubPages.DynamicSubPage), new SubPages.SubPageData()
            {
                Message = e.Channel.Name,
                ConfirmMessage = "",
                SubMessage = e.Channel.Topic,
                StartText = "",
                PlaceHolderText = null,
                ConfirmRed = false,
                args = null,
                function = null
            });
        }
        private void App_NavigateToCreateChannelHandler(object sender, EventArgs e)
        {
            SubFrameNavigator(typeof(SubPages.CreateChannel));
        }
        private void App_NavigateToSettingsHandler(object sender, EventArgs e)
        {
            SubFrameNavigator(typeof(SubPages.Settings));
        }
        private void App_NavigateToAboutHandler(object sender, bool e)
        {
            if (!e)
            {
                SubFrameNavigator(typeof(SubPages.About));
            } else
            {
                SubFrameNavigator(typeof(SubPages.WhatsNew));
            }
        }
        private void App_NavigateToAddServerHandler(object sender, EventArgs e)
        {
            SubFrameNavigator(typeof(SubPages.AddServer));
        }
        private void App_NavigateToMessageEditorHandler(object sender, App.MessageEditorNavigationArgs e)
        {
            SubFrameNavigator(typeof(SubPages.ExtendedMessageEditor), e);
        }
        private void App_NavigateToIAPSHandler(object sender, EventArgs e)
        {
            SubFrameNavigator(typeof(SubPages.InAppPurchases));
        }
        #endregion

        #region Flyouts
        private void App_MenuHandler(object sender, App.MenuArgs e)
        {
            e.Flyout.ShowAt((sender as UIElement), e.Point);
        }

        private async void App_ShowMemberFlyoutHandler(object sender, App.ProfileNavigationArgs e)
        {
            if (!App.CurrentGuildIsDM)
            {
                var member = new GuildMember();
                if (LocalState.Guilds[App.CurrentGuildId].members.ContainsKey(e.User.Id))
                {
                    member = LocalState.Guilds[App.CurrentGuildId].members[e.User.Id];
                } else
                {
                    member = await RESTCalls.GetGuildMember(App.CurrentGuildId, e.User.Id);
                }
                if (member.User?.Id != null)
                {
                    FlyoutManager.MakeUserDetailsFlyout(member).ShowAt(sender as FrameworkElement);
                } else if(member.User != null)
                {
                    FlyoutManager.MakeUserDetailsFlyout(e.User).ShowAt(sender as FrameworkElement);
                }
            }
            else
            {
                FlyoutManager.MakeUserDetailsFlyout(e.User).ShowAt(sender as FrameworkElement);
            }
        }
        #endregion

        #region Link
        private async void App_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (e.Link.StartsWith("#"))
            {
                string val = e.Link.Remove(0, 1);
                App.NavigateToGuildChannel(App.CurrentGuildId, val);
            }
            else if (e.Link.StartsWith("@!"))
            {
                string val = e.Link.Remove(0, 2);
               
                App.ShowMemberFlyout(sender, LocalState.Guilds[App.CurrentGuildId].members[val].User);
            }
            else if (e.Link.StartsWith("@&"))
            {
                string val = e.Link.Remove(0, 2);
                //TODO Fix this shit
               // MembersListView.ScrollIntoView(memberscvs.FirstOrDefault(x => ((Member)x.Value).MemberHoistRole.Id == val));
                sideDrawer.OpenRight();
            }
            else if (e.Link.StartsWith("@"))
            {
                string val = e.Link.Remove(0, 1);
                if (App.CurrentGuildIsDM)
                {
                    App.ShowMemberFlyout(sender, LocalState.DMs[App.CurrentChannelId].Users.FirstOrDefault(x => x.Id == val));
                } else
                {
                    App.ShowMemberFlyout(sender, LocalState.Guilds[App.CurrentGuildId].members[val].User);
                }
            }
            else
            {
                await Launcher.LaunchUriAsync(new Uri(e.Link));
            }
        }
        #endregion

        #region API
        private async void App_CreateMessageHandler(object sender, App.CreateMessageArgs e)
        {
            //MessageList.Items.Add(MessageManager.MakeMessage(e.ChannelId, e.Message));
            if (e.Message.Content.Length > 10000)
            {
                //To avoid spam
                MessageDialog md = new MessageDialog("Sorry, but this message is way too long to be sent, even with Quarrel", "Over 10 000 characters?!");
                await md.ShowAsync();
                return;
            }
            else if(e.Message.Content.Length > 2000)
            {
                MessagesLoading.Visibility = Visibility.Visible;
                //Split the message into <2000 char ones and send them individually
                var split = SplitToLines(e.Message.Content, 2000);
                var splitcount = split.Count();
                if (splitcount < 10)
                {
                    for (int i = 0; i < split.Count(); i++)
                    {
                        API.Channel.Models.MessageUpsert splitmessage = new API.Channel.Models.MessageUpsert();
                        splitmessage.Content = split.ElementAt(i);
                        if (i == splitcount)
                        {
                            //if it's the last message, send the file along with it
                            splitmessage.file = e.Message.file;
                        }
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        await RESTCalls.CreateMessage(e.ChannelId, splitmessage);
                        sw.Stop();
                        if (sw.ElapsedMilliseconds < 500) //make sure to wait at least 500ms between each message
                            await Task.Delay(Convert.ToInt32((500 - sw.ElapsedMilliseconds)));
                    }
                }
                else
                {
                    MessageDialog md = new MessageDialog("Sorry, but this message is way too long to be sent, even with Quarrel", "Wait, what?!");
                    await md.ShowAsync();
                    return;
                }
                MessagesLoading.Visibility = Visibility.Collapsed;
            }
            else
            {
                //Just send the message
                await RESTCalls.CreateMessage(e.ChannelId, e.Message);
            }
            
        }

        private void App_DeleteMessageHandler(object sender, App.DeleteMessageArgs e)
        {
            SubFrameNavigator(typeof(SubPages.DynamicSubPage), new SubPages.SubPageData()
            {
                Message = App.GetString("/Dialogs/DeleteMessageConfirm"),
                ConfirmMessage = App.GetString("/Dialogs/Delete"),
                SubMessage = "", //TODO: Make this the message
                StartText = "",
                PlaceHolderText = null,
                ConfirmRed = true,
                args = new Tuple<string, string>(e.ChannelId, e.MessageId),
                function = RESTCalls.DeleteMessage
            });
        }

        private static IEnumerable<string> SplitToLines(string stringToSplit, int maxLineLength)
        {
            string[] words = stringToSplit.Split(' ');
            System.Text.StringBuilder line = new System.Text.StringBuilder();
            foreach (string word in words)
            {
                if (word.Length + line.Length <= maxLineLength)
                {
                    line.Append(word + " ");
                }
                else
                {
                    if (line.Length > 0)
                    {
                        yield return line.ToString().Trim();
                        line.Clear();
                    }
                    string overflow = word;
                    while (overflow.Length > maxLineLength)
                    {
                        yield return overflow.Substring(0, maxLineLength);
                        overflow = overflow.Substring(maxLineLength);
                    }
                    line.Append(overflow + " ");
                }
            }
            yield return line.ToString().Trim();
        }

        //The typing cooldown disables the trigger typing event from being fired if it was already triggered less than 5 seconds ago
        //This is to avoid 429 errors (too many requests) because otherwise it would fire on every letter
        //This also improves performance
        DispatcherTimer typingCooldown = new DispatcherTimer() { Interval=TimeSpan.FromSeconds(5) };
        private void TypingCooldown_Tick(object sender, object e)
        {
            typingCooldown.Stop();
        }
        private async void App_StartTypingHandler(object sender, App.StartTypingArgs e)
        {
            if (!typingCooldown.IsEnabled)
            {
                await RESTCalls.TriggerTypingIndicator(e.ChannelId);
                typingCooldown.Start();
            }
        }

        private async void App_AddFriendHandler(object sender, App.AddFriendArgs e)
        {
            await RESTCalls.SendFriendRequest(e.UserId);
        }

        private async void App_BlockUserHandler(object sender, App.BlockUserArgs e)
        {
            await RESTCalls.BlockUser(e.UserId);
        }

        private async void App_MarkMessageAsReadHandler(object sender, App.MarkMessageAsReadArgs e)
        {
            await RESTCalls.AckMessage(e.ChannelId, e.MessageId);
        }

        private async void App_MarkChannelAsReadHandler(object sender, App.MarkChannelAsReadArgs e)
        {
            //Assumes you marked it from active guild
            if (!App.CurrentGuildIsDM)
            {
                if (LocalState.Guilds[App.CurrentGuildId].channels.ContainsKey(e.ChannelId))
                {
                    if(LocalState.Guilds.ContainsKey(App.CurrentGuildId) && LocalState.Guilds[App.CurrentGuildId].channels.ContainsKey(e.ChannelId))
                        await RESTCalls.AckMessage(e.ChannelId, LocalState.Guilds[App.CurrentGuildId].channels[e.ChannelId].raw.LastMessageId);
                    //Update Unread called on Gateway Event
                }
            } else
            {
               if(LocalState.DMs.ContainsKey(e.ChannelId))
                    await RESTCalls.AckMessage(e.ChannelId, LocalState.DMs[e.ChannelId].LastMessageId);
            }
        }

        private async void App_MarkGuildAsReadHandler(object sender, App.MarkGuildAsReadArgs e)
        {
            await RESTCalls.AckGuild(e.GuildId);
            //Update Unread called on Gateway Event
        }

        private async void App_MuteChannelHandler(object sender, App.MuteChannelArgs e)
        {
            
            Dictionary<string, ChannelOverride> chns = new Dictionary<string, ChannelOverride>();
            ChannelOverride chan;
            if (!LocalState.GuildSettings[App.CurrentGuildId].channelOverrides.ContainsKey(e.ChannelId))
                LocalState.GuildSettings[App.CurrentGuildId].channelOverrides.Add(e.ChannelId, new ChannelOverride() { Channel_Id = e.ChannelId, MessageNotifications = 0, Muted = true });
            chan = LocalState.GuildSettings[App.CurrentGuildId].channelOverrides[e.ChannelId];
            chan.Channel_Id = null;
            chan.Muted = !chan.Muted;
            chns.Add(e.ChannelId, chan);

            var returned = await RESTCalls.ModifyGuildSettings(App.CurrentGuildId, new GuildSettingModify() { ChannelOverrides = chns});

            LocalState.GuildSettings[App.CurrentGuildId].raw = returned;

            foreach (var chn in returned.ChannelOverrides)
            {
                if (chn.Channel_Id == e.ChannelId)
                {
                    LocalState.GuildSettings[App.CurrentGuildId].channelOverrides[e.ChannelId] = chn;
                }
            }
            App.UpdateUnreadIndicators();
        }

        private async void App_MuteGuildHandler(object sender, App.MuteGuildArgs e)
        {
            if (!LocalState.GuildSettings.ContainsKey(e.GuildId))
                LocalState.GuildSettings.Add(e.GuildId, new LocalModels.GuildSetting(await RESTCalls.ModifyGuildSettings(e.GuildId, new SharedModels.GuildSettingModify() { Muted = true })));
            else
                LocalState.GuildSettings[e.GuildId] = new LocalModels.GuildSetting(await RESTCalls.ModifyGuildSettings(e.GuildId, new SharedModels.GuildSettingModify() { Muted = !(LocalState.GuildSettings[e.GuildId].raw.Muted) }));
            App.UpdateUnreadIndicators();
        }

        private async void App_RemoveFriendHandler(object sender, App.RemoveFriendArgs e)
        {
            await RESTCalls.RemoveFriend(e.UserId);
        }

        private async void App_UpdatePresenceHandler(object sender, App.UpdatePresenceArgs e)
        {
            await RESTCalls.ChangeUserStatus(e.Status);
        }

        private async void App_VoiceConnectHandler(object sender, App.VoiceConnectArgs e)
        {
            if (e.ChannelId != null)
            {
                VoiceController.Show();
            } else
            {
                VoiceController.Hide();
            }
            foreach(SimpleChannel chn in ChannelList.Items)
            {
                if(chn.Type == 2)
                {
                    if (e.ChannelId == chn.Id)
                        chn.IsSelected = true;
                    else
                        chn.IsSelected = false;
                }
            }
            await GatewayManager.Gateway.VoiceStatusUpdate(e.GuildId, e.ChannelId, LocalState.VoiceState.SelfMute, LocalState.VoiceState.SelfDeaf);
        }
        #endregion

        #region UpdateUI

        #region RenderElement
        public void PopulateMessageArea()
        {
            MessageList.Items.Clear();
            PinnedMessageList.Items.Clear();
            SendMessage.Visibility = Visibility.Visible;
            if (Page.ActualWidth <= 500)
            {
                //CompressedChannelHeader.Visibility = Visibility.Visible;
            }
            PinnedMessags.Visibility = Visibility.Visible;
        }
        public void ClearMessageArea()
        {
            friendPanel.Visibility = Visibility.Collapsed;
            MessageList.Items.Clear();
            MoreNewMessageIndicator.Visibility = Visibility.Collapsed;
            SendMessage.Visibility = Visibility.Collapsed;
            //CompressedChannelHeader.Visibility = Visibility.Collapsed;
            PinnedMessags.Visibility = Visibility.Collapsed;
        }

        public void RenderCurrentUser()
        {
            ImageBrush image = new ImageBrush() { ImageSource = new BitmapImage(Common.AvatarUri(LocalState.CurrentUser.Avatar, LocalState.CurrentUser.Id)) };

            if (LocalState.CurrentUser.Avatar == null)
                AvatarBG.Fill = Common.DiscriminatorColor(LocalState.CurrentUser.Discriminator);
            else
                AvatarBG.Fill = Common.GetSolidColorBrush(Windows.UI.Colors.Transparent);

            Avatar.Fill = image;
            LargeAvatar.Fill = image;
            Username.Text = LocalState.CurrentUser.Username;
            Discriminator.Text = "#" + LocalState.CurrentUser.Discriminator;
            LargeUsername.Text = Username.Text;
            LargeDiscriminator.Text = Discriminator.Text;
        }

        public void RenderGuilds()
        {
            ServerList.Items.Clear();
            SimpleGuild DM = new SimpleGuild();
            DM.Id = "@me";
            DM.Name = App.GetString("/Main/DirectMessages");
            DM.IsDM = false;
            foreach (var chn in LocalState.DMs.Values)
                if (LocalState.RPC.ContainsKey(chn.Id))
                {
                    ReadState readstate = LocalState.RPC[chn.Id];
                    DM.NotificationCount += readstate.MentionCount;
                    var StorageChannel = LocalState.DMs[chn.Id];
                    if (StorageChannel.LastMessageId != null &&
                        readstate.LastMessageId != StorageChannel.LastMessageId)
                        DM.IsUnread = true;
                }
            ServerList.Items.Add(DM);

            foreach (var guild in LocalState.Guilds.OrderBy(x => x.Value.Position))
            {
                var sg = new SimpleGuild();
                sg.Id = guild.Value.Raw.Id;
                if (guild.Value.Raw.Icon != null && guild.Value.Raw.Icon != "")
                {
                    sg.ImageURL = "https://discordapp.com/api/guilds/" + guild.Value.Raw.Id + "/icons/" + guild.Value.Raw.Icon + ".jpg";
                }
                else
                {
                    sg.ImageURL = "empty";
                }
                sg.Name = guild.Value.Raw.Name;

                sg.IsMuted = LocalState.GuildSettings.ContainsKey(guild.Key) ? LocalState.GuildSettings[guild.Key].raw.Muted : false;
                sg.IsUnread = false; //Will change if true
                foreach (var chn in guild.Value.channels.Values)
                    if (LocalState.RPC.ContainsKey(chn.raw.Id))
                    {
                        ReadState readstate = LocalState.RPC[chn.raw.Id];
                        sg.NotificationCount += readstate.MentionCount;
                        var StorageChannel = LocalState.Guilds[sg.Id].channels[chn.raw.Id].raw;
                        if (readstate.LastMessageId != StorageChannel.LastMessageId && !sg.IsMuted)
                            sg.IsUnread = true;
                    }
                sg.IsValid = guild.Value.valid;
                ServerList.Items.Add(sg);
            }
        }

        public void RenderDMChannels(string id = null)
        {
            ClearMessageArea();

            ChannelLoading.IsActive = true;
            ChannelLoading.Visibility = Visibility.Visible;

            ServerNameButton.Visibility = Visibility.Collapsed;
            FriendsItem.Visibility = Visibility.Visible;
            DirectMessageBlock.Visibility = Visibility.Visible;

            //Select FriendPanel
            if (id == null)
            {
                FriendsItem.IsSelected = true;
                friendPanel.Visibility = Visibility.Visible;
                MoreNewMessageIndicator.Visibility = Visibility.Collapsed;
            }

            AddChannelButton.Visibility = Visibility.Collapsed;
            ChannelName.Text = /*CompChannelName.Text =*/ ChannelTopic.Text = /*CompChannelTopic.Text =*/ "";

            ChannelList.Items.Clear();

            foreach (SimpleChannel channel in ChannelManager.OrderChannels(LocalState.DMs.Values.ToList()))
            {
                if (App.CurrentGuildIsDM)
                {
                    ChannelList.Items.Add(channel);
                    if (id != null && channel.Id == id)
                    {
                        ChannelList.SelectedItem = channel;
                        App.CurrentChannelId = id;
                    }

                    if (!string.IsNullOrEmpty(autoselectchannel))
                    {
                        if (channel.Id == autoselectchannel)
                        {
                            ChannelSelectionWasClicked = true; //hehe, not actually true
                            ChannelList.SelectedItem = channel;
                        }
                    }
                }
            }

            ChannelLoading.IsActive = false;
            ChannelLoading.Visibility = Visibility.Collapsed;
        }

        public void RenderGuildChannels() //App.CurrentGuildId is set
        {
            ClearMessageArea();

            ChannelLoading.IsActive = true;
            ChannelLoading.Visibility = Visibility.Visible;

            ServerNameButton.Visibility = Visibility.Visible;
            FriendsItem.Visibility = Visibility.Collapsed;
            DirectMessageBlock.Visibility = Visibility.Collapsed;
            if (LocalState.Guilds[App.CurrentGuildId].permissions.ManageChannels || LocalState.Guilds[App.CurrentGuildId].permissions.Administrator || LocalState.Guilds[App.CurrentGuildId].Raw.OwnerId == LocalState.CurrentUser.Id)
            {
                AddChannelButton.Visibility = Visibility.Visible;
            } else
            {
                AddChannelButton.Visibility = Visibility.Collapsed;
            }

            ChannelName.Text = /*CompChannelName.Text =*/ ChannelTopic.Text = /*CompChannelTopic.Text =*/ "";

            ServerName.Text = LocalState.Guilds[App.CurrentGuildId].Raw.Name;

            ChannelList.Items.Clear();

            foreach (SimpleChannel channel in ChannelManager.OrderChannels(LocalState.Guilds[App.CurrentGuildId].channels.Values.ToList()))
            {
                if (VoiceController.channelid == channel.Id)
                    channel.IsSelected = true;
                ChannelList.Items.Add(channel);
                if (!string.IsNullOrEmpty(autoselectchannel))
                {
                    if (channel.Id == autoselectchannel)
                    {
                        ChannelSelectionWasClicked = true; //hehe, not actually true
                        ChannelList.SelectedItem = channel;
                    }
                    
                }
            }

            ChannelLoading.IsActive = false;
            ChannelLoading.Visibility = Visibility.Collapsed;
        }

        private void GoToLastRead_Click(object sender, RoutedEventArgs e)
        {
            LoadMessagesAround(App.LastReadMsgId);
        }

        //bool MessageRange_LastMessage = false;
        bool outofboundsNewMessage = false;
        public async void RenderMessages() //App.CurrentChannelId is set
        {
            outofboundsNewMessage = false; //assume this for the moment
            MoreNewMessageIndicator.Visibility = Visibility.Collapsed;
            MessagesLoading.Visibility = Visibility.Visible;
            FriendsItem.IsSelected = false;
            friendPanel.Visibility = Visibility.Collapsed;
            PopulateMessageArea();

            if (UISize.CurrentState == Small)
            {
                sideDrawer.CloseLeft();
            }

            ChannelName.Text = (ChannelList.SelectedItem as SimpleChannel).Type == 0 ? "#" + (ChannelList.SelectedItem as SimpleChannel).Name : (ChannelList.SelectedItem as SimpleChannel).Name;
            //CompChannelName.Text = ChannelName.Text;
            ChannelTopic.Text = (ChannelList.SelectedItem as SimpleChannel).Type == 0 ? LocalState.Guilds[App.CurrentGuildId].channels[(ChannelList.SelectedItem as SimpleChannel).Id].raw.Topic : "";
            //CompChannelTopic.Text = ChannelTopic.Text;
            if (ChannelTopic.Text == null || ChannelTopic.Text.Trim() == "")
            {
                ChannelTopic.Visibility = Visibility.Collapsed;
                ChannelName.Margin = new Thickness(0,10,0,0);
            }
            else
            {
                ChannelTopic.Visibility = Visibility.Visible;
                ChannelName.Margin = new Thickness(0,0,0,0);
            }

            MessageList.Items.Clear();
            IEnumerable<Message> emessages = null;
            await Task.Run(async () =>
            {
                emessages = await RESTCalls.GetChannelMessages(App.CurrentChannelId);
            });
            if (emessages != null)
            {
                var messages = await MessageManager.ConvertMessage(emessages.ToList());
                AddMessages(Position.After, true, messages, true);
            } else
            {
                //TODO: Check offline status and potentially set to offline mode
            }

            IEnumerable<Message> epinnedmessages = null;
            await Task.Run(async () =>
            {
                epinnedmessages = await RESTCalls.GetChannelPinnedMessages(App.CurrentChannelId);
            });
            if (epinnedmessages != null)
            {
                var pinnedmessages = await MessageManager.ConvertMessage(epinnedmessages.ToList());
                pinnedmessages.Reverse();
                if (pinnedmessages != null)
                {
                    foreach (var message in pinnedmessages)
                    {
                        PinnedMessageList.Items.Add(message);
                    }
                }
            }
            if (PinnedMessageList.Items.Count == 0)
                NoPinnedMessages.Visibility = Visibility.Visible;
            else
                NoPinnedMessages.Visibility = Visibility.Collapsed;
            MessagesLoading.Visibility = Visibility.Collapsed;
            sideDrawer.CloseLeft();
        }

        public enum Position { Before, After };
        public async void AddMessages(Position position, bool scroll, List<MessageContainer> messages, bool showNewMessageIndicator)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ReturnToPresentIndicator.Visibility = Visibility.Collapsed;
            MoreNewMessageIndicator.Visibility = Visibility.Collapsed;
            if (messages != null && messages.Count > 0)
            {
                MessageContainer scrollTo = null;
                if (showNewMessageIndicator)
                {
                    //(MAYBE) SHOW NEW MESSAGE INDICATOR
                    var FirstMessageTime = Common.SnowflakeToTime(messages.First().Message.Id);
                    var LastMessageTime = Common.SnowflakeToTime(messages.Last().Message.Id);
                    var LastReadTime = Common.SnowflakeToTime(App.LastReadMsgId);

                    if (FirstMessageTime < LastReadTime)
                    {
                        //the last read message is after the first one in the list
                        if (LastMessageTime > LastReadTime)
                        {
                            outofboundsNewMessage = false;
                            //the last read message is before the last one in the list
                            bool CanBeLastRead = true;
                            for (int i = 0; i < messages.Count(); i++)
                            {
                                if (CanBeLastRead)
                                {
                                    //the first one with a larger timestamp gets the "NEW MESSAGES" header
                                    var CurrentMessageTime = Common.SnowflakeToTime(messages[i].Message.Id);
                                    if (CurrentMessageTime > LastReadTime)
                                    {
                                        messages[i].LastRead = true;
                                        scrollTo = messages[i];
                                        CanBeLastRead = false;
                                    }
                                }
                                if (position == Position.After)
                                    MessageList.Items.Add(messages[i]);
                                else if (position == Position.Before)
                                    MessageList.Items.Insert(i, messages[i]);
                            }
                        }
                        else
                        {
                            //The last read message is after the span of currently displayed messages
                            outofboundsNewMessage = true;
                            for (int i = 0; i < messages.Count(); i++)
                                if (position == Position.After)
                                    MessageList.Items.Add(messages[i]);
                                else if (position == Position.Before)
                                    MessageList.Items.Insert(i, messages[i]);
                        }
                    }
                    else if(LastReadTime != 0)
                    {
                        //the last read message is before the first one in the list
                        outofboundsNewMessage = true;
                        for (int i = 0; i < messages.Count(); i++)
                            if (position == Position.After)
                                MessageList.Items.Add(messages[i]);
                            else if (position == Position.Before)
                                MessageList.Items.Insert(i, messages[i]);
                        scrollTo = messages.First();
                        MoreNewMessageIndicator.Opacity = 0;
                        MoreNewMessageIndicator.Visibility = Visibility.Visible;
                        MoreNewMessageIndicator.Fade(1, 300).Start();
                    }
                }
                else
                {
                    //DO NOT SHOW NEW MESSAGE INDICATOR. Just add everything before or after
                    for (int i = 0; i < messages.Count(); i++)
                        if (position == Position.After)
                            MessageList.Items.Add(messages[i]);
                        else if (position == Position.Before)
                            MessageList.Items.Insert(i, messages[i]);
                }
                if (scroll && scrollTo != null)
                {
                    MessageList.ScrollIntoView(scrollTo, ScrollIntoViewAlignment.Leading);
                }
            }

            Message last = MessageList.Items.Count > 0 ? (MessageList.Items.Last() as MessageContainer).Message : null;
            if (last != null && App.CurrentGuildId != null && App.CurrentChannelId != null && last.Id != LocalState.Guilds[App.CurrentGuildId].channels[App.CurrentChannelId].raw.LastMessageId)
            {
                ReturnToPresentIndicator.Opacity = 1;
                ReturnToPresentIndicator.Visibility = Visibility.Visible;
                ReturnToPresentIndicator.Fade(1, 300).Start();
            }
            else
            {
                ReturnToPresentIndicator.Visibility = Visibility.Collapsed;
            }
            sw.Stop();
            Debug.WriteLine("Messages took " + sw.ElapsedMilliseconds + "ms to load");
        }
        
        public void RenderGroupMembers()
        {
            if(memberscvs != null)
                memberscvs.Clean();
            List<Member> tempMembers = new List<Member>();
            foreach(var user in LocalState.DMs[App.CurrentChannelId].Users)
            {
                Member m = new Member(new GuildMember() {
                    User = user
                });
                m.DisplayName = user.Username;
                m.MemberHoistRole = new HoistRole("MEMBERS", 0, "MEMBERS", 0, -1);
                if (LocalState.PresenceDict.ContainsKey(m.Raw.User.Id))
                {
                    m.status = LocalState.PresenceDict[m.Raw.User.Id];
                }
                else
                {
                    m.status = new Presence() { Status = "offline", Game = null };
                }
                tempMembers.Add(m);
            }
            Member cm = new Member(new GuildMember()
            {
                User = LocalState.CurrentUser
            });
            cm.DisplayName = LocalState.CurrentUser.Username;
            cm.MemberHoistRole = new HoistRole("MEMBERS", 0, "MEMBERS", 0, -1);
            if (LocalState.PresenceDict.ContainsKey(cm.Raw.User.Id))
            {
                cm.status = LocalState.PresenceDict[cm.Raw.User.Id];
            }
            else
            {
                cm.status = new Presence() { Status = "offline", Game = null };
            }
            tempMembers.Add(cm);
            
            memberscvs = new GroupedObservableCollection<HoistRole, Member>(c => c.MemberHoistRole, tempMembers);

            MembersCvs.Source = memberscvs;
          //  memberscvs.Clear();
            //MembersCVS.Source = memberscvs.SkipWhile(m => m.Value.status.Status == "offline").GroupBy(m => m.Value.MemberDisplayedRole).OrderBy(m => m.Key.Position).ToList();
            //MembersCvs.Source = memberscvs.OrderBy(m => m.Value.Raw.User.Username).GroupBy(m => m.Value.status);
        }

        public void UpdateTyping()
        {
            string typingString = "";
            int DisplayedTyperCounter = 0;
            List<string> NamesTyping = new List<string>();
            foreach (SimpleChannel channel in ChannelList.Items)
                channel.IsTyping = LocalState.Typers.ContainsKey(channel.Id);

            if (App.CurrentChannelId != null)
            {
                if (LocalState.Typers.ContainsKey(App.CurrentChannelId))
                {
                    foreach (var typer in LocalState.Typers[App.CurrentChannelId])
                    {
                        if (App.CurrentGuildIsDM)
                        {
                            NamesTyping.Add(LocalState.DMs[App.CurrentChannelId].Users.FirstOrDefault(m => m.Id == typer.Key).Username);
                        } else
                        {
                            var member = LocalState.Guilds[App.CurrentGuildId].members[typer.Key];
                            string DisplayedName = member.User.Username;
                            if (member.Nick != null) DisplayedName = member.Nick;
                            NamesTyping.Add(DisplayedName);
                        }
                    }
                }
            }

            DisplayedTyperCounter = NamesTyping.Count();
            for (int i = 0; i < DisplayedTyperCounter; i++)
            { //TODO: Fix translate
                if (i == 0)
                    typingString += NamesTyping.ElementAt(i); //first element, no prefix
                else if (i == 2 && i == DisplayedTyperCounter)
                    typingString += " " + App.GetString("/Main/TypingAnd") + " " + " " + NamesTyping.ElementAt(i); //last element out of 2, prefix = "and"
                else if (i == DisplayedTyperCounter)
                    typingString +=
                        ", " + App.GetString("/Main/TypingAnd") + NamesTyping.ElementAt(i); //last element out of 2, prefix = "and" WITH OXFORD COMMA
                else
                    typingString += ", " + NamesTyping.ElementAt(i); //intermediary element, prefix = comma
            }
            if (DisplayedTyperCounter > 1)
                typingString += " " + App.GetString("/Main/TypingPlural");
            else
                typingString += " " + App.GetString("/Main/TypingSingular");

            if (DisplayedTyperCounter == 0)
            {
                TypingStackPanel.Fade(0, 200).Start();
            }
            else
            {
                TypingIndicator.Text = typingString;
                TypingStackPanel.Fade(1, 200).Start();
            }
        }

        int TempGuildCount = 0;
        List<SimpleGuild> oldTempGuilds;
        private async void UpdateGuildAndChannelUnread()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    FriendNotificationCounter.Text = App.FriendNotifications.ToString();
                    if (FriendNotificationCounter.Text != "0")
                    {
                        ShowFriendsBadge.Begin();
                    } else
                    {
                        HideFriendsBadge.Begin();
                    }
                    int Fullcount = 0;

                    foreach (SimpleGuild guild in ServerList.Items)
                    {
                        SimpleGuild gclone = guild.Clone();
                        gclone.NotificationCount = 0; //Will Change if true
                        gclone.IsUnread = false; //Will change if true
                        if (gclone.Id == "@me")
                        {
                            if (App.FriendNotifications > 0 && Storage.Settings.FriendsNotifyFriendRequest)
                            {
                                gclone.NotificationCount += App.FriendNotifications;
                            }
                            List<SimpleGuild> TempGuilds = new List<SimpleGuild>();
                            foreach (var chn in LocalState.DMs.Values)
                                if (LocalState.RPC.ContainsKey(chn.Id))
                                {
                                    ReadState readstate = LocalState.RPC[chn.Id];
                                    if (Storage.Settings.FriendsNotifyDMs)
                                    {
                                        gclone.NotificationCount += readstate.MentionCount;
                                        Fullcount += readstate.MentionCount;
                                    }
                                    // var StorageChannel = LocalState.DMs[chn.Id];
                                    //  if (StorageChannel.LastMessageId != null && readstate.LastMessageId != StorageChannel.LastMessageId)
                                    //     gclone.NotificationCount += 1;
                                /*    if (readstate.MentionCount > 0)
                                    {
                                        SimpleGuild tempguild = new SimpleGuild()
                                        {
                                            Name = chn.Name,
                                            IsDM = true,
                                            NotificationCount = readstate.MentionCount,
                                            Id = chn.Id,
                                            TempLastMessageId = chn.LastMessageId
                                        };
                                        TempGuilds.Add(tempguild);
                                    }*/
                                }
                            
                            //Remove all TempGuilds from serverlist;
                            /*bool TempGuildZone = true;
                            while (TempGuildZone)
                            {
                                if (((SimpleGuild)ServerList.Items[1]).IsDM)
                                    ServerList.Items.RemoveAt(1);
                                else
                                    TempGuildZone = false;
                            }
                            //Add all tempguilds
                            foreach (var tempguild in TempGuilds.OrderBy(x => Common.SnowflakeToTime(x.TempLastMessageId)).Reverse())
                                ServerList.Items.Insert(1, tempguild);
                            TempGuildCount = TempGuilds.Count;*/
                        }
                        else if(!guild.IsDM)
                        {
                            if (LocalState.GuildSettings.ContainsKey(gclone.Id))
                            {
                                gclone.IsMuted = LocalState.GuildSettings[gclone.Id].raw.Muted;
                            } else
                            {
                                gclone.IsMuted = false;
                            }
                            for (int i = 0; i < LocalState.Guilds[gclone.Id].channels.Count; i++)
                            {
                                //TODO fix "collection was modified" by making this shit thread-safe
                                var chn = LocalState.Guilds[gclone.Id].channels.ElementAt(i).Value; //TODO use stopwatch to see if this shit can be optimized
                                if (LocalState.RPC.ContainsKey(chn.raw.Id))
                                {
                                    var chan = LocalState.Guilds[gclone.Id].channels[chn.raw.Id];
                                    ReadState readstate = LocalState.RPC[chn.raw.Id];

                                    bool Muted = LocalState.GuildSettings.ContainsKey(gclone.Id) ? (LocalState.GuildSettings[gclone.Id].channelOverrides.ContainsKey(chan.raw.Id) ?
                                    LocalState.GuildSettings[gclone.Id].channelOverrides[chan.raw.Id].Muted
                                    : false) :
                                    false;

                                    gclone.NotificationCount += readstate.MentionCount;
                                    Fullcount += readstate.MentionCount;

                                    if (chan.raw.LastMessageId != null
                                    && chan.raw.LastMessageId != readstate.LastMessageId && (Storage.Settings.mutedChnEffectServer || !Muted)
                                    ) //if channel is unread and not muted
                                        gclone.IsUnread = true;
                                }
                            }
                        }

                        guild.Id = gclone.Id;
                        guild.ImageURL = gclone.ImageURL;
                        guild.IsDM = gclone.IsDM;
                        guild.IsMuted = gclone.IsMuted;
                        guild.IsUnread = gclone.IsUnread;
                        guild.Name = gclone.Name;
                        guild.NotificationCount = gclone.NotificationCount;
                    }
                    if (App.CurrentGuildIsDM)
                    {
                        foreach (SimpleChannel sc in ChannelList.Items)
                        {
                            if (LocalState.RPC.ContainsKey(sc.Id))
                            {
                                ReadState readstate = LocalState.RPC[sc.Id];
                                sc.NotificationCount = readstate.MentionCount;
                                //Just ignore unread indicators for DMs:

                                //  var StorageChannel = LocalState.DMs[sc.Id];
                                // if (StorageChannel.LastMessageId != null &&
                                //   readstate.LastMessageId != StorageChannel.LastMessageId)
                                //  sc.IsUnread = true;
                                //  else
                                //    sc.IsUnread = false;
                            }
                            sc.IsUnread = false;
                        }
                           
                    }
                    else
                    {
                        if (App.CurrentGuildId != null) //Incase called before intiialization
                        {
                            foreach (SimpleChannel sc in ChannelList.Items)
                                if (LocalState.RPC.ContainsKey(sc.Id) && LocalState.Guilds[App.CurrentGuildId].channels.ContainsKey(sc.Id))
                                {
                                    ReadState readstate = LocalState.RPC[sc.Id];
                                    sc.NotificationCount = readstate.MentionCount;
                                    var StorageChannel = LocalState.Guilds[App.CurrentGuildId].channels[sc.Id];
                                    if (StorageChannel != null && StorageChannel.raw.LastMessageId != null &&
                                        readstate.LastMessageId != StorageChannel.raw.LastMessageId)
                                        sc.IsUnread = true;
                                    else
                                        sc.IsUnread = false;

                                    sc.IsMuted = LocalState.GuildSettings.ContainsKey(App.CurrentGuildId) && LocalState.GuildSettings[App.CurrentGuildId].channelOverrides.ContainsKey(sc.Id) && LocalState.GuildSettings[App.CurrentGuildId].channelOverrides[sc.Id].Muted;
                                }
                        }
                    }

                    if (Storage.Settings.FriendsNotifyFriendRequest)
                    {
                        Fullcount += App.FriendNotifications;
                    }

                    if (App.FriendNotifications > 0)
                    {
                        FriendNotificationCounter.Text = App.FriendNotifications.ToString();
                        ShowFriendsBadge.Begin();
                    }
                    else
                    {
                        HideFriendsBadge.Begin();
                    }

                    if (Fullcount != App.AllNotifications)
                    {
                        if (Fullcount > 0)
                        {
                            App.AllNotifications = Fullcount;
                            //ShowBadge.Begin();//TODO
                            //BurgerNotificationCounter.Text = Fullcount.ToString();
                        }
                        else
                        {
                            App.AllNotifications = Fullcount;
                            //HideBadge.Begin();//TODO
                        }

                        if (Storage.Settings.Badge)
                        {
                            int count = 0;
                            foreach (var chn in LocalState.RPC.Values.ToList())
                            {
                                count += chn.MentionCount;
                            }
                            NotificationManager.SendBadgeNotification(count);
                        }
                    }
                    RefreshVisibilityIndicators();
                });

        }

        private async void LoadOlderMessages()
        {
            DisableLoadingMessages = true;
            MessagesLoadingTop.Visibility = Visibility.Visible;
            var messages = await MessageManager.ConvertMessage((await RESTCalls.GetChannelMessagesBefore(App.CurrentChannelId, (MessageList.Items.FirstOrDefault(x => (x as MessageContainer).Message != null) as MessageContainer).Message.Id)).ToList());
            AddMessages(Position.Before, false, messages, outofboundsNewMessage); //if there is an out of bounds new message, show the indicator. Otherwise, don't.
            MessagesLoadingTop.Visibility = Visibility.Collapsed;
            await Task.Delay(1000);
            DisableLoadingMessages = false;
        }
        private bool LastMessageIsLoaded()
        {
            if (App.CurrentGuildIsDM)
            {
                for (int i = MessageList.Items.Count; i < 0; i--)
                {
                    if (((MessageContainer)MessageList.Items[i]).Message != null)
                    {
                        if (((MessageContainer)MessageList.Items[i]).Message.Id == LocalState.DMs[App.CurrentChannelId].LastMessageId)
                            return true;
                        else return false;
                    }
                }
                return false;
            }
            else
            {
                for (int i = MessageList.Items.Count; i < 0; i--)
                {
                    if (((MessageContainer)MessageList.Items[i]).Message != null)
                    {
                        if (((MessageContainer)MessageList.Items[i]).Message.Id == LocalState.Guilds[App.CurrentGuildId].channels[App.CurrentChannelId].raw.LastMessageId)
                            return true;
                        else return false;
                    }
                }
                return false;
            }
        }
        private async void LoadNewerMessages()
        {
            try
            {
                Message last = (MessageList.Items.Last() as MessageContainer).Message;
                if (last != null && last.Id != LocalState.RPC[App.CurrentChannelId].LastMessageId)
                {
                    // var offset = MessageScrollviewer.VerticalOffset;
                    MessagesLoading.Visibility = Visibility.Visible;
                    DisableLoadingMessages = true;
                    var messages = await MessageManager.ConvertMessage((await RESTCalls.GetChannelMessagesAfter(App.CurrentChannelId, (MessageList.Items.LastOrDefault(x => (x as MessageContainer).Message != null) as MessageContainer).Message.Id)).ToList());
                    messageStacker.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;
                    AddMessages(Position.After, false, messages, outofboundsNewMessage); //if there is an out of bounds new message, show the indicator. Otherwise, don't.
                    MessagesLoading.Visibility = Visibility.Collapsed;
                    await Task.Delay(1000);
                    messageStacker.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;
                    DisableLoadingMessages = false;
                }
            }
            catch {}
        }

        private async void LoadMessagesAround(string id)
        {
            try
            {
                if (!LastMessageIsLoaded())
                {
                    MessagesLoadingTop.Visibility = Visibility.Visible;
                    MessageList.Items.Clear();
                    DisableLoadingMessages = true;
                    var messages = await MessageManager.ConvertMessage((await RESTCalls.GetChannelMessagesAround(App.CurrentChannelId, id)).ToList());
                    AddMessages(Position.After, true, messages, true);
                    MessagesLoadingTop.Visibility = Visibility.Collapsed;
                    await Task.Delay(1000);
                    DisableLoadingMessages = false;
                }
            }
            catch { }
        }
        #endregion
        private async void SetupUI()
        {
            //Remove clipping from all the listviews
            Common.RemoveScrollviewerClipping(ServerScrollviewer);
            Common.RemoveScrollviewerClipping(ChannelScrollviewer);
            Common.RemoveScrollviewerClipping(MembersListView);

        }
        private async void App_ReadyRecievedHandler(object sender, EventArgs e)
        {
            GatewayManager.Gateway.GuildMemberRemoved += Gateway_GuildMemberRemoved;
            GatewayManager.Gateway.GuildMemberAdded += Gateway_GuildMemberAdded;
            GatewayManager.Gateway.GuildMemberUpdated += Gateway_GuildMemberUpdated;
            GatewayManager.Gateway.ChannelRecipientAdded += Gateway_ChannelRecipientAdded;
            GatewayManager.Gateway.ChannelRecipientRemoved += Gateway_ChannelRecipientRemoved;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                 () =>
                 {
                     SetupUI();
                 RenderCurrentUser();
                 RenderGuilds();
                 ServerList.SelectedIndex = 0;
                 friendPanel.Load();
                 App.UpdateUnreadIndicators();
                 App.FullyLoaded = true;
                 if (App.PostLoadTask != null)
                 {
                     switch (App.PostLoadTask)
                     {
                         case "SelectGuildChannelTask":
                             App.SelectGuildChannel(((App.GuildChannelSelectArgs)App.PostLoadTaskArgs).GuildId, ((App.GuildChannelSelectArgs)App.PostLoadTaskArgs).ChannelId);
                             break;
                         case "invite":
                             App.NavigateToJoinServer(((App.GuildChannelSelectArgs)App.PostLoadTaskArgs).GuildId);
                             break;
                         }
                     }
                     //Check version number, and if it's different from before, open the what's new page
                     Package package = Package.Current;
                     PackageId packageId = package.Id;
                     string version = packageId.Version.Build.ToString()+packageId.Version.Major.ToString()+packageId.Version.Minor.ToString();
                     if (Storage.Settings.lastVerison == "0")
                     {
                         Storage.Settings.lastVerison = version;
                         Storage.SaveAppSettings();
                         App.NavigateToAbout(true);
                     }
                     else if (Storage.Settings.lastVerison != version)
                     {
                         Storage.Settings.lastVerison = version;
                         Storage.SaveAppSettings();
                         App.NavigateToAbout(true);
                     }
                     Loading.Hide(true);
                     if (Storage.Settings.VideoAd)
                     {
                         InterstitialAd videoAd = new InterstitialAd();
                         videoAd.AdReady += VideoAd_AdReady;
                         videoAd.ErrorOccurred += VideoAd_ErrorOccurred;
                         videoAd.RequestAd(AdType.Video, "9nbrwj777c8r", "1100015338");
                     }
                 });
            if (setupArgs != "")
            {
                if (setupArgs.StartsWith("quarrel://"))
                {
                    string[] segments = setupArgs.Replace("quarrel://", "").Split('/');
                    var count = segments.Count();
                    if (count > 0)
                    {
                        if (segments[0] == "guild")
                        {
                            if (count == 3)
                                App.SelectGuildChannel(segments[1], segments[2]);
                            else if (count == 2)
                                App.SelectGuildChannel(segments[1], null);
                        }
                        else if (segments[0] == "invite")
                        {
                            App.NavigateToJoinServer(segments[1]);
                        }
                    }
                }
                else if (setupArgs == "SHARETARGET")
                {
                    SubFrameNavigator(typeof(SubPages.ExtendedMessageEditor));
                    navigationHistory.Clear();
                }
            }
        }

        private async void Gateway_ChannelRecipientRemoved(object sender, Gateway.GatewayEventArgs<ChannelRecipientUpdate> e)
        {
            if (LocalState.DMs.ContainsKey(e.EventData.channel_id) && LocalState.DMs[e.EventData.channel_id].Users.FirstOrDefault(x => x.Id == e.EventData.user.Id) != null)
                LocalState.DMs[e.EventData.channel_id].Users.Remove(LocalState.DMs[e.EventData.channel_id].Users.FirstOrDefault(x => x.Id == e.EventData.user.Id));
            if (App.CurrentGuildIsDM)
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    foreach (SimpleChannel sc in ChannelList.Items)
                    {
                        if (sc.Id == e.EventData.channel_id && LocalState.DMs.ContainsKey(e.EventData.channel_id))
                            sc.Subtitle = App.GetString("/Main/members").Replace("<count>", (LocalState.DMs[e.EventData.channel_id].Users.Count() + 1).ToString());
                    }
                });
                
            if (App.CurrentChannelId == e.EventData.channel_id || !App.CurrentGuildIsDM)
            {
                if (App.CurrentChannelId == e.EventData.channel_id)
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        memberscvs.Remove(FindMember(e.EventData.user.Id));
                    });
            }
        }
       

        private async void Gateway_ChannelRecipientAdded(object sender, Gateway.GatewayEventArgs<ChannelRecipientUpdate> e)
        {
            if (LocalState.DMs.ContainsKey(e.EventData.channel_id))
                LocalState.DMs[e.EventData.channel_id].Users.Add(e.EventData.user);
            if (App.CurrentGuildIsDM)
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    foreach (SimpleChannel sc in ChannelList.Items)
                    {
                        if (sc.Id == e.EventData.channel_id && LocalState.DMs.ContainsKey(e.EventData.channel_id))

                            sc.Subtitle = App.GetString("/Main/members").Replace("<count>", (LocalState.DMs[e.EventData.channel_id].Users.Count() + 1).ToString());

                    }
                });
            if (App.CurrentChannelId == e.EventData.channel_id)
            {
                Member m = new Member(new GuildMember()
                {
                    User = e.EventData.user
                });
                AddToMembersCvs(m, true);
            }
        }

        private void VideoAd_AdReady(object sender, object e)
        {
            (sender as InterstitialAd).Show();
        }

        private async void VideoAd_ErrorOccurred(object sender, AdErrorEventArgs e)
        {
            Storage.Settings.VideoAd = false;
            Storage.SettingsChanged();
            Storage.SaveAppSettings();

            MessageDialog msg = new MessageDialog("Couldn't find a video ad to show, showing banner ads");
            await msg.ShowAsync();
        }

        private async void App_TypingHandler(object sender, App.TypingArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    UpdateTyping();
                });
        }

        private async void App_UpdateUnreadIndicatorsHandler(object sender, EventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                 () =>
                 {
                     UpdateGuildAndChannelUnread();
                 });
        }

        private async void App_UserStatusChangedHandler(object sender, App.UserStatusChangedArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    if(e.Settings.GuildOrder != null)
                    {
                        int position = 1;
                        foreach (var guild in e.Settings.GuildOrder)
                        {
                            var item = ServerList.Items.FirstOrDefault(x => (x as SimpleGuild).Id == guild);
                            if (item == null) return;
                            if (ServerList.Items.IndexOf(item) != position)
                            {
                                ServerList.Items.Remove(item);
                                ServerList.Items.Insert(position+TempGuildCount, item);
                            }
                            position++;
                        }
                        position = 0;
                    }
                        if (e.Settings.Status != null)
                        {
                            if (e.Settings.Status != "invisible")
                            {
                                UserStatusIndicator.Fill = (SolidColorBrush)App.Current.Resources[e.Settings.Status];
                            } else
                            {
                                UserStatusIndicator.Fill = (SolidColorBrush)App.Current.Resources["offline"];
                            }
                            switch (e.Settings.Status)
                            {
                                case "online":
                                    UserStatusOnline.IsChecked = true;
                                    break;
                                case "idle":
                                    UserStatusIdle.IsChecked = true;
                                    break;
                                case "dnd":
                                    UserStatusDND.IsChecked = true;
                                    break;
                                case "invisible":
                                    UserStatusInvisible.IsChecked = true;
                                    break;
                            }
                        var member = FindMember(LocalState.CurrentUser.Id);
                        if (member == null) return;
                        member.status = new Presence() { Game = member.status.Game, GuildId = member.status.GuildId, Roles = member.status.Roles, Status = e.Settings.Status, User = member.status.User };
                        if (LocalState.PresenceDict.ContainsKey(LocalState.CurrentUser.Id))
                            LocalState.PresenceDict[LocalState.CurrentUser.Id] = member.status;
                    }
                });
        }

        #region Messages
        private async void App_MessageCreatedHandler(object sender, App.MessageCreatedArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                 async () =>
                 {
                     //var lastMsg = MessageList.Items.LastOrDefault() as MessageContainer;
                     //if (e.Message.User.Id == LocalState.CurrentUser.Id)
                     //{
                     //    if (lastMsg.Pending)
                     //    {
                     //        lastMsg.Message = lastMsg.Message.Value.MergePending(e.Message);
                     //        if (lastMsg.Message.Value.User.Id == null)
                     //        {
                     //            lastMsg.Message.Value.SetUser(LocalModels.LocalState.CurrentUser);
                     //        }
                     //        lastMsg.Pending = false;
                     //    }
                     //} else
                     //{
                     Message last = null;
                     if (MessageList.Items.Count > 0)
                     {
                         last = (MessageList.Items.Last() as MessageContainer).Message;
                         if(last != null && last.Id == LocalState.RPC[App.CurrentChannelId].LastMessageId) { 
}
                            //Only add a message if the last one is functional
                            MessageList.Items.Add(MessageManager.MakeMessage(e.Message, MessageManager.ShouldContinuate(e.Message, last)));
                     }
                     else
                     {
                         MessageList.Items.Add(MessageManager.MakeMessage(e.Message, false));
                     }
                     
                     //set the last message id

                     //}
                     if (e.Message.User.Id == LocalState.CurrentUser.Id)
                     {
                         //do something????
                     }
                     else
                     {
                         App.MarkMessageAsRead(e.Message.Id, App.CurrentChannelId);
                     }
                     

                     if (Storage.Settings.Vibrate && e.Message.User.Id!=LocalState.CurrentUser.Id)
                     {
                         var vibrationDuration = TimeSpan.FromMilliseconds(200);
                         if (ApiInformation.IsTypePresent("Windows.Phone.Devices.Notification"))
                         {
                             var phonevibrate = Windows.Phone.Devices.Notification.VibrationDevice.GetDefault();
                             phonevibrate.Vibrate(vibrationDuration);
                         }
                         //This will be for another time, it clearly isn't working right now
                        /* var gamepad = Windows.Gaming.Input.Gamepad.Gamepads.FirstOrDefault();
                         if(gamepad!=null)
                         {
                             GamepadVibration vibration = new GamepadVibration();
                             await Task.Run(async () =>
                             {
                                 vibration.RightMotor = 0.5;
                                 gamepad.Vibration = vibration;
                                 await Task.Delay(vibrationDuration);
                                 vibration.RightMotor = 0;
                             });
                         }*/
                     }
                     if (e.Message.TTS)
                     {
                         MediaElement mediaplayer = new MediaElement();
                         using (var speech = new SpeechSynthesizer())
                         {
                             speech.Voice = SpeechSynthesizer.AllVoices.First(gender => gender.Gender == VoiceGender.Male);
                             string ssml = @"<speak version='1.0' " + "xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>" + e.Message.User.Username + "said" + e.Message.Content + "</speak>";
                             SpeechSynthesisStream stream = await speech.SynthesizeSsmlToStreamAsync(ssml);
                             mediaplayer.SetSource(stream, stream.ContentType);
                             mediaplayer.Play();
                         }
                     }
                 });
        }



        private async void App_MessageDeletedHandler(object sender, App.MessageDeletedArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                 () =>
                 {
                     for (int i = 0; i < MessageList.Items.Count; i++)
                     {
                         MessageContainer message = (MessageContainer)MessageList.Items[i];
                         if (message.Message != null && message.Message.Id == e.MessageId)
                         {
                             MessageList.Items.Remove(message);
                             if (LocalState.RPC[App.CurrentChannelId].LastMessageId == e.MessageId)
                             {
                                 MessageContainer last = (MessageContainer)MessageList.Items.LastOrDefault();
                                 if (last != null)
                                 {
                                     var temp = LocalState.RPC[App.CurrentChannelId];
                                     temp.LastMessageId = ((MessageContainer)MessageList.Items.Last()).Message.Id;
                                     LocalState.RPC[App.CurrentChannelId] = temp;
                                     LocalState.Guilds[App.CurrentGuildId].channels[App.CurrentChannelId].raw.LastMessageId = temp.LastMessageId;
                                 }
                             }
                         }
                     }

                 });
        }

        private async void App_MessageEditedHandler(object sender, App.MessageEditedArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                 () =>
                 {
                     if (MessageList.Items.Count > 0)
                     {
                         foreach (MessageContainer message in MessageList.Items)
                         {
                             if (message.Message != null && message.Message.Id == e.Message.Id)
                             {
                                 message.Message = e.Message;
                             }
                         }
                     }
                 });
        }

        #endregion

        #region DMs

        private async void App_DMCreatedHandler(object sender, App.DMCreatedArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        if (ChannelList.Items.Count > 0)
                        {
                            var chn = ChannelManager.MakeChannel(e.DMChannel);
                            if (chn != null)
                                ChannelList.Items.Insert(findLocation(chn), chn);
                        }
                    });
        }

        private async void App_DMDeletedHandler(object sender, App.DMDeletedArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        foreach (SimpleChannel chn in ChannelList.Items)
                        {
                            if (chn.Id == e.DMId)
                            {
                                ChannelList.Items.Remove(chn);
                            }
                        }
                    });
        }

        private async void App_DMUpdatePosHandler(object sender, App.DMUpdatePosArgs e)
        {
            
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        foreach (SimpleChannel chn in ChannelList.Items)
                        {
                            if (chn.Id == e.Id)
                            {
                                ChannelList.Items.Remove(chn);
                                ChannelList.Items.Insert(0, chn);
                            }
                        }
                    });
        }

        #endregion

        #region Channel
        private async void App_GuildChannelCreatedHandler(object sender, App.GuildChannelCreatedArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                 () =>
                 {
                     if (ChannelList.Items.Count > 0)
                     {
                         var chn = ChannelManager.MakeChannel(LocalState.Guilds[App.CurrentGuildId].channels[e.Channel.Id]);
                         if (chn != null)
                             ChannelList.Items.Insert(0, chn);
                     }
                 });
        }

        private async void App_GuildChannelDeletedHandler(object sender, App.GuildChannelDeletedArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                 () =>
                 {
                     if (ChannelList.Items.Count > 0)
                     {
                         foreach (SimpleChannel channel in ChannelList.Items)
                         {
                             if (channel.Id == e.ChannelId)
                             {
                                 ChannelList.Items.Remove(channel);
                             }
                         }
                     }
                 });
        }

        private void App_GuildChannelUpdatedHandler(object sender, App.GuildChannelUpdatedArgs e)
        {
            App_GuildChannelDeletedHandler(sender, new App.GuildChannelDeletedArgs() { ChannelId = e.Channel.Id, GuildId = e.Channel.GuildId});
            App_GuildChannelCreatedHandler(sender, new App.GuildChannelCreatedArgs() { Channel = e.Channel});
        }

        private int findLocation(SimpleChannel c)
        {
            if (c.Type == 0 || c.Type == 2 || c.Type == 4)
            {
                if (c.Type != 4)
                {
                    int pos = 0;
                    foreach (SimpleChannel chn in ChannelList.Items)
                    {
                        if (chn.Id == c.ParentId)
                        {
                            if (c.Type == 0)
                            {
                                return pos + c.Position + 1;
                            }
                            else if (c.Type == 2)
                            {
                                //TODO: Handle Voice channels
                            }
                        }
                        pos++;
                    }
                } else
                {
                    //TODO: Handle Categories
                }
            } else // type == 1 or 3
            {
                int pos = 0;
                foreach (SimpleChannel chn in ChannelList.Items)
                {
                    if (Common.SnowflakeToTime(c.LastMessageId) > Common.SnowflakeToTime(chn.LastMessageId))
                    {
                        return pos;
                    }
                    pos++;
                }
            }

            return 0;
        }
        #endregion

        #region Guilds
        private async void App_GuildCreatedHandler(object sender, App.GuildCreatedArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                 () =>
                 {
                     ServerList.Items.Insert(1+TempGuildCount, GuildManager.CreateGuild(e.Guild));
                 });
        }

        private async void App_GuildSyncedHandler(object sender, GuildSync e)
        {
            App.MemberListTrie = new Gma.DataStructures.StringSearch.PatriciaTrie<Common.AutoComplete>();
            if (!App.CurrentGuildIsDM && App.CurrentGuildId != null && App.CurrentGuildId == e.GuildId) //Reduntant I know
            {
                //await GatewayManager.Gateway.RequestAllGuildMembers(App.CurrentGuildId);
                var members = e.Members;
                var presences = e.Presences;
                foreach (var presence in presences)
                {
                    if (e.IsLarge && presence.Status == "offline") { }
                    else
                    {
                        if (LocalState.PresenceDict.ContainsKey(presence.User.Id))
                            LocalState.PresenceDict[presence.User.Id] = presence;
                        else
                            LocalState.PresenceDict.Add(presence.User.Id, presence);
                    }
                }
                int totalrolecounter = 0;
                
                foreach (var member in members)
                {
                    member.setRoles(member.Roles.TakeWhile(x => App.CurrentGuildId != null && LocalState.Guilds[App.CurrentGuildId].roles.ContainsKey(x)).OrderByDescending(x => LocalState.Guilds[App.CurrentGuildId].roles[x].Position));

                    bool added = LocalState.Guilds[App.CurrentGuildId].members.TryAdd(member.User.Id, member);
                    if(!added)
                        LocalState.Guilds[App.CurrentGuildId].members[member.User.Id] = member;
                    
                    if (!string.IsNullOrEmpty(member.Nick))
                        App.MemberListTrie.Add(member.Nick.ToLower(), new Common.AutoComplete(member.Nick, member.User.Username + "#" + member.User.Discriminator, Common.AvatarString(member.User.Avatar, member.User.Id)));
                    
                     App.MemberListTrie.Add(member.User.Username.ToLower(), new Common.AutoComplete(member.User.Username + "#" + member.User.Discriminator, null, Common.AvatarString(member.User.Avatar, member.User.Id)));
                }
                Debug.WriteLine("Trie traversal: " + App.MemberListTrie.Traversal() + "/end");
                if (LocalState.Guilds[App.CurrentGuildId].Raw.Roles != null)
                {
                    foreach (Role role in LocalState.Guilds[App.CurrentGuildId].Raw.Roles)
                    {
                        Role roleAlt = role;
                        //TODO Optimize the ConcurrentDictionary, because right now the .ContainsKey is running twice
                        if (LocalState.Guilds[App.CurrentGuildId].roles.ContainsKey(role.Id))
                        {
                            LocalState.Guilds[App.CurrentGuildId].roles[role.Id] = roleAlt;
                        }
                        else
                        {
                            LocalState.Guilds[App.CurrentGuildId].roles.TryAdd(role.Id, roleAlt);
                        }
                    }
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    List<Member> tempMembers = new List<Member>();
                    try
                    {
                        var keys = LocalState.Guilds[App.CurrentGuildId].members.Keys.ToArray();
                        for (int i = 0; keys.Count() > i; i++)
                        {
                            var member = LocalState.Guilds[App.CurrentGuildId].members[keys[i]];
                      //foreach(var member in LocalState.Guilds[App.CurrentGuildId].members)
                       // {
                            if (!e.IsLarge || LocalState.PresenceDict.ContainsKey(keys[i]))
                            {
                                Member m = new Member(member);
                                if (m.Raw.Roles != null)
                                {
                                    m.Raw.Roles = m.Raw.Roles.TakeWhile(x => LocalState.Guilds[App.CurrentGuildId].roles.ContainsKey(x)).OrderByDescending(x => LocalState.Guilds[App.CurrentGuildId].roles[x].Position);
                                }

                                //Set it to first Hoist Role or everyone if null
                             //   await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                              //  () =>
                               // {
                                    m.MemberHoistRole = MemberManager.GetRole(m.Raw.Roles.FirstOrDefault(x => LocalState.Guilds[App.CurrentGuildId].roles[x].Hoist), App.CurrentGuildId);
                               // });


                                if (LocalState.PresenceDict.ContainsKey(m.Raw.User.Id))
                                {
                                    m.status = LocalState.PresenceDict[m.Raw.User.Id];
                                }
                                else
                                {
                                    m.status = new Presence() { Status = "offline", Game = null };
                                }
                                if (member.Nick != null)
                                    m.DisplayName = member.Nick;
                                else
                                    m.DisplayName = member.User.Username;
                                // if (memberscvs(m.Raw.User.Id))
                                //{
                                //   memberscvs.Remove(m.Raw.User.Id);
                                //}
                                tempMembers.Add(m);
                            }
                        }
                        
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            memberscvs = new GroupedObservableCollection<HoistRole, Member>(c => c.MemberHoistRole, tempMembers);
                            MembersCvs.Source = memberscvs;
                        });
                    }
                    catch (Exception er)
                    {
                        Console.WriteLine(er.HResult + ": " + er.Message);
                    }
                    sw.Stop();
                    Debug.WriteLine("Itterating over all members took " + sw.ElapsedMilliseconds + "ms");
                    try
                    {
                       // var sortedMembers = memberscvs.I.OrderBy(m => (m)Raw.User.Username).GroupBy(m => ((Member)m.Value).MemberHoistRole).OrderByDescending(x => x.Key.Position);

                      //  foreach (var m in sortedMembers)
                      //  {
                      //      int count = m.Count();
                      //  }
                      //  await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                      //      () =>
                      //      {
                                // MembersCVS = new CollectionViewSource();
                      //          MembersCvs.Source = sortedMembers;
                        //    });
                    }
                    catch
                    {

                    }
                    //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    //sw.Start();
                    //sw.Stop();
                    //else
                    //    MembersCVS.Source = memberscvs.SkipWhile(m => m.Value.status.Status == "offline").GroupBy(m => m.Value.MemberDisplayedRole).OrderBy(m => m.Key.Position).ToList();
                }
            }
        }
        #endregion

        #region Members
        private async void App_MembersUpdatedHandler(object sender, EventArgs e)
        {
            int totalrolecounter = 0;
            int everyonecounter = LocalState.Guilds[App.CurrentGuildId].members.Count() - totalrolecounter;

            if (LocalState.Guilds[App.CurrentGuildId].Raw.Roles != null)
            {
                foreach (Role role in LocalState.Guilds[App.CurrentGuildId].Raw.Roles)
                {
                    Role roleAlt = role;
                    //TODO Optimize this, .ContainsKey is currently running twice
                    if (LocalState.Guilds[App.CurrentGuildId].roles.ContainsKey(role.Id))
                    {
                        LocalState.Guilds[App.CurrentGuildId].roles[role.Id] = roleAlt;
                    }
                    else
                    {
                        LocalState.Guilds[App.CurrentGuildId].roles.TryAdd(role.Id, roleAlt);
                    }
                }
            }

            foreach (var member in LocalState.Guilds[App.CurrentGuildId].members)
            {
                if (LocalState.Guilds[App.CurrentGuildId].Raw.MemberCount < 1000 || LocalState.PresenceDict.ContainsKey(member.Key)) //Small guild
                {
                    Member m = new Member(member.Value);
                    m.Raw.Roles = m.Raw.Roles.TakeWhile(x => LocalState.Guilds[App.CurrentGuildId].roles.ContainsKey(x)).OrderByDescending(x => LocalState.Guilds[App.CurrentGuildId].roles[x].Position);

                    //Set it to first Hoist Role or everyone if null
                    m.MemberHoistRole = MemberManager.GetRole(m.Raw.Roles.FirstOrDefault(x => LocalState.Guilds[App.CurrentGuildId].roles[x].Hoist), App.CurrentGuildId);

                    if (LocalState.PresenceDict.ContainsKey(m.Raw.User.Id))
                    {
                        m.status = LocalState.PresenceDict[m.Raw.User.Id];
                    }
                    else
                    {
                        m.status = new Presence() { Status = "offline", Game = null };
                    }
                   // if (memberscvs.ContainsKey(m.Raw.User.Id))
                   // {
                   //     memberscvs.Remove(m.Raw.User.Id);
                  //  }
                    memberscvs.Add(m);
                }
            }

            try
            {
               // App.DisposeMemberList(); //Clear all existing MemberList Items (cleanly)
           //     var sortedMembers =
           //         memberscvs.GroupBy(m => ((Member)m.Value).MemberHoistRole).OrderByDescending(x => x.Key.Position);

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                                // MembersCVS = new CollectionViewSource();
                                MembersCvs.Source = memberscvs;
                    });
            }
            catch
            {

            }
        }
        #endregion

        #endregion

        #endregion

        #region UIEvents
        private void ToggleSplitView(object sender, RoutedEventArgs e)
        {
            sideDrawer.ToggleLeft();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int extrawidth = 0;
            if (UISize.CurrentState == Large | UISize.CurrentState == ExtraLarge)
                extrawidth = 240;
            ChannelHeader.MaxWidth = e.NewSize.Width - (72*3)+1+extrawidth;
        }

        private void UserStatus_Checked(object sender, RoutedEventArgs e)
        {
            if (UserStatusOnline.IsChecked == true)
            {
                App.UpdatePresence("online");
            } else if (UserStatusIdle.IsChecked == true)
            {
                App.UpdatePresence("idle");
            } else if (UserStatusDND.IsChecked == true)
            {
                App.UpdatePresence("dnd");
            } else
            {
                App.UpdatePresence("invisible");
            }
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            userFlyout.Hide();
            App.NavigateToSettings();
        }

        private void ServerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServerList.SelectedItem != null && ServerSelectionWasClicked)
            {
                ServerSelectionWasClicked = false;
                
                var guildid = (ServerList.SelectedItem as SimpleGuild).Id;
                App.NavigateToGuild(guildid);
                
                sideDrawer.OpenLeft();
                Task.Run(() =>
                {
                    UserActivityManager.SwitchSession(guildid);
                }); 
            }
        }

        bool IgnoreChange = false;
        bool lastChangeProgrammatic = false;
        bool ChannelSelectionWasClicked = true;
        bool ServerSelectionWasClicked = true;
        object previousSelection;
        private void ChannelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChannelList.SelectedIndex != -1)
            {
                Ad.Visibility = Visibility.Collapsed;
            }
           
            //Verify if the last selection was navigated to (with a keyboard/controller) or actually clicked on

            //When selecting a category, we want to simulate ListView's Mode = Click, 
            //so we use IgnoreChange to immediately re-select the unselected item 
            //after having clicked on a category (without reloading anything)

            if (!lastChangeProgrammatic)
            {
                if (!IgnoreChange) //True if the last selection was a category, Voice channel
                {
                    if (ChannelSelectionWasClicked)
                    {
                        ChannelSelectionWasClicked = false; //clearly it was, but the next one will not necessarily be clicked. So set to false.

                        if (App.ShowAds)
                        {
                            if (UISize.CurrentState == Large || UISize.CurrentState == ExtraLarge)
                            {
                                PCAd.Visibility = Visibility.Visible;
                                MobileAd.Visibility = Visibility.Collapsed;
                            } else
                            {
                                PCAd.Visibility = Visibility.Collapsed;
                                MobileAd.Visibility = Visibility.Visible;
                            }
                        }

                        if (ChannelList.SelectedItem != null) //Called on clear
                        {
                            var channel = ChannelList.SelectedItem as SimpleChannel;
                            if (channel.Type == 4) //CATEGORY
                            {
                                foreach (SimpleChannel item in ChannelList.Items.Where(x => (x as SimpleChannel).ParentId == channel.Id))
                                {
                                    if (item.Hidden)
                                        item.Hidden = false;
                                    else
                                        item.Hidden = true;
                                }
                                channel.Hidden = !channel.Hidden;
                                if (previousSelection == null)
                                    ChannelList.SelectedIndex = -1;
                                else
                                    ChannelList.SelectedItem = previousSelection;
                            }
                            else if (channel.Type == 2) //VOICE
                            {
                                IgnoreChange = true;
                                App.ConnectToVoice(channel.Id, App.CurrentGuildId, channel.Name, LocalState.Guilds[App.CurrentGuildId].Raw.Name);
                                if (previousSelection == null)
                                    ChannelList.SelectedIndex = -1;
                                else
                                    ChannelList.SelectedItem = previousSelection;
                            }
                            else
                            {
                                sideDrawer.CloseLeft();
                                previousSelection = ChannelList.SelectedItem;
                               
                                if (App.CurrentGuildIsDM)
                                {
                                    var cid = (ChannelList.SelectedItem as SimpleChannel).Id;
                                    if (!string.IsNullOrEmpty(autoselectchannelcontent))
                                        App.NavigateToDMChannel(cid, autoselectchannelcontent);
                                    else
                                        App.NavigateToDMChannel(cid);
                                    Task.Run(() =>
                                    {
                                        UserActivityManager.SwitchSession(cid);
                                    });
                                }
                                else
                                {
                                    App.NavigateToGuildChannel(App.CurrentGuildId, (ChannelList.SelectedItem as SimpleChannel).Id);
                                }
                            }
                        }
                    }
                }
                else
                {
                    IgnoreChange = false;
                }

            }
            else
            {
                lastChangeProgrammatic = false;
            }
        }
        private void ChannelList_ItemClick(object sender, ItemClickEventArgs e)
        {
            autoselectchannel = null;
            ChannelSelectionWasClicked = true;
            if (e.ClickedItem == ChannelList.SelectedItem)
                //This is for xbox one, because when "clicking" on a channel, it is already selected
                ChannelList_SelectionChanged(null, null);
        }
        private void ServerList_ItemClick(object sender, ItemClickEventArgs e)
        {
            autoselectchannel = null;
            ServerSelectionWasClicked = true;
            if (e.ClickedItem == ServerList.SelectedItem)
                //This if for xbox one, because when clicking on a channel it is already selected
                ServerList_SelectionChanged(null, null);
        }

        private void AddChannelButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            App.NavigateToCreateChannel();
        }

        private void AddServer(object sender, RoutedEventArgs e)
        {
            App.NavigateToAddServer();
        }

        private void ServerNameButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavigateToGuildEdit(App.CurrentGuildId);
        }

        private void CreateMessage(object sender, RoutedEventArgs e)
        {
            var text = MessageBox1.Text;
            App.CreateMessage(App.CurrentChannelId, text);
            
            MessageBox1.Text = "";
            MessageBox1.FocusTextBox();

            //Add a user activity for this channel:
            
            var guild = ServerList.SelectedItem as SimpleGuild;
            var channel = ChannelList.SelectedItem as SimpleChannel;      
            Task.Run(async ()=>{
                if (App.CurrentGuildIsDM)
                {
                    await UserActivityManager.GenerateActivityAsync("@me", channel.Name, channel.ImageURL, channel.Id,"");
                }
                else
                {
                    await UserActivityManager.GenerateActivityAsync(guild.Id, guild.Name, guild.ImageURL, channel.Id, "#"+channel.Name);
                }
            });
            
        }

        private void TypingStarted(object sender, TextChangedEventArgs e)
        {
            App.StartTyping(App.CurrentChannelId);
        }

        private void MessageBox1_OpenAdvanced(object sender, Controls.MessageBox.OpenAdvancedArgs e)
        {
            if(e == null)
                App.NavigateToMessageEditor(MessageBox1.Text, false);
            else
                App.NavigateToMessageEditor(MessageBox1.Text, e.Paste);
            MessageBox1.Text = "";
        }

        private void ToggleMemberPane(object sender, RoutedEventArgs e)
        {
            sideDrawer.ToggleRight();
        }

        private void NavToAbout(object sender, RoutedEventArgs e)
        {
            App.NavigateToAbout();
        }

        private void NavToIAPs(object sender, RoutedEventArgs e)
        {
            App.NavigateToIAP();
        }

        private void ChannelHeader_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!App.CurrentGuildIsDM)
            {
                App.NavigateToChannelTopic(LocalState.Guilds[App.CurrentGuildId].channels[App.CurrentChannelId].raw);
            }
        }

        private void ScrollViewer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            App.UniversalPointerDown(e);
        }
        #endregion

        public GroupedObservableCollection<HoistRole, Member> memberscvs;

        private void ItemsStackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            messageStacker = sender as ItemsStackPanel;
        }

        private void WhatsNewClick(object sender, RoutedEventArgs e)
        {
            App.NavigateToAbout(true);
        }

        private void OpenFriendPanel(object sender, TappedRoutedEventArgs e)
        {
            App.CurrentChannelId = null;
            ClearMessageArea();
            FriendsItem.IsSelected = true;
            if (ChannelList.SelectedItem != null && ChannelList.SelectedItem is SimpleChannel)
            {
                (ChannelList.SelectedItem as SimpleChannel).IsSelected = false;
            }
            ChannelList.SelectedIndex = -1;
            friendPanel.Visibility = Visibility.Visible;
            MoreNewMessageIndicator.Visibility = Visibility.Collapsed;
            sideDrawer.CloseLeft();
            
        }

        private void HideBadge_Completed(object sender, object e)
        {

        }

        private void cmdBar_Opening(object sender, object e)
        {
            ChannelTopic.LineHeight = 12;
        }

        private void cmdBar_Closing(object sender, object e)
        {
            ChannelTopic.LineHeight = 24;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MessageScrollviewer = Common.GetScrollViewer(MessageList);
            if (MessageScrollviewer != null)
            {
                MessageScrollviewer.ViewChanged += MessageScrollviewer_ViewChanged;
            }
        }

        //private void TextBlock_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    GatewayManager.Gateway.UpdateStatus("online", null, new Game() { Name = PlayingBox.Text });
        //}

        private void ServerList_GotFocus(object sender, RoutedEventArgs e)
        {
            YHint.Show();
        }

        private void ServerList_LostFocus(object sender, RoutedEventArgs e)
        {
           // ChannelList.SelectedItem = ChannelList.Items.FirstOrDefault(x => ((SimpleChannel)x).Id == App.CurrentChannelId);
        }

        private void sideDrawer_SecondaryLeftFocused_1(object sender, EventArgs e)
        {
            if (App.CinematicMode)
            {
                if (ChannelList.SelectedItem != null)
                {
                    ListViewItem item = (ListViewItem)ChannelList.ContainerFromItem(ChannelList.SelectedItem);
                    item.Focus(FocusState.Keyboard);
                }
                else
                    ChannelList.Focus(FocusState.Keyboard);
            } 
            
        }

        private void ChannelList_GotFocus(object sender, RoutedEventArgs e)
        {
           // YHint.Show();
        }

        private void ChannelList_LostFocus(object sender, RoutedEventArgs e)
        {
            //ServerList.SelectedItem = ServerList.Items.FirstOrDefault(x => ((SimpleGuild)x).Id == App.CurrentGuildId);    
        }

        private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            //Delete account
        }

        private void AppBarButton_Click_2(object sender, RoutedEventArgs e)
        {
            //Add account
            SubFrameNavigator(typeof(LogScreen));
        }

        private void AccountView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //Switch account
            userFlyout.Hide();
            Storage.Settings.DefaultAccount = ((PasswordCredential)e.ClickedItem).UserName;
            Storage.SaveAppSettings();
            Loading.Show(true);
            Setup(null,null);
        }

        private async void App_ToggleCOModeHandler(object sender, EventArgs e)
        {
            if (ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.Default)
            {
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
            }
            else
            {
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
            }
            COVoice.Visibility = COVoice.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        #region KeyboardAccelorators
        private void KeyboardOpenPin(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            PinnedMessags.Flyout.ShowAt(PinnedMessags);
        }
        #endregion

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            SubFrameNavigator(typeof(SubPages.Search));
        }

        private void COVoice_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void IgnoreNewMessages_Click(object sender, RoutedEventArgs e)
        {
            MoreNewMessageIndicator.Visibility = Visibility.Collapsed;
        }

        private void ReturnToPresent_Click(object sender, RoutedEventArgs e)
        {
            RenderMessages();
        }

        
        private void content_DragOver(object sender, DragEventArgs e)
        {
            if(App.CurrentChannelId != null)
            {
                e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;

                DroppingRectangle.Fade(1, 300).Start();
                var cX = Convert.ToSingle(DroppingRectangle.ActualWidth / 2f);
                var cY = Convert.ToSingle(DroppingRectangle.ActualHeight / 2f);
                DroppingRectangle.Scale(1.05f, 1.05f, cX, cY, 0, 0).Start();
                DroppingRectangle.Scale(1f, 1f, cX, cY, 300).Start();
            }
           
        }

        private void content_DragLeave(object sender, DragEventArgs e)
        {
            DroppingRectangle.Fade(0, 300).Start();
            var cX = Convert.ToSingle(DroppingRectangle.ActualWidth / 2f);
            var cY = Convert.ToSingle(DroppingRectangle.ActualHeight / 2f);
            DroppingRectangle.Scale(1.05f, 1.05f, cX, cY, 300).Start();
        }

        private void content_Drop(object sender, DragEventArgs e)
        {
            if (App.CurrentChannelId != null)
            {
                SubFrameNavigator(typeof(SubPages.ExtendedMessageEditor), e.DataView);
                DroppingRectangle.Fade(0, 300).Start();
                var cX = Convert.ToSingle(DroppingRectangle.ActualWidth / 2f);
                var cY = Convert.ToSingle(DroppingRectangle.ActualHeight / 2f);
                DroppingRectangle.Scale(1.05f, 1.05f, cX, cY, 300).Start();
            }
        }

        private void MembersListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var memberItem = (ListViewItem)MembersListView.ContainerFromItem(e.ClickedItem);
            App.ShowMemberFlyout(memberItem, (e.ClickedItem as Member).Raw.User);
        }

        private void MessageBox1_OpenSpotify(object sender, RoutedEventArgs e)
        {
            SubFrameNavigator(typeof(SubPages.SpotifyShare));
        }

        private void SubFrame_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void SubFrame_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void SubFrame_LosingFocus(UIElement sender, LosingFocusEventArgs args)
        {
            if(SubFrame.Opacity == 1 && !IsParentFrame(args.NewFocusedElement))
            {
                    if (args.OldFocusedElement.GetType() == typeof(Control))
                        ((Control)args.OldFocusedElement).Focus(FocusState.Keyboard);
            }
        }
        private bool IsParentFrame(DependencyObject child)
        {
            //recursion recursion recursion recursion recursion recursion to figure out if one of the DependencyObject's parents is the SubFrame
            if (child == null || child.GetType() != typeof(Control)) return true;
            
            var childc = ((Control)child);
            if (childc.BaseUri == null) return true;
            if (childc.BaseUri.ToString().EndsWith("MainPage.xaml"))
                return false;
            else
                return true;
        }

        private void UISize_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {

        }
        Rectangle cmdBarShadow;
        private void cmdBarShadow_Loaded(object sender, RoutedEventArgs e)
        {
            cmdBarShadow = (Rectangle)sender;

            UISize_CurrentStateChanged(null, new VisualStateChangedEventArgs() { NewState = UISize.CurrentState });
        }
    }
}
