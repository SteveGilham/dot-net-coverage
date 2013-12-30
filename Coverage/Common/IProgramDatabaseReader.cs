//
// IProgramDatabaseReader.cs
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
using Mono.Cecil;

namespace Coverage.Common
{
	public interface IProgramDatabaseReader
	{
		/// <summary>
		/// Loads program database file that corresponds to assembly
		/// </summary>
		/// <param name="assemblyFilePath"></param>
		void Initialize(string assemblyFilePath);
	
		/// <summary>
		/// Retrieves source code segment locations with corresponding offsets in compiled assembly for given method
		/// </summary>
		/// <param name="methodDef">Method definition</param>
		/// <returns>Dictionary: Key is an instruction offset, Value - source code segment location</returns>
        IDictionary<int, CodeSegment> GetSegmentsByMethod(MethodDefinition methodDef);
	}
}