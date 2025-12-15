using System.Collections;
using UnityEngine;

namespace UltimateSurvival
{
	[RequireComponent(typeof(CharacterController))]
	public class CCDrivenController : PlayerBehaviour
	{
		public enum JumpedFrom
		{
			Ground = 0,
			Ladder = 1
		}

		[Header("General")]
		[SerializeField]
		[Tooltip("How fast the player will change direction / accelerate.")]
		private float m_Acceleration = 5f;

		[SerializeField]
		[Tooltip("How fast the player will stop if no input is given (applies only when grounded).")]
		private float m_Damping = 8f;

		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("How well the player can control direction while in air.")]
		private float m_AirControl = 0.15f;

		[SerializeField]
		private float m_ForwardSpeed = 4f;

		[SerializeField]
		private float m_SidewaysSpeed = 3.5f;

		[SerializeField]
		private float m_BackwardSpeed = 3f;

		[SerializeField]
		[Tooltip("Curve for multiplying speed based on slope.")]
		private AnimationCurve m_SlopeMultiplier = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

		[SerializeField]
		[Tooltip("A small number will make the player bump when descending slopes, a larger one will make it stick to the surface.")]
		private float m_AntiBumpFactor = 1f;

		[Header("Running")]
		[SerializeField]
		[Tooltip("Can the player run?")]
		private bool m_EnableRunning = true;

		[SerializeField]
		[Tooltip("The current movement speed will be multiplied by this value, when sprinting.")]
		private float m_RunSpeedMultiplier = 1.8f;

		[Header("Jumping")]
		[SerializeField]
		[Tooltip("Can the player jump?")]
		private bool m_EnableJumping = true;

		[SerializeField]
		[Tooltip("How high do we jump when pressing jump and letting go immediately.")]
		private float m_JumpHeight = 1f;

		[SerializeField]
		private float m_JumpSpeedFromLadder = 5f;

		[Header("Crouching")]
		[SerializeField]
		[Tooltip("Can the player crouch?")]
		private bool m_EnableCrouching = true;

		[SerializeField]
		[Tooltip("The current movement speed will be multiplied by this value, when moving crouched.")]
		private float m_CrouchSpeedMultiplier = 0.7f;

		[SerializeField]
		[Tooltip("The CharacterController's height when fully-crouched.")]
		private float m_CrouchHeight = 1f;

		[SerializeField]
		[Tooltip("How much time it takes to go in and out of crouch-mode.")]
		private float m_CrouchDuration = 0.3f;

		[Header("Ladder Climbing")]
		[SerializeField]
		[Tooltip("How fast the character moves on ladder.")]
		private float m_SpeedOnLadder = 1f;

		[Header("Sliding")]
		[SerializeField]
		[Tooltip("Will the player slide on steep surfaces?")]
		private bool m_EnableSliding = true;

		[SerializeField]
		private float m_SlideLimit = 32f;

		[Tooltip("How fast does the character slide on steep surfaces?")]
		[SerializeField]
		private float m_SlidingSpeed = 15f;

		[Header("Physics")]
		[SerializeField]
		private float m_PushForce = 60f;

		[SerializeField]
		[Tooltip("How fast we accelerate into falling.")]
		private float m_Gravity = 20f;

		private CharacterController m_Controller;

		private float m_DesiredSpeed;

		private Vector3 m_CurrentVelocity;

		private Vector3 m_SlideVelocity;

		private Vector3 m_DesiredVelocity;

		private Vector3 m_LastSurfaceNormal;

		private CollisionFlags m_LastCollisionFlags;

		private float m_UncrouchedHeight;

		private bool m_PreviouslyGrounded;

		private float m_LastTimeToggledCrouching;

		private JumpedFrom m_JumpedFrom;

