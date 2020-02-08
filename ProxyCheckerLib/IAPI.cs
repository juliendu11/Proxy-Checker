using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace ProxyCheckerLib
{
    public interface IAPI
    {
        IAPI SetProxyList(List<string> proxies);

        IAPI SetMaxParallelTask(int count = 50);

        ObservableCollection<Classes.Proxy> ProxyListWorked { get; }

        Task TestProxy();

        API Build();

        Task<int> VerifyProxyJudge();
    }
}
