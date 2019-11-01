using System;
using System.Collections.Generic;
using System.IO;
using RimWorld;
using Verse;

namespace SaveStorageSettings {
    public class DrugPolicyContainer : SaveableContainer {
        public DrugPolicy Policy { get; private set; }

        public DrugPolicyContainer(DrugPolicy policy) : base(new Version(1, 0)) {
            Policy = policy;
        }


        protected override void SaveFields(StreamWriter writer) {
            WriteField(writer, "name", Policy.label);

            for (int i = 0; i < Policy.Count; i++) {
                DrugPolicyEntry entry = Policy[i];

                WriteField(writer, "drug", i.ToString());
                WriteField(writer, "defName", entry.drug.defName);
                WriteField(writer, "allowedForAddiction", entry.allowedForAddiction.ToString());
                WriteField(writer, "allowedForJoy", entry.allowedForJoy.ToString());
                WriteField(writer, "allowScheduled", entry.allowScheduled.ToString());
                WriteField(writer, "daysFrequency", Math.Round(entry.daysFrequency, 1).ToString());
                WriteField(writer, "onlyIfJoyBelow", Math.Round(entry.onlyIfJoyBelow, 2).ToString());
                WriteField(writer, "onlyIfMoodBelow", Math.Round(entry.onlyIfMoodBelow, 2).ToString());
                WriteField(writer, "takeToInventory", entry.takeToInventory.ToString());
                WriteField(writer, "takeToInventoryTempBuffer", entry.takeToInventoryTempBuffer);
                writer.WriteLine(BREAK);
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

            
            for(int i = 0; i < fieldlist.Count - 1; i++) {
                var fields = fieldlist[i];

                if (fields.ContainsKey("name")) {
                    Policy.label = fields["name"];
                }

                if (TryCreatePolicyEntry(fields, out DrugPolicyEntry entry)) {
                    for (int j = 0; j < Policy.Count; ++j) {
                        if (Policy[j].drug.defName.Equals(entry.drug.defName)) {
                            Policy[j] = entry;
                        }
                    }
                }
            }
        }

        private bool TryCreatePolicyEntry(Dictionary<string, string> fields, out DrugPolicyEntry entry) {
            entry = null;

            foreach (var field in fields) {
                string key = field.Key;
                string value = field.Value;

                switch (key) {
                    case BREAK:
                        return true;
                    case "defName":
                        ThingDef def = DefDatabase<ThingDef>.GetNamed(value);
                        if (def == null) {
                            Log.Warning("Unable to load drug policy with Drug of [" + value + "]");
                            return false;
                        }
                        entry = new DrugPolicyEntry { drug = def };
                        break;
                    case "allowedForAddiction":
                        entry.allowedForAddiction = bool.Parse(value);
                        break;
                    case "allowedForJoy":
                        entry.allowedForJoy = bool.Parse(value);
                        break;
                    case "allowScheduled":
                        entry.allowScheduled = bool.Parse(value);
                        break;
                    case "daysFrequency":
                        entry.daysFrequency = int.Parse(value);
                        break;
                    case "onlyIfJoyBelow":
                        entry.onlyIfJoyBelow = float.Parse(value);
                        break;
                    case "onlyIfMoodBelow":
                        entry.onlyIfMoodBelow = float.Parse(value);
                        break;
                    case "takeToInventory":
                        entry.takeToInventory = int.Parse(value);
                        break;
                    case "takeToInventoryTempBuffer":
                        entry.takeToInventoryTempBuffer = value;
                        break;
                }
            }

            return false;
        }
    }
}
