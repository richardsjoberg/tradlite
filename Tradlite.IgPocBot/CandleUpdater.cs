using IGWebApiClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tradlite.Services.Ig;

namespace Tradlite.IgPocBot
{
    public class CandleUpdater
    {
        private readonly IIgService igService;
        private Dictionary<string, DateTimeOffset> _lastUpdate;
        public event EventHandler<CandleUpdatedEventArgs> _candlesUpdated;
        public CandleUpdater(IIgService igService)
        {
            this.igService = igService;
        }

        public async Task Initialize(string[] tickers, string igUserName, Dictionary<string, DateTimeOffset> lastUpdate)
        {
            var client = await igService.GetIgClient();
            var authenticationDetails = await igService.GetAuthenticationDetails();
            var streamingClient = new IGStreamingApiClient();
            var igContext = client.GetConversationContext();
            streamingClient.Connect(igUserName, igContext.cst, igContext.xSecurityToken, igContext.apiKey, authenticationDetails.lightstreamerEndpoint);
            var tableListener = new ChartCandleTableListerner();
            tableListener.Update += TableListener_Update;
            streamingClient.SubscribeToChartCandleData(tickers, ChartScale.FiveMinute, tableListener);
            _lastUpdate = lastUpdate;
        }
        
        private void TableListener_Update(object sender, UpdateArgs<ChartCandelData> e)
        {
            var ticker = e.ItemName.Split(':')[1];
            var difference = (e.UpdateData.UpdateTime - _lastUpdate[ticker].UtcDateTime);
            var newCandle = difference.HasValue && difference.Value.Minutes > 15;
            var candleTime = newCandle ? _lastUpdate[ticker].AddMinutes(15) : _lastUpdate[ticker];
            var open = (e.UpdateData.Bid.Open + e.UpdateData.Offer.Open) / 2;
            var close = (e.UpdateData.Bid.Close + e.UpdateData.Offer.Close) / 2;
            var high = (e.UpdateData.Bid.High + e.UpdateData.Offer.High) / 2;
            var low = (e.UpdateData.Bid.Low + e.UpdateData.Offer.Low) / 2;
            var volume = e.UpdateData.LastTradedVolume;
            if(open.HasValue && close.HasValue && high.HasValue && low.HasValue && volume.HasValue)
            {
                _candlesUpdated(null, new CandleUpdatedEventArgs
                {
                    Ticker = ticker,
                    NewCandle = newCandle,
                    Candle = new Trady.Core.Candle(candleTime, open.Value, high.Value, low.Value, close.Value, volume.Value)
                });
            }
            
            Console.WriteLine($"ticker {ticker}, {e.UpdateData.UpdateTime}, bid close {e.UpdateData.Bid.Close}, offer close {e.UpdateData.Offer.Close}, last traded volume: {e.UpdateData.LastTradedVolume}, tickcount: {e.UpdateData.TickCount}, itemposition {e.ItemPosition}");
        }
    }
}
