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
                AllowAutoRedirect =false
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
            this.proxy.proxyStatus = GetProxyStatus();

            long delay = 0;

            //JObject jobject = JObject.Parse(new WebClient().DownloadString("http://ip-api.com/json/" + this.proxy.Address));
            //this.proxy.Country = (string)jobject["regionName"];

            try
            {
                if (this.proxy.proxyStatus == Enums.ProxyStatus.Ok)
                {

                    //Test multiple link for get proxy information
                    int test = 0;
                    while(string.IsNullOrEmpty(this.proxy.Country) && test != ProxyInformation.proxyInformationJSON.Count)
                    {
                        try
                        {

                            var getCountry = await client.GetAsync(ProxyInformation.proxyInformationJSON[test]);
                            if (getCountry.IsSuccessStatusCode)
                            {
                                var getBody = await getCountry.Content.ReadAsStringAsync();
                                JObject jobject = JObject.Parse(getBody);

                                if (test == 0)
                                {
                                    this.proxy.Country = (string)jobject["regionName"];
                                }
                                else if (test ==1)
                                {
                                    this.proxy.Country = (string)jobject["country_name"];
                                }
                                else
                                {

                                }
                            }
                        }
                        catch { }

                        test++;
                    }
                      

                    string judge = ProxyJudge.GetRandomJudge();
                    var stopwatch = Stopwatch.StartNew();

                    var get = await client.GetAsync(judge);
                    if (!get.IsSuccessStatusCode)
                    {
                        //Proxy judge error
                        return;
                    }

                    var content = await get.Content.ReadAsStringAsync();
                    delay = stopwatch.ElapsedMilliseconds;

                    proxy.Time = delay;

                    var matches = GetMatches(content);

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

                    //this.proxy.proxyAnonymous = string.IsNullOrEmpty(GetValue(matches, "REMOTE_ADDR")) ? Enums.ProxyAnonymous.Hight : Enums.ProxyAnonymous.Medium;
                }
            }
            catch (IOException)
            {
                this.proxy.proxyStatus = Enums.ProxyStatus.Dead;
            }
            catch (System.Net.Http.HttpRequestException)
            {
                this.proxy.proxyStatus = Enums.ProxyStatus.Dead;
            }
            catch (SocketException)
            {
                this.proxy.proxyStatus = Enums.ProxyStatus.Dead;
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                this.proxy.proxyStatus = Enums.ProxyStatus.Dead;
            }
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
