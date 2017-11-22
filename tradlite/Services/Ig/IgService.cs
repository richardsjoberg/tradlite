using IGWebApiClient;
using IGWebApiClient.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tradlite.Models.Ig;
using Trady.Importer;

namespace Tradlite.Services.Ig
{
    public interface IIgService
    {
        Task<IgRestApiClient> GetIgClient();
        Task<SentimentResponse> GetSentiment(string igTicker);
    }
    public class IgService : IIgService
    {
        private readonly IgRestApiClient _igRestApiClient;
        private readonly string _igEnv;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _apiKey;
        private readonly PropertyEventDispatcher _eventDispatcher;
        private bool isAuthenticated;
        public IgService(string igEnv, string userName, string password, string apiKey, Action<string> onMessage = null)
        {
            _igEnv = igEnv;
            _userName = userName;
            _password = password;
            _apiKey = apiKey;
            _eventDispatcher = new EventDispatcher(onMessage);
            _igRestApiClient = new IgRestApiClient(igEnv, _eventDispatcher);
        }
        public async Task<IgRestApiClient> GetIgClient()
        {
            await Authenticate();
            return _igRestApiClient;
        }

        public async Task<SentimentResponse> GetSentiment(string igTicker)
        {
            var client = await GetIgClient();
            var marketDetails = await client.marketDetails(igTicker);
            var sentiment = await client.getClientSentiment(marketDetails.Response.instrument.marketId);
            var relatedSentiment = await client.getRelatedClientSentiment(marketDetails.Response.instrument.marketId);
            return new SentimentResponse
            {
                Sentiment = sentiment,
                RelatedSentiment = relatedSentiment
            };
        }

        private async Task Authenticate()
        {
            if (!isAuthenticated)
            {
                await _igRestApiClient.SecureAuthenticate(new dto.endpoint.auth.session.v2.AuthenticationRequest { identifier = _userName, password = _password }, _apiKey);
                isAuthenticated = true;
            }
        }
    }
}
