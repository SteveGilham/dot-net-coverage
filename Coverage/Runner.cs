//
// Runner.cs
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
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Coverage.Common;
using Coverage.Instrument;
using Coverage.Report;
using Mono.Options;

namespace Coverage
{
	static class Runner
	{
		public static void Main(string[] args)
		{
			Configuration.Initialize();

			var assemblies = new List<string>();

			for (var i = 0; i < args.Length; i++)
			{
				switch (args[i])
				{
					case "//x"://next argument is a coverage file name
				        i++;
						if (i < args.Length)
							Configuration.CoverageFile = args[i];
                        break;
                    case "//r":
						Configuration.NamingMode = Configuration.NamingModes.BackupOriginals;
                        break;
                    case "//a":
                        i++;
                        if (i < args.Length)
                            assemblies.AddRange(args[i].Split(';').SelectMany(ResolveFilesByMask));
				        break;
                    case "//ea":
                        ExtractFilter(args, i++, NameFilter.FilterTypes.AttributeFilter);
				        break;
                    case "//et":
                        ExtractFilter(args, i++, NameFilter.FilterTypes.TypeFilter);
                        break;
                    case "//ef":
                        ExtractFilter(args, i++, NameFilter.FilterTypes.FileFilter);
                        break;
                    case "//eas":
                        ExtractFilter(args, i++, NameFilter.FilterTypes.AssemblyFilter);
                        break;
                    case "//em":
                        ExtractFilter(args, i++, NameFilter.FilterTypes.MethodFilter);
                        break;
                    case "/?":
                    case "/h":
						ShowHelp();
						return;
                    default:
                        if (!args[i].StartsWith("/") && i < args.Length - 1)
                        {
                            Configuration.Executable = args[i];
                            Configuration.NamingMode = Configuration.NamingModes.BackupOriginals;

                            i++;
                            if (i < args.Length)
                                Configuration.ExecutableArgs = args.Skip(i).ToArray();

                            i = args.Length;
                        }
                        break;
				}
			}

			var executor = new Executor(assemblies.ToArray());
			executor.Execute(new CompositeVisitor(new ReportVisitor(), new InstrumentorVisitor()));
			
			if(string.IsNullOrEmpty(Configuration.Executable))
				return;

            AppDomain.CurrentDomain.ExecuteAssembly(Configuration.Executable, Configuration.ExecutableArgs);

		    Configuration.CleanupCallback();
		}

	    private static void ExtractFilter(string[] args, int i, NameFilter.FilterTypes filterType)
	    {
	        if (i >= args.Length)
                return;

            Configuration.NameFilters
                .AddRange(args[i + 1]
                .Split(';')
                .Select(arg => new NameFilter { FilteredName = arg, Type = filterType }));
	    }

		private static IEnumerable<string> ResolveFilesByMask(string filePathMask)
		{
		    var dir = Path.GetDirectoryName(filePathMask);
            dir = string.IsNullOrEmpty(dir) ? Environment.CurrentDirectory : dir;
			var files = Directory.GetFiles(dir, Path.GetFileName(filePathMask));

			var acceptedFiles = files.Where(
					file => (Path.GetExtension(file) == ".dll" || Path.GetExtension(file) == ".exe")
                        && (!file.EndsWith(".backup.dll")) && (!file.EndsWith(".backup.exe"))
                        && File.Exists(file)
				);
		    return acceptedFiles;
		}

		private static void ShowHelp()
		{
			Console.WriteLine(
@"Usage: coverage.exe {<assemblyPaths>} [{-[<ExclusionType>:]NameFilter}] [<commands> [<commandArgs>]]

Exclusion Types:
    f:  filter files by name

    s:  filter assemblies by name
           This could be useful if strong name for some
           assembly should be weakened, however, coverage
           report is redundant for it

    t:  filter types by full name

    m:  filter methods by full name

    default (a:)  filter members by their custom attribute names

Commands:
    //r  If this command is selected - instrumented assemblies replace existing ones.
        Old assemblies are backed up along with corresponding pdb files

    //x  Path to a coverage xml file

Example command line to launch instrumenting:
    coverage myapp.exe myapp.lib.dll -CodeGeneratedAttribute -t:Test /r /x coverage2.xml

    This will generate instrumented myapp.exe and myapp.lib.dll, moving old assemblies into
    myapp.bak.exe and myapp.lib.bak.dll respectively. Members marked by attributes that contain
    'CodeGeneratedAttribute' in their name as well as types that contain 'Test' in their full names
    will be excluded from report");
		}
	}
}