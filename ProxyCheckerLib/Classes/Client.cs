using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyCheckerLib.Classes
{
    public class Client
    {
        public HttpClient client { get; }
        private HttpClientHandler clientHandler;
        private WebProxy webProxy;

        private Classes.Proxy proxy;

        private string myIP;


        private CancellationToken token = new CancellationToken();

        public Client(Classes.Proxy proxy, string myIP)
        {
            this.proxy = proxy;
            this.myIP = myIP;

            this.webProxy = new WebProxy()
            {
                Address = new Uri($"http://{this.proxy.Address}:{this.proxy.Port}"),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false,

            };
            if (!string.IsNullOrEmpty(this.proxy.Id))
            {
                webProxy.Credentials = new NetworkCredential(userName:this.proxy.Id, password: this.proxy.Password);
            }
            clientHandler = new HttpClientHandler()
            {
                Proxy = this.webProxy,
                UseProxy = true,
                AllowAutoRedirect =true
            };
            this.client = new HttpClient(clientHandler);
            this.client.Timeout = TimeSpan.FromSeconds(10);
        }


        /// <summary>
        /// Proxy status OK
        /// Proxy Country OK
        /// Proxy Speed
        /// Proxy Anonymous level OK
        /// Proxy Type
        /// </summary>
        /// <returns></returns>
        public async Task TestProxy()
        {
            await Task.Run(async () =>
            {
                this.proxy.proxyStatus = GetProxyStatus();


                if (this.proxy.proxyStatus == Enums.ProxyStatus.Ok)
                {

                    try
                    {
                       var firstRequest = await client.GetAsync("http://google.com");
                    }
                    catch
                    {
                        proxy.proxyStatus = Enums.ProxyStatus.Dead;
                        return;
                    }


                    //Test multiple link for get proxy information
                    int test = 0;
                    while (string.IsNullOrEmpty(this.proxy.Country) && test != ProxyInformation.proxyInformationJSON.Count)
                    {
                        HttpResponseMessage getCountry;
                        try
                        {
                            getCountry = await client.GetAsync(ProxyInformation.proxyInformationJSON[test], cancellationToken: token);
                        }
                        catch { continue; }
                        if (token.IsCancellationRequested)
                        {
                            continue;
                        }
                        if (getCountry.IsSuccessStatusCode)
                        {
                            var getBody = await getCountry.Content.ReadAsStringAsync();
                            JObject jobject = JObject.Parse(getBody);

                            if (test == 0)
                            {
                                this.proxy.Country = (string)jobject["regionName"];
                            }
                            else if (test == 1)
                            {
                                this.proxy.Country = (string)jobject["country_name"];
                            }
                            else
                            {

                            }
                        }
                        test++;
                    }


                    string actualJudge = "";
                    string previousJudgeUsed = "";

                    bool noJudgeSelected = true;

                    var getJudge = ProxyJudge.GetJudge(previousJudgeUsed);

                    string bodyJudge = "";

                    Stopwatch stopwatch = null;

                    do
                    {
                        try
                        {
                            getJudge = ProxyJudge.GetJudge(previousJudgeUsed);

                            actualJudge = getJudge.judge;
                            previousJudgeUsed = getJudge.judge;

                           stopwatch = Stopwatch.StartNew();

                            var get = client.GetAsync(actualJudge, cancellationToken: token);
                            if (token.IsCancellationRequested || get.IsCanceled || get.IsFaulted || get.Status == TaskStatus.Faulted)
                            {
                                proxy.proxyStatus = Enums.ProxyStatus.Dead;
                                continue;
                            }

                            bodyJudge = await get.GetAwaiter().GetResult().Content.ReadAsStringAsync();
                            noJudgeSelected = false;
                            proxy.Time = stopwatch.ElapsedMilliseconds;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    while (noJudgeSelected == true && getJudge.result == false);


                    if (!getJudge.result && noJudgeSelected== true)
                    {
                        proxy.proxyStatus = Enums.ProxyStatus.Dead;
                        return;
                    }
                   

                    var matches = GetMatches(bodyJudge);

                    if (!string.IsNullOrEmpty(GetValue(matches, "REMOTE_ADDR")))
                    {
                        string ipValue = GetValue(matches, "REMOTE_ADDR");
                        if (ipValue == proxy.Address)
                        {
                            this.proxy.proxyAnonymous = Enums.ProxyAnonymous.Medium;
                        }
                        else if (ipValue == myIP)
                        {
                            this.proxy.proxyAnonymous = Enums.ProxyAnonymous.Low;
                        }
                        else
                        {
                            this.proxy.proxyAnonymous = Enums.ProxyAnonymous.Hight;
                        }
                    }
                    else
                    {
                        this.proxy.proxyAnonymous = Enums.ProxyAnonymous.Hight;
                    }
                }
            });
        }


        private Enums.ProxyStatus GetProxyStatus()
        {
            Enums.ProxyStatus proxy = Enums.ProxyStatus.Ok;

            var ping = new Ping();
            var reply = ping.Send(this.proxy.Address);

            if (reply.Status != IPStatus.Success)
            {
                proxy = Enums.ProxyStatus.Dead;
            }

                return proxy;
        }

        private MatchCollection GetMatches(string content)
        {
            return RegexInstance.ProxyJudgeRegex.Value.Matches(content);
        }

        private string GetValue(MatchCollection matches, string name)
        {
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                if (match.Groups[1].Value == name)
                {
                    return match.Groups[2].Value;
                }
            }
            return string.Empty;
        }

       
    }
}
