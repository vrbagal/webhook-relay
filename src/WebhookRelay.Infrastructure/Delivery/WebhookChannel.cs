using System.Runtime.CompilerServices;
using System.Threading.Channels;
using WebhookRelay.Core.Entities;
using WebhookRelay.Core.Interfaces;

namespace WebhookRelay.Infrastructure.Delivery;

public class WebhookChannel : IWebhookChannel
{
    private readonly Channel<WebhookEvent> _channel =
        Channel.CreateUnbounded<WebhookEvent>(new UnboundedChannelOptions { SingleReader = false });

    public void Enqueue(WebhookEvent webhookEvent) =>
        _channel.Writer.TryWrite(webhookEvent);

    public async IAsyncEnumerable<WebhookEvent> ReadAllAsync(
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var item in _channel.Reader.ReadAllAsync(ct))
            yield return item;
    }
}
