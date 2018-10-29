using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Tradlite.Models.ScanConfig;
using Tradlite.Services.SqlConnectionFactory;

namespace Tradlite.Controllers
{
    public class ScanConfigController : Controller
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public ScanConfigController(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }
        [Route("api/scanconfig")]
        [HttpGet]
        public async Task<IReadOnlyList<ScanConfig>> Get()
        {
            using (var _dbConnection = _sqlConnectionFactory.CreateConnection())
            {
                var sql = @"select * from scanConfigs sc 
                        inner join tickerLists tl on sc.tickerListId = tl.id
                        inner join signalConfigs sc2 on sc.signalConfigId = sc2.id";
                var configs = await _dbConnection.QueryAsync<ScanConfigView>(sql);
                return configs.ToList();
            }
        }
        
        [Route("api/scanconfig")]
        [HttpPost]
        public async Task Create([FromBody]ScanConfig scanConfig)
        {
            using (var _dbConnection = _sqlConnectionFactory.CreateConnection())
            {
                await _dbConnection.InsertAsync(scanConfig);
            }
        }

        [Route("api/scanconfig")]
        [HttpPut]
        public async Task Update([FromBody]ScanConfig scanConfig)
        {
            using (var _dbConnection = _sqlConnectionFactory.CreateConnection())
            {
                await _dbConnection.UpdateAsync(scanConfig);
            }
        }

        [Route("api/scanconfig/{id}")]
        [HttpDelete]
        public async Task Delete(int id)
        {
            using (var _dbConnection = _sqlConnectionFactory.CreateConnection())
            {
                await _dbConnection.DeleteAsync(new ScanConfig { Id = id });
            }
        }
    }
}
