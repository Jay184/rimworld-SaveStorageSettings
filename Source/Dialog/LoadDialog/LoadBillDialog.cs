using System.IO;
using RimWorld;

namespace SaveStorageSettings.Dialog {
    // Bills
    public class LoadBillDialog : LoadDialog {
        public enum LoadType {
            Replace,
            Append,
        }

        private readonly bool Append;
        private readonly BillStack BillStack;

        public LoadBillDialog(string type, BillStack billStack, bool append) : base(type) {
            Append = append;
            BillStack = billStack;
        }

        protected override void DoFileInteraction(FileInfo fi) {
            if (!fi.Exists) return;

            BillContainer container = new BillContainer(BillStack, Append);
            container.Load(fi);

            base.Close();
        }
    }
}
