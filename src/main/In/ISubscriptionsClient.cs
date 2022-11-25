using ei8.Cortex.Subscriptions.Common;
using ei8.Cortex.Subscriptions.Common.Receivers;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Subscriptions.Client.In
{
    public interface ISubscriptionsClient 
    {
        Task AddSubscription<T>(string baseUrl, IAddSubscriptionReceiverRequest<T> request, CancellationToken token = default) where T : IReceiverInfo;

        Task SendNotificationToUser(string baseUrl, string targetUserNeuronId, NotificationPayloadRequest request, CancellationToken token = default);
    }
}
