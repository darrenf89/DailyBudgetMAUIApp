using System.Transactions;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;

namespace DailyBudgetMAUIApp.Pages;

public partial class SelectPayeePage : ContentPage
{
	private readonly IRestDataService _ds;
	private readonly IProductTools _pt;
	public SelectPayeePage(int BudgetID, Transactions Transaction, IRestDataService ds, IProductTools pt)
	{
		_ds = ds;
		_pt = pt;

		InitializeComponent();
	}
}