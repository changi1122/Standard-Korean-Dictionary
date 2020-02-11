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
using Windows.UI.Popups;
using 표준국어대사전.Controls;

namespace 표준국어대사전.Classes
{
    public class DictionaryClass
    {
        const string WORD_DETAIL_URL = "https://stdict.korean.go.kr/api/view.do?&key={0}&method=TARGET_CODE&q={1}";
        string API_KEY;

        ProgressBar DetailProgressBar;

        public DictionaryClass(ProgressBar pbar)
        {
            //생성자
            DetailProgressBar = pbar;

            //API 키 처리
            API_KEY = DataStorageClass.GetSetting<string>(DataStorageClass.APIKey);
        }

        public async Task<WordDetailItem> GetWordDetail(string target_code, string wordname, int sup_no, bool showExampleItem)
        {
            DetailProgressBar.Visibility = Visibility.Visible;

            string responseBody = await DownloadWordDetailAsync(target_code);
            if (responseBody == null) //실패 여부 확인
            {
                string error_code = "404";
                string error_message = $"error_code : {error_code}" + Environment.NewLine + "message : Network Problem";
                ShowErrorMessage(error_code, error_message, null);
                DetailProgressBar.Visibility = Visibility.Collapsed;
                return null;
            }

            WordDetailItem wordDetail = new WordDetailItem();
            wordDetail = ParseWordDetail(responseBody, target_code, wordname, sup_no, showExampleItem);

            DetailProgressBar.Visibility = Visibility.Collapsed;
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

        private WordDetailItem ParseWordDetail(string responseBody, string target_code, string wordname, int sup_no, bool showExampleItem)
        {
            WordDetailItem wordDetail = new WordDetailItem();

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
            if ((int)xDoc.Descendants("total").ElementAt(0) == 0)
                return null;

            //target_code 단어 명, 어깨 번호
            wordDetail.target_code = target_code;
            wordDetail.wordname = wordname;
            wordDetail.sup_no = sup_no;

            //원어
            wordDetail.original_language = "";
            if (xDoc.Root.Element("item").Element("word_info").Element("original_language_info") != null)
            {
                IEnumerable<XElement> original_languages = xDoc.Descendants("original_language");
                for (int i = 0; i < original_languages.Count(); i++)
                    wordDetail.original_language += (string)original_languages.ElementAt(i);
            }

            //발음
            if (xDoc.Descendants("pronunciation") != null)
            {
                IEnumerable<XElement> pronunciations = xDoc.Descendants("pronunciation");
                IEnumerable<XElement> filter_prons = pronunciations.Where(p => p.Parent.Parent.Name == "word_info");
                wordDetail.prons = new List<string>();
                for (int i = 0; i < filter_prons.Count(); i++)
                {
                    wordDetail.prons.Add((string)filter_prons.ElementAt(i));
                }
            }

            //활용
            if (xDoc.Root.Element("item").Element("word_info").Element("conju_info") != null)
            {
                IEnumerable<XElement> conju_infos = xDoc.Descendants("conju_info");

                wordDetail.conjus = new List<WordDetailItem.ConjusItem>();
                for (int i = 0; i < conju_infos.Count(); i++)
                {
                    string conjusTemp = (string)conju_infos.ElementAt(i).Descendants("conjugation").ElementAt(0);
                    wordDetail.conjus.Add(new WordDetailItem.ConjusItem { conjus = conjusTemp });
                    if (conju_infos.ElementAt(i).Descendants("conjugation_info").Descendants("pronunciation") != null)
                    {
                        IEnumerable<XElement> _conju_prons = conju_infos.ElementAt(i).Descendants("conjugation_info").Descendants("pronunciation");
                        
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
                        IEnumerable<XElement> abbreviation_infos = xDoc.Descendants("abbreviation_info");

                        wordDetail.conjus[i].abbreviations = new List<WordDetailItem.AbbreviationItem>();
                        for (int j = 0; j < abbreviation_infos.Count(); j++)
                        {
                            string abbreviationTemp = (string)abbreviation_infos.ElementAt(j).Descendants("abbreviation").ElementAt(0);
                            wordDetail.conjus[i].abbreviations.Add(new WordDetailItem.AbbreviationItem { abbreviations = abbreviationTemp });

                            if (abbreviation_infos.ElementAt(j).Descendants("pronunciation") != null)
                            {
                                wordDetail.conjus[i].abbreviations[j].abbreviation_prons = new List<string>();

                                wordDetail.conjus[i].abbreviations[j].abbreviation_prons.Add("");
                                IEnumerable<XElement> _abbreviation_prons = conju_infos.ElementAt(i).Element("abbreviation_info").Descendants("pronunciation");
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

            //관사와 하위 항목
            if (xDoc.Root.Element("item").Element("word_info").Element("pos_info") != null)
            {
                IEnumerable<XElement> pos_infos = xDoc.Root.Element("item").Element("word_info").Descendants("pos_info");

                wordDetail.poses = new List<WordDetailItem.PosItem>();
                for (int i = 0; i < pos_infos.Count(); i++)
                {
                    //품사 명
                    if (pos_infos.ElementAt(i).Element("pos") != null)
                        wordDetail.poses.Add(new WordDetailItem.PosItem { pos = (string)pos_infos.ElementAt(i).Descendants("pos").ElementAt(0) });
                    else
                        wordDetail.poses.Add(new WordDetailItem.PosItem { pos = "" });


                    //문형 정보
                    if (pos_infos.ElementAt(i).Element("comm_pattern_info") != null)
                    {
                        IEnumerable<XElement> comm_pattern_infos = pos_infos.ElementAt(i).Descendants("comm_pattern_info");

                        wordDetail.poses[i].patterns = new List<WordDetailItem.PatternItem>();
                        for (int j = 0; j < comm_pattern_infos.Count(); j++)
                        {
                            //문형
                            wordDetail.poses[i].patterns.Add(new WordDetailItem.PatternItem { pattern = new List<string>() }); ;
                            if (pos_infos.ElementAt(i).Element("comm_pattern_info").Element("pattern_info") != null)
                            {
                                IEnumerable<XElement> pattern_infos = comm_pattern_infos.ElementAt(j).Descendants("pattern_info");

                                for (int k = 0; k < pattern_infos.Count(); k++)
                                    wordDetail.poses[i].patterns[j].pattern.Add((string)pattern_infos.ElementAt(k).Descendants("pattern").ElementAt(0));
                            }

                            //문형에 적용되는 문법
                            if (comm_pattern_infos.ElementAt(j).Element("grammar_info") != null)
                                wordDetail.poses[i].patterns[j].grammar = (string)comm_pattern_infos.ElementAt(j).Element("grammar_info").Descendants("grammar").ElementAt(0);

                            //정의와 예시(sense_info)
                            if (comm_pattern_infos.ElementAt(j).Element("sense_info") != null)
                            {
                                IEnumerable<XElement> sense_infos = comm_pattern_infos.ElementAt(j).Descendants("sense_info");

                                wordDetail.poses[i].patterns[j].definitions = new List<WordDetailItem.DefinitionItem>();

                                for (int k = 0; k < sense_infos.Count(); k++)
                                {
                                    wordDetail.poses[i].patterns[j].definitions.Add(new WordDetailItem.DefinitionItem());

                                    //분류(cat)
                                    if (sense_infos.ElementAt(k).Element("cat_info") != null)
                                        wordDetail.poses[i].patterns[j].definitions[k].cat = (string)sense_infos.ElementAt(k).Element("cat_info").Descendants("cat").ElementAt(0);

                                    //sense_pattern_info 정의 문형
                                    if (sense_infos.ElementAt(k).Element("sense_pattern_info") != null)
                                        wordDetail.poses[i].patterns[j].definitions[k].sense_pattern_info = (string)sense_infos.ElementAt(k).Element("sense_pattern_info").Descendants("pattern").ElementAt(0);

                                    //sense_grammar 정의에 참고하는 말
                                    if (sense_infos.ElementAt(k).Element("sense_grammar_info") != null)
                                        wordDetail.poses[i].patterns[j].definitions[k].sense_grammar = (string)sense_infos.ElementAt(k).Element("sense_grammar_info").Descendants("grammar").ElementAt(0);

                                    //정의
                                    wordDetail.poses[i].patterns[j].definitions[k].definition = (string)sense_infos.ElementAt(k).Descendants("definition").ElementAt(0);

                                    //예시
                                    if (sense_infos.ElementAt(k).Element("example_info") != null)
                                    {
                                        IEnumerable<XElement> example_info = sense_infos.ElementAt(k).Descendants("example_info");

                                        wordDetail.poses[i].patterns[j].definitions[k].examples = new List<string>();
                                        for (int l = 0; l < example_info.Count(); l++)
                                        {
                                            string example = (string)example_info.ElementAt(l).Descendants("example").ElementAt(0);
                                            if (example_info.ElementAt(l).Element("source") != null)
                                                example += $" ≪{(string)example_info.ElementAt(l).Descendants("source").ElementAt(0)}≫";
                                            wordDetail.poses[i].patterns[j].definitions[k].examples.Add(example);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //규범 정보 norm
                if (xDoc.Root.Element("item").Element("word_info").Element("norm_info") != null)
                {
                    IEnumerable<XElement> norms = xDoc.Root.Element("item").Element("word_info").Descendants("norm_info");

                    wordDetail.norms = new List<string>();
                    for (int i = 0; i < norms.Count(); i++)
                    {
                        wordDetail.norms.Add((string)norms.ElementAt(i).Descendants("desc").ElementAt(0));
                    }
                }

                //어원
                if (xDoc.Root.Element("item").Element("word_info").Element("origin") != null)
                {
                    string origin = (string)xDoc.Root.Element("item").Element("word_info").Descendants("origin").ElementAt(0);
                    if (origin.IndexOf("<equ>&#x21BC;</equ>") != -1)
                        origin = origin.Replace("<equ>&#x21BC;</equ>", "↼");

                    wordDetail.origin = origin;
                    wordDetail.IsOriginExist = true;
                }

                //관용구 속담
                if (xDoc.Root.Element("item").Element("word_info").Element("relation_info") != null)
                {
                    IEnumerable<XElement> relations = xDoc.Root.Element("item").Element("word_info").Descendants("relation_info");

                    wordDetail.relations = new List<WordDetailItem.RelationItem>();
                    for (int i = 0; i < relations.Count(); i++)
                    {
                        string wordTemp = (string)relations.ElementAt(i).Descendants("word").ElementAt(0);
                        string typeTemp = (string)relations.ElementAt(i).Descendants("type").ElementAt(0);
                        string target_codeTemp = (string)relations.ElementAt(i).Descendants("link_target_code").ElementAt(0);

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