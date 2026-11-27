using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace 표준국어대사전.Classes
{
    /// <summary>
    /// 앱 색상 모드(시스템/밝게/어둡게)를 재시작 없이 적용한다.
    /// </summary>
    /// <remarks>
    /// Application.RequestedTheme는 실행 중에 바꿀 수 없으므로 루트 요소(Frame)의 RequestedTheme를 바꾼다.
    /// 이때 Application.Current.Resources["키"]로 꺼낸 색은 앱 시작 시 테마 기준이라 맞지 않으므로,
    /// 코드에서 테마 색이 필요하면 GetBrush를 써서 현재 적용된 테마의 색을 가져온다.
    /// </remarks>
    internal static class ThemeHelper
    {
        /// <summary>현재 적용된 테마 (밝게 또는 어둡게)</summary>
        public static ElementTheme CurrentTheme
        {
            get
            {
                switch (StorageManager.GetSetting<string>(StorageManager.ColorTheme))
                {
                    case "Light": return ElementTheme.Light;
                    case "Dark": return ElementTheme.Dark;
                    default: return GetSystemTheme();
                }
            }
        }

        /// <summary>저장된 색상 모드를 화면과 제목 표시줄에 적용한다.</summary>
        public static void Apply()
        {
            if (Window.Current?.Content is FrameworkElement root)
                root.RequestedTheme = CurrentTheme;
            UpdateTitleBar();
        }

        /// <summary>App.xaml ThemeDictionaries에 정의한 색을 현재 테마 기준으로 가져온다.</summary>
        public static SolidColorBrush GetBrush(string key)
        {
            string themeKey = (CurrentTheme == ElementTheme.Dark) ? "Dark" : "Light";
            if (Application.Current.Resources.ThemeDictionaries.TryGetValue(themeKey, out object dictionary) &&
                dictionary is ResourceDictionary themeDictionary &&
                themeDictionary.TryGetValue(key, out object brush))
                return brush as SolidColorBrush;

            return Application.Current.Resources[key] as SolidColorBrush;
        }

        /// <summary>제목 표시줄 배경과 버튼 글자색을 현재 테마에 맞춘다.</summary>
        public static void UpdateTitleBar()
        {
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            Color background = GetBrush("NavigationViewBackground").Color;
            Color foreground = (CurrentTheme == ElementTheme.Dark) ? Colors.White : Colors.Black;

            titleBar.BackgroundColor = background;
            titleBar.ButtonBackgroundColor = background;
            titleBar.InactiveBackgroundColor = background;
            titleBar.ButtonInactiveBackgroundColor = background;
            titleBar.ForegroundColor = foreground;
            titleBar.ButtonForegroundColor = foreground;
        }

        private static ElementTheme GetSystemTheme()
        {
            Color background = new UISettings().GetColorValue(UIColorType.Background);
            return (background == Colors.Black) ? ElementTheme.Dark : ElementTheme.Light;
        }
    }
}
