using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assemblage
{
    class Program
    {
        static internal String Version = "0.1";

        static void Main(string[] args)
        {
            var options = new SwitchOptions();
            if (!CommandLine.ParseArguments(args, options))
                return;

            try
            {
                            if (String.IsNullOrEmpty(options.outFile))
                            {
                                Console.WriteLine("You must supply an outfile.");
                                return;
                            }

                            var source = new System.IO.StreamReader(options.inFile);
                            var lexed = new LexResult();
                            var r = Lexer.Lex(source, lexed);
                            if (r == 0x00)
                            {
                                var destination = System.IO.File.Open(options.outFile, System.IO.FileMode.Create);
                                var writer = new System.IO.BinaryWriter(destination);
                                r = Assembler.Assemble(lexed, writer);
                                writer.Flush();
                                destination.Flush();
                                writer.Close();
                            }
                            Console.WriteLine("Finished with code " + r);
            }
            /*case "emulate":
                        {
                            var source = System.IO.File.Open(options.inFile, System.IO.FileMode.Open);
                            var emulator = new IN8();
                            source.Read(emulator.MEM, 0, 0xFFFF);

                            // Attach devices
                            var teletypeTerminal = new TT3();
                            emulator.AttachHardware(teletypeTerminal, 0x04);

                            while (emulator.STATE_FLAGS == 0x00) emulator.Step();

                            Console.WriteLine(String.Format("Finished in state {0:X2}", emulator.STATE_FLAGS));
                            Console.WriteLine(String.Format("{0:X2} {1:X2} {2:X2} {3:X2} {4:X2} {5:X2} {6:X2} {7:X2}",
                                emulator.A, emulator.B, emulator.C, emulator.D, emulator.E, emulator.H, emulator.L, emulator.O));
                            Console.WriteLine(String.Format("{0:X4} {1:X4} {2:X8}", emulator.IP, emulator.SP, emulator.CLOCK));
                            break;
                        }
                }
            }*/
            catch (Exception e)
            {
                Console.WriteLine("An error occured.");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

    }
}
