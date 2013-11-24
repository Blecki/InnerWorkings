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

                var source = System.IO.File.Open(options.inFile, System.IO.FileMode.Open);
                var emulator = new IN8();
                source.Read(emulator.MEM, 0, 0xFFFF);

                // Attach devices
                var teletypeTerminal = new TT3();
                teletypeTerminal.Connect(emulator, 0x04);

                System.Threading.Barrier barrier = new System.Threading.Barrier(2);
                var screenThread = new System.Threading.Thread(() =>
                {
                    var screen = new ICM_CD2.VisualHardwareGrid(emulator);
                    screen.AddHardware(typeof(ICM_CD2.ICM), new Microsoft.Xna.Framework.Point(16, 16), 0x02, 0x03);
                    screen.AddHardware(typeof(ICM_CD2.SevenSegment), new Microsoft.Xna.Framework.Point(528 + 32, 16), 0x05);
                    barrier.SignalAndWait();
                    screen.Run();
                });
                screenThread.SetApartmentState(System.Threading.ApartmentState.STA);
                screenThread.Start();
    
                barrier.SignalAndWait();

                var cycleTime = DateTime.Now;
                uint cycles = 0;
                while ((emulator.STATE_FLAGS & 0x80) != 0x00)
                {
                    cycles += emulator.Step();
                    if (cycles > 10)
                    {
                        Console.SetCursorPosition(0, 0);
                        
                        //Limit to 1 mhz
                        var endCycleTime = DateTime.Now;
                        Console.WriteLine(
                            String.Format("Running at {0} per {1}", cycles, (endCycleTime - cycleTime).TotalMilliseconds));
                        
                        cycleTime = endCycleTime;
                        cycles -= 10;


                        System.Threading.Thread.Sleep(1);
                    }
                }

                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine(String.Format("Finished in state {0:X2}", emulator.STATE_FLAGS));
                Console.WriteLine(String.Format("A:{0:X2} B:{1:X2} C:{2:X2} D:{3:X2} E:{4:X2} H:{5:X2} L:{6:X2} O:{7:X2}",
                    emulator.A, emulator.B, emulator.C, emulator.D, emulator.E, emulator.H, emulator.L, emulator.O));
                Console.WriteLine(String.Format("IP:{0:X4} SP:{1:X4} CLK:{2:X8}", emulator.IP, emulator.SP, emulator.CLOCK));
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured.");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

    }
}
