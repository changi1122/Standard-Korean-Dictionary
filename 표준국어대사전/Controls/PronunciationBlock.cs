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

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace 표준국어대사전.Controls
{
    [TemplatePart(Name = StackPartName, Type = typeof(StackPanel))]
    public sealed class PronunciationBlock : Control
    {
        private const string StackPartName = "PART_itemstack";
        
        StackPanel _itemstack;

        public PronunciationBlock()
        {
            this.DefaultStyleKey = typeof(PronunciationBlock);
        }


        public List<string> WordItems
        {
            get { return (List<string>)GetValue(WordItemsProperty); }
            set { SetValue(WordItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WordItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WordItemsProperty =
            DependencyProperty.Register("WordItems", typeof(List<string>), typeof(PronunciationBlock), new PropertyMetadata(new List<string>(), OnWordItemsChanged));

        private static void OnWordItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pronunciationBlock = (PronunciationBlock)d;
            pronunciationBlock.UpdateControls();
        }



        public bool IsReaderEnabled
        {
            get { return (bool)GetValue(IsReaderEnabledProperty); }
            set { SetValue(IsReaderEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReaderEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReaderEnabledProperty =
            DependencyProperty.Register("IsReaderEnabled", typeof(bool), typeof(PronunciationBlock), new PropertyMetadata(false, OnIsReaderEnabledChanged));

        private static void OnIsReaderEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pronunciationBlock = (PronunciationBlock)d;
            pronunciationBlock.UpdateControls();
        }



        private void UpdateControls()
        {
            if (_itemstack != null)
            {
                _itemstack.Children.Clear();
                for (int i = 0; i < WordItems.Count; i++)
                {
                    if (i != 0)
                    {
                        TextBlock separator = new TextBlock { Text = "/", Margin = new Thickness(4,0,4,0), FontSize = 16, VerticalAlignment = VerticalAlignment.Center };
                        _itemstack.Children.Add(separator);
                    }
                    WordReader worditem = new WordReader { Text = WordItems[i], IsReaderEnabled = IsReaderEnabled };
                    _itemstack.Children.Add(worditem);
                }
            }
        }

        //템플릿이 적용될 때 실행
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _itemstack = GetTemplateChild(StackPartName) as StackPanel;
            if (_itemstack == null) return;
            UpdateControls();
        }



    }
}
