using System;
using System.Collections.Generic;
using System.Text;

namespace ProxyCheckerLib.Enums
{
    public enum ProxyAnonymous
    {
        /// <summary>
        /// Hight = Completely anonymous
        /// </summary>
        Hight,
        /// <summary>
        /// Medium = Let the proxy ip appear
        /// </summary>
        Medium,
        /// <summary>
        /// Low = Your IP is visible
        /// </summary>
        Low,
        Unknwow
    }
}
