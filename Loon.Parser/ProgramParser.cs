using Loon.Parser.Exceptions;
using Loon.Parser.Extensions;
using Loon.Parser.Models;
using Loon.Parser.Models.Declarations;
using Loon.Parser.Models.Expressions;
using Loon.Parser.Models.Statements;
using Loon.Shared.Enums;
using Loon.Tokenizer;
using Loon.Tokenizer.Constants;
using ParserLite;
using System.Globalization;
using TokenizerCore.Models.Constants;

namespace Loon.Parser
{
    public class ProgramParser: TokenParser
    {
        private readonly NumberFormatInfo DefaultNumberFormat = new NumberFormatInfo { NegativeSign = "-" };
        private bool InsideLoop = false;
        public IEnumerable<DeclarationBase> ParseFile(string path)
        {
            var tokenizer = Tokenizers.Default;
            var tokens = tokenizer.Tokenize(File.ReadAllText(path), false)
                .Where(token => token.Type != BuiltinTokenTypes.EndOfFile)
                .ToList();
            Initialize(tokens);

            while (!AtEnd())
            {
                yield return ParseDeclaration();
            }
            yield break;
        }

        private DeclarationBase ParseDeclaration()
        {
            if (AdvanceIfMatch(TokenTypes.Type)) return ParseTypeDeclaration();
            if (AdvanceIfMatch(TokenTypes.Function)) return ParseFunctionDeclaration(false);
            if (AdvanceIfMatch(TokenTypes.Export))
            {
                Consume(TokenTypes.Function, "expect function declaration after 'export'");
                return ParseFunctionDeclaration(true);
            }
            if (AdvanceIfMatch(TokenTypes.ForeignFunctionInterface)) return ParseForeignFunctionDeclaration();
            throw new ParsingException($"expect only top-level declarations", Current());
        }

        private DeclarationBase ParseTypeDeclaration()
        {
            var typeName = Consume(BuiltinTokenTypes.Word, "expect type name in type declaration");
            Consume(TokenTypes.LCurly, "expect field list in type declaration");
            var fields = new List<TypeDeclarationField>();
            if (!AdvanceIfMatch(TokenTypes.RCurly))
            {
                while (AdvanceIfMatch(TokenTypes.Dot))
                {
                    var fieldName = Consume(BuiltinTokenTypes.Word, "expect field name in type declaration");
                    var fieldType = ParseTypeSymbol();
                    var inline = AdvanceIfMatch(TokenTypes.Inline);
                    fields.Add(new(fieldName.Lexeme, fieldType, inline));
                }
                Consume(TokenTypes.RCurly, "expect enclosing } in type declaration");
            }
            return new TypeDeclaration(typeName.Lexeme, fields);
        }

        private DeclarationBase ParseFunctionDeclaration(bool isExport)
        {
            var isEntry = AdvanceIfMatch(TokenTypes.Entry);
            var functionName = Consume(BuiltinTokenTypes.Word, "expect function name in declaration");
            var parameters = new List<FunctionDeclarationParameter>();
            Consume(TokenTypes.LParen, "expect parameter list in function declaration");
            if (!AdvanceIfMatch(TokenTypes.RParen))
            {
                do
                {
                    var parameterType = ParseTypeSymbol();
                    var parameterName = Consume(BuiltinTokenTypes.Word, "expect parameter name in function declaration");

                    parameters.Add(new(parameterName.Lexeme, parameterType));
                } while (AdvanceIfMatch(TokenTypes.Comma));
                Consume(TokenTypes.RParen, "expect enclosing ) for parameter list");
            }
            Consume(TokenTypes.Colon, "expect return type specifier after parameter list");
            var returnType = ParseTypeSymbol();
            var body = ParseFunctionBody();
            return new FunctionDeclaration(false, isEntry, CallingConvention.StdCall, "", returnType, functionName.Lexeme, parameters, body, isExport);
        }

