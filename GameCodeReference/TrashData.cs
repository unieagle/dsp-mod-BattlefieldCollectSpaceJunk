using System;
using System.IO;
using UnityEngine;

// Token: 0x02000195 RID: 405
public struct TrashData
{
	// Token: 0x06000F03 RID: 3843 RVA: 0x000EC4A8 File Offset: 0x000EA6A8
	public void SetEmpty()
	{
		this.warningId = 0;
		this.landPlanetId = 0;
		this.nearPlanetId = 0;
		this.nearStarId = 0;
		this.nearStarGravity = 0.0;
		this.life = 0;
		this.lRot.w = (this.lRot.z = (this.lRot.y = (this.lRot.x = (this.lPos.z = (this.lPos.y = (this.lPos.x = 0f))))));
		this.uPos.z = (this.uPos.y = (this.uPos.x = 0.0));
		this.uRot.w = (this.uRot.z = (this.uRot.y = (this.uRot.x = 0f)));
		this.uVel.z = (this.uVel.y = (this.uVel.x = 0.0));
		this.uAgl.z = (this.uAgl.y = (this.uAgl.x = 0f));
	}

	// Token: 0x06000F04 RID: 3844 RVA: 0x000EC614 File Offset: 0x000EA814
	public void Export(BinaryWriter w)
	{
		w.Write(2);
		w.Write(this.warningId);
		w.Write(this.landPlanetId);
		w.Write(this.nearPlanetId);
		w.Write(this.nearStarId);
		w.Write(this.nearStarGravity);
		w.Write(this.life);
		w.Write(this.lPos.x);
		w.Write(this.lPos.y);
		w.Write(this.lPos.z);
		w.Write(this.lRot.x);
		w.Write(this.lRot.y);
		w.Write(this.lRot.z);
		w.Write(this.lRot.w);
		w.Write(this.uPos.x);
		w.Write(this.uPos.y);
		w.Write(this.uPos.z);
		w.Write(this.uRot.x);
		w.Write(this.uRot.y);
		w.Write(this.uRot.z);
		w.Write(this.uRot.w);
		w.Write(this.uVel.x);
		w.Write(this.uVel.y);
		w.Write(this.uVel.z);
		w.Write(this.uAgl.x);
		w.Write(this.uAgl.y);
		w.Write(this.uAgl.z);
	}

	// Token: 0x06000F05 RID: 3845 RVA: 0x000EC7C4 File Offset: 0x000EA9C4
	public void Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		if (num >= 1)
		{
			this.warningId = r.ReadInt32();
		}
		else
		{
			this.warningId = 0;
		}
		this.landPlanetId = r.ReadInt32();
		this.nearPlanetId = r.ReadInt32();
		this.nearStarId = r.ReadInt32();
		this.nearStarGravity = r.ReadDouble();
		if (num >= 2)
		{
			this.life = r.ReadInt32();
		}
		else
		{
			this.life = 0;
		}
		this.lPos.x = r.ReadSingle();
		this.lPos.y = r.ReadSingle();
		this.lPos.z = r.ReadSingle();
		this.lRot.x = r.ReadSingle();
		this.lRot.y = r.ReadSingle();
		this.lRot.z = r.ReadSingle();
		this.lRot.w = r.ReadSingle();
		this.uPos.x = r.ReadDouble();
		this.uPos.y = r.ReadDouble();
		this.uPos.z = r.ReadDouble();
		this.uRot.x = r.ReadSingle();
		this.uRot.y = r.ReadSingle();
		this.uRot.z = r.ReadSingle();
		this.uRot.w = r.ReadSingle();
		this.uVel.x = r.ReadDouble();
		this.uVel.y = r.ReadDouble();
		this.uVel.z = r.ReadDouble();
		this.uAgl.x = r.ReadSingle();
		this.uAgl.y = r.ReadSingle();
		this.uAgl.z = r.ReadSingle();
	}

	// Token: 0x04001081 RID: 4225
	public int warningId;

	// Token: 0x04001082 RID: 4226
	public int landPlanetId;

	// Token: 0x04001083 RID: 4227
	public int nearPlanetId;

	// Token: 0x04001084 RID: 4228
	public int nearStarId;

	// Token: 0x04001085 RID: 4229
	public double nearStarGravity;

	// Token: 0x04001086 RID: 4230
	public int life;

	// Token: 0x04001087 RID: 4231
	public Vector3 lPos;

	// Token: 0x04001088 RID: 4232
	public Quaternion lRot;

	// Token: 0x04001089 RID: 4233
	public VectorLF3 uPos;

	// Token: 0x0400108A RID: 4234
	public Quaternion uRot;

	// Token: 0x0400108B RID: 4235
	public VectorLF3 uVel;

	// Token: 0x0400108C RID: 4236
	public Vector3 uAgl;
}
