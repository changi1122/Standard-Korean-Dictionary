using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using 표준국어대사전.Classes;

namespace 표준국어대사전.Views
{
    public sealed partial class Settings : Page, INotifyPropertyChanged
    {
        public int ComboBoxLangIndex
        {
            get
            {
                switch (StorageManager.GetSetting<string>(StorageManager.Language))
                {
                    case "system": return 0;
                    case "ko": return 1;
                    default: return 2;
                }
            }
            set
            {
                // 화면을 다시 그릴 때 같은 값이 다시 들어오므로, 실제로 바뀔 때만 저장하고 재시작 안내를 띄운다
                if (value == ComboBoxLangIndex || value < 0)
                    return;
                switch (value)
                {
                    case 0: StorageManager.SetSetting<string>(StorageManager.Language, "system"); break;
                    case 1: StorageManager.SetSetting<string>(StorageManager.Language, "ko"); break;
                    case 2: StorageManager.SetSetting<string>(StorageManager.Language, "en"); break;
                }
                RestartNoticePanel.Visibility = Visibility.Visible;
            }
        }
        public int ComboBoxThemeIndex
        {
            get
            {
                switch (StorageManager.GetSetting<string>(StorageManager.ColorTheme))
                {
                    case "system": return 0;
                    case "Light": return 1;
                    default: return 2;
                }
            }
            set
            {
                if (value == ComboBoxThemeIndex || value < 0)
                    return;
                switch (value)
                {
                    case 0: StorageManager.SetSetting<string>(StorageManager.ColorTheme, "system"); break;
                    case 1: StorageManager.SetSetting<string>(StorageManager.ColorTheme, "Light"); break;
                    case 2: StorageManager.SetSetting<string>(StorageManager.ColorTheme, "Dark"); break;
                }
                // 재시작 없이 바로 적용
                ThemeHelper.Apply();
            }
        }
        public int ComboBoxFontSelectedIndex
        {
            get
            {
                return (StorageManager.GetSetting<string>(StorageManager.DisplayFont) == "나눔바른고딕 옛한글") ? 0 : 1;
            }
            set
            {
                if (value == 0) StorageManager.SetSetting<string>(StorageManager.DisplayFont, "나눔바른고딕 옛한글");
                else StorageManager.SetSetting<string>(StorageManager.DisplayFont, "맑은 고딕");
            }
        }
        public int ComboBoxFontSizeIndex
        {
            get
            {
                // 뜻풀이 화면의 +/- 버튼으로 생긴 부동소수점 오차를 반올림으로 흡수
                switch (Math.Round(StorageManager.GetSetting<double>(StorageManager.FONTMAGNIFICATION), 1))
                {
                    case 1.1: return 1;
                    case 1.2: return 2;
                    case 1.3: return 3;
                    case 1.4: return 4;
                    case 1.5: return 5;
                    case 1.6: return 6;
                    default:  return 0;
                }
            }
            set
            {
                switch (value)
                {
                    case 0: StorageManager.SetSetting<double>(StorageManager.FONTMAGNIFICATION, 1.0); break;
                    case 1: StorageManager.SetSetting<double>(StorageManager.FONTMAGNIFICATION, 1.1); break;
                    case 2: StorageManager.SetSetting<double>(StorageManager.FONTMAGNIFICATION, 1.2); break;
                    case 3: StorageManager.SetSetting<double>(StorageManager.FONTMAGNIFICATION, 1.3); break;
                    case 4: StorageManager.SetSetting<double>(StorageManager.FONTMAGNIFICATION, 1.4); break;
                    case 5: StorageManager.SetSetting<double>(StorageManager.FONTMAGNIFICATION, 1.5); break;
                    case 6: StorageManager.SetSetting<double>(StorageManager.FONTMAGNIFICATION, 1.6); break;
                }
            }
        }

        public bool LabWordReaderEnabled
        {
            get { return StorageManager.GetSetting<bool>(StorageManager.LabWordReaderEnabled); }
            set { StorageManager.SetSetting<bool>(StorageManager.LabWordReaderEnabled, value); }
        }

