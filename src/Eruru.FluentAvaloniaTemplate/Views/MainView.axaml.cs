using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Eruru.FluentAvaloniaTemplate.Models;
using Eruru.FluentAvaloniaTemplate.Services;
using Eruru.FluentAvaloniaTemplate.ViewModels;
using Eruru.JsonConfig;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace Eruru.FluentAvaloniaTemplate.Views;

public partial class MainView : UserControl {

	readonly JsonConfig<Config, App>? JsonConfig;
	readonly DialogService? DialogService;
	NavigationItemViewModel? NavigationItemViewModel;
	int IsPaneOpenCounter;

	public MainView () {
		Interlocked.Increment (ref IsPaneOpenCounter);
		try {
			InitializeComponent ();
			JsonConfig = App.ServiceProvider?.GetRequiredService<JsonConfig<Config, App>> ();
			DialogService = App.ServiceProvider?.GetRequiredService<DialogService> ();
			var navigationPageFactory = App.ServiceProvider?.GetRequiredService<NavigationService> ();
			navigationPageFactory?.Frame = Frame;
			Frame.NavigationPageFactory = navigationPageFactory;
			var viewModel = App.ServiceProvider?.GetRequiredService<MainViewModel> ();
			DataContext = viewModel;
			if (viewModel?.Page == null) {
				return;
			}
			Frame.Navigate (viewModel.Page.ViewType, viewModel.Page);
		} catch {
			Interlocked.Decrement (ref IsPaneOpenCounter);
			throw;
		}
	}

	protected override void OnLoaded (RoutedEventArgs e) {
		try {
			base.OnLoaded (e);
			if (TopLevel.GetTopLevel (this) is not TopLevel topLevel) {
				return;
			}
			if (DialogService != null) {
				DialogService.StorageProvider = topLevel.StorageProvider;
				DialogService.Launcher = topLevel.Launcher;
			}
			if (!OperatingSystem.IsAndroid ()) {
				return;
			}
			topLevel.InsetsManager?.DisplayEdgeToEdgePreference = true;
		} finally {
			Interlocked.Decrement (ref IsPaneOpenCounter);
		}
	}

	protected override void OnSizeChanged (SizeChangedEventArgs e) {
		base.OnSizeChanged (e);
		if (!OperatingSystem.IsAndroid () || TopLevel.GetTopLevel (this) is not TopLevel topLevel) {
			return;
		}
		topLevel.InsetsManager?.IsSystemBarVisible = topLevel.Screens?.Primary?.CurrentOrientation switch {
			ScreenOrientation.Landscape or ScreenOrientation.LandscapeFlipped => false,
			_ => true
		};
	}

	void NavigationView_Loaded (object? sender, RoutedEventArgs e) {
		if (JsonConfig?.Read ()?.IsNavigationViewExpanded ?? true) {
			return;
		}
		Interlocked.Increment (ref IsPaneOpenCounter);
		try {
			NavigationView.IsPaneOpen = true;
			NavigationView.IsPaneOpen = false;
		} finally {
			Interlocked.Decrement (ref IsPaneOpenCounter);
		}
	}

	void NavigationView_PaneOpened (FANavigationView sender, EventArgs _) {
		if (Volatile.Read (ref IsPaneOpenCounter) > 0 || sender.DisplayMode != FANavigationViewDisplayMode.Expanded) {
			return;
		}
		var task = JsonConfig?.TryWriteAsync (static (jsonConfig, value, state) => {
			value.IsNavigationViewExpanded = state;
		}, NavigationView.IsPaneOpen).ContinueWithShowExceptionAsync ();
	}

	internal void NavigationView_PaneClosed (FANavigationView sender, EventArgs e) {
		NavigationView_PaneOpened (sender, e);
	}

	void NavigationView_BackRequested (object? sender, FANavigationViewBackRequestedEventArgs e) {
		Frame.GoBack ();
	}

	void NavigationView_DisplayModeChanged (object? sender, FANavigationViewDisplayModeChangedEventArgs e) {
		Frame.Padding = e.DisplayMode == FANavigationViewDisplayMode.Minimal ? new Thickness (0, 40, 0, 0) : new Thickness ();
	}

	void NavigationView_ItemInvoked (object? sender, FANavigationViewItemInvokedEventArgs e) {
		if (NavigationView.SelectedItem is not NavigationItemViewModel navigationItemViewModel
			|| navigationItemViewModel == NavigationItemViewModel
		) {
			return;
		}
		Frame.Navigate (navigationItemViewModel.ViewType, navigationItemViewModel);
	}

	void Frame_Navigated (object sender, FANavigationEventArgs e) {
		if (e.Parameter is not NavigationItemViewModel navigationItemViewModel || e.Content is not Control control) {
			return;
		}
		if (e.NavigationMode == FANavigationMode.New) {
			navigationItemViewModel.Parent = DataContext is not MainViewModel viewModel
				|| viewModel.NavigationView.Contains (navigationItemViewModel)
				? null : NavigationItemViewModel;
		}
		NavigationItemViewModel = navigationItemViewModel;
		NavigationView.SelectedItem = navigationItemViewModel.GetRoot ();
		navigationItemViewModel.Initialize ();
		control.DataContext = navigationItemViewModel.DataContext;
		if (e.Source is Control sourceControl) {
			if (sourceControl is INavigationPage sourceControlNavigationViewPage) {
				sourceControlNavigationViewPage.OnHide ();
			}
			if (sourceControl.DataContext is INavigationPage sourceDataContextNavigationViewPage) {
				sourceDataContextNavigationViewPage.OnHide ();
			}
		}
		if (control is INavigationPage navigationViewPage) {
			navigationViewPage.OnShow ();
		}
		if (control.DataContext is INavigationPage dataContextNavigationViewPage) {
			dataContextNavigationViewPage.OnShow ();
		}
	}

}