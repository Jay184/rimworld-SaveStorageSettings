using System.IO;
using RimWorld;

namespace SaveStorageSettings.Dialog {
    // Stockpiles
    public class SaveZoneDialog : SaveDialog {
        public Zone_Stockpile Stockpile { get; set; }

        public SaveZoneDialog(string type, Zone_Stockpile stockpile) : base(type) {
            Stockpile = stockpile;
        }

        protected override void DoFileInteraction(FileInfo fi) {
            ZoneContainer container = new ZoneContainer(Stockpile);
            container.Save(fi);
            base.Close();
        }
    }
}
