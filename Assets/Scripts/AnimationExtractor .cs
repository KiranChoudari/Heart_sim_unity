#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class AnimationExtractor : MonoBehaviour
{
    [MenuItem("Tools/Extract Animation From GLB")]
    static void Extract()
    {
        var source = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/models/human_heart_3d_model.glb");

        if (source == null)
        {
            Debug.LogError("❌ Animation clip not found in GLB. Make sure the path is correct.");
            return;
        }

        // Create a copy
        AnimationClip newClip = new AnimationClip();
        EditorUtility.CopySerialized(source, newClip);

        // Save to disk as a new .anim file
        AssetDatabase.CreateAsset(newClip, "Assets/animations/heart_anim.anim");
        AssetDatabase.SaveAssets();

        Debug.Log("✅ Animation extracted to Assets/animations/heart_anim.anim");
    }
}
#endif
