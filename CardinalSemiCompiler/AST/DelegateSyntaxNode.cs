using CardinalSemiCompiler.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalSemiCompiler.AST
{
    public class DelegateSyntaxNode : SyntaxNode
    {
        public bool IsPublic {get; private set;}
        public bool IsPrivate {get; private set;}
        public bool IsProtected {get; private set;}
        public bool IsInternal {get; private set;}
        public Token Identifier {get; set;}
        public List<SyntaxNode> Parameters {get; set;}

        public DelegateSyntaxNode(bool isPub, bool isPriv, bool isProt, bool isIntern, Token t) : base(SyntaxNodeType.DelegateSyntaxNode, t)
        {
            IsPublic = isPub;
            IsPrivate = isPriv;
            IsProtected = isProt;
            IsInternal = isIntern;
            Parameters = new List<SyntaxNode>();
        }

        public override string ToString()
        {
            var str = base.ToString() + " | Props: ";

            return str.Substring(0, str.Length - 2);
        }
    }
}
