using System;
using System.Collections.Generic;

namespace Sapphire
{

    public enum PreprocessorTokenType
    {
        DefineDirective,
        IfDefDirective,
        ElseDirective,
        EndIfDirective,
        DirectiveArgument,
        Content
    }

    public struct PreprocessorToken
    {
        public PreprocessorTokenType type;
        public string value;
    }
/*
    public class Preprocessor
    {

        private string filePath;

        private List<PreprocessorToken> Tokenize(string inputStream)
        {
            var lines = inputStream.Split('\n');

            var tokenStream = new List<PreprocessorToken>();
            int lineNum = 0;
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.Length == 0)
                {
                    lineNum++;
                    continue;
                }

                if (trimmed[0] == '#')
                {
                    var splitLine = trimmed.Split(' ');
                    var directive = splitLine[0];

                    PreprocessorTokenType directiveToken;

                    if (directive == "#define") directiveToken = PreprocessorDirective.Define;
                    else if (directive == "#ifdef") directiveToken = PreprocessorDirective.IfDef;
                    else if (directive == "#else") directiveToken = PreprocessorDirective.Else;
                    else if (directive == "#endif") directiveToken = PreprocessorDirective.EndIf;
                    else throw new Exception(String.Format("Invalid preprocessor directive \"{0}\" at line {1} in {2}", directive, lineNum, filePath));

                    tokenStream.Add(new PreprocessorToken { type = directiveToken, value = "" });

                    for (int i = 1; i < splitLine.Length; i++)
                    {
                        tokenStream.Add(new PreprocessorToken { type = directiveToken, value = "" });
                    }
                }
                else
                {
                    tokenStream.Add(new PreprocessorToken { type = PreprocessorTokenType.Content, rawValue = line, args = null });
                }

                lineNum++;
            }
        }
    
    }
    */
}
