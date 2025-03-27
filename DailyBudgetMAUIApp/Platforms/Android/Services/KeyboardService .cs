using Android.Content;
using Android.Views.InputMethods;
using DailyBudgetMAUIApp;
using DailyBudgetMAUIApp.DataServices;
using YourAppNamespace.Droid;

[assembly: Dependency(typeof(KeyboardService))]
namespace YourAppNamespace.Droid
{
    public class KeyboardService : IKeyboardService
    {
        public void ShowKeyboard()
        {
            var activity = (MainActivity)Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            var inputMethodManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
            inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
        }

        public void HideKeyboard()
        {
            var activity = (MainActivity)Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            var inputMethodManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
            var currentFocus = activity.CurrentFocus;

            if (currentFocus != null)
            {
                inputMethodManager.HideSoftInputFromWindow(currentFocus.WindowToken, HideSoftInputFlags.None);
            }
        }
    }
}
