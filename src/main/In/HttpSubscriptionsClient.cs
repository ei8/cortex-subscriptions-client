using ei8.Cortex.Subscriptions.Common;
using ei8.Cortex.Subscriptions.Common.Attributes;
using ei8.Cortex.Subscriptions.Common.Receivers;
using neurUL.Common.Http;
using NLog;
using Polly;
using Polly.Retry;
using Splat;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Cortex.Subscriptions.Client.In
{
    public class HttpSubscriptionsClient<T> : ISubscriptionsClient<T> where T : IReceiverInfo
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static AsyncRetryPolicy ExponentialRetry = Policy.Handle<Exception>()
                                                                 .WaitAndRetryAsync(
                                                                    3,
                                                                    attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                                                                    (ex, _) => HttpSubscriptionsClient<T>.Logger.Error(ex, "Error occurred while communicating with ei8 Cortex Subscriptions. " + ex.InnerException?.Message)
                                                                 );
        private IRequestProvider requestProvider;

        public HttpSubscriptionsClient(IRequestProvider requestProvider = null)
        {
            this.requestProvider = requestProvider ?? Locator.Current.GetService<IRequestProvider>();
        }

        public async Task AddSubscription(string baseUrl, IAddSubscriptionReceiverRequest<T> request, CancellationToken token = default)
        {
            await ExponentialRetry.ExecuteAsync(async () => await AddSubscriptionInternal(baseUrl, request, token).ConfigureAwait(false));
        }

        private async Task AddSubscriptionInternal(string baseUrl, IAddSubscriptionReceiverRequest<T> request, CancellationToken token = default)
        {
            var subscriptionPath = GetSubscriptionPath(request.ReceiverInfo);
            var requestUrl = $"{baseUrl}/subscriptions/receivers/{subscriptionPath}";

            await requestProvider.PostAsync(requestUrl, request);
        }

        private string GetSubscriptionPath(IReceiverInfo request)
        {
            var type = request.GetType();
            var subscriptionPathAttribute = type.GetCustomAttributes(typeof(SubscriptionPathAttribute), false)
                                                .FirstOrDefault() as SubscriptionPathAttribute;

            if (subscriptionPathAttribute != null)
                return subscriptionPathAttribute.EndpointPath;

            else
                throw new ArgumentException($"Unsupported receiver info type: {type.FullName}");
        }
    }
}