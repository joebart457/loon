using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler._Generator
{
    internal enum Register
    {
        al,
        bl,
        cl,
        dl,
        ax,
        bx,
        cx,
        dx,
        eax,
        ebx,
        ecx,
        edx,
        mm0,
    }

    internal class Size
    {
        public string Repr { get; private set; }
        public Size(string repr)
        {
            Repr = repr;
        }
        public static Size BYTE(string addr) => new Size($"byte [{addr}]");
        public static Size WORD(string addr) => new Size($"word [{addr}]");
        public static Size DWORD(string addr) => new Size($"dword [{addr}]");
        public static Size DWORD(int immediateValue) => new Size($"dword {immediateValue}");
        public static Size QWORD(string addr) => new Size($"qword [{addr}]");
    }

    internal class Offset
    {
        public string Repr { get; private set; }
        public Offset(string repr)
        {
            Repr = repr;
        }
        public static Offset CreateForDWORD(string addr, int offset) => new Offset($"dword [{addr}+{offset}]");
    }


    internal static class Ins
    {
        public static string LEA(Register dest, string sourceAddr)
        {
            return $"lea {dest}, [{sourceAddr}]";
        }

        public static string MOV(Register dest, string sourceAddr)
        {
            return $"mov {dest}, [{sourceAddr}]";
        }

        public static string MOV(Register dest, int immediate)
        {
            return $"mov {dest}, dword {immediate}";
        }

        public static string MOV(string dest, int immediate)
        {
            return $"mov [{dest}], dword {immediate}";
        }

        public static string MOV(Size dest, Register register)
        {
            return $"mov {dest.Repr}, {register}";
        }

        public static string MOVQ(Size dest, Register register)
        {
            return $"movq {dest.Repr}, {register}";
        }

        public static string MOVQ(Register dest, Size source)
        {
            return $"movq {dest}, {source.Repr}";
        }

        public static string MOV_ADDRESS(Register dest, string sourceAddr)
        {
            return $"mov {dest}, {sourceAddr}";
        }

        public static string MOV(Register dest, Register source)
        {
            return $"mov {dest}, {source}";
        }

        public static string MOV(Register dest, Size source)
        {
            return $"mov {dest}, {source.Repr}";
        }

        public static string MOV(string destAddr, Register source)
        {
            return $"mov [{destAddr}], {source}";
        }

        public static string MOV(Register dest, Offset source)
        {
            return $"mov {dest}, {source.Repr}";
        }

        public static string MOV(Offset dest, Register source)
        {
            return $"mov {dest.Repr}, {source}";
        }

        public static string MOVSX(Register dest, Size source)
        {
            return $"movsx {dest}, {source.Repr}";
        }

        public static string XOR(Register dest, Register source)
        {
            return $"xor {dest}, {source}";
        }

        public static string PUSH(Register register)
        {
            return $"push {register}";
        }

        public static string CALL(string indirectAddress)
        {
            return $"call [{indirectAddress}]";
        }

        public static string NEG(Register register)
        {
            return $"neg {register}";
        }

        public static string CMP(Register dest, Register source)
        {
            return $"cmp {dest}, {source}";
        }

        public static string FCOMPP()
        {
            return $"fcompp";
        }

        public static string FSTSW(Register register)
        {
            return $"fstsw {register}";
        }

        public static string FCLEX()
        {
            return $"fclex";
        }

        public static string ADD(Register dest, Size source)
        {
            return $"add {dest}, {source.Repr}";
        }

        public static string ADD(Register dest, Register source)
        {
            return $"add {dest}, {source}";
        }

        public static string SUB(Register dest, Size source)
        {
            return $"sub {dest}, {source.Repr}";
        }

        public static string SUB(Register dest, Register source)
        {
            return $"sub {dest}, {source}";
        }

        public static string MUL(Register dest, Size source)
        {
            return $"mul {dest}, {source.Repr}";
        }
        public static string IMUL(Register dest, Size source)
        {
            return $"imul {dest}, {source.Repr}";
        }

        public static string IMUL(Register dest, Register source)
        {
            return $"imul {dest}, {source}";
        }

        public static string DIV(Register dest, Size source)
        {
            return $"div {dest}, {source.Repr}";
        }

        public static string IDIV(Register source)
        {
            return $"idiv {source}";
        }

        public static string FILD(Size operand)
        {
            return $"fild {operand.Repr}";
        }

        public static string FLD(Size operand)
        {
            return $"fld {operand.Repr}";
        }

        public static string FADD(Size operand)
        {
            return $"fadd {operand.Repr}";
        }

        public static string FSUB(Size operand)
        {
            return $"fsub {operand.Repr}";
        }

        public static string FMUL(Size operand)
        {
            return $"fmul {operand.Repr}";
        }

        public static string FDIV(Size operand)
        {
            return $"fdiv {operand.Repr}";
        }

        public static string FIADD(Size operand)
        {
            return $"fiadd {operand.Repr}";
        }

        public static string FISUB(Size operand)
        {
            return $"fisub {operand.Repr}";
        }

        public static string FIMUL(Size operand)
        {
            return $"fimul {operand.Repr}";
        }

        public static string FIDIV(Size operand)
        {
            return $"fidiv {operand.Repr}";
        }

        public static string FSTP(Size operand)
        {
            return $"fstp {operand.Repr}";
        }

        public static string TEST(Register operand1, Register operand2)
        {
            return $"test {operand1}, {operand2}";
        }

        public static string JZ(string label)
        {
            return $"jz {label}";
        }

        public static string JNZ(string label)
        {
            return $"jnz {label}";
        }

        public static string JBE(string label)
        {
            return $"jbe {label}";
        }

        public static string JAE(string label)
        {
            return $"jae {label}";
        }

        public static string JA(string label)
        {
            return $"ja {label}";
        }

        public static string JB(string label)
        {
            return $"jb {label}";
        }

        public static string JMP(string label)
        {
            return $"jmp {label}";
        }

        public static string VirtualAt(Register register)
        {
            return $"virtual at {register}";
        }

        public static string EndVirtual()
        {
            return "end virtual";
        }

        public static string RET()
        {
            return "ret";
        }

        public static string ENDP()
        {
            return "endp";
        }

        public static string RETURN()
        {
            return "return";
        }

        public static string Label(string label)
        {
            return $"{label}:";
        }

        public static string STDCALL(string functionName, string args)
        {
            return $"stdcall {functionName}{(string.IsNullOrWhiteSpace(args) ? "" : ",")} {args}";
        }

        public static string CINVOKE(string functionName, string args)
        {
            return $"cinvoke {functionName}{(string.IsNullOrWhiteSpace(args) ? "" : ",")} {args}";
        }

        public static string INVOKE(string functionName, string args)
        {

            return $"invoke {functionName}{(string.IsNullOrWhiteSpace(args) ? "" : ",")} {args}";
        }

        public static string CPY_STRING(string dest, string source)
        {
            return $"cpy_str {dest}, {source}";
        }

        public static string CPY_STRING(Register dest, string source)
        {
            return $"cpy_str_reg {dest}, {source}";
        }
    }
}
