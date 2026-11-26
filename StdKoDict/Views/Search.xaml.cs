using System;
using System.IO;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Popups;
using Windows.ApplicationModel.Resources;
using Microsoft.UI.Xaml.Controls;
using 표준국어대사전.Models;
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
            this.ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            this.InitializeComponent();

            this.ViewModel.IsWidthBigEnough = BasicGrid.ActualWidth >= 686;
            this.ViewModel.NetStatusRefresh();

            // 뒤로가기 버튼 이벤트 핸들러 추가
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.BackRequested += ViewModel.CloseDetailGrid;
        }

        #region 스크롤 자동 추가 로딩
        // 검색 결과 목록이 바닥 근처까지 스크롤되면 다음 결과를 자동으로 불러온다.
        // '더 보기' 버튼은 자동 로딩이 실패했을 때 다시 시도하는 수단으로 남겨 둔다.
        private const double LOAD_MORE_THRESHOLD = 100;

        private void SearchResultScrollView_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            if (scrollViewer.ScrollableHeight > 0 && scrollViewer.ScrollableHeight - scrollViewer.VerticalOffset <= LOAD_MORE_THRESHOLD)
                ViewModel.TryLoadMore();
        }

        // 결과를 불러온 뒤 목록이 화면을 다 채우지 못해 스크롤할 수 없으면 한 번 더 불러온다.
        // 직전 로딩에서 항목이 늘지 않았으면(네트워크 오류 등) 자동으로 반복하지 않는다.
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsMasterProgressBarVisible" &&
                ViewModel.IsMasterProgressBarVisible == Visibility.Collapsed && ViewModel.LastLoadAddedItems)
            {
                // 새 항목의 레이아웃이 끝난 뒤 스크롤 가능 높이를 확인
                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                {
                    if (SearchResultScrollView.ScrollableHeight <= LOAD_MORE_THRESHOLD)
                        ViewModel.TryLoadMore();
                });
            }
        }
        #endregion

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SearchBox.Focus(FocusState.Programmatic);
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
            // 키보드 이동으로 예약된 뜻풀이 표시는 취소 (Enter·클릭으로 바로 표시하므로)
            keyboardNavigationTimer?.Stop();

            var clickedItem = e.ClickedItem as SearchResultItem;
            ViewModel.DisplayWordDetail(clickedItem);
        }

        #region 키보드로 검색 결과 이동 시 뜻풀이 표시
        // 방향키 등으로 선택을 옮기면 잠시(300ms) 멈춘 뒤 그 단어의 뜻풀이를 표시한다.
        // - 마우스 클릭, 뒤로/앞으로, 새 검색 등 코드에 의한 선택 변경에는 반응하지 않는다.
        // - 좁은 창에서는 뜻풀이가 목록을 가리므로 자동으로 열지 않는다(Enter로 열기).
        private static readonly Windows.System.VirtualKey[] NavigationKeys =
        {
            Windows.System.VirtualKey.Up, Windows.System.VirtualKey.Down,
            Windows.System.VirtualKey.Home, Windows.System.VirtualKey.End,
            Windows.System.VirtualKey.PageUp, Windows.System.VirtualKey.PageDown
        };
        private bool isKeyboardNavigation = false;
        private int indexBeforeKeyboardNavigation = -1;
        private DispatcherTimer keyboardNavigationTimer;

        private void ListviewSearchResult_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (!NavigationKeys.Contains(e.Key))
                return;

            // 연속 이동의 시작이면 이동 전 위치를 기억 (뒤로 가기 기록용)
            if (keyboardNavigationTimer == null || !keyboardNavigationTimer.IsEnabled)
                indexBeforeKeyboardNavigation = ListviewSearchResult.SelectedIndex;
            isKeyboardNavigation = true;
        }

        private void ListviewSearchResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isKeyboardNavigation)
                return;
            isKeyboardNavigation = false;

            if (!ViewModel.IsWidthBigEnough)
                return;

            if (keyboardNavigationTimer == null)
            {
                keyboardNavigationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
                keyboardNavigationTimer.Tick += KeyboardNavigationTimer_Tick;
            }
            keyboardNavigationTimer.Stop();
            keyboardNavigationTimer.Start();
        }

        private void KeyboardNavigationTimer_Tick(object sender, object e)
        {
            keyboardNavigationTimer.Stop();

            if (ListviewSearchResult.SelectedItem is SearchResultItem selectedItem)
                ViewModel.DisplayWordDetail(selectedItem, indexBeforeKeyboardNavigation);
        }
        #endregion

        private void BtnInform_Click(object sender, RoutedEventArgs e)
        {
            WebViewOpen(new Uri("https://kdict.korean.go.kr/standard/help/popup/entry.do"));
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
                WebView2 w = (WebView2)g.FindName("SubWebView");
                if (w != null)
                {
                    w.Source = uri;
                }
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

                var ColorBar = new Grid
                {
                    Margin = new Thickness(0, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Height = 33,
                    Background = Application.Current.Resources["PageBackgroundBrush"] as SolidColorBrush,
                    BorderBrush = Application.Current.Resources["BorderBrush"] as SolidColorBrush,
                    BorderThickness = new Thickness(0, 0, 0, 1)
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

                var SubWebView = new WebView2
                {
                    Name = "SubWebView",
                    Margin = new Thickness(1, 33, 1, 1),
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