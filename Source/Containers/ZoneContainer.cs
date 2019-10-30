using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace SaveStorageSettings {
    public class ZoneContainer : SaveableContainer {
        public Color Color { get; private set; }
        public StoragePriority Priority { get; private set; }
        public List<ThingDef> AllowedThings { get; private set; }
        public List<SpecialThingFilterDef> DisallowedSpecialThings { get; private set; }
        public FloatRange? AllowedHitPoints { get; private set; }
        public QualityRange? AllowedQualities { get; private set; }

        private ThingFilterReflection Reflection { get; set; }
        private Zone_Stockpile Stockpile { get; set; }


        public ZoneContainer(Zone_Stockpile stockpile) : base(new Version(1, 0)) {
            ThingFilter filter = stockpile.settings.filter;

            Stockpile = stockpile;
            Color = stockpile.color;
            Priority = stockpile.settings.Priority;
            Reflection = new ThingFilterReflection(filter);

            // Allowed things
            var allowed = from thing in Reflection.AllStorableThingDefs
                          where filter.Allows(thing)
                          select thing;

            AllowedThings = new List<ThingDef>(allowed);


            // Can configure HP
            if (filter.allowedHitPointsConfigurable) {
                AllowedHitPoints = filter.AllowedHitPointsPercents;
            }


            // Can configure Qualities
            if (filter.allowedQualitiesConfigurable) {
                AllowedQualities = filter.AllowedQualityLevels;
            }


            // Disallowed Specials
            DisallowedSpecialThings = Reflection.DisallowedSpecialFilters;
        }


        protected override void SaveFields(StreamWriter writer) {
            StringBuilder builder = new StringBuilder();

            WriteField(writer, "color", $"{ Stockpile.color.r },{ Stockpile.color.g },{ Stockpile.color.b },{ Stockpile.color.a }");
            WriteField(writer, "priority", Priority.ToString());

            foreach (ThingDef thing in AllowedThings) {
                if (builder.Length > 0) builder.Append("/");
                builder.Append(thing.defName);
            }
            WriteField(writer, "allowedDefs", builder.ToString());


            if (AllowedHitPoints.HasValue) {
                builder = new StringBuilder();

                builder.Append(AllowedHitPoints.Value.min.ToString("N4"));
                builder.Append(":");
                builder.Append(AllowedHitPoints.Value.max.ToString("N4"));

                WriteField(writer, "allowedHitPointsPercents", builder.ToString());
            }

            if (AllowedQualities.HasValue) {
                builder = new StringBuilder();

                builder.Append(AllowedQualities.Value.min.ToString());
                builder.Append(":");
                builder.Append(AllowedQualities.Value.max.ToString());

                WriteField(writer, "allowedQualities", builder.ToString());
            }

            builder = new StringBuilder();
            foreach (SpecialThingFilterDef def in DisallowedSpecialThings) {
                if (builder.Length > 0) builder.Append("/");
                builder.Append(def.defName);
            }
            WriteField(writer, "disallowedSpecialFilters", builder.ToString());
        }
        protected override void LoadFields(StreamReader reader) {
            Dictionary<string, string> fields = new Dictionary<string, string>();
            while (!reader.EndOfStream) {
                if (TryReadField(reader, out Pair<string, string> pair)) {
                    fields.Add(pair.First, pair.Second);
                }
            }


            bool changed = false;

            foreach (var pair in fields) {
                string key = pair.Key;
                string value = pair.Value;

                switch (key) {
                    case BREAK: return;
                    case "color":
                        string[] color = value.Split(',');
                        Stockpile.color = new Color(
                            float.Parse(color[0]),
                            float.Parse(color[1]),
                            float.Parse(color[2]),
                            float.Parse(color[3]));

                        FieldInfo materialIntInfo = typeof(Zone).GetField("materialInt", BindingFlags.NonPublic | BindingFlags.Instance);
                        materialIntInfo.SetValue(Stockpile, null);

                        MapDrawer mapDrawer = Find.CurrentMap.mapDrawer;
                        foreach (IntVec3 cell in Stockpile.cells) {
                            mapDrawer.SectionAt(cell).RegenerateLayers(MapMeshFlag.Zone);
                        }

                        break;
                    case "priority":
                        Stockpile.settings.Priority = (StoragePriority)Enum.Parse(typeof(StoragePriority), value, true);
                        changed = true;
                        break;

                    case "allowedDefs":
                        Reflection.AllowedDefs.Clear();

                        if (value != null) {
                            HashSet<string> expected = new HashSet<string>(value.Split('/'));
                            IEnumerable<ThingDef> all = Reflection.AllStorableThingDefs;

                            var expectedContained = from thing in all
                                                    where expected.Contains(thing.defName)
                                                    select thing;

                            Reflection.AllowedDefs.AddRange(expectedContained.ToList());
                        }

                        changed = true;
                        break;

                    case "allowedHitPointsPercents":
                        if (!string.IsNullOrEmpty(value) && value.IndexOf(':') != -1) {
                            string[] values = value.Split(':');
                            float min = float.Parse(values[0]);
                            float max = float.Parse(values[1]);
                            Stockpile.settings.filter.AllowedHitPointsPercents = new FloatRange(min, max);
                            changed = true;
                        }
                        break;

                    case "allowedQualities":
                        if (!string.IsNullOrEmpty(value) && value.IndexOf(':') != -1) {
                            string[] values = value.Split(':');
                            Stockpile.settings.filter.AllowedQualityLevels = new QualityRange(
                                (QualityCategory)Enum.Parse(typeof(QualityCategory), values[0], true),
                                (QualityCategory)Enum.Parse(typeof(QualityCategory), values[1], true));
                            changed = true;
                        }
                        break;

                    case "disallowedSpecialFilters":
                        Reflection.DisallowedSpecialFilters.Clear();

                        if (!string.IsNullOrEmpty(value)) {
                            HashSet<string> expected = new HashSet<string>(value.Split('/'));
                            List<SpecialThingFilterDef> l = new List<SpecialThingFilterDef>();

                            foreach (SpecialThingFilterDef def in DefDatabase<SpecialThingFilterDef>.AllDefs) {
                                if (def != null && def.configurable && expected.Contains(def.defName)) {
                                    l.Add(def);
                                }
                            }

                            Reflection.DisallowedSpecialFilters = l;
                        }

                        changed = true;
                        break;
                }
            }

            if (changed) Reflection.SettingsChangedCallback();
        }
    }
}
