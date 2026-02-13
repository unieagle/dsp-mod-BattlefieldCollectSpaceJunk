using System;
using System.IO;
using UnityEngine;

// Token: 0x02000194 RID: 404
public struct TrashObject
{
	// Token: 0x06000EFF RID: 3839 RVA: 0x000EC25B File Offset: 0x000EA45B
	public TrashObject(int _item, int _cnt, int _inc, Vector3 _rpos, Quaternion _rrot)
	{
		this.item = _item;
		this.count = _cnt;
		this.inc = _inc;
		this.rPos = _rpos;
		this.rRot = _rrot;
		this.expire = -1;
	}

	// Token: 0x06000F00 RID: 3840 RVA: 0x000EC28C File Offset: 0x000EA48C
	public void SetEmpty()
	{
		this.count = (this.item = 0);
		this.inc = 0;
		this.rRot.w = (this.rRot.z = (this.rRot.y = (this.rRot.x = (this.rPos.z = (this.rPos.y = (this.rPos.x = 0f))))));
		this.expire = 0;
	}

	// Token: 0x06000F01 RID: 3841 RVA: 0x000EC31C File Offset: 0x000EA51C
	public void Export(BinaryWriter w)
	{
		w.Write(2);
		w.Write((short)this.item);
		w.Write((byte)this.count);
		w.Write((short)this.inc);
		w.Write(this.rPos.x);
		w.Write(this.rPos.y);
		w.Write(this.rPos.z);
		w.Write(this.rRot.x);
		w.Write(this.rRot.y);
		w.Write(this.rRot.z);
		w.Write(this.rRot.w);
		w.Write(this.expire);
	}

	// Token: 0x06000F02 RID: 3842 RVA: 0x000EC3DC File Offset: 0x000EA5DC
	public void Import(BinaryReader r)
	{
		int num = (int)r.ReadByte();
		this.item = (int)r.ReadInt16();
		this.count = (int)r.ReadByte();
		if (num >= 1)
		{
			this.inc = (int)r.ReadInt16();
		}
		else
		{
			this.inc = 0;
		}
		this.rPos.x = (float)r.ReadInt32();
		this.rPos.y = (float)r.ReadInt32();
		this.rPos.z = (float)r.ReadInt32();
		this.rRot.x = r.ReadSingle();
		this.rRot.y = r.ReadSingle();
		this.rRot.z = r.ReadSingle();
		this.rRot.w = r.ReadSingle();
		this.expire = r.ReadInt32();
	}

	// Token: 0x0400107A RID: 4218
	public int item;

	// Token: 0x0400107B RID: 4219
	public int count;

	// Token: 0x0400107C RID: 4220
	public int inc;

	// Token: 0x0400107D RID: 4221
	public Vector3 rPos;

	// Token: 0x0400107E RID: 4222
	public Quaternion rRot;

	// Token: 0x0400107F RID: 4223
	public int expire;

	// Token: 0x04001080 RID: 4224
	public const int dataLen = 44;
}
