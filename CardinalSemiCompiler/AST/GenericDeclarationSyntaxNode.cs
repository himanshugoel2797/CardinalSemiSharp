using CardinalSemiCompiler.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalSemiCompiler.AST
{
    public class GenericDeclarationSyntaxNode : SyntaxNode
    {
        public List<SyntaxNode> GenericParameters {get; private set;}

        public GenericDeclarationSyntaxNode(Token t) : base(SyntaxNodeType.GenericDeclarationSyntaxNode, t)
        {
            GenericParameters = new List<SyntaxNode>();
        }

        public override string ToString()
        {
            var str = base.ToString() + " | Props: ";

            return str.Substring(0, str.Length - 2);
        }
    }
}
