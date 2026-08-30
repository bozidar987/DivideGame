using System.Collections;
using UnityEngine;

public class PopUpAnimation : MonoBehaviour
{
    [SerializeField] private float popUpDuration = 0.5f;
    [SerializeField] private float popUpScaleFactor = 1.5f;
    [SerializeField] private float popUpShrinkDuration = 0.1f;
    private Vector3 originalScale = Vector3.one;

    private void OnEnable()
    {
        StartCoroutine(AnimateOpenPopUp());
    }

    public void ClosePopUp()
    {
        StartCoroutine(AnimateClosePopUp());
    }

    private IEnumerator AnimateClosePopUp()
    {
        yield return StartCoroutine(ScaleAnimation(originalScale * popUpScaleFactor, popUpShrinkDuration));
        yield return StartCoroutine(ScaleAnimation(Vector3.zero, popUpDuration));
        gameObject.SetActive(false);
    }

    private IEnumerator AnimateOpenPopUp()
    {
        yield return StartCoroutine(ScaleAnimation(originalScale * popUpScaleFactor, popUpDuration));

        yield return StartCoroutine(ScaleAnimation(originalScale, popUpShrinkDuration));
    }

    private IEnumerator ScaleAnimation(Vector3 targetScale, float duration)
    {
        float elapsedTime = 0f;
        Vector3 initialScale = transform.localScale;
        while (elapsedTime<duration)
        {
            float t = elapsedTime / duration;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
    }
}
