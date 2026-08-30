using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableCell : MonoBehaviour,
                            IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private bool isKeepCell; // Flag to indicate if this cell is a keep cell
    public bool IsKeepCell => isKeepCell;
    [SerializeField] private Transform backgroundImage;
    private RectTransform rectTransform;
    private Vector3 startingPosition;
    private Image cellImage;
    private Canvas canvas;            
    private Transform originalParent;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        cellImage = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
        originalParent = transform.parent;
        startingPosition = rectTransform.localPosition;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetParent(canvas.transform);       //Free from parent hierarchy constraints for unrestricted movement
        cellImage.raycastTarget = false;
        backgroundImage.SetParent(canvas.transform); //Separates bakcground, so it stays in original place while cell is being dragged
        backgroundImage.SetAsFirstSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position, canvas.worldCamera, out Vector2 localPoint);
        rectTransform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        cellImage.raycastTarget = true;
        transform.SetParent(originalParent);
        rectTransform.localPosition = startingPosition;
        backgroundImage.SetParent(transform);
        backgroundImage.SetAsFirstSibling();     //Makes sure it gets rendered first so it stays under cellImage
    }
}
