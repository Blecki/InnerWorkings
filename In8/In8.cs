


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assemblage
{
	public class IN8
	{
		//Setup memory and registers

		public byte[] MEM = new byte[0x10000];
		public byte A, B, C, D, E, H, L = 0;
		public ushort IP, SP = 0;
		public byte O = 0;
		public uint CLOCK = 0;
		public byte[] PORT = new byte[0x100];
		public Hardware[] HARDWARE = new Hardware[0x100];
		public byte[] INTERUPT_QUEUE = new byte[0x100];
		public byte INTERUPT_HEAD = 0;
		public byte INTERUPT_END = 0;
		public ushort IQ = 0;

		private byte N { get { CLOCK++; return MEM[(byte)(IP++)]; } }
		private ushort HL { get { return (ushort)(((ushort)H << 8) + (ushort)L); }}
		private ushort DE { get { return (ushort)(((ushort)D << 8) + (ushort)E); }}

		public byte STATE_FLAGS = 0x80;

		public void AttachHardware(Hardware h, params byte[] ports)
		{
			foreach (var port in ports)
				HARDWARE[port] = h;
		}

		private void RFQ()
		{
			unchecked
			{
				STATE_FLAGS &= (byte)(~0x08);
			}
			O = MEM[(ushort)SP++];
			L = MEM[(ushort)SP++];
			H = MEM[(ushort)SP++];
			E = MEM[(ushort)SP++];
			D = MEM[(ushort)SP++];
			C = MEM[(ushort)SP++];
			B = MEM[(ushort)SP++];
			A = MEM[(ushort)SP++];
			IP = (ushort)(MEM[(ushort)SP++] << 8);
			IP += MEM[(ushort)SP++];
		}

		public void Interrupt(byte data)
		{
			INTERUPT_QUEUE[INTERUPT_END] = data;
			INTERUPT_END += 1;
			if (INTERUPT_END == INTERUPT_HEAD)
				STATE_FLAGS |= 0x10;
		}

		public uint Step()
		{
			if ((STATE_FLAGS & 0x80) == 0) return 1;

			var startCLOCK = CLOCK;

			if ((STATE_FLAGS & 0x08) == 0 && INTERUPT_HEAD != INTERUPT_END)
			{
				if (IQ != 0x0000)
				{
					CLOCK += 48;

					MEM[(ushort)--SP] = (byte)IP;
					MEM[(ushort)--SP] = (byte)(IP >> 8);
					MEM[(ushort)--SP] = A;
					MEM[(ushort)--SP] = B;
					MEM[(ushort)--SP] = C;
					MEM[(ushort)--SP] = D;
					MEM[(ushort)--SP] = E;
					MEM[(ushort)--SP] = H;
					MEM[(ushort)--SP] = L;
					MEM[(ushort)--SP] = O;

					A = INTERUPT_QUEUE[INTERUPT_HEAD];
					INTERUPT_HEAD += 1;

					STATE_FLAGS |= 0x08;
					IP = IQ;
				}
				else
					INTERUPT_HEAD += 1;
			}

			switch (N)
			{
				case 0  : /* CAL     */ { CLOCK+=4; MEM[(ushort)(--SP)] = (byte)IP; MEM[(ushort)(--SP)] = (byte)(IP >> 8); IP = HL; break; }
				case 1  : /* MTA A B */ { CLOCK+=1;  A = B; break; }
				case 2  : /* MTA A C */ { CLOCK+=1;  A = C; break; }
				case 3  : /* MTA A D */ { CLOCK+=1;  A = D; break; }
				case 4  : /* MTA A E */ { CLOCK+=1;  A = E; break; }
				case 5  : /* MTA A H */ { CLOCK+=1;  A = H; break; }
				case 6  : /* MTA A L */ { CLOCK+=1;  A = L; break; }
				case 7  : /* MTA A N */ { CLOCK+=1; var X = N; A = X; break; }
				case 8  : /* MTA B A */ { CLOCK+=1;  B = A; break; }
				case 9  : /* RET     */ { CLOCK+=4; H = MEM[(ushort)(SP++)]; L = MEM[(ushort)(SP++)]; IP = HL; break; }
				case 10 : /* MTA B C */ { CLOCK+=1;  B = C; break; }
				case 11 : /* MTA B D */ { CLOCK+=1;  B = D; break; }
				case 12 : /* MTA B E */ { CLOCK+=1;  B = E; break; }
				case 13 : /* MTA B H */ { CLOCK+=1;  B = H; break; }
				case 14 : /* MTA B L */ { CLOCK+=1;  B = L; break; }
				case 15 : /* MTA B N */ { CLOCK+=1; var X = N; B = X; break; }
				case 16 : /* SSR     */ { CLOCK+=1; A >>= B; break; }
				case 17 : /* SSL     */ { CLOCK+=1; A <<= B; break; }
				case 18 : /* MFA A C */ { CLOCK+=1;  C = A; break; }
				case 19 : /* MFA A D */ { CLOCK+=1;  D = A; break; }
				case 20 : /* MFA A E */ { CLOCK+=1;  E = A; break; }
				case 21 : /* MFA A H */ { CLOCK+=1;  H = A; break; }
				case 22 : /* MFA A L */ { CLOCK+=1;  L = A; break; }
				case 23 : /* NOT A   */ { CLOCK+=2; A = (byte)~A; break; }
				case 24 : /* IAQ     */ { CLOCK+=1; IQ = HL; break; }
				case 25 : /* RFQ     */ { CLOCK+=1; RFQ(); break; }
				case 26 : /* MFA B C */ { CLOCK+=1;  C = B; break; }
				case 27 : /* MFA B D */ { CLOCK+=1;  D = B; break; }
				case 28 : /* MFA B E */ { CLOCK+=1;  E = B; break; }
				case 29 : /* MFA B H */ { CLOCK+=1;  H = B; break; }
				case 30 : /* MFA B L */ { CLOCK+=1;  L = B; break; }
				case 31 : /* NOT B   */ { CLOCK+=2; B = (byte)~B; break; }
				case 32 : /* ADD A A */ { CLOCK+=2;  var Y = A; A += A; O = (byte)((Y + A) >> 8); break; }
				case 33 : /* ADD A B */ { CLOCK+=2;  var Y = A; A += B; O = (byte)((Y + B) >> 8); break; }
				case 34 : /* ADD A C */ { CLOCK+=2;  var Y = A; A += C; O = (byte)((Y + C) >> 8); break; }
				case 35 : /* ADD A D */ { CLOCK+=2;  var Y = A; A += D; O = (byte)((Y + D) >> 8); break; }
				case 36 : /* ADD A E */ { CLOCK+=2;  var Y = A; A += E; O = (byte)((Y + E) >> 8); break; }
				case 37 : /* ADD A H */ { CLOCK+=2;  var Y = A; A += H; O = (byte)((Y + H) >> 8); break; }
				case 38 : /* ADD A L */ { CLOCK+=2;  var Y = A; A += L; O = (byte)((Y + L) >> 8); break; }
				case 39 : /* ADD A N */ { CLOCK+=2; var X = N; var Y = A; A += X; O = (byte)((Y + X) >> 8); break; }
				case 40 : /* ADD B A */ { CLOCK+=2;  var Y = B; B += A; O = (byte)((Y + A) >> 8); break; }
				case 41 : /* ADD B B */ { CLOCK+=2;  var Y = B; B += B; O = (byte)((Y + B) >> 8); break; }
				case 42 : /* ADD B C */ { CLOCK+=2;  var Y = B; B += C; O = (byte)((Y + C) >> 8); break; }
				case 43 : /* ADD B D */ { CLOCK+=2;  var Y = B; B += D; O = (byte)((Y + D) >> 8); break; }
				case 44 : /* ADD B E */ { CLOCK+=2;  var Y = B; B += E; O = (byte)((Y + E) >> 8); break; }
				case 45 : /* ADD B H */ { CLOCK+=2;  var Y = B; B += H; O = (byte)((Y + H) >> 8); break; }
				case 46 : /* ADD B L */ { CLOCK+=2;  var Y = B; B += L; O = (byte)((Y + L) >> 8); break; }
				case 47 : /* ADD B N */ { CLOCK+=2; var X = N; var Y = B; B += X; O = (byte)((Y + X) >> 8); break; }
				case 48 : /* SUB A A */ { CLOCK+=2;  var Y = A; A -= A; O = (byte)((Y << 8) - (A << 8)); break; }
				case 49 : /* SUB A B */ { CLOCK+=2;  var Y = A; A -= B; O = (byte)((Y << 8) - (B << 8)); break; }
				case 50 : /* SUB A C */ { CLOCK+=2;  var Y = A; A -= C; O = (byte)((Y << 8) - (C << 8)); break; }
				case 51 : /* SUB A D */ { CLOCK+=2;  var Y = A; A -= D; O = (byte)((Y << 8) - (D << 8)); break; }
				case 52 : /* SUB A E */ { CLOCK+=2;  var Y = A; A -= E; O = (byte)((Y << 8) - (E << 8)); break; }
				case 53 : /* SUB A H */ { CLOCK+=2;  var Y = A; A -= H; O = (byte)((Y << 8) - (H << 8)); break; }
				case 54 : /* SUB A L */ { CLOCK+=2;  var Y = A; A -= L; O = (byte)((Y << 8) - (L << 8)); break; }
				case 55 : /* SUB A N */ { CLOCK+=2; var X = N; var Y = A; A -= X; O = (byte)((Y << 8) - (X << 8)); break; }
				case 56 : /* SUB B A */ { CLOCK+=2;  var Y = B; B -= A; O = (byte)((Y << 8) - (A << 8)); break; }
				case 57 : /* SUB B B */ { CLOCK+=2;  var Y = B; B -= B; O = (byte)((Y << 8) - (B << 8)); break; }
				case 58 : /* SUB B C */ { CLOCK+=2;  var Y = B; B -= C; O = (byte)((Y << 8) - (C << 8)); break; }
				case 59 : /* SUB B D */ { CLOCK+=2;  var Y = B; B -= D; O = (byte)((Y << 8) - (D << 8)); break; }
				case 60 : /* SUB B E */ { CLOCK+=2;  var Y = B; B -= E; O = (byte)((Y << 8) - (E << 8)); break; }
				case 61 : /* SUB B H */ { CLOCK+=2;  var Y = B; B -= H; O = (byte)((Y << 8) - (H << 8)); break; }
				case 62 : /* SUB B L */ { CLOCK+=2;  var Y = B; B -= L; O = (byte)((Y << 8) - (L << 8)); break; }
				case 63 : /* SUB B N */ { CLOCK+=2; var X = N; var Y = B; B -= X; O = (byte)((Y << 8) - (X << 8)); break; }
				case 64 : /* MUL A A */ { CLOCK+=8;  var Y = A; A *= A; O = (byte)((Y * A) >> 8); break; }
				case 65 : /* MUL A B */ { CLOCK+=8;  var Y = A; A *= B; O = (byte)((Y * B) >> 8); break; }
				case 66 : /* MUL A C */ { CLOCK+=8;  var Y = A; A *= C; O = (byte)((Y * C) >> 8); break; }
				case 67 : /* MUL A D */ { CLOCK+=8;  var Y = A; A *= D; O = (byte)((Y * D) >> 8); break; }
				case 68 : /* MUL A E */ { CLOCK+=8;  var Y = A; A *= E; O = (byte)((Y * E) >> 8); break; }
				case 69 : /* MUL A H */ { CLOCK+=8;  var Y = A; A *= H; O = (byte)((Y * H) >> 8); break; }
				case 70 : /* MUL A L */ { CLOCK+=8;  var Y = A; A *= L; O = (byte)((Y * L) >> 8); break; }
				case 71 : /* MUL A N */ { CLOCK+=8; var X = N; var Y = A; A *= X; O = (byte)((Y * X) >> 8); break; }
				case 72 : /* MUL B A */ { CLOCK+=8;  var Y = B; B *= A; O = (byte)((Y * A) >> 8); break; }
				case 73 : /* MUL B B */ { CLOCK+=8;  var Y = B; B *= B; O = (byte)((Y * B) >> 8); break; }
				case 74 : /* MUL B C */ { CLOCK+=8;  var Y = B; B *= C; O = (byte)((Y * C) >> 8); break; }
				case 75 : /* MUL B D */ { CLOCK+=8;  var Y = B; B *= D; O = (byte)((Y * D) >> 8); break; }
				case 76 : /* MUL B E */ { CLOCK+=8;  var Y = B; B *= E; O = (byte)((Y * E) >> 8); break; }
				case 77 : /* MUL B H */ { CLOCK+=8;  var Y = B; B *= H; O = (byte)((Y * H) >> 8); break; }
				case 78 : /* MUL B L */ { CLOCK+=8;  var Y = B; B *= L; O = (byte)((Y * L) >> 8); break; }
				case 79 : /* MUL B N */ { CLOCK+=8; var X = N; var Y = B; B *= X; O = (byte)((Y * X) >> 8); break; }
				case 80 : /* DIV A A */ { CLOCK+=32;  if (A == 0) DIV_ZERO(); A /= A; O = 0; break; }
				case 81 : /* DIV A B */ { CLOCK+=32;  if (B == 0) DIV_ZERO(); A /= B; O = 0; break; }
				case 82 : /* DIV A C */ { CLOCK+=32;  if (C == 0) DIV_ZERO(); A /= C; O = 0; break; }
				case 83 : /* DIV A D */ { CLOCK+=32;  if (D == 0) DIV_ZERO(); A /= D; O = 0; break; }
				case 84 : /* DIV A E */ { CLOCK+=32;  if (E == 0) DIV_ZERO(); A /= E; O = 0; break; }
				case 85 : /* DIV A H */ { CLOCK+=32;  if (H == 0) DIV_ZERO(); A /= H; O = 0; break; }
				case 86 : /* DIV A L */ { CLOCK+=32;  if (L == 0) DIV_ZERO(); A /= L; O = 0; break; }
				case 87 : /* DIV A N */ { CLOCK+=32; var X = N; if (X == 0) DIV_ZERO(); A /= X; O = 0; break; }
				case 88 : /* DIV B A */ { CLOCK+=32;  if (A == 0) DIV_ZERO(); B /= A; O = 0; break; }
				case 89 : /* DIV B B */ { CLOCK+=32;  if (B == 0) DIV_ZERO(); B /= B; O = 0; break; }
				case 90 : /* DIV B C */ { CLOCK+=32;  if (C == 0) DIV_ZERO(); B /= C; O = 0; break; }
				case 91 : /* DIV B D */ { CLOCK+=32;  if (D == 0) DIV_ZERO(); B /= D; O = 0; break; }
				case 92 : /* DIV B E */ { CLOCK+=32;  if (E == 0) DIV_ZERO(); B /= E; O = 0; break; }
				case 93 : /* DIV B H */ { CLOCK+=32;  if (H == 0) DIV_ZERO(); B /= H; O = 0; break; }
				case 94 : /* DIV B L */ { CLOCK+=32;  if (L == 0) DIV_ZERO(); B /= L; O = 0; break; }
				case 95 : /* DIV B N */ { CLOCK+=32; var X = N; if (X == 0) DIV_ZERO(); B /= X; O = 0; break; }
				case 96 : /* MOD A A */ { CLOCK+=32;  if (A == 0) DIV_ZERO(); A %= A; O = 0; break; }
				case 97 : /* MOD A B */ { CLOCK+=32;  if (B == 0) DIV_ZERO(); A %= B; O = 0; break; }
				case 98 : /* MOD A C */ { CLOCK+=32;  if (C == 0) DIV_ZERO(); A %= C; O = 0; break; }
				case 99 : /* MOD A D */ { CLOCK+=32;  if (D == 0) DIV_ZERO(); A %= D; O = 0; break; }
				case 100: /* MOD A E */ { CLOCK+=32;  if (E == 0) DIV_ZERO(); A %= E; O = 0; break; }
				case 101: /* MOD A H */ { CLOCK+=32;  if (H == 0) DIV_ZERO(); A %= H; O = 0; break; }
				case 102: /* MOD A L */ { CLOCK+=32;  if (L == 0) DIV_ZERO(); A %= L; O = 0; break; }
				case 103: /* MOD A N */ { CLOCK+=32; var X = N; if (X == 0) DIV_ZERO(); A %= X; O = 0; break; }
				case 104: /* MOD B A */ { CLOCK+=32;  if (A == 0) DIV_ZERO(); B %= A; O = 0; break; }
				case 105: /* MOD B B */ { CLOCK+=32;  if (B == 0) DIV_ZERO(); B %= B; O = 0; break; }
				case 106: /* MOD B C */ { CLOCK+=32;  if (C == 0) DIV_ZERO(); B %= C; O = 0; break; }
				case 107: /* MOD B D */ { CLOCK+=32;  if (D == 0) DIV_ZERO(); B %= D; O = 0; break; }
				case 108: /* MOD B E */ { CLOCK+=32;  if (E == 0) DIV_ZERO(); B %= E; O = 0; break; }
				case 109: /* MOD B H */ { CLOCK+=32;  if (H == 0) DIV_ZERO(); B %= H; O = 0; break; }
				case 110: /* MOD B L */ { CLOCK+=32;  if (L == 0) DIV_ZERO(); B %= L; O = 0; break; }
				case 111: /* MOD B N */ { CLOCK+=32; var X = N; if (X == 0) DIV_ZERO(); B %= X; O = 0; break; }
				case 112: /* OVE A   */ { CLOCK+=1; A = O; break; }
				case 113: /* AND A B */ { CLOCK+=2;  A &= B; O = 0; break; }
				case 114: /* AND A C */ { CLOCK+=2;  A &= C; O = 0; break; }
				case 115: /* AND A D */ { CLOCK+=2;  A &= D; O = 0; break; }
				case 116: /* AND A E */ { CLOCK+=2;  A &= E; O = 0; break; }
				case 117: /* AND A H */ { CLOCK+=2;  A &= H; O = 0; break; }
				case 118: /* AND A L */ { CLOCK+=2;  A &= L; O = 0; break; }
				case 119: /* AND A N */ { CLOCK+=2; var X = N; A &= X; O = 0; break; }
				case 120: /* AND B A */ { CLOCK+=2;  B &= A; O = 0; break; }
				case 121: /* OVE B   */ { CLOCK+=1; B = O; break; }
				case 122: /* AND B C */ { CLOCK+=2;  B &= C; O = 0; break; }
				case 123: /* AND B D */ { CLOCK+=2;  B &= D; O = 0; break; }
				case 124: /* AND B E */ { CLOCK+=2;  B &= E; O = 0; break; }
				case 125: /* AND B H */ { CLOCK+=2;  B &= H; O = 0; break; }
				case 126: /* AND B L */ { CLOCK+=2;  B &= L; O = 0; break; }
				case 127: /* AND B N */ { CLOCK+=2; var X = N; B &= X; O = 0; break; }
				case 128: /* MUS     */ { CLOCK+=8; var Y = B; B = (byte)((sbyte)A * (sbyte)Y); O = (byte)(((sbyte)A * (sbyte)Y) >> 8); break; }
				case 129: /* BOR A B */ { CLOCK+=2;  A |= B; O = 0; break; }
				case 130: /* BOR A C */ { CLOCK+=2;  A |= C; O = 0; break; }
				case 131: /* BOR A D */ { CLOCK+=2;  A |= D; O = 0; break; }
				case 132: /* BOR A E */ { CLOCK+=2;  A |= E; O = 0; break; }
				case 133: /* BOR A H */ { CLOCK+=2;  A |= H; O = 0; break; }
				case 134: /* BOR A L */ { CLOCK+=2;  A |= L; O = 0; break; }
				case 135: /* BOR A N */ { CLOCK+=2; var X = N; A |= X; O = 0; break; }
				case 136: /* BOR B A */ { CLOCK+=2;  B |= A; O = 0; break; }
				case 137: /* DIS     */ { CLOCK+=32; if (B == 0) DIV_ZERO(); B = (byte)((sbyte)A / (sbyte)B); O = 0; break; }
				case 138: /* BOR B C */ { CLOCK+=2;  B |= C; O = 0; break; }
				case 139: /* BOR B D */ { CLOCK+=2;  B |= D; O = 0; break; }
				case 140: /* BOR B E */ { CLOCK+=2;  B |= E; O = 0; break; }
				case 141: /* BOR B H */ { CLOCK+=2;  B |= H; O = 0; break; }
				case 142: /* BOR B L */ { CLOCK+=2;  B |= L; O = 0; break; }
				case 143: /* BOR B N */ { CLOCK+=2; var X = N; B |= X; O = 0; break; }
				case 144: /* XOR A A */ { CLOCK+=2;  A ^= A; O = 0; break; }
				case 145: /* XOR A B */ { CLOCK+=2;  A ^= B; O = 0; break; }
				case 146: /* XOR A C */ { CLOCK+=2;  A ^= C; O = 0; break; }
				case 147: /* XOR A D */ { CLOCK+=2;  A ^= D; O = 0; break; }
				case 148: /* XOR A E */ { CLOCK+=2;  A ^= E; O = 0; break; }
				case 149: /* XOR A H */ { CLOCK+=2;  A ^= H; O = 0; break; }
				case 150: /* XOR A L */ { CLOCK+=2;  A ^= L; O = 0; break; }
				case 151: /* XOR A N */ { CLOCK+=2; var X = N; A ^= X; O = 0; break; }
				case 152: /* XOR B A */ { CLOCK+=2;  B ^= A; O = 0; break; }
				case 153: /* XOR B B */ { CLOCK+=2;  B ^= B; O = 0; break; }
				case 154: /* XOR B C */ { CLOCK+=2;  B ^= C; O = 0; break; }
				case 155: /* XOR B D */ { CLOCK+=2;  B ^= D; O = 0; break; }
				case 156: /* XOR B E */ { CLOCK+=2;  B ^= E; O = 0; break; }
				case 157: /* XOR B H */ { CLOCK+=2;  B ^= H; O = 0; break; }
				case 158: /* XOR B L */ { CLOCK+=2;  B ^= L; O = 0; break; }
				case 159: /* XOR B N */ { CLOCK+=2; var X = N; B ^= X; O = 0; break; }
				case 160: /* PSH A   */ { CLOCK+=4; MEM[(ushort)(--SP)] = A; break; }
				case 161: /* PSH B   */ { CLOCK+=4; MEM[(ushort)(--SP)] = B; break; }
				case 162: /* PSH C   */ { CLOCK+=4; MEM[(ushort)(--SP)] = C; break; }
				case 163: /* PSH D   */ { CLOCK+=4; MEM[(ushort)(--SP)] = D; break; }
				case 164: /* PSH E   */ { CLOCK+=4; MEM[(ushort)(--SP)] = E; break; }
				case 165: /* PSH H   */ { CLOCK+=4; MEM[(ushort)(--SP)] = H; break; }
				case 166: /* PSH L   */ { CLOCK+=4; MEM[(ushort)(--SP)] = L; break; }
				case 167: /* PSH N   */ { CLOCK+=4; MEM[(ushort)(--SP)] = N; break; }
				case 168: /* POP A   */ { CLOCK+=4; A = MEM[(ushort)(SP++)]; break; }
				case 169: /* POP B   */ { CLOCK+=4; B = MEM[(ushort)(SP++)]; break; }
				case 170: /* POP C   */ { CLOCK+=4; C = MEM[(ushort)(SP++)]; break; }
				case 171: /* POP D   */ { CLOCK+=4; D = MEM[(ushort)(SP++)]; break; }
				case 172: /* POP E   */ { CLOCK+=4; E = MEM[(ushort)(SP++)]; break; }
				case 173: /* POP H   */ { CLOCK+=4; H = MEM[(ushort)(SP++)]; break; }
				case 174: /* POP L   */ { CLOCK+=4; L = MEM[(ushort)(SP++)]; break; }
				case 175: /* RSP     */ { CLOCK+=2; H = (byte)(SP >> 8); L = (byte)SP; break; }
				case 176: /* PEK A   */ { CLOCK+=2; A = MEM[SP]; break; }
				case 177: /* PEK B   */ { CLOCK+=2; B = MEM[SP]; break; }
				case 178: /* PEK C   */ { CLOCK+=2; C = MEM[SP]; break; }
				case 179: /* PEK D   */ { CLOCK+=2; D = MEM[SP]; break; }
				case 180: /* PEK E   */ { CLOCK+=2; E = MEM[SP]; break; }
				case 181: /* PEK H   */ { CLOCK+=2; H = MEM[SP]; break; }
				case 182: /* PEK L   */ { CLOCK+=2; L = MEM[SP]; break; }
				case 183: /* SSP     */ { CLOCK+=2; SP = HL; break; }
				case 184: /* LOD A   */ { CLOCK+=8; A = MEM[HL]; break; }
				case 185: /* LOD B   */ { CLOCK+=8; B = MEM[HL]; break; }
				case 186: /* LOD C   */ { CLOCK+=8; C = MEM[HL]; break; }
				case 187: /* LOD D   */ { CLOCK+=8; D = MEM[HL]; break; }
				case 188: /* LOD E   */ { CLOCK+=8; E = MEM[HL]; break; }
				case 189: /* LOD H   */ { CLOCK+=8; H = MEM[HL]; break; }
				case 190: /* LOD L   */ { CLOCK+=8; L = MEM[HL]; break; }
				case 191: /* LLH     */ { CLOCK+=2; H = N; L = N; break; }
				case 192: /* STR A   */ { CLOCK+=8; MEM[HL] = A; break; }
				case 193: /* STR B   */ { CLOCK+=8; MEM[HL] = B; break; }
				case 194: /* STR C   */ { CLOCK+=8; MEM[HL] = C; break; }
				case 195: /* STR D   */ { CLOCK+=8; MEM[HL] = D; break; }
				case 196: /* STR E   */ { CLOCK+=8; MEM[HL] = E; break; }
				case 197: /* STR H   */ { CLOCK+=8; MEM[HL] = H; break; }
				case 198: /* STR L   */ { CLOCK+=8; MEM[HL] = L; break; }
				case 199: /* STR N   */ { CLOCK+=8; MEM[HL] = N; break; }
				case 200: /* LDW     */ { CLOCK+=12; if (HL%2 == 1) ALIGN_FAULT(); A = MEM[HL]; B = MEM[HL + 1]; break; }
				case 201: /* SDW     */ { CLOCK+=12; if (HL%2 == 1) ALIGN_FAULT(); MEM[HL] = A; MEM[HL + 1] = B; break; }
				case 202: /* STP     */ { CLOCK+=1; STOP(); break; }
				case 203: /* CFP     */ { CLOCK+=2; H = D; L = E; break; }
				case 204: /* SWP     */ { CLOCK+=2; var T = D; D = H; H = T; T = E; E = L; L = T; break; }
				case 205: /* RIP     */ { CLOCK+=2; H = (byte)(IP >> 8); L = (byte)IP; break; }
				case 206: /* JMP     */ { CLOCK+=2; IP = HL; break; }
				case 207: /* JPL     */ { CLOCK+=2; IP = (ushort)((N << 8) + N); break; }
				case 208: /* BIE     */ { CLOCK+=8; if (A == B) IP = HL; break; }
				case 209: /* BNE     */ { CLOCK+=8; if (A != B) IP = HL; break; }
				case 210: /* BGT     */ { CLOCK+=8; if (A > B) IP = HL; break; }
				case 211: /* BLT     */ { CLOCK+=8; if (A < B) IP = HL; break; }
				case 212: /* BEG     */ { CLOCK+=8; if (A >= B) IP = HL; break; }
				case 213: /* BEL     */ { CLOCK+=8; if (A <= B) IP = HL; break; }
				case 214: /* BSL     */ { CLOCK+=8; if ((sbyte)A < (sbyte)B) IP = HL; break; }
				case 215: /* BSG     */ { CLOCK+=8; if ((sbyte)A > (sbyte)B) IP = HL; break; }
				case 216: /* CAD A   */ { CLOCK+=4; var T = HL; T += A; L = (byte)T; H = (byte)(T >> 8); break; }
				case 217: /* CAD B   */ { CLOCK+=4; var T = HL; T += B; L = (byte)T; H = (byte)(T >> 8); break; }
				case 218: /* CAD C   */ { CLOCK+=4; var T = HL; T += C; L = (byte)T; H = (byte)(T >> 8); break; }
				case 219: /* CAD D   */ { CLOCK+=4; var T = HL; T += D; L = (byte)T; H = (byte)(T >> 8); break; }
				case 220: /* CAD E   */ { CLOCK+=4; var T = HL; T += E; L = (byte)T; H = (byte)(T >> 8); break; }
				case 221: /* CAD H   */ { CLOCK+=4; var T = HL; T += H; L = (byte)T; H = (byte)(T >> 8); break; }
				case 222: /* CAD L   */ { CLOCK+=4; var T = HL; T += L; L = (byte)T; H = (byte)(T >> 8); break; }
				case 223: /* CAD N   */ { CLOCK+=4; var T = HL; T += N; L = (byte)T; H = (byte)(T >> 8); break; }
				case 224: /* CSB A   */ { CLOCK+=4; var T = HL; T -= A; L = (byte)T; H = (byte)(T >> 8); break; }
				case 225: /* CSB B   */ { CLOCK+=4; var T = HL; T -= B; L = (byte)T; H = (byte)(T >> 8); break; }
				case 226: /* CSB C   */ { CLOCK+=4; var T = HL; T -= C; L = (byte)T; H = (byte)(T >> 8); break; }
				case 227: /* CSB D   */ { CLOCK+=4; var T = HL; T -= D; L = (byte)T; H = (byte)(T >> 8); break; }
				case 228: /* CSB E   */ { CLOCK+=4; var T = HL; T -= E; L = (byte)T; H = (byte)(T >> 8); break; }
				case 229: /* CSB H   */ { CLOCK+=4; var T = HL; T -= H; L = (byte)T; H = (byte)(T >> 8); break; }
				case 230: /* CSB L   */ { CLOCK+=4; var T = HL; T -= L; L = (byte)T; H = (byte)(T >> 8); break; }
				case 231: /* CSB N   */ { CLOCK+=4; var T = HL; T -= N; L = (byte)T; H = (byte)(T >> 8); break; }
				case 232: /* ADS A   */ { CLOCK+=4; SP += A; break; }
				case 233: /* ADS B   */ { CLOCK+=4; SP += B; break; }
				case 234: /* ADS C   */ { CLOCK+=4; SP += C; break; }
				case 235: /* ADS D   */ { CLOCK+=4; SP += D; break; }
				case 236: /* ADS E   */ { CLOCK+=4; SP += E; break; }
				case 237: /* ADS H   */ { CLOCK+=4; SP += H; break; }
				case 238: /* ADS L   */ { CLOCK+=4; SP += L; break; }
				case 239: /* ADS N   */ { CLOCK+=4; SP += N; break; }
				case 240: /* SBS A   */ { CLOCK+=4; SP -= A; break; }
				case 241: /* SBS B   */ { CLOCK+=4; SP -= B; break; }
				case 242: /* SBS C   */ { CLOCK+=4; SP -= C; break; }
				case 243: /* SBS D   */ { CLOCK+=4; SP -= D; break; }
				case 244: /* SBS E   */ { CLOCK+=4; SP -= E; break; }
				case 245: /* SBS H   */ { CLOCK+=4; SP -= H; break; }
				case 246: /* SBS L   */ { CLOCK+=4; SP -= L; break; }
				case 247: /* SBS N   */ { CLOCK+=4; SP -= N; break; }
				case 248: /* OUT     */ { CLOCK+=2; PORT[A] = B; if (HARDWARE[A] != null) HARDWARE[A].PortWritten(A, B); break; }
				case 249: /* IIN     */ { CLOCK+=2; B = PORT[A]; break; }
				case 250: /* LLD     */ { CLOCK+=1; D = N; E = N; break; }
				case 251: /* JIG     */ { CLOCK+=1; if (HL >= DE) { IP = (ushort)((N << 8) + N); } else { IP += 2; } break; }
				case 252: /* NPG     */ { CLOCK+=1;  break; }
				case 253: /* CLK     */ { CLOCK+=4; D = (byte)(CLOCK>>24); E = (byte)(CLOCK>>16); H = (byte)(CLOCK>>8); L = (byte)CLOCK; break; }
				case 254: /* SFJ     */ { CLOCK+=2; IP += N; break; }
				case 255: /* SBJ     */ { CLOCK+=2; IP -= N; break; }

			}

			return (uint)(CLOCK - startCLOCK);
		}

		public void ALIGN_FAULT()
		{
			STATE_FLAGS |= 0x01;
		}

		public void STOP()
		{
			STATE_FLAGS |= 0x02;
		}

		public void DIV_ZERO()
		{
			STATE_FLAGS |= 0x04;
		}

		public static Dictionary<String, byte> GetEncodingTable()
		{
			var r = new Dictionary<String, byte>();

				r.Add("CAL", 0);
				r.Add("MTA A B", 1);
				r.Add("MTA A C", 2);
				r.Add("MTA A D", 3);
				r.Add("MTA A E", 4);
				r.Add("MTA A H", 5);
				r.Add("MTA A L", 6);
				r.Add("MTA A N", 7);
				r.Add("MTA B A", 8);
				r.Add("RET", 9);
				r.Add("MTA B C", 10);
				r.Add("MTA B D", 11);
				r.Add("MTA B E", 12);
				r.Add("MTA B H", 13);
				r.Add("MTA B L", 14);
				r.Add("MTA B N", 15);
				r.Add("SSR", 16);
				r.Add("SSL", 17);
				r.Add("MFA A C", 18);
				r.Add("MFA A D", 19);
				r.Add("MFA A E", 20);
				r.Add("MFA A H", 21);
				r.Add("MFA A L", 22);
				r.Add("NOT A", 23);
				r.Add("IAQ", 24);
				r.Add("RFQ", 25);
				r.Add("MFA B C", 26);
				r.Add("MFA B D", 27);
				r.Add("MFA B E", 28);
				r.Add("MFA B H", 29);
				r.Add("MFA B L", 30);
				r.Add("NOT B", 31);
				r.Add("ADD A A", 32);
				r.Add("ADD A B", 33);
				r.Add("ADD A C", 34);
				r.Add("ADD A D", 35);
				r.Add("ADD A E", 36);
				r.Add("ADD A H", 37);
				r.Add("ADD A L", 38);
				r.Add("ADD A N", 39);
				r.Add("ADD B A", 40);
				r.Add("ADD B B", 41);
				r.Add("ADD B C", 42);
				r.Add("ADD B D", 43);
				r.Add("ADD B E", 44);
				r.Add("ADD B H", 45);
				r.Add("ADD B L", 46);
				r.Add("ADD B N", 47);
				r.Add("SUB A A", 48);
				r.Add("SUB A B", 49);
				r.Add("SUB A C", 50);
				r.Add("SUB A D", 51);
				r.Add("SUB A E", 52);
				r.Add("SUB A H", 53);
				r.Add("SUB A L", 54);
				r.Add("SUB A N", 55);
				r.Add("SUB B A", 56);
				r.Add("SUB B B", 57);
				r.Add("SUB B C", 58);
				r.Add("SUB B D", 59);
				r.Add("SUB B E", 60);
				r.Add("SUB B H", 61);
				r.Add("SUB B L", 62);
				r.Add("SUB B N", 63);
				r.Add("MUL A A", 64);
				r.Add("MUL A B", 65);
				r.Add("MUL A C", 66);
				r.Add("MUL A D", 67);
				r.Add("MUL A E", 68);
				r.Add("MUL A H", 69);
				r.Add("MUL A L", 70);
				r.Add("MUL A N", 71);
				r.Add("MUL B A", 72);
				r.Add("MUL B B", 73);
				r.Add("MUL B C", 74);
				r.Add("MUL B D", 75);
				r.Add("MUL B E", 76);
				r.Add("MUL B H", 77);
				r.Add("MUL B L", 78);
				r.Add("MUL B N", 79);
				r.Add("DIV A A", 80);
				r.Add("DIV A B", 81);
				r.Add("DIV A C", 82);
				r.Add("DIV A D", 83);
				r.Add("DIV A E", 84);
				r.Add("DIV A H", 85);
				r.Add("DIV A L", 86);
				r.Add("DIV A N", 87);
				r.Add("DIV B A", 88);
				r.Add("DIV B B", 89);
				r.Add("DIV B C", 90);
				r.Add("DIV B D", 91);
				r.Add("DIV B E", 92);
				r.Add("DIV B H", 93);
				r.Add("DIV B L", 94);
				r.Add("DIV B N", 95);
				r.Add("MOD A A", 96);
				r.Add("MOD A B", 97);
				r.Add("MOD A C", 98);
				r.Add("MOD A D", 99);
				r.Add("MOD A E", 100);
				r.Add("MOD A H", 101);
				r.Add("MOD A L", 102);
				r.Add("MOD A N", 103);
				r.Add("MOD B A", 104);
				r.Add("MOD B B", 105);
				r.Add("MOD B C", 106);
				r.Add("MOD B D", 107);
				r.Add("MOD B E", 108);
				r.Add("MOD B H", 109);
				r.Add("MOD B L", 110);
				r.Add("MOD B N", 111);
				r.Add("OVE A", 112);
				r.Add("AND A B", 113);
				r.Add("AND A C", 114);
				r.Add("AND A D", 115);
				r.Add("AND A E", 116);
				r.Add("AND A H", 117);
				r.Add("AND A L", 118);
				r.Add("AND A N", 119);
				r.Add("AND B A", 120);
				r.Add("OVE B", 121);
				r.Add("AND B C", 122);
				r.Add("AND B D", 123);
				r.Add("AND B E", 124);
				r.Add("AND B H", 125);
				r.Add("AND B L", 126);
				r.Add("AND B N", 127);
				r.Add("MUS", 128);
				r.Add("BOR A B", 129);
				r.Add("BOR A C", 130);
				r.Add("BOR A D", 131);
				r.Add("BOR A E", 132);
				r.Add("BOR A H", 133);
				r.Add("BOR A L", 134);
				r.Add("BOR A N", 135);
				r.Add("BOR B A", 136);
				r.Add("DIS", 137);
				r.Add("BOR B C", 138);
				r.Add("BOR B D", 139);
				r.Add("BOR B E", 140);
				r.Add("BOR B H", 141);
				r.Add("BOR B L", 142);
				r.Add("BOR B N", 143);
				r.Add("XOR A A", 144);
				r.Add("XOR A B", 145);
				r.Add("XOR A C", 146);
				r.Add("XOR A D", 147);
				r.Add("XOR A E", 148);
				r.Add("XOR A H", 149);
				r.Add("XOR A L", 150);
				r.Add("XOR A N", 151);
				r.Add("XOR B A", 152);
				r.Add("XOR B B", 153);
				r.Add("XOR B C", 154);
				r.Add("XOR B D", 155);
				r.Add("XOR B E", 156);
				r.Add("XOR B H", 157);
				r.Add("XOR B L", 158);
				r.Add("XOR B N", 159);
				r.Add("PSH A", 160);
				r.Add("PSH B", 161);
				r.Add("PSH C", 162);
				r.Add("PSH D", 163);
				r.Add("PSH E", 164);
				r.Add("PSH H", 165);
				r.Add("PSH L", 166);
				r.Add("PSH N", 167);
				r.Add("POP A", 168);
				r.Add("POP B", 169);
				r.Add("POP C", 170);
				r.Add("POP D", 171);
				r.Add("POP E", 172);
				r.Add("POP H", 173);
				r.Add("POP L", 174);
				r.Add("RSP", 175);
				r.Add("PEK A", 176);
				r.Add("PEK B", 177);
				r.Add("PEK C", 178);
				r.Add("PEK D", 179);
				r.Add("PEK E", 180);
				r.Add("PEK H", 181);
				r.Add("PEK L", 182);
				r.Add("SSP", 183);
				r.Add("LOD A", 184);
				r.Add("LOD B", 185);
				r.Add("LOD C", 186);
				r.Add("LOD D", 187);
				r.Add("LOD E", 188);
				r.Add("LOD H", 189);
				r.Add("LOD L", 190);
				r.Add("LLH", 191);
				r.Add("STR A", 192);
				r.Add("STR B", 193);
				r.Add("STR C", 194);
				r.Add("STR D", 195);
				r.Add("STR E", 196);
				r.Add("STR H", 197);
				r.Add("STR L", 198);
				r.Add("STR N", 199);
				r.Add("LDW", 200);
				r.Add("SDW", 201);
				r.Add("STP", 202);
				r.Add("CFP", 203);
				r.Add("SWP", 204);
				r.Add("RIP", 205);
				r.Add("JMP", 206);
				r.Add("JPL", 207);
				r.Add("BIE", 208);
				r.Add("BNE", 209);
				r.Add("BGT", 210);
				r.Add("BLT", 211);
				r.Add("BEG", 212);
				r.Add("BEL", 213);
				r.Add("BSL", 214);
				r.Add("BSG", 215);
				r.Add("CAD A", 216);
				r.Add("CAD B", 217);
				r.Add("CAD C", 218);
				r.Add("CAD D", 219);
				r.Add("CAD E", 220);
				r.Add("CAD H", 221);
				r.Add("CAD L", 222);
				r.Add("CAD N", 223);
				r.Add("CSB A", 224);
				r.Add("CSB B", 225);
				r.Add("CSB C", 226);
				r.Add("CSB D", 227);
				r.Add("CSB E", 228);
				r.Add("CSB H", 229);
				r.Add("CSB L", 230);
				r.Add("CSB N", 231);
				r.Add("ADS A", 232);
				r.Add("ADS B", 233);
				r.Add("ADS C", 234);
				r.Add("ADS D", 235);
				r.Add("ADS E", 236);
				r.Add("ADS H", 237);
				r.Add("ADS L", 238);
				r.Add("ADS N", 239);
				r.Add("SBS A", 240);
				r.Add("SBS B", 241);
				r.Add("SBS C", 242);
				r.Add("SBS D", 243);
				r.Add("SBS E", 244);
				r.Add("SBS H", 245);
				r.Add("SBS L", 246);
				r.Add("SBS N", 247);
				r.Add("OUT", 248);
				r.Add("IIN", 249);
				r.Add("LLD", 250);
				r.Add("JIG", 251);
				r.Add("NPG", 252);
				r.Add("CLK", 253);
				r.Add("SFJ", 254);
				r.Add("SBJ", 255);


			return r;
		}
	}
}
