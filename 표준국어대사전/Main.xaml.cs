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
using Microsoft.UI.Xaml.Controls;

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
            SetTitleBarColor();
        }

        private void SetTitleBarColor()
        {
            // 제목 표시줄 색상 설정
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            if (App.Current.RequestedTheme == ApplicationTheme.Light)
            {
                titleBar.BackgroundColor = Color.FromArgb(0xFF, 0xE6, 0xE6, 0xE6);
                titleBar.ButtonBackgroundColor = Color.FromArgb(0xFF, 0xE6, 0xE6, 0xE6);
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0xFF, 0xD6, 0xD6, 0xD6);
                titleBar.ButtonInactiveBackgroundColor = Color.FromArgb(0xFF, 0xE6, 0xE6, 0xE6);
                titleBar.InactiveBackgroundColor = Color.FromArgb(0xFF, 0xE6, 0xE6, 0xE6);
                titleBar.ButtonHoverForegroundColor = Colors.Black;
            }
            else if (App.Current.RequestedTheme == ApplicationTheme.Dark)
            {
                titleBar.BackgroundColor = Color.FromArgb(0x1f, 0x1f, 0x1f, 0x1f);
                titleBar.ButtonBackgroundColor = Color.FromArgb(0xFF, 0x1f, 0x1f, 0x1f);
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0xFF, 0x2f, 0x2f, 0x2f);
                titleBar.ButtonHoverForegroundColor = Colors.White;
                titleBar.ButtonInactiveBackgroundColor = Color.FromArgb(0x1f, 0x1f, 0x1f, 0x1f);
                titleBar.InactiveBackgroundColor = Color.FromArgb(0xFF, 0x1f, 0x1f, 0x1f);
            }
        }

        private void MainNavigation_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(Pages.Settings));
            }
            else
            {

                Microsoft.UI.Xaml.Controls.NavigationViewItem item = args.SelectedItem as Microsoft.UI.Xaml.Controls.NavigationViewItem;

                switch (item.Tag)
                {
                    case "Pages.Dic":  //검색  
                        ContentFrame.Navigate(typeof(Pages.DicAppSearch));
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
