using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Reflection;
using Windows.Storage;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace 표준국어대사전.Pages
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values["#UseOriginWeb"] == null)
            {
                localSettings.Values["#UseOriginWeb"] = false;
            }

            if (localSettings.Values["#UseDevelopermode"] == null)
            {
                localSettings.Values["#UseDevelopermode"] = false;
            }

            if (localSettings.Values["#SearchEngine"] == null)
            {
                localSettings.Values["#SearchEngine"] = "DicAppSearch";
            }

            if (localSettings.Values["#DisplayFont"] == null)
            {
                localSettings.Values["#DisplayFont"] = "나눔바른고딕 옛한글";
            }

            if (localSettings.Values["#DisplayFontSize"] == null)
            {
                localSettings.Values["#DisplayFontSize"] = 18;
            }

            CheckDevelopermode.IsChecked = (bool)localSettings.Values["#UseDevelopermode"];
            if ((string)localSettings.Values["#SearchEngine"] == "DicAppSearch")
                RadioButtonDicAppSearch.IsChecked = true;
            else if ((string)localSettings.Values["#SearchEngine"] == "Dic" && (bool)localSettings.Values["#UseOriginWeb"] == false)
                RadioButtonDic.IsChecked = true;
            else
                RadioButtonDicWeb.IsChecked = true;

            if ((string)localSettings.Values["#DisplayFont"] == "나눔바른고딕 옛한글")
                ComboBoxFont.SelectedIndex = 0;
            else if ((string)localSettings.Values["#DisplayFont"] == "맑은 고딕")
                ComboBoxFont.SelectedIndex = 1;

            if ((int)localSettings.Values["#DisplayFontSize"] == 24)
                ComboBoxFontSize.SelectedIndex = 0;
            else if ((int)localSettings.Values["#DisplayFontSize"] == 21)
                ComboBoxFontSize.SelectedIndex = 1;
            else if ((int)localSettings.Values["#DisplayFontSize"] == 18)
                ComboBoxFontSize.SelectedIndex = 2;
            else if ((int)localSettings.Values["#DisplayFontSize"] == 15)
                ComboBoxFontSize.SelectedIndex = 3;
            else if ((int)localSettings.Values["#DisplayFontSize"] == 12)
                ComboBoxFontSize.SelectedIndex = 4;

            Version.Text = "Version " + typeof(App).GetTypeInfo().Assembly.GetName().Version;
        }

        private async void Mail_Click(object sender, RoutedEventArgs e)
        {
            string uriToLaunch = @"mailto:changi112242@gmail.com";
            var uri = new Uri(uriToLaunch);

            await Windows.System.Launcher.LaunchUriAsync(uri);
        }

        private async void BtnReview_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9PPS9L58110J"));
        }

        private async void BtnMail_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(@"mailto:changi112242@gmail.com"));
        }

        private void CheckDevelopermode_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            var value = localSettings.Values["#UseDevelopermode"];

            localSettings.Values["#UseDevelopermode"] = CheckDevelopermode.IsChecked;
        }

        private void BtnResetSetting_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values.Clear();

            localSettings.Values["#UseOriginWeb"] = false;
            localSettings.Values["#UseDevelopermode"] = false;
            CheckDevelopermode.IsChecked = (bool)localSettings.Values["#UseDevelopermode"];
            localSettings.Values["#SearchEngine"] = "DicAppSearch";
            RadioButtonDicAppSearch.IsChecked = true;
            localSettings.Values["#DisplayFont"] = "나눔바른고딕 옛한글";
            ComboBoxFont.SelectedIndex = 0;
            localSettings.Values["#DisplayFontSize"] = 18;
            ComboBoxFontSize.SelectedIndex = 2;
            localSettings.Values["#FontCheckNoLater"] = false;
        }

        private void RadioButtonDicAppSearch_Checked(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            var value = localSettings.Values["#SearchEngine"];

            localSettings.Values["#SearchEngine"] = "DicAppSearch";
        }

        private void RadioButtonDic_Checked(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            var value = localSettings.Values["#SearchEngine"];

            localSettings.Values["#SearchEngine"] = "Dic";
            localSettings.Values["#UseOriginWeb"] = false;
        }

        private void RadioButtonDicWeb_Checked(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            var value = localSettings.Values["#SearchEngine"];

            localSettings.Values["#SearchEngine"] = "Dic";
            localSettings.Values["#UseOriginWeb"] = true;
        }

        private void ComboBoxFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (ComboBoxFont.SelectedIndex == 0)
                localSettings.Values["#DisplayFont"] = "나눔바른고딕 옛한글";
            else if (ComboBoxFont.SelectedIndex == 1)
                localSettings.Values["#DisplayFont"] = "맑은 고딕";
        }

        private void ComboBoxFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (ComboBoxFontSize.SelectedIndex == 0) //크게
                localSettings.Values["#DisplayFontSize"] = 24;
            else if (ComboBoxFontSize.SelectedIndex == 1) //조금 크게
                localSettings.Values["#DisplayFontSize"] = 21;
            else if (ComboBoxFontSize.SelectedIndex == 2) //보통
                localSettings.Values["#DisplayFontSize"] = 18;
            else if (ComboBoxFontSize.SelectedIndex == 3) //조금 작게
                localSettings.Values["#DisplayFontSize"] = 15;
            else if (ComboBoxFontSize.SelectedIndex == 4) //작게
                localSettings.Values["#DisplayFontSize"] = 12;
        }

        private void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            Controls.CheckFont checkFont = new Controls.CheckFont();
            checkFont.BtnInstall_Click(sender, e);
        }
    }
}
