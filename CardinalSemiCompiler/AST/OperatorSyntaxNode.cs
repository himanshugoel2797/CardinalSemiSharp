using CardinalSemiCompiler.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalSemiCompiler.AST
{
    public class OperatorSyntaxNode : SyntaxNode
    {
        public List<Token> Operator {get; set;}

        public OperatorSyntaxNode(SyntaxNodeType nT, Token t) : base(nT, t)
        {
            Operator = new List<Token>();
        }

        public override string ToString()
        {
            var str = base.ToString() + " | Ops: ";

            for(int i = 0; i < Operator.Count; i++)
                str += Operator[i].TokenValue + ", ";

            return str.Substring(0, str.Length - 2);
        }
    }
}
