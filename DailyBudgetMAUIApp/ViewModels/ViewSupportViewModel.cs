using DailyBudgetMAUIApp.DataServices;
using CommunityToolkit.Mvvm.ComponentModel;
using DailyBudgetMAUIApp.Models;
using CommunityToolkit.Mvvm.Input;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(SupportID), nameof(SupportID))]
    public partial class ViewSupportViewModel : BaseViewModel
    {
        private readonly IRestDataService _ds;
        private readonly IProductTools _pt;
        private readonly FilePickerFileType FileTypes;

        [ObservableProperty]
        private int supportID;
        [ObservableProperty]
        private bool isOpen;
        [ObservableProperty]
        private bool isDownloaded;
        [ObservableProperty]
        private bool isFileUpload;
        [ObservableProperty]
        private string status;        
        [ObservableProperty]
        private string newMessageText;
        [ObservableProperty]
        private CustomerSupport support = new CustomerSupport();
        [ObservableProperty]
        public FileResult uploadFile;
        [ObservableProperty]
        public string fileName;
        [ObservableProperty]
        public string txtDownloadFile; 
        [ObservableProperty]
        public bool isUploadInValid;

        public ViewSupportViewModel(IProductTools pt, IRestDataService ds)
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
        }       

        public async Task GetSupport()
        {
            this.Support = await _ds.GetSupport(SupportID,"ViewSupport");
            if(!string.IsNullOrWhiteSpace(Support.FileName))
            {
                IsFileUpload = true;   
                TxtDownloadFile = "File loading ...";
                FileName = $"{Support.FileName}";
                await Task.Run(async () => await DownloadSupportFile());

            }

            if(Support.IsClosed)
            {
                Status = "Closed";
            }
            else
            {
                Status = "Active";
            }
            IsOpen = !Support.IsClosed;
            Title = $"Support Request No. {Support.Whenadded.ToString("mmss")}{SupportID}";

            await _ds.SetAllMessagesRead(SupportID);
            return;
        }

        private async Task DownloadSupportFile()
        {
            Stream File = await _ds.DownloadFile(SupportID);
            long sizeInBytes = File.Length;
            string FileSize = "";

            if (sizeInBytes >= 1024 * 1024)
                FileSize = $"{sizeInBytes / (1024 * 1024.0):F2} MB";
            else if (sizeInBytes >= 1024)
                FileSize = $"{sizeInBytes / 1024.0:F2} KB";
            else
                FileSize = $"{sizeInBytes} bytes";

            UploadFile = await SaveStreamAsFileResultAsync(File, Support.FileName);
            TxtDownloadFile = "Open File";
            FileName = $"{Support.FileName} ({FileSize})";
            IsDownloaded = true;
        }

        public async Task<FileResult> SaveStreamAsFileResultAsync(Stream stream, string fileName)
        {
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            Directory.CreateDirectory(FileSystem.CacheDirectory);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await stream.CopyToAsync(fileStream);
            }

            return new FileResult(filePath);
        }

        [RelayCommand]
        public async Task DownloadFile()
        {
            try
            {
                if (UploadFile != null)
                {

                    await Launcher.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(UploadFile.FullPath)
                    });
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewSupport", "OnFileClicked");
            }
        }

        [RelayCommand]
        public async Task Upload_File()
        {
            
            try
            {
                IsUploadInValid = false;

                var pickOptions = new PickOptions
                {
                    PickerTitle = "Select an image or PDF file",
                    FileTypes = FileTypes
                };

                UploadFile = await FilePicker.PickAsync(pickOptions);

                if (UploadFile is null) return;

                long sizeInBytes = UploadFile.OpenReadAsync().Result.Length;

                if (sizeInBytes < 3000000)
                {
                    Support.FileName = UploadFile.FileName;
                    string FileLocation = await _ds.SaveSupportFile(UploadFile);
                    Support.FileLocation = FileLocation;

                    List<PatchDoc> SupportUpdate = new List<PatchDoc>();

                    PatchDoc NewFileName = new PatchDoc
                    {
                        op = "replace",
                        path = "/FileName",
                        value = Support.FileName
                    };

                    SupportUpdate.Add(NewFileName);

                    PatchDoc NewFileLocation = new PatchDoc
                    {
                        op = "replace",
                        path = "/FileLocation",
                        value = Support.FileLocation
                    };

                    SupportUpdate.Add(NewFileLocation);


                    string Status = await _ds.PatchSupport(SupportID, SupportUpdate);
                    if(Status != "OK")
                    {
                        await Task.Delay(3000);
                        await _pt.MakeSnackBar("Sorry there was an issue uploading your file", null, null, new TimeSpan(0, 0, 10), "Danger");
                    }
                    else
                    {

                        string FileSize = "";
                        if (sizeInBytes >= 1024 * 1024)
                            FileSize = $"{sizeInBytes / (1024 * 1024.0):F2} MB";
                        else if (sizeInBytes >= 1024)
                            FileSize = $"{sizeInBytes / 1024.0:F2} KB";
                        else
                            FileSize = $"{sizeInBytes} bytes";

                        IsFileUpload = true;
                        TxtDownloadFile = "Open File";
                        FileName = $"{Support.FileName} ({FileSize})";
                        IsDownloaded = true;

                        try
                        {
                            await Task.Delay(3000);
                            await _pt.MakeSnackBar("We have uploaded your document!", null, null, new TimeSpan(0, 0, 10), "Success");
                        }
                        catch
                        {

                        }

                    }
                    
                }
                else
                {
                    UploadFile = null;
                    IsUploadInValid = true;
                }
            }
            catch (Exception ex)
            {
                await _pt.MakeSnackBar("Sorry there was an issue uploading your file", null, null, new TimeSpan(0, 0, 10), "Danger");
                await _pt.HandleException(ex, "ViewSupport", "Upload_File");
            }
        }

        [RelayCommand]
        public async Task DeleteFile()
        {
            try
            {
                string Status = await _ds.DeleteSupportFile(SupportID);
                if (Status != "OK")
                {                    
                    try
                    {
                        await _pt.MakeSnackBar("Sorry there was an issue deleting your file", null, null, new TimeSpan(0, 0, 10), "Danger");
                    }
                    catch
                    {

                    }
                    return;
                }

                Support.FileName = "";
                Support.FileLocation = "";

                List<PatchDoc> SupportUpdate = new List<PatchDoc>();

                PatchDoc NewFileName = new PatchDoc
                {
                    op = "replace",
                    path = "/FileName",
                    value = Support.FileName
                };

                SupportUpdate.Add(NewFileName);

                PatchDoc NewFileLocation = new PatchDoc
                {
                    op = "replace",
                    path = "/FileLocation",
                    value = Support.FileLocation
                };

                SupportUpdate.Add(NewFileLocation);


                Status = await _ds.PatchSupport(SupportID, SupportUpdate);
                if(Status != "OK")
                {
                    try
                    {
                        await _pt.MakeSnackBar("Sorry there was an issue deleting your file", null, null, new TimeSpan(0, 0, 10), "Danger");
                    }
                    catch
                    {

                    }
                    
                }
                else
                {
                    try
                    {
                        await _pt.MakeSnackBar("File deleted", null, null, new TimeSpan(0, 0, 10), "Success");
                    }
                    catch
                    {

                    }                    

                    UploadFile = null;
                    IsFileUpload = false;
                }

            }
            catch (Exception ex)
            {
                await _pt.MakeSnackBar("Sorry there was an issue deleting your file", null, null, new TimeSpan(0, 0, 10), "Danger");
                await _pt.HandleException(ex, "ViewSupport", "DeleteFileCommand");
            }
        }

        [RelayCommand]
        public async Task Refresh()
        {
            try
            {
                await GetSupport();
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewSupport", "Refresh");
            }
           
        }

        [RelayCommand]
        public async Task SendMessage()
        {
            try
            {
                CustomerSupportMessage NewMessage = new CustomerSupportMessage()
                {
                    Message = NewMessageText,
                    IsCustomerReply = true,
                    IsRead = true,
                    Whenadded = DateTime.Now

                };

                await _ds.AddReply(SupportID, NewMessage);
                NewMessageText = "";
                await GetSupport();
            }
            catch (Exception ex)
            {
                await _pt.MakeSnackBar("Sorry there was an issue sending your message", null, null, new TimeSpan(0, 0, 10), "Danger");
                await _pt.HandleException(ex, "ViewSupport", "SendMessage");
            }
        }

        [RelayCommand]
        public async Task CloseSupport()
        {
            try
            {
                Support.IsClosed = true;
                List<PatchDoc> SupportUpdate = new List<PatchDoc>();

                PatchDoc NewIsClosed = new PatchDoc
                {
                    op = "replace",
                    path = "/IsClosed",
                    value = Support.IsClosed
                };

                SupportUpdate.Add(NewIsClosed);

                string UpdateStatus = await _ds.PatchSupport(SupportID, SupportUpdate);
                if (UpdateStatus == "OK")
                {
                    IsOpen = false;
                    Status = "Closed";                        
                    await _pt.MakeSnackBar("Support closed, thank you for you inquiry", null, null, new TimeSpan(0, 0, 10), "Success");
                }

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewSupport", "CloseSupport");
            }
        }

        public async Task HandleException(Exception ex, string Page, string Method)
        {
            await _pt.HandleException(ex, Page, Method);
        }

    }
}