        private DeclarationBase ParseForeignFunctionDeclaration()
        {
            var callingConvention = CallingConvention.CInvoke;
            var module = Consume(TokenTypes.FunctionModule, "expect module location in foreign function declaration");
            if (!module.Lexeme.IsValidModulePath()) throw new ParsingException($"{module.Lexeme} is not a valid ffi path", Previous());
            if (Match(TokenTypes.CInvoke))
            {
                callingConvention = CallingConvention.CInvoke;
                Advance();
            }
            else if (Match(TokenTypes.Invoke))
            {
                callingConvention = CallingConvention.Invoke;
                Advance();
            }
            else if (Match(TokenTypes.StdCall))
            {
                callingConvention = CallingConvention.StdCall;
                Advance();
            }
            else callingConvention = CallingConvention.CInvoke;

            var functionName = Consume(BuiltinTokenTypes.Word, "expect function name in declaration");
            var parameters = new List<FunctionDeclarationParameter>();
            Consume(TokenTypes.LParen, "expect parameter list in function declaration");
            if (!AdvanceIfMatch(TokenTypes.RParen))
            {
                do
                {
                    var parameterType = ParseTypeSymbol();
                    var parameterName = Consume(BuiltinTokenTypes.Word, "expect parameter name in function declaration");

                    parameters.Add(new(parameterName.Lexeme, parameterType));
                } while (AdvanceIfMatch(TokenTypes.Comma));
                Consume(TokenTypes.RParen, "expect enclosing ) for parameter list");
            }
            Consume(TokenTypes.Colon, "expect return type specifier after parameter list");
            var returnType = ParseTypeSymbol();
            
            return new FunctionDeclaration(true, false, callingConvention, module.Lexeme, returnType, functionName.Lexeme, parameters, new(), false);
        }

        private StatementBase ParseStatement()
        {
            if (AdvanceIfMatch(TokenTypes.If)) return ParseIfStatement();
            if (AdvanceIfMatch(TokenTypes.Return)) return ParseReturnStatement();
            if (AdvanceIfMatch(TokenTypes.Var)) return ParseVariableDeclarationStatement();
            if (AdvanceIfMatch(TokenTypes.For)) return ParseForStatement();
            if (AdvanceIfMatch(TokenTypes.While)) return ParseWhileStatement();
            if (AdvanceIfMatch(TokenTypes.LCurly)) return ParseBlockStatement();
            if (InsideLoop && AdvanceIfMatch(TokenTypes.Break)) return new BreakStatement();
            if (InsideLoop && AdvanceIfMatch(TokenTypes.Continue)) return new ContinueStatement();
            if (AdvanceIfMatch(TokenTypes.InlineAssembly)) return new InlineAssemblyStatement(Previous().Lexeme);
            return ParseExpressionStatement();
        }

        private StatementBase ParseWhileStatement()
        {
            Consume(TokenTypes.LParen, "expect (condition) in while loop");
            var condition = ParseExpression();
            Consume(TokenTypes.RParen, "expect enclosing ) in while statement loop");
            var previous = InsideLoop;
            InsideLoop = true;
            var then = ParseStatement();
            InsideLoop = previous;
            return new WhileStatement(condition, then);
        }

        private StatementBase ParseForStatement()
        {
            Consume(TokenTypes.LParen, "expect (<initializer>;<condition>;<iter>) in for loop");
            StatementBase? initializer = null;
            ExpressionBase? condition = null;
            ExpressionBase? iterator = null;
            if (!AdvanceIfMatch(TokenTypes.SemiColon)) initializer = ParseStatement();
            if (!AdvanceIfMatch(TokenTypes.SemiColon))
            {
                condition = ParseExpression();
                Consume(TokenTypes.SemiColon, "expect ; after for loop condition");
            }
            if (!AdvanceIfMatch(TokenTypes.SemiColon)) iterator = ParseExpression();
            Consume(TokenTypes.RParen, "expect enclosing ) in for loop");
            var then = ParseStatement();
            return new ForStatement(initializer, condition, iterator, then);
        }

        private StatementBase ParseBlockStatement()
        {
            var statements = new List<StatementBase>();
            if (!AdvanceIfMatch(TokenTypes.RCurly))
            {
                do
                {
                    statements.Add(ParseStatement());
                } while (!AdvanceIfMatch(TokenTypes.RCurly));
            }
            return new BlockStatement(statements);
        }

        private StatementBase ParseIfStatement()
        {
            Consume(TokenTypes.LParen, "expect (condition) in if statement");
            var condition = ParseExpression();
            Consume(TokenTypes.RParen, "expect enclosing ) in if statement condition");
            var then = ParseStatement();
            StatementBase ? elseThen = null;
            if (AdvanceIfMatch(TokenTypes.Else)) elseThen = ParseStatement();
            return new IfStatement(condition, then, elseThen);
        }

