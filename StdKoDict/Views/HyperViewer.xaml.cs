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
