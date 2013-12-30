//
// Context.cs
//
// Author:
//   Sergiy Sakharov (sakharov@gmail.com)
//
// (C) 2011 Sergiy Sakharov
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
using System.Linq;
using System.IO;
using Coverage.Instrument;
using Coverage.Report;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Coverage.Common
{
	/// <summary>
	/// Contains shared informations for visitors
	/// </summary>
	internal class Context
	{
		#region Filtering

		private bool _skipAssembly = false;
		internal bool SkipAssembly
		{
			private get { return _skipAssembly; }
			set { _skipAssembly = value; SkipType = false; }
		}

		private bool _skipType = false;
		internal bool SkipType
		{
			private get { return _skipType; }
			set { _skipType = value; SkipMethod = false; }
		}

		internal bool SkipMethod { private get; set; }

		internal bool ShouldInstrumentCurrentMember
		{
			get { return !SkipAssembly && !SkipType && !SkipMethod; }
		}

		#endregion Filtering

		#region Global Context

		internal string[] AssemblyNames { get; private set; }
		internal CoverageReportBuilder ReportBuilder { get; private set; }
		internal CounterAssemblyBuilder CounterAssemblyBuilder { get; private set; }

		internal Context(string[] assemblyPaths)
		{
			AssemblyNames = assemblyPaths.Select(path => Path.GetFileNameWithoutExtension(path)).ToArray();
			ReportBuilder = new CoverageReportBuilder();
			CounterAssemblyBuilder = new CounterAssemblyBuilder();
			AssemblyReferenceSubstitutions = new Dictionary<string, string>();
		}

		#endregion Global Context

		#region Assembly Context

		internal string CurrentAssemblyPath { get; private set;}
		internal void SetCurrentAssembly(string currentAssemblyPath)
		{
			CurrentAssemblyPath = currentAssemblyPath;
			CodeSegmentReader = new PdbReaderProxy(currentAssemblyPath);
		}

		internal IProgramDatabaseReader CodeSegmentReader { get; private set; }
		internal Dictionary<string, string> AssemblyReferenceSubstitutions { get; private set;}

		#endregion Assembly Context

		#region Module Context

		internal string CurrentModuleId { get; set; }
		internal int CurrentPointId { get; set; }

		#endregion Module Context

		#region Method Context

		internal MethodReference CounterMethodRef { get; set; }
		internal ILProcessor MethodWorker { get; set; }

		#endregion Method Context
	}
}