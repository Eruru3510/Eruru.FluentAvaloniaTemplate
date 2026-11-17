using Antelcat.I18N.Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Presentation;
using Presentation.Dialogs;
using Presentation.Localization;

namespace Eruru.FluentAvaloniaTemplate.Consolonia.ViewModels;

public partial class HomeViewModel : ObservableObject {

	[ObservableProperty]
	public partial DialogResult? DialogResult { get; set; }

	[RelayCommand]
	async Task MessageBoxAsync (CancellationToken cancellationToken = default) {
		DialogResult = null;
		DialogResult = await PresentationApi.Services!.GetRequiredService<IDialogService> ().ShowMessageAsync (
			"Content", "Title", cancellationToken
		).ConfigureAwait (true);
	}

	[RelayCommand]
	async Task ExceptionBoxAsync (CancellationToken cancellationToken = default) {
		DialogResult = null;
		try {
			throw new KeyNotFoundException ();
		} catch (KeyNotFoundException exception) {
			DialogResult = await PresentationApi.Services!.GetRequiredService<IDialogService> ().ShowExceptionAsync (
				exception, "Title", cancellationToken
			).ConfigureAwait (true);
		}
	}

	[RelayCommand]
	async Task OkCancelBoxAsync (CancellationToken cancellationToken = default) {
		DialogResult = null;
		DialogResult = await PresentationApi.Services!.GetRequiredService<IDialogService> ().ShowOkCancelAsync (
			"Content", "Title", cancellationToken
		).ConfigureAwait (true);
	}

	[RelayCommand]
	async Task WaitTaskBoxAsync (CancellationToken cancellationToken = default) {
		DialogResult = null;
		DialogResult = await PresentationApi.Services!.GetRequiredService<IDialogService> ().WaitDialogAsync (
			"Custom View", static async (content, cancellationToken) => {
				await Task.Delay (2000, cancellationToken).ConfigureAwait (true);
			}, "Title", I18NExtension.Translate (LangKeys.Cancel), cancellationToken
		).ConfigureAwait (true);
	}

}