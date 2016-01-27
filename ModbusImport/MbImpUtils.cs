/*
    Copyright © 2016 Yegor Petrov

    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the "Software"),
    to deal in the Software without restriction, including without limitation
    the rights to use, copy, modify, merge, publish, distribute, sublicense,
    and/or sell copies of the Software, and to permit persons to whom the Software
    is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
    OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
    OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using ModbusImport.Output;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ModbusImport
{
    public static class MbImpUtils
    {
        private static readonly TraceSource TRACE = new TraceSource(nameof(ModbusImport));

        public static IEnumerable<XElement> EnumModbusSlaveNodes(IObjectStore store)
        {
            var cfg = default(XDocument);
            try
            {
                cfg = XDocument.Load(new StringReader(store.ReadObject("", CodesysConst.CFG)));
            }
            catch (XmlException xe)
            {
                TRACE.TraceEvent(TraceEventType.Error, 0,
                    "XML exception at PLC config loading: “{0}”. Returning empty node collection.",
                    xe.Message);

                return Enumerable.Empty<XElement>();
            }
            return cfg.Root
                .Elements("module")
                .Elements("module-list")
                .Elements("module")
                .Where(x => (string)x.Attribute("type-name") == "MODBUS_ID400");
        }

        public static IEnumerable<AbstractOutput> ParseSlaveNode(XElement slave, Action<ushort> regCount)
        {
            if (slave == null)
            {
                return Enumerable.Empty<AbstractOutput>();
            }

            var modbusChannels = slave
                .Elements("module-list")
                .Elements("module")
                .Elements("module-list")
                .Elements("channel");

            return AbstractOutput.CreateFromConfig(
                modbusChannels.Select(channel =>
                    new ConfigEntry()
                    {
                        Name = channel.Element("name").Value,
                        TypeName = channel.Attribute("type-name").Value,
                        IecAddress = channel.Element("iec-address").Value,
                        Comment =
                            channel.Parent.Parent.Element("common-properties").Element("comment").Value +
                            channel.Element("comment").Value
                    }
                ), regCount);
        }

        public static int? GetSlaveAddress(XElement el)
        {
            var addrNode = el
                .Elements("common-properties")
                .Elements("parameter-list")
                .Elements("parameter")
                .Where(x => (string)x.Attribute("id") == "1200")
                .Elements("value").FirstOrDefault();

            return el == null ? null : (int?)int.Parse(addrNode.Value);
        }

        public static bool CheckSlaveAddress(XElement el, int address)
        {
            return GetSlaveAddress(el) == address;
        }

        public static bool CheckSlaveComment(XElement el, string comment)
        {
            var commentNode = el
                .Elements("common-properties")
                .Elements("comment")
                .FirstOrDefault();

            return commentNode != null && (string)commentNode.Value == comment;
        }

        public static string Recode(Encoding shouldBe, Encoding happensToBe, string invalid)
        {
            return shouldBe.GetString(happensToBe.GetBytes(invalid));
        }
    }
}
