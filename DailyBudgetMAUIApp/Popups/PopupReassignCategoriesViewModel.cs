using CommunityToolkit.Mvvm.ComponentModel;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;



namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopupReassignCategoriesViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IRestDataService _ds;
        private readonly IProductTools _pt;

        public double ScreenWidth { get; }
        public double ScreenHeight { get; }
        public double PopupWidth { get; }
        public double EntryWidth { get; }
        public double ButtonOneWidth { get; }
        public double ButtonTwoWidth { get; }
        public double ButtonThreeWidth { get; }
        public double MaxHeight { get; }

        [ObservableProperty]
        public partial List<Categories> Categories { get; set; } = new List<Categories>();

        [ObservableProperty]
        public partial Dictionary<string, int> ReAssignCategories { get; set; } = new Dictionary<string, int>();

        [ObservableProperty]
        public partial int HeaderCatID { get; set; }

        [ObservableProperty]
        public partial List<string> SelectedReAssignCat { get; set; } = new List<string>();

        [ObservableProperty]
        public partial List<string> DdlCategories { get; set; } = new List<string>();


        public PopupReassignCategoriesViewModel(IRestDataService ds, IProductTools pt)
        {
            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            MaxHeight = ScreenHeight * 0.4;
            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            PopupWidth = ScreenWidth - 30;
            EntryWidth = PopupWidth * 0.6;
            ButtonOneWidth = ((PopupWidth - 60) / 2);
            ButtonTwoWidth = ((PopupWidth - 140) / 3);
            ButtonThreeWidth = ((PopupWidth - 260) / 2);

            _ds = ds;
            _pt = pt;

            ReAssignCategories.Add("Do not reassign", 0);
            
            foreach(string item in ReAssignCategories.Keys)
            {
                DdlCategories.Add(item);
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(nameof(Categories), out var categories) && categories is List<Categories> catList)
            {
                Categories = catList;
            }

            if (query.TryGetValue(nameof(ReAssignCategories), out var reassign) && reassign is Dictionary<string, int> reassignDict)
            {
                ReAssignCategories = reassignDict;
            }

            if (query.TryGetValue(nameof(HeaderCatID), out var headerId) && headerId is int id)
            {
                HeaderCatID = id;
            }
        }

        public async Task DeleteCategory(Categories Cat, int ReAssignID, bool IsReAssign)
        {
            try
            {
                await _ds.DeleteCategory(Cat.CategoryID, IsReAssign, ReAssignID);
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "PopupReassignCategoriesViewModel", "ChangeSelectedProfilePic");
            }
        }
    }
}
