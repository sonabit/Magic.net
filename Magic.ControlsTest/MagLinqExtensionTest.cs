using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Magic.ControlsTest
{
    [TestClass]
    public class MagLinqExtensionTest
    {

        class TestLinqProcess
        {
            public int Int1 { get; set; } 
        }

        [TestMethod]
        public void LinqProcess()
        {
            var items = new[]
            {
                new TestLinqProcess { Int1 = 1},
                new TestLinqProcess { Int1 = 2},
                new TestLinqProcess { Int1 = 3},
                new TestLinqProcess { Int1 = 4},
                new TestLinqProcess { Int1 = 5},
                new TestLinqProcess { Int1 = 6}
            };

            var test = 2;
            foreach (var item in items.Process(item => item.Int1 += 1).Select(item => item.Int1))
            {
                Assert.AreEqual(test, item);
                test++;
            }

        }

        [TestMethod]
        public void LinqProcessWithParameter()
        {
            var items = new[]
            {
                new TestLinqProcess { Int1 = 1},
                new TestLinqProcess { Int1 = 2},
                new TestLinqProcess { Int1 = 3},
                new TestLinqProcess { Int1 = 4},
                new TestLinqProcess { Int1 = 5},
                new TestLinqProcess { Int1 = 6}
            };

            var test = 2;
            foreach (var item in items.Process((item, param) => item.Int1 += param, param: 1).Select(item => item.Int1))
            {
                Assert.AreEqual(test, item);
                test++;
            }

        }

        [TestMethod]
        public void LinqEach()
        {
            IEnumerable<TestLinqProcess> items = new[]
            {
                new TestLinqProcess { Int1 = 1},
                new TestLinqProcess { Int1 = 2},
                new TestLinqProcess { Int1 = 3},
                new TestLinqProcess { Int1 = 4},
                new TestLinqProcess { Int1 = 5},
                new TestLinqProcess { Int1 = 6}
            };

            items.Each(item => item.Int1 += 1);
            var test = 2;
            foreach (var item in items)
            {
                Assert.AreEqual(test, item.Int1);
                test++;
            }

        }

        [TestMethod]
        public void LinqEachWithParam()
        {
            IEnumerable<TestLinqProcess> items = new[]
            {
                new TestLinqProcess { Int1 = 1},
                new TestLinqProcess { Int1 = 2},
                new TestLinqProcess { Int1 = 3},
                new TestLinqProcess { Int1 = 4},
                new TestLinqProcess { Int1 = 5},
                new TestLinqProcess { Int1 = 6}
            };

            items.Each((item, param) => item.Int1 += param, param: 1);
            var test = 2;
            foreach (var item in items)
            {
                Assert.AreEqual(test, item.Int1);
                test++;
            }

        }
    }
}
