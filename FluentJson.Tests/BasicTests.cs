// Copyright (c) 2011, Adaptiv Design
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
//     * Redistributions of source code must retain the above copyright notice, this
// list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation and/or
// other materials provided with the distribution.
//    * Neither the name of the <ORGANIZATION> nor the names of its contributors may
// be used to endorse or promote products derived from this software without specific
// prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
// IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
// NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Dynamic;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using FluentJson;
using FluentJson.Helpers;

namespace FluentJson.Tests
{
    [TestClass]
    public class BasicTests
    {
        #region Numbers

        [TestMethod]
        public void TestByte()
        {
            byte input = 0x77;
            byte input_max = byte.MaxValue;
            byte input_min = byte.MinValue;

            Assert.IsTrue((double)Json.Decode(Json.EncodeType(input)) == input);
            Assert.IsTrue((double)Json.Decode(Json.EncodeType(input_max)) == input_max);
            Assert.IsTrue((double)Json.Decode(Json.EncodeType(input_min)) == input_min);
        }

        [TestMethod]
        public void TestInt()
        {
            int input = 77;
            int input_max = int.MaxValue;
            int input_min = int.MinValue;

            Assert.IsTrue((double)Json.Decode(Json.EncodeType(input)) == input);
            Assert.IsTrue((double)Json.Decode(Json.EncodeType(input_max)) == input_max);
            Assert.IsTrue((double)Json.Decode(Json.EncodeType(input_min)) == input_min);
        }

        [TestMethod]
        public void TestUInt()
        {
            uint input = 77;
            uint input_max = uint.MaxValue;
            uint input_min = uint.MinValue;

            Assert.IsTrue((double)Json.Decode(Json.EncodeType(input)) == input);
            Assert.IsTrue((double)Json.Decode(Json.EncodeType(input_max)) == input_max);
            Assert.IsTrue((double)Json.Decode(Json.EncodeType(input_min)) == input_min);
        }

        [TestMethod]
        public void TestFloat()
        {
            float input = 77.77F;
            float input_max = float.MaxValue;
            float input_min = float.MinValue;

            Assert.IsTrue(Convert.ToSingle(Json.Decode(Json.EncodeType(input))) == input);
            Assert.IsTrue(Convert.ToSingle(Json.Decode(Json.EncodeType(input_max))) == input_max);
            Assert.IsTrue(Convert.ToSingle(Json.Decode(Json.EncodeType(input_min))) == input_min);
            
        }

        [TestMethod]
        public void TestDouble()
        {
            double input = 77.77;
            double input_max = long.MaxValue;
            double input_min = long.MinValue;

            Assert.IsTrue((double)Json.Decode(Json.EncodeType(input)) == input);
            Assert.IsTrue((double)Json.Decode(Json.EncodeType(input_max)) == input_max);
            Assert.IsTrue((double)Json.Decode(Json.EncodeType(input_min)) == input_min);
        }

        [TestMethod]
        public void TestLong()
        {
            long input = 77;
            long input_max = long.MaxValue;
            long input_min = long.MinValue;

            Assert.IsTrue((double)Json.Decode(Json.EncodeType(input)) == input);
            Assert.IsTrue((double)Json.Decode(Json.EncodeType(input_max)) == input_max);
            Assert.IsTrue((double)Json.Decode(Json.EncodeType(input_min)) == input_min);

        }

        [TestMethod]
        public void TestULong()
        {
            ulong input = 77;
            ulong input_max = ulong.MaxValue;
            ulong input_min = ulong.MinValue;

            Assert.IsTrue((double)Json.Decode(Json.EncodeType(input)) == input);
            Assert.IsTrue((double)Json.Decode(Json.EncodeType(input_max)) == input_max);
            Assert.IsTrue((double)Json.Decode(Json.EncodeType(input_min)) == input_min);
        }

        #endregion

        #region Strings

