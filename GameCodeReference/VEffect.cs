using System;
using UnityEngine;

// Token: 0x02000291 RID: 657
public class VFEffect : MonoBehaviour
{
	// Token: 0x1700041F RID: 1055
	// (get) Token: 0x06001D01 RID: 7425 RVA: 0x001EE88E File Offset: 0x001ECA8E
	public bool active
	{
		get
		{
			return base.gameObject.activeInHierarchy && base.enabled;
		}
	}

	// Token: 0x06001D02 RID: 7426 RVA: 0x001EE8A5 File Offset: 0x001ECAA5
	public virtual void Open()
	{
		base.gameObject.SetActive(true);
	}

	// Token: 0x06001D03 RID: 7427 RVA: 0x001EE8B3 File Offset: 0x001ECAB3
	public virtual void Close()
	{
		base.gameObject.SetActive(false);
	}

	// Token: 0x06001D04 RID: 7428 RVA: 0x001EE8C1 File Offset: 0x001ECAC1
	public virtual void Reset()
	{
		this.relateObject = null;
		this.relateId = 0;
		this.relateIndex = -1;
		this.relateType = EObjectType.None;
	}

	// Token: 0x17000420 RID: 1056
	// (get) Token: 0x06001D05 RID: 7429 RVA: 0x001EE8E0 File Offset: 0x001ECAE0
	public static Transform effectGroup
	{
		get
		{
			if (VFEffect._effectGroup == null)
			{
				VFEffect._effectGroup = new GameObject("Game Effects")
				{
					transform = 
					{
						position = Vector3.zero,
						rotation = Quaternion.identity
					}
				}.transform;
			}
			return VFEffect._effectGroup;
		}
	}

	// Token: 0x06001D06 RID: 7430 RVA: 0x001EE934 File Offset: 0x001ECB34
	public static void ClearEffects()
	{
		foreach (object obj in VFEffect._effectGroup)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
	}

	// Token: 0x04002435 RID: 9269
	public object relateObject;

	// Token: 0x04002436 RID: 9270
	public int relateId;

	// Token: 0x04002437 RID: 9271
	public int relateIndex;

	// Token: 0x04002438 RID: 9272
	public EObjectType relateType;

	// Token: 0x04002439 RID: 9273
	private static Transform _effectGroup;
}
