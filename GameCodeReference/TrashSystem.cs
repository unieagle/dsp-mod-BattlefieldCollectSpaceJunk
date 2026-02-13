using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Token: 0x0200018D RID: 397
public class TrashSystem
{
	// Token: 0x06000E9B RID: 3739 RVA: 0x000E5828 File Offset: 0x000E3A28
	public TrashSystem(GameData _data)
	{
		this.gameData = _data;
		this.galaxy = this.gameData.galaxy;
		this.player = this.gameData.mainPlayer;
		this.container = new TrashContainer();
		this.randSeed = this.galaxy.seed;
		RandomTable.Integer(ref this.randSeed, 1);
		RandomTable.Integer(ref this.randSeed, 1);
		this.enemyDropBans = new HashSet<int>();
	}

	// Token: 0x06000E9C RID: 3740 RVA: 0x000E58A5 File Offset: 0x000E3AA5
	public void SetForNewGame()
	{
		this.container.SetForNewGame();
	}

	// Token: 0x06000E9D RID: 3741 RVA: 0x000E58B2 File Offset: 0x000E3AB2
	public void Free()
	{
		this.container.Free();
		this.gameData = null;
		this.galaxy = null;
		this.player = null;
		this.enemyDropBans.Clear();
		this.enemyDropBans = null;
	}

	// Token: 0x06000E9E RID: 3742 RVA: 0x000E58E6 File Offset: 0x000E3AE6
	public void Export(BinaryWriter w)
	{
		w.Write(0);
		this.container.Export(w);
	}

	// Token: 0x06000E9F RID: 3743 RVA: 0x000E58FB File Offset: 0x000E3AFB
	public void Import(BinaryReader r)
	{
		r.ReadInt32();
		this.container.Import(r);
	}

	// Token: 0x06000EA0 RID: 3744 RVA: 0x000E5910 File Offset: 0x000E3B10
	public void AddTrashOnPlanet(int itemId, int count, int inc, int objId, PlanetData planet)
	{
		ItemProto itemProto = LDB.items.Select(itemId);
		if (itemProto != null && (objId != 0 && planet != null && ((objId > 0 && planet.factory.entityPool[objId].id == objId) || (objId < 0 && planet.factory.prebuildPool[-objId].id == -objId))))
		{
			VectorLF3 vectorLF = (objId > 0) ? planet.factory.entityPool[objId].pos : planet.factory.prebuildPool[-objId].pos;
			vectorLF += vectorLF.normalized;
			if (vectorLF.magnitude < 170.0)
			{
				return;
			}
			VectorLF3 normalized = Maths.QRotateLF(planet.runtimeRotation, vectorLF).normalized;
			VectorLF3 lhs = Maths.QRotateLF(planet.runtimeRotation, vectorLF) + planet.uPosition;
			int astroId = planet.star.astroId;
			double starGravity = this.GetStarGravity(planet.star.id);
			WarningSystem warningSystem = this.gameData.warningSystem;
			int num = 500;
			while (count > 0 && num > 0)
			{
				int num2 = (count < itemProto.StackSize) ? count : itemProto.StackSize;
				int inc2 = this.split_inc(ref count, ref inc, num2);
				TrashData trashData = default(TrashData);
				trashData.landPlanetId = 0;
				trashData.nearPlanetId = 0;
				trashData.nearStarId = astroId;
				trashData.nearStarGravity = starGravity;
				trashData.lPos = Vector3.zero;
				trashData.lRot = Quaternion.identity;
				trashData.uPos = lhs + RandomTable.SphericNormal(ref this.randSeed, 0.2);
				trashData.uRot = Quaternion.LookRotation(RandomTable.SphericNormal(ref this.randSeed, 1.0).normalized);
				trashData.uVel = planet.GetUniversalVelocityAtLocalPoint(GameMain.gameTime, vectorLF) + normalized * 15.0 + RandomTable.SphericNormal(ref this.randSeed, 4.0);
				trashData.uAgl = RandomTable.SphericNormal(ref this.randSeed, 0.03);
				TrashObject trashObj = new TrashObject(itemId, num2, inc2, Maths.QInvRotateLF(this.gameData.relativeRot, trashData.uPos - this.gameData.relativePos), Quaternion.Inverse(this.gameData.relativeRot) * trashData.uRot);
				int objectId = this.container.NewTrash(trashObj, trashData);
				if (trashData.warningId > 0)
				{
					warningSystem.warningPool[trashData.warningId].objectId = objectId;
					warningSystem.warningPool[trashData.warningId].detailId1 = itemId;
				}
				num--;
			}
		}
	}

