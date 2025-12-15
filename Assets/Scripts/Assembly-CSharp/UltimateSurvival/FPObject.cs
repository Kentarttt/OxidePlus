using UnityEngine;

namespace UltimateSurvival
{
	public class FPObject : PlayerBehaviour
	{
		public Message Draw = new Message();

		public Message Holster = new Message();

		[Header("General")]
		[SerializeField]
		private string m_ObjectName = "Unnamed";

		[SerializeField]
		[Range(15f, 100f)]
		private int m_TargetFOV = 45;

		protected ItemProperty.Value m_Durability;

		public bool IsEnabled { get; private set; }

		public string ObjectName
		{
			get
			{
				return m_ObjectName;
			}
		}

		public SavableItem CorrespondingItem { get; private set; }

		public int TargetFOV
		{
			get
			{
				return m_TargetFOV;
			}
		}

		public float LastDrawTime { get; private set; }

		public virtual void On_Draw(SavableItem correspondingItem)
		{
			IsEnabled = true;
			CorrespondingItem = correspondingItem;
			LastDrawTime = Time.time;
			m_Durability = correspondingItem.GetPropertyValue("Durability");
			Draw.Send();
		}

		public virtual void On_Holster()
		{
			IsEnabled = false;
			CorrespondingItem = null;
			Holster.Send();
		}
	}
}
