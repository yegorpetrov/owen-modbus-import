using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModbusImport.Output;
using CSharp.DataStructures.TrieSpace;

namespace ModbusImport.Refine
{
    public class RemoveCommonPrefix : IRefine
    {
        public IEnumerable<AbstractOutput> Refine(IEnumerable<AbstractOutput> source)
        {
            var trie = new Trie();
            source.AsParallel().ForAll(s => trie.Add(s.Name));
            return source;
        }
    }
}
