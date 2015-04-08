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
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage;
using Windows.Storage.Streams;
using Common.GattService;
using Common.Service.ApplicationException;
using Common.Utility;

namespace Common.Service.GattService
{
#if WINDOWS_PHONE_APP
	/// <summary>
	/// DFUservice provide functions to handle logic when communicate with peripheral.
	/// </summary>
	public interface IDeviceFirmwareUpdateService : IGattService
	{
		/// <summary>
		/// After service changed, notify client.
		/// </summary>
		event Common.Service.GattService.DeviceFirmwareUpdateService.ServiceChangedIndication ServiceChanged;
		/// <summary>
		/// After sending complete, notify client.
		/// </summary>
		event Common.Service.GattService.DeviceFirmwareUpdateService.DeviceFirmwareUpdateCompleteIndication DeviceFirmwareUpdateComplete;
		/// <summary>
		/// Comfirmed packet receipt from
		/// </summary>
		event Common.Service.GattService.DeviceFirmwareUpdateService.ComfirmPacketReceiptIndication PacketReceiptConfirmed;
		Task<bool> Start(GattDeviceService deviceFirmwareUpdateService, GattDeviceService genericAttribute, FirmwareTypeEnum firmwareType, StorageFile dataFile, StorageFile imageFile, int softDeviceSize = 0, int bootLoaderSize = 0);
		/// <summary>copy
		/// Enable service change on dfu at application mode.
		/// </summary>
		Task EnableServiceChange();
		/// <summary>
		/// After enabled service change, then switch from application to bootloader mode. 
		/// Current we support updating three types of images softdevice(0x01), bootloader(0x02), application(0x04)
		/// </summary>
		Task SwitchOnBootLoader();
		/// <summary>
		/// When it is on bootloader mode, start the dfu process
		/// <see ref="http://developer.nordicsemi.com/nRF51_SDK/doc/7.1.0/s110/html/a00066.html"/>
		/// </summary>
		void StarDfu(GattDeviceService deviceFirmwareUpdate);
		void Stop();
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
		public delegate void ComfirmPacketReceiptIndication(int sizeOfBytesSent, int totalFirmwareLength, string messageType, string messageData);
		public event ComfirmPacketReceiptIndication PacketReceiptConfirmed;
		#endregion

		public async Task<bool> Start(GattDeviceService deviceFirmwareUpdateService, GattDeviceService genericAttribute, FirmwareTypeEnum firmwareType, StorageFile dataFile, StorageFile imageFile, int softDeviceSize = 0, int bootLoaderSize = 0)
		{
			if (deviceFirmwareUpdateService != null && genericAttribute != null 
				&& deviceFirmwareUpdateService.Uuid.Equals(ToolboxIdentifications.GattServiceUuids.DeviceFirmwareUpdate) 
				&& genericAttribute.Uuid.Equals(GattServiceUuids.GenericAttribute))
			{
				this.deviceFirmwareUpdateServiceOnApplicationMode = deviceFirmwareUpdateService;
				this.genericAttributeOnApplicationModel = genericAttribute;
				this.firmwareType = firmwareType;
				if (firmwareType.Equals(FirmwareTypeEnum.MultiFiles))
				{
					if (softDeviceSize == 0 || bootLoaderSize == 0)
						throw new ArgumentException();
					initialSizes = GetSizeOfImage(softDeviceSize, bootLoaderSize);
				}
				initialPacket = await File.ReadToBuffer(dataFile);
				firmwareImage = await File.ReadToBytes(imageFile);
				initialSizes = GetSizeOfImage();
				return IsServiceInitialized = true;
			}
			else
				return  IsServiceInitialized = false;
		}

