using System;
using System.Collections.Generic;
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
	}
}
