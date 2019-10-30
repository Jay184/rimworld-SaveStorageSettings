using RimWorld;
using Verse;

namespace SaveStorageSettings.Dialog {
    // Stockpiles
    public abstract class LoadDialog : FileListDialog {
        public LoadDialog(string type) : base(type) {
            buttonLabel = "LoadGameButton".Translate();
        }

        protected override bool ShowTextInput => false;
    }
}
