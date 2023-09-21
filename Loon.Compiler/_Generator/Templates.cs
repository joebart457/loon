using Crater.Shared.Models;
using Loon.Analyzer._Analyzer;
using Loon.Analyzer.Models;
using Loon.Compiler.Extensions;
using Loon.Compiler.Models._CompilationUnit;
using Loon.Compiler.Models._Function;
using Loon.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler._Generator
{
    internal static class Templates
    {
        public static CompilationUnit Comment(ResolvedStatement origin)
        {
            return new CompilationUnit(origin, new());
        }

        public static CompilationUnit Comment(TypedExpression origin)
        {
            return new CompilationUnit(origin, new());
        }

        public static CompilationUnit InlineAssembly(string asm)
        {
            var cu = new CompilationUnit();
            asm.Split("\r\n").ToList().ForEach(line =>
            {
                cu.Append(line.TrimStart());
            });
            return cu;
        }

        public static CompilationUnit TestCondition(string conditionAddr, out string elseLabel)
        {
            Generator.Label("LOCAL_ELSE", out elseLabel);
            return new CompilationUnit()
                .Append(Ins.MOV(Register.eax, Size.DWORD(conditionAddr)))
                .Append(Ins.TEST(Register.eax, Register.eax))
                .Append(Ins.JZ(elseLabel));
        }

        public static CompilationUnit EnterElseBlock(string elseLabel, out string endIfLabel)
        {
            Generator.Label("END_IF", out endIfLabel);

            return new CompilationUnit()
                .Append(Ins.JMP(endIfLabel))
                .Append(Ins.Label(elseLabel));
        }

        public static CompilationUnit FinalizeIf(string endIfLabel)
        {
            return new CompilationUnit().Append(Ins.Label(endIfLabel));
        }


        public static CompilationUnit Return(string returnAlias)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.edx, Size.DWORD(0)))
                .Append(Ins.MOV(Register.eax, returnAlias))
                .Append(Ins.RET());
        }

        public static CompilationUnit Returnq(string returnAlias)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.edx, Size.DWORD(0)))
                .Append(Ins.MOV(Register.eax, Size.DWORD(returnAlias)))
                .Append(Ins.MOV(Register.ebx, Offset.CreateForDWORD(returnAlias, 4)))
                .Append(Ins.RET());
        }

        public static CompilationUnit Returnb(string returnAlias)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.edx, Size.DWORD(0)))
                .Append(Ins.MOVSX(Register.eax, Size.BYTE(returnAlias)))
                .Append(Ins.RET());
        }

        public static CompilationUnit Returnw(string returnAlias)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.edx, Size.DWORD(0)))
                .Append(Ins.MOVSX(Register.eax, Size.WORD(returnAlias)))
                .Append(Ins.RET());
        }

        public static CompilationUnit Allocate_Struct(CrateType type, string destination)
        {
            var size = type.GetAssemblySize();
            return new CompilationUnit()
                .Append(Ins.STDCALL("AllocateMemory", $"dword {size}"))
                .Append(Ins.MOV(destination, Register.eax));
        }

        public static CompilationUnit Mov(Register dest, Register source)
        {
            return new CompilationUnit().Append(Ins.MOV(dest, source));
        }

        public static CompilationUnit Mov(Register dest, int immediate)
        {
            return new CompilationUnit().Append(Ins.MOV(dest, immediate));
        }

        public static CompilationUnit Mov(Register dest, string sourceAddr)
        {
            return new CompilationUnit().Append(Ins.MOV(dest, sourceAddr));
        }

        public static CompilationUnit Mov(string destAddr, string sourceAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.eax, sourceAddr))
                .Append(Ins.MOV(destAddr, Register.eax));
        }

        public static CompilationUnit MovAddress(string destAddr, string sourceAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV_ADDRESS(Register.eax, sourceAddr))
                .Append(Ins.MOV(destAddr, Register.eax));
        }

        public static CompilationUnit Mov(string destAddr, Register source)
        {
            return new CompilationUnit().Append(Ins.MOV(destAddr, source));
        }

        public static CompilationUnit Mov(string destAddr, int immediate)
        {
            return new CompilationUnit().Append(Ins.MOV(destAddr, immediate));
        }

        public static CompilationUnit Movq(string destAddr, string sourceAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.eax, Offset.CreateForDWORD(sourceAddr, 4)))
                .Append(Ins.MOV(Offset.CreateForDWORD(destAddr, 4), Register.eax))
                .Append(Ins.MOV(Register.eax, Size.DWORD(sourceAddr)))
                .Append(Ins.MOV(Size.DWORD(destAddr), Register.eax));

        }

        public static CompilationUnit Movb(string destAddr, string sourceAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOVSX(Register.eax, Size.BYTE(sourceAddr)))
                .Append(Ins.MOV(destAddr, Register.al));
        }

        public static CompilationUnit Movw(string destAddr, string sourceAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOVSX(Register.eax, Size.WORD(sourceAddr)))
                .Append(Ins.MOV(destAddr, Register.ax));
        }


        public static CompilationUnit AssignMember(string instanceAddr, CrateType instanceType, CrateFieldInfo field, string valueToAssignAddr)
        {
            Generator.UniqueTypeIdentifier(instanceType, out var instanceAlias);
            return new CompilationUnit()
                .Append(Ins.MOV(Register.ebx, Size.DWORD(instanceAddr)))
                .Append(Ins.TEST(Register.ebx, Register.ebx))
                .Append(Ins.JZ("FAIL_NULL_PTR"))
                .Append(Ins.VirtualAt(Register.ebx))
                .Append($"\t{instanceAlias} {instanceType.Name}")
                .Append(Ins.EndVirtual())
                .Append(Ins.MOV(Register.eax, valueToAssignAddr))
                .Append(Ins.MOV($"{instanceAlias}.{field.Name}", Register.eax));
        }

        public static CompilationUnit AssignMemberq(string instanceAddr, CrateType instanceType, CrateFieldInfo field, string valueToAssignAddr)
        {
            Generator.UniqueTypeIdentifier(instanceType, out var instanceAlias);
            return new CompilationUnit()
                .Append(Ins.MOV(Register.ebx, Size.DWORD(instanceAddr)))
                .Append(Ins.TEST(Register.ebx, Register.ebx))
                .Append(Ins.JZ("FAIL_NULL_PTR"))
                .Append(Ins.VirtualAt(Register.ebx))
                .Append($"\t{instanceAlias} {instanceType.Name}")
                .Append(Ins.EndVirtual())
                .Append(Ins.MOV(Register.eax, Offset.CreateForDWORD(valueToAssignAddr, 4)))
                .Append(Ins.MOV(Offset.CreateForDWORD($"{instanceAlias}.{field.Name}", 4), Register.eax))
                .Append(Ins.MOV(Register.eax, Size.DWORD(valueToAssignAddr)))
                .Append(Ins.MOV($"{instanceAlias}.{field.Name}", Register.eax));
        }

        public static CompilationUnit AssignMemberb(string instanceAddr, CrateType instanceType, CrateFieldInfo field, string valueToAssignAddr)
        {
            Generator.UniqueTypeIdentifier(instanceType, out var instanceAlias);
            return new CompilationUnit()
                .Append(Ins.MOV(Register.ebx, Size.DWORD(instanceAddr)))
                .Append(Ins.TEST(Register.ebx, Register.ebx))
                .Append(Ins.JZ("FAIL_NULL_PTR"))
                .Append(Ins.VirtualAt(Register.ebx))
                .Append($"\t{instanceAlias} {instanceType.Name}")
                .Append(Ins.EndVirtual())
                .Append(Ins.MOV(Register.al, valueToAssignAddr))
                .Append(Ins.MOV($"{instanceAlias}.{field.Name}", Register.al));
        }

        public static CompilationUnit AssignMemberw(string instanceAddr, CrateType instanceType, CrateFieldInfo field, string valueToAssignAddr)
        {
            Generator.UniqueTypeIdentifier(instanceType, out var instanceAlias);
            return new CompilationUnit()
                .Append(Ins.MOV(Register.ebx, Size.DWORD(instanceAddr)))
                .Append(Ins.TEST(Register.ebx, Register.ebx))
                .Append(Ins.JZ("FAIL_NULL_PTR"))
                .Append(Ins.VirtualAt(Register.ebx))
                .Append($"\t{instanceAlias} {instanceType.Name}")
                .Append(Ins.EndVirtual())
                .Append(Ins.MOV(Register.ax, valueToAssignAddr))
                .Append(Ins.MOV($"{instanceAlias}.{field.Name}", Register.ax));
        }

        public static CompilationUnit AssignMemberStruct(string instanceAddr, CrateType instanceType, CrateFieldInfo field, string valueToAssignAddr)
        {
            Generator.UniqueTypeIdentifier(instanceType, out var instanceAlias);
            return new CompilationUnit()
                .Append(Ins.MOV(Register.ebx, Size.DWORD(instanceAddr)))
                .Append(Ins.TEST(Register.ebx, Register.ebx))
                .Append(Ins.JZ("FAIL_NULL_PTR"))
                .Append(Ins.VirtualAt(Register.ebx))
                .Append($"\t{instanceAlias} {instanceType.Name}")
                .Append(Ins.EndVirtual())
                .Append(Ins.LEA(Register.eax, $"{instanceAlias}.{field.Name}"))
                .Append(Ins.STDCALL("CopyMemory", $"{field.CrateType.GetAssemblySize()}, {Register.eax}, dword [{valueToAssignAddr}]"));
        }

        public static CompilationUnit CopyStruct(CrateType type, LocalVariable destination, LocalVariable valueToAssign)
        {
            return new CompilationUnit()
                .Append(Ins.STDCALL("CopyMemory", $"{type.GetAssemblySize()}, dword [{destination.Symbol}], dword [{valueToAssign.Symbol}]"));
        }

        public static CompilationUnit GetMember(string instanceAddr, CrateType instanceType, CrateFieldInfo field, string destAddr)
        {
            Generator.UniqueTypeIdentifier(instanceType, out var instanceAlias);
            var unit = new CompilationUnit()
                .Append(Ins.MOV(Register.ebx, Size.DWORD(instanceAddr)))
                .Append(Ins.TEST(Register.ebx, Register.ebx))
                .Append(Ins.JZ("FAIL_NULL_PTR"))
                .Append(Ins.VirtualAt(Register.ebx))
                .Append($"\t{instanceAlias} {instanceType.Name}")
                .Append(Ins.EndVirtual());
            if (field.CrateType == BuiltinTypes.Double)
            {
                unit.Append(Ins.MOV(Register.eax, $"{instanceAlias}.{field.Name}"))
                    .Append(Ins.MOV(destAddr, Register.eax))
                    .Append(Ins.MOV(Register.eax, Offset.CreateForDWORD($"{instanceAlias}.{field.Name}", 4)))
                    .Append(Ins.MOV(Offset.CreateForDWORD(destAddr, 4), Register.eax));
            }
            if (field.CrateType == BuiltinTypes.Int8)
            {
                unit.Append(Ins.MOV(Register.al, $"{instanceAlias}.{field.Name}"))
                    .Append(Ins.MOV(destAddr, Register.al));
            }
            if (field.CrateType == BuiltinTypes.Int8)
            {
                unit.Append(Ins.MOV(Register.ax, $"{instanceAlias}.{field.Name}"))
                    .Append(Ins.MOV(destAddr, Register.ax));
            }
            else if (!field.CrateType.IsBuiltin && !field.CrateType.IsReferenceType)
            {
                unit.Append(Ins.LEA(Register.eax, $"{instanceAlias}.{field.Name}"))
                    .Append(Ins.MOV(destAddr, Register.eax));
            }
            else
            {
                unit.Append(Ins.MOV(Register.eax, $"{instanceAlias}.{field.Name}"))
                    .Append(Ins.MOV(destAddr, Register.eax));
            }
            return unit;
        }


        public static CompilationUnit BeginCall(CrateFunction function, List<(CrateType type, string addr)> arguments)
        {
            if (function.CallingConvention == CallingConvention.StdCall)
            {
                var args = string.Join(", ", arguments.Select(x => ArgumentHelper(x.type, x.addr)));
                return new CompilationUnit()
                    .Append(Ins.STDCALL(function.Name, args));
            }
            else if (function.CallingConvention == CallingConvention.CInvoke)
            {
                var args = string.Join(", ", arguments.Select(x => ArgumentHelper(x.type, x.addr)));
                return new CompilationUnit()
                    .Append(Ins.CINVOKE(function.Name, args));
            }
            else if (function.CallingConvention == CallingConvention.Invoke)
            {
                var args = string.Join(", ", arguments.Select(x => ArgumentHelper(x.type, x.addr)));
                return new CompilationUnit()
                    .Append(Ins.INVOKE(function.Name, args));
            }
            throw new Exception($"unsupported calling convention {function.CallingConvention}");
        }

        public static CompilationUnit FinishCall(CrateFunction function, string destAddr)
        {
            if (function.CallingConvention == CallingConvention.StdCall)
            {
                return new CompilationUnit()
                    .Append(Ins.MOV(destAddr, Register.eax));
            }
            else if (function.CallingConvention == CallingConvention.CInvoke)
            {
                return new CompilationUnit()
                    .Append(Ins.MOV(destAddr, Register.eax));
            }
            else if (function.CallingConvention == CallingConvention.Invoke)
            {
                return new CompilationUnit()
                    .Append(Ins.MOV(destAddr, Register.eax));
            }
            throw new Exception($"unsupported calling convention {function.CallingConvention}");
        }

        public static CompilationUnit FinishCallq(CrateFunction function, string destAddr)
        {
            if (function.CallingConvention == CallingConvention.StdCall)
            {
                return new CompilationUnit()
                    .Append(Ins.MOV(Size.DWORD(destAddr), Register.eax))
                    .Append(Ins.MOV(Offset.CreateForDWORD(destAddr, 4), Register.ebx));
            }
            else if (function.CallingConvention == CallingConvention.CInvoke)
            {
                return new CompilationUnit()
                    .Append(Ins.MOVQ(Size.QWORD(destAddr), Register.mm0));
            }
            else if (function.CallingConvention == CallingConvention.Invoke)
            {
                return new CompilationUnit()
                    .Append(Ins.FSTP(Size.QWORD(destAddr))); // does this work?
            }
            throw new Exception($"unsupported calling convention {function.CallingConvention}");
        }

        public static CompilationUnit FinishCallb(CrateFunction function, string destAddr)
        {
            if (function.CallingConvention == CallingConvention.StdCall)
            {
                return new CompilationUnit()
                    .Append(Ins.MOV(destAddr, Register.al));
            }
            else if (function.CallingConvention == CallingConvention.CInvoke)
            {
                return new CompilationUnit()
                    .Append(Ins.MOV(destAddr, Register.al));
            }
            else if (function.CallingConvention == CallingConvention.Invoke)
            {
                return new CompilationUnit()
                    .Append(Ins.MOV(destAddr, Register.al));
            }
            throw new Exception($"unsupported calling convention {function.CallingConvention}");
        }

        public static CompilationUnit FinishCallw(CrateFunction function, string destAddr)
        {
            if (function.CallingConvention == CallingConvention.StdCall)
            {
                return new CompilationUnit()
                    .Append(Ins.MOV(destAddr, Register.ax));
            }
            else if (function.CallingConvention == CallingConvention.CInvoke)
            {
                return new CompilationUnit()
                    .Append(Ins.MOV(destAddr, Register.ax));
            }
            else if (function.CallingConvention == CallingConvention.Invoke)
            {
                return new CompilationUnit()
                    .Append(Ins.MOV(destAddr, Register.ax));
            }
            throw new Exception($"unsupported calling convention {function.CallingConvention}");
        }

        public static CompilationUnit Test(string conditionAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.eax, conditionAddr))
                .Append(Ins.TEST(Register.eax, Register.eax));
        }

        public static CompilationUnit Jmp(string label)
        {
            return new CompilationUnit().Append(Ins.JMP(label));
        }


        public static CompilationUnit Jz(string label)
        {
            return new CompilationUnit().Append(Ins.JZ(label));
        }

        public static CompilationUnit Jnz(string label)
        {
            return new CompilationUnit().Append(Ins.JNZ(label));
        }

        public static CompilationUnit Jump(BinaryOperator op, string label)
        {

            if (op == BinaryOperator.Equal)
            {
                return new CompilationUnit()
                    .Append(Ins.JZ(label));
            }
            if (op == BinaryOperator.NotEqual)
            {
                return new CompilationUnit()
                    .Append(Ins.JNZ(label));
            }
            if (op == BinaryOperator.LessThan)
            {
                return new CompilationUnit()
                    .Append(Ins.JB(label));
            }
            if (op == BinaryOperator.LessThanEqual)
            {
                return new CompilationUnit()
                    .Append(Ins.JBE(label));
            }
            if (op == BinaryOperator.GreaterThan)
            {
                return new CompilationUnit()
                    .Append(Ins.JA(label));
            }
            if (op == BinaryOperator.GreaterThanEqual)
            {
                return new CompilationUnit()
                    .Append(Ins.JAE(label));
            }
            throw new Exception($"unsupported comparison operation {op}");
        }

        public static CompilationUnit Label(string label)
        {
            return new CompilationUnit().Append(Ins.Label(label));
        }


        #region BinaryOperations
        public static CompilationUnit Convert_Int8_Int32(string sourceAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOVSX(Register.eax, Size.BYTE(sourceAddr)))
                .Append(Ins.MOV(destAddr, Register.eax));
        }

        public static CompilationUnit Convert_Int8_Int16(string sourceAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOVSX(Register.ax, Size.BYTE(sourceAddr)))
                .Append(Ins.MOV(destAddr, Register.ax));
        }

        public static CompilationUnit Convert_Int16_Int32(string sourceAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOVSX(Register.eax, Size.WORD(sourceAddr)))
                .Append(Ins.MOV(destAddr, Register.eax));
        }

        public static CompilationUnit Convert_Int16_Int8(string sourceAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.ax, sourceAddr))
                .Append(Ins.MOV(destAddr, Register.al));
        }

        public static CompilationUnit Convert_Int32_Int16(string sourceAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.eax, sourceAddr))
                .Append(Ins.MOV(destAddr, Register.ax));
        }

        public static CompilationUnit Convert_Int32_Int8(string sourceAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.eax, sourceAddr))
                .Append(Ins.MOV(destAddr, Register.al));
        }


        public static CompilationUnit Add_Int32_Int32(string lhsAddr, string rhsAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.ebx, lhsAddr))
                .Append(Ins.ADD(Register.ebx, Size.DWORD(rhsAddr)))
                .Append(Ins.MOV(destAddr, Register.ebx));
        }
        public static CompilationUnit Sub_Int32_Int32(string lhsAddr, string rhsAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.ebx, lhsAddr))
                .Append(Ins.SUB(Register.ebx, Size.DWORD(rhsAddr)))
                .Append(Ins.MOV(destAddr, Register.ebx));
        }
        public static CompilationUnit Mul_Int32_Int32(string lhsAddr, string rhsAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.ebx, lhsAddr))
                .Append(Ins.IMUL(Register.ebx, Size.DWORD(rhsAddr)))
                .Append(Ins.MOV(destAddr, Register.ebx));
        }
        public static CompilationUnit Div_Int32_Int32(string lhsAddr, string rhsAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.ebx, rhsAddr))
                .Append(Ins.TEST(Register.ebx, Register.ebx))
                .Append(Ins.JZ("FAIL_DIVISION_BY_ZERO"))
                .Append(Ins.MOV(Register.eax, Size.DWORD(lhsAddr)))
                .Append(Ins.XOR(Register.edx, Register.edx))
                .Append(Ins.IDIV(Register.ebx))
                .Append(Ins.MOV(destAddr, Register.eax));
        }

        public static CompilationUnit Add_Int32_Double(string lhsAddr, string rhsAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.FILD(Size.DWORD(lhsAddr)), "load 32 bit integer into st0")
                .Append(Ins.FADD(Size.QWORD(rhsAddr)), "add double at ecx to st0")
                .Append(Ins.FSTP(Size.QWORD(destAddr)));
        }

        public static CompilationUnit Sub_Int32_Double(string lhsAddr, string rhsAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.FILD(Size.DWORD(lhsAddr)))
                .Append(Ins.FSUB(Size.QWORD(rhsAddr)))
                .Append(Ins.FSTP(Size.QWORD(destAddr)));
        }

        public static CompilationUnit Mul_Int32_Double(string lhsAddr, string rhsAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.FILD(Size.DWORD(lhsAddr)))
                .Append(Ins.FMUL(Size.QWORD(rhsAddr)))
                .Append(Ins.FSTP(Size.QWORD(destAddr)));
        }

        public static CompilationUnit Div_Int32_Double(string lhsAddr, string rhsAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOVQ(Register.mm0, Size.QWORD(rhsAddr)))
                .Append(Ins.TEST(Register.mm0, Register.mm0))
                .Append(Ins.JZ("FAIL_DIVISION_BY_ZERO"))
                .Append(Ins.FILD(Size.DWORD(lhsAddr)))
                .Append(Ins.FDIV(Size.QWORD(rhsAddr)))
                .Append(Ins.FSTP(Size.QWORD(destAddr)));
        }

        public static CompilationUnit Add_Double_Int32(string lhsAddr, string rhsAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.FLD(Size.QWORD(lhsAddr)))
                .Append(Ins.FIADD(Size.DWORD(rhsAddr)))
                .Append(Ins.FSTP(Size.QWORD(destAddr)));
        }

        public static CompilationUnit Sub_Double_Int32(string lhsAddr, string rhsAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.FLD(Size.QWORD(lhsAddr)))
                .Append(Ins.FISUB(Size.DWORD(rhsAddr)))
                .Append(Ins.FSTP(Size.QWORD(destAddr)));
        }

        public static CompilationUnit Mul_Double_Int32(string lhsAddr, string rhsAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.FLD(Size.QWORD(lhsAddr)))
                .Append(Ins.FIMUL(Size.DWORD(rhsAddr)))
                .Append(Ins.FSTP(Size.QWORD(destAddr)));
        }

        public static CompilationUnit Div_Double_Int32(string lhsAddr, string rhsAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.ebx, rhsAddr))
                .Append(Ins.TEST(Register.ebx, Register.ebx))
                .Append(Ins.JZ("FAIL_DIVISION_BY_ZERO"))
                .Append(Ins.FLD(Size.QWORD(lhsAddr)))
                .Append(Ins.FIDIV(Size.DWORD(rhsAddr)))
                .Append(Ins.FSTP(Size.QWORD(destAddr)));
        }

        public static CompilationUnit Add_Double_Double(string lhsAddr, string rhsAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.FLD(Size.QWORD(lhsAddr)))
                .Append(Ins.FADD(Size.QWORD(rhsAddr)))
                .Append(Ins.FSTP(Size.QWORD(destAddr)));
        }

        public static CompilationUnit Sub_Double_Double(string lhsAddr, string rhsAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.FLD(Size.QWORD(lhsAddr)))
                .Append(Ins.FSUB(Size.QWORD(rhsAddr)))
                .Append(Ins.FSTP(Size.QWORD(destAddr)));
        }

        public static CompilationUnit Mul_Double_Double(string lhsAddr, string rhsAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.FLD(Size.QWORD(lhsAddr)))
                .Append(Ins.FMUL(Size.QWORD(rhsAddr)))
                .Append(Ins.FSTP(Size.QWORD(destAddr)));
        }

        public static CompilationUnit Div_Double_Double(string lhsAddr, string rhsAddr, string destAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOVQ(Register.mm0, Size.QWORD(rhsAddr)))
                .Append(Ins.TEST(Register.mm0, Register.mm0))
                .Append(Ins.JZ("FAIL_DIVISION_BY_ZERO"))
                .Append(Ins.FLD(Size.QWORD(lhsAddr)))
                .Append(Ins.FADD(Size.QWORD(rhsAddr)))
                .Append(Ins.FSTP(Size.QWORD(destAddr)));
        }

        public static CompilationUnit Cmp_Int8_Int8(string lhsAddr, string rhsAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.al, lhsAddr))
                .Append(Ins.MOV(Register.bl, rhsAddr))
                .Append(Ins.CMP(Register.al, Register.bl));
        }

        public static CompilationUnit Cmp_Int8_Int16(string lhsAddr, string rhsAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOVSX(Register.ax, Size.BYTE(lhsAddr)))
                .Append(Ins.MOV(Register.bx, rhsAddr))
                .Append(Ins.CMP(Register.ax, Register.bx));
        }

        public static CompilationUnit Cmp_Int8_Int32(string lhsAddr, string rhsAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOVSX(Register.eax, Size.BYTE(lhsAddr)))
                .Append(Ins.MOV(Register.ebx, rhsAddr))
                .Append(Ins.CMP(Register.eax, Register.ebx));
        }

        public static CompilationUnit Cmp_Int8_Double(string lhsAddr, string rhsAddr, out LocalVariable producedLocal)
        {
            Generator.LocalVariable(BuiltinTypes.Int16, out producedLocal);
            return new CompilationUnit()
                .Append(Ins.MOVSX(Register.ax, Size.WORD(lhsAddr)))
                .Append(Ins.FILD(Size.WORD(producedLocal.Symbol)))
                .Append(Ins.FLD(Size.QWORD(rhsAddr)))
                .Append(Ins.FCOMPP())
                .Append(Ins.FSTSW(Register.ax))
                .Append(Ins.FCLEX());
        }

        public static CompilationUnit Cmp_Int16_Int16(string lhsAddr, string rhsAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.ax, lhsAddr))
                .Append(Ins.MOV(Register.bx, rhsAddr))
                .Append(Ins.CMP(Register.ax, Register.bx));
        }

        public static CompilationUnit Cmp_Int16_Int8(string lhsAddr, string rhsAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.ax, lhsAddr))
                .Append(Ins.MOVSX(Register.bx, Size.BYTE(rhsAddr)))
                .Append(Ins.CMP(Register.eax, Register.ebx));
        }

        public static CompilationUnit Cmp_Int16_Int32(string lhsAddr, string rhsAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOVSX(Register.eax, Size.WORD(lhsAddr)))
                .Append(Ins.MOV(Register.ebx, rhsAddr))
                .Append(Ins.CMP(Register.eax, Register.ebx));
        }

        public static CompilationUnit Cmp_Int16_Double(string lhsAddr, string rhsAddr)
        {
            return new CompilationUnit()
                .Append(Ins.FILD(Size.WORD(lhsAddr)))
                .Append(Ins.FLD(Size.QWORD(rhsAddr)))
                .Append(Ins.FCOMPP())
                .Append(Ins.FSTSW(Register.ax))
                .Append(Ins.FCLEX());
        }

        public static CompilationUnit Cmp_Int32_Int32(string lhsAddr, string rhsAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.eax, lhsAddr))
                .Append(Ins.MOV(Register.ebx, rhsAddr))
                .Append(Ins.CMP(Register.eax, Register.ebx));
        }

        public static CompilationUnit Cmp_Int32_Int8(string lhsAddr, string rhsAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.eax, lhsAddr))
                .Append(Ins.MOVSX(Register.ebx, Size.BYTE(rhsAddr)))
                .Append(Ins.CMP(Register.eax, Register.ebx));
        }

        public static CompilationUnit Cmp_Int32_Int16(string lhsAddr, string rhsAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.eax, lhsAddr))
                .Append(Ins.MOVSX(Register.ebx, Size.WORD(rhsAddr)))
                .Append(Ins.CMP(Register.eax, Register.ebx));
        }

        public static CompilationUnit Cmp_Int32_Double(string lhsAddr, string rhsAddr)
        {
            return new CompilationUnit()
                .Append(Ins.FILD(Size.DWORD(lhsAddr)))
                .Append(Ins.FLD(Size.QWORD(rhsAddr)))
                .Append(Ins.FCOMPP())
                .Append(Ins.FSTSW(Register.ax))
                .Append(Ins.FCLEX());
        }

        public static CompilationUnit Cmp_Double_Double(string lhsAddr, string rhsAddr)
        {
            return new CompilationUnit()
                .Append(Ins.FLD(Size.QWORD(lhsAddr)))
                .Append(Ins.FLD(Size.QWORD(rhsAddr)))
                .Append(Ins.FCOMPP())
                .Append(Ins.FSTSW(Register.ax))
                .Append(Ins.FCLEX());
        }

        public static CompilationUnit Cmp_Double_Int8(string lhsAddr, string rhsAddr, out LocalVariable producedLocal)
        {
            Generator.LocalVariable(BuiltinTypes.Int16, out producedLocal);
            return new CompilationUnit()
                .Append(Ins.MOVSX(Register.ax, Size.BYTE(rhsAddr)))
                .Append(Ins.MOV(producedLocal.Symbol, Register.ax))
                .Append(Ins.FLD(Size.QWORD(lhsAddr)))
                .Append(Ins.FILD(Size.WORD(rhsAddr)))
                .Append(Ins.FCOMPP())
                .Append(Ins.FSTSW(Register.ax))
                .Append(Ins.FCLEX());
        }

        public static CompilationUnit Cmp_Double_Int16(string lhsAddr, string rhsAddr)
        {
            return new CompilationUnit()
                .Append(Ins.FLD(Size.QWORD(lhsAddr)))
                .Append(Ins.FILD(Size.WORD(rhsAddr)))
                .Append(Ins.FCOMPP())
                .Append(Ins.FSTSW(Register.ax))
                .Append(Ins.FCLEX());
        }

        public static CompilationUnit Cmp_Double_Int32(string lhsAddr, string rhsAddr)
        {
            return new CompilationUnit()
                .Append(Ins.FLD(Size.QWORD(lhsAddr)))
                .Append(Ins.FILD(Size.DWORD(rhsAddr)))
                .Append(Ins.FCOMPP())
                .Append(Ins.FSTSW(Register.ax))
                .Append(Ins.FCLEX());
        }

        public static CompilationUnit Cmp_Ref_Nullptr(string address)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.eax, address))
                .Append(Ins.TEST(Register.eax, Register.eax));
        }

        public static CompilationUnit Cmp_String_String(string lhsAddr, string rhsAddr)
        {
            return new CompilationUnit()
                .Append(Ins.STDCALL("strcmp", $"dword [{lhsAddr}], dword [{rhsAddr}]"))
                .Append(Ins.TEST(Register.eax, Register.eax));
        }

        public static CompilationUnit Add_String_String(string lhsAddr, string rhsAddr, string destination)
        {
            return new CompilationUnit()
                .Append(Ins.STDCALL("strcat", $"dword [{lhsAddr}], dword [{rhsAddr}]"))
                .Append(Ins.MOV(destination, Register.eax));
        }

        #endregion

        #region UnaryOperations

        public static CompilationUnit StringSize(string sourceAddress)
        {
            return new CompilationUnit()
                .Append(Ins.STDCALL("strlen", $"dword [{sourceAddress}]"));
        }

        public static CompilationUnit Not(string destAddr, string sourceAddr)
        {
            var local_set_true = Generator.Label("LOCAL_SET_TRUE");
            var local_finalize = Generator.Label("LOCAL_FINALIZE");
            return new CompilationUnit()
                .Append(Ins.MOV(Register.eax, sourceAddr))
                .Append(Ins.TEST(Register.eax, Register.eax))
                .Append(Ins.JZ(local_set_true))
                .Append("; otherise, set to false")
                .Append(Ins.MOV(Register.eax, Size.DWORD(0)))
                .Append(Ins.JMP(local_finalize))
                .Append(Ins.MOV(destAddr, Register.eax));

        }

        public static CompilationUnit Negate(string destAddr, string sourceAddr)
        {
            return new CompilationUnit()
                .Append(Ins.MOV(Register.eax, sourceAddr))
                .Append(Ins.NEG(Register.eax))
                .Append(Ins.MOV(destAddr, Register.eax));
        }

        #endregion

        #region Helpers

        private static string ArgumentHelper(CrateType type, string argAddr)
        {
            //if (type == BuiltinTypes.Double) return $"qword [{argAddr}]"; //only available for 64 bit
            if (type == BuiltinTypes.Double) return $"dword [{argAddr}], dword [{argAddr}+4]";
            if (!type.IsBuiltin && !type.IsReferenceType) return argAddr;
            if (type == BuiltinTypes.Int8) return $"dword [{argAddr}]";
            if (type == BuiltinTypes.Int16) return $"dword [{argAddr}]";
            return $"dword [{argAddr}]";
        }

        #endregion
    }
}
