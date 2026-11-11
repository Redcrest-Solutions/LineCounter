namespace LineCounter
{
    internal class Program
    {
        static Dictionary<string, string> KnownFileTypes = new Dictionary<string, string>
        {
            {".cs","C#" },
            {".cshtml","Razor" },
            {".js","Javascript" },
            {".less","LESS" },
            {".scss","SASS" },
            {".md","Markdown" },
            {".sh","Shell" },
            {".py","Python" },
            {".java","Java" }
        };

        static Dictionary<string, int> LinesPerDayFactor = new Dictionary<string, int>
        {
            {".cs",300 },
            {".cshtml",600 },
            {".js",300 },
            {".less",600 },
            {".scss",600 },
            {".md",300 },
            {".sh",100 },
            {".py",300 },
            {".java",300 },
        };

        static List<string> PathToIgnore = new List<string>
        {
            "/Migrations/",
            "/lib/",
            "/Release/",
            "/bin/",
            "/obj/"
        };

        static void Main(string[] args)
        {
            // Replace the / with whatever directory separator is needed
            PathToIgnore = PathToIgnore.Select(r => r.Replace("/", "" + Path.DirectorySeparatorChar)).ToList();

            var directories = new List<string>();
            for (var i = 0; i < args.Length; i++)
            {
                var a = args[i];
                switch (a)
                {
                    default:
                        directories.Add(a);
                        break;
                }
            }
            if (directories.Count == 0)
                directories.Add(Environment.CurrentDirectory);
            foreach (var d in directories)
            {
                if (!Directory.Exists(d))
                {
                    Console.WriteLine($"\"{d}\" doesn't exist.");
                    Environment.Exit(-1);
                }
            }

            Console.WriteLine("Checking file structure of " + string.Join(", ", directories.Select(d => "\"" + d + "\"")));
            var files = GetRecursiveFiles(directories).ToList();
            Console.WriteLine($"Found {files.Count} file(s) to count.");
            var nbRows = new Dictionary<string, int>();
            var nbFiles = new Dictionary<string, int>();
            foreach (var f in files)
            {
                var ext = f.Substring(f.LastIndexOf('.')).ToLowerInvariant();
                if (!nbRows.ContainsKey(ext))
                {
                    nbRows.Add(ext, 0);
                    nbFiles.Add(ext, 0);
                }
                nbRows[ext] += NbRows(f);
                nbFiles[ext]++;
            }
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("## Code base");
            Console.WriteLine("| Type | Files | Lines |");
            Console.WriteLine("|:--|--:|--:|");
            foreach (var f in nbFiles.Keys.OrderBy(r => KnownFileTypes[r]))
                Console.WriteLine("| " + KnownFileTypes[f] + " | " + NumberFormat(nbFiles[f]) + " | " + NumberFormat(nbRows[f]) + " |");

            Console.WriteLine("");
            Console.WriteLine("### Project total");
            Console.WriteLine("| Stats | Nb |");
            Console.WriteLine("|:--|--:|");
            Console.WriteLine("| Lines | " + NumberFormat(nbRows.Sum(r => r.Value)) + " |");
            Console.WriteLine("| Files | " + NumberFormat(nbFiles.Sum(r => r.Value)) + " |");
            Console.WriteLine("| Book pages | " + NumberFormat(nbRows.Sum(r => r.Value) / 65) + " |");
            Console.WriteLine("| Usual hours(s) | " + NumberFormat(nbRows.Sum(r => r.Value / LinesPerDayFactor[r.Key]) * 8) + " |");
            Console.WriteLine("| Usual day(s) | " + NumberFormat(nbRows.Sum(r => r.Value / LinesPerDayFactor[r.Key])) + " |");
            Console.WriteLine("| Usual weeks(s) | " + NumberFormat(nbRows.Sum(r => r.Value / LinesPerDayFactor[r.Key]) / 5) + " |");
            Console.WriteLine("| If self dev | " + NumberFormat(nbRows.Sum(r => r.Value / LinesPerDayFactor[r.Key]) * 477) + " CHF |");
            Console.WriteLine("| If outsourced dev | " + NumberFormat(nbRows.Sum(r => r.Value / LinesPerDayFactor[r.Key]) * 1440) + " CHF |");
        }

        static string NumberFormat(int src) => src.ToString("#,##0").Replace(",", "'");

        static IEnumerable<string> GetRecursiveFiles(List<string> startDirectories)
        {
            var directoryToDo = new Queue<string>();
            startDirectories.ForEach(d => directoryToDo.Enqueue(d));
            var extToDo = KnownFileTypes.Keys.ToList();
            while (directoryToDo.Count > 0)
            {
                var d = directoryToDo.Dequeue();
                // Add newlyfound directories
                Directory.GetDirectories(d).ToList().ForEach(i => directoryToDo.Enqueue(i));
                var files = Directory.GetFiles(d)
                    .Where(r => r.IndexOf('.') != -1
                        && extToDo.Contains(r.Substring(r.LastIndexOf('.')).ToLowerInvariant())
                        && !PathToIgnore.Any(r2 => r.Contains(r2, StringComparison.CurrentCultureIgnoreCase)))
                    .ToList();
                foreach (var f in files)
                    yield return f;
            }
        }

        static int NbRows(string file)
        {
            var result = 0;
            using var f = File.OpenRead(file);
            using var reader = new StreamReader(f);
            while (reader.ReadLine() != null)
                result++;
            return result;
        }
    }
}
