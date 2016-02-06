﻿using System.Linq;
using Magic;
using MagicTest.Models;
using NUnit.Framework;

namespace NUnit.MagicTest
{
    [TestFixture()]
    public class NestedSetTest
    {
        private NestedSet<TestClass> _set;
        private NestedSetItem<TestClass> _item1;
        private NestedSetItem<TestClass> _item2;
        private NestedSetItem<TestClass> _item8;
        private NestedSetItem<TestClass> _item12;
        private NestedSetItem<TestClass> _item13;
        private NestedSetItem<TestClass> _item19;


        /*

                                        1 (10) 22
                      2 (2) 7         8 (1) 11              12 (4) 21
              3 (0) 4     5 (0) 6       9 (0) 10          13 (2) 18     19 (0) 20
                                                14 (0) 15  16 (0) 17


        */



        [SetUp]
        public void InitTest()
        { 
            _set = new NestedSet<TestClass>();
            _set.Root = new TestClass(); ;
            _item1 = _set.RootItem;

            _item2 = _item1.Add(new TestClass());
            var item3 = _item2.Add(new TestClass());
            var item5 = _item2.Add(new TestClass());

            _item8 = _item1.Add(new TestClass());
            var item9 = _item8.Add(new TestClass());

            _item12 = _item1.Add(new TestClass());
            _item13 = _item12.Add(new TestClass());
            var item14 = _item13.Add(new TestClass());
            var item16 = _item13.Add(new TestClass());
            _item19 = _item12.Add(new TestClass());
        }


        [Test]
        public void TestSetCount_true()
        {
            Assert.AreEqual(11, _set.Count());
        }

       [Test]
        public void TestRoot()
        {
            Assert.AreEqual(1, _set.RootItem.Left);
            Assert.AreEqual(22, _set.RootItem.Right);

            Assert.AreEqual(10, _set.RootItem.TotalCount);
            Assert.AreEqual(3, _set.RootItem.Count());
            Assert.AreEqual(3, _set.RootItem.Count);
        }


       [Test]
        public void TestItem2Level1()
        { 
            Assert.AreEqual(2, _item2.Left);
            Assert.AreEqual(7, _item2.Right);

            Assert.AreEqual(2, _item2.TotalCount);
        }

       [Test]
        public void TestAddItemsLevel1()
        {
            var expectRight = _item1.Right + 2;

            _item1.Add(new TestClass());
            
            Assert.AreEqual(expectRight, _item1.Right);
            Assert.AreEqual(1, _item1.Left);

            Assert.AreEqual(12, _set.Count());
            Assert.AreEqual(11, _item1.TotalCount);
            Assert.AreEqual(4, _item1.Count);
        }


       [Test]
        public void TestAddItemsLevel2()
        {
            var expectRight1 = _item1.Right + 2;
            var expectRight8 = _item8.Right + 2;

            _item8.Add(new TestClass());
            Assert.AreEqual(12, _set.Count());

            Assert.AreEqual(1, _item1.Left);
            Assert.AreEqual(expectRight1, _item1.Right);
            Assert.AreEqual(11, _item1.TotalCount);
            Assert.AreEqual(3, _item1.Count());
            Assert.AreEqual(3, _item1.Count);

            Assert.AreEqual(8, _item8.Left);
            Assert.AreEqual(expectRight8, _item8.Right);
            Assert.AreEqual(2, _item8.TotalCount);
            Assert.AreEqual(2, _item8.Count());
            Assert.AreEqual(2, _item8.Count);

        }


       [Test]
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

       [Test]
        public void TestAddCountLevel2()
        {
            var set = new NestedSet<TestClass>();
            var item = new TestClass();
            set.Root = item;
            var setItem = set.RootItem.Add(new TestClass());
            set.RootItem.Add(new TestClass());

            setItem.Add(new TestClass());

            Assert.AreEqual(1, setItem.TotalCount);
            Assert.AreEqual(3, set.RootItem.TotalCount);

            Assert.AreEqual(1, setItem.Count);
            Assert.AreEqual(3, set.RootItem.TotalCount);
            Assert.AreEqual(2, set.RootItem.Count);
        }

       [Test]
        public void TestLevelCountTrue()
        {
            Assert.AreEqual(_item12.Count, 2);
        }

       [Test]
        public void TestEnumerable()
        {
            var expectItems = new [] { _item13, _item19 };
            var idx = 0;

            foreach (var item in _item12)
            {
                Assert.AreNotEqual(2, idx);
                Assert.AreEqual(expectItems[idx], item);
                idx++;
            }
            Assert.AreEqual(1, idx-1);
        }
    }
}
