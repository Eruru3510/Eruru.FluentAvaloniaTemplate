using Android.Content.PM;
using Avalonia.Android;

namespace Eruru.FluentAvaloniaTemplate.Android;

[Activity (
	Label = "@string/app_name",
	Theme = "@style/MyTheme.NoActionBar",
	Icon = "@drawable/icon",
	MainLauncher = true,
	ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity {

}