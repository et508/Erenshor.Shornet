using UnityEngine;
using UnityEngine.EventSystems;

namespace ShorNet
{
    public class ResizeHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform PanelToResize;

        private bool _resizing;
        private Vector2 _initialSize;
        private Vector2 _initialMouse;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || PanelToResize == null)
                return;

            _resizing = true;
            _initialSize = PanelToResize.sizeDelta;
            _initialMouse = eventData.position;
            GameData.DraggingUIElement = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_resizing || PanelToResize == null)
                return;

            var delta = eventData.position - _initialMouse;

            // Grow to the right and down; invert Y to match UI coords
            var newSize = _initialSize + new Vector2(delta.x, -delta.y);
            newSize.x = Mathf.Max(300f, newSize.x);
            newSize.y = Mathf.Max(150f, newSize.y);

            PanelToResize.sizeDelta = newSize;

            // Let NetUIController recompute child layout based on new container size
            NetUIController.OnPanelResized(PanelToResize);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _resizing = false;
            GameData.DraggingUIElement = false;
        }
    }
}