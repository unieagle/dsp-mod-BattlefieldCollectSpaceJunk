using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Token: 0x02000138 RID: 312
public struct GeneralMissile : IPoolElement
{
	// Token: 0x1700015D RID: 349
	// (get) Token: 0x0600099C RID: 2460 RVA: 0x0007C47A File Offset: 0x0007A67A
	// (set) Token: 0x0600099D RID: 2461 RVA: 0x0007C482 File Offset: 0x0007A682
	public int ID
	{
		get
		{
			return this.id;
		}
		set
		{
			this.id = value;
		}
	}

	// Token: 0x0600099E RID: 2462 RVA: 0x0007C48B File Offset: 0x0007A68B
	public void Reset()
	{
		this = default(GeneralMissile);
	}

	// Token: 0x0600099F RID: 2463 RVA: 0x0007C494 File Offset: 0x0007A694
	public void Export(BinaryWriter w)
	{
		w.Write(2);
		w.Write(this.id);
		if (this.id > 0)
		{
			w.Write(this.nearAstroId);
			w.Write(this.targetCombatStatId);
			w.Write(this.uPos.x);
			w.Write(this.uPos.y);
			w.Write(this.uPos.z);
			w.Write(this.lastTargetPos.x);
			w.Write(this.lastTargetPos.y);
			w.Write(this.lastTargetPos.z);
			w.Write(this.lastTargetCurrentAstroId);
			w.Write(this.uRot.x);
			w.Write(this.uRot.y);
			w.Write(this.uRot.z);
			w.Write(this.uRot.w);
			w.Write(this.uVel.x);
			w.Write(this.uVel.y);
			w.Write(this.uVel.z);
			w.Write(this.casterVel.x);
			w.Write(this.casterVel.y);
			w.Write(this.casterVel.z);
			w.Write(this.moveAcc);
			w.Write(this.turnAcc);
			w.Write(this.damage);
			w.Write(this.damageIncoming);
			w.Write((short)this.mask);
			w.Write((short)this.life);
			w.Write((short)this.target.type);
			w.Write(this.target.id);
			w.Write(this.target.astroId);
			w.Write((byte)this.caster.type);
			w.Write(this.caster.id);
			w.Write(this.caster.astroId);
			w.Write(this.modelIndex);
		}
	}

	// Token: 0x060009A0 RID: 2464 RVA: 0x0007C6B4 File Offset: 0x0007A8B4
	public void Import(BinaryReader r)
	{
		int num = (int)r.ReadByte();
		this.id = r.ReadInt32();
		if (this.id > 0)
		{
			this.nearAstroId = r.ReadInt32();
			this.targetCombatStatId = r.ReadInt32();
			this.uPos.x = r.ReadDouble();
			this.uPos.y = r.ReadDouble();
			this.uPos.z = r.ReadDouble();
			if (num >= 1)
			{
				this.lastTargetPos.x = r.ReadDouble();
				this.lastTargetPos.y = r.ReadDouble();
				this.lastTargetPos.z = r.ReadDouble();
				this.lastTargetCurrentAstroId = r.ReadInt32();
			}
			else
			{
				this.lastTargetPos.x = 0.0;
				this.lastTargetPos.y = 0.0;
				this.lastTargetPos.z = 0.0;
				this.lastTargetCurrentAstroId = 0;
			}
			this.uRot.x = r.ReadSingle();
			this.uRot.y = r.ReadSingle();
			this.uRot.z = r.ReadSingle();
			this.uRot.w = r.ReadSingle();
			this.uVel.x = r.ReadSingle();
			this.uVel.y = r.ReadSingle();
			this.uVel.z = r.ReadSingle();
			this.casterVel.x = r.ReadSingle();
			this.casterVel.y = r.ReadSingle();
			this.casterVel.z = r.ReadSingle();
			this.moveAcc = r.ReadSingle();
			this.turnAcc = r.ReadSingle();
			this.damage = r.ReadInt32();
			if (num >= 2)
			{
				this.damageIncoming = r.ReadInt32();
			}
			else
			{
				this.damageIncoming = this.damage;
			}
			this.mask = (ETargetTypeMask)r.ReadInt16();
			this.life = (int)r.ReadInt16();
			this.target.type = (ETargetType)r.ReadInt16();
			this.target.id = r.ReadInt32();
			this.target.astroId = r.ReadInt32();
			this.caster.type = (ETargetType)r.ReadByte();
			this.caster.id = r.ReadInt32();
			this.caster.astroId = r.ReadInt32();
			this.modelIndex = r.ReadInt32();
		}
	}

	// Token: 0x060009A1 RID: 2465 RVA: 0x0007C929 File Offset: 0x0007AB29
	public void HandleRemoving(SkillSystem skillSystem)
	{
		if (this.targetCombatStatId == 0)
		{
			return;
		}
		if (skillSystem.GetCombatStat(ref this.target).id != this.targetCombatStatId)
		{
			return;
		}
		skillSystem.AddCombatStatHPIncoming(ref this.target, this.damageIncoming);
		this.targetCombatStatId = 0;
	}

	// Token: 0x060009A2 RID: 2466 RVA: 0x0007C968 File Offset: 0x0007AB68
	public void HandleSkillTargetRemove(SkillSystem skillSystem)
	{
		HashSet<SkillTarget> removedSkillTargets = skillSystem.removedSkillTargets;
		if (removedSkillTargets.Contains(this.caster))
		{
			this.caster.id = 0;
		}
		if (removedSkillTargets.Contains(this.target))
		{
			this.target.id = 0;
		}
		SkillTarget item;
		item.astroId = this.target.astroId;
		item.id = this.targetCombatStatId;
		item.type = ETargetType.CombatStat;
		if (removedSkillTargets.Contains(item))
		{
			this.targetCombatStatId = 0;
		}
	}

