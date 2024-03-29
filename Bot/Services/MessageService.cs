﻿using System.Text.RegularExpressions;
using Bot.Constants;
using Microsoft.Extensions.Options;

namespace Bot.Services;

public class MessageService
{
    private readonly ITelegramBotClient _bot;
    private readonly IAmazonSQS _sqsClient;
    private readonly ServicesSettings _servicesSettings;
    private static readonly Regex Mp4Regex = new("[^ ]*.mp4");
    private static readonly Regex Mp4LinkRegex = new("https?[^ ]*.mp4");

    public MessageService(ITelegramBotClient bot, IAmazonSQS sqsClient, IOptions<ServicesSettings> servicesSettings)
    {
        _bot = bot;
        _sqsClient = sqsClient;
        _servicesSettings = servicesSettings.Value;
    }

    public async Task HandleAsync(Message message)
    {
        if (message.Text?.StartsWith("/start") == true)
        {
            await _bot.SendTextMessageAsync(new(message.Chat.Id),
                "Send me a video or link to MP4 or add bot to group.");
        }
        else
        {
            await ProcessMessageAsync(message);
        }
    }

    private async Task ProcessMessageAsync(Message message)
    {
        await ExtractLinksFromTextAsync(message, message.Text);
        await ExtractLinksFromTextAsync(message, message.Caption);

        if (!string.IsNullOrEmpty(message.Document?.FileName) && Mp4Regex.IsMatch(message.Document.FileName))
        {
            await SendMessageAsync(message, fileId: message.Document.FileId);
        }

        if (message.Video != null)
        {
            await SendMessageAsync(message, fileId: message.Video.FileId);
        }
    }

    private async Task ExtractLinksFromTextAsync(Message message, string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            if (text.Contains("!nsfw", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            foreach (Match match in Mp4LinkRegex.Matches(text))
            {
                await SendMessageAsync(message, match.Value);
            }
        }
    }

    private async Task SendMessageAsync(Message receivedMessage, string link = null, string fileId = null)
    {
        var sentMessage = await _bot.SendTextMessageAsync(new(receivedMessage.Chat.Id),
            "File is waiting to be downloaded 🕒",
            replyToMessageId: receivedMessage.MessageId,
            disableNotification: true);

        var downloaderMessage = new DownloaderMessage(receivedMessage, sentMessage, link, fileId);

        await _sqsClient.SendMessageAsync(_servicesSettings.DownloaderQueueUrl,
            JsonSerializer.Serialize(downloaderMessage, JsonSerializerConstants.SerializerOptions));
    }
}