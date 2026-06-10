using CommunityToolkit.Mvvm.ComponentModel;

namespace Eruru.FluentAvaloniaTemplate.ViewModels;

public partial class KeyValueViewModel (object key, object value) : ViewModelBase {

	[ObservableProperty]
	public partial object Key { get; set; } = key;
	[ObservableProperty]
	public partial object Value { get; set; } = value;

}