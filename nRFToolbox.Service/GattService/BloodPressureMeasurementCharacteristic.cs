using Common.Service.ApplicationException;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Common.Service.GattService
{
	public interface IBloodPressureMeasurementCharacteristic
	{
		BloodPressureMeasurementValue ProcessData(byte[] data);
	}
	public class BloodPressureMeasurementCharacteristic : IBloodPressureMeasurementCharacteristic
	{
		public BloodPressureMeasurementValue ProcessData(byte[] data) 
		{
			if (data == null || data.Length == 0)
				throw new ApplicationArgumentException();

			var measurement = new BloodPressureMeasurementValue();
			byte currentOffSet = 0;
			int bluetoothDataTimeLength = 7;
			int MeasurementStatusDataLength = 2;

			bool IsBloodPressureUnitsFlagmmHg = false;
			bool HasTimeStampFlag = false;
			bool HasPulseRateFlag = false;
			bool HasUserIDFlag = false;
			bool HasMeasurementStatusFlag = false;
			if ((data[currentOffSet] & 0x01) == 0)
				IsBloodPressureUnitsFlagmmHg = true;
			else
				IsBloodPressureUnitsFlagmmHg = true;
			if ((data[currentOffSet] & 0x02) != 0)
				HasTimeStampFlag = true;
			if ((data[currentOffSet] & 0x04) != 0)
				HasPulseRateFlag = true;
			if ((data[currentOffSet] & 0x08) != 0)
				HasUserIDFlag = true;
			if ((data[currentOffSet] & 0x10) != 0)
				HasMeasurementStatusFlag = true;
			if (IsBloodPressureUnitsFlagmmHg)
			{
				measurement.Unit = "mm/g";
				currentOffSet++;
				measurement.Systolic = File.ToSFloat(new byte[] { data[currentOffSet], data[currentOffSet + 1] });
				currentOffSet += 2;
				measurement.Diastolic = File.ToSFloat(new byte[] { data[currentOffSet], data[currentOffSet + 1] });
				currentOffSet += 2;
				measurement.MeanArterialPressure = File.ToSFloat(new byte[] { data[currentOffSet], data[currentOffSet + 1] });
				currentOffSet += 2;
			}
			else
			{
				measurement.Unit = "kPa";
				currentOffSet += 6;
				measurement.Systolic = File.ToSFloat(new byte[] { data[currentOffSet], data[currentOffSet + 1] });
				currentOffSet += 2;
				measurement.Diastolic = File.ToSFloat(new byte[] { data[currentOffSet], data[currentOffSet + 1] });
				currentOffSet += 2;
				measurement.MeanArterialPressure = File.ToSFloat(new byte[] { data[currentOffSet], data[currentOffSet + 1] });
				currentOffSet += 2;
			}
			if(HasTimeStampFlag)
			{
				byte[] baseTime = new byte[bluetoothDataTimeLength];
				Array.Copy(data, currentOffSet, baseTime, 0, bluetoothDataTimeLength);
				measurement.TimeStanp = File.ToDateTime(baseTime);
				currentOffSet += 7;
			}
			if(HasPulseRateFlag)
			{
				measurement.PulseRate = File.ToSFloat(new byte[] { data[currentOffSet], data[currentOffSet + 1] });
				currentOffSet += 2;
			}
			if(HasUserIDFlag)
			{
				measurement.UserID = data[currentOffSet];
				currentOffSet++;
			}
			if(HasMeasurementStatusFlag)
			{
				byte[] measurementStatus = new byte[MeasurementStatusDataLength];
				Array.Copy(data, currentOffSet, measurementStatus, 0, MeasurementStatusDataLength);
				measurement.MeasurementStatusFlags.ProcessData(measurementStatus);
			}
			return measurement;
		}
	}

	public class BloodPressureMeasurementValue
	{
		public float Systolic { get; set; }
		public float Diastolic { get; set; }
		public float MeanArterialPressure { get; set; }
		public DateTime? TimeStanp { get; set; }
		public float PulseRate { get; set; }
		public byte UserID { get; set; }
		public string Unit { get; set; }
		private MeasurementStatusFlag measurementStatusFlags = new MeasurementStatusFlag ();
		public MeasurementStatusFlag MeasurementStatusFlags 
		{
			get 
			{
				return measurementStatusFlags;
			}		
		}
	}

	public class MeasurementStatusFlag
	{
		public bool BodyMovementDetectionFlag;
		public bool CuffFitDetectionFlag;
		public bool IrregularPulseDetectionFlag;
		public int PulseRateRangeDetectionFlags; // 2 bit
		public bool MeasurementPositionDetectionFlag;

		public void ProcessData(byte[] data)
		{
			int currentOffSet = 0;
			BodyMovementDetectionFlag = (data[currentOffSet] & 0x01) != 0;
			currentOffSet++;
			CuffFitDetectionFlag = (data[currentOffSet] & 0x01) != 0;
			currentOffSet++;
			IrregularPulseDetectionFlag = (data[currentOffSet] & 0x01) != 0;
			currentOffSet++;
			PulseRateRangeDetectionFlags = File.ToInt32(new byte[] { data[currentOffSet], data[currentOffSet + 1] });
			MeasurementPositionDetectionFlag = (data[currentOffSet] & 0x01) != 0;
		}
	}
}
