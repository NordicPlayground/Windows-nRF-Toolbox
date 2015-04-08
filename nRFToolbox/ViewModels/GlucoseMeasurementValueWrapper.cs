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
