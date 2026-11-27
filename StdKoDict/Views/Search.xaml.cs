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
using 표준국어대사전.Classes;
using 표준국어대사전.Models;
using 표준국어대사전.ViewModels;


namespace 표준국어대사전.Views
{
    public sealed partial class Search : Page
    {
        const int MASTERGRID_WIDTH = 310;          // 검색 결과 목록 기본 너비
        const double MASTERGRID_MIN_WIDTH = 240;
        const double MASTERGRID_MAX_WIDTH = 700;
        const double DETAILGRID_MIN_WIDTH = 360;   // 목록을 넓혀도 뜻풀이 영역은 이만큼 남긴다
        const double SPLITTER_KEY_STEP = 20;
        const int MULTISEARCHGRID_WIDTH = 400;

        private bool IsWebViewOpen = false;
        private static bool IsFirstPageOpen = true;

        private SearchViewModel ViewModel;

        public Search()
        {
            this.ViewModel = new SearchViewModel(ref IsFirstPageOpen, () => { SearchBox.Focus(FocusState.Programmatic); });
            this.ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            this.InitializeComponent();
            InitializeSearchOptions();
            masterWidth = LoadMasterWidth();

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
                MasterGrid.HorizontalAlignment = HorizontalAlignment.Left;
                MasterSplitter.Visibility = Visibility.Visible;
                ApplyMasterWidth(masterWidth);
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
                MasterSplitter.Visibility = Visibility.Collapsed;
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

        #region 검색 결과 목록 너비 조절 (큰 창)
        private double masterWidth = MASTERGRID_WIDTH;
        private bool isDraggingSplitter = false;
        private double dragStartX;
        private double dragStartWidth;

        private static double LoadMasterWidth()
        {
            object value = Windows.Storage.ApplicationData.Current.LocalSettings.Values[StorageManager.SearchListWidth];
            return (value is double width) ? width : MASTERGRID_WIDTH;
        }

        private void SaveMasterWidth()
        {
            Windows.Storage.ApplicationData.Current.LocalSettings.Values[StorageManager.SearchListWidth] = masterWidth;
        }

        // 최소·최대 너비와, 뜻풀이 영역이 남아야 할 폭을 지키도록 제한한다
        private double ClampMasterWidth(double width)
        {
            double max = Math.Min(MASTERGRID_MAX_WIDTH, BasicGrid.ActualWidth - DETAILGRID_MIN_WIDTH - 30);
            return Math.Max(MASTERGRID_MIN_WIDTH, Math.Min(width, Math.Max(MASTERGRID_MIN_WIDTH, max)));
        }

        // 목록 너비에 맞춰 목록·분할선·뜻풀이 위치를 함께 옮긴다 (저장된 값은 바꾸지 않음)
        private void ApplyMasterWidth(double width)
        {
            double applied = ClampMasterWidth(width);
            MasterGrid.Width = applied;
            MasterSplitter.Margin = new Thickness(applied + 10, 50, 0, 10);
            DetailGrid.Margin = new Thickness(applied + 20, 50, 10, 0);
        }

        private void SetMasterWidth(double width)
        {
            masterWidth = ClampMasterWidth(width);
            ApplyMasterWidth(masterWidth);
            SaveMasterWidth();
        }

        private static void SetCursor(Windows.UI.Core.CoreCursorType type)
        {
            Windows.UI.Core.CoreWindow.GetForCurrentThread().PointerCursor = new Windows.UI.Core.CoreCursor(type, 1);
        }

        private void MasterSplitter_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            SetCursor(Windows.UI.Core.CoreCursorType.SizeWestEast);
        }

