using DeepDigger.Gameplay.Inventories;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DeepDigger.UI
{
    /// <summary>
    /// A single inventory slot widget: shows the item icon and quantity, and forwards drag/drop and
    /// hover events to its <see cref="InventoryView"/>. Holds no inventory logic — it is a thin view.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class InventorySlotUI : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text quantityText;

        private InventoryView _view;

        public int Index { get; private set; }

        public void Bind(InventoryView view, int index)
        {
            _view = view;
            Index = index;
        }

        /// <summary>Updates the visuals to match a stack (empty stack clears the slot).</summary>
        public void Render(ItemStack stack)
        {
            if (stack.IsEmpty)
            {
                if (icon != null) { icon.enabled = false; icon.sprite = null; }
                if (quantityText != null) quantityText.text = string.Empty;
                return;
            }

            if (icon != null)
            {
                icon.enabled = true;
                icon.sprite = stack.Item.Icon;
            }
            if (quantityText != null)
                quantityText.text = stack.Quantity > 1 ? stack.Quantity.ToString() : string.Empty;
        }

        public void OnBeginDrag(PointerEventData eventData) => _view.BeginDrag(Index);
        public void OnDrag(PointerEventData eventData) => _view.DragTo(eventData.position);
        public void OnEndDrag(PointerEventData eventData) => _view.EndDrag();
        public void OnDrop(PointerEventData eventData) => _view.DropOnto(Index);
        public void OnPointerEnter(PointerEventData eventData) => _view.ShowTooltip(Index, eventData.position);
        public void OnPointerExit(PointerEventData eventData) => _view.HideTooltip();
    }
}
