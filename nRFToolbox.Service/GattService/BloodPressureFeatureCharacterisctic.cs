using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Service.GattService
{
	public interface IBloodPressureFeatureCharacterisctic
	{
		BloodPressureFeatureValue ProcessData(byte[] data);
	}

	public class BloodPressureFeatureCharacterisctic : IBloodPressureFeatureCharacterisctic
	{
		public BloodPressureFeatureValue ProcessData(byte[] data) 
		{
			var bloodPressureFeatureValue = new BloodPressureFeatureValue();
			int currentOffSet = 0;
			bloodPressureFeatureValue.BodyMovementDetectionSupport = (data[currentOffSet] & 0x01) != 0;
			bloodPressureFeatureValue.CuffFitDetectionSupport = (data[currentOffSet] & 0x02) != 0;
			bloodPressureFeatureValue.IrregularPulseDetectionSupport = (data[currentOffSet] & 0x04) != 0;
			bloodPressureFeatureValue.PulseRateRangeDetectionSupport = (data[currentOffSet] & 0x08) != 0;
			bloodPressureFeatureValue.MeasurementPositionDetectionSupport = (data[currentOffSet] & 0x10) != 0;
			bloodPressureFeatureValue.MultipleBondSupport = (data[currentOffSet] & 0x20) != 0;
			return bloodPressureFeatureValue;
		}

	}

	public class BloodPressureFeatureValue 
	{
		public bool BodyMovementDetectionSupport { get; set; }
		public bool CuffFitDetectionSupport { get; set; }
		public bool IrregularPulseDetectionSupport { get; set; }
		public bool PulseRateRangeDetectionSupport { get; set; }
		public bool MeasurementPositionDetectionSupport { get; set; }
		public bool MultipleBondSupport { get; set; }	
	}

}
