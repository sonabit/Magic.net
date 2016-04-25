using System.ComponentModel;
using System.Diagnostics;

namespace MagicTest.Models
{
    [DebuggerDisplay("{Name}")]
    [DebuggerStepThrough]
    class TestClass
    {
        private readonly string _name;

        public TestClass(string name)
        {
            _name = name;
        }

        public string Name { get { return _name; } }
    }
}