        private StatementBase ParseVariableDeclarationStatement()
        {
            var variableName = Consume(BuiltinTokenTypes.Word, "expect variable name in declaration");
            Consume(TokenTypes.Equal, "expect initializer in declaration");
            var initializerValue = ParseExpression();
            Consume(TokenTypes.SemiColon, "expect ; after statement");
            return new VariableDeclarationStatement(variableName.Lexeme, initializerValue);
        }

        private StatementBase ParseReturnStatement()
        {
            ExpressionBase? returnValue = null;
            if (!Match(TokenTypes.SemiColon))
                returnValue = ParseExpression();
            Consume(TokenTypes.SemiColon, "expect ; after statement");
            return new ReturnStatement(returnValue);
        }

        private StatementBase ParseExpressionStatement()
        {
            var expression = ParseExpression();
            Consume(TokenTypes.SemiColon, "expect ; after statement");
            return new ExpressionStatement(expression);
        }

        private ExpressionBase ParseExpression()
        {
            return ParseAssignment();
        }

        private ExpressionBase ParseAssignment()
        {
            var expr = ParseBinaryExpression();
            if (AdvanceIfMatch(TokenTypes.Equal))
            {
                var value = ParseExpression();
                if (expr is GetExpression getExpr)
                {
                    return new AssignmentExpression(getExpr.InstanceTarget, getExpr.FieldName, value);
                }
                else if (expr is IdentifierExpression identifierExpr)
                {
                    return new AssignmentExpression(null, identifierExpr.IdentifierSymbol.Lexeme, value);

                }
                throw new ParsingException("invalid assignment target, must be modifiable lvalue", Current());                
            }
            return expr;
        }

        private ExpressionBase ParseBinaryExpression()
        {
            return ParseBinaryHelper(new List<(string, BinaryOperator)>()
            {
                (TokenTypes.DoubleAmpersand, BinaryOperator.And),
                (TokenTypes.DoublePipe, BinaryOperator.Or),
                (TokenTypes.DoubleEqual, BinaryOperator.Equal),
                (TokenTypes.NotEqual, BinaryOperator.NotEqual),
                (TokenTypes.LessThan, BinaryOperator.LessThan),
                (TokenTypes.LessThanEqual, BinaryOperator.LessThanEqual),
                (TokenTypes.GreaterThan, BinaryOperator.GreaterThan),
                (TokenTypes.GreaterThanEqual, BinaryOperator.GreaterThanEqual),
                (TokenTypes.Asterisk, BinaryOperator.Multiply),
                (TokenTypes.ForwardSlash, BinaryOperator.Divide),
                (TokenTypes.Plus, BinaryOperator.Add),
                (TokenTypes.Minus, BinaryOperator.Subtract),
            }, 0);
        }

        private ExpressionBase ParseBinaryHelper(List<(string, BinaryOperator)> operators, int index)
        {
            if (index >= operators.Count) return ParseUnaryExpression();
            var expr = ParseBinaryHelper(operators, index + 1);
            while (AdvanceIfMatch(operators[index].Item1))
            {
                expr = new BinaryExpression(operators[index].Item2, expr, ParseExpression());
            }
            return expr;
        }

        private ExpressionBase ParseUnaryExpression()
        {
            ExpressionBase expr;
            if (AdvanceIfMatch(TokenTypes.Not))
            {
                expr = new UnaryExpression(UnaryOperator.Not, ParseGetOrCall());
            }
            else if (AdvanceIfMatch(TokenTypes.Minus))
            {
                expr = new UnaryExpression(UnaryOperator.Negate, ParseGetOrCall());
            }
            else if (AdvanceIfMatch(TokenTypes.SizeOperator))
            {
                expr = new UnaryExpression(UnaryOperator.StringSize, ParseGetOrCall());
            }
            else
            {
                expr = ParseGetOrCall();
            }
            return expr;
        }

