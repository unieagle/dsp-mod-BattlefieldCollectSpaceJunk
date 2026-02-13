using System;
using UnityEngine;

// Token: 0x02000290 RID: 656
public class TestParticleSystem : MonoBehaviour
{
	// Token: 0x06001CFE RID: 7422 RVA: 0x001EE7D6 File Offset: 0x001EC9D6
	private void Start()
	{
	}

	// Token: 0x06001CFF RID: 7423 RVA: 0x001EE7D8 File Offset: 0x001EC9D8
	private void Update()
	{
		this.psys.time = 0f;
		this.psys.randomSeed = 1U;
		this.psys.useAutoRandomSeed = false;
		if (this.time < this.psys.main.duration)
		{
			this.psys.Simulate(this.time, true, true, true);
			return;
		}
		this.psys.Simulate(this.psys.main.duration, true, true, true);
		this.psys.Simulate(this.time - this.psys.main.duration, true, false, true);
	}

	// Token: 0x04002433 RID: 9267
	public ParticleSystem psys;

	// Token: 0x04002434 RID: 9268
	public float time;
}
