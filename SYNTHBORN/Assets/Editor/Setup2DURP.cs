// ONE-TIME SETUP SCRIPT — Creates 2D URP pipeline assets on first Editor open
// Delete this file after running: Assets > SYNTHBORN > Setup 2D URP
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Setup2DURP
{
    [MenuItem("Assets/SYNTHBORN/Setup 2D URP (Run Once)")]
    public static void Setup()
    {
        // Create 2D Renderer Data
        var renderer2D = ScriptableObject.CreateInstance<Renderer2DData>();
        AssetDatabase.CreateAsset(renderer2D, "Assets/Settings/Renderer2D.asset");

        // Create URP Pipeline Asset with 2D Renderer
        var pipelineAsset = UniversalRenderPipelineAsset.Create(renderer2D);
        AssetDatabase.CreateAsset(pipelineAsset, "Assets/Settings/URP-2D-PipelineAsset.asset");

        // Assign to Graphics Settings
        UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline = pipelineAsset;

        // Assign to Quality Settings (all levels)
        var qualityLevels = QualitySettings.names;
        for (int i = 0; i < qualityLevels.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = pipelineAsset;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("SYNTHBORN: 2D URP Pipeline configured! You can delete Assets/Editor/Setup2DURP.cs now.");
        EditorUtility.DisplayDialog("SYNTHBORN Setup",
            "2D URP Pipeline created and assigned!\n\nAssets/Settings/URP-2D-PipelineAsset.asset\nAssets/Settings/Renderer2D.asset\n\nYou can delete Assets/Editor/Setup2DURP.cs now.",
            "OK");
    }
}
#endif
