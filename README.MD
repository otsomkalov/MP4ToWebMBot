**This project is archived and now a part of [ffmpeg-telegram-bot](https://github.com/otsomkalov/ffmpeg-telegram-bot)**

# MP4 to WebM Telegram Bot

Telegram Bot for converting MP4 videos to WebM. 

## Getting Started
### Prerequisites

- [.NET Core 3.1](https://dotnet.microsoft.com/download) or higher

### Installing

**Telegram:**
1. Contact to [@BotFather](https://t.me/BotFather) in Telegram
2. Create new bot
3. Copy bot token

**Project:**
1. Clone project
2. Run in folder
```
docker-compose run -d mp4towebmtelegrambot <telegram_bot_token>
```
or go to **MP4ToWebMBotTelegramBot** folder and run:
```
dotnet run <telegram_bot_token>
```


## Usage

You can try this bot in [Telegram](https://t.me/MP4ToWebMBot)


## Built With

* [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot) - .NET Client for Telegram Bot API
* [FFmpeg.NET](https://github.com/cmxl/FFmpeg.NET) - .NET wrapper for common ffmpeg tasks

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
