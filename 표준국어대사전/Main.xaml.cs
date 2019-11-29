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
using 표준국어대사전.Classes;

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
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(Pages.Settings));
            }
            else
            {
                switch (args.InvokedItem)
                {
                    case "Pages.Dic":  //검색
                        if (DataStorageClass.GetSetting<string>(DataStorageClass.SearchEngine) == "DicAppSearch")
                            ContentFrame.Navigate(typeof(Pages.DicAppSearch));
                        else
                            ContentFrame.Navigate(typeof(Pages.Dic));
                        break;

                    case "Pages.HangulSpelling":  //한글 맞춤법
                        ContentFrame.Navigate(typeof(Pages.HangulSpelling));
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
                ContentFrame.Navigate(typeof(Pages.Settings));
            }
            else
            {

                NavigationViewItem item = args.SelectedItem as NavigationViewItem;

                switch (item.Tag)
                {
                    case "Pages.Dic":  //검색
                        if (DataStorageClass.GetSetting<string>(DataStorageClass.SearchEngine) == "DicAppSearch")
                            ContentFrame.Navigate(typeof(Pages.DicAppSearch));
                        else
                            ContentFrame.Navigate(typeof(Pages.Dic));
                        break;

                    case "Pages.HangulSpelling":  //한글 맞춤법
                        ContentFrame.Navigate(typeof(Pages.HangulSpelling));
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
