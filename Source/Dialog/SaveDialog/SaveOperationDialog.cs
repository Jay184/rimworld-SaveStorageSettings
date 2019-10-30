using System.IO;
using Verse;

namespace SaveStorageSettings.Dialog {
    // Operations
    public class SaveOperationDialog : SaveDialog {
        private readonly Pawn Pawn;

        public SaveOperationDialog(string type, Pawn pawn) : base(type) {
            Pawn = pawn;
        }

        protected override void DoFileInteraction(FileInfo fi) {
            OperationContainer container = new OperationContainer(Pawn);
            container.Save(fi);
            base.Close();
        }
    }
}
