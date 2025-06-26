#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class LegacyClipFixer : MonoBehaviour
{
    [MenuItem("Tools/Make Extracted Clip Non-Legacy")]
    static void FixClip()
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/animations/heart_anim.anim");
        if (clip == null)
        {
            Debug.LogError("❌ Couldn't find animation clip at path.");
            return;
        }

        // Set it as a non-legacy clip
        SerializedObject serializedClip = new SerializedObject(clip);
        serializedClip.FindProperty("m_Legacy").boolValue = false;
        serializedClip.ApplyModifiedProperties();

        Debug.Log("✅ heart_anim is now non-legacy and ready for Animator Controller!");
    }
}
#endif
