using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000293 RID: 659
public class VFParticleEffect : VFEffect
{
	// Token: 0x06001D0E RID: 7438 RVA: 0x001EEAE8 File Offset: 0x001ECCE8
	public static VFParticleEffect Create(int effectId, Vector3 position, Quaternion rotation)
	{
		if (VFParticleEffect.pool == null)
		{
			VFParticleEffect.pool = new Dictionary<int, List<VFParticleEffect>>();
		}
		if (!VFParticleEffect.pool.ContainsKey(effectId))
		{
			VFParticleEffect.pool.Add(effectId, new List<VFParticleEffect>());
		}
		List<VFParticleEffect> list = VFParticleEffect.pool[effectId];
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null && !list[i].gameObject.activeSelf)
			{
				list[i].Reset();
				list[i].transform.position = position;
				list[i].transform.rotation = rotation;
				return list[i];
			}
		}
		VFParticleEffect vfparticleEffect = Object.Instantiate<VFParticleEffect>(Configs.builtin.particleEffectPrefabs[effectId], VFEffect.effectGroup);
		vfparticleEffect.Reset();
		vfparticleEffect.transform.position = position;
		vfparticleEffect.transform.rotation = rotation;
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j] == null)
			{
				list[j] = vfparticleEffect;
				return vfparticleEffect;
			}
		}
		list.Add(vfparticleEffect);
		return vfparticleEffect;
	}

	// Token: 0x06001D0F RID: 7439 RVA: 0x001EEC04 File Offset: 0x001ECE04
	public static void CloseAll(int effectId)
	{
		if (VFParticleEffect.pool != null && VFParticleEffect.pool.ContainsKey(effectId))
		{
			List<VFParticleEffect> list = VFParticleEffect.pool[effectId];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != null)
				{
					list[i].Close();
				}
			}
		}
	}

	// Token: 0x06001D10 RID: 7440 RVA: 0x001EEC5D File Offset: 0x001ECE5D
	public override void Open()
	{
		base.gameObject.SetActive(true);
	}

	// Token: 0x06001D11 RID: 7441 RVA: 0x001EEC6B File Offset: 0x001ECE6B
	public override void Close()
	{
		base.gameObject.SetActive(false);
	}

	// Token: 0x06001D12 RID: 7442 RVA: 0x001EEC79 File Offset: 0x001ECE79
	public override void Reset()
	{
		base.Reset();
	}

	// Token: 0x06001D13 RID: 7443 RVA: 0x001EEC81 File Offset: 0x001ECE81
	private void OnDisable()
	{
		this.Reset();
	}

	// Token: 0x0400243D RID: 9277
	private static Dictionary<int, List<VFParticleEffect>> pool;

	// Token: 0x0400243E RID: 9278
	public ParticleSystem psys;
}
