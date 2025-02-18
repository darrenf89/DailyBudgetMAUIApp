using System.Windows.Input;
using IeuanWalker.Maui.Switch.Events;
using IeuanWalker.Maui.Switch.Helpers;

namespace DailyBudgetMAUIApp.Handlers;

public partial class CustomSwitch : ContentView
{
    public CustomSwitch()
    {
        InitializeComponent();
    }

    public event EventHandler<ToggledEventArgs>? Toggled = null;

    public static readonly BindableProperty IsToggledProperty = BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(CustomSwitch), false, BindingMode.TwoWay);

    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }

    public static readonly BindableProperty ToggledCommandProperty = BindableProperty.Create(nameof(ToggledCommand), typeof(ICommand), typeof(CustomSwitch));

    public ICommand ToggledCommand
    {
        get => (ICommand)GetValue(ToggledCommandProperty);
        set => SetValue(ToggledCommandProperty, value);
    }

    void CustomSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        Toggled?.Invoke(sender, e);
    }

    static void CustomSwitch_SwitchPanUpdate(IeuanWalker.Maui.Switch.CustomSwitch customSwitch, SwitchPanUpdatedEventArgs e)
    {
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("PrimaryLight", out var PrimaryLight);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);

        //Switch Color Animation
        Color fromSwitchColor = e.IsToggled ? (Color)Primary : (Color)Gray400;
        Color toSwitchColor = e.IsToggled ? (Color)Gray400 : (Color)Primary;

        //BackGroundColor Animation
        Color fromColor = e.IsToggled ? (Color)Tertiary : (Color)PrimaryLight;
        Color toColor = e.IsToggled ? (Color)PrimaryLight : (Color)Tertiary;

        double t = e.Percentage * 0.01;

        customSwitch.KnobBackgroundColor = ColorAnimationUtil.ColorAnimation(fromSwitchColor, toSwitchColor, t);
        customSwitch.BackgroundColor = ColorAnimationUtil.ColorAnimation(fromColor, toColor, t);

    }

}