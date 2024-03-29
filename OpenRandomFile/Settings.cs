using System;
using System.IO;

namespace OpenRandomFile {
    public class Settings {
        private static Settings _instance;
        public static Settings Instance {
            get {
                if (_instance == null) {
                    _instance = new Settings();
                }

                return _instance;
            }
        }

        private string DirectoryPath { get; set; }
        public string OptionFilePath { get; private set; }
        public string FilesPath { get; private set; }


        public string Extension { get; private set; }
        public bool OpenAgain { get; private set; }
        public bool CheckSubFolders { get; private set; }


        private Settings() {
            DirectoryPath = $@"C:\Users\{Environment.UserName}\AppData\Roaming\OpenRandomFile\";
            FilesPath = $@"{DirectoryPath}alredy\";
            OptionFilePath = $@"{DirectoryPath}options.conf";


            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);

            if (!Directory.Exists(FilesPath))
                Directory.CreateDirectory(FilesPath);

            try {
                using (StreamReader sr = new StreamReader(OptionFilePath)) {
                    Extension = sr.ReadLine();
                    OpenAgain = bool.Parse(sr.ReadLine());
                    CheckSubFolders = bool.Parse(sr.ReadLine());
                }
            } catch {
                WriteOptionFile(Extension = "*", OpenAgain = false, CheckSubFolders = true);
            }
        }


        private void WriteOptionFile(string ext, bool reOpen, bool subDir) {
            using (StreamWriter sw = new StreamWriter(OptionFilePath)) {
                sw.WriteLine($"{ext}");
                sw.WriteLine($"{reOpen}");
                sw.WriteLine($"{subDir}");
            }
        }
    }
}
