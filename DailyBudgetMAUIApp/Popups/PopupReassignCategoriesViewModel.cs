using CommunityToolkit.Mvvm.ComponentModel;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;



namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopupReassignCategoriesViewModel : BaseViewModel
    {
        private readonly IRestDataService _ds;

        public double ScreenWidth { get; }
        public double ScreenHeight { get; }
        public double PopupWidth { get; }
        public double EntryWidth { get; }
        public double ButtonOneWidth { get; }
        public double ButtonTwoWidth { get; }
        public double ButtonThreeWidth { get; }
        public double MaxHeight { get; }

        [ObservableProperty]
        private List<Categories> categories = new List<Categories>();
        [ObservableProperty]
        private Dictionary<string, int> reAssignCategories = new Dictionary<string, int>();
        [ObservableProperty]
        private int headerCatID;
        [ObservableProperty]
        private List<string> selectedReAssignCat = new List<string>();
        [ObservableProperty]
        private List<string> ddlCategories = new List<string>();

        public PopupReassignCategoriesViewModel(Dictionary<string, int> InputReAssignCategories, int InputHeaderCatID, List<Categories> InputCategories, IRestDataService ds)
        {
            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            MaxHeight = ScreenHeight * 0.4;
            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            PopupWidth = ScreenWidth - 30;
            EntryWidth = PopupWidth * 0.6;
            ButtonOneWidth = ((PopupWidth - 60) / 2);
            ButtonTwoWidth = ((PopupWidth - 140) / 3);
            ButtonThreeWidth = ((PopupWidth - 260) / 2);

            Categories = InputCategories;
            ReAssignCategories = InputReAssignCategories;
            HeaderCatID = InputHeaderCatID;

            _ds = ds;

            ReAssignCategories.Add("Do not reassign", 0);
            
            foreach(string item in ReAssignCategories.Keys)
            {
                DdlCategories.Add(item);
            }
        }

        public async Task DeleteCategory(Categories Cat, int ReAssignID, bool IsReAssign)
        {
            await _ds.DeleteCategory(Cat.CategoryID, IsReAssign, ReAssignID);
        }
    }
}
