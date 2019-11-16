using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Management.Deployment;
using Windows.Networking.Connectivity;
using 표준국어대사전.Classes;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace 표준국어대사전.Pages
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class Dic : Page
    {
        bool IsSubSearchOpen = false;

        public Dic()
        {
            this.InitializeComponent();

            if (new DataStorageClass().GetSetting<bool>(DataStorageClass.UseDevelopermode) == false)
                BtnReadingMode.Visibility = Visibility.Collapsed;
            else
                BtnReadingMode.Visibility = Visibility.Visible;

            WebViewDic.Navigate(new Uri("https://stdict.korean.go.kr/main/main.do"));
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
                WebViewDic.Visibility = Visibility.Collapsed;
                return false;
            }
        }

        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
        // AppBarButton
        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if(NetworkCheck() == true)
            {
                WebViewDic.Refresh();
                WebViewDic.Visibility = Visibility.Visible;
                NetNoticeGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkCheck() == true)
            {
                WebViewDic.Navigate(new Uri("https://stdict.korean.go.kr/main/main.do"));
                WebViewDic.Visibility = Visibility.Visible;
                NetNoticeGrid.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnOtherApp_Click(object sender, RoutedEventArgs e)
        {
            var success = await Windows.System.Launcher.LaunchUriAsync(WebViewDic.Source);

            if (success) // URI launched
            {

            }
            else // URI launch failed
            {

            }
        }

        private async void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://costudio1122.blogspot.com/p/blog-page_76.html"));
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if(WebViewDic.CanGoBack == true)
                WebViewDic.GoBack();
        }

        private void BtnMemo_Click(object sender, RoutedEventArgs e)
        {
            if (MemoGrid.Visibility == Visibility.Collapsed)
                MemoGrid.Visibility = Visibility.Visible;
            else
                MemoGrid.Visibility = Visibility.Collapsed;
        }

        private void BtnMemoClose_Click(object sender, RoutedEventArgs e)
        {
            BtnMemo_Click(sender, e);
        }

        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
        // AppBarButton - WebView Html Read
        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

        private async void BtnReadingMode_Click(object sender, RoutedEventArgs e)
        {
            ReadingModeGrid.Visibility = Visibility.Visible;

            string Data = await WebViewDic.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });

            ReadingModeText.Text = Data;
        }

        private void ReadingModeCopy_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
            dataPackage.SetText(ReadingModeText.Text);
            Clipboard.SetContent(dataPackage);
        }

        private async void ReadingModeRefresh_Click(object sender, RoutedEventArgs e)
        {
            ReadingModeGrid.Visibility = Visibility.Visible;

            string Data = await WebViewDic.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });

            ReadingModeText.Text = Data;
        }

        private void ReadingModeClose_Click(object sender, RoutedEventArgs e)
        {
            ReadingModeGrid.Visibility = Visibility.Collapsed;
        }

        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
        // AppBarButton - MultiSearch
        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

        private void BtnMultiSearch_Click(object sender, RoutedEventArgs e)
        {
            if (IsSubSearchOpen == true)
            {
                BtnSubSearchClose_Click(this, new RoutedEventArgs());
                return;
            }

            var SubGrid = new Grid
            {
                Name = "SubGrid",
                Margin = new Thickness(0, ActualHeight * 3 / 10, 0, 0),
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = (SolidColorBrush)Resources["BarColor"]
            };

            var CloseBtn = new Button
            {
                Name = "BtnSubSearchClose",
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Height = 40,
                Width = 40,
                FontSize = 20,
                Content = "",
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Background = null,
                Foreground = (SolidColorBrush)Resources["Black"]
            };
            CloseBtn.Click += BtnSubSearchClose_Click;
            SubGrid.Children.Add(CloseBtn);

            var SubSearch = new Controls.ConMultiSearch
            {
                Name = "SubSearch",
                Margin = new Thickness(1, 40, 1, 1),
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            SubGrid.Children.Add(SubSearch);

            BasicGrid.Children.Add(SubGrid);

            IsSubSearchOpen = true;
        }

        private void BtnSubSearchClose_Click(object sender, RoutedEventArgs e)
        {
            BasicGrid.Children.Remove((UIElement)this.FindName("SubGrid"));

            IsSubSearchOpen = false;
        }

        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
        // WebView
        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

        private void WebViewDic_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            NetNoticeGrid.Visibility = Visibility.Visible;
            WebViewDic.Visibility = Visibility.Collapsed;
        }

        private void WebViewDic_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            WebViewDic.Navigate(args.Uri);
            args.Handled = true;
        }
    }
}
