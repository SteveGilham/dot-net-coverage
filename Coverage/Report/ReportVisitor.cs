//
// ReportVisitor.cs
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

using Coverage.Common;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.IO;

namespace Coverage.Report
{
	internal class ReportVisitor : BaseVisitor
	{
		/// <summary>
		/// Saves report file
		/// </summary>
		public override void LastVisit(Context context)
		{
			File.WriteAllText(Configuration.CoverageFile, context.ReportBuilder.GetXml());
		}

		/// <summary>
		/// Adds new module to report
		/// </summary>
		public override void VisitModule(ModuleDefinition moduleDef, Context context)
		{
			var module = new ModuleEntry(context.CurrentModuleId, moduleDef.Name, moduleDef.Assembly.Name.Name, moduleDef.Assembly.Name.FullName);
			context.ReportBuilder.AddModule(module);
		}

		/// <summary>
		/// Adds new method to report
		/// </summary>
		public override void VisitMethod(MethodDefinition methodDef, Context context)
		{
			var method = new MethodEntry(methodDef.Name, methodDef.DeclaringType.FullName, context.ShouldInstrumentCurrentMember);
			context.ReportBuilder.AddMethod(method);
		}

		/// <summary>
		/// Adds new sequence point to report
		/// </summary>
		public override void VisitMethodPoint(Instruction instruction, CodeSegment segment, Context context)
		{
			var pointEntry = new PointEntry(
				segment.StartLine,
				segment.StartColumn,
				segment.EndLine,
				segment.EndColumn,
				segment.Document,
				context.ShouldInstrumentCurrentMember
			);
			
			context.ReportBuilder.AddPoint(pointEntry);
		}
	}
}