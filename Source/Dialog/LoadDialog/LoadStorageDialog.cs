using System.IO;
using RimWorld;

namespace SaveStorageSettings.Dialog {
    // Shelves, Graves and Sarcophagus'
    public class LoadStorageDialog : LoadDialog {
        public StorageSettings Settings { get; set; }

        public LoadStorageDialog(string type, StorageSettings settings) : base(type) {
            Settings = settings;
        }

        protected override void DoFileInteraction(FileInfo fi) {
            if (!fi.Exists) return;

            StorageContainer container = new StorageContainer(Settings);
            container.Load(fi);
            base.Close();
        }
    }
}
