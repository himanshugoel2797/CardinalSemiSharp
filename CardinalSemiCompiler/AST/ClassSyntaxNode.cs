using CardinalSemiCompiler.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalSemiCompiler.AST
{
    public class ClassSyntaxNode : SyntaxNode
    {
        public bool IsPublic { get; private set; }
        public bool IsPartial { get; private set; }
        public bool IsStatic { get; private set; }

        public bool ValueType { get; private set; }

        public ClassSyntaxNode(Token t, bool valueType, bool pub, bool part, bool stat) : base(SyntaxNodeType.ClassSyntaxNode, t)
        {
            IsPublic = pub;
            IsPartial = part;
            IsStatic = stat;
            ValueType = valueType;
        }

        public override string ToString()
        {
            var str = base.ToString() + " | Props: ";

            if (IsPublic)
                str += "Public, ";
            else
                str += "Private, ";

            if (IsPartial)
                str += "Partial, ";

            if (IsStatic)
                str += "Static, ";

            return str.Substring(0, str.Length - 2);
        }
    }
}
