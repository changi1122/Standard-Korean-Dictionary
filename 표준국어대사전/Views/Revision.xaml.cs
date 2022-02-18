using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Popups;
using System.Collections.ObjectModel;
using HtmlAgilityPack;
using 표준국어대사전.Models;

namespace 표준국어대사전.Views
{
    public sealed partial class Revision : Page
    {
        ObservableCollection<RevisionCollection> collections;

        public Revision()
        {
            collections = new ObservableCollection<RevisionCollection>();
            this.InitializeComponent();

            try
            {
                GetRevisionData();
            }
            catch
            {
                RevisionProgressBar.Visibility = Visibility.Collapsed;
                NetNoticeGrid.Visibility = Visibility.Visible;
            }
        }

        private void GetRevisionData()
        {
            RevisionProgressBar.Visibility = Visibility.Visible;

            const string URL = @"https://costudio1122.blogspot.com/p/standard-korean-dictionary-revisions.html";
            
            HtmlWeb client = new HtmlWeb();
            HtmlDocument document = client.Load(URL);

            HtmlNodeCollection revisions = document.DocumentNode.SelectNodes("//revision");
            HtmlNode versionTarget = null;

            foreach (HtmlNode revisionNode in revisions)
            {
                if (revisionNode.Attributes["version"].Value == "1")
                {
                    versionTarget = revisionNode;
                    break;
                }
            }

            if (versionTarget == null)
            {
                throw new Exception("올바른 versionTarget이 없습니다.");
            }

            HtmlNodeCollection years = versionTarget.SelectNodes("./year");
            foreach (HtmlNode yearNode in years)
            {
                RevisionCollection year = new RevisionCollection(yearNode.Attributes["year"].Value);

                HtmlNodeCollection articles = yearNode.SelectNodes("./a");
                foreach (HtmlNode articleNode in articles)
                {
                    year.Add(articleNode.InnerText, articleNode.Attributes["href"].Value);
                }

                collections.Add(year);
            }

            RevisionProgressBar.Visibility = Visibility.Collapsed;
            RevisionPivot.Visibility = Visibility.Visible;
        }

        private async void OpenWithDefaultBrowser(Uri uri)
        {
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }

        private void Href_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.Tag != null)
                OpenWithDefaultBrowser(new Uri(button.Tag.ToString()));
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            NetNoticeGrid.Visibility = Visibility.Collapsed;

            try
            {
                GetRevisionData();
            }
            catch
            {
                RevisionProgressBar.Visibility = Visibility.Collapsed;
                NetNoticeGrid.Visibility = Visibility.Visible;
            }
        }
    }
}
