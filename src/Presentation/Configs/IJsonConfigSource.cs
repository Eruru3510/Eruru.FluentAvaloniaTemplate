namespace Presentation.Configs;

public interface IJsonConfigSource : IDisposable {

	Task<Stream?> OpenInputStreamAsync (CancellationToken cancellationToken = default);

	Task CloseInputStreamAsync (Stream stream, CancellationToken cancellationToken = default);

	Task<Stream> OpenOutputStreamAsync (CancellationToken cancellationToken = default);

	Task CloseOutputStreamAsync (Stream stream, CancellationToken cancellationToken = default);

}