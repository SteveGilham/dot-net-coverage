//
// MethodEntry.cs
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

using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Coverage.Report
{
	public class MethodEntry
	{
		public MethodEntry(string name, string clazz, bool instrumented)
		{
			Name = name;
			Class = clazz;
			Excluded = !instrumented;
			Instrumented = instrumented;
		}

		public string Name { get; set; }
		public bool Excluded { get; set; }
		public bool Instrumented { get; set; }
		public string Class { get; set; }

		public List<PointEntry> Points = new List<PointEntry>();

		public string GetXml()
		{
			var sb = new StringBuilder();
			sb.AppendFormat(
				@"<method name=""{0}"" excluded=""{1}"" instrumented=""{2}"" class=""{3}"">",
				HttpUtility.HtmlEncode(Name),
				Excluded.ToString().ToLower(),
				Instrumented.ToString().ToLower(),
				HttpUtility.HtmlEncode(Class)
				);
			sb.AppendLine();
			foreach (var point in Points)
			{
				sb.AppendLine(point.GetXml());
			}
			sb.Append("</method>");

			return sb.ToString();
		}
	}
}