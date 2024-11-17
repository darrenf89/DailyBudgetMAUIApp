using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using Microsoft.Maui.Storage;



namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopUpContactUsViewModel : BaseViewModel
    {
        public double ScreenWidth { get; }
        public double ScreenHeight { get; }
        public double PopupWidth { get; }
        public double ErrorWidth { get; }


        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        private readonly FilePickerFileType FileTypes;

        [ObservableProperty]
        public string email = App.UserDetails.Email;
        [ObservableProperty]
        public string phoneNumber;
        [ObservableProperty]
        public string inquiryType;
        [ObservableProperty]
        public string details;
        [ObservableProperty]
        public string fileName;
        [ObservableProperty]
        public string charactersRemaining = "1000 characters remaining";
        [ObservableProperty]
        public bool isFileUpload;
        [ObservableProperty]
        public bool isAgree;
        [ObservableProperty]
        public bool isDetailsInValid;
        [ObservableProperty]
        public bool isAgreeInValid;
        [ObservableProperty]
        public bool isCategoryInValid;
        [ObservableProperty]
        public bool isUploadInValid;
        [ObservableProperty]
        public FileResult uploadFile;

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

            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            PopupWidth = ScreenWidth - 30;
            ErrorWidth = PopupWidth - 60;


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

                if(string.IsNullOrWhiteSpace(Details) || Details.Length < 80)
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

                if(UploadFile is not null)
                {
                    Support.FileName = UploadFile.FileName;
                    string FileLocation = await _ds.SaveSupportFile(UploadFile);
                    Support.FileLocation = FileLocation;
                }

                Support = await _ds.CreateSupport(App.UserDetails.UserID, Support);

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

                if (UploadFile.OpenReadAsync().Result.Length < 3000000)
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
            if(newValue is not null)
            {
                IsFileUpload = true;
                IsUploadInValid = false;

                long sizeInBytes = UploadFile.OpenReadAsync().Result.Length;
                string FileSize = "";

                if (sizeInBytes >= 1024 * 1024)
                    FileSize = $"{sizeInBytes / (1024 * 1024.0):F2} MB";
                else if (sizeInBytes >= 1024)
                    FileSize = $"{sizeInBytes / 1024.0:F2} KB";
                else
                    FileSize = $"{sizeInBytes} bytes";

                FileName = $"Uploaded {newValue.FileName} ({FileSize})";
            }
            else
            {
                IsFileUpload = false;
            }
        }
    }
}
