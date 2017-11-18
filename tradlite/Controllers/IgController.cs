using dto.endpoint.browse;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public class BrowseResponse
        {
            public List<HierarchyNode> Nodes { get; set; }
            public List<HierarchyMarket> Markets { get; set; }
        }
    }
}
