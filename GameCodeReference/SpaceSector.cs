using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

// Token: 0x0200024D RID: 589
public class SpaceSector
{
	// Token: 0x1700038A RID: 906
	// (get) Token: 0x060019A7 RID: 6567 RVA: 0x001C49DF File Offset: 0x001C2BDF
	public int enemyCount
	{
		get
		{
			return this.enemyCursor - this.enemyRecycleCursor - 1;
		}
	}

	// Token: 0x1700038B RID: 907
	// (get) Token: 0x060019A8 RID: 6568 RVA: 0x001C49F0 File Offset: 0x001C2BF0
	public int craftCount
	{
		get
		{
			return this.craftCursor - this.craftRecycleCursor - 1;
		}
	}

	// Token: 0x1700038C RID: 908
	// (get) Token: 0x060019A9 RID: 6569 RVA: 0x001C4A01 File Offset: 0x001C2C01
	public int maxHiveCount
	{
		get
		{
			return this.galaxy.starCount * 8 + 2;
		}
	}

	// Token: 0x060019AA RID: 6570 RVA: 0x001C4A14 File Offset: 0x001C2C14
	public void Init(GameData _gameData)
	{
		this.gameData = _gameData;
		this.galaxy = this.gameData.galaxy;
		this.isCombatMode = this.gameData.gameDesc.isCombatMode;
		this.galaxyAstros = this.galaxy.astrosData;
		this.spaceRuins = new DataPool<SpaceRuinData>();
		this.spaceRuins.Reset();
		this.physics = new SectorPhysics(this);
		this.physics.Init();
		this.audio = new SectorAudio(this);
		this.audio.Init();
		this.skillSystem = new SkillSystem(this);
		this.skillSystem.Init();
		this.dfHivesByAstro = new EnemyDFHiveSystem[this.maxHiveCount];
		this.creationSystem = new CreationSystem(this.gameData);
		this.creationSystem.Init();
	}

	// Token: 0x060019AB RID: 6571 RVA: 0x001C4AE8 File Offset: 0x001C2CE8
	public void SetForNewGame()
	{
		this.SetAstroCapacity(1024);
		this.astroCursor = 1;
		for (int i = 0; i < this.maxHiveCount; i++)
		{
			this.NewAstroData(new AstroData
			{
				type = EAstroType.EnemyHive
			});
		}
		this.SetEnemyCapacity(this.isCombatMode ? 16384 : 32);
		this.enemyCursor = 1;
		this.enemyRecycleCursor = 0;
		this.SetCraftCapacity(64);
		this.craftCursor = 1;
		this.craftRecycleCursor = 0;
		this.spaceRuins.Reset();
		this.skillSystem.SetForNewGame();
		int starCount = this.galaxy.starCount;
		this.dfHives = new EnemyDFHiveSystem[starCount];
		if (this.isCombatMode)
		{
			HighStopwatch highStopwatch = new HighStopwatch();
			highStopwatch.Begin();
			for (int j = 0; j < starCount; j++)
			{
				this.dfHives[j] = null;
				int num = this.galaxy.stars[j].initialHiveCount;
				if (j >= 100)
				{
					num = 0;
				}
				EnemyDFHiveSystem enemyDFHiveSystem = null;
				for (int k = 0; k < num; k++)
				{
					EnemyDFHiveSystem enemyDFHiveSystem2 = new EnemyDFHiveSystem();
					enemyDFHiveSystem2.Init(this.gameData, this.galaxy.stars[j].id, k);
					if (enemyDFHiveSystem == null)
					{
						this.dfHives[j] = enemyDFHiveSystem2;
					}
					else
					{
						enemyDFHiveSystem.nextSibling = enemyDFHiveSystem2;
					}
					enemyDFHiveSystem2.prevSibling = enemyDFHiveSystem;
					enemyDFHiveSystem = enemyDFHiveSystem2;
					enemyDFHiveSystem2.SetForNewGame();
				}
			}
			double duration = highStopwatch.duration;
			Debug.Log(string.Format("Initialize and generate space enemy complete, time cost = {0:0.00} ms", duration * 1000.0));
		}
		this.combatSpaceSystem = new CombatSpaceSystem(this);
		this.creationSystem.SetForNewGame();
	}

	// Token: 0x060019AC RID: 6572 RVA: 0x001C4C9C File Offset: 0x001C2E9C
	public void Free()
	{
		this.galaxyAstros = null;
		this.astros = null;
		this.enemyPool = null;
		this.enemyAnimPool = null;
		this.enemyRecycle = null;
		this.craftPool = null;
		this.craftAnimPool = null;
		this.craftRecycle = null;
		if (this.spaceRuins != null)
		{
			this.spaceRuins.Free();
			this.spaceRuins = null;
		}
		if (this.physics != null)
		{
			this.physics.Free();
			this.physics = null;
		}
		if (this.audio != null)
		{
			this.audio.Free();
			this.audio = null;
		}
		if (this.skillSystem != null)
		{
			this.skillSystem.Free();
			this.skillSystem = null;
		}
		if (this.dfHives != null)
		{
			for (int i = 0; i < this.dfHives.Length; i++)
			{
				EnemyDFHiveSystem enemyDFHiveSystem = this.dfHives[i];
				this.dfHives[i] = null;
				while (enemyDFHiveSystem != null)
				{
					EnemyDFHiveSystem nextSibling = enemyDFHiveSystem.nextSibling;
					enemyDFHiveSystem.Free();
					enemyDFHiveSystem = nextSibling;
				}
			}
			this.dfHives = null;
		}
		if (this.combatSpaceSystem != null)
		{
			this.combatSpaceSystem.Free();
			this.combatSpaceSystem = null;
		}
		if (this.creationSystem != null)
		{
			this.creationSystem.Free();
			this.creationSystem = null;
		}
		this.gameData = null;
	}

