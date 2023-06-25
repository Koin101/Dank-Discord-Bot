using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Discord_Bot
{
    internal class Pickwick
    {
        string path = $"C:\\Users\\Koen\\source\\repos\\Discord Bot\\bin\\Debug\\net7.0\\pickwick_teatopics.txt\\";
        public List<string> PickWickLines = new List<string>();
        Random random = new Random();

        public Pickwick()
        {
            ReadFile(path);
            
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
                        PickWickLines.Add(line);
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

            int randomNr = random.Next(0, PickWickLines.Count);

            return PickWickLines[randomNr];
        }


    }

    
}
