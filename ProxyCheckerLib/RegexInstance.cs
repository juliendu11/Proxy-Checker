using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ProxyCheckerLib
{
    public static class RegexInstance
    {
        public static readonly Lazy<Regex> ProxyJudgeRegex =
           new Lazy<Regex>(() => new Regex("([A-Z].+) = ([A-z-0-9].+)", RegexOptions.Compiled));
    }
}
