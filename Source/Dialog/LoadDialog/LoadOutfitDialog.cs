using System.IO;
using RimWorld;

namespace SaveStorageSettings.Dialog {
    // Outfits
    public class LoadOutfitDialog : LoadDialog {
        private readonly Outfit Outfit;

        public LoadOutfitDialog(string type, Outfit outfit) : base(type) {
            Outfit = outfit;
        }

        protected override void DoFileInteraction(FileInfo fi) {
            if (!fi.Exists) return;

            OutfitContainer container = new OutfitContainer(Outfit);
            container.Load(fi);
            base.Close();
        }
    }
}
