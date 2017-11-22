using dto.endpoint.browse;
using System.Collections.Generic;

namespace Tradlite.Models.Ig
{
    public class BrowseResponse
    {
        public List<HierarchyNode> Nodes { get; set; }
        public List<HierarchyMarket> Markets { get; set; }
    }
}
