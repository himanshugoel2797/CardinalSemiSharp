﻿using CardinalSemiCompiler.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalSemiCompiler.AST
{
    public enum SyntaxNodeType
    {
        Unknown,
        RootSyntaxNode,
        UsingSyntaxNode,
        UsingStaticSyntaxNode,
        CompoundIdentifierNode,
        NamespaceSyntaxNode,
        ClassSyntaxNode,
        EnumSyntaxNode,
        EnumEntrySyntaxNode,
        Block,
        Attribute,
        GenericParameterNode,
        GenericDeclarationSyntaxNode,
        InheritanceSyntaxNode,
        GlobalDeclSyntaxNode,
        ArraySpecifier,
        DelegateSyntaxNode,
        PrefixSyntaxNode,
        ParameterNode,
        FunctionSyntaxNode,
        SpecialStatement,
        VariableNode,
        ConstantNode,
        AssignAndLambdaNode,
        ConditionalNode,
        NullCoalesceNode,
        CondOrNode,
        MultiplicativeNode,
        AdditiveNode,
        ShiftNode,
        RelationalTypeTestingNode,
        EqualityNode,
        LogicAndNode,
        LogicXorNode,
        LogicOrNode,
        CondAndNode,
        UnaryNode,
        PrimaryNode,
        SubexpressionNode,
        VariableDeclNode,
        IndexerAccess,
        FunctionCallNode,
        NestedExpression,
        TypeNode,
        TypeCastNode,
    }

    public class SyntaxNode
    {
        public SyntaxNodeType NodeType { get; set; }
        public Token Token { get; private set; }

        public List<SyntaxNode> ChildNodes { get; private set; }

        public SyntaxNode(SyntaxNodeType type, Token tkn)
        {
            NodeType = type;
            Token = tkn;
            ChildNodes = new List<SyntaxNode>();
        }

        public override string ToString()
        {
            return $"Type = {NodeType}" + ((Token == null) ? "" : $" | Value = {Token?.TokenValue}");
        }
    }
}
