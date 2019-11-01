using Harmony;
using RimWorld;
using SaveStorageSettings.Dialog;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using System;

namespace SaveStorageSettings {
    [StaticConstructorOnStartup]
    class HarmonyPatches {
        public static readonly Texture2D DeleteXTexture;
        public static readonly Texture2D SaveTexture;
        public static readonly Texture2D LoadTexture;
        public static readonly Texture2D AppendTexture;

        static HarmonyPatches() {
            var harmony = HarmonyInstance.Create("com.savestoragesettings.rimworld.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());


            Log.Message("SaveStorageSettings: Harmony Patches applied.");
            Log.Message("SaveStorageSettings: \tZone_Stockpile.GetGizmos(IEnumerable<Gizmo>)");
            Log.Message("SaveStorageSettings: \tHealthCardUtility.DrawHealthSummary()");
            Log.Message("SaveStorageSettings: \tPawn.GetGizmos(IEnumerable<Gizmo>)");
            Log.Message("SaveStorageSettings: \tDialog_ManageDrugPolicies.GetGizmos(IEnumerable<Gizmo>)");
            Log.Message("SaveStorageSettings: \tDialog_ManageFoodRestrictions.GetGizmos(IEnumerable<Gizmo>)");
            Log.Message("SaveStorageSettings: \tDialog_ManageOutfits.GetGizmos(IEnumerable<Gizmo>)");
            Log.Message("SaveStorageSettings: \tBuilding.GetGizmos(IEnumerable<Gizmo>)");
            Log.Message("SaveStorageSettings: \tBuilding_Storage.GetGizmos(IEnumerable<Gizmo>)");
            Log.Message("SaveStorageSettings: \tBuilding_Grave.GetGizmos(IEnumerable<Gizmo>)");


            DeleteXTexture = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true);
            SaveTexture = ContentFinder<Texture2D>.Get("UI/save", true);
            LoadTexture = ContentFinder<Texture2D>.Get("UI/load", true);
            AppendTexture = ContentFinder<Texture2D>.Get("UI/append", true);
        }
    }


    // Stockpiles
    [HarmonyPatch(typeof(Zone_Stockpile), "GetGizmos")]
    static class Patch_Zone_Stockpile_GetGizmos {
        static readonly SaveZoneDialog SaveDialog = new SaveZoneDialog("stockpiles", null);
        static readonly LoadZoneDialog LoadDialog = new LoadZoneDialog("stockpiles", null);

        static readonly Gizmo SaveGizmo = GizmoUtil.SaveGizmo(SaveDialog, "SaveStorageSettings.SaveZoneSettings", "SaveStorageSettings.SaveZoneSettingsDesc");
        static readonly Gizmo LoadGizmo = GizmoUtil.LoadGizmo(LoadDialog, "SaveStorageSettings.LoadZoneSettings", "SaveStorageSettings.LoadZoneSettingsDesc");

        static void Postfix(Zone_Stockpile __instance, ref IEnumerable<Gizmo> __result) {
            SaveDialog.Stockpile = __instance;
            LoadDialog.Stockpile = __instance;

            __result = new List<Gizmo>(__result) { SaveGizmo, LoadGizmo };
        }
    }


    // Operations tab
    [HarmonyPatch(typeof(HealthCardUtility), "DrawHealthSummary")]
    static class Patch_HealthCardUtility_DrawHealthSummary {
        public static long LastCallTime = 0;

