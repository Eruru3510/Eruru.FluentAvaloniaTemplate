using System.Text;
using Eruru.JsonConfig;

namespace Eruru.FluentAvaloniaTemplate.Browser;

internal sealed class JsonConfigLocalStorageSource (string name) : IJsonConfigSource {

	public event EventHandler? OnChanged;

	readonly string Name = name;
	readonly MemoryStream MemoryStream = new ();
	int State;

	public void Dispose () {
		if (Interlocked.Exchange (ref State, 1) != 0) {
			return;
		}
		OnChanged?.Invoke (null, EventArgs.Empty);
		OnChanged = null;
		GC.SuppressFinalize (this);
	}

	public Task<Stream?> OpenInputStreamAsync () {
		var value = JsInterop.LocalStorageGetItem (Name);
		if (string.IsNullOrWhiteSpace (value)) {
			return Task.FromResult<Stream?> (null);
		}
		MemoryStream.Position = 0;
		MemoryStream.SetLength (0);
		MemoryStream.Write (Encoding.UTF8.GetBytes (value));
		MemoryStream.Position = 0;
		return Task.FromResult<Stream?> (MemoryStream);
	}

	public Task CloseInputStreamAsync (Stream? stream) {
		return Task.CompletedTask;
	}

	public Task<Stream?> OpenOutputStreamAsync () {
		MemoryStream.Position = 0;
		MemoryStream.SetLength (0);
		return Task.FromResult<Stream?> (MemoryStream);
	}

	public Task CloseOutputStreamAsync (Stream? stream) {
		MemoryStream.Position = 0;
		JsInterop.LocalStorageSetItem (Name, Encoding.UTF8.GetString (MemoryStream.ToArray ()));
		return Task.CompletedTask;
	}

	public Task BackupAsync () {
		return Task.CompletedTask;
	}

	public Task DeleteAsync () {
		JsInterop.LocalStorageSetItem (Name, string.Empty);
		return Task.CompletedTask;
	}

}