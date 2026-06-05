using Antelcat.I18N.Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Threading;
using Eruru.FluentAvaloniaTemplate.Resources;
using FluentAvalonia.UI.Controls;

namespace Eruru.FluentAvaloniaTemplate.Services {

	public class DialogService {

		public Task ShowExceptionAsync (Exception exception, string? title = null) {
			return Dispatcher.UIThread.InvokeAsync (async () => {
				var contentDialog = new FAContentDialog () {
					Title = CreateTitle (title),
					Content = new SelectableTextBlock () { Text = $"{exception}", TextWrapping = TextWrapping.Wrap },
					PrimaryButtonText = I18NExtension.Translate (LangKeys.Ok)
				};
				await contentDialog.ShowAsync ().ConfigureAwait (false);
			});
		}

		public Task ShowMessageAsync (string content, string? title = null) {
			return Dispatcher.UIThread.InvokeAsync (async () => {
				var contentDialog = new FAContentDialog () {
					Title = CreateTitle (title),
					Content = new SelectableTextBlock () { Text = $"{content}", TextWrapping = TextWrapping.Wrap },
					PrimaryButtonText = I18NExtension.Translate (LangKeys.Ok)
				};
				await contentDialog.ShowAsync ().ConfigureAwait (false);
			});
		}

#pragma warning disable CA1822 // 将成员标记为 static
		TextBlock? CreateTitle (string? title = null) {
#pragma warning restore CA1822 // 将成员标记为 static
			if (string.IsNullOrEmpty (title)) {
				return null;
			}
			var textBlock = new TextBlock () { Text = title };
			textBlock.Bind (TextBlock.FontFamilyProperty, new DynamicResourceExtension ("ContentControlThemeFontFamily"));
			return textBlock;
		}

	}

}