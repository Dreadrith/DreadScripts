#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class DS_PersonalID : EditorWindow
{
    private static string ID = SystemInfo.deviceUniqueIdentifier;

    [MenuItem("DreadTools/Personal Identifier")]
    private static void showWindow()
    {
        DS_PersonalID window = GetWindow<DS_PersonalID>(false, "Personal ID", true);
        window.maxSize = new Vector2(350, 80);
        window.minSize = new Vector2(350, 80);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Your ID: ");
        EditorGUILayout.SelectableLabel(ID);
        EditorGUILayout.LabelField("Thank you for your support!");
        EditorGUILayout.LabelField(" ~Dreadrith <3");
    }
}
#endif