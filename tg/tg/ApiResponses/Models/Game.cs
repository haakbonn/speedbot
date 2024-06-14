using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tg.ApiResponses.Models.Intefaces;

namespace tg.ApiResponses.Models
{
    public class Game: IGameViewModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("releaseDate")]
        public string ReleaseDate { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        public string getName()
        {
            return Name;
        }

        public string getId()
        {
            return Id;
        }

        public string getReleaseDate()
        {
            return ReleaseDate;
        }

        public string getLink()
        {
            return Link;
        }
    }
}
