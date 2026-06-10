using System.Collections.ObjectModel;
using Antelcat.I18N.Avalonia;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Eruru.Debouncer;
using Eruru.FluentAvaloniaTemplate.Models;
using Eruru.FluentAvaloniaTemplate.Resources;
using Eruru.FluentAvaloniaTemplate.Services;
using Eruru.JsonConfig;
using FluentAvalonia.UI.Controls;

namespace Eruru.FluentAvaloniaTemplate.ViewModels;

public partial class SettingsPageViewModel : ViewModelBase, IDisposable {

	[ObservableProperty]
	public partial string? CurrentFluentAvaloniaVersion { get; set; }
	[ObservableProperty]
	public partial string? CurrentAvaloniaVersion { get; set; }
	[ObservableProperty]
	public partial ObservableCollection<KeyValueViewModel> AppLanguages { get; set; } = [];
	[ObservableProperty]
	public partial string AppLanguage { get; set; } = string.Empty;
	[ObservableProperty]
	public partial ObservableCollection<KeyValueViewModel> AppFonts { get; set; } = [];
	[ObservableProperty]
	public partial string AppFont { get; set; } = string.Empty;
	[ObservableProperty]
	public partial ObservableCollection<KeyValueViewModel> AppThemes { get; set; } = [];
	[ObservableProperty]
	public partial string AppTheme { get; set; } = string.Empty;
	[ObservableProperty]
	public partial ObservableCollection<KeyValueViewModel> AppFlowDirections { get; set; } = [];
	[ObservableProperty]
	public partial string AppFlowDirection { get; set; } = string.Empty;
	[ObservableProperty]
	public partial bool IsAppAccentColorExpanded { get; set; }
	[ObservableProperty]
	public partial bool IsUseCustomAccentColor { get; set; }
	[ObservableProperty]
	public partial ObservableCollection<Color> PredefinedAccentColors { get; set; } = [];
	[ObservableProperty]
	public partial Color PredefinedAccentColor { get; set; } = Config.FallbackAccentColor;
	[ObservableProperty]
	public partial Color CustomAccentColor { get; set; } = Config.FallbackAccentColor;
	[ObservableProperty]
	public partial bool IsMinimizeToTrayIcon { get; set; }

	readonly Debouncer<SettingsPageViewModel, Color> Debouncer;
	readonly JsonConfig<Config, App> JsonConfig;
	readonly DialogService DialogService;
	int Counter;
	int State;

	public SettingsPageViewModel (JsonConfig<Config, App> jsonConfig, DialogService dialogService) {
		Interlocked.Increment (ref Counter);
		try {
			JsonConfig = jsonConfig;
			DialogService = dialogService;
			Debouncer = new (TimeSpan.FromMilliseconds (100), this, static (debouncer, exception) => {
				_ = debouncer.Context?.DialogService.ShowExceptionAsync (exception).ContinueWithShowExceptionAsync ();
			});
			CurrentFluentAvaloniaVersion = typeof (FANavigationView).Assembly.GetName ().Version?.ToString ();
			CurrentAvaloniaVersion = typeof (Application).Assembly.GetName ().Version?.ToString ();
			AppLanguages = new (Config.AppLanguages.Select (x => new KeyValueViewModel (x.Key, x.Value)));
			AppFonts = new (Config.AppFonts.Select (x => new KeyValueViewModel (x.Key, x.Value)));
			AppThemes = new (Config.AppThemes.Select (x => new KeyValueViewModel (x.Key, x.Value)));
			AppFlowDirections = new (Config.AppFlowDirections.Select (x => new KeyValueViewModel (x.Key, x.Value)));
			PredefinedAccentColors = new (Config.PredefinedAccentColors);
			JsonConfigOnChanged ();
			JsonConfig.OnChanged += (sender, e) => JsonConfigOnChanged ();
			Dispatcher.UIThread.Post (static state => {
				if (state is not SettingsPageViewModel settingsPageViewModel) {
					return;
				}
				Interlocked.Decrement (ref settingsPageViewModel.Counter);
			}, this);
		} catch {
			Interlocked.Decrement (ref Counter);
			throw;
		}
	}

