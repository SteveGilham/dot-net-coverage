//
// Executor.cs
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

using System.Linq;
using Mono.Cecil;

namespace Coverage.Common
{
	/// <summary>
	/// Executes enumeration of visits to assemblies' member definitions
	/// down to methods and methods' sequence points
	/// that have corresponding entries in pdb files
	/// </summary>
	/// TODO: Visit all fields/properties in order to check "indirect"
	/// references to strongly typed assemblies in custom attributes
	internal class Executor
	{
		/// <summary>
		/// Special debug-purpose marker
		/// Sequence points marked by this marker
		/// are skipped from enumeration
		/// </summary>
		private const int FeeFeeMarker = 0xFEEFEE;

		private readonly Context _context;
		private readonly string[] _assemblyPaths;

		private static bool IsFiltered<T>(T nameProvider)
			where T : class
		{
			return Configuration.NameFilters.Any(filter => filter.Match(nameProvider));
		}

		public Executor(string[] assemblyPaths)
		{
			_assemblyPaths = assemblyPaths.Where(path => !IsFiltered(path)).ToArray();
			_context = new Context(_assemblyPaths);
		}

		/// <summary>
		/// Executes visits
		/// </summary>
		public void Execute(BaseVisitor visitor)
		{
			foreach (var assemblyPath in _assemblyPaths)
			{
				ProcessAssembly(assemblyPath, visitor);
			}
			visitor.LastVisit(_context);
		}

		/// <summary>
		/// Executes visits to assembly and processes its modules
		/// </summary>
		private void ProcessAssembly(string assemblyPath, BaseVisitor visitor)
		{
			_context.SetCurrentAssembly(assemblyPath);

            var assemblyDef = AssemblyDefinition.ReadAssembly(assemblyPath);

			_context.SkipAssembly = IsFiltered(assemblyDef);
			visitor.VisitAssembly(assemblyDef, _context);

			foreach (ModuleDefinition moduleDef in assemblyDef.Modules)
			{
				ProcessModule(visitor, moduleDef);
			}

			visitor.LastVisitAssembly(assemblyDef, _context);
		}

		/// <summary>
		/// Executes visits to module and processes its types
		/// </summary>
		private void ProcessModule(BaseVisitor visitor, ModuleDefinition moduleDef)
		{
		    _context.CurrentModuleId = moduleDef.Mvid.ToString();
			_context.CurrentPointId = 0;
			visitor.VisitModule(moduleDef, _context);
			foreach (TypeDefinition typeDef in moduleDef.Types)
			{
				ProcessType(visitor, typeDef);
			}
		}

		/// <summary>
		/// Executes visits to type and processes its methods
		/// </summary>
		private void ProcessType(BaseVisitor visitor, TypeDefinition typeDef)
		{
			_context.SkipType = IsFiltered(typeDef);
			visitor.VisitType(typeDef, _context);

            foreach(var type2 in typeDef.NestedTypes)
            {
                ProcessType(visitor, type2);
            }

			//Take all methods and constructors definitions
            var methods = typeDef.Methods;//TODO: where are constructors: .Cast<MethodDefinition>().Union(typeDef.Methods.Constructors.Cast<MethodDefinition>());

			foreach (var methodDef in methods)
			{
				if (methodDef.IsAbstract || methodDef.IsRuntime || methodDef.IsPInvokeImpl)
					continue;

				ProcessMethod(visitor, methodDef);
			}
		}

		/// <summary>
		/// Visits all method sequence points that
		/// have corresponding source code segments
		/// locations in pdb files
		/// </summary>
		private void ProcessMethod(BaseVisitor visitor, MethodDefinition methodDef)
		{
			var segments = _context.CodeSegmentReader.GetSegmentsByMethod(methodDef);
			if (segments.Count == 0)
				return;

			_context.SkipMethod = IsFiltered(methodDef);
			visitor.VisitMethod(methodDef, _context);

			//Probably there is no need in sequence point enumeration
			//if this method should be skipped from instrumentation...
			//But still for some reason there is an "excluded" attribute defined
			//for sequence point in NCover's xml...
			//In case of this optimization following lines should be uncomented:
			//if(!_context.ShouldInstrumentCurrentMember)
			//	return;

			var instructions = methodDef.Body.Instructions;
			var instIdx = instructions.Count - 1;
			foreach (var offsetPointPair in segments.OrderByDescending(pair => pair.Key))
			{
				var pair = offsetPointPair;
				//take only these sequence poins that have corresponding code segment locations
				if (pair.Value.StartLine == FeeFeeMarker)
					continue;

				//Locate instruction by offset
				while (instIdx > 0 && (instructions[instIdx].Offset > pair.Key || instructions[instIdx].Offset == 0))
					instIdx--;
				var instruction = instructions[instIdx];
				if (instruction.Offset != pair.Key)
					continue;

				_context.CurrentPointId++;
				visitor.VisitMethodPoint(instruction, pair.Value, _context);
			}

			visitor.LastVisitMethod(methodDef, _context);
		}
	}
}