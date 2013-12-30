//
// InstrumentorVisitor.cs
//
// Author:
//   Sergiy Sakharov (sakharov@gmail.com)
//
// (C) 2010 Sergiy Sakharov
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

using System.Collections.Generic;
using Coverage.Common;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections;
using System.IO;
using Mono.Cecil.Rocks;

namespace Coverage.Instrument
{
	/// <summary>
	/// Instruments assembly members
	/// Adds Counter.Hit method invocation to all methods
	/// Removes strong names from assemblies and references
	/// </summary>
	internal class InstrumentorVisitor : BaseVisitor
	{
		/// <summary>
		/// Modifies Assembly references
		/// </summary>
		public override void VisitAssembly(AssemblyDefinition assembly, Context context)
		{
			WeakenStrongReferences(context, assembly);
			//Weaken inline references in assembly-wide custom attributes
			SubstituteAttributeScopeReferences(context, assembly.CustomAttributes);

			if(context.ShouldInstrumentCurrentMember)
			{
				//Add reference to hit counter assembly
				assembly.MainModule.AssemblyReferences.Add(context.CounterAssemblyBuilder.CounterAssemblyDef.Name);
			}
		}

		/// <summary>
		/// Saves instrumented assembly
		/// </summary>
		public override void LastVisitAssembly(AssemblyDefinition assembly, Context context)
		{
			//Save counter assembly near instrumented one
			var path = context.CurrentAssemblyPath;
			context.CounterAssemblyBuilder.Save(Path.GetDirectoryName(path));

			//Save instrumented assembly with respect to NamingMode:
			var oldExt = Path.GetExtension(path);
			if (Configuration.NamingMode == Configuration.NamingModes.MarkInstrumented)
			{
                assembly.Write(Path.ChangeExtension(path, "Instrumented" + oldExt));
				return;
			}

		    var pdbPath = Path.ChangeExtension(path, "pdb");
		    var backupPath = Path.ChangeExtension(path, "backup_" + oldExt.Substring(1));
		    var backupPdbPath = Path.ChangeExtension(path, "backup_pdb");

            File.Delete(backupPath);
            File.Delete(backupPdbPath);
            File.Move(path, backupPath);
            File.Move(pdbPath, backupPdbPath);
            assembly.Write(path);

		    Configuration.CleanupCallback += delegate
            {
                File.Delete(path);
                File.Delete(pdbPath);
                File.Move(backupPath, path);
                File.Move(backupPdbPath, pdbPath);
            };
		}

		/// <summary>
		/// Creates counter method reference in module metadata
		/// </summary>
		public override void VisitModule(ModuleDefinition moduleDef, Context context)
		{
			if(context.ShouldInstrumentCurrentMember)
				context.CounterMethodRef = moduleDef.Import(context.CounterAssemblyBuilder.CounterMethodDef);

			//Weaken inline references in module-wide custom attributes
			SubstituteAttributeScopeReferences(context, moduleDef.CustomAttributes);
		}

		
		public override void VisitType(TypeDefinition typeDef, Context context)
		{
			//Weaken inline references in type-wide custom attributes
			SubstituteAttributeScopeReferences(context, typeDef.CustomAttributes);
		}

		/// <summary>
		/// Preparations for method instrumentation
		/// </summary>
		public override void VisitMethod(MethodDefinition methodDef, Context context)
		{
			if (context.ShouldInstrumentCurrentMember)
				context.MethodWorker = methodDef.Body.GetILProcessor();

			//Weaken inline references in method-wide custom attributes
			SubstituteAttributeScopeReferences(context, methodDef.CustomAttributes);
		}

		/// <summary>
		/// Fixes overflows in short branch instructions:
		/// </summary>
		public override void LastVisitMethod(MethodDefinition methodDef, Context context)
		{
			if (!context.ShouldInstrumentCurrentMember)
				return;

			//changes conditional (br.s, brtrue.s ...) operators to corresponding "long" ones (br, brtrue)
            methodDef.Body.SimplifyMacros();
			//changes "long" conditional operators to their short representation where possible
            methodDef.Body.OptimizeMacros();
		}

		/// <summary>
		/// Instruments method instruction, that has corresponging segment of source code
		/// </summary>
		/// <param name="instruction">instruction that should be preceded by counter invocation</param>
		/// <param name="segment"></param>
		/// <param name="context"></param>
		public override void VisitMethodPoint(Instruction instruction, CodeSegment segment, Context context)
		{
			if (!context.ShouldInstrumentCurrentMember)
				return;

			var counterMethodCall = context.MethodWorker.Create(OpCodes.Call, context.CounterMethodRef);
			var instrLoadModuleId = context.MethodWorker.Create(OpCodes.Ldstr, context.CurrentModuleId);
			var instrLoadPointId = context.MethodWorker.Create(OpCodes.Ldc_I4, context.CurrentPointId);
 
			context.MethodWorker.InsertBefore(instruction, instrLoadModuleId);
			context.MethodWorker.InsertAfter(instrLoadModuleId, instrLoadPointId);
			context.MethodWorker.InsertAfter(instrLoadPointId, counterMethodCall);

			//Change references in operands from "instruction" to first counter invocation instruction (instrLoadModuleId)
			foreach (Instruction instr in context.MethodWorker.Body.Instructions)
			{
				SubstituteInstructionOperand(instr, instruction, instrLoadModuleId);
			}

            foreach (ExceptionHandler handler in context.MethodWorker.Body.ExceptionHandlers)
			{
				SubstituteExceptionBoundary(handler, instruction, instrLoadModuleId);
			}
		}

