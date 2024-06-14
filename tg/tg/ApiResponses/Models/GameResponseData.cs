using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tg.ApiResponses.Models.Intefaces;

namespace tg.ApiResponses.Models
{
    public class GameResponseData
    {
        [JsonProperty("data")]
        public List<Game> Games { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }
    }
}
