using RimWorld;
using SaveStorageSettings.Dialog;
using System;
using System.Collections.Generic;
using Verse;

namespace SaveStorageSettings {
    public static class GizmoUtil {
        public static Gizmo SaveGizmo(SaveDialog dialog, string labelKey = "SaveStorageSettings.SaveSettings", string descKey = "SaveStorageSettings.SaveSettingsDesc", int groupkey = 987767552) {
            if (dialog == null) throw new ArgumentNullException("dialog");

            return new Command_Action {
                icon = HarmonyPatches.SaveTexture,
                defaultLabel = labelKey.Translate(),
                defaultDesc = descKey.Translate(),
                activateSound = SoundDef.Named("Click"),
                action = delegate {
                    Find.WindowStack.Add(dialog);
                },
                groupKey = groupkey
            };
        }
        public static Gizmo LoadGizmo(LoadDialog dialog, string labelKey = "SaveStorageSettings.LoadSettings", string descKey = "SaveStorageSettings.LoadSettingsDesc", int groupkey = 987767553) {
            if (dialog == null) throw new ArgumentNullException("dialog");

            return new Command_Action {
                icon = HarmonyPatches.LoadTexture,
                defaultLabel = labelKey.Translate(),
                defaultDesc = descKey.Translate(),
                activateSound = SoundDef.Named("Click"),
                action = delegate {
                    Find.WindowStack.Add(dialog);
                },
                groupKey = groupkey
            };
        }
        public static Gizmo AppendGizmo(LoadDialog dialog, string labelKey = "SaveStorageSettings.AppendSettings", string descKey = "SaveStorageSettings.AppendSettingsDesc", int groupkey = 987767554) {
            if (dialog == null) throw new ArgumentNullException("dialog");

            return new Command_Action {
                icon = HarmonyPatches.AppendTexture,
                defaultLabel = labelKey.Translate(),
                defaultDesc = descKey.Translate(),
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

            gizmos.Add(SaveGizmo(saveDialog, groupkey: groupKey));
            gizmos.Add(LoadGizmo(loadDialog, groupkey: groupKey + 1));

            return gizmos;
        }
    }
}
