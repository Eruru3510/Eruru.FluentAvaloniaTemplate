using CommunityToolkit.Mvvm.ComponentModel;
using Eruru.FluentAvaloniaTemplate.Resources;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Eruru.FluentAvaloniaTemplate.ViewModels {

	public partial class MainViewModel : ViewModelBase {

		[ObservableProperty]
		public partial NavigationItemViewModel? Page { get; set; }
		[ObservableProperty]
		public partial NavigationViewModel? NavigationView { get; set; } = new () {
			Items = [new (
				LangKeys.HomePage, typeof (HomePageView),
				static state => (state as ServiceProvider)?.GetRequiredService<HomePageViewModel> (),
				App.ServiceProvider, new FASymbolIconSource () { Symbol = FASymbol.Home }
			)],
			FooterItems = [new (
				LangKeys.Settings, typeof (SettingsPageView),
				static state => (state as ServiceProvider)?.GetRequiredService<SettingsPageViewModel> (),
				App.ServiceProvider, new FASymbolIconSource () { Symbol = FASymbol.Settings }
			)]
		};

		public MainViewModel () {
			Page = NavigationView.Items[0];
		}

	}

}