		private void Start()
		{
			m_Controller = GetComponent<CharacterController>();
			m_UncrouchedHeight = m_Controller.height;
			base.Player.Jump.AddStartTryer(TryStart_Jump);
			base.Player.Run.AddStartTryer(TryStart_Run);
			base.Player.Crouch.AddStartTryer(TryStart_Crouch);
			base.Player.Crouch.AddStopTryer(TryStop_Crouch);
			base.Player.Sleep.AddStopListener(delegate
			{
				m_CurrentVelocity = Vector3.zero;
			});
		}

		private void Update()
		{
			m_LastCollisionFlags = m_Controller.Move(m_CurrentVelocity * Time.deltaTime);
			if ((m_LastCollisionFlags & CollisionFlags.Below) == CollisionFlags.Below && !m_PreviouslyGrounded)
			{
				if (base.Player.Jump.Active)
				{
					base.Player.Jump.ForceStop();
				}
				base.Player.Land.Send(Mathf.Abs(base.Player.Velocity.Get().y));
			}
			base.Player.IsGrounded.Set(m_Controller.isGrounded);
			base.Player.Velocity.Set(m_Controller.velocity);
			if (base.Player.NearLadders.Count > 0)
			{
				if (base.Player.Walk.Active)
				{
					base.Player.Walk.ForceStop();
				}
				if (base.Player.Run.Active)
				{
					base.Player.Run.ForceStop();
				}
				if (base.Player.Crouch.Active)
				{
					base.Player.Crouch.TryStop();
				}
				if (base.Player.Jump.Active && m_JumpedFrom == JumpedFrom.Ground)
				{
					base.Player.Jump.ForceStop();
				}
				UpdateLadderMovement();
			}
			else if (!m_Controller.isGrounded)
			{
				if (base.Player.Walk.Active)
				{
					base.Player.Walk.ForceStop();
				}
				if (base.Player.Run.Active)
				{
					base.Player.Run.ForceStop();
				}
				UpdateFalling();
			}
			else if (!base.Player.Jump.Active)
			{
				UpdateMovement();
			}
			m_PreviouslyGrounded = m_Controller.isGrounded;
		}

		private void UpdateLadderMovement()
		{
			Vector2 vector = Vector2.ClampMagnitude(base.Player.MovementInput.Get(), 1f);
			if (MonoSingleton<InventoryController>.Instance.IsClosed)
			{
				Transform transform = base.Player.NearLadders.Peek();
				Vector3 vector2 = base.Player.LookDirection.Get();
				bool flag = Vector3.Dot(transform.forward, vector2) < 0f;
				bool flag2 = (vector.y < 0f && !flag) || (vector.y > 0f && flag);
				if (base.Player.IsGrounded.Get() && flag2)
				{
					m_DesiredVelocity = base.transform.forward * vector.y * 3f;
				}
				else
				{
					Vector3 vector3 = transform.right * vector.x * m_SpeedOnLadder / 2f;
					if (flag)
					{
						vector3 *= -1f;
					}
					Vector3 vector4 = vector2 * vector.y * m_SpeedOnLadder;
					Vector3 vector5 = vector3 + vector4;
					Vector3 fromDirection = Vector3.ProjectOnPlane(vector5, transform.right);
					Vector3 toDirection = transform.up * Mathf.Sign(vector5.y);
					vector5 = Quaternion.FromToRotation(fromDirection, toDirection) * vector5;
					if (vector.sqrMagnitude > 0f)
					{
						vector5 += transform.forward;
					}
					m_DesiredVelocity = vector5;
				}
			}
			else
			{
				m_DesiredVelocity = Vector3.zero;
			}
			float num = ((!(m_DesiredVelocity.sqrMagnitude > 0f)) ? m_Damping : m_Acceleration);
			m_CurrentVelocity = Vector3.Lerp(m_CurrentVelocity, m_DesiredVelocity, num * Time.deltaTime);
		}

