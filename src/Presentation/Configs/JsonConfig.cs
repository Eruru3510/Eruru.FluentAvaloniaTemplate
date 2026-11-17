using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Presentation.Configs;

public class JsonConfig<T> (IJsonConfigSource jsonConfigSource, JsonTypeInfo<T> jsonTypeInfo) : IDisposable where T : new() {

	public IJsonConfigSource JsonConfigSource { get; } = jsonConfigSource;
	public JsonTypeInfo<T> JsonTypeInfo { get; } = jsonTypeInfo;

	readonly SemaphoreSlim SemaphoreSlim = new (1, 1);
	T Value = new ();
	bool IsLoaded;
	int DisposeState;

	protected virtual void Dispose (bool disposing) {
		if (Interlocked.Exchange (ref DisposeState, 1) != 0 || !disposing) {
			return;
		}
		SemaphoreSlim.Dispose ();
	}
	public void Dispose () {
		Dispose (true);
		GC.SuppressFinalize (this);
	}

	~JsonConfig () {
		Dispose (false);
	}

	public async Task<TValue> ReadAsync<TValue> (Func<T, TValue> onRead, CancellationToken cancellationToken = default) {
		ArgumentNullException.ThrowIfNull (onRead);
		await SemaphoreSlim.WaitAsync (cancellationToken).ConfigureAwait (false);
		try {
			if (!IsLoaded) {
				await LoadAsync (cancellationToken).ConfigureAwait (false);
			}
			using var jsonDocument = JsonSerializer.SerializeToDocument (Value, JsonTypeInfo);
			return onRead (jsonDocument.Deserialize (JsonTypeInfo)!);
		} finally {
			SemaphoreSlim.Release ();
		}
	}
	public Task<T> ReadAsync (CancellationToken cancellationToken = default) {
		return ReadAsync (static x => x, cancellationToken);
	}

	public async Task WriteAsync<TState> (Action<T, TState> onWrite, TState state, CancellationToken cancellationToken = default) {
		ArgumentNullException.ThrowIfNull (onWrite);
		await SemaphoreSlim.WaitAsync (cancellationToken).ConfigureAwait (false);
		try {
			onWrite (Value, state);
			if (!IsLoaded) {
				throw new InvalidOperationException ("未加载配置禁止保存");
			}
			await SaveAsync (Value, cancellationToken).ConfigureAwait (false);
		} finally {
			SemaphoreSlim.Release ();
		}
	}
	public Task WriteAsync (Action<T> onWrite, CancellationToken cancellationToken = default) {
		return WriteAsync (static (value, state) => state (value), onWrite, cancellationToken);
	}

	async Task LoadAsync (CancellationToken cancellationToken = default) {
		T? value;
		var inputStream = await JsonConfigSource.OpenInputStreamAsync (cancellationToken).ConfigureAwait (false);
		try {
			if (inputStream == null) {
				value = new ();
				await SaveAsync (value, cancellationToken).ConfigureAwait (false);
			} else {
				value = await JsonSerializer.DeserializeAsync (inputStream, JsonTypeInfo, cancellationToken).ConfigureAwait (false);
				value ??= new T ();
			}
			Value = value;
			IsLoaded = true;
		} finally {
			if (inputStream != null) {
				await JsonConfigSource.CloseInputStreamAsync (inputStream, cancellationToken).ConfigureAwait (false);
			}
		}
	}

	async Task SaveAsync (T value, CancellationToken cancellationToken = default) {
		var outputStream = await JsonConfigSource.OpenOutputStreamAsync (cancellationToken).ConfigureAwait (false);
		try {
			await JsonSerializer.SerializeAsync (outputStream, value, JsonTypeInfo, cancellationToken).ConfigureAwait (false);
		} finally {
			if (outputStream != null) {
				await JsonConfigSource.CloseOutputStreamAsync (outputStream, cancellationToken).ConfigureAwait (false);
			}
		}
	}

}