using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ShorNet
{
    public class DragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform PanelToMove;
        
        public RectTransform SnapSizeRect;

        private bool _dragging;
        private Vector2 _offset;
        
        public Action<Vector2> OnDragFinished;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || PanelToMove == null)
                return;

            _dragging = true;
            GameData.DraggingUIElement = true;

            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                PanelToMove, eventData.position, null, out local);
            _offset = local;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging || PanelToMove == null)
                return;

            RectTransform parent = PanelToMove.parent as RectTransform;
            if (parent == null)
                return;

            Vector2 pointerPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, eventData.position, null, out pointerPos);
            
            Vector2 anchored = pointerPos - _offset;
            
            Rect parentRect = parent.rect;
            
            RectTransform sizeRect = SnapSizeRect != null ? SnapSizeRect : PanelToMove;
            Vector2 size = sizeRect.sizeDelta;

            float halfWidth  = size.x * 0.5f;
            float halfHeight = size.y * 0.5f;

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
