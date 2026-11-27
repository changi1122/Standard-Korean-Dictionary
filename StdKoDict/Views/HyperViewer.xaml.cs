using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using 표준국어대사전.Classes;
using 표준국어대사전.Models;
using 표준국어대사전.Utils;
using 표준국어대사전.ViewModels;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace 표준국어대사전.Views
{
    public sealed partial class HyperViewer : UserControl
    {
        private HyperViewerViewModel ViewModel;

        public HyperViewer()
        {
            this.ViewModel = new HyperViewerViewModel();
            this.ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            this.InitializeComponent();

            this.ViewModel.NetStatusRefresh();
        }

        public void SearchWords(string query)
        {
            ViewModel.SearchWords(query);
        }

        public void DisplayWordDetail(string target_code, int sup_no)
        {
            ViewModel.DisplayWordDetail(target_code, sup_no);
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

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            ((Grid)this.Parent).Children.Remove(this);
        }

        private void ListviewSearchResult_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = e.ClickedItem as SearchResultItem;
            ViewModel.DisplayWordDetail(clickedItem);
        }
    }
}
