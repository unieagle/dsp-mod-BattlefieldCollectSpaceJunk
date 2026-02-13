using System;
using UnityEngine;

// Token: 0x02000334 RID: 820
[Serializable]
public class FleetProto : Proto
{
	// Token: 0x170004E9 RID: 1257
	// (get) Token: 0x060022E4 RID: 8932 RVA: 0x00245D66 File Offset: 0x00243F66
	public Sprite iconSprite
	{
		get
		{
			return this._iconSprite;
		}
	}

	// Token: 0x060022E5 RID: 8933 RVA: 0x00245D6E File Offset: 0x00243F6E
	public void RefreshTranslation()
	{
		base.name = this.Name.Translate();
	}

	// Token: 0x060022E6 RID: 8934 RVA: 0x00245D84 File Offset: 0x00243F84
	public void Preload()
	{
		base.name = this.Name.Translate();
		ModelProto modelProto = LDB.models.modelArray[this.ModelIndex];
		if (modelProto != null)
		{
			this.prefabDesc = modelProto.prefabDesc;
		}
		else
		{
			this.prefabDesc = PrefabDesc.none;
		}
		if (!string.IsNullOrEmpty(this.IconPath))
		{
			this._iconSprite = Resources.Load<Sprite>(this.IconPath);
		}
	}

	// Token: 0x04002A24 RID: 10788
	public string Description;

	// Token: 0x04002A25 RID: 10789
	public int ModelIndex;

	// Token: 0x04002A26 RID: 10790
	public bool IsSpace;

	// Token: 0x04002A27 RID: 10791
	public string IconPath;

	// Token: 0x04002A28 RID: 10792
	[NonSerialized]
	public PrefabDesc prefabDesc;

	// Token: 0x04002A29 RID: 10793
	public static int[] kMechaFleetIds = new int[]
	{
		1,
		2,
		3,
		4,
		5
	};

	// Token: 0x04002A2A RID: 10794
	public static int[] kMechaFleetGroundIds = new int[]
	{
		1
	};

	// Token: 0x04002A2B RID: 10795
	public static int[] kMechaFleetSpaceIds = new int[]
	{
		2,
		3,
		4,
		5
	};

	// Token: 0x04002A2C RID: 10796
	private Sprite _iconSprite;
}
