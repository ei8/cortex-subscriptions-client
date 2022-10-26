using ei8.Cortex.Subscriptions.Common;
using ei8.Cortex.Subscriptions.Common.Extensions;
using ei8.Cortex.Subscriptions.Common.Receivers;
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

        public async Task AddSubscription<T>(string baseUrl, IAddSubscriptionReceiverRequest<T> request, CancellationToken token = default) where T : IReceiverInfo
        {
            await ExponentialRetry.ExecuteAsync(async () => await AddSubscriptionInternal(baseUrl, request, token).ConfigureAwait(false));
        }

        public async Task SendNotificationToUser(string baseUrl, string targetUserNeuronId, NotificationPayloadRequest request, CancellationToken token = default)
        {
            await ExponentialRetry.ExecuteAsync(async () =>  await SendNotificationToUserInternal(baseUrl, targetUserNeuronId, request, token)
                                  .ConfigureAwait(false));
        }

        private async Task SendNotificationToUserInternal(string baseUrl, string targetUserNeuronId, NotificationPayloadRequest request, CancellationToken token = default)
        {
            var requestUrl = $"{baseUrl}/notify/{targetUserNeuronId}";

            await requestProvider.PostAsync(requestUrl, request);
        }

        private async Task AddSubscriptionInternal<T>(string baseUrl, IAddSubscriptionReceiverRequest<T> request, CancellationToken token = default) where T : IReceiverInfo
        {
            var subscriptionPath = request.ReceiverInfo.GetSubscriptionPath();
            var requestUrl = $"{baseUrl}/subscriptions/receivers/{subscriptionPath}";

            await requestProvider.PostAsync(requestUrl, request);
        }
    }
}