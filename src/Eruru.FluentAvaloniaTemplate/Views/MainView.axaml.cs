using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Eruru.FluentAvaloniaTemplate.ViewModels;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace Eruru.FluentAvaloniaTemplate.Views {

	public partial class MainView : UserControl {

		int Counter;

		public MainView () {
			InitializeComponent ();
			Frame.NavigationPageFactory = App.ServiceProvider?.GetRequiredService<NavigationPageFactory> ();
			DataContext = App.ServiceProvider?.GetRequiredService<MainViewModel> ();
		}

		protected override void OnLoaded (RoutedEventArgs e) {
			base.OnLoaded (e);
			if (!OperatingSystem.IsAndroid ()) {
				return;
			}
			TopLevel.GetTopLevel (this)?.InsetsManager?.DisplayEdgeToEdgePreference = true;
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

		void NavigationView_BackRequested (object? sender, FANavigationViewBackRequestedEventArgs e) {
			Frame.GoBack ();
		}

		void NavigationView_DisplayModeChanged (object? sender, FANavigationViewDisplayModeChangedEventArgs e) {
			Frame.Padding = e.DisplayMode == FANavigationViewDisplayMode.Minimal ? new Thickness (0, 40, 0, 0) : new Thickness ();
		}

		void NavigationView_SelectionChanged (object? sender, FANavigationViewSelectionChangedEventArgs e) {
			if (Volatile.Read (ref Counter) > 0 || e.SelectedItem is not NavigationItemViewModel navigationItemViewModel) {
				return;
			}
			Frame.Navigate (navigationItemViewModel.ViewType, navigationItemViewModel);
		}

		void Frame_Navigated (object sender, FANavigationEventArgs e) {
			if (e.Parameter is not NavigationItemViewModel navigationItemViewModel) {
				return;
			}
			Interlocked.Increment (ref Counter);
			try {
				NavigationView.SelectedItem = navigationItemViewModel;
			} finally {
				Interlocked.Decrement (ref Counter);
			}
			if (e.Content is not Control control) {
				return;
			}
			navigationItemViewModel.Initialize ();
			control.DataContext = navigationItemViewModel.DataContext;
		}

	}

}