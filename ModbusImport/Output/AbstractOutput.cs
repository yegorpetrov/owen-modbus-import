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

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ModbusImport.Output
{
    public abstract class AbstractOutput
    {
        private static readonly TraceSource TRACE = new TraceSource(nameof(ModbusImport));

        static readonly IDictionary<string, int> channelSize = new Dictionary<string, int>();
        private const string
            ByteOutput = "ByteOutput",
            WordOutput = "WordOutput",
            DWordOutput = "DWordOutput",
            FloatOutput = "FloatOutput";

        static AbstractOutput()
        {
            channelSize[ByteOutput] = 1;
            channelSize[WordOutput] = 2;
            channelSize[DWordOutput] = 4;
            channelSize[FloatOutput] = 4;
        }

        protected AbstractOutput(string name, string iecAddress, int regOffset)
        {
            Name = name;
            IecAddress = iecAddress;
            RegOffset = regOffset;
        }

        public string Name
        {
            get;
            set;
        }

        public string IecAddress
        {
            get;
            private set;
        }

        public int RegOffset
        {
            get;
            private set;
        }

        public string Comment
        {
            get;
            set;
        }

        public abstract string ConversionName
        {
            get;
        }

        public virtual IEnumerable<string> ConversionArgs
        {
            get { yield return RegOffset.ToString(); }
        }


        public override string ToString()
        {
            return string.Format("{0}: {1}\t{2}", Name, RegOffset, IecAddress);
        }

        public static IEnumerable<AbstractOutput> CreateFromConfig(IEnumerable<ConfigEntry> nameAndTypeEntries, Action<ushort> regCounter)
        {
            if (nameAndTypeEntries == null) throw new ArgumentNullException(nameof(nameAndTypeEntries));
            if (regCounter == null) throw new ArgumentNullException(nameof(regCounter));

            int i = 0;
            foreach (var cfgEntry in nameAndTypeEntries)
            {
                int size = 0;
                if (channelSize.TryGetValue(cfgEntry.TypeName ?? "", out size))
                {
                    var regOffset = (i = roundUp(i, size)) / 2;
                    AbstractOutput result = null;
                    switch (cfgEntry.TypeName)
                    {
                        case "ByteOutput":
                            result = new ByteOutput(cfgEntry.Name, cfgEntry.IecAddress, regOffset, (byte)(i % 2), false);
                            break;
                        case "WordOutput":
                            result = new WordOutput(cfgEntry.Name, cfgEntry.IecAddress, regOffset, false);
                            break;
                        case "DWordOutput":
                            result = new DWordOutput(cfgEntry.Name, cfgEntry.IecAddress, regOffset, false);
                            break;
                        case "FloatOutput":
                            result = new FloatOutput(cfgEntry.Name, cfgEntry.IecAddress, regOffset);
                            break;
                    }
                    result.Comment = cfgEntry.Comment;
                    yield return result;
                    i += size;
                }
                else
                {
                    TRACE.TraceEvent(TraceEventType.Error, 0,
                        "Unsupported channel type “{0}”", cfgEntry.TypeName);

                    TRACE.TraceData(TraceEventType.Error, 0, cfgEntry);
                }
            }
            regCounter((ushort)(roundUp(i, 2) / 2));
        }

        static int roundUp(int numToRound, int multiple)
        {
            return ((numToRound + multiple - 1) / multiple) * multiple;
        }
    }
}
