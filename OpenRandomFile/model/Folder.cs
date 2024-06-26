﻿using System;
using System.IO;
using System.Text;

namespace OpenRandomFile.model {
    internal class Folder {
        private enum eHeaderField {
            TotalNumber,
            NotViewedNumber,
            LastModifyDate
        }



        public static Folder Instance { get; private set; }
        Encoding encoding = Encoding.GetEncoding(1252);

        public static void Init(string path) {
            Instance = new Folder(path);
        }



        private DirectoryInfo DirInfo { get; set; }

        private Folder(string path) {
            DirInfo = new DirectoryInfo(path);
        }

        public string ChooseRandomFile() {
            ushort maxIndex = 0;

            if (!File.Exists($"{Settings.Instance.FilesPath}{DirInfo.Name}")) {
                SearchOption opt = Settings.Instance.CheckSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                FileInfo[] files = DirInfo.GetFiles($"*.{Settings.Instance.Extension}", opt);
                CreateFile(files);

                maxIndex = (ushort)files.Length;
            } else {
                if (CheckIfDirectoryChanged()) {
                    //todo
                }

                maxIndex = GetValidFilesCount();
            }


            if (maxIndex == 0) {
                SetAllFileNotViewed();
            }

            int fileIndex = new Random().Next(0, maxIndex);
            return GetFileNameAndUpdateFile(fileIndex);
        }

        private void SetAllFileNotViewed() {
            var fs = new FileStream($"{Settings.Instance.FilesPath}{DirInfo.Name}", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryWriter bw = new BinaryWriter(fs, encoding);
            BinaryReader br = new BinaryReader(fs, encoding);

            int totNum = br.ReadInt32();
            bw.Write(totNum); // metto che i file non visti sono uguali al numero di file totali

            fs.Seek(21, SeekOrigin.Current); //salto la data

            while (fs.Position < fs.Length) {
                bw.Write(true);
                fs.Seek(255, SeekOrigin.Current);
            }

            bw.Close();
            fs.Close();
        }

        private string GetFileNameAndUpdateFile(int index) {
            var fs = new FileStream($"{Settings.Instance.FilesPath}{DirInfo.Name}", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryWriter bw = new BinaryWriter(fs, encoding);
            BinaryReader br = new BinaryReader(fs, encoding);

            fs.Seek(4, SeekOrigin.Begin);
            int oldCount = br.ReadInt32();
            fs.Seek(-4, SeekOrigin.Current);
            bw.Write(oldCount - 1);

            fs.Seek(21, SeekOrigin.Current);

            ushort i = 0;
            string res = "";

            do {
                if (br.ReadBoolean() || Settings.Instance.OpenAgain)
                    i++;

                // nel caso in cui index sia 0 e' un caso speciale
                if ((i == index && index != 0) || (i == 1 && index == 0)) {
                    fs.Seek(-1, SeekOrigin.Current);
                    bw.Write(false); // sovrascrivo il vecchio bool
                    res = br.ReadString().Trim();
                    break;
                } else { // se non e' la riga che mi interessa mi sposot in avanti per saltare la stringa con il nome del file
                    fs.Seek(255, SeekOrigin.Current);
                }
            } while (fs.Position < fs.Length);

            br.Close();
            bw.Close();
            fs.Close();

            return res;
        }

        private bool CheckIfDirectoryChanged() {
            DateTime lastMod = GetHeaderField<DateTime>(eHeaderField.LastModifyDate);

            return lastMod == DirInfo.LastWriteTime;
        }

        private ushort GetValidFilesCount() {
            return Settings.Instance.OpenAgain ? GetHeaderField<ushort>(eHeaderField.TotalNumber) : GetHeaderField<ushort>(eHeaderField.NotViewedNumber);
        }

        private void CreateFile(FileInfo[] items) {
            var fs = new FileStream($"{Settings.Instance.FilesPath}{DirInfo.Name}", FileMode.Create, FileAccess.ReadWrite);
            BinaryWriter bw = new BinaryWriter(fs, encoding);


            // header:
            // <numTot(4, int)><numNonVisti(4, int)><dataUltimaModifica(20, string)>
            // lunghTot = 29 (+1 lo aggiunge C# per la stringa)
            bw.Write(items.Length);
            bw.Write(items.Length);
            bw.Write(DirInfo.LastWriteTime.ToString().PadRight(20));


            // body
            // <visto(1, bool)><nomeFile(253, string)>
            // lunghezza tot = 256 (+2 li aggiunge C# per la stringa)
            for (short i = 0; i < items.Length; i++) {
                if (items[i].Name == "Thumbs.db")
                    continue;

                bw.Write(true);
                bw.Write(items[i].FullName.PadRight(253));
            }


            bw.Close();
            fs.Close();
        }

        private T GetHeaderField<T>(eHeaderField field) where T : IConvertible {
            var fs = new FileStream($"{Settings.Instance.FilesPath}{DirInfo.Name}", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryReader br = new BinaryReader(fs, encoding);

            string res = null;

            switch (field) {
                case eHeaderField.TotalNumber:
                    fs.Seek(0, SeekOrigin.Begin);
                    res = $"{br.ReadInt32()}";
                    break;
                case eHeaderField.NotViewedNumber:
                    fs.Seek(4, SeekOrigin.Begin);
                    res = $"{br.ReadInt32()}";
                    break;
                case eHeaderField.LastModifyDate:
                    fs.Seek(8, SeekOrigin.Begin);
                    res = br.ReadString();
                    break;
            }

            br.Close();
            fs.Close();

            return (T)Convert.ChangeType(res, typeof(T));
        }
    }
}
