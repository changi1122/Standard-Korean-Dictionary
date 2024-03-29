﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using 표준국어대사전.Classes;


namespace 표준국어대사전.Views
{
    public sealed partial class SpellingChecker : Page
    {
        private const string SPELLCHECKURL = "http://speller.cs.pusan.ac.kr/";

        public SpellingChecker()
        {
            this.InitializeComponent();
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
                WebViewMain.Navigate(new Uri(SPELLCHECKURL));
                NetworkCheck();
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
                WebViewMain.Navigate(new Uri(SPELLCHECKURL));
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

        private void WebViewMain_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            WebViewMain.Navigate(args.Uri);
            args.Handled = true;
        }

        private void WebViewMain_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            WebViewProgressBar.Visibility = Visibility.Visible;
        }

        private void WebViewMain_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            WebViewProgressBar.Visibility = Visibility.Collapsed;
        }
        private void WebViewMain_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            WebViewProgressBar.Visibility = Visibility.Collapsed;
            NetNoticeGrid.Visibility = Visibility.Visible;
            WebViewMain.Visibility = Visibility.Collapsed;
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkCheck() == true)
            {
                WebViewMain.Refresh();
                WebViewMain.Visibility = Visibility.Visible;
                NetNoticeGrid.Visibility = Visibility.Collapsed;
            }
        }
    }
}