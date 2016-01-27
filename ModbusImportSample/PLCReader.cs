using Modbus.Device;
using Modbus.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusImportSample
{
    // The hand-coded part
    partial class PLCReader
    {
        IDictionary<Tag, object> storage = new Dictionary<Tag, object>();

        public IEnumerable<KeyValuePair<Tag, object>> Update(IModbusMaster master)
        {
            Parse(storage, master.ReadHoldingRegisters(addr, 0, nregs));
            return storage.ToList();
        }

        public object this[Tag tag]
        {
            get
            {
                return storage[tag];
            }
        }

        static ushort GetUInt16(ushort[] data, int index)
        {
            return data[index];
        }

        static float GetFloat(ushort[] data, int offset)
        {
            return ModbusUtility.GetSingle(data[offset + 1], data[offset]);
        }

        static bool GetBit(ushort[] data, int offset, int bit)
        {
            return (data[offset] & 1 << bit) != 0;
        }
    }
}