		public async Task EnableServiceChange()
		{
			serviceChangeOnApplicationModel = genericAttributeOnApplicationModel.GetCharacteristics(GattCharacteristicUuids.GattServiceChanged).FirstOrDefault();
			if(serviceChangeOnApplicationModel != null)
			{
				serviceChangeOnApplicationModel.ValueChanged += serviceChange_ValueChanged;
				var result = await serviceChangeOnApplicationModel.ReadClientCharacteristicConfigurationDescriptorAsync();
				if (result != null && result.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.Indicate)
				{
					serviceChangeOnApplicationModel.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Indicate);
				}
			}
		}
		public async Task SwitchOnBootLoader()
		{
			GattCharacteristic controlPoint = deviceFirmwareUpdateServiceOnApplicationMode.GetCharacteristics(ToolboxIdentifications.GattCharacteristicsUuid.DFUControlPoint).FirstOrDefault();
			if (controlPoint != null)
			{
				await controlPoint.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
				var buffer = deviceFirmwareUpdateControlPointCharacteristics.StartBootLoaderCommand(this.firmwareType);
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
					var buffer = deviceFirmwareUpdatePacketCharacteristics.ImageSizeCommand(initialSizes);
					await packet.WriteValueAsync(buffer, GattWriteOption.WriteWithoutResponse);
				}
			}
			catch(Exception)
			{
			}
		}

		private int[] GetSizeOfImage(int softDevice = 0, int bootLoader = 0)
		{
			//as the specification, size should be give in order
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
				case FirmwareTypeEnum.MultiFiles:
					if (softDevice == 0 || bootLoader == 0)
						throw new ArgumentException();
					sizes[0] = softDevice;
					sizes[1] = bootLoader;
					break;
				default:
					throw new ArgumentException();
			}
			return sizes;
		}

		private async void controlPoint_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
		{
			var values = File.ToBytes(args.CharacteristicValue);
			var stepAt = deviceFirmwareUpdateControlPointCharacteristics.ProcessData(values);
			var messageType = stepAt[1];
			if (messageType.Equals(deviceFirmwareUpdateControlPointCharacteristics.OTHER_OP_CODE) 
				|| messageType.Equals(deviceFirmwareUpdateControlPointCharacteristics.OTHER_RESPONSE_CODE))
			{
				if (PacketReceiptConfirmed != null)
					PacketReceiptConfirmed(0, 0, messageType, stepAt[0]);
			}
			switch (stepAt[0]) 
			{
				case DfuOperationCode.StartDfuSucceded:
					var InitialPacketStart = deviceFirmwareUpdateControlPointCharacteristics.InitialPacketStartCommand();
					await controlPoint.WriteValueAsync(InitialPacketStart);
				   await packet.WriteValueAsync(initialPacket, GattWriteOption.WriteWithoutResponse);
					var InitialPacketComplete = deviceFirmwareUpdateControlPointCharacteristics.InitialPacketCompleteCommand();
					await controlPoint.WriteValueAsync(InitialPacketComplete);
					break;
				case DfuOperationCode.InitialzeDFUParameterSucceded:
					var RequestPacketReceiptNotificationCommand = deviceFirmwareUpdateControlPointCharacteristics.RequestPacketReceiptNotificationCommand(numberOfPacketsAtATime);
					await controlPoint.WriteValueAsync(RequestPacketReceiptNotificationCommand);
					var ReceiveFirmwareImageCommand = deviceFirmwareUpdateControlPointCharacteristics.ReceiveFirmwareImageCommand();
					await controlPoint.WriteValueAsync(ReceiveFirmwareImageCommand);
					firmwareImageTrunks = firmwareImage.Slice(maxSizePerGroup);
					NumberOfTimes(firmwareImageTrunks);
					SendingImage(firmwareImageTrunks);
					break;
				case DfuOperationCode.PacketReceiptNotification:
					var comfirmedBytes = Convert.ToInt32(stepAt[1]);
					if (PacketReceiptConfirmed != null)
						PacketReceiptConfirmed(comfirmedBytes, firmwareImage.Length, string.Empty, string.Empty);
					if (comfirmedBytes == sendedBytes)
						SendingImage(firmwareImageTrunks);
					break;
				case DfuOperationCode.ReceiveFirmwareImageSucceded:
					var ValidateFirmwareCommand = deviceFirmwareUpdateControlPointCharacteristics.ValidateFirmwareCommand();
					await controlPoint.WriteValueAsync(ValidateFirmwareCommand);
					break;
				case DfuOperationCode.ValidateFirmareSucceded:
					var ActiveAndResetCommand = deviceFirmwareUpdateControlPointCharacteristics.ActiveAndResetCommand();
					await controlPoint.WriteValueAsync(ActiveAndResetCommand);
					if (DeviceFirmwareUpdateComplete != null)
						DeviceFirmwareUpdateComplete(true);
					Stop();
					break;
			}
		}

		private void SendingImage(byte[][] trunks) 
		{
			if (sentTimes == sendFullPackCompleteIndicator)
			{
				int limitation = sentTimes + sendPartialPacketsNumberOfTimes;
				WriteImage(trunks, limitation);
			}
			else if(sentTimes < sendFullPackCompleteIndicator)
			{
				int limitation = sentTimes + numberOfPacketsAtATime;
				WriteImage(trunks, limitation);
			}
		}

		private async void WriteImage(byte[][] trunks, int limitation) 
		{
			while (sentTimes < limitation) 
			{
				var buffer = deviceFirmwareUpdateControlPointCharacteristics.PartialOfFirmwareImage(trunks, sentTimes);
				await packet.WriteValueAsync(buffer, GattWriteOption.WriteWithoutResponse);
				sendedBytes += trunks[sentTimes].Length;
				sentTimes++;
			}
		}

		private void NumberOfTimes(byte[][] trunks)
		{
			sendFullPackCompleteIndicator = (trunks.Length / numberOfPacketsAtATime) * numberOfPacketsAtATime;
			sendPartialPacketsNumberOfTimes = trunks.Length % numberOfPacketsAtATime;
		}

		public void Stop()
		{
			if (IsServiceChanged && IsServiceInitialized && controlPoint != null)
				controlPoint.ValueChanged -= controlPoint_ValueChanged;
			else
			{
				//throw new ServiceNotInitializedException();
			}
		}

		#region Properties
		private GattCharacteristic controlPoint { get; set; }

		private GattCharacteristic packet { get; set; }

		private GattCharacteristic dFUVersion { get; set; }

		private bool IsServiceChanged = false;

		private bool IsServiceInitialized { get; set; }

		public int maxSizePerGroup = 20;

		public short numberOfPacketsAtATime = 10;

		private int sendFullPackCompleteIndicator = 0;

		private int sendPartialPacketsNumberOfTimes = 0;

		private int sentTimes = 0;

		private int sendedBytes = 0;

		private byte[][] firmwareImageTrunks { get; set; }

		// Save as Softdevice when sending in packs
		private byte[] firmwareImage { get; set; }
		private int[] initialSizes { get; set; }

		private  GattDeviceService genericAttributeOnApplicationModel { get; set; }
		private  GattDeviceService deviceFirmwareUpdateServiceOnApplicationMode { get; set; }
		public GattCharacteristic serviceChangeOnApplicationModel { get; set; }
		public IBuffer initialPacket { get; set; }
		public FirmwareTypeEnum firmwareType { get; set; }

		//Save as Bootload when sending in pack
		#endregion

		public string Name
		{
			get { return ToolboxIdentifications.GattServiceNames.DEVICE_FIRMWARE_UPDATE; }
		}
	}
#endif
}