        public int ComboBoxAPIKeyIndex
        {
            get
            {
                return (StorageManager.GetSetting<bool>(StorageManager.UseCustomAPIKey) == false) ? 0 : 1;
            }
            set
            {
                if (value == 0)
                {
                    StorageManager.SetSetting<bool>(StorageManager.UseCustomAPIKey, false);
                    StorageManager.SetSetting<string>(StorageManager.APIKey, StorageManager.PublicAPIKey);
                }
            }
        }

        public bool IsAPIKeyInputEnabled
        {
            get; set;
        }

        public Settings()
        {
            this.InitializeComponent();
            this.IsAPIKeyInputEnabled = false;

            var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            ComboBoxAPIKey.Items.Add(res.GetString("ComboBoxAPIKeyItemPublic"));
            ComboBoxAPIKey.Items.Add(res.GetString("ComboBoxAPIKeyItemCustom"));

            ComboBoxTheme.Items.Add(res.GetString("ComboBoxThemeSystem"));
            ComboBoxTheme.Items.Add(res.GetString("ComboBoxThemeLight"));
            ComboBoxTheme.Items.Add(res.GetString("ComboBoxThemeDark"));

            if (StorageManager.GetSetting<bool>(StorageManager.UseCustomAPIKey) == false)
                ComboBoxAPIKey.SelectedIndex = 0;
            else
            {
                ComboBoxAPIKey.SelectedIndex = 1;
                TextBoxAPIKey.Text = StorageManager.GetSetting<string>(StorageManager.APIKey);
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

        private async void BtnResetSetting_Click(object sender, RoutedEventArgs e)
        {
            // 최근 검색어 등도 함께 지워지고 되돌릴 수 없으므로 먼저 확인
            var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            var dialog = new ContentDialog
            {
                Title = res.GetString("Set_ResetConfirmTitle"),
                Content = res.GetString("Set_ResetConfirmContent"),
                PrimaryButtonText = res.GetString("Set_ResetConfirmPrimary"),
                CloseButtonText = res.GetString("Set_ResetConfirmCancel"),
                DefaultButton = ContentDialogButton.Close
            };
            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
                return;

            string languageBefore = StorageManager.GetSetting<string>(StorageManager.Language);

            StorageManager.Clear();
            StorageManager.StartUpSetup();

            // 저장된 기본값으로 화면 전체(콤보 상자, 토글, API 키 입력칸)를 다시 그린다
            TextBoxAPIKey.Text = "";
            TextAPIKeyStatus.Visibility = Visibility.Collapsed;
            Bindings.Update();
            ThemeHelper.Apply();

            // 언어가 실제로 바뀐 경우에만 재시작 안내
            RestartNoticePanel.Visibility = (languageBefore != StorageManager.GetSetting<string>(StorageManager.Language))
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void BtnRestart_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync("");
        }

        private void ComboBoxAPIKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextAPIKeyStatus.Visibility = Visibility.Collapsed;
            if (ComboBoxAPIKey.SelectedIndex == 0)
            {
                TextBoxAPIKey.Text = "";
                IsAPIKeyInputEnabled = false;
                RaisePropertyChanged("IsAPIKeyInputEnabled");
            }
            if (ComboBoxAPIKey.SelectedIndex == 1)
            {
                IsAPIKeyInputEnabled = true;
                RaisePropertyChanged("IsAPIKeyInputEnabled");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var res = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            string key = TextBoxAPIKey.Text.Trim();

            // 인증키는 16진수 32자리 (오픈 API 안내 기준)
            if (!System.Text.RegularExpressions.Regex.IsMatch(key, "^[0-9A-Fa-f]{32}$"))
            {
                TextAPIKeyStatus.Text = res.GetString("Set_APIKeyInvalid");
                TextAPIKeyStatus.Visibility = Visibility.Visible;
                return;
            }

            TextBoxAPIKey.Text = key;
            StorageManager.SetSetting<string>(StorageManager.APIKey, key);
            StorageManager.SetSetting<bool>(StorageManager.UseCustomAPIKey, true);
            TextAPIKeyStatus.Text = res.GetString("Set_APIKeySaved");
            TextAPIKeyStatus.Visibility = Visibility.Visible;
        }

        public async void OpenWithDefaultBrowser(Uri uri)
        {
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
