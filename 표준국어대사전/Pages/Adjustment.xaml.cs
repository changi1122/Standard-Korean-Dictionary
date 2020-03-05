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
        const string NOTICE201412 = "https://drive.google.com/open?id=1vZ1EMUCPT8NVORkvKeQCUBdOLC9MIhQp";
        const string NOTICE20143 = "https://drive.google.com/open?id=1Ri-LxfBr3Kl1rgpECpJDIOvdPnWteCmg";
        const string NOTICE20144 = "https://drive.google.com/open?id=1L7Sjr_oOT0sM10-EOF78kCIHhv7lDvM_";
        const string NOTICE20151 = "https://drive.google.com/open?id=16zJiBCNh-lyGvYqQ56-qjw8A0VbjstEr";
        const string NOTICE20152 = "https://drive.google.com/open?id=1605uaY_DxUm3fMMflk0to5_PjlN2v3Wy";
        const string NOTICE20153 = "https://drive.google.com/open?id=1PJ0oRaZM3I5MguWlv-UmXUrXpGZHT3g7";
        const string NOTICE20154 = "https://drive.google.com/open?id=11YT1J2xiKGcldU22PZRpVMUQAYTiT-G-";
        const string NOTICE20161 = "https://drive.google.com/open?id=1_7Q4C5kpDYaRPxgF4z3ZiFtKRKhQlYru";
        const string NOTICE20162 = "https://drive.google.com/open?id=1Sp4Gt08nppFlHz5wQxZC-iBza9zaK3Vj";
        const string NOTICE20163 = "https://drive.google.com/open?id=1jHb2V2mZ8odPneKURjJRLnpNt-miFPIc";
        const string NOTICE20164 = "https://drive.google.com/open?id=1T11dvkclNj4U4uXt8sH5OruAHapFDxAa";
        const string NOTICE20171 = "https://drive.google.com/open?id=1kWm8RtGp7dwYih-hiyGyQIcUt93PPFmj";
        const string NOTICESPACE = "https://drive.google.com/open?id=13Vo7UUalugJQzjVPpN7pmdefbOlzWqqA";
        const string NOTICE20172 = "https://drive.google.com/open?id=1Xp7DDzz45cre9ASvdax_QgZErcqhNqsu";
        const string NOTICE20173 = "https://drive.google.com/open?id=1x_-Z3tNFcNICPj717d4M9Smbb7Olas8e";
        const string NOTICE20174 = "https://drive.google.com/open?id=1hf5fPOTxHkXB8EjndRao1Un_ym-pJYXn";
        const string NOTICE201813 = "https://drive.google.com/open?id=1MLV-CX68GBWsefmG9GKoIBLxZcEtwwtn";
        const string NOTICE20184 = "https://drive.google.com/open?id=1c-HunTFNNDJIjI3aF2oQxCojprrC3Pkn";
        const string NOTICE201912 = "https://drive.google.com/open?id=15qq5eVdw4lJCGHGtnWvFGcebfFQSn3Zr";
        const string NOTICE20193 = "https://drive.google.com/open?id=12qxZe8xPaVhoUETsTpgHawnGXFiv5ERL";
        const string NOTICE20194 = "https://drive.google.com/open?id=1N0Q5Ot6DIKqOARi8d5oCWxB64ik8wqo5";
        #endregion

        public async void OpenWithEdge(Uri uri)
        {
            var options = new Windows.System.LauncherOptions();
            options.TargetApplicationPackageFamilyName = "Microsoft.MicrosoftEdge_8wekyb3d8bbwe";

            await Windows.System.Launcher.LaunchUriAsync(uri, options);
        }

        public async void OpenWithDefaultBrowser(Uri uri)
        {
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }

        public Adjustment()
        {
            this.InitializeComponent();
        }

        private void BtnENotice201412_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE201412);
            OpenWithEdge(uri);
        }

        private void BtnDNotice201412_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE201412);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice20143_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20143);
            OpenWithEdge(uri);
        }

        private void BtnDNotice20143_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20143);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice20144_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20144);
            OpenWithEdge(uri);
        }

        private void BtnDNotice20144_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20144);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice20151_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20151);
            OpenWithEdge(uri);
        }

        private void BtnDNotice20151_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20151);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice20152_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20152);
            OpenWithEdge(uri);
        }

        private void BtnDNotice20152_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20152);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice20153_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20153);
            OpenWithEdge(uri);
        }

        private void BtnDNotice20153_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20153);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice20154_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20154);
            OpenWithEdge(uri);
        }

        private void BtnDNotice20154_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20154);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice20161_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20161);
            OpenWithEdge(uri);
        }

        private void BtnDNotice20161_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20161);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice20162_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20162);
            OpenWithEdge(uri);
        }

        private void BtnDNotice20162_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20162);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice20163_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20163);
            OpenWithEdge(uri);
        }

        private void BtnDNotice20163_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20163);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice20164_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20164);
            OpenWithEdge(uri);
        }

        private void BtnDNotice20164_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20164);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice20171_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20171);
            OpenWithEdge(uri);
        }

        private void BtnDNotice20171_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20171);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENoticeSpace_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICESPACE);
            OpenWithEdge(uri);
        }

        private void BtnDNoticeSpace_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICESPACE);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice20172_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20172);
            OpenWithEdge(uri);
        }

        private void BtnDNotice20172_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20172);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice20173_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20173);
            OpenWithEdge(uri);
        }

        private void BtnDNotice20173_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20173);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice20174_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20174);
            OpenWithEdge(uri);
        }

        private void BtnDNotice20174_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20174);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice201813_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE201813);
            OpenWithEdge(uri);
        }

        private void BtnDNotice201813_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE201813);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice20184_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20184);
            OpenWithEdge(uri);
        }

        private void BtnDNotice20184_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20184);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice201912_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE201912);
            OpenWithEdge(uri);
        }

        private void BtnDNotice201912_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE201912);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice20193_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20193);
            OpenWithEdge(uri);
        }

        private void BtnDNotice20193_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20193);
            OpenWithDefaultBrowser(uri);
        }

        private void BtnENotice20194_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20194);
            OpenWithEdge(uri);
        }

        private void BtnDNotice20194_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(NOTICE20194);
            OpenWithDefaultBrowser(uri);
        }
    }
}
