using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Game = tg.ApiResponses.Models.Game;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using tg.ApiResponses.Models;
using SpeedrunDotComAPI.Games;
using SpeedrunDotComAPI.Users;
using System.Text;
using tg.ApiResponses.Models.Intefaces;
using System.Xml.Linq;

namespace tg
{
    class Program
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string _apiEndpoint = "https://localhost:7162/api/Games";
        private static readonly Dictionary<long, List<string>> _userFavoriteGames = new Dictionary<long, List<string>>();

        static ITelegramBotClient bot = new TelegramBotClient("6988957161:AAGkbTF17cJoFJnQ5sjbHdc3YD7jceZwCJA");

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(update));

            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;

                if (message.Text.ToLower().StartsWith("/search "))
                {
                    var searchTerm = message.Text.Substring("/search ".Length).Trim();
                    await HandleSearchRequestAsync(botClient, message.Chat, searchTerm, cancellationToken);
                }
                else if (message.Text.ToLower() == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Привіт!\nДля перегляду команд використовувайте команду /help.\n");
                }
                else if (message.Text.ToLower().StartsWith("/addfavorite "))
                {
                    var searchTerm = message.Text.Substring("/addfavorite ".Length).Trim();
                    await HandleAddFavoriteGameAsync(botClient, message.Chat, searchTerm, cancellationToken);
                }
                else if (message.Text.ToLower().StartsWith("/removefavorite "))
                {
                    var searchTerm = message.Text.Substring("/removefavorite ".Length).Trim();
                    await HandleRemoveFavoriteGameAsync(botClient, message.Chat, searchTerm, cancellationToken);
                }
                else if (message.Text.ToLower() == "/listfavorites")
                {
                    await HandleListFavoriteGamesAsync(botClient, message.Chat, cancellationToken);
                }
                else if (message.Text.ToLower().StartsWith("/leaderboard "))
                {
                    var gameName = message.Text.Substring("/leaderboard ".Length).Trim();
                    await HandleGetLeaderboardAsync(botClient, message.Chat, gameName, cancellationToken);
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Для пошуку ігор використовуйте команду /search <назва гри>.\nДля того щоб додати гру в улюблені використовуйте команду /addfavorite <назва гри>.\nДля видалення гри з улюблених використовуйте команду /removefavorite <назва гри>.\nДля перегляду списка улюблених ігр використовуйте команду /listfavorites.\nДля перегляду таблиці рейтингу гравців гри використовуйте команду /leaderboard <назва гри>.");
                }
            }
        }

        public static async Task HandleSearchRequestAsync(ITelegramBotClient botClient, Chat chat, string searchTerm, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                await botClient.SendTextMessageAsync(chat, "Будь ласка, введіть назву гри для пошуку.");
                return;
            }

            try
            {
                var response = await _httpClient.GetAsync($"{_apiEndpoint}?search={searchTerm}");
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var gameResponseData = JsonConvert.DeserializeObject<GameResponseData>(responseBody);
               
                Console.WriteLine("Raw JSON response: " + responseBody);
                Console.WriteLine("Deserialized response: " + JsonConvert.SerializeObject(gameResponseData));

                if (gameResponseData?.Games == null || !gameResponseData.Games.Any())
                {
                    await botClient.SendTextMessageAsync(chat, $"Не знайдено жодної ігри '{searchTerm}'.");
                    return;
                }

                var games = gameResponseData.Games;
                string message = $"Знайдені ігри '{searchTerm}':\n{MakeTable(games.Cast<IGameViewModel>().ToList())}";
                await botClient.SendTextMessageAsync(chat, message, parseMode: ParseMode.Markdown);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка під час пошуку: {ex.Message}");
                await botClient.SendTextMessageAsync(chat, "Під час пошуку ігор сталася помилка. Будь-ласка спробуйте пізніше.");
            }
        }

        public static async Task HandleAddFavoriteGameAsync(ITelegramBotClient botClient, Chat chat, string gameName, CancellationToken cancellationToken)
        {
            try
            {
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(chat.Id.ToString()), "userId");
                content.Add(new StringContent(gameName), "gameName");

                HttpResponseMessage response = await _httpClient.PostAsync($"{_apiEndpoint}/favorite", content);

                string message = "";
                if (response.IsSuccessStatusCode)
                {
                    message = "Додано";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    message = "Гра не знайдена";
                }
                else {
                    message = "Помилка";
                }
                
                await botClient.SendTextMessageAsync(chat, message, parseMode: ParseMode.Markdown);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка під час пошуку вибраного: {ex.Message}");
                await botClient.SendTextMessageAsync(chat, "Під час пошуку улюблених ігор сталася помилка. Будь-ласка спробуйте пізніше.");
            }
        }

        public static async Task HandleRemoveFavoriteGameAsync(ITelegramBotClient botClient, Chat chat, string gameName, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiEndpoint}/favorite?userId={chat.Id}&gameName={Uri.EscapeDataString(gameName)}");

                string message = "";
                if (response.IsSuccessStatusCode)
                {
                    message = "Гра успішно видалена зі списку улюблених.";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    message = "Гру не знайдено.";
                }
                else
                {
                    message = "Виникла помилка при видаленні гри з обраного.";
                }

                await botClient.SendTextMessageAsync(chat, message, parseMode: ParseMode.Markdown);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка під час видалення улюбленої гри: {ex.Message}");
                await botClient.SendTextMessageAsync(chat, "Ой, сталася помилка при видаленні гри з вибраного. Спробуй пізніше.");
            }
        }


        public static async Task HandleListFavoriteGamesAsync(ITelegramBotClient botClient, Chat chat, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiEndpoint}/favorite/" + chat.Id);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var gameResponseData = JsonConvert.DeserializeObject<FavoriteGameResponseData>(responseBody);

                if (gameResponseData?.Games == null || !gameResponseData.Games.Any())
                {
                    await botClient.SendTextMessageAsync(chat, "У вас немає збережених ігор.");
                    return;
                }

                var games = gameResponseData.Games;

                string message = MakeFavoritesTable(games.Cast<IGameViewModel>().ToList());
                await botClient.SendTextMessageAsync(chat, message, parseMode: ParseMode.Markdown);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка під час отримання улюблених ігор: {ex.Message}");
                await botClient.SendTextMessageAsync(chat, "Виникла помилка при отриманні списку ваших улюблених ігор. Спробуй пізніше.");
            }
        }
        private static string MakeFavoritesTable(List<IGameViewModel> favoriteGames)
        {
            if (favoriteGames == null || !favoriteGames.Any())
            {
                return "У вас немає збережених ігор.";
            }

            var tableBuilder = new StringBuilder();
            tableBuilder.AppendLine("| # | Назва гри | Дата релізу |");
           

            int index = 1;
            foreach (var game in favoriteGames)
            {
                

                tableBuilder.AppendLine($"| {index} | {EscapeMarkdown(game.getName())} | {game.getReleaseDate()} |");
                index++;
            }

            return tableBuilder.ToString();
        }

        public static async Task HandleGetLeaderboardAsync(ITelegramBotClient botClient, Chat chat, string gameName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(gameName))
            {
                await botClient.SendTextMessageAsync(chat, "Пожалуйста, укажите название игры для получения лідерборду.");
                return;
            }

            try
            {
                var response = await _httpClient.GetAsync($"{_apiEndpoint}/{gameName}/leaderboards");
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                dynamic leaderboardData = JsonConvert.DeserializeObject(responseBody);

                if (leaderboardData == null)
                {
                    await botClient.SendTextMessageAsync(chat, "Лідерборд не найден.");
                    return;
                }

                var runs = leaderboardData.data.runs as IEnumerable<dynamic>;
                if (runs != null && runs.Any())
                {
                    string leaderboardTable = MakeLeaderboardTable(runs);
                    string message = $"Лідерборд для '{gameName}':\n{leaderboardTable}";

                    await botClient.SendTextMessageAsync(chat, message, parseMode: ParseMode.Html);
                }
                else
                {
                    Console.WriteLine($"Leaderboard data for game '{gameName}' is empty or invalid: {responseBody}");
                    await botClient.SendTextMessageAsync(chat, "Лідерборд для данной игры пуст.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during get leaderboard: {ex.Message}");
                await botClient.SendTextMessageAsync(chat, "Ой, произошла ошибка при получении лідерборду. Попробуйте позже.");
            }
        }

        private static string MakeTable(List<IGameViewModel> tableData)
        {
            if (tableData == null || tableData.Count == 0)
            {
                return "На ваш запит ігор не знайдено.";
            }

            string tableHeader =
                "| # | Id | Назва гри | Дата Релізу | Посилання |  |\n";              

            string tableBody = "";
            int index = 1;

            foreach (var game in tableData)
            {
                tableBody += $"| {index} | {game.getId()} | {EscapeMarkdown(game.getName())} | {game.getReleaseDate()} | " +
                    (!string.IsNullOrEmpty(game.getLink()) ? $"[відкрити]({game.getLink()})" : "    ") +
                    $" |\n";
                index++;
            }

            return $"{tableHeader}{tableBody}";
        }

        private static string MakeLeaderboardTable(IEnumerable<dynamic> runs)
        {
            string header = "| Позиція | Гравець | Час |\n";
            var rows = new StringBuilder();

            foreach (var run in runs)
            {
                string time = ConvertToTimeFormat(run.time.ToString());
                rows.AppendLine($"| {run.place} | {run.playerNames} | {time} |");
            }

            return header + rows.ToString();
        }
        private static string ConvertToTimeFormat(string time)
        {
            if (time.StartsWith("PT"))
            {
                try
                {
                    var duration = System.Xml.XmlConvert.ToTimeSpan(time);
                    return duration.ToString(@"hh\:mm\:ss");
                }
                catch (FormatException)
                {
                    return time; 
                }
            }

           
            if (TimeSpan.TryParse(time, out var timeSpan))
            {
                return timeSpan.ToString(@"hh\:mm\:ss");
            }

            return time;
        }
        private static string EscapeMarkdown(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Replace("_", "\\_").Replace("*", "\\*").Replace("[", "\\[").Replace("]", "\\]").Replace("`", "\\`");
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(JsonConvert.SerializeObject(exception));
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Запуск бота...");
            var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token
            );

            Console.WriteLine("Бот запущений.");
            Console.ReadLine();

            cts.Cancel();
        }
    }
}