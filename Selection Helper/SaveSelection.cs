#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

public class SaveSelection : Editor
{
    //By Dreadrith#3238
    //https://discord.gg/ZsPfrGn

    static Object[] oldSelection;

    [MenuItem("Assets/Selection Helper/Save Selection")]
    [MenuItem("GameObject/Selection Helper/Save Selection", false, 10)]
    static void saveSelected()
    {
        oldSelection = Selection.GetFiltered<Object>(SelectionMode.OnlyUserModifiable);
    }

    [MenuItem("Assets/Selection Helper/Load Selection")]
    [MenuItem("GameObject/Selection Helper/Load Selection", false, 10)]
    static void loadSelected()
    {
        if (oldSelection != null)
            Selection.objects = oldSelection;
    }

    [MenuItem("Assets/Selection Helper/Load Selection (ADD)")]
    [MenuItem("GameObject/Selection Helper/Load Selection (ADD)", false, 10)]
    static void loadSelectedC()
    {
        if (oldSelection != null)
            Selection.objects = Selection.objects.Concat(oldSelection).ToArray();
    }
}
#endif
