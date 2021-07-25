using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TdLib;
using TelegramLib;

namespace TelegramClientService.Controllers
{
    /// <summary>
    /// https://localhost:5001/swagger - Swagger tests
    /// https://localhost:5001/TelegramClient/Ping - req
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class TelegramClientController : ControllerBase
    {
        private readonly ILogger<TelegramClientController> _logger;
        private static TelegramController tgClient;
        private readonly int sleepMs = 30;
        private static readonly string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        private static readonly string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
        private readonly string configName = "ClientSettings.json";

        public TelegramClientController(ILogger<TelegramClientController> logger)
        {
            _logger = logger;
        }

        [HttpGet("Ping")]
        public string Ping()
        {
            return DateTime.Now.ToString();
        }

        /// <summary>
        /// Get login status of user.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetUserLoginStatus")]
        public string GetUserLoginStatus()
        {
            try
            {
                return tgClient.UserLoginStatus.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        /// <summary>
        /// Set appId & ApiHash to service config
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="ApiHash"></param>
        /// <returns></returns>
        [HttpGet("SetControllerConfig")]
        public string SetControllerConfig(string appId, string ApiHash)
        {
            try
            {
                var settings = new TelegramClientSettings { ApiId = Convert.ToInt32(appId), ApiHash = ApiHash };
                
                if (!System.IO.File.Exists(configName))
                {
                    string json = JsonConvert.SerializeObject(settings);
                    System.IO.File.WriteAllText(Path.Combine(strWorkPath, configName), json);
                }
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        /// <summary>
        /// Run TelegramController.
        /// </summary>
        /// <returns></returns>
        [HttpGet("RunTelegramController")]
        public string RunTelegramController()
        {
            try
            {
                if (System.IO.File.Exists(configName))
                {
                    var json = System.IO.File.ReadAllText(Path.Combine(strWorkPath, configName));
                    var config = JsonConvert.DeserializeObject<TelegramClientSettings>(json);
                    tgClient = new TelegramController(config);
                    tgClient.Run();

                    Thread.Sleep(sleepMs);
                    return tgClient.UserLoginStatus.ToString();
                }
                else return $"{configName} not found!";
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        /// <summary>
        /// Send user phone
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet("SendLoginPhone")]
        public async Task<string> SetLoginPhoneAsync(string phone)
        {
            try
            {
                await tgClient.RequestCode(phone);
                Thread.Sleep(sleepMs);
                return tgClient.UserLoginStatus.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        /// <summary>
        /// Send confirm code from app.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet("SendConfirmCode")]
        public async Task<string> SendConfirmCode(string code)
        {
            try
            {
                await tgClient.SendCode(code);
                Thread.Sleep(sleepMs);
                return tgClient.UserLoginStatus.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        /// <summary>
        /// Send password if needed.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet("SendPassword")]
        public async Task<string> SendPassword(string pass)
        {
            try
            {
                await tgClient.SendPassword(pass);
                Thread.Sleep(sleepMs);
                return tgClient.UserLoginStatus.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        /// <summary>
        /// Get basic info about user chats.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetChats")]
        public async Task<Dictionary<string, long>> GetChats()
        {
            try
            {
                List<TdApi.Chat> chats = await tgClient.GetChats();
                return chats.ToDictionary(c => c.Title, c => c.Id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Send file to Telegram.
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="filePath"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        [HttpGet("SendLocalFile")]
        public async Task<string> SendLocalFile(string chatId, string filePath, string caption)
        {
            try
            {
                var id = Convert.ToInt64(chatId);
                var mesResult = await tgClient.SendLocalFileToChatAsync(id, filePath, caption);
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }
    }
}
