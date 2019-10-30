using RimWorld;
using Verse;

namespace SaveStorageSettings.Dialog {
    // Bills
    public abstract class SaveDialog : FileListDialog {

        public SaveDialog(string type) : base(type) {
            buttonLabel = "OverwriteButton".Translate();
        }

        protected override bool ShowTextInput => true;
    }
}
