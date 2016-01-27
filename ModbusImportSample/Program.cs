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

/*  This program doesn't reference the ModbusImport class library directly.
    Instead, the library is used to generate some of this program's code in a T4 template
    from a sample .pro file.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Device;
using System.Net.Sockets;
using System.Threading;

namespace ModbusImportSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader = new PLCReader();

            using (var master = ModbusIpMaster.CreateIp(new TcpClient("10.1.6.10", 502)))
            {
                while (true)
                {
                    Console.Clear();
                    foreach (var pair in reader.Update(master))
                    {
                        Console.WriteLine("{0}:\t{1}", pair.Key, pair.Value);
                    }
                    Thread.Sleep(100);
                }
            }
        }
    }
}
