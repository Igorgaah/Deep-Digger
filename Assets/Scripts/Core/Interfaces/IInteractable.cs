using UnityEngine;

namespace DeepDigger.Core.Interfaces
{
    /// <summary>
    /// Anything the player can interact with: NPCs, chests, altars, elevators, shop stalls.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Short prompt shown to the player (e.g. "Abrir", "Falar").</summary>
        string InteractionPrompt { get; }

        /// <summary>Triggered when the player confirms interaction.</summary>
        void Interact(GameObject interactor);
    }
}
