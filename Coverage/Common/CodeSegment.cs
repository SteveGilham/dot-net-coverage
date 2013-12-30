//
// CodeSegment.cs
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

namespace Coverage.Common
{
	/// <summary>
	/// Source code segment location.
	/// </summary>
	public struct CodeSegment
	{
        public CodeSegment(int startColumn, int endColumn, int startRow, int endRow, string url)
		{
			StartLine = startRow;
			EndLine = endRow;
			StartColumn = startColumn;
			EndColumn = endColumn;
			Document = url;
		}

        public readonly int StartLine;
        public readonly int EndLine;
		public readonly int StartColumn;
		public readonly int EndColumn;
		public readonly string Document;
	}
}