	// Token: 0x06000EA1 RID: 3745 RVA: 0x000E5C24 File Offset: 0x000E3E24
	public int AddTrashFromGroundEnemy(int itemId, int count, int life, int enemyId, PlanetFactory factory)
	{
		if (this.enemyDropBans.Contains(itemId))
		{
			return 0;
		}
		ItemProto itemProto = LDB.items.Select(itemId);
		if (itemProto == null || (factory == null || enemyId <= 0 || factory.enemyPool[enemyId].id != enemyId))
		{
			return 0;
		}
		ref EnemyData ptr = ref factory.enemyPool[enemyId];
		float num = SkillSystem.RoughHeightByModelIndex[(int)ptr.modelIndex];
		VectorLF3 v = ptr.pos + ptr.pos.normalized * (double)(num * 0.6f);
		if (v.magnitude < 170.0)
		{
			return 0;
		}
		PlanetData planet = factory.planet;
		VectorLF3 lhs = Maths.QRotateLF(planet.runtimeRotation, v);
		VectorLF3 normalized = lhs.normalized;
		VectorLF3 lhs2 = lhs + planet.uPosition;
		Vector3 vel = ptr.vel;
		vel.x *= 0.7f;
		vel.y *= 0.7f;
		vel.z *= 0.7f;
		Vector3 vec;
		this.galaxy.astrosData[planet.astroId].VelocityL2U(ref v, ref vel, out vec);
		int astroId = planet.star.astroId;
		double starGravity = this.GetStarGravity(planet.star.id);
		int cnt = (count < itemProto.StackSize * 20) ? count : (itemProto.StackSize * 20);
		int trashCount = this.trashCount;
		float num2 = (float)(500 / (trashCount + 100));
		TrashData trashData = default(TrashData);
		trashData.warningId = 0;
		trashData.landPlanetId = 0;
		trashData.nearPlanetId = 0;
		trashData.nearStarId = astroId;
		trashData.nearStarGravity = starGravity;
		trashData.life = life / 3 + (int)((double)((float)life * num2 * 2f / 3f) + 0.5);
		trashData.lPos = Vector3.zero;
		trashData.lRot = Quaternion.identity;
		trashData.uPos = lhs2 + RandomTable.SphericNormal(ref this.randSeed, 0.2);
		trashData.uRot = Quaternion.LookRotation(RandomTable.SphericNormal(ref this.randSeed, 1.0).normalized);
		trashData.uVel = vec + normalized * 10.0 + RandomTable.SphericNormal(ref this.randSeed, 3.0);
		trashData.uAgl = RandomTable.SphericNormal(ref this.randSeed, 0.03);
		TrashObject trashObj = new TrashObject(itemId, cnt, 0, Maths.QInvRotateLF(this.gameData.relativeRot, trashData.uPos - this.gameData.relativePos), Quaternion.Inverse(this.gameData.relativeRot) * trashData.uRot);
		return this.container.NewTrash(trashObj, trashData);
	}

