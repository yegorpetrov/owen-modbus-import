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

using System.Linq;
using System.IO;
using System.Diagnostics;
using System;

namespace ModbusImport
{
    public sealed class DumpWrapper : IObjectStore
    {
        private static readonly TraceSource TRACE = new TraceSource(nameof(ModbusImport));

        readonly string dir;
        public static readonly string nameFormat = "{0},{1},{2:0000},{3}.xml";

        public DumpWrapper(string dir)
        {
            TRACE.TraceEvent(TraceEventType.Verbose, 0,
                "Binding new dump wrapper to directory “{0}”", dir ?? "null");

            if (dir == null)
            {
                throw new ArgumentNullException(nameof(dir));
            }

            this.dir = dir;
        }

        public void Dispose()
        {
        }

        public int GetObjectCount(string type)
        {
            if (Directory.Exists(dir))
            {
                return Directory
                    .EnumerateFiles(dir, "*.xml")
                    .Select(fname => fname.Split(',').First())
                    .Where(split => split.Trim() == type)
                    .Count();
            }
            else return 0;
        }

        public string GetObjectName(string type, int index)
        {
            if (Directory.Exists(dir))
            {
                return Directory
                    .EnumerateFiles(dir, "*.xml")
                    .Select(fname =>
                    {
                        var split = fname.Split(',', '.').Select(s => s.Trim()).ToArray();
                        return new
                        {
                            typeGuid = split[0],
                            typeDesc = split[1],
                            index = int.Parse(split[2]),
                            name = split[3]
                        };
                    })
                    .Where(fname => fname.typeGuid == type && fname.index == index)
                    .First().name;
            }
            else return string.Empty;

        }

        public string ReadObject(string name, string type)
        {
            if (Directory.Exists(dir))
            {
                var fileName = Directory
                            .EnumerateFiles(dir, "*.xml")
                            .Where(fname => fname.Contains(type) && fname.Contains(name))
                            .FirstOrDefault();

                return string.IsNullOrEmpty(fileName) ? string.Empty : File.ReadAllText(fileName);
            }
            else return string.Empty;
        }
    }
}
