using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace 표준국어대사전.Views
{
    public sealed partial class HangulSpelling : Page
    {
        public HangulSpelling()
        {
            this.InitializeComponent();

            WebViewMain.Navigate(new Uri("https://korean.go.kr/kornorms/main/main.do"));
            NetworkCheck();
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

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (WebViewMain.CanGoBack == true)
                WebViewMain.GoBack();
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkCheck() == true)
            {
                WebViewMain.Navigate(new Uri("http://kornorms.korean.go.kr/regltn/regltnView.do"));
                WebViewMain.Visibility = Visibility.Visible;
                NetNoticeGrid.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnOtherApp_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(WebViewMain.Source);
        }
    }
}
