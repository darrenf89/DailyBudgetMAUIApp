using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Helpers;
using DailyBudgetMAUIApp.Models;
using Microsoft.Maui.Storage;



namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopUpContactUsViewModel : BaseViewModel, IQueryAttributable
    {
        public double ScreenWidth { get; }
        public double ScreenHeight { get; }
        public double PopupWidth { get; }
        public double ErrorWidth { get; }


        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        private readonly FilePickerFileType FileTypes;

        [ObservableProperty]
        public partial string Email { get; set; } = (App.IsFamilyAccount ? App.FamilyUserDetails.Email : App.UserDetails.Email);

        [ObservableProperty]
        public partial string PhoneNumber { get; set; }

        [ObservableProperty]
        public partial string InquiryType { get; set; }

        [ObservableProperty]
        public partial string Details { get; set; }

        [ObservableProperty]
        public partial string FileName { get; set; }

        [ObservableProperty]
        public partial string CharactersRemaining { get; set; } = "1000 characters remaining";

        [ObservableProperty]
        public partial bool IsFileUpload { get; set; }

        [ObservableProperty]
        public partial bool IsAgree { get; set; }

        [ObservableProperty]
        public partial bool IsDetailsInValid { get; set; }

        [ObservableProperty]
        public partial bool IsAgreeInValid { get; set; }

        [ObservableProperty]
        public partial bool IsCategoryInValid { get; set; }

        [ObservableProperty]
        public partial bool IsUploadInValid { get; set; }

        [ObservableProperty]
        public partial FileResult UploadFile { get; set; }


        public PopUpContactUsViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { "public.image", "public.jpeg", "public.png", "com.adobe.pdf" } },
                { DevicePlatform.Android, new[] { "image/*", "application/pdf" } },
                { DevicePlatform.WinUI, new[] { ".jpg", ".jpeg", ".png", ".pdf" } },
                { DevicePlatform.MacCatalyst, new[] { "public.jpeg", "public.png", "com.adobe.pdf" } }
            });

            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density) - 100;
            ScreenWidth = (DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density) - 30;
            PopupWidth = ScreenWidth - 30;
            ErrorWidth = PopupWidth - 60;


        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {

        }

        [RelayCommand]
        public async Task CreateSupportRequest()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(InquiryType))
                {
                    IsCategoryInValid = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(Details) || Details.Length < 80)
                {
                    IsDetailsInValid = true;
                    return;
                }

                if (!IsAgree)
                {
                    IsAgreeInValid = true;
                    return;
                }

                CustomerSupport Support = new CustomerSupport
                {
                    Details = Details,
                    PhoneNumber = PhoneNumber,
                    Type = InquiryType,
                    Whenadded = DateTime.UtcNow,
                    IsClosed = false,
                    Replys = new List<CustomerSupportMessage>()
                };

                if (UploadFile is not null)
                {
                    Support.FileName = UploadFile.FileName;
                    string FileLocation = await _ds.SaveSupportFile(UploadFile);
                    Support.FileLocation = FileLocation;
                }

                Support = await _ds.CreateSupport(App.IsFamilyAccount ? App.FamilyUserDetails.UniqueUserID : App.UserDetails.UniqueUserID, Support);

                if (Support.SupportID == 0)
                {
                    WeakReferenceMessenger.Default.Send(new ClosePopupMessage(false, 0));
                }
                else
                {
                    WeakReferenceMessenger.Default.Send(new ClosePopupMessage(true, Support.SupportID));
                }

            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ClosePopupMessage(false, 0));
                await _pt.HandleException(ex, "PopUpContactUs", "CreateSupportRequest");
            }
        }

        [RelayCommand]
        async Task Upload_File()
        {
            try
            {
                var pickOptions = new PickOptions
                {
                    PickerTitle = "Select an image or PDF file",
                    FileTypes = FileTypes
                };

                UploadFile = await FilePicker.PickAsync(pickOptions);

                if (UploadFile is null) return;

                if (await _pt.GetFileLengthAsync(UploadFile) < 3000000)
                {

                }
                else
                {
                    IsUploadInValid = true;
                    UploadFile = null;
                }
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ClosePopupMessage(false, 0));
                await _pt.HandleException(ex, "PopUpContactUs", "UploadPicture_Clicked");
            }
        }

        partial void OnInquiryTypeChanged(string oldValue, string newValue)
        {
            if (string.IsNullOrWhiteSpace(newValue))
            {
                IsCategoryInValid = true;
            }
            else
            {
                IsCategoryInValid = false;
            }
        }

        partial void OnIsAgreeChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                IsAgreeInValid = false;
            }
        }

        partial void OnDetailsChanged(string oldValue, string newValue)
        {
            CharactersRemaining = $"{1000 - newValue.Length} characters remaining";

            if (!string.IsNullOrWhiteSpace(newValue) && newValue.Length > 200)
            {
                IsDetailsInValid = false;
            }

        }

        partial void OnUploadFileChanged(FileResult oldValue, FileResult newValue)
        {
            if (newValue is not null)
            {
                IsFileUpload = true;
                IsUploadInValid = false;

                HandleFileUploadAsync(newValue).FireAndForgetSafeAsync();
            }
            else
            {
                IsFileUpload = false;
            }
        }

        // Async helper method for handling file upload
        private async Task HandleFileUploadAsync(FileResult newValue)
        {
            try
            {
                long sizeInBytes = await _pt.GetFileLengthAsync(newValue);
                string fileSize = sizeInBytes >= 1024 * 1024
                    ? $"{sizeInBytes / (1024 * 1024.0):F2} MB"
                    : sizeInBytes >= 1024
                        ? $"{sizeInBytes / 1024.0:F2} KB"
                        : $"{sizeInBytes} bytes";

                FileName = $"Uploaded {newValue.FileName} ({fileSize})";
            }
            catch (Exception ex)
            {
                // Log the error or handle it as needed
                Console.WriteLine($"Error while processing file: {ex.Message}");
            }
        }
    }
}
