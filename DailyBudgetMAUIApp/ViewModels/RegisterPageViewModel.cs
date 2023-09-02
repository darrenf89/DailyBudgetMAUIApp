using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class RegisterPageViewModel : BaseViewModel  
    {
        public RegisterPageViewModel(IRestDataService ds, IProductTools pt)
        {
            Title = "Please Sign Up!";
        }

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _nickName;
        [ObservableProperty]
        private bool _isDPAPermissions;
        [ObservableProperty]
        private bool _isAgreedToTerms;

        [ICommand]
        
        SignUp()
        {

        }
    }
}
