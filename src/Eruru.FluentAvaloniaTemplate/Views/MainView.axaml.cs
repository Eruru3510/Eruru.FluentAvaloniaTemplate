using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Eruru.FluentAvaloniaTemplate.ViewModels;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Presentation;
using Presentation.Configs;
using Presentation.Localization;
using Presentation.Navigations;

namespace Eruru.FluentAvaloniaTemplate.Views;

public partial class MainView : UserControl {

	public MainView () {
		DataContext = PresentationApi.Services!.GetRequiredService<MainViewModel> ();
		InitializeComponent ();
	}

	void NavigationView_BackRequested (object? sender, FANavigationViewBackRequestedEventArgs e) {
		PresentationApi.Services!.GetRequiredService<NavigationService> ().Back ();
	}

	void FANavigationView_DisplayModeChanged (object? sender, FANavigationViewDisplayModeChangedEventArgs e) {
		PageContainer.Margin = new (0, e.DisplayMode == FANavigationViewDisplayMode.Minimal ? 44 : 0, 0, 0);
	}

	void FABreadcrumbBar_ItemClicked (FABreadcrumbBar _, FABreadcrumbBarItemClickedEventArgs args) {
		var navigationService = PresentationApi.Services!.GetRequiredService<NavigationService> ();
		navigationService.Navigate (navigationService.BreadcrumbItems[args.Index].Id);
	}

	void FANavigationView_ItemInvoked (object? sender, FANavigationViewItemInvokedEventArgs e) {
		PresentationApi.Services!.GetRequiredService<NavigationService> ().Navigate (
			((NavigationItem)e.InvokedItemContainer.DataContext!).Id
		);
	}

	protected override void OnInitialized () {
		base.OnInitialized ();
		PresentationApi.Services!.GetRequiredService<SettingsViewModel> ();
		var topLevel = PresentationApi.Services!.GetRequiredService<TopLevel> ();
		topLevel.BackRequested += TopLevel_BackRequested;
		topLevel.InsetsManager?.DisplayEdgeToEdgePreference = true;
		var navigationService = PresentationApi.Services!.GetRequiredService<NavigationService> ();
		navigationService.Register (new (
			NavigationIds.Home, LangKeys.HomePage, typeof (HomeView),
			isShowBreadcrumb: false, iconSource: nameof (FASymbol.Home)
		));
		navigationService.Register (new ("Home.Sub", "子主页", typeof (SubHomeView), NavigationIds.Home));
		navigationService.Register (new ("Home.Dynamic", "动态主页", typeof (DynamicHomeView), "Home.Sub"));
		navigationService.Register (new (
			NavigationIds.Settings, LangKeys.Settings, typeof (SettingsView),
			isShowBreadcrumb: false, iconSource: nameof (FASymbol.Settings), isFooterMenuItem: true
		));
	}

	protected override void OnLoaded (RoutedEventArgs e) {
		base.OnLoaded (e);
		_ = LoadedAsync ().OnFaultedShowDialogAsync ();
	}

	void FANavigationView_PaneOpened (FANavigationView sender, EventArgs args) {
		if (sender.DisplayMode == FANavigationViewDisplayMode.Expanded) {
			_ = PresentationApi.Services!.GetRequiredService<JsonConfig<PresentationConfig>> ().WriteAsync (
				static (value, state) => value.IsNavigationExpanded = state, sender.IsPaneOpen
			).OnFaultedShowDialogAsync ();
		}
	}

	void FANavigationView_PaneClosed (FANavigationView sender, EventArgs args) {
		FANavigationView_PaneOpened (sender, args);
	}

	void TopLevel_BackRequested (object? sender, RoutedEventArgs e) {
		var navigationService = PresentationApi.Services!.GetRequiredService<NavigationService> ();
		if (navigationService.CanBack) {
			e.Handled = true;
			navigationService.Back ();
		}
	}

	async Task LoadedAsync (CancellationToken cancellationToken = default) {
		var presentationConfig = await PresentationApi.Services!.GetRequiredService<JsonConfig<PresentationConfig>> ()
			.ReadAsync (cancellationToken).ConfigureAwait (true);
		if (!presentationConfig.IsNavigationExpanded.GetValueOrDefault (true)) {
			NavigationView.IsPaneOpen = true;
			NavigationView.IsPaneOpen = false;
		}
		PresentationApi.Services!.GetRequiredService<NavigationService> ().Navigate (
			string.IsNullOrWhiteSpace (presentationConfig.StartPage) ? NavigationIds.Home : presentationConfig.StartPage
		);
		NavigationView.PaneOpened += FANavigationView_PaneOpened;
		NavigationView.PaneClosed += FANavigationView_PaneClosed;
	}

	protected override void OnSizeChanged (SizeChangedEventArgs e) {
		base.OnSizeChanged (e);
		var topLevel = PresentationApi.Services!.GetRequiredService<TopLevel> ();
		topLevel.InsetsManager?.IsSystemBarVisible = topLevel.Screens?.Primary?.CurrentOrientation switch {
			ScreenOrientation.Landscape or ScreenOrientation.LandscapeFlipped => false,
			_ => true
		};
	}

}