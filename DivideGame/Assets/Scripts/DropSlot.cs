using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour,
                        IDropHandler
{
    [SerializeField] private bool isKeepCell;

    [Header("Position in grid")]
    [SerializeField] private int rowInGrid;
    [SerializeField] private int colInGrid;

    public void OnDrop(PointerEventData eventData)
    {
        DraggableCell droppedObject = eventData.pointerDrag.GetComponent<DraggableCell>();
        if (droppedObject == null) return;
        
        if (droppedObject.IsKeepCell)
        {
            if (isKeepCell) return;
            Game.Instance.TryPlaceInGrid(true, rowInGrid, colInGrid);
        }
        else
        {
            if (isKeepCell)
            {
                Game.Instance.TryPlaceInKeep();
            }
            else
            {
                Game.Instance.TryPlaceInGrid(false, rowInGrid, colInGrid);
            }
        }
    }
}
