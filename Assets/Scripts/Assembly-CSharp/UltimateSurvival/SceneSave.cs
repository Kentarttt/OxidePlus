using Mirror;
using UltimateSurvival.Building;
using UnityEngine;

namespace UltimateSurvival
{
	public class SceneSave : MonoBehaviour
	{
		public Transform buildings;

		public PlayerBuilder playerBuilder;

		public GameObject[] buildingPrefabs;

		private void Start()
		{
			LoadObjectPlacing();
		}

		public void SaveObjectPlacing()
		{
			string text = string.Empty;
			string text2 = string.Empty;
			string text3 = string.Empty;
			string text4 = string.Empty;
			int num = 0;
			if (buildings == null)
			{
				return;
			}
			foreach (Transform building in buildings)
			{
				BuildingPiece component = building.GetComponent<BuildingPiece>();
				if (component != null)
				{
					text = text + num + ",";
					text2 = text2 + component.Name + ",";
					string text5 = text3;
					text3 = text5 + building.position.x + "," + building.position.y + "," + building.position.z + "|";
					text5 = text4;
					text4 = text5 + building.rotation.x + "," + building.rotation.y + "," + building.rotation.z + "," + building.rotation.w + "|";
					if (component.Name.Equals("Anvil"))
					{
						building.GetComponent<Anvil>().SetID(num);
					}
					else if (component.Name.Equals("Campfire") || component.Name.Equals("Furnace"))
					{
						building.GetComponent<SmeltingStation>().SetID(num);
					}
					else if (component.Name.Equals("Storage Box"))
					{
						building.GetComponent<LootObject>().SetID(num);
					}
					num++;
				}
				else
				{
					Debug.Log("Component not found.");
				}
			}
			PlayerPrefs.SetString("OBJID", text);
			PlayerPrefs.SetString("OBJTYPE", text2);
			PlayerPrefs.SetString("OBJPOS", text3);
			PlayerPrefs.SetString("OBJROT", text4);
		}

		public void SaveObjectItems(int id)
		{
			if (id < 0)
			{
				Debug.Log("Save items error!");
				return;
			}
			string empty = string.Empty;
			string value = string.Empty;
			string value2 = string.Empty;
			string text = string.Empty;
			Transform child = buildings.GetChild(id);
			if (child != null)
			{
				BuildingPiece component = child.GetComponent<BuildingPiece>();
				if (component != null)
				{
					if (component.Name.Equals("Anvil"))
					{
						Anvil component2 = child.GetComponent<Anvil>();
						value2 = ((!component2.InputHolder.HasItem) ? "-1,-1" : (component2.InputHolder.CurrentItem.ItemData.Id + "," + ((!component2.InputHolder.CurrentItem.HasProperty("Durability")) ? (-1f) : component2.InputHolder.CurrentItem.GetPropertyValue("Durability").Float.Current)));
						text = ((!component2.ResultHolder.HasItem) ? "-1,-1,-1" : (component2.ResultHolder.CurrentItem.ItemData.Id + "," + component2.ResultHolder.CurrentItem.CurrentInStack + "," + ((!component2.ResultHolder.CurrentItem.HasProperty("Durability")) ? (-1f) : component2.ResultHolder.CurrentItem.GetPropertyValue("Durability").Float.Current)));
					}
					else if (component.Name.Equals("Campfire") || component.Name.Equals("Furnace"))
					{
						SmeltingStation component3 = child.GetComponent<SmeltingStation>();
						value = ((!component3.FuelSlot.HasItem) ? "-1,-1" : (component3.FuelSlot.CurrentItem.ItemData.Id + "," + component3.FuelSlot.CurrentItem.CurrentInStack));
						value2 = ((!component3.InputSlot.HasItem) ? "-1,-1" : (component3.InputSlot.CurrentItem.ItemData.Id + "," + component3.InputSlot.CurrentItem.CurrentInStack));
						foreach (ItemHolder lootSlot in component3.LootSlots)
						{
							if (lootSlot.HasItem)
							{
								string text2 = text;
								text = text2 + lootSlot.CurrentItem.ItemData.Id + "," + lootSlot.CurrentItem.CurrentInStack + "," + ((!lootSlot.CurrentItem.HasProperty("Durability")) ? (-1f) : lootSlot.CurrentItem.GetPropertyValue("Durability").Float.Current) + "|";
							}
							else
							{
								text += "-1,-1,-1|";
							}
						}
					}
					else if (component.Name.Equals("Storage Box"))
					{
						LootObject component4 = child.GetComponent<LootObject>();
						foreach (ItemHolder itemHolder in component4.ItemHolders)
						{
							if (itemHolder.HasItem)
							{
								string text2 = text;
								text = text2 + itemHolder.CurrentItem.ItemData.Id + "," + itemHolder.CurrentItem.CurrentInStack + "," + ((!itemHolder.CurrentItem.HasProperty("Durability")) ? (-1f) : itemHolder.CurrentItem.GetPropertyValue("Durability").Float.Current) + "|";
							}
							else
							{
								text += "-1,-1,-1|";
							}
						}
					}
					else
					{
						Debug.Log("Unknown object type.");
					}
					empty = component.Name;
				}
				else
				{
					Debug.Log("Component not found.");
				}
			}
			else
			{
				Debug.Log("Building not found.");
			}
			PlayerPrefs.SetString("OBJITMTYPE" + id, empty);
			PlayerPrefs.SetString("OBJFUEL" + id, value);
			PlayerPrefs.SetString("OBJINPUT" + id, value2);
			PlayerPrefs.SetString("OBJLOOT" + id, text);
		}

