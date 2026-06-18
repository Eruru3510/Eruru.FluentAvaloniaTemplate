using System.IO.Pipes;

namespace Eruru.FluentAvaloniaTemplate.Services;

public class SingleInstanceService (string name) : IDisposable {

	public Func<SingleInstanceService, string, Task>? OnReceivedAsync { get; set; }

	readonly string Name = name;
	FileStream? FileStream;
	int State;
	int ServerState;

	protected virtual void Dispose (bool disposing) {
		if (Interlocked.Exchange (ref State, 1) != 0 || !disposing) {
			return;
		}
		FileStream?.Dispose ();
	}

	public void Dispose () {
		Dispose (true);
		GC.SuppressFinalize (this);
	}

	public bool IsExists () {
		FileStream?.Dispose ();
		try {
			var fileInfo = new FileInfo (PathService.GetSingleInstanceLockPath (Name));
			fileInfo.Directory?.Create ();
			FileStream = new (
				fileInfo.FullName,
				FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None
			);
			_ = BeginServerAsync ().ContinueWithShowExceptionAsync ();
			return false;
		} catch (IOException) {
			return true;
		}
	}

	public async Task SendAsync (string text, int timeout = 100) {
		using var client = new NamedPipeClientStream (".", Name, PipeDirection.Out);
		await client.ConnectAsync (timeout).ConfigureAwait (false);
		using var writer = new StreamWriter (client);
		await writer.WriteAsync (text).ConfigureAwait (false);
	}

	async Task BeginServerAsync () {
		if (Interlocked.Exchange (ref ServerState, 1) != 0) {
			return;
		}
		while (true) {
			using var Server = new NamedPipeServerStream (Name, PipeDirection.In);
			await Server.WaitForConnectionAsync ().ConfigureAwait (false);
			using var reader = new StreamReader (Server);
			if (OnReceivedAsync == null) {
				continue;
			}
			await OnReceivedAsync (this, await reader.ReadToEndAsync ().ConfigureAwait (false)).ConfigureAwait (false);
		}
	}

}