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

        // Optional callback so others (like NetUIController) can react when dragging ends
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

            // Desired anchored position before clamping
            Vector2 anchored = pointerPos - _offset;

            // 🔹 Snap-to-screen-edge: clamp the panel so it stays fully on-screen
            Rect parentRect = parent.rect;
            Vector2 size = PanelToMove.sizeDelta;

            // Assume centered pivot (0.5, 0.5) for container; works for typical UI setups
            float halfWidth  = size.x * 0.5f;
            float halfHeight = size.y * 0.5f;

            // If the window is somehow bigger than the parent, bail out on clamping
            if (halfWidth * 2f <= parentRect.width && halfHeight * 2f <= parentRect.height)
            {
                float minX = parentRect.xMin + halfWidth;
                float maxX = parentRect.xMax - halfWidth;
                float minY = parentRect.yMin + halfHeight;
                float maxY = parentRect.yMax - halfHeight;

                anchored.x = Mathf.Clamp(anchored.x, minX, maxX);
                anchored.y = Mathf.Clamp(anchored.y, minY, maxY);
            }

            PanelToMove.anchoredPosition = anchored;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _dragging = false;
            GameData.DraggingUIElement = false;

            if (PanelToMove != null)
            {
                OnDragFinished?.Invoke(PanelToMove.anchoredPosition);
            }
        }
    }
}
