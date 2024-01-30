using Android.Content;
using Android.Content.Res;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using System.ComponentModel;


namespace DailyBudgetMAUIApp.Platforms.Android.Renderers
{    
    public class MyShellRenderer : ShellRenderer
    {
        public Resources resources { get; set; }

        public MyShellRenderer(Context context) : base(context)
        {
            resources = context.Resources;
        }


        protected override void OnElementSet(Shell shell)
        {
            base.OnElementSet(shell);

            int resourceId = resources.GetIdentifier("navigation_bar_height", "dimen", "android");
            if (resourceId > 0)
            {
                App.NavBarHeight = resources.GetDimensionPixelSize(resourceId) / DeviceDisplay.Current.MainDisplayInfo.Density;
            }

            int resourceId = getResources().getIdentifier("status_bar_height", "dimen", "android");
            if (resourceId > 0)
            {
                App.StatusBarHeight = resources.GetDimensionPixelSize(resourceId) / DeviceDisplay.Current.MainDisplayInfo.Density;
            }

        }

        protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
        {
            return new MyBottomNavViewAppearanceTracker(this, shellItem);
        }
    }

    class MyBottomNavViewAppearanceTracker : ShellBottomNavViewAppearanceTracker
    {
        public MyBottomNavViewAppearanceTracker(IShellContext shellContext, ShellItem shellItem) : base(shellContext, shellItem)
        {

        }

        public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
        {            
            base.SetAppearance(bottomView, appearance);
            //App.TabBarHeight = bottomView.Height;
        }
    }

}

