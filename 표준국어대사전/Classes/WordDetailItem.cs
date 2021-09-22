using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using System.Text.RegularExpressions;
using 표준국어대사전.Controls;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using System.IO;
using Windows.UI.Xaml.Data;

namespace 표준국어대사전.Classes
{
    public class WordDetailItem : INotifyPropertyChanged
    {
        private string FONTFAMILY = "/Fonts/NanumBarunGothic-YetHangul.ttf#NanumBarunGothic YetHangul";
        private double FONTMAGNIFICATION = 1.0;
        private bool LabWordReaderEnabled = StorageManager.GetSetting<bool>(StorageManager.LabWordReaderEnabled);

        // 필터
        public bool IsExampleVisible = true;

        #region Toolbar
        public StackPanel ToolBarSp
        {
            get
            {
                StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
                // ToolBar Buttons
                Button BtnFontDecrease= new Button { Content = "", FontSize = 14, FontFamily= new FontFamily("Segoe MDL2 Assets"), Width = 36, Height = 36, Margin = new Thickness(5, 0, 2, 0), Background = Application.Current.Resources["ApplicationPageBackgroundThemeBrush"] as SolidColorBrush };
                BtnFontDecrease.Click += BtnFontDecrease_Click;
                sp.Children.Add(BtnFontDecrease);

                Button BtnFontIncrease = new Button { Content = "", FontSize = 14, FontFamily = new FontFamily("Segoe MDL2 Assets"), Width = 36, Height = 36, Margin = new Thickness(2, 0, 2, 0), Background = Application.Current.Resources["ApplicationPageBackgroundThemeBrush"] as SolidColorBrush };
                BtnFontIncrease.Click += BtnFontIncrease_Click;
                sp.Children.Add(BtnFontIncrease);

                Button BtnFilter = new Button { Width = 80, Height = 36, Margin = new Thickness(5, 0, 5, 0), Background = Application.Current.Resources["ApplicationPageBackgroundThemeBrush"] as SolidColorBrush };
                var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                if (IsExampleVisible)
                    BtnFilter.Content = res.GetString("DC_BtnFilter_All");
                else
                    BtnFilter.Content = res.GetString("DC_BtnFilter_Meaning");
                BtnFilter.Click += BtnFilter_Click;

                sp.Children.Add(BtnFilter);

                return sp;
            }
        }


        public void BtnFontDecrease_Click(object sender, RoutedEventArgs e)
        {
            if (1.0 < FONTMAGNIFICATION)
                FONTMAGNIFICATION -= 0.1;
            StorageManager.SetSetting<double>(StorageManager.FONTMAGNIFICATION, FONTMAGNIFICATION);
            RaisePropertyChanged("wordnameRtb");
            RaisePropertyChanged("pronsconjusRtb");
            RaisePropertyChanged("pronsSp");
            RaisePropertyChanged("conjusSp");
            RaisePropertyChanged("lexicalsRtb");
            RaisePropertyChanged("detailRtb");
            RaisePropertyChanged("originRtb");
            RaisePropertyChanged("relationRtb");
            RaisePropertyChanged("homeRtb");
        }

        public void BtnFontIncrease_Click(object sender, RoutedEventArgs e)
        {
            if (FONTMAGNIFICATION < 1.6)
                FONTMAGNIFICATION += 0.1;
            StorageManager.SetSetting<double>(StorageManager.FONTMAGNIFICATION, FONTMAGNIFICATION);
            RaisePropertyChanged("wordnameRtb");
            RaisePropertyChanged("pronsconjusRtb");
            RaisePropertyChanged("pronsSp");
            RaisePropertyChanged("conjusSp");
            RaisePropertyChanged("lexicalsRtb");
            RaisePropertyChanged("detailRtb");
            RaisePropertyChanged("originRtb");
            RaisePropertyChanged("relationRtb");
            RaisePropertyChanged("homeRtb");
        }

        public void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            IsExampleVisible = (IsExampleVisible) ? false : true;

            Button BtnFilter = sender as Button;
            var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            if (IsExampleVisible)
                BtnFilter.Content = res.GetString("DC_BtnFilter_All");
            else
                BtnFilter.Content = res.GetString("DC_BtnFilter_Meaning");

            RaisePropertyChanged("detailRtb");
            RaisePropertyChanged("IsOriginVisible");
            RaisePropertyChanged("IsRelationVisible");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        //어원 문단 표시 여부
        public bool IsOriginExist = false;
        public Visibility IsOriginVisible
        {
            get { return (IsOriginExist && IsExampleVisible) ? Visibility.Visible : Visibility.Collapsed; }
        }

        //관용구 속담 문단 표시 여부
        public bool IsRelationExist = false;
        public Visibility IsRelationVisible
        {
            get { return (IsRelationExist && IsExampleVisible) ? Visibility.Visible : Visibility.Collapsed; }
        }

        public WordDetailItem()
        {
            // 글꼴
            if (StorageManager.GetSetting<string>(StorageManager.DisplayFont) == "맑은 고딕")
                FONTFAMILY = "#Malgun Gothic";
            else
                FONTFAMILY = "/Fonts/NanumBarunGothic-YetHangul.ttf#NanumBarunGothic YetHangul";
            FONTMAGNIFICATION = StorageManager.GetSetting<double>(StorageManager.FONTMAGNIFICATION);
            FONTMAGNIFICATION = Math.Truncate(FONTMAGNIFICATION * 10) / 10;
        }

        // 코드 번호
        public string target_code;
        // 단어 명
        public string wordname;
        // 어깨번호
        public int sup_no;
        // 원어
        public string original_language;

        // 발음
        public List<string> prons;
        // 활용
        public List<ConjusItem> conjus;

        // 단어 관계
        public List<LexicalItem> lexicals;

        // 관사와 하위 항목
        public List<PosItem> poses;
        // 규범 정보
        public List<string> norms;
        // 어원
        public string origin;

        // 관용구 속담
        public List<RelationItem> relations;

        // 단어명 RTB
        public RichTextBlock wordnameRtb
        {
            get
            {
                RichTextBlock rtb = new RichTextBlock();

                Paragraph para = new Paragraph { FontFamily = new FontFamily(FONTFAMILY) };
                if (wordname != null)
                    para.Inlines.Add(new Run { Text = wordname, FontSize = 32 * FONTMAGNIFICATION });
                if (sup_no != 0)
                    para.Inlines.Add(new Run { Text = ToSup(sup_no), FontSize = 32 * FONTMAGNIFICATION, FontFamily = new FontFamily("Arial") });
                if (original_language != "")
                    para.Inlines.Add(new Run { Text = $" ({original_language})", FontSize = 18 * FONTMAGNIFICATION });
                rtb.Blocks.Add(para);

                return rtb;
            }
        }

