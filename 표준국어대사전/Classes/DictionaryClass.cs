using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace 표준국어대사전.Classes
{
    public class DictionaryClass
    {
        const string WORD_DETAIL_URL = "https://stdict.korean.go.kr/api/view.do?&key={0}&method=TARGET_CODE&q={1}";
        string API_KEY;
        string FONTFAMILY = "/Fonts/NanumBarunGothic-YetHangul.ttf#NanumBarunGothic YetHangul";

        ListView ListviewWordDetail;
        Page page;
        ProgressBar DetailProgressBar;

        public DictionaryClass(ListView lvi, Page thispage, ProgressBar pbar)
        {
            //생성자
            ListviewWordDetail = lvi;
            page = thispage;
            DetailProgressBar = pbar;

            //글꼴
            if (new DataStorageClass().GetSetting<string>(DataStorageClass.DisplayFont) == "맑은 고딕")
                FONTFAMILY = "#Malgun Gothic";
            else
                FONTFAMILY = "/Fonts/NanumBarunGothic-YetHangul.ttf#NanumBarunGothic YetHangul";

            //API 키 처리
            API_KEY = new DataStorageClass().GetSetting<string>(DataStorageClass.APIKey);
        }

        public async void GetWordDetail(string target_code, string wordname, int sup_no)
        {
            DetailProgressBar.Visibility = Visibility.Visible;

            string temp = string.Format(WORD_DETAIL_URL, API_KEY, target_code);

            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync(temp);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            XDocument xDoc = XDocument.Parse(responseBody);

            //정의 Listview 지우기
            ListviewWordDetail.Items.Clear();

            //에러코드
            if (xDoc.Element("error") != null)
            {
                string error_code = (string)xDoc.Element("error").Descendants("error_code").ElementAt(0);
                string error_message = "error_code : " + error_code + Environment.NewLine + "message : " + (string)xDoc.Element("error").Descendants("message").ElementAt(0);

                var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                var contentDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = error_message,
                    CloseButtonText = res.GetString("ContentDialogText2")
                };
                if (error_code == "020" || error_code == "021")
                    contentDialog.PrimaryButtonText = res.GetString("ContentDialogText1");

                ContentDialogResult result = await contentDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    if (error_code == "020")
                        await Windows.System.Launcher.LaunchUriAsync(new Uri("https://costudio1122.blogspot.com/p/2.html"));
                    if (error_code == "021")
                        await Windows.System.Launcher.LaunchUriAsync(new Uri("https://costudio1122.blogspot.com/p/api.html"));
                }

                DetailProgressBar.Visibility = Visibility.Collapsed;
                return;
            }

            if ((int)xDoc.Descendants("total").ElementAt(0) == 0)
            {
                //검색 결과가 없을 때
                DetailProgressBar.Visibility = Visibility.Collapsed;
                return;
            }

            //원어
            string original_language = "";
            if (xDoc.Root.Element("item").Element("word_info").Element("original_language_info") != null)
                original_language = " (" + (string)xDoc.Root.Element("item").Element("word_info").Element("original_language_info").Descendants("original_language").ElementAt(0) + ")";

            //단어 명
            AddWordnameItem(wordname, sup_no, original_language);

            //발음
            if (xDoc.Descendants("pronunciation") != null)
            {
                IEnumerable<XElement> pronunciations = xDoc.Descendants("pronunciation");
                IEnumerable<XElement> filter_prons = pronunciations.Where(p => p.Parent.Parent.Name == "word_info");
                List<string> prons = new List<string>();
                for (int i = 0; i < filter_prons.Count(); ++i)
                {
                    prons.Add((string)filter_prons.ElementAt(i));
                }
                AddPronunciationItem(prons);
            }

            //활용
            if (xDoc.Root.Element("item").Element("word_info").Element("conju_info") != null)
            {
                IEnumerable<XElement> conju_infos = xDoc.Descendants("conju_info");

                List<string> conjus = new List<string>();
                List<string> conju_prons = new List<string>();
                List<string> abbreviations = new List<string>();
                List<string> abbreviation_prons = new List<string>();
                for (int i = 0; i < conju_infos.Count(); ++i)
                {
                    conjus.Add((string)conju_infos.ElementAt(i).Descendants("conjugation").ElementAt(0));
                    if (conju_infos.ElementAt(i).Descendants("conjugation_info").Descendants("pronunciation") != null)
                    {
                        conju_prons.Add("");
                        IEnumerable<XElement> _conju_prons = conju_infos.ElementAt(i).Descendants("conjugation_info").Descendants("pronunciation");
                        for (int j = 0; j < _conju_prons.Count(); ++j)
                        {
                            conju_prons[i] += (string)_conju_prons.ElementAt(j);
                            if (_conju_prons.Count() - j != 1)
                                conju_prons[i] += " / ";
                        }
                    }
                    else
                        conju_prons.Add(null);
                    //준말
                    if (conju_infos.ElementAt(i).Element("abbreviation_info") != null)
                    {
                        abbreviations.Add((string)conju_infos.ElementAt(i).Descendants("abbreviation").ElementAt(0));

                        if (conju_infos.ElementAt(i).Descendants("abbreviation_info").Descendants("pronunciation") != null)
                        {
                            abbreviation_prons.Add("");
                            IEnumerable<XElement> _abbreviation_prons = conju_infos.ElementAt(i).Element("abbreviation_info").Descendants("pronunciation");
                            for (int j = 0; j < _abbreviation_prons.Count(); ++j)
                            {
                                abbreviation_prons[i] += (string)_abbreviation_prons.ElementAt(j);
                                if (_abbreviation_prons.Count() - j != 1)
                                    abbreviation_prons[i] += " / ";
                            }
                        }
                        else
                            abbreviation_prons.Add(null);
                    }
                    else
                    {
                        abbreviations.Add(null);
                        abbreviation_prons.Add(null);
                    }
                }
                AddConjugationItem(conjus, conju_prons, abbreviations, abbreviation_prons);
            }

            //분리 막대
            AddSeparatorItem();

            //관사와 하위 항목
            if (xDoc.Root.Element("item").Element("word_info").Element("pos_info") != null)
            {
                IEnumerable<XElement> pos_infos = xDoc.Root.Element("item").Element("word_info").Descendants("pos_info");

                for (int i = 0; i < pos_infos.Count(); ++i)
                {
                    //품사 명
                    string pos;
                    if (pos_infos.Count() != 1)
                        pos = "[" + ToRoman(i + 1) + "] ";
                    else
                        pos = "";
                    if (pos_infos.ElementAt(i).Element("pos") != null)
                        pos += "「" + (string)pos_infos.ElementAt(i).Descendants("pos").ElementAt(0) + "」";
                    else
                        pos = null;
                    //품사 출력
                    AddPosItem(pos);

                    //comm_pattern
                    if (pos_infos.ElementAt(i).Element("comm_pattern_info") != null)
                    {
                        IEnumerable<XElement> comm_pattern_infos = pos_infos.ElementAt(i).Descendants("comm_pattern_info");

                        for (int j = 0; j < comm_pattern_infos.Count(); ++j)
                        {
                            string pattern;
                            if (comm_pattern_infos.Count() != 1)
                                pattern = "〔" + (j + 1) + "〕 ";
                            else
                                pattern = "";

                            //문형
                            if (pos_infos.ElementAt(i).Element("comm_pattern_info").Element("pattern_info") != null)
                            {
                                IEnumerable<XElement> pattern_infos = comm_pattern_infos.ElementAt(j).Descendants("pattern_info");

                                for (int k = 0; k < pattern_infos.Count(); ++k)
                                {
                                    pattern += "【";
                                    pattern += (string)pattern_infos.ElementAt(k).Descendants("pattern").ElementAt(0);
                                    pattern += "】";
                                }
                            }

                            //문형에 적용되는 문법
                            if (comm_pattern_infos.ElementAt(j).Element("grammar_info") != null)
                                pattern += " ((" + (string)comm_pattern_infos.ElementAt(j).Element("grammar_info").Descendants("grammar").ElementAt(0) + "))";

                            //문형과 문형에 적용되는 문법 출력
                            AddPatternItem(pattern);


                            //정의와 예시(sense_info)
                            if (comm_pattern_infos.ElementAt(j).Element("sense_info") != null)
                            {
                                IEnumerable<XElement> sense_infos = comm_pattern_infos.ElementAt(j).Descendants("sense_info");

                                for (int k = 0; k < sense_infos.Count(); ++k)
                                {
                                    string definition;
                                    if (sense_infos.Count() != 1)
                                        definition = "###" + (k + 1) + "### ";
                                    else
                                        definition = "";

                                    //분류(cat)
                                    if (sense_infos.ElementAt(k).Element("cat_info") != null)
                                        definition += "『" + (string)sense_infos.ElementAt(k).Element("cat_info").Descendants("cat").ElementAt(0) + "』 ";

                                    //sense_grammar 정의에 참고하는 말
                                    if (sense_infos.ElementAt(k).Element("sense_grammar_info") != null)
                                        definition += "((" + (string)sense_infos.ElementAt(k).Element("sense_grammar_info").Descendants("grammar").ElementAt(0) + ")) ";

                                    //정의
                                    definition += (string)sense_infos.ElementAt(k).Descendants("definition").ElementAt(0);
                                    AddDefinitionItem(definition);

                                    //예시
                                    if (sense_infos.ElementAt(k).Element("example_info") != null)
                                    {
                                        IEnumerable<XElement> example_info = sense_infos.ElementAt(k).Descendants("example_info");

                                        for (int l = 0; l < example_info.Count(); ++l)
                                        {
                                            AddExampleItem((string)example_info.ElementAt(l).Descendants("example").ElementAt(0));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //어원
                if (xDoc.Root.Element("item").Element("word_info").Element("origin") != null)
                {
                    string origin = (string)xDoc.Root.Element("item").Element("word_info").Descendants("origin").ElementAt(0);
                    if (origin.IndexOf("<equ>&#x21BC;</equ>") != -1)
                        origin = origin.Replace("<equ>&#x21BC;</equ>", "↼");
                    AddNewItem("", 10);
                    AddSeparatorItem();
                    AddNewItem("▹ 어원", 18);
                    AddNewItem(origin, 16);
                }

                //norm
                if (xDoc.Root.Element("item").Element("word_info").Element("norm_info") != null)
                {
                    IEnumerable<XElement> norms = xDoc.Root.Element("item").Element("word_info").Descendants("norm_info");

                    for (int i = 0; i < norms.Count(); ++i)
                    {
                        string desc = (string)norms.ElementAt(i).Descendants("desc").ElementAt(0);
                        AddNewItem("", 16);
                        AddNewItem("※ " + desc, 16);
                    }
                }
            }
            DetailProgressBar.Visibility = Visibility.Collapsed;
        }


        private void AddNewItem(string text, int fontsize)
        {
            if (text == null)
                return;
            ListViewItem item = new ListViewItem();

            StackPanel sp = new StackPanel();

            RichTextBlock rtb = new RichTextBlock();
            Paragraph para = new Paragraph();
            para.Inlines.Add(new Run { Text = text, FontSize = fontsize, FontFamily = new FontFamily(FONTFAMILY) });
            rtb.Blocks.Add(para);

            sp.Children.Add(rtb);
            item.Content = sp;

            ListviewWordDetail.Items.Add(item);
        }

        private void AddWordnameItem(string wordname, int sup_no, string original_language)
        {
            ListViewItem item = new ListViewItem();
            item.Margin = new Thickness(0, 0, 0, 6);

            StackPanel sp = new StackPanel();

            RichTextBlock rtb = new RichTextBlock();
            Paragraph para = new Paragraph();
            para.Inlines.Add(new Run { Text = wordname, FontSize = 32 });
            if (sup_no != 0)
                para.Inlines.Add(new Run { Text = sup_no.ToString(), FontSize = 18, FontFamily = new FontFamily(FONTFAMILY) });
            if (original_language != "")
                para.Inlines.Add(new Run { Text = original_language, FontSize = 18, FontFamily = new FontFamily(FONTFAMILY) });
            rtb.Blocks.Add(para);

            sp.Children.Add(rtb);
            item.Content = sp;

            ListviewWordDetail.Items.Add(item);
        }

        private void AddPronunciationItem(List<string> prons)
        {
            if (prons.Count() == 0)
                return;
            string text = "";
            for (int i = 0; i < prons.Count(); ++i)
            {
                text += prons[i];
                if (prons.Count() - i != 1)
                    text += " / ";
            }

            ListViewItem item = new ListViewItem();
            StackPanel sp = new StackPanel();
            item.MinHeight = 30;

            RichTextBlock rtb = new RichTextBlock();
            Paragraph para = new Paragraph();
            para.Inlines.Add(new Run { Text = "발음 ", FontSize = 17, FontWeight = Windows.UI.Text.FontWeights.Bold, FontFamily = new FontFamily(FONTFAMILY) });
            para.Inlines.Add(new Run { Text = "[" + text + "]", FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
            rtb.Blocks.Add(para);

            sp.Children.Add(rtb);
            item.Content = sp;

            ListviewWordDetail.Items.Add(item);
        }

        private void AddConjugationItem(List<string> conjus, List<string> conju_prons, List<string> abbreviations, List<string> abbreviation_prons)
        {
            string text = "";
            for (int i = 0; i < conjus.Count(); ++i)
            {
                text += conjus[i];

                if (conju_prons[i] != null)
                {
                    text += "[" + conju_prons[i] + "]";
                }

                if (abbreviations[i] != null)
                {
                    text += "(" + abbreviations[i];
                    if (abbreviation_prons[i] != null)
                        text += "[" + abbreviation_prons[i] + "]";
                    text += ")";
                }
                if (conjus.Count() - i != 1)
                    text += ", ";
            }

            ListViewItem item = new ListViewItem();
            StackPanel sp = new StackPanel();
            item.MinHeight = 30;

            RichTextBlock rtb = new RichTextBlock();
            Paragraph para = new Paragraph();
            para.Inlines.Add(new Run { Text = "활용 ", FontSize = 17, FontWeight = Windows.UI.Text.FontWeights.Bold, FontFamily = new FontFamily(FONTFAMILY) });
            para.Inlines.Add(new Run { Text = text, FontSize = 16, FontFamily = new FontFamily(FONTFAMILY) });
            rtb.Blocks.Add(para);

            sp.Children.Add(rtb);
            item.Content = sp;

            ListviewWordDetail.Items.Add(item);
        }

        private void AddSeparatorItem()
        {
            ListViewItem item = new ListViewItem();
            item.MinHeight = 0;
            item.Height = 20;
            item.Style = (Style)page.Resources["SeperatorItem"];
            
            item.Content = new Windows.UI.Xaml.Shapes.Rectangle { Fill = new SolidColorBrush(Windows.UI.Colors.WhiteSmoke), Margin = new Thickness(5, 0, 5, 0), Height = 3 };
            ListviewWordDetail.Items.Add(item);
        }

        private void AddPosItem(string pos)
        {
            if (pos == null || pos == "「품사 없음」" || pos == "")
                return;
            ListViewItem item = new ListViewItem();
            item.Margin = new Thickness(0, 10, 0, 0);

            StackPanel sp = new StackPanel();

            RichTextBlock rtb = new RichTextBlock();
            Paragraph para = new Paragraph();
            para.Inlines.Add(new Run { Text = pos, FontSize = 20, FontWeight = Windows.UI.Text.FontWeights.Bold, FontFamily = new FontFamily(FONTFAMILY) });
            rtb.Blocks.Add(para);

            sp.Children.Add(rtb);
            item.Content = sp;

            ListviewWordDetail.Items.Add(item);
        }

        private void AddPatternItem(string pattern)
        {
            if (pattern == null || pattern == "")
                return;
            ListViewItem item = new ListViewItem();
            item.Margin = new Thickness(15, 8, 0, 0);

            StackPanel sp = new StackPanel();

            RichTextBlock rtb = new RichTextBlock();
            Paragraph para = new Paragraph();
            para.Inlines.Add(new Run { Text = pattern, FontSize = 18, FontFamily = new FontFamily(FONTFAMILY) });
            rtb.Blocks.Add(para);

            sp.Children.Add(rtb);
            item.Content = sp;

            ListviewWordDetail.Items.Add(item);
        }

        private void AddDefinitionItem(string definition)
        {
            if (definition == null)
                return;

            List<string> OutputList = new List<string>();

            //의미 번호
            if (definition.IndexOf("###") != -1)
            {
                string num = definition.Substring(definition.IndexOf("###") + 3, definition.LastIndexOf("###") - definition.IndexOf("###") - 3);
                OutputList.Add("&NUM" + "「" + num + "」");
                definition = definition.Substring(definition.LastIndexOf("###") + 3);
            }
            //글꼴 크기가 다른 경우
            while (true)
            {
                if (!definition.Contains("<sub style='font-size:"))
                {
                    OutputList.Add("&FOS016" + definition);
                    break;
                }
                else
                {
                    if (definition.IndexOf("<sub style='font-size:") != 0)
                    {
                        OutputList.Add("&FOS016" + definition.Substring(0, definition.IndexOf("<sub style='font-size:")));
                        definition = definition.Substring(definition.IndexOf("<sub style='font-size:"));
                    }
                    string fontsize = definition.Substring(definition.IndexOf("<sub style='font-size:") + 22, definition.IndexOf("px;'>") - definition.IndexOf("<sub style='font-size:") - 22);
                    if (fontsize.Length == 1)
                        fontsize = "00" + fontsize;
                    else if (fontsize.Length == 2)
                        fontsize = "0" + fontsize;
                    OutputList.Add("&FOS" + fontsize + definition.Substring(definition.IndexOf("<sub style='font-size:") + 29, definition.IndexOf("</sub>") - definition.IndexOf("<sub style='font-size:") - 29));
                    definition = definition.Substring(definition.IndexOf("</sub>") + 6);
                }
            }

            //이탤릭체
            for (int i = 0; i < OutputList.Count; ++i)
            {
                while (true)
                {
                    if (!OutputList[i].Contains("<I>") && !OutputList[i].Contains("<i>"))
                        break;
                    else if (OutputList[i].Contains("<I>"))
                    {
                        string output = OutputList[i];
                        OutputList.RemoveAt(i);
                        string fontsize = "015"; //default
                        if (output.StartsWith("&FOS"))
                            fontsize = output.Substring(4, 3);
                        OutputList.Insert(i, output.Substring(0, output.IndexOf("<I>")));
                        output = output.Substring(output.IndexOf("<I>"));
                        OutputList.Insert(i + 1, "&ITA" + fontsize + output.Substring(output.IndexOf("<I>") + 3, output.IndexOf("</I>") - output.IndexOf("<I>") - 3));
                        OutputList.Insert(i + 2, "&FOS" + fontsize + output.Substring(output.IndexOf("</I>") + 4));
                    }
                    else
                    {
                        string output = OutputList[i];
                        OutputList.RemoveAt(i);
                        string fontsize = "015"; //default
                        if (output.StartsWith("&FOS"))
                            fontsize = output.Substring(4, 3);
                        OutputList.Insert(i, output.Substring(0, output.IndexOf("<i>")));
                        output = output.Substring(output.IndexOf("<i>"));
                        OutputList.Insert(i + 1, "&ITA" + fontsize + output.Substring(output.IndexOf("<i>") + 3, output.IndexOf("</i>") - output.IndexOf("<i>") - 3));
                        OutputList.Insert(i + 2, "&FOS" + fontsize + output.Substring(output.IndexOf("</i>") + 4));
                    }
                }
            }

            ListViewItem item = new ListViewItem();
            item.Margin = new Thickness(0, 6, 0, 0);

            StackPanel sp = new StackPanel();

            RichTextBlock rtb = new RichTextBlock();
            Paragraph para = new Paragraph();
            for (int i = 0; i < OutputList.Count(); ++i)
            {
                if (OutputList[i].StartsWith("&FOS"))
                {
                    int fontsize = int.Parse(OutputList[i].Substring(4, 3));
                    para.Inlines.Add(new Run { Text = OutputList[i].Substring(7), FontSize = fontsize, FontFamily = new FontFamily(FONTFAMILY) });
                }
                else if (OutputList[i].StartsWith("&NUM"))
                {
                    para.Inlines.Add(new Run { Text = OutputList[i].Substring(4), FontSize = 15, Foreground = new SolidColorBrush(Windows.UI.Colors.Red), FontFamily = new FontFamily(FONTFAMILY) });
                }
                else if (OutputList[i].StartsWith("&ITA"))
                {
                    int fontsize = int.Parse(OutputList[i].Substring(4, 3));
                    para.Inlines.Add(new Run { Text = OutputList[i].Substring(7), FontSize = fontsize, FontFamily = new FontFamily(FONTFAMILY), FontStyle = Windows.UI.Text.FontStyle.Italic });
                }
            }

            rtb.Blocks.Add(para);

            sp.Children.Add(rtb);
            item.Content = sp;

            ListviewWordDetail.Items.Add(item);
        }

        private void AddExampleItem(string example)
        {
            if (example == null)
                return;
            ListViewItem item = new ListViewItem();
            item.Margin = new Thickness(25, 0, 0, 0);
            item.MinHeight = 0;

            StackPanel sp = new StackPanel();

            RichTextBlock rtb = new RichTextBlock();
            Paragraph para = new Paragraph();
            para.Inlines.Add(new Run { Text = "· " + example, FontSize = 15, FontFamily = new FontFamily(FONTFAMILY) });
            rtb.Blocks.Add(para);

            sp.Children.Add(rtb);
            item.Content = sp;

            ListviewWordDetail.Items.Add(item);
        }

        public string ToRoman(int number)
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
}
