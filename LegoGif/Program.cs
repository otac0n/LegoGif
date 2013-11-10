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

        public static void Main(string[] args)
        {
            var file = args[0];

            Environment.SetEnvironmentVariable("LDRAWDIR", LDPath.Value, EnvironmentVariableTarget.Process);

            foreach (var angle in CameraAngles())
            {
                var frameName = "frame_" + angle;

                var l3pStartInfo = new ProcessStartInfo
                {
                    FileName = "l3p.exe",
                    Arguments = string.Format("-stdout \"{0}\" {1}.pov -o -cg30,{2}", file, frameName, angle, LDPath.Value),
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };

                using (var p = Process.Start(l3pStartInfo))
                {
                    Task.Factory.StartNew(MakePump(p.StandardOutput, ">"));
                    p.WaitForExit();
                }
            }
        }

        public static IEnumerable<int> CameraAngles()
        {
            for (int i = 0; i < 360; i++)
            {
                yield return i;
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
