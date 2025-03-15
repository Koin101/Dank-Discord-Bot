using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using System.Timers;

namespace Discord_Bot
{
    internal class Pickwick
    {
        List<string> pickWickLines = new List<string>();
        Random random = new Random();
        DiscordClient discord;
        public Pickwick(DiscordClient discord)
        {
            this.discord = discord;
            string root = Directory.GetCurrentDirectory();
            string path = Path.Combine(root, "Data/pickwick_teatopics.txt");
            ReadFile(path);
        }

        public void Init()
        {
            Timer PickWickTimer = new Timer();
        
        
            PickWickTimer.AutoReset = true;
            PickWickTimer.Interval = 86400000;
            PickWickTimer.Enabled = true;
            PickWickTimer.Elapsed += async (s, e) =>
            {
                string randomQuote = PickRandomQuote();

                var channel = await discord.GetChannelAsync(470924483302260748);

                await channel.SendMessageAsync(randomQuote);
            };

        }

        public void ReadFile(string path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        pickWickLines.Add(line);
                    }
                }

            }
            catch (IOException ex)
            {
                Console.WriteLine("Error reading the file: " + ex.Message);
            }
        }

        public string PickRandomQuote()
        {

            int randomNr = random.Next(0, pickWickLines.Count);

            return pickWickLines[randomNr];
        }


    }

    
}
