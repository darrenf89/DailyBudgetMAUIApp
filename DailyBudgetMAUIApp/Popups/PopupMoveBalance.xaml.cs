using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Models;
using System.Globalization;
using DailyBudgetMAUIApp.DataServices;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopupMoveBalance : Popup
{
    private readonly PopupMoveBalanceViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;

    public PopupMoveBalance(Budgets Budget, string Type, int id, bool IsCoverOverSpend, PopupMoveBalanceViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
        InitializeComponent();

        viewModel.Budget = Budget;        

        BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;

        _vm.Amount = 0;

        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("Danger", out var Danger);
        Application.Current.Resources.TryGetValue("pillSuccess", out var pillSuccess);
        Application.Current.Resources.TryGetValue("pillDanger", out var pillDanger);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        
        _vm.FromBalanceColor = (Color)Success;
        _vm.FromBalanceStyle = (Style)pillSuccess;
        _vm.ToBalanceColor = (Color)Success;
        _vm.ToBalanceStyle = (Style)pillSuccess;
        _vm.IsCoverOverspend = IsCoverOverSpend;

        double Amount = 0;
        entAmount.Text = Amount.ToString("c", CultureInfo.CurrentCulture);

        _vm.Budget = Budget;
        _vm.MoveBalances.Add(new MoveBalanceClass
        {
            Name = "Left to spend",
            Id = 0,
            Type = "Budget",
            Balance = _vm.Budget.LeftToSpendBalance.GetValueOrDefault()
        });

        foreach(Savings saving in Budget.Savings.Where(s => !s.IsSavingsClosed))
        {
            _vm.MoveBalances.Add(new MoveBalanceClass
            {
                Name = saving.SavingsName,
                Id = saving.SavingID,
                Type = "Saving",
                Balance = saving.CurrentBalance.GetValueOrDefault()
            });
        }

        if(!IsCoverOverSpend)
        {
            foreach (Bills bill in Budget.Bills.Where(b => !b.IsClosed))
            {
                _vm.MoveBalances.Add(new MoveBalanceClass
                {
                    Name = bill.BillName,
                    Id = bill.BillID,
                    Type = "Bill",
                    Balance = bill.BillCurrentBalance
                });
            }
        }

        _vm.FromEnabled = true;
        _vm.ToEnabled = true;
        if(Type == "Budget")
        {            
            double Balance = (double)_vm.Budget.LeftToSpendBalance.GetValueOrDefault();
            _vm.ToNewBalanceString = Balance.ToString("c", CultureInfo.CurrentCulture);

            for(int i = _vm.MoveBalances.Count - 1; i >= 0; i--)
            {
                if (_vm.MoveBalances[i].Name == "Left to spend")
                {
                    _vm.ToSelectedMoveBalance = _vm.MoveBalances[i];
                }
            }
        }
        else if(Type == "Saving")
        {
            Savings? s = Budget.Savings.Where(s => s.SavingID == id).FirstOrDefault();
            double Balance = (double)s.CurrentBalance.GetValueOrDefault();
            _vm.ToNewBalanceString = Balance.ToString("c", CultureInfo.CurrentCulture);

            for (int i = _vm.MoveBalances.Count - 1; i >= 0; i--)
            {
                if (_vm.MoveBalances[i].Id == id && _vm.MoveBalances[i].Type == "Saving")
                {
                    _vm.ToSelectedMoveBalance = _vm.MoveBalances[i];
                }
            }

        }
        else if(Type == "Bill")
        {
            Bills? b = Budget.Bills.Where(s => s.BillID == id).FirstOrDefault();
            double Balance = (double)b.BillCurrentBalance;
            _vm.ToNewBalanceString = Balance.ToString("c", CultureInfo.CurrentCulture);

            for (int i = _vm.MoveBalances.Count - 1; i >= 0; i--)
            {
                if (_vm.MoveBalances[i].Id == id && _vm.MoveBalances[i].Type == "Bill")
                {
                    _vm.ToSelectedMoveBalance = _vm.MoveBalances[i];
                }
            }
        }

        _vm.ToEnabled = false;
        if(IsCoverOverSpend)
        {
            brdFromTo.IsEnabled = false;
            brdFromTo.Stroke = (Color)Gray400;            
            if (_vm.Budget.LeftToSpendDailyAmount < 0)
            {
                decimal OverSpend = _vm.Budget.LeftToSpendDailyAmount * -1;
                entAmount.Text = OverSpend.ToString("c", CultureInfo.CurrentCulture);
            }    
            entAmount.IsEnabled = false;
            lblTitle.Text = "Cover todays overspend!";            
        }

    }

    void Amount_Changed(object sender, TextChangedEventArgs e)
    {
        decimal PayDayAmount = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
        entAmount.Text = PayDayAmount.ToString("c", CultureInfo.CurrentCulture);
        int position = e.NewTextValue.IndexOf(App.CurrentSettings.CurrencyDecimalSeparator);
        if (!string.IsNullOrEmpty(e.OldTextValue) && (e.OldTextValue.Length - position) == 2 && entAmount.CursorPosition > position)
        {
            entAmount.CursorPosition = entAmount.Text.Length;
        }  
        _vm.Amount = PayDayAmount;
    }

    private bool ValidatePage()
    {
        bool IsValid = true;

        if (_vm.Amount == 0)
        {
            validator.IsVisible = true;
            lblValidator.Text = "Select an amount to move between accounts";
            return false;
        }
        else
        {
            validator.IsVisible = false;
        }

        if (_vm.ToSelectedMoveBalance is null)
        {
            validator.IsVisible = true;
            lblValidator.Text = "Select an account to move a balance to";
            return false;
        }
        else
        {
            validator.IsVisible = false;
        }

        if (_vm.FromSelectedMoveBalance is null)
        {
            validator.IsVisible = true;
            lblValidator.Text = "Select an account to move a balance from";
            return false;
        }
        else
        {
            validator.IsVisible = false;
        }

        decimal FromAmount = _vm.FromSelectedMoveBalance.Balance - _vm.Amount;
        if (FromAmount < 0)
        {
            validator.IsVisible = true;
            lblValidator.Text = "Moving balance from this account will put it into negative, cant do that!";
            return false;
        }
        else
        {
            validator.IsVisible = false;
        }

        if (_vm.FromSelectedMoveBalance.Name == _vm.ToSelectedMoveBalance.Name && _vm.FromSelectedMoveBalance.Type == _vm.ToSelectedMoveBalance.Type && _vm.FromSelectedMoveBalance.Id == _vm.ToSelectedMoveBalance.Id)
        {
            validator.IsVisible = true;
            lblValidator.Text = "Can't move balance from an account to the same account, seems obvious to us!";
            return false;
        }
        else
        {
            validator.IsVisible = false;
        }

        return IsValid;
    }

    private async void AcceptUpdate_Saving(object sender, EventArgs e)
    {
        if(ValidatePage())
        {
            bool MoveBalance = await Application.Current.MainPage.DisplayAlert($"Move money around?", $"Are you sure you want to move {entAmount.Text} from {_vm.FromSelectedMoveBalance.Name} to {_vm.ToSelectedMoveBalance.Name}", "Yes", "No");
            if (MoveBalance)
            {
                decimal FromAmount = _vm.FromSelectedMoveBalance.Balance - _vm.Amount;
                decimal ToAmount = _vm.ToSelectedMoveBalance.Balance + _vm.Amount;
                switch (_vm.FromSelectedMoveBalance.Type)
                {
                    case "Budget":
                        break;
                    case "Bill":
                        Bills bill = await _ds.GetBillFromID(_vm.FromSelectedMoveBalance.Id);

                        List<PatchDoc> BillUpdate = new List<PatchDoc>();

                        PatchDoc BillAmount = new PatchDoc
                        {
                            op = "replace",
                            path = "/BillCurrentBalance",
                            value = FromAmount
                        };

                        BillUpdate.Add(BillAmount);

                        if(bill.BillBalanceAtLastPayDay > FromAmount)
                        {
                            PatchDoc BillBalanceAtLastPayDay = new PatchDoc
                            {
                                op = "replace",
                                path = "/BillBalanceAtLastPayDay",
                                value = FromAmount
                            };

                            BillUpdate.Add(BillBalanceAtLastPayDay);
                        }

                        await _ds.PatchBill(_vm.FromSelectedMoveBalance.Id, BillUpdate);
                        break;
                    case "Saving":

                        List<PatchDoc> SavingUpdate = new List<PatchDoc>();

                        PatchDoc SavingAmount = new PatchDoc
                        {
                            op = "replace",
                            path = "/CurrentBalance",
                            value = FromAmount
                        };

                        SavingUpdate.Add(SavingAmount);

                        await _ds.PatchSaving(_vm.FromSelectedMoveBalance.Id, SavingUpdate);
                        break;
                    default:
                        break;
                }

                switch (_vm.ToSelectedMoveBalance.Type)
                {
                    case "Budget":
                        break;
                    case "Bill":
                        Bills bill = await _ds.GetBillFromID(_vm.ToSelectedMoveBalance.Id);

                        List<PatchDoc> BillUpdate = new List<PatchDoc>();

                        PatchDoc BillAmount = new PatchDoc
                        {
                            op = "replace",
                            path = "/BillCurrentBalance",
                            value = ToAmount
                        };

                        BillUpdate.Add(BillAmount);

                        if (bill.BillBalanceAtLastPayDay > ToAmount)
                        {
                            PatchDoc BillBalanceAtLastPayDay = new PatchDoc
                            {
                                op = "replace",
                                path = "/BillBalanceAtLastPayDay",
                                value = ToAmount
                            };

                            BillUpdate.Add(BillBalanceAtLastPayDay);
                        }

                        await _ds.PatchBill(_vm.ToSelectedMoveBalance.Id, BillUpdate);
                        break;
                    case "Saving":
                        List<PatchDoc> SavingUpdate = new List<PatchDoc>();

                        PatchDoc SavingAmount = new PatchDoc
                        {
                            op = "replace",
                            path = "/CurrentBalance",
                            value = ToAmount
                        };

                        SavingUpdate.Add(SavingAmount);

                        await _ds.PatchSaving(_vm.ToSelectedMoveBalance.Id, SavingUpdate);
                        break;
                    default:
                        break;
                }
            }

            this.Close("OK");
        }
    }

    private void CancelUpdate_Saving(object sender, EventArgs e)
    {
        this.Close("Closed");
    }

    private void SwapFromTo_Tapped(object sender, TappedEventArgs e)
    {
        if (!_vm.IsCoverOverspend)
        {
            if (_vm.ToEnabled)
            {
                var a1 = brdFromTo.RotateTo(0, 600, Easing.Linear);
                Task.WhenAll(a1);

                _vm.ToEnabled = false;
                _vm.FromEnabled = true;

                MoveBalanceClass ToMove = _vm.ToSelectedMoveBalance;
                MoveBalanceClass FromMove = _vm.FromSelectedMoveBalance;

                _vm.FromSelectedMoveBalance = ToMove;
                _vm.ToSelectedMoveBalance = FromMove;

                _vm.RecalculatedBalances();

            }
            else if (_vm.FromEnabled)
            {
                var a1 = brdFromTo.RotateTo(180, 600, Easing.Linear);
                Task.WhenAll(a1);

                _vm.ToEnabled = true;
                _vm.FromEnabled = false;

                MoveBalanceClass ToMove = _vm.ToSelectedMoveBalance;
                MoveBalanceClass FromMove = _vm.FromSelectedMoveBalance;

                _vm.FromSelectedMoveBalance = ToMove;
                _vm.ToSelectedMoveBalance = FromMove;

                _vm.RecalculatedBalances();
            }
        }
    }
}