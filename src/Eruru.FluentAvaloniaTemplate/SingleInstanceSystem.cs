using System.IO.Pipes;

namespace Eruru.FluentAvaloniaTemplate;

public class SingleInstanceSystem (string namedPipeName) : IDisposable {

	readonly string NamedPipeName = namedPipeName;
	FileStream? FileStream;
	int DisposeState;

	protected virtual void Dispose (bool disposing) {
		if (Interlocked.Exchange (ref DisposeState, 1) != 0 || !disposing) {
			return;
		}
		FileStream?.Dispose ();
	}
	public void Dispose () {
		Dispose (true);
		GC.SuppressFinalize (this);
	}

	~SingleInstanceSystem () {
		Dispose (false);
	}

	public bool IsExists () {
		FileStream?.Dispose ();
		try {
			var fileInfo = new FileInfo (Path.Combine (Path.GetTempPath (), $"{NamedPipeName}.lock"));
			fileInfo.Directory!.Create ();
			FileStream = new (
				fileInfo.FullName,
				FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None
			);
			return false;
		} catch (IOException) {
			return true;
		}
	}

	public async Task SendAsync (string message, CancellationToken cancellationToken = default) {
		using var client = new NamedPipeClientStream (".", NamedPipeName, PipeDirection.Out);
		await client.ConnectAsync (cancellationToken).ConfigureAwait (false);
		using var writer = new StreamWriter (client);
		await writer.WriteAsync (message).ConfigureAwait (false);
	}

	public async Task StartServerAsync<TState> (
		Func<string, TState, CancellationToken, Task> onReceivedAsync, TState state, CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull (onReceivedAsync);
		while (true) {
			using var Server = new NamedPipeServerStream (NamedPipeName, PipeDirection.In);
			await Server.WaitForConnectionAsync (cancellationToken).ConfigureAwait (false);
			using var reader = new StreamReader (Server);
			await onReceivedAsync (
				await reader.ReadToEndAsync (cancellationToken).ConfigureAwait (false), state, cancellationToken
			).ConfigureAwait (false);
		}
	}
	public Task StartServerAsync (
		Func<string, CancellationToken, Task> onReceivedAsync, CancellationToken cancellationToken = default
	) {
		return StartServerAsync (
			static (message, state, cancellationToken) => state.onReceivedAsync (message, state.cancellationToken),
			(onReceivedAsync, cancellationToken), cancellationToken
		);
	}

}