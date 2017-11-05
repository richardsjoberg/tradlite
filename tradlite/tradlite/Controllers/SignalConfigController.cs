using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Tradlite.Models;

namespace Tradlite.Controllers
{
    public class SignalConfigController : Controller
    {
        private readonly IDbConnection _dbConnection;

        public SignalConfigController(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        [Route("api/signalconfig")]
        [HttpGet]
        public async Task<IReadOnlyList<SignalConfig>> Get()
        {
            var configs = await _dbConnection.GetAllAsync<SignalConfig>();
            return configs.ToList();
        }

        [Route("api/signalconfig/{type}")]
        [HttpGet]
        public async Task<IReadOnlyList<SignalConfig>> GetByType(string type)
        {
            var configs = await _dbConnection.QueryAsync<SignalConfig>("select * from dbo.signalconfigs where type=@type", new { type });
            return configs.ToList();
        }

        [Route("api/signalconfig")]
        [HttpPost]
        public async Task Create([FromBody]SignalConfig signalConfig)
        {
            await _dbConnection.InsertAsync(signalConfig);
        }

        [Route("api/signalconfig")]
        [HttpPut]
        public async Task Update([FromBody]SignalConfig signalConfig)
        {
            await _dbConnection.UpdateAsync(signalConfig);
        }

        [Route("api/signalconfig/{id}")]
        [HttpDelete]
        public async Task Delete(int id)
        {
            await _dbConnection.DeleteAsync(new SignalConfig { Id = id });
        }
    }
}