        private void MasterSplitter_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!isDraggingSplitter)
                SetCursor(Windows.UI.Core.CoreCursorType.Arrow);
        }

        private void MasterSplitter_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (MasterSplitter.CapturePointer(e.Pointer))
            {
                isDraggingSplitter = true;
                dragStartX = e.GetCurrentPoint(BasicGrid).Position.X;
                dragStartWidth = MasterGrid.ActualWidth;
                e.Handled = true;
            }
        }

        private void MasterSplitter_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!isDraggingSplitter)
                return;
            masterWidth = ClampMasterWidth(dragStartWidth + e.GetCurrentPoint(BasicGrid).Position.X - dragStartX);
            ApplyMasterWidth(masterWidth);
            e.Handled = true;
        }

        private void MasterSplitter_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            MasterSplitter.ReleasePointerCapture(e.Pointer);
            EndSplitterDrag();
            e.Handled = true;
        }

        private void MasterSplitter_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            EndSplitterDrag();
        }

        private void EndSplitterDrag()
        {
            if (!isDraggingSplitter)
                return;
            isDraggingSplitter = false;
            SaveMasterWidth();
            SetCursor(Windows.UI.Core.CoreCursorType.Arrow);
        }

        private void MasterSplitter_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            SetMasterWidth(MASTERGRID_WIDTH);
        }

        private void MasterSplitter_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Left: SetMasterWidth(MasterGrid.ActualWidth - SPLITTER_KEY_STEP); break;
                case Windows.System.VirtualKey.Right: SetMasterWidth(MasterGrid.ActualWidth + SPLITTER_KEY_STEP); break;
                case Windows.System.VirtualKey.Home: SetMasterWidth(MASTERGRID_WIDTH); break;
                default: return;
            }
            e.Handled = true;
        }
        #endregion

        #region 검색 결과 우클릭 메뉴 (단어장에 추가/삭제)
        private SearchResultItem contextMenuItem;

        private void SearchResultMenu_Opening(object sender, object e)
        {
            MenuFlyout menu = (MenuFlyout)sender;
            contextMenuItem = (menu.Target as FrameworkElement)?.DataContext as SearchResultItem;
            MenuFlyoutItem menuItem = (MenuFlyoutItem)menu.Items[0];

            var res = ResourceLoader.GetForCurrentView();
            bool isWord = contextMenuItem != null && contextMenuItem.target_code > 0; // [시작] 등은 단어가 아님
            bool isBookmarked = isWord && BookmarkManager.Contains(contextMenuItem.target_code.ToString());

            menuItem.Text = res.GetString(isBookmarked ? "BookmarkRemove" : "BookmarkAdd");
            menuItem.IsEnabled = isWord;
            ((FontIcon)menuItem.Icon).Glyph = ((char)(isBookmarked ? 0xE735 : 0xE734)).ToString();
        }

        private void SearchResultMenuBookmark_Click(object sender, RoutedEventArgs e)
        {
            SearchResultItem item = contextMenuItem;
            if (item == null || item.target_code <= 0)
                return;

            string code = item.target_code.ToString();
            if (BookmarkManager.Contains(code))
            {
                BookmarkManager.Remove(code);
                ViewModel.OnBookmarkRemoved(item);
            }
            else
            {
                BookmarkManager.Add(new BookmarkItem
                {
                    TargetCode = code,
                    Word = item.word,
                    SupNo = item.sup_no,
                    Pos = item.pos,
                    Origin = item.origin,
                    Definition = item.definition
                });
            }
        }
        #endregion

        #region 검색 옵션
        // 라디오 버튼 순서와 옵션 값 대응
        private static readonly SearchTarget[] SearchTargets = { SearchTarget.Headword, SearchTarget.Definition };
        private static readonly SearchMethod[] SearchMethods = { SearchMethod.Exact, SearchMethod.Include, SearchMethod.Start, SearchMethod.End };
        private bool isInitializingSearchOptions = false;

        private void InitializeSearchOptions()
        {
            var res = ResourceLoader.GetForCurrentView();
            isInitializingSearchOptions = true;

            RadioSearchTarget.Items.Add(res.GetString("SearchTargetHeadword"));
            RadioSearchTarget.Items.Add(res.GetString("SearchTargetDefinition"));
            foreach (SearchMethod method in SearchMethods)
                RadioSearchMethod.Items.Add(res.GetString("SearchMethod" + method.ToString()));

            // 페이지를 다시 열어도 현재 옵션이 선택되어 있도록
            RadioSearchTarget.SelectedIndex = System.Array.IndexOf(SearchTargets, ViewModel.SearchTarget);
            RadioSearchMethod.SelectedIndex = System.Array.IndexOf(SearchMethods, ViewModel.SearchMethod);
            UpdateSearchOptionControls();

            isInitializingSearchOptions = false;
        }

        private void SearchOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitializingSearchOptions || RadioSearchTarget.SelectedIndex < 0 || RadioSearchMethod.SelectedIndex < 0)
                return;

            UpdateSearchOptionControls();
            ViewModel.SetSearchOptions(SearchMethods[RadioSearchMethod.SelectedIndex], SearchTargets[RadioSearchTarget.SelectedIndex]);
        }

        // 뜻풀이 검색은 항상 포함 검색이므로 검색 방식을 고를 수 없게 하고 안내를 보여 준다
        private void UpdateSearchOptionControls()
        {
            bool isDefinition = RadioSearchTarget.SelectedIndex >= 0 && SearchTargets[RadioSearchTarget.SelectedIndex] == SearchTarget.Definition;
            RadioSearchMethod.IsEnabled = !isDefinition;
            TextSearchOptionHint.Visibility = isDefinition ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion

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
                    Background = 표준국어대사전.Classes.ThemeHelper.GetBrush("PageBackgroundBrush"),
                    CornerRadius = new CornerRadius(8),
                    BorderThickness = new Thickness(1),
                    BorderBrush = 표준국어대사전.Classes.ThemeHelper.GetBrush("BorderBrush")
                };

                var ColorBar = new Grid
                {
                    Margin = new Thickness(0, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Height = 33,
                    Background = 표준국어대사전.Classes.ThemeHelper.GetBrush("PageBackgroundBrush"),
                    BorderBrush = 표준국어대사전.Classes.ThemeHelper.GetBrush("BorderBrush"),
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