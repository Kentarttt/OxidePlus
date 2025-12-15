using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateSurvival
{
	public class SmeltingStation : InteractableObject, IInventoryTrigger
	{
		private const float UPDATE_INTERVAL = 0.1f;

		public Value<bool> IsBurning = new Value<bool>(false);

		public Value<float> Progress = new Value<float>(0f);

		public Message BurnedItem = new Message();

		[SerializeField]
		private SmeltingStationType m_Type;

		[SerializeField]
		[Range(1f, 18f)]
		private int m_LootContainerSize = 6;

		[Header("Fire")]
		[SerializeField]
		private ParticleSystem m_Fire;

		[SerializeField]
		private AudioSource m_FireSource;

		[SerializeField]
		private Firelight m_FireLight;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_FireVolume = 0.6f;

		[Header("Damage (Optional)")]
		[SerializeField]
		private DamageArea m_DamageArea;

		private WaitForSeconds m_UpdateInterval = new WaitForSeconds(0.1f);

		private Coroutine m_BurningHandler;

		private ItemProperty.Value m_BurnTimeProperty;

		private ItemProperty.Value m_FuelTimeProperty;

		private string m_ItemResult;

		private int id = -1;

		public ItemHolder FuelSlot { get; private set; }

		public ItemHolder InputSlot { get; private set; }

		public List<ItemHolder> LootSlots { get; private set; }

		public override void OnInteract(PlayerEventHandler player)
		{
			if (m_Type == SmeltingStationType.Campfire)
			{
				MonoSingleton<InventoryController>.Instance.OpenCampfire.Try(this);
			}
			else if (m_Type == SmeltingStationType.Furnace)
			{
				MonoSingleton<InventoryController>.Instance.OpenFurnace.Try(this);
			}
		}

		private void Start()
		{
			FuelSlot = new ItemHolder();
			InputSlot = new ItemHolder();
			FuelSlot.Updated.AddListener(On_HolderUpdated);
			InputSlot.Updated.AddListener(On_HolderUpdated);
			LootSlots = new List<ItemHolder>();
			for (int i = 0; i < m_LootContainerSize; i++)
			{
				LootSlots.Add(new ItemHolder());
			}
			IsBurning.AddChangeListener(OnChanged_IsBurning);
			IsBurning.SetAndForceUpdate(false);
		}

		private void On_HolderUpdated(ItemHolder holder)
		{
			bool flag = false;
			if (FuelSlot.HasItem && InputSlot.HasItem)
			{
				if (FuelSlot.CurrentItem.HasProperty("Fuel Time") && InputSlot.CurrentItem.HasProperty("Burn Time") && InputSlot.CurrentItem.HasProperty("Burn Result"))
				{
					if (IsBurning.Is(false))
					{
						m_FuelTimeProperty = FuelSlot.CurrentItem.GetPropertyValue("Fuel Time");
						m_BurnTimeProperty = InputSlot.CurrentItem.GetPropertyValue("Burn Time");
						m_ItemResult = InputSlot.CurrentItem.GetPropertyValue("Burn Result").String;
						IsBurning.Set(true);
						m_BurningHandler = StartCoroutine(C_Burn());
						return;
					}
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			if (IsBurning.Is(true) && flag)
			{
				StopBurning();
			}
		}

		private void OnChanged_IsBurning()
		{
			if (IsBurning.Is(true))
			{
				m_Fire.Play(true);
				GameController.Audio.LerpVolumeOverTime(m_FireSource, m_FireVolume, 3f);
				if ((bool)m_DamageArea)
				{
					m_DamageArea.Active = true;
				}
			}
			else
			{
				m_Fire.Stop(true);
				GameController.Audio.LerpVolumeOverTime(m_FireSource, 0f, 3f);
				if ((bool)m_DamageArea)
				{
					m_DamageArea.Active = false;
				}
				Progress.Set(0f);
			}
			m_FireLight.Toggle(IsBurning.Get());
		}

		private IEnumerator C_Burn()
		{
			while (true)
			{
				yield return m_UpdateInterval;
				if (!FuelSlot.CurrentItem || !InputSlot.CurrentItem)
				{
					StopBurning();
					yield break;
				}
				ItemProperty.Float burnTime = m_BurnTimeProperty.Float;
				burnTime.Current -= 0.1f;
				m_BurnTimeProperty.SetValue(ItemProperty.Type.Float, burnTime);
				Progress.Set(1f - burnTime.Ratio);
				if (burnTime.Current <= 0f)
				{
					ItemData itemData;
					if (GameController.ItemDatabase.FindItemByName(m_ItemResult, out itemData))
					{
						CollectionUtils.AddItem(itemData, 1, LootSlots);
					}
					else
					{
						Debug.LogWarning("The item has burned but no result was given, make sure the item has the 'Burn Result' property, so we know what to add as a result of burning / smelting.", this);
					}
					if (InputSlot.CurrentItem.CurrentInStack == 1)
					{
						InputSlot.SetItem(null);
						StopBurning();
						yield break;
					}
					burnTime.Current = burnTime.Default;
					m_BurnTimeProperty.SetValue(ItemProperty.Type.Float, burnTime);
					InputSlot.CurrentItem.CurrentInStack--;
				}
				ItemProperty.Float fuelTime = m_FuelTimeProperty.Float;
				fuelTime.Current -= 0.1f;
				m_FuelTimeProperty.SetValue(ItemProperty.Type.Float, fuelTime);
				if (fuelTime.Current <= 0f)
				{
					if (FuelSlot.CurrentItem.CurrentInStack == 1)
					{
						break;
					}
					fuelTime.Current = fuelTime.Default;
					m_FuelTimeProperty.SetValue(ItemProperty.Type.Float, fuelTime);
					FuelSlot.CurrentItem.CurrentInStack--;
				}
			}
			FuelSlot.SetItem(null);
			StopBurning();
		}

		private void StopBurning()
		{
			m_FuelTimeProperty = null;
			m_BurnTimeProperty = null;
			m_ItemResult = string.Empty;
			IsBurning.Set(false);
			if (m_BurningHandler != null)
			{
				StopCoroutine(m_BurningHandler);
			}
			m_BurningHandler = null;
		}

		public void SaveItems()
		{
			GameController.SceneSave.SaveObjectItems(id);
		}

		public void SetItems(int fuelId, int fuelAmount, int inputId, int inputAmount)
		{
			StartCoroutine(C_SetItems(fuelId, fuelAmount, inputId, inputAmount));
		}

		private IEnumerator C_SetItems(int fuelId, int fuelAmount, int inputId, int inputAmount)
		{
			while (FuelSlot == null || InputSlot == null)
			{
				yield return null;
			}
			ItemDatabase database = MonoSingleton<InventoryController>.Instance.Database;
			ItemData itemData;
			if (database.FindItemById(fuelId, out itemData))
			{
				FuelSlot.SetItem(new SavableItem(itemData, fuelAmount));
			}
			if (database.FindItemById(inputId, out itemData))
			{
				InputSlot.SetItem(new SavableItem(itemData, inputAmount));
			}
		}

		public void SetLootItems(int[] id, int[] amount, int[] durability)
		{
			StartCoroutine(C_SetLootItems(id, amount, durability));
		}

		private IEnumerator C_SetLootItems(int[] id, int[] amount, int[] durability)
		{
			while (LootSlots == null)
			{
				yield return null;
			}
			ItemDatabase database = MonoSingleton<InventoryController>.Instance.Database;
			for (int i = 0; i < LootSlots.Count; i++)
			{
				ItemData itemData;
				if (database.FindItemById(id[i], out itemData))
				{
					LootSlots[i].SetItem(new SavableItem(itemData, amount[i]));
				}
			}
		}

		public void SetID(int _id)
		{
			id = _id;
		}
	}
}
