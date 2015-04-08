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
	public interface IRXCharacteristic
	{
		IBuffer TalkTo(string text);
		string ReceivedEcho(IBuffer buffer);
	}
	public class RXCharacteristic : IRXCharacteristic
	{
		public IBuffer TalkTo(string text) 
		{
			var buffer = File.ToIBuffer(text);
			if (buffer.Length <= MaximunBytes)
				return buffer;
			else
				throw new ApplicationArgumentException();
		}

		public string ReceivedEcho(IBuffer buffer) 
		{
			return File.ToString(buffer);
		}

		private int MaximunBytes = 20;
	}
}
