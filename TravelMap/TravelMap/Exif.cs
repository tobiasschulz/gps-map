using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using Core.Math;
using Core.Shell.Common.FileSystems;

namespace TravelMap
{
	public class Exif
	{
		readonly ShellScript shellScript = new ShellScript ();

		public Exif ()
		{
		}

		public List<ExifTag> GetExifTags (VirtualFile virtualFile)
		{
			if (string.IsNullOrWhiteSpace (virtualFile.Path.RealPath)) {
				return new List<ExifTag> ();
			} else {
				return GetExifTags (virtualFile.Path.RealPath);
			}
		}

		public List<ExifTag> GetExifTags (string fullPath)
		{
			//string script = "exiftool -time:all -a -G0:1 -s " + fullPath.SingleQuoteShell ();
			string script = "exiftool -a -G0:1 -s " + fullPath.SingleQuoteShell ();

			List<ExifTag> tags = new List<ExifTag> ();
			Action<string> receiveOutput = line => {
				// example: [EXIF:ExifIFD]  DateTimeOriginal                : 2014:10:23 22:45:11

				if (!line.Contains ("ICC_Profile")) {
					ExifTag tag;
					if (ExifTag.ReadFromExiftoolConsoleOutput (line, out tag)) {
						tags.Add (tag);
						Log.Debug ("- ", tag.Name, ": ", tag.Value);
					}
				}
			};

			shellScript.WriteAllText (path: "run1.sh", contents: script);


			Log.Debug ("EXIF tags:");
			Log.Indent++;
			shellScript.ExecuteScript (path: "run1.sh", receiveOutput: receiveOutput, ignoreEmptyLines: true, verbose: false);
			Log.Indent--;

			return tags;
		}

		public PortableLocation GetLocation (VirtualFile file)
		{
			PortableLocation location = new PortableLocation ();
			location.ReferenceFile = file.Path.FileName;
			location.Provider = "exif";

			List<ExifTag> tags = GetExifTags (file);
			foreach (ExifTag tag in tags) {
				if (tag.Name == "GPSPosition") {
					string[] latlong = tag.Value.Split (',');
					if (latlong.Length == 2) {
						string latitude = latlong [0];
						string longitude = latlong [1];
						location.Latitude = LocationMath.DegreesFromString (str: latitude);
						location.Longitude = LocationMath.DegreesFromString (str: longitude);
					}
				}
				if (tag.Name == "GPSDateTime") {
					// like "2015:06:05 14:37:22Z"
					string dateString = tag.Value.TrimEnd ('Z');
					string format = "yyyy:MM:dd HH:mm:ss";
					location.DateTime = DateTime.ParseExact (s: dateString, format: format, provider: System.Globalization.CultureInfo.InvariantCulture);
				}
			}

			if (Math.Abs (location.Latitude) > 0.001 && Math.Abs (location.Longitude) > 0.001) {
				return location;
			} else {
				return null;
			}
		}

		public void SetExifDate (string fullPath, DateTime date)
		{
			string dateString = string.Format ("{0:yyyy:MM:dd HH:mm:ss}", date); //JJJJ:MM:TT HH:MM:SS
			string script = "exiftool -AllDates='" + dateString + "' '" + fullPath + "' && rm -f " + fullPath.SingleQuoteShell () + "_original";

			shellScript.WriteAllText (path: "run2.sh", contents: script);
			shellScript.ExecuteScript (path: "run2.sh", ignoreEmptyLines: true);
		}

		public void CopyExifTags (string sourcePath, string destPath)
		{
			string script = "exiftool -TagsFromFile " + sourcePath.SingleQuoteShell () + " " + destPath.SingleQuoteShell ()
			                + " && rm -f " + destPath.SingleQuoteShell () + "_original ";

			shellScript.WriteAllText (path: "run3.sh", contents: script);
			shellScript.ExecuteScript (path: "run3.sh", ignoreEmptyLines: true, verbose: false);
		}
	}

	public sealed class ExifTag
	{
		public string Name;
		public string Value;

		private ExifTag (string name, string value)
		{
			Name = name;
			Value = value;
		}

		public static bool ReadFromExiftoolConsoleOutput (string line, out ExifTag tag)
		{
			tag = null;
			string[] parts = line.Split (new []{ ": " }, 2, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 2) {
				string value = parts [1].Trim ();
				string[] parts2 = parts [0].Trim ().Split (new char[]{ ']' }, 2, StringSplitOptions.RemoveEmptyEntries);
				if (parts2.Length == 2) {
					string key = parts2 [1].Trim ();
					string type = parts2 [0].Trim ().Trim ('[');
					if (type != "File:System") {
						tag = new ExifTag (key, value);
					}
				} else {
					Log.Debug ("Invalid key in exiftool output line: ", parts [0].Trim ());
				}
			} else {
				Log.Debug ("Invalid exiftool output line: ", line);
			}
			return tag != null;
		}

		public bool Serialize (out string key, out string value)
		{
			key = "exif:" + Name;
			value = Value;
			return true;
		}

		public static bool Deserialize (string key, string value, out ExifTag deserialized)
		{
			if (key.StartsWith ("exif:")) {
				deserialized = new ExifTag (name: key.Substring (5), value: value);
				return true;
			} else {
				deserialized = null;
				return false;
			}
		}

		public string Serialize ()
		{
			return "exif:" + Name + ":=" + Value;
		}

		public static bool Deserialize (string keyAndValue, out ExifTag deserialized)
		{
			string[] splittedLine = keyAndValue.Split (new [] { ":=" }, 2, StringSplitOptions.RemoveEmptyEntries);
			if (splittedLine.Length == 2) {
				return Deserialize (splittedLine [0], splittedLine [1], out deserialized);
			} else {
				deserialized = null;
				return false;
			}
		}
	}

	public static class ExifTagExtensions
	{
		public static string Serialize (List<ExifTag> tags)
		{
			return string.Join ("\n", tags.Select (tag => tag.Serialize ()));
		}

		public static List<ExifTag> Deserialize (string serialized)
		{
			return serialized.Split (new [] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Select (line => {
				ExifTag tag;
				ExifTag.Deserialize (keyAndValue: line, deserialized: out tag);
				return tag;
			}).Where (tag => tag != null).ToList ();
		}
	}

	public static class QuoteExtensions
	{
		public static string SingleQuoteShell (this string str)
		{
			return "'" + str.Replace ("'", "'\"'\"'") + "'";
		}

		public static string DoubleQuoteShell (this string str)
		{
			return "\"" + str.Replace ("\"", "\"'\"'\"") + "\"";
		}
	}
}

