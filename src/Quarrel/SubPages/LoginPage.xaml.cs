﻿// Adam Dernis © 2022

using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Quarrel.ViewModels.SubPages;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Quarrel.SubPages
{
    /// <summary>
    /// The login page for Quarrel.
    /// </summary>
    public sealed partial class LoginPage : UserControl
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginPage"/> class.
        /// </summary>
        public LoginPage()
        {
            this.InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<LoginPageViewModel>();
        }

        public LoginPageViewModel ViewModel => (LoginPageViewModel)DataContext;

        private string NavigationUrl => "https://discord.com/app";

        private async void CaptchaView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            _ = sender.InvokeScriptAsync(
                "eval",
                new[]
                {
                @"
                    var pushState = history.pushState;
                    history.pushState = function () {
                        pushState.apply(history, arguments);
	                    window.external.notify('');
                    };
                ",
                });

            if (args.Uri.AbsolutePath == "/app")
            {
                string token = await GetTokenFromWebView();
                if (!string.IsNullOrEmpty(token))
                {
                    ViewModel.LoginWithToken(token);
                }
            }
        }

        private async Task<string> GetTokenFromWebView()
        {
            // Discord doesn't allow access to localStorage so create an iframe to bypass this.
            string token = await CaptchaView.InvokeScriptAsync(
                "eval",
                new[]
                {
                    @"
                    iframe = document.createElement('iframe');
                    document.body.appendChild(iframe);
                    iframe.contentWindow.localStorage.getItem('token');
                    //'<<token>>'",
                });

            // Delete token for future
            await CaptchaView.InvokeScriptAsync(
                "eval",
                new[]
                {
                    @"iframe.contentWindow.localStorage.removeItem('token')"
                });

            return string.IsNullOrEmpty(token) ? string.Empty : token.Trim('"');
        }
    }
}
