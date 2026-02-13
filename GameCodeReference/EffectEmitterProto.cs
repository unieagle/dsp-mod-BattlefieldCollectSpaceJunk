using System;
using UnityEngine;

// Token: 0x02000330 RID: 816
[Serializable]
public class EffectEmitterProto : Proto
{
	// Token: 0x060022D6 RID: 8918 RVA: 0x00245B90 File Offset: 0x00243D90
	public void Preload()
	{
		this.emitter = Resources.Load<ParticleSystem>(this.PrefabPath);
	}

	// Token: 0x04002A16 RID: 10774
	public string PrefabPath;

	// Token: 0x04002A17 RID: 10775
	[NonSerialized]
	public ParticleSystem emitter;
}
