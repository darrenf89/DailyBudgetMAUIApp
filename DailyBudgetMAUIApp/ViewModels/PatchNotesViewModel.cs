using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using Plugin.MauiMTAdmob;
using DailyBudgetMAUIApp.Handlers;


namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PatchNotesViewModel : BaseViewModel
    {
        private readonly IRestDataService _ds;
        private readonly IProductTools _pt;

        [ObservableProperty]
        private List<PatchNote> patchNotes = new List<PatchNote>();

        public PatchNotesViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

        }

        public void UpdatePatchNotes()
        {
            PatchNotes = new List<PatchNote>
            {
                new PatchNote { Title = "Update 1.34", Date = new DateTime(2024, 10, 1), Description = "Initial release with basic features." , Changes = new List<string> { "Change 1", "Change 2", "Change 3" }},
                new PatchNote { Title = "Update 1.33", Date = new DateTime(2024, 10, 15), Description = "Fixed bugs and improved performance." , Changes = new List<string> { "Change 1", "Change 2", "Change 3" }},
                new PatchNote { Title = "Update 1.32", Date = new DateTime(2024, 11, 1), Description = "Added new features and enhancements." , Changes = new List<string> { "Change 1", "Change 2", "Change 3" }},
                new PatchNote { Title = "Update 1.31", Date = new DateTime(2024, 11, 1), Description = "Added new features and enhancements." , Changes = new List<string> { "Change 1", "Change 2", "Change 3" }},
                new PatchNote { Title = "Update 1.30", Date = new DateTime(2024, 11, 1), Description = "Added new features and enhancements." , Changes = new List<string> { "Change 1", "Change 2", "Change 3" }}
            };
        }

        [RelayCommand]
        async Task GoToMainPage(object obj)
        {
            try
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.MainPage.ShowPopup(PopUp);
                    await Task.Delay(1);
                }

                await Shell.Current.GoToAsync($"///{nameof(MainPage)}");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "PatchNotesViewModel", "GoToMainPage");
            }

        }
    }

}
