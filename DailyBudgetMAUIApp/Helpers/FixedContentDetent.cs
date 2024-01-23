using The49.Maui.BottomSheet;

public class FixedContentDetent : ContentDetent
{
    // HACK: https://github.com/the49ltd/The49.Maui.BottomSheet/issues/93
    public override double GetHeight(BottomSheet page, double maxSheetHeight)
    {
        if (page.Content is null)
        {
            return maxSheetHeight;
        }

        if (page.Content is ScrollView sv)
        {
            var s = sv.Content.Measure(page.Window.Width - page.Padding.HorizontalThickness, maxSheetHeight);
        }
        // HACK: from 'page.Window.Width' to 'page.Window?.Width ?? page.Width' 
        var r = page.Content.Measure(page.Window?.Width ?? page.Width - page.Padding.HorizontalThickness, maxSheetHeight);

        return Math.Min(maxSheetHeight, r.Request.Height + page.Padding.VerticalThickness);
    }
}