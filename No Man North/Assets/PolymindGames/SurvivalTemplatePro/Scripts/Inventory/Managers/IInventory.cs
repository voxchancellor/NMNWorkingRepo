using SurvivalTemplatePro.InventorySystem;
using System.Collections.Generic;

namespace SurvivalTemplatePro
{
    public interface IInventory : ICharacterModule
    {
        IList<IItemContainer> Containers { get; }

        event ItemSlotChangedCallback onInventoryChanged;

        /// <summary>
        /// Adds a container to the container list.
        /// </summary>
        void AddContainer(IItemContainer itemContainer, bool add);

        /// <summary>
        /// Returns a container with the given name.
        /// </summary>
        IItemContainer GetContainerWithName(string name);

        /// <summary>
        /// Returns a container with the given flag.
        /// </summary>
        IItemContainer GetContainerWithFlags(ItemContainerFlags flags = ItemContainerFlags.Everything);

        /// <summary>
        /// Returns true if a container with the given flag is found.
        /// </summary>
        bool HasContainerWithFlags(ItemContainerFlags flags = ItemContainerFlags.Everything);

        /// <summary>
        /// Adds an item to a container with the corresponding flag.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="flags"></param>
		/// <returns> stack Added Count. </returns>
        int AddItem(IItem item, ItemContainerFlags flags = ItemContainerFlags.Everything);

        /// <summary>
        /// Adds an item with the given name, to a container with the corresponding flag.
        /// </summary>
        int AddItems(string itemName, int amountToAdd, ItemContainerFlags flags = ItemContainerFlags.Everything);

        /// <summary>
        /// Adds an item with the given id, to a container with the corresponding flag.
        /// </summary>
        int AddItems(int itemId, int amountToAdd, ItemContainerFlags flags = ItemContainerFlags.Everything);

        /// <summary>
        /// Adds or swaps an item to a different container with the given flag.
        /// </summary>
        bool AddOrSwap(IItemContainer slotParent, IItemSlot slot, ItemContainerFlags flags = ItemContainerFlags.Everything);

        /// <summary>
        /// Removes an item from a container corresponding with the given flag.
        /// </summary>
        bool RemoveItem(IItem item, ItemContainerFlags flags = ItemContainerFlags.Everything);

        /// <summary>
        /// Removes all the items with the given name from the containers that correspond to the given flag.
        /// </summary>
        int RemoveItems(string itemName, int amountToRemove, ItemContainerFlags flags = ItemContainerFlags.Everything);

        /// <summary>
        /// Removes all the items with the given id from the containers that correspond to the given flag.
        /// </summary>
        int RemoveItems(int itemId, int amountToRemove, ItemContainerFlags flags = ItemContainerFlags.Everything);

        /// <summary>
        /// Counts all the items with the given name, in all containers.
        /// </summary>
        int GetItemCount(string itemName, ItemContainerFlags flags = ItemContainerFlags.Everything);

        /// <summary>
        /// Counts all the items with the given id, in all containers.
        /// </summary>
        int GetItemCount(int itemId, ItemContainerFlags flags = ItemContainerFlags.Everything);

        /// <summary>
        /// Returns true if the item is found in any of the containers with the given flag.
        /// </summary>
        bool ContainsItem(IItem item, ItemContainerFlags flags = ItemContainerFlags.Everything);
    }
}