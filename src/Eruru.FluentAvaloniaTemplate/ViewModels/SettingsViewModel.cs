using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Antelcat.I18N.Avalonia;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using Presentation;
using Presentation.Configs;
using Presentation.Localization;
using Presentation.Mapper;
using Presentation.Navigations;
using Presentation.ViewModels;

namespace Eruru.FluentAvaloniaTemplate.ViewModels;

public partial class SettingsViewModel : ViewModelBase, IDisposable {

	public string? CurrentAvaloniaVersion { get; } = typeof (Application).Assembly.GetName ().Version?.ToString ();
	public string? CurrentFluentAvaloniaVersion { get; } = typeof (FluentAvaloniaTheme).Assembly.GetName ().Version?.ToString ();
	[ObservableProperty]
	public partial ObservableCollection<KeyValueViewModel> AppLanguages { get; set; } = [
		new (LangKeys.Default, string.Empty),
		.. PresentationApi.AppLanguages.Select (static x => new KeyValueViewModel (x.Key, x.Value))
	];
	public string? AppLanguage {

		get;

		set {
			var isDefault = false;
			if (string.IsNullOrWhiteSpace (value)) {
				value = DefaultCultureInfo.Name;
				isDefault = true;
			}
			var language = new CultureInfo (value);
			var languages = AppLanguages.Skip (1).Select (static x => new CultureInfo ($"{x.Value}")).ToArray ();
			if (!languages.Contains (language)) {
				language = languages.FirstOrDefault (
					x => x.TwoLetterISOLanguageName == language.TwoLetterISOLanguageName, new CultureInfo ("en-US")
				);
			}
			SetProperty (ref field, isDefault ? string.Empty : language.Name);
			CultureInfo.CurrentUICulture = language;
			CultureInfo.CurrentCulture = language;
			CultureInfo.DefaultThreadCurrentCulture = language;
			CultureInfo.DefaultThreadCurrentUICulture = language;
			I18NExtension.Culture = language;
		}

	}
	[ObservableProperty]
	public partial ObservableCollection<KeyValueViewModel> AppFonts { get; set; } = [
		.. FontManager.Current.SystemFonts
			.Where (static x => {
				if (string.IsNullOrWhiteSpace (x.Name) || x.Name.Length > 128 || x.Name.Any (char.IsControl)){
					return false;
				}
				return x.Name.ToUpperInvariant () switch {
					"EMOJI" or "SYMBOL" or "ICONS" => false,
					_ => true
				};
			})
			.Select (static x => new KeyValueViewModel (x.Name, x.Name))
			.Prepend (new (LangKeys.Default, string.Empty))
	];
	public string? AppFont {

		get;

		set {
			var isDefault = false;
			FontFamily font;
			if (string.IsNullOrWhiteSpace (value)) {
				font = FontManager.Current.DefaultFontFamily;
				isDefault = true;
			} else {
				font = new FontFamily (value);
				if (!AppFonts.Skip (1).Select (static x => $"{x.Value}").Contains (font.Name)) {
					font = FontManager.Current.DefaultFontFamily;
					isDefault = true;
				}
			}
			SetProperty (ref field, isDefault ? string.Empty : font.Name);
			Application.Current!.Resources["ContentControlThemeFontFamily"] = font;
		}

	}
	[ObservableProperty]
	public partial ObservableCollection<KeyValueViewModel> AppThemes { get; set; } = [
		new (LangKeys.System, string.Empty),
		new (LangKeys.AccentColorLight, nameof (ThemeVariant.Light)),
		new (LangKeys.AccentColorDark, nameof (ThemeVariant.Dark))
	];
	public string? AppTheme {

		get;

		set {
			ThemeVariant themeVariant;
			switch (value) {
				case nameof (ThemeVariant.Light):
					themeVariant = ThemeVariant.Light;
					break;
				case nameof (ThemeVariant.Dark):
					themeVariant = ThemeVariant.Dark;
					break;
				default:
					value = string.Empty;
					themeVariant = ThemeVariant.Default;
					break;
			}
			SetProperty (ref field, value);
			Application.Current!.RequestedThemeVariant = themeVariant;
		}

	}
	[ObservableProperty]
	public partial ObservableCollection<KeyValueViewModel> AppFlowDirections { get; set; } = [
		new (LangKeys.Default, string.Empty),
		new (LangKeys.LeftToRight, nameof (FlowDirection.LeftToRight)),
		new (LangKeys.RightToLeft, nameof (FlowDirection.RightToLeft))
	];
	public string? AppFlowDirection {

		get;

		set {
			FlowDirection flowDirection;
			switch (value) {
				case nameof (FlowDirection.LeftToRight):
					flowDirection = FlowDirection.LeftToRight;
					break;
				case nameof (FlowDirection.RightToLeft):
					flowDirection = FlowDirection.RightToLeft;
					break;
				default:
					value = string.Empty;
					flowDirection = DefaultFlowDirection;
					break;
			}
			SetProperty (ref field, value);
			switch (Application.Current!.ApplicationLifetime) {
				case IClassicDesktopStyleApplicationLifetime desktop: {
					desktop.MainWindow!.FlowDirection = flowDirection;
					break;
				}
				case IActivityApplicationLifetime activity: {
					activity.MainViewFactory! ().FlowDirection = flowDirection;
					break;
				}
				case ISingleViewApplicationLifetime view: {
					view.MainView!.FlowDirection = flowDirection;
					break;
				}
				default:
					throw new NotImplementedException ($"{Application.Current!.ApplicationLifetime}");
			}
		}

	}
	public bool? IsAppAccentColorExpanded { get; set => SetProperty (ref field, value ?? false); }
	public bool? IsUseCustomAccentColor {

		get;

		set {
			SetProperty (ref field, value ?? false);
			OnIsUseCustomAccentColorChanged ();
		}

	}
	[ObservableProperty]
	public partial ObservableCollection<Color> PredefinedAccentColors { get; set; } = [
		Color.FromRgb(255, 185, 0),
		Color.FromRgb(255, 140, 0),
		Color.FromRgb(247, 99, 12),
		Color.FromRgb(202, 80, 16),
		Color.FromRgb(218, 59, 1),
		Color.FromRgb(239, 105, 80),
		Color.FromRgb(209, 52, 56),
		Color.FromRgb(255, 67, 67),
		Color.FromRgb(231, 72, 86),
		Color.FromRgb(232, 17, 35),
		Color.FromRgb(234, 0, 94),
		Color.FromRgb(195, 0, 82),
		Color.FromRgb(227, 0, 140),
		Color.FromRgb(191, 0, 119),
		Color.FromRgb(194, 57, 179),
		Color.FromRgb(154, 0, 137),
		Color.FromRgb(0, 120, 212),
		Color.FromRgb(0, 99, 177),
		Color.FromRgb(142, 140, 216),
		Color.FromRgb(107, 105, 214),
		Color.FromRgb(135, 100, 184),
		Color.FromRgb(116, 77, 169),
		Color.FromRgb(177, 70, 194),
		Color.FromRgb(136, 23, 152),
		Color.FromRgb(0, 153, 188),
		Color.FromRgb(45, 125, 154),
		Color.FromRgb(0, 183, 195),
		Color.FromRgb(3, 131, 135),
		Color.FromRgb(0, 178, 148),
		Color.FromRgb(1, 133, 116),
		Color.FromRgb(0, 204, 106),
		Color.FromRgb(16, 137, 62),
		Color.FromRgb(122, 117, 116),
		Color.FromRgb(93, 90, 88),
		Color.FromRgb(104, 118, 138),
		Color.FromRgb(81, 92, 107),
		Color.FromRgb(86, 124, 115),
		Color.FromRgb(72, 104, 96),
		Color.FromRgb(73, 130, 5),
		Color.FromRgb(16, 124, 16),
		Color.FromRgb(118, 118, 118),
		Color.FromRgb(76, 74, 72),
		Color.FromRgb(105, 121, 126),
		Color.FromRgb(74, 84, 89),
		Color.FromRgb(100, 124, 100),
		Color.FromRgb(82, 94, 84),
		Color.FromRgb(132, 117, 69),
		Color.FromRgb(126, 115, 95)
	];
	public Color? PredefinedAccentColor {

		get => _PredefinedAccentColor;

		set {
			SetProperty (ref _PredefinedAccentColor, value ?? FallbackAccentColor);
			_CustomAccentColor = _PredefinedAccentColor;
			OnIsUseCustomAccentColorChanged ();
			OnPropertyChanged (nameof (CustomAccentColor));
		}

	}
	public Color? CustomAccentColor {

		get => _CustomAccentColor;

		set {
			SetProperty (ref _CustomAccentColor, value ?? FallbackAccentColor);
			_PredefinedAccentColor = _CustomAccentColor;
			OnIsUseCustomAccentColorChanged ();
			OnPropertyChanged (nameof (PredefinedAccentColor));
		}

	}
	[ObservableProperty]
	public partial ObservableCollection<KeyValueViewModel> CloseWindowBehaviours { get; set; } = [
		new (LangKeys.Ask, string.Empty),
		new (LangKeys.MinimizeToTray, nameof (Presentation.Configs.CloseWindowBehaviour.MinimizeToTray)),
		new (LangKeys.Quit, nameof (Presentation.Configs.CloseWindowBehaviour.Quit))
	];
	public string? CloseWindowBehaviour {

		get;

		set {
			switch (value) {
				case nameof (Presentation.Configs.CloseWindowBehaviour.MinimizeToTray):
				case nameof (Presentation.Configs.CloseWindowBehaviour.Quit):
					break;
				default:
					value = string.Empty;
					break;
			}
			SetProperty (ref field, value);
		}

	}
	public bool? IsAboutExpanded { get; set => SetProperty (ref field, value ?? false); }
	[ObservableProperty]
	public partial ObservableCollection<KeyValueViewModel> StartPages { get; set; } = [
		new (LangKeys.HomePage, string.Empty),
		new (LangKeys.Settings, NavigationIds.Settings)
	];
	public string? StartPage {

		get;

		set {
			switch (value) {
				case NavigationIds.Settings:
					break;
				default:
					value = string.Empty;
					break;
			}
			SetProperty (ref field, value);
		}

	}

