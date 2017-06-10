using System.Linq;
using Magic;
using NUnit.Framework;
using NUnit.MagicTest.Models;

namespace NUnit.MagicTest
{
    [TestFixture]
    public class NestedSetTests
    {
        /*

                                    Left (Count) Right

                                        1 (3) 22
                         ┌────────────────┬┴─────────────────────┐
                      2 (2) 7          8 (1) 11              12 (4) 21
               ┌─────────┴─┐              │                 ┌────┴────────┐
            3 (0) 4     5 (0) 6        9 (0) 10         13 (2) 18     19 (0) 20
                                                    ┌───────┴────┐
                                                14 (0) 15    16 (0) 17


        */


        [SetUp]
        public void InitTest()
        {
            _set = new NestedSet<TestClass>
            {
                Root = new TestClass("Root")
            };

            _item1 = _set.RootItem;
            Assert.AreEqual(1, _item1.Left);
            Assert.AreEqual(2, _item1.Right);

            _item2 = _item1.Add(new TestClass("Item 2"));
            Assert.AreEqual(1, _item1.Left);
            Assert.AreEqual(2, _item2.Left);
            Assert.AreEqual(3, _item2.Right);
            Assert.AreEqual(4, _item1.Right);

            var item3 = _item2.Add(new TestClass("Item 3"));
            Assert.AreEqual(1, _item1.Left);
            Assert.AreEqual(2, _item2.Left);
            Assert.AreEqual(3, item3.Left);
            Assert.AreEqual(4, item3.Right);
            Assert.AreEqual(5, _item2.Right);
            Assert.AreEqual(6, _item1.Right);

            var item5 = _item2.Add(new TestClass("Item 5"));
            Assert.AreEqual(1, _item1.Left);
            Assert.AreEqual(2, _item2.Left);
            Assert.AreEqual(3, item3.Left);
            Assert.AreEqual(4, item3.Right);
            Assert.AreEqual(5, item5.Left);
            Assert.AreEqual(6, item5.Right);
            Assert.AreEqual(7, _item2.Right);
            Assert.AreEqual(8, _item1.Right);

            _item8 = _item1.Add(new TestClass("Item 8"));
            Assert.AreEqual(1, _item1.Left);
            Assert.AreEqual(2, _item2.Left);
            Assert.AreEqual(3, item3.Left);
            Assert.AreEqual(4, item3.Right);
            Assert.AreEqual(5, item5.Left);
            Assert.AreEqual(6, item5.Right);
            Assert.AreEqual(7, _item2.Right);
            Assert.AreEqual(8, _item8.Left);
            Assert.AreEqual(9, _item8.Right);
            Assert.AreEqual(10, _item1.Right);

            var item9 = _item8.Add(new TestClass("Item 9"));
            Assert.AreEqual(1, _item1.Left);
            Assert.AreEqual(2, _item2.Left);
            Assert.AreEqual(3, item3.Left);
            Assert.AreEqual(4, item3.Right);
            Assert.AreEqual(5, item5.Left);
            Assert.AreEqual(6, item5.Right);
            Assert.AreEqual(7, _item2.Right);
            Assert.AreEqual(8, _item8.Left);
            Assert.AreEqual(9, item9.Left);
            Assert.AreEqual(10, item9.Right);
            Assert.AreEqual(11, _item8.Right);
            Assert.AreEqual(12, _item1.Right);

            _item12 = _item1.Add(new TestClass("Item 12"));
            Assert.AreEqual(1, _item1.Left);
            Assert.AreEqual(2, _item2.Left);
            Assert.AreEqual(3, item3.Left);
            Assert.AreEqual(4, item3.Right);
            Assert.AreEqual(5, item5.Left);
            Assert.AreEqual(6, item5.Right);
            Assert.AreEqual(7, _item2.Right);
            Assert.AreEqual(8, _item8.Left);
            Assert.AreEqual(9, item9.Left);
            Assert.AreEqual(10, item9.Right);
            Assert.AreEqual(11, _item8.Right);
            Assert.AreEqual(12, _item12.Left);
            Assert.AreEqual(13, _item12.Right);
            Assert.AreEqual(14, _item1.Right);

            _item13 = _item12.Add(new TestClass("Item 13"));
            Assert.AreEqual(1, _item1.Left);
            Assert.AreEqual(2, _item2.Left);
            Assert.AreEqual(3, item3.Left);
            Assert.AreEqual(4, item3.Right);
            Assert.AreEqual(5, item5.Left);
            Assert.AreEqual(6, item5.Right);
            Assert.AreEqual(7, _item2.Right);
            Assert.AreEqual(8, _item8.Left);
            Assert.AreEqual(9, item9.Left);
            Assert.AreEqual(10, item9.Right);
            Assert.AreEqual(11, _item8.Right);
            Assert.AreEqual(12, _item12.Left);
            Assert.AreEqual(13, _item13.Left);
            Assert.AreEqual(14, _item13.Right);
            Assert.AreEqual(15, _item12.Right);
            Assert.AreEqual(16, _item1.Right);

            var item14 = _item13.Add(new TestClass("Item 14"));
            Assert.AreEqual(1, _item1.Left);
            Assert.AreEqual(2, _item2.Left);
            Assert.AreEqual(3, item3.Left);
            Assert.AreEqual(4, item3.Right);
            Assert.AreEqual(5, item5.Left);
            Assert.AreEqual(6, item5.Right);
            Assert.AreEqual(7, _item2.Right);
            Assert.AreEqual(8, _item8.Left);
            Assert.AreEqual(9, item9.Left);
            Assert.AreEqual(10, item9.Right);
            Assert.AreEqual(11, _item8.Right);
            Assert.AreEqual(12, _item12.Left);
            Assert.AreEqual(13, _item13.Left);
            Assert.AreEqual(14, item14.Left);
            Assert.AreEqual(15, item14.Right);
            Assert.AreEqual(16, _item13.Right);
            Assert.AreEqual(17, _item12.Right);
            Assert.AreEqual(18, _item1.Right);

            _item13.Add(new TestClass("Item 16"));
            _item19 = _item12.Add(new TestClass("Item 19"));
        }

