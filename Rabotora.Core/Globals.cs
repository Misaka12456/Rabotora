using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Rabotora.Core 
{
	static class Globals
	{
		public static bool StandaloneMatch(this string source,string match,bool noCaseCheck = true)
		{
			if (noCaseCheck)
			{
				return Regex.IsMatch(source, $@"\b{match}\b", RegexOptions.IgnoreCase);
			}
			else
			{
				return Regex.IsMatch(source, $@"\b{match}\b");
			}
		}

		internal static class NumericsHelpers
		{
			public static int CombineHash(int n1,int n2)
			{
				return (int)CombineHash((uint)n1, (uint)n2);
			}

			public static uint CombineHash(uint u1, uint u2)
			{
				return ((u1 << 7) | (u1 >> 0x19)) ^ u2;
			}
		}
	}
}
