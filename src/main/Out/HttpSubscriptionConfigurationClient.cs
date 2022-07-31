using ei8.Cortex.Subscriptions.Common;
using neurUL.Common.Http;
using NLog;
using Polly;
using Polly.Retry;
using Splat;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Subscriptions.Client.Out
{
    public class HttpSubscriptionConfigurationClient : ISubscriptionsConfigurationClient
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static AsyncRetryPolicy ExponentialRetry = Policy.Handle<Exception>()
                                                                 .WaitAndRetryAsync(
                                                                    3,
                                                                    attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                                                                    (ex, _) => HttpSubscriptionConfigurationClient.Logger.Error(ex, "Error occurred while communicating with ei8 Cortex Subscriptions. " + ex.InnerException?.Message)
                                                                 );
        private IRequestProvider requestProvider;

        public HttpSubscriptionConfigurationClient(IRequestProvider requestProvider = null)
        {
            this.requestProvider = requestProvider ?? Locator.Current.GetService<IRequestProvider>();
        }

        public async Task<SubscriptionConfiguration> GetServerConfigurationAsync(string baseUrl, CancellationToken token = default)
        {
            return await ExponentialRetry.ExecuteAsync(async () => await GetServerConfigurationInternalAsync(baseUrl, token).ConfigureAwait(false));
        }

        private async Task<SubscriptionConfiguration> GetServerConfigurationInternalAsync(string baseUrl, CancellationToken token = default)
        {
            var requestUrl = $"{baseUrl}/config";

            return await requestProvider.GetAsync<SubscriptionConfiguration>(requestUrl, "", token);
        }
    }
}
