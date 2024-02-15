using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.RiotApiClasses
{
    public class MatchId
    {
        public List<string> MatchIds { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var matchId in MatchIds)
            {
                stringBuilder.Append(matchId.ToString());
                stringBuilder.Append("\n");
            }

            return stringBuilder.ToString();
        }
    }
}
