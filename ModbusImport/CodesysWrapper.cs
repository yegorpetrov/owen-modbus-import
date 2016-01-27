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

using CoDeSys;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ModbusImport
{
    public sealed class CodesysWrapper : IObjectStore, IDumper
    {
        private static readonly TraceSource TRACE = new TraceSource(nameof(ModbusImport));

        CCoDeSys codesys;
        IProject project;

        public CodesysWrapper(string projectPath)
        {
            TRACE.TraceEvent(
                TraceEventType.Verbose, 0,
                "Starting new CoDeSys v2.3 instance...");

            codesys = new CCoDeSys();

            TRACE.TraceEvent(
                TraceEventType.Verbose, 0, string.Format(
                    "Opening project “{0}”...", projectPath ?? string.Empty));

            codesys.OpenProject(projectPath);
            project = codesys.GetProject();
        }

        public void Dispose()
        {
            TRACE.TraceEvent(TraceEventType.Verbose, 0, "Releasing CoDeSys v2.3 instance...");
            Marshal.ReleaseComObject(codesys);
            codesys = null;
        }

        public int GetObjectCount(string type)
        {
            return project.GetObjectCount(type ?? string.Empty);
        }

        public string GetObjectName(string type, int index)
        {
            return project.GetObject(type ?? string.Empty, index);
        }

        public string ReadObject(string name, string type)
        {
            return project.ReadObject(name ?? string.Empty, type ?? string.Empty);
        }

        public void Dump(string dir)
        {
            TRACE.TraceEvent(TraceEventType.Verbose, 0, string.Format("Dumping to dir “{0}”", dir));

            Enumerate(codesys.GetObjectType, codesys.GetObjectTypeCount)
            .AsParallel()
            .ForAll(type =>
            {
                var typeDesc = codesys.GetObjectTypeDescription(type);
                for (int i = 0, _i = project.GetObjectCount(type); i < _i; i++)
                {
                    var name = project.GetObject(type, i);
                    var fname = string.Format(DumpWrapper.nameFormat, type, typeDesc, i, name);
                    var path = Path.Combine(dir, fname);

                    TRACE.TraceEvent(TraceEventType.Verbose, 0, string.Format("Dumping to file “{0}”", path));

                    File.WriteAllText(path, project.ReadObject(name, type));
                }
            });
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
