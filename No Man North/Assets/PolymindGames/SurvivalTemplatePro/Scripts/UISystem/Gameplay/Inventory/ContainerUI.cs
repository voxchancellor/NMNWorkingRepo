using UnityEngine;

namespace SurvivalTemplatePro.UISystem
{
    public abstract class ContainerUI<T> : PlayerUIBehaviour where T: SlotUI
	{
		public T[] SlotInterfaces => m_SlotInterfaces;

		[SerializeField]
		protected SlotUI m_SlotTemplate;

		[SerializeField]
		private RectTransform m_SlotsParent;

		[SerializeField]
		private int m_DefaultSlotCount = 1;

		protected T[] m_SlotInterfaces;
		protected T m_Selected;


		public virtual bool GenerateSlots() => GenerateSlots(m_DefaultSlotCount);

		public virtual bool GenerateSlots(int count)
		{
			if (m_SlotTemplate == null)
			{
				Debug.LogError("No slot template is provided, can't generate any slots.", gameObject);
				return false;
			}

			if (m_SlotsParent == null)
				Debug.LogWarning("The slots parent is not assigned. Will parent them under this object.", gameObject);

			var parent = m_SlotsParent == null ? transform : m_SlotsParent;
			int childCount = parent.childCount;

			// Destroy the old slots
			for (int i = 0;i < childCount;i ++)
			{
				var child = parent.GetChild(parent.childCount - 1);

				if (child != m_SlotTemplate.transform && child.GetComponent<T>())
					DestroyImmediate(child.gameObject);
			}

			// Make sure the slot template is active, so we don't spawn disabled slots
			bool slotTemplateActive = m_SlotTemplate.gameObject.activeSelf;
			m_SlotTemplate.gameObject.SetActive(true);

			// Create the new slots
			m_SlotInterfaces = new T[count]; 

			for (int i = 0;i < count;i ++)
				m_SlotInterfaces[i] = Instantiate(m_SlotTemplate, parent) as T;

			m_SlotTemplate.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

			return true;
		}

		public void Select(int index)
		{
			if (index >= m_SlotInterfaces.Length || (m_Selected != null && m_SlotInterfaces[index] == m_Selected))
				return;

			if (m_Selected != null)
				m_Selected.Deselect();

			m_Selected = m_SlotInterfaces[index];
			m_Selected.Select();
		}

		public void Select(SlotUI slot)
		{
			if ((m_Selected != null && slot == m_Selected))
				return;

			for (int i = 0; i < m_SlotInterfaces.Length; i++)
			{
				if (m_SlotInterfaces[i] == slot)
				{
					Select(i);
					return;
				}
			}
		}

		public void DeselectAll()
		{
			for (int i = 0;i < m_SlotInterfaces.Length;i++)
				m_SlotInterfaces[i].Deselect();

			m_Selected = null;
		}
	}
}