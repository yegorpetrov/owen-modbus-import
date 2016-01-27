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
using System.Linq;
using System.IO;
using ModbusImport;
using ModbusImport.Refine;
using Antlr4.StringTemplate;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fclp;
using System.Xml.Linq;

namespace ModbusImportShell
{
    class Program
    {
        static void Main(string[] args)
        {
            var Err = Console.Error;

            string
                dump = string.Empty,
                project = string.Empty,
                template = string.Empty,
                outfile = string.Empty;

            int nthSlave = 0;

            var parser = new FluentCommandLineParser();

            parser
                .Setup<string>('d', "dump")
                .Callback(d => dump = d)
                .WithDescription("Dump project objects to XML files");

            parser
                .SetupHelp("h", "help", "?")
                .UseForEmptyArgs()
                .Callback(() =>
                {
                    Err.WriteLine("Usage:");
                    foreach (var option in parser.Options.OrderByDescending(o => o.IsRequired ? 1 : 0))
                    {
                        Err.WriteLine("\t--{0}, -{1}{3}: {2}", option.LongName,
                            option.ShortName, option.Description,
                            option.IsRequired ? " (required)" : " (optional)");
                    }
                    Err.WriteLine();
                });

            parser
                .Setup<string>('i', "input")
                .Callback(i =>
                {
                    if (!File.Exists(i) && !Directory.Exists(i))
                    {
                        Err.WriteLine("Input file/directory {0} not found.", i);
                        Environment.Exit(-1);
                    }
                    else project = i;
                })
                .Required()
                .WithDescription("A .pro file or dump directory to process");

            parser
                .Setup<string>('t', "template")
                .Callback(t =>
                {
                    if (File.Exists(t)) template = t;
                    else
                    {
                        Err.WriteLine("Template file {0} not found", t);
                        Environment.Exit(-1);
                    }
                })
                .WithDescription("StringTemplate4 .stg output template");

            parser
                .Setup<string>('o', "out")
                .Callback(o => outfile = o)
                .WithDescription("Output file (default to console)");

            var selectors = new List<Func<XElement, bool>>();

            parser
                .Setup<int>('a', "address")
                .Callback(a => selectors.Add(x => MbImpUtils.CheckSlaveAddress(x, a)))
                .WithDescription("Select slave node by its address (0-255)");

            parser
                .Setup<string>('c', "comment")
                .Callback(c => selectors.Add(x => MbImpUtils.CheckSlaveComment(x, c)))
                .WithDescription("Select slave node by comment");

            parser
                .Setup<int>('n', "nth")
                .Callback(n => nthSlave = n)
                .WithDescription("Select Nth slave node (zero-based, zero by default)");


            var parseResult = parser.Parse(args);

            if (parseResult.HasErrors)
            {
                Err.WriteLine(parseResult.ErrorText);
                Environment.Exit(-1);
            }

            if (template == string.Empty && dump == string.Empty)
            {
                Err.WriteLine("Neither template (t) nor dump (d) arguments were specified correctly. No action to take. Exiting...");
                Environment.Exit(-1);
            }

            var store = default(IObjectStore);
            try
            {
                store = ObjectStoreFactory.CreateFromPath(project);
            }
            catch (COMException ce)
            {
                Err.WriteLine("Unable to open project “{0}”. Please make sure it opens in CoDeSys IDE without any messages.",
                    project);

                Err.WriteLine("CoDeSys COM exception “{0}”", ce.Message);

                Environment.Exit(ce.ErrorCode);
            }

            if (dump != string.Empty)
            {
                if (!(store is IDumper))
                {
                    Err.WriteLine("The specified input type doesn't support object dumping.");
                    Environment.Exit(-2);
                }
                (store as IDumper).Dump(dump);
            }

            ushort Nregs = 0;

            var slave = MbImpUtils.EnumModbusSlaveNodes(store);

            foreach (var selector in selectors)
            {
                slave = slave.Where(selector);
            }
            var selectedSlave = slave.Skip(nthSlave).FirstOrDefault();
            if (selectedSlave == null)
            {
                Err.WriteLine("No supported slave nodes found matching the criteria.");
                Environment.Exit(-1);
            }

            var outputs = MbImpUtils.ParseSlaveNode(selectedSlave, u => Nregs = u);

            var refinery = new IRefine[]
            {
                new RefineBitAccessFromConfig(slave),
                new RefineUnused(),
                new FixCommentEncoding(Encoding.GetEncoding(1251), Encoding.GetEncoding(1252))
            };

            foreach (var refiner in refinery)
            {
                outputs = refiner.Refine(outputs);
            }
            outputs = outputs.ToArray();

            if (template != string.Empty)
            {
                var file = Path.Combine(Environment.CurrentDirectory, template);
                var tmpg = new TemplateGroupFile(file);
                var tmp = tmpg.GetInstanceOf("Parser");
                tmp.Add("outs", outputs);
                tmp.Add("nregs", Nregs);

                var result = tmp.Render(78);
                if (outfile != string.Empty)
                {
                    try
                    {
                        File.WriteAllText(outfile, result ?? string.Empty);
                    }
                    catch (Exception e)
                    {
                        Err.WriteLine("Unable to write output file. Path: “{0}”, exception: {1}", outfile, e.Message);
                    }
                }
                else
                {
                    Console.WriteLine(result ?? string.Empty);
                }
            }
        }
    }
}
