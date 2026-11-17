using System.Diagnostics;
using System.Runtime.ExceptionServices;
using Antelcat.I18N.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Presentation;
using Presentation.Dialogs;
using Presentation.Localization;

namespace Eruru.FluentAvaloniaTemplate.Dialogs;

public class DialogService : IDialogService {

	public Task<DialogResult> ShowExceptionAsync (
		Exception exception, object? title = null, CancellationToken cancellationToken = default
	) {
		Debug.WriteLine (exception);
		Console.WriteLine (exception);
		return ShowMessageAsync (content: exception, title: title, cancellationToken: cancellationToken);
	}

	public Task<DialogResult> ShowMessageAsync (
		object? content = null, object? title = null, CancellationToken cancellationToken = default
	) {
		return ShowDialogAsync (
			content: content, title: title,
			primaryButtonText: I18NExtension.Translate (LangKeys.Ok),
			cancellationToken: cancellationToken
		);
	}

	public Task<DialogResult> ShowOkCancelAsync (
		object? content = null, object? title = null, CancellationToken cancellationToken = default
	) {
		return ShowDialogAsync (
			content: content, title: title,
			primaryButtonText: I18NExtension.Translate (LangKeys.Ok), secondaryButtonText: I18NExtension.Translate (LangKeys.Cancel),
			cancellationToken: cancellationToken
		);
	}

	public Task<DialogResult> ShowDialogAsync (
		object? content = null, object? title = null,
		string? primaryButtonText = null, string? secondaryButtonText = null, string? closeButtonText = null,
		CancellationToken cancellationToken = default
	) {
		return Dispatcher.UIThread.InvokeAsync (async () => {
			var contentDialog = new FAContentDialog ();
			if (content != null) {
				contentDialog.Content = content is Visual ? content : new SelectableTextBlock () {
					Text = $"{content}", TextWrapping = TextWrapping.Wrap
				};
			}
			if (title != null) {
				contentDialog.Title = FormatTitle (title);
			}
			if (primaryButtonText != null) {
				contentDialog.PrimaryButtonText = primaryButtonText;
			}
			if (secondaryButtonText != null) {
				contentDialog.SecondaryButtonText = secondaryButtonText;
			}
			if (closeButtonText != null) {
				contentDialog.CloseButtonText = secondaryButtonText;
			}
			if (primaryButtonText == null && secondaryButtonText == null && closeButtonText == null) {
				contentDialog.PrimaryButtonText = I18NExtension.Translate (LangKeys.Ok);
			}
			return (DialogResult)await contentDialog.ShowAsync (
				PresentationApi.Services!.GetRequiredService<TopLevel> ()
			).ConfigureAwait (true);
		});
	}

	public Task<DialogResult> WaitDialogAsync (
		object content, Func<object, CancellationToken, Task> taskAsync,
		object? title = null, string? secondaryButtonText = null,
		CancellationToken cancellationToken = default
	) {
		return Dispatcher.UIThread.InvokeAsync (async () => {
			using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource (cancellationToken);
			var waitDialog = new WaitDialog (taskAsync, cancellationTokenSource);
			var contentDialog = new FAContentDialog () { Content = content, Tag = waitDialog };
			if (title != null) {
				contentDialog.Title = FormatTitle (title);
			}
			if (secondaryButtonText != null) {
				contentDialog.SecondaryButtonText = secondaryButtonText;
			}
			try {
				contentDialog.Opened += ContentDialog_Opened;
				contentDialog.Closing += ContentDialog_Closing;
				await contentDialog.ShowAsync (
					PresentationApi.Services!.GetRequiredService<TopLevel> ()
				).ConfigureAwait (true);
				waitDialog.ExceptionDispatchInfo?.Throw ();
				if (waitDialog.Result! == DialogResult.None && waitDialog.IsUserCanceled) {
					waitDialog.Result = DialogResult.Secondary;
				}
				return waitDialog.Result!.Value;
			} finally {
				contentDialog.Opened -= ContentDialog_Opened;
				contentDialog.Closing -= ContentDialog_Closing;
			}
		});
	}

	void ContentDialog_Opened (FAContentDialog sender, EventArgs args) {
		var waitDialog = (WaitDialog)sender.Tag!;
		_ = waitDialog.TaskAsync (sender.Content!, waitDialog.CancellationTokenSource.Token).ContinueWith (
			static (task, state) => {
				var (contentDialog, waitDialog) = ((FAContentDialog, WaitDialog))state!;
				switch (task.Status) {
					case TaskStatus.Canceled:
						waitDialog.Result = DialogResult.None;
						break;
					case TaskStatus.Faulted:
						waitDialog.ExceptionDispatchInfo = ExceptionDispatchInfo.Capture (task.Exception!);
						waitDialog.Result = DialogResult.None;
						break;
					case TaskStatus.RanToCompletion:
						waitDialog.Result = DialogResult.Primary;
						break;
					default:
						throw new NotImplementedException ($"{task.Status}");
				}
				contentDialog.Hide ();
			},
			(sender, waitDialog),
			CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously,
			TaskScheduler.FromCurrentSynchronizationContext ()
		);
	}

	void ContentDialog_Closing (FAContentDialog sender, FAContentDialogClosingEventArgs args) {
		var waitDialog = (WaitDialog)sender.Tag!;
		if (waitDialog.Result != null) {
			return;
		}
		switch (args.Result) {
			case FAContentDialogResult.Primary:
			case FAContentDialogResult.Secondary:
			case FAContentDialogResult.None:
				args.Cancel = true;
				waitDialog.IsUserCanceled = true;
				_ = waitDialog.CancellationTokenSource.CancelAsync ().OnFaultedShowDialogAsync ();
				break;
			default:
				throw new NotImplementedException ($"{args.Result}");
		}
	}

	object FormatTitle (object title) {
		if (title is Visual) {
			return title;
		}
		var textBlock = new TextBlock () { TextWrapping = TextWrapping.Wrap, Text = $"{title}" };
		textBlock.Bind (TextBlock.FontFamilyProperty, new DynamicResourceExtension ("ContentControlThemeFontFamily"));
		return textBlock;
	}

}