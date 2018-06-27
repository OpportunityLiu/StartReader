﻿using Opportunity.MvvmUniverse.Views;
using StartReader.App.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace StartReader.App.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    [ViewOf(typeof(ReadVM))]
    public sealed partial class ReadPage : MvvmPage
    {
        public ReadPage()
        {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(ReadVM).TypeHandle);
            this.InitializeComponent();
        }

        private string getTitle(string cName, string bName) => cName + " - " + bName;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.ViewModel = ViewModelFactory.Get<ReadVM>(e.Parameter.ToString());
        }

        new ReadVM ViewModel { get => (ReadVM)base.ViewModel; set => base.ViewModel = value; }

        private bool autoNavEnabled = false;

        protected override void OnViewModelChanged(ViewModelBase oldValue, ViewModelBase newValue)
        {
            this.autoNavEnabled = false;
        }

        private async void rtbContent_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            this.autoNavEnabled = false;
            var s = (RichTextBlock)sender;
            var oldValue = s.Tag as string;
            var newValue = args.NewValue as string;
            if (newValue != oldValue)
            {
                s.Tag = newValue;
                s.Blocks.Clear();
                if (newValue.IsNullOrEmpty())
                {
                    s.Blocks.Add(new Paragraph { Inlines = { new Run() }, Margin = new Thickness(0, 0, 0, 12) });
                }
                else
                {
                    var content = args.NewValue.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    foreach (var item in content)
                    {
                        s.Blocks.Add(new Paragraph { Inlines = { new Run { Text = item } }, Margin = new Thickness(0, 0, 0, 12) });
                    }
                }

                this.svContent.UpdateLayout();
            }
            await Dispatcher.YieldIdle();
            var zero = this.spPrevious.ActualHeight;
            var one = this.svContent.ScrollableHeight - zero - this.spNext.ActualHeight;
            var offset = ViewModel.Position * one + zero;
            this.svContent.ChangeView(null, offset, null, true);
            await Task.Delay(250);
            this.autoNavEnabled = true;
        }

        private void svContent_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (e.IsIntermediate || !this.autoNavEnabled)
                return;
            var zero = this.spPrevious.ActualHeight;
            var one = this.svContent.ScrollableHeight - zero - this.spNext.ActualHeight;
            this.ViewModel.Position = (this.svContent.VerticalOffset - zero) / one;
            if (this.svContent.VerticalOffset < 1)
                ViewModel.GoToChapter.Execute(ViewModel.Previous?.Index ?? -1);
            else if (this.svContent.VerticalOffset > this.svContent.ScrollableHeight - 1)
                ViewModel.GoToChapter.Execute(ViewModel.Next?.Index ?? -1);
        }

        private void page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!this.autoNavEnabled)
                return;
            var zero = this.spPrevious.ActualHeight;
            var one = this.svContent.ScrollableHeight - zero - this.spNext.ActualHeight;
            var offset = ViewModel.Position * one + zero;
            this.svContent.ChangeView(null, offset, null, true);
        }
    }
}
