using System;
using System.Linq;
using Magic;
using MagicTest.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MagicTest
{
    [TestClass]
    public class NestedSetTest
    {
        [TestMethod]
        public void TestRoot()
        {
            var set = new NestedSet<TestClass>();
            var item = new TestClass();
            set.Root = item;

            Assert.AreEqual(1, set.RootItem.Left);
            Assert.AreEqual(2, set.RootItem.Right);

            Assert.AreEqual(1, set.Count());
        }


        [TestMethod]
        public void TestAdd1ItemLevel1()
        {
            var set = new NestedSet<TestClass>();
            var item = new TestClass();
            set.Root = item;

            Assert.AreEqual(1, set.RootItem.Left);
            Assert.AreEqual(2, set.RootItem.Right);

            set.RootItem.Add(new TestClass());

            Assert.AreEqual(2, set.Count());

            Assert.AreEqual(1, set.RootItem.Left);
            Assert.AreEqual(4, set.RootItem.Right);
            
        }

        [TestMethod]
        public void TestAdd2ItemsLevel1()
        {
            var set = new NestedSet<TestClass>();
            var item = new TestClass();
            set.Root = item;

            set.RootItem.Add(new TestClass());
            Assert.AreEqual(2, set.Count());

            set.RootItem.Add(new TestClass());
            Assert.AreEqual(3, set.Count());

            Assert.AreEqual(1, set.RootItem.Left);
            Assert.AreEqual(6, set.RootItem.Right);
        }


        [TestMethod]
        public void TestAdd1ItemLevel2()
        {
            var set = new NestedSet<TestClass>();
            var item = new TestClass();
            set.Root = item;
            var setItem = set.RootItem.Add(new TestClass());
            set.RootItem.Add(new TestClass());
            
            
            setItem.Add(new TestClass());

            Assert.AreEqual(4, set.Count());
            Assert.AreEqual(2, setItem.Left);
            Assert.AreEqual(5, setItem.Right);
            Assert.AreEqual(8, set.RootItem.Right);

        }


        [TestMethod]
        public void TestAddCollectionLevel2()
        {
            var set = new NestedSet<TestClass>();
            var item = new TestClass();
            set.Root = item;
            var setItem = set.RootItem.Add(new TestClass());
            set.RootItem.Add(new TestClass());

            var l3Item = setItem.Add(new TestClass());

            foreach (NestedSetItem<TestClass> testClass in setItem)
            {
                Assert.AreEqual(l3Item, testClass);
            }
        }

        [TestMethod]
        public void TestAddCountLevel2()
        {
            var set = new NestedSet<TestClass>();
            var item = new TestClass();
            set.Root = item;
            var setItem = set.RootItem.Add(new TestClass());
            set.RootItem.Add(new TestClass());

            setItem.Add(new TestClass());

            Assert.AreEqual(1, setItem.Count);
            Assert.AreEqual(3, set.RootItem.Count);
        }
    }
}
