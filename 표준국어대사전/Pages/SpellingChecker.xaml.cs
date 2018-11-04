using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
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
    public sealed partial class SpellingChecker : Page
    {
        public SpellingChecker()
        {
            this.InitializeComponent();

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            var value = localSettings.Values["#SpellingCheckerAgreement"];
            if (value == null)
            {
                localSettings.Values["#SpellingCheckerAgreement"] = false;
            }
        }

        private async void WebViewMain_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            var value = localSettings.Values["#SpellingCheckerAgreement"];

            if ((bool)value == true)
            {
                WebViewMain.Navigate(new Uri("http://speller.cs.pusan.ac.kr/PnuWebSpeller/Default.htm"));
            }
            else
            {
                var messageDialog = new MessageDialog("한국어 맞춤법/문법 검사기는 부산대학교 인공지능연구실과 (주)나라인포테크가 만들고, 운영합니다. 이 앱은 해당 웹 페이지에 연결할 뿐 어떠한 권리도 가지고 있지 않습니다." + Environment.NewLine + "이 검사기는 개인이나 학생만 무료로 사용할 수 있습니다.");

                messageDialog.Commands.Add(new UICommand("동의하지 않음", new UICommandInvokedHandler(this.CommandInvokedHandler)));
                messageDialog.Commands.Add(new UICommand("동의함", new UICommandInvokedHandler(this.CommandInvokedHandler)));
                messageDialog.DefaultCommandIndex = 1;
                messageDialog.CancelCommandIndex = 0;

                await messageDialog.ShowAsync();
            }
        }

        private void CommandInvokedHandler(IUICommand command)
        {
            if(command.Label == "동의함")
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["#SpellingCheckerAgreement"] = true;
                WebViewMain.Navigate(new Uri("http://speller.cs.pusan.ac.kr/PnuWebSpeller/Default.htm"));
            }
            else if(command.Label == "동의하지 않음")
            {
                AppBarButton btn = new AppBarButton();
                btn.Name = "BtnAgree";
                btn.Icon = new SymbolIcon(Symbol.View);
                btn.Label = "동의";
                btn.Click += BtnAgree_Click;

                PageCommandBar.PrimaryCommands.Add(btn);
            }
        }

        private async void BtnAgree_Click(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog("한국어 맞춤법/문법 검사기는 부산대학교 인공지능연구실과 (주)나라인포테크가 만들고, 운영합니다. 이 앱은 해당 웹 페이지에 연결할 뿐 어떠한 권리도 가지고 있지 않습니다." + Environment.NewLine + "이 검사기는 개인이나 학생만 무료로 사용할 수 있습니다.");

            messageDialog.Commands.Add(new UICommand("동의하지 않음", new UICommandInvokedHandler(this.CommandInvokedHandler2)));
            messageDialog.Commands.Add(new UICommand("동의함", new UICommandInvokedHandler(this.CommandInvokedHandler2)));
            messageDialog.DefaultCommandIndex = 1;
            messageDialog.CancelCommandIndex = 0;

            await messageDialog.ShowAsync();
        }

        private void CommandInvokedHandler2(IUICommand command)
        {
            if (command.Label == "동의함")
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["#SpellingCheckerAgreement"] = true;
                WebViewMain.Navigate(new Uri("http://speller.cs.pusan.ac.kr/PnuWebSpeller/Default.htm"));

                PageCommandBar.PrimaryCommands.Clear();
            }
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
    }
}
