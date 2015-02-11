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
using nRFToolbox.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace nRFToolbox.Service.GattService
{
	public interface IDeviceFirmwareUpdateControlPointCharacteristics 
	{
		string[] ProcessData(byte[] data);
		IBuffer InitialPacketStartCommand();
		IBuffer InitialPacketCompleteCommand();
		IBuffer RequestPacketReceiptNotificationCommand(Int16 numberOfPackets);
		IBuffer ReceiveFirmwareImageCommand();
		IBuffer PartialOfFirmwareImage(byte[][] trunk, int offSet);
		IBuffer ValidateFirmwareCommand();
		IBuffer StartBootLoaderCommand(FirmwareTypeEnum firmwareType);
		IBuffer ActiveAndResetCommand();
	}
	public class DeviceFirmwareUpdateControlPointCharacteristics : IDeviceFirmwareUpdateControlPointCharacteristics
	{
#region OpCode
		private byte OpCode_StartDfu = 0x01;
		private byte OpCode_InitialzeDFUParameter = 0x02;
		private byte OpCode_ReceiveFirmwareImage = 0x03;
		private byte OpCode_ValidateFirmware = 0x04;
		private byte OpCode_ActiveImageAndReset = 0x05;
		private byte OpCode_PacketReceiptNotificationRequest = 0x08;
		private byte OpCode_ResponseCode = 0x10;
		private byte OpCode_PacketReceiptNotification = 0x11;
#endregion

#region ResponseValue
		private byte success = 0x01;
#endregion

#region DFU_Initial_Packet
		private byte OpCode_InitialPacketReceive = 0x00;
		private byte OpCode_InitialPacketComplete = 0x01;
#endregion

		private int returnValueCode = 0;
		private int returnValueOptional = 1;

		public string[] ProcessData(byte[] data) 
		{
			if(data == null || data.Length == 0)
				throw new ApplicationArgumentException();
			var opCode = data[0];
			byte responseValue = byte.MinValue;
			byte requestedOpCode = byte.MinValue;
			var returnValues = new string[2];
			if(opCode == OpCode_ResponseCode)
			{
				requestedOpCode = data[1];
				responseValue = data[2];
				if(requestedOpCode == OpCode_StartDfu && responseValue == success)
				{
					returnValues[returnValueCode] = OperationCodeDefinition.StartDfuSucceded;
					returnValues[returnValueOptional] = string.Empty;
					return returnValues;
				}
				else if (requestedOpCode == OpCode_InitialzeDFUParameter && responseValue == success)
				{
					returnValues[returnValueCode] = OperationCodeDefinition.InitialzeDFUParameterSucceded;
					returnValues[returnValueOptional] = string.Empty;
					return returnValues;
				}
				else if(requestedOpCode == OpCode_ReceiveFirmwareImage && responseValue == success)
				{
					returnValues[returnValueCode] = OperationCodeDefinition.ReceiveFirmwareImageSucceded;
					returnValues[returnValueOptional] = string.Empty;
					return returnValues;
				}
				else if (requestedOpCode == OpCode_ValidateFirmware && responseValue == success)
				{
					returnValues[returnValueCode] = OperationCodeDefinition.ValidateFirmareSucceded;
					returnValues[returnValueOptional] = string.Empty;
					return returnValues;
				}
				return null;
			}
			else if(opCode == OpCode_PacketReceiptNotification)
			{
				var receivedBytes = new byte[4];
				Array.Copy(data, 1, receivedBytes, 0, 4);
				returnValues[returnValueCode] = OperationCodeDefinition.PacketReceiptNotification;
				returnValues[returnValueOptional] = File.ToInt32(receivedBytes).ToString();
				return returnValues;
			}
			return null;
		}


#region Commands
		public IBuffer StartBootLoaderCommand(FirmwareTypeEnum firmwareType)
		{
			var temp = new byte[] { OpCode_StartDfu, (byte)firmwareType };
			var buffer = File.ToIBuffer(temp);
			return buffer;
		}
		public IBuffer InitialPacketStartCommand()
		{
			var temp = new byte[] { OpCode_InitialzeDFUParameter, OpCode_InitialPacketReceive };
			var buffer = File.ToIBuffer(temp);
			return buffer;
		}
		public IBuffer InitialPacketCompleteCommand()
		{
			var temp = new byte[] { OpCode_InitialzeDFUParameter, OpCode_InitialPacketComplete };
			var buffer = File.ToIBuffer(temp);
			return buffer;
		}

		public IBuffer RequestPacketReceiptNotificationCommand(Int16 numberOfPackets) 
		{
			var bytes = File.ToBytes(numberOfPackets);
			var temp = new byte[ 1 + bytes.Length];
			temp[0] = OpCode_PacketReceiptNotificationRequest;
			Array.Copy(bytes, 0, temp, 1, bytes.Length);
			var buffer = File.ToIBuffer(temp);
			return buffer;
		}
		public IBuffer ReceiveFirmwareImageCommand() 
		{
			var temp = new byte[] { OpCode_ReceiveFirmwareImage };
			var buffer = File.ToIBuffer(temp);
			return buffer;
		}
		public IBuffer PartialOfFirmwareImage(byte[][] trunk, int offSet) 
		{
			var temp = trunk[offSet];
			var buffer = File.ToIBuffer(temp);
			return buffer;
		}
		public IBuffer ValidateFirmwareCommand() 
		{
			var temp = new byte[] { OpCode_ValidateFirmware };
			var buffer = File.ToIBuffer(temp);
			return buffer;
		}
		public IBuffer ActiveAndResetCommand() 
		{
			var temp = new byte[] { OpCode_ActiveImageAndReset };
			var buffer = File.ToIBuffer(temp);
			return buffer;
		}
#endregion		
	}
}
