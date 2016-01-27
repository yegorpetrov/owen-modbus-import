using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModbusImport.Output;

namespace ModbusImport.Refine
{
    public class FixCommentEncoding : IRefine
    {
        Encoding shouldBe, butActuallyIs;

        public FixCommentEncoding(Encoding original, Encoding happensToBe)
        {
            shouldBe = original;
            butActuallyIs = happensToBe;
        }

        public IEnumerable<AbstractOutput> Refine(IEnumerable<AbstractOutput> input)
        {
            foreach (var o in input)
            {
                if (!string.IsNullOrEmpty(o.Comment))
                {
                    o.Comment = MbImpUtils.Recode(shouldBe, butActuallyIs, o.Comment);
                }
                yield return o;
            }
        }
    }
}
