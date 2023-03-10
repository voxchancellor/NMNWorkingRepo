using UnityEngine;
using UnityEngine.Events;

namespace SurvivalTemplatePro
{
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/interaction/interactable/demo-interactables")]
    public class Consumable : Interactable
    {
        public event UnityAction<Consumable> onConsumed;

        [Title("Settings (Consumable)")]

        [SerializeField, Range(-100f, 100f)]
        [Tooltip("The minimum amount of hunger this consumable can restore.")]
        private int m_HungerRestoreMin = 15;

        [SerializeField, Range(-100f, 100f)]
        [Tooltip("The maximum amount of hunger this consumable can restore.")]
        private int m_HungerRestoreMax = 20;

        [SerializeField, Range(-100f, 100f)]
        [Tooltip("The minimum amount of thirst this consumable can restore.")]
        private int m_ThirstRestoreMin = 5;

        [SerializeField, Range(-100f, 100f)]
        [Tooltip("The maximum amount of thirst this consumable can restore.")]
        private int m_ThirstRestoreMax = 10;

        [Space]

        [SerializeField]
        [Tooltip("Audio that will be played after a character consumes this.")]
        private SoundPlayer m_ConsumeAudio;


        public override void OnInteract(ICharacter character)
        {
            base.OnInteract(character);

            bool consumed = false;

            if (character.TryGetModule(out IHungerManager hungerManager) && (hungerManager.MaxHunger - hungerManager.Hunger) > 1f)
            {              
                hungerManager.Hunger += Random.Range(m_HungerRestoreMin, m_HungerRestoreMax);
                consumed = true;
            }

            if (character.TryGetModule(out IThirstManager thirstManager) && (thirstManager.MaxThirst - thirstManager.Thirst) > 1f)
            {
                thirstManager.Thirst += Random.Range(m_ThirstRestoreMin, m_ThirstRestoreMax);
                consumed = true;
            }

            if (consumed)
            {
                m_ConsumeAudio.Play2D();

                gameObject.SetActive(false);
                onConsumed?.Invoke(this);
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            DescriptionText = $"Hunger: +{m_HungerRestoreMin}-{m_HungerRestoreMax}" + "\n" + $"Thirst: +{m_ThirstRestoreMin}-{m_ThirstRestoreMax}";
        }
#endif
    }
}
