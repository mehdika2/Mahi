using Mahi.Properties;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Mahi.Templates
{
	internal class Template
	{
		const string templates = @"
	Authentication:auth	authentication.zip
	API:api	api.zip
	Single Page Application:spa	singlepageapplication.zip
	MVC:mvc	modelviewcontroller.zip
";

		public static bool CreateFromArguments()
		{
			var args = Environment.GetCommandLineArgs();
			if (args == null || args.Length < 0)
				return false;

			int templateOptionIndex = ArgumentIndex("-t", args);
			if (templateOptionIndex == -1)
			{
				return false;
			}

			if (args.Length == templateOptionIndex + 1)
			{
				Console.WriteLine(Resources.Error_CreateTemplateNameRequired);
				return true;
			}

			string[] lines = templates.Split("\n", StringSplitOptions.RemoveEmptyEntries);

			if (args[templateOptionIndex + 1] == "-l")
			{
				Dictionary<string, string> tempList = new Dictionary<string, string>();
				for (int i = 0; i < lines.Length; i++)
				{
					string template = lines[i].Trim();
					string[] parts = template.Split(':');
					string title = parts[0];
					string name = parts[1].Trim().Split('\t')[0];
					tempList.Add(title, name);
				}
				int bigestHeader = tempList.Select(i => i.Key).OrderByDescending(i => i.Length).First().Length;
				string header = " Template Name " + new string(' ', (bigestHeader > 15 ? bigestHeader - 15 : 0)) + " |  ";
				int nameStartIndex = header.Length;
				header += "Short name " + new string(' ', tempList.Select(i => i.Value).OrderByDescending(i => i.Length).First().Length - 4);
				Console.WriteLine(header);
				Console.WriteLine(new string('-', header.Length));
				foreach (var temp in tempList)
				{
					Console.Write(' ' + temp.Key);
					Console.Write(new string(' ', nameStartIndex - temp.Key.Length));
					Console.WriteLine(temp.Value);
				}
				Console.WriteLine();
				return true;
			}

			for (int i = 0; i < lines.Length; i++)
			{
				string template = lines[i].Trim();
				string[] parts = template.Split(':');
				string name = parts[0];
				string[] valueParts = parts[1].Trim().Split('\t');
				if (args[templateOptionIndex + 1].ToLower() == valueParts[0])
				{
					int forceIndex = templateOptionIndex + 2;
					bool force = args.Length > forceIndex && args[forceIndex] == "-f";

					if (!ExtractZipFromEmbeddedResource("Mahi.Templates.authentication.zip", Environment.CurrentDirectory, force))
						return true;

					Console.WriteLine(Resources.Message_CreateTemplateSuccess);
					return true;
				}
			}

			Console.WriteLine(string.Format(Resources.Error_CreateTemplateNameNotFound, args[templateOptionIndex + 1]));
			return true;
		}

		private static int ArgumentIndex(string argument, string[] args)
		{
			for (int i = 0; i < args.Length; i++)
				if (args[i] == argument)
					return i;
			return -1;
		}

		private static bool ExtractZipFromEmbeddedResource(string resourceName, string destinationFolder, bool force)
		{
			var assembly = Assembly.GetExecutingAssembly();
			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			{
				if (stream == null)
					throw new IOException(string.Format("Resource not found with name {0}!", resourceName));

				try
				{
					using (var zipArchive = new ZipArchive(stream))
						foreach (var entry in zipArchive.Entries)
						{
							string destinationPath = Path.Combine(destinationFolder, entry.FullName);

							if (entry.FullName.EndsWith("/"))
								Directory.CreateDirectory(destinationPath);
							else entry.ExtractToFile(destinationPath, force);
						}
				}
				catch (IOException)
				{
					Console.WriteLine(Resources.Error_CreateTemplateNeedToForce);
					return false;
				}
			}
			return true;
		}
	}
}
