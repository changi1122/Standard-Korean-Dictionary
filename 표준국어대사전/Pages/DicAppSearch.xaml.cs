using 표준국어대사전.Models;
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

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace 표준국어대사전.Pages
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class DicAppSearch : Page
    {
        struct WordData
        {
            public string WordTitle;
            public string WordPronounce;
            public string WordDefinition;
            public string WordSubDefinition;
            public string WordJavascript;
        }

        string[] WordList = new string[10];
        WordData[] w = new WordData[10];

        private ObservableCollection<Word> Words;
        //private List<Word> Words;
        //Words = WordManager.GetWords();

        public DicAppSearch()
        {
            this.InitializeComponent();
            Words = new ObservableCollection<Word>();

            WebViewMain.Navigate(new Uri("http://stdweb2.korean.go.kr/search/List_dic.jsp"));
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var word = (Word)e.ClickedItem;
            //항목 클릭시 동작
            WordTitleItem.Content = word.WordTitle;
            WordPronounceItem.Content = word.WordPronounce;
            WordDefinitionItemTextBlock.Text = word.WordDefinition;
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
            var functionString_Text = string.Format(@"document.getElementById('SearchText').value = '{0}';", inputValue_Text);
            await WebViewMain.InvokeScriptAsync("eval", new string[] { functionString_Text });

            //검색 버튼을 누릅니다.
            var functionString = string.Format(@"fncTopSearch()");
            await WebViewMain.InvokeScriptAsync("eval", new string[] { functionString });
        }

        private void WebViewMain_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            TextboxSearch.IsEnabled = false;
            BtnSearch.IsEnabled = false;
        }

        private async void WebViewMain_NavigationCompletedAsync(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            Words.Clear();
            WordTitleItem.Content = "";
            WordPronounceItem.Content = "";
            WordDefinitionItemTextBlock.Text = "";
            w = new WordData[10];

            int a, b;
            string full = await WebViewMain.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });
            string Work;

            full = full.Replace(Environment.NewLine, " ");

            if (full.IndexOf("<p class=\"exp\">") == -1)
            {
                TextboxSearch.IsEnabled = true;
                BtnSearch.IsEnabled = true;
                return;
            }

            if (full.IndexOf("<td class=\"sword\" background=\"/image/sq_bg.gif\">") == -1)
            {
                var messageDialog = new MessageDialog("검색 실패. (Code1)");
                await messageDialog.ShowAsync();
                TextboxSearch.IsEnabled = true;
                BtnSearch.IsEnabled = true;
                return;
            }

            a = full.IndexOf("<td class=\"sword\" background=\"/image/sq_bg.gif\">");
            b = full.IndexOf("</span>", a);
            Work = full.Substring(a + 49, b - a - 49);
            Work = Work.Remove(Work.IndexOf('<'), Work.LastIndexOf('>') - Work.IndexOf('<') + 1);

            int MaxNum = Convert.ToInt32(Work.Substring(Work.IndexOf('(') + 1, Work.LastIndexOf('건') - Work.IndexOf('(') - 1));
            double LabelPageMax = Math.Ceiling((double)MaxNum / 10);
            
            Work = full.Substring(full.IndexOf("<span id=\"print_area\">"));
            for (a = 0; a < 10; a++)
            {
                if (a != 0)
                    Work = Work.Replace("<p class=\"exp\">" + WordList[a - 1] + "</p>", "");

                if (Work.IndexOf("<p class=\"exp\">") == -1)
                    break;

                WordList[a] = Work.Substring(Work.IndexOf("<p class=\"exp\">") + 15, Work.IndexOf("</p>") - Work.IndexOf("<p class=\"exp\">") - 15);
            }
            int Wordamount = a;
            
            for (a = 0; a < Wordamount; a++)
            {
                string[] wp = new string[2];
                wp[0] = WordList[a].Substring(0, WordList[a].IndexOf("&nbsp;&nbsp;<a"));
                wp[1] = WordList[a].Replace(wp[0] + "&nbsp;&nbsp;", "");
                w[a].WordJavascript = wp[0].Substring(wp[0].IndexOf("javascript"), wp[0].LastIndexOf(';') - wp[0].IndexOf("javascript") + 1);
                if (wp[0].IndexOf("<span class=\"sdblue\">") != -1)
                {
                    w[a].WordPronounce = wp[0].Substring(wp[0].IndexOf("<span class=\"sdblue\">"), wp[0].IndexOf("</span>") - wp[0].IndexOf("<span class=\"sdblue\">"));
                    w[a].WordPronounce = DeletePart(w[a].WordPronounce);
                    wp[0] = wp[0].Remove(wp[0].IndexOf("<span class=\"sdblue\">"));
                }
                if (wp[1].IndexOf('〔') != -1)
                {
                    w[a].WordSubDefinition = wp[1].Substring(wp[1].IndexOf('〔'), wp[1].LastIndexOf('〕') + 1 - wp[1].IndexOf('〔'));
                    wp[1] = wp[1].Replace(w[a].WordSubDefinition, "");
                    w[a].WordSubDefinition = DeletePart(w[a].WordSubDefinition);
                }
                w[a].WordTitle = DeletePart(wp[0]);
                w[a].WordDefinition = DeletePart(wp[1]);

                string Pronounce = w[a].WordPronounce + " " + w[a].WordSubDefinition;
                if (Pronounce.StartsWith(" "))
                {
                    Pronounce = Pronounce.Substring(1);
                }

                Words.Add(new Word { WordTitle = w[a].WordTitle, Javascript = w[a].WordJavascript, WordPronounce = Pronounce, WordDefinition = w[a].WordDefinition });
            }
            TextboxSearch.IsEnabled = true;
            BtnSearch.IsEnabled = true;
        }

        public string DeletePart(string Work)
        {
            /*
            Delete
                <a >
                <strong>
                <font >
                </ >
                <imgsrc >
                <span >
                <b>
            
            Replace
                (( )) -> +Space
                &nbsp; -> Space
                <br> -> Enter
                &lt; -> <
                &gt; -> >
            */
            while (true)
            {
                if (Work.IndexOf("<a") == -1)
                    break;
                Work = Work.Remove(Work.IndexOf("<a"), Work.IndexOf('>', Work.IndexOf("<a")) - Work.IndexOf("<a") + 1);
            }
            while (true)
            {
                if (Work.IndexOf("</") == -1)
                    break;
                Work = Work.Remove(Work.IndexOf("</"), Work.IndexOf('>', Work.IndexOf("</")) - Work.IndexOf("</") + 1);
            }
            while (true)
            {
                if (Work.IndexOf("<font") == -1)
                    break;
                Work = Work.Remove(Work.IndexOf("<font"), Work.IndexOf('>', Work.IndexOf("<font")) - Work.IndexOf("<font") + 1);
            }
            while (true)
            {
                if (Work.IndexOf("<span") == -1)
                    break;
                Work = Work.Remove(Work.IndexOf("<span"), Work.IndexOf('>', Work.IndexOf("<span")) - Work.IndexOf("<span") + 1);
            }
            while (true)
            {
                if (Work.IndexOf("<img") == -1)
                    break;
                Work = Work.Remove(Work.IndexOf("<img"), Work.IndexOf('>', Work.IndexOf("<img")) - Work.IndexOf("<img") + 1);
            }
            while (true)
            {
                if (Work.IndexOf("<imgsrc") == -1)
                    break;
                Work = Work.Remove(Work.IndexOf("<imgsrc"), Work.IndexOf('>', Work.IndexOf("<imgsrc")) - Work.IndexOf("<imgsrc") + 1);
            }
            while (true)
            {
                if (Work.IndexOf("&nbsp;") == -1)
                    break;
                Work = Work.Replace("&nbsp;", " ");
            }
            while (true)
            {
                if (Work.IndexOf("<strong>") == -1)
                    break;
                Work = Work.Replace("<strong>", "");
            }
            while (true)
            {
                if (Work.IndexOf("<b>") == -1)
                    break;
                Work = Work.Replace("<b>", "");
            }
            while (true)
            {
                if (Work.IndexOf("<br>") == -1)
                    break;
                Work = Work.Replace("<br>", Environment.NewLine);
            }
            while (true)
            {
                if (Work.IndexOf("&lt;") == -1)
                    break;
                Work = Work.Replace("&lt;", "<");
            }
            while (true)
            {
                if (Work.IndexOf("&gt;") == -1)
                    break;
                Work = Work.Replace("&gt;", ">");
            }
            return Work;
        }

        private void TextboxSearch_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                BtnSearch_Click(this, new RoutedEventArgs());
            }
        }
    }
}
