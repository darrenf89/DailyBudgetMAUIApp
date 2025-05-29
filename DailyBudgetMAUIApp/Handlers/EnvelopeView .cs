using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using System.Windows.Input;

namespace DailyBudgetMAUIApp.Handlers
{
    public class EnvelopeView : GraphicsView
    {
        private bool _longPressTriggered;

        public static readonly BindableProperty NameProperty = BindableProperty.Create(nameof(Name), typeof(string), typeof(EnvelopeView), "Envelope", propertyChanged: (bindable, oldValue, newValue) => ((EnvelopeView)bindable).Invalidate());

        public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(double), typeof(EnvelopeView), (double)0.0 , propertyChanged: (bindable, oldValue, newValue) => ((EnvelopeView)bindable).Invalidate());

        public static readonly BindableProperty ClickScaleProperty = BindableProperty.Create(nameof(ClickScale), typeof(double), typeof(EnvelopeView), (double)0.8 , propertyChanged: (bindable, oldValue, newValue) => ((EnvelopeView)bindable).Invalidate());

        public static readonly BindableProperty NameFontSizeProperty = BindableProperty.Create(nameof(NameFontSize), typeof(float), typeof(EnvelopeView), 12f, propertyChanged: (bindable, oldValue, newValue) => ((EnvelopeView)bindable).Invalidate());

        public static readonly BindableProperty TapCommandProperty = BindableProperty.Create(nameof(TapCommand), typeof(ICommand), typeof(EnvelopeView), propertyChanged: (bindable, oldValue, newValue) => ((EnvelopeView)bindable).Invalidate());

        public static readonly BindableProperty TapCommandParameterProperty = BindableProperty.Create(nameof(TapCommandParameter), typeof(object), typeof(EnvelopeView), propertyChanged: (bindable, oldValue, newValue) => ((EnvelopeView)bindable).Invalidate());

        public static readonly BindableProperty NameTextColorProperty = BindableProperty.Create(nameof(NameTextColor), typeof(Color), typeof(EnvelopeView), Application.Current.Resources["Primary"] as Color ?? Colors.Black, propertyChanged: (bindable, oldValue, newValue) => ((EnvelopeView)bindable).Invalidate());

        public static readonly BindableProperty EnvelopeFlapFillColorProperty = BindableProperty.Create(nameof(EnvelopeFlapFillColor), typeof(Color), typeof(EnvelopeView), Colors.White, propertyChanged: (bindable, oldValue, newValue) => ((EnvelopeView)bindable).Invalidate());
        
        public static readonly BindableProperty EnvelopeFillColorProperty = BindableProperty.Create(nameof(EnvelopeFillColor), typeof(Color), typeof(EnvelopeView), Colors.White, propertyChanged: (bindable, oldValue, newValue) => ((EnvelopeView)bindable).Invalidate());

        public static readonly BindableProperty EnvelopeStrokeColorProperty = BindableProperty.Create(nameof(EnvelopeStrokeColor), typeof(Color), typeof(EnvelopeView), Colors.Black, propertyChanged: (bindable, oldValue, newValue) => ((EnvelopeView)bindable).Invalidate());

        public static readonly BindableProperty EnvelopeStrokeThicknessProperty = BindableProperty.Create(nameof(EnvelopeStrokeThickness), typeof(float), typeof(EnvelopeView), 2f, propertyChanged: (bindable, oldValue, newValue) => ((EnvelopeView)bindable).Invalidate());

        public static readonly BindableProperty LongPressCommandProperty = BindableProperty.Create( nameof(LongPressCommand), typeof(ICommand), typeof(EnvelopeView), propertyChanged: (bindable, oldValue, newValue) => ((EnvelopeView)bindable).Invalidate());

