using System.Threading.Channels;

namespace BlazorSocial.Data.BackgroundJobs;

public sealed class ViewEventQueue
{
    private readonly Channel<ViewRecorded> _channel = Channel.CreateBounded<ViewRecorded>(
        new BoundedChannelOptions(10_000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = false,
            SingleWriter = false
        });

    public void Enqueue(ViewRecorded viewEvent) =>
        _channel.Writer.TryWrite(viewEvent);

    public IAsyncEnumerable<ViewRecorded> ReadAllAsync(CancellationToken ct) =>
        _channel.Reader.ReadAllAsync(ct);
}
