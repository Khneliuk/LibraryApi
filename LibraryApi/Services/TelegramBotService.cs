using System.Net.Http.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using LibraryApi.Models;

public class TelegramBotService : BackgroundService
{
    private readonly TelegramBotClient _bot;
    private readonly HttpClient _http;
    private static readonly Dictionary<long, string> _state = new();

    public TelegramBotService(IHttpClientFactory httpClientFactory)
    {
        _bot = new TelegramBotClient("8456396061:AAHTSSB9RJew8xRdH1KCy6mxvF33cpu3rKQ");
        _http = httpClientFactory.CreateClient("api");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _bot.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
            stoppingToken
        );

        Console.WriteLine("бот запущено");
        return Task.CompletedTask;
    }
    private ReplyKeyboardMarkup Menu() =>
        new ReplyKeyboardMarkup(new[]
        {
            new[] { new KeyboardButton("📚 Книги"), new KeyboardButton("➕ Додати") },
            new[] { new KeyboardButton("🔎 Пошук"), new KeyboardButton("✏️ Оновити") },
            new[] { new KeyboardButton("🗑 Видалити") }
        })
        { ResizeKeyboard = true };
    private string BuildBookList(List<SavedBook>? books)
    {
        if (books == null || books.Count == 0) return "";

        var result = "";
        foreach (var b in books)
        {
            result +=
                $"🆔 {b.Id}\n" +
                $"📖 {b.Title}\n" +
                $"👤 {string.Join(", ", b.Authors ?? new())}\n" +
                $"📌 {b.Status}\n" +
                $"⭐ {b.Rating}\n\n";
        }
        return result;
    }
    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        try
        {
            if (update.Message?.Text is not { } text)
                return;

            var chatId = update.Message.Chat.Id;
            Console.WriteLine($" [{chatId}]: {text}");
            if (text == "/start")
            {
                await bot.SendMessage(chatId, "📚Особиста бібліотека", replyMarkup: Menu(), cancellationToken: ct);
                return;
            }

            if (_state.TryGetValue(chatId, out var st))
            {
                if (st == "search")
                {
                    _state.Remove(chatId);

                    var books = await _http.GetFromJsonAsync<List<SavedBook>>(
                        $"api/books/search?query={Uri.EscapeDataString(text)}", ct);

                    var result = BuildBookList(books);

                    await bot.SendMessage(chatId,
                        result == "" ? "Нічого не знайдено" : result,
                        replyMarkup: Menu(), cancellationToken: ct);
                    return;
                }
                if (st == "update")
                {
                    var p = text.Split(',', StringSplitOptions.TrimEntries);

                    if (p.Length < 3)
                    {
                        await bot.SendMessage(chatId,
                            "Неправильний формат. Введіть:\nid, status, rating\n\nПриклад:\n1, read, 9",
                            cancellationToken: ct);
                        return;
                    }

                    _state.Remove(chatId);

                    var existing = await _http.GetFromJsonAsync<SavedBook>($"api/books/{p[0]}", ct);

                    if (existing == null)
                    {
                        await bot.SendMessage(chatId, "Книгу не знайдено", replyMarkup: Menu(), cancellationToken: ct);
                        return;
                    }

                    existing.Status = p[1];
                    existing.Rating = int.Parse(p[2]);

                    var res = await _http.PutAsJsonAsync($"api/books/{p[0]}", existing, ct);

                    await bot.SendMessage(chatId,
                        res.IsSuccessStatusCode ? "Оновлено" : "Помилка",
                        replyMarkup: Menu(), cancellationToken: ct);
                    return;
                }

                if (st == "delete")
                {
                    _state.Remove(chatId);

                    var res = await _http.DeleteAsync($"api/books/{text.Trim()}", ct);

                    await bot.SendMessage(chatId,
                        res.IsSuccessStatusCode ? "Видалено" : "Не знайдено",
                        replyMarkup: Menu(), cancellationToken: ct);
                    return;
                }
            }
            if (text == "📚 Книги")
            {
                var books = await _http.GetFromJsonAsync<List<SavedBook>>("api/books", ct);
                var result = BuildBookList(books);

                await bot.SendMessage(chatId,
                    result == "" ? "Бібліотека порожня" : result,
                    replyMarkup: Menu(), cancellationToken: ct);
            }

            else if (text == "➕ Додати")
            {
                await bot.SendMessage(chatId,
                    "Введіть:\nНазва, Автор, статус, рейтинг\n\nПриклад:\nDune, Frank Herbert, read, 10",
                    cancellationToken: ct);
            }

            else if (text.Contains(",") && text.Split(',').Length >= 4)
            {
                var p = text.Split(',', StringSplitOptions.TrimEntries);

                var res = await _http.PostAsJsonAsync("api/books", new
                {
                    title = p[0],
                    authors = new List<string> { p[1] },
                    status = p[2],
                    rating = int.Parse(p[3])
                }, ct);

                await bot.SendMessage(chatId,
                    res.IsSuccessStatusCode ? "Додано" : "Помилка",
                    replyMarkup: Menu(), cancellationToken: ct);
            }
            else if (text == "🔎 Пошук")
            {
                _state[chatId] = "search";
                await bot.SendMessage(chatId, "Введіть назву книги:", cancellationToken: ct);
            }
            else if (text == "✏️ Оновити")
            {
                _state[chatId] = "update";
                await bot.SendMessage(chatId,
                    "Введіть:\nid, status, rating\n\nПриклад:\n1, read, 9",
                    cancellationToken: ct);
            }
            else if (text == "🗑 Видалити")
            {
                _state[chatId] = "delete";
                await bot.SendMessage(chatId, "Введіть ID книги:", cancellationToken: ct);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            try
            {
                await bot.SendMessage(update.Message!.Chat.Id, $"Помилка: {ex.Message}", cancellationToken: ct);
            }
            catch { }
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
    {
        Console.WriteLine($"Bot error: {ex.Message}");
        return Task.CompletedTask;
    }
}