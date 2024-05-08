
using Maui.FixesAndWorkarounds;

namespace DailyBudgetMAUIApp.Handlers;

public partial class FocusedEntry : ContentView
{
	public FocusedEntry()
	{
		InitializeComponent();

        FEEntry.TextChanged += (sender, args) =>
        {
            OnTextChanged(args);
        };

        FEEntry.Focused += (sender, args) =>
        {
            OnFocused(args);
        };        
        
        FEEntry.Unfocused += (sender, args) =>
        {
            OnUnfocused(args);
        };
    }

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(FocusedEntry), propertyChanged: OnTextChanged, defaultBindingMode: BindingMode.TwoWay);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (FocusedEntry)bindable;
        //control.FEEntry.Text = (string)newValue;
        control.InvalidateLayout();
    }

    public new static BindableProperty WidthProperty =
    BindableProperty.Create(nameof(Width), typeof(double), typeof(FocusedEntry), propertyChanged: OnWidthChanged);

    public new double Width
    {
        get => (double)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    static void OnWidthChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (FocusedEntry)bindable;
        control.FEBorder.WidthRequest = (double)newValue;
        control.FEEntry.WidthRequest = (double)newValue - 10;
        control.InvalidateLayout();
    }

    public static BindableProperty IdentifierProperty =
        BindableProperty.Create(nameof(Identifier), typeof(string), typeof(FocusedEntry), propertyChanged: OnIdentifierChanged);

    public string Identifier
    {
        get => (string)GetValue(IdentifierProperty);
        set => SetValue(IdentifierProperty, value);
    }

    static void OnIdentifierChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (FocusedEntry)bindable;
        control.FEEntry.Identifier = (string)newValue;
        control.InvalidateLayout();
    }

    public static BindableProperty FEFontSizeProperty =
    BindableProperty.Create(nameof(FEFontSize), typeof(double), typeof(FocusedEntry), propertyChanged: OnFEFontSizeChanged);

    public double FEFontSize
    {
        get => (double)GetValue(FEFontSizeProperty);
        set => SetValue(FEFontSizeProperty, value);
    }

    static void OnFEFontSizeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (FocusedEntry)bindable;
        control.FEEntry.FontSize = (double)newValue;
        control.InvalidateLayout();
    }

    public static BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(FocusedEntry), propertyChanged: OnTextColorChanged);

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    static void OnTextColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (FocusedEntry)bindable;
        control.FEEntry.TextColor = (Color)newValue;
        control.InvalidateLayout();
    }

    public static BindableProperty KeyboardProperty =
    BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(FocusedEntry), propertyChanged: OnKeyboardChanged);

    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    static void OnKeyboardChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (FocusedEntry)bindable;
        control.FEEntry.Keyboard = (Keyboard)newValue;
        control.InvalidateLayout();
    }

    public static BindableProperty MaxLengthProperty =
    BindableProperty.Create(nameof(MaxLength), typeof(int), typeof(FocusedEntry), propertyChanged: OnMaxLengthChanged);

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    static void OnMaxLengthChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (FocusedEntry)bindable;
        control.FEEntry.MaxLength = (int)newValue;
        control.InvalidateLayout();
    }

    public new static BindableProperty MarginProperty =
    BindableProperty.Create(nameof(Margin), typeof(Thickness), typeof(FocusedEntry), propertyChanged: OnMarginChanged);

    public new Thickness Margin
    {
        get => (Thickness)GetValue(MarginProperty);
        set => SetValue(MarginProperty, value);
    }

    static void OnMarginChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (FocusedEntry)bindable;
        control.FEBorder.Margin = (Thickness)newValue;
        control.InvalidateLayout();
    }

    public new static BindableProperty HorizontalOptionsProperty =
    BindableProperty.Create(nameof(HorizontalOptions), typeof(LayoutOptions), typeof(FocusedEntry), propertyChanged: OnHorizontalOptionsChanged);

    public new LayoutOptions HorizontalOptions
    {
        get => (LayoutOptions)GetValue(HorizontalOptionsProperty);
        set => SetValue(HorizontalOptionsProperty, value);
    }

    static void OnHorizontalOptionsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (FocusedEntry)bindable;
        control.FEEntry.HorizontalOptions = (LayoutOptions)newValue;
        control.FEBorder.HorizontalOptions = (LayoutOptions)newValue;
        control.InvalidateLayout();
    }

    public new static BindableProperty VerticalOptionsProperty =
    BindableProperty.Create(nameof(VerticalOptions), typeof(LayoutOptions), typeof(FocusedEntry), propertyChanged: OnVerticalOptionsChanged);

    public new LayoutOptions VerticalOptions
    {
        get => (LayoutOptions)GetValue(VerticalOptionsProperty);
        set => SetValue(VerticalOptionsProperty, value);
    }

    static void OnVerticalOptionsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (FocusedEntry)bindable;
        control.FEBorder.VerticalOptions = (LayoutOptions)newValue;
        control.InvalidateLayout();        
    }

    public static BindableProperty ReturnTypeProperty =
    BindableProperty.Create(nameof(ReturnType), typeof(ReturnType), typeof(FocusedEntry), propertyChanged: OnReturnTypeChanged);

    public ReturnType ReturnType
    {
        get => (ReturnType)GetValue(ReturnTypeProperty);
        set => SetValue(ReturnTypeProperty, value);
    }

    static void OnReturnTypeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (FocusedEntry)bindable;
        control.FEEntry.ReturnType = (ReturnType)newValue;
        control.InvalidateLayout();
    }

    public event EventHandler<TextChangedEventArgs> TextChanged;
    protected virtual void OnTextChanged(TextChangedEventArgs e)
    {
        EventHandler<TextChangedEventArgs> handler = TextChanged;
        handler?.Invoke(this, e);
    }    

    public new event EventHandler<FocusEventArgs> Focused;
    protected virtual void OnFocused(FocusEventArgs e)
    {
        EventHandler<FocusEventArgs> handler = Focused;
        handler?.Invoke(this, e);
        FEEntry.ShowKeyboard();        
    }   
    
    public new event EventHandler<FocusEventArgs> Unfocused;
    protected virtual void OnUnfocused(FocusEventArgs e)
    {
        EventHandler<FocusEventArgs> handler = Unfocused;
        handler?.Invoke(this, e);
        FEEntry.HideKeyboard();        
    }

    private void FEEntry_Focused(object sender, FocusEventArgs e)
    {
        Application.Current.Resources.TryGetValue("Info", out var Info);
        Application.Current.Resources.TryGetValue("InfoBrush", out var InfoBrush);

        FEBorder.Stroke = (Color)Info;
        FEBorder.StrokeThickness = 2;
        FEBorder.Shadow = new Shadow
        {
            Brush = (Brush)InfoBrush,
            Opacity = (float)0.95,
            Offset = new Point(0,0),
            Radius = 10
        };
    }

    private void FEEntry_Unfocused(object sender, FocusEventArgs e)
    {
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);
        Application.Current.Resources.TryGetValue("Gray900Brush", out var Gray900Brush);

        FEBorder.Stroke = (Color)Gray900;
        FEBorder.StrokeThickness= 1;
        FEBorder.Shadow = new Shadow
        {
            Brush = (Brush)Gray900Brush,
            Opacity = (float)0,
            Offset = new Point(0, 0),
            Radius = 10
        };
    }
}