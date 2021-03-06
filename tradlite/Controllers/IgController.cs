﻿using dto.endpoint.marketdetails.v2;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tradlite.Models.Ig;
using Tradlite.Services.Ig;

namespace Tradlite.Controllers
{
    public class IgController : Controller
    {
        private readonly IIgService _igService;

        public IgController(IIgService igService)
        {
            _igService = igService;
        }
        
        [Route("api/ig/browse")]
        public async Task<BrowseResponse> Browse([FromQuery]string nodeId)
        {
            var client = await _igService.GetIgClient();
            if(!string.IsNullOrEmpty(nodeId))
            {
                var response = await client.browse(nodeId);
                return new BrowseResponse
                {
                    Nodes = response.Response.nodes,
                    Markets = response.Response.markets
                };
            }
            else
            {
                var response = await client.browseRoot();
                return new BrowseResponse
                {
                    Nodes = response.Response.nodes,
                    Markets = response.Response.markets
                };
            }
        }

        [Route("api/ig/sentiment/{igTicker}")]
        public async Task<SentimentResponse> Sentiment(string igTicker)
        {
            return await _igService.GetSentiment(igTicker);
        }

        [Route("api/ig/marketDetails/{igTicker}")]
        public async Task<MarketDetailsResponse> MarketDetails(string igTicker)
        {
            return await _igService.GetMarketDetails(igTicker);
        }

        [Route("api/ig/unlongable/{igTicker}")]
        public async Task<bool> Unlongable(string igTicker)
        {
            var marketDetails = await _igService.GetMarketDetails(igTicker);
            return marketDetails.instrument.specialInfo.Contains("unlongable");
        }
    }
}
