using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.GattService
{
		public static class DfuOperationCode
		{
			public const string StartDfuSucceded = "StartDfuSucceded";
			public const string InitialzeDFUParameterSucceded = "InitialzeDFUParameterSucceded";
			public const string PacketReceiptNotification = "PacketReceiptNotification";
			public const string ReceiveFirmwareImageSucceded = "ReceiveFirmwareImageSucceded";
			public const string ValidateFirmareSucceded = "ValidateFirmareSucceded";
		}
}
