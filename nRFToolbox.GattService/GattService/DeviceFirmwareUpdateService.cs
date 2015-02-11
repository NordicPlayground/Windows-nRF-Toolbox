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
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace nRFToolbox.Service.GattService
{
	public interface IDeviceFirmwareUpdateService : IGattService
	{
		event nRFToolbox.Service.GattService.DeviceFirmwareUpdateService.ServiceChangedIndication ServiceChanged;
		event nRFToolbox.Service.GattService.DeviceFirmwareUpdateService.DeviceFirmwareUpdateCompleteIndication DeviceFirmwareUpdateComplete;
		Task<bool> Start(GattDeviceService dFUService, GattDeviceService genericAttribute);
		void Stop();
		void EnableServiceChange();
		void SwitchOnBootLoader(FirmwareTypeEnum firmware);
		void StarDfu(GattDeviceService deviceFirmwareUpdate);
	}
	public class DeviceFirmwareUpdateService :IDeviceFirmwareUpdateService
	{
		public IDeviceFirmwareUpdateControlPointCharacteristics deviceFirmwareUpdateControlPointCharacteristics { get; set; }
		public IDeviceFirmwareUpdatePacketCharacteristics deviceFirmwareUpdatePacketCharacteristics { get; set; }
		public IDeviceFirmwareUpdateVersionCharacteristics deviceFirmwareUpdateVersionCharacteristics { get; set; }

		public DeviceFirmwareUpdateService(IDeviceFirmwareUpdatePacketCharacteristics dFUPacketCharacteristics,
			IDeviceFirmwareUpdateControlPointCharacteristics dFUControlPointCharacteristics, IDeviceFirmwareUpdateVersionCharacteristics dFUVersionCharacteristics)
		{
			deviceFirmwareUpdatePacketCharacteristics = dFUPacketCharacteristics;
			deviceFirmwareUpdateControlPointCharacteristics = dFUControlPointCharacteristics;
			deviceFirmwareUpdateVersionCharacteristics = dFUVersionCharacteristics;
		}

		public DeviceFirmwareUpdateService(IDeviceFirmwareUpdatePacketCharacteristics dFUPacketCharacteristics,
			IDeviceFirmwareUpdateControlPointCharacteristics dFUControlPointCharacteristics) 
		{
			deviceFirmwareUpdatePacketCharacteristics = dFUPacketCharacteristics;
			deviceFirmwareUpdateControlPointCharacteristics = dFUControlPointCharacteristics;
		}

		#region Events
		public delegate void ServiceChangedIndication(GattCharacteristic sender, GattValueChangedEventArgs args);
		public event ServiceChangedIndication ServiceChanged;
		public delegate void DeviceFirmwareUpdateCompleteIndication(bool IsComplete);
		public event DeviceFirmwareUpdateCompleteIndication DeviceFirmwareUpdateComplete;
		#endregion

		public async Task<bool> Start(GattDeviceService deviceFirmwareUpdateService, GattDeviceService genericAttribute)
		{
			if (deviceFirmwareUpdateService != null && genericAttribute != null 
				&& deviceFirmwareUpdateService.Uuid.Equals(ToolboxIdentifications.GattServiceUuids.DeviceFirmwareUpdate) 
				&& genericAttribute.Uuid.Equals(GattServiceUuids.GenericAttribute))
			{
				this.deviceFirmwareUpdateServiceOnApplicationMode = deviceFirmwareUpdateService;
				this.genericAttributeOnApplicationModel = genericAttribute;
				initialPacket = await File.ReadInitialPack();
				firmwareImage = await File.ReadBinFile();
				return IsServiceInitialized = true;
			}
			else
				return  IsServiceInitialized = false;
		}

		public void EnableServiceChange()
		{
			serviceChangeOnApplicationModel = genericAttributeOnApplicationModel.GetCharacteristics(GattCharacteristicUuids.GattServiceChanged).FirstOrDefault();
			if(serviceChangeOnApplicationModel != null)
			{
				serviceChangeOnApplicationModel.ValueChanged += serviceChange_ValueChanged;
				serviceChangeOnApplicationModel.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Indicate);
			}
		}
		public async void SwitchOnBootLoader(FirmwareTypeEnum firmware)
		{
			this.firmwareType = firmware;
			GattCharacteristic controlPoint = deviceFirmwareUpdateServiceOnApplicationMode.GetCharacteristics(ToolboxIdentifications.GattCharacteristicsUuid.DFUControlPoint).FirstOrDefault();
			if (controlPoint != null)
			{
				await controlPoint.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
				var buffer = deviceFirmwareUpdateControlPointCharacteristics.StartBootLoaderCommand(firmware);
				await controlPoint.WriteValueAsync(buffer);
			}
		}

		async void serviceChange_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
		{
			IsServiceChanged = true;
			await Task.Delay(TimeSpan.FromMilliseconds(300)); //do we need this?
			if (ServiceChanged != null)
				ServiceChanged(sender, args); // notify the client side to do service rediscovering
		}



		public async void StarDfu(GattDeviceService deviceFirmwareUpdate) 
		{
			if (!IsServiceChanged || !IsServiceInitialized)
				throw new Exception();
			deviceFirmwareUpdateControlPointCharacteristics.StartBootLoaderCommand(this.firmwareType);
			controlPoint = deviceFirmwareUpdate.GetCharacteristics(ToolboxIdentifications.GattCharacteristicsUuid.DFUControlPoint).FirstOrDefault();
			packet = deviceFirmwareUpdate.GetCharacteristics(ToolboxIdentifications.GattCharacteristicsUuid.DFUPacket).FirstOrDefault();
			dFUVersion = deviceFirmwareUpdate.GetCharacteristics(ToolboxIdentifications.GattCharacteristicsUuid.DFUVersion).FirstOrDefault();
			try 
			{
				if (controlPoint != null)
				{
					controlPoint.ValueChanged += controlPoint_ValueChanged;
					await controlPoint.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
					var buffer = deviceFirmwareUpdateControlPointCharacteristics.StartBootLoaderCommand(this.firmwareType);
					await controlPoint.WriteValueAsync(buffer);
				}
				if(packet != null)
				{
					var sizes = GetSizeOfImage();
					var buffer = deviceFirmwareUpdatePacketCharacteristics.ImageSizeCommand(sizes);
					await packet.WriteValueAsync(buffer, GattWriteOption.WriteWithoutResponse);
				}
			}
			catch(Exception)
			{
			}
		}

		private int[] GetSizeOfImage()
		{
			//as the specification size should be give in order
			//<Length of SoftDevice><Length of Bootloader><Length of Application> 
			var sizes = new int[] { 0, 0, 0 };
			switch (firmwareType)
			{
				case FirmwareTypeEnum.SoftDevice:
					sizes[0] = firmwareImage.Length;
					break;
				case FirmwareTypeEnum.BootLoader:
					sizes[1] = firmwareImage.Length;
					break;
				case FirmwareTypeEnum.Application:
					sizes[2] = firmwareImage.Length;
					break;
				default:
					throw new ArgumentException();
			}
			return sizes;
		}

		private bool IsValidStartDfuCommands(byte[] startDfuCommand, byte[] imageSize) 
		{
			if (startDfuCommand == null || imageSize == null ||
					startDfuCommand.Length > 2 || startDfuCommand[0] != OpCode_StartDfu)
				return false;
			else
				return true;
		}

		private async void controlPoint_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
		{
			var values = File.ToBytes(args.CharacteristicValue);
			var stepAt = deviceFirmwareUpdateControlPointCharacteristics.ProcessData(values);
			if(stepAt == null)
				throw new ServiceFailedException();
			switch (stepAt[0]) 
			{
				case OperationCodeDefinition.StartDfuSucceded:
					var InitialPacketStart = deviceFirmwareUpdateControlPointCharacteristics.InitialPacketStartCommand();
					await controlPoint.WriteValueAsync(InitialPacketStart);
					await packet.WriteValueAsync(initialPacket, GattWriteOption.WriteWithoutResponse);
					var InitialPacketComplete = deviceFirmwareUpdateControlPointCharacteristics.InitialPacketCompleteCommand();
					await controlPoint.WriteValueAsync(InitialPacketComplete);
					break;
				case OperationCodeDefinition.InitialzeDFUParameterSucceded:
					var RequestPacketReceiptNotificationCommand = deviceFirmwareUpdateControlPointCharacteristics.RequestPacketReceiptNotificationCommand(numberOfPacketsAtATime);
					await controlPoint.WriteValueAsync(RequestPacketReceiptNotificationCommand);
					var ReceiveFirmwareImageCommand = deviceFirmwareUpdateControlPointCharacteristics.ReceiveFirmwareImageCommand();
					await controlPoint.WriteValueAsync(ReceiveFirmwareImageCommand);
					firmwareImageTrunks = firmwareImage.Slice(maxSizePerGroup);
					NumberOfTimes(firmwareImageTrunks);
					SendingImage(firmwareImageTrunks);
					break;
				case OperationCodeDefinition.PacketReceiptNotification:
					var comfirmedBytes = Convert.ToInt32(stepAt[1]);
					if (comfirmedBytes == sendedBytes)
						SendingImage(firmwareImageTrunks);
					break;
				case OperationCodeDefinition.ReceiveFirmwareImageSucceded:
					var ValidateFirmwareCommand = deviceFirmwareUpdateControlPointCharacteristics.ValidateFirmwareCommand();
					await controlPoint.WriteValueAsync(ValidateFirmwareCommand);
					break;
				case OperationCodeDefinition.ValidateFirmareSucceded:
					var ActiveAndResetCommand = deviceFirmwareUpdateControlPointCharacteristics.ActiveAndResetCommand();
					await controlPoint.WriteValueAsync(ActiveAndResetCommand);
					if (DeviceFirmwareUpdateComplete != null)
						DeviceFirmwareUpdateComplete(true);
					Stop();
					break;
				default:
					throw new ServiceFailedException();
			}
		}

		private void SendingImage(byte[][] trunks) 
		{
			if (sendedTimes == sendFullPackCompleteIndicator)
			{
				int limitation = sendedTimes + sendPartialPacketsNumberOfTimes;
				WriteImage(trunks, limitation);
			}
			else
			{
				int limitation = sendedTimes + numberOfPacketsAtATime;
				WriteImage(trunks, limitation);
			}
		}

		private async void WriteImage(byte[][] trunks, int limitation) 
		{
			while (sendedTimes < limitation) 
			{
				var buffer = deviceFirmwareUpdateControlPointCharacteristics.PartialOfFirmwareImage(trunks, sendedTimes);
				await packet.WriteValueAsync(buffer, GattWriteOption.WriteWithoutResponse);
				sendedBytes += trunks[sendedTimes].Length;
				sendedTimes++;
			}
		}

		private void NumberOfTimes(byte[][] trunks)
		{
			sendFullPackCompleteIndicator = (trunks.Length / numberOfPacketsAtATime) * numberOfPacketsAtATime;
			sendPartialPacketsNumberOfTimes = trunks.Length % numberOfPacketsAtATime;
		}


		public void Stop()
		{
			if(IsServiceInitialized && IsServiceChanged)
			{
				serviceChangeOnApplicationModel.ValueChanged -= serviceChange_ValueChanged;
				controlPoint.ValueChanged -= controlPoint_ValueChanged;
			}
		}

		#region Properties
		private GattCharacteristic controlPoint { get; set; }

		private GattCharacteristic packet { get; set; }

		private GattCharacteristic dFUVersion { get; set; }

		private bool IsServiceChanged = false;

		private byte OpCode_StartDfu = 0x01;

		private bool IsServiceInitialized { get; set; }

		public int maxSizePerGroup = 20;

		public short numberOfPacketsAtATime = 10;

		private int sendFullPackCompleteIndicator = 0;

		private int sendPartialPacketsNumberOfTimes = 0;

		private int sendedTimes = 0;

		private int sendedBytes = 0;

		public byte[][] firmwareImageTrunks { get; set; }
		public byte[] firmwareImage { get; set; }

		public FirmwareTypeEnum firmwareType { get; set; }

		private  GattDeviceService genericAttributeOnApplicationModel { get; set; }
		private  GattDeviceService deviceFirmwareUpdateServiceOnApplicationMode { get; set; }
		public GattCharacteristic serviceChangeOnApplicationModel { get; set; }
		public IBuffer initialPacket { get; set; }
		#endregion

		public string Name
		{
			get { return ToolboxIdentifications.GattServiceNames.DEVICE_FIRMWARE_UPDATE; }
		}
	}
}
