using System.Runtime.ExceptionServices;
using Antelcat.I18N.Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Threading;
using Eruru.FluentAvaloniaTemplate.Resources;
using FluentAvalonia.UI.Controls;

namespace Eruru.FluentAvaloniaTemplate.Services {

	public class DialogService {

		public Task<FAContentDialogResult> ShowDialogAsync (FAContentDialog contentDialog) {
			return Dispatcher.UIThread.InvokeAsync (async () => {
				FormatContentDialog (contentDialog);
				return await contentDialog.ShowAsync ().ConfigureAwait (false);
			});
		}

		public Task<FAContentDialogResult> ShowMessageAsync (object content, object? title = null) {
			return Dispatcher.UIThread.InvokeAsync (async () => await ShowDialogAsync (new () {
				Title = title, Content = content, PrimaryButtonText = I18NExtension.Translate (LangKeys.Ok)
			}).ConfigureAwait (false));
		}

		public Task<FAContentDialogResult> ShowExceptionAsync (Exception exception, object? title = null) {
			return ShowMessageAsync (exception, title);
		}

		public Task<FAContentDialogResult> WaitDialogAsync (
			FAContentDialog contentDialog, Func<FAContentDialog, CancellationToken, Task> callbackAsync
		) {
			return Dispatcher.UIThread.InvokeAsync (async () => {
				FormatContentDialog (contentDialog);
				using var cancellationTokenSource = new CancellationTokenSource ();
				var waitDialogContext = new WaitDialogContext (callbackAsync, contentDialog, cancellationTokenSource);
				contentDialog.Tag = waitDialogContext;
				contentDialog.Opened += ContentDialog_Opened;
				contentDialog.Closing += ContentDialog_Closing;
				try {
					await contentDialog.ShowAsync ().ConfigureAwait (false);
					waitDialogContext.ExceptionDispatchInfo?.Throw ();
					return waitDialogContext.Result ?? FAContentDialogResult.None;
				} finally {
					contentDialog.Opened -= ContentDialog_Opened;
					contentDialog.Closing -= ContentDialog_Closing;
				}
			});
		}
		public Task<FAContentDialogResult> WaitDialogAsync (
			object content, Func<FAContentDialog, CancellationToken, Task> callbackAsync, object? title = null
		) {
			return Dispatcher.UIThread.InvokeAsync (async () => await WaitDialogAsync (new () {
				Title = title, Content = content, SecondaryButtonText = I18NExtension.Translate (LangKeys.Cancel)
			}, callbackAsync).ConfigureAwait (false));
		}

		static void ContentDialog_Opened (FAContentDialog contentDialog, EventArgs e) {
			if (contentDialog.Tag is not WaitDialogContext waitDialogContext) {
				return;
			}
			try {
				var task = waitDialogContext.CallbackAsync (contentDialog, waitDialogContext.CancellationTokenSource.Token);
				_ = task.ContinueWith (
					static (task, state) => {
						if (state is not WaitDialogContext waitDialogContext) {
							return;
						}
						if (task.Exception != null) {
							waitDialogContext.ExceptionDispatchInfo = ExceptionDispatchInfo.Capture (task.Exception);
						}
						Dispatcher.UIThread.Invoke (() => waitDialogContext.ContentDialog.Hide (
							task.IsCompletedSuccessfully ? FAContentDialogResult.Primary : FAContentDialogResult.None
						));
					}, waitDialogContext,
					CancellationToken.None, TaskContinuationOptions.RunContinuationsAsynchronously, TaskScheduler.Default
				);
#pragma warning disable CA1031 // 不捕获常规异常类型
			} catch (Exception exception) {
#pragma warning restore CA1031 // 不捕获常规异常类型
				waitDialogContext.ExceptionDispatchInfo = ExceptionDispatchInfo.Capture (exception);
				waitDialogContext.ContentDialog.Hide (FAContentDialogResult.None);
			}
		}

		static void ContentDialog_Closing (FAContentDialog contentDialog, FAContentDialogClosingEventArgs e) {
			e.Cancel = Dispatcher.UIThread.Invoke (() => {
				if (contentDialog.Tag is not WaitDialogContext waitDialogContext) {
					return true;
				}
				_ = waitDialogContext.CancellationTokenSource.CancelAsync ();
				switch (e.Result) {
					case FAContentDialogResult.Primary:
					case FAContentDialogResult.None:
						waitDialogContext.Result ??= e.Result;
						return false;
				}
				return true;
			});
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
				return new TextBox () { Text = $"{exception}", TextWrapping = TextWrapping.Wrap };
			}
			return content;
		}

		sealed record class WaitDialogContext (
			Func<FAContentDialog, CancellationToken, Task> CallbackAsync,
			FAContentDialog ContentDialog, CancellationTokenSource CancellationTokenSource
		) {

			public ExceptionDispatchInfo? ExceptionDispatchInfo { get; set; }
			public FAContentDialogResult? Result { get; set; }

		}

	}

}