	Color? _PredefinedAccentColor;
	Color? _CustomAccentColor;
	readonly Timer SaveDebouncer;
	int DisposeState;

	static readonly Color FallbackAccentColor = Color.FromRgb (0, 120, 212);
	static readonly CultureInfo DefaultCultureInfo = CultureInfo.CurrentUICulture;
	static readonly FlowDirection DefaultFlowDirection = Application.Current!.ApplicationLifetime switch {
		IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow!.FlowDirection,
		IActivityApplicationLifetime activity => activity.MainViewFactory! ().FlowDirection,
		ISingleViewApplicationLifetime view => view.MainView!.FlowDirection,
		_ => FlowDirection.LeftToRight
	};

	static SettingsViewModel () {

	}

	public SettingsViewModel () {
		SaveDebouncer = new (
			static state => _ = ((SettingsViewModel)state!).SaveAsync ().OnFaultedShowDialogAsync (),
			this, Timeout.Infinite, Timeout.Infinite
		);
		_ = LoadAsync ().OnFaultedShowDialogAsync ();
	}

	protected virtual void Dispose (bool disposing) {
		if (Interlocked.Exchange (ref DisposeState, 1) != 0 || !disposing) {
			return;
		}
		SaveDebouncer.Dispose ();
	}
	public void Dispose () {
		Dispose (true);
		GC.SuppressFinalize (this);
	}

