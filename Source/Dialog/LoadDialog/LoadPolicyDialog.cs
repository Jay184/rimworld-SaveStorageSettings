using System.IO;
using RimWorld;

namespace SaveStorageSettings.Dialog {
    // Drug policies
    public class LoadPolicyDialog : LoadDialog {
        private readonly DrugPolicy DrugPolicy;

        public LoadPolicyDialog(string type, DrugPolicy drugPolicy) : base(type) {
            DrugPolicy = drugPolicy;
        }

        protected override void DoFileInteraction(FileInfo fi) {
            if (!fi.Exists) return;

            DrugPolicyContainer container = new DrugPolicyContainer(DrugPolicy);
            container.Load(fi);
            base.Close();
        }
    }
}
