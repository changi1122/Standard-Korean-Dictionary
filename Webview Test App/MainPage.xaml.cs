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

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x412에 나와 있습니다.

namespace Webview_Test_App
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private string Progress(string Work)
        {
            Work = Work.Substring(Work.IndexOf("<div class=\"list\">"), Work.IndexOf("<!-- comment.jsp -->") - Work.IndexOf("<div class=\"list\">"));
            return Work;
        }

        private string CSS(string Work)
        {
            Work = "<style>::selection { background: #f00; color: #fff; }body { -ms-overflow-style: none }</style>" + Work;
            return Work;
        }

        private string PaintWordClass(string Work)
        {
            Work = Work.Replace("<span class=\"NumRG\">", "<span style=\"color: #549606;\">");
            return Work;
        }

        private string PaintUnitNum(string Work)
        {
            Work = Work.Replace("<span class=\"NumRG2\">", "<span style=\"color: #336699;\">");
            return Work;
        }

        private string PaintDefinitionNum(string Work)
        {
            Work = Work.Replace("<span class=\"NumNO\">", "<span style=\"color: #cb4a00;\">");
            return Work;
        }

        private string PaintIdiomProv(string Work)
        {
            Work = Work.Replace("<li class=\"idiom\">", "<li class=\"idiom\"><span style=\"color: #336699;\">[관용어] </span>");
            Work = Work.Replace("<li class=\"prov\">", "<li class=\"prov\"><span style=\"color: #f98217;\">[속담] </span>");
            return Work;
        }

        private string PaintRelatedWords(string Work)
        {
            Work = Work.Replace("「본」", "「본말」");
            Work = Work.Replace("「준」", "「준말」");
            Work = Work.Replace("「비」", "「비슷한말」");
            Work = Work.Replace("「반」", "「반대말」");
            Work = Work.Replace("「높」", "「높임말」");
            Work = Work.Replace("「낮」", "「낮춤말」");
            return Work;
        }

        private string ReplaceFont(string Work)
        {
            while (true)
            {
                if (Work.IndexOf("새굴림") == -1)
                    break;
                Work = Work.Replace("새굴림", "나눔바른고딕 옛한글");
            }
            return Work;
        }

        private string AddFont(string Work)
        {
            Work = "<font face=\"나눔바른고딕 옛한글\" style=\"font-size:18px\">" + Work + "</font>";

            return Work;
        }

        private string DeletePart(string Work)
        {
            /*
            Delete
                <a >
                <strong>
                <font >
                <imgsrc >
                </font>
                <div align="center" style="width: 90px; >
            Replace
                (( )) -> +Space
                &nbsp; -> Space
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
                if (Work.IndexOf("<font") == -1)
                    break;
                Work = Work.Remove(Work.IndexOf("<font"), Work.IndexOf('>', Work.IndexOf("<font")) - Work.IndexOf("<font") + 1);
            }
            while (true)
            {
                if (Work.IndexOf("</font") == -1)
                    break;
                Work = Work.Remove(Work.IndexOf("</font"), Work.IndexOf('>', Work.IndexOf("</font")) - Work.IndexOf("</font") + 1);
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
                if (Work.IndexOf("<div align=\"center\" style=\"width: 90px; ") == -1)
                    break;
                Work = Work.Remove(Work.IndexOf("<div align=\"center\" style=\"width: 90px; "), Work.IndexOf("</div>", Work.IndexOf("<div align=\"center\" style=\"width: 90px; ")) - Work.IndexOf("<div align=\"center\" style=\"width: 90px; ") + 6);
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

        private void BtnNaviToStr_Click(object sender, RoutedEventArgs e)
        {
            WebviewView.NavigateToString(TextBoxNavigateHTML.Text);
        }

        private void BtnNavi_Click(object sender, RoutedEventArgs e)
        {
            WebviewView.Navigate(new Uri(TextBoxUri.Text));
        }

        private async void BtnGetHtml_Click(object sender, RoutedEventArgs e)
        {
            string Data = await WebviewView.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });

            TextBoxGetHTML.Text = Data;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            WebviewView.GoBack();
        }

        private void BtnForward_Click(object sender, RoutedEventArgs e)
        {
            WebviewView.GoForward();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            WebviewView.Refresh();
        }

        private void BtnDeletePart_Click(object sender, RoutedEventArgs e)
        {
            TextBoxNavigateHTML.Text = DeletePart(TextBoxNavigateHTML.Text);
        }

        private void WebviewView_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            WebviewView.Navigate(args.Uri);
            args.Handled = true;
        }

        private void BtnReplaceFont_Click(object sender, RoutedEventArgs e)
        {
            TextBoxNavigateHTML.Text = ReplaceFont(TextBoxNavigateHTML.Text);
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            WebviewView.Navigate(new Uri("http://stdweb2.korean.go.kr/main.jsp"));
        }

        private void BtnAddFont_Click(object sender, RoutedEventArgs e)
        {
            TextBoxNavigateHTML.Text = AddFont(TextBoxNavigateHTML.Text);
        }

        private void BtnProgress_Click(object sender, RoutedEventArgs e)
        {
            TextBoxNavigateHTML.Text = TextBoxGetHTML.Text;
            TextBoxNavigateHTML.Text = Progress(TextBoxNavigateHTML.Text);
            TextBoxNavigateHTML.Text = DeletePart(TextBoxNavigateHTML.Text);
            TextBoxNavigateHTML.Text = AddFont(TextBoxNavigateHTML.Text);
        }

        private void BtnCSS_Click(object sender, RoutedEventArgs e)
        {
            TextBoxNavigateHTML.Text = CSS(TextBoxNavigateHTML.Text);
        }

        private void BtnPaint_Click(object sender, RoutedEventArgs e)
        {
            TextBoxNavigateHTML.Text = PaintWordClass(TextBoxNavigateHTML.Text);
            TextBoxNavigateHTML.Text = PaintUnitNum(TextBoxNavigateHTML.Text);
            TextBoxNavigateHTML.Text = PaintDefinitionNum(TextBoxNavigateHTML.Text);
            TextBoxNavigateHTML.Text = PaintIdiomProv(TextBoxNavigateHTML.Text);
            TextBoxNavigateHTML.Text = PaintRelatedWords(TextBoxNavigateHTML.Text);
        }

        private void BtnClass_Click(object sender, RoutedEventArgs e)
        {
            KorDic kd = new KorDic();
            kd.FontName = "나눔바른고딕 옛한글";

            TextBoxNavigateHTML.Text = kd.GetWordDefinitionInHtmlFormat(TextBoxGetHTML.Text);
            
            //TextBoxNavigateHTML.Text = kd.CanSoundPlay(TextBoxGetHTML.Text);
        }
    }
}
