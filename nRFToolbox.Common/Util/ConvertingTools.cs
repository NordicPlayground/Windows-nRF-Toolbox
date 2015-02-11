using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NRFToolbox.Common
{
  public class ConvertingTools
	{
		public static string GetBLEMacAddress(ulong address)
		{
			if (address == ulong.MinValue)
				return string.Empty;

			var macadres = address.ToString("x012");
			var regex = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
			var replace = "$1:$2:$3:$4:$5:$6";
			var newformat = Regex.Replace(macadres, regex, replace);
			return newformat.ToString().ToUpper();

		}
	}
}
