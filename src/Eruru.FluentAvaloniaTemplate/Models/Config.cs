using System.Collections.ObjectModel;
using System.Globalization;
using Antelcat.I18N.Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Eruru.FluentAvaloniaTemplate.Resources;
using FluentAvalonia.Styling;

namespace Eruru.FluentAvaloniaTemplate.Models;

public class Config {

	public static Collection<KeyValuePair<string, string>> AppLanguages { get; } = [
		new (I18NExtension.Translate (LangKeys.Default) ?? LangKeys.Default, string.Empty), ..Api.AppLanguages
	];
	public static Collection<KeyValuePair<string, string>> AppFonts { get; } = [.. FontManager.Current.SystemFonts
		.Where (x => {
			if (string.IsNullOrWhiteSpace (x.Name) || x.Name.Length > 128 || x.Name.Any (char.IsControl)){
				return false;
			}
			return x.Name.ToUpperInvariant () switch {
				"EMOJI" or "SYMBOL" or "ICONS" => false,
				_ => true
			};
		})
		.Select (x => KeyValuePair.Create (x.Name, x.Name))
		.Prepend (new (I18NExtension.Translate (LangKeys.Default) ?? LangKeys.Default, string.Empty))];
	public static Collection<KeyValuePair<string, string>> AppThemes { get; } = [
		new (LangKeys.System, string.Empty),
		new (LangKeys.AccentColor_Light, nameof (ThemeVariant.Light)),
		new (LangKeys.AccentColor_Dark, nameof (ThemeVariant.Dark))
	];
	public static Collection<KeyValuePair<string, string>> AppFlowDirections { get; } = [
		new (LangKeys.Default, string.Empty),
		new (LangKeys.LeftToRight, nameof (FlowDirection.LeftToRight)),
		new (LangKeys.RightToLeft, nameof (FlowDirection.RightToLeft))
	];
	public static Collection<Color> PredefinedAccentColors { get; } = [
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
	public static Color FallbackAccentColor { get; } = Color.FromRgb (0, 120, 212);
	public static CultureInfo DefaultCultureInfo { get; } = CultureInfo.CurrentUICulture;
	public static FlowDirection DefaultFlowDirection { get; } = App.Current?.ApplicationLifetime switch {
		IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow?.FlowDirection,
		ISingleViewApplicationLifetime view => view.MainView?.FlowDirection,
		_ => null
	} ?? FlowDirection.LeftToRight;

	public virtual string? AppLanguage {

		get;

		set {
			var isDefault = false;
			if (string.IsNullOrWhiteSpace (value)) {
				value = DefaultCultureInfo.Name;
				isDefault = true;
			}
			var language = FindLanguage (
				[.. AppLanguages.Skip (1).Select (x => new CultureInfo ($"{x.Value}"))], new CultureInfo (value),
				new CultureInfo ("en-US")
			);
			field = isDefault ? string.Empty : language.Name;
			if (App.Current == null) {
				return;
			}
			Dispatcher.UIThread.Invoke (() => {
				CultureInfo.CurrentUICulture = language;
				CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture;
				CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentUICulture;
				CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture;
				I18NExtension.Culture = CultureInfo.CurrentUICulture;
			});
		}

	}
	public virtual string? AppFont {

		get;

		set {
			var isDefault = false;
			FontFamily font;
			if (string.IsNullOrWhiteSpace (value)) {
				font = FontManager.Current.DefaultFontFamily;
				isDefault = true;
			} else {
				font = new FontFamily (value);
				if (!AppFonts.Skip (1).Select (x => new FontFamily ($"{x.Value}")).Contains (font)) {
					font = FontManager.Current.DefaultFontFamily;
					isDefault = true;
				}
			}
			field = isDefault ? string.Empty : font.Name;
			if (App.Current == null) {
				return;
			}
			Dispatcher.UIThread.Invoke (() => App.Current.Resources["ContentControlThemeFontFamily"] = font);
		}

	}
	public virtual string? AppTheme {

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
			field = value;
			if (App.Current == null) {
				return;
			}
			Dispatcher.UIThread.Invoke (() => App.Current.RequestedThemeVariant = themeVariant);
		}

	}
	public virtual string? AppFlowDirection {

		get;

		set {
			field = value;
			FlowDirection flowDirection;
			switch (value) {
				case nameof (FlowDirection.LeftToRight):
					flowDirection = FlowDirection.LeftToRight;
					break;
				case nameof (FlowDirection.RightToLeft):
					flowDirection = FlowDirection.RightToLeft;
					break;
				default:
					field = string.Empty;
					flowDirection = DefaultFlowDirection;
					break;
			}
			if (App.Current == null) {
				return;
			}
			Dispatcher.UIThread.Invoke (() => {
				switch (App.Current.ApplicationLifetime) {
					case IClassicDesktopStyleApplicationLifetime desktop: {
						desktop.MainWindow?.FlowDirection = flowDirection;
						break;
					}
					case ISingleViewApplicationLifetime view: {
						view.MainView?.FlowDirection = flowDirection;
						break;
					}
				}
			});
		}

	}
	public virtual bool? IsAppAccentColorExpanded { get; set; }
	public virtual bool? IsUseCustomAccentColor {

		get;

		set {
			field = value ?? false;
			OnIsUseCustomAccentColorChanged ();
		}

	}
	public virtual Color? PredefinedAccentColor {

		get => _PredefinedAccentColor;

		set {
			_PredefinedAccentColor = value ?? FallbackAccentColor;
			_CustomAccentColor = PredefinedAccentColor;
			OnIsUseCustomAccentColorChanged ();
		}

	}
	public virtual Color? CustomAccentColor {

		get => _CustomAccentColor;

		set {
			_CustomAccentColor = value ?? FallbackAccentColor;
			_PredefinedAccentColor = CustomAccentColor;
			OnIsUseCustomAccentColorChanged ();
		}

	}
	public virtual bool? IsMinimizeToTrayIcon { get; set; }
	public virtual double? WindowWidth { get; set; }
	public virtual double? WindowHeight { get; set; }
	public virtual WindowState? WindowState { get; set => field = value ?? Avalonia.Controls.WindowState.Normal; }

