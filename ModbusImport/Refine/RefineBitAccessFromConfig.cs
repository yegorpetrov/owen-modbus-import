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
using System.Linq;
using System.Xml.Linq;

namespace ModbusImport.Refine
{
    public sealed class RefineBitAccessFromConfig : IRefine
    {
        readonly IEnumerable<XElement> slaveModuleConfig;

        public RefineBitAccessFromConfig(IEnumerable<XElement> slaveModuleConfig)
        {
            if (slaveModuleConfig == null)
            {
                throw new ArgumentNullException("slaveModuleConfig");
            }
            this.slaveModuleConfig = slaveModuleConfig.ToArray();
        }

        public IEnumerable<AbstractOutput> Refine(IEnumerable<AbstractOutput> input)
        {
            foreach (var point in input)
            {
                if (point is ByteOutput && string.IsNullOrEmpty(point.Name))
                {
                    var bitChannels = slaveModuleConfig
                        .Elements("module-list")
                        .Elements("module")
                        .Elements("module-list")
                        .Elements("channel")
                        .Where(x => (string)x.Element("iec-address") == point.IecAddress)
                        .Elements("bit-channel-list")
                        .Elements("bit-channel")
                        .Where(x => !(x.Element("name") == null || string.IsNullOrEmpty(x.Element("name").Value)));

                    if (!bitChannels.Any())
                    {
                        yield return point;
                    }
                    else
                    {
                        foreach (var bitChannel in bitChannels)
                        {
                            var name = bitChannel.Element("name").Value;
                            var iecAddress = bitChannel.Element("iec-address").Value;
                            var bit =
                                int.Parse(iecAddress.Split('.').Last()) +
                                (point as ByteOutput).PositionInRegister * 8;

                            if (!string.IsNullOrEmpty(name))
                            {
                                yield return new BitOutput(name, iecAddress, point.RegOffset, bit)
                                {
                                    Comment = bitChannel.Element("comment").Value
                                };
                            }
                        }
                    }
                }
                else yield return point;
            }
        }
    }
}
