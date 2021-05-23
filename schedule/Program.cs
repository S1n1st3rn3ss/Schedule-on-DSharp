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
                // tokens and credentials stuff
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // api
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });


            // Define request parameters.
            String spreadsheetId = "placeholder";
            String range = GetNeededDate() + "!A3:P10";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(spreadsheetId, range);
            ValueRange response = request.Execute();
            IList<IList<Object>> values = response.Values;
            
            if (values != null && values.Count > 0)
            {
                FileStream StreamForThings = new FileStream("data.txt", FileMode.Create, FileAccess.Write, FileShare.None);
                StreamWriter StreamWrite = new StreamWriter(StreamForThings);
                foreach (var row in values)
                {
                    // Print columns A and E, which correspond to indices 0 and 4.
                    StreamWrite.WriteLine("{0} {1}", row[0], row[11]);
                }
                StreamWrite.Close();
                //StreamForThings.Close();
                Console.WriteLine("schedule worked");
                
                //string SchedResponse = ReaderForThing.ReadToEnd();
                //Console.WriteLine(ReaderForThing.ReadToEnd());
                StreamForThings.Close();
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
                Token = "placeholder",
                TokenType = TokenType.Bot
            });
            discord.MessageCreated += async (s, e) =>
            {

                if (e.Message.Content.ToLower().StartsWith("!расписание"))
                {

                    Console.WriteLine("we was sent");
                    Program.Schedule();
                    Console.WriteLine("schedule worked");
                    FileStream StreamForReading = new FileStream("data.txt", FileMode.Open, FileAccess.Read, FileShare.None);
                    StreamReader ReaderForThing = new StreamReader(StreamForReading);
                    string SchedResponse = await ReaderForThing.ReadToEndAsync();
                    await e.Message.RespondAsync(SchedResponse);
                    ReaderForThing.Close();
                    StreamForReading.Close();
                    Console.WriteLine(SchedResponse);
                }

            };

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}










