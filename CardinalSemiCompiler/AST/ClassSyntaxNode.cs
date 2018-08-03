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
        public bool IsAbstract {get; private set;}
        public bool IsInternal {get; private set;}

        public bool ValueType { get; private set; }

        public List<SyntaxNode> InheritanceSet {get; private set;}

        public List<SyntaxNode> GenericParameters {get; private set;}

        public ClassSyntaxNode(Token t, bool valueType, bool pub, bool intern, bool part, bool stat, bool abstr) : base(SyntaxNodeType.ClassSyntaxNode, t)
        {
            IsPublic = pub;
            IsInternal = intern;
            IsPartial = part;
            IsStatic = stat;
            IsAbstract = abstr;
            ValueType = valueType;
            InheritanceSet = new List<SyntaxNode>();
            GenericParameters = new List<SyntaxNode>();
        }

        public override string ToString()
        {
            var str = base.ToString() + " | Props: ";

            if (IsPublic)
                str += "Public, ";
            else
                str += "Private, ";

            if (IsInternal)
                str += "Internal, ";
                
            if (IsAbstract)
                str += "Abstract, ";

            if (IsPartial)
                str += "Partial, ";

            if (IsStatic)
                str += "Static, ";

            return str.Substring(0, str.Length - 2);
        }
    }
}