		public void LoadObjectPlacing()
		{
			string @string = PlayerPrefs.GetString("OBJID");
			string string2 = PlayerPrefs.GetString("OBJTYPE");
			string string3 = PlayerPrefs.GetString("OBJPOS");
			string string4 = PlayerPrefs.GetString("OBJROT");
			string[] array = @string.Split(","[0]);
			string[] array2 = string2.Split(","[0]);
			string[] array3 = string3.Split("|"[0]);
			string[] array4 = string4.Split("|"[0]);
			Vector3[] array5 = new Vector3[array.Length];
			Quaternion[] array6 = new Quaternion[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i] != string.Empty))
				{
					continue;
				}
				string[] array7 = array3[i].Split(","[0]);
				if (array7.Length > 2)
				{
					Vector3 zero = Vector3.zero;
					float.TryParse(array7[0], out zero.x);
					float.TryParse(array7[1], out zero.y);
					float.TryParse(array7[2], out zero.z);
					array5[i] = zero;
				}
				string[] array8 = array4[i].Split(","[0]);
				if (array8.Length > 3)
				{
					Quaternion identity = Quaternion.identity;
					float.TryParse(array8[0], out identity.x);
					float.TryParse(array8[1], out identity.y);
					float.TryParse(array8[2], out identity.z);
					float.TryParse(array8[3], out identity.w);
					array6[i] = identity;
				}
				string text = string.Empty;
				if (i < array2.Length && array2.Length > 0)
				{
					text = array2[i];
				}
				GameObject gameObject = null;
				for (int j = 0; j < buildingPrefabs.Length; j++)
				{
					if (text.Equals(buildingPrefabs[j].GetComponent<BuildingPiece>().Name))
					{
						gameObject = buildingPrefabs[j];
						break;
					}
				}
				if (gameObject != null)
				{
					BuildingPiece buildingPiece = playerBuilder.BuildingHelpers.PlacePiece(gameObject, array5[i], array6[i]);
					int result = -1;
					int.TryParse(array[i], out result);
					if (buildingPiece.Name.Equals("Anvil"))
					{
						buildingPiece.gameObject.GetComponent<Anvil>().SetID(result);
						LoadObjectItems(result);
					}
					else if (buildingPiece.Name.Equals("Campfire") || buildingPiece.Name.Equals("Furnace"))
					{
						buildingPiece.gameObject.GetComponent<SmeltingStation>().SetID(result);
						LoadObjectItems(result);
					}
					else if (buildingPiece.Name.Equals("Storage Box"))
					{
						buildingPiece.gameObject.GetComponent<LootObject>().SetID(result);
						LoadObjectItems(result);
					}
				}
				else
				{
					Debug.Log("Building type not found.");
				}
			}
		}

		public void LoadObjectItems(int id)
		{
			if (id < 0)
			{
				Debug.Log("Load items error!");
				return;
			}
			string @string = PlayerPrefs.GetString("OBJITMTYPE" + id);
			string string2 = PlayerPrefs.GetString("OBJFUEL" + id);
			string string3 = PlayerPrefs.GetString("OBJINPUT" + id);
			string string4 = PlayerPrefs.GetString("OBJLOOT" + id);
			Transform child = buildings.GetChild(id);
			if (child != null)
			{
				if (@string.Equals("Anvil"))
				{
					int result = -1;
					int result2 = -1;
					int result3 = -1;
					int result4 = -1;
					string[] array = string3.Split(","[0]);
					int.TryParse(array[0], out result);
					int.TryParse(array[1], out result2);
					string[] array2 = string4.Split(","[0]);
					int.TryParse(array2[0], out result3);
					int.TryParse(array2[1], out result4);
					child.GetComponent<Anvil>().SetItems(result, result2, result3, result4);
					return;
				}
				string[] array3 = string4.Split("|"[0]);
				int[] array4 = new int[array3.Length];
				int[] array5 = new int[array3.Length];
				int[] array6 = new int[array3.Length];
				for (int i = 0; i < array3.Length; i++)
				{
					if (array3[i] != string.Empty)
					{
						int result5 = -1;
						int result6 = -1;
						int result7 = -1;
						string[] array7 = array3[i].Split(","[0]);
						int.TryParse(array7[0], out result5);
						int.TryParse(array7[1], out result6);
						int.TryParse(array7[2], out result7);
						array4[i] = result5;
						array5[i] = result6;
						array6[i] = result7;
					}
				}
				if (@string.Equals("Campfire") || @string.Equals("Furnace"))
				{
					SmeltingStation component = child.GetComponent<SmeltingStation>();
					int result8 = -1;
					int result9 = -1;
					int result10 = -1;
					int result11 = -1;
					string[] array8 = string2.Split(","[0]);
					int.TryParse(array8[0], out result8);
					int.TryParse(array8[1], out result9);
					string[] array9 = string3.Split(","[0]);
					int.TryParse(array9[0], out result10);
					int.TryParse(array9[1], out result11);
					component.SetItems(result8, result9, result10, result11);
					component.SetLootItems(array4, array5, array6);
				}
				else if (@string.Equals("Storage Box"))
				{
					child.GetComponent<LootObject>().SetLootItems(array4, array5, array6);
				}
				else
				{
					Debug.Log("Unknown object type. (" + @string + ")");
				}
			}
			else
			{
				Debug.Log("Building not found.");
			}
		}
	}
}
