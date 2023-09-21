using TokenizerCore.Interfaces;


namespace Loon.Parser.Exceptions
{
    public class ParsingException: System.Exception
    {
        public IToken Token { get; private set; }

        public ParsingException(string msg, IToken token)
            :base($"{msg} at {token}") 
        {
            Token = token;
        }
    }
}
