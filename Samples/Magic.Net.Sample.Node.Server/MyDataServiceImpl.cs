using System.Linq;
using JetBrains.Annotations;

namespace Magic.Net.Sample.Node.Server
{
    [UsedImplicitly]
    internal sealed class MyDataServiceImpl : IMyDataService
    {
        public string ReverseString(MyData data, int startIndex)
        {
            var result = data.Text.Substring(0, startIndex);
            return result + new string(data.Text.Skip(startIndex).Reverse().ToArray());
        }
    }
}