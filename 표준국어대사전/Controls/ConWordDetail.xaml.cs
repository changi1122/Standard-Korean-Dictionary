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

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace 표준국어대사전.Controls
{
    public sealed partial class ConWordDetail : UserControl
    {
        private ObservableCollection<WordDetailItem> wordDetail;

        public ConWordDetail()
        {
            this.InitializeComponent();

            wordDetail = new ObservableCollection<WordDetailItem>();
            wordDetail.Add(new WordDetailItem(HandleHyperlinkClick));
        }

        public async void Load_WordDetail(string target_code, int sup_no)
        {
            WordDetailSP.Visibility = Visibility.Collapsed;

            DefinitionParser definitionParser = new DefinitionParser((Visibility visibility) => {
                DetailProgressBar.Visibility = visibility;
            }, HandleHyperlinkClick);
            WordDetailItem definitionItem = await definitionParser.GetWordDetail(target_code, null, sup_no);
            if (definitionItem != null)
                wordDetail[0] = definitionItem;

            WordDetailSP.Visibility = Visibility.Visible;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            ((Grid)this.Parent).Children.Remove(this);
        }

        private void HandleHyperlinkClick(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            // TO-DO
            // '태허0' 뜻풀이의 '하늘' 링크처럼 어깨번호가 명확하지 않을 때는 검색 API로 검색 후
            // '하늘'에 해당하는 단어 중 선택할 수 있게 만들기.

            // TO-DO
            // SearchViewModel과 함수 두 개로 하는 일 분리하기

            Hyperlink hyperlink = sender;
            if (hyperlink.FindName("DetailGrid") != null)
            {
                Grid DetailGrid = hyperlink.FindName("DetailGrid") as Grid;

                if (DetailGrid.FindName("HyperViewer") == null)
                {
                    ConWordDetail HyperViewer = new ConWordDetail();
                    HyperViewer.Name = "HyperViewer";
                    int sup_no;
                    if (2 < hyperlink.Inlines.Count)
                        int.TryParse(NumberConvertor.SupToNumber((hyperlink.Inlines[1] as Run).Text), out sup_no);
                    else
                        int.TryParse(Regex.Replace((hyperlink.Inlines[0] as Run).Text, "[^0-9.]", ""), out sup_no);
                    HyperViewer.Load_WordDetail(hyperlink.Inlines[hyperlink.Inlines.Count - 1].FontFamily.Source, sup_no);

                    DetailGrid.Children.Add(HyperViewer);
                }
            }
            else if (hyperlink.FindName("ConWordDetailGrid") != null)
            {
                // 현재 표시 중인 ConWordDetail 존재시
                Grid ConWordDetailGrid = hyperlink.FindName("ConWordDetailGrid") as Grid;
                ConWordDetail HyperViewer = ConWordDetailGrid.Parent as ConWordDetail;
                int sup_no;
                if (2 < hyperlink.Inlines.Count)
                    int.TryParse(NumberConvertor.SupToNumber((hyperlink.Inlines[1] as Run).Text), out sup_no);
                else
                    int.TryParse(Regex.Replace((hyperlink.Inlines[0] as Run).Text, "[^0-9.]", ""), out sup_no);
                HyperViewer.Load_WordDetail(hyperlink.Inlines[hyperlink.Inlines.Count - 1].FontFamily.Source, sup_no);
            }
        }
    }
}