        [HarmonyPriority(Priority.First)]
        static void Prefix() {
            LastCallTime = DateTime.Now.Ticks;
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    static class Patch_Pawn_GetGizmos {
        const long TENTH_SECOND = TimeSpan.TicksPerSecond / 10;
        static FieldInfo OnOperationTab = null;

        static readonly SaveOperationDialog SaveDialog = new SaveOperationDialog("operations", null);
        static readonly LoadOperationDialog LoadDialog = new LoadOperationDialog("operations", null);

        static readonly Gizmo SaveGizmo = GizmoUtil.SaveGizmo(SaveDialog, "SaveStorageSettings.SaveOperations", "SaveStorageSettings.SaveZoneSaveOperationsDescSettingsDesc");
        static readonly Gizmo LoadGizmo = GizmoUtil.LoadGizmo(LoadDialog, "SaveStorageSettings.LoadOperations", "SaveStorageSettings.LoadOperationsDesc");


        static Patch_Pawn_GetGizmos() {
            OnOperationTab = typeof(HealthCardUtility).GetField("onOperationTab", BindingFlags.Static | BindingFlags.NonPublic);
        }
        static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result) {
            if (!(bool)OnOperationTab.GetValue(null)) return;

            if (!__instance.IsColonist && !__instance.IsPrisoner) return;

            SaveDialog.Pawn = __instance;
            LoadDialog.Pawn = __instance;

            if (__instance.RaceProps.Animal) {
                SaveDialog.DirectoryName = "operationsAnimal";
                LoadDialog.DirectoryName = "operationsAnimal";
            } else {
                SaveDialog.DirectoryName = "operationsHuman";
                LoadDialog.DirectoryName = "operationsHuman";
            }

            if (DateTime.Now.Ticks - Patch_HealthCardUtility_DrawHealthSummary.LastCallTime < TENTH_SECOND) {
                __result = new List<Gizmo>(__result) { SaveGizmo, LoadGizmo };
            }
        }
    }


    // Drug policies
    [HarmonyPatch(typeof(Dialog_ManageDrugPolicies), "DoWindowContents")]
    static class Patch_Dialog_Dialog_ManageDrugPolicies {
        static void Postfix(Dialog_ManageDrugPolicies __instance, Rect inRect) {
            float x = 500;
            if (Widgets.ButtonText(new Rect(x, 0, 150f, 35f), "SaveStorageSettings.LoadAsNew".Translate(), true, false, true)) {
                DrugPolicy policy = Current.Game.drugPolicyDatabase.MakeNewDrugPolicy();
                SetDrugPolicy(__instance, policy);

                Find.WindowStack.Add(new LoadPolicyDialog("drugPolicies", policy));
            }
            x += 160;

            DrugPolicy selectedPolicy = GetDrugPolicy(__instance);
            if (selectedPolicy != null) {
                Text.Font = GameFont.Small;
                if (Widgets.ButtonText(new Rect(x, 0f, 75, 35f), "LoadGameButton".Translate(), true, false, true)) {
                    string label = selectedPolicy.label;
                    Find.WindowStack.Add(new LoadPolicyDialog("drugPolicies", selectedPolicy));
                    selectedPolicy.label = label;
                }
                x += 80;
                if (Widgets.ButtonText(new Rect(x, 0f, 75, 35f), "SaveGameButton".Translate(), true, false, true)) {
                    Find.WindowStack.Add(new SavePolicyDialog("drugPolicies", selectedPolicy));
                }
            }
        }

        private static DrugPolicy GetDrugPolicy(Dialog_ManageDrugPolicies dialog) {
            return (DrugPolicy)typeof(Dialog_ManageDrugPolicies).GetProperty("SelectedPolicy", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty).GetValue(dialog, null);
        }
        private static void SetDrugPolicy(Dialog_ManageDrugPolicies dialog, DrugPolicy selectedPolicy) {
            typeof(Dialog_ManageDrugPolicies).GetProperty("SelectedPolicy", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty).SetValue(dialog, selectedPolicy, null);
        }
    }


