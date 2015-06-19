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
using Common.Service.GattService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nRFToolbox.ViewModels
{
	public class GlucoseMeasurementValueWrapper : ViewModelBase
	{
		private GlucoseMeasurementValue Measurement;
		public GlucoseMeasurementValueWrapper(GlucoseMeasurementValue measurement) 
		{
			this.Measurement = measurement;
			this.timeOffSet = this.Measurement.TimeOffset.ToString();
			this.sequenceNumber = this.Measurement.SequenceNumber.ToString();
			this.baseTime = this.Measurement.BaseTime.ToString();
			this.location = this.Measurement.Location.ToString();
			this.type = this.Measurement.Type.ToString();
			this.glucoseConcentrationKgL = this.Measurement.GlucoseConcentrationKgL.ToString();
			this.glucoseConcentrationMolL = this.Measurement.GlucoseConcentrationMolL.ToString();
		}

		private string timeOffSet;
		public string TimeOffset 
		{
			get 
			{
				return "Time off set: " + this.timeOffSet;
			}
			set 
			{	
				if(this.timeOffSet != value)
				{
					timeOffSet = value;
					OnPropertyChanged("TimeOffSet");
				}
			}
		}

		private string sequenceNumber;
		public string SequenceNumber 
		{ 
			get
			{
				return "Sequence number: " + sequenceNumber;
			}
			set
			{
				if(this.sequenceNumber != value)
				{
					sequenceNumber = value;
					OnPropertyChanged("SequenceNumber");
				}
			}
		}

		public string baseTime;
		public string BaseTime 
		{ 
			get
			{
				return "Base time: " + baseTime;
			}
			set
			{
				if (this.baseTime != value)
				{
					baseTime = value;
					OnPropertyChanged("BaseTime");
				}
			}
		}

		private string glucoseConcentrationKgL;
		public string GlucoseConcentrationKgL
		{
			get
			{
				return glucoseConcentrationKgL;
			}
			set
			{
				if (this.glucoseConcentrationKgL != value)
				{
					glucoseConcentrationKgL = value;
					OnPropertyChanged("GlucoseConcentrationKgL");
				}
			}
		}

		private string glucoseConcentrationMolL;
		public string GlucoseConcentrationMolL
		{
			get
			{
				return glucoseConcentrationMolL;
			}
			set
			{
				if (this.glucoseConcentrationMolL != value)
				{
					glucoseConcentrationMolL = value;
					OnPropertyChanged("GlucoseConcentrationMolL");
				}
			}
		}

		private string type;
		public string Type
		{
			get
			{
				return "Type: " + type;
			}
			set
			{
				if (this.type != value)
				{
					type = value;
					OnPropertyChanged("Type");
				}
			}
		}

		private string location;
		public string Location
		{
			get
			{
				return "Location: " + location;
			}
			set
			{
				if (this.location != value)
				{
					location = value;
					OnPropertyChanged("Location");
				}
			}
		}

	}
}
