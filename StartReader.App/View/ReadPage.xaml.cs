using Opportunity.MvvmUniverse.Views;
using StartReader.App.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.ViewModel = ViewModelFactory.Get<ReadVM>(e.Parameter.ToString());
        }

        new ReadVM ViewModel { get => (ReadVM)base.ViewModel; set => base.ViewModel = value; }

        private bool contentLoaded = false;

        protected override void OnViewModelChanged(ViewModelBase oldValue, ViewModelBase newValue)
        {
            this.contentLoaded = false;
            if (newValue is ReadVM nv)
            {
                this.Title = nv.Chapter.Title + " - " + nv.Chapter.Book.Title;
            }
        }

        private async void rtbContent_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var s = (RichTextBlock)sender;
            s.Blocks.Clear();
            if (args.NewValue is null)
                return;
            var content = args.NewValue.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (var item in content)
            {
                s.Blocks.Add(new Paragraph { Inlines = { new Run { Text = item } }, Margin = new Thickness(0, 0, 0, 12) });
            }
            await Dispatcher.YieldIdle();
            var zero = this.spPrevious.ActualHeight;
            var one = this.svContent.ScrollableHeight - zero - this.spNext.ActualHeight;
            var offset = ViewModel.Position * one + zero;
            this.svContent.ChangeView(null, offset, null, true);
            this.contentLoaded = true;
        }

        private async void svContent_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (e.IsIntermediate || !this.contentLoaded)
                return;
            await Dispatcher.YieldIdle();
            var zero = this.spPrevious.ActualHeight;
            var one = this.svContent.ScrollableHeight - zero - this.spNext.ActualHeight;
            var position = (this.svContent.VerticalOffset - zero) / one;
            ViewModel.Position = position;
            if (position < 0)
                ViewModel.GoToChapter.Execute(ViewModel.Previous);
            else if (position > 1)
                ViewModel.GoToChapter.Execute(ViewModel.Next);
        }

        private async void page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            await Dispatcher.YieldIdle();
            var zero = this.spPrevious.ActualHeight;
            var one = this.svContent.ScrollableHeight - zero - this.spNext.ActualHeight;
            var offset = ViewModel.Position * one + zero;
            this.svContent.ChangeView(null, offset, null, true);
        }
    }
}
