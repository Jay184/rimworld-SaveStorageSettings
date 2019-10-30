using System.IO;
using RimWorld;

namespace SaveStorageSettings.Dialog {
    // Drug policies
    public class SavePolicyDialog : SaveDialog {
        private readonly DrugPolicy DrugPolicy;

        public SavePolicyDialog(string type, DrugPolicy drugPolicy) : base(type) {
            DrugPolicy = drugPolicy;
        }

        protected override void DoFileInteraction(FileInfo fi) {
            DrugPolicyContainer container = new DrugPolicyContainer(DrugPolicy);
            container.Save(fi);
            base.Close();
        }
    }
}
