//
// NameFilter.cs
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

using System.Linq;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Coverage.Common
{
	public class NameFilter
	{
		public enum FilterTypes : byte
		{
			FileFilter = (byte)'f',
			AssemblyFilter = (byte)'s',
			TypeFilter = (byte)'t',
			MethodFilter = (byte)'m',
			AttributeFilter = (byte)'a'
		}

		/// <summary>
		/// Specifies type of the member which name will be filtered.
		/// i.e. TypeFilter: all types with names that contain
		/// FilteredName will be skipped from instrumentation
		/// </summary>
		public FilterTypes Type { get; set; }

		public string FilteredName { get; set; }

		public bool Match<T>(T nameProvider)
			where T: class
		{
			switch(Type)
			{
				case FilterTypes.FileFilter:
					return nameProvider is string && MatchFile(nameProvider as string);

				case FilterTypes.AssemblyFilter:
					return nameProvider is AssemblyDefinition && MatchAssembly(nameProvider as AssemblyDefinition);

				case FilterTypes.TypeFilter:
					return nameProvider is TypeDefinition && MatchType(nameProvider as TypeDefinition);

				case FilterTypes.MethodFilter:
					return nameProvider is MethodDefinition && MatchMethod(nameProvider as MethodDefinition);

				default:
					return nameProvider is ICustomAttributeProvider && MatchAttribute(nameProvider as ICustomAttributeProvider);
			}
		}

		private bool MatchFile(string fileName)
		{
			return Path.GetFileName(fileName).Contains(FilteredName);
		}

		private bool MatchAssembly(AssemblyDefinition assembly)
		{
			return assembly.Name.Name.Contains(FilteredName);
		}

		private bool MatchType(TypeDefinition type)
		{
			return type.FullName.Contains(FilteredName);
		}

		private bool MatchMethod(MethodDefinition method)
		{
			return method.Name.Contains(FilteredName);
		}

		/// <summary>
		/// Checks if any of custom attributes types
		/// has a full name that should be filtered
		/// </summary>
		private bool MatchAttribute(ICustomAttributeProvider attributeProvider)
		{
			return attributeProvider.HasCustomAttributes &&
				attributeProvider.CustomAttributes.Cast<CustomAttribute>().
				Any(attr => 
					attr.Constructor.DeclaringType.FullName.Contains(FilteredName)
				);
		}
	}
}