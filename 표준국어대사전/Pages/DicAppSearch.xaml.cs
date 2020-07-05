using 표준국어대사전.Classes;
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
using Windows.UI.Popups;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using Windows.Management.Deployment;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking.Connectivity;
using System.Net.Http;
using System.Xml.Linq;
using Windows.ApplicationModel.Resources;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace 표준국어대사전.Pages
{
    public sealed partial class DicAppSearch : Page
    {
        const int MASTERGRID_WIDTH = 320;
        const int MULTISEARCHGRID_WIDTH = 400;

        private ObservableCollection<SearchResultItem> SearchResults;
        private ObservableCollection<WordDetailItem> wordDetail;

        private bool IsWebViewOpen = false;

        public DicAppSearch()
        {
            this.InitializeComponent();
            SearchResults = new ObservableCollection<SearchResultItem>();
            //SearchResults = new ObservableCollection<SearchResultItem>(SampleManager.GetWords());

            NetworkCheck();

            wordDetail = new ObservableCollection<WordDetailItem>();
            wordDetail.Add(new WordDetailItem());
            //wordDetail.Add(WordDetailItemSample.GetDetails());
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
                SearchBox.IsEnabled = false;

                //검색 결과 Listview 지우기
                SearchResults.Clear();
                //뜻풀이 감추기
                WordDetailViewer.Visibility = Visibility.Collapsed;
                var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                TextBlockErrorMessage.Text = res.GetString("ErrorMessageNoInternet");
                TextBlockErrorMessage.Visibility = Visibility.Visible;
                BtnNetStatusRefresh.Visibility = Visibility.Visible;
                return false;
            }
        }

        private void BtnNetStatusRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkCheck() == true)
            {
                SearchBox.IsEnabled = true;

                TextBlockErrorMessage.Visibility = Visibility.Collapsed;
                BtnNetStatusRefresh.Visibility = Visibility.Collapsed;
            }
        }

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (NetworkCheck() == false)
                return;

            var clickedItem = (SearchResultItem)e.ClickedItem;

            if (clickedItem.target_code == -321)
            {
                //더보기 누를 시 동작
                SearchClass sc = new SearchClass(SearchResults, MasterProgressBar, TextBlockErrorMessage);
                sc.GetSearchResults(clickedItem.sup_no + 1, 10, clickedItem.definition);
                SearchResults.Remove(clickedItem);
                return;
            }

            //항목 클릭시 동작

            if (BasicGrid.ActualWidth < 686)
            {
                DetailGrid.Visibility = Visibility.Visible;
                Separator.Visibility = Visibility.Visible;
                BtnMasterDetail.Visibility = Visibility.Visible;
            }

            //뜻풀이 감추기
            WordDetailViewer.Visibility = Visibility.Collapsed;

            DictionaryClass dc = new DictionaryClass(DetailProgressBar);
            WordDetailItem wd = await dc.GetWordDetail(clickedItem.target_code.ToString(), clickedItem.word, clickedItem.sup_no);
            if (wd != null)
            {
                wordDetail[0] = wd;
            }

            //뜻풀이 보이기
            WordDetailViewer.Visibility = Visibility.Visible;
        }

        private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            TextBlockErrorMessage.Visibility = Visibility.Collapsed; //검색 결과 없음. 표시 지우기

            if (NetworkCheck() == false)
                return;

            //검색어 숫자 지우기
            string searchText = SearchBox.Text;
            while (searchText.Length >= 1 && char.IsNumber(searchText[searchText.Length - 1]))
            {
                searchText = searchText.Substring(0, searchText.Length - 1);
            }

            if (searchText == "")
            {
                var res = ResourceLoader.GetForCurrentView();
                var messageDialog = new MessageDialog(res.GetString("DA_NoSearchText"));
                await messageDialog.ShowAsync();
                return;
            }

            //검색 결과 Listview 지우기
            SearchResults.Clear();
            //뜻풀이 감추기
            WordDetailViewer.Visibility = Visibility.Collapsed;
            SearchClass sc = new SearchClass(SearchResults, MasterProgressBar, TextBlockErrorMessage);
            sc.GetSearchResults(1, 10, searchText);
        }

        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SearchBox_QuerySubmitted(SearchBox, new AutoSuggestBoxQuerySubmittedEventArgs());
            }
        }

        private void BtnSubSearchClose_Click(object sender, RoutedEventArgs e)
        {
            BasicGrid.Children.Remove((UIElement)FindName("SubGrid"));
            IsWebViewOpen = false;
        }

        private void WebViewOpen(Uri uri)
        {
            if (IsWebViewOpen == true)
            {
                Grid g = (Grid)BasicGrid.FindName("SubGrid");
                WebView w = (WebView)g.FindName("SubSearch");
                w.Navigate(uri);
            }
            else
            {
                var SubGrid = new Grid
                {
                    Name = "SubGrid",
                    Margin = new Thickness(0, 40, 0, 0),
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Background = new SolidColorBrush(Windows.UI.Colors.White)
                };

                var ColorBar = new Windows.UI.Xaml.Shapes.Rectangle
                {
                    Margin = new Thickness(0, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Height = 40,
                    Fill = (SolidColorBrush)Resources["BarColor"]
                };
                SubGrid.Children.Add(ColorBar);

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
                    Foreground = new SolidColorBrush(Windows.UI.Colors.Black)
                };
                CloseBtn.Click += BtnSubSearchClose_Click;
                SubGrid.Children.Add(CloseBtn);

                var SubSearch = new WebView
                {
                    Name = "SubSearch",
                    Margin = new Thickness(1, 40, 1, 1),
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Source = uri
                };
                SubGrid.Children.Add(SubSearch);

                BasicGrid.Children.Add(SubGrid);
                IsWebViewOpen = true;
            }
        }

        private void BtnInform_Click(object sender, RoutedEventArgs e)
        {
            WebViewOpen(new Uri("https://stdict.korean.go.kr/help/popup/entry.do"));
        }

        private async void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://costudio1122.blogspot.com/p/blog-page_76.html"));
        }

        private void BtnMemo_Click(object sender, RoutedEventArgs e)
        {
            Controls.ConMemo conMemo = MasterGrid.FindName("Memo") as Controls.ConMemo;

            if (conMemo != null)
            {
                MasterGrid.Children.Remove(conMemo);
            }
            else
            {
                Controls.ConMemo memo = new Controls.ConMemo();
                memo.Name = "Memo";
                memo.VerticalAlignment = VerticalAlignment.Bottom;

                MasterGrid.Children.Add(memo);
            }
        }

        private void BasicGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //반응형
            if (BasicGrid.ActualWidth >= 686)
            {
                MasterGrid.Margin = new Thickness(0, 40, 0, 0);
                MasterGrid.Width = MASTERGRID_WIDTH;
                MasterGrid.HorizontalAlignment = HorizontalAlignment.Left;
                DetailGrid.Margin = new Thickness(MASTERGRID_WIDTH, 40, 0, 0);
                DetailGrid.Visibility = Visibility.Visible;
                if (BasicGrid.FindName("MultiSearchGrid") != null)
                {
                    Grid g = (Grid)BasicGrid.FindName("MultiSearchGrid");
                    g.HorizontalAlignment = HorizontalAlignment.Right;
                    g.Width = MULTISEARCHGRID_WIDTH;
                }
            }

            else if (BasicGrid.ActualWidth < 686)
            {
                MasterGrid.Margin = new Thickness(0, 40, 0, 0);
                MasterGrid.ClearValue(WidthProperty);
                MasterGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                DetailGrid.Margin = new Thickness(0, 40, 0, 0);
                DetailGrid.Visibility = Visibility.Collapsed;
                if (BasicGrid.FindName("MultiSearchGrid") != null)
                {
                    Grid g = (Grid)BasicGrid.FindName("MultiSearchGrid");
                    g.HorizontalAlignment = HorizontalAlignment.Stretch;
                    g.ClearValue(WidthProperty);
                }
            }

            //뒤로가기 키 지우기
            Separator.Visibility = Visibility.Collapsed;
            BtnMasterDetail.Visibility = Visibility.Collapsed;
        }

        private void BtnMasterDetail_Click(object sender, RoutedEventArgs e)
        {
            //뜻풀이 감추기
            WordDetailViewer.Visibility = Visibility.Collapsed;

            DetailGrid.Visibility = Visibility.Collapsed;
            Separator.Visibility = Visibility.Collapsed;
            BtnMasterDetail.Visibility = Visibility.Collapsed;
        }

        private void BtnMultiSearch_Click(object sender, RoutedEventArgs e)
        {
            if (BasicGrid.FindName("MultiSearchGrid") != null)
            {
                BtnMultiSearchClose_Click(sender, e);
                return;
            }

            var MultiSearchGrid = new Grid
            {
                Name = "MultiSearchGrid",
                Margin = new Thickness(0, 40, 0, 0),
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = (SolidColorBrush)Resources["BarColor"],
                Width = MULTISEARCHGRID_WIDTH
            };

            if (BasicGrid.ActualWidth < 686)
            {
                MultiSearchGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                MultiSearchGrid.ClearValue(WidthProperty);
            }

            var CloseBtn = new Button
            {
                Name = "BtnMultiSearchClose",
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Height = 40,
                Width = 40,
                FontSize = 20,
                Content = "",
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Background = null,
                Foreground = new SolidColorBrush(Windows.UI.Colors.Black)
            };
            CloseBtn.Click += BtnMultiSearchClose_Click;
            MultiSearchGrid.Children.Add(CloseBtn);

            var MultiSearchFrame = new Frame
            {
                Name = "MultiSearchFrame",
                Margin = new Thickness(1, 40, 1, 1),
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            MultiSearchFrame.Navigate(typeof(Pages.DicAppSearch));
            MultiSearchGrid.Children.Add(MultiSearchFrame);

            BasicGrid.Children.Add(MultiSearchGrid);
        }

        private void BtnMultiSearchClose_Click(object sender, RoutedEventArgs e)
        {
            BasicGrid.Children.Remove((UIElement)this.FindName("MultiSearchGrid"));
        }
    }
}