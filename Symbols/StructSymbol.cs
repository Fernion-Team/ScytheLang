using Scythe.Nodes.Bound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scythe.Symbols
{
    public class StructSymbol : Symbol
    {
        public string Name = "";
        public List<Nodes.Bound.MemberVariable> Members;

        public StructSymbol(string name, List<MemberVariable> members)
        {
            Name = name;
            Members = members;
        }
    }
}
