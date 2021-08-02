# TelegramLib
Easy to use library to interact with [Telegram API](https://core.telegram.org/api#telegram-api)
- **TelegramClientService** - telegram client api service.
- **TelegramLib** - main library. It contains *TelegramController* which allows to login, send and get files from chats/channels/groups with understandable functions.
- **TelegramCmd** - small console client to show all user login and file send steps. 

To use Telegam Api you also need to obtain your [AppId and ApiHash](https://core.telegram.org/api/obtaining_api_id#obtaining-api-id)
**Remember** - It is forbidden to pass this values to third parties.

**Repo folders**
 - Net5 - primary framework , NuGet: [TDLib 1.6.0](https://www.nuget.org/packages/TDLib/1.6.0), [TDLib.Api 1.6.0](https://www.nuget.org/packages/TDLib.Api/1.6.0), [tdlib.native 1.6.0](https://github.com/ForNeVeR/tdlib.native/releases/tag/v1.6.0).
