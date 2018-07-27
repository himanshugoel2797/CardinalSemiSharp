using CardinalSemiCompiler.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalSemiCompiler.AST
{
    public class EnumEntrySyntaxNode : SyntaxNode
    {
        public string Name { get; private set; }
        public ulong Value { get; private set; }

        public EnumEntrySyntaxNode(Token t, string nm, ulong val) : base(SyntaxNodeType.EnumEntrySyntaxNode, t)
        {
            Name = nm;
            Value = val;
        }

        public override string ToString()
        {
            var str = base.ToString() + " | Value: " + Value;
            
            return str;
        }
    }
}