        private ExpressionBase ParseGetOrCall()
        {

            var expr = ParsePrimary();
            while (true)
            {
                if (AdvanceIfMatch(TokenTypes.Dot))
                {
                    expr = new GetExpression(expr, Consume(BuiltinTokenTypes.Word, "expect property name").Lexeme);
                }
                else if (AdvanceIfMatch(TokenTypes.LParen))
                {
                    if (expr is IdentifierExpression identifierExpression)
                    {
                        var arguments = new List<ExpressionBase>();
                        if (!AdvanceIfMatch(TokenTypes.RParen))
                        {
                            do
                            {
                                arguments.Add(ParseExpression());
                            } while (AdvanceIfMatch(TokenTypes.Comma));
                            Consume(TokenTypes.RParen, "expect enclosing ) in call");
                        }


                        expr = new CallExpression(identifierExpression.IdentifierSymbol.Lexeme, arguments);
                    }
                    else throw new ParsingException("expect valid callee lvalue", Current());
             
                }
                else
                {
                    break;
                }
            }
            return expr;
        }

        private ExpressionBase ParsePrimary()
        {
            if (AdvanceIfMatch(TokenTypes.Create))
            {
                var typeSymbol = ParseTypeSymbol();
                Consume(TokenTypes.LParen, "expect call to start with (");
                Consume(TokenTypes.RParen, "expect enclosing )");
                return new TypeInitializerExpression(typeSymbol);
            }
            return ParseLiteralExpression();
        }

        private ExpressionBase ParseLiteralExpression()
        {
            if (AdvanceIfMatch(BuiltinTokenTypes.Word))
            {
                return new IdentifierExpression(Previous());
            }
            if (AdvanceIfMatch(BuiltinTokenTypes.Integer))
            {
                var previousLexeme = Previous().Lexeme;
                if (AdvanceIfMatch(TokenTypes.I8)) return new LiteralExpression(byte.Parse(previousLexeme, DefaultNumberFormat));
                if (AdvanceIfMatch(TokenTypes.I16)) return new LiteralExpression(short.Parse(previousLexeme, DefaultNumberFormat));
                if (AdvanceIfMatch(TokenTypes.I32)) return new LiteralExpression(int.Parse(previousLexeme, DefaultNumberFormat));
                return new LiteralExpression(int.Parse(previousLexeme, DefaultNumberFormat));
            }
            if (AdvanceIfMatch(BuiltinTokenTypes.Double))
            {
                return new LiteralExpression(double.Parse(Previous().Lexeme, DefaultNumberFormat));
            }
            if (AdvanceIfMatch(BuiltinTokenTypes.Float))
            {
                return new LiteralExpression(float.Parse(Previous().Lexeme, DefaultNumberFormat));
            }
            if (AdvanceIfMatch(BuiltinTokenTypes.UnsignedInteger))
            {
                return new LiteralExpression(uint.Parse(Previous().Lexeme, DefaultNumberFormat));
            }
            if (AdvanceIfMatch(TokenTypes.False))
            {
                return new LiteralExpression(0);
            }
            if (AdvanceIfMatch(TokenTypes.True))
            {
                return new LiteralExpression(1);
            }
            if (AdvanceIfMatch(TokenTypes.Nullptr))
            {
                return new LiteralExpression(new Nullptr());
            }
            if (AdvanceIfMatch(BuiltinTokenTypes.String))
            {
                return new LiteralExpression(Previous().Lexeme);
            }
            if (AdvanceIfMatch(TokenTypes.SizeOf))
            {
                Consume(TokenTypes.LParen, "expect sizeof(<type_symbol>)");
                var symbol = ParseTypeSymbol();
                Consume(TokenTypes.RParen, "expect sizeof(<type_symbol>)");
                return new SizeOfExpression(symbol);
            }

            if (AdvanceIfMatch(TokenTypes.LParen))
            {
                var expr = ParseExpression();
                Consume(TokenTypes.RParen, "expect enclosing ) in group");
                return expr;
            }

            throw new ParsingException($"encountered unexpected token {Current()}", Current());
        }

        #region Helpers

        private List<StatementBase> ParseFunctionBody()
        {
            var statements = new List<StatementBase>();
            Consume(TokenTypes.LCurly, "expect { at start of function body");
            if (!AdvanceIfMatch(TokenTypes.RCurly))
            {
                do
                {
                    statements.Add(ParseStatement());
                } while (!AdvanceIfMatch(TokenTypes.RCurly));
            }
            return statements;
        }


        private TypeSymbol ParseTypeSymbol()
        {
            var typeName = Consume(BuiltinTokenTypes.Word, "expect type name");
            return new TypeSymbol(typeName.Lexeme);
        }

        #endregion


    }
}
