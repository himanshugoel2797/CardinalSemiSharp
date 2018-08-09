using CardinalSemiCompiler.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalSemiCompiler.AST
{
    public class FunctionSyntaxNode : SyntaxNode
    {
        public bool IsPublic {get; private set;}
        public bool IsPrivate {get; private set;}
        public bool IsProtected {get; private set;}
        public bool IsInternal {get; private set;}
        public bool IsStatic {get; private set;}
        public bool IsVirtual {get; private set;}
        public bool IsOverride {get; private set;}
        public Token Identifier {get; set;}
        public List<SyntaxNode> Parameters {get; set;}
        public List<string> ParameterNames {get; set;}

        public FunctionSyntaxNode(bool isPub, bool isPriv, bool isProt, bool isIntern, bool isStat, bool isVirt, bool isOver, Token t) : base(SyntaxNodeType.FunctionSyntaxNode, t)
        {
            IsPublic = isPub;
            IsPrivate = isPriv;
            IsProtected = isProt;
            IsInternal = isIntern;
            IsStatic = isStat;
            IsVirtual = isVirt;
            IsOverride = isOver;
            Parameters = new List<SyntaxNode>();
            ParameterNames = new List<string>();
        }

        public override string ToString()
        {
            var str = base.ToString() + " | Props: ";

            if (IsPublic)
                str += "Public, ";
            
            if (IsPrivate)
                str += "Private, ";
            
            if (IsProtected)
                str += "Protected, ";
            
            if (IsInternal)
                str += "Internal, ";
            
            if (IsStatic)
                str += "Static, ";
            
            if (IsVirtual)
                str += "Virtual, ";
            
            if (IsOverride)
                str += "Override, ";

            return str.Substring(0, str.Length - 2);
        }
    }
}
