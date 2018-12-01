using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class ConMemo : UserControl
    {
        public ConMemo()
        {
            this.InitializeComponent();
        }

        private void BtnMemoCopy_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
            dataPackage.SetText(TextboxMemoBox.Text);
            Clipboard.SetContent(dataPackage);
        }

        private void BtnMemoDelete_Click(object sender, RoutedEventArgs e)
        {
            TextboxMemoBox.Text = "";
        }
    }
}
