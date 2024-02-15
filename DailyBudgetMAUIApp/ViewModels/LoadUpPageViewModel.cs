using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using DailySpendWebApp.Models;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Diagnostics;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class LoadUpPageViewModel : BaseViewModel
    {

        public LoadUpPageViewModel()
        {

        }

        [ICommand]
        async void Logon()
        {
            await Shell.Current.GoToAsync(nameof(LogonPage));
        }

        [ICommand]
        async void Register()
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }  
    }
}
