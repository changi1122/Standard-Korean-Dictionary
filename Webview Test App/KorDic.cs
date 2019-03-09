using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webview_Test_App
{
    class KorDic
    {
        public enum ScrollbarVisibility { Collapsed, Visible };
        /* Collapsed = none
         * Visible = scrollbar
         */

        private bool _IsSelectionColorEnabled = false;
        private string _SelBackground = "#f00";
        private string _SelFontColor = "#fff";
        private ScrollbarVisibility _scrollbarVisibility = ScrollbarVisibility.Collapsed;
        private bool _IsReplaceRelatedWordSignEnabled = true;
        private bool _IsPaintEnabled = true;
        private string _FontName = "새굴림";
        private int _FontSize = 18;


        #region attribute
        [Description("It is information about whether to decide the color and background color data of the selected text."), Category("css")]
        public bool IsSelectionColorEnabled
        {
            get { return _IsSelectionColorEnabled; }
            set { _IsSelectionColorEnabled = value; }
        }
        [Description("This string is for the selection background color. (Hex Color Codes)"), Category("css")]
        public string SelBackground
        {
            get { return _SelBackground; }
            set { _SelBackground = value; }
        }
        [Description("This string is for the selection font color. (Hex Color Codes)"), Category("css")]
        public string SelFontColor
        {
            get { return _SelFontColor; }
            set { _SelFontColor = value; }
        }
        [Description("This setting is for the scroll bar status."), Category("css")]
        public ScrollbarVisibility scrollbarVisibility
        {
            get { return _scrollbarVisibility; }
            set { _scrollbarVisibility = value; }
        }
        [Description("This setting is for replacing related word signs."), Category("text")]
        public bool IsReplaceRelatedWordSignEnabled
        {
            get { return _IsReplaceRelatedWordSignEnabled; }
            set { _IsReplaceRelatedWordSignEnabled = value; }
        }
        [Description("This setting is for painting signs"), Category("html")]
        public bool IsPaintEnabled
        {
            get { return _IsPaintEnabled; }
            set { _IsPaintEnabled = value; }
        }
        [Description("This string is for font."), Category("html")]
        public string FontName
        {
            get { return _FontName; }
            set { _FontName = value; }
        }
        [Description("This string is for font."), Category("html")]
        public int FontSize
        {
            get { return _FontSize; }
            set { _FontSize = value; }
        }
        #endregion


        public string GetWordDefinitionInHtmlFormat(string Html)
        {
            string Result = Crop(Html);
            Result = DiscardParts(Result);
            Result = AddFont(Result);
            if(_IsPaintEnabled == true)
            {
                Result = PaintWordClass(Result);
                Result = PaintUnitNum(Result);
                Result = PaintDefinitionNum(Result);
                Result = PaintIdiomProv(Result);
            }
            if (_IsReplaceRelatedWordSignEnabled == true)
                Result = ReplaceRelatedWords(Result);
            Result = ColorNScrollbar(Result);

            return Result;
        }

        public string CanSoundPlay(string Html)
        {
            if(Html.IndexOf("DicSoundPlaySeq") != -1)
            {
                return Html.Substring(Html.IndexOf("DicSoundPlaySeq"), Html.IndexOf(')', Html.IndexOf("DicSoundPlaySeq")) - Html.IndexOf("DicSoundPlaySeq") + 1);
            }
            return "false";
        }


        private string Crop(string Work)
        {
            if (Work == "") //_Html이 비어있을 경우
                return "Error 01";
            if (Work.IndexOf("<div class=\"list\">") == -1 || Work.IndexOf("<!-- comment.jsp -->") == -1) //Substring이 불가능할 때
                return "Error 02";
            Work = Work.Substring(Work.IndexOf("<div class=\"list\">"), Work.IndexOf("<!-- comment.jsp -->") - Work.IndexOf("<div class=\"list\">"));
            return Work;
        }

        private string DiscardParts(string Work)
        {
            /* Delete
             *  <a >
             *  <strong>
             *  <font >
             *  <imgsrc >
             *  </font>
             *  <div align="center" style="width: 90px; >
             * 
             * Replace
             *  (( )) -> +Space
             *  &nbsp; -> Space
             *  &lt; -> <
             *  &gt; -> >
             */
            while (true)
            {
                if (Work.IndexOf("<a") == -1)
                    break;
                Work = Work.Remove(Work.IndexOf("<a"), Work.IndexOf('>', Work.IndexOf("<a")) - Work.IndexOf("<a") + 1);
            }
            while (true)
            {
                if (Work.IndexOf("<font") == -1)
                    break;
                Work = Work.Remove(Work.IndexOf("<font"), Work.IndexOf('>', Work.IndexOf("<font")) - Work.IndexOf("<font") + 1);
            }
            while (true)
            {
                if (Work.IndexOf("</font") == -1)
                    break;
                Work = Work.Remove(Work.IndexOf("</font"), Work.IndexOf('>', Work.IndexOf("</font")) - Work.IndexOf("</font") + 1);
            }
            while (true)
            {
                if (Work.IndexOf("<img") == -1)
                    break;
                Work = Work.Remove(Work.IndexOf("<img"), Work.IndexOf('>', Work.IndexOf("<img")) - Work.IndexOf("<img") + 1);
            }
            while (true)
            {
                if (Work.IndexOf("<imgsrc") == -1)
                    break;
                Work = Work.Remove(Work.IndexOf("<imgsrc"), Work.IndexOf('>', Work.IndexOf("<imgsrc")) - Work.IndexOf("<imgsrc") + 1);
            }
            while (true)
            {
                if (Work.IndexOf("<div align=\"center\" style=\"width: 90px; ") == -1)
                    break;
                Work = Work.Remove(Work.IndexOf("<div align=\"center\" style=\"width: 90px; "), Work.IndexOf("</div>", Work.IndexOf("<div align=\"center\" style=\"width: 90px; ")) - Work.IndexOf("<div align=\"center\" style=\"width: 90px; ") + 6);
            }
            while (true)
            {
                if (Work.IndexOf("&nbsp;") == -1)
                    break;
                Work = Work.Replace("&nbsp;", " ");
            }
            while (true)
            {
                if (Work.IndexOf("<strong>") == -1)
                    break;
                Work = Work.Replace("<strong>", "");
            }
            while (true)
            {
                if (Work.IndexOf("&lt;") == -1)
                    break;
                Work = Work.Replace("&lt;", "<");
            }
            while (true)
            {
                if (Work.IndexOf("&gt;") == -1)
                    break;
                Work = Work.Replace("&gt;", ">");
            }
            return Work;
        }

        private string AddFont(string Work)
        {
            Work = "<font face=\""+ _FontName + "\" style=\"font-size:" + _FontSize + "px\">" + Work + "</font>";

            return Work;
        }

        private string PaintWordClass(string Work)
        {
            Work = Work.Replace("<span class=\"NumRG\">", "<span style=\"color: #549606;\">");
            return Work;
        }

        private string PaintUnitNum(string Work)
        {
            Work = Work.Replace("<span class=\"NumRG2\">", "<span style=\"color: #336699;\">");
            return Work;
        }

        private string PaintDefinitionNum(string Work)
        {
            Work = Work.Replace("<span class=\"NumNO\">", "<span style=\"color: #cb4a00;\">");
            return Work;
        }

        private string PaintIdiomProv(string Work)
        {
            Work = Work.Replace("<li class=\"idiom\">", "<li class=\"idiom\"><span style=\"color: #336699;\">[관용어] </span>");
            Work = Work.Replace("<li class=\"prov\">", "<li class=\"prov\"><span style=\"color: #f98217;\">[속담] </span>");
            return Work;
        }

        private string ReplaceRelatedWords(string Work)
        {
            Work = Work.Replace("「본」", "「본말」");
            Work = Work.Replace("「준」", "「준말」");
            Work = Work.Replace("「비」", "「비슷한말」");
            Work = Work.Replace("「반」", "「반대말」");
            Work = Work.Replace("「높」", "「높임말」");
            Work = Work.Replace("「낮」", "「낮춤말」");
            return Work;
        }

        private string ColorNScrollbar(string Work)
        {
            string strStyle = "<style>";
            if (_IsSelectionColorEnabled == true)
                strStyle += "::selection { background: " + _SelBackground + "; color: " + _SelFontColor + "; }";
            string strScrollbar = "";
            if (_scrollbarVisibility == ScrollbarVisibility.Collapsed)
                strScrollbar = "none";
            else if (_scrollbarVisibility == ScrollbarVisibility.Visible)
                strScrollbar = "scrollbar";
            strStyle += "body { -ms-overflow-style: " + strScrollbar + " }</style>";
            Work = strStyle + Work;
            return Work;
        }
    }
}

