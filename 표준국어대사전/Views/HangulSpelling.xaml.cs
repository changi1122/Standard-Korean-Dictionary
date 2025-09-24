using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using 표준국어대사전.Classes;


namespace 표준국어대사전.Views
{
    public sealed partial class HangulSpelling : Page
    {
        private const string KORNORMSURL = "https://korean.go.kr/kornorms/main/main.do";

        public HangulSpelling()
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

        private void WebViewMain_Loaded(object sender, RoutedEventArgs e)
        {
            if (NetworkCheck())
            {
                WebViewMain.Source = new Uri(KORNORMSURL);
            }
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

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkCheck() == true)
            {
                if (WebViewMain.CoreWebView2 != null)
                {
                    WebViewMain.Reload();
                }
                else
                {
                    // CoreWebView2가 초기화 되지 않았을 경우 예외 처리
                    // 이벤트 등록 후 한 번만 실행
                    TypedEventHandler<WebView2, CoreWebView2InitializedEventArgs> handler = null;
                    handler = (s, args) =>
                    {
                        WebViewMain.CoreWebView2Initialized -= handler; // 이벤트 제거
                        WebViewMain.Source = new Uri(KORNORMSURL);
                    };

                    WebViewMain.CoreWebView2Initialized += handler;
                    await WebViewMain.EnsureCoreWebView2Async();
                }
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
                WebViewMain.Source = new Uri(KORNORMSURL);
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