        // 발음 및 활용 RTB (단어 읽기 기능 비활성화시 사용)
        public RichTextBlock pronsconjusRtb
        {
            get
            {
                RichTextBlock rtb = new RichTextBlock { TextWrapping = TextWrapping.Wrap };

                if (LabWordReaderEnabled == false)
                {
                    // 발음
                    if (prons != null && prons.Count > 0) // 발음 정보 없는 예외 처리
                    {
                        Paragraph pProns = new Paragraph { FontSize = 16 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY), Margin = new Thickness(0, 4, 0, 4) };
                        pProns.Inlines.Add(new Run { Text = "발음  ", FontWeight = Windows.UI.Text.FontWeights.Bold });

                        pProns.Inlines.Add(new Run { Text = "[" });
                        for (int i = 0; i < prons.Count; i++)
                        {
                            pProns.Inlines.Add(new Run { Text = prons[i] });
                            if (i != prons.Count - 1)
                            {
                                pProns.Inlines.Add(new Run { Text = "/" });
                            }
                        }
                        pProns.Inlines.Add(new Run { Text = "]" });
                        rtb.Blocks.Add(pProns);
                    }

                    // 활용
                    if (conjus != null) // 활용 정보 없는 예외 처리
                    {
                        Paragraph pConjus = new Paragraph { FontSize = 16 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY), Margin = new Thickness(0, 4, 0, 4) };
                        pConjus.Inlines.Add(new Run { Text = "활용  ", FontWeight = Windows.UI.Text.FontWeights.Bold });

                        for (int i = 0; i < conjus.Count; i++)
                        {
                            pConjus.Inlines.Add(new Run { Text = conjus[i].conjus });

                            if (conjus[i].conju_prons != null)
                            {
                                pConjus.Inlines.Add(new Run { Text = "[" });
                                for (int j = 0; j < conjus[i].conju_prons.Count; j++)
                                {
                                    pConjus.Inlines.Add(new Run { Text = conjus[i].conju_prons[j] });
                                    if (j != conjus[i].conju_prons.Count - 1)
                                    {
                                        pConjus.Inlines.Add(new Run { Text = "/" });
                                    }
                                }
                                pConjus.Inlines.Add(new Run { Text = "]" });
                            }

                            if (conjus[i].abbreviations != null)
                            {
                                // 준말
                                List<AbbreviationItem> abbreviations = conjus[i].abbreviations;
                                for (int j = 0; j < abbreviations.Count; j++)
                                {
                                    pConjus.Inlines.Add(new Run { Text = $"({abbreviations[j].abbreviations}" });
                                    if (abbreviations[j].abbreviation_prons != null)
                                    {
                                        pConjus.Inlines.Add(new Run { Text = "[" });
                                        for (int k = 0; k < abbreviations[j].abbreviation_prons.Count; k++)
                                        {
                                            pConjus.Inlines.Add(new Run { Text = abbreviations[j].abbreviation_prons[k] });
                                            if (k != abbreviations[j].abbreviation_prons.Count - 1)
                                            {
                                                pConjus.Inlines.Add(new Run { Text = "/" });
                                            }
                                        }
                                        pConjus.Inlines.Add(new Run { Text = "]" });
                                    }
                                    pConjus.Inlines.Add(new Run { Text = ")" });
                                }
                            }

                            if (conjus.Count - i != 1)
                                pConjus.Inlines.Add(new Run { Text = ", " });
                        }
                        rtb.Blocks.Add(pConjus);
                    }
                }

                if (rtb.Blocks.Count == 0)
                    return null;

