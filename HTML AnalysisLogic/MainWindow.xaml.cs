using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HTML_AnalysisLogic
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        struct WordData
        {
            public string WordTitle;
            public string WordPronounce;
            public string WordDefinition;
            public string WordJavascript;
        }

        string[] WordList = new string[10];
        WordData[] w = new WordData[10];
        int Wordamount = 0;

        public MainWindow()
        {
            InitializeComponent();

            LabelVersion.Content = "Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void BtnAnalysis_Click(object sender, RoutedEventArgs e)
        {
            int a,  b;
            string full = TextBoxHTML.Text;
            string Work;
            

            if (full.IndexOf("<td class=\"sword\" background=\"/image/sq_bg.gif\">") == -1)
                return;

            a = full.IndexOf("<td class=\"sword\" background=\"/image/sq_bg.gif\">");
            b = full.IndexOf("</span>", a);
            Work = full.Substring(a + 49, b - a - 49);
            Work = Work.Remove(Work.IndexOf('<'), Work.LastIndexOf('>') - Work.IndexOf('<') + 1);
            LabelSearchResult.Content = Work;

            int MaxNum = Convert.ToInt32(Work.Substring(Work.IndexOf('(') + 1, Work.LastIndexOf('건') - Work.IndexOf('(') - 1));
            LabelMaxNum.Content = Work.Substring(Work.IndexOf('(') + 1, Work.LastIndexOf('건') - Work.IndexOf('(') - 1) + "개";
            LabelPageMax.Content = Math.Ceiling((double)MaxNum / 10);

            Work = full.Substring(full.IndexOf("<span id=\"print_area\">"));
            for (a = 0; a < 10; a++)
            {
                if (a != 0)
                    Work = Work.Replace("<p class=\"exp\">" + WordList[a-1] + "</p>", "");

                if (Work.IndexOf("<p class=\"exp\">") == -1)
                    break;

                WordList[a] = Work.Substring(Work.IndexOf("<p class=\"exp\">") + 15, Work.IndexOf("</p>") - Work.IndexOf("<p class=\"exp\">") - 15);
            }
            Wordamount = a;
            //LabelWordNum.Content = 0;
            //LabelWordDefinition.Text = WordList[0];

            for(a = 0; a < Wordamount; a++)
            {
                string[] wp = new string[2];
                wp[0] = WordList[a].Substring(0, WordList[a].IndexOf("&nbsp;&nbsp;<a"));
                wp[1] = WordList[a].Replace(wp[0] + "&nbsp;&nbsp;", "");

                w[a].WordJavascript = wp[0].Substring(wp[0].IndexOf("javascript"), wp[0].LastIndexOf(';') - wp[0].IndexOf("javascript") + 1);
                w[a].WordTitle = DeletePart(wp[0]);
                if(w[a].WordTitle.IndexOf('[') != -1)
                {
                    w[a].WordPronounce = w[a].WordTitle.Substring(w[a].WordTitle.IndexOf('['), w[a].WordTitle.IndexOf(']') + 1 - w[a].WordTitle.IndexOf('['));
                    w[a].WordTitle = w[a].WordTitle.Remove(w[a].WordTitle.IndexOf('['));
                }
                w[a].WordDefinition = DeletePart(wp[1]);
            }

            ShowWord(0);

            Work = full.Substring(full.IndexOf("<img align=\"middle\" alt=\"\" src=\"../image/icon_pre.gif\" hspace=\"5\">", full.IndexOf("<!-- paging.jsp -->")), full.IndexOf("<img align=\"middle\" alt=\"\" src=\"../image/icon_end.gif\" hspace=\"5\">", full.IndexOf("<!-- paging.jsp -->")) - full.IndexOf("<img align=\"middle\" alt=\"\" src=\"../image/icon_pre.gif\" hspace=\"5\">", full.IndexOf("<!-- paging.jsp -->") - 1));

            int BarCount = (Work.Length - Work.Replace("|", "").Length) / "|".Length; //Count "|"
            string[] page = Work.Split('|');

            for (a = 0; a <= BarCount; a++)
            {
                page[a] = DeletePart(page[a]);
                page[a] = page[a].Replace("|", "");
                page[a] = page[a].Replace(" ", "");
            }

            LabelPageNum.Content = page.Aggregate((cur, next) => cur + " " + next); //Combine pages' number
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
            
            Replace
                (( )) -> +Space
                &nbsp; -> Space
                <br> -> Enter
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
                if (Work.IndexOf("<br>") == -1)
                    break;
                Work = Work.Replace("<br>", Environment.NewLine);
            }

            return Work;
        }

        public void ShowWord(int a)
        {
            LabelWordNum.Content = a;
            LabelWordTitle.Content = w[a].WordTitle;
            LabelJavascript.Content = w[a].WordJavascript;
            LabelWordPronounce.Content = w[a].WordPronounce;
            LabelWordDefinition.Text = w[a].WordDefinition;
        }

        private void BtnLeft_Click(object sender, RoutedEventArgs e)
        {
            if (Wordamount == 0)
                return;

            int WordNowNum;
            int.TryParse(LabelWordNum.Content.ToString(), out WordNowNum);

            if (WordNowNum > 0)
            {
                ShowWord(WordNowNum - 1);
            }
        }

        private void BtnRight_Click(object sender, RoutedEventArgs e)
        {
            if (Wordamount == 0)
                return;

            int WordNowNum;
            int.TryParse(LabelWordNum.Content.ToString(), out WordNowNum);

            if (WordNowNum < Wordamount - 1)
            {
                ShowWord(WordNowNum + 1);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            TextBoxHTML.Text = "";
        }
    }
}
