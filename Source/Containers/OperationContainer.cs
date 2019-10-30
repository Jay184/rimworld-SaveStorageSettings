using System;
using System.Collections.Generic;
using System.IO;
using RimWorld;
using Verse;

namespace SaveStorageSettings {
    public class OperationContainer : SaveableContainer {
        public Pawn Pawn { get; private set; }

        public OperationContainer(Pawn pawn) : base(new Version(1, 0)) {
            Pawn = pawn;
        }


        protected override void SaveFields(StreamWriter writer) {
            foreach(Bill bill in Pawn.BillStack) {
                if (bill is Bill_Medical medicalBill) {
                    WriteField(writer, "recipeDefName", medicalBill.recipe.defName);
                    WriteField(writer, "suspended", medicalBill.suspended.ToString());

                    if (bill.recipe.targetsBodyPart) {
                        WriteField(writer, "part", $"{ medicalBill.Part.Label }:{ medicalBill.Part.def.defName }");
                    }

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


            Pawn.BillStack.Clear();
            for(int i = 0; i < fieldlist.Count - 1; i++) {
                var fields = fieldlist[i];
                if (TryCreateBill(fields, out Bill_Medical medicalBill)) {
                    Pawn.BillStack.AddBill(medicalBill);
                }
            }
        }

        private bool TryCreateBill(Dictionary<string, string> fields, out Bill_Medical bill) {
            bill = null;

            foreach (var field in fields) {
                string key = field.Key;
                string value = field.Value;

                switch (key) {
                    case BREAK:
                        return true;
                    case "recipeDefName":
                        var def = DefDatabase<RecipeDef>.GetNamed(value);

                        if (def == null) {
                            Log.Warning("Unable to load bill with RecipeDef of [" + value + "]");
                            return false;
                        }

                        bill = new Bill_Medical(def);
                        break;
                    case "suspended":
                        bill.suspended = bool.Parse(value);
                        break;
                    case "part":
                        string[] parts = value.Split(':');
                        if (bill.recipe.defName.StartsWith("Remove")) {
                            bill.Part = null;
                            string partToFind = parts[0];
                            foreach (BodyPartRecord part in Pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null)) {
                                if (part.Label.Equals(partToFind)) {
                                    bill.Part = part;
                                    break;
                                }
                            }
                            if (bill.Part == null) {
                                Log.Warning("Pawn [" + Pawn.Name.ToStringShort + "] does not have body part [" + partToFind + "] to have removed.");
                                return false;
                            }
                        } else if (bill.recipe.defName.StartsWith("Install")) {
                            string partToFind = parts[1];
                            foreach (BodyPartRecord p in Pawn.RaceProps.body.AllParts) {
                                if (p.def.defName.Equals(partToFind)) {
                                    bill.Part = p;
                                    break;
                                }
                            }
                            if (bill.Part == null) {
                                Log.Warning("Unknown body part [" + partToFind + "].");
                                return false;
                            }
                        }
                        break;
                }
            }

            return false;
        }
    }
}
