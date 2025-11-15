using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ShorNet
{
    public class DragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform PanelToMove;

        private bool _dragging;
        private Vector2 _offset;

        // 🔹 Optional callback so others (like NetUIController) can react when dragging ends
        public Action<Vector2> OnDragFinished;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || PanelToMove == null) return;

            _dragging = true;
            GameData.DraggingUIElement = true;

            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                PanelToMove, eventData.position, null, out local);
            _offset = local;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging || PanelToMove == null) return;

            RectTransform parent = PanelToMove.parent as RectTransform;
            if (parent == null) return;

            Vector2 pointerPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, eventData.position, null, out pointerPos);

            PanelToMove.anchoredPosition = pointerPos - _offset;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _dragging = false;
            GameData.DraggingUIElement = false;

            // 🔹 Notify listeners that dragging is finished, with final anchored position
            if (PanelToMove != null)
            {
                OnDragFinished?.Invoke(PanelToMove.anchoredPosition);
            }
        }
    }
}