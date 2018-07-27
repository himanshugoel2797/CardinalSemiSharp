using CardinalSemiCompiler.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalSemiCompiler.AST
{
    public class EnumSyntaxNode : SyntaxNode
    {
        public bool IsPublic { get; private set; }
        
        public EnumSyntaxNode(Token t, bool pub) : base(SyntaxNodeType.EnumSyntaxNode, t)
        {
            IsPublic = pub;
        }

        public override string ToString()
        {
            var str = base.ToString() + " | Props: ";

            if (IsPublic)
                str += "Public";
            else
                str += "Private";
            
            return str;
        }
    }
}
