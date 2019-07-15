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

            var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            ComboBoxAPIKey.Items.Add(res.GetString("ComboBoxAPIKeyItemPublic"));
            ComboBoxAPIKey.Items.Add(res.GetString("ComboBoxAPIKeyItemCustom"));

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values["#UseDevelopermode"] == null)
                localSettings.Values["#UseDevelopermode"] = false;

            if (localSettings.Values["#SearchEngine"] == null)
                localSettings.Values["#SearchEngine"] = "DicAppSearch";

            if (localSettings.Values["#DisplayFont"] == null)
                localSettings.Values["#DisplayFont"] = "나눔바른고딕 옛한글";

            if (localSettings.Values["#UseCustomAPIKey"] == null)
                localSettings.Values["#UseCustomAPIKey"] = false;

            if (localSettings.Values["#APIKey"] == null)
                localSettings.Values["#APIKey"] = "C58534E2D39CF7CA69BCA193541C1688";

            CheckDevelopermode.IsChecked = (bool)localSettings.Values["#UseDevelopermode"];
            if ((string)localSettings.Values["#SearchEngine"] == "DicAppSearch")
                RadioButtonDicAppSearch.IsChecked = true;
            else
                RadioButtonDicWeb.IsChecked = true;

            if ((string)localSettings.Values["#DisplayFont"] == "나눔바른고딕 옛한글")
                ComboBoxFont.SelectedIndex = 0;
            else if ((string)localSettings.Values["#DisplayFont"] == "맑은 고딕")
                ComboBoxFont.SelectedIndex = 1;

            if ((bool)localSettings.Values["#UseCustomAPIKey"] == false)
                ComboBoxAPIKey.SelectedIndex = 0;
            else
            {
                ComboBoxAPIKey.SelectedIndex = 1;
                TextBoxAPIKey.Text = (string)localSettings.Values["#APIKey"];
            }

            var package = Windows.ApplicationModel.Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            Version.Text = "Version " + $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
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

            localSettings.Values["#UseDevelopermode"] = false;
            CheckDevelopermode.IsChecked = (bool)localSettings.Values["#UseDevelopermode"];
            localSettings.Values["#SearchEngine"] = "DicAppSearch";
            RadioButtonDicAppSearch.IsChecked = true;
            localSettings.Values["#DisplayFont"] = "나눔바른고딕 옛한글";
            ComboBoxFont.SelectedIndex = 0;
            localSettings.Values["#UseCustomAPIKey"] = false;
            ComboBoxAPIKey.SelectedIndex = 0;
            localSettings.Values["#APIKey"] = "C58534E2D39CF7CA69BCA193541C1688";
        }

        private void RadioButtonDicAppSearch_Checked(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            var value = localSettings.Values["#SearchEngine"];

            localSettings.Values["#SearchEngine"] = "DicAppSearch";
        }

        private void RadioButtonDicWeb_Checked(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            var value = localSettings.Values["#SearchEngine"];

            localSettings.Values["#SearchEngine"] = "Dic";
        }

        private void ComboBoxFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (ComboBoxFont.SelectedIndex == 0)
                localSettings.Values["#DisplayFont"] = "나눔바른고딕 옛한글";
            else if (ComboBoxFont.SelectedIndex == 1)
                localSettings.Values["#DisplayFont"] = "맑은 고딕";
        }

        private void ComboBoxAPIKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxAPIKey.SelectedIndex == 0)
            {
                TextBoxAPIKey.Text = "";
                TextBoxAPIKey.IsEnabled = false;

                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["#UseCustomAPIKey"] = false;
                localSettings.Values["#APIKey"] = "C58534E2D39CF7CA69BCA193541C1688";

                BtnSaveNHelp.Content = "";
            }
            if (ComboBoxAPIKey.SelectedIndex == 1)
            {
                TextBoxAPIKey.IsEnabled = true;
            }
        }

        private void TextBoxAPIKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if ((TextBoxAPIKey.Text != (string)localSettings.Values["#APIKey"]) && (ComboBoxAPIKey.SelectedIndex == 1))
            {
                BtnSaveNHelp.Content = "";
            }
        }

        private void BtnSaveNHelp_Click(object sender, RoutedEventArgs e)
        {
            if ((string)BtnSaveNHelp.Content == "") //저장
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["#APIKey"] = TextBoxAPIKey.Text;
                localSettings.Values["#UseCustomAPIKey"] = true;

                BtnSaveNHelp.Content = "";
            }
            else if ((string)BtnSaveNHelp.Content == "") //도움말
            {
                //도움말 링크 열기
                OpenWithDefaultBrowser(new Uri("https://costudio1122.blogspot.com/p/blog-page_11.html"));
            }
        }

        public async void OpenWithDefaultBrowser(Uri uri)
        {
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }
    }
}