    // Food restrictions
    [HarmonyPatch(typeof(Dialog_ManageFoodRestrictions), "DoWindowContents")]
    static class Patch_Dialog_Dialog_ManageFoodRestrictions {
        static void Postfix(Dialog_ManageFoodRestrictions __instance, Rect inRect) {
            if (Widgets.ButtonText(new Rect(480f, 0f, 150f, 35f), "SaveStorageSettings.LoadAsNew".Translate(), true, false, true)) {
                FoodRestriction restriction = Current.Game.foodRestrictionDatabase.MakeNewFoodRestriction();
                SetSelectedRestriction(__instance, restriction);

                Find.WindowStack.Add(new LoadRestrictionDialog("foodRestrictions", restriction));
            }

            FoodRestriction selectedRestriction = GetSelectedRestriction(__instance);
            if (selectedRestriction != null) {
                Text.Font = GameFont.Small;
                GUI.BeginGroup(new Rect(220f, 49f, 300, 32f));
                if (Widgets.ButtonText(new Rect(0f, 0f, 150f, 32f), "SaveStorageSettings.LoadRestriction".Translate(), true, false, true)) {
                    Find.WindowStack.Add(new LoadRestrictionDialog("foodRestrictions", selectedRestriction));
                }
                if (Widgets.ButtonText(new Rect(160f, 0f, 150f, 32f), "SaveStorageSettings.SaveRestriction".Translate(), true, false, true)) {
                    Find.WindowStack.Add(new SaveRestrictionDialog("foodRestrictions", selectedRestriction));
                }
                GUI.EndGroup();
            }
        }

        private static FoodRestriction GetSelectedRestriction(Dialog_ManageFoodRestrictions dialog) {
            return (FoodRestriction)typeof(Dialog_ManageFoodRestrictions).GetProperty("SelectedFoodRestriction", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty).GetValue(dialog, null);
        }
        private static void SetSelectedRestriction(Dialog_ManageFoodRestrictions dialog, FoodRestriction selectedRestriction) {
            typeof(Dialog_ManageFoodRestrictions).GetProperty("SelectedFoodRestriction", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty).SetValue(dialog, selectedRestriction, null);
        }
    }


    // Outfits
    [HarmonyPatch(typeof(Dialog_ManageOutfits), "DoWindowContents")]
    static class Patch_Dialog_ManageOutfits_DoWindowContents {
        static void Postfix(Dialog_ManageOutfits __instance, Rect inRect) {
            if (Widgets.ButtonText(new Rect(480f, 0f, 150f, 35f), "SaveStorageSettings.LoadAsNew".Translate(), true, false, true)) {
                Outfit outfit = Current.Game.outfitDatabase.MakeNewOutfit();
                SetSelectedOutfit(__instance, outfit);

                Find.WindowStack.Add(new LoadOutfitDialog("outfits", outfit));
            }

            Outfit selectedOutfit = GetSelectedOutfit(__instance);
            if (selectedOutfit != null) {
                Text.Font = GameFont.Small;
                GUI.BeginGroup(new Rect(220f, 49f, 300, 32f));
                if (Widgets.ButtonText(new Rect(0f, 0f, 150f, 32f), "SaveStorageSettings.LoadOutfit".Translate(), true, false, true)) {
                    Find.WindowStack.Add(new LoadOutfitDialog("outfits", selectedOutfit));
                }
                if (Widgets.ButtonText(new Rect(160f, 0f, 150f, 32f), "SaveStorageSettings.SaveOutfit".Translate(), true, false, true)) {
                    Find.WindowStack.Add(new SaveOutfitDialog("outfits", selectedOutfit));
                }
                GUI.EndGroup();
            }
        }

        private static Outfit GetSelectedOutfit(Dialog_ManageOutfits dialog) {
            return (Outfit)typeof(Dialog_ManageOutfits).GetProperty("SelectedOutfit", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty).GetValue(dialog, null);
        }
        private static void SetSelectedOutfit(Dialog_ManageOutfits dialog, Outfit selectedOutfit) {
            typeof(Dialog_ManageOutfits).GetProperty("SelectedOutfit", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty).SetValue(dialog, selectedOutfit, null);
        }
    }


    // Buildings with bills
    [HarmonyPatch(typeof(Building), "GetGizmos")]
    static class Patch_Building_GetGizmos {
        static readonly SaveBillDialog SaveDialog = new SaveBillDialog("unknown", null);
        static readonly LoadBillDialog LoadDialog = new LoadBillDialog("unknown", null, false);
        static readonly LoadBillDialog AppendDialog = new LoadBillDialog("unknown", null, true);

