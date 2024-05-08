using Android.Graphics.Drawables;
using DailyBudgetMAUIApp.Handlers;
using Maui.FreakyEffects.Platforms.Android;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;


namespace DailyBudgetMAUIApp.Platforms.Android.Mappers
{
    public static class BorderEntryMapper
    {
        public static void Map(IElementHandler handler, IElement view)
        {
            if (view is BorderEntry)
            {
                var casted = (EntryHandler)handler;
                var viewData = (BorderEntry)view;

                var gd = new GradientDrawable();

                gd.SetCornerRadius((int)handler.MauiContext?.Context.ToPixels(viewData.CornerRadius));

                gd.SetStroke((int)handler.MauiContext?.Context.ToPixels(viewData.BorderThickness), viewData.BorderColor.ToAndroid());

                if (viewData.BackgroundColor != null)
                {
                    gd.SetColor(viewData.BackgroundColor.ToAndroid());
                }


                casted.PlatformView?.SetBackground(gd);

                int paddingPixel = (int)handler.MauiContext?.Context.ToPixels(10); // Example
                casted.PlatformView?.SetPadding(paddingPixel, paddingPixel, paddingPixel, paddingPixel);
            }
        }

    }
}


