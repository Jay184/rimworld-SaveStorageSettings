using System.IO;
using RimWorld;

namespace SaveStorageSettings.Dialog {
    // Bills
    public class SaveBillDialog : SaveDialog {
        public BillStack BillStack { get; set; }


        public SaveBillDialog(string type, BillStack billStack) : base(type) {
            BillStack = billStack;
        }

        protected override void DoFileInteraction(FileInfo fi) {
            BillContainer container = new BillContainer(BillStack, false);
            container.Save(fi);
            base.Close();
        }
    }
}