        public string Name
        {
            get => (string)GetValue(NameProperty);
            set => SetValue(NameProperty, string.IsNullOrEmpty(value) ? "Envelope" : value);
        }

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double ClickScale
        {
            get => (double)GetValue(ClickScaleProperty);
            set => SetValue(ClickScaleProperty, value);
        }
        public float NameFontSize
        {
            get => (float)GetValue(NameFontSizeProperty);
            set => SetValue(NameFontSizeProperty, value);
        }

        public ICommand TapCommand
        {
            get => (ICommand)GetValue(TapCommandProperty);
            set => SetValue(TapCommandProperty, value);
        }

        public object TapCommandParameter
        {
            get => GetValue(TapCommandParameterProperty);
            set => SetValue(TapCommandParameterProperty, value);
        }

        public Color NameTextColor
        {
            get => (Color)GetValue(NameTextColorProperty);
            set => SetValue(NameTextColorProperty, value ?? (Application.Current.Resources["Primary"] as Color ?? Colors.Black));
        }

        public Color EnvelopeFillColor
        {
            get => (Color)GetValue(EnvelopeFillColorProperty);
            set => SetValue(EnvelopeFillColorProperty, value ?? Colors.White);
        }

        public Color EnvelopeFlapFillColor
        {
            get => (Color)GetValue(EnvelopeFlapFillColorProperty);
            set => SetValue(EnvelopeFlapFillColorProperty, value ?? Colors.White);
        }

        public Color EnvelopeStrokeColor
        {
            get => (Color)GetValue(EnvelopeStrokeColorProperty);
            set => SetValue(EnvelopeStrokeColorProperty, value ?? Colors.Black);
        }

        public float EnvelopeStrokeThickness
        {
            get => (float)GetValue(EnvelopeStrokeThicknessProperty);
            set => SetValue(EnvelopeStrokeThicknessProperty, value);
        }
        public ICommand LongPressCommand
        {
            get => (ICommand)GetValue(LongPressCommandProperty);
            set => SetValue(LongPressCommandProperty, value);
        }



        public EnvelopeView()
        {
            Drawable = new EnvelopeDrawable(
                () => Name,
                () => Value,
                () => NameFontSize,
                () => NameTextColor,
                () => EnvelopeFillColor,
                () => EnvelopeStrokeColor,
                () => EnvelopeStrokeThickness,
                () => EnvelopeFlapFillColor);

            AddTapGesture();

        }
        private void AddTapGesture()
        {

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) =>
            {
                if (_longPressTriggered)
                {
                    _longPressTriggered = false;
                    return;
                }

                if (TapCommand != null)
                {
                    if (TapCommand.CanExecute(TapCommandParameter))
                    {
                        try
                        {
                            TapCommand.Execute(TapCommandParameter);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }                    
                }
            };
            GestureRecognizers.Add(tapGesture);

            var longPressGestureImageUpdate = new TouchBehavior();
            longPressGestureImageUpdate.LongPressDuration = 1;
            longPressGestureImageUpdate.LongPressCommand = new Command(async () =>
            {
                if(ClickScale > 1)
                {
                    ClickScale = 1;
                }

                if(ClickScale < 0)
                {
                    ClickScale = 0.05;
                }

                HeightRequest *= ClickScale;
                WidthRequest *= ClickScale;
                InvalidateMeasure();
                await Task.Delay(1);

                await Task.Run(async () =>
                {
                    await Task.Delay(750);
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        HeightRequest /= ClickScale;
                        WidthRequest /= ClickScale;
                        InvalidateMeasure();
                    });
                });
            });
            this.Behaviors.Add(longPressGestureImageUpdate);

            var longPressGesture = new TouchBehavior();
            longPressGesture.LongPressDuration = 750;
            longPressGesture.LongPressCommand = new Command(async () =>
            {
                _longPressTriggered = true;
                if (LongPressCommand != null)
                {
                    if (LongPressCommand.CanExecute(TapCommandParameter))
                    {
                        try
                        {
                            LongPressCommand.Execute(TapCommandParameter);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }                    
                }
            });

            this.Behaviors.Add(longPressGesture);
        }

    }
}
