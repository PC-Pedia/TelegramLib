using System;
using TelegramLib;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TdLib;

namespace TelegramCmd
{
    public class Program
    {
        public static TelegramController tgClient;
        static async Task Main(string[] args)
        {
            //https://core.telegram.org/api/obtaining_api_id#obtaining-api-id - Obtaining api_id & api_hash
            tgClient = new TelegramController(new TelegramClientSettings("config/TelegramConfig.json"));
            tgClient.Run();
            Thread.Sleep(TimeSpan.FromSeconds(2));

            if (tgClient.UserLoginStatus != TelegramEnums.UserLoginStatus.Logined)
            {
                if (tgClient.UserLoginStatus == TelegramEnums.UserLoginStatus.LoginNeeded)
                    await tgClient.RequestCode("+1234567890"); 

                Thread.Sleep(TimeSpan.FromSeconds(2));
                if (tgClient.UserLoginStatus == TelegramEnums.UserLoginStatus.WaitForActivationCode)
                    await tgClient.SendCode("123456");

                Thread.Sleep(TimeSpan.FromSeconds(2));
                if (tgClient.UserLoginStatus == TelegramEnums.UserLoginStatus.WaitForPassword)
                    await tgClient.SendPassword("123qwe");

                Thread.Sleep(TimeSpan.FromSeconds(2));
                if (tgClient.UserLoginStatus == TelegramEnums.UserLoginStatus.Logined)
                    Console.WriteLine("logined!");
            }

            // get all chats info
            var allChats = new List<TdApi.Chat>();
            await foreach (var item in tgClient.GetChats())
                allChats.Add(item);

            // chat/channel/group to save file.
            var meChat = allChats.Where(c => c.Title == "ChatName").FirstOrDefault();

            // upload local file to saved messages
            var mesResult = tgClient.SendLocalFileToChat(meChat.Id, "sample/image.png", "imgae caption id123");

            //you can search message in chat by filename or by caption text;
            var fileMessage = await tgClient.SearchMessage(meChat.Id, "imgae caption id123");

            //download file attched to message
            var fileInfo = await tgClient.DownloadMessageFile(fileMessage);
            string localPath = fileInfo.Local.Path;

        }
    }
}
