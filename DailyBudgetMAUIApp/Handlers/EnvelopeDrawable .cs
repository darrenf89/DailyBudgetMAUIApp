using System.Globalization;
using static Android.Icu.Text.ListFormatter;
using Application = Microsoft.Maui.Controls.Application;
using Font = Microsoft.Maui.Graphics.Font;
namespace DailyBudgetMAUIApp.Handlers
{
    public class EnvelopeDrawable : IDrawable
    {
        private readonly Func<string> _getName;
        private readonly Func<double> _getValue;
        private readonly Func<float> _getNameFontSize;
        private readonly Func<Color> _getNameTextColor;
        private readonly Func<Color> _getEnvelopeFillColor;
        private readonly Func<Color> _getEnvelopeStrokeColor;
        private readonly Func<Color> _getEnvelopeFlapFillColor;
        private readonly Func<float> _getEnvelopeStrokeThickness;

        public EnvelopeDrawable(Func<string> getName, Func<double> getValue, Func<float> getNameFontSize, Func<Color> getNameTextColor, Func<Color> getEnvelopeFillColor, Func<Color> getEnvelopeStrokeColor, Func<float> getEnvelopeStrokeThickness, Func<Color> getEnvelopeFlapFillColor)
        {
            _getName = getName;
            _getValue = getValue;
            _getNameFontSize = getNameFontSize;
            _getNameTextColor = getNameTextColor;
            _getEnvelopeFillColor = getEnvelopeFillColor;
            _getEnvelopeStrokeColor = getEnvelopeStrokeColor;
            _getEnvelopeStrokeThickness = getEnvelopeStrokeThickness;
            _getEnvelopeFlapFillColor = getEnvelopeFlapFillColor;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var name = _getName();
            var value = _getValue();
            var nameFontSize = _getNameFontSize();
            var nameTextColor = _getNameTextColor();
            var fillColor = _getEnvelopeFillColor();
            var fillColorFlap = _getEnvelopeFlapFillColor();
            var strokeColor = _getEnvelopeStrokeColor();
            var strokeThickness = _getEnvelopeStrokeThickness();

            float textHeight = canvas.GetStringSize(name, Font.Default, nameFontSize).Height;            

            float Height = dirtyRect.Height - (textHeight);
            float Width = dirtyRect.Width;

            var valueFontSize = Height * 0.15f;

            float x = 0;
            float y = 0;
            float cx = 0;
            float cy = 0;
            PathF path = new PathF();

            canvas.StrokeLineJoin = LineJoin.Round;

            //DRAW THE OUTLINE
            canvas.StrokeColor = strokeColor;
            canvas.StrokeSize = strokeThickness;
            canvas.FillColor = Colors.Transparent;

            path = new PathF();
            x = (Width / 2) - (Width * 0.1f);
            y = Height * 0.1f;
            path.MoveTo(x, y);

            x = Width * 0.05f;
            y = Height * 0.33f;
            path.LineTo(x, y);

            x = Width * 0.05f;
            y = Height * 0.80f;
            path.LineTo(x, y);

            cx = Width * 0.05f;
            cy = Height * 0.95f;
            x = Width * 0.20f;
            y = Height * 0.95f;
            path.QuadTo(cx, cy, x, y);

            x = Width * 0.80f;
            y = Height * 0.95f;
            path.LineTo(x, y);

            cx = Width * 0.95f;
            cy = Height * 0.95f;
            x = Width * 0.95f;
            y = Height * 0.80f;
            path.QuadTo(cx, cy, x, y);

            x = Width * 0.95f;
            y = Height * 0.33f;
            path.LineTo(x, y);

            x = (Width / 2) + (Width * 0.1f);
            y = Height * 0.1f;
            path.LineTo(x, y);

            cx = (Width / 2);
            cy = Height * 0.05f;
            x = (Width / 2) - (Width * 0.1f);
            y = Height * 0.1f;
            path.QuadTo(cx, cy, x, y);

            canvas.SaveState();
            canvas.SetShadow(new SizeF(6, 6), 10, Colors.Black.WithAlpha(0.6f));
            canvas.DrawPath(path);

            canvas.RestoreState();

            // DRAW THE TOP FLAP FILL
            canvas.FillColor = fillColorFlap;
            canvas.StrokeColor = Colors.Transparent;
            canvas.StrokeSize = 0;

            path = new PathF();
            x = Width * 0.05f;
            y = Height * 0.33f;
            path.MoveTo(x, y);

            x = (Width / 2) - (Width * 0.1f);
            y = Height * 0.48f;
            path.LineTo(x, y);

            cx = (Width / 2);
            cy = Height * 0.50f;
            x = (Width / 2) + (Width * 0.1f);
            y = Height * 0.48f;
            path.QuadTo(cx, cy, x, y);

            x = Width * 0.95f;
            y = Height * 0.33f;
            path.LineTo(x, y);

            x = (Width / 2) + (Width * 0.1f);
            y = Height * 0.1f;
            path.LineTo(x, y);

            cx = (Width / 2);
            cy = Height * 0.05f;
            x = (Width / 2) - (Width * 0.1f);
            y = Height * 0.1f;
            path.QuadTo(cx, cy, x, y);

            x = Width * 0.05f;
            y = Height * 0.33f;
            path.LineTo(x, y);

            canvas.FillPath(path);
            canvas.SaveState();

            //DRAW THE MONEY
            //If Value Greater than zero draw dollar bills
            if (value > 0)
            {
                canvas.StrokeColor = Colors.Black;
                canvas.StrokeSize = strokeThickness / 2;
                canvas.Font = Font.DefaultBold;

                path = new PathF();
                x = Width * 0.225f;
                y = Height * 0.25f;

                float BillWidth = Width * 0.6f;
                float BillHeight = Height * 0.3f;
                float BillCorner = Height * 0.05f;

                canvas.FillColor = Color.FromArgb("#56B74A");
                canvas.SetShadow(new SizeF(-3, -3), 3, Colors.Black.WithAlpha(0.3f));
                canvas.DrawRoundedRectangle(x,y, BillWidth, BillHeight, BillCorner);
                canvas.FillRoundedRectangle(x, y, BillWidth, BillHeight, BillCorner);
                canvas.SetShadow(new SizeF(-3, -3), 5, Colors.Transparent.WithAlpha(0f));

                x = (Width * 0.225f) + (Width * 0.3f) - (Height * 0.125f); 
                y = (Height * 0.25f) + (Height * 0.15f) - (Height * 0.125f);

                BillWidth = Height * 0.25f;
                BillHeight = Height * 0.25f;
                canvas.FillColor = Colors.LightGreen;
                canvas.DrawEllipse(x, y, BillWidth, BillHeight);
                canvas.FillEllipse(x, y, BillWidth, BillHeight);

                canvas.FontSize = Height * 0.2f;
                canvas.FontColor = Colors.Black;
                string currencySymbol = Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol;
                float moneyTextHeight = canvas.GetStringSize(currencySymbol, Font.DefaultBold, Height * 0.2f).Height;
                float moneyTextWidth = canvas.GetStringSize(currencySymbol, Font.DefaultBold, Height * 0.2f).Width;
                x = (Width * 0.225f) + (Width * 0.3f);
                y = (Height * 0.25f) + (Height * 0.3f) - (((Height * 0.3f) - (moneyTextHeight))/2) - Height * 0.0125f;
                canvas.DrawString(currencySymbol, x, y, HorizontalAlignment.Center);

                path = new PathF();
                x = Width * 0.125f;
                y = Height * 0.35f;
                BillWidth = Width * 0.6f;
                BillHeight = Height * 0.3f;
                BillCorner = Height * 0.05f;

                canvas.FillColor = Color.FromArgb("#56B74A");
                canvas.SetShadow(new SizeF(-3, -3), 3, Colors.Black.WithAlpha(0.3f));
                canvas.DrawRoundedRectangle(x, y, BillWidth, BillHeight, BillCorner);
                canvas.FillRoundedRectangle(x, y, BillWidth, BillHeight, BillCorner);
                canvas.SetShadow(new SizeF(-3, -3), 5, Colors.Transparent.WithAlpha(0f));

                x = (Width * 0.125f) + (Width * 0.3f) - (Height * 0.125f);
                y = (Height * 0.35f) + (Height * 0.15f) - (Height * 0.125f);

                BillWidth = Height * 0.25f;
                BillHeight = Height * 0.25f;
                canvas.FillColor = Colors.LightGreen;
                canvas.DrawEllipse(x, y, BillWidth, BillHeight);
                canvas.FillEllipse(x, y, BillWidth, BillHeight);

                x = (Width * 0.125f) + (Width * 0.3f);
                y = (Height * 0.35f) + (Height * 0.3f) - (((Height * 0.3f) - (moneyTextHeight)) / 2) - Height * 0.0125f;
                canvas.DrawString(currencySymbol, x, y, HorizontalAlignment.Center);

            }

            canvas.RestoreState();
            //DRAW THE BODY FILL
            canvas.FillColor = fillColor;
            canvas.StrokeColor = Colors.Transparent;
            canvas.StrokeSize = 0;

            path = new PathF();
            x = Width * 0.05f;
            y = Height * 0.33f;
            path.MoveTo(x, y);

            x = Width * 0.05f;
            y = Height * 0.80f;
            path.LineTo(x, y);

            cx = Width * 0.05f;
            cy = Height * 0.95f;
            x = Width * 0.20f;
            y = Height * 0.95f;
            path.QuadTo(cx, cy, x, y);

            x = Width * 0.80f;
            y = Height * 0.95f;
            path.LineTo(x, y);

            cx = Width * 0.95f;
            cy = Height * 0.95f;
            x = Width * 0.95f;
            y = Height * 0.80f;
            path.QuadTo(cx, cy, x, y);

            x = Width * 0.95f;
            y = Height * 0.33f;
            path.LineTo(x, y);

            x = (Width / 2) + (Width * 0.1f);
            y = Height * 0.48f;
            path.LineTo(x, y);

            cx = (Width / 2);
            cy = Height * 0.50f;
            x = (Width / 2) - (Width * 0.1f);
            y = Height * 0.48f;
            path.QuadTo(cx, cy, x, y);

            x = Width * 0.05f;
            y = Height * 0.33f;
            path.LineTo(x, y);

            canvas.FillPath(path);

            //DRAW INSIDE FLAP
            canvas.StrokeColor = strokeColor;
            canvas.StrokeSize = strokeThickness / 2;

            path = new PathF();
            x = Width * 0.05f;
            y = Height * 0.33f;
            path.MoveTo(x, y);

            x = (Width / 2) - (Width * 0.1f);
            y = Height * 0.48f;
            path.LineTo(x, y);

            cx = (Width / 2);
            cy = Height * 0.50f;
            x = (Width / 2) + (Width * 0.1f);
            y = Height * 0.48f;
            path.QuadTo(cx, cy, x, y);

            x = Width * 0.95f;
            y = Height * 0.33f;
            path.LineTo(x, y);

            canvas.SaveState();
            canvas.SetShadow(new SizeF(0, -2), 5, fillColor.WithAlpha(0.3f));
            canvas.DrawPath(path);

            canvas.RestoreState();           

            canvas.FontSize = valueFontSize;
            string CurrencyValue = value.ToString("c", CultureInfo.CurrentCulture);
            
            float pillX = Width * 0.15f;
            float pillY = Height * 0.6f;
            float pillWidth = Width * 0.7f;
            float pillHeight = Height * 0.25f;
            float pillCornerRadius = pillHeight / 2;

            canvas.StrokeColor = value > 0 ? (Application.Current.Resources["Success"] as Color ?? Colors.Green) : (Application.Current.Resources["Danger"] as Color ?? Colors.Red);
            canvas.StrokeSize = 1;
            canvas.DrawRoundedRectangle(pillX, pillY, pillWidth, pillHeight, pillCornerRadius);

            canvas.FillColor = value > 0 ? (Application.Current.Resources["SuccessLight"] as Color ?? Colors.Green) : (Application.Current.Resources["DangerLight"] as Color ?? Colors.Red);
            canvas.FillRoundedRectangle(pillX, pillY, pillWidth, pillHeight, pillCornerRadius);

            IFont valueFont = Font.Default;
            string formattedValue = TruncateString(CurrencyValue, Width * 0.65f, canvas, valueFontSize, valueFont);

            canvas.FontColor = value > 0 ? (Application.Current.Resources["Success"] as Color ?? Colors.White) : (Application.Current.Resources["Danger"] as Color ?? Colors.White);
            canvas.DrawString(formattedValue, pillX + (pillWidth / 2), pillY + (pillHeight - (Height * 0.06f)), HorizontalAlignment.Center);

            float nameMaxWidth = (float)Width * 0.9f;
            string formattedName = TruncateString(name, nameMaxWidth, canvas, nameFontSize, valueFont);

            canvas.FontColor = nameTextColor;
            canvas.FontSize = nameFontSize;
            canvas.DrawString(formattedName, Width / 2, dirtyRect.Height, HorizontalAlignment.Center);
        }

        private string TruncateString(string text, float maxWidth, ICanvas canvas, float fontSize, IFont font)
        {
            float textWidth = canvas.GetStringSize(text, font, fontSize).Width;

            if (textWidth <= maxWidth)
                return text;

            string truncated = text;
            while (canvas.GetStringSize(truncated + "..", font, fontSize).Width > maxWidth && truncated.Length > 1)
            {
                truncated = truncated.Substring(0, truncated.Length - 1);
            }

            return truncated + "..";
        }
    }
}