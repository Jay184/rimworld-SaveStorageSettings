using SaveStorageSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RimWorld {
    public abstract class FileListDialog : Window {
        protected const float BoxMargin = 20f;

        protected const float EntrySpacing = 3f;

        protected const float EntryMargin = 1f;

        protected const float NameExtraLeftMargin = 15f;

        protected const float InfoExtraLeftMargin = 270f;

        protected const float DeleteButtonSpace = 5f;

        protected const float EntryHeight = 36f;

        protected const float NameTextFieldWidth = 400f;

        protected const float NameTextFieldHeight = 35f;

        protected const float NameTextFieldButtonSpace = 20f;

        protected string buttonLabel = "Error";

        protected float bottomAreaHeight;

        protected List<FileInfo> files = new List<FileInfo>();

        protected Vector2 scrollPosition = Vector2.zero;

        protected string typingName = string.Empty;

        private bool focusedNameArea;

        private static readonly Color DefaultFileTextColor = new Color(1f, 1f, 0.6f);

        public static readonly Color UnimportantTextColor = new Color(1f, 1f, 1f, 0.5f);

        public string DirectoryName {
            get => directoryName;
            set {
                directoryName = value;
                ReloadFiles();
            }
        }
        private string directoryName;


        public override Vector2 InitialSize => new Vector2(600f, 700f);
        protected virtual bool ShowTextInput => false;

        public FileListDialog(string type) {
            DirectoryName = type;

            closeOnClickedOutside = true;
            doCloseButton = true;
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
        }

        protected abstract void DoFileInteraction(FileInfo fi);

        public override void DoWindowContents(Rect inRect) {
            Vector2 vector = new Vector2(inRect.width - 16f, 36f);
            Vector2 vector2 = new Vector2(100f, vector.y - 2f);
            inRect.height -= 45f;
            float num = vector.y + 3f;
            float rectPosY = 0f;
            float height = files.Count * num;
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, height);
            Rect outRect = new Rect(inRect.AtZero());
            outRect.height -= bottomAreaHeight;

            if (ShowTextInput) {
                outRect.height -= 64f;
            }

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);

            for (int i = 0; i < files.Count; i++) {
                FileInfo current = files[i];
                Rect rect = new Rect(0f, rectPosY, vector.x, vector.y);

                if (i % 2 == 0) {
                    Widgets.DrawAltRect(rect);
                }

                Rect position = rect.ContractedBy(1f);

                GUI.BeginGroup(position);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(current.Name);
                GUI.color = FileNameColor(current);
                Rect rect2 = new Rect(15f, 0f, position.width, position.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Small;
                Widgets.Label(rect2, fileNameWithoutExtension);
                GUI.color = Color.white;
                Rect rect3 = new Rect(270f, 0f, 200f, position.height);
                DrawDateAndVersion(current, rect3);
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
                float num4 = vector.x - 2f - vector2.x - vector2.y;
                Rect rect4 = new Rect(num4, 0f, vector2.x, vector2.y);
                if (Widgets.ButtonText(rect4, buttonLabel, true, false, true)) {
                    DoFileInteraction(current);
                }
                Rect rect5 = new Rect(num4 + vector2.x + 5f, 0f, vector2.y, vector2.y);
                if (Widgets.ButtonImage(rect5, HarmonyPatches.DeleteXTexture)) {
                    FileInfo localFile = current;

                    string message = TranslatorFormattedStringExtensions.Translate("ConfirmDelete", new NamedArgument(localFile.Name.Replace(localFile.Extension, ""), ""));

                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(message, delegate {
                        localFile.Delete();
                        ReloadFiles();
                    }, true, null));
                }
                TooltipHandler.TipRegion(rect5, "SaveStorageSettings.DeleteSettings".Translate());

                GUI.EndGroup();
                rectPosY += vector.y + 3f;
            }

            Widgets.EndScrollView();

            if (ShowTextInput) {
                DoTypeInField(inRect.AtZero());
            }
        }

        protected void ReloadFiles() {
            files.Clear();
            if (TryGetBaseDirectory(out string path)) {
                string destination = Path.Combine(path, DirectoryName);
                if (!Directory.Exists(destination)) return;

                foreach (string file in Directory.GetFiles(destination)) {
                    files.Add(new FileInfo(file));
                }
            }
        }

        protected virtual void DoTypeInField(Rect rect) {
            GUI.BeginGroup(rect);

            bool flag = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;
            float y = rect.height - 52f;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.SetNextControlName("MapNameField");
            Rect rect2 = new Rect(5f, y, 400f, 35f);
            Rect rect3 = new Rect(420f, y, rect.width - 400f - 20f, 35f);
            string str = Widgets.TextField(rect2, typingName);

            if (GenText.IsValidFilename(str)) {
                typingName = str;
            }

            if (!focusedNameArea) {
                UI.FocusControl("MapNameField", this);
                focusedNameArea = true;
            }

            if (Widgets.ButtonText(rect3, "SaveGameButton".Translate(), true, false, true) || flag) {
                if (typingName.NullOrEmpty()) {
                    Messages.Message("NeedAName".Translate(), MessageTypeDefOf.RejectInput);
                } else {
                    TryGetFileInfo(DirectoryName, typingName, out FileInfo fi);
                    DoFileInteraction(fi);
                }
            }

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
        }

        protected virtual Color FileNameColor(FileInfo fi) {
            return DefaultFileTextColor;
        }

        public static void DrawDateAndVersion(FileInfo fi, Rect rect) {
            GUI.BeginGroup(rect);
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect2 = new Rect(0f, 2f, rect.width, rect.height / 2f);
            GUI.color = UnimportantTextColor;
            Widgets.Label(rect2, fi.LastWriteTime.ToString("g"));
            Rect rect3 = new Rect(0f, rect2.yMax, rect.width, rect.height / 2f);
            GUI.EndGroup();
        }


        public static bool TryGetFileInfo(string storageTypeName, string fileName, out FileInfo fi) {
            fi = null;

            if (TryGetBaseDirectory(out string path)) {
                string destination = Path.Combine(path, storageTypeName);
                Directory.CreateDirectory(destination);

                fi = new FileInfo(Path.Combine(destination, $"{ fileName }.txt"));
            }

            return fi != null;
        }
        private static bool TryGetBaseDirectory(out string path) {
            try {
                MethodInfo method = typeof(GenFilePaths).GetMethod("FolderUnderSaveData", BindingFlags.Static | BindingFlags.NonPublic);

                try {
                    path = (string)method.Invoke(null, new object[] { "SaveStorageSettings" });
                    return true;

                } catch (Exception ex) when (ex is ArgumentException || ex is TargetParameterCountException) {
                    Log.Error($"SaveStorageSettings: Failed to get folder name.{ Environment.NewLine }{ ex }");
                    path = null;
                    return false;
                }

            } catch (Exception ex) when (ex is AmbiguousMatchException || ex is ArgumentNullException) {
                Log.Error($"SaveStorageSettings: Failed to get save directory.{ Environment.NewLine }{ ex }");
                path = null;
                return false;
            }
        }
    }
}
