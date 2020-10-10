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

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace 표준국어대사전.Pages
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class Adjustment : Page
    {
        #region URL Constants
        const string NOTICE201412 = "https://drive.google.com/file/d/1vZ1EMUCPT8NVORkvKeQCUBdOLC9MIhQp/view?usp=sharing";
        const string NOTICE20143 = "https://drive.google.com/file/d/1Ri-LxfBr3Kl1rgpECpJDIOvdPnWteCmg/view?usp=sharing";
        const string NOTICE20144 = "https://drive.google.com/file/d/1L7Sjr_oOT0sM10-EOF78kCIHhv7lDvM_/view?usp=sharing";
        const string NOTICE20151 = "https://drive.google.com/file/d/16zJiBCNh-lyGvYqQ56-qjw8A0VbjstEr/view?usp=sharing";
        const string NOTICE20152 = "https://drive.google.com/file/d/1605uaY_DxUm3fMMflk0to5_PjlN2v3Wy/view?usp=sharing";
        const string NOTICE20153 = "https://drive.google.com/file/d/1PJ0oRaZM3I5MguWlv-UmXUrXpGZHT3g7/view?usp=sharing";
        const string NOTICE20154 = "https://drive.google.com/file/d/11YT1J2xiKGcldU22PZRpVMUQAYTiT-G-/view?usp=sharing";
        const string NOTICE20161 = "https://drive.google.com/file/d/1_7Q4C5kpDYaRPxgF4z3ZiFtKRKhQlYru/view?usp=sharing";
        const string NOTICE20162 = "https://drive.google.com/file/d/1Sp4Gt08nppFlHz5wQxZC-iBza9zaK3Vj/view?usp=sharing";
        const string NOTICE20163 = "https://drive.google.com/file/d/1jHb2V2mZ8odPneKURjJRLnpNt-miFPIc/view?usp=sharing";
        const string NOTICE20164 = "https://drive.google.com/file/d/1T11dvkclNj4U4uXt8sH5OruAHapFDxAa/view?usp=sharing";
        const string NOTICE20171 = "https://drive.google.com/file/d/1kWm8RtGp7dwYih-hiyGyQIcUt93PPFmj/view?usp=sharing";
        const string NOTICESPACE = "https://drive.google.com/file/d/13Vo7UUalugJQzjVPpN7pmdefbOlzWqqA/view?usp=sharing";
        const string NOTICE20172 = "https://drive.google.com/file/d/1Xp7DDzz45cre9ASvdax_QgZErcqhNqsu/view?usp=sharing";
        const string NOTICE20173 = "https://drive.google.com/file/d/1x_-Z3tNFcNICPj717d4M9Smbb7Olas8e/view?usp=sharing";
        const string NOTICE20174 = "https://drive.google.com/file/d/1hf5fPOTxHkXB8EjndRao1Un_ym-pJYXn/view?usp=sharing";
        const string NOTICE201813 = "https://drive.google.com/file/d/1MLV-CX68GBWsefmG9GKoIBLxZcEtwwtn/view?usp=sharing";
        const string NOTICE20184 = "https://drive.google.com/file/d/1c-HunTFNNDJIjI3aF2oQxCojprrC3Pkn/view?usp=sharing";
        const string NOTICE201912 = "https://drive.google.com/file/d/15qq5eVdw4lJCGHGtnWvFGcebfFQSn3Zr/view?usp=sharing";
        const string NOTICE20193 = "https://drive.google.com/file/d/12qxZe8xPaVhoUETsTpgHawnGXFiv5ERL/view?usp=sharing";
        const string NOTICE20194 = "https://drive.google.com/file/d/1N0Q5Ot6DIKqOARi8d5oCWxB64ik8wqo5/view?usp=sharing";
        const string NOTICE202012 = "https://drive.google.com/file/d/19HpB-BTFGz56JbtGDlLz1PcYze0AtyHd/view?usp=sharing";
        #endregion

        public async void OpenWithDefaultBrowser(Uri uri)
        {
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }

        public Adjustment()
        {
            this.InitializeComponent();
        }

        private void BtnDNotice201412_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE201412);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice20143_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20143);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice20144_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20144);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice20151_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20151);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice20152_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20152);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice20153_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20153);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice20154_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20154);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice20161_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20161);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice20162_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20162);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice20163_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20163);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice20164_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20164);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice20171_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20171);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNoticeSpace_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICESPACE);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice20172_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20172);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice20173_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20173);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice20174_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20174);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice201813_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE201813);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice20184_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20184);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice201912_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE201912);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice20193_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20193);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice20194_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20194);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnDNotice202012_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE202012);
            OpenWithDefaultBrowser(uri);
        }
    }
}
