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
                if (StorageManager.GetSetting<string>(StorageManager.SearchEngine) == "DicAppSearch") return true;
                else return false;
            }
            set
            {
                if (value == true) StorageManager.SetSetting<string>(StorageManager.SearchEngine, "DicAppSearch");
                else StorageManager.SetSetting<string>(StorageManager.SearchEngine, "Dic");
            }
        }
        public bool RadioBtnDic
        {
            get
            {
                if (StorageManager.GetSetting<string>(StorageManager.SearchEngine) == "Dic") return true;
                else return false;
            }
            set
            {
                if (value == true) StorageManager.SetSetting<string>(StorageManager.SearchEngine, "Dic");
                else StorageManager.SetSetting<string>(StorageManager.SearchEngine, "DicAppSearch");
            }
        }
        public bool IsEnableDevelopermode
        {
            get { return StorageManager.GetSetting<bool>(StorageManager.UseDevelopermode); }
            set { StorageManager.SetSetting<bool>(StorageManager.UseDevelopermode, value); }
        }
        public int ComboBoxFontSelectedIndex
        {
            get
            {
                if (StorageManager.GetSetting<string>(StorageManager.DisplayFont) == "나눔바른고딕 옛한글") return 0;
                else return 1;
            }
            set
            {
                if (value == 0) StorageManager.SetSetting<string>(StorageManager.DisplayFont, "나눔바른고딕 옛한글");
                else StorageManager.SetSetting<string>(StorageManager.DisplayFont, "맑은 고딕");
            }
        }
        public int ComboBoxAPIKeyIndex
        {
            get
            {
                if (StorageManager.GetSetting<bool>(StorageManager.UseCustomAPIKey) == false) return 0;
                else return 1;
            }
            set
            {
                if (value == 0) { StorageManager.SetSetting<bool>(StorageManager.UseCustomAPIKey, false);
                                  StorageManager.SetSetting<string>(StorageManager.APIKey, "C58534E2D39CF7CA69BCA193541C1688"); }
            }
        }
        public int ComboBoxLangIndex
        {
            get
            {
                if (StorageManager.GetSetting<string>(StorageManager.Language) == "system") return 0;
                else if (StorageManager.GetSetting<string>(StorageManager.Language) == "ko") return 1;
                else return 2;
            }
            set
            {
                if (value == 0) StorageManager.SetSetting<string>(StorageManager.Language, "system");
                else if (value == 1) StorageManager.SetSetting<string>(StorageManager.Language, "ko");
                else if (value == 2) StorageManager.SetSetting<string>(StorageManager.Language, "en");
                TextRestartNotice.Visibility = Visibility.Visible;
            }
        }
        public int ComboBoxThemeIndex
        {
            get
            {
                if (StorageManager.GetSetting<string>(StorageManager.ColorTheme) == "system") return 0;
                else if (StorageManager.GetSetting<string>(StorageManager.ColorTheme) == "Light") return 1;
                else return 2;
            }
            set
            {
                if (value == 0) StorageManager.SetSetting<string>(StorageManager.ColorTheme, "system");
                else if (value == 1) StorageManager.SetSetting<string>(StorageManager.ColorTheme, "Light");
                else if (value == 2) StorageManager.SetSetting<string>(StorageManager.ColorTheme, "Dark");
                TextRestartNotice2.Visibility = Visibility.Visible;
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
            RadioButtonDicAppSearch.IsChecked = true;
            CheckDevelopermode.IsChecked = false;
            ComboBoxFont.SelectedIndex = 0; ;
            ComboBoxAPIKey.SelectedIndex = 0;
            ComboBoxLang.SelectedIndex = 0;
            ComboBoxTheme.SelectedIndex = 0;
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
            if ((TextBoxAPIKey.Text != StorageManager.GetSetting<string>(StorageManager.APIKey)) && (ComboBoxAPIKey.SelectedIndex == 1))
            {
                BtnSaveNHelp.Content = "";
            }
        }

        private void BtnSaveNHelp_Click(object sender, RoutedEventArgs e)
        {
            if ((string)BtnSaveNHelp.Content == "") //저장
            {
                StorageManager.SetSetting<string>(StorageManager.APIKey, TextBoxAPIKey.Text);
                StorageManager.SetSetting<bool>(StorageManager.UseCustomAPIKey, true);

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

        private void BtnLabFunction_Click(object sender, RoutedEventArgs e)
        {
            var SubGrid = new Grid
            {
                Name = "SubGrid",
                Margin = new Thickness(0, 40, 0, 0),
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(Windows.UI.Colors.White)
            };

            var ColorBar = new Windows.UI.Xaml.Shapes.Rectangle
            {
                Margin = new Thickness(0, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 40,
                Fill = (SolidColorBrush)Resources["BarColor"]
            };
            SubGrid.Children.Add(ColorBar);

            var CloseBtn = new Button
            {
                Name = "BtnSubFrameClose",
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Height = 40,
                Width = 40,
                FontSize = 20,
                Content = "",
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Background = null,
                Foreground = new SolidColorBrush(Windows.UI.Colors.Black)
            };
            CloseBtn.Click += BtnSubFrameClose_Click;
            SubGrid.Children.Add(CloseBtn);

            var SubFrame = new Frame
            {
                Name = "SubFrame",
                Margin = new Thickness(1, 40, 1, 1),
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            SubFrame.Loaded += SubFrame_Loaded;
            SubGrid.Children.Add(SubFrame);

            BasicGrid.Children.Add(SubGrid);
        }

        private void SubFrame_Loaded(object sender, RoutedEventArgs e)
        {
            Frame frame = (Frame)sender;
            frame.Navigate(typeof(Pages.LabFunction));
        }

        private void BtnSubFrameClose_Click(object sender, RoutedEventArgs e)
        {
            BasicGrid.Children.Remove((UIElement)FindName("SubGrid"));
        }
    }
}
