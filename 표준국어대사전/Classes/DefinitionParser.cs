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
using Windows.Foundation;
using 표준국어대사전.Models;

namespace 표준국어대사전.Classes
{
    public class DefinitionParser
    {
        const string WORD_DETAIL_URL = "https://stdict.korean.go.kr/api/view.do?&key={0}&method=TARGET_CODE&q={1}";
        string API_KEY;

        Action<Visibility> SetProgressBar;
        TypedEventHandler<Hyperlink, HyperlinkClickEventArgs> HandleHyperlinkClick;

        public DefinitionParser(Action<Visibility> setProgressBar, TypedEventHandler<Hyperlink, HyperlinkClickEventArgs> handleHyperlinkClick)
        {
            //생성자
            this.SetProgressBar = setProgressBar;
            this.HandleHyperlinkClick = handleHyperlinkClick;

            //API 키 처리
            this.API_KEY = StorageManager.GetSetting<string>(StorageManager.APIKey);
        }

        public async Task<WordDetailItem> GetWordDetail(string target_code, string wordname, int sup_no)
        {
            SetProgressBar(Visibility.Visible);

            string responseBody = await DownloadWordDetailAsync(target_code);
            if (responseBody == null) //실패 여부 확인
            {
                string error_code = "404";
                string error_message = $"error_code : {error_code}" + Environment.NewLine + "message : Network Problem";
                ShowErrorMessage(error_code, error_message, null);
                SetProgressBar(Visibility.Collapsed);
                return null;
            }

            WordDetailItem wordDetail = ParseWordDetail(responseBody, target_code, wordname, sup_no);

            SetProgressBar(Visibility.Collapsed);
            return wordDetail;
        }

        private async Task<string> DownloadWordDetailAsync(string target_code)
        {
            string url = string.Format(WORD_DETAIL_URL, API_KEY, target_code);

            HttpClient client = new HttpClient();

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch
            {
                //GetAsync 실패
                return null;
            }
        }

