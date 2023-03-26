using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Gpt_Bot
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Escribe una consulta:");

            var Bot = new TelegramBotClient("YOUR_BOT_TOKEN_HERE");
            Bot.StartReceiving(Update, Error);
            Console.ReadLine();
        }

        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var gpt3 = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = "YOUR_CHATGPT_API_KEY_HERE"
            });
            var message = update.Message;

            if (message.Text != null)
            {
                var completionResult = await gpt3.Completions.CreateCompletion(new CompletionCreateRequest()
                {
                    Prompt = message.Text,
                    Model = Models.TextDavinciV3,
                    Temperature = 0.5F,
                    MaxTokens = 100
                });

                if (completionResult.Successful)
                {
                    foreach (var choice in completionResult.Choices)
                    {
                        if (update.Type != UpdateType.Message)
                            return;
                        var response = choice.Text;

                        if (message.Type != MessageType.Text)
                            return;

                        Console.WriteLine($"Mensaje recibido: {message.Text}");

                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat,
                            text: response
                        );
                    }
                }
                else
                {
                    if (completionResult.Error == null)
                    {
                        throw new Exception("Unknown Error");
                    }
                    Console.WriteLine($"{completionResult.Error.Code}: {completionResult.Error.Message}");
                }

            }
        }

        private static Task Error(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error: {exception.GetType().Name} - {exception.Message}");
            return Task.CompletedTask;
        }
    }
}