using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Popups;
using System.Collections.ObjectModel;
using Windows.Networking.Connectivity;
using Windows.ApplicationModel.Resources;
using 표준국어대사전.Classes;
using 표준국어대사전.ViewModels;


namespace 표준국어대사전.Views
{
    public sealed partial class Search : Page
    {
        const int MASTERGRID_WIDTH = 310;
        const int MULTISEARCHGRID_WIDTH = 400;

        private bool IsWebViewOpen = false;
        private static bool IsFirstPageOpen = true;

        private SearchViewModel ViewModel;

        public Search()
        {
            this.ViewModel = new SearchViewModel(ref IsFirstPageOpen, () => { SearchBox.Focus(FocusState.Programmatic); });

            this.InitializeComponent();

            this.ViewModel.IsWidthBigEnough = BasicGrid.ActualWidth >= 686;
            this.ViewModel.NetStatusRefresh();

            // 뒤로가기 버튼 이벤트 핸들러 추가
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.BackRequested += ViewModel.CloseDetailGrid;
        }

        /// <summary>
        /// 반응형 레이아웃 적용
        /// </summary>
        private void BasicGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (BasicGrid.ActualWidth >= 686)
            {
                // 큰 창일 때
                ViewModel.IsWidthBigEnough = true;

                MasterGrid.Margin = new Thickness(10, 50, 0, 10);
                MasterGrid.Width = MASTERGRID_WIDTH;
                MasterGrid.HorizontalAlignment = HorizontalAlignment.Left;
                DetailGrid.Margin = new Thickness(MASTERGRID_WIDTH + 20, 50, 10, 0);
                ViewModel.SetDetailGridVisible();
                if (BasicGrid.FindName("MultiSearchGrid") != null)
                {
                    Grid g = (Grid)BasicGrid.FindName("MultiSearchGrid");
                    g.HorizontalAlignment = HorizontalAlignment.Right;
                    g.Width = MULTISEARCHGRID_WIDTH;
                }
            }

            else if (BasicGrid.ActualWidth < 686)
            {
                // 작은 창일 때
                ViewModel.IsWidthBigEnough = false;

                MasterGrid.Margin = new Thickness(10, 50, 10, 10);
                MasterGrid.ClearValue(WidthProperty);
                MasterGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                DetailGrid.Margin = new Thickness(10, 50, 10, 0);
                ViewModel.SetDetailGridCollapsed();
                if (BasicGrid.FindName("MultiSearchGrid") != null)
                {
                    Grid g = (Grid)BasicGrid.FindName("MultiSearchGrid");
                    g.HorizontalAlignment = HorizontalAlignment.Stretch;
                    g.ClearValue(WidthProperty);
                }
            }

            //뒤로가기 키 지우기
            ViewModel.IsTitleBarBackButtonEnabled = false;
        }

        /// <summary>
        /// 단어 검색시 실행
        /// </summary>
        private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (SearchBox.Text == "")
            {
                var res = ResourceLoader.GetForCurrentView();
                var messageDialog = new MessageDialog(res.GetString("DA_NoSearchText"));
                await messageDialog.ShowAsync();
                return;
            }

            ViewModel.SearchWords();
        }

        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                SearchBox_QuerySubmitted(SearchBox, new AutoSuggestBoxQuerySubmittedEventArgs());
        }

        /// <summary>
        /// 검색 결과 리스트뷰의 항목을 클릭시 실행
        /// </summary>
        private void ListviewSearchResult_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = e.ClickedItem as SearchResultItem;
            ViewModel.DisplayWordDetail(clickedItem);
        }

        private void BtnInform_Click(object sender, RoutedEventArgs e)
        {
            WebViewOpen(new Uri("https://stdict.korean.go.kr/help/popup/entry.do"));
        }

        private async void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://costudio1122.blogspot.com/p/blog-page_76.html"));
        }

        private void WebViewOpen(Uri uri)
        {
            if (IsWebViewOpen == true)
            {
                Grid g = (Grid)BasicGrid.FindName("SubGrid");
                WebView w = (WebView)g.FindName("SubWebView");
                w.Navigate(uri);
            }
            else
            {
                var SubGrid = new Grid
                {
                    Name = "SubGrid",
                    Margin = new Thickness(10, 50, 10, 10),
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Background = Application.Current.Resources["PageBackgroundBrush"] as SolidColorBrush,
                    CornerRadius = new CornerRadius(8),
                    BorderThickness = new Thickness(1),
                    BorderBrush = Application.Current.Resources["BorderBrush"] as SolidColorBrush
                };

                var ColorBar = new Windows.UI.Xaml.Shapes.Rectangle
                {
                    Margin = new Thickness(0, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Height = 32,
                    Fill = Application.Current.Resources["PageBackgroundBrush"] as SolidColorBrush
                };
                SubGrid.Children.Add(ColorBar);

                var CloseBtn = new Button
                {
                    Name = "BtnWebViewClose",
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Height = 32,
                    Width = 48,
                    FontSize = 16,
                    Content = "",
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    Style = Resources["AccentButtonStyle"] as Style
                };
                CloseBtn.Click += BtnWebViewClose_Click;
                SubGrid.Children.Add(CloseBtn);

                var SubWebView = new WebView
                {
                    Name = "SubWebView",
                    Margin = new Thickness(1, 32, 1, 1),
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Source = uri
                };
                SubGrid.Children.Add(SubWebView);

                BasicGrid.Children.Add(SubGrid);
                IsWebViewOpen = true;
            }
        }

        private void BtnWebViewClose_Click(object sender, RoutedEventArgs e)
        {
            BasicGrid.Children.Remove((UIElement)FindName("SubGrid"));
            IsWebViewOpen = false;
        }
    }
}