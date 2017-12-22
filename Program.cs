using System;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace FetchFile
{
	class Program
	{
		private static Uri _targetUri;
		private static FileInfo _targetFile;
		private static bool _doUnzip;
		private static bool _deleteFile;

		static void Main(string[] args)
		{
			try
			{
				// parse arguments
				ParseArgs(args);

				// make sure target location exists
				if ((_targetFile != null) && !Directory.Exists(_targetFile.DirectoryName))
					Directory.CreateDirectory(_targetFile.DirectoryName);

				// download file
				using (var client = new WebClient())
				{
					if (_targetFile != null)
						client.DownloadFile(_targetUri.AbsoluteUri, _targetFile.FullName);
				}

				// optionally unzip it
				if (_doUnzip && (_targetFile != null))
				{
					try
					{
						ZipFile.ExtractToDirectory(_targetFile.FullName, _targetFile.DirectoryName);
					}
					catch (IOException ex)
					{
						if (ex.Message.Contains("already exists."))
							Console.WriteLine("One or more extracted files already exist, skipped unzip");
						else
							throw; // else re-throw the trapped exception
					}
				}

				// optionally delete original file
				if (_deleteFile && (_targetFile != null))
					File.Delete(_targetFile.FullName);
			}
			catch (Exception ex)
			{
				Console.WriteLine("An error occured: " + ex.Message);
			}
		}

		static void ParseArgs(string[] args)
		{
			const string uri = "-uri:";
			const string localfile = "-localfile:";
			const string unzip = "-unzip";
			const string delFile = "-delete";

			if (args.Length == 0)
			{
				Console.WriteLine("Syntax: -uri:<uri> -localfile:<filepath>");
				Console.WriteLine("        -unzip to decompress the target file after download");
				Console.WriteLine("        -delete to remove the downloaded file");
				throw new ArgumentException("No Arguments Specified");
			}

			foreach (string arg in args)
			{
				// scrub parameter
				string cleanArg = arg.Trim().ToLower();

				// save parameter values
				if (cleanArg.StartsWith(uri))
					_targetUri = new Uri(arg.Substring(uri.Length));
				if (cleanArg.StartsWith(localfile))
					_targetFile = new FileInfo(arg.Substring(localfile.Length));
				if (cleanArg.StartsWith(unzip))
					_doUnzip = true;
				if (cleanArg.StartsWith(delFile))
					_deleteFile = true;
			}
		}
	}
}
