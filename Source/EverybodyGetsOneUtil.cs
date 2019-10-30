using System.Reflection;
using RimWorld;
using Verse;

namespace SaveStorageSettings {
    class EverybodyGetsOneUtil {
        private static Assembly assembly;
        private static bool initialized;


        public static void Initialize() {
            if (initialized) return;

            foreach (ModContentPack pack in LoadedModManager.RunningMods) {
                foreach (Assembly assembly in pack.assemblies.loadedAssemblies) {
                    if (assembly.GetName().Name.Equals("Everybody_Gets_One") &&
                        assembly.GetType("TD_Enhancement_Pack.RepeatModeDefOf") != null) {
                        initialized = true;
                        EverybodyGetsOneUtil.assembly = assembly;
                        return;
                    }
                }
            }

            initialized = true;
        }

        public static bool TryGetRepeatModeDef(string defName, out BillRepeatModeDef def) {
            Log.Warning($"Try get { defName }");
            Initialize();

            def = assembly?.GetType("TD_Enhancement_Pack.RepeatModeDefOf").GetField(defName, BindingFlags.Static | BindingFlags.Public).GetValue(null) as BillRepeatModeDef;

            return def != null;
        }
    }
}
