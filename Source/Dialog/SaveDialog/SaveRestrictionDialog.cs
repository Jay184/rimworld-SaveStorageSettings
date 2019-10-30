using System.IO;
using RimWorld;

namespace SaveStorageSettings.Dialog {
    // Food restrictions
    public class SaveRestrictionDialog : SaveDialog {
        private readonly FoodRestriction Restriction;

        public SaveRestrictionDialog(string type, FoodRestriction restriction) : base(type) {
            Restriction = restriction;
        }

        protected override void DoFileInteraction(FileInfo fi) {
            FoodRestrictionContainer container = new FoodRestrictionContainer(Restriction);
            container.Save(fi);
            base.Close();
        }
    }
}
