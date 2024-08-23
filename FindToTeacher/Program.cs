using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;
using System.Data;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;

using AngleSharp;
using AngleSharp.Dom;

namespace FindYourTeacher
{
    internal class Program
    {
        private static readonly Dictionary<long, string> userCommands = new Dictionary<long, string>();
        private class TimeVariable
        {
            public int TimeIndex;
            public int DayIndex;
            public string WeekParity;
        }

        private readonly static string[] Teachers = FillTeachers(); // лист преподавателей
        private readonly static string[] LessonTime = FillLessonTime(); // лист времени занятий

        private static string[] FillTeachers() // заполнение листа преподавателей с сайта
        {
            var document = GetHtmlCode("https://imsit.ru/timetable/teach/Praspisan.html");
            return document.QuerySelectorAll("tr")
                           .Select(row => row.QuerySelector("td").TextContent.Trim())
                           .Select(text => text.Split(',')[0])
                           .ToArray();
        }

        private static string[] FillLessonTime() // заполнение листа времени занятий
        {
            var document = GetHtmlCode("https://imsit.ru/timetable/teach/m1.html");
            return document.QuerySelectorAll("tr")[1].QuerySelectorAll("td").Select(td => td.TextContent).ToArray();
        }

        private static IDocument GetHtmlCode(string link) // получение html документа
        { 
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            return context.OpenAsync(link).Result;
        }

        private static TimeVariable TimeRequestInfo() // получение данных о времени при запросе от user
        {
            int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

            Dictionary<int, TimeSpan> TimeIndex = new Dictionary<int, TimeSpan>() {
                {8, new TimeSpan(19, 40, 0)},
                {7, new TimeSpan(18, 10, 0)},
                {6, new TimeSpan(16, 30, 0) },
                {5, new TimeSpan(14, 50, 0) },
                {4, new TimeSpan(13, 10, 0) },
                {3, new TimeSpan(11, 30, 0) },
                {2, new TimeSpan(9, 40, 0) },
                {1, new TimeSpan(0, 0, 0) },
            };

            int index = 0;
            foreach (var S in TimeIndex)
            {
                if (DateTime.Now.TimeOfDay >= S.Value)
                {
                    index = S.Key;
                    break;
                }
            }

            return new TimeVariable()
            {
                TimeIndex = index,
                DayIndex = DateTime.Now.DayOfWeek == 0 ? 8 : (int)DateTime.Now.DayOfWeek + 1,
                WeekParity = weekNumber % 2 == 0 ? "четная" : "нечетная"
            };
        }

        private static string GetLinkFromTeacher(Message message) // получение ссылки по ФИО преподавателя
        {
            string link = null;
            for (int i = 0; i < Teachers.Length; i++)
            {
                if (message.Text == Teachers[i] || message.Text == Teachers[i].Split(' ')[0]) 
                    return $"https://imsit.ru/timetable/teach/m{i}.html";
            }
            return link;
        }

        private static string GetCurrentList(IDocument document) // получение текущей аудитории
        {
            TimeVariable timeVariable = TimeRequestInfo();

            string tableSelector = timeVariable.WeekParity == "четная" ? "body > table:nth-child(3)" : "body > table:nth-child(6)";
            var table = document.QuerySelector(tableSelector);

            var ResultRequest = table.QuerySelectorAll("tr")[timeVariable.DayIndex]
                                     .QuerySelectorAll("td")[timeVariable.TimeIndex];

            return CellOutOfRangeException(timeVariable) == false ? LessonTime[timeVariable.TimeIndex] + ResultRequest.TextContent
                                                                  : "Сейчас ни у кого нет занятий. Все отдыхают!";
        }

        private static string GetDayList(IDocument document) // получение расписания дня
        {
            TimeVariable timeVariable = TimeRequestInfo();

            string tableSelector = timeVariable.WeekParity == "четная" ? "body > table:nth-child(3)" : "body > table:nth-child(6)";
            var table = document.QuerySelector(tableSelector);

            var DayContainer = table.QuerySelectorAll("tr")[timeVariable.DayIndex]
                                    .QuerySelectorAll("td").Select(td => td.TextContent.Trim()).ToArray();

            string ResultRequest = "";
            for (int i = 1; i <= 7; i++)
                ResultRequest += LessonTime[i] + '\n' + DayContainer[i] + '\n';

            return CellOutOfRangeException(timeVariable) == false ? ResultRequest
                                                                  : "Сейчас ни у кого нет занятий. Все отдыхают!";
        }

        private static bool CellOutOfRangeException(TimeVariable time) { // проверка на воскресенье или окончание дня
            if (time.DayIndex == 8 || time.TimeIndex == 8)
                return true;

            return false;
        }

        static void Main()
        {
            ITelegramBotClient bot;
            
            bot = new TelegramBotClient("PnjhT0feWMCjSrvc"); // инициализация токена
            var Token = new CancellationTokenSource().Token; // получение токена
            var Options = new ReceiverOptions { AllowedUpdates = { } }; //обновление всех типов данных 
            bot.StartReceiving(UPDATE, ERROR, Options, Token); // запуск работы бота

            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName); 
            Console.ReadKey();
        }

        public static async Task UPDATE(ITelegramBotClient bot, Update update, CancellationToken Token) //отвечает за обновления
        {
            var message = update.Message;
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update)); // вывод данных о user

            if (message.Text == "/start") // блок возврата к началу
            {
                if(userCommands.ContainsKey(message.Chat.Id))
                    userCommands.Remove(message.Chat.Id);

                await bot.SendTextMessageAsync(message.Chat, "Добро пожаловать! Введите свою команду, а затем укажите ФИО преподавателя");
                return;
            }

            if (message.Text == "/now" || message.Text == "/list" || message.Text == "/link") // блок сохранения команды пользователя
            {
                userCommands[message.Chat.Id] = message.Text;
                await bot.SendTextMessageAsync(message.Chat, "Команда учтена! Введите ФИО преподавателя");
            }

            else if (userCommands.ContainsKey(message.Chat.Id) && GetLinkFromTeacher(message) != null) // блок вывода расписания
            {
                string link = GetLinkFromTeacher(message);
                IDocument document = GetHtmlCode(link);

                switch (userCommands[message.Chat.Id])
                {
                    case "/now":
                        await bot.SendTextMessageAsync(message.Chat, GetCurrentList(document));
                        break;
                    case "/list":
                        await bot.SendTextMessageAsync(message.Chat, GetDayList(document));
                        break;
                    case "/link":
                        await bot.SendTextMessageAsync(message.Chat, link);
                        break;
                }
            }

            else // блок обработки ошибки
            {
                await bot.SendTextMessageAsync(message.Chat, "Неизвестная команда [неверное имя или команда стерта] или отсутствие преподавателя в списках");
            }
        }

        public static async Task ERROR(ITelegramBotClient bot, Exception exception, CancellationToken Token) //отвечает за ошибки 
        {
            await bot.SendTextMessageAsync(exception.Message, "ОшибОчка", cancellationToken: Token);
        }
    }
}