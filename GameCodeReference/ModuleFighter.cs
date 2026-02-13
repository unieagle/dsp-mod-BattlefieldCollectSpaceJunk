using System;

// Token: 0x02000093 RID: 147
public struct ModuleFighter
{
	// Token: 0x0600064A RID: 1610 RVA: 0x00037C24 File Offset: 0x00035E24
	public void ClearFighterForeignKey()
	{
		this.craftId = 0;
	}

	// Token: 0x040004F6 RID: 1270
	public int itemId;

	// Token: 0x040004F7 RID: 1271
	public int craftId;

	// Token: 0x040004F8 RID: 1272
	public int count;

	// Token: 0x040004F9 RID: 1273
	public ECraftSize size;

	// Token: 0x040004FA RID: 1274
	public int rowInUI;

	// Token: 0x040004FB RID: 1275
	public int colInUI;

	// Token: 0x040004FC RID: 1276
	public ModuleFighterFormDesc formDesc;
}
