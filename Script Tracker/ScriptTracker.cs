#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;

namespace DreadScripts.ScriptTracker
{
    internal sealed class ScriptTracker : AssetPostprocessor
    {
        internal const string PREFS_SETTINGSGUID = "ScriptTrackerSettingsGUID";
        private static readonly string warnDLL = "\nWARNING: This is a DLL and can't be scanned! Only Import DLLs from trusted sources!";
        private static bool check = true;
        private static bool flagged = true;
        private static bool checkDLL = true;

        private static string GetPath()
        {
            string myPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("ScriptTracker")[0]);
            myPath = myPath.Substring(0, myPath.LastIndexOf('/'));
            return myPath;
        }

        private static bool KeywordMatch(string content, string match)
        {
            return content.IndexOf(match, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static string StringToHash(string s)
        {
            using (HashAlgorithm hashing = SHA256.Create())
            {
                byte[] hash = null;

                hash = hashing.ComputeHash(Encoding.UTF8.GetBytes(s));

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] unused1, string[] unused2, string[] unused3)
        {
            if (importedAssets.Length == 0)
                return;
            ScriptTrackerSettings settings = LoadScriptTrackerSettings();

            check = EditorPrefs.GetBool("ScriptTrackerCheck", true);
            flagged = EditorPrefs.GetBool("ScriptTrackerFlagged", true);

            if (!check && !checkDLL) return;
            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (string path in importedAssets)
                {
                    bool isScript = Path.GetExtension(path) == ".cs";
                    bool isDLL = Path.GetExtension(path) == ".dll";
                    string pathGUID = AssetDatabase.AssetPathToGUID(path);
                    bool hasKey = EditorPrefs.HasKey("ScriptTracker-" + pathGUID);

                    if (isScript && check)
                    {
                        bool isLow = false, isNormal = false, isHigh = false;
                        Dictionary<int, string> triggers = new Dictionary<int, string>();

                        string[] scriptLines = File.ReadAllLines(path);

                        string key = hasKey ? EditorPrefs.GetString("ScriptTracker-" + pathGUID) : "";
                        string scriptHash = StringToHash(string.Join("", scriptLines));
                        bool isChanged = key != scriptHash;

                        bool IterateSettings(int lineIndex, List<string> keywords, string flagMessage, ref bool riskBool)
                        {
                            foreach (var k in keywords)
                            {
                                if (string.IsNullOrEmpty(k)) continue;
                                if (!KeywordMatch(scriptLines[lineIndex], k)) continue;

                                triggers.Add(lineIndex, k);
                                scriptLines[lineIndex] += flagMessage;
                                riskBool = true;
                                return true;
                            }

                            return false;
                        }

                        for (int i = 0; i < scriptLines.Length; i++)
                        {
                            if (IterateSettings(i, settings.HighRisk, " //[ST] FLAGGED HIGH RISK", ref isHigh)) break;
                            if (IterateSettings(i, settings.NormalRisk, " //[ST] FLAGGED NORMAL RISK", ref isNormal)) break;
                            IterateSettings(i, settings.LowRisk, " //[ST] FLAGGED LOW RISK", ref isLow);
                        }

                        string warnMessage = "Importing Script: " + Path.GetFileNameWithoutExtension(path);
                        if (isHigh) warnMessage += "\nWARNING: FLAGGED AS HIGH RISK!";
                        else if (isNormal) warnMessage += "\nWarning: Flagged as Normal Risk!";
                        else if (isLow) warnMessage += "\nWarning: Flagged as Low Risk!";

                        foreach (var p in triggers)
                        {
                            warnMessage += $"\n Line {p.Key} - {p.Value}";
                        }

                        if ((!hasKey && (isNormal || isLow))
                            || (hasKey && isNormal && isChanged)
                            || isHigh
                            || !flagged)
                            switch (EditorUtility.DisplayDialogComplex("Importing Script", warnMessage, "Allow", "Delete", "Revise"))
                            {
                                case 0:
                                    EditorPrefs.SetString("ScriptTracker-" + pathGUID, scriptHash);
                                    Debug.Log(Path.GetFileNameWithoutExtension(path) + " added to whitelist.");
                                    break;
                                case 1:
                                    File.Delete(path);
                                    File.Delete(path + ".meta");
                                    break;
                                case 2:
                                    string newFilePath = path.Substring(0, path.Length - 2) + "txt";
                                    File.Move(path, newFilePath);
                                    File.WriteAllLines(newFilePath, scriptLines);
                                    File.Delete(path + ".meta");
                                    break;
                            }
                    }
                    else
                    {
                        if (isDLL)
                        {
                            if ((!hasKey && settings.promptDLL) || (settings.promptDLL && settings.alwaysDLL) || !flagged)
                                if (EditorUtility.DisplayDialog("Importing DLL", "Importing DLL: " + Path.GetFileNameWithoutExtension(path) + warnDLL, "Allow", "Delete"))
                                {
                                    EditorPrefs.SetString("ScriptTracker-" + AssetDatabase.AssetPathToGUID(path), "Set");
                                }
                                else
                                {
                                    File.Delete(path);
                                    File.Delete(path + ".meta");
                                }
                        }
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            PostPackageImport();
            AssetDatabase.Refresh();

        }

        private static bool wasCompiling;
        private static bool wasImporting;

        [InitializeOnLoadMethod]
        private static void HookPackageImport()
        {
            AssetDatabase.importPackageStarted -= PrePackageImport;
            AssetDatabase.importPackageStarted += PrePackageImport;
        }

        private static void PrePackageImport(string name)
        {
            wasImporting = true;
            wasCompiling = (bool)GetType("UnityEditor.EditorApplication, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetMethod("CanReloadAssemblies", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, null);
            EditorApplication.LockReloadAssemblies();
        }

        private static void PostPackageImport()
        {
            if (wasImporting)
            {
                if (wasCompiling) EditorApplication.UnlockReloadAssemblies();
                wasImporting = false;
            }
        }

        internal static ScriptTrackerSettings LoadScriptTrackerSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ScriptTrackerSettings>(AssetDatabase.GUIDToAssetPath(PlayerPrefs.GetString(PREFS_SETTINGSGUID)));
            if (settings) return settings;

            string defaultGUID = "9f5f97932281c834bbcc8e130bd396ce";
            settings = AssetDatabase.LoadAssetAtPath<ScriptTrackerSettings>(defaultGUID);
            if (settings)
            {
                PlayerPrefs.SetString(PREFS_SETTINGSGUID, defaultGUID);
                return settings;
            }

            settings = ScriptableObject.CreateInstance<ScriptTrackerSettings>();
            string folderPath = "Assets/DreadScripts/Script Tracker";
            ReadyPath(folderPath);
            string filePath = $"{folderPath}/Default Settings.asset";
            AssetDatabase.CreateAsset(settings, filePath);
            AssetDatabase.ImportAsset(filePath);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(settings, out string guid, out long id);
            PlayerPrefs.SetString(PREFS_SETTINGSGUID, guid);
            return settings;
        }

        private static void ReadyPath(string folderPath)
        {
            string[] folderNames = folderPath.Split('/');
            string[] folderPaths = new string[folderNames.Length];

            for (int i = 0; i < folderNames.Length; i++)
            {
                folderPaths[i] = folderNames[0];
                for (int j = 1; j <= i; j++)
                    folderPaths[i] += $"/{folderNames[j]}";
            }

            for (int i = 1; i < folderPaths.Length; i++)
                if (!AssetDatabase.IsValidFolder(folderPaths[i]))
                    AssetDatabase.CreateFolder(folderPaths[i - 1], folderNames[i]);
        }

        private static System.Type GetType(string typeName)
        {
            var myType = System.Type.GetType(typeName);
            if (myType != null)
                return myType;
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                myType = assembly.GetType(typeName);
                if (myType != null)
                    return myType;
            }
            return null;
        }

    }

    [CustomEditor(typeof(ScriptTrackerSettings))]
    internal sealed class ScriptTrackerEditor : Editor
    {
        private static Vector2 scroll;

        private static GUIContent[] riskTabs =
        {
            new GUIContent("Low Risk", "Scripts flagged as Low Risk need to be allowed only once."),
            new GUIContent("Normal Risk", "Scripts flagged as Normal Risk need to be allowed once or each time they are changed"),
            new GUIContent("High Risk", "Scripts flagged as High Risk need to be allowed any time they are imported")
        };

        private static int tabIndex;

        SerializedObject settings;
        SerializedProperty lowProp;
        SerializedProperty normalProp;
        SerializedProperty highProp;
        SerializedProperty dllProp;
        SerializedProperty alwaysdllProp;
        private void OnEnable()
        {
            settings = new SerializedObject((ScriptTrackerSettings)target);
            lowProp = settings.FindProperty("LowRisk");
            normalProp = settings.FindProperty("NormalRisk");
            highProp = settings.FindProperty("HighRisk");
            dllProp = settings.FindProperty("promptDLL");
            alwaysdllProp = settings.FindProperty("alwaysDLL");
        }
        public override void OnInspectorGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            settings.Update();
            dllProp.boolValue = EditorGUILayout.Toggle("Warn for DLLs", dllProp.boolValue);
            if (dllProp.boolValue)
                alwaysdllProp.boolValue = EditorGUILayout.Toggle("Always Warn for DLLs", alwaysdllProp.boolValue);

            tabIndex = GUILayout.Toolbar(tabIndex, riskTabs, "toolbarbutton");

            void DisplayRiskProperty(SerializedProperty prop)
            {
                for (int i = 0; i < prop.arraySize; i++)
                {
                    using (new GUILayout.HorizontalScope("box"))
                    {
                        if (GUILayout.Button("X", GUI.skin.label, GUILayout.Width(17), GUILayout.Height(17)))
                        {
                            prop.DeleteArrayElementAtIndex(i);
                            continue;
                        }
                        prop.GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(prop.GetArrayElementAtIndex(i).stringValue);
                    }
                }
                if (GUILayout.Button("Add", "toolbarbutton"))
                    prop.InsertArrayElementAtIndex(prop.arraySize);
            }

            switch (tabIndex)
            {
                case 0:
                    DisplayRiskProperty(lowProp);
                    break;
                case 1:
                    DisplayRiskProperty(normalProp);
                    break;
                case 2:
                    DisplayRiskProperty(highProp);
                    break;
            }

            settings.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
        }
    }

    internal sealed class ScriptTrackerWindow : EditorWindow
    {
        ScriptTrackerSettings settings;
        private enum CST
        {
            AllScripts,
            Flagged,
            Disable
        }
        private static CST checkScriptType = CST.Flagged;

        [MenuItem("DreadTools/Scripts Settings/Script Tracker")]
        private static void showWindow()
        {
            GetWindow<ScriptTrackerWindow>(false, "Script Tracker Settings", true);
        }
        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            checkScriptType = (CST)EditorGUILayout.EnumPopup("Prompt Scripts", checkScriptType);
            if (EditorGUI.EndChangeCheck())
            {
                switch ((int)checkScriptType)
                {
                    case 0:
                        EditorPrefs.SetBool("ScriptTrackerFlagged", false);
                        EditorPrefs.SetBool("ScriptTrackerCheck", true);
                        break;
                    case 1:
                        EditorPrefs.SetBool("ScriptTrackerFlagged", true);
                        EditorPrefs.SetBool("ScriptTrackerCheck", true);
                        break;
                    case 2:
                        EditorPrefs.SetBool("ScriptTrackerFlagged", false);
                        EditorPrefs.SetBool("ScriptTrackerCheck", false);
                        break;
                }
            }

            if (checkScriptType== CST.Flagged)
            {
                ScriptTrackerSettings dummy;
                EditorGUI.BeginChangeCheck();
                dummy = (ScriptTrackerSettings)EditorGUILayout.ObjectField("Settings", settings, typeof(ScriptTrackerSettings), false);
                if (EditorGUI.EndChangeCheck())
                {
                    if (dummy!=null)
                    {
                        settings = dummy;
                        PlayerPrefs.SetString(ScriptTracker.PREFS_SETTINGSGUID, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(settings)));
                    }
                }
            }

            if (GUILayout.Button("Allow Current Project Scripts and DLLs"))
            {
                string[] paths = AssetDatabase.FindAssets("t:Script");
                foreach (var p in paths)
                {
                    EditorPrefs.SetBool(p, true);
                }
                Debug.Log("All Scripts in projects added to whitelist");
            }
        }

        private void OnEnable()
        {
            bool Flagged = EditorPrefs.GetBool("ScriptTrackerFlagged", true);
            bool Check = EditorPrefs.GetBool("ScriptTrackerCheck", true);
            checkScriptType = Check ? (Flagged ? CST.Flagged : CST.AllScripts) : CST.Disable;
            settings = ScriptTracker.LoadScriptTrackerSettings();
        }

    }
}
#endif