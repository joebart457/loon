using Crater.Shared.Models;
using Loon.Analyzer._Analyzer.Comparers;
using Loon.Analyzer.Extensions;
using Loon.Analyzer.Models;
using Loon.Parser.Models;
using Loon.Parser.Models.Declarations;
using Loon.Parser.Models.Expressions;
using Loon.Parser.Models.Statements;
using Loon.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer._Analyzer
{
    public class AnalysisResult
    {
        public List<CrateType> CrateTypes { get; set; }
        public List<CrateFunction> CrateFunctions { get; set; }

        public AnalysisResult(List<CrateType> crateTypes, List<CrateFunction> crateFunctions)
        {
            CrateTypes = crateTypes;
            CrateFunctions = crateFunctions;
        }

    }
    public class StaticAnalyzer
    {
        private class CurrentRunData
        {
            public CrateFunction? CurrentScopedFunction { get; set; } = null;
            private Dictionary<string, CrateType> _localFunctionScope = new();


            public void RegisterLocalVariable(string name, CrateType type)
            {
                if (_localFunctionScope.ContainsKey(name)) throw new Exception($"redeclaration of variable {name}");
                _localFunctionScope[name] = type;
            }

            public void EnterScope()
            {
                _localFunctionScope.Clear();
            }
            public void ExitScope()
            {
                _localFunctionScope.Clear();
            }

            public CrateType GetLocalVariableType(string name)
            {
                if (_localFunctionScope.TryGetValue(name, out var type)) return type;
                throw new Exception($"variable {name} is not defined in this scope");
            }
        }

        private List<CrateType> _registeredTypes = new();
        private List<CrateFunction> _registeredFunctions = new();

        private Dictionary<string, GenericTypeDeclaration> _genericTypeDefinitions = new();
        private List<GenericFunctionDeclaration> _genericFunctionDefinitions = new();

        private CurrentRunData _currentRunData = new();
        private List<FunctionDeclaration> _functionsToAnalyze = new();  
        public AnalysisResult Analyze(List<DeclarationBase> declarations)
        {
            _registeredTypes.Clear();
            _registeredFunctions.Clear();
            _currentRunData = new();
            _functionsToAnalyze.Clear();
            RegisterBuiltinTypes();
            RegisterBuiltinFunctions();
            FirstPass(declarations);
            SecondPass(declarations);
            ThirdPass();
            CheckEntry();
            return new AnalysisResult(_registeredTypes.ToList(), _registeredFunctions);
        }

        private void FirstPass(List<DeclarationBase> declarations)
        {
            foreach (var decl in declarations)
            {
                if (decl is GenericTypeDeclaration genericTypeDeclaration) AddGenericTypeDefinition(genericTypeDeclaration);
            }
            foreach (var decl in declarations)
            {
                if (decl is GenericFunctionDeclaration genericFunctionDeclaration) AddGenericFunctionDefinition(genericFunctionDeclaration);
            }
            foreach (var decl in declarations)
            {
                if (decl is TypeDeclaration typeDeclaration) GatherTypeSignature(typeDeclaration, new());
            }
            foreach(var decl in declarations)
            {
                if (decl is FunctionDeclaration functionDeclaration) GatherFunctionSignature(functionDeclaration);
            }
        }

        private void SecondPass(List<DeclarationBase> declarations)
        {
            foreach (var decl in declarations)
            {
                if (decl is TypeDeclaration typeDeclaration) AnalyzeTypeDeclaration(typeDeclaration);
            }
            foreach (var decl in declarations)
            {
                if (decl is FunctionDeclaration functionDeclaration) AnalyzeFunctionDeclaration(functionDeclaration);
            }
        }

        private void ThirdPass()
        {
            int count = 0;
            foreach(var fn in _functionsToAnalyze)
            {
                if (count == 1000) throw new Exception("encountered maximum recursion depth when resolving generic function arguments");
                AnalyzeFunctionDeclaration(fn);
            }
        }

        private void CheckEntry()
        {
            if (!_registeredFunctions.Any(fn => fn.IsEntry)) throw new Exception("no entry point is defined");
        }

        private void AddGenericTypeDefinition(GenericTypeDeclaration genericTypeDeclaration)
        {
            if (_genericTypeDefinitions.ContainsKey(genericTypeDeclaration.TypeName)) throw new Exception($"redefinition of generic type {genericTypeDeclaration.TypeName}");
            _genericTypeDefinitions[genericTypeDeclaration.TypeName] = genericTypeDeclaration;
        }

        private void AddGenericFunctionDefinition(GenericFunctionDeclaration genericFunctionDeclaration)
        {
            if (_genericFunctionDefinitions.Any(fn => fn.FunctionName == genericFunctionDeclaration.FunctionName)) throw new Exception($"redefinition of generic function {genericFunctionDeclaration.FunctionName}");
            _genericFunctionDefinitions.Add(genericFunctionDeclaration);
        }

        private CrateType GatherTypeSignature(TypeDeclaration typeDeclaration, List<TypeSymbol> genericTypeArguments)
        {
            var typeName = typeDeclaration.TypeName;
            var typeArguments = genericTypeArguments.Select(t => ResolveTypeSymbol(t)).ToList();
            if (_registeredTypes.Any(t => t.Name == typeName && t.TypeArguments.SequenceEqual(typeArguments))) throw new Exception($"redefinition of type {typeName}");
            var type = new CrateType(typeName, new());
            type.TypeArguments = typeArguments;
            _registeredTypes.Add(type);
            return type;
        }

        private CrateType AnalyzeTypeDeclaration(TypeDeclaration typeDeclaration)
        {
            var typeName = typeDeclaration.TypeName;
            var existingType = _registeredTypes.FirstOrDefault(t => t.Name == typeName && !t.HasGenericTypeArguments);
            if (existingType == null) throw new Exception($"unable to determine type of symbol {typeName}");
            var fields = typeDeclaration.Fields.Select(f => new CrateFieldInfo(f.FieldName, ResolveTypeSymbol(f))).ToList();
            existingType.Fields = fields;
            return existingType;
        }

        private CrateType AnalyzeGenericTypeDeclaration(TypeDeclaration typeDeclaration, List<CrateType> typeArguments)
        {
            var existingType = _registeredTypes.FirstOrDefault(t => t.Name == typeDeclaration.TypeName && t.TypeArguments.SequenceEqual(typeArguments));
            if (existingType == null) throw new Exception($"error analyzing generic type instantiation {typeDeclaration.TypeName}");
            var fields = typeDeclaration.Fields.Select(f => new CrateFieldInfo(f.FieldName, ResolveTypeSymbol(f))).ToList();
            existingType.Fields = fields;
            return existingType;
        }

        private CrateFunction AnalyzeFunctionDeclaration(FunctionDeclaration functionDeclaration)
        {
            var returnType = ResolveReturnTypeSymbol(functionDeclaration.ReturnType);
            var fnName = functionDeclaration.FunctionName;
            var parameters = functionDeclaration.Parameters.Select(p => new CrateParameterInfo(p.ParameterName, ResolveTypeSymbol(p.ParameterType))).ToList();
            var fn = new CrateFunction(functionDeclaration.IsFFI, functionDeclaration.IsEntry, functionDeclaration.CallingConvention, functionDeclaration.Module, fnName, returnType, parameters, new(), functionDeclaration.IsExport);           
            _currentRunData.CurrentScopedFunction = fn;
            _currentRunData.EnterScope();
            parameters.ForEach(p => _currentRunData.RegisterLocalVariable(p.Name, p.CrateType));
            var body = functionDeclaration.Body.Select(s => ResolveStatement(s)).ToList();
            _currentRunData.ExitScope();
            fn.Body = body;
            AppendFunctionBody(fn);
            return fn;
        }

        private CrateFunction GatherFunctionSignature(FunctionDeclaration functionDeclaration)
        {
            var returnType = ResolveReturnTypeSymbol(functionDeclaration.ReturnType);
            var fnName = functionDeclaration.FunctionName;
            var parameters = functionDeclaration.Parameters.Select(p => new CrateParameterInfo(p.ParameterName, ResolveTypeSymbol(p.ParameterType))).ToList();
            var fn = new CrateFunction(functionDeclaration.IsFFI, functionDeclaration.IsEntry, functionDeclaration.CallingConvention, functionDeclaration.Module, fnName, returnType, parameters, new(), functionDeclaration.IsExport);
            RegisterFunction(fn);
            return fn;
        }

        #region Statements

        private ResolvedStatement ResolveStatement(StatementBase statement)
        {
            if (statement is Parser.Models.Statements.IfStatement ifStatement) return ResolveIfStatement(ifStatement);
            if (statement is Parser.Models.Statements.ReturnStatement returnStatement) return ResolveReturnStatement(returnStatement);
            if (statement is Parser.Models.Statements.VariableDeclarationStatement variableDeclarationStatement) return ResolveVariableDeclarationStatement(variableDeclarationStatement);
            if (statement is Parser.Models.Statements.ExpressionStatement expressionStatement) return ResolveExpressionStatement(expressionStatement);
            if (statement is Parser.Models.Statements.BlockStatement blockStatement) return ResolveBlockStatement(blockStatement);
            if (statement is Parser.Models.Statements.WhileStatement whileStatement) return ResolveWhileStatement(whileStatement);
            if (statement is Parser.Models.Statements.ForStatement forStatement) return ResolveForStatement(forStatement);
            if (statement is Parser.Models.Statements.BreakStatement) return new Models.BreakStatement();
            if (statement is Parser.Models.Statements.ContinueStatement) return new Models.ContinueStatement();
            if (statement is Parser.Models.Statements.InlineAssemblyStatement inlineAssemblyStatement) return new Models.InlineAssemblyStatement(inlineAssemblyStatement.Asm);
            throw new Exception($"unsupported statement type {statement}");
        }

        private ResolvedStatement ResolveWhileStatement(Parser.Models.Statements.WhileStatement whileStatement)
        {
            var condition = ResolveExpression(whileStatement.Condition);
            if (!IsTruthyType(condition.Type)) throw new Exception($"type {condition.Type} is not able to be evaluated for truthiness");
            var then = ResolveStatement(whileStatement.Then);
            return new Models.WhileStatement(condition, then);
        }

        private ResolvedStatement ResolveForStatement(Parser.Models.Statements.ForStatement forStatement)
        {
            ResolvedStatement? intializer = forStatement.Initializer == null? null : ResolveStatement(forStatement.Initializer);
            TypedExpression? condition = forStatement.Condition == null? null : ResolveExpression(forStatement.Condition);
            TypedExpression? iterator = forStatement.Iterator == null ? null : ResolveExpression(forStatement.Iterator);
            if (condition != null && !IsTruthyType(condition.Type)) throw new Exception($"type {condition.Type} is not able to be evaluated for truthiness");
            var then = ResolveStatement(forStatement.Then);
            return new Models.ForStatement(intializer, condition, iterator, then);
        }

        private ResolvedStatement ResolveBlockStatement(Parser.Models.Statements.BlockStatement blockStatement)
        {
            return new Models.BlockStatement(blockStatement.ChildStatements.Select(s => ResolveStatement(s)).ToList());
        }

        private ResolvedStatement ResolveIfStatement(Parser.Models.Statements.IfStatement ifStatement)
        {
            var condition = ResolveExpression(ifStatement.Condition);
            if (!IsTruthyType(condition.Type)) throw new Exception($"type {condition.Type} is not able to be evaluated for truthiness");
            var then = ResolveStatement(ifStatement.Then);
            var elseThen = ifStatement.Else == null ? null : ResolveStatement(ifStatement.Else);
            return new Models.IfStatement(condition, then, elseThen);
        }

        private ResolvedStatement ResolveReturnStatement(Parser.Models.Statements.ReturnStatement returnStatement)
        {
            if (_currentRunData.CurrentScopedFunction == null) throw new Exception($"return statement must be located within function body");
            var returnValue = returnStatement.ReturnValue == null? null: ResolveExpression(returnStatement.ReturnValue);
            if (returnValue == null)
            {
                if (_currentRunData.CurrentScopedFunction.ReturnType != BuiltinTypes.Void) throw new Exception($"expect return value of type {_currentRunData.CurrentScopedFunction.ReturnType}");
            }
            else if (_currentRunData.CurrentScopedFunction.ReturnType != returnValue.Type) throw new Exception($"expect return type of {_currentRunData.CurrentScopedFunction.ReturnType} but got {returnValue.Type}");
            return new Models.ReturnStatement(returnValue);
        }

        private ResolvedStatement ResolveExpressionStatement(Parser.Models.Statements.ExpressionStatement expressionStatement)
        {
            return new Models.ExpressionStatement(ResolveExpression(expressionStatement.Expression));
        }

        private ResolvedStatement ResolveVariableDeclarationStatement(Parser.Models.Statements.VariableDeclarationStatement variableDeclarationStatement)
        {
            if (_currentRunData.CurrentScopedFunction == null) throw new Exception($"variables can only be declared within a function body");
            var initializerValue = ResolveExpression(variableDeclarationStatement.InitializerValue);
            CrateType variableType;
            if (variableDeclarationStatement.TypeSymbol != null)
            {
                variableType = ResolveTypeSymbol(variableDeclarationStatement.TypeSymbol);
            }else
            {
                variableType = initializerValue.Type;
            }
            if (variableType == BuiltinTypes.Void) throw new Exception($"unable to instantiate type {BuiltinTypes.Void} for variable {variableDeclarationStatement.VariableName}");
            if (variableType == BuiltinTypes.Nullptr) throw new Exception($"unable to determine type of variable {variableDeclarationStatement.VariableName}");
            if (!variableType.IsAssignableFrom(variableType)) throw new Exception($"unable to instantiate variable of type {variableType} with value of type {initializerValue.Type}");
            _currentRunData.RegisterLocalVariable(variableDeclarationStatement.VariableName, initializerValue.Type);
            return new Models.VariableDeclarationStatement(variableType, variableDeclarationStatement.VariableName, initializerValue);
        }

        #endregion

        #region Expressions

        private TypedExpression ResolveExpression(ExpressionBase expression)
        {
            if (expression is Parser.Models.Expressions.AssignmentExpression assignmentExpression) return ResolveAssignmentExpression(assignmentExpression);
            if (expression is Parser.Models.Expressions.SizeOfExpression sizeOfExpression) return ResolveSizeOfExpression(sizeOfExpression);
            if (expression is Parser.Models.Expressions.BinaryExpression binaryExpression) return ResolveBinaryExpression(binaryExpression);
            if (expression is Parser.Models.Expressions.UnaryExpression unaryExpression) return ResolveUnaryExpression(unaryExpression);
            if (expression is Parser.Models.Expressions.CallExpression callExpression) return ResolveCallExpression(callExpression);
            if (expression is Parser.Models.Expressions.GetExpression getExpression) return ResolveGetExpression(getExpression);
            if (expression is Parser.Models.Expressions.IdentifierExpression identifierExpression) return ResolveIdentifierExpression(identifierExpression);
            if (expression is Parser.Models.Expressions.LiteralExpression literalExpression) return ResolveLiteralExpression(literalExpression);
            if (expression is Parser.Models.Expressions.TypeInitializerExpression typeInitializerExpression) return ResolveTypeInitializerExpression(typeInitializerExpression);
            throw new Exception($"unsupported expression type {expression}");
        }

        private TypedExpression ResolveAssignmentExpression(Parser.Models.Expressions.AssignmentExpression assignmentExpression)
        {
            var valueToAssign = ResolveExpression(assignmentExpression.ValueToAssign);
            if (assignmentExpression.InstanceTarget != null)
            {
                var instanceTarget = ResolveExpression(assignmentExpression.InstanceTarget);
                var assignmentTarget = instanceTarget.Type.GetField(assignmentExpression.AssignmentTarget);
                if (assignmentTarget == null) throw new Exception($"field {assignmentExpression.AssignmentTarget} does not exist on type {instanceTarget.Type}");
                if (!assignmentTarget.CrateType.IsAssignableFrom(valueToAssign.Type)) throw new Exception($"type {assignmentTarget.CrateType} is not assignable from type {valueToAssign.Type}");
                return new Models.AssignmentExpression(assignmentTarget.CrateType, instanceTarget, assignmentTarget, valueToAssign);
            }

            var type = _currentRunData.GetLocalVariableType(assignmentExpression.AssignmentTarget);
            if (!type.IsAssignableFrom(valueToAssign.Type)) throw new Exception($"type {type} is not assignable from type {valueToAssign.Type}");
            return new Models.AssignmentExpression(type, null, new CrateFieldInfo(assignmentExpression.AssignmentTarget, type), valueToAssign);
        }


        private TypedExpression ResolveSizeOfExpression(Parser.Models.Expressions.SizeOfExpression sizeOfExpression)
        {
            var type = ResolveTypeSymbol(sizeOfExpression.TypeSymbol);
            return new Models.SizeOfExpression(type);
        }
        private TypedExpression ResolveBinaryExpression(Parser.Models.Expressions.BinaryExpression binaryExpression)
        {
            var lhs = ResolveExpression(binaryExpression.Lhs);
            var rhs = ResolveExpression(binaryExpression.Rhs);
            var resultingType = GetResultingType(binaryExpression.Operator, lhs.Type, rhs.Type);
            if (resultingType == null) throw new Exception($"invalid operation {binaryExpression.Operator} for types lhs {lhs.Type} rhs {rhs.Type}");
            return new Models.BinaryExpression(resultingType, binaryExpression.Operator, lhs, rhs);
        }

        private TypedExpression ResolveUnaryExpression(Parser.Models.Expressions.UnaryExpression unaryExpression)
        {
            var rhs = ResolveExpression(unaryExpression.Rhs);
            var resultingType = GetResultingType(unaryExpression.Operator, rhs.Type);
            if (resultingType == null) throw new Exception($"invalid operation {unaryExpression.Operator} for type rhs {rhs.Type}");
            return new Models.UnaryExpression(resultingType, unaryExpression.Operator, rhs);
        }

        private TypedExpression ResolveCallExpression(Parser.Models.Expressions.CallExpression callExpression)
        {
            var arguments = callExpression.Arguments.Select(a => ResolveExpression(a)).ToList();
            var calleeName = callExpression.CalleeName;
            var argumentTypes = arguments.Select(a => a.Type).ToList();
            var function = GetRegisteredFunction(calleeName, argumentTypes);
            if (function == null) 
                throw new Exception($"undefined symbol {calleeName}({string.Join(",", argumentTypes)})");
            return new Models.CallExpression(function, arguments);
        }

        private TypedExpression ResolveGetExpression(Parser.Models.Expressions.GetExpression getExpression)
        {
            var instanceTarget = ResolveExpression(getExpression.InstanceTarget);
            var field = instanceTarget.Type.GetField(getExpression.FieldName);
            if (field == null) throw new Exception($"field {getExpression.FieldName} does not exist on type {instanceTarget.Type}");
            return new Models.GetExpression(field.CrateType, instanceTarget, field);
        }

        private TypedExpression ResolveIdentifierExpression(Parser.Models.Expressions.IdentifierExpression identifierExpression)
        {
            var type = _currentRunData.GetLocalVariableType(identifierExpression.IdentifierSymbol.Lexeme);
            return new Models.IdentifierExpression(type, identifierExpression.IdentifierSymbol);
        }

        private TypedExpression ResolveLiteralExpression(Parser.Models.Expressions.LiteralExpression literalExpression)
        {
            if (literalExpression.Value.GetType() == typeof(byte)) return new Models.LiteralExpression(BuiltinTypes.Int8, literalExpression.Value);
            if (literalExpression.Value.GetType() == typeof(short)) return new Models.LiteralExpression(BuiltinTypes.Int16, literalExpression.Value);
            if (literalExpression.Value.GetType() == typeof(int)) return new Models.LiteralExpression(BuiltinTypes.Int32, literalExpression.Value);
            if (literalExpression.Value.GetType() == typeof(double)) return new Models.LiteralExpression(BuiltinTypes.Double, literalExpression.Value);
            if (literalExpression.Value.GetType() == typeof(string)) return new Models.LiteralExpression(BuiltinTypes.String, literalExpression.Value);
            if (literalExpression.Value.GetType() == typeof(Nullptr)) return new Models.LiteralExpression(BuiltinTypes.Nullptr, literalExpression.Value);
            throw new Exception($"unsupported literal value {literalExpression.Value}");
        }

        private TypedExpression ResolveTypeInitializerExpression(Parser.Models.Expressions.TypeInitializerExpression typeInitializerExpression)
        {
            var type = ResolveTypeSymbol(typeInitializerExpression.TypeSymbol);
           
            return new Models.TypeInitializerExpression(type);
        }


        #endregion

        private CrateType ResolveTypeSymbol(TypeSymbol typeSymbol)
        {
            if (typeSymbol.IsGenericTypeParameter) throw new Exception($"cannot resolve generic type parameter {typeSymbol}");
            if (typeSymbol.HasGenericTypeArguments)
            {
                var instantiatedGenericType = _registeredTypes.FirstOrDefault(ty => ty.Name == typeSymbol.Name && ty.TypeArguments.Select(t => t.ToTypeSymbol()).SequenceEqual(typeSymbol.GenericTypeArguments));

                if (instantiatedGenericType != null)
                {
                    return instantiatedGenericType;
                } else
                {
                    if (!_genericTypeDefinitions.TryGetValue(typeSymbol.Name, out var genericDefinition)) throw new Exception($"unable to resolve generic symbol '{typeSymbol}' to type");
                    if (genericDefinition.GenericTypeParameters.Count != typeSymbol.GenericTypeArguments.Count) throw new Exception($"error resolving symbol '{typeSymbol}' to type");
                    var typeReplacements = new Dictionary<TypeSymbol, TypeSymbol>();
                    for (int i = 0;i < genericDefinition.GenericTypeParameters.Count; i++)
                    {
                        typeReplacements[genericDefinition.GenericTypeParameters[i]] = typeSymbol.GenericTypeArguments[i];
                    }
                    var createdTypeDeclaration = genericDefinition.BuildNonGenericType(typeReplacements);
                    var createdType = GatherTypeSignature(createdTypeDeclaration, typeSymbol.GenericTypeArguments);
                    AnalyzeGenericTypeDeclaration(createdTypeDeclaration, createdType.TypeArguments);
                    return createdType;
                }                
            }
            if (typeSymbol.HasGenericTypeArguments) throw new Exception($"cannot resolve generic type {typeSymbol}");
            var type = _registeredTypes.FirstOrDefault(ty => !ty.HasGenericTypeArguments && ty.Name == typeSymbol.Name);
            if (type != null)
            {
                return type;
            }
            throw new Exception($"unable to resolve symbol '{typeSymbol.Name}' to type");
        }

        private CrateType ResolveTypeSymbol(TypeDeclarationField field)
        {
            var type = ResolveTypeSymbol(field.FieldType);
            if (field.Inline) return type.CreateInline();
            return type;
        }

        private CrateType ResolveReturnTypeSymbol(TypeSymbol typeSymbol)
        {
            if (typeSymbol.Name == BuiltinTypes.Void.Name && !typeSymbol.HasGenericTypeArguments) return BuiltinTypes.Void;
            return ResolveTypeSymbol(typeSymbol);
        }

        private void RegisterBuiltinTypes()
        {
            _registeredTypes.Add(BuiltinTypes.Int8);
            _registeredTypes.Add(BuiltinTypes.Int16);
            _registeredTypes.Add(BuiltinTypes.Int32);
            _registeredTypes.Add(BuiltinTypes.String);
            _registeredTypes.Add(BuiltinTypes.Double);
            //_registeredTypes.Add("void", BuiltinTypes.Void);
        }

        private void RegisterBuiltinFunctions()
        {
            _registeredFunctions.Add(new CrateFunction(true, false, CallingConvention.Invoke, "kernel32.dll", "GetProcessHeap", BuiltinTypes.Int32, new(), new(), false, true));
            _registeredFunctions.Add(new CrateFunction(true, false, CallingConvention.Invoke, "kernel32.dll", "ExitProcess", BuiltinTypes.Void, new(), new(), false, true));
            _registeredFunctions.Add(new CrateFunction(true, false, CallingConvention.Invoke, "kernel32.dll", "HeapAlloc", BuiltinTypes.Int32, new() { new("hHeap", BuiltinTypes.Int32), new("mode", BuiltinTypes.Int32), new("nBytes", BuiltinTypes.Int32) }, new(), false, true));
            _registeredFunctions.Add(new CrateFunction(true, false, CallingConvention.Invoke, "kernel32.dll", "HeapFree", BuiltinTypes.Int32, new() { new("dataPtr", BuiltinTypes.Int32) }, new(), false, true));

        }

        private void RegisterFunction(CrateFunction crateFunction)
        {
            if (crateFunction.IsEntry && _registeredFunctions.Any(fn => fn.IsEntry)) throw new Exception("application entry point defined multiple times");
            if (crateFunction.IsEntry && crateFunction.ReturnType != BuiltinTypes.Int32) throw new Exception($"entry point must have return type {BuiltinTypes.Int32}");
            var found = _registeredFunctions.FirstOrDefault(fn =>
                fn.Name == crateFunction.Name
                && fn.ReturnType == crateFunction.ReturnType
                && fn.Parameters.SequenceEqual(crateFunction.Parameters)
            );
            if (found == null) _registeredFunctions.Add(crateFunction);
            else throw new Exception($"redefinition of function {crateFunction}");
        }

        private void AppendFunctionBody(CrateFunction crateFunction)
        {
            var found = _registeredFunctions.FirstOrDefault(fn =>
                fn.Name == crateFunction.Name
                && fn.ReturnType == crateFunction.ReturnType
                && fn.Parameters.SequenceEqual(crateFunction.Parameters)
            );
            if (found == null) throw new Exception($"function signature {crateFunction} is not found");
            else found.Body = crateFunction.Body;
        }

        private CrateFunction? GetRegisteredFunction(string calleeName, List<CrateType> argumentTypes)
        {
            var function = _registeredFunctions.FirstOrDefault(fn => fn.Name == calleeName && fn.Parameters.Select(p => p.CrateType).ToList().SequenceEqual(argumentTypes, new AssignableTypeEqualityComparer()));
            if (function == null)
            {
                foreach(var genFnDef in _genericFunctionDefinitions)
                {
                    if (genFnDef.FunctionName != calleeName) continue;
                    if (argumentTypes.Count != genFnDef.Parameters.Count) continue;
                    var resolvedTypes = new Dictionary<TypeSymbol, CrateType>();
                    for(int i = 0; i < genFnDef.Parameters.Count; i++)
                    {
                        if (!Extract(argumentTypes[i], genFnDef.Parameters[i].ParameterType, ref resolvedTypes)) continue;
                    }
                    var resolvedTypeSymbols = new Dictionary<TypeSymbol, TypeSymbol>();
                    foreach(var kv in resolvedTypes)
                    {
                        resolvedTypeSymbols[kv.Key] = kv.Value.ToTypeSymbol();
                    }
                    var functionDeclaration = genFnDef.BuildNonGenericFunctionDeclaration(resolvedTypeSymbols);
                    var createdFn = GatherFunctionSignature(functionDeclaration);
                    _functionsToAnalyze.Add(functionDeclaration);
                    return createdFn;
                }
                return null;
            }
            return function;
        }

        private bool Extract(CrateType type, TypeSymbol typeSymbol, ref Dictionary<TypeSymbol, CrateType> resolvedTypes)
        {
            if (typeSymbol.IsGenericTypeParameter)
            {
                if (resolvedTypes.TryGetValue(typeSymbol, out var typeReplacement))
                {
                    if (type != typeReplacement) throw new Exception($"more than one type argument provided for {typeSymbol}. Provided arguments were {typeReplacement}, {type}");
                    return true;
                }
                resolvedTypes.Add(typeSymbol, type);
                return true;
            }
            if (type.Name == typeSymbol.Name)
            {
                if (!type.HasGenericTypeArguments && !typeSymbol.HasGenericTypeArguments) return true;
                else if (type.TypeArguments.Count != typeSymbol.GenericTypeArguments.Count) return false;
                for (int i = 0; i < typeSymbol.GenericTypeArguments.Count; i++)
                {
                    if (!Extract(type.TypeArguments[i], typeSymbol.GenericTypeArguments[i], ref resolvedTypes)) return false;
                }
                return true;
            }
          
            return false;
        }

        private bool IsTruthyType(CrateType type)
        {
            if (type == BuiltinTypes.Int32) return true;
            return false;
        }

        private CrateType? GetResultingType(BinaryOperator op, CrateType lhs, CrateType rhs)
        {
            if (op == BinaryOperator.Equal || op == BinaryOperator.NotEqual)
            {
                if (lhs == rhs || (lhs.IsNumeric && rhs.IsNumeric) || (lhs.IsReferenceType && rhs == BuiltinTypes.Nullptr)) return BuiltinTypes.Int32;
                return null;
            }
            if (op == BinaryOperator.LessThanEqual || op == BinaryOperator.LessThan || op == BinaryOperator.GreaterThanEqual || op == BinaryOperator.GreaterThan)
            {
                if (lhs.IsNumeric && rhs.IsNumeric) return BuiltinTypes.Int32;
                return null;
            }
            if (op == BinaryOperator.And || op == BinaryOperator.Or)
            {
                if (lhs == BuiltinTypes.Int32 && rhs == BuiltinTypes.Int32) return BuiltinTypes.Int32;
                return null;
            }
            if (op == BinaryOperator.Add || op == BinaryOperator.Subtract || op == BinaryOperator.Multiply || op == BinaryOperator.Divide)
            {
                if (lhs.IsNumeric && rhs.IsNumeric)
                {
                    if (rhs == BuiltinTypes.Double || lhs == BuiltinTypes.Double) return BuiltinTypes.Double;
                    return BuiltinTypes.Int32; // Arithmetic operations are promoted to Int32, then can be later cast down
                }
                if (op == BinaryOperator.Add && lhs == BuiltinTypes.String && rhs == BuiltinTypes.String) return BuiltinTypes.String;
                return null;
            }
            return null;
        }

        private CrateType? GetResultingType(UnaryOperator op, CrateType rhs)
        {
            if (op == UnaryOperator.Not)
            {
                if (rhs == BuiltinTypes.Int32) return BuiltinTypes.Int32;
                return null;
            }
            if (op == UnaryOperator.Negate)
            {
                if (rhs == BuiltinTypes.Int32) return BuiltinTypes.Int32;
                if(rhs == BuiltinTypes.Double) return BuiltinTypes.Double;
                return null;
            }
            if (op == UnaryOperator.StringSize)
            {
                if (rhs == BuiltinTypes.String) return BuiltinTypes.Int32;
                return null;
            }

            return null;
        }
    }



    public class BuiltinType : CrateType
    {
        private bool _isNumeric = false;
        public override bool IsBuiltin => true;
        public override bool IsNumeric => _isNumeric;
        public override bool IsReferenceType => Name == "string";
        public BuiltinType(string name, bool isNumeric = false) : base(name, new())
        {
            _isNumeric = isNumeric;
        }
    }

    public static class BuiltinTypes
    {
        public static CrateType Int8 = new BuiltinType("int8", true);
        public static CrateType Int16 = new BuiltinType("int16", true);
        public static CrateType Int32 = new BuiltinType("int32", true);
        public static CrateType String = new BuiltinType("string");
        public static CrateType Double = new BuiltinType("double", true);
        public static CrateType Nullptr = new BuiltinType("nullptr_t", false);
        public static CrateType Void = new BuiltinType("void_t", false);
    }
}
