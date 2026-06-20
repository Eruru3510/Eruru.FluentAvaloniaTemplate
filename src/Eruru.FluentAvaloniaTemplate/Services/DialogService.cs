using System.Runtime.ExceptionServices;
using Antelcat.I18N.Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Eruru.FluentAvaloniaTemplate.Resources;
using FluentAvalonia.UI.Controls;

namespace Eruru.FluentAvaloniaTemplate.Services;

public class DialogService {

	public ILauncher? Launcher { get; set; }
	public IStorageProvider? StorageProvider { get; set; }

	public Task<FAContentDialogResult> ShowDialogAsync (FAContentDialog contentDialog) {
		return Dispatcher.UIThread.InvokeAsync (async () => {
			FormatContentDialog (contentDialog);
			return await contentDialog.ShowAsync ().ConfigureAwait (true);
		});
	}

	public async Task<bool> ShowMessageAsync (object content, object? title = null) {
		return await Dispatcher.UIThread.InvokeAsync (async () => await ShowDialogAsync (new () {
			Title = title, Content = content, PrimaryButtonText = I18NExtension.Translate (LangKeys.Ok)
		}).ConfigureAwait (true)).ConfigureAwait (true) == FAContentDialogResult.Primary;
	}

	public Task<bool> ShowExceptionAsync (Exception exception, object? title = null) {
		return ShowMessageAsync (exception, title);
	}

	public async Task<bool> ShowAskAsync (object content, object? title = null) {
		return await Dispatcher.UIThread.InvokeAsync (async () => await ShowDialogAsync (new () {
			Title = title, Content = content, PrimaryButtonText = I18NExtension.Translate (LangKeys.Ok),
			SecondaryButtonText = I18NExtension.Translate (LangKeys.Cancel)
		}).ConfigureAwait (true)).ConfigureAwait (true) == FAContentDialogResult.Primary;
	}

	public Task<FAContentDialogResult> WaitDialogAsync (
		FAContentDialog contentDialog, Func<FAContentDialog, object?, CancellationToken, Task<bool>> callbackAsync,
		object? state = null
	) {
		return Dispatcher.UIThread.InvokeAsync (async () => {
			FormatContentDialog (contentDialog);
			using var cancellationTokenSource = new CancellationTokenSource ();
			var waitDialogContext = new WaitDialogContext (callbackAsync, contentDialog, cancellationTokenSource, state);
			contentDialog.Tag = waitDialogContext;
			contentDialog.Opened += ContentDialog_Opened;
			contentDialog.Closing += ContentDialog_Closing;
			try {
				await contentDialog.ShowAsync ().ConfigureAwait (true);
				waitDialogContext.ExceptionDispatchInfo?.Throw ();
				return waitDialogContext.Result ?? FAContentDialogResult.None;
			} finally {
				contentDialog.Opened -= ContentDialog_Opened;
				contentDialog.Closing -= ContentDialog_Closing;
			}
		});
	}
	public async Task<bool> WaitDialogAsync (
		object content, Func<FAContentDialog, object?, CancellationToken, Task<bool>> callbackAsync,
		object? state = null, object? title = null
	) {
		return await Dispatcher.UIThread.InvokeAsync (async () => await WaitDialogAsync (new () {
			Title = title, Content = content, SecondaryButtonText = I18NExtension.Translate (LangKeys.Cancel)
		}, callbackAsync, state).ConfigureAwait (true)).ConfigureAwait (true) == FAContentDialogResult.Primary;
	}

	static void ContentDialog_Opened (FAContentDialog contentDialog, EventArgs e) {
		_ = Async (contentDialog).ContinueWithShowExceptionAsync ();
		static async Task Async (FAContentDialog contentDialog) {
			if (contentDialog.Tag is not WaitDialogContext waitDialogContext) {
				return;
			}
			try {
				waitDialogContext.ContentDialog.Hide (await waitDialogContext.CallbackAsync (
					contentDialog, waitDialogContext.State, waitDialogContext.CancellationTokenSource.Token
				).ConfigureAwait (true) ? FAContentDialogResult.Primary : FAContentDialogResult.None);
#pragma warning disable CA1031 // 不捕获常规异常类型
			} catch (Exception exception) {
#pragma warning restore CA1031 // 不捕获常规异常类型
				waitDialogContext.ExceptionDispatchInfo = ExceptionDispatchInfo.Capture (exception);
				waitDialogContext.ContentDialog.Hide (FAContentDialogResult.None);
			}
		}
	}

	static void ContentDialog_Closing (FAContentDialog contentDialog, FAContentDialogClosingEventArgs e) {
		if (contentDialog.Tag is not WaitDialogContext waitDialogContext) {
			return;
		}
		switch (e.Result) {
			case FAContentDialogResult.Primary:
			case FAContentDialogResult.None:
				waitDialogContext.Result ??= e.Result;
				break;
			default:
				e.Cancel = true;
				break;
		}
		_ = waitDialogContext.CancellationTokenSource.CancelAsync ().ContinueWithShowExceptionAsync ();
	}

	void FormatContentDialog (FAContentDialog contentDialog) {
		contentDialog.Title = FormatTitle (contentDialog.Title);
		contentDialog.Content = FormatContent (contentDialog.Content);
	}

#pragma warning disable CA1822 // 将成员标记为 static
	object? FormatTitle (object? title = null) {
#pragma warning restore CA1822 // 将成员标记为 static
		if (title is string value) {
			var textBlock = new TextBlock () { Text = $"{value}", TextWrapping = TextWrapping.Wrap };
			textBlock.Bind (TextBlock.FontFamilyProperty, new DynamicResourceExtension ("ContentControlThemeFontFamily"));
			return textBlock;
		}
		return title;
	}

#pragma warning disable CA1822 // 将成员标记为 static
	object? FormatContent (object? content = null) {
#pragma warning restore CA1822 // 将成员标记为 static
		if (content is Exception exception) {
			return new TextBox () { Text = $"{exception}", TextWrapping = TextWrapping.Wrap, IsReadOnly = true };
		}
		return content;
	}

	sealed record class WaitDialogContext (
		Func<FAContentDialog, object?, CancellationToken, Task<bool>> CallbackAsync,
		FAContentDialog ContentDialog, CancellationTokenSource CancellationTokenSource, object? State
	) {

		public ExceptionDispatchInfo? ExceptionDispatchInfo { get; set; }
		public FAContentDialogResult? Result { get; set; }

	}

}