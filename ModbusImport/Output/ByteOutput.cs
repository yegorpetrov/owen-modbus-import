﻿/*
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

using System.Collections.Generic;

namespace ModbusImport.Output
{
    sealed class ByteOutput : AbstractIntegerOutput
    {
        public ByteOutput(string name, string iecAddress, int regOffset, byte posInReg, bool signed)
            : base(name, iecAddress, regOffset, signed)
        {
            PositionInRegister = posInReg;
        }

        public byte PositionInRegister
        {
            get;
            private set;
        }

        public override string ConversionName
        {
            get { return IsSigned ? "GetInt8" : "GetUInt8"; }
        }

        public override IEnumerable<string> ConversionArgs
        {
            get
            {
                foreach (var arg in base.ConversionArgs)
                {
                    yield return arg;
                }
                yield return PositionInRegister.ToString();
            }
        }
    }
}
