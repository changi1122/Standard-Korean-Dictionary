using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.Media;
using Windows.Media.SpeechSynthesis;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace 표준국어대사전.Controls
{
    [TemplatePart(Name = WordPartName, Type = typeof(TextBlock))]
    [TemplatePart(Name = BtnPartName, Type = typeof(HyperlinkButton))]
    public sealed class WordReader : Control
    {
        private const string WordPartName = "PART_word";
        private const string BtnPartName = "PART_btn";

        TextBlock _word;
        HyperlinkButton _btn;

        public WordReader()
        {
            this.DefaultStyleKey = typeof(WordReader);
        }

        //Text : 표시될 텍스트
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(WordReader), new PropertyMetadata("", OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wordReader = (WordReader)d;
            wordReader.UpdateControls();
        }

        //IsReaderEnabled : 텍스트 읽기 버튼을 표시할 지 여부
        public bool IsReaderEnabled
        {
            get { return (bool)GetValue(IsReaderEnabledProperty); }
            set { SetValue(IsReaderEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReaderEnabledProperty =
            DependencyProperty.Register("IsReaderEnabled", typeof(bool), typeof(WordReader), new PropertyMetadata(false, OnIsReaderEnabledChanged));

        private static void OnIsReaderEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wordReader = (WordReader)d;
            wordReader.UpdateControls();
        }


        private void UpdateControls()
        {
            if (_word != null)
                _word.Text = Text;
            if (_word != null)
                _btn.Visibility = (IsReaderEnabled == false) ? Visibility.Collapsed : Visibility.Visible;
        }

        //템플릿이 적용될 때 실행
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _word = GetTemplateChild(WordPartName) as TextBlock;
            if (_word == null) return;
            _btn = GetTemplateChild(BtnPartName) as HyperlinkButton;
            if (_btn == null) return;
            _btn.Click += _btn_Click;
            UpdateControls();
        }

        private async void _btn_Click(object sender, RoutedEventArgs e)
        {
            MediaElement mediaplayer = new MediaElement();
            using (var speech = new SpeechSynthesizer())
            {
                speech.Voice = SpeechSynthesizer.AllVoices.First(gender => gender.Gender == VoiceGender.Female);
                SpeechSynthesisStream stream = await speech.SynthesizeTextToStreamAsync(_word.Text);
                mediaplayer.SetSource(stream, stream.ContentType);
                mediaplayer.Play();
            }
        }
    }
}
