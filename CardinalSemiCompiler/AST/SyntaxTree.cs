using CardinalSemiCompiler.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalSemiCompiler.AST
{
    public partial class SyntaxTree
    {
        #region Context management
        private static Queue<Token> ContextTokens;
        private static int AddContext(Token[] tkns, int idx)
        {
            ContextTokens.Enqueue(tkns[idx]);
            return idx + 1;
        }

        private static void ClearContext()
        {
            ContextTokens.Clear();
        }

        private static void HandleUnexpectedContext()
        {
            if (ContextTokens.Count != 0)
                foreach (Token t in ContextTokens)
                    HandleSingleUnexpectedContext(t);
        }

        private static void HandleSingleUnexpectedContext(Token t)
        {
            throw new SyntaxException("Unexpected context token.", t);
        }

        private static Token ConsumeContext()
        {
            return ContextTokens.Dequeue();
        }

        private static bool HasContext()
        {
            return ContextTokens.Count > 0;
        }
        #endregion

        #region Attribute management
        private static List<SyntaxNode> AttributeCollection;

        private static void ClearAttributes()
        {
            AttributeCollection.Clear();
        }
        #endregion

        public static SyntaxNode Build(Token[] tkns)
        {
            SyntaxNode rNode = new SyntaxNode(SyntaxNodeType.RootSyntaxNode, null);
            ContextTokens = new Queue<Token>();
            AttributeCollection = new List<SyntaxNode>();

            int pos = 0;
            do
            {
                pos = ProcessNode(rNode, tkns, pos);
            } while (pos < tkns.Length);

            //cleanup the tree, removing useless nodes.
            CleanupTree(rNode);

            //perform constant optimization
            ConstantOptimization.Optimize(rNode);

            //cleanup the tree again, removing useless nodes.
            CleanupTree(rNode);

            return rNode;
        }

        private static void CleanupTree(SyntaxNode n){
            if(n == null)
                return;
            for(int i = 0; i < n.ChildNodes.Count; i++){
                var node_t = n.ChildNodes[i];
                CleanupTree(node_t);
                n.ChildNodes[i] = node_t;

                if(n.ChildNodes[i] is OperatorSyntaxNode){
                    var node = n.ChildNodes[i] as OperatorSyntaxNode;
                    if(node.ChildNodes.Count == 1 && node.Operator.Count == 0){
                        n.ChildNodes[i] = n.ChildNodes[i].ChildNodes[0];
                    }
                }

                if(n.ChildNodes.Count == 1 && n.ChildNodes[0].NodeType == n.NodeType && n.ChildNodes[0].Token == n.Token){
                    var ents = n.ChildNodes[0].ChildNodes;
                    n.ChildNodes.Clear();
                    n.ChildNodes.AddRange(ents);
                }
                /*if(n.ChildNodes.Count == 1 && n.ChildNodes[0].ChildNodes.Count == 1){
                    var node2 = n.ChildNodes[0].ChildNodes[0];
                    n.ChildNodes.Clear();
                    n.ChildNodes.Add(node2);
                }*/
            }
        }

        //Parse only top level nodes
        private static int ProcessNode(SyntaxNode parent, Token[] tkns, int idx)
        {
            var curTkn = tkns[idx];
            if (curTkn.TokenType == TokenType.Keyword)
                switch (curTkn.TokenValue)
                {
                    case "using":
                        HandleUnexpectedContext();
                        idx = ParseUsing(parent, tkns, idx);
                        break;
                    case "namespace":
                        HandleUnexpectedContext();
                        idx = ParseNamespace(parent, tkns, idx);
                        break;
                    case "public":
                    case "internal":
                    case "partial":
                    case "abstract":
                    case "static":
                        idx = AddContext(tkns, idx);
                        break;
                    case "class":
                        idx = ParseClass(parent, tkns, idx, false);
                        break;
                    case "struct":
                        idx = ParseClass(parent, tkns, idx, true);
                        break;
                    case "enum":
                        idx = ParseEnum(parent, tkns, idx);
                        break;
                    default:
                        return idx + 1; //TODO: change this to throw an unknown syntax error
                }
            else if (curTkn.TokenType == TokenType.OpeningBracket)
            {
                //TODO: handle attribute parsing
                return idx + 1;
            }
            else
                return idx + 1; //TODO: change this to throw an unknown syntax error

            return idx;
        }

        private static int ProcessTypeDefNode(SyntaxNode parent, Token[] tkns, int idx)
        {
            var curTkn = tkns[idx];
            if (curTkn.TokenType == TokenType.Keyword)
                switch (curTkn.TokenValue)
                {
                    case "public":
                    case "private":
                    case "protected":
                    case "internal":
                    case "virtual":
                    case "abstract":
                    case "static":
                    case "extern":
                    case "volatile":
                    case "implicit":
                    case "explicit":
                    case "const":
                    case "override":
                    case "unsafe":
                    case "atomic":
                        idx = AddContext(tkns, idx);
                        break;
                    case "delegate":
                        idx = ParseDelegate(parent, tkns, idx);
                        break;
                    case "event":
                        idx = ParseEvent(parent, tkns, idx);
                        break;
                    case "property":
                        idx = ParseProperty(parent, tkns, idx);
                        break;
                    case "indexer":
                        idx = ParseIndexer(parent, tkns, idx);
                        break;
                    case "var":
                        idx = ParseGlobalVar(parent, tkns, idx);
                        break;
                    case "operator":
                        idx = ParseFunction(parent, tkns, idx, true);
                        break;
                    case "func":
                        idx = ParseFunction(parent, tkns, idx, false);
                        break;
                    default:
                        return idx + 1; //TODO: change this to throw an unknown syntax error
                }
            else if (curTkn.TokenType == TokenType.OpeningBracket)
            {
                //TODO: handle attribute parsing
                return idx + 1;
            }
            else
                return idx + 1; //TODO: change this to throw an unknown syntax error

            return idx;
        }
        private static int ParseCompoundIdentifier(SyntaxNode parent, Token[] tkns, int idx)
        {
            //Build the identifier
            var curTkn = tkns[idx++];

            if (curTkn.TokenType != TokenType.Identifier)
                throw new SyntaxException("Expected identifier.", curTkn);

            string identifier = "";
            int startPos = curTkn.StartPosition;
            int line = curTkn.Line;
            int col = curTkn.Column;

            bool dotExpected = false;
            do
            {
                if (curTkn.TokenType == TokenType.Identifier && !dotExpected)
                    identifier += curTkn.TokenValue;
                else if (curTkn.TokenType == TokenType.Dot && dotExpected)
                    identifier += ".";
                else if (dotExpected)
                {
                    parent.ChildNodes.Add(new SyntaxNode(SyntaxNodeType.CompoundIdentifierNode, new Token(TokenType.Identifier, identifier, startPos, line, col)));
                    return idx - 1;
                }
                else
                    throw new SyntaxException("Unexpected end of identifier.", curTkn);

                dotExpected = !dotExpected;

                curTkn = tkns[idx++];
            } while (true);
        }

        private static int ParseTypeReference(SyntaxNode parent, Token[] tkns, int idx, bool acceptPrefix){
            
            //Build the identifier
            var curTkn = tkns[idx++];

            if (curTkn.TokenType != TokenType.Identifier && curTkn.TokenType != TokenType.Keyword)
                throw new SyntaxException("Expected identifier.", curTkn);

            Token prefix = null;
            string identifier = "";
            string[] builtin_types = new string[] {
                "bool",
                "byte",
                "char",
                "decimal",
                "double",
                "float",
                "int",
                "long",
                "object",
                "sbyte",
                "short",
                "string",
                "uint",
                "ulong",
                "ushort",
            };

            string[] prefixes = new string[] {
                "in",
                "out",
                "ref",
                "params"
            };

            int startPos = curTkn.StartPosition;
            int line = curTkn.Line;
            int col = curTkn.Column;

            bool dotExpected = false;
            bool typeNameCompleted = false;
            do
            {
                if (curTkn.TokenType == TokenType.Identifier && !dotExpected && !typeNameCompleted)
                    identifier += curTkn.TokenValue;
                else if(curTkn.TokenType == TokenType.Keyword && !dotExpected && !typeNameCompleted){
                    if(builtin_types.Contains(curTkn.TokenValue)){
                        identifier += curTkn.TokenValue;
                        typeNameCompleted = true;
                    }
                    else if(prefixes.Contains(curTkn.TokenValue) && acceptPrefix){
                        prefix = curTkn;
                        dotExpected = !dotExpected;
                    }else
                        throw new SyntaxException("Unexpected keyword.", curTkn);
                }
                else if (curTkn.TokenType == TokenType.Dot && dotExpected && !typeNameCompleted)
                    identifier += ".";
                else if (dotExpected | typeNameCompleted)
                {
                    var nNode = new SyntaxNode(SyntaxNodeType.CompoundIdentifierNode, new Token(TokenType.Identifier, identifier, startPos, line, col));
                    parent.ChildNodes.Add(nNode);
                    if(prefix != null)
                        nNode.ChildNodes.Add(new SyntaxNode(SyntaxNodeType.PrefixSyntaxNode, prefix));

                    if(tkns[idx - 1].TokenType == TokenType.OpeningBracket)
                        if(tkns[idx].TokenType == TokenType.ClosingBracket){
                            nNode.ChildNodes.Add(new SyntaxNode(SyntaxNodeType.ArraySpecifier, tkns[idx - 1]));
                            return idx + 1;
                        }
                        else
                            throw new SyntaxException("Expected closing bracket.", tkns[idx]);

                    return idx - 1;
                }
                else
                    throw new SyntaxException("Unexpected end of identifier.", curTkn);

                dotExpected = !dotExpected;

                curTkn = tkns[idx++];
            } while (true);
        }

        private static int ParseGenericDeclaration(SyntaxNode parent, Token[] tkns, int idx)
        {
            idx = ParseCompoundIdentifier(parent, tkns, idx);

            if (tkns[idx].TokenType == TokenType.OpeningAngle)
            {
                //Handle generics
                bool expectedIdent = true;
                idx++;
                while(tkns[idx].TokenType != TokenType.ClosingAngle)
                {
                    var gTkn = tkns[idx];
                    if(gTkn.TokenType == TokenType.Identifier && expectedIdent){
                        idx = ParseCompoundIdentifier(parent, tkns, idx);
                    }else if(gTkn.TokenType == TokenType.Comma && !expectedIdent){
                        idx++;
                    }
                    expectedIdent = !expectedIdent;
                }
                idx++;
            }

            return idx;
        }

        //public delegate int DelegateName(Type, Type, Type);
        private static int ParseDelegate(SyntaxNode parent, Token[] tkns, int idx){
            //Check for any context and consume it
            var curTkn = tkns[idx];

            bool isPub = false;
            bool isPriv = false;
            bool isProt = false;
            bool isIntern = false;

            while (HasContext())
            {
                var tkn = ConsumeContext();
                if (tkn.TokenType == TokenType.Keyword)
                    switch (tkn.TokenValue)
                    {
                        case "public":
                            isPub = true;
                            break;
                        case "private":
                            isPriv = true;
                            break;
                        case "protected":
                            isProt = true;
                            break;
                        case "internal":
                            isIntern = true;
                            break;
                        default:
                            HandleSingleUnexpectedContext(tkn);
                            break;
                    }
            }

            var node = new DelegateSyntaxNode(isPub, isPriv, isProt, isIntern, curTkn);
            parent.ChildNodes.Add(node);
            idx = ParseTypeReference(node, tkns, idx + 1, false);
            
            if(tkns[idx].TokenType == TokenType.Identifier){
                node.Identifier = tkns[idx];
            }else
                throw new SyntaxException("Expected Identifier.", tkns[idx]);

            //Parse the arguments
            idx++;
            if(tkns[idx].TokenType == TokenType.OpeningParen){
                idx++;
                while(true){
                    if(tkns[idx].TokenType == TokenType.ClosingParen){
                        idx++;
                        break;
                    }

                    var nNode = new SyntaxNode(SyntaxNodeType.ParameterNode, tkns[idx]);
                    node.Parameters.Add(nNode);
                    idx = ParseTypeReference(node, tkns, idx, true);

                    if(tkns[idx].TokenType == TokenType.Comma)
                        idx++;
                }
            }else
                throw new SyntaxException("Expected parameter list.", tkns[idx]);

            if(tkns[idx].TokenType == TokenType.Semicolon){
                return idx + 1;
            }else
                throw new SyntaxException("Expected semicolon.", tkns[idx]);
        }

        private static int ParseEvent(SyntaxNode parent, Token[] tkns, int idx){
            throw new NotImplementedException("Events not implemented yet.");
        }

        //CONST_EXPR
        private static int ParseConstExpression(SyntaxNode parent, Token[] tkns, int idx){
            throw new NotImplementedException("Constant expressions not implemented.");
        }

        private static int ParseConditional(SyntaxNode parent, Token[] tkns, int idx){
            //CONDITIONAL
            throw new NotImplementedException("Conditionals not implemented.");
        }

        private static int ParseAssignmentExpression(SyntaxNode parent, Token[] tkns, int idx) {
            throw new NotImplementedException("LValues not implemented.");
        }

        private static int ParseSubExpression(SyntaxNode parent, Token[] tkns, int idx){
            var terms = new TokenType[]{ TokenType.Semicolon, TokenType.ClosingParen, TokenType.Comma, TokenType.ClosingBracket };
            while(!terms.Contains(tkns[idx].TokenType))
                idx = ParseAssignAndLambdaExpr(parent, tkns, idx);
            return idx;
        }

        private static int ParseExpression(SyntaxNode parent, Token[] tkns, int idx){
            //SUB_EXPRESSION(, EXPRESSION)*
            var nNode = new SyntaxNode(SyntaxNodeType.SpecialStatement, tkns[idx]);
            parent.ChildNodes.Add(nNode);


            idx = ParseSubExpression(nNode, tkns, idx);
            if(tkns[idx].TokenType == TokenType.Comma){
                while(tkns[idx].TokenType == TokenType.Comma){
                    var nNode2 = new SyntaxNode(SyntaxNodeType.SubexpressionNode, tkns[idx]);
                    nNode.ChildNodes.Add(nNode2);
                    idx = ParseSubExpression(nNode2, tkns, idx + 1);
                }
            }
            return idx;
        }

        private static int ParseStatement(SyntaxNode parent, Token[] tkns, int idx){
            //CODE_BLOCK;
            if(tkns[idx].TokenType == TokenType.OpeningBrace){
                var nNode = new SyntaxNode(SyntaxNodeType.Block, tkns[idx]);
                parent.ChildNodes.Add(nNode);
                return ParseCodeBlock(nNode, tkns, idx);
            } else if(tkns[idx].TokenType == TokenType.Keyword) {
                switch(tkns[idx].TokenValue){
                    //while (CONDITIONAL) CODE_BLOCK
                    case "while":
                        {
                            var nNode = new SyntaxNode(SyntaxNodeType.SpecialStatement, tkns[idx]);
                            if(tkns[idx + 1].TokenType == TokenType.OpeningParen)
                            {
                                idx = ParseExpression(nNode, tkns, idx + 2);
                                parent.ChildNodes.Add(nNode);

                                if(tkns[idx].TokenType != TokenType.ClosingParen)
                                    throw new SyntaxException("Expected closing parenthesis.", tkns[idx]);
                                return ParseCodeBlock(nNode, tkns, idx + 1);
                            }else
                                throw new SyntaxException("Expected conditional.", tkns[idx + 1]);
                        }
                        break;
                    //{else} if (CONDITIONAL) CODE_BLOCK
                    case "else":
                    case "if":
                        {
                            var nNode = new SyntaxNode(SyntaxNodeType.SpecialStatement, tkns[idx]);
                            if(tkns[idx].TokenValue == "else"){
                                if(tkns[idx + 1].TokenValue == "if"){
                                    var ifNode = new SyntaxNode(SyntaxNodeType.SpecialStatement, tkns[idx + 1]);
                                    nNode.ChildNodes.Add(ifNode);
                                    nNode = ifNode;
                                    idx++;
                                }else{
                                    parent.ChildNodes.Add(nNode);
                                    return ParseCodeBlock(nNode, tkns, idx + 1);
                                }
                            }
                            if(tkns[idx + 1].TokenType == TokenType.OpeningParen)
                            {
                                idx = ParseExpression(nNode, tkns, idx + 2);
                                parent.ChildNodes.Add(nNode);

                                if(tkns[idx].TokenType != TokenType.ClosingParen)
                                    throw new SyntaxException("Expected closing parenthesis.", tkns[idx]);
                                return ParseCodeBlock(nNode, tkns, idx + 1);
                            }else
                                throw new SyntaxException("Expected conditional.", tkns[idx + 1]);
                        }
                        break;
                    //do CODE_BLOCK while(EXPRESSION);
                    case "do":
                        {
                            var nNode = new SyntaxNode(SyntaxNodeType.SpecialStatement, tkns[idx]);
                            parent.ChildNodes.Add(nNode);
                            idx = ParseCodeBlock(nNode, tkns, idx + 1);
                            if(tkns[idx].TokenType != TokenType.Keyword && tkns[idx].TokenValue == "while")
                                throw new SyntaxException("Expected 'while' statement.", tkns[idx]);

                            if(tkns[idx + 1].TokenType != TokenType.OpeningParen)
                                throw new SyntaxException("Expected opening parenthesis.", tkns[idx + 1]);

                            idx = ParseExpression(nNode, tkns, idx);

                            if (tkns[idx].TokenType != TokenType.ClosingParen)
                                throw new SyntaxException("Expected closing parenthesis.", tkns[idx]);
                                
                            if (tkns[idx + 1].TokenType != TokenType.Semicolon)
                                throw new SyntaxException("Expected semicolon.", tkns[idx + 1]);
                            
                            return idx + 2;
                        }
                        break;
                    //for({EXPRESSION};{EXPRESSION};{EXPRESSION}) CODE_BLOCK
                    case "for":
                        {
                            var nNode = new SyntaxNode(SyntaxNodeType.SpecialStatement, tkns[idx]);
                            parent.ChildNodes.Add(nNode);

                            if(tkns[idx + 1].TokenType != TokenType.OpeningParen)
                                throw new SyntaxException("Expected opening parenthesis.", tkns[idx + 1]);

                            idx++;
                            for(int i = 0; i < 3; i++){
                                if(tkns[idx + 1].TokenType != TokenType.Semicolon){
                                    idx = ParseExpression(nNode, tkns, idx + 1);
                                }
                                else{
                                    nNode.ChildNodes.Add(null);
                                    idx++;
                                }
                            }

                            if(tkns[idx].TokenType != TokenType.ClosingParen)
                                throw new SyntaxException("Expected closing parenthesis.", tkns[idx]);

                            return ParseCodeBlock(nNode, tkns, idx + 1);
                        }
                        break;
                    //foreach(TYPE IDENTIFIER in LVALUE) CODE_BLOCK
                    case "foreach":
                        {
                            var nNode = new SyntaxNode(SyntaxNodeType.SpecialStatement, tkns[idx]);
                            parent.ChildNodes.Add(nNode);

                            if(tkns[idx + 1].TokenType != TokenType.OpeningParen)
                                throw new SyntaxException("Expected opening parenthesis.", tkns[idx + 1]);

                            idx = ParseTypeReference(nNode, tkns, idx + 2, false);
                            if(tkns[idx].TokenType != TokenType.Identifier)
                                throw new SyntaxException("Expected identifier.", tkns[idx]);
                            nNode.ChildNodes.Add(new SyntaxNode(SyntaxNodeType.VariableNode, tkns[idx]));
                            
                            if(tkns[idx + 1].TokenType != TokenType.Keyword | tkns[idx + 1].TokenValue != "in")
                                throw new SyntaxException("Expected 'in' keyword.", tkns[idx + 1]);

                            idx = ParseExpression(nNode, tkns, idx + 2);
                            if(tkns[idx].TokenType != TokenType.ClosingParen)
                                throw new SyntaxException("Expected closing parenthesis.", tkns[idx]);
                            return ParseCodeBlock(nNode, tkns, idx + 1);
                        }
                        break;
                    //switch(EXPRESSION) CODE_BLOCK
                    case "switch":
                        {
                            var nNode = new SyntaxNode(SyntaxNodeType.SpecialStatement, tkns[idx]);
                            parent.ChildNodes.Add(nNode);

                            if(tkns[idx + 1].TokenType != TokenType.OpeningParen)
                                throw new SyntaxException("Expected opening parenthesis.", tkns[idx + 1]);

                            idx = ParseExpression(nNode, tkns, idx + 2);
                            
                            if(tkns[idx].TokenType != TokenType.ClosingParen)
                                throw new SyntaxException("Expected closing parenthesis.", tkns[idx]);
                            return ParseCodeBlock(nNode, tkns, idx + 1);
                        }
                        break;
                    //case CONST_EXPR : CODE_BLOCK
                    case "case":
                        {
                            var nNode = new SyntaxNode(SyntaxNodeType.SpecialStatement, tkns[idx + 1]);
                            parent.ChildNodes.Add(nNode);
                            idx = ParseConstExpression(nNode, tkns, idx + 1);
                            if(tkns[idx].TokenType != TokenType.Colon)
                                throw new SyntaxException("Expected colon.", tkns[idx]);

                            return ParseCodeBlock(nNode, tkns, idx + 1);
                        }
                        break;
                    //default : CODE_BLOCK
                    case "default":
                        {
                            if(tkns[idx + 1].TokenType != TokenType.Colon)
                                throw new SyntaxException("Expected colon.", tkns[idx + 1]);

                            var nNode = new SyntaxNode(SyntaxNodeType.SpecialStatement, tkns[idx]);
                            parent.ChildNodes.Add(nNode);
                            return ParseCodeBlock(nNode, tkns, idx + 2);
                        }
                        break;
                    //continue;
                    //break;
                    case "continue":
                    case "break":
                        {
                            var nNode = new SyntaxNode(SyntaxNodeType.SpecialStatement, tkns[idx]);
                            parent.ChildNodes.Add(nNode);
                            if(tkns[idx + 1].TokenType != TokenType.Semicolon)
                                throw new SyntaxException("Expected semicolon.", tkns[idx + 1]);
                            return idx + 2;
                        }
                        break;
                    //return EXPRESSION;
                    //throw EXPRESSION;
                    case "return":
                    case "throw":
                        {
                            var nNode = new SyntaxNode(SyntaxNodeType.SpecialStatement, tkns[idx]);
                            parent.ChildNodes.Add(nNode);
                            idx = ParseExpression(nNode, tkns, idx + 1);
                            if(tkns[idx].TokenType != TokenType.Semicolon)
                                throw new SyntaxException("Expected semicolon.", tkns[idx + 1]);
                            return idx + 1;
                        }
                        break;
                    //try CODE_BLOCK catch(TYPE IDENTIFIER) CODE_BLOCK finally CODE_BLOCK
                    case "try":
                        {
                            var nNode = new SyntaxNode(SyntaxNodeType.SpecialStatement, tkns[idx]);
                            idx = ParseCodeBlock(nNode, tkns, idx + 1);
                            int catch_cnt = 0;
                            while(true){

                                var catchNode = new SyntaxNode(SyntaxNodeType.SpecialStatement, tkns[idx]);
                                nNode.ChildNodes.Add(catchNode);
                                if(tkns[idx].TokenType != TokenType.Keyword | tkns[idx].TokenValue != "catch")
                                    if(catch_cnt == 0)throw new SyntaxException("Expected catch statement.", tkns[idx]);
                                    else break;
                            
                                if(tkns[idx + 1].TokenType != TokenType.OpeningParen)
                                    throw new SyntaxException("Expected opening parenthesis.", tkns[idx + 1]);

                                idx = ParseTypeReference(catchNode, tkns, idx + 2, false);
                                
                                if(tkns[idx].TokenType == TokenType.Identifier)
                                    catchNode.ChildNodes.Add(new SyntaxNode(SyntaxNodeType.VariableNode, tkns[idx]));

                                if(tkns[idx + 1].TokenType != TokenType.ClosingParen)
                                    throw new SyntaxException("Expected closing parenthesis.", tkns[idx]);

                                idx = ParseCodeBlock(catchNode, tkns, idx);
                                catch_cnt++;
                            }
                            if(tkns[idx].TokenType == TokenType.Keyword && tkns[idx].TokenValue == "finally"){
                                var finallyNode = new SyntaxNode(SyntaxNodeType.SpecialStatement, tkns[idx]);
                                nNode.ChildNodes.Add(finallyNode);
                                idx = ParseCodeBlock(finallyNode, tkns, idx);
                            }
                            return idx;
                        }
                        break;
                }
            }else if(tkns[idx].TokenType == TokenType.Semicolon)
                return idx + 1;
            else
                return ParseExpression(parent, tkns, idx);

            throw new NotImplementedException("Function code parsing not implemented yet.");
        }
        private static int ParseCodeBlock(SyntaxNode parent, Token[] tkns, int idx) {

            var nNode = new SyntaxNode(SyntaxNodeType.Block, tkns[idx]);
            parent.ChildNodes.Add(nNode);
            if(tkns[idx].TokenType != TokenType.OpeningBrace)
                return ParseStatement(nNode, tkns, idx);
            else{
                idx++;
                while(tkns[idx].TokenType != TokenType.ClosingBrace)
                    idx = ParseStatement(nNode, tkns, idx);

                return idx + 1;
            }
        }

        //public static func int FunctionName(TypeName a, TypeName b, TypeName c) {}
        private static int ParseFunction(SyntaxNode parent, Token[] tkns, int idx, bool oper){
            //Check for any context and consume it
            var curTkn = tkns[idx];

            bool isPub = false;
            bool isPriv = false;
            bool isProt = false;
            bool isIntern = false;
            bool isStat = false;
            bool isVirt = false;
            bool isOver = false;

            while (HasContext())
            {
                var tkn = ConsumeContext();
                if (tkn.TokenType == TokenType.Keyword)
                    switch (tkn.TokenValue)
                    {
                        case "public":
                            isPub = true;
                            break;
                        case "private":
                            isPriv = true;
                            break;
                        case "protected":
                            isProt = true;
                            break;
                        case "internal":
                            isIntern = true;
                            break;
                        case "static":
                            isStat = true;
                            break;
                        case "virtual":
                            isVirt = true;
                            break;
                        case "override":
                            isOver = true;
                            break;
                        default:
                            HandleSingleUnexpectedContext(tkn);
                            break;
                    }
            }

            var node = new FunctionSyntaxNode(isPub, isPriv, isProt, isIntern, isStat, isVirt, isOver, curTkn);
            parent.ChildNodes.Add(node);
            idx = ParseTypeReference(node, tkns, idx + 1, false);
            
            if(tkns[idx].TokenType == TokenType.Identifier){
                node.Identifier = tkns[idx];
            }else
                throw new SyntaxException("Expected Identifier.", tkns[idx]);

            //Parse the arguments
            idx++;
            if(tkns[idx].TokenType == TokenType.OpeningParen){
                idx++;
                while(true){
                    if(tkns[idx].TokenType == TokenType.ClosingParen){
                        idx++;
                        break;
                    }

                    var nNode = new SyntaxNode(SyntaxNodeType.ParameterNode, tkns[idx]);
                    node.Parameters.Add(nNode);
                    idx = ParseTypeReference(node, tkns, idx, true);
                    if(tkns[idx].TokenType == TokenType.Identifier){
                        node.ParameterNames.Add(tkns[idx].TokenValue);
                        idx++;
                    }
                    else
                        throw new SyntaxException("Invalid parameter name.", tkns[idx]);

                    if(tkns[idx].TokenType == TokenType.Comma)
                        idx++;
                }
            }else
                throw new SyntaxException("Expected parameter list.", tkns[idx]);

            if(tkns[idx].TokenType == TokenType.OpeningBrace){
                var blk_node = new SyntaxNode(SyntaxNodeType.Block, tkns[idx]);
                node.ChildNodes.Add(blk_node);

                idx++;
                while (tkns[idx].TokenType != TokenType.ClosingBrace)
                {
                    if (idx >= tkns.Length)
                        throw new SyntaxException("Expected '}'", tkns[idx]);

                    idx = ParseCodeBlock(blk_node, tkns, idx);
                }

                return idx + 1;
            }else
                throw new SyntaxException("Expected semicolon.", tkns[idx]);
        }

        private static int ParseProperty(SyntaxNode parent, Token[] tkns, int idx){
            throw new NotImplementedException();
        }
        
        private static int ParseIndexer(SyntaxNode parent, Token[] tkns, int idx){
            throw new NotImplementedException();
        }
        
        //public static var int VarName = ConstVal;
        //public static var int VarName;
        private static int ParseGlobalVar(SyntaxNode parent, Token[] tkns, int idx){
            //Check for any context and consume it
            var curTkn = tkns[idx];

            bool isPub = false;
            bool isPriv = false;
            bool isProt = false;
            bool isStat = false;
            bool isIntern = false;
            bool isConst = false;
            bool isVolatile = false;
            bool isAtomic = false;

            while (HasContext())
            {
                var tkn = ConsumeContext();
                if (tkn.TokenType == TokenType.Keyword)
                    switch (tkn.TokenValue)
                    {
                        case "public":
                            isPub = true;
                            break;
                        case "private":
                            isPriv = true;
                            break;
                        case "protected":
                            isProt = true;
                            break;
                        case "internal":
                            isIntern = true;
                            break;
                        case "static":
                            isStat = true;
                            break;
                        case "const":
                            isConst = true;
                            break;
                        case "volatile":
                            isVolatile = true;
                            break;
                        case "atomic":
                            isAtomic = true;
                            break;
                        default:
                            HandleSingleUnexpectedContext(tkn);
                            break;
                    }
            }

            var node = new GlobalDeclSyntaxNode(isPub, isPriv, isProt, isIntern, isStat, isConst, isVolatile, isAtomic, curTkn);
            parent.ChildNodes.Add(node);
            idx = ParseTypeReference(node, tkns, idx + 1, false);
            
            if(tkns[idx].TokenType == TokenType.Identifier){
                node.Identifier = tkns[idx];
            }else
                throw new SyntaxException("Expected Identifier.", tkns[idx]);

            idx++;

            if(tkns[idx].TokenType == TokenType.Semicolon){
                return idx + 1;
            }else if(tkns[idx].TokenType == TokenType.Operator && tkns[idx].TokenValue == "="){
                //Parse the constant expression
                return ParseConstExpression(node, tkns, idx + 1);
            }else
                throw new SyntaxException("Expected expression or semicolon.", tkns[idx]);
        }

        private static int ParseClass(SyntaxNode parent, Token[] tkns, int idx, bool valueType)
        {
            //Check for any context and consume it
            var curTkn = tkns[idx];

            bool isPub = false;
            bool isPart = false;
            bool isStat = false;
            bool isIntern = false;
            bool isAbstract = false;

            while (HasContext())
            {
                var tkn = ConsumeContext();
                if (tkn.TokenType == TokenType.Keyword)
                    switch (tkn.TokenValue)
                    {
                        case "public":
                            isPub = true;
                            break;
                        case "partial":
                            isPart = true;
                            break;
                        case "static":
                            isStat = true;
                            break;
                        case "internal":
                            isIntern = true;
                            break;
                        case "abstract":
                            isAbstract = true;
                            break;
                        default:
                            HandleSingleUnexpectedContext(tkn);
                            break;
                    }
            }

            var node = new ClassSyntaxNode(curTkn, valueType, isPub, isIntern, isPart, isStat, isAbstract);
            parent.ChildNodes.Add(node);
            idx = ParseCompoundIdentifier(node, tkns, idx + 1);

            if (tkns[idx].TokenType == TokenType.OpeningAngle)
            {
                //Handle generics
                bool expectedIdent = true;
                idx++;

                while(tkns[idx].TokenType != TokenType.ClosingAngle)
                {
                    var gTkn = tkns[idx];
                    if(gTkn.TokenType == TokenType.Identifier && expectedIdent){
                        node.GenericParameters.Add(new SyntaxNode(SyntaxNodeType.GenericParameterNode, gTkn));
                    }else if(gTkn.TokenType == TokenType.Comma && !expectedIdent){

                    }

                    expectedIdent = !expectedIdent;
                    idx++;
                }
            }

            if (tkns[idx].TokenType == TokenType.Colon)
            {
                //Handle inheritance
                while(true){
                    idx++;

                    var iNode = new SyntaxNode(SyntaxNodeType.InheritanceSyntaxNode, tkns[idx]);
                    idx = ParseGenericDeclaration(iNode, tkns, idx);
                    node.InheritanceSet.Add(iNode);

                    if(tkns[idx].TokenType == TokenType.Comma)
                        continue;
                    else if(tkns[idx].TokenType == TokenType.Keyword && tkns[idx].TokenValue == "where")
                        break;
                    else if(tkns[idx].TokenType == TokenType.OpeningBrace)
                        break;
                    else
                        throw new SyntaxException("Unexpected token.", tkns[idx]);
                }
            }

            if (tkns[idx].TokenType == TokenType.Keyword && tkns[idx].TokenValue == "where"){
                //Handle generic constraints
                while(tkns[idx].TokenType == TokenType.Keyword && tkns[idx].TokenValue == "where")
                {
                    throw new NotImplementedException("Generic constraint support not implemented yet.");
                    idx++;
                }
            }

            if (tkns[idx].TokenType != TokenType.OpeningBrace)
                throw new SyntaxException("Expected '{'", tkns[idx]);

            var attr_node = new SyntaxNode(SyntaxNodeType.Attribute, null);
            node.ChildNodes.Add(attr_node);
            attr_node.ChildNodes.AddRange(AttributeCollection);
            ClearAttributes();

            var blk_node = new SyntaxNode(SyntaxNodeType.Block, tkns[idx]);
            node.ChildNodes.Add(blk_node);

            while (tkns[idx].TokenType != TokenType.ClosingBrace)
            {
                if (idx >= tkns.Length)
                    throw new SyntaxException("Expected '}'", tkns[idx]);

                idx = ProcessTypeDefNode(blk_node, tkns, idx);
            }
            return idx + 1;
        }

        private static int ParseEnumEntry(SyntaxNode parent, Token[] tkns, int idx)
        {
            var curTkn = tkns[idx++];

            if (curTkn.TokenType != TokenType.Identifier)
                throw new SyntaxException("Expected identifier.", curTkn);

            string nm = curTkn.TokenValue;
            ulong val = (ulong)parent.ChildNodes.Count;
            EnumEntrySyntaxNode ent = new EnumEntrySyntaxNode(curTkn, nm, val);
            parent.ChildNodes.Add(ent);

            if(tkns[idx].TokenType == TokenType.Operator && tkns[idx].TokenValue == "=")
            {
                //Parse the constant expression
                return ParseConstExpression(ent, tkns, idx + 1);
            }


            if (tkns[idx].TokenType == TokenType.Comma)
                idx++;

            return idx;
        }

        private static int ParseEnum(SyntaxNode parent, Token[] tkns, int idx)
        {
            //Check for any context and consume it
            var curTkn = tkns[idx];

            bool isPub = false;

            while (HasContext())
            {
                var tkn = ConsumeContext();
                if (tkn.TokenType == TokenType.Keyword)
                    switch (tkn.TokenValue)
                    {
                        case "public":
                            isPub = true;
                            break;
                        default:
                            HandleSingleUnexpectedContext(tkn);
                            break;
                    }
            }

            var node = new EnumSyntaxNode(curTkn, isPub);
            parent.ChildNodes.Add(node);
            idx = ParseCompoundIdentifier(node, tkns, idx + 1);

            if (tkns[idx].TokenType == TokenType.Colon)
            {
                //TODO: Handle inheritance
                throw new NotImplementedException("Implement enum inheritance.");
            }

            while (tkns[idx].TokenType != TokenType.OpeningBrace)
                idx++;

            if (tkns[idx].TokenType != TokenType.OpeningBrace)
                throw new SyntaxException("Expected '{'", tkns[idx]);

            var attr_node = new SyntaxNode(SyntaxNodeType.Attribute, null);
            node.ChildNodes.Add(attr_node);
            attr_node.ChildNodes.AddRange(AttributeCollection);
            ClearAttributes();

            var blk_node = new SyntaxNode(SyntaxNodeType.Block, tkns[idx]);
            node.ChildNodes.Add(blk_node);

            idx++;
            while (tkns[idx].TokenType != TokenType.ClosingBrace)
            {
                if (idx >= tkns.Length)
                    throw new SyntaxException("Expected '}'", tkns[idx]);

                idx = ParseEnumEntry(blk_node, tkns, idx);
            }
            return idx + 1;
        }

        private static int ParseNamespace(SyntaxNode parent, Token[] tkns, int idx)
        {
            var curTkn = tkns[idx];
            var node = new SyntaxNode(SyntaxNodeType.NamespaceSyntaxNode, curTkn);
            parent.ChildNodes.Add(node);

            idx = ParseCompoundIdentifier(node, tkns, idx + 1);

            if (tkns[idx].TokenType != TokenType.OpeningBrace)
                throw new SyntaxException("Expected '{'", tkns[idx]);

            var attr_node = new SyntaxNode(SyntaxNodeType.Attribute, null);
            node.ChildNodes.Add(attr_node);
            attr_node.ChildNodes.AddRange(AttributeCollection);
            ClearAttributes();

            var blk_node = new SyntaxNode(SyntaxNodeType.Block, tkns[idx]);
            node.ChildNodes.Add(blk_node);

            while (tkns[idx].TokenType != TokenType.ClosingBrace)
            {
                if (idx >= tkns.Length)
                    throw new SyntaxException("Expected '}'", tkns[idx]);

                idx = ProcessNode(blk_node, tkns, idx);
            }
            return idx + 1;
        }

        private static int ParseUsing(SyntaxNode parent, Token[] tkns, int idx)
        {
            var curTkn = tkns[idx];
            var nTkn = tkns[idx + 1];
            if (nTkn.TokenType == TokenType.Keyword && nTkn.TokenValue == "static")
            {
                var node = new SyntaxNode(SyntaxNodeType.UsingStaticSyntaxNode, tkns[idx]);
                parent.ChildNodes.Add(node);
                idx = ParseCompoundIdentifier(node, tkns, idx + 2);

                if (tkns[idx].TokenType == TokenType.Semicolon)
                    return idx;
                else
                    throw new SyntaxException("Expected semicolon.", tkns[idx]);
            }
            else
            {
                var node = new SyntaxNode(SyntaxNodeType.UsingSyntaxNode, tkns[idx]);
                parent.ChildNodes.Add(node);
                idx = ParseCompoundIdentifier(node, tkns, idx + 1);

                //Check for alias mode
                if (tkns[idx].TokenType == TokenType.Operator && tkns[idx].TokenValue == "=")
                    return ParseCompoundIdentifier(node, tkns, idx + 1);
                else if (tkns[idx].TokenType == TokenType.Semicolon)
                    return idx + 1;
                else
                    throw new SyntaxException("Expected semicolon.", tkns[idx]);
            }
        }
    }
}
