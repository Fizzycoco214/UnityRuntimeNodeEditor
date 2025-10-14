using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RuntimeNodeEditor
{
	public class GraphPointerListener : MonoBehaviour, IPointerClickHandler, IDragHandler, IScrollHandler, IPointerDownHandler, IPointerUpHandler
    {
        private SignalSystem    _signalSystem;

        public void Init(SignalSystem signalSystem)
        {
            _signalSystem = signalSystem;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _signalSystem.InvokeGraphPointerDown(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _signalSystem.InvokeGraphPointerUp(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _signalSystem.InvokeGraphPointerClick(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _signalSystem.InvokeGraphPointerDrag(eventData);
        }

        public void OnScroll(PointerEventData eventData)
        {
            _signalSystem.InvokeGraphPointerScroll(eventData);
        }
    }
}