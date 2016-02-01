using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModbusImport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusImport.Tests
{
    [TestClass()]
    public class CodesysWrapperTests
    {
        CodesysWrapper wrapper;

        [TestInitialize()]
        public void CodesysWrapperTest()
        {
            var prj =
                Path.GetFullPath(
                    Path.Combine(
                        Environment.CurrentDirectory,
                        "..\\..\\CodesysWrapperTest.pro"));
            wrapper = new CodesysWrapper(prj);
        }

        [TestCleanup()]
        public void DisposeTest()
        {
            wrapper.Dispose();
            wrapper = null;
        }

        [TestMethod()]
        public void GetObjectCountTest()
        {
            Assert.AreEqual(1, wrapper.GetObjectCount("{9A9A3E9A-D363-11d5-823E-0050DA6124B7}"));
        }

        [TestMethod()]
        public void GetObjectNameTest()
        {
            Assert.AreEqual("PLC_PRG", wrapper.GetObjectName("{9A9A3E90-D363-11d5-823E-0050DA6124B7}", 0));
        }

        [TestMethod()]
        public void ReadObjectTest()
        {
            Assert.IsTrue(wrapper.ReadObject("PLC_PRG", "{9A9A3E90-D363-11d5-823E-0050DA6124B7}").StartsWith("<?xml"));
        }

        static IEnumerable<S> Enumerate<S>(Func<int, S> indexer, Func<int> counter)
        {
            for (int i = default(int), _i = counter(); i < _i; i++)
            {
                yield return indexer(i);
            }
        }
    }
}