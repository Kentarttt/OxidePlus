namespace UltimateSurvival
{
	public class ET
	{
		public enum ActionRepeatType
		{
			Single = 0,
			Repetitive = 1
		}

		public enum PointOrder
		{
			Sequenced = 0,
			Random = 1
		}

		public enum AIMovementState
		{
			Idle = 0,
			Walking = 1,
			Running = 2
		}

		public enum BuildableType
		{
			Foundation = 0,
			Wall = 1,
			Floor = 2
		}

		public enum MaterialType
		{
			Wood = 0,
			Stone = 1,
			Metal = 2
		}

		public enum InputType
		{
			Standalone = 0,
			Mobile = 1
		}

		public enum InputMode
		{
			Buttons = 0,
			Axes = 1
		}

		public enum StandaloneAxisType
		{
			Unity = 0,
			Custom = 1
		}

		public enum MobileAxisType
		{
			Custom = 0
		}

		public enum ButtonState
		{
			Down = 0,
			Up = 1
		}

		public enum CharacterType
		{
			Player = 0
		}

		public enum FireMode
		{
			SemiAuto = 0,
			Burst = 1,
			FullAuto = 2
		}

		public enum FileCreatorMode
		{
			ScriptableObject = 0,
			ScriptFile = 1,
			Both = 2
		}

		public enum TimeOfDay
		{
			Day = 0,
			Night = 1
		}

		public enum InventoryState
		{
			Closed = 0,
			Normal = 1,
			Loot = 2,
			Furnace = 3,
			Anvil = 4,
			Campfire = 6
		}
	}
}
