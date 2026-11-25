using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 표준국어대사전.Models
{
    public class RevisionCollection
    {
        public string Year;
        public List<RevisionArticle> articles;
        public BindableVector articlesSource; // XAML ItemsSource 바인딩용

        public RevisionCollection(string year)
        {
            this.Year = year;
            this.articles = new List<RevisionArticle>();
            this.articlesSource = new BindableVector(this.articles);
        }

        public void Add(string title, string href)
        {
            RevisionArticle a = new RevisionArticle(title, href);
            articles.Add(a);
        }
    }

    public class RevisionArticle
    {
        public string Title;
        public string Herf;

        public RevisionArticle(string title, string href)
        {
            this.Title = title;
            this.Herf = href;
        }
    }
}
