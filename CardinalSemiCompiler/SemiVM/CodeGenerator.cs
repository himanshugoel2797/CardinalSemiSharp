using CardinalSemiCompiler.SemiVM;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalSemiCompiler
{
    public class CodeGenerator : ICodeGenerator
    {
        int tabCnt = 0;

        public void Generate(string src, string[] defs)
        {
            Generate(Parser.ToSyntaxTree(src, defs));
        }

        public void Generate(SyntaxTree tree)
        {
            var diag = tree.GetDiagnostics();
            if (diag.Count() != 0)
            {
                foreach(Diagnostic d in diag)
                    Console.WriteLine(d);

                return;
            }

            var rootNode = tree.GetCompilationUnitRoot();
            ProcessToken(rootNode);
        }

        private void ProcessToken(SyntaxNode tkn)
        {
            for (int i = 0; i < tabCnt; i++)
                Console.Write("\t");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(tkn.Kind() + " : ");
            Console.ForegroundColor = ConsoleColor.Gray;

            //TODO: Emit SemiVM code from each directive
            //Build class structures and add method entries
            //SemiVM can then execute the code at runtime
            //SemiVM is written in C, with minimal external dependencies, allowing it to be placed ontop of the CardinalSemi kernel
            //SemiVM implements the C# reflection API
            switch (tkn.Kind())
            {
                case SyntaxKind.UsingDirective:
                    {
                        UsingDirectiveSyntax stx = tkn as UsingDirectiveSyntax;
                        Console.WriteLine(stx.Name);
                    }
                    break;
                case SyntaxKind.NamespaceDeclaration:
                    {
                        NamespaceDeclarationSyntax stx = tkn as NamespaceDeclarationSyntax;
                        Console.WriteLine(stx.Name);
                    }
                    break;
                case SyntaxKind.ClassDeclaration:
                    {
                        ClassDeclarationSyntax stx = tkn as ClassDeclarationSyntax;
                        Console.WriteLine(stx.Identifier);
                    }
                    break;
                case SyntaxKind.SimpleBaseType:
                    {
                        SimpleBaseTypeSyntax stx = tkn as SimpleBaseTypeSyntax;
                        Console.WriteLine(stx.Type);
                    }
                    break;
                case SyntaxKind.MethodDeclaration:
                    {
                        MethodDeclarationSyntax stx = tkn as MethodDeclarationSyntax;
                        Console.WriteLine(stx.Identifier);
                    }
                    break;
                case SyntaxKind.PredefinedType:
                    {
                        PredefinedTypeSyntax stx = tkn as PredefinedTypeSyntax;
                        Console.WriteLine(stx.Keyword);
                    }
                    break;
                default:
                    {
                        Console.WriteLine();
                    }
                    break;
            }

            {
                tabCnt++;
                var nodes = tkn.ChildNodes()?.ToArray();
                if (nodes != null)
                    foreach (SyntaxNode n in nodes)
                        ProcessToken(n);
                tabCnt--;
            }
        }
    }
}
