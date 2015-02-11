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

namespace nRFToolbox.Service.GattService
{
	public interface IHeartRateMeasurementCharacteristic 
	{
		HeartRateMeasurementCharacteristic ProcessHeartRateMeasurementData(byte[] bmpData);
	}
	public class HeartRateMeasurementCharacteristic : IHeartRateMeasurementCharacteristic
	{
		public ushort BeatPerMinute { get; set; }
		public bool HasEnergyExpended { get; set; }
		public ushort EnergyExpended { get; set; }
		public ushort RR_Interval { get; set; }

		public HeartRateMeasurementCharacteristic ProcessHeartRateMeasurementData(byte[] bmpData)
		{
			if (bmpData == null)
				throw new ArgumentException();
			// FLAGS
			const byte HEART_RATE_VALUE_FORMAT = 0x01;
			const byte ENERGY_EXPENDED_STATUS = 0x08;
			const byte rR_INTERVAL = 0x10;

			ushort expendedEnergyValue = ushort.MinValue;
			ushort heartRateMeasurementValue = ushort.MinValue;
			ushort rR_IntervalValue = ushort.MinValue;
			byte currentOffSet = 0;
			byte flags = bmpData[currentOffSet];
			bool isHeartRateValueSizeLong = ((flags & HEART_RATE_VALUE_FORMAT) != 0);
			bool hasEnergyExpended = ((flags & ENERGY_EXPENDED_STATUS) != 0);
			bool hasRR_Interval = ((flags & rR_INTERVAL) != 0);

			currentOffSet++;
			if (isHeartRateValueSizeLong)
			{
				heartRateMeasurementValue = (ushort)((bmpData[currentOffSet + 1] << 8) + bmpData[currentOffSet]);
				currentOffSet += 2;
			}
			else
			{
				heartRateMeasurementValue = bmpData[currentOffSet];
				currentOffSet++;
			}
			if (hasEnergyExpended)
			{
				expendedEnergyValue = (ushort)((bmpData[currentOffSet + 1] << 8) + bmpData[currentOffSet]);
				currentOffSet += 2;
			}
			if(hasRR_Interval)
			{  
				//For future use not complete yet
				rR_IntervalValue = (ushort)((bmpData[currentOffSet + 1] << 8) + bmpData[currentOffSet]);
			}

			return new HeartRateMeasurementCharacteristic
			{
				BeatPerMinute = heartRateMeasurementValue,
				HasEnergyExpended = hasEnergyExpended,
				EnergyExpended = expendedEnergyValue,
				RR_Interval = rR_IntervalValue
			};
		}
	}
}
