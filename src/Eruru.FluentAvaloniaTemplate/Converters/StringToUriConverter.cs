using System.Globalization;
using Avalonia.Data.Converters;

namespace Eruru.FluentAvaloniaTemplate;

public class StringToUriConverter : IValueConverter {

	public object? Convert (object? value, Type targetType, object? parameter, CultureInfo culture) {
		return new Uri ($"{value}");
	}

	public object? ConvertBack (object? value, Type targetType, object? parameter, CultureInfo culture) {
		return (value as Uri)?.ToString () ?? string.Empty;
	}

}