//
// Counter.cs
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using System.Xml.Linq;
using System.Xml;

namespace Coverage
{
	/// <summary>
	/// This class collects and stores
	/// history of hits of sequence points
	/// </summary>
	public static class Counter
	{
		private static DateTime _startTime;
		private static DateTime _measureTime;
		static Counter()
		{
			_startTime = DateTime.Now;
			//These handlers execute flushing all hit counts to the xml file
			AppDomain.CurrentDomain.DomainUnload += delegate { FlushCounter(); };
			AppDomain.CurrentDomain.ProcessExit += delegate { FlushCounter(); };
		}
		
		private static readonly Dictionary<string, Dictionary<int, int>> Hits = new Dictionary<string, Dictionary<int, int>>();
		private static readonly Mutex Mutex = new Mutex(false, "CoverageReportUpdate");

		/// <summary>
		/// Location of coverage xml file
		/// This property's IL code is modified to store actual file location
		/// </summary>
		public static string CoverageFilePath
		{
			get { return @"c:\temp\BINS\cReport.xml"; }
		}

		/// <summary>
		/// This method flushes hit count buffers.
		/// </summary>
		public static void FlushCounter()
		{
			if (Hits.Count == 0)
				return;

			KeyValuePair<string, Dictionary<int, int>>[] hitCounts;
			lock(Hits)
			{
				if(Hits.Count == 0)
					return;

				hitCounts = Hits.ToArray();
				Hits.Clear();
			}

			_measureTime = DateTime.Now;
			UpdateFileReport(hitCounts);
		}

		/// <summary>
		/// Save sequence point hit counts to xml report file
		/// </summary>
		static void UpdateFileReport(KeyValuePair<string, Dictionary<int, int>>[] hitCounts)
		{
			Mutex.WaitOne(10000);

			var flushStart = DateTime.Now;
			DateTime flushEnd;
			try
			{
				using (var coverageFile = new FileStream(CoverageFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.SequentialScan))
				{
					//Edit xml report to store new hits
					var xDoc = XDocument.Load(new XmlTextReader(coverageFile));

					var startTimeAttr = xDoc.Root.Attribute("startTime");
					var measureTimeAttr = xDoc.Root.Attribute("measureTime");
					var oldStartTime = DateTime.ParseExact(startTimeAttr.Value, "o", null);
					var oldMeasureTime = DateTime.ParseExact(measureTimeAttr.Value, "o", null);
					
					_startTime = _startTime < oldStartTime ? _startTime : oldStartTime; //Min
					_measureTime = _measureTime > oldMeasureTime ? _measureTime : oldMeasureTime; //Max

					startTimeAttr.SetValue(_startTime.ToString("o"));
					measureTimeAttr.SetValue(_measureTime.ToString("o"));

					foreach (var pair in hitCounts)
					{
						var moduleId = pair.Key;
						var moduleHits = pair.Value;
						var xModule = xDoc.Descendants("module").Where(el => el.Attribute("moduleId").Value == moduleId).First();

						var counter = 0;
						foreach (var pt in xModule.Descendants("seqpnt"))
						{
							counter++;
							if (!moduleHits.ContainsKey(counter))
								continue;

							var visits = int.Parse(pt.Attribute("visitcount").Value);
							pt.SetAttributeValue("visitcount", visits + moduleHits[counter]);
						}
					}
					
					//Save modified xml to a file
					coverageFile.Seek(0, SeekOrigin.Begin);
					var writer = XmlWriter.Create(coverageFile);
					xDoc.WriteTo(writer);
					writer.Flush();
				}
			}
			finally
			{
				flushEnd = DateTime.Now;
				Mutex.ReleaseMutex();
			}

			try
			{
				Console.WriteLine(
					"Coverage statistics flushing took {0:N} seconds",
					new TimeSpan(flushEnd.Ticks - flushStart.Ticks).TotalSeconds
				);
			}
			catch(Exception){}
		}

		/// <summary>
		/// This method is executed from instrumented assemblies.
		/// </summary>
		public static void Hit(string moduleId, int hitPointId)
		{
			lock (Hits)
			{
				if (!Hits.ContainsKey(moduleId))
				{
					Hits[moduleId] = new Dictionary<int,int>();
				}
				if (!Hits[moduleId].ContainsKey(hitPointId))
				{
					Hits[moduleId][hitPointId] = 0;
				}
				Hits[moduleId][hitPointId]++;
			}
		}
	}
}