using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.classes.functional.search
{
    public class Searcher
    {
        public Searcher() { }

        public char[] SearchPattern { get; set; }
        public char[] CurrentString { get; set; }

        public int GetMatchesCount()
        {
            if (SearchPattern == null || CurrentString == null || SearchPattern.Length == 0)
                return 0;

            int count = 0;
            for (int i = 0; i <= CurrentString.Length - SearchPattern.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < SearchPattern.Length; j++)
                {
                    if (CurrentString[i + j] != SearchPattern[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                    count++;
            }
            return count;
        }

        public bool SearchFullPattern()
        {
            if (SearchPattern == null || CurrentString == null || SearchPattern.Length == 0)
                return false;

            for (int i = 0; i <= CurrentString.Length - SearchPattern.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < SearchPattern.Length; j++)
                {
                    if (CurrentString[i + j] != SearchPattern[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                    return true;
            }
            return false;
        }
    }
}
