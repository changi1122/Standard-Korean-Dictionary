using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 표준국어대사전.Classes;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace 표준국어대사전.Controls
{
    public sealed partial class ConMemo : UserControl
    {
        private bool IsFolded = false;

        public ConMemo()
        {
            this.InitializeComponent();
        }

        private void BtnMemoFold_Click(object sender, RoutedEventArgs e)
        {
            if (IsFolded)
            {
                BtnMemoCopy.Visibility = Visibility.Visible;
                BtnMemoDelete.Visibility = Visibility.Visible;
                TextboxMemoBox.Visibility = Visibility.Visible;
                WhiteBack.Visibility = Visibility.Visible;
                MaxHeight = Double.PositiveInfinity;
                BtnMemoFold.Content = "";
                BtnMemoFold.VerticalAlignment = VerticalAlignment.Top;
                BtnMemoClose.VerticalAlignment = VerticalAlignment.Top;
                IsFolded = false;
            }
            else
            {
                BtnMemoCopy.Visibility = Visibility.Collapsed;
                BtnMemoDelete.Visibility = Visibility.Collapsed;
                TextboxMemoBox.Visibility = Visibility.Collapsed;
                WhiteBack.Visibility = Visibility.Collapsed;
                MaxHeight = 40;
                BtnMemoFold.Content = "";
                BtnMemoFold.VerticalAlignment = VerticalAlignment.Bottom;
                BtnMemoClose.VerticalAlignment = VerticalAlignment.Bottom;
                IsFolded = true;
            }
        }

        private void BtnMemoClose_Click(object sender, RoutedEventArgs e)
        {
            DataStorageClass.SetSetting(DataStorageClass.MemoData, TextboxMemoBox.Text);

            if (this.Parent as Grid != null)
                (this.Parent as Grid).Children.Remove(this);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            string MemoData = DataStorageClass.GetSetting<string>(DataStorageClass.MemoData);
            if (MemoData != null)
                TextboxMemoBox.Text = MemoData;

            if (this.Parent as Grid != null)
                MemoGrid.Height = (this.Parent as Grid).ActualHeight * 2 / 3 - 41;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.Parent as Grid != null)
                MemoGrid.Height = (this.Parent as Grid).ActualHeight * 2 / 3 - 41;
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

        private void TextboxMemoBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DataStorageClass.SetSetting(DataStorageClass.MemoData, TextboxMemoBox.Text);
        }
    }
}
