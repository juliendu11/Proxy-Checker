using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ProxyCheckerLib.Classes
{
    public class Proxy
    {
        public string Address { get; }
        public int Port { get; }
        public string Id { get; }
        public string Password { get; }

        public string Country { get; internal set; }

        public long Time { get; internal set; }

        public Enums.ProxyAnonymous proxyAnonymous { get; internal set; } = Enums.ProxyAnonymous.Unknwow;
        public Enums.ProxySpeed proxySpeed { get; internal set; }
        public Enums.ProxyStatus proxyStatus { get; internal set; } = Enums.ProxyStatus.Dead;
        public Enums.ProxyVersion proxyVersion { get; internal set; }

        public Proxy(string proxy)
        {
            if (string.IsNullOrEmpty(proxy))
            {
                return;
            }

            var splitProxy = SplitProxy(proxy);
            this.Address = splitProxy.address;
            this.Port = int.Parse(splitProxy.port);

            if (splitProxy.auth)
            {
                this.Id = splitProxy.id;
                this.Password = splitProxy.password;
            }
        }

        public void SetInformation(string country,Enums.ProxyAnonymous proxyAnonymous, Enums.ProxySpeed proxySpeed, Enums.ProxyStatus proxyStatus, Enums.ProxyVersion proxyVersion, long time)
        {
            this.Country = country;

            this.proxyAnonymous = proxyAnonymous;
            this.proxySpeed = proxySpeed;
            this.proxyStatus = proxyStatus;
            this.proxyVersion = proxyVersion;

            this.Time = time;
        }


        private  (string address, string port, bool auth, string id, string password) SplitProxy(string proxy)
        {
            string address = "";
            string port = "";
            string id = "";
            string password = "";

            bool auth = false;
            int seperatorCount = Regex.Matches(proxy, ":").Count;

            address = proxy.Split(':')[0];
            port = proxy.Split(':')[1];


            if (seperatorCount > 1)
            {
                id = proxy.Split(':')[2];
                password = proxy.Split(':')[3];
                auth = true;
            }

            return (address, port, auth, id, password);
        }
    }
}
