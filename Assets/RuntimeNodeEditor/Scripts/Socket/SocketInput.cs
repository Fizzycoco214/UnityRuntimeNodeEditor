using UnityEngine;
using UnityEngine.EventSystems;

namespace RuntimeNodeEditor
{
    public class SocketInput : Socket, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            Signal.InvokeInputSocketClick(this, eventData);
        }
    }
}