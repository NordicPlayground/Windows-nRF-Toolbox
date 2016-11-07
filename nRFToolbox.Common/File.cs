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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;

namespace Common.Utility
{
	public class File
	{
		public static async Task<IBuffer> ReadToBuffer(StorageFile dataFile) 
		{
			var buffer = await FileIO.ReadBufferAsync(dataFile);
			DataReader reader = DataReader.FromBuffer(buffer);
			byte[] fileContent = new byte[buffer.Length];
			reader.ReadBytes(fileContent);
			return buffer;
		}

		public static async Task<byte[]> ReadToBytes(StorageFile imageFile)
		{
			var buffer = await FileIO.ReadBufferAsync(imageFile);
			DataReader reader = DataReader.FromBuffer(buffer);
			byte[] fileContent = new byte[buffer.Length];
			reader.ReadBytes(fileContent);
			return fileContent;
		}

		public async static Task<StorageFile> GetStoredFile(string token) 
		{
			StorageFile manifest = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
			return manifest;
		}

		public static async Task<StorageFile> GetDFUExampleApplication() 
		{
			StorageFile blinkyZipExample = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/dat/DFUBlinkyExample.zip"));
			return blinkyZipExample;
		}

		public static IBuffer ToIBuffer(byte[] value) 
		{ 
			if(value == null && value.Length == 0)
				throw new ArgumentException();
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
				throw new ArgumentException();
			var temp = new byte[value.Length];
			DataReader.FromBuffer(value).ReadBytes(temp);
			return temp;
		}

		public static int ToInt32(byte[] value)
		{
			if (value == null || value.Length != 4)
				throw new ArgumentException();
			var result = BitConverter.ToInt32(value, 0);
			return result;
		}

		public static byte[] ToBytes(Int16 value) 
		{
			if (value == 0 || !value.GetType().Equals(typeof(Int16)))
				throw new ArgumentException();
			var result = BitConverter.GetBytes(value);
			return result;
		}

		public static string ToString(IBuffer buffer)
		{
			if(buffer == null)
				throw new ArgumentException();
			var result = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffer);
			return result;
		}

		public static IBuffer ToIBuffer(string text) 
		{
			if (text == null)
				throw new ArgumentException();
			IBuffer result = CryptographicBuffer.ConvertStringToBinary(text, BinaryStringEncoding.Utf8);
			return result;
		}

		public static float ToSFloat(byte[] value)
		{
			if (value.Length != 2)
				throw new ArgumentException();
			byte b0 = value[0];
			byte b1 = value[1];
			///int mantisssa
			var mantissa = unsignedToSigned(ToInt(b0) + ((ToInt(b1) & 0x0F) << 8), 12);
			var exponent = unsignedToSigned(ToInt(b1) >> 4, 4);
			return (float)(mantissa * Math.Pow(10, exponent));
		}

		private static int ToInt(byte value)
		{
			return value & 0xFF;
		}

		private static int unsignedToSigned(int unsigned, int size)
		{
			if ((unsigned & (1 << size - 1)) != 0)
			{
				unsigned = -1 * ((1 << size - 1) - (unsigned & ((1 << size - 1) - 1)));
			}
			return unsigned;
		}

		//org.bluetooth.characteristic.date_time
		public static DateTime? ToDateTime(byte[] baseTime)
		{
			if (baseTime == null && baseTime.Length != 7)
				throw new ArgumentException();
			int UINT16 = 2;
			int UINT8 = 1;
			int year;
			int month;
			int day;
			int hour;
			int minute;
			int second;
			int currentOffset = 0;
			DateTime? time = null;
			year = (ushort)((baseTime[currentOffset + 1] << 8) + baseTime[currentOffset]);
			currentOffset += UINT16;
			month = (byte)baseTime[currentOffset];
			currentOffset += UINT8;
			day = (byte)baseTime[currentOffset];
			currentOffset += UINT8;
			hour = (byte)baseTime[currentOffset];
			currentOffset += UINT8;
			minute = (byte)baseTime[currentOffset];
			currentOffset += UINT8;
			second = (byte)baseTime[currentOffset];
			if (!IsTimeValid(year, month, day, hour, minute, second))
				return time;
			time = new DateTime(year, month, day, hour, minute, second);
			return time;
		}

		private static bool IsTimeValid(int year, int month, int day, int hour, int minute, int second)
		{
			bool isValid = false;
			if (year <= 1582 || year >= 9999)
				return isValid;
			if (month <= 0  || month > 12)
				return isValid;
			if (day <= 0 || day > 31)
				return isValid;
			if (hour <= 0 || hour > 24)
				return isValid;
			if (minute < 0 || minute > 59)
				return isValid; 
			isValid = true;
			return isValid;
		}

		public static async Task<Dictionary<string, string>> UnzipFile(StorageFile zipFile)
		{
			if (zipFile == null || zipFile.FileType != ".zip")
				throw new ArgumentException();
			StorageFile file = zipFile;
			var zippedFiles = new Dictionary<string, string>();
			using (var stream = await file.OpenStreamForReadAsync())
			{
				using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read))
				{
					foreach (ZipArchiveEntry zipEntry in archive.Entries)
					{
						using (BinaryReader writer = new BinaryReader(zipEntry.Open()))
						{
							int count;
							checked
							{
								count = (int)zipEntry.Length;
							}
							byte[] content = writer.ReadBytes(count);
							var accessToken = await UpzipFile(zipEntry, content);
							zippedFiles.Add(zipEntry.FullName, accessToken);
						}
					}
				}
			}
			return zippedFiles;
		}

		private async static Task<string> UpzipFile(ZipArchiveEntry zipEntries, byte[] contents)
		{
			StorageFolder favoriteFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
			var unZippedFile = await favoriteFolder.CreateFileAsync(zipEntries.FullName, CreationCollisionOption.ReplaceExisting);
			await FileIO.WriteBytesAsync(unZippedFile, contents);
			return StorageApplicationPermissions.FutureAccessList.Add(unZippedFile);
		}
	}
}