	protected virtual void Dispose (bool disposing) {
		if (Interlocked.Exchange (ref State, 1) != 0 || !disposing) {
			return;
		}
		Debouncer.Dispose ();
	}

	public void Dispose () {
		Dispose (true);
		GC.SuppressFinalize (this);
	}

	void JsonConfigOnChanged () {
		if (JsonConfig.Read () is not Config value) {
			return;
		}
		Interlocked.Increment (ref Counter);
		try {
			AppLanguage = value.AppLanguage ?? string.Empty;
			AppFont = value.AppFont ?? string.Empty;
			AppTheme = value.AppTheme ?? string.Empty;
			AppFlowDirection = value.AppFlowDirection ?? string.Empty;
			IsAppAccentColorExpanded = value.IsAppAccentColorExpanded ?? false;
			IsUseCustomAccentColor = value.IsUseCustomAccentColor ?? false;
			PredefinedAccentColor = value.PredefinedAccentColor ?? Config.FallbackAccentColor;
			CustomAccentColor = value.CustomAccentColor ?? Config.FallbackAccentColor;
			IsMinimizeToTrayIcon = value.IsMinimizeToTrayIcon ?? true;
			AppFonts[0].Key = I18NExtension.Translate (LangKeys.Default) ?? LangKeys.Default;
		} finally {
			Interlocked.Decrement (ref Counter);
		}
	}

	partial void OnAppLanguageChanged (string value) {
		if (Volatile.Read (ref Counter) > 0) {
			return;
		}
		_ = JsonConfig.TryWriteAsync (
			static (_, value, state) => value.AppLanguage = state, value
		).ContinueWithShowExceptionAsync ();
	}

	partial void OnAppFontChanged (string value) {
		if (Volatile.Read (ref Counter) > 0) {
			return;
		}
		_ = JsonConfig.TryWriteAsync (
			static (_, value, state) => value.AppFont = state, value
		).ContinueWithShowExceptionAsync ();
	}

	partial void OnAppThemeChanged (string value) {
		if (Volatile.Read (ref Counter) > 0) {
			return;
		}
		_ = JsonConfig.TryWriteAsync (
			static (_, value, state) => value.AppTheme = state, value
		).ContinueWithShowExceptionAsync ();
	}

	partial void OnAppFlowDirectionChanged (string value) {
		if (Volatile.Read (ref Counter) > 0) {
			return;
		}
		_ = JsonConfig.TryWriteAsync (
			static (_, value, state) => value.AppFlowDirection = state, value
		).ContinueWithShowExceptionAsync ();
	}

	partial void OnIsAppAccentColorExpandedChanged (bool value) {
		if (Volatile.Read (ref Counter) > 0) {
			return;
		}
		_ = JsonConfig.TryWriteAsync (
			static (_, value, state) => value.IsAppAccentColorExpanded = state, value
		).ContinueWithShowExceptionAsync ();
	}

	partial void OnIsUseCustomAccentColorChanged (bool value) {
		if (Volatile.Read (ref Counter) > 0) {
			return;
		}
		_ = JsonConfig.TryWriteAsync (
			static (_, value, state) => value.IsUseCustomAccentColor = state, value
		).ContinueWithShowExceptionAsync ();
	}

	partial void OnPredefinedAccentColorChanged (Color value) {
		if (Volatile.Read (ref Counter) > 0) {
			return;
		}
		_ = JsonConfig.TryWriteAsync (
			static (_, value, state) => value.PredefinedAccentColor = state, value
		).ContinueWithShowExceptionAsync ();
	}

	partial void OnCustomAccentColorChanged (Color value) {
		if (Volatile.Read (ref Counter) > 0) {
			return;
		}
		Debouncer.Post (static (debouncer, state) => {
			if (debouncer.Context == null) {
				return Task.CompletedTask;
			}
			return debouncer.Context.JsonConfig.TryWriteAsync (
				static (_, value, state) => value.CustomAccentColor = state, state
			);
		}, value);
	}

	partial void OnIsMinimizeToTrayIconChanged (bool value) {
		if (Volatile.Read (ref Counter) > 0) {
			return;
		}
		_ = JsonConfig.TryWriteAsync (
			static (_, value, state) => value.IsMinimizeToTrayIcon = state, value
		).ContinueWithShowExceptionAsync ();
	}

}