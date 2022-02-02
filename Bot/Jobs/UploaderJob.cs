using Bot.Constants;
using Microsoft.Extensions.Options;
using Telegram.Bot.Exceptions;
using File = System.IO.File;

namespace Bot.Jobs;

[DisallowConcurrentExecution]
public class UploaderJob : IJob
{
    private readonly ITelegramBotClient _bot;
    private readonly IAmazonSQS _sqsClient;
    private readonly ILogger<UploaderJob> _logger;
    private readonly ServicesSettings _servicesSettings;

    public UploaderJob(ITelegramBotClient bot, ILogger<UploaderJob> logger,
        IOptions<ServicesSettings> servicesSettings, IAmazonSQS sqsClient)
    {
        _bot = bot;
        _logger = logger;
        _sqsClient = sqsClient;
        _servicesSettings = servicesSettings.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var response = await _sqsClient.ReceiveMessageAsync(_servicesSettings.UploaderQueueUrl);
        var queueMessage = response.Messages.FirstOrDefault();

        if (queueMessage == null)
        {
            return;
        }

        var (receivedMessage, sentMessage, inputFilePath, outputFileName, thumbnailFilePath) =
            JsonSerializer.Deserialize<UploaderMessage>(queueMessage.Body)!;

        try
        {
            await _bot.EditMessageTextAsync(new(sentMessage.Chat.Id),
                sentMessage.MessageId,
                "Your file is uploading 🚀");

            var outputFilePath = Path.Combine(Path.GetTempPath(), outputFileName);

            await using (var videoStream = File.OpenRead(outputFilePath))
            await using (var imageStream = File.OpenRead(thumbnailFilePath))
            {
                await _bot.SendDocumentAsync(new(sentMessage.Chat.Id),
                    new InputMedia(videoStream, outputFileName),
                    replyToMessageId: receivedMessage.MessageId,
                    thumb: new(imageStream, thumbnailFilePath),
                    disableNotification: true);
            }

            await _bot.DeleteMessageAsync(new(sentMessage.Chat.Id),
                sentMessage.MessageId);

            var cleanerMessage = new CleanerMessage(inputFilePath, outputFilePath, thumbnailFilePath);

            await _sqsClient.SendMessageAsync(_servicesSettings.CleanerQueueUrl,
                JsonSerializer.Serialize(cleanerMessage, JsonSerializerConstants.SerializerOptions));

            await _sqsClient.DeleteMessageAsync(_servicesSettings.UploaderQueueUrl, queueMessage.ReceiptHandle);
        }
        catch (ApiRequestException telegramException)
        {
            _logger.LogError(telegramException, "Telegram error during Uploader execution:");
            await _sqsClient.DeleteMessageAsync(_servicesSettings.UploaderQueueUrl, queueMessage.ReceiptHandle);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during Uploader execution:");
        }
    }
}