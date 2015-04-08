using Common.Utility;
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
using Windows.Storage.Streams;

namespace Common.Service.GattService
{
	/// <summary>
	/// DFUPacket provide functions to handle logic when writes to Packet Characteristics
	/// </summary>
	public interface IDeviceFirmwareUpdatePacketCharacteristics 
	{
		/// <summary>
		/// This function construct image size command
		/// </summary>
		IBuffer ImageSizeCommand(int[] sizeOfImage);
	}
	public class DeviceFirmwareUpdatePacketCharacteristics : IDeviceFirmwareUpdatePacketCharacteristics
	{
		public IBuffer ImageSizeCommand(int[] sizeOfImage) 
		{
			if (sizeOfImage.Length != 3)
				throw new ArgumentException();
			int softDeviceSize = sizeOfImage[0];
			int bootLoaderSize = sizeOfImage[1];
			int applicationSize = sizeOfImage[2];
			if (softDeviceSize * bootLoaderSize * applicationSize < 0)
				throw new ArgumentException();
			//as the specification <Length of SoftDevice><Length of Bootloader><Length of Application> 
			var softDeviceBytes = BitConverter.GetBytes(softDeviceSize);
			var bootLoaderBytes = BitConverter.GetBytes(bootLoaderSize);
			var applicationSizeBytes = BitConverter.GetBytes(applicationSize);
			var temp = new byte[softDeviceBytes.Length + bootLoaderBytes.Length + applicationSizeBytes.Length];
			Array.Copy(softDeviceBytes, 0, temp, 0, softDeviceBytes.Length);
			Array.Copy(bootLoaderBytes, 0, temp, 4, bootLoaderBytes.Length);
			Array.Copy(applicationSizeBytes, 0, temp, 8, applicationSizeBytes.Length);
			var buffer = File.ToIBuffer(temp);
			return buffer;
		}
	}
}
