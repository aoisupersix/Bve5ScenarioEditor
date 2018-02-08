using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Bve5_Parsing;

namespace Bve5ScenarioEditor
{
    /// <summary>
    /// シナリオパースエラーを取得するクラス
    /// </summary>
    public class ScenarioErrorListener : ParseErrorListener
    {

        public List<ScenarioError> Error { get; private set; }

        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            Error.Add(
                new ScenarioError()
                {
                    Line = line,
                    Column = charPositionInLine,
                    Message = msg
                }
            );
            base.SyntaxError(recognizer, offendingSymbol, line, charPositionInLine, msg, e);
        }
    }

    /// <summary>
    /// シナリオエラーを保持する構造体
    /// </summary>
    public struct ScenarioError
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string Message { get; set; }
    }
}
