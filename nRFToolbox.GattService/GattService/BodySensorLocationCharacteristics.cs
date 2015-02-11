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
	public interface IBodySensorLocationCharacteristics
	{
		 BodySensorLocationCharacteristics ProcessBodySensorLocationData(byte[] bodySensorLocationData);
	}

	public class BodySensorLocationCharacteristics : IBodySensorLocationCharacteristics
	{
		public string BodySensorLocation { get; set; }

		public BodySensorLocationCharacteristics ProcessBodySensorLocationData(byte[] bodySensorLocationData)
		{
			byte bodySensorLocationValue = bodySensorLocationData[0];
			string retval;

			retval = "";
			switch (bodySensorLocationValue)
			{
				case 0:
					retval += "Other";
					break;
				case 1:
					retval += "Chest";
					break;
				case 2:
					retval += "Wrist";
					break;
				case 3:
					retval += "Finger";
					break;
				case 4:
					retval += "Hand";
					break;
				case 5:
					retval += "Ear Lobe";
					break;
				case 6:
					retval += "Foot";
					break;
				default:
					retval = "";
					break;
			}
			return new BodySensorLocationCharacteristics
			{
				BodySensorLocation = retval
			};
		}
	}
}
