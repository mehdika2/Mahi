 using Fardin;
using Mahi.HtmLua;
using Mahi.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Mahi
{
    public class Logger : IDisposable
    {
        private StreamWriter writer;

        public Logger()
        {
            string logsPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(logsPath)) Directory.CreateDirectory(logsPath);
            writer = new StreamWriter(Path.Combine(logsPath, DateTime.Now.ToString("yyyy-MM-dd") + ".log"), true);
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        }

        public void Log(string message, bool newLine = true)
        {
            try
            {
                writer.Write($"{DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.ff")}: ");

                for (int i = 0; i < message.Length; i++)
                {
                    if (message[i] == '&' && message.Length >= i && Regex.Match(message[i + 1].ToString(), @"^[0-9A-Ra-r]+$").Success)
                    {
                        if (char.ToLower(message[i + 1]) == 'r')
                            Console.ResetColor();
                        else Console.ForegroundColor = (ConsoleColor)Convert.ToInt32(message[i + 1].ToString(), 16);
                        i++;
                        if (i + 1 >= message.Length)
                            break;
                        continue;
                    }
                    writer.Write(message[i]);
                    Console.Write(message[i]);
                }

                if (newLine)
                {
                    writer.WriteLine();
                    Console.WriteLine();
                }

                writer.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log: {ex.Message}");
            }
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            Dispose();
        }

        public void Dispose()
        {
            writer?.Dispose();
        }
    }
}
