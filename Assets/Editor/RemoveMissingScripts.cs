using UnityEngine;
using UnityEditor;

public class RemoveMissingScripts : MonoBehaviour
{
    [MenuItem("Tools/Cleanup/🧨Удалить ВСЕ Missing Scripts из выбранных префабов")]
    static void RemoveMissingScriptsDeeply()
    {
        var selected = Selection.assetGUIDs;

        if (selected.Length == 0)
        {
            EditorUtility.DisplayDialog("Удаление Missing Scripts", "Пожалуйста, выбери один или несколько префабов в Project окне.", "Ок");
            return;
        }

        int totalRemoved = 0;

        foreach (var guid in selected)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);

            if (prefabRoot == null)
            {
                Debug.LogWarning($"❌Не удалось загрузить префаб: {path}");
                continue;
            }

            int removed = RemoveMissingRecursive(prefabRoot);
            if (removed > 0)
            {
                totalRemoved += removed;
                Debug.Log($"🧹 Удалено {removed} missing script(ов) из: {path}");
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Готово", $"Удалено всего missing script компонентов: {totalRemoved}", "ОК");
    }

    static int RemoveMissingRecursive(GameObject obj)
    {
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);

        foreach (Transform child in obj.transform)
        {
            removed += RemoveMissingRecursive(child.gameObject);
        }

        return removed;
    }
}