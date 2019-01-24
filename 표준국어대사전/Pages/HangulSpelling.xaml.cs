using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace 표준국어대사전.Pages
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class HangulSpelling : Page
    {
        public HangulSpelling()
        {
            this.InitializeComponent();

            //WebViewMain.Navigate(new Uri("ms-appx-web:///Files/Han.html"));
            //WebViewMain.Navigate(new Uri("http://korean.go.kr/front/page/pageView.do?page_id=P000060&mn_id=30"));
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

        private void WebViewMain_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            /*var pointerPosition = Windows.UI.Core.CoreWindow.GetForCurrentThread().PointerPosition;
                        var Position = new Point(pointerPosition.X - Window.Current.Bounds.X - 48, pointerPosition.Y - Window.Current.Bounds.Y - 48);

                        BasicMenuFlyout.ShowAt((UIElement)e.OriginalSource, Position);*/

            /*MenuFlyout myFlyout = new MenuFlyout();
            MenuFlyoutItem firstItem = new MenuFlyoutItem { Text = "OneIt" };
            MenuFlyoutItem secondItem = new MenuFlyoutItem { Text = "TwoIt" };
            myFlyout.Items.Add(firstItem);
            myFlyout.Items.Add(secondItem);*/

            //if you only want to show in left or buttom 
            //myFlyout.Placement = FlyoutPlacementMode.Left;

            FrameworkElement senderElement = sender as FrameworkElement;

            //the code can show the flyout in your mouse click 
            BasicMenuFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }

        private async void MenuFlyoutCopy_ClickAsync(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = await WebViewMain.CaptureSelectedContentToDataPackageAsync();

            dataPackage.RequestedOperation = DataPackageOperation.Copy;
        }

        private async void MenuFlyoutCut_ClickAsync(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = await WebViewMain.CaptureSelectedContentToDataPackageAsync();

            dataPackage.RequestedOperation = DataPackageOperation.Move;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WebViewMain.Refresh();
            WebViewMain.Visibility = Visibility.Visible;
            NetNoticeGrid.Visibility = Visibility.Collapsed;
        }

        public async void OpenWithEdge(Uri uri)
        {
            var options = new Windows.System.LauncherOptions();
            options.TargetApplicationPackageFamilyName = "Microsoft.MicrosoftEdge_8wekyb3d8bbwe";

            await Windows.System.Launcher.LaunchUriAsync(uri, options);
        }

        public async void OpenWithDefaultBrowser(Uri uri)
        {
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://www.korean.go.kr/common/download.do?file_path=reportData&c_file_name=1da1b06c-2dec-4949-88cb-5e8dd28738a5_0.pdf&o_file_name=한글맞춤법%20표준어규정%20해설.pdf&downGubun=reportDataViewForm&report_seq=944");
            OpenWithEdge(uri);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://www.korean.go.kr/common/download.do?file_path=reportData&c_file_name=1da1b06c-2dec-4949-88cb-5e8dd28738a5_0.pdf&o_file_name=한글맞춤법%20표준어규정%20해설.pdf&downGubun=reportDataViewForm&report_seq=944");
            OpenWithDefaultBrowser(uri);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://www.korean.go.kr/common/download.do?file_path=etcData&c_file_name=7a06ab7c-0caa-4164-956b-eba0a049b8a9_0.pdf&o_file_name=문장%20부호%20해설(2014년).pdf");
            OpenWithEdge(uri);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://www.korean.go.kr/common/download.do?file_path=etcData&c_file_name=7a06ab7c-0caa-4164-956b-eba0a049b8a9_0.pdf&o_file_name=문장%20부호%20해설(2014년).pdf");
            OpenWithDefaultBrowser(uri);
        }
    }
}
