using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICellAnimation : MonoBehaviour
{
    private TextMeshProUGUI text;
    [SerializeField] private RectTransform image;   
    [SerializeField] private float shrinkAnimationDuration = 0.2f;
    [SerializeField] private float growAnimationDuration = 0.2f;

    private void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
    }
    
    public IEnumerator ShrinkAnimation()
    {
        while(image.localScale.x > 0)
        {
            image.localScale -= new Vector3(Time.deltaTime / shrinkAnimationDuration, Time.deltaTime / shrinkAnimationDuration, 0);
            yield return null;
        }
        image.localScale = Vector3.zero;

    }

    public IEnumerator GrowAnimation()
    {
        while (image.localScale.x < 1)
        {
            image.localScale += new Vector3(Time.deltaTime / growAnimationDuration, Time.deltaTime / growAnimationDuration, 0);
            yield return null;
        }
        image.localScale = Vector3.one;
    }

    public void RemoveCellWithoutAnimation() //Instantly removes the cell
    {
        image.localScale = Vector3.zero;
        text.text = "";
    }

    public void PlaceCellWithoutAnimation(string newText) //Instantly places the cell
    {
        image.localScale = Vector3.one;
        text.text = newText;
    }

    public IEnumerator AnimateCellChange(string newText) //Change cell with animation
    {
        yield return StartCoroutine(ShrinkAnimation());
        text.text = newText;
        yield return StartCoroutine(GrowAnimation());
    }

    public bool IsVisible()
    {
        return image.localScale.x > 0 && image.localScale.y > 0;
    }
}