	// Token: 0x060009A3 RID: 2467 RVA: 0x0007C9E8 File Offset: 0x0007ABE8
	public void TickSkillLogic(SkillSystem skillSystem, VFTrailRenderer trailRenderer, SpaceSector sector, ref VectorLF3 relativePos, ref Quaternion relativeRot, long time)
	{
		if (this.life != 0)
		{
			this.life++;
			VectorLF3 vectorLF;
			vectorLF.x = 0.0;
			vectorLF.y = 0.0;
			vectorLF.z = 0.0;
			Vector3 vector;
			vector.x = 0f;
			vector.y = 0f;
			vector.z = 0f;
			Vector3 b;
			b.x = 0f;
			b.y = 0f;
			b.z = 0f;
			int trailStride = trailRenderer.trailStride;
			int num = (int)(GameMain.gameTick & 2147483647L);
			int num2 = num % trailStride;
			ref SmokeData ptr = ref trailRenderer.smokePool[this.id * trailStride + num2];
			ptr.type = (uint)(this.modelIndex - 431);
			ref Vector3 ptr2 = ref ptr.vel;
			if (this.life > 3600 || this.life < -trailStride)
			{
				this.life = -trailStride;
			}
			if (this.target.id > 0)
			{
				if (!skillSystem.GetObjectUPositionAndVelocity(ref this.target, out vectorLF, out vector, out b))
				{
					this.target.id = 0;
				}
				else if (this.target.type == ETargetType.Enemy)
				{
					if (this.target.astroId > 100 && this.target.astroId <= 204899 && this.target.astroId % 100 > 0)
					{
						PlanetFactory planetFactory = skillSystem.astroFactories[this.target.astroId];
						if (planetFactory != null)
						{
							ref EnemyData ptr3 = ref planetFactory.enemyPool[this.target.id];
							if (ptr3.id == this.target.id)
							{
								this.lastTargetCurrentAstroId = ptr3.astroId;
								this.lastTargetPos = ptr3.pos;
							}
						}
					}
					else if (this.target.astroId > 1000000)
					{
						ref EnemyData ptr4 = ref sector.enemyPool[this.target.id];
						if (ptr4.id == this.target.id)
						{
							this.lastTargetCurrentAstroId = ptr4.astroId;
							this.lastTargetPos = ptr4.pos;
						}
					}
				}
			}
			else if (this.life > 0)
			{
				if (this.lastTargetCurrentAstroId > 100 && this.lastTargetCurrentAstroId <= 204899 && this.lastTargetCurrentAstroId % 100 > 0)
				{
					if (((long)this.id + time) % 60L == 0L)
					{
						skillSystem.MissileSearchGroundTarget(ref this, 30f);
					}
				}
				else if (((long)this.id + time) % 30L == 0L)
				{
					skillSystem.MissileSearchSpaceTarget(ref this, 1000f);
				}
				if (this.target.id > 0)
				{
					skillSystem.GetObjectUPositionAndVelocity(ref this.target, out vectorLF, out vector, out b);
					this.damageIncoming = skillSystem.CalculateDamageIncoming(ref this.target, this.damage, 1);
					int num3 = skillSystem.AddCombatStatHPIncoming(ref this.target, -this.damageIncoming);
					this.targetCombatStatId = num3;
				}
				else
				{
					sector.TransformFromAstro_ref(this.lastTargetCurrentAstroId, out vectorLF, ref this.lastTargetPos);
					if (this.lastTargetCurrentAstroId > 1000000)
					{
						sector.astros[this.lastTargetCurrentAstroId - 1000000].VelocityU(ref this.lastTargetPos, out vector);
					}
					else if (this.lastTargetCurrentAstroId >= 100 && this.lastTargetCurrentAstroId <= 204899)
					{
						sector.galaxyAstros[this.lastTargetCurrentAstroId].VelocityU(ref this.lastTargetPos, out vector);
					}
					else
					{
						vector.x = 0f;
						vector.y = 0f;
						vector.z = 0f;
					}
				}
			}
			AstroData[] galaxyAstros = sector.galaxyAstros;
			if (this.life > 0)
			{
				double num4 = vectorLF.x - this.uPos.x;
				double num5 = vectorLF.y - this.uPos.y;
				double num6 = vectorLF.z - this.uPos.z;
				double num7 = Math.Sqrt(num4 * num4 + num5 * num5 + num6 * num6);
				float num8 = (float)(num4 / num7);
				float num9 = (float)(num5 / num7);
				float num10 = (float)(num6 / num7);
				float num11 = this.uVel.x - vector.x;
				float num12 = this.uVel.y - vector.y;
				float num13 = this.uVel.z - vector.z;
				float num14 = Mathf.Sqrt(num11 * num11 + num12 * num12 + num13 * num13);
				float num15 = (float)(num7 / (double)((num14 > 50f) ? num14 : 50f));
				if (this.life > 3 && num15 < 2f)
				{
					num15 -= 0.001f;
					if (num15 < 0.01666666f)
					{
						num15 = 0.01666666f;
					}
					float num16 = 1f - Mathf.Pow(num15 / 2f, 0.06f / num15);
					float num17 = 1f / (num15 * 60f);
					num11 += vector.x * num17;
					num12 += vector.y * num17;
					num13 += vector.z * num17;
					float num18 = num11 * num8 + num12 * num9 + num13 * num10;
					num11 -= num8 * num18;
					num12 -= num9 * num18;
					num13 -= num10 * num18;
					num11 *= num16;
					num12 *= num16;
					num13 *= num16;
					this.uVel.x = this.uVel.x - num11;
					this.uVel.y = this.uVel.y - num12;
					this.uVel.z = this.uVel.z - num13;
				}
				float num19 = this.moveAcc * 8f;
				float num20 = num15;
				if (num20 > 12f)
				{
					num20 = 12f;
				}
				if (num20 < 0.6f)
				{
					num20 = 0.6f;
				}
				if (this.life <= (int)(5f * num20 + 0.5f))
				{
					num20 = 0f;
				}
				else if ((float)this.life < 20f * num20)
				{
					num20 = ((float)this.life - 5f * num20) / (15f * num20);
				}
				else
				{
					num20 = 1f;
				}
				num19 *= num20;
				this.uVel.x = this.uVel.x + num8 * num19;
				this.uVel.y = this.uVel.y + num9 * num19;
				this.uVel.z = this.uVel.z + num10 * num19;
				float num21 = (this.target.astroId > 100 && this.target.astroId <= 204899 && this.target.astroId % 100 > 0) ? 0.02f : 0.0001f;
				float num22 = num21;
				this.vel = this.uVel;
				if (this.nearAstroId > 0)
				{
					if (this.nearAstroId % 100 == 0)
					{
						for (int i = this.nearAstroId + 1; i <= this.nearAstroId + 8; i++)
						{
							AstroData astroData = galaxyAstros[i];
							float uRadius = astroData.uRadius;
							if (uRadius < 1f)
							{
								break;
							}
							VectorLF3 vectorLF2;
							vectorLF2.x = astroData.uPos.x - this.uPos.x;
							vectorLF2.y = astroData.uPos.y - this.uPos.y;
							vectorLF2.z = astroData.uPos.z - this.uPos.z;
							if ((float)Math.Sqrt(vectorLF2.x * vectorLF2.x + vectorLF2.y * vectorLF2.y + vectorLF2.z * vectorLF2.z) < uRadius + 450f)
							{
								this.nearAstroId = i;
								break;
							}
						}
						ptr2 = this.uVel + (this.casterVel - this.uVel) * 0.2f;
					}
					else
					{
						AstroData astroData2 = galaxyAstros[this.nearAstroId];
						float uRadius2 = astroData2.uRadius;
						VectorLF3 vectorLF3;
						vectorLF3.x = astroData2.uPos.x - this.uPos.x;
						vectorLF3.y = astroData2.uPos.y - this.uPos.y;
						vectorLF3.z = astroData2.uPos.z - this.uPos.z;
						float num23 = (float)Math.Sqrt(vectorLF3.x * vectorLF3.x + vectorLF3.y * vectorLF3.y + vectorLF3.z * vectorLF3.z);
						float num24 = num23 - uRadius2;
						if (num24 > 500f)
						{
							this.nearAstroId = this.nearAstroId / 100 * 100;
						}
						else
						{
							if (num24 < -2f)
							{
								this.life = -trailStride;
								int ammoHitIndex = SpaceSector.PrefabDescByModelIndex[this.modelIndex].AmmoHitIndex;
								if (ammoHitIndex > 0)
								{
									ref ParticleData ptr5 = ref skillSystem.hitEffects[ammoHitIndex].Add();
									ptr5.duration = 50;
									ptr5.astroId = this.nearAstroId;
									ptr5.pos = Maths.QInvRotate(astroData2.uRot, -vectorLF3);
									ptr5.dir = ptr5.pos.normalized;
									ptr5.inertia = 0.45f;
									ptr5.size = 1.8f;
								}
								return;
							}
							VectorLF3 vectorLF4 = Maths.QInvRotateLF(astroData2.uRot, -vectorLF3);
							Vector3 vector2;
							astroData2.VelocityU(ref vectorLF4, out vector2);
							this.vel -= vector2;
							ptr2 = vector2;
							if (uRadius2 < 600f)
							{
								double num25 = (double)((500f - num24) / 500f);
								if (num25 < 0.0)
								{
									num25 = -num25;
								}
								num22 += (float)num25 * (0.015f - num21);
							}
							double num26 = (vectorLF3.x * num4 + vectorLF3.y * num5 + vectorLF3.z * num6) / num7;
							float num27 = 150f;
							if (num26 > 0.0 && num7 >= num26 && (double)(num23 * num23) - num26 * num26 < (double)((uRadius2 + 5f) * (uRadius2 + 5f)) && num24 < num27)
							{
								num11 = this.uVel.x - vector.x;
								num12 = this.uVel.y - vector.y;
								num13 = this.uVel.z - vector.z;
								float num28 = (float)((double)num11 * vectorLF3.x + (double)num12 * vectorLF3.y + (double)num13 * vectorLF3.z) / num23;
								if (num28 > 0f)
								{
									float num29 = (float)vectorLF3.x * num28 / num23;
									float num30 = (float)vectorLF3.y * num28 / num23;
									float num31 = (float)vectorLF3.z * num28 / num23;
									float num32 = (num27 - num24) / num27;
									num32 = ((num24 > 2f) ? (num32 * num32 * num32 * num32) : 1f);
									this.uVel.x = this.uVel.x - num29 * num32;
									this.uVel.y = this.uVel.y - num30 * num32;
									this.uVel.z = this.uVel.z - num31 * num32;
								}
							}
						}
					}
				}
				float num33 = this.vel.x * this.vel.x + this.vel.y * this.vel.y + this.vel.z * this.vel.z;
				float num34 = (num33 - 2500f) / num33;
				if (num34 < 0f)
				{
					num34 = 0f;
				}
				this.uVel.x = this.uVel.x - this.vel.x * num22 * num34;
				this.uVel.y = this.uVel.y - this.vel.y * num22 * num34;
				this.uVel.z = this.uVel.z - this.vel.z * num22 * num34;
				Quaternion to = Quaternion.LookRotation(this.vel - this.casterVel);
				this.uRot = Quaternion.RotateTowards(this.uRot, to, 20.000002f);
				if (this.target.id <= 0 && num7 < 2.0)
				{
					this.life = -trailStride;
					if (this.lastTargetCurrentAstroId > 100 && this.lastTargetCurrentAstroId <= 204899 && this.lastTargetCurrentAstroId % 100 > 0)
					{
						PlanetFactory planetFactory2 = skillSystem.astroFactories[this.lastTargetCurrentAstroId];
						if (planetFactory2 != null)
						{
							PrefabDesc prefabDesc = SpaceSector.PrefabDescByModelIndex[this.modelIndex];
							AstroData astroData3 = sector.galaxyAstros[this.lastTargetCurrentAstroId];
							Vector3 vector3 = Maths.QInvRotate(astroData3.uRot, this.uPos - astroData3.uPos);
							Vector3 normalized = this.vel.normalized;
							if (this.lastTargetCurrentAstroId == skillSystem.localAstroId)
							{
								int ammoHitIndex2 = prefabDesc.AmmoHitIndex;
								if (ammoHitIndex2 > 0)
								{
									ref ParticleData ptr6 = ref skillSystem.hitEffects[ammoHitIndex2].Add();
									ptr6.duration = 50;
									ptr6.astroId = this.lastTargetCurrentAstroId;
									ptr6.pos = vector3;
									ptr6.dir = normalized;
									ptr6.inertia = 0.45f;
									ptr6.size = 1.8f;
								}
							}
							if (this.target.type == ETargetType.Enemy)
							{
								SkillTargetLocal skillTargetLocal = default(SkillTargetLocal);
								skillTargetLocal.type = ETargetType.Enemy;
								Vector3 vector4 = Maths.QInvRotate(astroData3.uRot, vectorLF - astroData3.uPos);
								float x = vector4.x;
								float y = vector4.y;
								float z = vector4.z;
								uint num35 = SimpleHash.GenerateHashMask(x, y, z, 1);
								for (int j = 1; j < planetFactory2.enemyCursor; j++)
								{
									ref EnemyData ptr7 = ref planetFactory2.enemyPool[j];
									if (ptr7.id != 0 && !ptr7.isInvincible)
									{
										float num36 = (float)ptr7.pos.x;
										float num37 = (float)ptr7.pos.y;
										float num38 = (float)ptr7.pos.z;
										if (ptr7.dynamic)
										{
											ptr7.hash.InitHashBits(num36, num37, num38);
										}
										uint bits = ptr7.hash.bits;
										if ((bits & num35) == bits)
										{
											float num39 = num36 - x;
											float num40 = num37 - y;
											float num41 = num38 - z;
											float num42 = Mathf.Max(Mathf.Sqrt(num39 * num39 + num40 * num40 + num41 * num41) - 1f, 0f);
											float ammoBlastRadius = prefabDesc.AmmoBlastRadius0;
											float ammoBlastRadius2 = prefabDesc.AmmoBlastRadius1;
											float ammoBlastFalloff = prefabDesc.AmmoBlastFalloff;
											if (num42 < ammoBlastRadius2)
											{
												float num43 = Mathf.Min(Mathf.Pow((ammoBlastRadius2 - num42) / (ammoBlastRadius2 - ammoBlastRadius), ammoBlastFalloff), 1f);
												int num44 = (int)((float)this.damage * num43 + 0.999f);
												if (num44 < 1)
												{
													num44 = 1;
												}
												if (this.caster.astroId == this.lastTargetCurrentAstroId)
												{
													skillTargetLocal.id = ptr7.id;
													SkillTargetLocal skillTargetLocal2 = default(SkillTargetLocal);
													skillTargetLocal2.id = this.caster.id;
													skillTargetLocal2.type = this.caster.type;
													ref CombatStat ptr8 = ref skillSystem.DamageGroundObjectByLocalCaster(planetFactory2, num44, 1, ref skillTargetLocal, ref skillTargetLocal2);
													ptr8.lastImpact.mass = 18f;
													ptr8.lastImpact.point = vector3;
													ptr8.lastImpact.velocity = 32f * num43 * normalized;
												}
												else
												{
													SkillTarget skillTarget = default(SkillTarget);
													skillTarget.id = ptr7.id;
													skillTarget.astroId = ptr7.originAstroId;
													skillTarget.type = ETargetType.Enemy;
													ref CombatStat ptr9 = ref skillSystem.DamageObject(this.damage, 1, ref skillTarget, ref this.caster);
													ptr9.lastImpact.mass = 18f;
													ptr9.lastImpact.point = vector3;
													ptr9.lastImpact.velocity = 32f * num43 * normalized;
												}
											}
										}
									}
								}
							}
						}
					}
					else
					{
						int ammoHitIndex3 = SpaceSector.PrefabDescByModelIndex[this.modelIndex].AmmoHitIndex;
						if (ammoHitIndex3 > 0)
						{
							ref ParticleData ptr10 = ref skillSystem.hitEffects[ammoHitIndex3].Add();
							ptr10.duration = 50;
							this.lastTargetCurrentAstroId = sector.enemyPool[this.target.id].astroId;
							if (this.lastTargetCurrentAstroId < 1000000)
							{
								ptr10.astroId = this.lastTargetCurrentAstroId;
								VectorLF3 vec;
								sector.InverseTransformToAstro(this.lastTargetCurrentAstroId, this.uPos, out vec);
								ptr10.pos = vec;
								ptr10.dir = vec.normalized;
								skillSystem.GetObjectLVelocity(ref this.target, out ptr10.vel);
								ptr10.inertia = 0.45f;
								ptr10.size = 1.8f;
							}
							else
							{
								ptr10.astroId = 0;
								ptr10.upos = this.uPos;
								ptr10.CalculatePosFromUPos(ref relativePos, ref relativeRot);
								ptr10.dir = this.uPos.normalized;
								ptr10.vel = vector + b;
								ptr10.inertia = 1f;
								ptr10.size = 6f;
							}
						}
					}
				}
			}
			else
			{
				this.vel = this.uVel;
				if (this.nearAstroId > 100 && this.nearAstroId <= 204899 && this.nearAstroId % 100 > 0)
				{
					AstroData astroData4 = sector.galaxyAstros[this.nearAstroId];
					VectorLF3 vec2;
					vec2.x = astroData4.uPos.x - this.uPos.x;
					vec2.y = astroData4.uPos.y - this.uPos.y;
					vec2.z = astroData4.uPos.z - this.uPos.z;
					VectorLF3 v = Maths.QInvRotateLF(astroData4.uRot, -vec2);
					Vector3 b2 = (Maths.QRotateLF(astroData4.uRotNext, v) + astroData4.uPosNext - this.uPos) * 60.0;
					this.vel -= b2;
				}
			}
			this.vel = Maths.QInvRotate(relativeRot, this.vel);
			if (this.life > 0)
			{
				int astroId = this.target.astroId;
				bool flag = astroId > 100 && astroId <= 204899 && astroId % 100 > 0;
				bool flag2 = astroId > 1000000;
				bool flag3 = (this.mask & ETargetTypeMask.Player) > ETargetTypeMask.None;
				bool flag4 = false;
				if (flag3)
				{
					Mecha mecha = skillSystem.mecha;
					VectorLF3 lhs = this.uVel.normalized;
					double rhs = (double)this.uVel.magnitude * 0.016666666666666666;
					VectorLF3 rhs2 = this.uPos + (skillSystem.playerSkillTargetU - skillSystem.playerSkillTargetULast);
					VectorLF3 vectorLF5 = this.uPos + lhs * rhs - rhs2;
					double num45 = Math.Sqrt(vectorLF5.x * vectorLF5.x + vectorLF5.y * vectorLF5.y + vectorLF5.z * vectorLF5.z);
					vectorLF5.x /= num45;
					vectorLF5.y /= num45;
					vectorLF5.z /= num45;
					RCHCPULF rchcpulf;
					if (mecha.energyShieldEnergy >= mecha.energyShieldEnergyRate && Phys.RayCastSphereLF(ref rhs2, ref vectorLF5, num45, ref skillSystem.playerSkillTargetU, (double)skillSystem.playerEnergyShieldRadius, out rchcpulf))
					{
						float num46 = (float)this.damage / 4500f;
						if (num46 > 2f)
						{
							num46 = 2f;
						}
						else if (num46 < 1f)
						{
							num46 = 1f;
						}
						num46 *= 2f;
						if (skillSystem.MechaEnergyShieldResist(this.caster, ref this.damage))
						{
							ref ParticleData ptr11 = ref skillSystem.hitEffects[8].Add();
							ptr11.duration = (int)(45f * (1f + num46) * 0.5f);
							ptr11.astroId = 0;
							ptr11.upos = rchcpulf.point;
							ptr11.CalculatePosFromUPos(ref relativePos, ref relativeRot);
							ptr11.dir = rchcpulf.normal;
							ptr11.vel = skillSystem.playerVelocityU;
							ptr11.inertia = 1f;
							ptr11.size = num46;
						}
						int ammoHitIndex4 = SpaceSector.PrefabDescByModelIndex[this.modelIndex].AmmoHitIndex;
						if (ammoHitIndex4 > 0)
						{
							ref ParticleData ptr12 = ref skillSystem.hitEffects[ammoHitIndex4].Add();
							ptr12.duration = 50;
							ptr12.astroId = 0;
							ptr12.upos = rchcpulf.point;
							ptr12.CalculatePosFromUPos(ref relativePos, ref relativeRot);
							ptr12.dir = ptr12.pos.normalized;
							ptr12.vel = skillSystem.playerVelocityU;
							ptr12.size = 1.2f;
							if (skillSystem.localPlanetAstroId > 0)
							{
								ptr12.inertia = Mathf.Clamp01(0.78f + skillSystem.playerAltL * 0.001f);
							}
							else
							{
								ptr12.inertia = 1f;
							}
						}
						if (this.damage == 0)
						{
							this.life = -trailStride;
							return;
						}
					}
				}
				if (flag)
				{
					float num47;
					int num48;
					int k;
					if (this.target.type == ETargetType.Enemy)
					{
						ref EnemyData ptr13 = ref skillSystem.astroFactories[astroId].enemyPool[this.target.id];
						num47 = SkillSystem.RoughRadiusByModelIndex[(int)ptr13.modelIndex] + 0.5f;
						num48 = SkillSystem.ColliderComplexityByModelIndex[(int)ptr13.modelIndex];
						k = ptr13.colliderId;
					}
					else
					{
						num47 = 0f;
						num48 = 0;
						k = 0;
					}
					if (num47 > 0f)
					{
						PlanetFactory planetFactory3 = skillSystem.astroFactories[astroId];
						PlanetPhysics planetPhysics = (planetFactory3 != null) ? planetFactory3.planet.physics : null;
						if (num48 == 0 || planetPhysics == null || k == 0)
						{
							float num49 = Mathf.Sqrt(this.uVel.x * this.uVel.x + this.uVel.y * this.uVel.y + this.uVel.z * this.uVel.z);
							num49 = ((num49 > 0.001f) ? num49 : 0.001f);
							VectorLF3 vectorLF6 = new VectorLF3(this.uVel.x / num49, this.uVel.y / num49, this.uVel.z / num49);
							double length = (double)num49 * 0.016666666666666666;
							RCHCPULF rchcpulf2;
							if (Phys.RayCastSphereLF(ref this.uPos, ref vectorLF6, length, ref vectorLF, (double)num47, out rchcpulf2))
							{
								length = rchcpulf2.dist;
								flag4 = true;
							}
						}
						else if (num48 == 1)
						{
							AstroData astroData5 = sector.galaxyAstros[astroId];
							Vector3 vector5 = Maths.QInvRotate(astroData5.uRot, this.uPos - astroData5.uPos);
							Vector3 normalized2 = this.vel.normalized;
							float length2 = this.vel.magnitude * 0.016666668f;
							ref ColliderData colliderDataRef = ref planetPhysics.GetColliderDataRef(k);
							RCHCPU rchcpu;
							if (Phys.RayCastSphere(ref vector5, ref normalized2, length2, ref colliderDataRef.pos, num47, out rchcpu))
							{
								length2 = rchcpu.dist;
								flag4 = true;
							}
						}
						else
						{
							float num50 = Mathf.Sqrt(this.uVel.x * this.uVel.x + this.uVel.y * this.uVel.y + this.uVel.z * this.uVel.z);
							num50 = ((num50 > 0.001f) ? num50 : 0.001f);
							VectorLF3 vectorLF7 = new VectorLF3(this.uVel.x / num50, this.uVel.y / num50, this.uVel.z / num50);
							double num51 = (double)num50 * 0.016666666666666666;
							if (Phys.RayCastSphereLF(ref this.uPos, ref vectorLF7, num51, ref vectorLF, (double)num47))
							{
								RCHCPULF rchcpulf3;
								if (Phys.RayCastSphereLF(ref this.uPos, ref vectorLF7, num51, ref vectorLF, 0.699999988079071, out rchcpulf3))
								{
									num51 = rchcpulf3.dist;
									flag4 = true;
								}
								else
								{
									AstroData astroData6 = sector.galaxyAstros[astroId];
									Vector3 origin = Maths.QInvRotate(astroData6.uRot, this.uPos - astroData6.uPos);
									Vector3 normalized3 = this.vel.normalized;
									num51 = (double)this.vel.magnitude * 0.016666666666666666;
									while (k > 0)
									{
										ref ColliderData colliderDataRef2 = ref planetPhysics.GetColliderDataRef(k);
										if (colliderDataRef2.notForBuild || colliderDataRef2.link == 0)
										{
											RCHCPU rchcpu2;
											if (colliderDataRef2.shape == EColliderShape.Box)
											{
												if (Phys.RayCastOBB(origin, normalized3, (float)num51, colliderDataRef2.ext, colliderDataRef2.pos, colliderDataRef2.q, out rchcpu2))
												{
													num51 = (double)rchcpu2.dist;
													flag4 = true;
													break;
												}
											}
											else if (colliderDataRef2.shape == EColliderShape.Sphere)
											{
												if (Phys.RayCastSphere(origin, normalized3, (float)num51, colliderDataRef2.pos, colliderDataRef2.radius, out rchcpu2))
												{
													num51 = (double)rchcpu2.dist;
													flag4 = true;
													break;
												}
											}
											else if (colliderDataRef2.shape == EColliderShape.Capsule && Phys.RayCastCapsule(origin, normalized3, (float)num51, colliderDataRef2.pos - colliderDataRef2.ext, colliderDataRef2.pos + colliderDataRef2.ext, colliderDataRef2.radius, out rchcpu2))
											{
												num51 = (double)rchcpu2.dist;
												flag4 = true;
												break;
											}
										}
										k = colliderDataRef2.link;
									}
								}
							}
						}
					}
					this.uPos.x = this.uPos.x + (double)this.uVel.x * 0.016666666666666666;
					this.uPos.y = this.uPos.y + (double)this.uVel.y * 0.016666666666666666;
					this.uPos.z = this.uPos.z + (double)this.uVel.z * 0.016666666666666666;
					if (flag4)
					{
						this.life = -trailStride;
						PlanetFactory planetFactory4 = skillSystem.astroFactories[astroId];
						if (planetFactory4 != null)
						{
							PrefabDesc prefabDesc2 = SpaceSector.PrefabDescByModelIndex[this.modelIndex];
							AstroData astroData7 = sector.galaxyAstros[astroId];
							Vector3 vector6 = Maths.QInvRotate(astroData7.uRot, this.uPos - astroData7.uPos);
							Vector3 normalized4 = this.vel.normalized;
							if (astroId == skillSystem.localAstroId)
							{
								int ammoHitIndex5 = prefabDesc2.AmmoHitIndex;
								if (ammoHitIndex5 > 0)
								{
									ref ParticleData ptr14 = ref skillSystem.hitEffects[ammoHitIndex5].Add();
									ptr14.duration = 50;
									ptr14.astroId = astroId;
									ptr14.pos = vector6;
									ptr14.dir = normalized4;
									skillSystem.GetObjectLVelocity(ref this.target, out ptr14.vel);
									ptr14.inertia = 0.45f;
									ptr14.size = 1.8f;
								}
							}
							if (this.target.type == ETargetType.Enemy)
							{
								SkillTargetLocal skillTargetLocal3 = default(SkillTargetLocal);
								skillTargetLocal3.type = ETargetType.Enemy;
								Vector3 vector7 = Maths.QInvRotate(astroData7.uRot, vectorLF - astroData7.uPos);
								float x2 = vector7.x;
								float y2 = vector7.y;
								float z2 = vector7.z;
								uint num52 = SimpleHash.GenerateHashMask(x2, y2, z2, 1);
								for (int l = 1; l < planetFactory4.enemyCursor; l++)
								{
									ref EnemyData ptr15 = ref planetFactory4.enemyPool[l];
									if (ptr15.id != 0 && !ptr15.isInvincible)
									{
										float num53 = (float)ptr15.pos.x;
										float num54 = (float)ptr15.pos.y;
										float num55 = (float)ptr15.pos.z;
										if (ptr15.dynamic)
										{
											ptr15.hash.InitHashBits(num53, num54, num55);
										}
										uint bits2 = ptr15.hash.bits;
										if ((bits2 & num52) == bits2)
										{
											float num56 = num53 - x2;
											float num57 = num54 - y2;
											float num58 = num55 - z2;
											float num59 = Mathf.Max(Mathf.Sqrt(num56 * num56 + num57 * num57 + num58 * num58) - 1f, 0f);
											float ammoBlastRadius3 = prefabDesc2.AmmoBlastRadius0;
											float ammoBlastRadius4 = prefabDesc2.AmmoBlastRadius1;
											float ammoBlastFalloff2 = prefabDesc2.AmmoBlastFalloff;
											if (num59 < ammoBlastRadius4)
											{
												float num60 = Mathf.Min(Mathf.Pow((ammoBlastRadius4 - num59) / (ammoBlastRadius4 - ammoBlastRadius3), ammoBlastFalloff2), 1f);
												int num61 = (int)((float)this.damage * num60 + 0.999f);
												if (num61 < 1)
												{
													num61 = 1;
												}
												if (this.caster.astroId == this.lastTargetCurrentAstroId)
												{
													skillTargetLocal3.id = ptr15.id;
													SkillTargetLocal skillTargetLocal4 = default(SkillTargetLocal);
													skillTargetLocal4.id = this.caster.id;
													skillTargetLocal4.type = this.caster.type;
													ref CombatStat ptr16 = ref skillSystem.DamageGroundObjectByLocalCaster(planetFactory4, num61, 1, ref skillTargetLocal3, ref skillTargetLocal4);
													ptr16.lastImpact.mass = 18f;
													ptr16.lastImpact.point = vector6;
													ptr16.lastImpact.velocity = 32f * num60 * normalized4;
												}
												else
												{
													SkillTarget skillTarget2 = default(SkillTarget);
													skillTarget2.id = ptr15.id;
													skillTarget2.astroId = ptr15.originAstroId;
													skillTarget2.type = ETargetType.Enemy;
													ref CombatStat ptr17 = ref skillSystem.DamageObject(this.damage, 1, ref skillTarget2, ref this.caster);
													ptr17.lastImpact.mass = 18f;
													ptr17.lastImpact.point = vector6;
													ptr17.lastImpact.velocity = 32f * num60 * normalized4;
												}
											}
										}
									}
								}
								this.HandleRemoving(skillSystem);
							}
						}
					}
				}
				else if (flag2)
				{
					float num62;
					int num63;
					int m;
					if (this.target.type == ETargetType.Enemy)
					{
						ref EnemyData ptr18 = ref sector.enemyPool[this.target.id];
						num62 = SkillSystem.RoughRadiusByModelIndex[(int)ptr18.modelIndex];
						num63 = SkillSystem.ColliderComplexityByModelIndex[(int)ptr18.modelIndex];
						m = ptr18.colliderId;
					}
					else
					{
						num62 = 0f;
						num63 = 0;
						m = 0;
					}
					if (num62 > 0f)
					{
						SectorPhysics physics = sector.physics;
						if (num63 == 0 || m == 0)
						{
							float num64 = Mathf.Sqrt(this.uVel.x * this.uVel.x + this.uVel.y * this.uVel.y + this.uVel.z * this.uVel.z);
							num64 = ((num64 > 0.001f) ? num64 : 0.001f);
							VectorLF3 vectorLF8 = new VectorLF3(this.uVel.x / num64, this.uVel.y / num64, this.uVel.z / num64);
							double length3 = (double)num64 * 0.016666666666666666;
							RCHCPULF rchcpulf4;
							if (Phys.RayCastSphereLF(ref this.uPos, ref vectorLF8, length3, ref vectorLF, (double)num62, out rchcpulf4))
							{
								length3 = rchcpulf4.dist;
								flag4 = true;
							}
						}
						else if (num63 == 1)
						{
							if (this.target.type == ETargetType.Enemy)
							{
								astroId = sector.enemyPool[this.target.id].astroId;
							}
							if (astroId > 1000000)
							{
								AstroData astroData8 = sector.astros[astroId - 1000000];
								VectorLF3 vectorLF9 = Maths.QInvRotate(astroData8.uRot, this.uPos - astroData8.uPos);
								Vector3 b3;
								astroData8.VelocityU(ref vectorLF9, out b3);
								VectorLF3 vectorLF10 = Maths.QInvRotate(astroData8.uRot, this.uVel - b3).normalized;
								double length4 = (double)(this.uVel - b3).magnitude * 0.016666666666666666;
								ref ColliderDataLF colliderDataRef3 = ref physics.GetColliderDataRef(m);
								if (colliderDataRef3.idType != 0)
								{
									RCHCPULF rchcpulf5;
									if (Phys.RayCastSphereLF(ref vectorLF9, ref vectorLF10, length4, ref colliderDataRef3.pos, (double)colliderDataRef3.radius, out rchcpulf5))
									{
										length4 = rchcpulf5.dist;
										flag4 = true;
									}
								}
								else
								{
									Assert.CannotBeReached();
								}
							}
							else if (astroId > 100 && astroId <= 204899)
							{
								AstroData astroData9 = sector.galaxyAstros[astroId];
								VectorLF3 vectorLF11 = Maths.QInvRotate(astroData9.uRot, this.uPos - astroData9.uPos);
								VectorLF3 vectorLF12 = this.vel.normalized;
								double length5 = (double)this.vel.magnitude * 0.016666666666666666;
								ref ColliderDataLF colliderDataRef4 = ref physics.GetColliderDataRef(m);
								if (colliderDataRef4.idType != 0)
								{
									RCHCPULF rchcpulf6;
									if (Phys.RayCastSphereLF(ref vectorLF11, ref vectorLF12, length5, ref colliderDataRef4.pos, (double)colliderDataRef4.radius, out rchcpulf6))
									{
										length5 = rchcpulf6.dist;
										flag4 = true;
									}
								}
								else
								{
									Assert.CannotBeReached();
								}
							}
						}
						else
						{
							float num65 = Mathf.Sqrt(this.uVel.x * this.uVel.x + this.uVel.y * this.uVel.y + this.uVel.z * this.uVel.z);
							num65 = ((num65 > 0.001f) ? num65 : 0.001f);
							VectorLF3 vectorLF13 = new VectorLF3(this.uVel.x / num65, this.uVel.y / num65, this.uVel.z / num65);
							double length6 = (double)num65 * 0.016666666666666666;
							if (Phys.RayCastSphereLF(ref this.uPos, ref vectorLF13, length6, ref vectorLF, (double)num62))
							{
								RCHCPULF rchcpulf7;
								if (Phys.RayCastSphereLF(ref this.uPos, ref vectorLF13, length6, ref vectorLF, 0.699999988079071, out rchcpulf7))
								{
									length6 = rchcpulf7.dist;
									flag4 = true;
								}
								else
								{
									if (this.target.type == ETargetType.Enemy)
									{
										astroId = sector.enemyPool[this.target.id].astroId;
									}
									Vector3 origin2;
									Vector3 dir;
									float length7;
									if (astroId > 1000000)
									{
										AstroData astroData10 = sector.astros[astroId - 1000000];
										origin2 = Maths.QInvRotate(astroData10.uRot, this.uPos - astroData10.uPos);
										Vector3 b4;
										astroData10.VelocityU(ref origin2, out b4);
										dir = Maths.QInvRotate(astroData10.uRot, this.uVel - b4).normalized;
										length7 = (this.uVel - b4).magnitude * 0.016666668f;
									}
									else if (astroId > 100 && astroId <= 204899)
									{
										AstroData astroData11 = sector.galaxyAstros[astroId];
										origin2 = Maths.QInvRotate(astroData11.uRot, this.uPos - astroData11.uPos);
										dir = this.vel.normalized;
										length7 = this.vel.magnitude * 0.016666668f;
									}
									else
									{
										Assert.CannotBeReached();
										origin2 = Vector3.zero;
										dir = Vector3.one;
										length7 = 0f;
									}
									while (m > 0)
									{
										ref ColliderDataLF colliderDataRef5 = ref physics.GetColliderDataRef(m);
										if (colliderDataRef5.idType != 0)
										{
											if (colliderDataRef5.notForBuild || colliderDataRef5.link == 0)
											{
												RCHCPU rchcpu3;
												if (colliderDataRef5.shape == EColliderShape.Box)
												{
													if (Phys.RayCastOBB(origin2, dir, length7, colliderDataRef5.ext, colliderDataRef5.pos, colliderDataRef5.q, out rchcpu3))
													{
														length7 = rchcpu3.dist;
														flag4 = true;
														break;
													}
												}
												else if (colliderDataRef5.shape == EColliderShape.Sphere)
												{
													if (Phys.RayCastSphere(origin2, dir, length7, colliderDataRef5.pos, colliderDataRef5.radius, out rchcpu3))
													{
														length7 = rchcpu3.dist;
														flag4 = true;
														break;
													}
												}
												else if (colliderDataRef5.shape == EColliderShape.Capsule && Phys.RayCastCapsule(origin2, dir, length7, colliderDataRef5.pos - colliderDataRef5.ext, colliderDataRef5.pos + colliderDataRef5.ext, colliderDataRef5.radius, out rchcpu3))
												{
													length7 = rchcpu3.dist;
													flag4 = true;
													break;
												}
											}
										}
										else
										{
											Assert.CannotBeReached();
										}
										m = colliderDataRef5.link;
									}
								}
							}
						}
					}
					this.uPos.x = this.uPos.x + (double)this.uVel.x * 0.016666666666666666;
					this.uPos.y = this.uPos.y + (double)this.uVel.y * 0.016666666666666666;
					this.uPos.z = this.uPos.z + (double)this.uVel.z * 0.016666666666666666;
					if (flag4)
					{
						this.life = -trailStride;
						int ammoHitIndex6 = SpaceSector.PrefabDescByModelIndex[this.modelIndex].AmmoHitIndex;
						if (ammoHitIndex6 > 0)
						{
							ref ParticleData ptr19 = ref skillSystem.hitEffects[ammoHitIndex6].Add();
							ptr19.duration = 50;
							astroId = sector.enemyPool[this.target.id].astroId;
							if (astroId < 1000000)
							{
								ptr19.astroId = astroId;
								VectorLF3 vec3;
								sector.InverseTransformToAstro(astroId, this.uPos, out vec3);
								ptr19.pos = vec3;
								ptr19.dir = vec3.normalized;
								skillSystem.GetObjectLVelocity(ref this.target, out ptr19.vel);
								ptr19.inertia = 0.45f;
								ptr19.size = 1.8f;
							}
							else
							{
								ptr19.astroId = 0;
								ptr19.upos = this.uPos;
								ptr19.CalculatePosFromUPos(ref relativePos, ref relativeRot);
								ptr19.dir = this.uPos.normalized;
								ptr19.vel = vector + b;
								ptr19.inertia = 1f;
								ptr19.size = 6f;
							}
						}
						skillSystem.DamageObject(this.damage, 1, ref this.target, ref this.caster);
						this.HandleRemoving(skillSystem);
					}
				}
				else if (this.target.type == ETargetType.Player)
				{
					VectorLF3 lhs2 = this.uVel.normalized;
					double rhs3 = (double)this.uVel.magnitude * 0.016666666666666666;
					Player mainPlayer = GameMain.mainPlayer;
					VectorLF3 vectorLF14 = this.uPos + (skillSystem.playerSkillTargetU - skillSystem.playerSkillTargetULast);
					VectorLF3 vectorLF15 = this.uPos + lhs2 * rhs3 - vectorLF14;
					float num66 = (float)Math.Sqrt(vectorLF15.x * vectorLF15.x + vectorLF15.y * vectorLF15.y + vectorLF15.z * vectorLF15.z);
					vectorLF15.x /= (double)num66;
					vectorLF15.y /= (double)num66;
					vectorLF15.z /= (double)num66;
					Vector3 origin3 = Maths.QInvRotate(relativeRot, vectorLF14 - relativePos);
					Vector3 dir2 = Maths.QInvRotate(relativeRot, vectorLF15);
					ref ColliderData ptr20 = ref skillSystem.playerSkillColliderL;
					RCHCPU rchcpu4;
					if (Phys.RayCastCapsule(origin3, dir2, num66, ptr20.pos - ptr20.ext, ptr20.pos + ptr20.ext, ptr20.radius, out rchcpu4))
					{
						rhs3 = (double)rchcpu4.dist;
						flag4 = true;
					}
					this.uPos.x = this.uPos.x + (double)this.uVel.x * 0.016666666666666666;
					this.uPos.y = this.uPos.y + (double)this.uVel.y * 0.016666666666666666;
					this.uPos.z = this.uPos.z + (double)this.uVel.z * 0.016666666666666666;
					if (flag4)
					{
						this.life = -trailStride;
						int ammoHitIndex7 = SpaceSector.PrefabDescByModelIndex[this.modelIndex].AmmoHitIndex;
						if (ammoHitIndex7 > 0)
						{
							ref ParticleData ptr21 = ref skillSystem.hitEffects[ammoHitIndex7].Add();
							ptr21.duration = 50;
							ptr21.vel = skillSystem.playerVelocityL;
							ptr21.size = 1.3f;
							ptr21.astroId = skillSystem.localPlanetOrStarAstroId;
							bool localPlanet = GameMain.localPlanet != null;
							StarData localStar = GameMain.localStar;
							if (localPlanet)
							{
								ptr21.pos = rchcpu4.point;
								ptr21.dir = rchcpu4.normal;
								ptr21.inertia = Mathf.Clamp01(0.78f + skillSystem.playerAltL * 0.001f);
							}
							else if (localStar != null)
							{
								ptr21.pos = mainPlayer.uPosition - localStar.uPosition + Maths.QRotate(mainPlayer.uRotation, rchcpu4.point);
								ptr21.dir = Maths.QRotate(mainPlayer.uRotation, rchcpu4.normal);
								ptr21.inertia = 1f;
							}
							else
							{
								ptr21.upos = mainPlayer.uPosition + Maths.QRotate(mainPlayer.uRotation, rchcpu4.point);
								ptr21.CalculatePosFromUPos(ref relativePos, ref relativeRot);
								ptr21.inertia = 1f;
							}
						}
						skillSystem.DamageObject(this.damage, 1, ref this.target, ref this.caster);
						this.HandleRemoving(skillSystem);
					}
				}
				if (this.nearAstroId > 0)
				{
					int num67 = this.nearAstroId / 100 * 100;
					ref VectorLF3 ptr22 = ref galaxyAstros[num67].uPos;
					ptr.createTime = (uint)num;
					ptr.vel = ptr2;
					ptr.astroId = num67;
					ptr.pos.x = (float)(this.uPos.x - ptr22.x);
					ptr.pos.y = (float)(this.uPos.y - ptr22.y);
					ptr.pos.z = (float)(this.uPos.z - ptr22.z);
					return;
				}
			}
			else
			{
				this.uPos.x = this.uPos.x + (double)this.uVel.x * 0.016666666666666666;
				this.uPos.y = this.uPos.y + (double)this.uVel.y * 0.016666666666666666;
				this.uPos.z = this.uPos.z + (double)this.uVel.z * 0.016666666666666666;
			}
			return;
		}
	}

