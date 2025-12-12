using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class MaterialAutomator : EditorWindow
{
    [MenuItem("Tools/Convert Materials to URP Lit")]
    public static void ShowWindow()
    {
        GetWindow<MaterialAutomator>("Material Converter");
    }

    void OnGUI()
    {
        GUILayout.Label("Automatisation URP", EditorStyles.boldLabel);
        GUILayout.Label("Ce script va :\n1. Trouver tous les matériaux.\n2. Les passer en 'Universal Render Pipeline/Lit'.\n3. Chercher une texture dans le même dossier.\n4. Assigner la texture au slot 'Base Map'.", EditorStyles.helpBox);

        if (GUILayout.Button("Lancer la conversion"))
        {
            ConvertMaterials();
        }
    }

    static void ConvertMaterials()
    {
        string[] matGuids = AssetDatabase.FindAssets("t:Material");

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");

        if (urpLit == null)
        {
            Debug.LogError("Shader URP Lit introuvable ! Vérifie que le package URP est bien installé.");
            return;
        }

        int count = 0;

        foreach (string guid in matGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat == null) continue;

            mat.shader = urpLit;

            string folderPath = Path.GetDirectoryName(path);
            Texture2D foundTexture = FindTextureInFolder(folderPath, mat.name);

            if (foundTexture != null)
            {
                mat.SetTexture("_BaseMap", foundTexture);
                Debug.Log($"[Succès] Matériau '{mat.name}' mis à jour avec la texture '{foundTexture.name}'");
            }
            else
            {
                Debug.LogWarning($"[Partiel] Matériau '{mat.name}' converti, mais aucune texture correspondante trouvée dans {folderPath}");
            }

            EditorUtility.SetDirty(mat);
            count++;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Terminé", $"{count} matériaux ont été traités.", "Ok");
    }

    static Texture2D FindTextureInFolder(string folderPath, string materialName)
    {
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

        foreach (string guid in textureGuids)
        {
            string texPath = AssetDatabase.GUIDToAssetPath(guid);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

            if (tex == null) continue;

            if (tex.name.Contains(materialName) || materialName.Contains(tex.name))
            {
                return tex;
            }
        }

        return null;
    }
}