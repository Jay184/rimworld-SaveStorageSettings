using System.IO;
using RimWorld;

namespace SaveStorageSettings.Dialog {
    // Stockpiles
    public class LoadZoneDialog : LoadDialog {
        private readonly Zone_Stockpile Stockpile;

        public LoadZoneDialog(string type, Zone_Stockpile stockpile) : base(type) {
            Stockpile = stockpile;
        }
        
        protected override void DoFileInteraction(FileInfo fi) {
            if (!fi.Exists) return;

            ZoneContainer container = new ZoneContainer(Stockpile);
            container.Load(fi);
            base.Close();
        }
    }
}
