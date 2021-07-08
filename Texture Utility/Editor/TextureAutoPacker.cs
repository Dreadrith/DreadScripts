using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using UnityEngine.Networking;

namespace DreadScripts
{
    public class TextureAutoPackerWindow : EditorWindow
    {
        private static TextureAutoPackerData data;
        private static SerializedObject serializedObject;
        private static SerializedProperty _active;
        private static SerializedProperty _activeModules;
        private static UnityEditorInternal.ReorderableList modulesList;
        private static Texture2D titleTexture;

        [MenuItem("DreadTools/Utilities/Texture Auto-Packer")]
        public static void ShowWindow()
        {
            EditorWindow w = GetWindow<TextureAutoPackerWindow>(false, "Texture Auto-Packer", true);
            if (!titleTexture)
            {
                titleTexture = TextureUtility.GetColors((Texture2D)EditorGUIUtility.IconContent("Texture2D Icon").image, 16, 16, out _);
                titleTexture.Apply();
            }
            w.titleContent.image = titleTexture;
        }
        private void OnEnable()
        {
            data = TextureAutoPackerData.GetInstance();
            serializedObject = new SerializedObject(data);
            _active = serializedObject.FindProperty("active");
            _activeModules = serializedObject.FindProperty("activeModules");
            modulesList = new UnityEditorInternal.ReorderableList(serializedObject, _activeModules, true, true, true, false)
            {
                drawElementCallback = DrawElement,
                drawHeaderCallback = DrawHeader
            };
        }

        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Active Modules");
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(index < _activeModules.arraySize && index >= 0))
                return;
            rect.y += 2;
            rect.height = EditorGUIUtility.singleLineHeight;
            if (GUI.Button(new Rect(rect.x, rect.y, 20, rect.height), "X"))
            {
                _activeModules.DeleteArrayElementAtIndex(index);
                return;
            }
            rect.x += 20;
            rect.width -= 20;
            _activeModules.GetArrayElementAtIndex(index).objectReferenceValue = (TextureAutoPackerModule)EditorGUI.ObjectField(rect, _activeModules.GetArrayElementAtIndex(index).objectReferenceValue, typeof(TextureAutoPackerModule), false);
        }

        private void OnGUI()
        {
            serializedObject.Update();
            Color og = GUI.backgroundColor;
            GUI.backgroundColor = _active.boolValue ? Color.green : Color.grey;
                _active.boolValue = GUILayout.Toggle(_active.boolValue, new GUIContent("Active","Determine whether the Auto-Packer should initiate on texture import"), "Toolbarbutton");
            GUI.backgroundColor = og;
            
            EditorGUI.BeginDisabledGroup(!_active.boolValue);
            modulesList.DoLayoutList();
            if (GUILayout.Button(new GUIContent("Force Check", "Initiate the Auto-Packer without having to trigger a texture import")))
            {
                TextureAutoPacker.InitiateAutoPacking();
                TextureAutoPackerProcessor.OnAutoPackingEnd();
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(TextureAutoPackerModule))]
    public class TextureAutoPacker : Editor
    {
        static GUIContent removeIcon;
        TextureAutoPackerModule module;
        SerializedProperty packingList;
        GUIStyle freeButtonStyle;
        public override void OnInspectorGUI()
        {
            freeButtonStyle = new GUIStyle("toolbarbutton") { padding = new RectOffset(1, 1, 1, 1) };
            serializedObject.Update();

            if (GUILayout.Button(new GUIContent("Add","Create a new Auto-Packed texture"), "toolbarbutton"))
            {
                module.packedTextures.Add(new AutoPackedTexture());
                serializedObject.Update();
            }

            for (int i = packingList.arraySize - 1; i >= 0; i--)
                DrawPackingProperty(i);


            serializedObject.ApplyModifiedProperties();
        }
        private void OnEnable()
        {
            module = (TextureAutoPackerModule)target;
            packingList = serializedObject.FindProperty("packedTextures");
            removeIcon = EditorGUIUtility.IconContent("winbtn_win_close");
        }

        static bool Running;
        public static bool InitiateAutoPacking()
        {
            bool HasPacked=false;
            if (Running)
                return true;
            Running = true;
            TextureAutoPackerData data = TextureAutoPackerData.GetInstance();
            if (!data.active)
                return false;
            data.activeModules.ForEach(m =>
            {
                if (!m)
                    goto Skip;

                SerializedObject module = new SerializedObject(m);
                module.Update();
                for (int i = 0; i < m.packedTextures.Count; i++)
                {
                    if (m.packedTextures[i].WasModified())
                    {
                        HasPacked = true;
                        string newTexturePath = m.packedTextures[i].Pack();

                        if (string.IsNullOrEmpty(newTexturePath))
                            goto Skip;

                        SerializedProperty packedTexture = module.FindProperty("packedTextures").GetArrayElementAtIndex(i);
                        SerializedProperty hashes = packedTexture.FindPropertyRelative("channelsHashes");
                        for (int j = 0; j < 4; j++)
                        {
                            hashes.GetArrayElementAtIndex(j).stringValue = string.Empty;
                            if (m.packedTextures[i].channels[j].texture)
                                hashes.GetArrayElementAtIndex(j).stringValue = m.packedTextures[i].channels[j].texture.imageContentsHash.ToString();
                        }
                        packedTexture.FindPropertyRelative("forceModified").boolValue = false;
                        AssetDatabase.ImportAsset(newTexturePath, ImportAssetOptions.ForceUpdate);
                        TextureAutoPackerProcessor.PathToProperty.Add(new System.Tuple<string, SerializedProperty>(newTexturePath, packedTexture.FindPropertyRelative("packed")));
                    }
                }
                module.ApplyModifiedPropertiesWithoutUndo();

            Skip:;
            });
            Running = false;
            
            return HasPacked;
        }

        public void DrawPackingProperty(int index)
        {
            SerializedProperty t = packingList.GetArrayElementAtIndex(index);
            SerializedProperty _name = t.FindPropertyRelative("name");
            SerializedProperty _expanded = t.FindPropertyRelative("expanded");
            SerializedProperty _channels = t.FindPropertyRelative("channels");
            SerializedProperty _packed = t.FindPropertyRelative("packed");
            SerializedProperty _encoding = t.FindPropertyRelative("encoding");
            SerializedProperty _quality = t.FindPropertyRelative("jpgQuality");
            SerializedProperty _modified = t.FindPropertyRelative("forceModified");
            AutoPackedTexture autoTexture = ((TextureAutoPackerModule)t.serializedObject.targetObject).packedTextures[index];
            using (new GUILayout.VerticalScope("box"))
            {
                using (new GUILayout.HorizontalScope())
                {
                    using (new GUILayout.HorizontalScope("toolbarbutton", GUILayout.Width(12)))
                        _expanded.boolValue = GUILayout.Toggle(_expanded.boolValue, GUIContent.none, "foldout", GUILayout.Width(10));

                    using (new GUILayout.HorizontalScope("toolbarbutton"))
                        _name.stringValue = EditorGUILayout.TextField(GUIContent.none, _name.stringValue, GUI.skin.label);

                    if (GUILayout.Button(removeIcon, freeButtonStyle, GUILayout.Width(17)))
                    {
                        packingList.DeleteArrayElementAtIndex(index);
                        return;
                    }
                }
                if (_expanded.boolValue)
                {
                    using (new GUILayout.HorizontalScope("box"))
                    {
                        _encoding.enumValueIndex = (int)(TextureUtility.TexEncoding)EditorGUILayout.EnumPopup((TextureUtility.TexEncoding)_encoding.enumValueIndex, GUILayout.Width(95));

                        EditorGUI.BeginDisabledGroup(_encoding.enumValueIndex != 1);
                        EditorGUIUtility.labelWidth = 50;
                        _quality.intValue = EditorGUILayout.IntSlider("Quality", _quality.intValue, 1, 100);
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUI.EndDisabledGroup();
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        for (int i = 0; i < _channels.arraySize; i++)
                        {
                            if (DrawChannelProperty(_channels.GetArrayElementAtIndex(i)))
                                _modified.boolValue = true;
                        }
                        GUILayout.Label("", GUI.skin.verticalSlider, GUILayout.Height(133));
                        using (new GUILayout.VerticalScope("box"))
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();
                                GUILayout.Label("Packed", "boldlabel");
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.Label(GUIContent.none);
                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();
                                _packed.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField("", _packed.objectReferenceValue, typeof(Texture2D), false, GUILayout.Width(66));
                                GUILayout.FlexibleSpace();
                            }
                            using (new GUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button("Force Pack", "toolbarbutton"))
                                {
                                    string newTexturePath = autoTexture.Pack();
                                    if (!string.IsNullOrEmpty(newTexturePath))
                                    {
                                        AssetDatabase.ImportAsset(newTexturePath);
                                        _packed.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Texture2D>(newTexturePath);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool DrawChannelProperty(SerializedProperty channel)
        {
            bool edited = false;
            SerializedProperty _name = channel.FindPropertyRelative("name");
            SerializedProperty _texture = channel.FindPropertyRelative("texture");
            SerializedProperty _invert = channel.FindPropertyRelative("invert");
            SerializedProperty _mode = channel.FindPropertyRelative("mode");
            

            GUIStyle buttonGroupStyle = new GUIStyle(GUI.skin.GetStyle("toolbarbutton")) { padding = new RectOffset(1, 1, 1, 1), margin = new RectOffset(0, 0, 1, 1) };
            using (new GUILayout.VerticalScope("box"))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(_name.stringValue, "boldlabel");
                    GUILayout.FlexibleSpace();
                }
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();

                    GUILayout.FlexibleSpace();
                    bool dummy;
                    EditorGUI.BeginChangeCheck();
                    dummy = GUILayout.Toggle(_mode.enumValueIndex == (int)ChannelTexture.ColorMode.Red, "R", buttonGroupStyle, GUILayout.Width(16));
                    if (EditorGUI.EndChangeCheck())
                        if (dummy)
                            _mode.enumValueIndex = 0;

                    EditorGUI.BeginChangeCheck();
                    dummy = GUILayout.Toggle(_mode.enumValueIndex == (int)ChannelTexture.ColorMode.Green, "G", buttonGroupStyle, GUILayout.Width(16));
                    if (EditorGUI.EndChangeCheck())
                        if (dummy)
                            _mode.enumValueIndex = 1;

                    EditorGUI.BeginChangeCheck();
                    dummy = GUILayout.Toggle(_mode.enumValueIndex == (int)ChannelTexture.ColorMode.Blue, "B", buttonGroupStyle, GUILayout.Width(16));
                    if (EditorGUI.EndChangeCheck())
                        if (dummy)
                            _mode.enumValueIndex = 2;

                    EditorGUI.BeginChangeCheck();
                    dummy = GUILayout.Toggle(_mode.enumValueIndex == (int)ChannelTexture.ColorMode.Alpha, "A", buttonGroupStyle, GUILayout.Width(16));
                    if (EditorGUI.EndChangeCheck())
                        if (dummy)
                            _mode.enumValueIndex = 3;
                    GUILayout.FlexibleSpace();

                    if (EditorGUI.EndChangeCheck())
                        if (_texture.objectReferenceValue)
                            edited = true;
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    _texture.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField("", _texture.objectReferenceValue, typeof(Texture2D), false, GUILayout.Width(66));
                    GUILayout.FlexibleSpace();
                }

                EditorGUI.BeginChangeCheck();
                _invert.boolValue = GUILayout.Toggle(_invert.boolValue, "Invert", "toolbarbutton");
                if (EditorGUI.EndChangeCheck())
                    if (_texture.objectReferenceValue)
                        edited = true;
            }
            return edited;
        }
    }

    public class TextureAutoPackerProcessor : AssetPostprocessor
    {
        public static List<System.Tuple<string, SerializedProperty>> PathToProperty = new List<System.Tuple<string, SerializedProperty>>();
        public static bool TextureImported = false;
        void OnPostprocessTexture(Texture2D texture)
        {
            TextureImported = true;
        }
        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (TextureImported)
            {
                TextureImported = false;
                if (!TextureAutoPacker.InitiateAutoPacking())
                    OnAutoPackingEnd();
            }
        }
        public static void OnAutoPackingEnd()
        {
            PathToProperty.ForEach(t =>
            {
                t.Item2.serializedObject.Update();
                t.Item2.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Texture2D>(t.Item1);
                t.Item2.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            });
            PathToProperty.Clear();
        }
    }

    public abstract class TAPDreadData<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance = null;
        private static string _SavePath;

        protected static T GetInstance(string SavePath)
        {
            _SavePath = SavePath;
            if (!_instance && Exists())
            {
                _instance = AssetDatabase.LoadAssetAtPath<T>(SavePath);
            }
            if (!_instance)
            {
                _instance = CreateInstance<T>();
                string directoryPath = System.IO.Path.GetDirectoryName(SavePath);
                if (!System.IO.Directory.Exists(directoryPath))
                    System.IO.Directory.CreateDirectory(directoryPath);

                AssetDatabase.CreateAsset(_instance, _SavePath);
            }
            return _instance;
        }

        public static bool Exists()
        {
            return System.IO.File.Exists(_SavePath);
        }

    }

}