	// Token: 0x060019AD RID: 6573 RVA: 0x001C4DCC File Offset: 0x001C2FCC
	public void OnDFAggressivenessChanged(EAggressiveLevel oldAgglv, EAggressiveLevel newAgglv)
	{
		if (this.dfHives == null)
		{
			return;
		}
		for (int i = 0; i < this.dfHives.Length; i++)
		{
			for (EnemyDFHiveSystem enemyDFHiveSystem = this.dfHives[i]; enemyDFHiveSystem != null; enemyDFHiveSystem = enemyDFHiveSystem.nextSibling)
			{
				float num = (float)((double)enemyDFHiveSystem.evolve.threat / (double)enemyDFHiveSystem.evolve.maxThreat);
				int spaceThreatMaxByWaves = EvolveData.GetSpaceThreatMaxByWaves(enemyDFHiveSystem.evolve.waves, newAgglv);
				enemyDFHiveSystem.evolve.maxThreat = spaceThreatMaxByWaves;
				enemyDFHiveSystem.evolve.threat = (int)((float)spaceThreatMaxByWaves * num + 0.5f);
				if (oldAgglv <= EAggressiveLevel.Passive || newAgglv <= EAggressiveLevel.Passive)
				{
					enemyDFHiveSystem.evolve.threat = 0;
				}
				if (newAgglv <= EAggressiveLevel.Passive)
				{
					enemyDFHiveSystem.hatred.Reset();
					enemyDFHiveSystem.evolve.waveTicks = 0;
					enemyDFHiveSystem.evolve.waveAsmTicks = 0;
					for (int j = 1; j < enemyDFHiveSystem.units.cursor; j++)
					{
						ref EnemyUnitComponent ptr = ref enemyDFHiveSystem.units.buffer[j];
						if (ptr.id == j)
						{
							ptr.hatredLock.Enter();
							ptr.hatred.Reset();
							ptr.hatredLock.Exit();
							ptr.assaults.Clear();
							this.enemyPool[ptr.enemyId].isAssaultingUnit = false;
							if (ptr.behavior == EEnemyBehavior.SeekTarget || ptr.behavior == EEnemyBehavior.ApproachTarget || ptr.behavior == EEnemyBehavior.OrbitTarget || ptr.behavior == EEnemyBehavior.Engage)
							{
								ptr.behavior = EEnemyBehavior.SeekForm;
								ptr.stateTick = 0;
							}
						}
					}
				}
			}
		}
		PlanetFactory[] factories = this.gameData.factories;
		int factoryCount = this.gameData.factoryCount;
		for (int k = 0; k < factoryCount; k++)
		{
			PlanetFactory planetFactory = factories[k];
			if (planetFactory != null)
			{
				ObjectPool<DFGBaseComponent> bases = planetFactory.enemySystem.bases;
				for (int l = 1; l < bases.cursor; l++)
				{
					DFGBaseComponent dfgbaseComponent = bases.buffer[l];
					if (dfgbaseComponent != null && dfgbaseComponent.id == l)
					{
						float num2 = (float)((double)dfgbaseComponent.evolve.threat / (double)dfgbaseComponent.evolve.maxThreat);
						int groundThreatMaxByWaves = EvolveData.GetGroundThreatMaxByWaves(dfgbaseComponent.evolve.waves, newAgglv);
						dfgbaseComponent.evolve.maxThreat = groundThreatMaxByWaves;
						dfgbaseComponent.evolve.threat = (int)((float)groundThreatMaxByWaves * num2 + 0.5f);
						if (oldAgglv <= EAggressiveLevel.Passive || newAgglv <= EAggressiveLevel.Passive)
						{
							dfgbaseComponent.evolve.threat = 0;
						}
						if (newAgglv <= EAggressiveLevel.Passive)
						{
							dfgbaseComponent.hatred.Reset();
							dfgbaseComponent.evolve.waveTicks = 0;
							dfgbaseComponent.evolve.waveAsmTicks = 0;
						}
					}
				}
				if (newAgglv <= EAggressiveLevel.Passive)
				{
					for (int m = 1; m < planetFactory.enemySystem.units.cursor; m++)
					{
						ref EnemyUnitComponent ptr2 = ref planetFactory.enemySystem.units.buffer[m];
						if (ptr2.id == m)
						{
							ptr2.hatredLock.Enter();
							ptr2.hatred.Reset();
							ptr2.hatredLock.Exit();
							ptr2.assaults.Clear();
							this.enemyPool[ptr2.enemyId].isAssaultingUnit = false;
							if (ptr2.behavior == EEnemyBehavior.SeekTarget || ptr2.behavior == EEnemyBehavior.Engage)
							{
								ptr2.behavior = EEnemyBehavior.SeekForm;
								ptr2.stateTick = 0;
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x060019AE RID: 6574 RVA: 0x001C5150 File Offset: 0x001C3350
	public void OnDFTruce()
	{
		for (int i = 0; i < this.dfHives.Length; i++)
		{
			for (EnemyDFHiveSystem enemyDFHiveSystem = this.dfHives[i]; enemyDFHiveSystem != null; enemyDFHiveSystem = enemyDFHiveSystem.nextSibling)
			{
				enemyDFHiveSystem.hatred.Reset();
				enemyDFHiveSystem.evolve.waveTicks = 0;
				enemyDFHiveSystem.evolve.waveAsmTicks = 0;
				for (int j = 1; j < enemyDFHiveSystem.units.cursor; j++)
				{
					ref EnemyUnitComponent ptr = ref enemyDFHiveSystem.units.buffer[j];
					if (ptr.id == j)
					{
						ptr.hatredLock.Enter();
						ptr.hatred.Reset();
						ptr.hatredLock.Exit();
						ptr.assaults.Clear();
						this.enemyPool[ptr.enemyId].isAssaultingUnit = false;
						if (ptr.behavior == EEnemyBehavior.SeekTarget || ptr.behavior == EEnemyBehavior.ApproachTarget || ptr.behavior == EEnemyBehavior.OrbitTarget || ptr.behavior == EEnemyBehavior.Engage)
						{
							ptr.behavior = EEnemyBehavior.SeekForm;
							ptr.stateTick = 0;
						}
					}
				}
			}
		}
		PlanetFactory[] factories = this.gameData.factories;
		int factoryCount = this.gameData.factoryCount;
		for (int k = 0; k < factoryCount; k++)
		{
			PlanetFactory planetFactory = factories[k];
			if (planetFactory != null)
			{
				ObjectPool<DFGBaseComponent> bases = planetFactory.enemySystem.bases;
				for (int l = 1; l < bases.cursor; l++)
				{
					DFGBaseComponent dfgbaseComponent = bases.buffer[l];
					if (dfgbaseComponent != null && dfgbaseComponent.id == l)
					{
						dfgbaseComponent.hatred.Reset();
						dfgbaseComponent.evolve.waveTicks = 0;
						dfgbaseComponent.evolve.waveAsmTicks = 0;
					}
				}
				for (int m = 1; m < planetFactory.enemySystem.units.cursor; m++)
				{
					ref EnemyUnitComponent ptr2 = ref planetFactory.enemySystem.units.buffer[m];
					if (ptr2.id == m)
					{
						ptr2.hatredLock.Enter();
						ptr2.hatred.Reset();
						ptr2.hatredLock.Exit();
						ptr2.assaults.Clear();
						this.enemyPool[ptr2.enemyId].isAssaultingUnit = false;
						if (ptr2.behavior == EEnemyBehavior.SeekTarget || ptr2.behavior == EEnemyBehavior.Engage)
						{
							ptr2.behavior = EEnemyBehavior.SeekForm;
							ptr2.stateTick = 0;
						}
					}
				}
			}
		}
	}

	// Token: 0x060019AF RID: 6575 RVA: 0x001C53D0 File Offset: 0x001C35D0
	public void AddHivePlanetHatred(int starIndex, int planetAstroId, int hatred)
	{
		for (EnemyDFHiveSystem enemyDFHiveSystem = this.dfHives[starIndex]; enemyDFHiveSystem != null; enemyDFHiveSystem = enemyDFHiveSystem.nextSibling)
		{
			EnemyDFHiveSystem obj = enemyDFHiveSystem;
			lock (obj)
			{
				enemyDFHiveSystem.AddPlanetHatred(planetAstroId, hatred);
			}
		}
	}

	// Token: 0x060019B0 RID: 6576 RVA: 0x001C5424 File Offset: 0x001C3624
	public void AddHiveThreat(int starIndex, int threat)
	{
		int aggressiveLevel = (int)this.gameData.history.combatSettings.aggressiveLevel;
		float powerThreatFactor = this.gameData.history.combatSettings.powerThreatFactor;
		if (aggressiveLevel <= 10)
		{
			return;
		}
		if (this.gameData.history.dfTruceTimer > 0L)
		{
			return;
		}
		for (EnemyDFHiveSystem enemyDFHiveSystem = this.dfHives[starIndex]; enemyDFHiveSystem != null; enemyDFHiveSystem = enemyDFHiveSystem.nextSibling)
		{
			EnemyDFHiveSystem obj = enemyDFHiveSystem;
			lock (obj)
			{
				EnemyDFHiveSystem enemyDFHiveSystem2 = enemyDFHiveSystem;
				enemyDFHiveSystem2.evolve.threat = enemyDFHiveSystem2.evolve.threat + (int)((float)threat * powerThreatFactor + 0.5f);
			}
		}
	}

	// Token: 0x060019B1 RID: 6577 RVA: 0x001C54D0 File Offset: 0x001C36D0
	public void GameTick(long time)
	{
		DeepProfiler.BeginSample(DPEntry.Creation, -1, 0L);
		this.creationSystem.PrepareTick();
		DeepProfiler.EndSample(-1, -2L);
		DeepProfiler.BeginSample(DPEntry.Creation, -1, 1L);
		this.creationSystem.GameTick(time);
		DeepProfiler.EndSample(-1, -2L);
		DeepProfiler.BeginSample(DPEntry.SkillSystem, -1, 0L);
		this.skillSystem.PrepareTick();
		DeepProfiler.EndSample(-1, -2L);
		DeepProfiler.BeginSample(DPEntry.SkillSystem, -1, 1L);
		this.skillSystem.GameTick(time);
		DeepProfiler.EndSample(-1, -2L);
		DeepProfiler.BeginSample(DPEntry.Creation, -1, 2L);
		this.creationSystem.AfterTick();
		DeepProfiler.EndSample(-1, -2L);
		DeepProfiler.BeginSample(DPEntry.SkillSystem, -1, 2L);
		this.skillSystem.AfterTick();
		DeepProfiler.EndSample(-1, -2L);
		StarData localStar = this.gameData.localStar;
		if (this.dfHives != null)
		{
			DeepProfiler.BeginSample(DPEntry.DFSSystem, -1, 1L);
			int num = this.dfHives.Length;
			long num2 = (time - 1L) * (long)num / 60L;
			long num3 = time * (long)num / 60L;
			for (long num4 = 0L; num4 < (long)num; num4 += 1L)
			{
				for (EnemyDFHiveSystem enemyDFHiveSystem = this.dfHives[(int)(checked((IntPtr)num4))]; enemyDFHiveSystem != null; enemyDFHiveSystem = enemyDFHiveSystem.nextSibling)
				{
					enemyDFHiveSystem.GameTickLogic(time, this.galaxyAstros, this.astros, this.enemyPool, this.enemyAnimPool);
					enemyDFHiveSystem.ExecuteDeferredEnemyChange();
				}
			}
			DeepProfiler.EndSample(-1, -2L);
			DeepProfiler.BeginSample(DPEntry.DFSSystem, -1, 2L);
			for (long num5 = num2; num5 < num3; num5 += 1L)
			{
				int num6 = (int)(num5 % (long)num);
				EnemyDFHiveSystem enemyDFHiveSystem2 = this.dfHives[num6];
				int num7 = 0;
				int num8 = 0;
				while (enemyDFHiveSystem2 != null)
				{
					if (!enemyDFHiveSystem2.isEmpty)
					{
						DeepProfiler.BeginSample(DPEntry.DFSHive, -1, (long)(enemyDFHiveSystem2.hiveAstroId - 1000000));
						enemyDFHiveSystem2.DecisionAI(time);
						bool flag;
						enemyDFHiveSystem2.KeyTickLogic(time, out flag);
						if (flag)
						{
							if (enemyDFHiveSystem2.ticks % 3 != 0)
							{
								enemyDFHiveSystem2.InterLearningFromLocalSystem();
							}
							else
							{
								enemyDFHiveSystem2.InterLearningFromOtherSystem();
							}
						}
						enemyDFHiveSystem2.ExecuteDeferredEnemyChange();
						enemyDFHiveSystem2.ExecuteDeferredUnitFormation();
						num7 += enemyDFHiveSystem2.evolve.exppshr;
						num8 += enemyDFHiveSystem2.evolve.threatshr;
						if (enemyDFHiveSystem2.evolve.waveTicks == 0 && enemyDFHiveSystem2.evolve.waveAsmTicks == 0)
						{
							EnemyDFHiveSystem enemyDFHiveSystem3 = enemyDFHiveSystem2;
							enemyDFHiveSystem3.evolve.threat = enemyDFHiveSystem3.evolve.threat + enemyDFHiveSystem2.evolve.threatshr / 250;
						}
						DeepProfiler.EndSample(-1, -2L);
					}
					enemyDFHiveSystem2 = enemyDFHiveSystem2.nextSibling;
				}
				for (enemyDFHiveSystem2 = this.dfHives[num6]; enemyDFHiveSystem2 != null; enemyDFHiveSystem2 = enemyDFHiveSystem2.nextSibling)
				{
					if (!enemyDFHiveSystem2.isEmpty)
					{
						DeepProfiler.BeginSample(DPEntry.DFSHive, -1, (long)(enemyDFHiveSystem2.hiveAstroId - 1000000));
						enemyDFHiveSystem2.evolve.AddExpPoint((num7 - enemyDFHiveSystem2.evolve.exppshr) / 40);
						if (enemyDFHiveSystem2.evolve.waveTicks == 0 && enemyDFHiveSystem2.evolve.waveAsmTicks == 0)
						{
							EnemyDFHiveSystem enemyDFHiveSystem4 = enemyDFHiveSystem2;
							enemyDFHiveSystem4.evolve.threat = enemyDFHiveSystem4.evolve.threat + (num8 - enemyDFHiveSystem2.evolve.threatshr) / 50;
						}
						enemyDFHiveSystem2.evolve.exppshr = 0;
						enemyDFHiveSystem2.evolve.threatshr = 0;
						DeepProfiler.EndSample(-1, -2L);
					}
				}
			}
			DeepProfiler.EndSample(-1, -2L);
		}
		DeepProfiler.BeginSample(DPEntry.CBSSystem, -1, -1L);
		this.combatSpaceSystem.GameTick(time);
		this.ExecuteDeferredCraftChange();
		DeepProfiler.EndSample(-1, -2L);
		DeepProfiler.BeginSample(DPEntry.Wreckage, -1, -1L);
		this.RuinDataGameTick(time);
		DeepProfiler.EndSample(-1, -2L);
	}

	// Token: 0x060019B2 RID: 6578 RVA: 0x001C5850 File Offset: 0x001C3A50
	private void RuinDataGameTick(long time)
	{
		if (this.spaceRuins != null)
		{
			int num = this.spaceRuins.cursor - 1;
			long num2 = (time - 1L) * (long)num / 60L;
			long num3 = time * (long)num / 60L;
			for (long num4 = num2; num4 < num3; num4 += 1L)
			{
				int num5 = (int)(num4 % (long)num);
				num5++;
				if (this.spaceRuins.buffer[num5].id == num5 && !this.spaceRuins.buffer[num5].UpdateLifeTime())
				{
					this.RemoveSpaceRuinWithComponet(num5);
				}
			}
			if (this.spaceRuins.count == 0 && this.spaceRuins.capacity > 256)
			{
				this.spaceRuins.Flush();
			}
		}
	}

	// Token: 0x060019B3 RID: 6579 RVA: 0x001C5904 File Offset: 0x001C3B04
	public void BeginSave()
	{
		int num = this.dfHives.Length;
		for (long num2 = 0L; num2 < (long)num; num2 += 1L)
		{
			for (EnemyDFHiveSystem enemyDFHiveSystem = this.dfHives[(int)(checked((IntPtr)num2))]; enemyDFHiveSystem != null; enemyDFHiveSystem = enemyDFHiveSystem.nextSibling)
			{
				enemyDFHiveSystem.BeginSave();
			}
		}
	}

	// Token: 0x060019B4 RID: 6580 RVA: 0x001C5948 File Offset: 0x001C3B48
	public void EndSave()
	{
		int num = this.dfHives.Length;
		for (long num2 = 0L; num2 < (long)num; num2 += 1L)
		{
			for (EnemyDFHiveSystem enemyDFHiveSystem = this.dfHives[(int)(checked((IntPtr)num2))]; enemyDFHiveSystem != null; enemyDFHiveSystem = enemyDFHiveSystem.nextSibling)
			{
				enemyDFHiveSystem.EndSave();
			}
		}
	}

	// Token: 0x060019B5 RID: 6581 RVA: 0x001C598C File Offset: 0x001C3B8C
	public void Export(BinaryWriter w)
	{
		PerformanceMonitor.BeginData(ESaveDataEntry.Combat);
		w.Write(1);
		w.Write(this.astros.Length);
		w.Write(this.astroCursor);
		for (int i = 1; i < this.astroCursor; i++)
		{
			if (this.astros[i].id > 0)
			{
				w.Write(this.astros[i].id);
				w.Write((int)this.astros[i].type);
				w.Write(this.astros[i].parentId);
				w.Write(this.astros[i].uPos.x);
				w.Write(this.astros[i].uPos.y);
				w.Write(this.astros[i].uPos.z);
				w.Write(this.astros[i].uPosNext.x);
				w.Write(this.astros[i].uPosNext.y);
				w.Write(this.astros[i].uPosNext.z);
				w.Write(this.astros[i].uRot.x);
				w.Write(this.astros[i].uRot.y);
				w.Write(this.astros[i].uRot.z);
				w.Write(this.astros[i].uRot.w);
				w.Write(this.astros[i].uRotNext.x);
				w.Write(this.astros[i].uRotNext.y);
				w.Write(this.astros[i].uRotNext.z);
				w.Write(this.astros[i].uRotNext.w);
				w.Write(this.astros[i].uRadius);
			}
			else
			{
				w.Write(0);
			}
		}
		w.Write(this.enemyCapacity);
		w.Write(this.enemyCursor);
		w.Write(this.enemyRecycleCursor);
		for (int j = 1; j < this.enemyCursor; j++)
		{
			this.enemyPool[j].Export(w);
		}
		for (int k = 0; k < this.enemyRecycleCursor; k++)
		{
			w.Write(this.enemyRecycle[k]);
		}
		for (int l = 1; l < this.enemyCursor; l++)
		{
			w.Write(this.enemyAnimPool[l].time);
			w.Write(this.enemyAnimPool[l].prepare_length);
			w.Write(this.enemyAnimPool[l].working_length);
			w.Write(this.enemyAnimPool[l].state);
			w.Write(this.enemyAnimPool[l].power);
		}
		w.Write(this.craftCapacity);
		w.Write(this.craftCursor);
		w.Write(this.craftRecycleCursor);
		for (int m = 1; m < this.craftCursor; m++)
		{
			this.craftPool[m].Export(w);
		}
		for (int n = 0; n < this.craftRecycleCursor; n++)
		{
			w.Write(this.craftRecycle[n]);
		}
		for (int num = 1; num < this.craftCursor; num++)
		{
			w.Write(this.craftAnimPool[num].time);
			w.Write(this.craftAnimPool[num].prepare_length);
			w.Write(this.craftAnimPool[num].working_length);
			w.Write(this.craftAnimPool[num].state);
			w.Write(this.craftAnimPool[num].power);
		}
		this.spaceRuins.Export(w);
		PerformanceMonitor.BeginData(ESaveDataEntry.Skill);
		this.skillSystem.Export(w);
		PerformanceMonitor.EndData(ESaveDataEntry.Skill);
		if (this.dfHives != null)
		{
			w.Write(this.dfHives.Length);
			for (int num2 = 0; num2 < this.dfHives.Length; num2++)
			{
				for (EnemyDFHiveSystem enemyDFHiveSystem = this.dfHives[num2]; enemyDFHiveSystem != null; enemyDFHiveSystem = enemyDFHiveSystem.nextSibling)
				{
					w.Write(19884);
					enemyDFHiveSystem.Export(w);
				}
				w.Write(0);
			}
		}
		else
		{
			w.Write(0);
		}
		this.combatSpaceSystem.Export(w);
		PerformanceMonitor.EndData(ESaveDataEntry.Combat);
		PerformanceMonitor.BeginData(ESaveDataEntry.Creation);
		PerformanceMonitor.EndData(ESaveDataEntry.Creation);
	}

	// Token: 0x060019B6 RID: 6582 RVA: 0x001C5E78 File Offset: 0x001C4078
	public void Import(BinaryReader r)
	{
		PerformanceMonitor.BeginData(ESaveDataEntry.Combat);
		int num = r.ReadInt32();
		int astroCapacity = r.ReadInt32();
		this.SetAstroCapacity(astroCapacity);
		this.astroCursor = r.ReadInt32();
		for (int i = 1; i < this.astroCursor; i++)
		{
			this.astros[i].id = r.ReadInt32();
			if (this.astros[i].id > 0)
			{
				this.astros[i].type = (EAstroType)r.ReadInt32();
				this.astros[i].parentId = r.ReadInt32();
				this.astros[i].uPos.x = r.ReadDouble();
				this.astros[i].uPos.y = r.ReadDouble();
				this.astros[i].uPos.z = r.ReadDouble();
				this.astros[i].uPosNext.x = r.ReadDouble();
				this.astros[i].uPosNext.y = r.ReadDouble();
				this.astros[i].uPosNext.z = r.ReadDouble();
				this.astros[i].uRot.x = r.ReadSingle();
				this.astros[i].uRot.y = r.ReadSingle();
				this.astros[i].uRot.z = r.ReadSingle();
				this.astros[i].uRot.w = r.ReadSingle();
				this.astros[i].uRotNext.x = r.ReadSingle();
				this.astros[i].uRotNext.y = r.ReadSingle();
				this.astros[i].uRotNext.z = r.ReadSingle();
				this.astros[i].uRotNext.w = r.ReadSingle();
				this.astros[i].uRadius = r.ReadSingle();
			}
			else
			{
				this.astros[i].SetEmpty();
			}
		}
		astroCapacity = r.ReadInt32();
		this.SetEnemyCapacity(astroCapacity);
		this.enemyCursor = r.ReadInt32();
		this.enemyRecycleCursor = r.ReadInt32();
		for (int j = 1; j < this.enemyCursor; j++)
		{
			this.enemyPool[j].Import(r);
		}
		for (int k = 0; k < this.enemyRecycleCursor; k++)
		{
			this.enemyRecycle[k] = r.ReadInt32();
		}
		for (int l = 1; l < this.enemyCursor; l++)
		{
			this.enemyAnimPool[l].time = r.ReadSingle();
			this.enemyAnimPool[l].prepare_length = r.ReadSingle();
			this.enemyAnimPool[l].working_length = r.ReadSingle();
			this.enemyAnimPool[l].state = r.ReadUInt32();
			this.enemyAnimPool[l].power = r.ReadSingle();
		}
		astroCapacity = r.ReadInt32();
		this.SetCraftCapacity(astroCapacity);
		this.craftCursor = r.ReadInt32();
		this.craftRecycleCursor = r.ReadInt32();
		for (int m = 1; m < this.craftCursor; m++)
		{
			this.craftPool[m].Import(r);
		}
		for (int n = 0; n < this.craftRecycleCursor; n++)
		{
			this.craftRecycle[n] = r.ReadInt32();
		}
		for (int num2 = 1; num2 < this.craftCursor; num2++)
		{
			this.craftAnimPool[num2].time = r.ReadSingle();
			this.craftAnimPool[num2].prepare_length = r.ReadSingle();
			this.craftAnimPool[num2].working_length = r.ReadSingle();
			this.craftAnimPool[num2].state = r.ReadUInt32();
			this.craftAnimPool[num2].power = r.ReadSingle();
		}
		if (num >= 1)
		{
			this.spaceRuins.Import(r);
		}
		PerformanceMonitor.BeginData(ESaveDataEntry.Skill);
		this.skillSystem.Import(r);
		PerformanceMonitor.EndData(ESaveDataEntry.Skill);
		int num3 = r.ReadInt32();
		if (num3 > 0)
		{
			if (num3 > 65535)
			{
				throw new Exception("invalid dfcnt!");
			}
			this.dfHives = new EnemyDFHiveSystem[num3];
			for (int num4 = 0; num4 < num3; num4++)
			{
				this.dfHives[num4] = null;
				EnemyDFHiveSystem enemyDFHiveSystem = null;
				while (r.ReadInt32() == 19884)
				{
					EnemyDFHiveSystem enemyDFHiveSystem2 = new EnemyDFHiveSystem();
					enemyDFHiveSystem2.Init(this.gameData, this.galaxy.stars[num4].id, 0);
					enemyDFHiveSystem2.Import(r);
					if (enemyDFHiveSystem == null)
					{
						this.dfHives[num4] = enemyDFHiveSystem2;
					}
					else
					{
						enemyDFHiveSystem.nextSibling = enemyDFHiveSystem2;
					}
					enemyDFHiveSystem2.prevSibling = enemyDFHiveSystem;
					enemyDFHiveSystem = enemyDFHiveSystem2;
				}
			}
		}
		this.combatSpaceSystem = new CombatSpaceSystem(this);
		this.combatSpaceSystem.Import(r);
		PerformanceMonitor.EndData(ESaveDataEntry.Combat);
		if (num >= 2)
		{
			PerformanceMonitor.BeginData(ESaveDataEntry.Creation);
			this.creationSystem.Import(r);
			PerformanceMonitor.EndData(ESaveDataEntry.Creation);
		}
	}

	// Token: 0x060019B7 RID: 6583 RVA: 0x001C63F0 File Offset: 0x001C45F0
	public static void InitPrefabDescArray()
	{
		if (SpaceSector.PrefabDescByModelIndex == null)
		{
			ModelProto[] dataArray = LDB.models.dataArray;
			SpaceSector.PrefabDescByModelIndex = new PrefabDesc[dataArray.Length + 64];
			for (int i = 0; i < dataArray.Length; i++)
			{
				SpaceSector.PrefabDescByModelIndex[dataArray[i].ID] = dataArray[i].prefabDesc;
			}
		}
	}

	// Token: 0x060019B8 RID: 6584 RVA: 0x001C6444 File Offset: 0x001C4644
	private void SetAstroCapacity(int newCapacity)
	{
		AstroData[] array = this.astros;
		this.astros = new AstroData[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.astros, (newCapacity > array.Length) ? array.Length : newCapacity);
		}
		for (int i = 0; i < newCapacity; i++)
		{
			if (this.astros[i].id == 0)
			{
				this.astros[i].uRot.w = 1f;
				this.astros[i].uRotNext.w = 1f;
			}
		}
	}

	// Token: 0x060019B9 RID: 6585 RVA: 0x001C64D4 File Offset: 0x001C46D4
	public int NewAstroData(AstroData astro)
	{
		int num = this.astroCursor;
		this.astroCursor = num + 1;
		int num2 = num;
		if (num2 == this.astros.Length)
		{
			this.SetAstroCapacity(this.astros.Length * 2);
		}
		this.astros[num2] = astro;
		this.astros[num2].id = num2 + 1000000;
		this.astros[num2].uPosNext = this.astros[num2].uPos;
		this.astros[num2].uRotNext = this.astros[num2].uRot;
		return this.astros[num2].id;
	}

	// Token: 0x060019BA RID: 6586 RVA: 0x001C6588 File Offset: 0x001C4788
	public void RemoveAstroData(int astroId)
	{
		if (astroId >= 1000000)
		{
			this.astros[astroId - 1000000].SetEmpty();
		}
	}

	// Token: 0x060019BB RID: 6587 RVA: 0x001C65AC File Offset: 0x001C47AC
	public EnemyDFHiveSystem GetHiveByAstroId(int astroId)
	{
		int num = astroId - 1000000;
		if ((ulong)num < (ulong)((long)this.maxHiveCount))
		{
			return this.dfHivesByAstro[num];
		}
		return null;
	}

	// Token: 0x060019BC RID: 6588 RVA: 0x001C65D8 File Offset: 0x001C47D8
	public void InverseTransformToAstro(int astroId, VectorLF3 upos, out VectorLF3 lpos)
	{
		if (astroId == 0)
		{
			lpos = upos;
			return;
		}
		if (astroId < 1000000)
		{
			Quaternion uRot = this.galaxyAstros[astroId].uRot;
			lpos = Maths.QInvRotateLF(uRot, upos - this.galaxyAstros[astroId].uPos);
			return;
		}
		int num = astroId - 1000000;
		Quaternion uRot2 = this.astros[num].uRot;
		lpos = Maths.QInvRotateLF(uRot2, upos - this.astros[num].uPos);
	}

	// Token: 0x060019BD RID: 6589 RVA: 0x001C666C File Offset: 0x001C486C
	public void InverseTransformToAstro(int astroId, VectorLF3 upos, Quaternion urot, out VectorLF3 lpos, out Quaternion lrot)
	{
		if (astroId == 0)
		{
			lpos = upos;
			lrot = urot;
			return;
		}
		if (astroId < 1000000)
		{
			Quaternion uRot = this.galaxyAstros[astroId].uRot;
			lpos = Maths.QInvRotateLF(uRot, upos - this.galaxyAstros[astroId].uPos);
			uRot.w = -uRot.w;
			lrot = uRot * urot;
			return;
		}
		int num = astroId - 1000000;
		Quaternion uRot2 = this.astros[num].uRot;
		lpos = Maths.QInvRotateLF(uRot2, upos - this.astros[num].uPos);
		uRot2.w = -uRot2.w;
		lrot = uRot2 * urot;
	}

	// Token: 0x060019BE RID: 6590 RVA: 0x001C6744 File Offset: 0x001C4944
	public void InverseTransformToAstro_ref(int astroId, ref VectorLF3 upos, out VectorLF3 lpos)
	{
		if (astroId == 0)
		{
			lpos = upos;
			return;
		}
		if (astroId < 1000000)
		{
			Quaternion uRot = this.galaxyAstros[astroId].uRot;
			lpos.x = upos.x - this.galaxyAstros[astroId].uPos.x;
			lpos.y = upos.y - this.galaxyAstros[astroId].uPos.y;
			lpos.z = upos.z - this.galaxyAstros[astroId].uPos.z;
			Maths.QInvRotateLF_ref(ref uRot, ref lpos, ref lpos);
			uRot.w = -uRot.w;
			return;
		}
		int num = astroId - 1000000;
		Quaternion uRot2 = this.astros[num].uRot;
		lpos.x = upos.x - this.astros[num].uPos.x;
		lpos.y = upos.y - this.astros[num].uPos.y;
		lpos.z = upos.z - this.astros[num].uPos.z;
		Maths.QInvRotateLF_ref(ref uRot2, ref lpos, ref lpos);
		uRot2.w = -uRot2.w;
	}

	// Token: 0x060019BF RID: 6591 RVA: 0x001C689C File Offset: 0x001C4A9C
	public void InverseTransformToAstro_ref(int astroId, ref VectorLF3 upos, ref Quaternion urot, out VectorLF3 lpos, out Quaternion lrot)
	{
		if (astroId == 0)
		{
			lpos = upos;
			lrot = urot;
			return;
		}
		if (astroId < 1000000)
		{
			Quaternion uRot = this.galaxyAstros[astroId].uRot;
			lpos.x = upos.x - this.galaxyAstros[astroId].uPos.x;
			lpos.y = upos.y - this.galaxyAstros[astroId].uPos.y;
			lpos.z = upos.z - this.galaxyAstros[astroId].uPos.z;
			Maths.QInvRotateLF_ref(ref uRot, ref lpos, ref lpos);
			uRot.w = -uRot.w;
			lrot = uRot * urot;
			return;
		}
		int num = astroId - 1000000;
		Quaternion uRot2 = this.astros[num].uRot;
		lpos.x = upos.x - this.astros[num].uPos.x;
		lpos.y = upos.y - this.astros[num].uPos.y;
		lpos.z = upos.z - this.astros[num].uPos.z;
		Maths.QInvRotateLF_ref(ref uRot2, ref lpos, ref lpos);
		uRot2.w = -uRot2.w;
		lrot = uRot2 * urot;
	}

	// Token: 0x060019C0 RID: 6592 RVA: 0x001C6A30 File Offset: 0x001C4C30
	public void InverseTransformToAstro_inout(int astroId, ref VectorLF3 pos, ref Quaternion rot)
	{
		if (astroId == 0)
		{
			return;
		}
		if (astroId < 1000000)
		{
			Quaternion uRot = this.galaxyAstros[astroId].uRot;
			pos.x -= this.galaxyAstros[astroId].uPos.x;
			pos.y -= this.galaxyAstros[astroId].uPos.y;
			pos.z -= this.galaxyAstros[astroId].uPos.z;
			Maths.QInvRotateLF_ref(ref uRot, ref pos, ref pos);
			uRot.w = -uRot.w;
			rot = uRot * rot;
			return;
		}
		int num = astroId - 1000000;
		Quaternion uRot2 = this.astros[num].uRot;
		pos.x -= this.astros[num].uPos.x;
		pos.y -= this.astros[num].uPos.y;
		pos.z -= this.astros[num].uPos.z;
		Maths.QInvRotateLF_ref(ref uRot2, ref pos, ref pos);
		uRot2.w = -uRot2.w;
		rot = uRot2 * rot;
	}

	// Token: 0x060019C1 RID: 6593 RVA: 0x001C6BA0 File Offset: 0x001C4DA0
	public void InverseTransformToAstroNext(int astroId, VectorLF3 upos, Quaternion urot, out VectorLF3 lpos, out Quaternion lrot)
	{
		if (astroId == 0)
		{
			lpos = upos;
			lrot = urot;
			return;
		}
		if (astroId < 1000000)
		{
			Quaternion uRotNext = this.galaxyAstros[astroId].uRotNext;
			lpos = Maths.QInvRotateLF(uRotNext, upos - this.galaxyAstros[astroId].uPosNext);
			uRotNext.w = -uRotNext.w;
			lrot = uRotNext * urot;
			return;
		}
		int num = astroId - 1000000;
		Quaternion uRotNext2 = this.astros[num].uRotNext;
		lpos = Maths.QInvRotateLF(uRotNext2, upos - this.astros[num].uPosNext);
		uRotNext2.w = -uRotNext2.w;
		lrot = uRotNext2 * urot;
	}

	// Token: 0x060019C2 RID: 6594 RVA: 0x001C6C78 File Offset: 0x001C4E78
	public void TransformFromAstro(int astroId, out VectorLF3 upos, Vector3 lpos)
	{
		if (astroId == 0)
		{
			upos = lpos;
		}
		if (astroId < 1000000)
		{
			upos = Maths.QRotateLF(this.galaxyAstros[astroId].uRot, lpos) + this.galaxyAstros[astroId].uPos;
			return;
		}
		int num = astroId - 1000000;
		upos = Maths.QRotateLF(this.astros[num].uRot, lpos) + this.astros[num].uPos;
	}

	// Token: 0x060019C3 RID: 6595 RVA: 0x001C6D18 File Offset: 0x001C4F18
	public void TransformFromAstro(int astroId, out VectorLF3 upos, VectorLF3 lpos)
	{
		if (astroId == 0)
		{
			upos = lpos;
		}
		if (astroId < 1000000)
		{
			upos = Maths.QRotateLF(this.galaxyAstros[astroId].uRot, lpos) + this.galaxyAstros[astroId].uPos;
			return;
		}
		int num = astroId - 1000000;
		upos = Maths.QRotateLF(this.astros[num].uRot, lpos) + this.astros[num].uPos;
	}

	// Token: 0x060019C4 RID: 6596 RVA: 0x001C6DA8 File Offset: 0x001C4FA8
	public void TransformFromAstro(int astroId, out VectorLF3 upos, out Quaternion urot, VectorLF3 lpos, Quaternion lrot)
	{
		if (astroId == 0)
		{
			upos = lpos;
			urot = lrot;
		}
		if (astroId < 1000000)
		{
			Quaternion uRot = this.galaxyAstros[astroId].uRot;
			upos = Maths.QRotateLF(uRot, lpos) + this.galaxyAstros[astroId].uPos;
			urot = uRot * lrot;
			return;
		}
		int num = astroId - 1000000;
		Quaternion uRot2 = this.astros[num].uRot;
		upos = Maths.QRotateLF(uRot2, lpos) + this.astros[num].uPos;
		urot = uRot2 * lrot;
	}

	// Token: 0x060019C5 RID: 6597 RVA: 0x001C6E64 File Offset: 0x001C5064
	public void TransformFromAstro_ref(int astroId, out VectorLF3 upos, ref VectorLF3 lpos)
	{
		if (astroId == 0)
		{
			upos = lpos;
			return;
		}
		if (astroId < 1000000)
		{
			upos.x = 0.0;
			upos.y = 0.0;
			upos.z = 0.0;
			Maths.QRotateLF_ref(ref this.galaxyAstros[astroId].uRot, ref lpos, ref upos);
			upos.x += this.galaxyAstros[astroId].uPos.x;
			upos.y += this.galaxyAstros[astroId].uPos.y;
			upos.z += this.galaxyAstros[astroId].uPos.z;
			return;
		}
		int num = astroId - 1000000;
		upos.x = 0.0;
		upos.y = 0.0;
		upos.z = 0.0;
		Maths.QRotateLF_ref(ref this.astros[num].uRot, ref lpos, ref upos);
		upos.x += this.astros[num].uPos.x;
		upos.y += this.astros[num].uPos.y;
		upos.z += this.astros[num].uPos.z;
	}

	// Token: 0x060019C6 RID: 6598 RVA: 0x001C6FF4 File Offset: 0x001C51F4
	public void TransformFromAstro_ref(int astroId, out VectorLF3 upos, out Quaternion urot, ref VectorLF3 lpos, ref Quaternion lrot)
	{
		if (astroId == 0)
		{
			upos = lpos;
			urot = lrot;
			return;
		}
		if (astroId < 1000000)
		{
			Quaternion uRot = this.galaxyAstros[astroId].uRot;
			upos = new VectorLF3(0f, 0f, 0f);
			Maths.QRotateLF_ref(ref uRot, ref lpos, ref upos);
			upos.x += this.galaxyAstros[astroId].uPos.x;
			upos.y += this.galaxyAstros[astroId].uPos.y;
			upos.z += this.galaxyAstros[astroId].uPos.z;
			urot = uRot * lrot;
			return;
		}
		int num = astroId - 1000000;
		Quaternion uRot2 = this.astros[num].uRot;
		upos = new VectorLF3(0f, 0f, 0f);
		Maths.QRotateLF_ref(ref uRot2, ref lpos, ref upos);
		upos.x += this.astros[num].uPos.x;
		upos.y += this.astros[num].uPos.y;
		upos.z += this.astros[num].uPos.z;
		urot = uRot2 * lrot;
	}

	// Token: 0x060019C7 RID: 6599 RVA: 0x001C7198 File Offset: 0x001C5398
	public void TransformFromAstro_inout(int astroId, ref VectorLF3 pos, ref Quaternion rot)
	{
		if (astroId == 0)
		{
			return;
		}
		if (astroId < 1000000)
		{
			Quaternion uRot = this.galaxyAstros[astroId].uRot;
			Maths.QRotateLF_ref(ref uRot, ref pos, ref pos);
			pos.x += this.galaxyAstros[astroId].uPos.x;
			pos.y += this.galaxyAstros[astroId].uPos.y;
			pos.z += this.galaxyAstros[astroId].uPos.z;
			rot = uRot * rot;
			return;
		}
		int num = astroId - 1000000;
		Quaternion uRot2 = this.astros[num].uRot;
		Maths.QRotateLF_ref(ref uRot2, ref pos, ref pos);
		pos.x += this.astros[num].uPos.x;
		pos.y += this.astros[num].uPos.y;
		pos.z += this.astros[num].uPos.z;
		rot = uRot2 * rot;
	}

	// Token: 0x060019C8 RID: 6600 RVA: 0x001C72EC File Offset: 0x001C54EC
	public void TransformVelocityFromAstro_ref(int astroId, out Vector3 uvel, ref VectorLF3 lpos, ref Vector3 vel)
	{
		if (astroId == 0)
		{
			uvel = vel;
			return;
		}
		if (astroId < 1000000)
		{
			ref AstroData ptr = ref this.galaxyAstros[astroId];
			ptr.VelocityU(ref lpos, out uvel);
			uvel += Maths.QRotate(ptr.uRot, vel);
			return;
		}
		int num = astroId - 1000000;
		ref AstroData ptr2 = ref this.astros[num];
		ptr2.VelocityU(ref lpos, out uvel);
		uvel += Maths.QRotate(ptr2.uRot, vel);
	}

	// Token: 0x060019C9 RID: 6601 RVA: 0x001C738C File Offset: 0x001C558C
	public void InverseTransformVelocityFromAstro_ref(int astroId, out Vector3 lvel, ref VectorLF3 lpos, ref Vector3 uvel)
	{
		if (astroId == 0)
		{
			lvel = uvel;
			return;
		}
		if (astroId < 1000000)
		{
			ref AstroData ptr = ref this.galaxyAstros[astroId];
			ptr.VelocityU(ref lpos, out lvel);
			lvel = Maths.QInvRotate(ptr.uRot, uvel) - lvel;
			return;
		}
		int num = astroId - 1000000;
		ref AstroData ptr2 = ref this.astros[num];
		ptr2.VelocityU(ref lpos, out lvel);
		lvel = Maths.QInvRotate(ptr2.uRot, uvel) - lvel;
	}

	// Token: 0x060019CA RID: 6602 RVA: 0x001C742C File Offset: 0x001C562C
	public void TransformFromAstroNext(int astroId, out VectorLF3 upos, out Quaternion urot, VectorLF3 lpos, Quaternion lrot)
	{
		if (astroId == 0)
		{
			upos = lpos;
			urot = lrot;
			return;
		}
		if (astroId < 1000000)
		{
			Quaternion uRotNext = this.galaxyAstros[astroId].uRotNext;
			upos = Maths.QRotateLF(uRotNext, lpos) + this.galaxyAstros[astroId].uPosNext;
			urot = uRotNext * lrot;
			return;
		}
		int num = astroId - 1000000;
		Quaternion uRotNext2 = this.astros[num].uRotNext;
		upos = Maths.QRotateLF(uRotNext2, lpos) + this.astros[num].uPosNext;
		urot = uRotNext2 * lrot;
	}

	// Token: 0x060019CB RID: 6603 RVA: 0x001C74E8 File Offset: 0x001C56E8
	public Pose GetRelativePose(int astroId, VectorLF3 lpos, Quaternion lrot)
	{
		VectorLF3 lhs;
		Quaternion rhs;
		if (astroId == 0)
		{
			lhs = lpos;
			rhs = lrot;
		}
		else if (astroId < 1000000)
		{
			Quaternion uRot = this.galaxyAstros[astroId].uRot;
			lhs = Maths.QRotateLF(uRot, lpos) + this.galaxyAstros[astroId].uPos;
			rhs = uRot * lrot;
		}
		else
		{
			int num = astroId - 1000000;
			Quaternion uRot2 = this.astros[num].uRot;
			lhs = Maths.QRotateLF(uRot2, lpos) + this.astros[num].uPos;
			rhs = uRot2 * lrot;
		}
		return new Pose(Maths.QInvRotateLF(this.gameData.relativeRot, lhs - this.gameData.relativePos), Quaternion.Inverse(this.gameData.relativeRot) * rhs);
	}

	// Token: 0x060019CC RID: 6604 RVA: 0x001C75C0 File Offset: 0x001C57C0
	public StarData GetNearestStar(ref VectorLF3 upos, out double dist)
	{
		StarData starData = null;
		double num = double.MaxValue;
		StarData[] stars = this.galaxy.stars;
		int starCount = this.galaxy.starCount;
		for (int i = 0; i < starCount; i++)
		{
			double sqrMagnitude = (upos - stars[i].uPosition).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				starData = stars[i];
				num = sqrMagnitude;
			}
		}
		dist = ((starData == null) ? 0.0 : Math.Sqrt(num));
		return starData;
	}

	// Token: 0x060019CD RID: 6605 RVA: 0x001C7644 File Offset: 0x001C5844
	public PlanetData GetNearestPlanet(ref VectorLF3 upos, out double dist)
	{
		PlanetData planetData = null;
		double num = double.MaxValue;
		StarData[] stars = this.galaxy.stars;
		int starCount = this.galaxy.starCount;
		for (int i = 0; i < starCount; i++)
		{
			PlanetData[] planets = stars[i].planets;
			int planetCount = stars[i].planetCount;
			for (int j = 0; j < planetCount; j++)
			{
				double sqrMagnitude = (upos - planets[j].uPosition).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					planetData = planets[j];
					num = sqrMagnitude;
				}
			}
		}
		dist = ((planetData == null) ? 0.0 : Math.Sqrt(num));
		return planetData;
	}

	// Token: 0x060019CE RID: 6606 RVA: 0x001C76F4 File Offset: 0x001C58F4
	public PlanetData GetNearestPlanet(int starAstroId, out double dist, ref VectorLF3 pos, bool isLocalPos)
	{
		if (starAstroId < 100 || starAstroId > 204899 || starAstroId % 100 > 0)
		{
			dist = 0.0;
			return null;
		}
		VectorLF3 lhs = pos;
		if (isLocalPos)
		{
			this.TransformFromAstro_ref(starAstroId, out lhs, ref pos);
		}
		PlanetData planetData = null;
		double num = double.MaxValue;
		StarData starData = this.galaxy.StarById(starAstroId / 100);
		PlanetData[] planets = starData.planets;
		int planetCount = starData.planetCount;
		for (int i = 0; i < planetCount; i++)
		{
			double sqrMagnitude = (lhs - planets[i].uPosition).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				planetData = planets[i];
				num = sqrMagnitude;
			}
		}
		dist = ((planetData == null) ? 0.0 : Math.Sqrt(num));
		return planetData;
	}

	// Token: 0x060019CF RID: 6607 RVA: 0x001C77B0 File Offset: 0x001C59B0
	public void SetEnemyCapacity(int newCapacity)
	{
		EnemyData[] array = this.enemyPool;
		this.enemyPool = new EnemyData[newCapacity];
		this.enemyRecycle = new int[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.enemyPool, (newCapacity > this.enemyCapacity) ? this.enemyCapacity : newCapacity);
		}
		AnimData[] array2 = this.enemyAnimPool;
		this.enemyAnimPool = new AnimData[newCapacity];
		if (array2 != null)
		{
			Array.Copy(array2, this.enemyAnimPool, (newCapacity > this.enemyCapacity) ? this.enemyCapacity : newCapacity);
		}
		this.enemyCapacity = newCapacity;
	}

	// Token: 0x060019D0 RID: 6608 RVA: 0x001C7838 File Offset: 0x001C5A38
	public int AddEnemyData(ref EnemyData enemy)
	{
		if (this.enemyRecycleCursor > 0)
		{
			int[] array = this.enemyRecycle;
			int num = this.enemyRecycleCursor - 1;
			this.enemyRecycleCursor = num;
			enemy.id = array[num];
		}
		else
		{
			int num = this.enemyCursor;
			this.enemyCursor = num + 1;
			enemy.id = num;
		}
		if (enemy.id == this.enemyCapacity)
		{
			this.SetEnemyCapacity(this.enemyCapacity * 2);
		}
		this.enemyPool[enemy.id] = enemy;
		return enemy.id;
	}

	// Token: 0x060019D1 RID: 6609 RVA: 0x001C78C0 File Offset: 0x001C5AC0
	public void CreateEnemyDisplayComponents(int enemyId, EnemyDFHiveSystem specify_hive, bool isPreview = false)
	{
		int modelIndex = (int)this.enemyPool[enemyId].modelIndex;
		PrefabDesc prefabDesc = SpaceSector.PrefabDescByModelIndex[modelIndex];
		if (prefabDesc == null)
		{
			return;
		}
		this.enemyPool[enemyId].modelId = this.model.gpuiManager.AddModel(modelIndex, enemyId, this.enemyPool[enemyId].astroId, this.enemyPool[enemyId].pos, this.enemyPool[enemyId].rot, true);
		this.model.starmapgpuiManager.RegisterModelIndex(modelIndex);
		this.enemyPool[enemyId].mmblockId = 0;
		if (!isPreview)
		{
			if (prefabDesc.colliders != null && prefabDesc.colliders.Length != 0)
			{
				int dfSConnectorId = this.enemyPool[enemyId].dfSConnectorId;
				if (dfSConnectorId > 0)
				{
					int originAstroId = this.enemyPool[enemyId].originAstroId;
					EnemyDFHiveSystem enemyDFHiveSystem = specify_hive;
					if (enemyDFHiveSystem == null && originAstroId > 1000000 && originAstroId - 1000000 < this.dfHivesByAstro.Length)
					{
						enemyDFHiveSystem = this.dfHivesByAstro[originAstroId - 1000000];
					}
					float num = 1f;
					if (enemyDFHiveSystem != null)
					{
						num = enemyDFHiveSystem.connectors.buffer[dfSConnectorId].length;
					}
					for (int i = 0; i < prefabDesc.spacePhysicsColliders.Length; i++)
					{
						ColliderDataLF colliderDataLF = prefabDesc.spacePhysicsColliders[i];
						colliderDataLF.ext.z = num / 2f;
						colliderDataLF.pos.z = (double)(num / 2f);
						this.enemyPool[enemyId].colliderId = this.physics.AddColliderData(colliderDataLF.BindToObject(enemyId, this.enemyPool[enemyId].colliderId, EObjectType.Enemy, this.enemyPool[enemyId].astroId, this.enemyPool[enemyId].pos, this.enemyPool[enemyId].rot));
					}
					for (int j = 0; j < prefabDesc.spaceBuildColliders.Length; j++)
					{
						ColliderDataLF colliderDataLF2 = prefabDesc.spaceBuildColliders[j];
						colliderDataLF2.ext.z = num / 2f;
						colliderDataLF2.pos.z = (double)(num / 2f);
						this.enemyPool[enemyId].colliderId = this.physics.AddColliderData(colliderDataLF2.BindToObject(enemyId, this.enemyPool[enemyId].colliderId, EObjectType.Enemy, this.enemyPool[enemyId].astroId, this.enemyPool[enemyId].pos, this.enemyPool[enemyId].rot));
					}
				}
				else
				{
					for (int k = 0; k < prefabDesc.spacePhysicsColliders.Length; k++)
					{
						this.enemyPool[enemyId].colliderId = this.physics.AddColliderData(prefabDesc.spacePhysicsColliders[k].BindToObject(enemyId, this.enemyPool[enemyId].colliderId, EObjectType.Enemy, this.enemyPool[enemyId].astroId, this.enemyPool[enemyId].pos, this.enemyPool[enemyId].rot));
					}
					for (int l = 0; l < prefabDesc.spaceBuildColliders.Length; l++)
					{
						this.enemyPool[enemyId].colliderId = this.physics.AddColliderData(prefabDesc.spaceBuildColliders[l].BindToObject(enemyId, this.enemyPool[enemyId].colliderId, EObjectType.Enemy, this.enemyPool[enemyId].astroId, this.enemyPool[enemyId].pos, this.enemyPool[enemyId].rot));
					}
				}
			}
			if (prefabDesc.hasAudio)
			{
				this.enemyPool[enemyId].audioId = this.audio.AddAudioData(enemyId, EObjectType.Enemy, this.enemyPool[enemyId].astroId, this.enemyPool[enemyId].pos, prefabDesc);
			}
		}
	}

	// Token: 0x060019D2 RID: 6610 RVA: 0x001C7CEC File Offset: 0x001C5EEC
	public void CreateEnemyLogicComponents(int enemyId, PrefabDesc desc, EnemyDFHiveSystem hive, int builderIndex, bool isPreview = false)
	{
		int originAstroId = this.enemyPool[enemyId].originAstroId;
		if (hive == null && originAstroId > 1000000 && originAstroId - 1000000 < this.dfHivesByAstro.Length)
		{
			hive = this.dfHivesByAstro[originAstroId - 1000000];
		}
		if (hive != null)
		{
			if (desc.isEnemyBuilder)
			{
				hive.NewEnemyBuilderComponent(enemyId, desc, builderIndex);
			}
			if (builderIndex > 0)
			{
				if (desc.isDFSpaceCore && !isPreview)
				{
					hive.NewDFSCoreComponent(enemyId, desc);
				}
				if (desc.isDFSpaceNode && !isPreview)
				{
					hive.NewDFSNodeComponent(enemyId, desc);
				}
				if (desc.isDFSpaceConnector && !isPreview)
				{
					hive.NewDFSConnectorComponent(enemyId, desc);
				}
				if (desc.isDFSpaceReplicator && !isPreview)
				{
					hive.NewDFSReplicatorComponent(enemyId, desc);
				}
				if (desc.isDFSpaceGammaReceiver && !isPreview)
				{
					hive.NewDFSGammaComponent(enemyId, desc);
				}
				if (desc.isDFSpaceTurret && !isPreview)
				{
					hive.NewDFSTurretComponent(enemyId, desc);
				}
			}
			if (desc.isDFRelay)
			{
				hive.NewDFRelayComponent(enemyId, desc);
			}
			if (desc.isDFTinder)
			{
				hive.NewDFTinderComponent(enemyId, desc);
			}
			if (desc.isEnemyUnit)
			{
				hive.NewEnemyUnitComponent(enemyId);
			}
		}
		this.enemyAnimPool[enemyId].time = 0f;
		this.enemyAnimPool[enemyId].prepare_length = desc.anim_prepare_length;
		this.enemyAnimPool[enemyId].working_length = desc.anim_working_length;
		this.enemyAnimPool[enemyId].state = 0U;
		this.enemyAnimPool[enemyId].power = 0f;
	}

	// Token: 0x060019D3 RID: 6611 RVA: 0x001C7E74 File Offset: 0x001C6074
	private int AddEnemyDataWithComponents(ref EnemyData enemy, EnemyDFHiveSystem hive, int builderIndex, bool isPreview = false)
	{
		int num = this.AddEnemyData(ref enemy);
		PrefabDesc prefabDesc = SpaceSector.PrefabDescByModelIndex[(int)enemy.modelIndex];
		if (prefabDesc == null)
		{
			return num;
		}
		this.CreateEnemyLogicComponents(num, prefabDesc, hive, builderIndex, isPreview);
		if (this.model != null)
		{
			this.CreateEnemyDisplayComponents(num, hive, isPreview);
		}
		hive.SetupReferenceOnEnemyCreate(num);
		return num;
	}

	// Token: 0x060019D4 RID: 6612 RVA: 0x001C7EC8 File Offset: 0x001C60C8
	public void RemoveEnemyWithComponents(int id)
	{
		if (id != 0 && this.enemyPool[id].id != 0)
		{
			int originAstroId = this.enemyPool[id].originAstroId;
			EnemyDFHiveSystem enemyDFHiveSystem = null;
			if (originAstroId > 1000000 && originAstroId - 1000000 < this.dfHivesByAstro.Length)
			{
				enemyDFHiveSystem = this.dfHivesByAstro[originAstroId - 1000000];
			}
			if (enemyDFHiveSystem != null)
			{
				enemyDFHiveSystem.ClearReferencesOnEnemyRemove(id);
				if (this.enemyPool[id].dfSCoreId != 0)
				{
					enemyDFHiveSystem.RemoveDFSCoreComponent(this.enemyPool[id].dfSCoreId);
					this.enemyPool[id].dfSCoreId = 0;
				}
				if (this.enemyPool[id].dfSNodeId != 0)
				{
					enemyDFHiveSystem.RemoveDFSNodeComponent(this.enemyPool[id].dfSNodeId);
					this.enemyPool[id].dfSNodeId = 0;
				}
				if (this.enemyPool[id].dfSConnectorId != 0)
				{
					enemyDFHiveSystem.RemoveDFSConnectorComponent(this.enemyPool[id].dfSConnectorId);
					this.enemyPool[id].dfSConnectorId = 0;
				}
				if (this.enemyPool[id].dfSReplicatorId != 0)
				{
					enemyDFHiveSystem.RemoveDFSReplicatorComponent(this.enemyPool[id].dfSReplicatorId);
					this.enemyPool[id].dfSReplicatorId = 0;
				}
				if (this.enemyPool[id].dfSGammaId != 0)
				{
					enemyDFHiveSystem.RemoveDFSGammaComponent(this.enemyPool[id].dfSGammaId);
					this.enemyPool[id].dfSGammaId = 0;
				}
				if (this.enemyPool[id].dfSTurretId != 0)
				{
					enemyDFHiveSystem.RemoveDFSTurretComponent(this.enemyPool[id].dfSTurretId);
					this.enemyPool[id].dfSTurretId = 0;
				}
				if (this.enemyPool[id].dfTinderId != 0)
				{
					enemyDFHiveSystem.RemoveDFTinderComponent(this.enemyPool[id].dfTinderId);
					this.enemyPool[id].dfTinderId = 0;
				}
				if (this.enemyPool[id].dfRelayId != 0)
				{
					enemyDFHiveSystem.RemoveDFRelayComponent(this.enemyPool[id].dfRelayId);
					this.enemyPool[id].dfRelayId = 0;
				}
			}
			if (this.enemyPool[id].combatStatId != 0)
			{
				int combatStatId = this.enemyPool[id].combatStatId;
				this.skillSystem.OnRemovingSkillTarget(combatStatId, this.skillSystem.combatStats.buffer[combatStatId].originAstroId, ETargetType.CombatStat);
				this.skillSystem.combatStats.Remove(combatStatId);
				this.enemyPool[id].combatStatId = 0;
			}
			if (enemyDFHiveSystem != null)
			{
				if (this.enemyPool[id].unitId != 0)
				{
					enemyDFHiveSystem.RemoveEnemyUnitComponent(this.enemyPool[id].unitId);
					this.enemyPool[id].unitId = 0;
				}
				if (this.enemyPool[id].builderId != 0)
				{
					enemyDFHiveSystem.RemoveEnemyBuilderComponent(this.enemyPool[id].builderId);
					this.enemyPool[id].builderId = 0;
				}
			}
			if (this.enemyPool[id].modelId != 0)
			{
				this.model.gpuiManager.RemoveModel((int)this.enemyPool[id].modelIndex, this.enemyPool[id].modelId, true);
				this.enemyPool[id].modelId = 0;
			}
			if (this.enemyPool[id].mmblockId != 0)
			{
				this.enemyPool[id].mmblockId = 0;
			}
			if (this.enemyPool[id].colliderId != 0)
			{
				if (this.physics != null)
				{
					this.physics.RemoveLinkedColliderData(this.enemyPool[id].colliderId);
				}
				this.enemyPool[id].colliderId = 0;
			}
			if (this.enemyPool[id].audioId != 0)
			{
				if (this.audio != null)
				{
					this.audio.RemoveAudioData(this.enemyPool[id].audioId);
				}
				this.enemyPool[id].audioId = 0;
			}
			this.skillSystem.OnRemovingSkillTarget(id, this.enemyPool[id].originAstroId, ETargetType.Enemy);
			this.enemyPool[id].SetEmpty();
			this.enemyAnimPool[id].time = 0f;
			this.enemyAnimPool[id].prepare_length = 0f;
			this.enemyAnimPool[id].working_length = 0f;
			this.enemyAnimPool[id].state = 0U;
			this.enemyAnimPool[id].power = 0f;
			int[] array = this.enemyRecycle;
			int num = this.enemyRecycleCursor;
			this.enemyRecycleCursor = num + 1;
			array[num] = id;
		}
		if (this.physics != null)
		{
			this.physics.NotifyObjectRemove(EObjectType.Enemy, id);
		}
		if (this.audio != null)
		{
			this.audio.NotifyObjectRemove(EObjectType.Enemy, id);
		}
	}

	// Token: 0x060019D5 RID: 6613 RVA: 0x001C83F4 File Offset: 0x001C65F4
	public int CreateEnemyFinal(EnemyDFHiveSystem hive, int builderIndex, bool isPreview = false)
	{
		ref GrowthPattern_DFSpace.Builder ptr = ref hive.pbuilders[builderIndex];
		if (ptr.instId > 0)
		{
			return 0;
		}
		int protoId = ptr.protoId;
		EnemyProto enemyProto = LDB.enemies.Select(protoId);
		if (enemyProto != null)
		{
			EnemyData enemyData = default(EnemyData);
			enemyData.protoId = (short)protoId;
			enemyData.modelIndex = (short)enemyProto.ModelIndex;
			enemyData.astroId = hive.hiveAstroId;
			enemyData.originAstroId = hive.hiveAstroId;
			enemyData.owner = 0;
			enemyData.port = 0;
			enemyData.dynamic = !enemyProto.IsBuilding;
			enemyData.isSpace = true;
			enemyData.localized = !isPreview;
			enemyData.stateFlags = 0;
			enemyData.pos = ptr.pos;
			enemyData.rot = ptr.rot;
			return this.AddEnemyDataWithComponents(ref enemyData, hive, builderIndex, isPreview);
		}
		return 0;
	}

	// Token: 0x060019D6 RID: 6614 RVA: 0x001C84D4 File Offset: 0x001C66D4
	public int CreateEnemyFinal(EnemyDFHiveSystem hive, int protoId, int astroId, VectorLF3 lpos, Quaternion lrot)
	{
		EnemyProto enemyProto = LDB.enemies.Select(protoId);
		if (enemyProto != null)
		{
			EnemyData enemyData = default(EnemyData);
			enemyData.protoId = (short)protoId;
			enemyData.modelIndex = (short)enemyProto.ModelIndex;
			enemyData.astroId = astroId;
			enemyData.originAstroId = hive.hiveAstroId;
			enemyData.owner = 0;
			enemyData.port = 0;
			enemyData.dynamic = !enemyProto.IsBuilding;
			enemyData.isSpace = true;
			enemyData.localized = true;
			enemyData.stateFlags = 0;
			enemyData.pos = lpos;
			enemyData.rot = lrot;
			return this.AddEnemyDataWithComponents(ref enemyData, hive, 0, false);
		}
		return 0;
	}

	// Token: 0x060019D7 RID: 6615 RVA: 0x001C8580 File Offset: 0x001C6780
	public int CreateEnemyFinal(EnemyDFHiveSystem hive, int protoId, int astroId, int portId, int formTicks)
	{
		EnemyProto enemyProto = LDB.enemies.Select(protoId);
		if (enemyProto != null)
		{
			EnemyData enemyData = default(EnemyData);
			enemyData.protoId = (short)protoId;
			enemyData.modelIndex = (short)enemyProto.ModelIndex;
			enemyData.astroId = astroId;
			enemyData.originAstroId = hive.hiveAstroId;
			enemyData.owner = (short)(hive.hiveAstroId - 1000000);
			enemyData.port = (short)portId;
			enemyData.dynamic = !enemyProto.IsBuilding;
			enemyData.isSpace = true;
			enemyData.localized = true;
			enemyData.stateFlags = 0;
			enemyData.Formation(formTicks, (float)hive.orbitRadius, ref enemyData.pos, ref enemyData.rot, ref enemyData.vel);
			return this.AddEnemyDataWithComponents(ref enemyData, hive, 0, false);
		}
		return 0;
	}

	// Token: 0x060019D8 RID: 6616 RVA: 0x001C864C File Offset: 0x001C684C
	public int CreateEnemyFinal(EnemyDFHiveSystem hive, int protoId, int astroId, int portId, Vector3 pos, Quaternion rot, Vector3 vel)
	{
		EnemyProto enemyProto = LDB.enemies.Select(protoId);
		if (enemyProto != null)
		{
			EnemyData enemyData = default(EnemyData);
			enemyData.protoId = (short)protoId;
			enemyData.modelIndex = (short)enemyProto.ModelIndex;
			enemyData.astroId = astroId;
			enemyData.originAstroId = hive.hiveAstroId;
			enemyData.owner = (short)(hive.hiveAstroId - 1000000);
			enemyData.port = (short)portId;
			enemyData.dynamic = !enemyProto.IsBuilding;
			enemyData.isSpace = true;
			enemyData.localized = true;
			enemyData.stateFlags = 0;
			enemyData.pos = pos;
			enemyData.rot = rot;
			enemyData.vel = vel;
			return this.AddEnemyDataWithComponents(ref enemyData, hive, 0, false);
		}
		return 0;
	}

	// Token: 0x060019D9 RID: 6617 RVA: 0x001C8712 File Offset: 0x001C6912
	public void RemoveEnemyFinal(int enemyId)
	{
		if (enemyId <= 0)
		{
			return;
		}
		this.RemoveEnemyWithComponents(enemyId);
	}

	// Token: 0x060019DA RID: 6618 RVA: 0x001C8720 File Offset: 0x001C6920
	public void KillEnemyFinal(int enemyId, ref CombatStat combatStat)
	{
		if (enemyId <= 0)
		{
			return;
		}
		ref EnemyData ptr = ref this.enemyPool[enemyId];
		if (ptr.dynamic && ptr.port > 0)
		{
			EnemyDFHiveSystem hiveByAstroId = this.GetHiveByAstroId(ptr.originAstroId);
			if (hiveByAstroId != null)
			{
				hiveByAstroId.NotifyUnitKilled(ref ptr);
			}
		}
		if (ptr.dfRelayId > 0)
		{
			EnemyDFHiveSystem hiveByAstroId2 = this.GetHiveByAstroId(ptr.originAstroId);
			if (hiveByAstroId2 != null)
			{
				hiveByAstroId2.NotifyRelayKilled(ref ptr);
			}
		}
		if (ptr.dfSCoreId > 0)
		{
			this.gameData.warningSystem.Broadcast(EBroadcastVocal.DFHiveCoreDestroyed, -1, 0, 0);
		}
		if (ptr.positionIsValid)
		{
			ModelProto modelProto = LDB.models.modelArray[(int)ptr.modelIndex];
			if (modelProto.RuinId > 0)
			{
				this.CreateSpaceRuinFinal(modelProto, ref ptr);
			}
			if (SpaceSector.PrefabDescByModelIndex[(modelProto.RuinId != 0) ? modelProto.RuinId : modelProto.ID].wreckagePrefab != null)
			{
				int stateIndex = 0;
				this.model.AddWreckage(modelProto, stateIndex, ptr.astroId, ref ptr.pos, ref ptr.rot, ref ptr.vel);
			}
		}
		this.RemoveEnemyWithComponents(enemyId);
	}

	// Token: 0x060019DB RID: 6619 RVA: 0x001C882C File Offset: 0x001C6A2C
	public int QueryHiveCountInStar(int starIndex)
	{
		if (!this.isCombatMode || this.dfHives == null)
		{
			return 0;
		}
		int num = 0;
		for (EnemyDFHiveSystem enemyDFHiveSystem = this.dfHives[starIndex]; enemyDFHiveSystem != null; enemyDFHiveSystem = enemyDFHiveSystem.nextSibling)
		{
			num++;
		}
		return num;
	}

	// Token: 0x060019DC RID: 6620 RVA: 0x001C8868 File Offset: 0x001C6A68
	public int QueryNewHiveOrbit(int starIndex)
	{
		int num = 0;
		EnemyDFHiveSystem enemyDFHiveSystem = this.dfHives[starIndex];
		EnemyDFHiveSystem enemyDFHiveSystem2 = null;
		while (enemyDFHiveSystem != null)
		{
			num |= 1 << enemyDFHiveSystem.hiveOrbitIndex;
			enemyDFHiveSystem2 = enemyDFHiveSystem;
			enemyDFHiveSystem = enemyDFHiveSystem.nextSibling;
		}
		if (enemyDFHiveSystem2 == null)
		{
			return 0;
		}
		if (enemyDFHiveSystem2 != null)
		{
			for (int i = 0; i < 8; i++)
			{
				if ((num >> i & 1) == 0)
				{
					return i;
				}
			}
		}
		return -1;
	}

	// Token: 0x060019DD RID: 6621 RVA: 0x001C88C0 File Offset: 0x001C6AC0
	public EnemyDFHiveSystem TryCreateNewHive(StarData star)
	{
		if (star == null)
		{
			return null;
		}
		int num = 0;
		EnemyDFHiveSystem enemyDFHiveSystem = this.dfHives[star.index];
		EnemyDFHiveSystem enemyDFHiveSystem2 = null;
		while (enemyDFHiveSystem != null)
		{
			num |= 1 << enemyDFHiveSystem.hiveOrbitIndex;
			enemyDFHiveSystem2 = enemyDFHiveSystem;
			enemyDFHiveSystem = enemyDFHiveSystem.nextSibling;
		}
		int num2 = (enemyDFHiveSystem2 == null) ? 0 : -1;
		if (enemyDFHiveSystem2 != null)
		{
			for (int i = 0; i < 8; i++)
			{
				if ((num >> i & 1) == 0)
				{
					num2 = i;
					break;
				}
			}
		}
		if (num2 == -1)
		{
			return null;
		}
		int num3 = star.index * 8 + num2 + 1;
		Assert.Null(this.dfHivesByAstro[num3]);
		if (this.dfHivesByAstro[num3] == null)
		{
			EnemyDFHiveSystem enemyDFHiveSystem3 = new EnemyDFHiveSystem();
			enemyDFHiveSystem3.Init(this.gameData, star.id, num2);
			enemyDFHiveSystem3.SetForNewCreate();
			if (enemyDFHiveSystem2 != null)
			{
				enemyDFHiveSystem2.nextSibling = enemyDFHiveSystem3;
				enemyDFHiveSystem3.prevSibling = enemyDFHiveSystem2;
			}
			else
			{
				this.dfHives[star.index] = enemyDFHiveSystem3;
			}
			return enemyDFHiveSystem3;
		}
		return null;
	}

	// Token: 0x060019DE RID: 6622 RVA: 0x001C89A4 File Offset: 0x001C6BA4
	public void RemoveHive(EnemyDFHiveSystem hive)
	{
		if (hive == null)
		{
			return;
		}
		int index = hive.starData.index;
		if (this.dfHives[index] == hive)
		{
			this.dfHives[index] = hive.nextSibling;
		}
		this.dfHivesByAstro[hive.hiveAstroId - 1000000] = null;
		if (hive.prevSibling != null)
		{
			hive.prevSibling.nextSibling = hive.nextSibling;
		}
		if (hive.nextSibling != null)
		{
			hive.nextSibling.prevSibling = hive.prevSibling;
		}
		hive.Free();
	}

	// Token: 0x060019DF RID: 6623 RVA: 0x001C8A28 File Offset: 0x001C6C28
	public void AlterEnemyAstroId(int newAstroId, ref EnemyData enemyData)
	{
		VectorLF3 upos;
		Quaternion urot;
		this.TransformFromAstro(enemyData.astroId, out upos, out urot, enemyData.pos, enemyData.rot);
		enemyData.astroId = newAstroId;
		this.InverseTransformToAstro(newAstroId, upos, urot, out enemyData.pos, out enemyData.rot);
		enemyData.colliderId = this.physics.AlterColliderAstroId(enemyData.colliderId, newAstroId, enemyData.pos, enemyData.rot);
		enemyData.audioId = this.audio.AlterAudioAstroId(enemyData.audioId, newAstroId, enemyData.pos);
	}

	// Token: 0x060019E0 RID: 6624 RVA: 0x001C8AB0 File Offset: 0x001C6CB0
	public void AlterEnemyAstroId(int newAstroId, ref EnemyData enemyData, out VectorLF3 uPos, out Quaternion uRot)
	{
		this.TransformFromAstro(enemyData.astroId, out uPos, out uRot, enemyData.pos, enemyData.rot);
		enemyData.astroId = newAstroId;
		this.InverseTransformToAstro(newAstroId, uPos, uRot, out enemyData.pos, out enemyData.rot);
		enemyData.colliderId = this.physics.AlterColliderAstroId(enemyData.colliderId, newAstroId, enemyData.pos, enemyData.rot);
		enemyData.audioId = this.audio.AlterAudioAstroId(enemyData.audioId, newAstroId, enemyData.pos);
	}

	// Token: 0x060019E1 RID: 6625 RVA: 0x001C8B44 File Offset: 0x001C6D44
	public void AlterCraftAstroId(int newAstroId, ref CraftData craftData)
	{
		VectorLF3 upos;
		Quaternion urot;
		this.TransformFromAstro(craftData.astroId, out upos, out urot, craftData.pos, craftData.rot);
		craftData.astroId = newAstroId;
		this.InverseTransformToAstro(newAstroId, upos, urot, out craftData.pos, out craftData.rot);
		craftData.colliderId = this.physics.AlterColliderAstroId(craftData.colliderId, newAstroId, craftData.pos, craftData.rot);
		craftData.audioId = this.audio.AlterAudioAstroId(craftData.audioId, newAstroId, craftData.pos);
	}

	// Token: 0x060019E2 RID: 6626 RVA: 0x001C8BCC File Offset: 0x001C6DCC
	public void AlterCraftAstroId(int newAstroId, ref CraftData craftData, out VectorLF3 uPos, out Quaternion uRot)
	{
		this.TransformFromAstro(craftData.astroId, out uPos, out uRot, craftData.pos, craftData.rot);
		craftData.astroId = newAstroId;
		this.InverseTransformToAstro(newAstroId, uPos, uRot, out craftData.pos, out craftData.rot);
		craftData.colliderId = this.physics.AlterColliderAstroId(craftData.colliderId, newAstroId, craftData.pos, craftData.rot);
		craftData.audioId = this.audio.AlterAudioAstroId(craftData.audioId, newAstroId, craftData.pos);
	}

	// Token: 0x060019E3 RID: 6627 RVA: 0x001C8C60 File Offset: 0x001C6E60
	public void SetCraftCapacity(int newCapacity)
	{
		CraftData[] array = this.craftPool;
		this.craftPool = new CraftData[newCapacity];
		this.craftRecycle = new int[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.craftPool, (newCapacity > this.craftCapacity) ? this.craftCapacity : newCapacity);
		}
		AnimData[] array2 = this.craftAnimPool;
		this.craftAnimPool = new AnimData[newCapacity];
		if (array2 != null)
		{
			Array.Copy(array2, this.craftAnimPool, (newCapacity > this.craftCapacity) ? this.craftCapacity : newCapacity);
		}
		this.craftCapacity = newCapacity;
	}

	// Token: 0x060019E4 RID: 6628 RVA: 0x001C8CE8 File Offset: 0x001C6EE8
	public int AddCraftData(ref CraftData craft)
	{
		if (this.craftRecycleCursor > 0)
		{
			int[] array = this.craftRecycle;
			int num = this.craftRecycleCursor - 1;
			this.craftRecycleCursor = num;
			craft.id = array[num];
		}
		else
		{
			int num = this.craftCursor;
			this.craftCursor = num + 1;
			craft.id = num;
		}
		if (craft.id == this.craftCapacity)
		{
			this.SetCraftCapacity(this.craftCapacity * 2);
		}
		this.craftPool[craft.id] = craft;
		return craft.id;
	}

	// Token: 0x060019E5 RID: 6629 RVA: 0x001C8D70 File Offset: 0x001C6F70
	public void CreateCraftDisplayComponents(int craftId)
	{
		int modelIndex = (int)this.craftPool[craftId].modelIndex;
		if (modelIndex == 0)
		{
			return;
		}
		PrefabDesc prefabDesc = SpaceSector.PrefabDescByModelIndex[modelIndex];
		if (prefabDesc == null)
		{
			return;
		}
		this.craftPool[craftId].modelId = this.model.gpuiManager.AddModel(modelIndex, craftId, this.craftPool[craftId].astroId, this.craftPool[craftId].pos, this.craftPool[craftId].rot, true);
		this.model.starmapgpuiManager.RegisterModelIndex(modelIndex);
		this.craftPool[craftId].mmblockId = 0;
		if (prefabDesc.colliders != null && prefabDesc.colliders.Length != 0)
		{
			if (this.craftPool[craftId].prototype != ECraftProto.CreationPart)
			{
				for (int i = 0; i < prefabDesc.spacePhysicsColliders.Length; i++)
				{
				}
			}
			else
			{
				for (int j = 0; j < prefabDesc.spacePhysicsColliders.Length; j++)
				{
					this.craftPool[craftId].colliderId = this.physics.AddColliderData(prefabDesc.spacePhysicsColliders[j].BindToObject(craftId, this.craftPool[craftId].colliderId, EObjectType.Craft, this.craftPool[craftId].astroId, this.craftPool[craftId].pos, this.craftPool[craftId].rot));
				}
				for (int k = 0; k < prefabDesc.spaceBuildColliders.Length; k++)
				{
					this.craftPool[craftId].colliderId = this.physics.AddColliderData(prefabDesc.spaceBuildColliders[k].BindToObject(craftId, this.craftPool[craftId].colliderId, EObjectType.Craft, this.craftPool[craftId].astroId, this.craftPool[craftId].pos, this.craftPool[craftId].rot));
				}
			}
		}
		if (prefabDesc.hasAudio)
		{
			this.craftPool[craftId].audioId = this.audio.AddAudioData(craftId, EObjectType.Craft, this.craftPool[craftId].astroId, this.craftPool[craftId].pos, prefabDesc);
		}
	}

	// Token: 0x060019E6 RID: 6630 RVA: 0x001C8FC4 File Offset: 0x001C71C4
	public void CreateCraftLogicComponents(int craftId, PrefabDesc desc)
	{
		if (desc.isFleet)
		{
			this.combatSpaceSystem.NewFleetComponent(craftId, desc);
		}
		if (desc.isCraftUnit)
		{
			this.combatSpaceSystem.NewUnitComponent(craftId, desc);
		}
		if (this.craftPool[craftId].prototype == ECraftProto.Vehicle)
		{
			this.creationSystem.NewVehicleComponent(craftId);
		}
		else if (this.craftPool[craftId].prototype == ECraftProto.CreationPart && desc.creationBlock != null)
		{
			CreationPartPropertyBlock creationBlock = desc.creationBlock;
			if (creationBlock.type < ECreationPart.ConstructAnchor && creationBlock.type > ECreationPart.Unknown)
			{
				this.creationSystem.NewVehiclePartComponent(craftId, desc);
				ECreationPart type = creationBlock.type;
				if (type <= ECreationPart.VDSuspension)
				{
					if (type <= ECreationPart.VDGyroscope)
					{
						if (type != ECreationPart.VDCockpit)
						{
							if (type == ECreationPart.VDGyroscope)
							{
								this.creationSystem.NewVDGyroscopeComponent(craftId, desc);
							}
						}
						else
						{
							this.creationSystem.NewVDCockpitComponent(craftId, desc);
						}
					}
					else if (type != ECreationPart.VDTyre)
					{
						if (type == ECreationPart.VDSuspension)
						{
							this.creationSystem.NewVDSuspensionComponent(craftId, desc);
						}
					}
					else
					{
						this.creationSystem.NewVDTyreComponent(craftId, desc);
					}
				}
				else if (type <= ECreationPart.VDWarp)
				{
					if (type != ECreationPart.VDEngine)
					{
						if (type == ECreationPart.VDWarp)
						{
							this.creationSystem.NewVDWarpComponent(craftId, desc);
						}
					}
					else
					{
						this.creationSystem.NewVDEngineComponent(craftId, desc);
					}
				}
				else
				{
					switch (type)
					{
					case ECreationPart.VDBattery:
						this.creationSystem.NewVDBatteryComponent(craftId, desc);
						break;
					case (ECreationPart)31:
						break;
					case ECreationPart.VDStorage:
						this.creationSystem.NewVDStorageComponent(craftId, desc);
						break;
					case ECreationPart.VDFuelStorage:
						this.creationSystem.NewVDFuelStorageComponent(craftId, desc);
						break;
					case ECreationPart.VDTank:
						this.creationSystem.NewVDTankComponent(craftId, desc);
						break;
					default:
						if (type != ECreationPart.VDConnector)
						{
							switch (type)
							{
							case ECreationPart.VWGauss:
								this.creationSystem.NewVWGaussComponent(craftId, desc);
								break;
							case ECreationPart.VWLaser:
								this.creationSystem.NewVWLaserComponent(craftId, desc);
								break;
							case ECreationPart.VWCannon:
								this.creationSystem.NewVWCannonComponent(craftId, desc);
								break;
							case ECreationPart.VWMissile:
								this.creationSystem.NewVWMissileComponent(craftId, desc);
								break;
							case ECreationPart.VWPlasma:
								this.creationSystem.NewVWPlasmaComponent(craftId, desc);
								break;
							case ECreationPart.VWDisturb:
								this.creationSystem.NewVWDisturbComponent(craftId, desc);
								break;
							case ECreationPart.VWThrow:
								this.creationSystem.NewVWThrowComponent(craftId, desc);
								break;
							case ECreationPart.VWShield:
								this.creationSystem.NewVWShieldComponent(craftId, desc);
								break;
							}
						}
						else
						{
							this.creationSystem.NewVDConnectorComponent(craftId, desc);
						}
						break;
					}
				}
			}
			else if (creationBlock.type == ECreationPart.ConstructAnchor)
			{
				this.creationSystem.NewConstructAnchorComponent(craftId);
			}
		}
		this.craftAnimPool[craftId].time = 0f;
		this.craftAnimPool[craftId].prepare_length = desc.anim_prepare_length;
		this.craftAnimPool[craftId].working_length = desc.anim_working_length;
		this.craftAnimPool[craftId].state = 0U;
		this.craftAnimPool[craftId].power = 0f;
	}

	// Token: 0x060019E7 RID: 6631 RVA: 0x001C9314 File Offset: 0x001C7514
	public int AddCraftDataWithComponents(ref CraftData craft)
	{
		int num = this.AddCraftData(ref craft);
		PrefabDesc prefabDesc = SpaceSector.PrefabDescByModelIndex[(int)craft.modelIndex];
		if (prefabDesc == null)
		{
			return num;
		}
		this.CreateCraftLogicComponents(num, prefabDesc);
		this.creationSystem.physics.SetupCraftPhysics(ref craft);
		if (this.model != null)
		{
			this.CreateCraftDisplayComponents(num);
		}
		this.SetupReferenceOnCraftCreate(num);
		return num;
	}

	// Token: 0x060019E8 RID: 6632 RVA: 0x001C9374 File Offset: 0x001C7574
	public void RemoveCraftWithComponents(int id)
	{
		if (id != 0 && this.craftPool[id].id != 0)
		{
			this.ClearReferencesOnCraftRemove(id);
			this.creationSystem.physics.UnsetCraftPhysics(ref this.craftPool[id]);
			if (this.craftPool[id].combatStatId != 0)
			{
				int combatStatId = this.craftPool[id].combatStatId;
				this.skillSystem.OnRemovingSkillTarget(combatStatId, this.skillSystem.combatStats.buffer[combatStatId].originAstroId, ETargetType.CombatStat);
				this.skillSystem.combatStats.Remove(combatStatId);
				this.craftPool[id].combatStatId = 0;
			}
			if (this.craftPool[id].unitId != 0)
			{
				this.combatSpaceSystem.RemoveUnitComponent(this.craftPool[id].unitId);
				this.craftPool[id].unitId = 0;
			}
			if (this.craftPool[id].fleetId != 0)
			{
				this.combatSpaceSystem.RemoveFleetComponent(this.craftPool[id].fleetId);
				this.craftPool[id].fleetId = 0;
			}
			if (this.craftPool[id].cAnchorId != 0)
			{
				this.creationSystem.RemoveConstructAnchorComponent(this.craftPool[id].cAnchorId);
				this.craftPool[id].cAnchorId = 0;
			}
			if (this.craftPool[id].vehicleId != 0)
			{
				this.creationSystem.RemoveVehicleComponent(this.craftPool[id].vehicleId);
				this.craftPool[id].vehicleId = 0;
			}
			if (this.craftPool[id].vPartId != 0)
			{
				this.creationSystem.RemoveVehiclePartComponent(this.craftPool[id].vPartId);
				this.craftPool[id].vPartId = 0;
			}
			if (this.craftPool[id].vdCockpitId != 0)
			{
				this.creationSystem.RemoveVDCockpitComponent(this.craftPool[id].vdCockpitId);
				this.craftPool[id].vdCockpitId = 0;
			}
			if (this.craftPool[id].vdGyroscopeId != 0)
			{
				this.creationSystem.RemoveVDGyroscopeComponent(this.craftPool[id].vdGyroscopeId);
				this.craftPool[id].vdGyroscopeId = 0;
			}
			if (this.craftPool[id].vdTyreId != 0)
			{
				this.creationSystem.RemoveVDTyreComponent(this.craftPool[id].vdTyreId);
				this.craftPool[id].vdTyreId = 0;
			}
			if (this.craftPool[id].vdSuspensionId != 0)
			{
				this.creationSystem.RemoveVDSuspensionComponent(this.craftPool[id].vdSuspensionId);
				this.craftPool[id].vdSuspensionId = 0;
			}
			if (this.craftPool[id].vdEngineId != 0)
			{
				this.creationSystem.RemoveVDEngineComponent(this.craftPool[id].vdEngineId);
				this.craftPool[id].vdEngineId = 0;
			}
			if (this.craftPool[id].vdWarpId != 0)
			{
				this.creationSystem.RemoveVDWarpComponent(this.craftPool[id].vdWarpId);
				this.craftPool[id].vdWarpId = 0;
			}
			if (this.craftPool[id].vdBatteryId != 0)
			{
				this.creationSystem.RemoveVDBatteryComponent(this.craftPool[id].vdBatteryId);
				this.craftPool[id].vdBatteryId = 0;
			}
			if (this.craftPool[id].vdStorageId != 0)
			{
				this.creationSystem.RemoveVDStorageComponent(this.craftPool[id].vdStorageId);
				this.craftPool[id].vdStorageId = 0;
			}
			if (this.craftPool[id].vdFuelStorageId != 0)
			{
				this.creationSystem.RemoveVDFuelStorageComponent(this.craftPool[id].vdFuelStorageId);
				this.craftPool[id].vdFuelStorageId = 0;
			}
			if (this.craftPool[id].vdTankId != 0)
			{
				this.creationSystem.RemoveVDTankComponent(this.craftPool[id].vdTankId);
				this.craftPool[id].vdTankId = 0;
			}
			if (this.craftPool[id].vdConnectorId != 0)
			{
				this.creationSystem.RemoveVDConnectorComponent(this.craftPool[id].vdConnectorId);
				this.craftPool[id].vdConnectorId = 0;
			}
			if (this.craftPool[id].vwGaussId != 0)
			{
				this.creationSystem.RemoveVWGaussComponent(this.craftPool[id].vwGaussId);
				this.craftPool[id].vwGaussId = 0;
			}
			if (this.craftPool[id].vwLaserId != 0)
			{
				this.creationSystem.RemoveVWLaserComponent(this.craftPool[id].vwLaserId);
				this.craftPool[id].vwLaserId = 0;
			}
			if (this.craftPool[id].vwCannonId != 0)
			{
				this.creationSystem.RemoveVWCannonComponent(this.craftPool[id].vwCannonId);
				this.craftPool[id].vwCannonId = 0;
			}
			if (this.craftPool[id].vwMissileId != 0)
			{
				this.creationSystem.RemoveVWMissileComponent(this.craftPool[id].vwMissileId);
				this.craftPool[id].vwMissileId = 0;
			}
			if (this.craftPool[id].vwPlasmaId != 0)
			{
				this.creationSystem.RemoveVWPlasmaComponent(this.craftPool[id].vwPlasmaId);
				this.craftPool[id].vwPlasmaId = 0;
			}
			if (this.craftPool[id].vwDisturbId != 0)
			{
				this.creationSystem.RemoveVWDisturbComponent(this.craftPool[id].vwDisturbId);
				this.craftPool[id].vwDisturbId = 0;
			}
			if (this.craftPool[id].vwThrowId != 0)
			{
				this.creationSystem.RemoveVWThrowComponent(this.craftPool[id].vwThrowId);
				this.craftPool[id].vwThrowId = 0;
			}
			if (this.craftPool[id].vwShieldId != 0)
			{
				this.creationSystem.RemoveVWShieldComponent(this.craftPool[id].vwShieldId);
				this.craftPool[id].vwShieldId = 0;
			}
			if (this.craftPool[id].modelId != 0)
			{
				this.model.gpuiManager.RemoveModel((int)this.craftPool[id].modelIndex, this.craftPool[id].modelId, true);
				this.craftPool[id].modelId = 0;
			}
			if (this.craftPool[id].mmblockId != 0)
			{
				this.craftPool[id].mmblockId = 0;
			}
			if (this.craftPool[id].colliderId != 0)
			{
				if (this.physics != null)
				{
					this.physics.RemoveLinkedColliderData(this.craftPool[id].colliderId);
				}
				this.craftPool[id].colliderId = 0;
			}
			if (this.craftPool[id].audioId != 0)
			{
				if (this.audio != null)
				{
					this.audio.RemoveAudioData(this.craftPool[id].audioId);
				}
				this.craftPool[id].audioId = 0;
			}
			this.skillSystem.OnRemovingSkillTarget(id, this.craftPool[id].astroId, ETargetType.Craft);
			this.craftPool[id].SetEmpty();
			this.craftAnimPool[id].time = 0f;
			this.craftAnimPool[id].prepare_length = 0f;
			this.craftAnimPool[id].working_length = 0f;
			this.craftAnimPool[id].state = 0U;
			this.craftAnimPool[id].power = 0f;
			int[] array = this.craftRecycle;
			int num = this.craftRecycleCursor;
			this.craftRecycleCursor = num + 1;
			array[num] = id;
		}
		if (this.physics != null)
		{
			this.physics.NotifyObjectRemove(EObjectType.Craft, id);
		}
		if (this.audio != null)
		{
			this.audio.NotifyObjectRemove(EObjectType.Craft, id);
		}
	}

	// Token: 0x060019E9 RID: 6633 RVA: 0x001C9C34 File Offset: 0x001C7E34
	public int CreateCraftFinally(ECraftProto prototype, int protoId, int ownerId, int port, int astroId, VectorLF3 pos, Quaternion rot, Vector3 vel)
	{
		short modelIndex;
		bool dynamic;
		bool isSpace;
		bool isInvincible;
		if (prototype == ECraftProto.Item)
		{
			ItemProto itemProto = LDB.items.Select(protoId);
			if (itemProto == null || !itemProto.isCraft)
			{
				return 0;
			}
			modelIndex = (short)itemProto.ModelIndex;
			dynamic = itemProto.isDynamicCraft;
			isSpace = itemProto.isSpaceCraft;
			isInvincible = false;
			if (this.craftPool[ownerId].id != ownerId)
			{
				return 0;
			}
		}
		else if (prototype == ECraftProto.Fleet)
		{
			FleetProto fleetProto = LDB.fleets.Select(protoId);
			if (fleetProto == null)
			{
				return 0;
			}
			modelIndex = (short)fleetProto.ModelIndex;
			dynamic = true;
			isSpace = fleetProto.IsSpace;
			isInvincible = true;
			if (ownerId == 0)
			{
				return 0;
			}
		}
		else if (prototype == ECraftProto.CreationPart)
		{
			CreationPartProto creationPartProto = LDB.creationParts.Select(protoId);
			if (creationPartProto == null)
			{
				return 0;
			}
			modelIndex = (short)creationPartProto.ModelIndex;
			dynamic = true;
			isSpace = true;
			isInvincible = false;
			if (ownerId == 0)
			{
				return 0;
			}
		}
		else
		{
			if (prototype != ECraftProto.Vehicle)
			{
				return 0;
			}
			modelIndex = 0;
			dynamic = true;
			isSpace = true;
			isInvincible = true;
		}
		CraftData craftData = default(CraftData);
		craftData.protoId = (short)protoId;
		craftData.modelIndex = modelIndex;
		craftData.astroId = astroId;
		craftData.owner = ownerId;
		craftData.port = (short)port;
		craftData.prototype = prototype;
		craftData.dynamic = dynamic;
		craftData.isSpace = isSpace;
		craftData.pos = pos;
		craftData.rot = rot;
		craftData.vel = vel;
		craftData.isInvincible = isInvincible;
		return this.AddCraftDataWithComponents(ref craftData);
	}

	// Token: 0x060019EA RID: 6634 RVA: 0x001C9D77 File Offset: 0x001C7F77
	public void RemoveCraftFinal(int craftId)
	{
		if (craftId <= 0)
		{
			return;
		}
		this.RemoveCraftWithComponents(craftId);
	}

	// Token: 0x060019EB RID: 6635 RVA: 0x001C9D88 File Offset: 0x001C7F88
	public void KillCraftFinal(int craftId, ref CombatStat combatStat)
	{
		if (craftId <= 0)
		{
			return;
		}
		ref CraftData ptr = ref this.craftPool[craftId];
		if (ptr.unitId > 0)
		{
			ref CraftData ptr2 = ref this.craftPool[ptr.owner];
			if (ptr2.owner < 0)
			{
				this.gameData.mainPlayer.mecha.spaceCombatModule.OnFighterKilled((int)ptr2.port, (int)ptr.port);
			}
			else
			{
				int owner = ptr2.owner;
			}
		}
		ModelProto modelProto = LDB.models.modelArray[(int)ptr.modelIndex];
		if (modelProto.RuinId > 0)
		{
			this.CreateSpaceRuinFinal(modelProto, ref ptr);
		}
		if (SpaceSector.PrefabDescByModelIndex[(modelProto.RuinId != 0) ? modelProto.RuinId : modelProto.ID].wreckagePrefab != null)
		{
			int stateIndex = 0;
			this.model.AddWreckage(modelProto, stateIndex, ptr.astroId, ref ptr.pos, ref ptr.rot, ref ptr.vel);
		}
		this.RemoveCraftWithComponents(craftId);
	}

	// Token: 0x060019EC RID: 6636 RVA: 0x001C9E78 File Offset: 0x001C8078
	public void SetupReferenceOnCraftCreate(int newCraftId)
	{
		ref CraftData ptr = ref this.craftPool[newCraftId];
		if (ptr.fleetId > 0)
		{
			ptr.isInvincible = true;
		}
		if (ptr.unitId > 0 && ptr.owner > 0)
		{
			UnitComponent[] buffer = this.combatSpaceSystem.units.buffer;
			int unitId = ptr.unitId;
			ref CraftData ptr2 = ref this.craftPool[ptr.owner];
			if (ptr2.owner == -1)
			{
				this.gameData.mainPlayer.mecha.spaceCombatModule.moduleFleets[(int)ptr2.port].fighters[(int)ptr.port].craftId = ptr.id;
			}
		}
	}

	// Token: 0x060019ED RID: 6637 RVA: 0x001C9F30 File Offset: 0x001C8130
	public void ClearReferencesOnCraftRemove(int removingCraftId)
	{
		ref CraftData ptr = ref this.craftPool[removingCraftId];
		if (ptr.unitId > 0 && ptr.owner > 0)
		{
			ref UnitComponent ptr2 = ref this.combatSpaceSystem.units.buffer[ptr.unitId];
			ref CraftData ptr3 = ref this.craftPool[ptr.owner];
			if (ptr2.behavior == EUnitBehavior.Initialize)
			{
				FleetComponent[] buffer = this.combatSpaceSystem.fleets.buffer;
				int fleetId = ptr3.fleetId;
				buffer[fleetId].currentAssembleUnitsCount = buffer[fleetId].currentAssembleUnitsCount - 1;
			}
			if (ptr2.isCharging)
			{
				ptr2.isCharging = false;
				FleetComponent[] buffer2 = this.combatSpaceSystem.fleets.buffer;
				int fleetId2 = ptr3.fleetId;
				buffer2[fleetId2].currentChargingUnitsCount = buffer2[fleetId2].currentChargingUnitsCount - 1;
			}
			if (ptr3.owner == -1)
			{
				if (!ptr.isSpace)
				{
					this.gameData.mainPlayer.mecha.groundCombatModule.NotifyFighterRemoved((int)ptr3.port, (int)ptr.port, null);
				}
				else
				{
					this.gameData.mainPlayer.mecha.spaceCombatModule.NotifyFighterRemoved((int)ptr3.port, (int)ptr.port, null);
				}
			}
		}
		if (ptr.fleetId > 0)
		{
			ptr.isInvincible = false;
		}
	}

	// Token: 0x060019EE RID: 6638 RVA: 0x001CA065 File Offset: 0x001C8265
	public void RemoveCraftDeferred(int craftId)
	{
		if (this._rmv_id_list == null)
		{
			this._rmv_id_list = new HashSet<int>();
		}
		this._rmv_id_list.Add(craftId);
	}

	// Token: 0x060019EF RID: 6639 RVA: 0x001CA088 File Offset: 0x001C8288
	public void AddCraftDeferred(ECraftProto prototype, int protoId, int ownerId, int port, int astroId, VectorLF3 pos, Quaternion rot, Vector3 vel)
	{
		if (this._add_id_list == null)
		{
			this._add_id_list = new HashSet<ValueTuple<ECraftProto, int, int, int, int, VectorLF3, Quaternion, ValueTuple<Vector3>>>();
		}
		this._add_id_list.Add(new ValueTuple<ECraftProto, int, int, int, int, VectorLF3, Quaternion, ValueTuple<Vector3>>(prototype, protoId, ownerId, port, astroId, pos, rot, new ValueTuple<Vector3>(vel)));
	}

	// Token: 0x060019F0 RID: 6640 RVA: 0x001CA0CC File Offset: 0x001C82CC
	public void ExecuteDeferredCraftChange()
	{
		if (this._rmv_id_list != null && this._rmv_id_list.Count > 0)
		{
			foreach (int id in this._rmv_id_list)
			{
				this.RemoveCraftWithComponents(id);
			}
			this._rmv_id_list.Clear();
		}
		if (this._add_id_list != null && this._add_id_list.Count > 0)
		{
			foreach (ValueTuple<ECraftProto, int, int, int, int, VectorLF3, Quaternion, ValueTuple<Vector3>> valueTuple in this._add_id_list)
			{
				this.CreateCraftFinally(valueTuple.Item1, valueTuple.Item2, valueTuple.Item3, valueTuple.Item4, valueTuple.Item5, valueTuple.Item6, valueTuple.Item7, valueTuple.Rest.Item1);
			}
			this._add_id_list.Clear();
		}
	}

	// Token: 0x060019F1 RID: 6641 RVA: 0x001CA1DC File Offset: 0x001C83DC
	public void CreateSpaceRuinFinal(ModelProto modelProto, ref EnemyData enemy)
	{
		int num;
		if (enemy.dfRelayId > 0)
		{
			if (this.dfHivesByAstro[enemy.originAstroId - 1000000].relays.buffer[enemy.dfRelayId].stage == 2)
			{
				num = modelProto.RuinId;
			}
			else
			{
				num = modelProto.RuinId + 1;
			}
		}
		else if (enemy.dfTinderId > 0)
		{
			ref DFTinderComponent ptr = ref this.dfHivesByAstro[enemy.originAstroId - 1000000].tinders.buffer[enemy.dfTinderId];
			if (ptr.stage == -2 || ptr.stage == 2)
			{
				num = modelProto.RuinId;
			}
			else
			{
				num = modelProto.RuinId + 1;
			}
		}
		else
		{
			num = modelProto.RuinId;
		}
		ModelProto modelProto2 = LDB.models.modelArray[num];
		if (modelProto2 != null)
		{
			SpaceRuinData spaceRuinData = default(SpaceRuinData);
			spaceRuinData.modelIndex = (short)modelProto2.prefabDesc.modelIndex;
			spaceRuinData.astroId = enemy.astroId;
			spaceRuinData.lifeTime = modelProto2.RuinLifeTime;
			spaceRuinData.pos = enemy.pos;
			spaceRuinData.rot = enemy.rot;
			spaceRuinData.renderingData = default(SpaceRuinRenderingData);
			spaceRuinData.renderingData.createTime = (uint)(GameMain.gameTick & 2147483647L);
			spaceRuinData.renderingData.vel = enemy.vel;
			this.AddSpaceRuinDataWithComponent(ref spaceRuinData);
		}
	}

	// Token: 0x060019F2 RID: 6642 RVA: 0x001CA340 File Offset: 0x001C8540
	public void CreateSpaceRuinFinal(ModelProto modelProto, ref CraftData craft)
	{
		int ruinId = modelProto.RuinId;
		ModelProto modelProto2 = LDB.models.modelArray[ruinId];
		if (modelProto2 != null)
		{
			SpaceRuinData spaceRuinData = default(SpaceRuinData);
			spaceRuinData.modelIndex = (short)modelProto2.prefabDesc.modelIndex;
			spaceRuinData.astroId = craft.astroId;
			spaceRuinData.lifeTime = modelProto2.RuinLifeTime;
			spaceRuinData.pos = craft.pos;
			spaceRuinData.rot = craft.rot;
			spaceRuinData.renderingData = default(SpaceRuinRenderingData);
			spaceRuinData.renderingData.createTime = (uint)(GameMain.gameTick & 2147483647L);
			spaceRuinData.renderingData.vel = craft.vel;
			this.AddSpaceRuinDataWithComponent(ref spaceRuinData);
		}
	}

	// Token: 0x060019F3 RID: 6643 RVA: 0x001CA3FC File Offset: 0x001C85FC
	public int AddSpaceRuinDataWithComponent(ref SpaceRuinData ruin)
	{
		ref SpaceRuinData ptr = ref this.spaceRuins.Add();
		ruin.id = ptr.id;
		ptr = ruin;
		if (this.model != null)
		{
			this.CreateSpaceRuinDisplayComponent(ref ptr);
		}
		return ptr.id;
	}

	// Token: 0x060019F4 RID: 6644 RVA: 0x001CA448 File Offset: 0x001C8648
	public void CreateSpaceRuinDisplayComponent(ref SpaceRuinData ruin)
	{
		int modelIndex = (int)ruin.modelIndex;
		ModelProto modelProto = LDB.models.modelArray[modelIndex];
		if (modelProto == null)
		{
			return;
		}
		if (modelProto.prefabDesc == null)
		{
			return;
		}
		ruin.modelId = this.model.gpuiManager.AddModel(modelIndex, ruin.id, ruin.astroId, ruin.pos, ruin.rot, true);
	}

	// Token: 0x060019F5 RID: 6645 RVA: 0x001CA4A8 File Offset: 0x001C86A8
	public void RemoveSpaceRuinWithComponet(int id)
	{
		ref SpaceRuinData ptr = ref this.spaceRuins.buffer[id];
		if (id != 0 && ptr.id != 0)
		{
			if (ptr.modelId != 0)
			{
				this.model.gpuiManager.RemoveModel((int)ptr.modelIndex, ptr.modelId, true);
				ptr.modelId = 0;
			}
			this.skillSystem.OnRemovingSkillTarget(id, ptr.astroId, ETargetType.Ruin);
			this.spaceRuins.Remove(id);
		}
	}

	// Token: 0x0400207D RID: 8317
	public const int kAstroIdBase = 1000000;

	// Token: 0x0400207E RID: 8318
	public GameData gameData;

	// Token: 0x0400207F RID: 8319
	public GalaxyData galaxy;

	// Token: 0x04002080 RID: 8320
	public bool isCombatMode;

	// Token: 0x04002081 RID: 8321
	public AstroData[] galaxyAstros;

	// Token: 0x04002082 RID: 8322
	public AstroData[] astros;

	// Token: 0x04002083 RID: 8323
	public int astroCursor = 1;

	// Token: 0x04002084 RID: 8324
	public EnemyData[] enemyPool;

	// Token: 0x04002085 RID: 8325
	public AnimData[] enemyAnimPool;

	// Token: 0x04002086 RID: 8326
	public int enemyCursor = 1;

	// Token: 0x04002087 RID: 8327
	private int enemyCapacity;

	// Token: 0x04002088 RID: 8328
	private int[] enemyRecycle;

	// Token: 0x04002089 RID: 8329
	private int enemyRecycleCursor;

	// Token: 0x0400208A RID: 8330
	public CraftData[] craftPool;

	// Token: 0x0400208B RID: 8331
	public AnimData[] craftAnimPool;

	// Token: 0x0400208C RID: 8332
	public int craftCursor = 1;

	// Token: 0x0400208D RID: 8333
	private int craftCapacity;

	// Token: 0x0400208E RID: 8334
	private int[] craftRecycle;

	// Token: 0x0400208F RID: 8335
	private int craftRecycleCursor;

	// Token: 0x04002090 RID: 8336
	public DataPool<SpaceRuinData> spaceRuins;

	// Token: 0x04002091 RID: 8337
	public SectorModel model;

	// Token: 0x04002092 RID: 8338
	public SectorPhysics physics;

	// Token: 0x04002093 RID: 8339
	public SectorAudio audio;

	// Token: 0x04002094 RID: 8340
	public SkillSystem skillSystem;

	// Token: 0x04002095 RID: 8341
	public EnemyDFHiveSystem[] dfHives;

	// Token: 0x04002096 RID: 8342
	public EnemyDFHiveSystem[] dfHivesByAstro;

	// Token: 0x04002097 RID: 8343
	public CombatSpaceSystem combatSpaceSystem;

	// Token: 0x04002098 RID: 8344
	public CreationSystem creationSystem;

	// Token: 0x04002099 RID: 8345
	public static PrefabDesc[] PrefabDescByModelIndex;

	// Token: 0x0400209A RID: 8346
	private HashSet<int> _rmv_id_list;

	// Token: 0x0400209B RID: 8347
	[TupleElementNames(new string[]
	{
		"prototype",
		"protoId",
		"ownerId",
		"port",
		"astroId",
		"pos",
		"rot",
		"vel",
		null
	})]
	private HashSet<ValueTuple<ECraftProto, int, int, int, int, VectorLF3, Quaternion, ValueTuple<Vector3>>> _add_id_list;
}
