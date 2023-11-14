using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace DailyBudgetMAUIApp.Handlers
{
    public class CustomRefreshViewHandler : RefreshViewHandler
    {
        protected override void ConnectHandler(MauiSwipeRefreshLayout platformView)
        {

            int deviceOffset = platformView?.ProgressViewEndOffset ?? 0;

            base.ConnectHandler(platformView);

            platformView?.SetProgressViewEndTarget(true, deviceOffset);
        }
    }
}