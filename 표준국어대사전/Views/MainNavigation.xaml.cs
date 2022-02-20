using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 표준국어대사전.Classes;
using muxc = Microsoft.UI.Xaml.Controls;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x412에 나와 있습니다.

namespace 표준국어대사전.Views
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainNavigation : Page
    {
        public MainNavigation()
        {
            InitializeComponent();
            SetTitleBarColor();

            // NavigationView 최초 SelectedItem 설정
            foreach (muxc.NavigationViewItem item in NaviView.MenuItems)
            {
                if (item is muxc.NavigationViewItem && item.Tag.ToString() == "Views.Search")
                {
                    NaviView.SelectedItem = item;
                    break;
                }
            }
        }

        private void SetTitleBarColor()
        {
            // 제목 표시줄 색상 설정
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = (Application.Current.Resources["NavigationViewBackground"] as SolidColorBrush).Color;
            titleBar.ButtonBackgroundColor = (Application.Current.Resources["NavigationViewBackground"] as SolidColorBrush).Color;
            titleBar.InactiveBackgroundColor = (Application.Current.Resources["NavigationViewBackground"] as SolidColorBrush).Color;
            titleBar.ButtonInactiveBackgroundColor = (Application.Current.Resources["NavigationViewBackground"] as SolidColorBrush).Color;
        }

        private void NaviView_SelectionChanged(muxc.NavigationView sender, muxc.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(Pages.Settings));
            }
            else
            {

                var item = args.SelectedItem as muxc.NavigationViewItem;

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
