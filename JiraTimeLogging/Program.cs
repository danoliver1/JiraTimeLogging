using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace JiraTimeLogging
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            const string header = @"
       _ _____ _____              _______ _____ __  __ ______   ________   _________ _____            _____ _______ 
      | |_   _|  __ \     /\     |__   __|_   _|  \/  |  ____| |  ____\ \ / /__   __|  __ \     /\   / ____|__   __|
      | | | | | |__) |   /  \       | |    | | | \  / | |__    | |__   \ V /   | |  | |__) |   /  \ | |       | |   
  _   | | | | |  _  /   / /\ \      | |    | | | |\/| |  __|   |  __|   > <    | |  |  _  /   / /\ \| |       | |   
 | |__| |_| |_| | \ \  / ____ \     | |   _| |_| |  | | |____  | |____ / . \   | |  | | \ \  / ____ \ |____   | |   
  \____/|_____|_|  \_\/_/    \_\    |_|  |_____|_|  |_|______| |______/_/ \_\  |_|  |_|  \_\/_/    \_\_____|  |_|   
                                                                                                                    
                                                                                                                    ";
            Console.WriteLine(header);

            Console.Write("Please enter your Jira Email: ");
            var username = Console.ReadLine();

            var password = ReadPassword("Please enter your Jira Password: ");

            Console.WriteLine("");
            Console.Write("Please enter the Sprint Id: ");
            var sprintId = int.Parse(Console.ReadLine());

            Console.Clear();
            Console.WriteLine(header);
            Console.Write("Please wait while the magic happens...");

            using (var client = new WebClient())
            {
                client.BaseAddress = "https://riverjira.atlassian.net/rest/api/2/";
                client.Headers[HttpRequestHeader.Authorization] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));

                var sprintAsString = client.DownloadString($"search?jql=sprint={sprintId}");
                dynamic sprint = JObject.Parse(sprintAsString);

                var issueKeys = new List<string>();

                foreach (var issue in sprint.issues)
                {
                    issueKeys.Add((string)issue.key);
                }

                var userTimeLogs = new List<UserTimeLog>();

                foreach (var issueKey in issueKeys.OrderBy(x => x))
                {
                    var issueAsString = client.DownloadString($"issue/{issueKey}/worklog");
                    dynamic issue = JObject.Parse(issueAsString);

                    foreach (var worklog in issue.worklogs)
                    {
                        userTimeLogs.Add(new UserTimeLog()
                        {
                            User = (string)worklog.author.key,
                            Story = issueKey,
                            TimeSpendSeconds = (int)worklog.timeSpentSeconds
                        });
                    }
                }

                Console.Clear();
                Console.WriteLine(header);

                var first = true;
                foreach (var group in userTimeLogs.GroupBy(x => x.User).OrderBy(x => x.Key))
                {

                    if (first)
                        first = false;
                    else
                        Console.WriteLine("");

                    Console.WriteLine($"Time logged by: {group.Key}");
                    Console.WriteLine($"Stories: {string.Join(", ", group.Select(x => x.Story).Distinct())}");
                    Console.WriteLine($"Hours (h): {group.Sum(x => x.TimeSpendSeconds) / 3600}");
                    
                }

                Console.WriteLine("");
                Console.ReadLine();
            }
        }

        private static string ReadPassword(string displayMessage)
        {
            string pass = "";
            Console.Write(displayMessage);
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (!char.IsControl(key.KeyChar))
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Remove(pass.Length - 1);
                        Console.Write("\b \b");
                    }
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);
            return pass;
        }
    }

    internal class UserTimeLog
    {
        public string User { get; set; }
        public string Story { get; set; }
        public int TimeSpendSeconds { get; set; }
    }
}
