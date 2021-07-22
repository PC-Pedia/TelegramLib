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
            //https://core.telegram.org/api/obtaining_api_id#obtaining-api-id - Obtaining ApiId & ApiHash

            Console.WriteLine("Enter Telegram ApiId:");
            int appID = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Enter Telegram ApiHash:");
            string apiHash = Console.ReadLine();

            tgClient = new TelegramController(new TelegramClientSettings { ApiId = appID, ApiHash = apiHash });
            tgClient.Run();
            Thread.Sleep(TimeSpan.FromSeconds(2));

            if (tgClient.UserLoginStatus != TelegramEnums.UserLoginStatus.Logined)
            {
                if (tgClient.UserLoginStatus == TelegramEnums.UserLoginStatus.LoginNeeded)
                {
                    Console.WriteLine("Enter your telegram phone number (+123456789):");
                    string phone = Console.ReadLine();
                    await tgClient.RequestCode(phone);
                }

                Thread.Sleep(TimeSpan.FromSeconds(2));
                if (tgClient.UserLoginStatus == TelegramEnums.UserLoginStatus.WaitForActivationCode)
                {
                    Console.WriteLine("Enter confirm code:");
                    string code = Console.ReadLine();
                    await tgClient.SendCode(code);
                }

                Thread.Sleep(TimeSpan.FromSeconds(2));
                if (tgClient.UserLoginStatus == TelegramEnums.UserLoginStatus.WaitForPassword)
                {
                    Console.WriteLine("Enter password:");
                    string pass = Console.ReadLine();
                    await tgClient.SendPassword(pass);
                }

                Thread.Sleep(TimeSpan.FromSeconds(2));
                if (tgClient.UserLoginStatus == TelegramEnums.UserLoginStatus.Logined)
                    Console.WriteLine("Logined!");
            }

            // get all chats info
            var allChats = await tgClient.GetChats();
            Console.WriteLine($"Chats count: {allChats.Count}");
            foreach (var c in allChats)
                Console.WriteLine(c.Title);

            // chat/channel/group to save file.
            Console.WriteLine("Enter chat name to send file:");
            string name = Console.ReadLine();

            var meChat = allChats.Where(c => c.Title == name).FirstOrDefault();

            if (meChat == null)
                Console.WriteLine("chat not found!");
            // upload local file to saved messages
            var mesResult = tgClient.SendLocalFileToChat(meChat.Id, "sample/image.png", "imgae caption id123");
            Console.WriteLine("File sended!");

            //you can search message in chat by filename or by caption text;
            var fileMessage = await tgClient.SearchMessage(meChat.Id, "imgae caption id123");

            //download file attched to message
            var fileInfo = await tgClient.DownloadMessageFile(fileMessage);
            Console.WriteLine($"File received: {fileInfo.Local.Path}");
        }
    }
}
