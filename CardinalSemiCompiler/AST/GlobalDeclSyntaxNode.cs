using CardinalSemiCompiler.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalSemiCompiler.AST
{
    public class GlobalDeclSyntaxNode : SyntaxNode
    {
        public bool IsPublic {get; private set;}
        public bool IsPrivate {get; private set;}
        public bool IsProtected {get; private set;}
        public bool IsInternal {get; private set;}
        public bool IsStatic {get; private set;}
        public bool IsConst {get; private set;}
        public bool IsVolatile {get; private set;}
        public bool IsAtomic {get; private set;}
        public Token Identifier {get; set;}

        public GlobalDeclSyntaxNode(bool isPub, bool isPriv, bool isProt, bool isIntern, bool isStat, bool isConst, bool isVolatile, bool isAtomic, Token t) : base(SyntaxNodeType.GlobalDeclSyntaxNode, t)
        {
            IsPublic = isPub;
            IsPrivate = isPriv;
            IsProtected = isProt;
            IsInternal = isIntern;
            IsStatic = isStat;
            IsConst = isConst;
            IsVolatile = isVolatile;
            IsAtomic = isAtomic;
        }

        public override string ToString()
        {
            var str = base.ToString() + " | Props: ";

            return str.Substring(0, str.Length - 2);
        }
    }
}
