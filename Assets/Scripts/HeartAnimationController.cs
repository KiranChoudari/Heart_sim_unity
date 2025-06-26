using UnityEngine;
using System.Collections;

public class HeartAnimationController : MonoBehaviour
{
    private Animator animator;
    private AnimationClip clip;
    private float totalFrames;
    private float frameRate;

    // --- Frame ranges for each phase ---
    private const int PQ_START_FRAME = 0;
    private const int PQ_END_FRAME = 14;
    private const int QRS_START_FRAME = 15;
    private const int QRS_END_FRAME = 30;
    private const int ST_START_FRAME = 31;
    private const int ST_END_FRAME = 60;


    void Awake()
    {
        animator = GetComponent<Animator>();
        // Ensure the animator starts in the manual control state and doesn't play automatically
        animator.speed = 0; // Double-check speed is 0
        animator.Play("ManualControlState", 0, 0f);

        // Get info from the animation clip
        clip = animator.runtimeAnimatorController.animationClips[0];
        frameRate = clip.frameRate;
        totalFrames = clip.length * frameRate;
    }

    // This will be called by our main simulation controller
    public void PlayPhase(string phase, float duration, float timeScale)
    {
        int startFrame = 0;
        int endFrame = 0;

        switch (phase)
        {
            case "QRS":
                startFrame = QRS_START_FRAME;
                endFrame = QRS_END_FRAME;
                break;
            case "PQ":
                startFrame = PQ_START_FRAME;
                endFrame = PQ_END_FRAME;
                break;
            case "ST":
                startFrame = ST_START_FRAME;
                endFrame = ST_END_FRAME;
                break;
        }

        // Stop any previous animation coroutine to prevent overlap
        StopAllCoroutines();
        StartCoroutine(AnimateFrames(startFrame, endFrame, duration, timeScale));
    }

    private IEnumerator AnimateFrames(int startFrame, int endFrame, float phaseDuration, float timeScale)
    {
        float startTime = Time.time;
        float elapsedTime = 0f;

        while (elapsedTime < phaseDuration)
        {
            // Calculate how far we are through this phase's duration
            float progress = elapsedTime / phaseDuration;

            // Map this progress to the specified frame range
            float currentFrame = Mathf.Lerp(startFrame, endFrame, progress);

            // Calculate the normalized time (0 to 1) for the entire animation clip
            float normalizedTime = currentFrame / totalFrames;

            // Manually set the animator's playback time
            animator.Play("ManualControlState", 0, normalizedTime);

            // Increment elapsed time, adjusted by the master timeScale from the slider
            elapsedTime += Time.deltaTime * timeScale;
            yield return null; // Wait for the next frame
        }

        // Ensure the animation ends exactly on the end frame
        animator.Play("ManualControlState", 0, endFrame / totalFrames);
    }
}