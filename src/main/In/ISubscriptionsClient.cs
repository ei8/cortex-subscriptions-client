using ei8.Cortex.Subscriptions.Common;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Subscriptions.Client.In
{
    public interface ISubscriptionsClient
    {
        Task AddSubscription(string baseUrl, BrowserSubscriptionInfo request, CancellationToken token = default);
    }
}
