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
using 표준국어대사전.Classes;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace 표준국어대사전.Pages
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public bool RadioBtnDicAppSearch
        {
            get
            {
                if (new DataStorageClass().GetSetting<string>(DataStorageClass.SearchEngine) == "DicAppSearch") return true;
                else return false;
            }
            set
            {
                if (value == true) new DataStorageClass().SetSetting<string>(DataStorageClass.SearchEngine, "DicAppSearch");
                else new DataStorageClass().SetSetting<string>(DataStorageClass.SearchEngine, "Dic");
            }
        }
        public bool RadioBtnDic
        {
            get
            {
                if (new DataStorageClass().GetSetting<string>(DataStorageClass.SearchEngine) == "Dic") return true;
                else return false;
            }
            set
            {
                if (value == true) new DataStorageClass().SetSetting<string>(DataStorageClass.SearchEngine, "Dic");
                else new DataStorageClass().SetSetting<string>(DataStorageClass.SearchEngine, "DicAppSearch");
            }
        }
        public bool IsEnableDevelopermode
        {
            get { return new DataStorageClass().GetSetting<bool>(DataStorageClass.UseDevelopermode); }
            set { new DataStorageClass().SetSetting<bool>(DataStorageClass.UseDevelopermode, value); }
        }
        public int ComboBoxFontSelectedIndex
        {
            get
            {
                if (new DataStorageClass().GetSetting<string>(DataStorageClass.DisplayFont) == "나눔바른고딕 옛한글") return 0;
                else return 1;
            }
            set
            {
                if (value == 0) new DataStorageClass().SetSetting<string>(DataStorageClass.DisplayFont, "나눔바른고딕 옛한글");
                else new DataStorageClass().SetSetting<string>(DataStorageClass.DisplayFont, "맑은 고딕");
            }
        }
        public int ComboBoxAPIKeyIndex
        {
            get
            {
                if (new DataStorageClass().GetSetting<bool>(DataStorageClass.UseCustomAPIKey) == false) return 0;
                else return 1;
            }
            set
            {
                if (value == 0) { new DataStorageClass().SetSetting<bool>(DataStorageClass.UseCustomAPIKey, false);
                                  new DataStorageClass().SetSetting<string>(DataStorageClass.APIKey, "C58534E2D39CF7CA69BCA193541C1688"); }
            }
        }
        public int ComboBoxLangIndex
        {
            get
            {
                if (new DataStorageClass().GetSetting<string>(DataStorageClass.Language) == "system") return 0;
                else if (new DataStorageClass().GetSetting<string>(DataStorageClass.Language) == "ko-KR") return 1;
                else return 2;
            }
            set
            {
                if (value == 0) new DataStorageClass().SetSetting<string>(DataStorageClass.Language, "system");
                else if (value == 1) new DataStorageClass().SetSetting<string>(DataStorageClass.Language, "ko-KR");
                else if (value == 2) new DataStorageClass().SetSetting<string>(DataStorageClass.Language, "en");
                TextRestartNotice.Visibility = Visibility.Visible;
            }
        }

        public Settings()
        {
            this.InitializeComponent();

            var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            ComboBoxAPIKey.Items.Add(res.GetString("ComboBoxAPIKeyItemPublic"));
            ComboBoxAPIKey.Items.Add(res.GetString("ComboBoxAPIKeyItemCustom"));

            DataStorageClass data = new DataStorageClass();

            if (data.GetSetting<bool>(DataStorageClass.UseCustomAPIKey) == false)
                ComboBoxAPIKey.SelectedIndex = 0;
            else
            {
                ComboBoxAPIKey.SelectedIndex = 1;
                TextBoxAPIKey.Text = data.GetSetting<string>(DataStorageClass.APIKey);
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

        private void BtnResetSetting_Click(object sender, RoutedEventArgs e)
        {
            new DataStorageClass().Clear();
            new DataStorageClass().StartUpSetup();
            RadioButtonDicAppSearch.IsChecked = true;
            CheckDevelopermode.IsChecked = false;
            ComboBoxFont.SelectedIndex = 0; ;
            ComboBoxAPIKey.SelectedIndex = 0;
            ComboBoxLang.SelectedIndex = 0;
        }

        private void RadioButtonDicAppSearch_Checked(object sender, RoutedEventArgs e)
        {
            RadioBtnDicAppSearch = true;
            RadioBtnDic = false;
        }

        private void RadioButtonDicWeb_Checked(object sender, RoutedEventArgs e)
        {
            RadioBtnDicAppSearch = false;
            RadioBtnDic = true;
        }

        private void ComboBoxAPIKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxAPIKey.SelectedIndex == 0)
            {
                TextBoxAPIKey.Text = "";
                TextBoxAPIKey.IsEnabled = false;

                BtnSaveNHelp.Content = "";
            }
            if (ComboBoxAPIKey.SelectedIndex == 1)
            {
                TextBoxAPIKey.IsEnabled = true;
            }
        }

        private void TextBoxAPIKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((TextBoxAPIKey.Text != new DataStorageClass().GetSetting<string>(DataStorageClass.APIKey)) && (ComboBoxAPIKey.SelectedIndex == 1))
            {
                BtnSaveNHelp.Content = "";
            }
        }

        private void BtnSaveNHelp_Click(object sender, RoutedEventArgs e)
        {
            if ((string)BtnSaveNHelp.Content == "") //저장
            {
                new DataStorageClass().SetSetting<string>(DataStorageClass.APIKey, TextBoxAPIKey.Text);
                new DataStorageClass().SetSetting<bool>(DataStorageClass.UseCustomAPIKey, true);

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
