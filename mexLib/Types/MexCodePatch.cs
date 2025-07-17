using mexLib.HsdObjects;
using System.ComponentModel;
using System.Text;

namespace mexLib.Types
{
    public class MexCodePatch : MexCodeBase
    {
        [Browsable(false)]
        public HSDFunctionDat Function { get; internal set; }

        [Browsable(false)]
        public string Disassembled => Disassemble();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="function"></param>
        public MexCodePatch(string name, HSDFunctionDat function)
        {
            Name = name;
            Function = function;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string Disassemble()
        {
            var code = Function.Code;

            // grab additional information
            var debug = Function.DebugTable.Array;
            var reloc = Function.RelocationTable.Array;
            Array.Resize(ref debug, Function.DebugCount);
            Array.Resize(ref reloc, Function.RelocationCount);

            StringBuilder asm = new();
            for (int i = 0; i < code.Length;)
            {
                // check for symbol
                var db = debug.FirstOrDefault(e => i == e.CodeStartOffset);
                if (db != null)
                    asm.AppendLine($"# {db.Symbol}:");

                // check for relocation
                var rel = reloc.FirstOrDefault(e => e.CodeOffset >= i && e.CodeOffset < (i + 4));
                if (rel != null)
                {
                    asm.AppendLine($"{code[i++]:X2}{code[i++]:X2}{code[i++]:X2}{code[i++]:X2} #0x{rel.Address:X8}");
                }
                else
                {
                    asm.AppendLine($"{code[i++]:X2}{code[i++]:X2}{code[i++]:X2}{code[i++]:X2}");
                }

            }
            return asm.ToString();
            //byte[] ppcCode = new byte[]
            //{
            //    0x7C, 0x08, 0x02, 0xA6, // mflr r0
            //    0x3C, 0x80, 0x00, 0x01, // lis r4, 1
            //    0x38, 0x84, 0x00, 0x01  // addi r4, r4, 1
            //};

            //using var capstone = CapstoneDisassembler.CreatePowerPcDisassembler(
            //    PowerPcDisassembleMode.Bit32 | PowerPcDisassembleMode.BigEndian);

            //var instructions = capstone.Disassemble(Function.Code, 0);

            //// check hooks?
            //var hook = Function.FunctionTable.Array;
            //Array.Resize(ref hook, Function.FunctionCount);

            //// grab additional information
            //var debug = Function.DebugTable.Array;
            //var reloc = Function.RelocationTable.Array;
            //Array.Resize(ref debug, Function.DebugCount);
            //Array.Resize(ref reloc, Function.RelocationCount);

            //StringBuilder asm = new ();

            //foreach (var ins in instructions)
            //{
            //    // check for symbol
            //    var db = debug.FirstOrDefault(e => ins.Address == e.CodeStartOffset);
            //    if (db != null)
            //        asm.AppendLine($"{db.Symbol}:");

            //    // check for relocation
            //    var rel = reloc.FirstOrDefault(e => e.CodeOffset >= ins.Address && e.CodeOffset < (ins.Address + 4));
            //    if (rel != null)
            //    {
            //        asm.AppendLine($"0x{ins.Address:X}: {ins.Mnemonic} {ins.Operand} 0x{rel.Address:X8}");
            //    }
            //    else
            //    {
            //        asm.AppendLine($"0x{ins.Address:X}: {ins.Mnemonic} {ins.Operand}");
            //    }
            //}

            //return asm.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<uint> UsedAddresses()
        {
            for (int i = 0; i < Function.FunctionCount; i++)
            {
                if ((Function.FunctionTable[i].Address & 0x80000000) != 0)
                {
                    yield return Function.FunctionTable[i].Address;
                }
            }
        }
    }
}
