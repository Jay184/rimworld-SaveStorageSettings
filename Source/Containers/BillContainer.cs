using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace SaveStorageSettings {
    public class BillContainer : SaveableContainer {
        public BillStack Bills { get; private set; }
        public bool Append { get; set; }

        public BillContainer(BillStack bills, bool append) : base(new Version(1, 0)) {
            Bills = bills;
            Append = append;
        }


        protected override void SaveFields(StreamWriter writer) {
            foreach (Bill bill in Bills) {
                if (bill is Bill_Production productionBill) {
                    ThingFilter filter = productionBill.ingredientFilter;
                    ThingFilterReflection reflection = new ThingFilterReflection(filter);
                    string defName = "recipeDefName";

                    if (bill is Bill_ProductionWithUft) {
                        defName = "recipeDefNameUft";
                    }

                    WriteField(writer, defName, productionBill.recipe.defName);
                    WriteField(writer, "suspended", productionBill.suspended.ToString());
                    WriteField(writer, "countEquipped", productionBill.includeEquipped.ToString());
                    WriteField(writer, "countTainted", productionBill.includeTainted.ToString());
                    WriteField(writer, "skillRange", productionBill.allowedSkillRange.ToString());
                    WriteField(writer, "ingSearchRadius", productionBill.ingredientSearchRadius.ToString());
                    WriteField(writer, "repeatMode", productionBill.repeatMode.defName);
                    WriteField(writer, "repeatCount", productionBill.repeatCount.ToString());
                    WriteField(writer, "targetCount", productionBill.targetCount.ToString());
                    WriteField(writer, "pauseWhenSatisfied", productionBill.pauseWhenSatisfied.ToString());
                    WriteField(writer, "unpauseWhenYouHave", productionBill.unpauseWhenYouHave.ToString());
                    WriteField(writer, "hpRange", productionBill.hpRange.ToString());
                    WriteField(writer, "qualityRange", productionBill.qualityRange.ToString());
                    WriteField(writer, "onlyAllowedIngredients", productionBill.limitToAllowedStuff.ToString());

                    BillStoreModeDef storeMode = productionBill.GetStoreMode();
                    WriteField(writer, "storeMode", storeMode.ToString());
                    if (storeMode == BillStoreModeDefOf.SpecificStockpile) {
                        WriteField(writer, "storeZone", productionBill.GetStoreZone().label);
                    }

                    if (productionBill.includeFromZone != null) {
                        WriteField(writer, "lookIn", productionBill.includeFromZone.label);
                    }


                    StringBuilder builder = new StringBuilder();
                    foreach (ThingDef thing in filter.AllowedThingDefs) {
                        if (builder.Length > 0) builder.Append("/");
                        builder.Append(thing.defName);
                    }
                    WriteField(writer, "allowedDefs", builder.ToString());


                    if (filter.allowedHitPointsConfigurable) {
                        builder = new StringBuilder();

                        builder.Append(Math.Round(filter.AllowedHitPointsPercents.min, 2).ToString());
                        builder.Append(":");
                        builder.Append(Math.Round(filter.AllowedHitPointsPercents.max, 2).ToString());

                        WriteField(writer, "allowedHitPointsPercents", builder.ToString());
                    }

                    if (filter.allowedQualitiesConfigurable) {
                        builder = new StringBuilder();

                        builder.Append(filter.AllowedQualityLevels.min.ToString());
                        builder.Append(":");
                        builder.Append(filter.AllowedQualityLevels.max.ToString());

                        WriteField(writer, "allowedQualities", builder.ToString());
                    }

                    builder = new StringBuilder();
                    foreach (SpecialThingFilterDef def in reflection.DisallowedSpecialFilters) {
                        if (builder.Length > 0) builder.Append("/");
                        builder.Append(def.defName);
                    }
                    WriteField(writer, "disallowedSpecialFilters", builder.ToString());

                    writer.WriteLine(BREAK);
                }
            }
        }
        protected override void LoadFields(StreamReader reader) {
            List<Dictionary<string, string>> fieldlist = new List<Dictionary<string, string>> {
                new Dictionary<string, string>()
            };

            while (!reader.EndOfStream) {
                if (TryReadField(reader, out Pair<string, string> pair)) {
                    fieldlist[fieldlist.Count - 1].Add(pair.First, pair.Second);

                    if (pair.First == BREAK) {
                        fieldlist.Add(new Dictionary<string, string>());
                    }
                }
            }


            List<Bill_Production> bills = new List<Bill_Production>();

            for (int i = 0; i < fieldlist.Count - 1; i++) {
                var fields = fieldlist[i];

                if (TryCreateBill(fields, out Bill_Production productionBill)) {
                    if (Bills.Count < BillStack.MaxCount) {
                        bills.Add(productionBill);
                    } else {
                        Log.Warning("Work Table has too many bills. Bill for [" + productionBill.recipe.defName + "] will not be added.");
                    }
                }
            }


            if (bills.Count > 0 && !Append) Bills.Clear();
            foreach (Bill_Production bill in bills) {
                Bills.AddBill(bill);
            }
        }

        private bool TryCreateBill(Dictionary<string, string> fields, out Bill_Production bill) {
            bill = null;
            RecipeDef def;
            ThingFilter filter = null;
            ThingFilterReflection reflection = null;
            bool changed = false;

            foreach (var field in fields) {
                string key = field.Key;
                string value = field.Value;

                switch (key) {
                    case BREAK:
                        return true;
                    case "recipeDefName":
                    case "recipeDefNameUft":
                        def = DefDatabase<RecipeDef>.GetNamed(value);
                        if (def == null) {
                            string msg = "SaveStorageSettings.UnableToLoadRecipeDef".Translate().Replace("%s", value);
                            Messages.Message(msg, MessageTypeDefOf.SilentInput);
                            Log.Warning(msg);
                            return false;
                        }
                        if (def.researchPrerequisite != null && !def.researchPrerequisite.IsFinished) {
                            string msg = "SaveStorageSettings.ResearchNotDoneForRecipeDef".Translate().Replace("%s", def.label);
                            Messages.Message(msg, MessageTypeDefOf.SilentInput);
                            Log.Warning(msg);
                            return false;
                        }

                        if (key == "recipeDefName") bill = new Bill_Production(def);
                        else bill = new Bill_ProductionWithUft(def);

                        filter = bill.ingredientFilter;
                        reflection = new ThingFilterReflection(filter);

                        break;
                    case "suspended":
                        bill.suspended = bool.Parse(value);
                        break;
                    case "countEquipped":
                        bill.includeEquipped = bool.Parse(value);
                        break;
                    case "countTainted":
                        bill.includeTainted = bool.Parse(value);
                        break;
                    case "skillRange":
                        string[] skillRange = value.Split('~');
                        bill.allowedSkillRange = new IntRange(int.Parse(skillRange[0]), int.Parse(skillRange[1]));
                        break;
                    case "ingSearchRadius":
                        bill.ingredientSearchRadius = float.Parse(value);
                        break;
                    case "repeatMode":
                        bill.repeatMode = null;
                        if (BillRepeatModeDefOf.Forever.defName.Equals(value))
                            bill.repeatMode = BillRepeatModeDefOf.Forever;
                        else if (BillRepeatModeDefOf.RepeatCount.defName.Equals(value))
                            bill.repeatMode = BillRepeatModeDefOf.RepeatCount;
                        else if (BillRepeatModeDefOf.TargetCount.defName.Equals(value))
                            bill.repeatMode = BillRepeatModeDefOf.TargetCount;
                        else if ("TD_ColonistCount".Equals(value))
                            EverybodyGetsOneUtil.TryGetRepeatModeDef("TD_ColonistCount", out bill.repeatMode);
                        else if ("TD_XPerColonist".Equals(value))
                            EverybodyGetsOneUtil.TryGetRepeatModeDef("TD_XPerColonist", out bill.repeatMode);
                        else if ("TD_WithSurplusIng".Equals(value))
                            EverybodyGetsOneUtil.TryGetRepeatModeDef("TD_WithSurplusIng", out bill.repeatMode);

                        if (bill.repeatMode == null) {
                            Log.Warning("Unknown repeatMode of [" + value + "] for bill " + bill.recipe.defName);
                            bill = null;
                            return false;
                        }
                        break;
                    case "repeatCount":
                        bill.repeatCount = int.Parse(value);
                        break;
                    case "targetCount":
                        bill.targetCount = int.Parse(value);
                        break;
                    case "pauseWhenSatisfied":
                        bill.pauseWhenSatisfied = bool.Parse(value);
                        break;
                    case "unpauseWhenYouHave":
                        bill.unpauseWhenYouHave = int.Parse(value);
                        break;
                    case "hpRange":
                        string[] hpRange = value.Split('~');
                        bill.hpRange = new FloatRange(float.Parse(hpRange[0]), float.Parse(hpRange[1]));
                        break;
                    case "qualityRange":
                        if (!string.IsNullOrEmpty(value) && value.IndexOf('~') != -1) {
                            string[] qualityRange = value.Split('~');
                            bill.qualityRange = new QualityRange(
                                (QualityCategory)Enum.Parse(typeof(QualityCategory), qualityRange[0], true),
                                (QualityCategory)Enum.Parse(typeof(QualityCategory), qualityRange[1], true));
                        }
                        break;
                    case "onlyAllowedIngredients":
                        bill.limitToAllowedStuff = bool.Parse(value);
                        break;
                    case "storeMode":
                        BillStoreModeDef storeMode = DefDatabase<BillStoreModeDef>.GetNamedSilentFail(value);
                        if (storeMode == null || value == BillStoreModeDefOf.SpecificStockpile.ToString()) {
                            storeMode = BillStoreModeDefOf.BestStockpile;
                        }

                        bill.SetStoreMode(storeMode);
                        break;
                    case "storeZone":
                        var destinationZone = (Zone_Stockpile)Current.Game.CurrentMap.zoneManager.AllZones.Find(z => z.label == value);

                        if (destinationZone != null) bill.SetStoreMode(BillStoreModeDefOf.SpecificStockpile, destinationZone);
                        else bill.SetStoreMode(BillStoreModeDefOf.BestStockpile);
                        break;
                    case "lookIn":
                        var ingredientZone = (Zone_Stockpile)Current.Game.CurrentMap.zoneManager.AllZones.Find(z => z.label == value);

                        bill.includeFromZone = ingredientZone;
                        break;
                    case "allowedDefs":
                        reflection.AllowedDefs.Clear();

                        if (value != null) {
                            HashSet<string> expected = new HashSet<string>(value.Split('/'));
                            IEnumerable<ThingDef> all = reflection.AllStorableThingDefs;

                            var expectedContained = from thing in all
                                                    where expected.Contains(thing.defName)
                                                    select thing;

                            reflection.AllowedDefs.AddRange(expectedContained.ToList());
                        }

                        changed = true;
                        break;

                    case "allowedHitPointsPercents":
                        if (!string.IsNullOrEmpty(value) && value.IndexOf(':') != -1) {
                            string[] values = value.Split(':');
                            float min = float.Parse(values[0]);
                            float max = float.Parse(values[1]);
                            filter.AllowedHitPointsPercents = new FloatRange(min, max);
                            changed = true;
                        }
                        break;

                    case "allowedQualities":
                        if (!string.IsNullOrEmpty(value) && value.IndexOf(':') != -1) {
                            string[] values = value.Split(':');
                            filter.AllowedQualityLevels = new QualityRange(
                                (QualityCategory)Enum.Parse(typeof(QualityCategory), values[0], true),
                                (QualityCategory)Enum.Parse(typeof(QualityCategory), values[1], true));
                            changed = true;
                        }
                        break;

                    case "disallowedSpecialFilters":
                        reflection.DisallowedSpecialFilters.Clear();

                        if (!string.IsNullOrEmpty(value)) {
                            HashSet<string> expected = new HashSet<string>(value.Split('/'));
                            List<SpecialThingFilterDef> l = new List<SpecialThingFilterDef>();

                            foreach (SpecialThingFilterDef specialDef in DefDatabase<SpecialThingFilterDef>.AllDefs) {
                                if (specialDef != null && specialDef.configurable && expected.Contains(specialDef.defName)) {
                                    l.Add(specialDef);
                                }
                            }

                            reflection.DisallowedSpecialFilters = l;
                        }

                        changed = true;
                        break;
                }
            }

            if (changed) reflection.SettingsChangedCallback();
            return false;
        }
    }
}
