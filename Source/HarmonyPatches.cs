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
        static void Postfix(Zone_Stockpile __instance, ref IEnumerable<Gizmo> __result) {
            List<Gizmo> gizmos = new List<Gizmo>(__result) {
                GizmoUtil.SaveGizmo(new SaveZoneDialog("stockpiles", __instance)),
                GizmoUtil.LoadGizmo(new LoadZoneDialog("stockpiles", __instance)),
            };

            __result = gizmos;
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

        static Patch_Pawn_GetGizmos() {
            OnOperationTab = typeof(HealthCardUtility).GetField("onOperationTab", BindingFlags.Static | BindingFlags.NonPublic);
        }
        static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result) {
            if (!(bool)OnOperationTab.GetValue(null)) return;

            if (!__instance.IsColonist && !__instance.IsPrisoner) return;


            if (DateTime.Now.Ticks - Patch_HealthCardUtility_DrawHealthSummary.LastCallTime < TENTH_SECOND) {
                string type = "operationsHuman";
                if (__instance.RaceProps.Animal) type = "operationsAnimal";

                List<Gizmo> gizmos = new List<Gizmo>(__result) {
                    GizmoUtil.SaveGizmo(new SaveOperationDialog(type, __instance)),
                    GizmoUtil.LoadGizmo(new LoadOperationDialog(type, __instance))
                };

                __result = gizmos;
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
        static void Postfix(Building __instance, ref IEnumerable<Gizmo> __result) {
            if (!__instance.def.IsWorkTable) return;

            string typeName = GetTypeFromDef(__instance.def.defName);

            List<Gizmo> gizmos = new List<Gizmo>(__result) {
                GizmoUtil.SaveGizmo(new SaveBillDialog(typeName, ((Building_WorkTable)__instance).billStack)),
                GizmoUtil.LoadGizmo(new LoadBillDialog(typeName, ((Building_WorkTable)__instance).billStack, false)),
                GizmoUtil.AppendGizmo(new LoadBillDialog(typeName, ((Building_WorkTable)__instance).billStack, true)),
            };

            __result = gizmos;
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
        static void Postfix(Building __instance, ref IEnumerable<Gizmo> __result) {
            List<Gizmo> gizmos = new List<Gizmo>(__result) {
                GizmoUtil.SaveGizmo(new SaveStorageDialog(__instance.def.defName, ((Building_Storage)__instance).settings)),
                GizmoUtil.LoadGizmo(new LoadStorageDialog(__instance.def.defName, ((Building_Storage)__instance).settings)),
            };

            __result = gizmos;
        }
    }

    // Graves
    [HarmonyPatch(typeof(Building_Grave), "GetGizmos")]
    static class Patch_BuildingGrave_GetGizmos {
        static void Postfix(Building __instance, ref IEnumerable<Gizmo> __result) {
            if (( (Building_Grave)__instance ).assignedPawn != null) return;

            List<Gizmo> gizmos = new List<Gizmo>(__result) {
                GizmoUtil.SaveGizmo(new SaveStorageDialog("graves", ((Building_Grave)__instance).GetStoreSettings())),
                GizmoUtil.LoadGizmo(new LoadStorageDialog("graves", ((Building_Grave)__instance).GetStoreSettings())),
            };

            __result = gizmos;
        }
    }
}
