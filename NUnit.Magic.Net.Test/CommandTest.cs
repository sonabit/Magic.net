using System;
using System.Linq.Expressions;
using System.Reflection;
using FakeItEasy;
using Magic.Net;
using NUnit.Framework;

namespace NUnit.Magic.Net.Test
{
    [TestFixture]
    public class CommandTest
    {
        interface IFact
        {
            void SimpleMethode();

            void ActionWithValueType(int i);

            void ActionWithComplexType(TestComplex complex);
        }

        class TestComplex
        {
            public string text { get; set; }
        }


        [Test, Category("Command")]
        public void LambdaExpressionSimpelActionToCommand()
        {
            //Given
            IFact proxy = null;
            Expression<Action> expression = () => proxy.SimpleMethode();

            //When
            INetCommand result = expression.ToNetCommand();

            //Then
            Assert.AreEqual(typeof(IFact), result.ServiceType);
            Assert.AreEqual("SimpleMethode", result.MethodName.Name);
            Assert.AreEqual(new ParameterInfo[0], result.ParameterInfos);
            Assert.AreEqual(new object[0], result.ParameterValues);
        }

        [Test, Category("Command")]
        public void LambdaExpressionActionWithValueTypeToCommand()
        {
            //Given
            IFact proxy = null;
            Expression<Action> expression = () => proxy.ActionWithValueType(42);

            //When
            INetCommand result = expression.ToNetCommand();

            //Then
            Assert.AreEqual(typeof(IFact), result.ServiceType);
            Assert.AreEqual("ActionWithValueType", result.MethodName.Name);
            Assert.AreEqual(1, result.ParameterInfos.Length);
            Assert.AreEqual(0, result.ParameterInfos[0].Position);
            Assert.AreEqual(typeof(int), result.ParameterInfos[0].ParameterType);
            
            Assert.AreEqual(1, result.ParameterValues.Length);
            Assert.AreEqual(42, result.ParameterValues[0]);
        }

        [Test, Category("Command")]
        public void LambdaExpressionActionWithComplexTypeToCommand()
        {
            TestComplex dummyData = new TestComplex();
            //Given
            IFact proxy = null;
            Expression<Action> expression = () => proxy.ActionWithComplexType(dummyData);

            //When
            INetCommand result = expression.ToNetCommand();

            //Then
            Assert.AreEqual(typeof(IFact), result.ServiceType);
            Assert.AreEqual("ActionWithComplexType", result.MethodName.Name);
            Assert.AreEqual(1, result.ParameterInfos.Length);
            Assert.AreEqual(0, result.ParameterInfos[0].Position);
            Assert.AreEqual(typeof(TestComplex), result.ParameterInfos[0].ParameterType);

            Assert.AreEqual(1, result.ParameterValues.Length);
            Assert.AreEqual(dummyData, result.ParameterValues[0]);
        }
    }

    [TestFixture]
    public sealed class ValueTypWriteExtensionsTests
    {

        [Test]
        public void Int32ToBuffer_offset_0()
        {
            // Given
            byte[] bytes = new byte[10];

            // When
            42.ToBuffer(bytes, 0);
            int i = BitConverter.ToInt32(bytes, 0);

            // Then
            Assert.AreEqual(42, i);
        }

        [Test]
        public void Int32ToBuffer_offset_3()
        {
            // Given
            byte[] bytes = new byte[10];

            // When
            42.ToBuffer(bytes, 3);
            int i = BitConverter.ToInt32(bytes, 3);

            // Then
            Assert.AreEqual(42, i);
        }
    }
}