using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 표준국어대사전.Classes;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace 표준국어대사전.Pages
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class SpellingChecker : Page
    {
        private const string SPELLCHECKURL = "https://speller.cs.pusan.ac.kr/";

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
            bool value = new DataStorageClass().GetSetting<bool>(DataStorageClass.SpellingCheckerAgreement);

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
                new DataStorageClass().SetSetting<bool>(DataStorageClass.SpellingCheckerAgreement, true);
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

        private void WebViewMain_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
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