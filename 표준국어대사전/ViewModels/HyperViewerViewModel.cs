using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using 표준국어대사전.Classes;
using 표준국어대사전.Models;
using 표준국어대사전.Utils;
using 표준국어대사전.Views;

namespace 표준국어대사전.ViewModels
{
    internal class HyperViewerViewModel : INotifyPropertyChanged
    {
        private bool IsDefinitionOnlyMode;

        public HyperViewerViewModel()
        {
            this.IsDefinitionOnlyMode = false;
            this.IsMasterProgressBarVisible = Visibility.Collapsed;
            this.Query = "";
            this.IsMoreButtonVisible = Visibility.Collapsed;
            this.IsErrorMessageVisible = Visibility.Collapsed;
            this.IsDetailProgressBarVisible = Visibility.Collapsed;
            this.IsDefinitionViewerVisible = Visibility.Collapsed;
            this.IsDetailGridVisible = Visibility.Visible;
            this.IsTitleBarBackButtonVisible = Visibility.Collapsed;

            this.SearchResults = new ObservableCollection<SearchResultItem>();
            this.Definitions = new WordDetailItem(HandleHyperlinkClick);
            this.SearchResultSelectedIndex = -1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // 검색 결과
        public ObservableCollection<SearchResultItem> SearchResults { get; private set; }

        // 뜻 풀이
        public WordDetailItem Definitions { get; private set; }

        public Visibility IsMasterProgressBarVisible { get; private set; }
        public string Query { get; set; }

        public int SearchResultSelectedIndex { get; set; }
        public Visibility IsMoreButtonVisible { get; private set; }
        public Visibility IsErrorMessageVisible { get; private set; }
        public string ErrorMessageText { get; private set; }
        public Visibility IsDetailProgressBarVisible { get; private set; }
        public Visibility IsDefinitionViewerVisible { get; private set; }

        public Visibility IsDetailGridVisible { get; private set; }
        public Visibility IsTitleBarBackButtonVisible { get; private set; }


        public void SearchWords(string q)
        {
            this.Query = q;
            this.IsDefinitionOnlyMode = false;

            IsErrorMessageVisible = Visibility.Collapsed;
            RaisePropertyChanged("IsErrorMessageVisible");

            if (NetworkCheck() == false)
                return;

            // 검색어 숫자 지우기
            string query = Regex.Replace(Query, @"/[1-9]/g", "");

            // 검색 결과 지우기
            SearchResults.Clear();
            // 뜻풀이 감추기
            IsTitleBarBackButtonVisible = Visibility.Collapsed;
            SearchResultSelectedIndex = -1;
            IsDefinitionViewerVisible = Visibility.Collapsed;
            IsDetailGridVisible = Visibility.Collapsed;
            RaisePropertyChanged("IsTitleBarBackButtonVisible", "SearchResultSelectedIndex", "IsDefinitionViewerVisible", "IsDetailGridVisible");

            WordFinder wordFinder = new WordFinder(SearchResults, (Visibility visibility) => {
                IsMasterProgressBarVisible = visibility;
                RaisePropertyChanged("IsMasterProgressBarVisible");
            }, (string text) =>
            {
                ErrorMessageText = text;
                IsErrorMessageVisible = Visibility.Visible;
                RaisePropertyChanged("ErrorMessageText", "IsErrorMessageVisible");
            });
            wordFinder.GetSearchResults(1, 10, query, IsMoreButtonVisible, (visibility) => {
                IsMoreButtonVisible = visibility;
                RaisePropertyChanged("IsMoreButtonVisible");
            });
        }

        public void SearchMore()
        {
            int start = SearchResults.Last().sup_no / 10 + 1;

            WordFinder wordFinder = new WordFinder(SearchResults, (Visibility visibility) => {
                IsMasterProgressBarVisible = visibility;
                RaisePropertyChanged("IsMasterProgressBarVisible");
            }, (string text) =>
            {
                ErrorMessageText = text;
                IsErrorMessageVisible = Visibility.Visible;
                RaisePropertyChanged("ErrorMessageText", "IsErrorMessageVisible");
            });
            wordFinder.GetSearchResults(start, 10, Query, IsMoreButtonVisible, (visibility) => {
                IsMoreButtonVisible = visibility;
                RaisePropertyChanged("IsMoreButtonVisible");
            });
        }

        public async void DisplayWordDetail(string target_code, int sup_no)
        {
            this.IsDefinitionOnlyMode = true;

            if (NetworkCheck() == false)
                return;

            //뜻풀이 감추기
            IsDefinitionViewerVisible = Visibility.Collapsed;

            IsDetailGridVisible = Visibility.Visible;
            IsTitleBarBackButtonVisible = Visibility.Collapsed;
            RaisePropertyChanged("IsDefinitionViewerVisible", "IsDetailGridVisible", "IsTitleBarBackButtonVisible");

            DefinitionParser definitionParser = new DefinitionParser((Visibility visibility) => {
                IsDetailProgressBarVisible = visibility;
                RaisePropertyChanged("IsDetailProgressBarVisible");
            }, HandleHyperlinkClick);
            WordDetailItem definitionItem = await definitionParser.GetWordDetail(target_code, null, sup_no);
            if (definitionItem != null)
            {
                Definitions = definitionItem;
            }
            IsDefinitionViewerVisible = Visibility.Visible;

            RaisePropertyChanged("Definitions", "IsDefinitionViewerVisible");
        }

        public async void DisplayWordDetail(SearchResultItem clickedItem)
        {
            this.IsDefinitionOnlyMode = false;

            if (NetworkCheck() == false)
                return;

            //뜻풀이 감추기
            IsDefinitionViewerVisible = Visibility.Collapsed;

            IsDetailGridVisible = Visibility.Visible;
            IsTitleBarBackButtonVisible = Visibility.Visible;
            RaisePropertyChanged("IsDefinitionViewerVisible", "IsDetailGridVisible", "IsTitleBarBackButtonVisible");

            DefinitionParser definitionParser = new DefinitionParser((Visibility visibility) => {
                IsDetailProgressBarVisible = visibility;
                RaisePropertyChanged("IsDetailProgressBarVisible");
            }, HandleHyperlinkClick);
            WordDetailItem definitionItem = await definitionParser.GetWordDetail(clickedItem.target_code.ToString(), clickedItem.word, clickedItem.sup_no);
            if (definitionItem != null)
            {
                Definitions = definitionItem;
            }
            IsDefinitionViewerVisible = Visibility.Visible;

            RaisePropertyChanged("Definitions", "IsDefinitionViewerVisible");
        }

        public void CloseDetailGrid()
        {
            //뜻풀이 감추기
            IsDetailGridVisible = Visibility.Collapsed;
            IsTitleBarBackButtonVisible = Visibility.Collapsed;
            SearchResultSelectedIndex = -1;
            RaisePropertyChanged("IsDetailGridVisible", "IsTitleBarBackButtonVisible", "SearchResultSelectedIndex");
        }

        public void NetStatusRefresh()
        {
            if (NetworkCheck() == true)
            {
                IsErrorMessageVisible = Visibility.Collapsed;
                RaisePropertyChanged("IsErrorMessageVisible");
            }
        }


        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        protected void RaisePropertyChanged(params string[] names)
        {
            for (int i = 0; i < names.Length; i++)
                RaisePropertyChanged(names[i]);
        }

        private void HandleHyperlinkClick(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            Hyperlink hyperlink = sender;
            if (hyperlink.FindName("HyperViewerGrid") != null)
            {
                // 현재 표시 중인 HyperViewer 존재시
                Grid HyperViewerGrid = hyperlink.FindName("HyperViewerGrid") as Grid;
                HyperViewer HyperViewer = HyperViewerGrid.Parent as HyperViewer;
                int sup_no;
                if (2 < hyperlink.Inlines.Count)
                    int.TryParse(NumberConvertor.SupToNumber((hyperlink.Inlines[1] as Run).Text), out sup_no);
                else
                    int.TryParse(Regex.Replace((hyperlink.Inlines[0] as Run).Text, "[^0-9.]", ""), out sup_no);

                string target_code = hyperlink.Inlines[hyperlink.Inlines.Count - 1].FontFamily.Source;
                if (target_code == "0")
                    HyperViewer.SearchWords((hyperlink.Inlines[0] as Run).Text);
                else
                    HyperViewer.DisplayWordDetail(target_code, sup_no);
            }
        }

        private static bool IsInternetConnected()
        {
            Windows.Networking.Connectivity.ConnectionProfile connections = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
            bool internet = (connections != null) &&
                (connections.GetNetworkConnectivityLevel() == Windows.Networking.Connectivity.NetworkConnectivityLevel.InternetAccess);
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
                //검색 결과 Listview 지우기
                SearchResults.Clear();
                //뜻풀이 감추기
                IsDefinitionViewerVisible = Visibility.Collapsed;

                var res = ResourceLoader.GetForCurrentView();
                ErrorMessageText = res.GetString("ErrorMessageNoInternet");
                IsErrorMessageVisible = Visibility.Visible;

                RaisePropertyChanged("IsDefinitionViewerVisible", "Definitions", "ErrorMessageText", "IsErrorMessageVisible");
                return false;
            }
        }
    }
}
