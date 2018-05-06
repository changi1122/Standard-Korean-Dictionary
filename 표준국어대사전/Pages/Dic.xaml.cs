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
using Windows.Storage;
using System.Threading.Tasks;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace 표준국어대사전.Pages
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class Dic : Page
    {
        bool IsOpenOriginWeb = false;
        bool IsSubSearchOpen = false;

        public Dic()
        {
            this.InitializeComponent();

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            var value = localSettings.Values["#UseOriginWeb"];
            if (value == null)
            {
                localSettings.Values["#UseOriginWeb"] = false;
            }
            if (localSettings.Values["#UseDevelopermode"] == null)
            {
                localSettings.Values["#UseDevelopermode"] = false;
            }
            IsOpenOriginWeb = (bool)localSettings.Values["#UseOriginWeb"];
            ToggleOriginWeb.IsChecked = (bool)localSettings.Values["#UseOriginWeb"];

            if (IsOpenOriginWeb == false)
                MainPageGrid.Visibility = Visibility.Visible;
            else
                MainPageGrid.Visibility = Visibility.Collapsed;

            if ((bool)localSettings.Values["#UseDevelopermode"] == false)
                BtnReadingMode.Visibility = Visibility.Collapsed;
            else
                BtnReadingMode.Visibility = Visibility.Visible;

            WebViewDic.Navigate(new Uri("http://stdweb2.korean.go.kr/main.jsp"));
        }


        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
        // AppBarButton
        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            WebViewDic.Refresh();
            WebViewDic.Visibility = Visibility.Visible;
            NetNoticeGrid.Visibility = Visibility.Collapsed;
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            WebViewDic.Navigate(new Uri("http://stdweb2.korean.go.kr/main.jsp"));
            WebViewDic.Visibility = Visibility.Visible;
            NetNoticeGrid.Visibility = Visibility.Collapsed;
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

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if(WebViewDic.CanGoBack == true)
                WebViewDic.GoBack();
        }

        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
        // AppBarButton - WebView Html Read
        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

        private async void BtnReadingMode_Click(object sender, RoutedEventArgs e)
        {
            ReadingModeGrid.Visibility = Visibility.Visible;

            string Data = await WebViewDic.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });
            /*string Work = "";

            Data = Data.Substring(Data.IndexOf("<p class=\"exp\">"));

            Work = Data.Substring(Data.IndexOf("</span>"));  //"</span>"으로 자르면 안됨. <p>, </p>
            Data = Data.Replace(Work, "");

            //구현 방향 : <p> </p>로 뜻 하나 단위로 자름. 그 뒤 수정.

            while (true)
            {
                //if (Data.IndexOf('<') == -1)
                //break;

                //Work = Data.Substring(Data.IndexOf('<'), Data.IndexOf('>') - Data.IndexOf('<') + 1);
                //Data.Replace(Work, "");

                //ReadingModeText.Text = Data;
                //break;
            }*/
            ReadingModeText.Text = Data; //임시
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
        // AppBarButton - Memo
        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

        private void BtnMemo_Click(object sender, RoutedEventArgs e)
        {
            if (MemoGrid.Visibility == Visibility.Collapsed)
                MemoGrid.Visibility = Visibility.Visible;
            else
                MemoGrid.Visibility = Visibility.Collapsed;
        }

        private void BtnMemoCopy_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
            dataPackage.SetText(TextboxMemoBox.Text);
            Clipboard.SetContent(dataPackage);
        }

        private void BtnMemoDelete_Click(object sender, RoutedEventArgs e)
        {
            TextboxMemoBox.Text = "";
        }

        private void BtnMemoMax_Click(object sender, RoutedEventArgs e)
        {
            if (MemoGrid.Width < 600)
            {
                MemoGrid.Width = MemoGrid.Width + 60;
                MemoGrid.Height = MemoGrid.Height + 56;
            }
        }

        private void BtnMemoMin_Click(object sender, RoutedEventArgs e)
        {
            if (MemoGrid.Width > 300)
            {
                MemoGrid.Width = MemoGrid.Width - 60;
                MemoGrid.Height = MemoGrid.Height - 56;
            }
        }

        private void BtnMemoClose_Click(object sender, RoutedEventArgs e)
        {
            MemoGrid.Visibility = Visibility.Collapsed;
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
            CloseBtn.Click += new RoutedEventHandler(BtnSubSearchClose_Click);
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
        // MainPageGrid
        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

        private void ToggleOriginWeb_Click(object sender, RoutedEventArgs e)
        {
            if (ToggleOriginWeb.IsChecked == true)
            {
                IsOpenOriginWeb = true;
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["#UseOriginWeb"] = IsOpenOriginWeb;
                BtnSearchOriginWeb.Visibility = Visibility.Collapsed;
                if (WebViewDic.Source == new Uri("http://stdweb2.korean.go.kr/main.jsp"))
                    WebViewDic.Refresh();
            }
            else
            {
                IsOpenOriginWeb = false;
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["#UseOriginWeb"] = IsOpenOriginWeb;
                BtnSearchOriginWeb.Visibility = Visibility.Visible;
                if (WebViewDic.Source == new Uri("http://stdweb2.korean.go.kr/main.jsp"))
                    WebViewDic.Refresh();
            }
        }

        private void BtnSearchOriginWeb_Click(object sender, RoutedEventArgs e)
        {
            ToggleOriginWeb.IsChecked = !ToggleOriginWeb.IsChecked;
            ToggleOriginWeb_Click(this, new RoutedEventArgs());
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
            await WebViewDic.InvokeScriptAsync("eval", new string[] { functionString_Text });

            //검색 버튼을 누릅니다.
            var inputValue = TextboxSearch.Text;
            var functionString = string.Format(@"document.getElementById('sch1').click()");
            await WebViewDic.InvokeScriptAsync("eval", new string[] { functionString });

            TextboxSearch.Text = "";
            /*var inputValue = TextboxSearch.Text;
            var functionString = string.Format(@"document.getElementsByClassName('word_ins')[0].value = '{0}';", inputValue);
            await WebViewDic.InvokeScriptAsync("eval", new string[] { functionString });*/

            /*var inputValue = TextboxSearch.Text;
            var functionString = string.Format(@"document.getElementById('SearchText').value = '{0}';", inputValue);
            await WebViewDic.InvokeScriptAsync("eval", new string[] { functionString });*/

            /*var functionString_Text = string.Format(@"document.getElementsByClassName('word_ins')[0].parentElement.click();");
            await WebViewDic.InvokeScriptAsync("eval", new string[] { functionString_Text });*/
        }

        private void TextboxSearch_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                BtnSearch_Click(this, new RoutedEventArgs());
            }
        }

        private void BtnInform_Click(object sender, RoutedEventArgs e)
        {
            WebViewDic.Navigate(new Uri("http://stdweb2.korean.go.kr/guide/entry.jsp"));
        }

        private void BtnSeparate_Click(object sender, RoutedEventArgs e)
        {
            SeparateSearchFlyout.ShowAt(BtnSeparate);
        }

        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
        // MainPageGrid - SeparateSearchFlyout
        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

        private void MenuFlyoutIdiom_Click(object sender, RoutedEventArgs e)
        {
            WebViewDic.Navigate(new Uri("http://stdweb2.korean.go.kr/section/idiom_list.jsp"));
        }

        private void MenuFlyoutProverb_Click(object sender, RoutedEventArgs e)
        {
            WebViewDic.Navigate(new Uri("http://stdweb2.korean.go.kr/section/proverb_list.jsp"));
        }

        private void MenuFlyoutDialect_Click(object sender, RoutedEventArgs e)
        {
            WebViewDic.Navigate(new Uri("http://stdweb2.korean.go.kr/section/region_list.jsp"));
        }

        private void MenuFlyoutCulture_Click(object sender, RoutedEventArgs e)
        {
            WebViewDic.Navigate(new Uri("http://stdweb2.korean.go.kr/section/north_list.jsp"));
        }

        private void MenuFlyoutKoreaOrigin_Click(object sender, RoutedEventArgs e)
        {
            WebViewDic.Navigate(new Uri("http://stdweb2.korean.go.kr/section/origin_list.jsp"));
        }


        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
        // WebView
        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

        private void WebViewDic_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (WebViewDic.Source == new Uri("http://stdweb2.korean.go.kr/main.jsp"))
            {
                if (IsOpenOriginWeb == false)
                {
                    MainPageGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    WebViewDic.Visibility = Visibility.Visible;
                    MainPageGrid.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                WebViewDic.Visibility = Visibility.Visible;
                MainPageGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void WebViewDic_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            string address = args.Uri.ToString();

            if (address.IndexOf("DicSoundPlayWordNo") != -1)
            {
                args.Handled = true;

                var DicSoundPlay = new WebView
                {
                    Name = "WebViewDicSoundPlay",
                    Visibility = Visibility.Collapsed
                };
                BasicGrid.Children.Add(DicSoundPlay);
                var view = (WebView)FindName("WebViewDicSoundPlay");
                view.NavigationCompleted += PlayDic;
                view.Navigate(args.Uri);
                return;
            }
            WebViewDic.Navigate(args.Uri);
            args.Handled = true;
        }

        private async void PlayDic(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            var view = (WebView)FindName("WebViewDicSoundPlay");
            var functionString = string.Format(@"document.getElementsByTagName('img').item(1).click()");
            await view.InvokeScriptAsync("eval", new string[] { functionString });

            BasicGrid.Children.Remove((UIElement)this.FindName("WebViewDicSoundPlay"));
        }

        private void WebViewDic_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            NetNoticeGrid.Visibility = Visibility.Visible;
            MainPageGrid.Visibility = Visibility.Collapsed;
            WebViewDic.Visibility = Visibility.Collapsed;
        }

        
        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
        // Layout
        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //반응형
            if (BasicGrid.ActualWidth > 648)
            {
                Logo.FontSize = 48;
            }
            else if (BasicGrid.ActualWidth <= 648 && 592 <= BasicGrid.ActualWidth)
            {
                Logo.FontSize = 42;
                TextboxSearch.Width = 415;
                BtnSearch.Margin = new Thickness(497, 136, 53, 20);
            }
            else if (BasicGrid.ActualWidth < 592)
            {
                Logo.FontSize = 37;
                TextboxSearch.Width = 355;
                BtnSearch.Margin = new Thickness(437, 136, 53, 20);
            }
        }
    }
}
