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
    public sealed partial class MainPage : MvvmPage
    {
        public MainPage()
        {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(MainVM).TypeHandle);
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.ViewModel = ViewModelFactory.Get<MainVM>(e.Parameter.ToString());
        }

        new MainVM ViewModel { get => (MainVM)base.ViewModel; set => base.ViewModel = value; }

        private void asbSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            ViewModel.Search(args.QueryText);
        }

        private void lv_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.Open((BookDataBrief)e.ClickedItem);
        }
    }
}
