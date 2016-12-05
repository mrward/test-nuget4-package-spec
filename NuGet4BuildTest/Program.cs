//
// Program.cs
//
// Author:
//       Matt Ward <matt.ward@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using NuGet.ProjectModel;

namespace NuGet4BuildTest
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			try {
				string fileName = Path.GetFullPath (@"../../../NetCoreConsoleTest/NetCoreConsoleTest.csproj");
				//string fileName = Path.GetFullPath(@"../../../ProjectJsonLibTest/ProjectJsonLibTest.csproj");
				RunMSBuild (fileName);

				//ReadProjectJson(fileName);
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}

		static void RunMSBuild (string projectFileName)
		{
			string projectName = Path.GetFileNameWithoutExtension(projectFileName);
			string directory = Path.GetDirectoryName (typeof (MainClass).Assembly.Location);
			string nugetTargetsFileName = Path.Combine (directory, "NuGet.targets");
			string nugetBuildTasksFileName = Path.Combine (directory, "NuGet.Build.Tasks.dll");
			string resultsFileName = Path.Combine (directory, string.Format ("results-{0}.json", projectName));

			var args = new StringBuilder ();
			args.Append ("/t:GenerateRestoreGraphFile ");
			args.Append ("/nologo /nr:false /p:RestoreUseCustomAfterTargets=true ");
			args.Append ("/p:BuildProjectReferences=false ");
			args.AppendFormat ("/p:NuGetRestoreTargets=\"{0}\" ", nugetTargetsFileName);
			args.AppendFormat ("/p:RestoreTaskAssemblyFile=\"{0}\" ", nugetBuildTasksFileName);
			args.AppendFormat ("/p:RestoreGraphOutputPath=\"{0}\" ", resultsFileName);
			args.Append ("/p:ExcludeRestorePackageImports=true ");
			args.AppendFormat ("/p:RestoreGraphProjectInput=\"{0}\" ", projectFileName);
			args.AppendFormat ("\"{0}\"", nugetTargetsFileName);

			var startInfo = new ProcessStartInfo ();
			startInfo.FileName = "msbuild";
			startInfo.Arguments = args.ToString ();
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
			Process p = Process.Start (startInfo);
			p.OutputDataReceived += P_OutputDataReceived;
			p.ErrorDataReceived += P_ErrorDataReceived;
			p.WaitForExit ();
			if (p.ExitCode != 0) {
				Console.WriteLine ("MSBuild failed");
			} else {
				var spec = DependencyGraphSpec.Load (resultsFileName);
				var projectSpec = spec.GetProjectSpec (projectFileName);
				int c = projectSpec.Dependencies.Count;
			}
		}

		static void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			P_OutputDataReceived(sender, e);
		}

		static void P_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data != null)
			{		
				Console.WriteLine(e.Data);
			}
		}

		static void ReadProjectJson (string projectFileName)
		{
			string projectName = Path.GetFileNameWithoutExtension (projectFileName);
			string directory = Path.GetDirectoryName (projectFileName);
			string projectJsonFileName = Path.Combine (directory, "project.json");

			var spec = JsonPackageSpecReader.GetPackageSpec (projectName, projectJsonFileName);
			int c = spec.Dependencies.Count;
		}
	}
}
