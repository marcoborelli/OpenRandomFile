using System;
using System.Diagnostics;
using OpenRandomFile.model;

namespace OpenRandomFile {
    internal class Program {
        static void Main(string[] args) {
            if (args.Length != 1) {
                throw new Exception("Missing param\nOpenRandomFile.exe <directory>");
            }

            Folder.Init(args[0]);
            string fileName = Folder.Instance.ChooseRandomFile();
            Process.Start(fileName);
        }
    }
}
