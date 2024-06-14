using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tg.ApiResponses.Models
{
    public class Pagination
    {
        [JsonProperty("nextPage")]
        public int? NextPage { get; set; }


        [JsonProperty("prevPage")]
        public int? PrevPage { get; set; }
    }
}
