/*Copyright (c) 2015, Nordic Semiconductor ASA
 *
 *Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 *
 *1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 *
 *2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other 
 *materials provided with the distribution.
 *
 *3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific 
 *prior written permission.
 *
 *THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 *PURPOSE ARE DISCLAIMED. *IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF *SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, *DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 *ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED *OF THE POSSIBILITY OF SUCH DAMAGE.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utility
{
	public static class Extension
	{
		public static T[][] Slice<T>(this T[] source, int maxSizePerGroup)
		{
			var nummberof = source.Length / maxSizePerGroup;
			var lastPacketSize = source.Length % maxSizePerGroup;
			int sizeOfNewBytes = lastPacketSize == 0 ? nummberof : nummberof + 1;
			T[][] newBytes = new T[sizeOfNewBytes][];
			int offset = 0;
			for (int i = 0; i < nummberof; i++, offset += maxSizePerGroup)
			{
				newBytes[i] = new T[maxSizePerGroup];
				Array.Copy(source, offset, newBytes[i], 0, maxSizePerGroup);
			}
			if (lastPacketSize != 0)
			{
				newBytes[sizeOfNewBytes - 1] = new T[lastPacketSize];
				Array.Copy(source, offset, newBytes[sizeOfNewBytes - 1], 0, lastPacketSize);
			}
			return newBytes;
		}
	}
}
