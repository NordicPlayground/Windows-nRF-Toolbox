using Common.Service.ApplicationException;
using Common.Service.GattService;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Common.Service.DFUPackage
{
	public static class DFUPackageHandler
	{
		private const string ManifestFileName = "manifest.json";
		public static async Task<Dictionary<string, string>> UpzipDFUPackage(StorageFile zipFile, FirmwareTypeEnum type)
		{
			var fileDictionary = await File.UnzipFile(zipFile);
			string token;
			DFUServiceManifestRoot manifestObject = null;
			StorageFile manifest = null;
			if (fileDictionary.TryGetValue(ManifestFileName, out token))
			{
				manifest = await File.GetStoredFile(token);
				manifestObject = await GetManifestObject(manifest);
				switch (type)
				{
					case FirmwareTypeEnum.Application:
						return GetBinDatFileTokens(fileDictionary, manifestObject.manifest.application.bin_file, manifestObject.manifest.application.dat_file);
					case FirmwareTypeEnum.BootLoader:
						return GetBinDatFileTokens(fileDictionary, manifestObject.manifest.bootloader.bin_file, manifestObject.manifest.bootloader.dat_file);
					case FirmwareTypeEnum.SoftDevice:
						return GetBinDatFileTokens(fileDictionary, manifestObject.manifest.softdevice.bin_file, manifestObject.manifest.softdevice.dat_file);
					case FirmwareTypeEnum.MultiFiles:
						return GetBinDatFileTokens(fileDictionary, manifestObject.manifest.softdevice_bootloader.bin_file, manifestObject.manifest.softdevice_bootloader.dat_file);
				}
			}
			return null;
		}

		public static async Task<Dictionary<string, string>> UpzipDFUPackage(StorageFile zipFile)
		{
			var fileDictionary = await File.UnzipFile(zipFile);
			return fileDictionary;
		}

		public async static Task<DFUServiceManifestRoot> GetManifestObject(StorageFile manifestText)
		{
			StorageFile file = manifestText;
			DFUServiceManifestRoot manifestObject = null;
			using (var transaction = await file.OpenStreamForReadAsync())
			{
				using (StreamReader dataReader = new StreamReader(transaction))
				{
					DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(DFUServiceManifestRoot));
					manifestObject = (DFUServiceManifestRoot)js.ReadObject(dataReader.BaseStream);
				}
			}
			return manifestObject;
		}

		private static Dictionary<string, string> GetBinDatFileTokens(Dictionary<string, string> tokens, string binFileName, string datFileName)
		{
			var binDatToken = new Dictionary<string, string>();
			string binToken;
			string datToken;
			if (tokens.TryGetValue(binFileName, out binToken))
			{
				binDatToken.Add(binFileName, binToken);
			}
			if (tokens.TryGetValue(datFileName, out datToken))
			{
				binDatToken.Add(datFileName, datToken);
			}
			return binDatToken;
		}

		public static FirmwareTypeEnum GetFirmwareType(string firmwareType)
		{
			FirmwareTypeEnum result;
			if (Enum.TryParse(firmwareType, true, out result))
				return result;
			else
				throw new ApplicationArgumentException();
		}
	}
}
