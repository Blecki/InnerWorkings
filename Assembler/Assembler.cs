using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assemblage
{
    public class Assembler
    {
        public static byte Assemble(LexResult lexed, System.IO.BinaryWriter into)
        {
            var encodingTable = IN8.GetEncodingTable();

            foreach (var token in lexed.Tokens)
            {
                if (token.StartsWith("0X"))
                {
                    byte value = 0;
                    if (!byte.TryParse(token.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out value))
                        return 0x02;
                    into.Write(value);
                }
                else if (token.StartsWith("$"))
                {
                    if (lexed.Labels.ContainsKey(token.Substring(1)))
                    {
                        ushort value = lexed.Labels[token.Substring(1)];
                        into.Write((byte)(value >> 8));
                        into.Write((byte)value); //Avoid endian issues.
                    }
                    else
                        return 0x04;
                }
                else if (token.StartsWith("["))
                {
                    foreach (var c in token.Substring(1, token.Length - 2))
                        into.Write((byte)c);
                }
                else if (encodingTable.ContainsKey(token))
                    into.Write(encodingTable[token]);
                else
                    return 0x05;
            }
            return 0x00;
        }
    }
}
