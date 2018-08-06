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
        //NOTE: Sorted by lowest to highest precedence
        private static int ParseAssignAndLambdaExpr(SyntaxNode parent, Token[] tkns, int idx) {
            var nNode = new OperatorSyntaxNode(SyntaxNodeType.AssignAndLambdaNode, tkns[idx]);
            parent.ChildNodes.Add(nNode);
            idx = ParseConditionalExpr(nNode, tkns, idx);

            if(tkns[idx].TokenType == TokenType.Operator && tkns[idx].TokenValue == "=>"){
                nNode.Operator.Add(tkns[idx]);
                return ParseCodeBlock(nNode, tkns, idx + 1);
            }else if(tkns[idx].TokenType == TokenType.AssignmentOperator){
                nNode.Operator.Add(tkns[idx]);
                return ParseConditionalExpr(nNode, tkns, idx + 1);
            }else
                return idx;
        }

        private static int ParseConditionalExpr(SyntaxNode parent, Token[] tkns, int idx){
            var nNode = new OperatorSyntaxNode(SyntaxNodeType.ConditionalNode, tkns[idx]);
            parent.ChildNodes.Add(nNode);
            idx = ParseNullCoalesceExpr(nNode, tkns, idx);

            if(tkns[idx].TokenType == TokenType.Operator &&  tkns[idx].TokenValue == "?"){
                nNode.Operator.Add(tkns[idx]);
                idx = ParseNullCoalesceExpr(nNode, tkns, idx + 1);
                if(tkns[idx].TokenType == TokenType.Operator && tkns[idx].TokenValue == ":"){
                    return ParseNullCoalesceExpr(nNode, tkns, idx + 1);
                }else
                    throw new SyntaxException("Incomplete conditional operator.", tkns[idx]);
            }else
                return idx;
        }

        private static int ParseNullCoalesceExpr(SyntaxNode parent, Token[] tkns, int idx){
            var nNode = new OperatorSyntaxNode(SyntaxNodeType.NullCoalesceNode, tkns[idx]);
            parent.ChildNodes.Add(nNode);
            idx = ParseCondOrExpr(nNode, tkns, idx);

            if(tkns[idx].TokenType == TokenType.Operator && tkns[idx].TokenValue == "??"){
                while(tkns[idx].TokenType == TokenType.Operator && tkns[idx].TokenValue == "??"){
                    nNode.Operator.Add(tkns[idx]);
                    idx = ParseCondOrExpr(nNode, tkns, idx + 1);
                }
                return idx;
            }
            else
                return idx;
        }

        private static int ParseCondOrExpr(SyntaxNode parent, Token[] tkns, int idx){
            var nNode = new OperatorSyntaxNode(SyntaxNodeType.CondOrNode, tkns[idx]);
            parent.ChildNodes.Add(nNode);
            idx = ParseCondAndExpr(nNode, tkns, idx);
            
            if(tkns[idx].TokenType == TokenType.Operator && tkns[idx].TokenValue == "||"){
                while(tkns[idx].TokenType == TokenType.Operator && tkns[idx].TokenValue == "||"){
                    nNode.Operator.Add(tkns[idx]);
                    idx = ParseCondAndExpr(nNode, tkns, idx + 1);
                }
                return idx;
            }
            else
                return idx;
        }

        private static int ParseCondAndExpr(SyntaxNode parent, Token[] tkns, int idx){
            var nNode = new OperatorSyntaxNode(SyntaxNodeType.CondAndNode, tkns[idx]);
            parent.ChildNodes.Add(nNode);
            idx = ParseLogicOrExpr(nNode, tkns, idx);
            
            if(tkns[idx].TokenType == TokenType.Operator && tkns[idx].TokenValue == "&&"){
                while(tkns[idx].TokenType == TokenType.Operator && tkns[idx].TokenValue == "&&"){
                    nNode.Operator.Add(tkns[idx]);
                    idx = ParseLogicOrExpr(nNode, tkns, idx + 1);
                }
                return idx;
            }
            else
                return idx;
        }

        private static int ParseLogicOrExpr(SyntaxNode parent, Token[] tkns, int idx){
            var nNode = new OperatorSyntaxNode(SyntaxNodeType.LogicOrNode, tkns[idx]);
            parent.ChildNodes.Add(nNode);
            idx = ParseLogicXorExpr(nNode, tkns, idx);
            
            if(tkns[idx].TokenType == TokenType.Operator && tkns[idx].TokenValue == "|"){
                while(tkns[idx].TokenType == TokenType.Operator && tkns[idx].TokenValue == "|"){
                    nNode.Operator.Add(tkns[idx]);
                    idx = ParseLogicXorExpr(nNode, tkns, idx + 1);
                }
                return idx;
            }
            else
                return idx;
        }

        private static int ParseLogicXorExpr(SyntaxNode parent, Token[] tkns, int idx){
            var nNode = new OperatorSyntaxNode(SyntaxNodeType.LogicXorNode, tkns[idx]);
            parent.ChildNodes.Add(nNode);
            idx = ParseLogicAndExpr(nNode, tkns, idx);
            
            if(tkns[idx].TokenType == TokenType.Operator && tkns[idx].TokenValue == "^"){
                while(tkns[idx].TokenType == TokenType.Operator && tkns[idx].TokenValue == "^"){
                    nNode.Operator.Add(tkns[idx]);
                    idx = ParseLogicAndExpr(nNode, tkns, idx + 1);
                }
                return idx;
            }
            else
                return idx;
        }

        private static int ParseLogicAndExpr(SyntaxNode parent, Token[] tkns, int idx){
            var nNode = new OperatorSyntaxNode(SyntaxNodeType.LogicAndNode, tkns[idx]);
            parent.ChildNodes.Add(nNode);
            idx = ParseEqualityExpr(nNode, tkns, idx);
            
            if(tkns[idx].TokenType == TokenType.Operator && tkns[idx].TokenValue == "&"){
                while(tkns[idx].TokenType == TokenType.Operator && tkns[idx].TokenValue == "&"){
                    nNode.Operator.Add(tkns[idx]);
                    idx = ParseEqualityExpr(nNode, tkns, idx + 1);
                }
                return idx;
            }
            else
                return idx;
        }

        private static int ParseEqualityExpr(SyntaxNode parent, Token[] tkns, int idx){
            var nNode = new OperatorSyntaxNode(SyntaxNodeType.EqualityNode, tkns[idx]);
            parent.ChildNodes.Add(nNode);
            idx = ParseRelationTypeTestingExpr(nNode, tkns, idx);
            
            string[] ops = new string[] {"==", "!="};

            if(tkns[idx].TokenType == TokenType.ConditionalOperator && ops.Contains(tkns[idx].TokenValue)){
                nNode.Operator.Add(tkns[idx]);
                return ParseRelationTypeTestingExpr(nNode, tkns, idx + 1);
            }
            else
                return idx;
        }

        private static int ParseRelationTypeTestingExpr(SyntaxNode parent, Token[] tkns, int idx){
            var nNode = new OperatorSyntaxNode(SyntaxNodeType.RelationalTypeTestingNode, tkns[idx]);
            parent.ChildNodes.Add(nNode);
            idx = ParseShiftExpr(nNode, tkns, idx);
            
            string[] ops = new string[] {"<", ">"};
            string[] ops_1 = new string[] {"<=", ">="};
            string[] ops_2 = new string[] {"is", "as"};

            if(tkns[idx].TokenType == TokenType.Operator && ops.Contains(tkns[idx].TokenValue)){
                nNode.Operator.Add(tkns[idx]);
                return ParseShiftExpr(nNode, tkns, idx + 1);
            }
            else if(tkns[idx].TokenType == TokenType.ConditionalOperator && ops_1.Contains(tkns[idx].TokenValue)){
                nNode.Operator.Add(tkns[idx]);
                return ParseShiftExpr(nNode, tkns, idx + 1);
            }
            else if(tkns[idx].TokenType == TokenType.Keyword && ops_2.Contains(tkns[idx].TokenValue))
                throw new SyntaxException("is/as operators not implemented yet.", tkns[idx]);
            else
                return idx;
        }

        private static int ParseShiftExpr(SyntaxNode parent, Token[] tkns, int idx){
            var nNode = new OperatorSyntaxNode(SyntaxNodeType.ShiftNode, tkns[idx]);
            parent.ChildNodes.Add(nNode);
            idx = ParseAdditiveExpr(nNode, tkns, idx);
            
            string[] ops = new string[] {"<<", ">>"};

            if(tkns[idx].TokenType == TokenType.Operator && ops.Contains(tkns[idx].TokenValue)){
                while(tkns[idx].TokenType == TokenType.Operator && ops.Contains(tkns[idx].TokenValue)){
                    nNode.Operator.Add(tkns[idx]);
                    idx = ParseAdditiveExpr(nNode, tkns, idx + 1);
                }
                return idx;
            }
            else
                return idx;
        }

        private static int ParseAdditiveExpr(SyntaxNode parent, Token[] tkns, int idx){
            var nNode = new OperatorSyntaxNode(SyntaxNodeType.AdditiveNode, tkns[idx]);
            parent.ChildNodes.Add(nNode);
            idx = ParseMultiplicativeExpr(nNode, tkns, idx);
            
            string[] ops = new string[] {"+", "-"};

            if(tkns[idx].TokenType == TokenType.Operator && ops.Contains(tkns[idx].TokenValue)){
                while(tkns[idx].TokenType == TokenType.Operator && ops.Contains(tkns[idx].TokenValue)){
                    nNode.Operator.Add(tkns[idx]);
                    idx = ParseMultiplicativeExpr(nNode, tkns, idx + 1);
                }
                return idx;
            }
            else
                return idx;
        }

        private static int ParseMultiplicativeExpr(SyntaxNode parent, Token[] tkns, int idx){
            var nNode = new OperatorSyntaxNode(SyntaxNodeType.MultiplicativeNode, tkns[idx]);
            parent.ChildNodes.Add(nNode);
            idx = ParseUnaryExpr(nNode, tkns, idx);
            
            string[] ops = new string[] {"*", "/", "%"};

            if(tkns[idx].TokenType == TokenType.Operator && ops.Contains(tkns[idx].TokenValue)){
                while(tkns[idx].TokenType == TokenType.Operator && ops.Contains(tkns[idx].TokenValue)){
                    nNode.Operator.Add(tkns[idx]);
                    idx = ParseUnaryExpr(nNode, tkns, idx + 1);
                }
                return idx;
            }
            else
                return idx;
        }

        private static int ParseUnaryExpr(SyntaxNode parent, Token[] tkns, int idx){
            var nNode = new OperatorSyntaxNode(SyntaxNodeType.UnaryNode, tkns[idx]);
            parent.ChildNodes.Add(nNode);
            
            string[] ops = new string[] {"+", "-", "!", "~", "++", "--", "&", "*"};

            //TODO: Parse type casting

            if(tkns[idx].TokenType == TokenType.Operator && ops.Contains(tkns[idx].TokenValue)){
                nNode.Operator.Add(tkns[idx]);
                return ParseUnaryExpr(nNode, tkns, idx + 1);
            }
            else
                return ParsePrimaryExpr(nNode, tkns, idx);
        }

        private static int ParsePrimaryExpr(SyntaxNode parent, Token[] tkns, int idx){
            var nNode = new OperatorSyntaxNode(SyntaxNodeType.PrimaryNode, tkns[idx]);
            parent.ChildNodes.Add(nNode);
            
            //SUB_EXPRESSION (ASSIGNMENT ASSIGNMENT_EXPRESSION)*;
            //ASSIGNMENT_EXPRESSION
            //CONDITIONAL
            string[] primary_ops = new string[] {"true", "false", "null", "typeof", "sizeof", "nameof"};

            if(tkns[idx].TokenType == TokenType.StringLiteral | tkns[idx].TokenType == TokenType.CharLiteral | tkns[idx].TokenType == TokenType.IntegerLiteral | tkns[idx].TokenType == TokenType.HexLiteral | tkns[idx].TokenType == TokenType.BinaryLiteral){
                //STRING | INTEGER | CHAR | HEX | BINARY
                nNode.ChildNodes.Add(new SyntaxNode(SyntaxNodeType.ConstantNode, tkns[idx]));
                return idx + 1;
            }else if(tkns[idx].TokenType == TokenType.OpeningParen){
                //(EXPRESSION)
                var nNode2 = new SyntaxNode(SyntaxNodeType.SpecialStatement, tkns[idx]);
                nNode.ChildNodes.Add(nNode2);
                idx = ParseExpression(nNode2, tkns, idx + 1);
                if(tkns[idx].TokenType != TokenType.ClosingParen)
                    throw new SyntaxException("Expected closing parenthesis.", tkns[idx]);
                return idx + 1;    
            }else if(tkns[idx].TokenType == TokenType.Keyword){
                switch(tkns[idx].TokenValue){
                    //true | false | null
                    case "true":
                    case "false":
                    case "null":
                        nNode.ChildNodes.Add(new SyntaxNode(SyntaxNodeType.ConstantNode, tkns[idx]));
                        break;
                    case "checked":
                    case "unchecked":
                    case "typeof":
                    case "default":
                    case "sizeof":
                    case "nameof":
                        {
                            var nNode2 = new SyntaxNode(SyntaxNodeType.SpecialStatement, tkns[idx]);
                            nNode.ChildNodes.Add(nNode2);
                            
                            if(tkns[idx + 1].TokenType != TokenType.OpeningParen)
                                throw new SyntaxException("Expected opening parenthesis.", tkns[idx + 1]);

                            idx = ParseExpression(nNode2, tkns, idx + 2);
                            
                            if(tkns[idx].TokenType != TokenType.ClosingParen)
                                throw new SyntaxException("Expected closing parenthesis.", tkns[idx]);
                            return idx + 1;
                        }
                    case "new":
                    //TODO: Implement the following operators
                        break;
                    case "delegate":
                        break;
                    default:
                        throw new SyntaxException("Unexpected keyword.", tkns[idx]);
                }
                return idx + 1;
            }else if(tkns[idx].TokenType == TokenType.Identifier){
                nNode.ChildNodes.Add(new SyntaxNode(SyntaxNodeType.VariableNode, tkns[idx]));
                return idx + 1;
            }else{
                string[] ops = new string[] { "++", "--" };
                string[] ops_1 = new string[] { ".", "?.", "->" };
                
                idx = ParseAssignAndLambdaExpr(nNode, tkns, idx);

                nNode.Operator.Add(tkns[idx]);
                if(tkns[idx].TokenType == TokenType.Operator && ops.Contains(tkns[idx].TokenValue))
                    return idx + 1;
                else if(tkns[idx].TokenType == TokenType.Operator && ops_1.Contains(tkns[idx].TokenValue))
                    return ParseAssignAndLambdaExpr(parent, tkns, idx + 1);

                return ParseAssignAndLambdaExpr(nNode, tkns, idx);
            }
            //TODO: Handle array and function calls
        }

    }
}
