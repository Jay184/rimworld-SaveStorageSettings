using System;
using System.IO;
using RimWorld;
using Verse;

namespace SaveStorageSettings {
    public abstract class SaveableContainer {
        public Version Version { get; set; }
        protected const string BREAK = "---{{BREAK}}---";


        public SaveableContainer(Version version) {
            Version = version;
        }


        public void Save(FileInfo fileInfo) {
            try {
                //Directory.CreateDirectory(fileInfo.Directory.FullName);

                using (FileStream stream = File.Open(fileInfo.FullName, FileMode.Create, FileAccess.Write)) {
                    using (StreamWriter writer = new StreamWriter(stream)) {

                        WriteField(writer, "Version", Version.ToString());
                        SaveFields(writer);
                        Log.Message($"SaveStorageSettings: Saved settings in { fileInfo.FullName }.");
                        Messages.Message("Saved settings!", MessageTypeDefOf.TaskCompletion, false);

                    }
                }
            } catch (Exception ex) when (ex is IOException) {
                Log.Error($"SaveStorageSettings: Failed to save to file \"{ fileInfo.FullName }\". { ex }");
            }
        }
        public void Load(FileInfo fileInfo) {
            try {
                using (StreamReader reader = fileInfo.OpenText()) {
                    if (reader.EndOfStream || !TryReadField(reader, out Pair<string, string> header)) {
                        throw new BadHeaderException("Trying to read from an empty file");
                    }

                    if (!header.First.Equals("version", StringComparison.CurrentCultureIgnoreCase)) {
                        throw new BadHeaderException($"Missing header line. Expected \"version\", got: { header.ToString() }");
                    }

                    if (header.Second != Version.ToString()) {
                        throw new BadHeaderException($"Unsupported version. Expected \"{ Version.ToString() }\", got: { header.Second }");
                    }


                    LoadFields(reader);
                    Log.Message($"SaveStorageSettings: Loaded settings from { fileInfo.FullName }.");
                    Messages.Message("Loaded settings!", MessageTypeDefOf.TaskCompletion, false);
                }
            } catch (BadHeaderException ex) {
                Log.Error($"SaveStorageSettings: Failed to load from file \"{ fileInfo.FullName }\". { ex }");
            } catch (Exception ex) when (ex is IOException) {
                Log.Error($"SaveStorageSettings: Failed to load from file \"{ fileInfo.FullName }\". { ex }");
            }
        }

        protected abstract void SaveFields(StreamWriter writer);
        protected abstract void LoadFields(StreamReader reader);

        protected void WriteField(StreamWriter writer, string key, string value) {
            if (string.IsNullOrEmpty(value)) value = "null";
            writer.WriteLine($"{ key }:{ value }");
        }
        protected bool TryReadField(StreamReader reader, out Pair<string, string> pair) {
            string line = reader.ReadLine().Trim();
            int seperatorIndex = line.IndexOf(':');

            if (line == BREAK) {
                pair = new Pair<string, string>(BREAK, string.Empty);
                return true;
            }

            if (string.IsNullOrEmpty(line) || seperatorIndex == -1 || seperatorIndex >= line.Length - 1) {
                pair = new Pair<string, string>();
                return false;
            }

            string key = line.Substring(0, seperatorIndex);
            string value = line.Substring(seperatorIndex + 1, line.Length - seperatorIndex - 1);

            if (value == "null") value = null;

            pair = new Pair<string, string>(key, value);
            return true;
        }
    }
}
