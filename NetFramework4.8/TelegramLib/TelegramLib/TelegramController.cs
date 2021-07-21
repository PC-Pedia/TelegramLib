using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TdLib;
using static TdLib.TdApi;
using static TdLib.TdApi.InputFile;
using static TdLib.TdApi.InputMessageContent;
using static TdLib.TdApi.MessageContent;
using static TelegramLib.TelegramEnums;

namespace TelegramLib
{
    public class TelegramController
    {
        private TdClient _client;
        public TelegramClientSettings Settings;

        public ClientStatus ClientStatus;
        public UserLoginStatus UserLoginStatus;

        private static readonly ManualResetEventSlim ResetEvent = new ManualResetEventSlim();

        public TelegramController(TelegramClientSettings settings)
        {
            _client = new TdClient();
            Settings = settings;
            ClientStatus = ClientStatus.NotStarted;
            UserLoginStatus = UserLoginStatus.Unauthorized;
        }

        public void Run()
        {
            _client = new TdClient();
            _client.UpdateReceived += async (sender, update) => await Client_UpdateReceivedAsync(sender, update);

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
            }).Start();

            ClientStatus = ClientStatus.Started;
            ResetEvent.Wait();
        }

        /// <summary>
        /// update handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        private async Task Client_UpdateReceivedAsync(object sender, TdApi.Update update)
        {
            switch (update)
            {
                //case TdApi.Update.UpdateOption option:
                //    await _client.ExecuteAsync(new TdApi.SetOption
                //    {
                //        DataType = option.DataType,
                //        Extra = option.Extra,
                //        Name = option.Name,
                //        Value = option.Value
                //    });
                //    break;
                case TdApi.Update.UpdateAuthorizationState updateAuthorizationState when updateAuthorizationState.AuthorizationState.GetType() == typeof(TdApi.AuthorizationState.AuthorizationStateWaitTdlibParameters):
                    await _client.ExecuteAsync(new TdApi.SetTdlibParameters
                    {
                        Parameters = new TdApi.TdlibParameters
                        {
                            ApiId = Settings.ApiId,
                            ApiHash = Settings.ApiHash,
                            ApplicationVersion = "1.0.0",
                            DeviceModel = "PC",
                            SystemLanguageCode = "en",
                            SystemVersion = "Win 10.0"
                        }
                    });
                    break;
                case TdApi.Update.UpdateAuthorizationState updateAuthorizationState when updateAuthorizationState.AuthorizationState.GetType() == typeof(TdApi.AuthorizationState.AuthorizationStateWaitEncryptionKey):
                    await _client.ExecuteAsync(new TdApi.CheckDatabaseEncryptionKey());
                    break;
                case TdApi.Update.UpdateAuthorizationState updateAuthorizationState when updateAuthorizationState.AuthorizationState.GetType() == typeof(TdApi.AuthorizationState.AuthorizationStateWaitPhoneNumber):
                    UserLoginStatus = UserLoginStatus.LoginNeeded;
                    ResetEvent.Set();
                    break;
                case TdApi.Update.UpdateAuthorizationState updateAuthorizationState when updateAuthorizationState.AuthorizationState.GetType() == typeof(TdApi.AuthorizationState.AuthorizationStateWaitCode):
                    UserLoginStatus = UserLoginStatus.WaitForActivationCode;
                    ResetEvent.Set();
                    break;
                case TdApi.Update.UpdateAuthorizationState updateAuthorizationState when updateAuthorizationState.AuthorizationState.GetType() == typeof(TdApi.AuthorizationState.AuthorizationStateWaitPassword):
                    UserLoginStatus = UserLoginStatus.WaitForPassword;
                    ResetEvent.Set();
                    break;
                case TdApi.Update.UpdateAuthorizationState updateAuthorizationState when updateAuthorizationState.AuthorizationState.GetType() == typeof(TdApi.AuthorizationState.AuthorizationStateReady):
                    UserLoginStatus = UserLoginStatus.Logined;
                    ResetEvent.Set();
                    break;
                case TdApi.Update.UpdateUser updateUser:
                    ResetEvent.Set();
                    break;
                case TdApi.Update.UpdateConnectionState updateConnectionState when updateConnectionState.State.GetType() == typeof(TdApi.ConnectionState.ConnectionStateReady):
                    ResetEvent.Set();
                    break;
                case TdApi.Update.UpdateNewMessage updatedmessage:
                    break;
                default:
                    ; // add a breakpoint here to see other events
                    break;
            }
        }

        /// <summary>
        /// Request login code
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public async Task RequestCode(string phone)
        {
            await _client.ExecuteAsync(new TdApi.SetAuthenticationPhoneNumber
            {
                PhoneNumber = phone
            });
        }

        /// <summary>
        /// Enter login code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task SendCode(string code)
        {
            await _client.ExecuteAsync(new TdApi.CheckAuthenticationCode
            {
                Code = code
            });
        }

        /// <summary>
        /// Enter password
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task SendPassword(string password)
        {
            await _client.ExecuteAsync(new TdApi.CheckAuthenticationPassword
            {
                Password = password
            });
        }

        /// <summary>
        /// Get all user chats/channels with basic info
        /// </summary>
        /// <param name="offsetOrder"></param>
        /// <param name="offsetId"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<TdApi.Chat> GetChats(long offsetOrder = long.MaxValue, long offsetId = 0, int limit = 1000)
        {
            var chats = await _client.ExecuteAsync(new TdApi.GetChats { OffsetOrder = offsetOrder, Limit = limit, OffsetChatId = offsetId });
            foreach (var chat in chats.ChatIds)
            {
                Debug.WriteLine(chat);
            }
            foreach (var chatId in chats.ChatIds)
            {
                var chat = await _client.ExecuteAsync(new TdApi.GetChat { ChatId = chatId });
                if (chat.Type is TdApi.ChatType.ChatTypeSupergroup || chat.Type is TdApi.ChatType.ChatTypeBasicGroup || chat.Type is TdApi.ChatType.ChatTypePrivate)
                {
                    yield return chat;
                }
            }
        }

        /// <summary>
        /// Sends local file to selected chat
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="filePath"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public Message SendLocalFileToChat(long chatId, string filePath, string caption)
        {
            try
            {
                return _client.SendMessageAsync(chatId, 0, options: new SendMessageOptions { DisableNotification = true },
                inputMessageContent: new InputMessageDocument
                {
                    Caption = new FormattedText { Text = caption },
                    Document = new InputFileLocal
                    {
                        Path = filePath
                    }
                }).Result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Search message by filename or text/caption
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="query"></param>
        /// <param name="senderUserId"></param>
        /// <returns></returns>
        public async Task<Message> SearchMessage(long chatId, string query, int senderUserId = 0)
        {
            long fromMessage = 0;
            int limit = 50;
            var message = await _client.SearchChatMessagesAsync(chatId, query, senderUserId: senderUserId, fromMessage, 0, limit);
            return message.Messages_.FirstOrDefault();
        }

        /// <summary>
        /// Download file from message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="saveLocation"></param>
        /// <returns></returns>
        public async Task<File> DownloadMessageFile(Message message)
        {
            if (message.Content.GetType() != typeof(TdLib.TdApi.MessageContent.MessageDocument))
                return null;

            var docInfo = (MessageDocument)message.Content;
            return await DownloadRemoteFile(docInfo.Document.Document_);
        }
        
        /// <summary>
        /// Save document from file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="saveLocation"></param>
        /// <returns></returns>
        public async Task<File> DownloadRemoteFile(File file)
        {
            int filePart = 512 * 1024;
            int offset = 0;
      
            File locFile = null;
            while (offset < file.Remote.UploadedSize)
            {
                locFile = await _client.DownloadFileAsync(file.Id, 1, offset, filePart, true);
                offset += filePart;
            }
            return locFile;
        }

        /// <summary>
        /// Resends remote file to chat
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="remoteFileId"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public async Task<Message> SendRemoteFileMessageToChat(long chatId, string remoteFileId, string caption)
        {
            try
            {
                return await _client.SendMessageAsync(chatId: chatId, options: new SendMessageOptions { DisableNotification = true },
                inputMessageContent: new InputMessageDocument
                {
                    Caption = new FormattedText { Text = caption },
                    Document = new InputFileRemote
                    {
                        Id = remoteFileId
                    }
                });
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Resend document from message
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="message"></param>
        /// <param name="caption"></param>
        public async Task<Message> ResendMessageDocument(long chatId, Message message, string caption)
        {
            var doc = (MessageDocument)message.Content;
            return await _client.SendMessageAsync(chatId, 0, options: new SendMessageOptions { DisableNotification = true },
            inputMessageContent: new InputMessageDocument
            {
                Caption = new FormattedText { Text = caption },
                Document = new InputFileRemote
                {
                    Id = doc.Document.Document_.Remote.Id
                }
            });
        }

        /// <summary>
        /// Get remote file info by id
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public async Task<File> GetremoteFile(string fileId)
        {
            return await _client.GetRemoteFileAsync(fileId);
        }
        
        /// <summary>
        /// Get full chat by id
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        public async Task<Chat> GetFullChat(long chatId)
        {
            return await _client.GetChatAsync(chatId);
        }

        /// <summary>
        /// Get full cahts list
        /// </summary>
        /// <param name="chats"></param>
        /// <returns></returns>
        public async Task<List<Chat>> GetFullChats(List<long> chats)
        {
            var fullChats = new List<Chat>();
            foreach (var chatId in chats)
            {
                var chat = await _client.ExecuteAsync(new TdApi.GetChat { ChatId = chatId });
                if (chat.Type is TdApi.ChatType.ChatTypeSupergroup || chat.Type is TdApi.ChatType.ChatTypeBasicGroup || chat.Type is TdApi.ChatType.ChatTypePrivate)
                {
                    fullChats.Add(chat);
                }
            }
            return fullChats;
        }

        /// <summary>
        /// Get all messages from chat
        /// </summary>
        /// <param name="chat"></param>
        /// <returns></returns>
        public async Task<List<Message>> GetAllChatMessages(TdApi.Chat chat)
        {
            var all = new List<Message>();
            var first = await _client.GetChatHistoryAsync(chat.Id, 0, 0, 100, false);
            all.Add(first.Messages_[0]);

            long fromMessage = first.Messages_[0].Id;
            int limit = 50;
            bool contimue = true;
            while (contimue)
            {
                var mes = await _client.GetChatHistoryAsync(chat.Id, fromMessage, 0, limit, false);
                all.AddRange(mes.Messages_);
                fromMessage = mes.Messages_[mes.TotalCount - 1].Id;

                if (mes.TotalCount < limit)
                    contimue = false;
            }
            return all;
        }
    }
}
