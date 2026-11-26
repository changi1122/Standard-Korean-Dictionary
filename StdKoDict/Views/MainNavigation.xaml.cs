using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using MUXC = Microsoft.UI.Xaml.Controls;

namespace 표준국어대사전.Views
{
    public sealed partial class MainNavigation : Page
    {
        public MainNavigation()
        {
            InitializeComponent();
            표준국어대사전.Classes.ThemeHelper.UpdateTitleBar();

            // NavigationView 최초 SelectedItem 설정
            foreach (MUXC.NavigationViewItem item in NaviView.MenuItems)
            {
                if (item is MUXC.NavigationViewItem && item.Tag.ToString() == "Views.Search")
                {
                    NaviView.SelectedItem = item;
                    break;
                }
            }
        }

        private void NaviView_SelectionChanged(MUXC.NavigationView sender, MUXC.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(Views.Settings));
            }
            else
            {

                var item = args.SelectedItem as MUXC.NavigationViewItem;

                switch (item.Tag)
                {
                    case "Views.Search":  // 검색  
                        ContentFrame.Navigate(typeof(Views.Search));
                        break;

                    case "Views.HangulSpelling":  // 한글 맞춤법
                        ContentFrame.Navigate(typeof(Views.HangulSpelling));
                        break;

                    case "Views.Revision":  // 수정 내용
                        ContentFrame.Navigate(typeof(Views.Revision));
                        break;

                    case "Views.SpellingChecker":  // 맞춤법/문법 검사기
                        ContentFrame.Navigate(typeof(Views.SpellingChecker));
                        break;
                }
            }
        }
    }
}
