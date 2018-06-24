using Opportunity.MvvmUniverse.Views;

namespace StartReader.App.ViewModel
{
    class ReadVM : ViewModelBase
    {
        static ReadVM()
        {
            ViewModelFactory.Register(s => new ReadVM());
        }
    }
}
