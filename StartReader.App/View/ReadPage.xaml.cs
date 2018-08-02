using Opportunity.MvvmUniverse.Views;
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
    public sealed partial class ReadPage : MvvmPage
    {
        public ReadPage()
        {
            this.InitializeComponent();
        }

        private string getTitle(string cName, string vName, string bName)
        {
            if (vName.IsNullOrWhiteSpace())
                return $"{cName} - {bName}";
            else
                return $"{cName} - {vName} - {bName}";
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.ViewModel = ViewModelFactory.Get<ReadVM>(e.Parameter.ToString());
        }

        new ReadVM ViewModel { get => (ReadVM)base.ViewModel; set => base.ViewModel = value; }

        private async void rtbContent_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var s = (RichTextBlock)sender;
            var oldValue = s.Tag as string;
            var newValue = args.NewValue as string;
            var position = ViewModel.Position;
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
            }
            await Dispatcher.YieldIdle();
            var svContent = s.Ancestors<ScrollViewer>().First();
            var offset = position * svContent.ScrollableHeight;
            svContent.ChangeView(null, offset, null, true);
        }

        private void svContent_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (e.IsIntermediate)
                return;
            var s = (ScrollViewer)sender;
            ViewModel.Position = s.VerticalOffset / s.ScrollableHeight;
        }
    }
}