	~SettingsViewModel () {
		Dispose (false);
	}

	async Task LoadAsync (CancellationToken cancellationToken = default) {
		var presentationConfig = await PresentationApi.Services!.GetRequiredService<JsonConfig<PresentationConfig>> ()
			.ReadAsync (cancellationToken).ConfigureAwait (true);
		var presentationMapper = PresentationApi.Services!.GetRequiredService<PresentationMapper> ();
		AppLanguage = presentationConfig.AppLanguage;
		AppFont = presentationConfig.AppFont;
		AppTheme = presentationConfig.AppTheme;
		AppFlowDirection = presentationConfig.AppFlowDirection;
		IsAppAccentColorExpanded = presentationConfig.IsAppAccentColorExpanded;
		IsUseCustomAccentColor = presentationConfig.IsUseCustomAccentColor;
		CustomAccentColor = presentationConfig.CustomAccentColor == null
			? null : presentationMapper.ToColor (presentationConfig.CustomAccentColor);
		StartPage = presentationConfig.StartPage;
		CloseWindowBehaviour = presentationConfig.CloseWindowBehaviour;
		IsAboutExpanded = presentationConfig.IsAboutExpanded;
		PropertyChanged += SettingsViewModel_PropertyChanged;
	}

	[RelayCommand]
	Task<bool> OpenFluentAvaloniaUrlAsync (CancellationToken cancellationToken = default) {
		return PresentationApi.Services!.GetRequiredService<ILauncher> ().LaunchUriAsync (new ("https://github.com/amwx/FluentAvalonia"));
	}