                return rtb;
            }
        }
        
        // 발음 SP
        public StackPanel pronsSp
        {
            get
            {
                StackPanel sp = null;

                //null 예외
                if (prons == null || prons.Count == 0)
                    return null;

                if (LabWordReaderEnabled)
                {
                    sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4) };

                    TextBlock title = new TextBlock { Text = "발음", Margin = new Thickness(0, 0, 10, 0), FontSize = 16 * FONTMAGNIFICATION, FontWeight = Windows.UI.Text.FontWeights.Bold, FontFamily = new FontFamily(FONTFAMILY) };
                    sp.Children.Add(title);
                    sp.Children.Add(new PronunciationBlock { WordItems = prons, FontFamily = new FontFamily(FONTFAMILY), IsReaderEnabled = LabWordReaderEnabled });
                }

                return sp;
            }
        }

        // 활용 SP
        public StackPanel conjusSp
        {
            get
            {
                StackPanel sp = null;

                // null 예외
                if (conjus == null)
                    return null;

                if (LabWordReaderEnabled)
                {
                    sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4) };

                    TextBlock title = new TextBlock { Text = "활용", Margin = new Thickness(0, 0, 10, 0), FontSize = 16 * FONTMAGNIFICATION, FontWeight = Windows.UI.Text.FontWeights.Bold, FontFamily = new FontFamily(FONTFAMILY) };
                    sp.Children.Add(title);

                    for (int i = 0; i < conjus.Count; i++)
                    {
                        sp.Children.Add(new TextBlock { Text = conjus[i].conjus, FontSize = 16 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) });

                        if (conjus[i].conju_prons != null)
                        {
                            sp.Children.Add(new PronunciationBlock { WordItems = conjus[i].conju_prons, IsReaderEnabled = LabWordReaderEnabled, FontFamily = new FontFamily(FONTFAMILY) });
                        }

                        if (conjus[i].abbreviations != null)
                        {
                            // 준말
                            List<AbbreviationItem> abbreviations = conjus[i].abbreviations;
                            for (int j = 0; j < abbreviations.Count; j++)
                            {
                                sp.Children.Add(new TextBlock { Text = "(" + abbreviations[j].abbreviations, FontSize = 16 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) });
                                if (abbreviations[j].abbreviation_prons != null)
                                    sp.Children.Add(new PronunciationBlock { WordItems = abbreviations[j].abbreviation_prons, IsReaderEnabled = LabWordReaderEnabled, FontFamily = new FontFamily(FONTFAMILY) });
                                sp.Children.Add(new TextBlock { Text = ")", FontSize = 16 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) });
                            }
                        }
                        if (conjus.Count - i != 1)
                            sp.Children.Add(new TextBlock { Text = ",", Margin = new Thickness(0, 0, 6, 0), FontSize = 16 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) });
                    }
                }

                return sp;
            }
        }

        // 단어 관계 RTB
        public RichTextBlock lexicalsRtb
        {
            get
            {
                RichTextBlock rtb = new RichTextBlock { Margin = new Thickness(0, 4, 0, 4) };

                // null 예외
                if (lexicals == null || lexicals.Count == 0)
                    return null;

                List<Paragraph> paras = new List<Paragraph>();
                for (int i = 0; i < lexicals.Count; i++)
                {
                    if (i != 0 && lexicals[i - 1].type == lexicals[i].type)
                    {
                        paras[paras.Count - 1].Inlines.Add(new Run { Text = ", " });

                        Hyperlink link = new Hyperlink();
                        string word = lexicals[i].word, number = "";
                        if (SplitWordnameAndNumber(ref word, ref number))
                        {
                            link.Inlines.Add(new Run { Text = word });
                            link.Inlines.Add(new Run { Text = ToSup(number), FontFamily = new FontFamily("Arial") });
                        }
                        else
                            link.Inlines.Add(new Run { Text = lexicals[i].word });
                        link.Inlines.Add(new Run { FontFamily = new FontFamily(lexicals[i].target_code) });
                        link.Click += Hyperlink_Click;
                        paras[paras.Count - 1].Inlines.Add(link);
                    }
                    else // 첫 단어
                    {
                        Paragraph para = new Paragraph { Margin = new Thickness(0, 0, 0, 4), FontSize = 16 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) };

                        para.Inlines.Add(new Run { Text = $"「{lexicals[i].type}」 ", FontWeight = Windows.UI.Text.FontWeights.Bold });

                        Hyperlink link = new Hyperlink();
                        string word = lexicals[i].word, number = "";
                        if (SplitWordnameAndNumber(ref word, ref number))
                        {
                            link.Inlines.Add(new Run { Text = word });
                            link.Inlines.Add(new Run { Text = ToSup(number), FontFamily = new FontFamily("Arial") });
                        }
                        else
                            link.Inlines.Add(new Run { Text = lexicals[i].word });
                        link.Inlines.Add(new Run { FontFamily = new FontFamily(lexicals[i].target_code) });
                        link.Click += Hyperlink_Click;
                        para.Inlines.Add(link);

                        paras.Add(para);
                    }
                }

                for (int i = 0; i < paras.Count; i++)
                {
                    paras[i].Inlines.Add(new Run { Text = " " });
                    rtb.Blocks.Add(paras[i]);
                }

                return rtb;
            }
        }

        // 뜻풀이 RTB
        public RichTextBlock detailRtb
        {
            get
            {
                RichTextBlock rtb = new RichTextBlock();

                // 예외
                if (poses == null)
                    return rtb;

                // 관사와 하위 항목
                for (int i = 0; i < poses.Count; i++)
                {
                    Paragraph para = new Paragraph { Margin = new Thickness(0, 40, 0, 0), FontSize = 20 * FONTMAGNIFICATION, FontWeight = Windows.UI.Text.FontWeights.Bold, FontFamily = new FontFamily(FONTFAMILY) };

                    // 관사 번호
                    if (1 < poses.Count)
                    {
                        para.Inlines.Add(new Run { Text = $"[{ToRoman(i + 1)}] " });
                    }

                    if (poses[i].pos != "품사 없음")
                    {
                        para.Inlines.Add(new Run { Text = $"「{poses[i].pos}」" });
                        rtb.Blocks.Add(para);
                    }

                    // 예외
                    if (poses[i].patterns == null)
                       continue;

                    // 문형 정보와 하위 항목
                    List<PatternItem> patterns = poses[i].patterns;

                    for (int j = 0; j < patterns.Count; j++)
                    {
                        Paragraph para2 = new Paragraph { Margin = new Thickness(20, 30, 0, 20), FontSize = 18 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) };

                        // 문형 번호
                        if (1 < patterns.Count)
                        {
                            para2.Inlines.Add(new Run { Text = $"({j + 1}) ", FontWeight = Windows.UI.Text.FontWeights.Bold });
                        }

                        for (int k = 0; k < patterns[j].pattern.Count; k++)
                            para2.Inlines.Add(new Run { Text = $"【{patterns[j].pattern[k]}】" });
                        // 문형 적용 문법 정보
                        if (patterns[j].grammar != null)
                            para2.Inlines.Add(new Run { Text = $" (({patterns[j].grammar}))" });

                        if (patterns.Count != 1 || patterns[j].pattern.Count > 0 || patterns[j].grammar != null)
                            rtb.Blocks.Add(para2);

                        // 예외
                        if (patterns[j].definitions == null)
                            continue;

                        // 정의
                        List<DefinitionItem> definitions = patterns[j].definitions;
                        for (int k = 0; k < definitions.Count; k++)
                        {
                            List<string> OutputList = new List<string>();

                            // cat 전문 분야
                            if (patterns[j].definitions[k].cat != null)
                                OutputList.Add("&FOS015" + $"『{patterns[j].definitions[k].cat}』 ");

                            // sense_pattern_info 정의 문형
                            if (patterns[j].definitions[k].sense_pattern_info != null)
                                OutputList.Add("&FOS015" + $"【{patterns[j].definitions[k].sense_pattern_info}】 ");

                            // sense_grammar 정의에 참고하는 말
                            if (patterns[j].definitions[k].sense_grammar != null)
                                OutputList.Add("&FOS015" + $"(({patterns[j].definitions[k].sense_grammar})) ");

                            // 글꼴 크기가 다른 경우
                            string cloneDefinition = definitions[k].definition;
                            while (true)
                            {
                                if (!cloneDefinition.Contains("<sub style='font-size:"))
                                {
                                    OutputList.Add("&FOS015" + cloneDefinition);
                                    break;
                                }
                                else
                                {
                                    if (cloneDefinition.IndexOf("<sub style='font-size:") != 0)
                                    {
                                        OutputList.Add("&FOS015" + cloneDefinition.Substring(0, cloneDefinition.IndexOf("<sub style='font-size:")));
                                        cloneDefinition = cloneDefinition.Substring(cloneDefinition.IndexOf("<sub style='font-size:"));
                                    }
                                    string fontsize = cloneDefinition.Substring(cloneDefinition.IndexOf("<sub style='font-size:") + 22, cloneDefinition.IndexOf("px;'>") - cloneDefinition.IndexOf("<sub style='font-size:") - 22);
                                    if (fontsize.Length == 1)
                                        fontsize = "00" + fontsize;
                                    else if (fontsize.Length == 2)
                                        fontsize = "0" + fontsize;
                                    OutputList.Add("&FOS" + fontsize + cloneDefinition.Substring(cloneDefinition.IndexOf("<sub style='font-size:") + 29, cloneDefinition.IndexOf("</sub>") - cloneDefinition.IndexOf("<sub style='font-size:") - 29));
                                    cloneDefinition = cloneDefinition.Substring(cloneDefinition.IndexOf("</sub>") + 6);
                                }
                            }

                            // 위첨자
                            for (int su = 0; su < OutputList.Count; su++)
                            {
                                while (OutputList[su].Contains("<sup") && OutputList[su].Contains("</sup>"))
                                {
                                    string output = OutputList[su];
                                    OutputList.RemoveAt(su);

                                    string fontsize = "015"; //default
                                    if (output.StartsWith("&FOS"))
                                        fontsize = output.Substring(4, 3);
                                    string text = output.Substring(output.IndexOf(">", output.IndexOf("<sup")) + 1, output.IndexOf("</sup>") - output.IndexOf(">", output.IndexOf("<sup")) - 1);

                                    OutputList.Insert(su, output.Substring(0, output.IndexOf("<sup")));
                                    OutputList.Insert(su + 1, "&SUP" + fontsize + text);
                                    OutputList.Insert(su + 2, "&FOS" + fontsize + output.Substring(output.IndexOf("</sup>") + 6));
                                }
                            }

                            // 하이퍼텍스트
                            for (int ht = 0; ht < OutputList.Count; ht++)
                            {
                                while (OutputList[ht].Contains("<link") && OutputList[ht].Contains("</link>"))
                                {
                                    string output = OutputList[ht];
                                    OutputList.RemoveAt(ht);

                                    string fontsize = "015"; //default
                                    if (output.StartsWith("&FOS"))
                                        fontsize = output.Substring(4, 3);
                                    string target = output.Substring(output.IndexOf("\"", output.IndexOf("<link")) + 1, output.IndexOf("\"", output.IndexOf("\"") + 1) - output.IndexOf("\"", output.IndexOf("<link")) - 1);
                                    string text = output.Substring(output.IndexOf(">", output.IndexOf("<link")) + 1, output.IndexOf("</link>") - output.IndexOf(">", output.IndexOf("<link")) - 1);

                                    OutputList.Insert(ht, output.Substring(0, output.IndexOf("<link")));
                                    OutputList.Insert(ht + 1, "&HLK" + fontsize + $"<{target}>" + text);
                                    OutputList.Insert(ht + 2, "&FOS" + fontsize + output.Substring(output.IndexOf("</link>") + 7));
                                }
                            }

                            // 이탤릭체
                            for (int it = 0; it < OutputList.Count; it++)
                            {
                                while (true)
                                {
                                    if (!OutputList[it].Contains("<I>") && !OutputList[it].Contains("<i>"))
                                        break;
                                    else if (OutputList[it].Contains("<I>"))
                                    {
                                        string output = OutputList[it];
                                        OutputList.RemoveAt(it);
                                        string fontsize = "015"; //default
                                        if (output.StartsWith("&FOS"))
                                            fontsize = output.Substring(4, 3);
                                        OutputList.Insert(it, output.Substring(0, output.IndexOf("<I>")));
                                        output = output.Substring(output.IndexOf("<I>"));
                                        OutputList.Insert(it + 1, "&ITA" + fontsize + output.Substring(output.IndexOf("<I>") + 3, output.IndexOf("</I>") - output.IndexOf("<I>") - 3));
                                        OutputList.Insert(it + 2, "&FOS" + fontsize + output.Substring(output.IndexOf("</I>") + 4));
                                    }
                                    else
                                    {
                                        string output = OutputList[it];
                                        OutputList.RemoveAt(it);
                                        string fontsize = "015"; //default
                                        if (output.StartsWith("&FOS"))
                                            fontsize = output.Substring(4, 3);
                                        OutputList.Insert(it, output.Substring(0, output.IndexOf("<i>")));
                                        output = output.Substring(output.IndexOf("<i>"));
                                        OutputList.Insert(it + 1, "&ITA" + fontsize + output.Substring(output.IndexOf("<i>") + 3, output.IndexOf("</i>") - output.IndexOf("<i>") - 3));
                                        OutputList.Insert(it + 2, "&FOS" + fontsize + output.Substring(output.IndexOf("</i>") + 4));
                                    }
                                }    
                            }

                            // 유니코드 문자
                            for (int uc = 0; uc < OutputList.Count; uc++)
                            {
                                while (OutputList[uc].Contains("<span class=\"korean-webfont\"") && OutputList[uc].Contains("</span>"))
                                {
                                    string output = OutputList[uc];
                                    OutputList.RemoveAt(uc);

                                    string fontsize = "015"; //default
                                    if (output.StartsWith("&FOS"))
                                        fontsize = output.Substring(4, 3);
                                    string text = output.Substring(output.IndexOf(">", output.IndexOf("<span class=\"korean-webfont\"")) + 1, output.IndexOf("</span>") - output.IndexOf(">", output.IndexOf("<span class=\"korean-webfont\"")) - 1);
                                    text = System.Net.WebUtility.HtmlDecode(text);

                                    OutputList.Insert(uc, output.Substring(0, output.IndexOf("<span class=\"korean-webfont\"")));
                                    OutputList.Insert(uc + 1, "&FOS" + fontsize + text);
                                    OutputList.Insert(uc + 2, "&FOS" + fontsize + output.Substring(output.IndexOf("</span>") + 7));
                                }
                            }

                            // 이미지
                            for (int im = 0; im < OutputList.Count; im++)
                            {
                                while (true)
                                {
                                    if (!OutputList[im].Contains("<img") || !OutputList[im].Contains("/>"))
                                        break;
                                    else if (OutputList[im].Contains("<img") && OutputList[im].Contains("/>"))
                                    {
                                        string output = OutputList[im];
                                        OutputList.RemoveAt(im);
                                        string fontsize = "015"; //default
                                        if (output.StartsWith("&FOS"))
                                            fontsize = output.Substring(4, 3);
                                        string source = output.Substring(output.IndexOf("src='") + 5, output.IndexOf("'", output.IndexOf("src='") + 5) - output.IndexOf("src='") - 5);
                                        OutputList.Insert(im, output.Substring(0, output.IndexOf("<img")));
                                        output = output.Substring(output.IndexOf("<img"));
                                        OutputList.Insert(im + 1, "&IMG" + fontsize + $"<{source}>");
                                        OutputList.Insert(im + 2, "&FOS" + fontsize + output.Substring(output.IndexOf("/>") + 2));
                                    }
                                }
                            }

                            Paragraph para3 = new Paragraph { Margin = new Thickness(5, 15, 0, 15), FontSize = 15 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) };

                            // 의미 번호
                            if (1 < definitions.Count)
                            {
                                para3.Inlines.Add(new Run { Text = $"「{k + 1}」 ", Foreground = new SolidColorBrush(Windows.UI.Colors.Red) });
                            }

                            for (int o = 0; o < OutputList.Count(); o++)
                            {
                                if (OutputList[o].StartsWith("&FOS"))
                                {
                                    int fontsize = int.Parse(OutputList[o].Substring(4, 3));
                                    para3.Inlines.Add(new Run { Text = OutputList[o].Substring(7), FontSize = fontsize * FONTMAGNIFICATION });
                                }
                                else if (OutputList[o].StartsWith("&SUP"))
                                {
                                    int fontsize = int.Parse(OutputList[o].Substring(4, 3));
                                    para3.Inlines.Add(new Run { Text = ToSup(OutputList[o].Substring(7)), FontSize = fontsize * FONTMAGNIFICATION, FontFamily = new FontFamily("Arial") });
                                }
                                else if (OutputList[o].StartsWith("&ITA"))
                                {
                                    int fontsize = int.Parse(OutputList[o].Substring(4, 3));
                                    para3.Inlines.Add(new Run { Text = OutputList[o].Substring(7), FontSize = fontsize * FONTMAGNIFICATION, FontStyle = Windows.UI.Text.FontStyle.Italic });
                                }
                                else if (OutputList[o].StartsWith("&HLK"))
                                {
                                    int fontsize = int.Parse(OutputList[o].Substring(4, 3));

                                    Hyperlink link = new Hyperlink();
                                    link.Inlines.Add(new Run { Text = OutputList[o].Substring(OutputList[o].IndexOf(">") + 1), FontSize = fontsize * FONTMAGNIFICATION });
                                    link.Inlines.Add(new Run { FontFamily = new FontFamily(OutputList[o].Substring(OutputList[o].IndexOf("<") + 1, OutputList[o].IndexOf(">") - OutputList[o].IndexOf("<") - 1)) });
                                    link.Click += Hyperlink_Click;
                                    para3.Inlines.Add(link);
                                }
                                else if (OutputList[o].StartsWith("&IMG"))
                                {
                                    int fontsize = int.Parse(OutputList[o].Substring(4, 3));
                                    string source = OutputList[o].Substring(OutputList[o].IndexOf("<") + 1, OutputList[o].IndexOf(">") - OutputList[o].IndexOf("<") - 1);

                                    InlineUIContainer container = new InlineUIContainer();
                                    Image image = new Image();
                                    
                                    if (source.StartsWith("data:image"))
                                    {
                                        source = source.Substring(source.IndexOf("base64,") + 7);
                                        BitmapImage bitmap = Base64StringToBitmap(source);
                                        image.Source = bitmap;
                                        image.Width = bitmap.PixelWidth;
                                        image.Height = bitmap.PixelHeight;
                                        image.Margin = new Thickness(0, 3, 0, -3);

                                        container.Child = image;
                                        para3.Inlines.Add(container);
                                    }
                                }
                            }

                            // 동의어 관계
                            if (IsExampleVisible && definitions[k].lexicals != null)
                            {
                                List<LexicalItem> lexicals = definitions[k].lexicals;
                                for (int l = 0; l < lexicals.Count; l++)
                                {
                                    if(lexicals[l].type == "동의어")
                                    {
                                        if(l != 0 && lexicals[l - 1].type == "동의어")
                                            para3.Inlines.Add(new Run { Text = ", " });
                                        else // 첫 단어
                                            para3.Inlines.Add(new Run { Text = " ≒ " });

                                        Hyperlink link = new Hyperlink();
                                        string word = lexicals[l].word, number = "";
                                        if (SplitWordnameAndNumber(ref word, ref number))
                                        {
                                            link.Inlines.Add(new Run { Text = word });
                                            link.Inlines.Add(new Run { Text = ToSup(number), FontFamily = new FontFamily("Arial") });
                                        }
                                        else
                                            link.Inlines.Add(new Run { Text = lexicals[l].word });
                                        link.Inlines.Add(new Run { FontFamily = new FontFamily(lexicals[l].target_code) });
                                        link.Click += Hyperlink_Click;
                                        para3.Inlines.Add(link);
                                    }
                                }
                                para3.Inlines.Add(new Run { Text = "." });
                            }

                            // 학명
                            if (patterns[j].definitions[k].scientific_name != null)
                                para3.Inlines.Add(new Run { Text = $"  ({patterns[j].definitions[k].scientific_name})", FontSize = 15 * FONTMAGNIFICATION, FontStyle = Windows.UI.Text.FontStyle.Italic });

                            rtb.Blocks.Add(para3);


                            // 예시
                            if (IsExampleVisible && definitions[k].examples != null)
                            {
                                List<string> examples = definitions[k].examples;
                                for (int l = 0; l < examples.Count; l++)
                                {
                                    Paragraph para4 = new Paragraph { Margin = new Thickness(30, 4, 0, 4), FontSize = 14 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) };
                                    para4.Inlines.Add(new Run { Text = "· " + examples[l] });
                                    rtb.Blocks.Add(para4);
                                }
                            }

                            // 단어 관계
                            if(IsExampleVisible && definitions[k].lexicals != null)
                            {
                                List<LexicalItem> lexicals = definitions[k].lexicals;

                                List<Paragraph> paras = new List<Paragraph>();
                                for (int l = 0; l < lexicals.Count; l++)
                                {
                                    if (lexicals[l].type == "동의어")
                                        continue;

                                    if (l != 0 && lexicals[l - 1].type == lexicals[l].type)
                                    {
                                        paras[paras.Count - 1].Inlines.Add(new Run { Text = ", " });

                                        Hyperlink link = new Hyperlink();
                                        string word = lexicals[l].word, number = "";
                                        if (SplitWordnameAndNumber(ref word, ref number))
                                        {
                                            link.Inlines.Add(new Run { Text = word });
                                            link.Inlines.Add(new Run { Text = ToSup(number), FontFamily = new FontFamily("Arial") });
                                        }
                                        else
                                            link.Inlines.Add(new Run { Text = lexicals[l].word });
                                        link.Inlines.Add(new Run { FontFamily = new FontFamily(lexicals[l].target_code) });
                                        link.Click += Hyperlink_Click;
                                        paras[paras.Count - 1].Inlines.Add(link);
                                    }
                                    else //첫 단어
                                    {
                                        Paragraph para4 = new Paragraph { Margin = new Thickness(30, 8, 0, 4) , FontSize = 13 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) };

                                        para4.Inlines.Add(new Run { Text = $"「{lexicals[l].type}」 " });

                                        Hyperlink link = new Hyperlink();
                                        string word = lexicals[l].word, number = "";
                                        if (SplitWordnameAndNumber(ref word, ref number))
                                        {
                                            link.Inlines.Add(new Run { Text = word });
                                            link.Inlines.Add(new Run { Text = ToSup(number), FontFamily = new FontFamily("Arial") });
                                        }
                                        else
                                            link.Inlines.Add(new Run { Text = lexicals[l].word });
                                        link.Inlines.Add(new Run { FontFamily = new FontFamily(lexicals[l].target_code) });
                                        link.Click += Hyperlink_Click;
                                        para4.Inlines.Add(link);

                                        paras.Add(para4);
                                    }
                                }
                                
                                for (int l = 0; l < paras.Count; l++)
                                {
                                    paras[l].Inlines.Add(new Run { Text = " " });
                                    rtb.Blocks.Add(paras[l]);
                                }
                            }
                        }
                    }
                }

                // norm 규범 정보
                if(norms != null)
                {
                    Paragraph para = new Paragraph { Margin = new Thickness(0, 30, 0, 0), FontSize = 14 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) };

                    for (int i = 0; i < norms.Count; i++)
                    {
                        para.Inlines.Add(new Run { Text = "※ " + norms[i] });
                    }
                    rtb.Blocks.Add(para);
                }

                return rtb;
            }
        }

        // 어원 RTB
        public RichTextBlock originRtb
        {
            get
            {
                RichTextBlock rtb = new RichTextBlock();

                if (origin != null)
                {
                    Paragraph title = new Paragraph { Margin = new Thickness(0, 15, 0, 25), FontSize = 18 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) };
                    title.Inlines.Add(new Run { Text = "▹ 어원" });
                    rtb.Blocks.Add(title);

                    Paragraph para = new Paragraph { Margin = new Thickness(0, 0, 0, 5), FontSize = 15 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) };
                    para.Inlines.Add(new Run { Text = origin });
                    rtb.Blocks.Add(para);
                }

                return rtb;
            }
        }

        // 관용구 속담 RTB
        public RichTextBlock relationRtb
        {
            get
            {
                RichTextBlock rtb = new RichTextBlock { HorizontalAlignment = HorizontalAlignment.Left };

                if (relations != null)
                {
                    Paragraph title = new Paragraph { Margin = new Thickness(0, 15, 0, 25), FontSize = 18 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) };
                    title.Inlines.Add(new Run { Text = "▹ 관용구/속담" });
                    rtb.Blocks.Add(title);

                    for (int i = 0; i < relations.Count; i++)
                    {
                        Paragraph para = new Paragraph { Margin = new Thickness(0, 15, 0, 15), FontSize = 15 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) };

                        string type = "";
                        if (relations[i].type == ERelationType.idiom)
                            type = "관용구";
                        else if (relations[i].type == ERelationType.proverb)
                            type = "속담";

                        para.Inlines.Add(new Run { Text = $"[{type}] ", Foreground = new SolidColorBrush(Windows.UI.Colors.Blue) });
                        Hyperlink link = new Hyperlink();
                        link.Inlines.Add(new Run { Text = relations[i].word });
                        link.Inlines.Add(new Run { FontFamily = new FontFamily(relations[i].target_code) });
                        link.Click += Hyperlink_Click;
                        para.Inlines.Add(link);
                        para.Inlines.Add(new Run { Text = " " });
                        rtb.Blocks.Add(para);
                    }
                }

                return rtb;
            }
        }

        // 시작 화면 RTB
        public RichTextBlock homeRtb
        {
            get
            {
                RichTextBlock rtb = new RichTextBlock { HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 40, 0, 0) };

                if (target_code == "-200")
                {
                    // 시작 화면인 경우

                    // 최근 검색
                    Paragraph subTitle1 = new Paragraph { FontFamily = new FontFamily(FONTFAMILY) };
                    subTitle1.Inlines.Add(new Run { Text = "최근 검색", FontSize = 20 * FONTMAGNIFICATION, FontWeight = Windows.UI.Text.FontWeights.Bold });
                    subTitle1.Inlines.Add(new Run { Text = "   " });
                    Hyperlink linkClear = new Hyperlink();
                    linkClear.Inlines.Add(new Run { Text = "지우기", FontSize = 15 * FONTMAGNIFICATION, Foreground = new SolidColorBrush(Windows.UI.Colors.Red) });
                    linkClear.Click += RecentWordClear_Click;
                    subTitle1.Inlines.Add(linkClear);
                    subTitle1.Inlines.Add(new Run { Text = " " });
                    rtb.Blocks.Add(subTitle1);

                    Paragraph recentSearch = new Paragraph { Margin = new Thickness(5, 15, 0, 15), FontSize = 15 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) };

                    List<string> recentWords = RecentWordManager.GetWords();
                    for (int i = recentWords.Count - 1; 0 <= i; i--)
                    {
                        Hyperlink link = new Hyperlink();
                        link.Inlines.Add(new Run { Text = recentWords[i] });
                        link.Click += RecentWordLink_Click;
                        recentSearch.Inlines.Add(link);
                        if (i != 0)
                            recentSearch.Inlines.Add(new Run { Text = ", " });
                        else
                            recentSearch.Inlines.Add(new Run { Text = " " });
                    }
                    if (recentWords.Count == 0)
                    {
                        recentSearch.Inlines.Add(new Run { Text = "최근 검색어 없음.", Foreground = new SolidColorBrush(Windows.UI.Colors.Gray) });
                    }
                    
                    rtb.Blocks.Add(recentSearch);

                    // 도움말 및 관련 링크
                    Paragraph subTitle2 = new Paragraph { Margin = new Thickness(0, 40, 0, 0), FontSize = 20 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) };
                    subTitle2.Inlines.Add(new Run { Text = "도움말 및 관련 링크", FontWeight = Windows.UI.Text.FontWeights.Bold });
                    rtb.Blocks.Add(subTitle2);

                    // 웹 브라우저에서 열기
                    Paragraph pOpenWeb = new Paragraph { Margin = new Thickness(5, 15, 0, 15), FontSize = 15 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) };
                    Hyperlink hOpenWeb = new Hyperlink { NavigateUri = new Uri("https://stdict.korean.go.kr/") };
                    hOpenWeb.Inlines.Add(new Run { Text = "웹 브라우저에서 열기" });
                    pOpenWeb.Inlines.Add(hOpenWeb);
                    pOpenWeb.Inlines.Add(new Run { Text = " " });
                    rtb.Blocks.Add(pOpenWeb);

                    // 일러두기
                    Paragraph pInform = new Paragraph { Margin = new Thickness(5, 15, 0, 15), FontSize = 15 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) };
                    Hyperlink hInform = new Hyperlink { NavigateUri = new Uri("https://stdict.korean.go.kr/help/popup/entry.do") };
                    hInform.Inlines.Add(new Run { Text = "일러두기" });
                    pInform.Inlines.Add(hInform);
                    pInform.Inlines.Add(new Run { Text = " " });
                    rtb.Blocks.Add(pInform);

                    //앱 도움말
                    Paragraph pHelp = new Paragraph { Margin = new Thickness(5, 15, 0, 15), FontSize = 15 * FONTMAGNIFICATION, FontFamily = new FontFamily(FONTFAMILY) };
                    Hyperlink hHelp = new Hyperlink { NavigateUri = new Uri("https://costudio1122.blogspot.com/p/blog-page_76.html") };
                    hHelp.Inlines.Add(new Run { Text = "앱 도움말" });
                    pHelp.Inlines.Add(hHelp);
                    pHelp.Inlines.Add(new Run { Text = " " });
                    rtb.Blocks.Add(pHelp);
                }

                return rtb;
            }
        }


        // 활용과 활용의 발음 클래스
        public class ConjusItem
        {
            public string conjus;
            public List<string> conju_prons;
            public List<AbbreviationItem> abbreviations;
        }
        // 준말과 줄말의 발음 클래스
        public class AbbreviationItem
        {
            public string abbreviations;
            public List<string> abbreviation_prons;
        }
        // 관사와 하위 항목 클래스
        public class PosItem
        {
            public string pos;
            public List<PatternItem> patterns;
        }
        // 문형 정보와 하위 항목 클래스
        public class PatternItem
        {
            public List<string> pattern;
            public string grammar;
            public List<DefinitionItem> definitions;
        }
        // 정의와 예시 클래스
        public class DefinitionItem
        {
            public string cat;
            public string sense_pattern_info;
            public string sense_grammar;
            public string definition;
            public string scientific_name;
            public List<string> examples;
            public List<LexicalItem> lexicals;
        }
        // 관용구 속담 클래스
        public class RelationItem
        {
            public string word;
            public ERelationType type;
            public string target_code;
        }
        // 어휘 관계 클래스
        public class LexicalItem
        {
            public string word;
            public string type;
            public string target_code;
        }


        private void Hyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            // TO-DO
            // '태허0' 뜻풀이의 '하늘' 링크처럼 어깨번호가 명확하지 않을 때는 검색 API로 검색 후
            // '하늘'에 해당하는 단어 중 선택할 수 있게 만들기.

            Hyperlink hyperlink = sender as Hyperlink;
            if (hyperlink.FindName("DetailGrid") != null)
            {
                Grid DetailGrid = hyperlink.FindName("DetailGrid") as Grid;

                if (DetailGrid.FindName("HyperViewer") == null)
                {
                    ConWordDetail HyperViewer = new ConWordDetail();
                    HyperViewer.Name = "HyperViewer";
                    int sup_no;
                    if (2 < hyperlink.Inlines.Count)
                        int.TryParse(ToNumber((hyperlink.Inlines[1] as Run).Text), out sup_no);
                    else
                        int.TryParse(Regex.Replace((hyperlink.Inlines[0] as Run).Text, "[^0-9.]", ""), out sup_no);
                    HyperViewer.Load_WordDetail(hyperlink.Inlines[hyperlink.Inlines.Count - 1].FontFamily.Source, sup_no);

                    DetailGrid.Children.Add(HyperViewer);
                }
            }
            else if (hyperlink.FindName("ConWordDetailGrid") != null)
            {
                Grid ConWordDetailGrid = hyperlink.FindName("ConWordDetailGrid") as Grid;
                ConWordDetail HyperViewer = ConWordDetailGrid.Parent as ConWordDetail;
                int sup_no;
                if (2 < hyperlink.Inlines.Count)
                    int.TryParse(ToNumber((hyperlink.Inlines[1] as Run).Text), out sup_no);
                else
                    int.TryParse(Regex.Replace((hyperlink.Inlines[0] as Run).Text, "[^0-9.]", ""), out sup_no);
                HyperViewer.Load_WordDetail(hyperlink.Inlines[hyperlink.Inlines.Count - 1].FontFamily.Source, sup_no);
            }
        }

        private void RecentWordLink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            Hyperlink hyperlink = sender as Hyperlink;
            if (hyperlink.FindName("SearchBox") != null)
            {
                AutoSuggestBox SearchBox = hyperlink.FindName("SearchBox") as AutoSuggestBox;
                Run run = hyperlink.Inlines[0] as Run;
                if (run != null)
                    SearchBox.Text = run.Text;

                Grid BasicGrid = hyperlink.FindName("BasicGrid") as Grid;
                Grid DetailGrid = hyperlink.FindName("DetailGrid") as Grid;
                AppBarButton BtnCloseDetail = hyperlink.FindName("BtnCloseDetail") as AppBarButton;
                if (BasicGrid != null && DetailGrid != null && BtnCloseDetail != null)
                {
                    if (BasicGrid.ActualWidth < 686)
                    {
                        DetailGrid.Visibility = Visibility.Collapsed;
                        BtnCloseDetail.Visibility = Visibility.Collapsed;
                    }
                }

                SearchBox.Focus(FocusState.Programmatic);
            }
        }

        private void RecentWordClear_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            RecentWordManager.Clear();
            RaisePropertyChanged("homeRtb");
        }

        // 로마자 변환
        private string ToRoman(int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value between 1 and 3999");
            if (number < 1) return string.Empty;
            if (number >= 1000) return "M" + ToRoman(number - 1000);
            if (number >= 900) return "CM" + ToRoman(number - 900);
            if (number >= 500) return "D" + ToRoman(number - 500);
            if (number >= 400) return "CD" + ToRoman(number - 400);
            if (number >= 100) return "C" + ToRoman(number - 100);
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            throw new ArgumentOutOfRangeException("something bad happened");
        }

        // 위 첨자로 변환
        private string ToSup(int number)
        {
            return ToSup(number.ToString());
        }
        private string ToSup(string number)
        {
            StringBuilder numString = new StringBuilder(number);
            for (int i = 0; i < numString.Length; i++)
            {
                switch(numString[i])
                {
                    case '1':
                        numString[i] = '¹';
                        break;
                    case '2':
                        numString[i] = '²';
                        break;
                    case '3':
                        numString[i] = '³';
                        break;
                    case '4':
                        numString[i] = '⁴';
                        break;
                    case '5':
                        numString[i] = '⁵';
                        break;
                    case '6':
                        numString[i] = '⁶';
                        break;
                    case '7':
                        numString[i] = '⁷';
                        break;
                    case '8':
                        numString[i] = '⁸';
                        break;
                    case '9':
                        numString[i] = '⁹';
                        break;
                    case '0':
                        numString[i] = '⁰';
                        break;
                    case '-':
                    case '－':
                        numString[i] = '⁻';
                        break;
                    case '(':
                        numString[i] = '⁽';
                        break;
                    case ')':
                        numString[i] = '⁾';
                        break;
                    case '+':
                        numString[i] = '⁺';
                        break;
                    default:
                        break;
                }
            }
            return numString.ToString();
        }

        // 위 첨자를 일반 숫자로 변환
        private string ToNumber(string sup_no)
        {
            StringBuilder numString = new StringBuilder(sup_no);
            for (int i = 0; i < numString.Length; i++)
            {
                switch (numString[i])
                {
                    case '¹':
                        numString[i] = '1';
                        break;
                    case '²':
                        numString[i] = '2';
                        break;
                    case '³':
                        numString[i] = '3';
                        break;
                    case '⁴':
                        numString[i] = '4';
                        break;
                    case '⁵':
                        numString[i] = '5';
                        break;
                    case '⁶':
                        numString[i] = '6';
                        break;
                    case '⁷':
                        numString[i] = '7';
                        break;
                    case '⁸':
                        numString[i] = '8';
                        break;
                    case '⁹':
                        numString[i] = '9';
                        break;
                    case '⁰':
                        numString[i] = '0';
                        break;
                    case '⁻':
                        numString[i] = '-';
                        break;
                    case '⁽':
                        numString[i] = '(';
                        break;
                    case '⁾':
                        numString[i] = ')';
                        break;
                    case '⁺':
                        numString[i] = '+';
                        break;
                    default:
                        break;
                }
            }
            return numString.ToString();
        }

        // 단어 이름과 어깨번호 분리 후 위 첨자로 변환
        private bool SplitWordnameAndNumber(ref string word, ref string number)
        {
            int numberStartIndex;
            for (numberStartIndex = word.Length - 1; 0 <= numberStartIndex; numberStartIndex--)
            {
                if (!char.IsDigit(word[numberStartIndex]))
                    break;
            }

            if (numberStartIndex < word.Length - 1)
            {
                number = word.Substring(numberStartIndex + 1).TrimStart(new char[] { '0' });
                word = word.Substring(0, numberStartIndex + 1);
                return true;
            }
            return false;
        }

        public static BitmapImage Base64StringToBitmap(string source)
        {
            var ims = new InMemoryRandomAccessStream();
            var bytes = Convert.FromBase64String(source);
            var dataWriter = new DataWriter(ims);
            dataWriter.WriteBytes(bytes);
#pragma warning disable CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
            dataWriter.StoreAsync();
#pragma warning restore CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
            ims.Seek(0);
            var img = new BitmapImage();
            img.SetSource(ims);
            return img;
        }
    }

    // 관용구 속담 구분 열거형
    public enum ERelationType { idiom, proverb };

    // s활용과 활용의 발음 클래스
    public class Conjus_AbbreviationItem
    {
        public string strings { get; set; }
        public List<string> prons { get; set; }
    }

    public class WordDetailStaticPage
    {
        public static WordDetailItem GetHomepage()
        {
            WordDetailItem item = new WordDetailItem
            {
                target_code = "-200",
                wordname = "표준국어대사전",
                sup_no = 0,
                original_language = "",
            };
            item.poses = new List<WordDetailItem.PosItem>
            {
                new WordDetailItem.PosItem
                {
                    pos = "품사 없음",
                    patterns = new List<WordDetailItem.PatternItem>
                    {
                        new WordDetailItem.PatternItem
                        {
                            pattern = new List<string>(),
                            definitions = new List<WordDetailItem.DefinitionItem>
                            {
                                new WordDetailItem.DefinitionItem
                                {
                                    definition = "《표준국어대사전》(標準國語大辭典)은 표준어 규정, 한글 맞춤법 등의 어문 규정을 준수하여 국립국어원에서 발행하는 한국어 사전이다.",
                                    examples = new List<string>
                                    {
                                        "검색창에 단어를 검색하여 모르는 단어를 찾고, 뜻풀이를 확인할 수 있습니다."
                                    }
                                },
                            }
                        },
                    }
                },
            };
            return item;
        }
    }

    public static class WordDetailItemSample
    {
        public static WordDetailItem GetDetails()
        {
            WordDetailItem item = new WordDetailItem
            {
                target_code = "707",
                wordname = "단어 이름",
                sup_no = 1,
                original_language = "<-원어",
                prons = new List<string> { "발음1", "발음2" },
            };
            item.conjus = new List<WordDetailItem.ConjusItem>
            {
                new WordDetailItem.ConjusItem
                {
                    conjus = "활용",
                    conju_prons = new List<string> { "활용 발음", "다른 발음" },
                    abbreviations = new List<WordDetailItem.AbbreviationItem>
                    {
                        new WordDetailItem.AbbreviationItem
                        {
                            abbreviations = "준말",
                            abbreviation_prons = new List<string> { "준말 발음" }
                        }
                    }
                },
                new WordDetailItem.ConjusItem
                {
                    conjus = "활용2",
                    conju_prons = new List<string> { "활용 발음2" }
                }
            };
            item.poses = new List<WordDetailItem.PosItem>
            {
                new WordDetailItem.PosItem
                {
                    pos = "관사",
                    patterns = new List<WordDetailItem.PatternItem>
                    {
                        new WordDetailItem.PatternItem
                        {
                            pattern = new List<string> { "문형", "문형" },
                            definitions = new List<WordDetailItem.DefinitionItem>
                            {
                                new WordDetailItem.DefinitionItem
                                {
                                    definition = "이 단어는 샘플 단어입니다.",
                                    sense_grammar = "정의 참고하는 말",
                                    examples = new List<string>
                                    {
                                        "이런 예시도 있어요.", "저런 예시도 있어요."
                                    }
                                },
                                new WordDetailItem.DefinitionItem
                                {
                                    definition = "이 단어는 샘플 단어입니다.",
                                    examples = new List<string>
                                    {
                                        "이런 예시도 있어요.", "저런 예시도 있어요."
                                    }
                                }
                            }
                        },
                        new WordDetailItem.PatternItem
                        {
                            pattern = new List<string> { "문형" },
                            grammar = "문형에 적용되는 문법",
                            definitions = new List<WordDetailItem.DefinitionItem>
                            {
                                new WordDetailItem.DefinitionItem
                                {
                                    cat = "분야",
                                    definition = "이 단어는 샘플 단어입니다.",
                                    examples = new List<string>
                                    {
                                        "이런 예시도 있어요.", "저런 예시도 있어요."
                                    }
                                },
                                new WordDetailItem.DefinitionItem
                                {
                                    definition = "이 단어는 샘플 단어입니다.",
                                    examples = new List<string>
                                    {
                                        "이런 예시도 있어요.", "저런 예시도 있어요."
                                    }
                                }
                            }
                        }
                    }
                },
                new WordDetailItem.PosItem
                {
                    pos = "관사",
                    patterns = new List<WordDetailItem.PatternItem>
                    {
                        new WordDetailItem.PatternItem
                        {
                            pattern = new List<string> { "문형" },
                            definitions = new List<WordDetailItem.DefinitionItem>
                            {
                                new WordDetailItem.DefinitionItem
                                {
                                    definition = "이 단어는 샘플 단어입니다.",
                                    examples = new List<string>
                                    {
                                        "이런 예시도 있어요.", "저런 예시도 있어요."
                                    }
                                },
                                new WordDetailItem.DefinitionItem
                                {
                                    definition = "이 단어는 샘플 단어입니다.",
                                    examples = new List<string>
                                    {
                                        "이런 예시도 있어요.", "저런 예시도 있어요."
                                    }
                                }
                            }
                        },
                        new WordDetailItem.PatternItem
                        {
                            pattern = new List<string> { "문형", "문형" },
                            definitions = new List<WordDetailItem.DefinitionItem>
                            {
                                new WordDetailItem.DefinitionItem
                                {
                                    definition = "이 단어는 샘플 단어입니다.",
                                    examples = new List<string>
                                    {
                                        "이런 예시도 있어요.", "저런 예시도 있어요."
                                    }
                                },
                                new WordDetailItem.DefinitionItem
                                {
                                    definition = "이 단어는 샘플 단어입니다.",
                                    examples = new List<string>
                                    {
                                        "이런 예시도 있어요.", "저런 예시도 있어요."
                                    }
                                }
                            }
                        }
                    }
                }
            };
            return item;
        }
    }
}
