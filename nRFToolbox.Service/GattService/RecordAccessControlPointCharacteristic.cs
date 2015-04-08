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
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Common.Service.GattService
{
	public interface IRecordAccessControlPointCharacteristic 
	{
		void ProcessData(IBuffer data);
		IBuffer GetAllStoredRecord();
		IBuffer DeleteAllStoredRecord();
		IBuffer GetFirstRecord();
		IBuffer GetLastRecord();
	}

	public class RecordAccessControlPointCharacteristic : IRecordAccessControlPointCharacteristic
	{
		private byte OpCode_ReportStored_Records = 0x01;
		private byte OpCode_DeleteStoredRecords = 0x02;

		private byte Operator_AllRecords = 0x01;
		private byte Operator_LessThanOrEqualTo = 0x2;
		private byte Operator_FirstRecord = 0x5;
		private byte Operator_LastRecord = 0x6;

		public void ProcessData(IBuffer data) 
		{
			var values = File.ToBytes(data);
		}

		#region Commands
		public IBuffer GetAllStoredRecord() 
		{
			var temp = new byte[] { OpCode_ReportStored_Records, Operator_AllRecords };
			var buffer = File.ToIBuffer(temp);
			return buffer;
		}

		public IBuffer DeleteAllStoredRecord()
		{
			var temp = new byte[] { OpCode_DeleteStoredRecords, Operator_AllRecords };
			var buffer = File.ToIBuffer(temp);
			return buffer;
		}

		public IBuffer GetFirstRecord() 
		{
			var temp = new byte[] { OpCode_ReportStored_Records, Operator_FirstRecord };
			var buffer = File.ToIBuffer(temp);
			return buffer;
		}

		public IBuffer GetLastRecord() 
		{
			var temp = new byte[] { OpCode_ReportStored_Records, Operator_LastRecord };
			var buffer = File.ToIBuffer(temp);
			return buffer;
		}
		#endregion
	}
}
