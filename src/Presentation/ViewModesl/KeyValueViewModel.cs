using CommunityToolkit.Mvvm.ComponentModel;

namespace Presentation.ViewModels;

public partial class KeyValueViewModel (object key, object? value) : ObservableObject {

	[ObservableProperty]
	public partial object Key { get; set; } = key;
	[ObservableProperty]
	public partial object? Value { get; set; } = value;

}