	// Token: 0x06000EA2 RID: 3746 RVA: 0x000E5F30 File Offset: 0x000E4130
	public void AddTrash(int itemId, int count, int inc, int objId, int life = 0)
	{
		ItemProto itemProto = LDB.items.Select(itemId);
		if (itemProto != null)
		{
			PlanetData localPlanet = this.gameData.localPlanet;
			VectorLF3 vectorLF = Maths.QRotateLF(this.player.uRotation, new VectorLF3(0f, 1f, 0f));
			VectorLF3 lhs = this.player.uPosition;
			int nearStarId = (this.gameData.localStar != null) ? this.gameData.localStar.astroId : 0;
			double nearStarGravity = (this.gameData.localStar != null) ? this.GetStarGravity(this.gameData.localStar.id) : 0.0;
			bool flag = objId != 0 && localPlanet != null && localPlanet.factoryLoaded && ((objId > 0 && localPlanet.factory.entityPool[objId].id == objId) || (objId < 0 && localPlanet.factory.prebuildPool[-objId].id == -objId));
			if (flag)
			{
				VectorLF3 vectorLF2 = (objId > 0) ? localPlanet.factory.entityPool[objId].pos : localPlanet.factory.prebuildPool[-objId].pos;
				vectorLF2 += vectorLF2.normalized;
				if (vectorLF2.magnitude < 170.0)
				{
					flag = false;
				}
				if (flag)
				{
					lhs = Maths.QRotateLF(localPlanet.runtimeRotation, vectorLF2) + localPlanet.uPosition;
				}
				else
				{
					lhs = this.player.uPosition + vectorLF * (this.player.sailing ? 1.0 : 2.5);
				}
			}
			WarningSystem warningSystem = this.gameData.warningSystem;
			int num = 500;
			int num2 = count;
			while (count > 0 && num > 0)
			{
				int num3 = (count < itemProto.StackSize) ? count : itemProto.StackSize;
				int inc2 = this.split_inc(ref count, ref inc, num3);
				TrashObject trashObj = new TrashObject(itemId, num3, inc2, Vector3.zero, Quaternion.identity);
				TrashData trashData = default(TrashData);
				trashData.landPlanetId = 0;
				trashData.nearPlanetId = 0;
				trashData.nearStarId = nearStarId;
				trashData.nearStarGravity = nearStarGravity;
				trashData.life = life;
				if (flag)
				{
					trashData.lPos = Vector3.zero;
					trashData.lRot = Quaternion.identity;
					trashData.uPos = lhs + RandomTable.SphericNormal(ref this.randSeed, 0.2);
					trashData.uRot = Quaternion.LookRotation(RandomTable.SphericNormal(ref this.randSeed, 1.0).normalized, vectorLF);
					trashData.uVel = this.player.uVelocity + RandomTable.SphericNormal(ref this.randSeed, 4.0) + vectorLF * 15.0;
					trashData.uAgl = RandomTable.SphericNormal(ref this.randSeed, 0.03);
				}
				else
				{
					trashData.lPos = Vector3.zero;
					trashData.lRot = Quaternion.identity;
					trashData.uPos = lhs + RandomTable.SphericNormal(ref this.randSeed, 0.3);
					trashData.uRot = Quaternion.LookRotation(RandomTable.SphericNormal(ref this.randSeed, 1.0).normalized, vectorLF);
					trashData.uVel = this.player.uVelocity + RandomTable.SphericNormal(ref this.randSeed, this.player.sailing ? 0.2 : 4.5) + vectorLF * (this.player.sailing ? 0.0 : 20.0);
					trashData.uAgl = RandomTable.SphericNormal(ref this.randSeed, 0.03);
				}
				int objectId = this.container.NewTrash(trashObj, trashData);
				if (trashData.warningId > 0)
				{
					warningSystem.warningPool[trashData.warningId].objectId = objectId;
					warningSystem.warningPool[trashData.warningId].detailId1 = itemId;
				}
				num--;
			}
			if (GameMain.gameScenario != null)
			{
				GameMain.gameScenario.NotifyOnAddTrash(itemId, num2 - count);
			}
		}
	}

