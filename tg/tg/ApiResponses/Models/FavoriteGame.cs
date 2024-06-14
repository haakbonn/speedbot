using Microsoft.Extensions.Configuration.UserSecrets;
using SpeedrunDotComAPI.Categories;
using SpeedrunDotComAPI.Games;
using SpeedrunDotComAPI.Runs;
using tg.ApiResponses.Models.Intefaces;

namespace tg.ApiResponses.Models
{
    public class FavoriteGame: IGameViewModel
    {
        public Int64 Id { get; set; } 
        public string UserId { get; set; } 
        public string GameId { get; set; }
        public string GameName { get; set; }
        public DateOnly? GameDate { get; set; }

        public FavoriteGame(
            Int64 id,
            string gameId,
            string gameName,
            DateTime gameDate
        )
        {
            Id = id;
            GameId = gameId;
            GameName = gameName;
            GameDate = DateOnly.FromDateTime(gameDate);
        }

        public string getName()
        {
            return GameName;
        }

        public string getId()
        {
            return Id.ToString();
        }

        public string getReleaseDate()
        {
            return GameDate.ToString();
        }

        public string getLink()
        {
            return ""; 
        }
    }
}