	// Token: 0x04000A8D RID: 2701
	public int id;

	// Token: 0x04000A8E RID: 2702
	public int nearAstroId;

	// Token: 0x04000A8F RID: 2703
	public int targetCombatStatId;

	// Token: 0x04000A90 RID: 2704
	public VectorLF3 uPos;

	// Token: 0x04000A91 RID: 2705
	public VectorLF3 lastTargetPos;

	// Token: 0x04000A92 RID: 2706
	public int lastTargetCurrentAstroId;

	// Token: 0x04000A93 RID: 2707
	public Quaternion uRot;

	// Token: 0x04000A94 RID: 2708
	public Vector3 uVel;

	// Token: 0x04000A95 RID: 2709
	public Vector3 vel;

	// Token: 0x04000A96 RID: 2710
	public Vector3 casterVel;

	// Token: 0x04000A97 RID: 2711
	public float moveAcc;

	// Token: 0x04000A98 RID: 2712
	public float turnAcc;

	// Token: 0x04000A99 RID: 2713
	public int damage;

	// Token: 0x04000A9A RID: 2714
	public int damageIncoming;

	// Token: 0x04000A9B RID: 2715
	public ETargetTypeMask mask;

	// Token: 0x04000A9C RID: 2716
	public int life;

	// Token: 0x04000A9D RID: 2717
	public SkillTarget target;

	// Token: 0x04000A9E RID: 2718
	public SkillTarget caster;

	// Token: 0x04000A9F RID: 2719
	public int modelIndex;

	// Token: 0x04000AA0 RID: 2720
	public const int MISSILE_MAX_UVEL_MAG_I = 180;

	// Token: 0x04000AA1 RID: 2721
	public const int MISSILE_MAX_UVEL_MAG_II = 350;

	// Token: 0x04000AA2 RID: 2722
	public const int MISSILE_MAX_UVEL_MAG_III = 400;

	// Token: 0x04000AA3 RID: 2723
	private const double TICK_DELTA_TIME = 0.016666666666666666;

	// Token: 0x04000AA4 RID: 2724
	private const float UNIT_MOVE_ACC = 8f;

	// Token: 0x04000AA5 RID: 2725
	private const float UNIT_TURN_ACC = 12f;
}
