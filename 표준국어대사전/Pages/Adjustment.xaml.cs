using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class Adjustment : Page
    {
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

        public Adjustment()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2014_1-2_4.pdf");
            OpenWithEdge(uri);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2014_1-2_4.pdf");
            OpenWithDefaultBrowser(uri);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2014_3_4.pdf");
            OpenWithEdge(uri);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2014_3_4.pdf");
            OpenWithDefaultBrowser(uri);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2014_4_4.pdf");
            OpenWithEdge(uri);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2014_4_4.pdf");
            OpenWithDefaultBrowser(uri);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2015_1_4.pdf");
            OpenWithEdge(uri);
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2015_1_4.pdf");
            OpenWithDefaultBrowser(uri);
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2015_2_4.pdf");
            OpenWithEdge(uri);
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2015_2_4.pdf");
            OpenWithDefaultBrowser(uri);
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2015_3_4.pdf");
            OpenWithEdge(uri);
        }

        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2015_3_4.pdf");
            OpenWithDefaultBrowser(uri);
        }

        private void Button_Click_12(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2015_4_4.pdf");
            OpenWithEdge(uri);
        }

        private void Button_Click_13(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2015_4_4.pdf");
            OpenWithDefaultBrowser(uri);
        }

        private void Button_Click_14(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2016_1_4.pdf");
            OpenWithEdge(uri);
        }

        private void Button_Click_15(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2016_1_4.pdf");
            OpenWithDefaultBrowser(uri);
        }

        private void Button_Click_16(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2016_2_4.pdf");
            OpenWithEdge(uri);
        }

        private void Button_Click_17(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2016_2_4.pdf");
            OpenWithDefaultBrowser(uri);
        }

        private void Button_Click_18(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2016_3_4.pdf");
            OpenWithEdge(uri);
        }

        private void Button_Click_19(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2016_3_4.pdf");
            OpenWithDefaultBrowser(uri);
        }

        private void Button_Click_20(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2016_4_4.pdf");
            OpenWithEdge(uri);
        }

        private void Button_Click_21(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2016_4_4.pdf");
            OpenWithDefaultBrowser(uri);
        }

        private void Button_Click_22(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2017_1_41.pdf");
            OpenWithEdge(uri);
        }

        private void Button_Click_23(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2017_1_41.pdf");
            OpenWithDefaultBrowser(uri);
        }

        private void Button_Click_24(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/announcement.pdf");
            OpenWithEdge(uri);
        }

        private void Button_Click_25(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/announcement.pdf");
            OpenWithDefaultBrowser(uri);
        }

        private void Button_Click_26(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2017_2_4.pdf");
            OpenWithEdge(uri);
        }

        private void Button_Click_27(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2017_2_4.pdf");
            OpenWithDefaultBrowser(uri);
        }

        private void Button_Click_28(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2017_3_4.pdf");
            OpenWithEdge(uri);
        }

        private void Button_Click_29(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://stdweb1.korean.go.kr:8080/AttachFiles/notice/2017_3_4.pdf");
            OpenWithDefaultBrowser(uri);
        }
    }
}
