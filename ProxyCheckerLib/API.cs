using ProxyCheckerLib.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProxyCheckerLib
{
    public class API : IAPI
    {
        private ObservableCollection<Classes.Proxy> proxyListWorked;
        private List<string> proxies;


        public ObservableCollection<Proxy> ProxyListWorked => proxyListWorked;

        private int maxParallelTask = 50;

        private string myIp;

        public static  IAPI CreateBuilder()
        {
            return new API();
        }

        public API Build()
        {
            myIp = GetIP();
            proxyListWorked = new ObservableCollection<Proxy>();
            return this;
        }

        public IAPI SetProxyList(List<string> proxies)
        {
            this.proxies = proxies;
            return this;
        }

        public IAPI SetMaxParallelTask(int count =50)
        {
            this.maxParallelTask = count;
            return this;
        }

        public async Task<int> VerifyProxyJudge()
        {
            int judgeDead = 0;

            List<string> goodJudge = new List<string>();

            using (var client = new HttpClient())
            {
                foreach (var judge in ProxyJudge.Judge)
                {
                   var getResult =await  client.GetAsync(judge);
                    if (!getResult.IsSuccessStatusCode)
                    {
                        judgeDead++;
                    }
                    else
                    {
                        goodJudge.Add(judge);
                    }
                }
            }

            ProxyJudge.Judge = goodJudge;

            return judgeDead;
        }

        public async Task TestProxy()
        {
            if (this.proxies == null)
                return;

            if (proxyListWorked == null) proxyListWorked = new ObservableCollection<Proxy>();

            await Task.Run(() =>
             {
                 Parallel.ForEach(proxies, new ParallelOptions { MaxDegreeOfParallelism = maxParallelTask }, async proxy =>
                 {
                     var p = new Proxy(proxy);
                     var client = new Classes.Client(p, this.myIp);
                     try
                     {
                         await client.TestProxy();
                         proxyListWorked.Add(p);
                     }
                     catch
                     {

                     }
                 });
             });


        }

        private string GetIP()
        {
            string externalIP = "";
            externalIP = (new WebClient()).DownloadString("http://checkip.dyndns.org/");
            externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")).Matches(externalIP)[0].ToString();
            return externalIP;
        }
    }
}
