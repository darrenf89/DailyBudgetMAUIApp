
using Maui.FixesAndWorkarounds;
using Microsoft.Maui.Controls;

namespace DailyBudgetMAUIApp.Handlers;

public partial class ReadMoreAccordion : ContentView
{
	public ReadMoreAccordion()
	{
        TextTitle = "Read More";
        IsBodyVisible = false;

        InitializeComponent();
        Application.Current.Resources.TryGetValue("Info", out var Info);
        TitleImage.Source = new FontImageSource
        {
            FontFamily = "MaterialDesignIcons",
            Glyph = "\ue313",
            Size = 15,
            Color = (Color)Info
        };

    }

    public static readonly BindableProperty IsBodyVisibleProperty =
        BindableProperty.Create(nameof(IsBodyVisible), typeof(bool), typeof(ReadMoreAccordion), propertyChanged: OnIsBodyVisibleChanged, defaultBindingMode: BindingMode.TwoWay);

    public bool IsBodyVisible
    {
        get => (bool)GetValue(IsBodyVisibleProperty);
        set => SetValue(IsBodyVisibleProperty, value);
    }

    static void OnIsBodyVisibleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        Application.Current.Resources.TryGetValue("Info", out var Info);

        var control = (ReadMoreAccordion)bindable;
        if ((bool)newValue)
        {
            control.TitleImage.Source = new FontImageSource
            {
                FontFamily = "MaterialDesignIcons",
                Glyph = "\ue316",
                Size = 15,
                Color = (Color)Info
            };
        }
        else
        {
            control.TitleImage.Source = new FontImageSource
            {
                FontFamily = "MaterialDesignIcons",
                Glyph = "\ue313",
                Size = 15,
                Color = (Color)Info
            };
        }
        control.InvalidateMeasure();
    }

    public static readonly BindableProperty TextBodyProperty =
        BindableProperty.Create(nameof(TextBody), typeof(string), typeof(ReadMoreAccordion), propertyChanged: OnTextBodyChanged, defaultBindingMode: BindingMode.TwoWay);

    public string TextBody
    {
        get => (string)GetValue(TextBodyProperty);
        set => SetValue(TextBodyProperty, value);
    }

    static void OnTextBodyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (ReadMoreAccordion)bindable;
        control.InvalidateMeasure();
    }

    public static readonly BindableProperty TextTitleProperty =
        BindableProperty.Create(nameof(TextTitle), typeof(string), typeof(ReadMoreAccordion), propertyChanged: OnTextTitleChanged, defaultBindingMode: BindingMode.TwoWay);

    public string TextTitle
    {
        get => (string)GetValue(TextTitleProperty);
        set => SetValue(TextTitleProperty, value);
    }

    static void OnTextTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (ReadMoreAccordion)bindable;
        control.InvalidateMeasure();
    }

    public new static BindableProperty WidthProperty =
    BindableProperty.Create(nameof(Width), typeof(double), typeof(ReadMoreAccordion), propertyChanged: OnWidthChanged);

    public new double Width
    {
        get => (double)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    static void OnWidthChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (ReadMoreAccordion)bindable;
        control.CustomReadMoreAccordion.WidthRequest = (double)newValue;
        control.InvalidateMeasure();
    }

    public static BindableProperty TitleTextColorProperty =
        BindableProperty.Create(nameof(TitleTextColor), typeof(Color), typeof(ReadMoreAccordion), propertyChanged: OnTitleTextColorChanged);

    public Color TitleTextColor
    {
        get => (Color)GetValue(TitleTextColorProperty);
        set => SetValue(TitleTextColorProperty, value);
    }

    static void OnTitleTextColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (ReadMoreAccordion)bindable;
        control.txtTitle.TextColor = (Color)newValue;
        control.InvalidateMeasure();
    }

    public new static BindableProperty MarginProperty =
    BindableProperty.Create(nameof(Margin), typeof(Thickness), typeof(ReadMoreAccordion), propertyChanged: OnMarginChanged);

    public new Thickness Margin
    {
        get => (Thickness)GetValue(MarginProperty);
        set => SetValue(MarginProperty, value);
    }

    static void OnMarginChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (ReadMoreAccordion)bindable;
        control.CustomReadMoreAccordion.Margin = (Thickness)newValue;
        control.InvalidateMeasure();
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        if(IsBodyVisible)
        {
            IsBodyVisible = false;
        }
        else
        {
            IsBodyVisible = true;
        }
    }
}