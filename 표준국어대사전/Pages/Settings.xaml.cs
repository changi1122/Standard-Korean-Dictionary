using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using 표준국어대사전.Classes;

namespace 표준국어대사전.Pages
{
    public sealed partial class Settings : Page
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
                switch (value)
                {
                    case 0: StorageManager.SetSetting<string>(StorageManager.Language, "system"); break;
                    case 1: StorageManager.SetSetting<string>(StorageManager.Language, "ko"); break;
                    case 2: StorageManager.SetSetting<string>(StorageManager.Language, "en"); break;
                }
                TextRestartNotice.Visibility = Visibility.Visible;
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
                switch (value)
                {
                    case 0: StorageManager.SetSetting<string>(StorageManager.ColorTheme, "system"); break;
                    case 1: StorageManager.SetSetting<string>(StorageManager.ColorTheme, "Light"); break;
                    case 2: StorageManager.SetSetting<string>(StorageManager.ColorTheme, "Dark"); break;
                }
                TextRestartNotice2.Visibility = Visibility.Visible;
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
                switch (Math.Truncate(StorageManager.GetSetting<double>(StorageManager.FONTMAGNIFICATION) * 10) / 10)
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
                    StorageManager.SetSetting<string>(StorageManager.APIKey, "C58534E2D39CF7CA69BCA193541C1688");
                }
            }
        }

        public Settings()
        {
            this.InitializeComponent();

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

        private void BtnResetSetting_Click(object sender, RoutedEventArgs e)
        {
            StorageManager.Clear();
            StorageManager.StartUpSetup();
            ComboBoxFontSize.SelectedIndex = 0; ;
            ComboBoxFont.SelectedIndex = 0; ;
            ComboBoxAPIKey.SelectedIndex = 0;
            ComboBoxLang.SelectedIndex = 0;
            ComboBoxTheme.SelectedIndex = 0;
        }

        private void ComboBoxAPIKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxAPIKey.SelectedIndex == 0)
            {
                TextBoxAPIKey.Text = "";
                TextBoxAPIKey.IsEnabled = false;
            }
            if (ComboBoxAPIKey.SelectedIndex == 1)
            {
                TextBoxAPIKey.IsEnabled = true;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            StorageManager.SetSetting<string>(StorageManager.APIKey, TextBoxAPIKey.Text);
            StorageManager.SetSetting<bool>(StorageManager.UseCustomAPIKey, true);
        }

        public async void OpenWithDefaultBrowser(Uri uri)
        {
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }
    }
}
