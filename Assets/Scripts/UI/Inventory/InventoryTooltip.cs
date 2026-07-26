using DeepDigger.Gameplay.Inventories;
using UnityEngine;
using UnityEngine.UI;

namespace DeepDigger.UI
{
    /// <summary>
    /// Small hover panel that describes the item under the cursor (nome, descrição, raridade, peso).
    /// Assumes a Screen Space – Overlay canvas so screen coordinates map directly to position.
    /// </summary>
    public sealed class InventoryTooltip : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text nameText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text detailText;
        [SerializeField] private Vector2 cursorOffset = new(16f, -16f);

        public void Show(ItemStack stack, Vector2 screenPosition)
        {
            if (stack.IsEmpty)
            {
                Hide();
                return;
            }

            if (root != null) root.SetActive(true);
            if (nameText != null) nameText.text = stack.Item.DisplayName;
            if (descriptionText != null) descriptionText.text = stack.Item.Description;
            if (detailText != null)
                detailText.text = $"{stack.Item.Rarity} • Peso {stack.Item.Weight:0.##} • x{stack.Quantity}";

            transform.position = screenPosition + cursorOffset;
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }
    }
}
