using System.Threading.Channels;

namespace BlazorSocial.Data.BackgroundJobs;

public sealed class MetadataEventQueue
{
    private readonly Channel<PostEvent> _channel = Channel.CreateBounded<PostEvent>(
        new BoundedChannelOptions(10_000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = false,
            SingleWriter = false
        });

    public void Enqueue(PostEvent postEvent) =>
        _channel.Writer.TryWrite(postEvent);

    public IAsyncEnumerable<PostEvent> ReadAllAsync(CancellationToken ct) =>
        _channel.Reader.ReadAllAsync(ct);
}
