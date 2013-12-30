//
// CoverageReport.cs
//
// Author:
//   Sergiy Sakharov (sakharov@gmail.com)
//
// (C) 2009 Sergiy Sakharov
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Coverage.Report
{
	/// <summary>
	/// Coverage XML report builder
	/// </summary>
	public class CoverageReportBuilder
	{
		public CoverageReportBuilder()
		{
			ProfilerVersion = 0;
			DriverVersion = 0;
			StartTime = DateTime.MaxValue;
			MeasureTime = DateTime.MinValue;
		}

		private int ProfilerVersion { get; set; }
		private int DriverVersion { get; set; }
		private DateTime StartTime { get; set; }
		private DateTime MeasureTime { get; set; }

		public readonly List<ModuleEntry> Modules = new List<ModuleEntry>();

		private ModuleEntry _currentModule;
		private MethodEntry _currentMethod;

		/// <summary>
		/// Add new module to report.
		/// Set this module as current
		/// </summary>
		public void AddModule(ModuleEntry newModule)
		{
			Modules.Add(newModule);
			_currentModule = newModule;
		}

		/// <summary>
		/// Add new method to current module.
		/// Set method as current
		/// </summary>
		public void AddMethod(MethodEntry newMethod)
		{
			_currentModule.Methods.Add(newMethod);
			_currentMethod = newMethod;
		}

		/// <summary>
		/// Add new sequence point to report
		/// </summary>
		/// <param name="newPoint"></param>
		public void AddPoint(PointEntry newPoint)
		{
			_currentMethod.Points.Add(newPoint);
		}

		public string GetXml()
		{
			var sb = new StringBuilder();
			sb.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
			sb.AppendLine(@"<?xml-stylesheet href=""coverage.xsl"" type=""text/xsl""?>");
			sb.AppendFormat(
				@"<coverage profilerVersion=""{0}"" driverVersion=""{1}"" startTime=""{2}"" measureTime=""{3}"">",
				ProfilerVersion,
				DriverVersion,
				StartTime.ToString("o"),
				MeasureTime.ToString("o")
			);
			sb.AppendLine();
			foreach(var module in Modules)
			{
				sb.AppendLine(module.GetXml());
			}
			sb.Append("</coverage>");

			return sb.ToString();
		}
	}
}