	// Token: 0x06000EA3 RID: 3747 RVA: 0x000E63CC File Offset: 0x000E45CC
	public void GameTick(long time)
	{
		int trashCursor = this.container.trashCursor;
		VectorLF3 relativePos = this.gameData.relativePos;
		Quaternion relativeRot = this.gameData.relativeRot;
		AstroData[] astrosData = this.galaxy.astrosData;
		WarningSystem warningSystem = this.gameData.warningSystem;
		double num = 0.016666666666666666;
		int num2 = ((int)(time % (long)this.galaxy.starCount) + 1) * 100;
		int localPlanetId = (this.gameData.localPlanet == null) ? 0 : this.gameData.localPlanet.id;
		double starGravity = this.GetStarGravity(num2 / 100);
		TrashObject[] trashObjPool = this.container.trashObjPool;
		TrashData[] trashDataPool = this.container.trashDataPool;
		PlanetRawData localPlanetData = (this.gameData.localPlanet == null) ? null : this.gameData.localPlanet.data;
		int num3 = 0;
		for (int i = 0; i < trashCursor; i++)
		{
			if (trashObjPool[i].item > 0)
			{
				if (trashObjPool[i].expire > 0)
				{
					TrashObject[] array = trashObjPool;
					int num4 = i;
					array[num4].expire = array[num4].expire - 1;
					if (trashDataPool[i].life > 0)
					{
						trashDataPool[i].life = -1;
					}
					bool trashLife = trashDataPool[i].life != 0;
					if (trashObjPool[i].expire == 0)
					{
						if (!this.player.isAlive)
						{
							trashObjPool[i].expire = -1;
							goto IL_4E2;
						}
						int item = trashObjPool[i].item;
						int count = trashObjPool[i].count;
						int inc = trashObjPool[i].inc;
						this.RemoveTrash(i);
						int num5 = this.player.TryAddItemToPackage(item, count, inc, true, 0, trashLife);
						if (num5 > 0)
						{
							UIItemup.Up(item, num5);
							VFAudio.Create("transfer-item", null, Vector3.zero, true, 3, -1, -1L);
							goto IL_4E2;
						}
						goto IL_4E2;
					}
				}
				if (trashDataPool[i].life > 0)
				{
					TrashData[] array2 = trashDataPool;
					int num6 = i;
					array2[num6].life = array2[num6].life - 1;
					if (trashDataPool[i].life == 0)
					{
						trashDataPool[i].life = -1;
						this.RemoveTrash(i);
						goto IL_4E2;
					}
				}
				int landPlanetId = trashDataPool[i].landPlanetId;
				if (landPlanetId == 0)
				{
					TrashData[] array3 = trashDataPool;
					int num7 = i;
					array3[num7].uPos.x = array3[num7].uPos.x + trashDataPool[i].uVel.x * num;
					TrashData[] array4 = trashDataPool;
					int num8 = i;
					array4[num8].uPos.y = array4[num8].uPos.y + trashDataPool[i].uVel.y * num;
					TrashData[] array5 = trashDataPool;
					int num9 = i;
					array5[num9].uPos.z = array5[num9].uPos.z + trashDataPool[i].uVel.z * num;
					if (!this.Gravity(ref trashDataPool[i], astrosData, num, num2, starGravity, localPlanetId, localPlanetData))
					{
						this.RemoveTrash(i);
						goto IL_4E2;
					}
				}
				else
				{
					trashDataPool[i].uPos = Maths.QRotate(astrosData[landPlanetId].uRot, trashDataPool[i].lPos) + astrosData[landPlanetId].uPos;
				}
				trashDataPool[i].uRot = Quaternion.AngleAxis(1f, trashDataPool[i].uAgl) * trashDataPool[i].uRot;
				trashObjPool[i].rPos = Maths.QInvRotateLF(relativeRot, trashDataPool[i].uPos - relativePos);
				trashObjPool[i].rRot = Quaternion.Inverse(relativeRot) * trashDataPool[i].uRot;
				if (num3 < 100 && trashDataPool[i].life == 0 && trashDataPool[i].warningId == 0)
				{
					trashDataPool[i].warningId = warningSystem.NewWarningData(-3, i, 406).id;
				}
				if (trashDataPool[i].warningId > 0)
				{
					WarningData[] warningPool = warningSystem.warningPool;
					int warningId = trashDataPool[i].warningId;
					warningPool[warningId].localPos.x = trashObjPool[i].rPos.x;
					warningPool[warningId].localPos.y = trashObjPool[i].rPos.y;
					warningPool[warningId].localPos.z = trashObjPool[i].rPos.z;
					warningPool[warningId].state = ((trashObjPool[i].expire < 0) ? 1 : 0);
					warningPool[warningId].detailId1 = trashObjPool[i].item;
					num3++;
				}
			}
			IL_4E2:;
		}
	}

