using System.IO;
using RimWorld;

namespace SaveStorageSettings.Dialog {
    // Outfits
    class SaveOutfitDialog : SaveDialog {
        private readonly Outfit Outfit;

        internal SaveOutfitDialog(string type, Outfit outfit) : base(type) {
            Outfit = outfit;
        }

        protected override void DoFileInteraction(FileInfo fi) {
            OutfitContainer container = new OutfitContainer(Outfit);
            container.Save(fi);
            base.Close();
        }
    }
}
