using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace SheetsQuickstart
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static string ApplicationName = "Google Sheets API .NET Quickstart";
        static string NextSchoolDay;
        static string GetNeededDate()
        // Функция, определяющая следующий рабочий день
        {
            DateTime today = DateTime.Now;
            DateTime tomorrow;
            if (today.DayOfWeek == DayOfWeek.Saturday)
            {
                DateTime nextday = today.AddDays(2);
                tomorrow = nextday;
            }
            else
            {
                DateTime nextday = today.AddDays(1);
                tomorrow = nextday;
            }
            return NextSchoolDay = (string)tomorrow.ToString("dd.MM.yy");
        }

        //static void Main(string[] args)
        static void Schedule()
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // Токен в JSON, нужен для авторизации и чтения
                // НЕ ТРОГАТЬ!
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Подключение АПИ, что-то чисто Гугловское
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });


            // Define request parameters.
            String spreadsheetId = "1mAodHGjv2gSQacFuZPKzmj5UJhoSo21ipMJ9YgsCDjQ";
            String range = GetNeededDate() + "!A3:P10";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(spreadsheetId, range);
            ValueRange response = request.Execute();
            IList<IList<Object>> values = response.Values;


            if (values != null && values.Count > 0)
            {
                Console.WriteLine("Номер, Урок");
                foreach (var row in values)
                {
                    // Print columns A and E, which correspond to indices 0 and 4.
                    Console.WriteLine("{0} {1}", row[0], row[13]);
                    using (StreamWriter middleData = new ("E:/Sirius.Severstal/db/data.txt", true))
                    {
                        middleData.Write("{0} {1}" + "\r\n", row[0], row[13]);
                    }

                }
            }
            else
            {
                Console.WriteLine("No data found.");
            }


        }

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = "ODQyODMxMjE4NDk5NjQ5NTg3.YJ7BvQ.OiRF4i2Dt4nMzR76dw4178LdUrE",
                TokenType = TokenType.Bot
            });
            discord.MessageCreated += async (s, e) =>
            {
                File.Delete("E:/Sirius.Severstal/db/data.txt");
                Program.Schedule();
                string content;
                using (StreamReader middleData = new("E:/Sirius.Severstal/db/data.txt", true))
                {
                    content = middleData.ReadToEnd();
                }
                if (e.Message.Content.ToLower().StartsWith("!расписание"))
                    await e.Message.RespondAsync(content);
                File.Delete("E:/Sirius.Severstal/db/data.txt");


            };

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

    }
}
