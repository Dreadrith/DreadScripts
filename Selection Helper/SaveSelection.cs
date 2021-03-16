#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace DreadScripts
{
    public class SaveSelection
    {
        //By Dreadrith#3238
        //https://discord.gg/ZsPfrGn

        static Object[] oldSelection;

        [MenuItem("Assets/Selection Helper/Save Selection")]
        [MenuItem("GameObject/Selection Helper/Save\\Load/Save Selection", false, 0)]
        static void SaveSelected()
        {
            oldSelection = Selection.objects;
        }

        [MenuItem("Assets/Selection Helper/Load Selection")]
        [MenuItem("GameObject/Selection Helper/Save\\Load/Load Selection", false, 1)]
        static void LoadSelected()
        {
            if (oldSelection != null)
                Selection.objects = Selection.objects.Concat(oldSelection).ToArray();
        }
    }
}
#endif