		private void UpdateMovement()
		{
			CalculateDesiredVelocity();
			Vector3 b = ((!MonoSingleton<InventoryController>.Instance.IsClosed) ? Vector3.zero : m_DesiredVelocity);
			float num = Vector3.Angle(Vector3.up, m_LastSurfaceNormal);
			b *= m_SlopeMultiplier.Evaluate(num / m_Controller.slopeLimit);
			float num2 = ((!(b.sqrMagnitude > 0f)) ? m_Damping : m_Acceleration);
			m_CurrentVelocity = Vector3.Lerp(m_CurrentVelocity, b, num2 * Time.deltaTime);
			if (!base.Player.Walk.Active && b.sqrMagnitude > 0.1f && !base.Player.Run.Active)
			{
				base.Player.Walk.ForceStart();
			}
			else if (base.Player.Walk.Active && (b.sqrMagnitude < 0.1f || base.Player.Run.Active))
			{
				base.Player.Walk.ForceStop();
			}
			if (base.Player.Run.Active)
			{
				if (base.Player.Stamina.Is(0f))
				{
					base.Player.Run.ForceStop();
				}
				else if (base.Player.MovementInput.Get().y < 0f || m_DesiredSpeed == 0f)
				{
					base.Player.Run.ForceStop();
				}
			}
			if (m_EnableSliding)
			{
				if (num > m_SlideLimit)
				{
					Vector3 vector = m_LastSurfaceNormal + Vector3.down;
					m_SlideVelocity += vector * m_SlidingSpeed * Time.deltaTime;
				}
				else
				{
					m_SlideVelocity = Vector3.Lerp(m_SlideVelocity, Vector3.zero, Time.deltaTime * 10f);
				}
				m_CurrentVelocity += m_SlideVelocity;
			}
			if (!base.Player.Jump.Active)
			{
				m_CurrentVelocity.y = 0f - m_AntiBumpFactor;
			}
		}

		private void UpdateFalling()
		{
			if (m_PreviouslyGrounded && !base.Player.Jump.Active)
			{
				m_CurrentVelocity.y = 0f;
			}
			m_CurrentVelocity += m_DesiredVelocity * m_Acceleration * m_AirControl * Time.deltaTime;
			m_CurrentVelocity.y -= m_Gravity * Time.deltaTime;
		}

		private void CalculateDesiredVelocity()
		{
			Vector2 vector = Vector2.ClampMagnitude(base.Player.MovementInput.Get(), 1f);
			bool flag = vector.sqrMagnitude > 0f;
			Vector3 vector2 = ((!flag) ? m_Controller.velocity.normalized : base.transform.TransformDirection(new Vector3(vector.x, 0f, vector.y)));
			m_DesiredSpeed = 0f;
			if (flag)
			{
				m_DesiredSpeed = m_ForwardSpeed * base.Player.MovementSpeedFactor.Get();
				if (Mathf.Abs(vector.x) > 0f)
				{
					m_DesiredSpeed = m_SidewaysSpeed;
				}
				if (vector.y < 0f)
				{
					m_DesiredSpeed = m_BackwardSpeed;
				}
				if (base.Player.Run.Active && (m_DesiredSpeed == m_ForwardSpeed || m_DesiredSpeed == m_SidewaysSpeed))
				{
					m_DesiredSpeed *= m_RunSpeedMultiplier;
				}
				if (base.Player.Crouch.Active)
				{
					m_DesiredSpeed *= m_CrouchSpeedMultiplier;
				}
			}
			m_DesiredVelocity = vector2 * m_DesiredSpeed;
		}

		private bool TryStart_Run()
		{
			if (!m_EnableRunning)
			{
				return false;
			}
			bool flag = base.Player.MovementInput.Get().y > 0f;
			bool isClosed = MonoSingleton<InventoryController>.Instance.IsClosed;
			bool flag2 = base.Player.Stamina.Get() > 0f;
			return base.Player.IsGrounded.Get() && flag2 && flag && isClosed && !base.Player.Crouch.Active && !base.Player.Aim.Active;
		}

