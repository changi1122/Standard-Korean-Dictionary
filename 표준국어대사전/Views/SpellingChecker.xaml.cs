using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using 표준국어대사전.Classes;


namespace 표준국어대사전.Views
{
    public sealed partial class SpellingChecker : Page
    {
        private const string SPELLCHECKURL = "https://nara-speller.co.kr/speller";

        public SpellingChecker()
        {
            this.InitializeComponent();

            // WebView2 이벤트 등록
            WebViewMain.NavigationStarting += WebViewMain_NavigationStarting;
            WebViewMain.NavigationCompleted += WebViewMain_NavigationCompleted;
        }

        private static bool IsInternetConnected()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = (connections != null) &&
                (connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
            return internet;
        }

        private bool NetworkCheck()
        {
            if (IsInternetConnected() == true)
            {
                return true;
            }
            else
            {
                NetNoticeGrid.Visibility = Visibility.Visible;
                WebViewMain.Visibility = Visibility.Collapsed;
                return false;
            }
        }

        private async void WebViewMain_Loaded(object sender, RoutedEventArgs e)
        {
            bool value = StorageManager.GetSetting<bool>(StorageManager.SpellingCheckerAgreement);

            if (value == true)
            {
                if (NetworkCheck())
                {
                    WebViewMain.Source = new Uri(SPELLCHECKURL);
                }
            }
            else
            {
                var res = ResourceLoader.GetForCurrentView();
                var messageDialog = new MessageDialog(res.GetString("SPC_DialogText"));

                messageDialog.Commands.Add(new UICommand(res.GetString("SPC_Disagree"), new UICommandInvokedHandler(this.CommandInvokedHandler)));
                messageDialog.Commands.Add(new UICommand(res.GetString("SPC_Agree"), new UICommandInvokedHandler(this.CommandInvokedHandler)));
                messageDialog.DefaultCommandIndex = 1;
                messageDialog.CancelCommandIndex = 0;

                await messageDialog.ShowAsync();
            }
        }

        private void CommandInvokedHandler(IUICommand command)
        {
            var res = ResourceLoader.GetForCurrentView();

            if (command.Label == res.GetString("SPC_Agree"))
            {
                StorageManager.SetSetting<bool>(StorageManager.SpellingCheckerAgreement, true);
                WebViewMain.Source = new Uri(SPELLCHECKURL);
                BtnAgree.Visibility = Visibility.Collapsed;
            }
            else if(command.Label == res.GetString("SPC_Disagree"))
            {
                BtnAgree.Visibility = Visibility.Visible;
            }
        }

        private async void BtnAgree_Click(object sender, RoutedEventArgs e)
        {
            var res = ResourceLoader.GetForCurrentView();
            var messageDialog = new MessageDialog(res.GetString("SPC_DialogText"));

            messageDialog.Commands.Add(new UICommand(res.GetString("SPC_Disagree"), new UICommandInvokedHandler(this.CommandInvokedHandler)));
            messageDialog.Commands.Add(new UICommand(res.GetString("SPC_Agree"), new UICommandInvokedHandler(this.CommandInvokedHandler)));
            messageDialog.DefaultCommandIndex = 1;
            messageDialog.CancelCommandIndex = 0;

            await messageDialog.ShowAsync();
        }

        private void WebViewMain_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            WebViewProgressBar.Visibility = Visibility.Visible;
        }

        private void WebViewMain_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            WebViewProgressBar.Visibility = Visibility.Collapsed;

            if (!args.IsSuccess)
            {
                NetNoticeGrid.Visibility = Visibility.Visible;
                WebViewMain.Visibility = Visibility.Collapsed;
            }
            else
            {
                NetNoticeGrid.Visibility = Visibility.Collapsed;
                WebViewMain.Visibility = Visibility.Visible;
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkCheck() == true)
            {
                WebViewMain.Reload();
                WebViewMain.Visibility = Visibility.Visible;
                NetNoticeGrid.Visibility = Visibility.Collapsed;
            }
        }
    }
}