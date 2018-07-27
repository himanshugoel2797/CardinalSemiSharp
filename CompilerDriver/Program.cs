using CardinalSemiCompiler;
using CardinalSemiCompiler.AST;
using CardinalSemiCompiler.Tokenizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerDriver
{
    class Program
    {
        static int tabCnt = 0;
        static void PrintSyntaxNode(SyntaxNode node)
        {
            for (int i = 0; i < tabCnt; i++)
                Console.Write("\t");

            Console.WriteLine(node);

            tabCnt++;
            for (int i = 0; i < node.ChildNodes.Count; i++)
                PrintSyntaxNode(node.ChildNodes[i]);
            tabCnt--;
        }

        static void Compile(string programText)
        {
            Tokenizer tknizer = new Tokenizer();
            TokenStreamCleaner tknCleaner = new TokenStreamCleaner();

            tknCleaner.StripLineComments = true;
            tknCleaner.StripBlockComments = true;

            var tkns = tknizer.Tokenize(programText);
            for (int i = 0; i < TokenStreamCleaner.CleanupPassCount; i++)
                tkns = tknCleaner.Cleanup(tkns, i);

            for (int i = 0; i < tkns.Length; i++)
                Console.WriteLine(tkns[i]);

            //Build the syntax tree
            SyntaxNode rNode = SyntaxTree.Build(tkns);

            //Print the syntax tree
            Console.WriteLine("\n");
            PrintSyntaxNode(rNode);
        }

        static void Main(string[] args)
        {
            const string programText =
@"using System;
            int @a = 10;
using System.Collections;
using System.Linq;
using System.Text;
 
namespace HelloWorld
{
    class Program : IDisposable
    {
        static void Main(string[] args)
        {
            if(args.Length >= 1) Console.WriteLine($@""\tHello, World!"");
        }
    }
}";

            string codePath = @"C:\Users\Himanshu Goel\source\repos\CardinalSemiCompiler\CardinalSemiCompiler";
            var code = Directory.GetFiles(codePath, "*.cs", SearchOption.AllDirectories);

            for(int i = 0; i < code.Length; i++)
            {
                if (code[i].Contains("\\obj\\"))
                    continue;

                if (!code[i].Contains("Token.cs"))
                    continue;

                Console.WriteLine($"\nCompiling: {Path.GetFileName(code[i])}\n");
                Compile(File.ReadAllText(code[i]));
            }

            //CodeGenerator gen = new CodeGenerator();
            //gen.Generate(programText, null);

            Console.ReadLine();
        }
    }
}
