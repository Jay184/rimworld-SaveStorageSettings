using System.IO;
using RimWorld;

namespace SaveStorageSettings.Dialog {
    // Shelves, Graves and Sarcophagus'
    public class SaveStorageDialog : SaveDialog {
        public StorageSettings Settings { get; set; }

        public SaveStorageDialog(string type, StorageSettings settings) : base(type) {
            Settings = settings;
        }

        protected override void DoFileInteraction(FileInfo fi) {
            StorageContainer container = new StorageContainer(Settings);
            container.Save(fi);
            base.Close();
        }
    }
}
