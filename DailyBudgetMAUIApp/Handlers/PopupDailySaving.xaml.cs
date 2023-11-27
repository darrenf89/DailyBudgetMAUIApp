using CommunityToolkit.Maui.Views;
using System.ComponentModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Models;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopupDailySaving : Popup
{
	public PopupDailySaving(ref Savings Saving)
	{
		InitializeComponent();
	}
}