using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace NoodleCLI_v2;

class Program {

	const string projectLibrariesFolder = ".ndllib";
	const string manifestFileName = "ndlmanifest.json";
	static string manifestFile = $"{projectLibrariesFolder}\\{manifestFileName}";

	static string CurrentLocation {
		get => Environment.CurrentDirectory;
	}


	static void Main(string[] args) {

		if (args.Length == 0) {

			CommandHelp();
			return;
		}

		switch (args[0].ToLower()) {

			case "add":

				CommandAdd(
					args.Length >= 2 ? args[1] : "" // NO MANIFEST;
				);
				return;

			case "list":

				CommandList();
				return;

			case "get":

				CommandGet(
					args.Length >= 2 ? args[1] : "" // SOURCE
				);
				return;
			case "remove":
				CommandRemove(
					args.Length >= 2 ? args[1] : "" // FILE
				);
				return;

			case "update":
				CommandUpdate();
				return;

			case "demo":
				CommandDemo();
				return;

			case "help":
				CommandHelp();
				return;

			case "manifest":
				CommandManifest(
					args.Length >= 2 ? args[1] : "" // BUILD PATH
					);
				return;
		}
	}

	static void CommandAdd(string noManifest = "") {

		string libFolder = GetLibrariesFolder();

		StreamReader reader = new StreamReader($"{CurrentLocation}\\{manifestFile}");
		string json = reader.ReadToEnd();
		reader.Close();

		Manifest? manifest = JsonSerializer.Deserialize<Manifest>(json);
		if (manifest == null) {

			Console.WriteLine($"Failed to parse Manifest: {CurrentLocation}\\{manifestFile}");
			return;
		}

		// Copy lib file
		FileInfo file = new FileInfo(manifest.BuildPath);
		string dirName = file.Name.Remove(file.Name.IndexOf(".dll"));
		Directory.CreateDirectory($"{GetLibrariesFolder()}\\{dirName}");
		file.CopyTo($"{libFolder}\\{dirName}\\{file.Name}", true);

		// Copy manifest file
		if (noManifest == "--no-manifest") return;
		file = new FileInfo($"{CurrentLocation}\\{manifestFile}");
		file.CopyTo($"{libFolder}\\{dirName}\\{manifestFileName}", true);
	}

	static void CommandList() {

		int pathLength = GetLibrariesFolder().Length;

		string value = "";

		foreach (string i in Directory.EnumerateDirectories(GetLibrariesFolder()).ToArray()) {

			value += i.Remove(0, pathLength + 1) + "\n";
		}
		if (value.Length <= 0) {
			Console.WriteLine("No Libraries Found");
			return;
		}
		value = value.Remove(value.Length - 1, 1);

		Console.WriteLine(value);
	}

	static void CommandGet(string source) {

		if (!source.EndsWith(".dll")) {
			source += ".dll";
		}
		string sourceName = source.Remove(source.IndexOf(".dll"));

		DirectoryInfo dir = new DirectoryInfo($"{CurrentLocation}\\{projectLibrariesFolder}");
		if (dir.Exists == false) {

			dir.Create();
		}

		if (File.Exists(GetLibrariesFolder() + $"\\{sourceName}\\{manifestFileName}")) {

			Manifest manifest = ParseManifest(GetLibrariesFolder() + $"\\{sourceName}\\{manifestFileName}");

			foreach (string dependency in manifest.Dependencies) {

				FileInfo fileDep = new FileInfo(GetLibrariesFolder() + $"\\{dependency.Remove(dependency.IndexOf(".dll"))}\\{dependency}");
				fileDep.CopyTo($"{CurrentLocation}\\{projectLibrariesFolder}\\{dependency}", true);
			}
		}

		FileInfo file = new FileInfo(GetLibrariesFolder() + $"\\{sourceName}\\{source}");
		file.CopyTo($"{CurrentLocation}\\{projectLibrariesFolder}\\{source}", true);
		Console.WriteLine("Please Rebuild Manifest");
	}

	static void CommandRemove(string source) {

		string fileName = $"{CurrentLocation}\\{projectLibrariesFolder}\\{source}";

		FileInfo file = new FileInfo(fileName);
		if (file.Exists) {
			file.Delete();
			Console.WriteLine("Please Rebuild Manifest");
		}
	}
	static void CommandUpdate() {

		foreach (string i in Directory.EnumerateFiles($"{CurrentLocation}\\{projectLibrariesFolder}").ToArray()) {

			string[] dirPath = i.Split("\\");

			string fileName = dirPath[dirPath.Count() - 1];

			FileInfo file = new FileInfo(GetLibrariesFolder() + $"\\{fileName}");
			file.CopyTo($"{Environment.CurrentDirectory}\\{projectLibrariesFolder}\\{fileName}", true);
			Console.WriteLine($"Updated: {fileName}");
		}
	}
	static void CommandManifest(string buildPath) {

		Manifest manifest = ParseManifest($"{CurrentLocation}\\{manifestFile}");

		if (Directory.Exists($"{CurrentLocation}\\{projectLibrariesFolder}")) {

			foreach (string i in Directory.EnumerateFiles($"{CurrentLocation}\\{projectLibrariesFolder}").ToArray()) {

				string[] dirPath = i.Split("\\");

				string fileName = dirPath[dirPath.Count() - 1];

				manifest.AddDependency(fileName);
			}
		}
		else {

			Directory.CreateDirectory($"{CurrentLocation}\\{projectLibrariesFolder}");
		}
		manifest.AddBuildPath(buildPath);
		File.WriteAllText($"{CurrentLocation}\\{manifestFile}", manifest.Serialize());
	}
	static void CommandHelp() {
		Console.WriteLine("------------------------");
		Console.WriteLine("Add: Adds a new file to the Library folder\nndllib add [Optional: --no-manifest]\n");
		Console.WriteLine("Get: Copies the specified file from the Library folder to the working directory\nndllib get [FileName]\n");
		Console.WriteLine("List: Lists the files added to the Library folder\nndllib list\n");
		Console.WriteLine("Remove: Removes the specified file from the working directory\nndllib remove [FileName]\n");
		Console.WriteLine("Update: Overwrites the added libraries for the working directory\nndllib update\n");
		Console.WriteLine("Manifest: Builds the manifest file used by add and get\nnndllib manifest [Optional: BuildPath]\n");
		Console.WriteLine($"All files added to a project are added into a new folder '{projectLibrariesFolder}'");
		Console.WriteLine("------------------------");
	}

	static void CommandDemo() {
		Console.WriteLine("Demo");
	}

	static string GetLibrariesFolder() {

		return Environment.GetEnvironmentVariable("USERPROFILE") + "\\.ndllib";
	}

	static Manifest ParseManifest(string manifestPath) {

		if (File.Exists(manifestPath) == false) return new Manifest();


		StreamReader reader = new StreamReader(manifestPath);
		string json = reader.ReadToEnd();
		reader.Close();
		Manifest? manifest = JsonSerializer.Deserialize<Manifest>(json);

		if (manifest == null) {

			throw new Exception("Failed To Parse Manifest");
		}
		return manifest;
	}
}
