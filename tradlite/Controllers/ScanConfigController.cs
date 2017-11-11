using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Tradlite.Models.ScanConfig;

namespace Tradlite.Controllers
{
    public class ScanConfigController : Controller
    {
        private readonly IDbConnection _dbConnection;

        public ScanConfigController(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        [Route("api/scanconfig")]
        [HttpGet]
        public async Task<IReadOnlyList<ScanConfig>> Get()
        {
            var sql = @"select * from scanConfigs sc 
                        inner join tickerLists tl on sc.tickerListId = tl.id
                        inner join signalConfigs sc2 on sc.signalConfigId = sc2.id";
            var configs = await _dbConnection.QueryAsync<ScanConfigView>(sql);
            return configs.ToList();
        }
        
        [Route("api/scanconfig")]
        [HttpPost]
        public async Task Create([FromBody]ScanConfig scanConfig)
        {
            await _dbConnection.InsertAsync(scanConfig);
        }

        [Route("api/scanconfig")]
        [HttpPut]
        public async Task Update([FromBody]ScanConfig scanConfig)
        {
            await _dbConnection.UpdateAsync(scanConfig);
        }

        [Route("api/scanconfig/{id}")]
        [HttpDelete]
        public async Task Delete(int id)
        {
            await _dbConnection.DeleteAsync(new ScanConfig { Id = id });
        }
    }
}
