using System.IO;
using Verse;

namespace SaveStorageSettings.Dialog {
    // Operations
    public class LoadOperationDialog : LoadDialog {
        private readonly Pawn Pawn;

        public LoadOperationDialog(string type, Pawn pawn) : base(type) {
            Pawn = pawn;
        }

        protected override void DoFileInteraction(FileInfo fi) {
            if (!fi.Exists) return;

            OperationContainer container = new OperationContainer(Pawn);
            container.Load(fi);
            base.Close();
        }
    }
}
