using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Eruru.FluentAvaloniaTemplate.Consolonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Presentation;
using Presentation.Configs;
using Presentation.Localization;
using Presentation.Navigations;

namespace Eruru.FluentAvaloniaTemplate.Consolonia.Views;

public partial class MainView : UserControl {

	public MainView () {
		DataContext = PresentationApi.Services!.GetRequiredService<MainViewModel> ();
		InitializeComponent ();
	}

	protected override void OnInitialized () {
		base.OnInitialized ();
		PresentationApi.Services!.GetRequiredService<SettingsViewModel> ();
		var navigationService = PresentationApi.Services!.GetRequiredService<NavigationService> ();
		navigationService.Register (new (
			NavigationIds.Home, LangKeys.HomePage, typeof (HomeView),
			isShowBreadcrumb: false, iconSource: Application.Current!.FindResource ("Icon_Home")
		));
		navigationService.Register (new ("Home.Sub", "子主页", typeof (HomeView), NavigationIds.Home));
		navigationService.Register (new ("Home.Dynamic", "动态主页", typeof (HomeView), "Home.Sub"));
		navigationService.Register (new (
			NavigationIds.Settings, LangKeys.Settings, typeof (SettingsView),
			isShowBreadcrumb: false, iconSource: Application.Current!.FindResource ("Icon_Settings"), isFooterMenuItem: true
		));
	}

	protected override void OnLoaded (RoutedEventArgs e) {
		base.OnLoaded (e);
		_ = LoadedAsync ().OnFaultedShowDialogAsync ();
	}

	async Task LoadedAsync (CancellationToken cancellationToken = default) {
		var startPage = await PresentationApi.Services!.GetRequiredService<JsonConfig<PresentationConfig>> ()
			.ReadAsync (static x => x.StartPage, cancellationToken).ConfigureAwait (true);
		PresentationApi.Services!.GetRequiredService<NavigationService> ().Navigate (
			string.IsNullOrWhiteSpace (startPage) ? NavigationIds.Home : startPage
		);
	}

	void NavigationItem_Tapped (object? sender, TappedEventArgs e) {
		PresentationApi.Services!.GetRequiredService<NavigationService> ().Navigate (((NavigationItem)((Control)sender!).Tag!).Id);
	}

	void BackButton_Click (object? sender, RoutedEventArgs e) {
		PresentationApi.Services!.GetRequiredService<NavigationService> ().Back ();
	}

	void BreadcurmbItem_Tapped (object? sender, TappedEventArgs e) {
		PresentationApi.Services!.GetRequiredService<NavigationService> ().Navigate (((NavigationEntry)((Control)sender!).Tag!).Id);
	}

}