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
            public int WordNum;
            public string WordTitle;
            public string WordType;
            public string WordDefinition;
            public string WordJavascript;
        }


        public MainWindow()
        {
            InitializeComponent();

            LabelVersion.Content = "Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void BtnAnalysis_Click(object sender, RoutedEventArgs e)
        {
            int a,  b;
            String full = TextBoxHTML.Text;
            String Work;
            
            a = full.IndexOf("<td class=\"sword\" background=\"/image/sq_bg.gif\">");
            b = full.IndexOf("<img src=\"/image/sq_r.gif\">", a);
            Work = full.Substring(a + 49, b - a - 49 - 29);
            Work = Work.Remove(Work.IndexOf('<'), Work.LastIndexOf('>') - Work.IndexOf('<') + 1);
            LabelSearchResult.Text = Work;

            WordData[] w = new WordData[9];

        }
    }
}
