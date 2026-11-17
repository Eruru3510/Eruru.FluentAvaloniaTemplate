namespace Presentation.Configs;

public class JsonConfigFileSource (string path) : IJsonConfigSource {

	public string Path { get; } = path;

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

	~JsonConfigFileSource () {
		Dispose (false);
	}

	public Task<Stream?> OpenInputStreamAsync (CancellationToken cancellationToken = default) {
		var fileInfo = new FileInfo (Path);
		return Task.FromResult<Stream?> (fileInfo.Exists ? fileInfo.OpenRead () : null);
	}

	public Task CloseInputStreamAsync (Stream stream, CancellationToken cancellationToken = default) {
		ArgumentNullException.ThrowIfNull (stream);
		return stream.DisposeAsync ().AsTask ();
	}

	public Task<Stream> OpenOutputStreamAsync (CancellationToken cancellationToken = default) {
		var tempFileInfo = new FileInfo ($"{Path}.tmp");
		tempFileInfo.Directory!.Create ();
		return Task.FromResult<Stream> (tempFileInfo.Create ());
	}

	public async Task CloseOutputStreamAsync (Stream stream, CancellationToken cancellationToken = default) {
		ArgumentNullException.ThrowIfNull (stream);
		await stream.DisposeAsync ().ConfigureAwait (false);
		var fileInfo = new FileInfo (Path);
		var tempFileInfo = new FileInfo ($"{fileInfo.FullName}.tmp");
		fileInfo.Directory!.Create ();
		tempFileInfo.CopyTo (fileInfo.FullName, true);
		tempFileInfo.Delete ();
	}

}