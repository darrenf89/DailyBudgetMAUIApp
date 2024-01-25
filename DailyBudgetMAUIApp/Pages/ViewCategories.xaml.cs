using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Maui.Views;
using Syncfusion.Maui.ListView;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Syncfusion.Maui.DataSource;
using Syncfusion.Maui.DataSource.Extensions;
using Syncfusion.Maui.ListView.Helpers;
using System.Globalization;
using System.Collections.Specialized;
using The49.Maui.BottomSheet;
using System.Runtime.CompilerServices;
using Syncfusion.Maui.Charts;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewCategories : ContentPage
{
	public ViewCategories()
	{
		InitializeComponent();
	}

	protected override void OnSizeAllocated(double width, double height) 
  	{ 
		base.OnSizeAllocated(width, height); 
	
		if (width > 0 && pageWidth != width) 
		{       
			var size = Application.Current.MainPage.Width / listView.ItemSize; 
			gridLayout.SpanCount = (int)size; 
			listView.LayoutManager = gridLayout; 
		} 
	}
}