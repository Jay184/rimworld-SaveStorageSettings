using RimWorld;
using SaveStorageSettings.Dialog;
using System;
using System.Collections.Generic;
using Verse;

namespace SaveStorageSettings {
    public static class GizmoUtil {
        public static Gizmo SaveGizmo(SaveDialog dialog, int groupkey = 987767552) {
            if (dialog == null) throw new ArgumentNullException("dialog");

            return new Command_Action {
                icon = HarmonyPatches.SaveTexture,
                defaultLabel = "SaveStorageSettings.SaveZoneSettings".Translate(),
                defaultDesc = "SaveStorageSettings.SaveZoneSettingsDesc".Translate(),
                activateSound = SoundDef.Named("Click"),
                action = delegate {
                    Find.WindowStack.Add(dialog);
                },
                groupKey = groupkey
            };
        }
        public static Gizmo LoadGizmo(LoadDialog dialog, int groupkey = 987767553) {
            if (dialog == null) throw new ArgumentNullException("dialog");

            return new Command_Action {
                icon = HarmonyPatches.LoadTexture,
                defaultLabel = "SaveStorageSettings.LoadBills".Translate(),
                defaultDesc = "SaveStorageSettings.LoadBillsDesc".Translate(),
                activateSound = SoundDef.Named("Click"),
                action = delegate {
                    Find.WindowStack.Add(dialog);
                },
                groupKey = groupkey
            };
        }
        public static Gizmo AppendGizmo(LoadDialog dialog, int groupkey = 987767554) {
            if (dialog == null) throw new ArgumentNullException("dialog");

            return new Command_Action {
                icon = HarmonyPatches.AppendTexture,
                defaultLabel = "SaveStorageSettings.AppendBills".Translate(),
                defaultDesc = "SaveStorageSettings.AppendBillsDesc".Translate(),
                activateSound = SoundDef.Named("Click"),
                action = delegate {
                    Find.WindowStack.Add(dialog);
                },
                groupKey = groupkey
            };
        }


        public static List<Gizmo> AddSaveLoadGizmos(List<Gizmo> gizmos, string type, StorageSettings settings, int groupKey = 987767552) {
            SaveStorageDialog saveDialog = new SaveStorageDialog(type, settings);
            LoadStorageDialog loadDialog = new LoadStorageDialog(type, settings);

            gizmos.Add(SaveGizmo(saveDialog, groupKey));
            gizmos.Add(LoadGizmo(loadDialog, groupKey + 1));

            return gizmos;
        }
    }
}
