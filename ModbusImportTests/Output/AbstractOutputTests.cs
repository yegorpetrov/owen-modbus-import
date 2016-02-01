using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModbusImport.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModbusImport.Output.Tests
{
    [TestClass()]
    public class AbstractOutputTests
    {
        [TestMethod()]
        public void ToStringTest()
        {
            var testEntry = new ConfigEntry()
            {
                Comment = "Lorem ipsum",
                IecAddress = "%IW1.0.0",
                Name = "foobar",
                TypeName = "ByteOutput"
            };
            var first = AbstractOutput.CreateFromConfig(new[] { testEntry }, n => { }).First();
            Assert.IsTrue(first.ToString().Contains(testEntry.Name));
        }

        [TestMethod(), ExpectedException(typeof(ArgumentNullException))]
        public void CreateFromNullTest()
        {
            AbstractOutput.CreateFromConfig(null, null).ToArray();
        }

        [TestMethod()]
        public void CreateFromConfigTest()
        {
            var testData = new[]
            {
                new { Type = "ByteOutput", Reg = 0},
                new { Type = "WordOutput", Reg = 1},
                new { Type = "ByteOutput", Reg = 2},
                new { Type = "FloatOutput", Reg = 4},
                new { Type = "DWordOutput", Reg = 6}
            };

            var entries = testData
                .Select((a, i) => new ConfigEntry()
                {
                    Name = "regVarAt" + a.Reg, TypeName = a.Type
                })
                .ToArray();

            var outs = AbstractOutput.CreateFromConfig(entries, n => { }).ToArray();
            Assert.AreEqual(entries.Length, outs.Length);
            for (int i = 0; i < testData.Length; i++)
            {
                Assert.AreEqual(outs[i].RegOffset, testData[i].Reg);
            }
        }

        [TestMethod()]
        public void CreateFromBogusData()
        {
            var testData = new[]
            {
                new { Type = "", Reg = 0},
                new { Type = (string)null, Reg = 1},
                new { Type = "AAVV", Reg = 2},
                new { Type = "\r\n", Reg = 4},
                new { Type = "............", Reg = 6}
            };

            var entries = testData
                .Select((a, i) => new ConfigEntry()
                {
                    Name = "regVarAt" + a.Reg,
                    TypeName = a.Type
                })
                .ToArray();

            var outs = AbstractOutput.CreateFromConfig(entries, n => { });
            Assert.IsFalse(outs.Any());
        }

    }
}