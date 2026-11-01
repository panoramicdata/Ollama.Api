using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Ollama.Api;

/// <summary>
/// Sends a caller's stream as a request body without disposing it.
/// </summary>
/// <remarks>
/// <see cref="StreamContent"/> disposes its stream along with the request, which would close a file
/// the caller still owns.
/// </remarks>
internal sealed class LeaveOpenStreamContent(Stream stream) : HttpContent
{
	private readonly Stream _stream = stream ?? throw new ArgumentNullException(nameof(stream));

	[ExcludeFromCodeCoverage(Justification = "Abstract, but never called once the cancellable overload is overridden.")]
	protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
		=> SerializeToStreamAsync(stream, context, CancellationToken.None);

	protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
		=> _stream.CopyToAsync(stream, cancellationToken);

	protected override bool TryComputeLength(out long length)
	{
		length = _stream.CanSeek ? _stream.Length - _stream.Position : 0;

		return _stream.CanSeek;
	}
}
