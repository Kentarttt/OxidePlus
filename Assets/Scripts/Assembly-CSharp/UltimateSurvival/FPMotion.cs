using UnityEngine;

namespace UltimateSurvival
{
	[RequireComponent(typeof(FPObject))]
	public class FPMotion : PlayerBehaviour
	{
		[Header("Setup")]
		[SerializeField]
		private Transform m_Model;

		[SerializeField]
		private Transform m_Pivot;

		[Header("Sway")]
		[SerializeField]
		private Sway m_MovementSway;

		[SerializeField]
		private Sway m_RotationSway;

		[Header("Bob")]
		[SerializeField]
		private TrigonometricBob m_WalkBob;

		[SerializeField]
		private TrigonometricBob m_AimBob;

		[SerializeField]
		private TrigonometricBob m_RunBob;

		[SerializeField]
		private LerpControlledBob m_LandBob;

		[SerializeField]
		private float m_MaxLandSpeed = 12f;

		[Header("Offset")]
		[SerializeField]
		private TransformOffset m_IdleOffset;

		[SerializeField]
		private TransformOffset m_RunOffset;

		[SerializeField]
		private TransformOffset m_AimOffset;

		[SerializeField]
		private TransformOffset m_OnLadderOffset;

		[SerializeField]
		private TransformOffset m_JumpOffset;

		[SerializeField]
		[Tooltip("The object position and rotation offset, when the character is too close to an object. NOTE: Will not be taken into consideration if the object can be used when near other objects (see the 'CanUseWhileNearObjects' setting).")]
		private TransformOffset m_TooCloseOffset;

		private Transform m_Root;

		private FPObject m_Object;

		private FPWeaponBase m_Weapon;

		private TransformOffset m_CurrentOffset;

		private bool m_HolsterActive;

		private void Awake()
		{
			m_Object = GetComponent<FPObject>();
			m_Weapon = m_Object as FPWeaponBase;
			m_Object.Draw.AddListener(On_Draw);
			m_Object.Holster.AddListener(On_Holster);
			SetupTransforms();
			base.Player.Land.AddListener(On_Land);
			m_CurrentOffset = m_IdleOffset;
		}

		private void On_Draw()
		{
			m_IdleOffset.Reset();
			m_CurrentOffset = m_IdleOffset;
			m_HolsterActive = false;
		}

		private void On_Holster()
		{
			m_HolsterActive = true;
		}

		private void On_Land(float landSpeed)
		{
			if (m_Object.IsEnabled && base.gameObject.activeInHierarchy)
			{
				StartCoroutine(m_LandBob.DoBobCycle(landSpeed / m_MaxLandSpeed));
			}
		}

		private void SetupTransforms()
		{
			Transform transform = new GameObject("Root").transform;
			transform.transform.SetParent(base.transform);
			transform.position = m_Pivot.position;
			transform.rotation = m_Pivot.rotation;
			m_Pivot.SetParent(transform, true);
			m_Model.SetParent(m_Pivot, true);
		}

		private void Update()
		{
			Vector2 vector = ((!MonoSingleton<InventoryController>.Instance.IsClosed) ? Vector2.zero : base.Player.LookInput.Get());
			m_MovementSway.CalculateSway(-vector, Time.deltaTime);
			m_RotationSway.CalculateSway(new Vector2(vector.y, 0f - vector.x), Time.deltaTime);
			m_Pivot.localPosition = m_MovementSway.Value;
			m_Pivot.localRotation = Quaternion.Euler(m_RotationSway.Value);
			float magnitude = base.Player.Velocity.Get().magnitude;
			Vector3 zero = Vector3.zero;
			if (base.Player.Aim.Active && magnitude > 1f)
			{
				zero += m_AimBob.CalculateBob(magnitude, Time.deltaTime);
			}
			else
			{
				zero += m_AimBob.Cooldown(Time.deltaTime);
			}
			if (base.Player.Walk.Active && !base.Player.Aim.Active)
			{
				zero += m_WalkBob.CalculateBob(magnitude, Time.deltaTime);
			}
			else
			{
				zero += m_WalkBob.Cooldown(Time.deltaTime);
			}
			if (base.Player.Run.Active)
			{
				zero += m_RunBob.CalculateBob(magnitude, Time.deltaTime);
			}
			else
			{
				zero += m_RunBob.Cooldown(Time.deltaTime);
			}
			m_Pivot.localPosition += zero;
			m_Pivot.localPosition += Vector3.up * m_LandBob.Value;
			bool flag = (bool)m_Weapon && !m_Weapon.UseWhileNearObjects && base.Player.IsCloseToAnObject.Get() && base.Player.RaycastData.Get().GameObject.layer != LayerMask.NameToLayer("Hitbox") && !base.Player.RaycastData.Get().GameObject.CompareTag("Ladder");
			if (m_HolsterActive)
			{
				TryChangeOffset(m_IdleOffset);
			}
			else if (base.Player.NearLadders.Count > 0)
			{
				TryChangeOffset(m_OnLadderOffset);
			}
			else if (flag)
			{
				TryChangeOffset(m_TooCloseOffset);
			}
			else if (base.Player.Run.Active)
			{
				TryChangeOffset(m_RunOffset);
			}
			else if (base.Player.Aim.Active)
			{
				TryChangeOffset(m_AimOffset);
			}
			else if (!base.Player.IsGrounded.Get())
			{
				TryChangeOffset(m_JumpOffset);
			}
			else
			{
				TryChangeOffset(m_IdleOffset);
			}
			Vector3 position;
			Quaternion rotation;
			m_CurrentOffset.Update(Time.deltaTime, out position, out rotation);
			m_Pivot.localPosition += position;
			m_Pivot.localRotation *= rotation;
		}

		private void TryChangeOffset(TransformOffset newOffset)
		{
			if (m_CurrentOffset != newOffset)
			{
				newOffset.ContinueFrom(m_CurrentOffset);
				m_CurrentOffset = newOffset;
			}
		}
	}
}
