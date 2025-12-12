using System.Collections;
using UnityEngine;

public class CrossFade : SceneTransition
{
    public CanvasGroup crossFade;
    public float duration = 1f;

    public override IEnumerator AnimateTransitionIn()
    {
        // ´Ó alpha 0 ¡ú 1
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            crossFade.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }

        crossFade.alpha = 1f; 
    }

    public override IEnumerator AnimateTransitionOut()
    {
        // ´Ó alpha 1 ¡ú 0
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            crossFade.alpha = Mathf.Lerp(1f, 0f, t / duration);
            yield return null;
        }

        crossFade.alpha = 0f;
    }
}
