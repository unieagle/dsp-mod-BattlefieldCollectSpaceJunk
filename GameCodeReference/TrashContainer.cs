using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x0200018C RID: 396
public class TrashContainer
{
	// Token: 0x06000E91 RID: 3729 RVA: 0x000E51A8 File Offset: 0x000E33A8
	public TrashContainer()
	{
		this.argBuffer = new ComputeBuffer(5, 4, ComputeBufferType.DrawIndirect);
		this.trashMatInst = Object.Instantiate<Material>(Configs.builtin.trashMat);
		this.trashMatInstHigh = Object.Instantiate<Material>(Configs.builtin.trashMatHighlight);
		this.trashMesh = Configs.builtin.trashMesh;
	}

	// Token: 0x06000E92 RID: 3730 RVA: 0x000E5214 File Offset: 0x000E3414
	public void Free()
	{
		this.trashObjPool = null;
		this.trashDataPool = null;
		this.trashRecycle = null;
		this.trashCapacity = (this.trashCursor = (this.trashRecycleCursor = 0));
		if (this.computeBuffer != null)
		{
			this.computeBuffer.Release();
			this.computeBuffer = null;
		}
		if (this.argBuffer != null)
		{
			this.argBuffer.Release();
			this.argBuffer = null;
		}
		if (this.trashMatInst != null)
		{
			Object.Destroy(this.trashMatInst);
			this.trashMatInst = null;
		}
		if (this.trashMatInstHigh != null)
		{
			Object.Destroy(this.trashMatInstHigh);
			this.trashMatInstHigh = null;
		}
	}

	// Token: 0x06000E93 RID: 3731 RVA: 0x000E52C3 File Offset: 0x000E34C3
	public void SetForNewGame()
	{
		this.SetTrashCapacity(16);
	}

