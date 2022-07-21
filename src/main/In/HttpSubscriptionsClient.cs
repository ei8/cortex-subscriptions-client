using ei8.Cortex.Subscriptions.Common;
using neurUL.Common.Http;
using NLog;
using Polly;
using Polly.Retry;
using Splat;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Subscriptions.Client.In
{
    public class HttpSubscriptionsClient : ISubscriptionsClient
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static AsyncRetryPolicy ExponentialRetry = Policy.Handle<Exception>()
                                                                 .WaitAndRetryAsync(
                                                                    3,
                                                                    attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                                                                    (ex, _) => HttpSubscriptionsClient.Logger.Error(ex, "Error occurred while communicating with ei8 Cortex Subscriptions. " + ex.InnerException?.Message)
                                                                 );
        private IRequestProvider requestProvider;

        public HttpSubscriptionsClient(IRequestProvider requestProvider = null)
        {
            this.requestProvider = requestProvider ?? Locator.Current.GetService<IRequestProvider>();
        }

        public async Task AddSubscription(string baseUrl, BrowserSubscriptionInfo request, CancellationToken token = default)
        {
            await ExponentialRetry.ExecuteAsync(async () => await AddSubscriptionInternal(baseUrl, request, token).ConfigureAwait(false));
        }

        private async Task AddSubscriptionInternal(string baseUrl, BrowserSubscriptionInfo request, CancellationToken token = default)
        {
            var requestUrl = $"{baseUrl}/subscriptions";

            await requestProvider.PostAsync(requestUrl, request);
        }
    }
}