using SurvivalTemplatePro.InventorySystem;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SurvivalTemplatePro
{
    /// <summary>
    /// Represents an asset that stores all the user-defined items.
    /// </summary>
    [CreateAssetMenu(menuName = "Survival Template Pro/Inventory/Item Database")]
	public class ItemDatabase : AssetDatabase<ItemDatabase>
	{
		public static string Untagged = "Untagged";
		public static ItemInfo NullItem = new ItemInfo(-100, "Empty");

		[SerializeField]
		private ItemCategory[] m_Categories;

		[SerializeField, Reorderable]
		private ItemPropertyDefinitionList m_ItemProperties;

		[SerializeField, Reorderable]
		private StringList m_ItemTags;

		private List<ItemInfo> m_Items;
		private Dictionary<int, ItemInfo> m_ItemsById;
		private Dictionary<int, InventorySystem.ItemPropertyDefinition> m_PropertiesById;


		#region Initialization
		protected override void GenerateDictionaries()
		{
			if (m_Categories == null || m_ItemProperties == null || m_ItemTags == null)
				return;

			m_Items = new List<ItemInfo>();
			m_ItemsById = new Dictionary<int, ItemInfo>();
            m_PropertiesById = new Dictionary<int, InventorySystem.ItemPropertyDefinition>();

			m_Items.Add(NullItem);

			for (int c = 0; c < m_Categories.Length; c++)
			{
				var category = m_Categories[c];

				for (int i = 0; i < category.Items.Length; i++)
				{
					ItemInfo item = category.Items[i];

					m_Items.Add(item);

					if (!m_ItemsById.ContainsKey(item.Id))
						m_ItemsById.Add(item.Id, item);
				}
			}

			for (int i = 0; i < m_ItemProperties.Length; i++)
			{
				if (!m_PropertiesById.ContainsKey(m_ItemProperties[i].Id))
					m_PropertiesById.Add(m_ItemProperties[i].Id, m_ItemProperties[i]);
			}
		}

#if UNITY_EDITOR
		protected override void RefreshIDs()
        {
			if (m_Categories == null || m_ItemProperties == null || m_ItemTags == null)
				return;

			RefreshItemCategories();
			RefreshItemIDs();
			RefreshPropertyIDs();
		}

		private void RefreshItemIDs()
		{
			List<int> idList = new List<int>();

			foreach (var category in m_Categories)
			{
				foreach (var item in category.Items)
				{
					idList.Add(item.Id);
				}
			}

			int maxAssignmentTries = 50;
			int i = 0;

			foreach (var category in m_Categories)
			{
				foreach (var item in category.Items)
				{
					int assignmentTries = 0;
					int assignedId = idList[i];

					while ((assignedId == 0 || idList.Contains(assignedId) && (idList.IndexOf(assignedId) != i)) && assignmentTries < maxAssignmentTries)
					{
						assignedId = IdGenerator.GenerateIntegerId();
						assignmentTries++;
					}

					if (assignmentTries == maxAssignmentTries)
					{
						Debug.LogError("Couldn't generate an unique id for item: " + item.Name);
						return;
					}
					else
					{
						idList[i] = assignedId;
						AssignIdToItem(item, assignedId);
					}

					i++;
				}
			}
		}

		private void AssignIdToItem(ItemInfo itemInfo, int id)
		{
			Type itemInfoType = typeof(ItemInfo);
			FieldInfo idField = itemInfoType.GetField("m_Id", BindingFlags.NonPublic | BindingFlags.Instance);

			idField.SetValue(itemInfo, id);
		}

		private void RefreshPropertyIDs()
		{
			List<int> idList = new List<int>();

			foreach (var property in m_ItemProperties)
				idList.Add(property.Id);

			int maxAssignmentTries = 50;
			int i = 0;

			foreach (var property in m_ItemProperties)
			{
				int assignmentTries = 0;
				int assignedId = idList[i];

				while ((assignedId == 0 || idList.Contains(assignedId) && (idList.IndexOf(assignedId) != i)) && assignmentTries < maxAssignmentTries)
				{
					assignedId = IdGenerator.GenerateIntegerId();
					assignmentTries++;
				}

				if (assignmentTries == maxAssignmentTries)
				{
					Debug.LogError("Couldn't generate an unique id for property: " + property.Name);
					return;
				}
				else
				{
					idList[i] = assignedId;
					AssignIdToProperty(property, assignedId);
				}

				i++;
			}
		}

		private void AssignIdToProperty(InventorySystem.ItemPropertyDefinition itemProperty, int id)
		{
            Type itemInfoType = typeof(InventorySystem.ItemPropertyDefinition);
			FieldInfo idField = itemInfoType.GetField("Id", BindingFlags.Public | BindingFlags.Instance);

			idField.SetValue(itemProperty, id);
		}
#endif
		#endregion

		#region Category Methods

		public static ItemCategory[] GetAllCategories() => Instance.m_Categories;

		public static ItemCategory GetCategoryByName(string name)
		{
			for (int i = 0; i < Instance.m_Categories.Length; i++)
				if (Instance.m_Categories[i].Name == name)
					return Instance.m_Categories[i];

			return null;
		}

		public static ItemCategory GetRandomCategory()
		{
			return Instance.m_Categories[Random.Range(0, Instance.m_Categories.Length)];
		}

		public static List<string> GetCategoryNames()
		{
			List<string> names = new List<string>();

			for (int i = 0; i < Instance.m_Categories.Length; i++)
				names.Add(Instance.m_Categories[i].Name);

			return names;
		}

		private void RefreshItemCategories()
		{
			if (m_Categories == null || m_Categories.Length == 0)
				return;

			foreach (var category in m_Categories)
			{
				for (int j = 0; j < category.Items.Length; j++)
					category.Items[j].Category = category.Name;
			}
		}

		#endregion

		#region Item Methods

		public static ItemInfo[] GetAllItems() => Instance.m_Items.ToArray();

		public static ItemInfo GetItemAtIndex(int index)
		{
			List<ItemInfo> items = Instance.m_Items;

			if (index == items.Count)
				return NullItem;

			if (items != null && items.Count > 0)
				return items[Mathf.Clamp(index, 0, items.Count - 1)];

			return null;
		}

		public static int GetIndexOfItem(int itemId)
		{
			List<ItemInfo> items = Instance.m_Items;

			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].Id == itemId)
					return i;
			}

			return -1;
		}

		public static int GetItemCount()
		{
			int count = 0;

			for (int c = 0; c < Instance.m_Categories.Length; c++)
				count += Instance.m_Categories[c].Items.Length;

			return count;
		}

		public static bool TryGetItemByName(string name, out ItemInfo itemInfo)
		{
			itemInfo = GetItemByName(name);

			return itemInfo != null;
		}

		public static bool TryGetItemById(int id, out ItemInfo itemInfo)
		{
			itemInfo = GetItemById(id);

			return itemInfo != null;
		}

		public static ItemInfo GetItemByName(string name)
		{
			if (Instance == null)
			{
				Debug.LogError("No item database asset found in the Resources folder!");
				return null;
			}

			foreach (var item in Instance.m_Items)
			{
				if (item.Name == name)
					return item;
			}

			return NullItem;
		}

		public static ItemInfo GetItemById(int id)
		{
			if (Instance == null)
			{
				Debug.LogError("No item database asset found in the Resources folder!");
				return null;
			}

			if (Instance.m_ItemsById.TryGetValue(id, out ItemInfo itemInfo))
				return itemInfo;
			else
				return NullItem;
		}

		public static ItemInfo GetRandomItemFromCategory(string categoryName)
		{
			ItemCategory category = GetCategoryByName(categoryName);

			if (category != null && category.Items.Length > 0)
				return category.Items[Random.Range(0, category.Items.Length)];

			return null;
		}

		public static List<string> GetItemNames()
		{
			List<string> names = new List<string>();

			names.Add(NullItem.Name);

			for (int i = 0; i < Instance.m_Categories.Length; i++)
			{
				var category = Instance.m_Categories[i];

				for (int j = 0; j < category.Items.Length; j++)
					names.Add(category.Items[j].Name);
			}

			return names;
		}

		public static List<string> GetItemNamesFullPath()
		{
			List<string> names = new List<string>();

			names.Add(NullItem.Name);

			for (int i = 0; i < Instance.m_Categories.Length; i++)
			{
				var category = Instance.m_Categories[i];

				for (int j = 0; j < category.Items.Length; j++)
					names.Add(Instance.m_Categories[i].Name + "/" + category.Items[j].Name);
			}

			return names;
		}

		#endregion

		#region Property Methods

		public static InventorySystem.ItemPropertyDefinition[] GetAllProperties() => Instance.m_ItemProperties.ToArray();

		public static InventorySystem.ItemPropertyDefinition GetPropertyAtIndex(int index)
		{
			var properties = Instance.m_ItemProperties;

			if (properties != null && properties.Count > 0)
				return properties[Mathf.Clamp(index, 0, properties.Count - 1)];
			
			return null;
		}

		public static InventorySystem.ItemPropertyDefinition GetPropertyById(int id)
		{
			if (Instance == null)
			{
				Debug.LogError("No item database asset found in the Resources folder!");
				return null;
			}

			if (Instance.m_PropertiesById.TryGetValue(id, out InventorySystem.ItemPropertyDefinition propertyDefinition))
				return propertyDefinition;
			else
				return null;
		}

		public static int GetIndexOfProperty(int propertyId)
		{
			var properties = Instance.m_ItemProperties;

			for (int i = 0; i < properties.Count; i++)
			{
				if (properties[i].Id == propertyId)
					return i;
			}

			return -1;
		}

		public static InventorySystem.ItemPropertyDefinition GetPropertyByName(string name)
		{
			foreach (var property in Instance.m_ItemProperties)
			{
				if (property.Name == name)
					return property;
			}

			return null;
		}

		public static string[] GetPropertyNames()
		{
			string[] names = new string[Instance.m_ItemProperties.Length];

			for (int i = 0; i < Instance.m_ItemProperties.Length; i++)
				names[i] = Instance.m_ItemProperties[i].Name;

			return names;
		}

		#endregion

		#region Tag Methods

		public static List<string> GetAllTags()
		{
			List<string> allTags = new List<string>() { Untagged };
			allTags.AddRange(Instance.m_ItemTags);

			return allTags;
		}

		public static List<ItemInfo> GetAllItemsWithTag(string tag)
		{
			List<ItemInfo> itemsWithTag = new List<ItemInfo>();
			var allItems = Instance.m_Items;

			for (int i = 0; i < allItems.Count; i++)
			{
				if (allItems[i].CompareTag(tag))
					itemsWithTag.Add(allItems[i]);
			}

			return itemsWithTag;
		}

		#endregion

		#region Merging
