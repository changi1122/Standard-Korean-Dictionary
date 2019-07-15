using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace 표준국어대사전.Controls
{
    public sealed partial class ConMultiSearch : UserControl
    {
        public static readonly DependencyProperty MultiSearchProperty = DependencyProperty.Register(
            nameof(MultiSearch), typeof(IEnumerable<bool>), typeof(ConMultiSearch), new PropertyMetadata(default(IEnumerable<bool>)));

        public IEnumerable<bool> MultiSearch
        {
            get { return (IEnumerable<bool>)GetValue(MultiSearchProperty); }
            set { SetValue(MultiSearchProperty, value); }
        }

        public ConMultiSearch()
        {
            this.InitializeComponent();

            SubWebView.Navigate(new Uri("https://stdict.korean.go.kr/main/main.do"));
            NetworkCheck();
        }

        public static bool IsInternetConnected()
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
                SubWebView.Visibility = Visibility.Collapsed;
                return false;
            }
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkCheck() == true)
            {
                SubWebView.Navigate(new Uri("https://stdict.korean.go.kr/main/main.do"));
                SubWebView.Visibility = Visibility.Visible;
                NetNoticeGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkCheck() == true)
            {
                SubWebView.Refresh();
                SubWebView.Visibility = Visibility.Visible;
                NetNoticeGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (SubWebView.CanGoBack == true)
                SubWebView.GoBack();
        }

        private void SubWebView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            NetNoticeGrid.Visibility = Visibility.Visible;
            SubWebView.Visibility = Visibility.Collapsed;
        }
    }
}