		#region Helpers
		private static void SubstituteExceptionBoundary(ExceptionHandler handler, Instruction oldBoundary, Instruction newBoundary)
		{
//			if (handler.Filter == oldBoundary)
//				handler.Filter = newBoundary;

			if (handler.FilterStart == oldBoundary)
				handler.FilterStart = newBoundary;

			if (handler.HandlerEnd == oldBoundary)
				handler.HandlerEnd = newBoundary;

			if (handler.HandlerStart == oldBoundary)
				handler.HandlerStart = newBoundary;

			if (handler.TryEnd == oldBoundary)
				handler.TryEnd = newBoundary;

			if (handler.TryStart == oldBoundary)
				handler.TryStart = newBoundary;
		}

		private static void SubstituteInstructionOperand(Instruction instruction, Instruction oldOperand, Instruction newOperand)
		{
			var ondType = instruction.OpCode.OperandType;
			//Performance reasons - only 3 types of operators have operands of Instruction types
			//instruction.Operand getter - is rather slow to execute it for every operator
			var isConditional = ondType == OperandType.InlineBrTarget ||
				ondType == OperandType.ShortInlineBrTarget ||
				ondType == OperandType.InlineSwitch;

			if (!isConditional)
				return;

			if (instruction.Operand == oldOperand)
			{
				instruction.Operand = newOperand;
				return;
			}

			//At this point instruction.Operand will be either Operand != oldOperand
			//or instruction.Operand will be of type Instruction[]
			//(in other words - it will be a switch operator's operand)
			if(ondType != OperandType.InlineSwitch)
				return;

			var opserands = (Instruction[])instruction.Operand;
			for (var i = 0; i < opserands.Length; i++)
			{
				if (opserands[i] == oldOperand)
				{
					opserands[i] = newOperand;
				}
			}
		}

		/// <summary>
		/// Weakens assembly strong name
		/// Weakens references to other instrumented assemblies
		/// inside current assembly metadata
		/// </summary>
		private static void WeakenStrongReferences(Context context, AssemblyDefinition assembly)
		{
			context.AssemblyReferenceSubstitutions.Clear();
			assembly.Name.PublicKey = null;
			assembly.Name.PublicKeyToken = null;
			assembly.Name.HasPublicKey = false;

			foreach (AssemblyNameReference reference in assembly.MainModule.AssemblyReferences)
			{
				if (!context.AssemblyNames.Contains(reference.Name))
					continue;

				var original = reference.ToString();

				reference.HasPublicKey = false;
				reference.PublicKeyToken = null;
				reference.PublicKey = null;

				context.AssemblyReferenceSubstitutions[original] = reference.ToString();
			}
		}

		private static void SubstituteAttributeScopeReferences(Context context, IEnumerable<CustomAttribute> customAttributes)
		{
			foreach (CustomAttribute attr in customAttributes)
			{
				SubstituteAttributeParameterScopeReferences(context, attr.ConstructorArguments/*ConstructorParameters*/);
				SubstituteAttributeParameterScopeReferences(context, attr.Properties);
			}
		}

		private static void SubstituteAttributeParameterScopeReferences(Context context, IList parameters)
		{
			for (var i = 0; i < parameters.Count; i++)
			{
				var parameter = parameters[i] as string;
				if (parameter == null)
					continue;

				var oldReferences = context.AssemblyReferenceSubstitutions.Keys.Where(parameter.Contains);
				if (oldReferences.Count() == 0)
					continue;

				foreach (var oldReference in oldReferences)
				{
					//In this version of cecil attribute constructor parameters are represented as strings
					// So we have to replace strong named strings into their weak name representation
					parameters[i] = parameter.Replace(oldReference, context.AssemblyReferenceSubstitutions[oldReference]);
				}
			}
		}

		private static void SubstituteAttributeParameterScopeReferences(Context context, IDictionary parameters)
		{
			foreach (var key in parameters.Keys)
			{
				var parameter = parameters[key] as string;
				if (parameter == null)
					continue;

				var oldReferences = context.AssemblyReferenceSubstitutions.Keys.Where(parameter.Contains);
				if (oldReferences.Count() == 0)
					continue;

				foreach (var oldReference in oldReferences)
				{
					//In this version of cecil attribute parameters are represented as strings
					// So we have to replace strong named strings into their weak name representation
					parameters[key] = parameter.Replace(oldReference, context.AssemblyReferenceSubstitutions[oldReference]);
				}
			}
		}
		#endregion Helpers
	}
}