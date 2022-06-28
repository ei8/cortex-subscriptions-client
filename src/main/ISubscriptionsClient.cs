using ei8.Cortex.Subscriptions.Common;

namespace ei8.Cortex.Subscriptions.Client
{
    public interface ISubscriptionsClient
    {
        Task AddSubscription(string baseUrl, BrowserSubscriptionInfo request, CancellationToken token = default);
    }
}
