using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;

namespace SaveStorageSettingsUtil {
    public static class SaveStorageSettingsGizmoUtil {
        private static Assembly assembly;
        private static bool initialized;


        public static void Initialize() {
            if (initialized) return;

            foreach (ModContentPack pack in LoadedModManager.RunningMods) {
                foreach (Assembly assembly in pack.assemblies.loadedAssemblies) {
                    if (assembly.GetName().Name.Equals("SaveStorageSettings") &&
                        assembly.GetType("SaveStorageSettings.GizmoUtil") != null) {
                        initialized = true;
                        SaveStorageSettingsGizmoUtil.assembly = assembly;
                        return;
                    }
                }
            }

            initialized = true;
        }


        public static IEnumerable<Gizmo> AddSaveLoadGizmos(IEnumerable<Gizmo> gizmos, string type, StorageSettings settings, int groupKey = 987767552) {
            List<Gizmo> list;

            if (gizmos != null) {
                list = new List<Gizmo>(gizmos);
            } else {
                list = new List<Gizmo>(2);
            }

            return AddSaveLoadGizmos(list, type, settings, groupKey);
        }
        public static List<Gizmo> AddSaveLoadGizmos(List<Gizmo> gizmos, string type, StorageSettings settings, int groupKey = 987767552) {
            try {
                Initialize();

                assembly?.GetType("SaveStorageSettings.GizmoUtil").GetMethod("AddSaveLoadGizmos", BindingFlags.Static | BindingFlags.Public).Invoke(
                        null, new object[] { gizmos, type, settings, groupKey });
            } catch (Exception e) {
                // Do nothing
                Log.Warning($"{ e.GetType().Name } { e.Message }\n{ e.StackTrace }");
            }

            return gizmos;
        }
    }
}
