using Crater.Shared.Models;
using Loon.Compiler._Generator;
using Loon.Compiler.Constants;
using Loon.Compiler.Extensions;
using Loon.Compiler.Models._Function;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler.Models
{
    internal class EntryPoint
    {
        public CompiledFunction Function { get; private set; }

        public EntryPoint(CompiledFunction function)
        {
            Function = function;
        }

        public string GenerateAssembly(CompilationSettings settings, int indentLevel = 0)
        {
            var sb = new StringBuilder();

            if (settings.OutputType == OutputType.Exe)
            {
                sb.AppendLine("_start:".Indent(indentLevel));
                //Win specific code
                sb.AppendLine(Ins.INVOKE("GetProcessHeap", "").Indent(indentLevel + 1));
                sb.AppendLine(Ins.TEST(Register.eax, Register.eax).Indent(indentLevel + 1));
                sb.AppendLine(Ins.JZ("FAIL_ALLOC").Indent(indentLevel + 1));
                sb.AppendLine(Ins.MOV(CompilationConstants.HHeap, Register.eax).Indent(indentLevel + 1));
                // end win specific code
                sb.AppendLine(Ins.MOV(Register.eax, 0).Indent(indentLevel + 1));
                sb.AppendLine(Ins.STDCALL(Function.Function.Name, "").Indent(indentLevel + 1));
                sb.AppendLine(Ins.PUSH(Register.eax).Indent(indentLevel + 1));
                sb.AppendLine(Ins.CALL("ExitProcess").Indent(indentLevel + 1));
                sb.AppendLine(Ins.Label("FAIL_ALLOC").Indent(indentLevel + 1));
                sb.AppendLine(Ins.Label("FAIL_NULL_PTR").Indent(indentLevel + 1));
                sb.AppendLine(Ins.Label("FAIL_DIVISION_BY_ZERO").Indent(indentLevel + 1));
                sb.AppendLine(Ins.Label("FAIL_HEAP_FREE").Indent(indentLevel + 1));
                sb.AppendLine("push 1".Indent(indentLevel + 1));
                sb.AppendLine(Ins.CALL("ExitProcess").Indent(indentLevel + 1));
            }
            else if (settings.OutputType == OutputType.Dll)
            {
                sb.AppendLine("proc DllEntryPoint hinstDLL, fdwReason, lpvReserved".Indent(indentLevel));
                //Win specific code
                sb.AppendLine(Ins.INVOKE("GetProcessHeap", "").Indent(indentLevel + 1));
                sb.AppendLine(Ins.TEST(Register.eax, Register.eax).Indent(indentLevel + 1));
                sb.AppendLine(Ins.JZ("FAIL_ALLOC").Indent(indentLevel + 1));
                sb.AppendLine(Ins.MOV(CompilationConstants.HHeap, Register.eax).Indent(indentLevel + 1));
                // end win specific code
                sb.AppendLine(Ins.STDCALL(Function.Function.Name, "").Indent(indentLevel + 1));
                sb.AppendLine(Ins.RET().Indent(indentLevel + 1));
                sb.AppendLine(Ins.Label("FAIL_ALLOC").Indent(indentLevel + 1));
                sb.AppendLine(Ins.Label("FAIL_NULL_PTR").Indent(indentLevel + 1));
                sb.AppendLine(Ins.Label("FAIL_DIVISION_BY_ZERO").Indent(indentLevel + 1));
                sb.AppendLine(Ins.Label("FAIL_HEAP_FREE").Indent(indentLevel + 1));
                sb.AppendLine(Ins.MOV(Register.eax, 0).Indent(indentLevel + 1));
                sb.AppendLine(Ins.RET().Indent(indentLevel + 1));
                sb.AppendLine(Ins.ENDP().Indent(indentLevel));
            }
            else throw new Exception($"unable to generate code for output type {settings.OutputType}");
            return sb.ToString();
        }
    }
}
