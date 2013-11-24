using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assemblage
{
    public class SwitchOptions
    {
        public String inFile;
        public String outFile;
        public List<String> options = new List<string>();

        public bool isset(String opt)
        {
            return options.Contains(opt);
        }
    }

    public class CommandLine
    {
        public static bool ParseArguments(string[] args, SwitchOptions options)
        {
            if (args.Length == 0)
            {
                PrintHelpTopic("main");
                return false;
            }

            for (var i = 0; i < args.Length; ++i)
            {
                if (args[i] == "-help" || args[i] == "-h")
                {
                    if (i + 1 < args.Length)
                        PrintHelpTopic(args[i + 1]);
                    else
                        PrintHelpTopic("main");
                    return false;
                }
                else if (args[i] == "-i" || args[i] == "-in" || args[i] == "-input" || args[i] == "-infile")
                {
                    if (i + 1 < args.Length)
                    {
                        options.inFile = args[i + 1];
                        ++i;
                    }
                    else
                        return false;
                }
                else if (args[i] == "-o" || args[i] == "-out" || args[i] == "-output" || args[i] == "-outfile")
                {
                    if (i + 1 < args.Length)
                    {
                        options.outFile = args[i + 1];
                        ++i;
                    }
                    else
                    {
                        Console.WriteLine("You must supply an argument to -out");
                        return false;
                    }
                }
                else if (args[i] == "-option" || args[i] == "-op")
                {
                    if (i + 1 < args.Length)
                    {
                        options.options.Add(args[i + 1]);
                        ++i;
                    }
                    else
                    {
                        Console.WriteLine("You must supply an argument to -option");
                        return false;
                    }
                }
                else
                {
                    if (options.inFile == null) options.inFile = args[i];
                    else if (options.outFile == null) options.outFile = args[i];
                    else
                    {
                        Console.WriteLine("I do not understand the switch '" + args[i] + "'");
                        return false;
                    }
                }
            }

            return true;
        }

        private static void PrintHelpTopic(String topic)
        {
            if (topic == "main")
            {
                Console.WriteLine("Passages version " + Program.Version + ".");
                Console.WriteLine("-help/-h [topic] : Get help on a specific topic");
                Console.WriteLine("-in/-i : Specify input file");
                Console.WriteLine("-out/-o : Specify output file");
                Console.WriteLine("-mode/-m : Specify mode. See -help mode for list of modes");
                Console.WriteLine("-option/-op : Specify options for file to be built");
            }
            else if (topic == "topic" || topic == "topics")
            {
                Console.WriteLine("Help topics:");
                Console.WriteLine("-in -out -mode modes -option");
            }
            else if (topic == "-mode" || topic == "mode" || topic == "-m" || topic == "modes")
            {
                Console.WriteLine("-mode 'mode' specifies the mode of operation. Available modes:");
                Console.WriteLine("extract-prose : Default mode; extracts all leaf passages");
                Console.WriteLine("pre : Preprocesses the file, but does not parse it");
            }
            else
            {
                Console.WriteLine("I don't seem to have help for the topic '" + topic + "'.");
                Console.WriteLine("Try -help topics for a list of topics");
            }
        }
    }
}