#if UNITY_EDITOR
		public override void MergeDatabases(ItemDatabase[] databases)
        {
			foreach (var database in databases)
			{
				MergeItemCategoriesAndItems(database);
				MergeItemProperties(database);
				MergeItemTags(database);

                string databasePath = AssetDatabase.GetAssetPath(database);
                AssetDatabase.DeleteAsset(databasePath);

                EditorUtility.SetDirty(this);
			}
		}

		private void MergeItemCategoriesAndItems(ItemDatabase database) 
		{
			List<ItemCategory> categories = new List<ItemCategory>();
			categories.AddRange(m_Categories);

			foreach (var category in database.m_Categories)
			{
				if (ContainsCategory(category))
				{
					foreach (var item in category.Items)
					{
						var mainCategory = GetCategoryWithName(category.Name, categories);
						List<ItemInfo> itemsInCategory = new List<ItemInfo>();
						itemsInCategory.AddRange(mainCategory.Items);

						if (!ContainsItemInCategory(item, mainCategory))
							itemsInCategory.Add(item);

						mainCategory.Items = itemsInCategory.ToArray();
					}
				}
				else
				{
					categories.Add(category.Clone() as ItemCategory);
				}
			}

			m_Categories = categories.ToArray();
		}

		private void MergeItemProperties(ItemDatabase database) 
		{
			foreach (var property in database.m_ItemProperties)
			{
				if (!ContainsProperty(property))
                    m_ItemProperties.Add(property.Clone() as InventorySystem.ItemPropertyDefinition);
			}
		}

		private void MergeItemTags(ItemDatabase database) 
		{
			foreach (var tag in database.m_ItemTags)
			{
				if (!m_ItemTags.Contains(tag))
					m_ItemTags.Add(tag.Clone() as string);
			}
		}

		private bool ContainsProperty(InventorySystem.ItemPropertyDefinition propertyDef) 
		{
            foreach (var property in m_ItemProperties)
            {
				if (property.Name == propertyDef.Name || property.Id == propertyDef.Id)
					return true;
			}

			return false;
		}

		private bool ContainsItemInCategory(ItemInfo itemInfo, ItemCategory category)
		{
			foreach (var item in category.Items)
			{
				if (item.Name == itemInfo.Name || item.Id == itemInfo.Id)
					return true;
			}

			return false;
		}

		private bool ContainsCategory(ItemCategory itemCategory)
		{
			foreach (var category in m_Categories)
			{
				if (category.Name == itemCategory.Name)
					return true;
			}

			return false;
		}

		private ItemCategory GetCategoryWithName(string name, List<ItemCategory> categories) 
		{
            foreach (var category in categories)
            {
				if (category.Name == name)
					return category;
			}

			return null;
		}
#endif
		#endregion
	}
}