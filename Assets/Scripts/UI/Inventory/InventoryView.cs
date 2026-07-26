using DeepDigger.Gameplay.Inventories;
using UnityEngine;
using UnityEngine.UI;

namespace DeepDigger.UI
{
    /// <summary>
    /// Builds the slot grid for an <see cref="InventoryComponent"/> and keeps it in sync with the model,
    /// and coordinates drag-and-drop (a shared ghost icon follows the cursor; dropping calls
    /// <c>MoveOrSwap</c>). All inventory rules live in the model; this class is pure presentation.
    /// Assumes a Screen Space – Overlay canvas.
    /// </summary>
    public sealed class InventoryView : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private InventoryComponent source;

        [Header("Layout")]
        [Tooltip("Container com um GridLayoutGroup para posicionar os slots.")]
        [SerializeField] private RectTransform slotParent;
        [SerializeField] private InventorySlotUI slotPrefab;

        [Header("Drag & Tooltip")]
        [Tooltip("Ícone que segue o cursor durante o arrasto. Deixe 'Raycast Target' desmarcado.")]
        [SerializeField] private Image dragIcon;
        [SerializeField] private InventoryTooltip tooltip;

        private Inventory _inventory;
        private InventorySlotUI[] _slots;
        private int _dragSource = -1;

        private void Awake()
        {
            if (source == null) source = FindFirstObjectByType<InventoryComponent>();
        }

        private void Start()
        {
            if (source == null || slotPrefab == null || slotParent == null)
            {
                Debug.LogError($"{nameof(InventoryView)}: fonte/slotPrefab/slotParent não configurados.", this);
                enabled = false;
                return;
            }

            _inventory = source.Inventory;
            BuildSlots();
            RefreshAll();

            _inventory.SlotChanged += OnSlotChanged;
            _inventory.Changed += RefreshAll;

            if (dragIcon != null) dragIcon.enabled = false;
            tooltip?.Hide();
        }

        private void OnDestroy()
        {
            if (_inventory == null) return;
            _inventory.SlotChanged -= OnSlotChanged;
            _inventory.Changed -= RefreshAll;
        }

        private void BuildSlots()
        {
            _slots = new InventorySlotUI[_inventory.SlotCount];
            for (int i = 0; i < _slots.Length; i++)
            {
                InventorySlotUI slot = Instantiate(slotPrefab, slotParent);
                slot.Bind(this, i);
                _slots[i] = slot;
            }
        }

        private void RefreshAll()
        {
            if (_slots == null) return;
            for (int i = 0; i < _slots.Length; i++)
                _slots[i].Render(_inventory.GetSlot(i));
        }

        private void OnSlotChanged(int index)
        {
            if (_slots != null && index >= 0 && index < _slots.Length)
                _slots[index].Render(_inventory.GetSlot(index));
        }

        // ----- Called by slots ------------------------------------------------------------------

        public void BeginDrag(int index)
        {
            ItemStack stack = _inventory.GetSlot(index);
            if (stack.IsEmpty)
            {
                _dragSource = -1;
                return;
            }

            _dragSource = index;
            if (dragIcon != null)
            {
                dragIcon.sprite = stack.Item.Icon;
                dragIcon.enabled = true;
            }
        }

        public void DragTo(Vector2 screenPosition)
        {
            if (dragIcon != null && dragIcon.enabled)
                dragIcon.rectTransform.position = screenPosition;
        }

        public void EndDrag()
        {
            _dragSource = -1;
            if (dragIcon != null) dragIcon.enabled = false;
        }

        public void DropOnto(int index)
        {
            if (_dragSource >= 0)
                _inventory.MoveOrSwap(_dragSource, index);
        }

        public void ShowTooltip(int index, Vector2 screenPosition)
        {
            ItemStack stack = _inventory.GetSlot(index);
            if (!stack.IsEmpty) tooltip?.Show(stack, screenPosition);
        }

        public void HideTooltip() => tooltip?.Hide();
    }
}
