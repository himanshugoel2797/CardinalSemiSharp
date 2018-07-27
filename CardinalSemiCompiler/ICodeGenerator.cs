using Microsoft.CodeAnalysis;

namespace CardinalSemiCompiler.SemiVM
{
    public interface ICodeGenerator
    {
        void Generate(SyntaxTree tree);
    }
}