        private NestedSet<TestClass> _set;
        private NestedSetItem<TestClass> _item1;
        private NestedSetItem<TestClass> _item2;
        private NestedSetItem<TestClass> _item8;
        private NestedSetItem<TestClass> _item12;
        private NestedSetItem<TestClass> _item13;
        private NestedSetItem<TestClass> _item19;


        [Test]
        public void TestAddCollectionLevel2()
        {
            var set = new NestedSet<TestClass>();
            TestClass item = new TestClass("root");
            set.Root = item;
            var setItem = set.RootItem.Add(new TestClass("1. Item"));
            set.RootItem.Add(new TestClass("2. Item"));

            var l3Item = setItem.Add(new TestClass("Item 3"));

            foreach (var testClass in setItem)
                Assert.AreEqual(l3Item, testClass);
        }

        [Test]
        public void TestAddCountLevel2()
        {
            var set = new NestedSet<TestClass>();
            TestClass item = new TestClass("root");
            set.Root = item;
            var setItem1 = set.RootItem.Add(new TestClass("1. Item "));

            Assert.AreEqual(1, set.RootItem.Left);
            Assert.AreEqual(4, set.RootItem.Right);
            Assert.AreEqual(2, setItem1.Left);
            Assert.AreEqual(3, setItem1.Right);

            var setItem2 = set.RootItem.Add(new TestClass("2. Item"));

            Assert.AreEqual(1, set.RootItem.Left);
            Assert.AreEqual(6, set.RootItem.Right);
            Assert.AreEqual(2, setItem1.Left);
            Assert.AreEqual(3, setItem1.Right);
            Assert.AreEqual(4, setItem2.Left);
            Assert.AreEqual(5, setItem2.Right);

            var setItem3 = setItem1.Add(new TestClass("3. Item"));

            Assert.AreEqual(1, set.RootItem.Left);
            Assert.AreEqual(2, setItem1.Left);
            Assert.AreEqual(5, setItem1.Right);
            Assert.AreEqual(3, setItem3.Left);
            Assert.AreEqual(4, setItem3.Right);
            Assert.AreEqual(6, setItem2.Left);
            Assert.AreEqual(7, setItem2.Right);
            Assert.AreEqual(8, set.RootItem.Right);


            Assert.AreEqual(1, setItem1.TotalCount);
            Assert.AreEqual(3, set.RootItem.TotalCount);

            Assert.AreEqual(1, setItem1.Count);
            Assert.AreEqual(3, set.RootItem.TotalCount);
            Assert.AreEqual(2, set.RootItem.Count);
        }

        [Test]
        public void TestAddItemsLevel1()
        {
            var expectRight = _item1.Right + 2;

            _item1.Add(new TestClass("Item"));

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

            _item8.Add(new TestClass("Item"));
            Assert.AreEqual(12, _set.Count());

            Assert.AreEqual(1, _item1.Left);
            Assert.AreEqual(expectRight1, _item1.Right);
            Assert.AreEqual(11, _item1.TotalCount);
            Assert.AreEqual(3, _item1.Count);
            Assert.AreEqual(3, _item1.Count);

            Assert.AreEqual(8, _item8.Left);
            Assert.AreEqual(expectRight8, _item8.Right);
            Assert.AreEqual(2, _item8.TotalCount);
            Assert.AreEqual(2, _item8.Count);
            Assert.AreEqual(2, _item8.Count);
        }

        [Test]
        public void TestEnumerable()
        {
            var expectItems = new[] {_item13, _item19};
            var idx = 0;

            foreach (var item in _item12)
            {
                Assert.AreNotEqual(2, idx);
                Assert.AreEqual(expectItems[idx], item);
                idx++;
            }
            Assert.AreEqual(1, idx - 1);
        }


        [Test]
        public void TestItem2Level1()
        {
            Assert.AreEqual(2, _item2.Left);
            Assert.AreEqual(7, _item2.Right);

            Assert.AreEqual(2, _item2.TotalCount);
        }

        [Test]
        public void TestLevelCountTrue()
        {
            Assert.AreEqual(_item12.Count, 2);
        }

        [Test]
        public void TestRoot()
        {
            Assert.AreEqual(1, _set.RootItem.Left);
            Assert.AreEqual(22, _set.RootItem.Right);

            Assert.AreEqual(10, _set.RootItem.TotalCount);
            Assert.AreEqual(3, _set.RootItem.Count);
            Assert.AreEqual(3, _set.RootItem.Count);
        }


        [Test]
        public void TestSetCount_true()
        {
            Assert.AreEqual(11, _set.Count());
        }
    }
}