using DailyBudgetMAUIApp.ViewModels;
namespace DailyBudgetMAUIApp.Pages;

public partial class LandingPage : ContentPage
{ 

    public LandingPage(LandingPageViewModel viewModel)
	{
		InitializeComponent();
		this.BindingContext = viewModel;

        
    }

    protected async override void OnAppearing()
    {    
        base.OnAppearing();
        await OpenAnimate();
    }

    private async Task OpenAnimate()
    {
        var animation = new Animation(v => brdImage.Scale = v, 1, 1.2);
        
        animation.Commit(this, "OpenAnimation", 16, 1000, Easing.Linear, async (v, c) =>
        {
            await CloseAnimate();
        });
    }

    private async Task CloseAnimate()
    {
        var CloseAnimation = new Animation(v => brdImage.Scale = v, 1.2, 1);
        CloseAnimation.Commit(this, "CloseAnimation", 16, 1000, Easing.Linear, async (v, c) =>
        {
            await OpenAnimate();
        });
    }
}