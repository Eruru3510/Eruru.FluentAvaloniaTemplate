using System.Diagnostics;
using System.Runtime.ExceptionServices;
using Antelcat.I18N.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Consolonia.Controls;
using Presentation;
using Presentation.Dialogs;
using Presentation.Localization;

namespace Eruru.FluentAvaloniaTemplate.Consolonia.Dialogs;

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
			var contentDialog = new MessageBox () { MessageBoxStyle = MessageBoxStyle.Ok };
			if (content != null) {
				contentDialog.Message = content is Visual ? content : new SelectableTextBlock () {
					Text = $"{content}", TextWrapping = TextWrapping.Wrap
				};
			}
			if (title != null) {
				contentDialog.Title = $"{title}";
			}
			if (primaryButtonText != null) {
				contentDialog.Ok = primaryButtonText;
			}
			if (secondaryButtonText != null) {
				contentDialog.Cancel = secondaryButtonText;
				contentDialog.MessageBoxStyle = MessageBoxStyle.OkCancel;
			}
			if (primaryButtonText == null && secondaryButtonText == null) {
				contentDialog.Ok = I18NExtension.Translate (LangKeys.Ok);
			}
			return ToDialogResult (await contentDialog.ShowDialog ().ConfigureAwait (true));
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
			var contentDialog = new MessageBox () {
				Message = content, Tag = waitDialog, MessageBoxStyle = MessageBoxStyle.Ok
			};
			if (title != null) {
				contentDialog.Title = $"{title}";
			}
			if (secondaryButtonText != null) {
				contentDialog.Ok = secondaryButtonText;
				contentDialog.MessageBoxStyle = MessageBoxStyle.Ok;
			}
			try {
				contentDialog.Opened += ContentDialog_Opened;
				contentDialog.Closing += ContentDialog_Closing;
				await contentDialog.ShowDialog ().ConfigureAwait (true);
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

	void ContentDialog_Opened (object? sender, EventArgs args) {
		var messageBox = (MessageBox)sender!;
		var waitDialog = (WaitDialog)messageBox.Tag!;
		_ = waitDialog.TaskAsync (messageBox.Content!, waitDialog.CancellationTokenSource.Token).ContinueWith (
			static (task, state) => {
				var (contentDialog, waitDialog) = ((MessageBox, WaitDialog))state!;
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
				contentDialog.Close ();
			},
			(messageBox, waitDialog),
			CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously,
			TaskScheduler.FromCurrentSynchronizationContext ()
		);
	}

	void ContentDialog_Closing (object? sender, WindowClosingEventArgs args) {
		var messageBox = (MessageBox)sender!;
		var waitDialog = (WaitDialog)messageBox.Tag!;
		if (waitDialog.Result != null) {
			return;
		}
		args.Cancel = true;
		waitDialog.IsUserCanceled = true;
		_ = waitDialog.CancellationTokenSource.CancelAsync ().OnFaultedShowDialogAsync ();
	}

	DialogResult ToDialogResult (MessageBoxResult result) {
		return result switch {
			MessageBoxResult.Ok or MessageBoxResult.Yes => DialogResult.Primary,
			MessageBoxResult.Cancel => DialogResult.Secondary,
			_ => throw new NotImplementedException ($"{result}")
		};
	}

}