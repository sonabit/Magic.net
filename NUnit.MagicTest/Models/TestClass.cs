using System.Diagnostics;

namespace NUnit.MagicTest.Models
{
    [DebuggerDisplay("{Name}")]
    [DebuggerStepThrough]
    internal class TestClass
    {
        public TestClass(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}