using Opportunity.MvvmUniverse.Views;
using StartReader.App.ViewModel;
using StartReader.DataExchange.Model;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace StartReader.App.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    [ViewOf(typeof(BookVM))]
    public sealed partial class BookPage : MvvmPage
    {
        public BookPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.ViewModel = ViewModelFactory.Get<BookVM>(e.Parameter.ToString());
        }

        new BookVM ViewModel { get => (BookVM)base.ViewModel; set => base.ViewModel = value; }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.Open((ChapterDataBrief)e.ClickedItem);
        }
    }
}
