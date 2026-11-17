namespace Presentation.Dialogs;

public interface IDialogService {

	Task<DialogResult> ShowExceptionAsync (
	   Exception exception, object? title = null, CancellationToken cancellationToken = default
   );

	Task<DialogResult> ShowMessageAsync (
	   object? content = null, object? title = null, CancellationToken cancellationToken = default
   );

	Task<DialogResult> ShowOkCancelAsync (
	   object? content = null, object? title = null, CancellationToken cancellationToken = default
   );

	Task<DialogResult> ShowDialogAsync (
	   object? content = null, object? title = null,
	   string? primaryButtonText = null, string? secondaryButtonText = null, string? closeButtonText = null,
	   CancellationToken cancellationToken = default
   );

	Task<DialogResult> WaitDialogAsync (
	   object content, Func<object, CancellationToken, Task> taskAsync,
	   object? title = null, string? secondaryButtonText = null,
	   CancellationToken cancellationToken = default
   );

}