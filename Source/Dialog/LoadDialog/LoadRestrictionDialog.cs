using System.IO;
using RimWorld;

namespace SaveStorageSettings.Dialog {
    // Food restrictions
    public class LoadRestrictionDialog : LoadDialog {
        private readonly FoodRestriction Restriction;

        public LoadRestrictionDialog(string type, FoodRestriction restriction) : base(type) {
            Restriction = restriction;
        }

        protected override void DoFileInteraction(FileInfo fi) {
            if (!fi.Exists) return;

            FoodRestrictionContainer container = new FoodRestrictionContainer(Restriction);
            container.Load(fi);
            base.Close();
        }
    }
}
