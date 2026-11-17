using System.Diagnostics;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Dialogs;

namespace Presentation;

public static class PresentationApi {

	public static IServiceProvider? Services { get; set; }
	public static IReadOnlyList<KeyValuePair<string, string>> AppLanguages { get; } = [
		new ("English (United States)", "en-US"),
		new ("中文 (简体)", "zh-CN")
	];

	public static Task OnFaultedWriteLineAsync (this Task task) {
		ArgumentNullException.ThrowIfNull (task);
		return task.ContinueWith (static x => {
			if (x.IsFaulted) {
				Debug.WriteLine (x.Exception);
				Console.WriteLine (x.Exception);
			}
		}, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
	}

	public static Task OnFaultedShowDialogAsync (this Task task) {
		ArgumentNullException.ThrowIfNull (task);
		return task.ContinueWith (static x => {
			if (x.IsFaulted) {
				_ = Services!.GetRequiredService<IDialogService> ().ShowExceptionAsync (x.Exception)
					.OnFaultedWriteLineAsync ();
			}
		}, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
	}

	public static void RegisterGlobalExceptionHandler () {
		Dispatcher.UIThread.UnhandledException += UIThread_UnhandledException;
		TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
	}

	static void CurrentDomain_UnhandledException (object sender, UnhandledExceptionEventArgs e) {
		_ = Services!.GetRequiredService<IDialogService> ().ShowExceptionAsync ((Exception)e.ExceptionObject, nameof (AppDomain));
	}

	static void TaskScheduler_UnobservedTaskException (object? sender, UnobservedTaskExceptionEventArgs e) {
		e.SetObserved ();
		_ = Services!.GetRequiredService<IDialogService> ().ShowExceptionAsync (e.Exception, nameof (TaskScheduler));
	}

	static void UIThread_UnhandledException (object sender, DispatcherUnhandledExceptionEventArgs e) {
		e.Handled = true;
		_ = Services!.GetRequiredService<IDialogService> ().ShowExceptionAsync (e.Exception, nameof (Dispatcher));
	}

}