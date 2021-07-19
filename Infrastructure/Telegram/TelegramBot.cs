using AuctionController.Infrastructure.JSON;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Message = Telegram.Bot.Types.Message;
//Используется: https://github.com/TelegramBots/telegram.bot

namespace AuctionController.Infrastructure.Telegram
{
    class TelegramBot
    {
        #region Singleton

        private static TelegramBot instance;
        public static TelegramBot GetInstance()
        {
            if (instance == null) instance = new TelegramBot();
            return instance;
        }

        #endregion

        static ITelegramBotClient botClient;

        public delegate bool ExecuteCommand(string commandName, long chatId, string shatName, List<string> attributes);
        public event ExecuteCommand onCommandExecute;

        //Dictionary<long, ITelegramBotCommand> activeCommands;

        TelegramBot()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string token = JSONConverter.OpenJSONFile<string>("token.json");
            botClient = new TelegramBotClient(token);

            //var me = botClient.GetMeAsync().Result;
            //activeCommands = new Dictionary<long, ITelegramBotCommand>();

            botClient.StartReceiving();
            //botClient.OnMessage += GetMessageFromChat;
        }
        ~TelegramBot()
        {
            botClient.StopReceiving();
        }

        #region UpdateLog

        public delegate void UpdateLog(string msg, bool isError = false);
        public event UpdateLog onLogUpdate;
        void AddLog(string msg, bool isError = false)
        {
            if (onLogUpdate != null)
            {
                onLogUpdate(msg, isError);
            }
        }

        #endregion

        #region Отправка сообщений
        public async void SendMessageToChat(string msg, long chatId)
        {
            try
            {
                await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: msg,
                parseMode: ParseMode.Markdown,
                disableNotification: true);
            }
            catch (System.Exception ex)
            {
                AddLog("ОШИБКА: " + ex.Message, true);
            }

        }
        #endregion

        #region Получение сообщений

        /*
        async void GetMessageFromchat(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                long chId = e.Message.Chat.Id;
                string msg = e.Message.Text;

                ITelegramBotCommand command;
                if (activeCommands.Count == 0) command = null;
                else command = activeCommands.Where(ac => ac.Key == chId).SingleOrDefault().Value;

                if (command != null) command.SetAttribute(msg);
                else
                {
                    command = TelegramBotCommand.InstantiateCommand(msg);
                    if (command != null) activeCommands.Add(chId, command);
                }

                if (command != null)
                {
                    string question = command.GetAttributeQuestion();
                    if (!string.IsNullOrEmpty(question)) SendMessageToChat(question, chId);

                    if (command.IsAllAttributesFilled)
                    {
                        //Execute
                        bool result = false;
                        if (onCommandExecute != null) result = onCommandExecute(command.CommandName, chId, e.Message.Chat.Username, command.Attributes);

                        if (result) SendMessageToChat(command.SuccessMessage + "\n🥳", chId);
                        else SendMessageToChat(command.FailMessage, chId);

                        activeCommands.Remove(chId);
                    }
                }
                else SendMessageToChat("Неизвестная команда", chId);

                //onMessage(e.Message.Chat.Id, e.Message.Text);
            }
        }
        */
        #endregion

    }
}
