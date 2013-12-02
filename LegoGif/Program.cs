// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="(none)">
//   Copyright © 2013 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace LegoGif
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Win32;

    internal class Program
    {
        private static readonly Lazy<string> LDPath = new Lazy<string>(() =>
        {
            return GetLDrawPaths().FirstOrDefault(p => p != null && Directory.Exists(p));
        });

        private static readonly Lazy<string> PovRayPath = new Lazy<string>(() =>
        {
            return GetPovRayPaths().FirstOrDefault(p => p != null && File.Exists(p));
        });

        public static void Main(string[] args)
        {
            var file = args[0];

            Environment.SetEnvironmentVariable("LDRAWDIR", LDPath.Value, EnvironmentVariableTarget.Process);

            foreach (var angle in CameraAngles(start: 45))
            {
                var frameName = "frame_" + angle;
                var povFile = frameName + ".pov";
                var pngFile = frameName + ".png";

                if (File.Exists(pngFile))
                {
                    continue;
                }

                var l3pStartInfo = new ProcessStartInfo
                {
                    FileName = "l3p.exe",
                    Arguments = string.Format("-stdout \"{0}\" {1} -o -cg30,{2} -q4 -iblighting.pov", file, povFile, angle, LDPath.Value),
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };

                using (var p = Process.Start(l3pStartInfo))
                {
                    MakePump(p.StandardOutput, ">")();
                    p.WaitForExit();
                }

                var contents = File.ReadAllText(povFile)
                    .Replace("reflection 0.08", "reflection 0.04")
                    .Replace("color rgb <1,1,1>", "color rgb 0.4 * <1,1,1>")
                    .Replace("#declare L3FinishPearlescent = finish { L3FinishOpaque }", "#declare L3FinishPearlescent = finish { ambient 0.25 diffuse 0.6 brilliance 3 metallic specular 0.80 roughness 10/100 reflection 0.2 }");
                File.WriteAllText(povFile, contents);

                var povStartInfo = new ProcessStartInfo
                {
                    FileName = PovRayPath.Value,
                    Arguments = string.Format(" /NR /RENDER \"{0}\" /EXIT +F +FN +UA +W512 +H384 +Q11 +A", povFile),
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                };

                using (var p = Process.Start(povStartInfo))
                {
                    Task.Factory.StartNew(MakePump(p.StandardError, "!"));
                    Task.Factory.StartNew(MakePump(p.StandardOutput, ">"));
                    p.WaitForExit();
                }

                File.Delete(povFile);
            }
        }

        public static IEnumerable<int> CameraAngles(bool forceOrder = false, int start = 0)
        {
            var done = new bool[360];

            for (var step = forceOrder ? 1 : 360; step >= 1; step /= 2)
            {
                for (int i = 0; i < 360; i += step)
                {
                    if (!done[i])
                    {
                        done[i] = true;
                        yield return (i + start) % 360;
                    }
                }
            }
        }

        public static IEnumerable<string> GetLDrawPaths()
        {
            yield return Environment.GetEnvironmentVariable("LDRAWDIR");

            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\LDView.exe"))
            {
                var value = key.GetValue("") as string;
                if (value != null)
                {
                    yield return Path.GetDirectoryName(value);
                }
            }

            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "LDView");
            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "LDView");
        }

        public static IEnumerable<string> GetPovRayPaths()
        {
            var paths = new[]
            {
                Path.Combine("POV-Ray", "v3.7", "bin"),
                Path.Combine("POV-Ray", "v3.6", "bin"),
                Path.Combine("POV-Ray for Windows v3.6", "v3.6", "bin"),
            };

            var files = new[]
            {
                "pvengine64.exe",
                "pvengine32-sse2.exe",
                "pvengine.exe",
            };

            var roots = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            };

            foreach (var path in paths)
            {
                foreach (var file in files)
                {
                    foreach (var root in roots)
                    {
                        yield return Path.Combine(root, path, file);
                    }
                }
            }
        }

        public static Action MakePump(TextReader reader, string prefix)
        {
            return () =>
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine("{0} {1}", prefix, line);
                }
            };
        }
    }
}
