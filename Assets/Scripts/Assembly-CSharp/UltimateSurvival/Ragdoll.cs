using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UltimateSurvival
{
	public class Ragdoll : MonoBehaviour
	{
		[Serializable]
		public class BoneToReparent
		{
			[SerializeField]
			private Transform m_Bone;

			[SerializeField]
			private Transform m_NewParent;

			public void Reparent()
			{
				m_Bone.SetParent(m_NewParent, true);
			}
		}

		[SerializeField]
		private Rigidbody m_Pelvis;

		[SerializeField]
		private string m_NormalLayer = "Hitbox";

		[SerializeField]
		private string m_RagdollLayer = "Default";

		[Header("Helpers")]
		[SerializeField]
		private BoneToReparent[] m_BonesToReparent;

		[SerializeField]
		private Texture m_SurfaceTexture;

		[SerializeField]
		private bool m_AutoAssignHitboxes = true;

		private List<Rigidbody> m_Bones = new List<Rigidbody>();

		public void Enable()
		{
			foreach (Rigidbody bone in m_Bones)
			{
				bone.isKinematic = false;
				bone.gameObject.layer = LayerMask.NameToLayer(m_RagdollLayer);
			}
			BoneToReparent[] bonesToReparent = m_BonesToReparent;
			foreach (BoneToReparent boneToReparent in bonesToReparent)
			{
				boneToReparent.Reparent();
			}
		}

		public void Disable()
		{
			foreach (Rigidbody bone in m_Bones)
			{
				bone.isKinematic = true;
				bone.gameObject.layer = LayerMask.NameToLayer(m_NormalLayer);
			}
		}

		private void Awake()
		{
			m_Bones = (from joint in GetComponentsInChildren<CharacterJoint>()
				select joint.GetComponent<Rigidbody>()).ToList();
			m_Bones.Add(m_Pelvis);
			Disable();
			foreach (Rigidbody bone in m_Bones)
			{
				if (m_AutoAssignHitboxes && bone.gameObject.GetComponent<HitBox>() == null)
				{
					bone.gameObject.AddComponent<HitBox>();
				}
				if ((bool)m_SurfaceTexture && bone.gameObject.GetComponent<SurfaceIdentity>() == null)
				{
					SurfaceIdentity surfaceIdentity = bone.gameObject.AddComponent<SurfaceIdentity>();
					surfaceIdentity.Texture = m_SurfaceTexture;
				}
			}
		}
	}
}