        static readonly Gizmo SaveGizmo = GizmoUtil.SaveGizmo(SaveDialog, "SaveStorageSettings.SaveBills", "SaveStorageSettings.SaveBillsDesc");
        static readonly Gizmo LoadGizmo = GizmoUtil.LoadGizmo(LoadDialog, "SaveStorageSettings.LoadBills", "SaveStorageSettings.LoadBillsDesc");
        static readonly Gizmo AppendGizmo = GizmoUtil.AppendGizmo(AppendDialog, "SaveStorageSettings.AppendBills", "SaveStorageSettings.AppendBillsDesc");


        static void Postfix(Building __instance, ref IEnumerable<Gizmo> __result) {
            if (!__instance.def.IsWorkTable) return;

            string typeName = GetTypeFromDef(__instance.def.defName);

            SaveDialog.DirectoryName = typeName;
            LoadDialog.DirectoryName = typeName;
            AppendDialog.DirectoryName = typeName;

            SaveDialog.BillStack = ( (Building_WorkTable)__instance ).billStack;
            LoadDialog.BillStack = ( (Building_WorkTable)__instance ).billStack;
            AppendDialog.BillStack = ( (Building_WorkTable)__instance ).billStack;

            __result = new List<Gizmo>(__result) { SaveGizmo, LoadGizmo, AppendGizmo };
        }

        private static string GetTypeFromDef(string type) {
            if (string.IsNullOrEmpty(type)) return "unknown";

            switch (type) {
                case "TableButcher":
                case "ButcherSpot":
                    return "butcher";
                case "HandTailoringBench":
                case "ElectricTailoringBench":
                    return "tailoringBench";
                case "FueledSmithy":
                case "ElectricSmithy":
                    return "smithy";
                case "FueledStove":
                case "ElectricStove":
                    return "stove";
                default:
                    return char.ToLowerInvariant(type[0]) + type.Substring(1);
            }
        }
    }

    // Buildings that have a storage option (shelves)
    [HarmonyPatch(typeof(Building_Storage), "GetGizmos")]
    static class Patch_BuildingStorage_GetGizmos {
        static readonly SaveStorageDialog SaveDialog = new SaveStorageDialog("unknown", null);
        static readonly LoadStorageDialog LoadDialog = new LoadStorageDialog("unknown", null);

        static readonly Gizmo SaveGizmo = GizmoUtil.SaveGizmo(SaveDialog, "SaveStorageSettings.SaveZoneSettings", "SaveStorageSettings.SaveZoneSettingsDesc");
        static readonly Gizmo LoadGizmo = GizmoUtil.LoadGizmo(LoadDialog, "SaveStorageSettings.LoadZoneSettings", "SaveStorageSettings.LoadZoneSettingsDesc");


        static void Postfix(Building __instance, ref IEnumerable<Gizmo> __result) {
            SaveDialog.DirectoryName = __instance.def.defName;
            LoadDialog.DirectoryName = __instance.def.defName;

            SaveDialog.Settings = ( (Building_Storage)__instance ).settings;
            LoadDialog.Settings = ( (Building_Storage)__instance ).settings;

            __result = new List<Gizmo>(__result) { SaveGizmo, LoadGizmo };
        }
    }

    // Graves
    [HarmonyPatch(typeof(Building_Grave), "GetGizmos")]
    static class Patch_BuildingGrave_GetGizmos {
        static readonly SaveStorageDialog SaveDialog = new SaveStorageDialog("graves", null);
        static readonly LoadStorageDialog LoadDialog = new LoadStorageDialog("graves", null);

        static readonly Gizmo SaveGizmo = GizmoUtil.SaveGizmo(SaveDialog, "SaveStorageSettings.SaveGrave", "SaveStorageSettings.SaveGraveDesc");
        static readonly Gizmo LoadGizmo = GizmoUtil.LoadGizmo(LoadDialog, "SaveStorageSettings.LoadGrave", "SaveStorageSettings.LoadGraveDesc");

        static void Postfix(Building __instance, ref IEnumerable<Gizmo> __result) {
            if (( (Building_Grave)__instance ).assignedPawn != null) return;

            StorageSettings settings = ( (Building_Grave)__instance ).GetStoreSettings();
            SaveDialog.Settings = settings;
            LoadDialog.Settings = settings;

            __result = new List<Gizmo>(__result) { SaveGizmo, LoadGizmo };
        }
    }
}
