# [C#].NET Core Proxy Checker

It's simple Proxy Checker, get proxy status, anonymous level, type, time, country

ONLY HTTP/S PROXIES

- [x] Proxy status
- [x] Proxy anonymous level 
- [X] Proxy country
- [ ] Type 
- [X] Proxy Time 

## Install

```
PM> Install-Package SimplyTaskScheduler -Version 1.0.0
```

# How to use ?

Example in simple console app

```c#
using ProxyCheckerLib;


        static IAPI proxyCheckerAPI;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            List<string> proxiesList = File.ReadAllLines("proxies.txt").ToList();

            proxyCheckerAPI = ProxyCheckerLib.API.CreateBuilder().SetProxyList(proxiesList)
                .Build();

            proxyCheckerAPI.ProxyListWorked.CollectionChanged += ProxyListWorked_CollectionChanged;

             proxyCheckerAPI.TestProxy();

            Console.ReadKey();
        }
        
        private static void ProxyListWorked_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var lastElement = proxyCheckerAPI.ProxyListWorked.Last();
            if (lastElement.proxyStatus == ProxyCheckerLib.Enums.ProxyStatus.Dead)
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine($"Address: {lastElement.Address}, Port: {lastElement.Port}, Status: {lastElement.proxyStatus}, Anonymous: {lastElement.proxyAnonymous}, Country: {lastElement.Country}, Time: {lastElement.Time}ms");
        }
```

## Parallel tasks

You can change the maximum number of proxy checks in parallel

```c#
        static ProxyCheckerLib.IAPI proxyCheckerAPI;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            List<string> proxiesList = File.ReadAllLines("proxies.txt").ToList();

            proxyCheckerAPI = ProxyCheckerLib.API.CreateBuilder()
                .SetProxyList(proxiesList)
                .SetMaxParallelTask(20)
                .Build();

            proxyCheckerAPI.ProxyListWorked.CollectionChanged += ProxyListWorked_CollectionChanged;

             proxyCheckerAPI.TestProxy();

            Console.ReadKey();
        }
```


# Test proxy judge link

You can test the proxy judge links before launching the test in order to remove the links that no longer work

```c#
        var count = await proxyCheckerAPI.VerifyProxyJudge();
        Console.WriteLine($"Link dead: {count}")
```
