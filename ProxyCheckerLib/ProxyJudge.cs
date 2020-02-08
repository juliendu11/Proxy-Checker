using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyCheckerLib
{
    public class ProxyJudge
    {
        public static List<string> Judge { get; internal set; } = new List<string>
        {
            {"http://www.proxy-listen.de/azenv.php"},
{"http://mojeip.net.pl/asdfa/azenv.php"},
{"http://www.sbjudge2.com/azenv.php"},
//{"http://azenv.net/"},
//{"http://birdingonthe.net/cgi-bin/env.pl"},
//{"http://proxyjudge.us/azenv.php"},
//{"http://proxyjudge.us/judge.php"},
//{"http://www.sbjudge3.com/azenv.php"},
//{"http://pascal.hoez.free.fr/azenv.php"},
//{"http://www.e-cotton.jp/env.cgi"},
//{"http://users.on.net/~emerson/env/env.pl"},
//{"http://www3.wind.ne.jp/hassii/env.cgi"},
//{"http://cgi.www5a.biglobe.ne.jp/~cafesp/env.cgi"},
//{"http://www2t.biglobe.ne.jp/~take52/test/env.cgi" }
        };


        public static (bool result ,string judge) GetJudge(string oldJudgeUsed)
        {
            if (!string.IsNullOrEmpty(oldJudgeUsed))
            {
                int oldIndex = Judge.FindIndex(x => x == oldJudgeUsed);
                if (oldIndex == Judge.IndexOf(Judge.Max()))
                {
                    return (false, string.Empty);
                }
                return (true, Judge[oldIndex++]);

            }
            return (true, Judge[0]);
        }

        static Random rnd = new Random();
        public static string GetRandomJudge()
        {
            int r = rnd.Next(Judge.Count);
            return Judge[r];
        }
    }
}
