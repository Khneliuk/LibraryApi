using System.Net.Http.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using LibraryApi.Models;
public class TelegramBotService : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private readonly HttpClient _http;

    private static readonly Dictionary<long, string> State = new();
    private static readonly Dictionary<long, List<SavedBook>> GoogleCache = new();

    public TelegramBotService(ITelegramBotClient bot, IHttpClientFactory httpClientFactory)
    {
        _bot = bot;
        _http = httpClientFactory.CreateClient("api");
    }

    protected override Task ExecuteAsync(CancellationToken ct)
    {
        _bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync,
            new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() }, ct);

        Console.WriteLine("Bot started");
        return Task.CompletedTask;
    }

    //меню
    private ReplyKeyboardMarkup Menu() =>
        new(new[]
        {
            new[] { new KeyboardButton("📚 Книги"), new KeyboardButton("➕ Додати") },
            new[] { new KeyboardButton("🔎 Пошук у бібліотеці"), new KeyboardButton("🌍 Google Books") },
            new[] { new KeyboardButton("✏️ Оновити"), new KeyboardButton("🗑 Видалити") }
        })
        { ResizeKeyboard = true };

    //пошук в бібліотеці користувача
    private InlineKeyboardMarkup LocalSearchMenu() =>
        new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📖 За назвою", "search_title"),
                InlineKeyboardButton.WithCallbackData("👤 За автором", "search_author")
            }
        });

    //пошук в гугл букс
    private InlineKeyboardMarkup GoogleSearchMenu() =>
        new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📖 За назвою", "google_title"),
                InlineKeyboardButton.WithCallbackData("👤 За автором", "google_author")
            }
        });
    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        long chatId;

        if (update.CallbackQuery is { } cb)
        {
            chatId = cb.Message!.Chat.Id;
            await bot.AnswerCallbackQuery(cb.Id, cancellationToken: ct);

            switch (cb.Data)
            {
                //пошук у своїй бібліотеці
                case "search_title":
                    State[chatId] = "search_title";
                    await bot.SendMessage(chatId, "Введіть назву книги:", cancellationToken: ct);
                    break;

                case "search_author":
                    State[chatId] = "search_author";
                    await bot.SendMessage(chatId, "Введіть ім'я автора:", cancellationToken: ct);
                    break;

                //пошук у гугл букс
                case "google_title":
                    State[chatId] = "google_title";
                    await bot.SendMessage(chatId, "Введіть назву книги для пошуку в Google Books:", cancellationToken: ct);
                    break;

                case "google_author":
                    State[chatId] = "google_author";
                    await bot.SendMessage(chatId, "Введіть ім'я автора для пошуку в Google Books:", cancellationToken: ct);
                    break;

                //додати книгу з гугл
                case var data when data != null && data.StartsWith("add_google_"):
                {
                    if (!GoogleCache.ContainsKey(chatId)) break;
                    if (!int.TryParse(data.Replace("add_google_", ""), out int idx)) break;
                    if (idx < 0 || idx >= GoogleCache[chatId].Count) break;

                    var book = GoogleCache[chatId][idx];
                    var resp = await _http.PostAsJsonAsync("api/books", book, ct);

                    GoogleCache.Remove(chatId);
                    State.Remove(chatId);

                    if (resp.IsSuccessStatusCode)
                        await bot.SendMessage(chatId, $"«{book.Title}» додано до бібліотеки!", replyMarkup: Menu(), cancellationToken: ct);
                    else
                        await bot.SendMessage(chatId, "Помилка при додаванні", replyMarkup: Menu(), cancellationToken: ct);
                    break;
                }
            }

            return;
        }
        if (update.Message?.Text is not { } text)
            return;
        chatId = update.Message.Chat.Id;
        //головне меню
        switch (text)
        {
            case "/start":
                await bot.SendMessage(chatId,
                    "Вітаємо в бібліотеці!\n\nОберіть дію:",
                    replyMarkup: Menu(), cancellationToken: ct);
                return;

            case "📚 Книги":
                var all = await _http.GetFromJsonAsync<List<SavedBook>>("api/books", ct);
                await SendLocalBooks(chatId, all, ct);
                return;

            case "🔎 Пошук у бібліотеці":
                State.Remove(chatId);
                await bot.SendMessage(chatId, "Як шукати?", replyMarkup: LocalSearchMenu(), cancellationToken: ct);
                return;

            case "🌍 Google Books":
                State.Remove(chatId);
                await bot.SendMessage(chatId, "Як шукати в Google Books?", replyMarkup: GoogleSearchMenu(), cancellationToken: ct);
                return;

            case "➕ Додати":
                State[chatId] = "add";
                await bot.SendMessage(chatId,
                    "📝 Введіть дані через кому:\n<b>Назва, Автор, Статус, Рейтинг</b>\n\n" +
                    "Приклад: <i>Кобзар, Тарас Шевченко, Прочитано, 5</i>",
                    parseMode: ParseMode.Html, cancellationToken: ct);
                return;

            case "✏️ Оновити":
                State[chatId] = "update";
                await bot.SendMessage(chatId,
                    "✏️ Введіть через кому:\n<b>ID, Статус, Рейтинг</b>\n\n" +
                    "Приклад: <i>3, Читаю, 4</i>",
                    parseMode: ParseMode.Html, cancellationToken: ct);
                return;

            case "🗑 Видалити":
                State[chatId] = "delete";
                await bot.SendMessage(chatId, "Введіть ID книги яку хочете видалити:", cancellationToken: ct);
                return;
        }
        if (!State.TryGetValue(chatId, out var state))
            return;

        //додати вручну
        if (state == "add")
        {
            var p = text.Split(',');
            if (p.Length < 4)
            {
                await bot.SendMessage(chatId, "Потрібно 4 значення через кому: Назва, Автор, Статус, Рейтинг", cancellationToken: ct);
                return;
            }

            if (!int.TryParse(p[3].Trim(), out int rating))
            {
                await bot.SendMessage(chatId, "Рейтинг має бути числом (1-10)", cancellationToken: ct);
                return;
            }

            var resp = await _http.PostAsJsonAsync("api/books", new SavedBook
            {
                Title = p[0].Trim(),
                Authors = new List<string> { p[1].Trim() },
                Status = p[2].Trim(),
                Rating = rating
            }, ct);

            State.Remove(chatId);

            if (resp.IsSuccessStatusCode)
                await bot.SendMessage(chatId, "Книгу додано", replyMarkup: Menu(), cancellationToken: ct);
            else
                await bot.SendMessage(chatId, "Помилка при додаванні", replyMarkup: Menu(), cancellationToken: ct);
            return;
        }

        //пошук в бібліотеці за назвою
        if (state == "search_title")
        {
            var books = await _http.GetFromJsonAsync<List<SavedBook>>(
                $"api/books/search?query={Uri.EscapeDataString(text)}", ct);

            await SendLocalBooks(chatId, books, ct);
            State.Remove(chatId);
            return;
        }

        //пошук в бібліотеці за автором
        if (state == "search_author")
        {
            var books = await _http.GetFromJsonAsync<List<SavedBook>>(
                $"api/books/search?query={Uri.EscapeDataString(text)}", ct);

            await SendLocalBooks(chatId, books, ct);
            State.Remove(chatId);
            return;
        }

        //пошук гугл букс за назвою
        if (state == "google_title")
        {
            await SearchGoogleBooks(chatId, text, byAuthor: false, ct);
            return;
        }

        //пошук гугл букс за автором
        if (state == "google_author")
        {
            await SearchGoogleBooks(chatId, text, byAuthor: true, ct);
            return;
        }

        //оновити
        if (state == "update")
        {
            var p = text.Split(',');
            if (p.Length < 3)
            {
                await bot.SendMessage(chatId, "Потрібно 3 значення: ID, Статус, Рейтинг", cancellationToken: ct);
                return;
            }

            if (!int.TryParse(p[0].Trim(), out int id))
            {
                await bot.SendMessage(chatId, "ID має бути числом", cancellationToken: ct);
                return;
            }

            if (!int.TryParse(p[2].Trim(), out int rating))
            {
                await bot.SendMessage(chatId, "Рейтинг має бути числом", cancellationToken: ct);
                return;
            }

            var book = await _http.GetFromJsonAsync<SavedBook>($"api/books/{id}", ct);
            if (book == null)
            {
                await bot.SendMessage(chatId, $"Книгу з ID {id} не знайдено", cancellationToken: ct);
                State.Remove(chatId);
                return;
            }

            book.Status = p[1].Trim();
            book.Rating = rating;

            var resp = await _http.PutAsJsonAsync($"api/books/{id}", book, ct);

            State.Remove(chatId);

            if (resp.IsSuccessStatusCode)
                await bot.SendMessage(chatId, "✏Книгу оновлено!", replyMarkup: Menu(), cancellationToken: ct);
            else
                await bot.SendMessage(chatId, "Помилка при оновленні", replyMarkup: Menu(), cancellationToken: ct);
            return;
        }

        //видалити
        if (state == "delete")
        {
            if (!int.TryParse(text.Trim(), out int id))
            {
                await bot.SendMessage(chatId, "Введіть числовий ID", cancellationToken: ct);
                return;
            }

            var resp = await _http.DeleteAsync($"api/books/{id}", ct);

            State.Remove(chatId);

            if (resp.IsSuccessStatusCode)
                await bot.SendMessage(chatId, "Книгу видалено!", replyMarkup: Menu(), cancellationToken: ct);
            else
                await bot.SendMessage(chatId, $"Книгу з ID {id} не знайдено", replyMarkup: Menu(), cancellationToken: ct);
            return;
        }
    }

    //пошук гугл букс
    private async Task SearchGoogleBooks(long chatId, string query, bool byAuthor, CancellationToken ct)
    {
        var escapedQuery = Uri.EscapeDataString(query);
        var endpoint = byAuthor
            ? $"api/books/google/author?author={escapedQuery}"
            : $"api/books/google?query={escapedQuery}";

        List<SavedBook>? books;
        try
        {
            books = await _http.GetFromJsonAsync<List<SavedBook>>(endpoint, ct);
        }
        catch
        {
            await _bot.SendMessage(chatId, "Помилка з'єднання з Google Books", cancellationToken: ct);
            State.Remove(chatId);
            return;
        }

        if (books == null || books.Count == 0)
        {
            await _bot.SendMessage(chatId, "Нічого не знайдено в Google Books", replyMarkup: Menu(), cancellationToken: ct);
            State.Remove(chatId);
            return;
        }

        GoogleCache[chatId] = books;
        State.Remove(chatId);

        await _bot.SendMessage(chatId, $"Знайдено {books.Count} книг(и) у Google Books:", cancellationToken: ct);

        for (int i = 0; i < books.Count; i++)
        {
            var b = books[i];
            var authors = b.Authors != null && b.Authors.Count > 0
                ? string.Join(", ", b.Authors)
                : "Невідомий автор";

            //анотація
            var description = !string.IsNullOrWhiteSpace(b.Description)
                ? (b.Description.Length > 300
                    ? b.Description[..300] + "..."
                    : b.Description)
                : "Опис відсутній";

            var msg =
                $"📖 <b>{b.Title}</b>\n" +
                $"👤 {authors}\n" +
                $"\n📝 <i>{description}</i>";

            var addButton = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("➕ Додати до бібліотеки", $"add_google_{i}")
                }
            });

            await _bot.SendMessage(chatId, msg,
                parseMode: ParseMode.Html,
                replyMarkup: addButton,
                cancellationToken: ct);
        }
    }

    //вивід книг з бібліотеки
    private async Task SendLocalBooks(long chatId, List<SavedBook>? books, CancellationToken ct)
    {
        if (books == null || books.Count == 0)
        {
            await _bot.SendMessage(chatId, "Не знайдено", replyMarkup: Menu(), cancellationToken: ct);
            return;
        }

        var msg = $"Знайдено {books.Count} книг(и):\n\n";

        foreach (var b in books)
        {
            var authors = b.Authors != null && b.Authors.Count > 0
                ? string.Join(", ", b.Authors)
                : "Невідомий автор";

            msg +=
                $"🆔 {b.Id}\n" +
                $"📖 {b.Title}\n" +
                $"👤 {authors}\n" +
                $"📌 {b.Status}\n" +
                $"⭐ {b.Rating}/10\n" +
                "\n";
        }
        const int limit = 4000;
        while (msg.Length > 0)
        {
            var chunk = msg.Length <= limit ? msg : msg[..limit];
            msg = msg.Length <= limit ? "" : msg[limit..];
            await _bot.SendMessage(chatId, chunk, parseMode: ParseMode.Html, cancellationToken: ct);
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
    {
        Console.WriteLine($"Bot error: {ex.Message}");
        return Task.CompletedTask;
    }
}