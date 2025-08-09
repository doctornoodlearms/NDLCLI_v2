using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks.Sources;

namespace NoodleCLI_v2;
public class Manifest {

	System.Collections.Generic.List<string> dependencies = new System.Collections.Generic.List<string>(0);
	string buildPath = "";

	public List<string> Dependencies {
		get => dependencies;
		set => dependencies = value;
	}

	public string BuildPath {
		get => buildPath;
		set => buildPath = value;
	}

	public Manifest() {


	}

	public void AddDependency(string dependency) {

		if (dependencies.Contains(dependency)) return;
		if (!dependency.EndsWith(".dll")) return;

		dependencies.Add(dependency);
	}
	public void AddBuildPath(string path) {

		if (buildPath == "") buildPath = path;
	}

	public string Serialize() {

		return JsonSerializer.Serialize(this);
	}
	public override string ToString() {

		return $"BuildPath: {BuildPath}";
	}
}