	// Token: 0x06000E94 RID: 3732 RVA: 0x000E52D0 File Offset: 0x000E34D0
	private void SetTrashCapacity(int newCapacity)
	{
		TrashObject[] array = this.trashObjPool;
		TrashData[] array2 = this.trashDataPool;
		this.trashObjPool = new TrashObject[newCapacity];
		this.trashDataPool = new TrashData[newCapacity];
		this.trashRecycle = new int[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.trashObjPool, (newCapacity > this.trashCapacity) ? this.trashCapacity : newCapacity);
		}
		if (array2 != null)
		{
			Array.Copy(array2, this.trashDataPool, (newCapacity > this.trashCapacity) ? this.trashCapacity : newCapacity);
		}
		this.trashCapacity = newCapacity;
		if (this.computeBuffer != null)
		{
			this.computeBuffer.Release();
		}
		this.computeBuffer = new ComputeBuffer(this.trashCapacity, 44, ComputeBufferType.Default);
	}

	// Token: 0x06000E95 RID: 3733 RVA: 0x000E5380 File Offset: 0x000E3580
	public int NewTrash(TrashObject trashObj, TrashData trashData)
	{
		int num = 0;
		TrashObject[] obj = this.trashObjPool;
		lock (obj)
		{
			if (this.trashRecycleCursor > 0)
			{
				int[] array = this.trashRecycle;
				int num2 = this.trashRecycleCursor - 1;
				this.trashRecycleCursor = num2;
				num = array[num2];
			}
			else
			{
				int num2 = this.trashCursor;
				this.trashCursor = num2 + 1;
				num = num2;
				if (num == this.trashCapacity)
				{
					this.SetTrashCapacity(this.trashCapacity * 2);
				}
			}
			this.trashObjPool[num] = trashObj;
			this.trashDataPool[num] = trashData;
		}
		return num;
	}

	// Token: 0x06000E96 RID: 3734 RVA: 0x000E5424 File Offset: 0x000E3624
	public void RemoveTrash(int index)
	{
		TrashObject[] obj = this.trashObjPool;
		lock (obj)
		{
			if (this.trashObjPool[index].item != 0)
			{
				this.trashObjPool[index].SetEmpty();
				this.trashDataPool[index].SetEmpty();
				int[] array = this.trashRecycle;
				int num = this.trashRecycleCursor;
				this.trashRecycleCursor = num + 1;
				array[num] = index;
			}
		}
	}

	// Token: 0x06000E97 RID: 3735 RVA: 0x000E54AC File Offset: 0x000E36AC
	public void Draw()
	{
		uint indexCount = this.trashMesh.GetIndexCount(0);
		this.computeBuffer.SetData(this.trashObjPool, 0, 0, this.trashCursor);
		if (this.trashCursor - this.trashRecycleCursor > 0)
		{
			Vector3 position = GameMain.mainPlayer.position;
			this.trashMatInst.SetBuffer("_Buffer", this.computeBuffer);
			this.trashMatInst.SetTexture("_MainTex", GameMain.iconSet.texture);
			this.trashMatInst.SetBuffer("_IndexBuffer", GameMain.iconSet.itemIconIndexBuffer);
			this.trashMatInst.SetVector("_PlayerPos", position + position.normalized * 2f);
			this.trashMatInstHigh.SetBuffer("_Buffer", this.computeBuffer);
			this.trashMatInstHigh.SetTexture("_MainTex", GameMain.iconSet.texture);
			this.trashMatInstHigh.SetBuffer("_IndexBuffer", GameMain.iconSet.itemIconIndexBuffer);
			this.trashMatInstHigh.SetVector("_PlayerPos", position + position.normalized * 2f);
			this.argArray[0] = indexCount;
			this.argArray[1] = (uint)this.trashCursor;
			this.argArray[2] = 0U;
			this.argArray[3] = 0U;
			this.argArray[4] = 0U;
			this.argBuffer.SetData(this.argArray);
			Graphics.DrawMeshInstancedIndirect(this.trashMesh, 0, this.trashMatInst, new Bounds(Vector3.zero, new Vector3(200000f, 200000f, 200000f)), this.argBuffer, 0, null, ShadowCastingMode.On, true, 0, null, LightProbeUsage.BlendProbes);
			Graphics.DrawMeshInstancedIndirect(this.trashMesh, 0, this.trashMatInstHigh, new Bounds(Vector3.zero, new Vector3(200000f, 200000f, 200000f)), this.argBuffer, 0, null, ShadowCastingMode.On, true, 0, null, LightProbeUsage.BlendProbes);
			this.GpuAnalysis();
		}
	}

	// Token: 0x06000E98 RID: 3736 RVA: 0x000E56AC File Offset: 0x000E38AC
	public void Export(BinaryWriter w)
	{
		w.Write(0);
		w.Write(this.trashCapacity);
		w.Write(this.trashCursor);
		w.Write(this.trashRecycleCursor);
		for (int i = 0; i < this.trashCursor; i++)
		{
			this.trashObjPool[i].Export(w);
		}
		for (int j = 0; j < this.trashCursor; j++)
		{
			this.trashDataPool[j].Export(w);
		}
		for (int k = 0; k < this.trashRecycleCursor; k++)
		{
			w.Write(this.trashRecycle[k]);
		}
	}

	// Token: 0x06000E99 RID: 3737 RVA: 0x000E574C File Offset: 0x000E394C
	public void Import(BinaryReader r)
	{
		r.ReadInt32();
		int num = r.ReadInt32();
		this.trashCursor = r.ReadInt32();
		this.trashRecycleCursor = r.ReadInt32();
		this.SetTrashCapacity(num);
		for (int i = 0; i < this.trashCursor; i++)
		{
			this.trashObjPool[i].Import(r);
		}
		for (int j = 0; j < this.trashCursor; j++)
		{
			this.trashDataPool[j].Import(r);
		}
		for (int k = 0; k < this.trashRecycleCursor; k++)
		{
			this.trashRecycle[k] = r.ReadInt32();
		}
	}

	// Token: 0x06000E9A RID: 3738 RVA: 0x000E57EC File Offset: 0x000E39EC
	private void GpuAnalysis()
	{
		if (PerformanceMonitor.GpuProfilerOn)
		{
			int num = (this.trashCursor - this.trashRecycleCursor) * 2;
			int vertexCount = this.trashMesh.vertexCount;
			PerformanceMonitor.RecordGpuWork(EGpuWorkEntry.Trash, num, num * vertexCount);
		}
	}

	// Token: 0x0400101D RID: 4125
	public int trashCapacity;

	// Token: 0x0400101E RID: 4126
	public int trashCursor;

	// Token: 0x0400101F RID: 4127
	public TrashObject[] trashObjPool;

	// Token: 0x04001020 RID: 4128
	public TrashData[] trashDataPool;

	// Token: 0x04001021 RID: 4129
	public int[] trashRecycle;

	// Token: 0x04001022 RID: 4130
	public int trashRecycleCursor;

	// Token: 0x04001023 RID: 4131
	public ComputeBuffer computeBuffer;

	// Token: 0x04001024 RID: 4132
	private ComputeBuffer argBuffer;

	// Token: 0x04001025 RID: 4133
	private uint[] argArray = new uint[5];

	// Token: 0x04001026 RID: 4134
	private Material trashMatInst;

	// Token: 0x04001027 RID: 4135
	private Material trashMatInstHigh;

	// Token: 0x04001028 RID: 4136
	private Mesh trashMesh;
}
