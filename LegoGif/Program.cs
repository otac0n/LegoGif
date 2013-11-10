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
    using System.IO;
    using System.Linq;
    using Microsoft.Win32;

    internal class Program
    {
        private static readonly Lazy<string> LDPath = new Lazy<string>(() =>
        {
            return GetLDrawPaths().FirstOrDefault(p => p != null && Directory.Exists(p));
        });

        public static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("LDRAWDIR", LDPath.Value, EnvironmentVariableTarget.Process);
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
    }
}