        [TestMethod]
        public void TestString1()
        {
            /*
            Assert.AreEqual(JsonDataReader.ReadFile("String1.txt"), Json.EncodeType(JsonDataReader.ReadFile("String1Input.txt")));
            Assert.AreEqual(JsonDataReader.ReadFile("String1Input.txt"), Json.Decode(JsonDataReader.ReadFile("String1.txt")));
            */
        }

        [TestMethod]
        public void TestString2()
        {
            /*
            Assert.AreEqual(JsonDataReader.ReadFile("String2.txt"), Json.EncodeType(JsonDataReader.ReadFile("String2Input.txt")));
            Assert.AreEqual(JsonDataReader.ReadFile("String2Input.txt"), Json.Decode(JsonDataReader.ReadFile("String2.txt")));
            */
        }

        [TestMethod]
        public void TestString3()
        {
            /*
            Assert.AreEqual(JsonDataReader.ReadFile("String3.txt"), Json.EncodeType(JsonDataReader.ReadFile("String3Input.txt")));
            Assert.AreEqual(JsonDataReader.ReadFile("String3Input.txt"), Json.Decode(JsonDataReader.ReadFile("String3.txt")));
            */
        }

        #endregion

        #region Type matching

        [TestMethod]
        public void TestTypeMatch1()
        {
            Assert.IsTrue(TypeHelper.IsThreatableAs(typeof(string), typeof(object)));
            Assert.IsTrue(TypeHelper.IsThreatableAs(typeof(int), typeof(object)));

            Assert.IsTrue(TypeHelper.IsThreatableAs(typeof(object[]), typeof(IList)));
            Assert.IsTrue(TypeHelper.IsThreatableAs(typeof(int[]), typeof(IList)));

            Assert.IsTrue(TypeHelper.IsThreatableAs(typeof(ArrayList), typeof(IList)));
            Assert.IsTrue(TypeHelper.IsThreatableAs(typeof(List<object>), typeof(IList)));
            Assert.IsTrue(TypeHelper.IsThreatableAs(typeof(List<int>), typeof(IList)));

            Assert.IsTrue(TypeHelper.IsThreatableAs(typeof(DictionaryBase), typeof(IDictionary)));
            Assert.IsTrue(TypeHelper.IsThreatableAs(typeof(Dictionary<object, object>), typeof(IDictionary)));
            Assert.IsTrue(TypeHelper.IsThreatableAs(typeof(Dictionary<int, int>), typeof(IDictionary)));

            Assert.IsFalse(TypeHelper.IsThreatableAs(typeof(IList<object>), typeof(IList)));
            Assert.IsFalse(TypeHelper.IsThreatableAs(typeof(IList<int>), typeof(IList)));

            Assert.IsFalse(TypeHelper.IsThreatableAs(typeof(IDictionary<object, object>), typeof(IDictionary)));
            Assert.IsFalse(TypeHelper.IsThreatableAs(typeof(IDictionary<int, int>), typeof(IDictionary)));

            Assert.IsTrue(TypeHelper.IsThreatableAs(typeof(IList<object>), typeof(IList<>)));
            Assert.IsTrue(TypeHelper.IsThreatableAs(typeof(IList<int>), typeof(IList<>)));

            Assert.IsTrue(TypeHelper.IsThreatableAs(typeof(IDictionary<object, object>), typeof(IDictionary<,>)));
            Assert.IsTrue(TypeHelper.IsThreatableAs(typeof(IDictionary<int, int>), typeof(IDictionary<,>)));

            Assert.IsFalse(TypeHelper.IsThreatableAs(typeof(IList<string>), typeof(IList<object>)));
            Assert.IsFalse(TypeHelper.IsThreatableAs(typeof(IDictionary<string, object>), typeof(IDictionary<object, object>)));

            Assert.IsTrue(TypeHelper.IsThreatableAs(typeof(ExpandoObject), typeof(IDictionary<string, object>)));
            Assert.IsFalse(TypeHelper.IsThreatableAs(typeof(ExpandoObject), typeof(IDictionary<,>)));
        }

        #endregion
    }
}
