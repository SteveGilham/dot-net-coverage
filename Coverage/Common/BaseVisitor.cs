//
// BaseVisitor.cs
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
	internal abstract class BaseVisitor
	{
		public virtual void VisitAssembly(AssemblyDefinition assembly, Context context) {}
		public virtual void VisitModule(ModuleDefinition moduleDef, Context context) { }
		public virtual void VisitType(TypeDefinition typeDef, Context context) { }
		public virtual void VisitMethod(MethodDefinition methodDef, Context context) { }
		public virtual void VisitMethodPoint(Instruction instruction, CodeSegment segment, Context context) { }
		public virtual void LastVisitMethod(MethodDefinition methodDef, Context context) { }
		public virtual void LastVisitAssembly(AssemblyDefinition assembly, Context context) { }
		public virtual void LastVisit(Context context) { }
	}
}