	Color? _PredefinedAccentColor;
	Color? _CustomAccentColor;

	public void Apply () {
#pragma warning disable CA2245 // 请勿将属性分配给其自身
		AppLanguage = AppLanguage;
		AppFont = AppFont;
		AppTheme = AppTheme;
		AppFlowDirection = AppFlowDirection;
		IsAppAccentColorExpanded = IsAppAccentColorExpanded;
		IsUseCustomAccentColor = IsUseCustomAccentColor;
		PredefinedAccentColor = PredefinedAccentColor;
		CustomAccentColor = CustomAccentColor;
		IsMinimizeToTrayIcon = IsMinimizeToTrayIcon;
		WindowWidth = WindowWidth;
		WindowHeight = WindowHeight;
		WindowState = WindowState;
#pragma warning restore CA2245 // 请勿将属性分配给其自身
	}

	void OnIsUseCustomAccentColorChanged () {
		if (App.Current == null) {
			return;
		}
		Dispatcher.UIThread.Invoke (() => {
			if (App.Current.Styles[0] is not FluentAvaloniaTheme fluentAvaloniaTheme) {
				return;
			}
			if (IsUseCustomAccentColor ?? false) {
				fluentAvaloniaTheme.CustomAccentColor = PredefinedAccentColor ?? CustomAccentColor ?? FallbackAccentColor;
				return;
			}
			if (OperatingSystem.IsWindowsVersionAtLeast (10, 0, 22000)) {
				fluentAvaloniaTheme.CustomAccentColor = null;
				return;
			}
			fluentAvaloniaTheme.CustomAccentColor = GetSystemAccentColor (fluentAvaloniaTheme, FallbackAccentColor);
		});
	}

	static CultureInfo FindLanguage (IList<CultureInfo> languages, CultureInfo language, CultureInfo fallbackLanguage) {
		if (languages.Contains (language)) {
			return language;
		}
		return languages.FirstOrDefault (x => x.TwoLetterISOLanguageName == language.TwoLetterISOLanguageName, fallbackLanguage);
	}

	static Color GetSystemAccentColor (FluentAvaloniaTheme fluentAvaloniaTheme, Color fallbackColor) {
		var customAccentColor = fluentAvaloniaTheme.CustomAccentColor;
		try {
			fluentAvaloniaTheme.CustomAccentColor = null;
			if (fluentAvaloniaTheme.TryGetResource ("SystemAccentColor", App.Current?.ActualThemeVariant, out var value)
				&& value is Color color
			) {
				return color;
			}
			return fallbackColor;
		} finally {
			fluentAvaloniaTheme.CustomAccentColor = customAccentColor;
		}
	}

}