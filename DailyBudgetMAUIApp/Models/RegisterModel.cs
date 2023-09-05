using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace DailyBudgetMAUIApp.Models
{
    public class RegisterModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        int _id;
        [Key]
        public int Id
        {
            get => _id;
            set
            {
                if (_id == value)
                {
                    return;
                }

                _id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
            }
        }

        string _email;
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email
        {
            get => _email;
            set
            {
                if (_email == value)
                {
                    return;
                }

                _email = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Email)));
            }
        }

        string _password;
        [Required]
        public string Password
        {
            get => _password;
            set
            {
                if (_password == value)
                {
                    return;
                }

                _password = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Password)));
            }
        }


        bool _isDPAPermissions;
        public bool isDPAPermissions
        {
            get => _isDPAPermissions;
            set
            {
                if (_isDPAPermissions == value)
                {
                    return;
                }

                _isDPAPermissions = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isDPAPermissions)));
            }
        }

        bool _isAgreedToTerms;
        public bool isAgreedToTerms
        {
            get => _isAgreedToTerms;
            set
            {
                if (_isAgreedToTerms == value)
                {
                    return;
                }

                _isAgreedToTerms = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isAgreedToTerms)));
            }
        }

        string _salt;
        [Required]
        public string Salt
        {
            get => _salt;
            set
            {
                if (_salt == value)
                {
                    return;
                }

                _salt = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Salt)));
            }
        }


        string _nickName;
        [MaxLength(20)]
        public string? NickName
        {
            get => _nickName;
            set
            {
                if (_nickName == value)
                {
                    return;
                }

                _nickName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NickName)));
            }
        }
    }
}
