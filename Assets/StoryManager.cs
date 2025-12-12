using System.Collections;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public CanvasGroup storyPanel;
    public float fadeTime = 1f;
    public float displayTime = 3f;

    // LevelManager will handle scene loading
    public string sceneName = "Level1";

    public void PlayStory()
    {
        StartCoroutine(ShowStory());
    }

    private IEnumerator ShowStory()
    {
        // Fade in
        yield return Fade(storyPanel, 0f, 1f, fadeTime);

        // Wait while the story is displayed
        yield return new WaitForSeconds(displayTime);

        // Fade out
        yield return Fade(storyPanel, 1f, 0f, fadeTime);

        // Load the actual game scene using your LevelManager
        LevelManager.Instance.LoadScene(sceneName, "CrossFade");
    }

    private IEnumerator Fade(CanvasGroup cg, float from, float to, float time)
    {
        float t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / time);
            yield return null;
        }

        cg.alpha = to;
    }
}
