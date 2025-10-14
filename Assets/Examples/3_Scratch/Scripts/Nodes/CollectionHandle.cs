using UnityEngine;
using UnityEngine.EventSystems;

public class CollectionHandle : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    Vector2 pointerOffset;

    RectTransform collection;
    private void Start()
    {
        collection = GetComponentInParent<Collection>().PanelRect;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(collection, eventData.position,
                                                                eventData.pressEventCamera, out Vector2 newPos);

        transform.localPosition = newPos - pointerOffset;

        collection.sizeDelta = new Vector2(transform.localPosition.x, -transform.localPosition.y);
        transform.localPosition = collection.sizeDelta * new Vector2(1, -1);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), eventData.position,
                                                                eventData.pressEventCamera, out pointerOffset);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }
}
