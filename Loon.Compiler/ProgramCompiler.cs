using Crater.Shared.Models;
using Loon.Analyzer._Analyzer;
using Loon.Analyzer.Models;
using Loon.Compiler._Generator;
using Loon.Compiler.Extensions;
using Loon.Compiler.Models;
using Loon.Compiler.Models._CompilationUnit;
using Loon.Compiler.Models._Function;
using Loon.Parser;
using Loon.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler
{
    

    public class ProgramCompiler
    {
        private CompilationState CompilationState = new();

        public string? Compile(CompilationSettings settings)
        {
            try
            {
                var parser = new ProgramParser();
                var analyzer = new StaticAnalyzer();
                var analysisResult = analyzer.Analyze(parser.ParseFile(settings.InputFilePath).ToList());
                return Compile(analysisResult, settings);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string? Compile(AnalysisResult analysisResult, CompilationSettings settings)
        {       
            try
            {
                CompilationState = new();
                CompilationState.Initialize(settings);
                AddDefaultIncludes();
                AddPlatformSpecificCode();
                foreach (var type in analysisResult.CrateTypes) CompileType(type);
                foreach (var fn in analysisResult.CrateFunctions) CompileFunction(fn);
                CompilationState.GenerateAssembly();
                return CompilationState.GenerateExecutable();
            }catch (Exception ex)
            {
                return ex.Message;
            }
        }
        private void AddDefaultIncludes()
        {
            CompilationState.Add(new AssemblyInclude($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\fasm\\INCLUDE\\macro\\proc32.inc"));
            CompilationState.Add(new AssemblyInclude($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\fasm\\INCLUDE\\win32ax.inc"));
            CompilationState.Add(new AssemblyInclude($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Assembly\\stdlib.inc"));
        }

        private void AddPlatformSpecificCode()
        {
            CompilationState.Add(new StaticVariable(BuiltinTypes.Int32, "_hHeap", 0));
        }

        private void CompileType(CrateType type)
        {
            if (!type.IsBuiltin) 
                CompilationState.Add(type);
        }

        private void CompileFunction(CrateFunction function)
        {
            if (function.IsFFI)
            {
                CompilationState.AddForeignFunctionReference(function);
                return;
            }
            var compiledFunction = new CompiledFunction(function);
            CompilationState.Add(compiledFunction);
            foreach (var statement in function.Body)
            {
                CompileStatement(statement);
            }
            if (function.IsEntry) CompilationState.AddEntryPoint(compiledFunction);
            
        }


        private void CompileStatement(ResolvedStatement statement)
        {
            CompilationState.Add(Templates.Comment(statement));
            if (statement is ExpressionStatement expressionStatement) CompileExpressionStatement(expressionStatement);
            else if (statement is IfStatement ifStatement) CompileIfStatement(ifStatement);
            else if (statement is ReturnStatement returnStatement) CompileReturnStatement(returnStatement);
            else if (statement is VariableDeclarationStatement variableDeclarationStatement) CompileVariableDeclarationStatement(variableDeclarationStatement);
            else if (statement is BlockStatement blockStatement) CompileBlockStatement(blockStatement);
            else if (statement is WhileStatement whileStatement) CompileWhileStatement(whileStatement);
            else if (statement is ForStatement forStatement) CompileForStatement(forStatement);
            else if (statement is InlineAssemblyStatement inlineAssemblyStatement) CompileInlineAssemblyStatement(inlineAssemblyStatement);
            else if (statement is BreakStatement breakStatement) CompileBreakStatement(breakStatement);
            else if (statement is ContinueStatement continueStatement) CompileContinueStatement(continueStatement);
            else throw new Exception($"unsupported statement type {statement.GetType().Name}");
        }

        private void CompileInlineAssemblyStatement(InlineAssemblyStatement inlineAssemblyStatement)
        {
            CompilationState.Add(Templates.InlineAssembly(inlineAssemblyStatement.Asm));
        }

        private void CompileBreakStatement(BreakStatement breakStatement)
        {
            if (CompilationState.LoopEndLabel == null) throw new Exception("unable to generate code for break statement not inside of loop");
            CompilationState.Add(Templates.Jmp(CompilationState.LoopEndLabel));
        }

        private void CompileContinueStatement(ContinueStatement continueStatement)
        {
            if (CompilationState.LoopStartLabel == null) throw new Exception("unable to generate code for continue statement not inside of loop");
            CompilationState.Add(Templates.Jmp(CompilationState.LoopStartLabel));
        }

        private void CompileExpressionStatement(ExpressionStatement expressionStatement)
        {
            CompilationState.Add(Generator.LocalVariable(expressionStatement.Expression.Type, true, out var discardAlias));
            CompileExpression(expressionStatement.Expression, discardAlias);
        }

        private void CompileIfStatement(IfStatement ifStatement)
        {
            CompilationState.Add(Generator.LocalVariable(BuiltinTypes.Int32, out var conditionAlias));
            CompileExpression(ifStatement.Condition, conditionAlias);
            CompilationState.Add(Templates.TestCondition(conditionAlias.Symbol, out var elseLabel));
            CompileStatement(ifStatement.Then);
            CompilationState.Add(Templates.EnterElseBlock(elseLabel, out var endifLabel));
            if (ifStatement.Else != null) CompileStatement(ifStatement.Else);
            CompilationState.Add(Templates.FinalizeIf(endifLabel));
        }

        private void CompileBlockStatement(BlockStatement blockStatement)
        {
            foreach (var statement in blockStatement.ChildStatements)
            {
                CompileStatement(statement);
            }
        }

        private void CompileWhileStatement(WhileStatement whileStatement)
        {
            CompilationState.Add(Generator.LocalVariable(BuiltinTypes.Int32, out var conditionLocal));
            CompilationState.Add(Templates.Label(Generator.Label("while_start", out var loopStart)));
            Generator.Label("while_end", out var loopEnd);
            CompileExpression(whileStatement.Condition, conditionLocal);
            CompilationState.Add(Templates.Jz(loopEnd));

            var prevStart = CompilationState.LoopStartLabel;
            var prevEnd = CompilationState.LoopEndLabel;
            CompilationState.LoopStartLabel = loopStart;
            CompilationState.LoopEndLabel = loopEnd;
            CompileStatement(whileStatement.Then);
            CompilationState.LoopStartLabel = prevStart;
            CompilationState.LoopEndLabel = prevEnd;
            CompilationState.Add(Templates.Jmp(loopStart));
            CompilationState.Add(Templates.Label(loopEnd));
        }

        private void CompileForStatement(ForStatement forStatement)
        {
            if (forStatement.Initializer != null) CompileStatement(forStatement.Initializer);
            CompilationState.Add(Templates.Label(Generator.Label("loop_start", out var loopStart)));
            Generator.Label("loop_end", out var loopEnd);
            if (forStatement.Condition != null)
            {
                CompilationState.Add(Generator.LocalVariable(BuiltinTypes.Int32, out var conditionLocal));
                CompileExpression(forStatement.Condition, conditionLocal);
                CompilationState.Add(Templates.Jz(loopEnd));

            }
            var prevStart = CompilationState.LoopStartLabel;
            var prevEnd = CompilationState.LoopEndLabel;
            CompilationState.LoopStartLabel = loopStart;
            CompilationState.LoopEndLabel = loopEnd;
            CompileStatement(forStatement.Then);
            CompilationState.LoopStartLabel = prevStart;
            CompilationState.LoopEndLabel = prevEnd;
            if (forStatement.Iterator != null)
            {
                CompilationState.Add(Generator.LocalVariable(forStatement.Iterator.Type, true, out var discardAlias));
                CompileExpression(forStatement.Iterator, discardAlias);
            }
            CompilationState.Add(Templates.Jmp(loopStart));
            CompilationState.Add(Templates.Label(loopEnd));
        }

        private void CompileVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement)
        {
            CompilationState.Add(Generator.LocalVariable(variableDeclarationStatement.InitializerValue.Type, variableDeclarationStatement.VariableName.Decorate(), out var destination));
            CompileExpression(variableDeclarationStatement.InitializerValue, destination);
        }

        private void CompileReturnStatement(ReturnStatement returnStatement)
        {
            if (returnStatement.ReturnValue == null)
            {
                CompilationState.Add(Templates.Return());
                return;
            }
            CompilationState.Add(Generator.LocalVariable(returnStatement.ReturnValue.Type, out var returnAlias));
            CompileExpression(returnStatement.ReturnValue, returnAlias);
            if (returnStatement.ReturnValue.Type == BuiltinTypes.Double)
            {
                CompilationState.Add(Templates.Returnq(returnAlias.Symbol));
            }
            else if (returnStatement.ReturnValue.Type == BuiltinTypes.String)
            {
                CompilationState.Add(Templates.Return(returnAlias.Symbol));
            }
            else if (returnStatement.ReturnValue.Type == BuiltinTypes.Int8)
            {
                CompilationState.Add(Templates.Returnb(returnAlias.Symbol));
            }
            else if (returnStatement.ReturnValue.Type == BuiltinTypes.Int16)
            {
                CompilationState.Add(Templates.Returnw(returnAlias.Symbol));
            }
            else if (returnStatement.ReturnValue.Type == BuiltinTypes.Int32)
            {
                CompilationState.Add(Templates.Return(returnAlias.Symbol));
            }
            else
            {
                // Since structs are all heap allocated, we can just return a pointer to it on eax and let gc cleanup
                CompilationState.Add(Templates.Return(returnAlias.Symbol));
            }
        }

        private void CompileExpression(TypedExpression typedExpression, LocalVariable destination)
        {
            CompilationState.Add(Templates.Comment(typedExpression));
            if (typedExpression is AssignmentExpression assignmentExpression) CompileAssignment(assignmentExpression, destination);
            if (typedExpression is GetExpression getExpression) CompileGet(getExpression, destination);
            if (typedExpression is SizeOfExpression sizeOfExpression) CompileSizeOf(sizeOfExpression, destination);
            if (typedExpression is CallExpression callExpression) CompileCall(callExpression, destination);
            if (typedExpression is IdentifierExpression identifierExpression) CompileIdentifier(identifierExpression, destination);
            if (typedExpression is LiteralExpression literalExpression) CompileLiteral(literalExpression, destination);
            if (typedExpression is BinaryExpression binaryExpression) CompileBinary(binaryExpression, destination);
            if (typedExpression is UnaryExpression unaryExpression) CompileUnary(unaryExpression, destination);
            if (typedExpression is TypeInitializerExpression typeInitializerExpression) CompileTypeInitializer(typeInitializerExpression, destination);
        }

        private void CompileSizeOf(SizeOfExpression sizeOfExpression, LocalVariable destination)
        {
            if (destination.IsDiscard) return;
            CompilationState.Add(Templates.Mov(destination.Symbol, sizeOfExpression.TypeArgument.GetAssemblySize()));
        }

        private void CompileAssignment(AssignmentExpression assignmentExpression, LocalVariable destination)
        {
            CompilationState.Add(Generator.LocalVariable(assignmentExpression.ValueToAssign.Type, out var valueToAssignAlias));
            CompileExpression(assignmentExpression.ValueToAssign, valueToAssignAlias);

            if (assignmentExpression.InstanceTarget != null)
            {
                CompilationState.Add(Generator.LocalVariable(assignmentExpression.InstanceTarget.Type, out var instanceAddr));
                CompileExpression(assignmentExpression.InstanceTarget, instanceAddr);
                if (assignmentExpression.Type == BuiltinTypes.Double)
                {
                    CompilationState.Add(Templates.AssignMemberq(instanceAddr.Symbol, assignmentExpression.InstanceTarget.Type, assignmentExpression.AssignmentTarget, valueToAssignAlias.Symbol));
                    if (!destination.IsDiscard) CompilationState.Add(Templates.Movq(destination.Symbol, valueToAssignAlias.Symbol));
                }
                else if (assignmentExpression.Type == BuiltinTypes.Int8)
                {
                    CompilationState.Add(Templates.AssignMemberb(instanceAddr.Symbol, assignmentExpression.InstanceTarget.Type, assignmentExpression.AssignmentTarget, valueToAssignAlias.Symbol));
                    if (!destination.IsDiscard) CompilationState.Add(Templates.Movb(destination.Symbol, valueToAssignAlias.Symbol));
                }
                else if (assignmentExpression.Type == BuiltinTypes.Int16)
                {
                    CompilationState.Add(Templates.AssignMemberw(instanceAddr.Symbol, assignmentExpression.InstanceTarget.Type, assignmentExpression.AssignmentTarget, valueToAssignAlias.Symbol));
                    if (!destination.IsDiscard) CompilationState.Add(Templates.Movw(destination.Symbol, valueToAssignAlias.Symbol));
                }
                else if (assignmentExpression.Type == BuiltinTypes.Int32)
                {
                    CompilationState.Add(Templates.AssignMember(instanceAddr.Symbol, assignmentExpression.InstanceTarget.Type, assignmentExpression.AssignmentTarget, valueToAssignAlias.Symbol));
                    if (!destination.IsDiscard) CompilationState.Add(Templates.Mov(destination.Symbol, valueToAssignAlias.Symbol));
                }
                else if (assignmentExpression.Type == BuiltinTypes.String)
                {
                    CompilationState.Add(Templates.AssignMember(instanceAddr.Symbol, assignmentExpression.InstanceTarget.Type, assignmentExpression.AssignmentTarget, valueToAssignAlias.Symbol));
                    if (!destination.IsDiscard) CompilationState.Add(Templates.Mov(destination.Symbol, valueToAssignAlias.Symbol));
                }
                else if (!assignmentExpression.Type.IsBuiltin && !assignmentExpression.Type.IsReferenceType)
                {
                    CompilationState.Add(Templates.AssignMemberStruct(instanceAddr.Symbol, assignmentExpression.InstanceTarget.Type, assignmentExpression.AssignmentTarget, valueToAssignAlias.Symbol));
                    if (!destination.IsDiscard) CompilationState.Add(Templates.AssignMemberStruct(instanceAddr.Symbol, assignmentExpression.InstanceTarget.Type, assignmentExpression.AssignmentTarget, valueToAssignAlias.Symbol));
                }
                else
                {
                    CompilationState.Add(Templates.AssignMember(instanceAddr.Symbol, assignmentExpression.InstanceTarget.Type, assignmentExpression.AssignmentTarget, valueToAssignAlias.Symbol));
                    if (!destination.IsDiscard) CompilationState.Add(Templates.Mov(destination.Symbol, valueToAssignAlias.Symbol));
                }
            }
            else
            {
                if (assignmentExpression.Type == BuiltinTypes.Double)
                {
                    CompilationState.Add(Templates.Movq(assignmentExpression.AssignmentTarget.Name.Decorate(), valueToAssignAlias.Symbol));
                    if (!destination.IsDiscard) CompilationState.Add(Templates.Movq(destination.Symbol, assignmentExpression.AssignmentTarget.Name.Decorate()));
                }
                else if (assignmentExpression.Type == BuiltinTypes.Int8)
                {
                    CompilationState.Add(Templates.Movb(assignmentExpression.AssignmentTarget.Name.Decorate(), valueToAssignAlias.Symbol));
                    if (!destination.IsDiscard) CompilationState.Add(Templates.Movb(destination.Symbol, valueToAssignAlias.Symbol));
                }
                else if (assignmentExpression.Type == BuiltinTypes.Int16)
                {
                    CompilationState.Add(Templates.Movw(assignmentExpression.AssignmentTarget.Name.Decorate(), valueToAssignAlias.Symbol));
                    if (!destination.IsDiscard) CompilationState.Add(Templates.Movw(destination.Symbol, valueToAssignAlias.Symbol));
                }
                else if (assignmentExpression.Type == BuiltinTypes.Int32)
                {
                    CompilationState.Add(Templates.Mov(Register.eax, valueToAssignAlias.Symbol));
                    CompilationState.Add(Templates.Mov(assignmentExpression.AssignmentTarget.Name.Decorate(), Register.eax));
                    if (!destination.IsDiscard) CompilationState.Add(Templates.Mov(destination.Symbol, Register.eax));
                }
                else if (assignmentExpression.Type == BuiltinTypes.String)
                {
                    CompilationState.Add(Templates.Mov(Register.eax, valueToAssignAlias.Symbol));
                    CompilationState.Add(Templates.Mov(assignmentExpression.AssignmentTarget.Name.Decorate(), Register.eax));
                    if (!destination.IsDiscard) CompilationState.Add(Templates.Mov(destination.Symbol, Register.eax));
                }
                else if (!assignmentExpression.Type.IsBuiltin && !assignmentExpression.Type.IsReferenceType)
                {
                    throw new Exception("unable to assign to local struct");
                }
                else
                {
                    CompilationState.Add(Templates.Mov(Register.eax, valueToAssignAlias.Symbol));
                    CompilationState.Add(Templates.Mov(assignmentExpression.AssignmentTarget.Name.Decorate(), Register.eax));
                    if (!destination.IsDiscard) CompilationState.Add(Templates.Mov(destination.Symbol, Register.eax));
                }
            }
        }

        private void CompileGet(GetExpression getExpression, LocalVariable destination)
        {
            if (destination.IsDiscard) return;
            CompilationState.Add(Generator.LocalVariable(getExpression.InstanceTarget.Type, out var instanceTargetAlias));
            CompileExpression(getExpression.InstanceTarget, instanceTargetAlias);
            CompilationState.Add(Templates.GetMember(instanceTargetAlias.Symbol, getExpression.InstanceTarget.Type, getExpression.Field, destination.Symbol));
        }

        private void CompileIdentifier(IdentifierExpression identifierExpression, LocalVariable destination)
        {
            if (destination.IsDiscard) return;

            if (identifierExpression.Type == BuiltinTypes.Double)
            {
                CompilationState.Add(Templates.Movq(destination.Symbol, identifierExpression.IdentifierSymbol.Lexeme.Decorate()));
            }
            else if (identifierExpression.Type == BuiltinTypes.String)
            {
                CompilationState.Add(Templates.Mov(Register.eax, identifierExpression.IdentifierSymbol.Lexeme.Decorate()));
                CompilationState.Add(Templates.Mov(destination.Symbol, Register.eax));
            }
            else if (identifierExpression.Type == BuiltinTypes.Int8)
            {
                CompilationState.Add(Templates.Movb(destination.Symbol, identifierExpression.IdentifierSymbol.Lexeme.Decorate()));
            }
            else if (identifierExpression.Type == BuiltinTypes.Int16)
            {
                CompilationState.Add(Templates.Movw(destination.Symbol, identifierExpression.IdentifierSymbol.Lexeme.Decorate()));
            }
            else
            {
                CompilationState.Add(Templates.Mov(Register.eax, identifierExpression.IdentifierSymbol.Lexeme.Decorate()));
                CompilationState.Add(Templates.Mov(destination.Symbol, Register.eax));
            }
        }

        private void CompileCall(CallExpression callExpression, LocalVariable destination)
        {
            var args = new List<(CrateType type, string addr)>();
            foreach (var arg in callExpression.Arguments)
            {
                CompilationState.Add(Generator.LocalVariable(arg.Type, out var argAddr));
                CompileExpression(arg, argAddr);
                args.Add((arg.Type, argAddr.Symbol));
            }
            CompilationState.Add(Templates.BeginCall(callExpression.CrateFunction, args));
            if (callExpression.CrateFunction.ReturnType == BuiltinTypes.Double)
            {
                CompilationState.Add(Templates.FinishCallq(callExpression.CrateFunction, destination.Symbol));
            }
            else if (callExpression.CrateFunction.ReturnType == BuiltinTypes.String)
            {
                CompilationState.Add(Templates.FinishCall(callExpression.CrateFunction, destination.Symbol));
            }
            else if (callExpression.CrateFunction.ReturnType == BuiltinTypes.Int8)
            {
                CompilationState.Add(Templates.FinishCallb(callExpression.CrateFunction, destination.Symbol));
            }
            else if (callExpression.CrateFunction.ReturnType == BuiltinTypes.Int16)
            {
                CompilationState.Add(Templates.FinishCallw(callExpression.CrateFunction, destination.Symbol));
            }
            else if (callExpression.CrateFunction.ReturnType == BuiltinTypes.Int32)
            {
                CompilationState.Add(Templates.FinishCall(callExpression.CrateFunction, destination.Symbol));
            }
            else if (callExpression.CrateFunction.ReturnType == BuiltinTypes.Void)
            {
                //pass
            }
            else 
            {
                CompilationState.Add(Templates.FinishCall(callExpression.CrateFunction, destination.Symbol));
            }
        }

        private void CompileLiteral(LiteralExpression literalExpression, LocalVariable destination)
        {
            if (destination.IsDiscard) return;
            if (literalExpression.Type == BuiltinTypes.Nullptr)
            {
                CompilationState.Add(Templates.Mov(destination.Symbol, 0));
                return;
            }

            CompilationState.AddStaticData(literalExpression.Type, literalExpression.Value, out var valueAlias);

            if (literalExpression.Type == BuiltinTypes.Int8)
            {
                CompilationState.Add(Templates.Movb(destination.Symbol, valueAlias.Symbol));
            }
            else if (literalExpression.Type == BuiltinTypes.Int16)
            {
                CompilationState.Add(Templates.Movw(destination.Symbol, valueAlias.Symbol));
            }
            else if (literalExpression.Type == BuiltinTypes.Double)
            {
                CompilationState.Add(Templates.Movq(destination.Symbol, valueAlias.Symbol));
            }
            else if (literalExpression.Type == BuiltinTypes.String)
            {
                CompilationState.Add(Templates.MovAddress(destination.Symbol, valueAlias.Symbol));
            }
            else
            {
                CompilationState.Add(Templates.Mov(Register.eax, valueAlias.Symbol));
                CompilationState.Add(Templates.Mov(destination.Symbol, Register.eax));
            }

        }

        private void CompileBinary(BinaryExpression binaryExpression, LocalVariable destination)
        {
            CompilationState.Add(Generator.LocalVariable(binaryExpression.Lhs.Type, out var lhsAlias));
            CompilationState.Add(Generator.LocalVariable(binaryExpression.Rhs.Type, out var rhsAlias));


            if (binaryExpression.Operator == BinaryOperator.And)
            {
                // Allows for short-circuiting of conditional ands
                var labelEnd = Generator.Label("AND_END");
                var labelSetFalse = Generator.Label("SET_FALSE");
                CompileExpression(binaryExpression.Lhs, lhsAlias);
                CompilationState.Add(Templates.Test(lhsAlias.Symbol));
                CompilationState.Add(Templates.Jz(labelSetFalse));
                CompileExpression(binaryExpression.Rhs, rhsAlias);
                CompilationState.Add(Templates.Test(rhsAlias.Symbol));
                CompilationState.Add(Templates.Jz(labelSetFalse));
                CompilationState.Add(Templates.Mov(Register.eax, 1));
                CompilationState.Add(Templates.Mov(destination.Symbol, Register.eax));
                CompilationState.Add(Templates.Jmp(labelEnd));
                CompilationState.Add(Templates.Label(labelSetFalse));
                CompilationState.Add(Templates.Mov(Register.eax, 0));
                CompilationState.Add(Templates.Mov(destination.Symbol, Register.eax));
                CompilationState.Add(Templates.Label(labelEnd));
            }
            else if (binaryExpression.Operator == BinaryOperator.Or)
            {
                // Allows for short-circuiting of conditional ors
                var labelEnd = Generator.Label("OR_END");
                var labelSetTrue = Generator.Label("SET_TRUE");
                CompileExpression(binaryExpression.Lhs, lhsAlias);
                CompilationState.Add(Templates.Test(lhsAlias.Symbol));
                CompilationState.Add(Templates.Jnz(labelSetTrue));
                CompileExpression(binaryExpression.Rhs, rhsAlias);
                CompilationState.Add(Templates.Test(rhsAlias.Symbol));
                CompilationState.Add(Templates.Jnz(labelSetTrue));
                CompilationState.Add(Templates.Mov(Register.eax, 0));
                CompilationState.Add(Templates.Mov(destination.Symbol, Register.eax));
                CompilationState.Add(Templates.Jmp(labelEnd));
                CompilationState.Add(Templates.Label(labelSetTrue));
                CompilationState.Add(Templates.Mov(Register.eax, 1));
                CompilationState.Add(Templates.Mov(destination.Symbol, Register.eax));
                CompilationState.Add(Templates.Label(labelEnd));
            }
            else if (binaryExpression.Operator.IsComparison())
            {
                CompileExpression(binaryExpression.Lhs, lhsAlias);
                CompileExpression(binaryExpression.Rhs, rhsAlias);

                var labelEnd = Generator.Label("CMP_END");
                var labelSetTrue = Generator.Label("CMP_SET_TRUE");

                if (binaryExpression.Lhs.Type == BuiltinTypes.Int8
                    && binaryExpression.Rhs.Type == BuiltinTypes.Double)
                {
                    CompilationState.Add(Templates.Cmp_Int8_Double(lhsAlias.Symbol, rhsAlias.Symbol, out var producedLocal));
                    CompilationState.Add(producedLocal);
                }
                else if (binaryExpression.Lhs.Type == BuiltinTypes.Int8
                    && binaryExpression.Rhs.Type == BuiltinTypes.Double)
                {
                    CompilationState.Add(Templates.Cmp_Double_Int8(lhsAlias.Symbol, rhsAlias.Symbol, out var producedLocal));
                    CompilationState.Add(producedLocal);
                }else if (binaryExpression.Lhs.Type.IsReferenceType
                    && binaryExpression.Rhs.Type == BuiltinTypes.Nullptr)
                {
                    CompilationState.Add(Templates.Cmp_Ref_Nullptr(lhsAlias.Symbol)); // This works because we only allow equality operators for nullptr 
                }else if (binaryExpression.Lhs.Type == BuiltinTypes.Nullptr
                    && binaryExpression.Rhs.Type.IsReferenceType)
                {
                    CompilationState.Add(Templates.Cmp_Ref_Nullptr(rhsAlias.Symbol));
                }
                else CompilationState.Add(CompileBinaryComparisonHelper(binaryExpression.Lhs.Type, binaryExpression.Rhs.Type, lhsAlias.Symbol, rhsAlias.Symbol));
                CompilationState.Add(Templates.Jump(binaryExpression.Operator, labelSetTrue));
                CompilationState.Add(Templates.Mov(Register.eax, 0));
                CompilationState.Add(Templates.Mov(destination.Symbol, Register.eax));
                CompilationState.Add(Templates.Jmp(labelEnd));
                CompilationState.Add(Templates.Label(labelSetTrue));
                CompilationState.Add(Templates.Mov(Register.eax, 1));
                CompilationState.Add(Templates.Mov(destination.Symbol, Register.eax));
                CompilationState.Add(Templates.Label(labelEnd));
            }
            else
            {
                CompileExpression(binaryExpression.Lhs, lhsAlias);
                CompileExpression(binaryExpression.Rhs, rhsAlias);
                var lhsType = binaryExpression.Lhs.Type;
                var rhsType = binaryExpression.Rhs.Type;
                if (lhsType == BuiltinTypes.Int8 || lhsType == BuiltinTypes.Int16)
                {
                    CompilationState.Add(PromoteType(binaryExpression.Lhs.Type, lhsAlias.Symbol, out lhsAlias, out lhsType));
                    CompilationState.Add(lhsAlias);
                }
                if (rhsType == BuiltinTypes.Int8 || rhsType == BuiltinTypes.Int16)
                {
                    CompilationState.Add(PromoteType(binaryExpression.Rhs.Type, rhsAlias.Symbol, out rhsAlias, out rhsType));
                    CompilationState.Add(rhsAlias);
                }
                CompilationState.Add(CompileBinaryHelper(binaryExpression.Operator, lhsType, rhsType, lhsAlias.Symbol, rhsAlias.Symbol, destination.Symbol));
            }

        }

        private void CompileUnary(UnaryExpression unaryExpression, LocalVariable destination)
        {
            CompilationState.Add(Generator.LocalVariable(unaryExpression.Rhs.Type, out var rhsAlias));
            CompileExpression(unaryExpression.Rhs, rhsAlias);
            if (unaryExpression.Operator == UnaryOperator.Not)
            {
                CompilationState.Add(Templates.Not(destination.Symbol, rhsAlias.Symbol));
            }
            else if (unaryExpression.Operator == UnaryOperator.Negate)
            {
                if (unaryExpression.Rhs.Type == BuiltinTypes.Int32)
                {
                    CompilationState.Add(Templates.Negate(destination.Symbol, rhsAlias.Symbol));
                }
                else if (unaryExpression.Rhs.Type == BuiltinTypes.Double)
                {
                    CompilationState.Add(Generator.LocalVariable(BuiltinTypes.Int32, out var zeroLocal));
                    CompilationState.Add(Templates.Mov(Register.eax, 0));
                    CompilationState.Add(Templates.Mov(zeroLocal.Symbol, Register.eax));
                    CompilationState.Add(Templates.Sub_Int32_Double(zeroLocal.Symbol, rhsAlias.Symbol, destination.Symbol));
                }
                else throw new Exception($"unable to generate code for negation of type {unaryExpression.Rhs.Type}");
            }
            else
            {
                CompilationState.Add(Templates.StringSize(rhsAlias.Symbol));
                CompilationState.Add(Templates.Mov(destination.Symbol, Register.eax));
            }
        }

        private void CompileTypeInitializer(TypeInitializerExpression typeInitializerExpression, LocalVariable destination)
        {
            CompilationState.Add(Templates.Allocate_Struct(typeInitializerExpression.Type, destination.Symbol));
        }


        #region Helpers

        private CompilationUnit CompileBinaryHelper(BinaryOperator op, CrateType lhsType, CrateType rhsType, string lhsAlias, string rhsAlias, string destination)
        {
            if (op.ToString().Length < 3) throw new Exception($"unsupported operator {op}");
            var functionSignature = $"{op.ToString().Substring(0, 3)}_{lhsType.Name.CapitalizeFirst()}_{rhsType.Name.CapitalizeFirst()}";
            var method = typeof(Templates).GetMethod(functionSignature);
            if (method == null) throw new Exception($"unable to generate code for operation {op} with types {lhsType}, {rhsType}");
            try
            {
                return (CompilationUnit?)method.Invoke(null, new object[] { lhsAlias, rhsAlias, destination }) ?? throw new Exception();
            }catch (Exception)
            {
                throw new Exception($"unable to generate code for operation {op} with types {lhsType}, {rhsType}: internal exception");
            }

        }

        private CompilationUnit CompileBinaryComparisonHelper(CrateType lhsType, CrateType rhsType, string lhsAlias, string rhsAlias)
        {
            var functionSignature = $"Cmp_{lhsType.Name.CapitalizeFirst()}_{rhsType.Name.CapitalizeFirst()}";
            var method = typeof(Templates).GetMethod(functionSignature);
            if (method == null) throw new Exception($"unable to generate code for comparison with types {lhsType}, {rhsType}");
            try
            {
                return (CompilationUnit?)method.Invoke(null, new object[] { lhsAlias, rhsAlias }) ?? throw new Exception();
            }
            catch (Exception)
            {
                throw new Exception($"unable to generate code for comparison with types {lhsType}, {rhsType}: internal exception");
            }
        }

        private CompilationUnit PromoteType(CrateType type, string alias, out LocalVariable producedLocal, out CrateType newType)
        {
            newType = BuiltinTypes.Int32;
            if (type == BuiltinTypes.Int8) return Templates.Convert_Int8_Int32(alias, Generator.LocalVariable(BuiltinTypes.Int32, out producedLocal).Symbol);
            if (type == BuiltinTypes.Int16) return Templates.Convert_Int16_Int32(alias, Generator.LocalVariable(BuiltinTypes.Int32, out producedLocal).Symbol);
            throw new Exception($"unable to promote type {type} to {BuiltinTypes.Int32}");
        }


        #endregion

    }
}
