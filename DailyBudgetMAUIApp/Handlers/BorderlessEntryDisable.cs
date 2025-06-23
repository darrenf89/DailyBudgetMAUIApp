namespace DailyBudgetMAUIApp.Handlers
{
    public class BorderlessEntryDisable : BorderlessEntry
    {
        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            Application.Current.Resources.TryGetValue("White", out var White);
            Application.Current.Resources.TryGetValue("Gray200", out var Gray200);

            switch (propertyName)
            {
                case nameof(IsEnabled):

                    if (!IsEnabled)
                    {
                        BackgroundColor = (Color)Gray200;
                        TextColor = (Color)White;
                    }

                    break;

                case nameof(TextColor):

                    if (!IsEnabled)
                    {
                        TextColor = (Color)White;
                    }

                    break;

                case nameof(BackgroundColor):

                    if (!IsEnabled)
                    {
                        BackgroundColor = (Color)Gray200;
                    }

                    break;
            }

        }

    }
}
