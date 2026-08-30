using System.Collections;
using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeTime;

    private void Start()
    {
        StartCoroutine(Fade(0));
    }

    private IEnumerator Fade(float targetValue)
    {
        float elapsedTime = 0f;
        float initialValue = fadeImage.color.a;

        while (elapsedTime < fadeTime)
        {
            float t = elapsedTime / fadeTime;
            fadeImage.color = new Vector4(fadeImage.color.r,fadeImage.color.g,fadeImage.color.b, Mathf.Lerp(initialValue, targetValue, t));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = new Vector4(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, targetValue);
    }

    private IEnumerator FadeInAndChangeScene(string nextScene)
    {
        yield return StartCoroutine(Fade(1));
        SceneManager.LoadScene(nextScene);
    }

    public void ChangeScene(string nextScene)
    {
        StartCoroutine(FadeInAndChangeScene(nextScene));
    }
}
