//
// ModuleEntry.cs
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
using System.Text;

namespace Coverage.Report
{
	public class ModuleEntry
	{
		public ModuleEntry(string moduleId, string name, string assembly, string assemblyFullName)
		{
			ModuleId = moduleId;
			Name = name;
			Assembly = assembly;
			AssemblyIdentity = assemblyFullName;
		}

		public string ModuleId { get; set; }
		public string Name { get; set; }
		public string Assembly { get; set; }
		public string AssemblyIdentity { get; set; }

		public readonly List<MethodEntry> Methods = new List<MethodEntry>();

		public string GetXml()
		{
			var sb = new StringBuilder();
			sb.AppendFormat(
				@"<module moduleId=""{0}"" name=""{1}"" assembly=""{2}"" assemblyIdentity=""{3}"">",
				ModuleId,
				Name,
				Assembly,
				AssemblyIdentity
				);
			sb.AppendLine();
			foreach (var method in Methods)
			{
				sb.AppendLine(method.GetXml());
			}
			sb.Append("</module>");

			return sb.ToString();
		}
	}
}