using CommunityToolkit.Maui.Views;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopUpPageSingleInput : Popup
{
	public string _title;
	public string Title
	{
		get => _title;
	}

	public string _description;
	public string Description
	{
		get => _description;
	}

	public string _placeholder;
	public string Placeholder
	{
		get => _placeholder;
	}

	public string _returnData;
	public string ReturnData
	{
		get => _returnData;
		set
		{
			if(_returnData != value)
			{
				_returnData = value;
				OnPropertyChanged();
			}
		}
	}
	
	public bool _returnDataRequired;
	public bool ReturnDataRequired
	{
		get => _returnDataRequired;
		set
		{
			if(_returnDataRequired != value)
			{
				_returnDataRequired = value;
				OnPropertyChanged();
			}
		}
	}


	public PopUpPageSingleInput(string Title, string Description, string Placeholder)
	{
		InitializeComponent();

		this._title = Title;
		this._description = Description;
		this._placeholder = Placeholder;

		this.BindingContext = this;
	}

	private void Close_Popup(object sender, EventArgs e)
	{
		this.Close(ReturnData);
	}
}