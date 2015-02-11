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
using Windows.Storage;
using Windows.Storage.Streams;

namespace nRFToolbox.Common
{
	public class File
	{
		public static async Task<IBuffer> ReadInitialPack() 
		{
			Windows.Storage.StorageFolder installedLocation = Windows.ApplicationModel.Package.Current.InstalledLocation;
			StorageFile file2 = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/dat/blinky_s110.dat"));
			var buffer = await FileIO.ReadBufferAsync(file2);
			return buffer;
		}

		public static async Task<byte[]> ReadBinFile()
		{
			StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/dat/blinky_s110.bin"));
			var buffer = await FileIO.ReadBufferAsync(file);
			DataReader reader = DataReader.FromBuffer(buffer);
			byte[] fileContent = new byte[buffer.Length];
			reader.ReadBytes(fileContent);
			return fileContent;
		}

		public static IBuffer ToIBuffer(byte[] value) 
		{ 
			if(value == null && value.Length == 0)
				throw new ApplicationArgumentException();
			var temp = new byte[value.Length];
			Array.Copy(value, 0, temp, 0, value.Length);
			using (DataWriter writer = new DataWriter())
			{
				writer.WriteBytes(temp);
				var buffer = writer.DetachBuffer();
				return buffer;
			}
		}

		public static byte[] ToBytes(IBuffer value) 
		{
			if(value == null)
				throw new ApplicationArgumentException();
			var temp = new byte[value.Length];
			DataReader.FromBuffer(value).ReadBytes(temp);
			return temp;
		}

		public static int ToInt32(byte[] value)
		{
			if (value == null || value.Length != 4)
				throw new ApplicationArgumentException();
			var result = BitConverter.ToInt32(value, 0);
			return result;
		}

		public static byte[] ToBytes(Int16 value) 
		{
			if (value == 0 || !value.GetType().Equals(typeof(Int16)))
				throw new ApplicationArgumentException();
			var result = BitConverter.GetBytes(value);
			return result;
		}
	}
}
