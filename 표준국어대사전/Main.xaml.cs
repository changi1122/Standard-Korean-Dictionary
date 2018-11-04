using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x412에 나와 있습니다.

namespace 표준국어대사전
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values["#FirstSetup"] == null)
                localSettings.Values["#FirstSetup"] = 0;

            if((int)localSettings.Values["#FirstSetup"] < 1)
            {
                localSettings.Values["#SearchEngine"] = "DicAppSearch";
                localSettings.Values["#FirstSetup"] = 1;
            }

            if (localSettings.Values["#SearchEngine"] == null)
            {
                localSettings.Values["#SearchEngine"] = "DicAppSearch";
            }
        }

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            // set the initial SelectedItem 
            foreach (NavigationViewItemBase item in Main_Navigation.MenuItems)
            {
                if (item is NavigationViewItem && item.Tag.ToString() == "Pages.Dic")
                {
                    Main_Navigation.SelectedItem = item;
                    break;
                }
            }
        }

        private void Main_Navigation_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(Pages.Info));
            }
            else
            {
                switch (args.InvokedItem)
                {
                    case "Pages.Dic":  //검색
                        if ((string)localSettings.Values["#SearchEngine"] == "DicAppSearch")
                            ContentFrame.Navigate(typeof(Pages.DicAppSearch));
                        else
                            ContentFrame.Navigate(typeof(Pages.Dic));
                        break;

                    case "Pages.HangulSpelling":  //한글 맞춤법
                        ContentFrame.Navigate(typeof(Pages.HangulSpelling));
                        break;

                    case "Pages.StandardLanguageSpecification":  //표준어 규정
                        ContentFrame.Navigate(typeof(Pages.StandardLanguageSpecification));
                        break;

                    case "Pages.ForeignLanguageNotation":  //외래어 표기법
                        ContentFrame.Navigate(typeof(Pages.ForeignLanguageNotation));
                        break;

                    case "Pages.KoreanRomanization":  //국어의 로마자 표기법
                        ContentFrame.Navigate(typeof(Pages.KoreanRomanization));
                        break;

                    case "Pages.Adjustment":  //수정 내용
                        ContentFrame.Navigate(typeof(Pages.Adjustment));
                        break;

                    case "Pages.SpellingChecker":  //맞춤법/문법 검사기
                        ContentFrame.Navigate(typeof(Pages.SpellingChecker));
                        break;
                }
            }
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(Pages.Info));
            }
            else
            {

                NavigationViewItem item = args.SelectedItem as NavigationViewItem;

                switch (item.Tag)
                {
                    case "Pages.Dic":  //검색
                        if ((string)localSettings.Values["#SearchEngine"] == "DicAppSearch")
                            ContentFrame.Navigate(typeof(Pages.DicAppSearch));
                        else
                            ContentFrame.Navigate(typeof(Pages.Dic));
                        break;

                    case "Pages.HangulSpelling":  //한글 맞춤법
                        ContentFrame.Navigate(typeof(Pages.HangulSpelling));
                        break;

                    case "Pages.StandardLanguageSpecification":  //표준어 규정
                        ContentFrame.Navigate(typeof(Pages.StandardLanguageSpecification));
                        break;

                    case "Pages.ForeignLanguageNotation":  //외래어 표기법
                        ContentFrame.Navigate(typeof(Pages.ForeignLanguageNotation));
                        break;

                    case "Pages.KoreanRomanization":  //국어의 로마자 표기법
                        ContentFrame.Navigate(typeof(Pages.KoreanRomanization));
                        break;

                    case "Pages.Adjustment":  //수정 내용
                        ContentFrame.Navigate(typeof(Pages.Adjustment));
                        break;

                    case "Pages.SpellingChecker":  //맞춤법/문법 검사기
                        ContentFrame.Navigate(typeof(Pages.SpellingChecker));
                        break;
                }
            }
        }
    }
}
