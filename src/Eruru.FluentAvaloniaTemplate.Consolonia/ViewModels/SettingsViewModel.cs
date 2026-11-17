using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Antelcat.I18N.Avalonia;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using Consolonia.Themes;
using Microsoft.Extensions.DependencyInjection;
using Presentation;
using Presentation.Configs;
using Presentation.Localization;
using Presentation.Mapper;
using Presentation.Navigations;
using Presentation.ViewModels;

namespace Eruru.FluentAvaloniaTemplate.Consolonia.ViewModels;

public partial class SettingsViewModel : ObservableObject {

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
				if (string.IsNullOrWhiteSpace (x.Name) || x.Name.Length > 128 || x.Name.Any (char.IsControl)) {
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
	public partial ObservableCollection<KeyValueViewModel> AppStyles { get; set; } = [
		new (LangKeys.ModernTheme, string.Empty),
		new (LangKeys.ModernContrastTheme, nameof (ModernContrastTheme)),
		new (LangKeys.TurboVisionTheme, nameof (TurboVisionTheme)),
		new (LangKeys.TurboVisionCompatibleTheme, nameof (TurboVisionCompatibleTheme)),
		new (LangKeys.TurboVisionGrayTheme, nameof (TurboVisionGrayTheme)),
		new (LangKeys.TurboVisionElegantTheme, nameof (TurboVisionElegantTheme)),
	];
	public string? AppStyle {

		get;

		set {
			switch (value) {
				case nameof (ModernContrastTheme):
					Application.Current!.Styles[1] = new ModernContrastTheme ();
					break;
				case nameof (TurboVisionTheme):
					Application.Current!.Styles[1] = new TurboVisionTheme ();
					break;
				case nameof (TurboVisionCompatibleTheme):
					Application.Current!.Styles[1] = new TurboVisionCompatibleTheme ();
					break;
				case nameof (TurboVisionGrayTheme):
					Application.Current!.Styles[1] = new TurboVisionGrayTheme ();
					break;
				case nameof (TurboVisionElegantTheme):
					Application.Current!.Styles[1] = new TurboVisionElegantTheme ();
					break;
				default:
				case nameof (ModernTheme):
					value = string.Empty;
					Application.Current!.Styles[1] = new ModernTheme ();
					break;
			}
			field = value;
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

	static readonly CultureInfo DefaultCultureInfo = CultureInfo.CurrentUICulture;

	static SettingsViewModel () {

	}

	public SettingsViewModel () {
		_ = LoadAsync ().OnFaultedShowDialogAsync ();
	}

	async Task LoadAsync (CancellationToken cancellationToken = default) {
		var presentationConfig = await PresentationApi.Services!.GetRequiredService<JsonConfig<PresentationConfig>> ()
			.ReadAsync (cancellationToken).ConfigureAwait (true);
		var presentationMapper = PresentationApi.Services!.GetRequiredService<PresentationMapper> ();
		AppLanguage = presentationConfig.AppLanguage;
		AppFont = presentationConfig.AppFont;
		AppTheme = presentationConfig.AppTheme;
		AppStyle = presentationConfig.AppStyle;
		StartPage = presentationConfig.StartPage;
		PropertyChanged += SettingsViewModel_PropertyChanged;
	}

	void SettingsViewModel_PropertyChanged (object? sender, PropertyChangedEventArgs e) {
		_ = PresentationApi.Services!.GetRequiredService<JsonConfig<PresentationConfig>> ().WriteAsync (static (value, state) => {
			var presentationMapper = PresentationApi.Services!.GetRequiredService<PresentationMapper> ();
			value.AppLanguage = state.AppLanguage;
			value.AppFont = state.AppFont;
			value.AppTheme = state.AppTheme;
			value.AppStyle = state.AppStyle;
			value.StartPage = state.StartPage;
		}, this).OnFaultedShowDialogAsync ();
	}

}