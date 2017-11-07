using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Tradlite.Services
{
    public interface IHttpService
    {
        Task<JObject> Get(string url);
    }

    public class HttpService : IHttpService
    {
        public async Task<JObject> Get(string url)
        {
            var client = new WebClient();
            var json = await client.DownloadStringTaskAsync(url);
            return JObject.Parse(json);
        }
    }
}
