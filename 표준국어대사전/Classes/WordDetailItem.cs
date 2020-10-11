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

namespace 표준국어대사전.Classes
{
    public class WordDetailItem : INotifyPropertyChanged
    {
        string FONTFAMILY = "/Fonts/NanumBarunGothic-YetHangul.ttf#NanumBarunGothic YetHangul";
        bool LabWordReaderEnabled = StorageManager.GetSetting<bool>(StorageManager.LabWordReaderEnabled);

        //필터
        public bool IsExampleVisible = true;

        #region Toolbar
        public StackPanel ToolBarSp
        {
            get
            {
                StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
                //ToolBar Buttons
                Button BtnFilter = new Button { Width = 80, Margin = new Thickness(5, 0, 5, 0), Background = Application.Current.Resources["ApplicationPageBackgroundThemeBrush"] as SolidColorBrush };
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
            //글꼴
            if (StorageManager.GetSetting<string>(StorageManager.DisplayFont) == "맑은 고딕")
                FONTFAMILY = "#Malgun Gothic";
            else
                FONTFAMILY = "/Fonts/NanumBarunGothic-YetHangul.ttf#NanumBarunGothic YetHangul";
        }

        //코드 번호
        public string target_code;
        //단어 명
        public string wordname;
        //어깨번호
        public int sup_no;
        //원어
        public string original_language;

        //발음
        public List<string> prons;
        //활용
        public List<ConjusItem> conjus;

        //단어 관계
        public List<LexicalItem> lexicals;

        //관사와 하위 항목
        public List<PosItem> poses;
        //규범 정보
        public List<string> norms;
        //어원
        public string origin;

        //관용구 속담
        public List<RelationItem> relations;

        //단어명 RTB
        public RichTextBlock wordnameRtb
        {
            get
            {
                RichTextBlock rtb = new RichTextBlock();

                Paragraph para = new Paragraph();
                if (wordname != null)
                    para.Inlines.Add(new Run { Text = wordname, FontSize = 32, FontFamily = new FontFamily(FONTFAMILY) });
                if (sup_no != 0)
                    para.Inlines.Add(new Run { Text = sup_no.ToString(), FontSize = 18, FontFamily = new FontFamily(FONTFAMILY) });
                if (original_language != "")
                    para.Inlines.Add(new Run { Text = $" ({original_language})", FontSize = 18, FontFamily = new FontFamily(FONTFAMILY) });
                rtb.Blocks.Add(para);

                return rtb;
            }
        }

        //발음 및 활용 RTB (단어 읽기 기능 비활성화시 사용)
        public RichTextBlock pronsconjusRtb
        {
            get
            {
                RichTextBlock rtb = new RichTextBlock();
                rtb.TextWrapping = TextWrapping.Wrap;

                if (LabWordReaderEnabled == false)
                {
                    //발음
                    if (prons != null && prons.Count > 0) //발음 정보 없는 예외 처리
                    {
                        Paragraph pProns = new Paragraph();
                        pProns.Margin = new Thickness(0, 4, 0, 4);
                        pProns.Inlines.Add(new Run { Text = "발음  ", FontSize = 17, FontWeight = Windows.UI.Text.FontWeights.Bold, FontFamily = new FontFamily(FONTFAMILY) });

                        pProns.Inlines.Add(new Run { Text = "[", FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                        for (int i = 0; i < prons.Count; i++)
                        {
                            pProns.Inlines.Add(new Run { Text = prons[i], FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                            if (i != prons.Count - 1)
                            {
                                pProns.Inlines.Add(new Run { Text = "/", FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                            }
                        }
                        pProns.Inlines.Add(new Run { Text = "]", FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                        rtb.Blocks.Add(pProns);
                    }

                    //활용
                    if (conjus != null) //활용 정보 없는 예외 처리
                    {
                        Paragraph pConjus = new Paragraph();
                        pConjus.Margin = new Thickness(0, 4, 0, 4);
                        pConjus.Inlines.Add(new Run { Text = "활용  ", FontSize = 17, FontWeight = Windows.UI.Text.FontWeights.Bold, FontFamily = new FontFamily(FONTFAMILY) });

                        for (int i = 0; i < conjus.Count; i++)
                        {
                            pConjus.Inlines.Add(new Run { Text = conjus[i].conjus, FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });

                            if (conjus[i].conju_prons != null)
                            {
                                pConjus.Inlines.Add(new Run { Text = "[", FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                                for (int j = 0; j < conjus[i].conju_prons.Count; j++)
                                {
                                    pConjus.Inlines.Add(new Run { Text = conjus[i].conju_prons[j], FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                                    if (j != conjus[i].conju_prons.Count - 1)
                                    {
                                        pConjus.Inlines.Add(new Run { Text = "/", FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                                    }
                                }
                                pConjus.Inlines.Add(new Run { Text = "]", FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                            }

                            if (conjus[i].abbreviations != null)
                            {
                                //준말
                                List<AbbreviationItem> abbreviations = conjus[i].abbreviations;
                                for (int j = 0; j < abbreviations.Count; j++)
                                {
                                    pConjus.Inlines.Add(new Run { Text = $"({abbreviations[j].abbreviations}", FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                                    if (abbreviations[j].abbreviation_prons != null)
                                    {
                                        pConjus.Inlines.Add(new Run { Text = "[", FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                                        for (int k = 0; k < abbreviations[j].abbreviation_prons.Count; k++)
                                        {
                                            pConjus.Inlines.Add(new Run { Text = abbreviations[j].abbreviation_prons[k], FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                                            if (k != abbreviations[j].abbreviation_prons.Count - 1)
                                            {
                                                pConjus.Inlines.Add(new Run { Text = "/", FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                                            }
                                        }
                                        pConjus.Inlines.Add(new Run { Text = "]", FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                                    }
                                    pConjus.Inlines.Add(new Run { Text = ")", FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                                }
                            }

                            if (conjus.Count - i != 1)
                                pConjus.Inlines.Add(new Run { Text = ", ", FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                        }
                        rtb.Blocks.Add(pConjus);
                    }
                }

                if (rtb.Blocks.Count == 0)
                    return null;

                return rtb;
            }
        }
        
        //발음 SP
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

                    TextBlock title = new TextBlock { Text = "발음", Margin = new Thickness(0, 0, 10, 0), FontSize = 17, FontWeight = Windows.UI.Text.FontWeights.Bold, FontFamily = new FontFamily(FONTFAMILY) };
                    sp.Children.Add(title);
                    sp.Children.Add(new PronunciationBlock { WordItems = prons, FontFamily = new FontFamily(FONTFAMILY), IsReaderEnabled = LabWordReaderEnabled });
                }

                return sp;
            }
        }

        //활용 SP
        public StackPanel conjusSp
        {
            get
            {
                StackPanel sp = null;

                //null 예외
                if (conjus == null)
                    return null;

                if (LabWordReaderEnabled)
                {
                    sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4) };

                    TextBlock title = new TextBlock { Text = "활용", Margin = new Thickness(0, 0, 10, 0), FontSize = 17, FontWeight = Windows.UI.Text.FontWeights.Bold, FontFamily = new FontFamily(FONTFAMILY) };
                    sp.Children.Add(title);

                    for (int i = 0; i < conjus.Count; i++)
                    {
                        sp.Children.Add(new TextBlock { Text = conjus[i].conjus, FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });

                        if (conjus[i].conju_prons != null)
                        {
                            sp.Children.Add(new PronunciationBlock { WordItems = conjus[i].conju_prons, IsReaderEnabled = LabWordReaderEnabled, FontFamily = new FontFamily(FONTFAMILY) });
                        }

                        if (conjus[i].abbreviations != null)
                        {
                            //준말
                            List<AbbreviationItem> abbreviations = conjus[i].abbreviations;
                            for (int j = 0; j < abbreviations.Count; j++)
                            {
                                sp.Children.Add(new TextBlock { Text = "(" + abbreviations[j].abbreviations, FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                                if (abbreviations[j].abbreviation_prons != null)
                                    sp.Children.Add(new PronunciationBlock { WordItems = abbreviations[j].abbreviation_prons, IsReaderEnabled = LabWordReaderEnabled, FontFamily = new FontFamily(FONTFAMILY) });
                                sp.Children.Add(new TextBlock { Text = ")", FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                            }
                        }
                        if (conjus.Count - i != 1)
                            sp.Children.Add(new TextBlock { Text = ",", Margin = new Thickness(0, 0, 6, 0), FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                    }
                }

                return sp;
            }
        }

        //단어 관계 RTB
        public RichTextBlock lexicalsRtb
        {
            get
            {
                RichTextBlock rtb = new RichTextBlock { Margin = new Thickness(0, 4, 0, 4) };

                //null 예외
                if (lexicals == null || lexicals.Count == 0)
                    return null;

                List<Paragraph> paras = new List<Paragraph>();
                for (int i = 0; i < lexicals.Count; i++)
                {
                    if (i != 0 && lexicals[i - 1].type == lexicals[i].type)
                    {
                        paras[paras.Count - 1].Inlines.Add(new Run { Text = ", ", FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });

                        Hyperlink link = new Hyperlink();
                        link.Inlines.Add(new Run { Text = lexicals[i].word, FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
                        link.Inlines.Add(new Run { FontFamily = new FontFamily(lexicals[i].target_code) });
                        link.Click += Hyperlink_Click;
                        paras[paras.Count - 1].Inlines.Add(link);
                    }
                    else //첫 단어
                    {
                        Paragraph para = new Paragraph();
                        para.Margin = new Thickness(0, 0, 0, 4);

                        para.Inlines.Add(new Run { Text = $"「{lexicals[i].type}」 ", FontSize = 17, FontFamily = new FontFamily(FONTFAMILY), FontWeight = Windows.UI.Text.FontWeights.Bold });

                        Hyperlink link = new Hyperlink();
                        link.Inlines.Add(new Run { Text = lexicals[i].word, FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
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

        //뜻풀이 RTB
        public RichTextBlock detailRtb
        {
            get
            {
                RichTextBlock rtb = new RichTextBlock();

                //예외
                if (poses == null)
                    return rtb;

                //관사와 하위 항목
                for (int i = 0; i < poses.Count; i++)
                {
                    Paragraph para = new Paragraph();
                    para.Margin = new Thickness(0, 40, 0, 0);

                    // 관사 번호
                    if (1 < poses.Count)
                    {
                        para.Inlines.Add(new Run { Text = $"[{ToRoman(i + 1)}] ", FontSize = 20, FontWeight = Windows.UI.Text.FontWeights.Bold, FontFamily = new FontFamily(FONTFAMILY) });
                    }

                    if (poses[i].pos != "품사 없음")
                    {
                        para.Inlines.Add(new Run { Text = $"「{poses[i].pos}」", FontSize = 20, FontWeight = Windows.UI.Text.FontWeights.Bold, FontFamily = new FontFamily(FONTFAMILY) });
                        rtb.Blocks.Add(para);
                    }

                    //예외
                    if (poses[i].patterns == null)
                       continue;

                    //문형 정보와 하위 항목
                    List<PatternItem> patterns = poses[i].patterns;

                    for (int j = 0; j < patterns.Count; j++)
                    {
                        Paragraph para2 = new Paragraph();
                        para2.Margin = new Thickness(20, 30, 0, 20);

                        // 문형 번호
                        if (1 < patterns.Count)
                        {
                            para2.Inlines.Add(new Run { Text = $"({j + 1}) ", FontSize = 18, FontFamily = new FontFamily(FONTFAMILY), FontWeight = Windows.UI.Text.FontWeights.Bold });
                        }

                        for (int k = 0; k < patterns[j].pattern.Count; k++)
                            para2.Inlines.Add(new Run { Text = $"【{patterns[j].pattern[k]}】", FontSize = 18, FontFamily = new FontFamily(FONTFAMILY) });
                        //문형 적용 문법 정보
                        if (patterns[j].grammar != null)
                            para2.Inlines.Add(new Run { Text = $" (({patterns[j].grammar}))", FontSize = 18, FontFamily = new FontFamily(FONTFAMILY) });

                        if (patterns.Count != 1 || patterns[j].pattern.Count > 0 || patterns[j].grammar != null)
                            rtb.Blocks.Add(para2);

                        //예외
                        if (patterns[j].definitions == null)
                            continue;

                        //정의
                        List<DefinitionItem> definitions = patterns[j].definitions;
                        for (int k = 0; k < definitions.Count; k++)
                        {
                            List<string> OutputList = new List<string>();

                            //cat 전문 분야
                            if (patterns[j].definitions[k].cat != null)
                                OutputList.Add("&FOS015" + $"『{patterns[j].definitions[k].cat}』 ");

                            //sense_pattern_info 정의 문형
                            if (patterns[j].definitions[k].sense_pattern_info != null)
                                OutputList.Add("&FOS015" + $"【{patterns[j].definitions[k].sense_pattern_info}】 ");

                            //sense_grammar 정의에 참고하는 말
                            if (patterns[j].definitions[k].sense_grammar != null)
                                OutputList.Add("&FOS015" + $"(({patterns[j].definitions[k].sense_grammar})) ");

                            //글꼴 크기가 다른 경우
                            while (true)
                            {
                                if (!definitions[k].definition.Contains("<sub style='font-size:"))
                                {
                                    OutputList.Add("&FOS015" + definitions[k].definition);
                                    break;
                                }
                                else
                                {
                                    if (definitions[k].definition.IndexOf("<sub style='font-size:") != 0)
                                    {
                                        OutputList.Add("&FOS015" + definitions[k].definition.Substring(0, definitions[k].definition.IndexOf("<sub style='font-size:")));
                                        definitions[k].definition = definitions[k].definition.Substring(definitions[k].definition.IndexOf("<sub style='font-size:"));
                                    }
                                    string fontsize = definitions[k].definition.Substring(definitions[k].definition.IndexOf("<sub style='font-size:") + 22, definitions[k].definition.IndexOf("px;'>") - definitions[k].definition.IndexOf("<sub style='font-size:") - 22);
                                    if (fontsize.Length == 1)
                                        fontsize = "00" + fontsize;
                                    else if (fontsize.Length == 2)
                                        fontsize = "0" + fontsize;
                                    OutputList.Add("&FOS" + fontsize + definitions[k].definition.Substring(definitions[k].definition.IndexOf("<sub style='font-size:") + 29, definitions[k].definition.IndexOf("</sub>") - definitions[k].definition.IndexOf("<sub style='font-size:") - 29));
                                    definitions[k].definition = definitions[k].definition.Substring(definitions[k].definition.IndexOf("</sub>") + 6);
                                }
                            }

                            //하이퍼텍스트
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

                            //이탤릭체
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

                            Paragraph para3 = new Paragraph();
                            para3.Margin = new Thickness(5, 15, 0, 15);

                            //의미 번호
                            if (1 < definitions.Count)
                            {
                                para3.Inlines.Add(new Run { Text = $"「{k + 1}」 ", FontSize = 15, Foreground = new SolidColorBrush(Windows.UI.Colors.Red), FontFamily = new FontFamily(FONTFAMILY) });
                            }

                            for (int o = 0; o < OutputList.Count(); o++)
                            {
                                if (OutputList[o].StartsWith("&FOS"))
                                {
                                    int fontsize = int.Parse(OutputList[o].Substring(4, 3));
                                    para3.Inlines.Add(new Run { Text = OutputList[o].Substring(7), FontSize = fontsize, FontFamily = new FontFamily(FONTFAMILY) });
                                }
                                else if (OutputList[o].StartsWith("&ITA"))
                                {
                                    int fontsize = int.Parse(OutputList[o].Substring(4, 3));
                                    para3.Inlines.Add(new Run { Text = OutputList[o].Substring(7), FontSize = fontsize, FontStyle = Windows.UI.Text.FontStyle.Italic, FontFamily = new FontFamily(FONTFAMILY) });
                                }
                                else if (OutputList[o].StartsWith("&HLK"))
                                {
                                    int fontsize = int.Parse(OutputList[o].Substring(4, 3));

                                    Hyperlink link = new Hyperlink();
                                    link.Inlines.Add(new Run { Text = OutputList[o].Substring(OutputList[o].IndexOf(">") + 1), FontSize = fontsize, FontFamily = new FontFamily(FONTFAMILY) });
                                    link.Inlines.Add(new Run { FontFamily = new FontFamily(OutputList[o].Substring(OutputList[o].IndexOf("<") + 1, OutputList[o].IndexOf(">") - OutputList[o].IndexOf("<") - 1)) });
                                    link.Click += Hyperlink_Click;
                                    para3.Inlines.Add(link);
                                }
                            }

                            //동의어 관계
                            if (IsExampleVisible && definitions[k].lexicals != null)
                            {
                                List<LexicalItem> lexicals = definitions[k].lexicals;
                                for (int l = 0; l < lexicals.Count; l++)
                                {
                                    if(lexicals[l].type == "동의어")
                                    {
                                        if(l != 0 && lexicals[l - 1].type == "동의어")
                                            para3.Inlines.Add(new Run { Text = ", ", FontSize = 15, FontFamily = new FontFamily(FONTFAMILY) });
                                        else //첫 단어
                                            para3.Inlines.Add(new Run { Text = " ≒ ", FontSize = 15, FontFamily = new FontFamily(FONTFAMILY) });

                                        Hyperlink link = new Hyperlink();
                                        link.Inlines.Add(new Run { Text = lexicals[l].word, FontSize = 15, FontFamily = new FontFamily(FONTFAMILY) });
                                        link.Inlines.Add(new Run { FontFamily = new FontFamily(lexicals[l].target_code) });
                                        link.Click += Hyperlink_Click;
                                        para3.Inlines.Add(link);
                                    }
                                }
                                para3.Inlines.Add(new Run { Text = "." });
                            }

                            rtb.Blocks.Add(para3);


                            //예시
                            if (IsExampleVisible && definitions[k].examples != null)
                            {
                                List<string> examples = definitions[k].examples;
                                for (int l = 0; l < examples.Count; l++)
                                {
                                    Paragraph para4 = new Paragraph();
                                    para4.Margin = new Thickness(30, 4, 0, 4);
                                    para4.Inlines.Add(new Run { Text = "· " + examples[l], FontSize = 14, FontFamily = new FontFamily(FONTFAMILY) });
                                    rtb.Blocks.Add(para4);
                                }
                            }

                            //단어 관계
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
                                        paras[paras.Count - 1].Inlines.Add(new Run { Text = ", ", FontSize = 13, FontFamily = new FontFamily(FONTFAMILY) });

                                        Hyperlink link = new Hyperlink();
                                        link.Inlines.Add(new Run { Text = lexicals[l].word, FontSize = 13, FontFamily = new FontFamily(FONTFAMILY) });
                                        link.Inlines.Add(new Run { FontFamily = new FontFamily(lexicals[l].target_code) });
                                        link.Click += Hyperlink_Click;
                                        paras[paras.Count - 1].Inlines.Add(link);
                                    }
                                    else //첫 단어
                                    {
                                        Paragraph para4 = new Paragraph();
                                        para4.Margin = new Thickness(30, 8, 0, 4);

                                        para4.Inlines.Add(new Run { Text = $"「{lexicals[l].type}」 ", FontSize = 13, FontFamily = new FontFamily(FONTFAMILY) });

                                        Hyperlink link = new Hyperlink();
                                        link.Inlines.Add(new Run { Text = lexicals[l].word, FontSize = 13, FontFamily = new FontFamily(FONTFAMILY) });
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

                //norm 규범 정보
                if(norms != null)
                {
                    Paragraph para = new Paragraph();
                    para.Margin = new Thickness(0, 30, 0, 0);

                    for (int i = 0; i < norms.Count; i++)
                    {
                        para.Inlines.Add(new Run { Text = "※ " + norms[i], FontSize = 14, FontFamily = new FontFamily(FONTFAMILY) });
                    }
                    rtb.Blocks.Add(para);
                }

                return rtb;
            }
        }

        //어원 RTB
        public RichTextBlock originRtb
        {
            get
            {
                RichTextBlock rtb = new RichTextBlock();

                if (origin != null)
                {
                    Paragraph title = new Paragraph();
                    title.Margin = new Thickness(0, 15, 0, 25);
                    title.Inlines.Add(new Run { Text = "▹ 어원", FontSize = 18, FontFamily = new FontFamily(FONTFAMILY) });
                    rtb.Blocks.Add(title);

                    Paragraph para = new Paragraph();
                    para.Margin = new Thickness(0, 0, 0, 5);
                    para.Inlines.Add(new Run { Text = origin, FontSize = 15, FontFamily = new FontFamily(FONTFAMILY) });
                    rtb.Blocks.Add(para);
                }

                return rtb;
            }
        }

        //관용구 속담 RTB
        public RichTextBlock relationRtb
        {
            get
            {
                RichTextBlock rtb = new RichTextBlock { HorizontalAlignment = HorizontalAlignment.Left };

                if (relations != null)
                {
                    Paragraph title = new Paragraph();
                    title.Margin = new Thickness(0, 15, 0, 25);
                    title.Inlines.Add(new Run { Text = "▹ 관용구/속담", FontSize = 18, FontFamily = new FontFamily(FONTFAMILY) });
                    rtb.Blocks.Add(title);

                    for (int i = 0; i < relations.Count; i++)
                    {
                        Paragraph para = new Paragraph();
                        para.Margin = new Thickness(0, 15, 0, 15);

                        string type = "";
                        if (relations[i].type == ERelationType.idiom)
                            type = "관용구";
                        else if (relations[i].type == ERelationType.proverb)
                            type = "속담";

                        para.Inlines.Add(new Run { Text = $"[{type}] ", FontSize = 15, FontFamily = new FontFamily(FONTFAMILY), Foreground = new SolidColorBrush(Windows.UI.Colors.Blue) });
                        Hyperlink link = new Hyperlink();
                        link.Inlines.Add(new Run { Text = relations[i].word, FontSize = 15, FontFamily = new FontFamily(FONTFAMILY) });
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

        //시작 화면 RTB
        public RichTextBlock homeRtb
        {
            get
            {
                RichTextBlock rtb = new RichTextBlock { HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 40, 0, 0) };

                if (target_code == "-200")
                {
                    //시작 화면인 경우

                    //최근 검색
                    Paragraph subTitle1 = new Paragraph();
                    subTitle1.Inlines.Add(new Run { Text = "최근 검색", FontSize = 20, FontWeight = Windows.UI.Text.FontWeights.Bold, FontFamily = new FontFamily(FONTFAMILY) });
                    subTitle1.Inlines.Add(new Run { Text = "   " });
                    Hyperlink linkClear = new Hyperlink();
                    linkClear.Inlines.Add(new Run { Text = "지우기", FontSize = 15, FontFamily = new FontFamily(FONTFAMILY), Foreground = new SolidColorBrush(Windows.UI.Colors.Red) });
                    linkClear.Click += RecentWordClear_Click;
                    subTitle1.Inlines.Add(linkClear);
                    subTitle1.Inlines.Add(new Run { Text = " " });
                    rtb.Blocks.Add(subTitle1);

                    Paragraph recentSearch = new Paragraph();
                    recentSearch.Margin = new Thickness(5, 15, 0, 15);

                    List<string> recentWords = RecentWordManager.GetWords();
                    for (int i = recentWords.Count - 1; 0 <= i; i--)
                    {
                        Hyperlink link = new Hyperlink();
                        link.Inlines.Add(new Run { Text = recentWords[i], FontSize = 15, FontFamily = new FontFamily(FONTFAMILY) });
                        link.Click += RecentWordLink_Click;
                        recentSearch.Inlines.Add(link);
                        if (i != 0)
                            recentSearch.Inlines.Add(new Run { Text = ", ", FontSize = 15, FontFamily = new FontFamily(FONTFAMILY) });
                        else
                            recentSearch.Inlines.Add(new Run { Text = " " });
                    }
                    if (recentWords.Count == 0)
                    {
                        recentSearch.Inlines.Add(new Run { Text = "최근 검색어 없음.", FontSize = 15, FontFamily = new FontFamily(FONTFAMILY), Foreground = new SolidColorBrush(Windows.UI.Colors.Gray) });
                    }
                    
                    rtb.Blocks.Add(recentSearch);

                    //최근 검색
                    Paragraph subTitle2 = new Paragraph();
                    subTitle2.Margin = new Thickness(0, 40, 0, 0);
                    subTitle2.Inlines.Add(new Run { Text = "도움말 및 관련 링크", FontSize = 20, FontWeight = Windows.UI.Text.FontWeights.Bold, FontFamily = new FontFamily(FONTFAMILY) });
                    rtb.Blocks.Add(subTitle2);

                    //웹 브라우저에서 열기
                    Paragraph pOpenWeb = new Paragraph();
                    pOpenWeb.Margin = new Thickness(5, 15, 0, 15);
                    Hyperlink hOpenWeb = new Hyperlink();
                    hOpenWeb.Inlines.Add(new Run { Text = "웹 브라우저에서 열기", FontSize = 15, FontFamily = new FontFamily(FONTFAMILY) });
                    hOpenWeb.NavigateUri = new Uri("https://stdict.korean.go.kr/");
                    pOpenWeb.Inlines.Add(hOpenWeb);
                    pOpenWeb.Inlines.Add(new Run { Text = " " });
                    rtb.Blocks.Add(pOpenWeb);

                    //일러두기
                    Paragraph pInform = new Paragraph();
                    pInform.Margin = new Thickness(5, 15, 0, 15);
                    Hyperlink hInform = new Hyperlink();
                    hInform.Inlines.Add(new Run { Text = "일러두기", FontSize = 15, FontFamily = new FontFamily(FONTFAMILY) });
                    hInform.NavigateUri = new Uri("https://stdict.korean.go.kr/help/popup/entry.do");
                    pInform.Inlines.Add(hInform);
                    pInform.Inlines.Add(new Run { Text = " " });
                    rtb.Blocks.Add(pInform);

                    //앱 도움말
                    Paragraph pHelp = new Paragraph();
                    pHelp.Margin = new Thickness(5, 15, 0, 15);
                    Hyperlink hHelp = new Hyperlink();
                    hHelp.Inlines.Add(new Run { Text = "앱 도움말", FontSize = 15, FontFamily = new FontFamily(FONTFAMILY) });
                    hHelp.NavigateUri = new Uri("https://costudio1122.blogspot.com/p/blog-page_76.html");
                    pHelp.Inlines.Add(hHelp);
                    pHelp.Inlines.Add(new Run { Text = " " });
                    rtb.Blocks.Add(pHelp);
                }

                return rtb;
            }
        }


        //활용과 활용의 발음 클래스
        public class ConjusItem
        {
            public string conjus;
            public List<string> conju_prons;
            public List<AbbreviationItem> abbreviations;
        }
        //준말과 줄말의 발음 클래스
        public class AbbreviationItem
        {
            public string abbreviations;
            public List<string> abbreviation_prons;
        }
        //관사와 하위 항목 클래스
        public class PosItem
        {
            public string pos;
            public List<PatternItem> patterns;
        }
        //문형 정보와 하위 항목 클래스
        public class PatternItem
        {
            public List<string> pattern;
            public string grammar;
            public List<DefinitionItem> definitions;
        }
        //정의와 예시 클래스
        public class DefinitionItem
        {
            public string cat;
            public string sense_pattern_info;
            public string sense_grammar;
            public string definition;
            public List<string> examples;
            public List<LexicalItem> lexicals;
        }
        //관용구 속담 클래스
        public class RelationItem
        {
            public string word;
            public ERelationType type;
            public string target_code;
        }
        //어휘 관계 클래스
        public class LexicalItem
        {
            public string word;
            public string type;
            public string target_code;
        }


        private void Hyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            Hyperlink hyperlink = sender as Hyperlink;
            if (hyperlink.FindName("DetailGrid") != null)
            {
                Grid DetailGrid = hyperlink.FindName("DetailGrid") as Grid;

                if (DetailGrid.FindName("HyperViewer") == null)
                {
                    ConWordDetail HyperViewer = new ConWordDetail();
                    HyperViewer.Name = "HyperViewer";
                    Run word = hyperlink.Inlines[0] as Run;
                    int sup_no = 0;
                    int.TryParse(Regex.Replace(word.Text, "[^0-9.]", ""), out sup_no);
                    HyperViewer.Load_WordDetail(hyperlink.Inlines[1].FontFamily.Source, sup_no);

                    DetailGrid.Children.Add(HyperViewer);
                }
            }
            else if (hyperlink.FindName("ConWordDetailGrid") != null)
            {
                Grid ConWordDetailGrid = hyperlink.FindName("ConWordDetailGrid") as Grid;
                ConWordDetail HyperViewer = ConWordDetailGrid.Parent as ConWordDetail;
                Run word = hyperlink.Inlines[0] as Run;
                int sup_no = 0;
                int.TryParse(Regex.Replace(word.Text, "[^0-9.]", ""), out sup_no);
                HyperViewer.Load_WordDetail(hyperlink.Inlines[1].FontFamily.Source, sup_no);
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

        //로마자 변환
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
    }

    //관용구 속담 구분 열거형
    public enum ERelationType { idiom, proverb };

    //활용과 활용의 발음 클래스
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
