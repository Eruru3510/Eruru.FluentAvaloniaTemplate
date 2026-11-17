using System.Runtime.ExceptionServices;

namespace Presentation.Dialogs;

public class WaitDialog (
	Func<object, CancellationToken, Task> taskAsync, CancellationTokenSource cancellationTokenSource
) {

	public Func<object, CancellationToken, Task> TaskAsync { get; } = taskAsync;
	public CancellationTokenSource CancellationTokenSource { get; } = cancellationTokenSource;
	public DialogResult? Result { get; set; }
	public ExceptionDispatchInfo? ExceptionDispatchInfo { get; set; }
	public bool IsUserCanceled { get; set; }

}