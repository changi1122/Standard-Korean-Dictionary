using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using 표준국어대사전.Classes;

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
            wordDetail.Add(new WordDetailItem());
        }

        public async void Load_WordDetail(string target_code, int sup_no)
        {
            WordDetailSP.Visibility = Visibility.Collapsed;

            DictionaryClass dc = new DictionaryClass(DetailProgressBar);
            WordDetailItem wd = await dc.GetWordDetail(target_code, null, sup_no);
            if (wd != null)
                wordDetail[0] = wd;

            WordDetailSP.Visibility = Visibility.Visible;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            ((Grid)this.Parent).Children.Remove(this);
        }
    }
}
