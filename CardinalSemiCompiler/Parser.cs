using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CardinalSemiCompiler
{
    public class Parser
    {
        public static SyntaxTree ToSyntaxTree(string src, string[] definitions)
        {
            return CSharpSyntaxTree.ParseText(src, new CSharpParseOptions(LanguageVersion.CSharp7_3, DocumentationMode.None, SourceCodeKind.Regular, definitions));
        }
    }
}
