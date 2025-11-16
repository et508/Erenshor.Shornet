using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ShorNet
{
    public class ResizeHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform PanelToResize;

        private bool _resizing;
        private Vector2 _startSize;
        private Vector2 _startMouseLocal;

        // Minimum size to avoid collapsing the window
        public float MinWidth = 200f;
        public float MinHeight = 100f;

        // 🔹 Callbacks so any controller can react to resize & persist size
        public Action<RectTransform> OnResizing;
        public Action<RectTransform> OnResizeFinished;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || PanelToResize == null)
                return;

            _resizing = true;
            GameData.DraggingUIElement = true;

            _startSize = PanelToResize.sizeDelta;

            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                PanelToResize, eventData.position, null, out local);
            _startMouseLocal = local;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_resizing || PanelToResize == null)
                return;

            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                PanelToResize, eventData.position, null, out local);

            // Assuming bottom-right corner resize handle:
            Vector2 delta   = local - _startMouseLocal;
            Vector2 newSize = _startSize + new Vector2(delta.x, -delta.y);

            if (newSize.x < MinWidth)  newSize.x = MinWidth;
            if (newSize.y < MinHeight) newSize.y = MinHeight;

            PanelToResize.sizeDelta = newSize;

            // Notify listeners while resizing (e.g., to update children or preview)
            OnResizing?.Invoke(PanelToResize);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_resizing)
                return;

            _resizing = false;
            GameData.DraggingUIElement = false;

            if (PanelToResize != null)
            {
                // Final notify – perfect place for saving size via WindowLayoutStore, etc.
                OnResizeFinished?.Invoke(PanelToResize);
            }
        }
    }
}
