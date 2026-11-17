using System.Text;
using Presentation.Configs;

namespace Eruru.FluentAvaloniaTemplate;

public class JsonConfigLocalStorageSource (string key) : IJsonConfigSource {

	public string Key { get; } = key;

	readonly MemoryStream MemoryStream = new ();
	int DisposeState;

	protected virtual void Dispose (bool disposing) {
		if (Interlocked.Exchange (ref DisposeState, 1) != 0 || !disposing) {
			return;
		}
	}
	public void Dispose () {
		Dispose (true);
		GC.SuppressFinalize (this);
	}

	~JsonConfigLocalStorageSource () {
		Dispose (false);
	}

	public async Task<Stream?> OpenInputStreamAsync (CancellationToken cancellationToken = default) {
		var value = JsInterop.LocalStorageGetItem (Key);
		if (string.IsNullOrWhiteSpace (value)) {
			return null;
		}
		MemoryStream.Position = 0;
		MemoryStream.SetLength (0);
		await MemoryStream.WriteAsync (Encoding.UTF8.GetBytes (value), cancellationToken).ConfigureAwait (false);
		MemoryStream.Position = 0;
		return MemoryStream;
	}

	public Task CloseInputStreamAsync (Stream stream, CancellationToken cancellationToken = default) {
		return Task.CompletedTask;
	}

	public Task<Stream> OpenOutputStreamAsync (CancellationToken cancellationToken = default) {
		MemoryStream.Position = 0;
		MemoryStream.SetLength (0);
		return Task.FromResult<Stream> (MemoryStream);
	}

	public Task CloseOutputStreamAsync (Stream stream, CancellationToken cancellationToken = default) {
		MemoryStream.Position = 0;
		JsInterop.LocalStorageSetItem (Key, Encoding.UTF8.GetString (MemoryStream.ToArray ()));
		return Task.CompletedTask;
	}

}