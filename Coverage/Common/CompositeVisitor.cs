//
// CompositeVisitor.cs
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

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Coverage.Common
{
	internal class CompositeVisitor : BaseVisitor
	{
		private readonly BaseVisitor[] _visitors;
		public CompositeVisitor(params BaseVisitor[] visitors)
		{
			_visitors = visitors;
		}

		public override void VisitAssembly(AssemblyDefinition assembly, Context context)
		{
			foreach (var visitor in _visitors)
			{
				visitor.VisitAssembly(assembly, context);
			}
		}

		public override void VisitModule(ModuleDefinition moduleDef, Context context)
		{
			foreach (var visitor in _visitors)
			{
				visitor.VisitModule(moduleDef, context);
			}
		}

		public override void VisitType(TypeDefinition typeDef, Context context)
		{
			foreach (var visitor in _visitors)
			{
				visitor.VisitType(typeDef, context);
			}
		}

		public override void VisitMethod(MethodDefinition methodDef, Context context)
		{
			foreach (var visitor in _visitors)
			{
				visitor.VisitMethod(methodDef, context);
			}
		}

		public override void VisitMethodPoint(Instruction instruction, CodeSegment segment, Context context)
		{
			foreach (var visitor in _visitors)
			{
				visitor.VisitMethodPoint(instruction, segment, context);
			}
		}

		public override void LastVisitMethod(MethodDefinition methodDef, Context context)
		{
			foreach (var visitor in _visitors)
			{
				visitor.LastVisitMethod(methodDef, context);
			}
		}
		public override void LastVisitAssembly(AssemblyDefinition assembly, Context context)
		{
			foreach (var visitor in _visitors)
			{
				visitor.LastVisitAssembly(assembly, context);
			}
		}

		public override void LastVisit(Context context)
		{
			foreach (var visitor in _visitors)
			{
				visitor.LastVisit(context);
			}
		}
	}
}