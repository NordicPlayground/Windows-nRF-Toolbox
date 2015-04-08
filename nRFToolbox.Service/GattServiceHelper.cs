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
using Common.Service.ApplicationException;
using Common.Service.DFUPackage;
using Common.Service.GattService;
using Common.Utility;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Runtime.Serialization.Json;
using System.IO.Compression;
using Windows.Storage.AccessCache;

namespace Common.Service
{
	public static class GattServiceHelper
	{
		public static string ToStringAddress(ulong address) 
		{
			return ConvertingTools.GetBLEMacAddress(address);
		}

		public static int GetPercentage(int number, int total) 
		{
			return ConvertingTools.Percent(number, total);
		}

		public async static Task<byte[]> ReadToBytes(StorageFile dataFile) 
		{
			return await File.ReadToBytes(dataFile);
		}
		public static async Task<StorageFile> GetDFUExampleApplication()
		{
			return await File.GetDFUExampleApplication();
		}

		public static string TryGetShortFileName(string fullName)
		{
			return ConvertingTools.TryGetShortFileName(fullName);
		}

		public static IBuffer ToIBuffer(string text)
		{
			return File.ToIBuffer(text);
		}

		public async static Task<StorageFile> GetStoredFile(string token) 
		{
			return await File.GetStoredFile(token);
		} 

		public async static Task<Dictionary<string, string>> UnzipFile(StorageFile zipFile)
		{
			return await File.UnzipFile(zipFile);
		}
	}
}