	// Token: 0x06000EA4 RID: 3748 RVA: 0x000E68CC File Offset: 0x000E4ACC
	public bool Gravity(ref TrashData trash, AstroData[] astroPoses, double dt, int testStarId, double testStarGravity, int localPlanetId, PlanetRawData localPlanetData)
	{
		bool result = true;
		trash.nearPlanetId = 0;
		VectorLF3 zero = VectorLF3.zero;
		int nearStarId = trash.nearStarId;
		if (nearStarId > 0)
		{
			double num = 0.0;
			VectorLF3 zero2 = VectorLF3.zero;
			bool flag = false;
			for (int i = nearStarId + 1; i <= nearStarId + 8; i++)
			{
				double num2 = (double)astroPoses[i].uRadius;
				if (num2 < 1.0)
				{
					break;
				}
				VectorLF3 vectorLF = new VectorLF3(astroPoses[i].uPos.x - trash.uPos.x, astroPoses[i].uPos.y - trash.uPos.y, astroPoses[i].uPos.z - trash.uPos.z);
				double num3 = Math.Sqrt(vectorLF.x * vectorLF.x + vectorLF.y * vectorLF.y + vectorLF.z * vectorLF.z);
				double num4 = num2 / num3;
				if (num4 <= 1.5 && num4 >= 0.05)
				{
					double num5 = 1.0;
					if (num3 < 800.0)
					{
						num5 = Math.Pow(10.0, (800.0 - num3) / 150.0);
						flag = true;
						trash.nearPlanetId = i;
					}
					double num6 = num2 + 0.35;
					if (i == localPlanetId && num3 < 210.0)
					{
						Vector3 vpos = Maths.QInvRotateLF(astroPoses[i].uRot, new Vector3((float)(-(float)vectorLF.x), (float)(-(float)vectorLF.y), (float)(-(float)vectorLF.z)));
						num6 = (double)localPlanetData.QueryModifiedHeight(vpos) + 0.15;
						if (num6 < num2 + 0.15)
						{
							num6 = num2 + 0.15;
						}
					}
					if (num3 < num6)
					{
						if (num6 > 600.0)
						{
							result = false;
						}
						else
						{
							double rhs = (double)((trash.life == 0) ? 0.35f : 0.15f);
							double rhs2 = (double)((trash.life == 0) ? 0.925f : 0.75f);
							VectorLF3 v = -vectorLF;
							VectorLF3 vectorLF2 = Maths.QInvRotateLF(astroPoses[i].uRot, v);
							VectorLF3 rhs3 = (Maths.QRotateLF(astroPoses[i].uRotNext, vectorLF2) + astroPoses[i].uPosNext - trash.uPos) * 60.0;
							VectorLF3 vectorLF3 = trash.uVel - rhs3;
							VectorLF3 normalized = v.normalized;
							if (vectorLF3.magnitude > 1.0)
							{
								VectorLF3 vectorLF4 = normalized;
								if (i == localPlanetId)
								{
									Vector3 normalized2 = Maths.QInvRotate(astroPoses[i].uRot, vectorLF3).normalized;
									RaycastHit raycastHit;
									if (Physics.Raycast(new Ray(vectorLF2 - normalized2, normalized2), out raycastHit, 3f, 512))
									{
										vectorLF4 = Maths.QRotateLF(astroPoses[i].uRot, raycastHit.normal).normalized;
									}
								}
								double rhs4 = -vectorLF4.x * vectorLF3.x - vectorLF4.y * vectorLF3.y - vectorLF4.z * vectorLF3.z;
								VectorLF3 vectorLF5 = vectorLF4 * rhs4;
								VectorLF3 lhs = vectorLF3 + vectorLF5;
								trash.uPos = astroPoses[i].uPos + normalized * (num6 + 0.005);
								trash.uVel = vectorLF5 * rhs + lhs * rhs2 + rhs3;
							}
							else
							{
								trash.landPlanetId = i;
								trash.uPos = astroPoses[i].uPos + normalized * (num6 + 0.005);
								trash.uVel = VectorLF3.zero;
								trash.lPos = vectorLF2;
							}
						}
					}
					double num7 = 50.0 * num4 * num4 * num5 / num3;
					zero2.x += vectorLF.x * num7;
					zero2.y += vectorLF.y * num7;
					zero2.z += vectorLF.z * num7;
					num += num5;
				}
			}
			if (!flag)
			{
				double num8 = (double)astroPoses[nearStarId].uRadius;
				VectorLF3 vectorLF6 = new VectorLF3(astroPoses[nearStarId].uPos.x - trash.uPos.x, astroPoses[nearStarId].uPos.y - trash.uPos.y, astroPoses[nearStarId].uPos.z - trash.uPos.z);
				double num9 = vectorLF6.x * vectorLF6.x + vectorLF6.y * vectorLF6.y + vectorLF6.z * vectorLF6.z;
				double num10 = Math.Sqrt(num9);
				double num11 = trash.nearStarGravity / num10 / num9;
				zero2.x += vectorLF6.x * num11;
				zero2.y += vectorLF6.y * num11;
				zero2.z += vectorLF6.z * num11;
				num += 1.0;
				if (num10 > 2400000.0)
				{
					trash.nearPlanetId = 0;
					trash.nearStarId = 0;
					trash.nearStarGravity = 0.0;
				}
				else if (num10 < num8)
				{
					result = false;
				}
			}
			double num12 = dt / num;
			trash.uVel.x = trash.uVel.x + zero2.x * num12;
			trash.uVel.y = trash.uVel.y + zero2.y * num12;
			trash.uVel.z = trash.uVel.z + zero2.z * num12;
		}
		else if (testStarId > 0)
		{
			VectorLF3 vectorLF7 = new VectorLF3(astroPoses[testStarId].uPos.x - trash.uPos.x, astroPoses[testStarId].uPos.y - trash.uPos.y, astroPoses[testStarId].uPos.z - trash.uPos.z);
			if (vectorLF7.x * vectorLF7.x + vectorLF7.y * vectorLF7.y + vectorLF7.z * vectorLF7.z < 4000000000000.0)
			{
				trash.nearStarId = testStarId;
				trash.nearStarGravity = testStarGravity;
			}
		}
		return result;
	}

