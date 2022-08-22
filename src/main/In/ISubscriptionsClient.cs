using ei8.Cortex.Subscriptions.Common;
using ei8.Cortex.Subscriptions.Common.Receivers;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Subscriptions.Client.In
{
    public interface ISubscriptionsClient<T> where T : IReceiverInfo
    {
        Task AddSubscription(string baseUrl, IAddSubscriptionReceiverRequest<T> request, CancellationToken token = default);
    }
}
