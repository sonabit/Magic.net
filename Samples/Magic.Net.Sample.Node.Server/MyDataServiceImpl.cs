using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Magic.Net.Sample.Node.Server
{
    [UsedImplicitly]
    internal sealed class MyDataServiceImpl : IMyDataService
    {
        public string RemoveDuplicates(MyData data, int startIndex)
        {
            return data.Text;
        }
    }
}