	// Token: 0x06000EA5 RID: 3749 RVA: 0x000E6FB4 File Offset: 0x000E51B4
	public double GetStarGravity(int starId)
	{
		return 346586930.95732176 * (double)this.galaxy.StarById(starId).mass;
	}

	// Token: 0x170001BA RID: 442
	// (get) Token: 0x06000EA6 RID: 3750 RVA: 0x000E6FD2 File Offset: 0x000E51D2
	public int trashCount
	{
		get
		{
			return this.container.trashCursor - this.container.trashRecycleCursor;
		}
	}

	// Token: 0x06000EA7 RID: 3751 RVA: 0x000E6FEC File Offset: 0x000E51EC
	public void RemoveTrash(int id)
	{
		if (id >= 0)
		{
			if (this.container.trashDataPool[id].warningId > 0)
			{
				this.gameData.warningSystem.RemoveWarningData(this.container.trashDataPool[id].warningId);
			}
			this.container.RemoveTrash(id);
		}
	}

	// Token: 0x06000EA8 RID: 3752 RVA: 0x000E7048 File Offset: 0x000E5248
	public void ClearAllTrash()
	{
		this.gameData.warningSystem.ClearAllTrashWarnings();
		TrashObject[] trashObjPool = this.container.trashObjPool;
		lock (trashObjPool)
		{
			Array.Clear(this.container.trashObjPool, 0, this.container.trashCapacity);
			Array.Clear(this.container.trashDataPool, 0, this.container.trashCapacity);
			Array.Clear(this.container.trashRecycle, 0, this.container.trashCapacity);
			this.container.trashCursor = 0;
			this.container.trashRecycleCursor = 0;
		}
	}

	// Token: 0x06000EA9 RID: 3753 RVA: 0x000E7104 File Offset: 0x000E5304
	private int split_inc(ref int n, ref int m, int p)
	{
		if (n == 0)
		{
			return 0;
		}
		int num = m / n;
		int num2 = m - num * n;
		n -= p;
		num2 -= n;
		num = ((num2 > 0) ? (num * p + num2) : (num * p));
		m -= num;
		return num;
	}

	// Token: 0x04001029 RID: 4137
	public GameData gameData;

	// Token: 0x0400102A RID: 4138
	public GalaxyData galaxy;

	// Token: 0x0400102B RID: 4139
	public Player player;

	// Token: 0x0400102C RID: 4140
	public TrashContainer container;

	// Token: 0x0400102D RID: 4141
	public int randSeed;

	// Token: 0x0400102E RID: 4142
	public HashSet<int> enemyDropBans;
}
