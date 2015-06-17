using System;
using System.Diagnostics;
using Core.Common;
using System.IO;
using Core.IO;

namespace TravelMap
{
	public class ShellScript
	{
		readonly string RootDirectory;

		public ShellScript ()
		{
			RootDirectory = Path.GetTempPath ();
		}

		public void WriteAllText (string path, string contents)
		{
			File.WriteAllText (PathHelper.CombinePath (RootDirectory, path), contents);
		}

		public void ExecuteScript (string path, Action<string> receiveOutput = null, bool verbose = true, bool debug = true, bool sudo = false, bool ignoreEmptyLines = false)
		{
			// Use ProcessStartInfo class
			ProcessStartInfo startInfo = new ProcessStartInfo () {
				CreateNoWindow = false,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
			};
			startInfo.EnvironmentVariables ["LC_ALL"] = "C";
			if (sudo) {
				startInfo.FileName = "/usr/bin/sudo";
				startInfo.Arguments = "/bin/bash -x \"" + PathHelper.CombinePath (RootDirectory, path) + "\"";
			} else {
				startInfo.FileName = "/bin/bash";
				startInfo.Arguments = "-x \"" + PathHelper.CombinePath (RootDirectory, path) + "\"";
			}

			try {
				using (Process process = new Process ()) {
					process.EnableRaisingEvents = true;
					process.StartInfo = startInfo;
					Action<object, DataReceivedEventArgs> actionWrite = (sender, e) => {
						if (ignoreEmptyLines && string.IsNullOrWhiteSpace (e.Data)) {
							// the line is empty, ignore it
						} else {
							if (verbose && (debug || !e.Data.StartsWith ("+ "))) {
								Log.Debug ("    ", e.Data);
							}
							if (receiveOutput != null && !e.Data.StartsWith ("+ ")) {
								receiveOutput (e.Data);
							}
						}
					};

					process.ErrorDataReceived += (sender, e) => actionWrite (sender, e);
					process.OutputDataReceived += (sender, e) => actionWrite (sender, e);

					Log.Debug ("Start Process (executable='", PathHelper.CombinePath (RootDirectory, path), "')");
					process.Start ();
					process.BeginOutputReadLine ();
					process.BeginErrorReadLine ();
					process.WaitForExit ();
					Log.Debug ("Stop Process");
				}
			} catch (Exception e) {
				Log.Error (e);
			}
		}

	}
}

