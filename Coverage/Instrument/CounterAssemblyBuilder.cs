//
// CounterAssemblyBuilder.cs
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
using System.IO;
using System.Linq.Expressions;
using Coverage.Common;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace Coverage.Instrument
{
	/// <summary>
	/// Creates custom Coverage.Counter assembly
	/// This assembly has build-in location of
	/// coverage xml file.
	/// Also provides method definition of a Hit
	/// method of counter in created assembly
	/// </summary>
	public class CounterAssemblyBuilder
	{
		private MethodDefinition _counterMethodDef;
		private AssemblyDefinition _counterAssemblyDef;
		
		/// <summary>
		/// Gets Hit method definition
		/// </summary>
		public MethodDefinition CounterMethodDef
		{
			get
			{
				if(_counterMethodDef == null)
				{
					//Retrieving MethodInfo using expressions will guarantee compile time error if method signature changes
					Expression<Action<string, int>> counterExpression = (moduleId, id) => Counter.Hit(moduleId, id);
					var counterMethodToken = ((MethodCallExpression)counterExpression.Body).Method.MetadataToken;
                    _counterMethodDef = (MethodDefinition)CounterAssemblyDef.MainModule.LookupToken(counterMethodToken);
				}

				return _counterMethodDef;
			}
		}
		
		/// <summary>
		/// Gets counter assembly definition with auto-"hardcoded" coverage file location
		/// </summary>
		public AssemblyDefinition CounterAssemblyDef
		{
			get
			{
				if (_counterAssemblyDef == null)
				{
					_counterAssemblyDef = AssemblyDefinition.ReadAssembly(typeof(Counter).Assembly.Location);
					_counterAssemblyDef.Name.Name += ".Gen";

					SetCoverageFilePath(Path.GetFullPath(Configuration.CoverageFile));
				}

				return _counterAssemblyDef;
			}
		}

		private void SetCoverageFilePath(string coverageReportPath)
		{
			//Retrieving PropertyInfo using expressions
			Expression<Func<string>> coverageFileResolver = () => Counter.CoverageFilePath;

			//Modifying getter of the coverage file path - adding 2 instructions at the beginning: loadstr /new path/ and ret.
			//Old instructions become unreachable...
			/*
			 * TODO: Awaiting Mono.Cecil fix
			 * var pathAccessorToken = ((MemberExpression)coverageFileResolver.Body).Member.MetadataToken;
			 * var pathAccessorDef = (PropertyDefinition)CounterAssemblyDef.MainModule.LookupByToken(new MetadataToken(pathAccessorToken));
			 * var pathGetterDef = pathAccessorDef.GetMethod;
			*/

			var pathGetterToken = ((PropertyInfo)((MemberExpression)coverageFileResolver.Body).Member).GetGetMethod().MetadataToken;
			var pathGetterDef = (MethodDefinition)CounterAssemblyDef.MainModule.LookupToken(pathGetterToken);

            var worker = pathGetterDef.Body.GetILProcessor();
			worker.InsertBefore(pathGetterDef.Body.Instructions[0], worker.Create(OpCodes.Ret));
			worker.InsertBefore(pathGetterDef.Body.Instructions[0], worker.Create(OpCodes.Ldstr, coverageReportPath));
		}

		/// <summary>
		/// Saves customized Counter assembly to specified location
		/// </summary>
		/// <param name="path">Path</param>
		public void Save(string path)
		{
			var counterAssemblyFile = Path.Combine(path, CounterAssemblyDef.Name.Name + ".dll");
            CounterAssemblyDef.Write(counterAssemblyFile);

            Configuration.CleanupCallback += () => File.Delete(counterAssemblyFile);
		}
	}
}