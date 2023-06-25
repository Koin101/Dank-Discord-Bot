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
        List<string> pickWickLines = new List<string>();
        Random random = new Random();

        public Pickwick()
        {
            string root = Directory.GetCurrentDirectory();
            string path = Path.Combine(root, "pickwick_teatopics.txt");
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
