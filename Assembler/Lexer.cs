using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assemblage
{
    public class LexResult
    {
        public Dictionary<String, ushort> Labels = new Dictionary<string,ushort>();
        public List<String> Tokens = new List<string>();
        public ushort ByteCount = 0;
    }

    public class Lexer
    {
        public static byte Lex(System.IO.StreamReader code, LexResult into)
        {
            var token = "";
            byte r;
            while (!code.EndOfStream)
            {
                var c = code.Read();
                if (c == '[')
                {
                    r = AddToken(token, into);
                    token = "";
                    if (r != 0x00) return r;

                    while (!code.EndOfStream && c != ']')
                    {
                        token += (char)c;
                        c = code.Read();
                    }

                    if (c != ']') return 0x08;
                    token += ']';

                    r = AddToken(token, into);
                    token = "";
                    if (r != 0x00) return r;
                }
                else if (c == '.' || c == '\n' || c == '\r')
                {
                    r = AddToken(token, into);
                    if (r != 0x00) return r;
                    token = "";
                }   
                else if (c == ';')
                {
                    r = AddToken(token, into);
                    token = "";
                    if (r != 0x00) return r;
                    while (!code.EndOfStream && c != '\n' && c != '\r') c = code.Read();
                }
                else
                    token += (char)c;
            }
            r = AddToken(token, into);
            if (r != 0x00) return r;

            Console.WriteLine(String.Format("Binary size: {0:X2} bytes.", into.ByteCount));

            return 0x00;
        }

        private static byte AddToken(String token, LexResult into)
        {
            token = token.Trim();
            if (String.IsNullOrEmpty(token)) return 0x00;
            if (token[0] == ':')
            {
                if (into.Labels.ContainsKey(token.Substring(1))) return 0x03;
                else into.Labels.Add(token.Substring(1), into.ByteCount);
            }
            else
            {
                into.Tokens.Add(token);
                if (token[0] == '$') into.ByteCount += 2;
                else if (token[0] == '[') into.ByteCount += (ushort)(token.Length - 2);
                else into.ByteCount += 1;
            }
            return 0x00;
        }
    }
}