	void OnIsUseCustomAccentColorChanged () {
		var fluentAvaloniaTheme = (FluentAvaloniaTheme)Application.Current!.Styles[0];
		if (IsUseCustomAccentColor ?? false) {
			fluentAvaloniaTheme.CustomAccentColor = PredefinedAccentColor ?? CustomAccentColor ?? FallbackAccentColor;
			return;
		}
		if (OperatingSystem.IsWindowsVersionAtLeast (10, 0, 10240)) {
			fluentAvaloniaTheme.CustomAccentColor = null;
			return;
		}
		fluentAvaloniaTheme.CustomAccentColor = GetSystemAccentColor (fluentAvaloniaTheme, FallbackAccentColor);
	}

	static Color GetSystemAccentColor (FluentAvaloniaTheme fluentAvaloniaTheme, Color fallbackColor) {
		var customAccentColor = fluentAvaloniaTheme.CustomAccentColor;
		try {
			fluentAvaloniaTheme.CustomAccentColor = null;
			if (fluentAvaloniaTheme.TryGetResource ("SystemAccentColor", Application.Current!.ActualThemeVariant, out var value)
				&& value is Color color
			) {
				return color;
			}
			return fallbackColor;
		} finally {
			fluentAvaloniaTheme.CustomAccentColor = customAccentColor;
		}
	}

	void SettingsViewModel_PropertyChanged (object? sender, PropertyChangedEventArgs e) {
		if (e.PropertyName == nameof (CustomAccentColor)) {
			SaveDebouncer.Change (TimeSpan.FromMilliseconds (1000), Timeout.InfiniteTimeSpan);
			return;
		}
		SaveDebouncer.Change (Timeout.Infinite, Timeout.Infinite);
		_ = SaveAsync ().OnFaultedShowDialogAsync ();
	}

	Task SaveAsync (CancellationToken cancellationToken = default) {
		return PresentationApi.Services!.GetRequiredService<JsonConfig<PresentationConfig>> ().WriteAsync (static (value, state) => {
			var presentationMapper = PresentationApi.Services!.GetRequiredService<PresentationMapper> ();
			value.AppLanguage = state.AppLanguage;
			value.AppFont = state.AppFont;
			value.AppTheme = state.AppTheme;
			value.AppFlowDirection = state.AppFlowDirection;
			value.IsAppAccentColorExpanded = state.IsAppAccentColorExpanded;
			value.IsUseCustomAccentColor = state.IsUseCustomAccentColor;
			value.CustomAccentColor = state.CustomAccentColor == null
				? null : presentationMapper.ToColorConfig (state.CustomAccentColor.Value);
			value.StartPage = state.StartPage;
			value.CloseWindowBehaviour = state.CloseWindowBehaviour;
			value.IsAboutExpanded = state.IsAboutExpanded;
		}, this, cancellationToken);
	}

}