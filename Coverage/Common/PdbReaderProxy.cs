//
// PdbReaderProxy.cs
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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Cci.Pdb;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Coverage.Common
{
	/// <summary>
	/// Reads Microsoft pdb files using internal classes of Pdb2Mdb converter tool
	/// </summary>
	public class PdbReaderProxy : IProgramDatabaseReader
	{
//		private Dictionary<uint, PdbFunction> _pdbFunctions;
        private ISymbolReader _reader;

		public PdbReaderProxy(string assemblyFilePath)
		{
			Initialize(assemblyFilePath);
		}

		public void Initialize(string assemblyFilePath)
		{
			_reader = new Mono.Cecil.Pdb.PdbReaderProvider().GetSymbolReader(null, assemblyFilePath);
		    var res = (bool) _reader.GetType().GetMethod("PopulateFunctions", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_reader, new object[0]);
//            _pdbFunctions = PdbFile.LoadFunctions(pdbFileStream, true).ToDictionary(func => func.token, func => func);
		}

		/// <summary>
		/// Retrieves source code segment locations with corresponding offsets in compiled assembly
		/// </summary>
		/// <param name="methodDef">Method definition</param>
		/// <returns>Dictionary: Key is an instruction offset, Value - source code segment location</returns>
		public IDictionary<int, CodeSegment> GetSegmentsByMethod(MethodDefinition methodDef)
		{
		    var symbols = new MethodSymbols(methodDef.MetadataToken);
            _reader.Read(symbols);

            return symbols.Instructions.ToDictionary(
                inst => inst.Offset,
                inst => new CodeSegment(
                    inst.SequencePoint.StartColumn,
                    inst.SequencePoint.EndColumn,
                    inst.SequencePoint.StartLine,
                    inst.SequencePoint.EndLine,
                    inst.SequencePoint.Document.Url
                )
            );
		}
	}
}