		private bool TryStart_Jump()
		{
			if (m_EnableJumping && base.Player.Stamina.Get() > 5f && (base.Player.IsGrounded.Get() || base.Player.NearLadders.Count > 0) && (!base.Player.Crouch.Active || base.Player.Crouch.TryStop()))
			{
				Vector3 vector = Vector3.up;
				if (base.Player.NearLadders.Count > 0)
				{
					vector = -base.Player.NearLadders.Peek().forward;
				}
				m_CurrentVelocity.y = 0f;
				if (base.Player.NearLadders.Count > 0)
				{
					m_CurrentVelocity += vector * m_JumpSpeedFromLadder;
					m_JumpedFrom = JumpedFrom.Ladder;
				}
				else
				{
					m_CurrentVelocity += vector * CalculateJumpSpeed(m_JumpHeight);
					m_JumpedFrom = JumpedFrom.Ground;
				}
				return true;
			}
			return false;
		}

		private float CalculateJumpSpeed(float heightToReach)
		{
			return Mathf.Sqrt(2f * m_Gravity * heightToReach);
		}

		private bool TryStart_Crouch()
		{
			bool flag = m_EnableCrouching && base.Player.NearLadders.Count == 0 && Time.time > m_LastTimeToggledCrouching + m_CrouchDuration && base.Player.IsGrounded.Get() && !base.Player.Run.Active;
			if (flag)
			{
				StartCoroutine(C_SetHeight(m_CrouchHeight));
				m_LastTimeToggledCrouching = Time.time;
			}
			return flag;
		}

		private bool TryStop_Crouch()
		{
			bool flag = Time.time > m_LastTimeToggledCrouching + m_CrouchDuration;
			bool flag2 = CheckForObstacles(true, Mathf.Abs(m_CrouchHeight - m_UncrouchedHeight));
			bool flag3 = flag && !flag2;
			if (flag3)
			{
				StartCoroutine(C_SetHeight(m_UncrouchedHeight));
				m_LastTimeToggledCrouching = Time.time;
			}
			return flag3;
		}

		private IEnumerator C_SetHeight(float targetHeight)
		{
			float speed = Mathf.Abs(targetHeight - m_Controller.height) / m_CrouchDuration;
			while (Mathf.Abs(targetHeight - m_Controller.height) > Mathf.Epsilon)
			{
				m_Controller.height = Mathf.MoveTowards(m_Controller.height, targetHeight, Time.deltaTime * speed);
				m_Controller.center = Vector3.up * m_Controller.height / 2f;
				yield return null;
			}
		}

		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			m_LastSurfaceNormal = hit.normal;
			if ((bool)hit.rigidbody)
			{
				float num = m_PushForce * m_Controller.velocity.magnitude;
				Vector3 force = (hit.moveDirection + Vector3.up * 0.35f) * num;
				hit.rigidbody.AddForceAtPosition(force, hit.point);
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Ladder"))
			{
				base.Player.NearLadders.Enqueue(other.transform);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.CompareTag("Ladder"))
			{
				base.Player.NearLadders.Dequeue();
			}
		}

		private bool CheckForObstacles(bool checkAbove, float maxDistance, out RaycastHit hitInfo)
		{
			Vector3 origin = base.transform.position + ((!checkAbove) ? Vector3.zero : (Vector3.up * m_Controller.height));
			Vector3 direction = ((!checkAbove) ? Vector3.down : Vector3.up);
			return Physics.SphereCast(new Ray(origin, direction), m_Controller.radius, out hitInfo, maxDistance, -1, QueryTriggerInteraction.Ignore);
		}

		private bool CheckForObstacles(bool checkAbove, float maxDistance)
		{
			Vector3 origin = base.transform.position + ((!checkAbove) ? Vector3.zero : (Vector3.up * m_Controller.height));
			Vector3 direction = ((!checkAbove) ? Vector3.down : Vector3.up);
			return Physics.SphereCast(new Ray(origin, direction), m_Controller.radius, maxDistance, -1, QueryTriggerInteraction.Ignore);
		}
	}
}
