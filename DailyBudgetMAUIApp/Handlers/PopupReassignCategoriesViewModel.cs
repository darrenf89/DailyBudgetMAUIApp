using CommunityToolkit.Maui.Views;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Newtonsoft.Json;
using System.Diagnostics;
using Firebase.HeartBeatInfo;


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

        [ObservableProperty]
        public List<Categories> _categories;
        [ObservableProperty]
        public Dictionary<string, int> _reAssignCategories;
        [ObservableProperty]
        public int _headerCatID;
        [ObservableProperty]
        public List<string> _selectedReAssignCat = new List<string>();
        [ObservableProperty]
        public List<string> _ddlCategories = new List<string>();

        public PopupReassignCategoriesViewModel(Dictionary<string, int> ReAssignCategories, int HeaderCatID, List<Categories> Categories, IRestDataService ds)
        {
            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            PopupWidth = ScreenWidth - 30;
            EntryWidth = PopupWidth * 0.6;
            ButtonOneWidth = ((PopupWidth - 60) / 2);
            ButtonTwoWidth = ((PopupWidth - 140) / 3);
            ButtonThreeWidth = ((PopupWidth - 260) / 2);

            this.Categories = Categories;
            this.ReAssignCategories = ReAssignCategories;
            this.HeaderCatID = HeaderCatID;

            _ds = ds;

            ReAssignCategories.Add("Do not reassign", 0);
            
            foreach(string item in ReAssignCategories.Keys)
            {
                DdlCategories.Add(item);
            }
        }
    }
}
