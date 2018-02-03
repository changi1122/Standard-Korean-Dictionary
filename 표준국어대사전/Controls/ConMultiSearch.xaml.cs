using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

            SubWebView.Navigate(new Uri("http://stdweb2.korean.go.kr/main.jsp"));
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            SubWebView.Navigate(new Uri("http://stdweb2.korean.go.kr/main.jsp"));
            SubWebView.Visibility = Visibility.Visible;
            NetNoticeGrid.Visibility = Visibility.Collapsed;
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            SubWebView.Refresh();
            SubWebView.Visibility = Visibility.Visible;
            NetNoticeGrid.Visibility = Visibility.Collapsed;
        }

        private void SubWebView_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            SubWebView.Navigate(args.Uri);
            args.Handled = true;
        }

        private void SubWebView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            NetNoticeGrid.Visibility = Visibility.Visible;
            MainPageGrid.Visibility = Visibility.Collapsed;
            SubWebView.Visibility = Visibility.Collapsed;
        }

        private void SubWebView_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (SubWebView.Source == new Uri("http://stdweb2.korean.go.kr/main.jsp"))
            {
                MainPageGrid.Visibility = Visibility.Visible;
            }
            else
            {
                MainPageGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void TextboxSearch_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                BtnSearch_Click(this, new RoutedEventArgs());
            }
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (TextboxSearch.Text == "")
            {
                // Create a MessageDialog
                var messageDialog = new MessageDialog("찾을 말 또는 단어를 입력하세요.");
                // Show MessageDialog
                await messageDialog.ShowAsync();

                return;
            }

            //텍스트를 웹뷰에 입력합니다.
            var inputValue_Text = TextboxSearch.Text;
            var functionString_Text = string.Format(@"document.getElementsByClassName('word_ins')[0].value = '{0}';", inputValue_Text);
            await SubWebView.InvokeScriptAsync("eval", new string[] { functionString_Text });

            //검색 버튼을 누릅니다.
            var inputValue = TextboxSearch.Text;
            var functionString = string.Format(@"document.getElementById('sch1').click()");
            await SubWebView.InvokeScriptAsync("eval", new string[] { functionString });

            TextboxSearch.Text = "";
        }
    }
}
