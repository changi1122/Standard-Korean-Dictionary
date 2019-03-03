using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace 표준국어대사전.Controls
{
    public sealed partial class CheckFont : UserControl
    {
        public CheckFont()
        {
            this.InitializeComponent();

            TextBlockWebview.NavigateToString("<style>body { -ms-overflow-style: none }</style><font face=\"나눔바른고딕 옛한글\" style=\"font-size:15px\">닭 콩팥 훔친 집사</font>");
        }

        public async void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync(@"Fonts\");
            await Windows.System.Launcher.LaunchFolderAsync(folder);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (CheckBoxNoLater.IsChecked == true)
                localSettings.Values["#FontCheckNoLater"] = true;

            ((Grid)this.Parent).Children.Remove(this);
        }
    }
}
