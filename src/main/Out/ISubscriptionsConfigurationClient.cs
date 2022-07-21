using ei8.Cortex.Subscriptions.Common;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Subscriptions.Client.Out
{
    public interface ISubscriptionsConfigurationClient
    {
        Task<SubscriptionConfiguration> GetServerConfigurationAsync(string baseUrl, CancellationToken cancellationToken = default);
    }
}