        private WordDetailItem ParseWordDetail(string responseBody, string target_code, string wordname, int sup_no)
        {
            WordDetailItem wordDetail = new WordDetailItem(HandleHyperlinkClick);

            XDocument xDoc = XDocument.Parse(responseBody);

            //에러코드
            if (xDoc.Element("error") != null)
            {
                string error_code = (string)xDoc.Element("error").Descendants("error_code").ElementAt(0);
                string error_message = $"error_code : {error_code}" + Environment.NewLine + $"message : {(string)xDoc.Element("error").Descendants("message").ElementAt(0)}";

                var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                ShowErrorMessage(error_code, error_message, res.GetString("ContentDialogText1"));

                return wordDetail;
            }

            //검색 결과가 없을 때
            if ((int)xDoc.Root.Element("total") == 0)
                return null;

            //target_code 단어 명, 어깨 번호
            wordDetail.target_code = target_code;

            //단어 명 예외 (ConWordDetail)
            if (wordname != null)
                wordDetail.wordname = wordname;
            else
                if (xDoc.Root.Element("item").Element("word_info").Element("word") != null)
                wordDetail.wordname = (string)xDoc.Root.Element("item").Element("word_info").Element("word");
            wordDetail.sup_no = sup_no;

            //원어
            wordDetail.original_language = "";
            if (xDoc.Root.Element("item").Element("word_info").Element("original_language_info") != null)
            {
                IEnumerable<XElement> original_languages = xDoc.Root.Element("item").Element("word_info").Elements("original_language_info").Elements("original_language");
                for (int i = 0; i < original_languages.Count(); i++)
                    wordDetail.original_language += (string)original_languages.ElementAt(i);
            }

            //발음
            if (xDoc.Root.Element("item").Element("word_info").Element("pronunciation_info") != null)
            {
                IEnumerable<XElement> pronunciations = xDoc.Root.Element("item").Element("word_info").Element("pronunciation_info").Elements("pronunciation");
                wordDetail.prons = new List<string>();
                for (int i = 0; i < pronunciations.Count(); i++)
                {
                    wordDetail.prons.Add((string)pronunciations.ElementAt(i));
                }
            }

            //활용
            if (xDoc.Root.Element("item").Element("word_info").Element("conju_info") != null)
            {
                IEnumerable<XElement> conju_infos = xDoc.Root.Element("item").Element("word_info").Elements("conju_info");

                wordDetail.conjus = new List<WordDetailItem.ConjusItem>();
                for (int i = 0; i < conju_infos.Count(); i++)
                {
                    string conjusTemp = (string)conju_infos.ElementAt(i).Element("conjugation_info").Element("conjugation");
                    wordDetail.conjus.Add(new WordDetailItem.ConjusItem { conjus = conjusTemp });
                    if (conju_infos.ElementAt(i).Element("conjugation_info").Element("pronunciation_info") != null)
                    {
                        IEnumerable<XElement> _conju_prons = conju_infos.ElementAt(i).Elements("conjugation_info").Elements("pronunciation_info").Elements("pronunciation");

                        wordDetail.conjus[i].conju_prons = new List<string>();
                        for (int j = 0; j < _conju_prons.Count(); j++)
                        {
                            wordDetail.conjus[i].conju_prons.Add("");

                            wordDetail.conjus[i].conju_prons[j] += (string)_conju_prons.ElementAt(j);
                        }
                    }

                    //준말
                    if (conju_infos.ElementAt(i).Element("abbreviation_info") != null)
                    {
                        IEnumerable<XElement> abbreviation_infos = conju_infos.ElementAt(i).Elements("abbreviation_info");

                        wordDetail.conjus[i].abbreviations = new List<WordDetailItem.AbbreviationItem>();
                        for (int j = 0; j < abbreviation_infos.Count(); j++)
                        {
                            string abbreviationTemp = (string)abbreviation_infos.ElementAt(j).Element("abbreviation");
                            wordDetail.conjus[i].abbreviations.Add(new WordDetailItem.AbbreviationItem { abbreviations = abbreviationTemp });

                            if (abbreviation_infos.ElementAt(j).Element("pronunciation_info") != null)
                            {
                                wordDetail.conjus[i].abbreviations[j].abbreviation_prons = new List<string>();

                                wordDetail.conjus[i].abbreviations[j].abbreviation_prons.Add("");
                                IEnumerable<XElement> _abbreviation_prons = conju_infos.ElementAt(i).Element("abbreviation_info").Elements("pronunciation_info").Elements("pronunciation");
                                for (int k = 0; k < _abbreviation_prons.Count(); k++)
                                {
                                    wordDetail.conjus[i].abbreviations[j].abbreviation_prons[j] += (string)_abbreviation_prons.ElementAt(k);
                                    if (_abbreviation_prons.Count() - k != 1)
                                        wordDetail.conjus[i].abbreviations[j].abbreviation_prons[j] += " / ";
                                }
                            }
                        }
                    }
                }
            }

            //단어 관계
            if (xDoc.Root.Element("item").Element("word_info").Element("lexical_info") != null)
            {
                IEnumerable<XElement> lexical_infos = xDoc.Root.Element("item").Element("word_info").Elements("lexical_info");

                wordDetail.lexicals = new List<WordDetailItem.LexicalItem>();
                for (int i = 0; i < lexical_infos.Count(); i++)
                {
                    WordDetailItem.LexicalItem lexical = new WordDetailItem.LexicalItem();
                    if (lexical_infos.ElementAt(i).Element("type") != null)
                        lexical.type = (string)lexical_infos.ElementAt(i).Element("type");
                    if (lexical_infos.ElementAt(i).Element("word") != null)
                        lexical.word = (string)lexical_infos.ElementAt(i).Element("word");
                    if (lexical_infos.ElementAt(i).Element("link") != null)
                    {
                        string link = (string)lexical_infos.ElementAt(i).Element("link");
                        if (link.Contains("word_no="))
                        {
                            if (link.IndexOf("&", link.IndexOf("word_no=") + 8) != -1)
                                lexical.target_code = link.Substring(link.IndexOf("word_no=") + 8, link.IndexOf("&", link.IndexOf("word_no=") + 8) - link.IndexOf("word_no=") - 8);
                            else
                                lexical.target_code = link.Substring(link.IndexOf("word_no=") + 8);
                        }
                    }
                    wordDetail.lexicals.Add(lexical);
                }
                wordDetail.lexicals.Sort((a, b) => {
                    return String.Compare(a.type, b.type);
                });
            }


            //관사와 하위 항목
            if (xDoc.Root.Element("item").Element("word_info").Element("pos_info") != null)
            {
                IEnumerable<XElement> pos_infos = xDoc.Root.Element("item").Element("word_info").Elements("pos_info");

                wordDetail.poses = new List<WordDetailItem.PosItem>();
                for (int i = 0; i < pos_infos.Count(); i++)
                {
                    //품사 명
                    if (pos_infos.ElementAt(i).Element("pos") != null)
                        wordDetail.poses.Add(new WordDetailItem.PosItem { pos = (string)pos_infos.ElementAt(i).Element("pos") });
                    else
                        wordDetail.poses.Add(new WordDetailItem.PosItem { pos = "" });


                    //문형 정보
                    if (pos_infos.ElementAt(i).Element("comm_pattern_info") != null)
                    {
                        IEnumerable<XElement> comm_pattern_infos = pos_infos.ElementAt(i).Elements("comm_pattern_info");

                        wordDetail.poses[i].patterns = new List<WordDetailItem.PatternItem>();
                        for (int j = 0; j < comm_pattern_infos.Count(); j++)
                        {
                            //문형
                            wordDetail.poses[i].patterns.Add(new WordDetailItem.PatternItem { pattern = new List<string>() }); ;
                            if (pos_infos.ElementAt(i).Element("comm_pattern_info").Element("pattern_info") != null)
                            {
                                IEnumerable<XElement> pattern_infos = comm_pattern_infos.ElementAt(j).Elements("pattern_info");

                                for (int k = 0; k < pattern_infos.Count(); k++)
                                    wordDetail.poses[i].patterns[j].pattern.Add((string)pattern_infos.ElementAt(k).Element("pattern"));
                            }

                            //문형에 적용되는 문법
                            if (comm_pattern_infos.ElementAt(j).Element("grammar_info") != null)
                                wordDetail.poses[i].patterns[j].grammar = (string)comm_pattern_infos.ElementAt(j).Element("grammar_info").Element("grammar");

                            //정의와 예시(sense_info)
                            if (comm_pattern_infos.ElementAt(j).Element("sense_info") != null)
                            {
                                IEnumerable<XElement> sense_infos = comm_pattern_infos.ElementAt(j).Elements("sense_info");

                                wordDetail.poses[i].patterns[j].definitions = new List<WordDetailItem.DefinitionItem>();

                                for (int k = 0; k < sense_infos.Count(); k++)
                                {
                                    wordDetail.poses[i].patterns[j].definitions.Add(new WordDetailItem.DefinitionItem());

                                    //분류(cat)
                                    if (sense_infos.ElementAt(k).Element("cat_info") != null)
                                        wordDetail.poses[i].patterns[j].definitions[k].cat = (string)sense_infos.ElementAt(k).Element("cat_info").Element("cat");

                                    //학명
                                    if (sense_infos.ElementAt(k).Element("scientific_name") != null)
                                        wordDetail.poses[i].patterns[j].definitions[k].scientific_name = (string)sense_infos.ElementAt(k).Element("scientific_name");

                                    //sense_pattern_info 정의 문형
                                    if (sense_infos.ElementAt(k).Element("sense_pattern_info") != null)
                                        wordDetail.poses[i].patterns[j].definitions[k].sense_pattern_info = (string)sense_infos.ElementAt(k).Element("sense_pattern_info").Element("pattern");

                                    //sense_grammar 정의에 참고하는 말
                                    if (sense_infos.ElementAt(k).Element("sense_grammar_info") != null)
                                        wordDetail.poses[i].patterns[j].definitions[k].sense_grammar = (string)sense_infos.ElementAt(k).Element("sense_grammar_info").Element("grammar");

                                    //정의
                                    if (sense_infos.ElementAt(k).Element("definition") != null)
                                    {
                                        wordDetail.poses[i].patterns[j].definitions[k].definition = (string)sense_infos.ElementAt(k).Element("definition");

                                        //하이퍼링크
                                        if (sense_infos.ElementAt(k).Element("definition_original") != null)
                                        {
                                            string definition_original = (string)sense_infos.ElementAt(k).Element("definition_original");

                                            //<word_no> 또는 <sense_no> 포함 시
                                            if (definition_original.Contains("<word_no>") || definition_original.Contains("<sense_no>"))
                                            {
                                                List<int> link_type = new List<int>(); // word_no : 0, sense_no : 1
                                                List<string> link_targets = new List<string>();
                                                List<string> link_texts = new List<string>();

                                                while (definition_original.Contains("<word_no>") || definition_original.Contains("<sense_no>"))
                                                {
                                                    string tag = "word_no";
                                                    if (!definition_original.Contains("<word_no>") ||
                                                        (definition_original.Contains("<sense_no>") &&
                                                        definition_original.IndexOf("<word_no>") > definition_original.IndexOf("<sense_no>")))
                                                        tag = "sense_no";

                                                    string link_target = definition_original.Substring(definition_original.IndexOf($"<{tag}>") + tag.Length + 2, definition_original.IndexOf($"</{tag}>") - definition_original.IndexOf($"<{tag}>") - tag.Length - 2);
                                                    string link_text = "";
                                                    definition_original = definition_original.Substring(definition_original.IndexOf($"</{tag}>") + tag.Length + 3);
                                                    if (definition_original.Contains("_."))
                                                    {
                                                        link_text = "_" + definition_original.Substring(0, definition_original.IndexOf("_."));
                                                        definition_original = definition_original.Substring(definition_original.IndexOf("_.") + 1);
                                                    }
                                                    else if (definition_original.Contains("’"))
                                                    {
                                                        link_text = "’" + definition_original.Substring(0, definition_original.IndexOf("’"));
                                                        definition_original = definition_original.Substring(definition_original.IndexOf("’") + 1);
                                                    }

                                                    if (link_text != "")
                                                    {
                                                        if (tag != "sense_no")
                                                        {
                                                            link_type.Add(0);
                                                            link_targets.Add(link_target);
                                                        }
                                                        else
                                                        {
                                                            link_type.Add(1);
                                                            link_targets.Add("0");
                                                        }
                                                        link_texts.Add(link_text);
                                                    }
                                                }

                                                string definition = (string)sense_infos.ElementAt(k).Element("definition_original");

                                                for (int hl = 0; hl < link_targets.Count; hl++)
                                                {
                                                    string tag = (link_type[hl] == 0) ? "word_no" : "sense_no";
                                                    if (link_texts[hl].StartsWith('_'))
                                                    {
                                                        definition = definition.Replace(definition.Substring(definition.IndexOf($"<{tag}>"), definition.IndexOf("_", definition.IndexOf($"</{tag}>") + tag.Length + 3) + 1 - definition.IndexOf($"<{tag}>")), $"<link target=\"{link_targets[hl]}\">{link_texts[hl].Substring(1)}</link>");
                                                    }
                                                    else if (link_texts[hl].StartsWith('’'))
                                                    {
                                                        definition = definition.Replace(definition.Substring(definition.IndexOf($"<{tag}>"), definition.IndexOf("’", definition.IndexOf($"<{tag}>")) - definition.IndexOf($"<{tag}>")), $"<link target=\"{link_targets[hl]}\">{link_texts[hl].Substring(1)}</link>");
                                                    }
                                                }

                                                wordDetail.poses[i].patterns[j].definitions[k].definition = definition;
                                            }
                                        }
                                    }

                                    //예시
                                    if (sense_infos.ElementAt(k).Element("example_info") != null)
                                    {
                                        IEnumerable<XElement> example_info = sense_infos.ElementAt(k).Elements("example_info");

                                        wordDetail.poses[i].patterns[j].definitions[k].examples = new List<string>();
                                        for (int l = 0; l < example_info.Count(); l++)
                                        {
                                            string example = (string)example_info.ElementAt(l).Element("example");
                                            if (example_info.ElementAt(l).Element("source") != null)
                                                example += $" ≪{(string)example_info.ElementAt(l).Element("source")}≫";
                                            wordDetail.poses[i].patterns[j].definitions[k].examples.Add(example);
                                        }
                                    }

                                    //단어 관계
                                    if (sense_infos.ElementAt(k).Element("lexical_info") != null)
                                    {
                                        IEnumerable<XElement> lexical_infos = sense_infos.ElementAt(k).Elements("lexical_info");

                                        wordDetail.poses[i].patterns[j].definitions[k].lexicals = new List<WordDetailItem.LexicalItem>();
                                        for (int l = 0; l < lexical_infos.Count(); l++)
                                        {
                                            WordDetailItem.LexicalItem lexical = new WordDetailItem.LexicalItem();
                                            if (lexical_infos.ElementAt(l).Element("type") != null)
                                                lexical.type = (string)lexical_infos.ElementAt(l).Element("type");
                                            if (lexical_infos.ElementAt(l).Element("word") != null)
                                                lexical.word = (string)lexical_infos.ElementAt(l).Element("word");
                                            if (lexical_infos.ElementAt(l).Element("link") != null)
                                            {
                                                string link = (string)lexical_infos.ElementAt(l).Element("link");
                                                if (link.Contains("word_no="))
                                                {
                                                    if (link.IndexOf("&", link.IndexOf("word_no=") + 8) != -1)
                                                        lexical.target_code = link.Substring(link.IndexOf("word_no=") + 8, link.IndexOf("&", link.IndexOf("word_no=") + 8) - link.IndexOf("word_no=") - 8);
                                                    else
                                                        lexical.target_code = link.Substring(link.IndexOf("word_no=") + 8);
                                                }
                                            }
                                            wordDetail.poses[i].patterns[j].definitions[k].lexicals.Add(lexical);
                                        }
                                        //type에 따라 분류
                                        wordDetail.poses[i].patterns[j].definitions[k].lexicals.Sort((a, b) =>
                                        {
                                            return String.Compare(a.type, b.type);
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                //규범 정보 norm
                if (xDoc.Root.Element("item").Element("word_info").Element("norm_info") != null)
                {
                    IEnumerable<XElement> norms = xDoc.Root.Element("item").Element("word_info").Elements("norm_info");

                    wordDetail.norms = new List<string>();
                    for (int i = 0; i < norms.Count(); i++)
                    {
                        wordDetail.norms.Add((string)norms.ElementAt(i).Element("desc"));
                    }
                }

                //어원
                if (xDoc.Root.Element("item").Element("word_info").Element("origin") != null)
                {
                    string origin = (string)xDoc.Root.Element("item").Element("word_info").Element("origin");
                    if (origin.Contains("<equ>&#x21BC;</equ>"))
                        origin = origin.Replace("<equ>&#x21BC;</equ>", "↼");
                    if (origin.Contains("**＊**"))
                        origin = origin.Replace("**＊**", "＊");

                    wordDetail.origin = origin;
                    wordDetail.IsOriginExist = true;
                }

                //관용구 속담
                if (xDoc.Root.Element("item").Element("word_info").Element("relation_info") != null)
                {
                    IEnumerable<XElement> relations = xDoc.Root.Element("item").Element("word_info").Elements("relation_info");

                    wordDetail.relations = new List<WordDetailItem.RelationItem>();
                    for (int i = 0; i < relations.Count(); i++)
                    {
                        string wordTemp = (string)relations.ElementAt(i).Element("word");
                        string typeTemp = (string)relations.ElementAt(i).Element("type");
                        string target_codeTemp = (string)relations.ElementAt(i).Element("link_target_code");

                        ERelationType EtypeTemp = ERelationType.idiom;
                        if (typeTemp == "관용구")
                            EtypeTemp = ERelationType.idiom;
                        else if (typeTemp == "속담")
                            EtypeTemp = ERelationType.proverb;

                        wordDetail.relations.Add(new WordDetailItem.RelationItem { word = wordTemp, type = EtypeTemp, target_code = target_codeTemp });
                    }
                    wordDetail.IsRelationExist = true;
                }

            }

            return wordDetail;
        }

        private async void ShowErrorMessage(string error_code, string error_message, string PrimaryButtonText)
        {
            #region URL_LIST
            const string URLCODE020 = "https://costudio1122.blogspot.com/p/2.html";
            const string URLCODE021 = "https://costudio1122.blogspot.com/p/api.html";
            #endregion

            var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            var contentDialog = new ContentDialog
            {
                Title = "Error",
                Content = error_message,
                CloseButtonText = res.GetString("ContentDialogText2")
            };
            if (PrimaryButtonText != null)
                contentDialog.PrimaryButtonText = PrimaryButtonText;

            ContentDialogResult result = await contentDialog.ShowAsync();

            //도움말 클릭시 웹페이지 열기
            if (PrimaryButtonText != null && result == ContentDialogResult.Primary)
            {
                if (error_code == "020")
                    await Windows.System.Launcher.LaunchUriAsync(new Uri(URLCODE020));
                else if (error_code == "021")
                    await Windows.System.Launcher.LaunchUriAsync(new Uri(URLCODE021));
            }
        }


    }
}