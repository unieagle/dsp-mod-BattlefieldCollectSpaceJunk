using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

// Token: 0x02000174 RID: 372
public class EnemyDFGroundSystem
{
	// Token: 0x06000C23 RID: 3107 RVA: 0x000B4F74 File Offset: 0x000B3174
	public EnemyDFGroundSystem(PlanetData _planet)
	{
		this.planet = _planet;
		this.factory = this.planet.factory;
		this.gameData = this.factory.gameData;
		this.builders = new DataPool<EnemyBuilderComponent>();
		this.bases = new ObjectPool<DFGBaseComponent>();
		this.connectors = new DataPool<DFGConnectorComponent>();
		this.replicators = new DataPool<DFGReplicatorComponent>();
		this.turrets = new DataPool<DFGTurretComponent>();
		this.shields = new DataPool<DFGShieldComponent>();
		this.units = new DataPool<EnemyUnitComponent>();
		this.builders.Reset();
		this.bases.Reset();
		this.connectors.Reset();
		this.replicators.Reset();
		this.turrets.Reset();
		this.shields.Reset();
		this.units.Reset();
		this.rtseed = (_planet.seed & 65535);
		this.maxAssaultWaves = ((this.planet.factory.index == 0) ? 0 : 1);
		ThemeProto themeProto = LDB.themes.Select(this.planet.theme);
		if (themeProto != null)
		{
			this.enemyDropMask = (1 << themeProto.EigenBit | 16777216);
		}
		this.enemyDropMultiplier = this.gameData.gameDesc.enemyDropMultiplier;
		this.LoadPattern();
	}

	// Token: 0x06000C24 RID: 3108 RVA: 0x000B5120 File Offset: 0x000B3320
	public EnemyDFGroundSystem(PlanetData _planet, bool import)
	{
		this.planet = _planet;
		this.factory = this.planet.factory;
		this.gameData = this.factory.gameData;
		this.builders = new DataPool<EnemyBuilderComponent>();
		this.bases = new ObjectPool<DFGBaseComponent>();
		this.connectors = new DataPool<DFGConnectorComponent>();
		this.replicators = new DataPool<DFGReplicatorComponent>();
		this.turrets = new DataPool<DFGTurretComponent>();
		this.shields = new DataPool<DFGShieldComponent>();
		this.units = new DataPool<EnemyUnitComponent>();
		ThemeProto themeProto = LDB.themes.Select(this.planet.theme);
		if (themeProto != null)
		{
			this.enemyDropMask = (1 << themeProto.EigenBit | 16777216);
		}
		this.enemyDropMultiplier = this.gameData.gameDesc.enemyDropMultiplier;
		this.LoadPattern();
	}

	// Token: 0x06000C25 RID: 3109 RVA: 0x000B5254 File Offset: 0x000B3454
	public void Free()
	{
		this.builders.Free();
		this.bases.Free();
		this.connectors.Free();
		this.replicators.Free();
		this.turrets.Free();
		this.shields.Free();
		this.units.Free();
		this.builders = null;
		this.bases = null;
		this.connectors = null;
		this.replicators = null;
		this.turrets = null;
		this.shields = null;
		this.units = null;
		this.planet = null;
		this.factory = null;
		this.gameData = null;
	}

	// Token: 0x17000189 RID: 393
	// (get) Token: 0x06000C26 RID: 3110 RVA: 0x000B52F4 File Offset: 0x000B34F4
	public bool isLocalLoaded
	{
		get
		{
			return this.gameData.localLoadedPlanetFactory == this.factory;
		}
	}

	// Token: 0x14000009 RID: 9
	// (add) Token: 0x06000C27 RID: 3111 RVA: 0x000B530C File Offset: 0x000B350C
	// (remove) Token: 0x06000C28 RID: 3112 RVA: 0x000B5344 File Offset: 0x000B3544
	public event Action onBaseRemoved;

	// Token: 0x06000C29 RID: 3113 RVA: 0x000B537C File Offset: 0x000B357C
	public void Export(BinaryWriter w)
	{
		w.Write(0);
		w.Write(this.rtseed);
		w.Write(this.rtseed_lehmer);
		w.Write(this.maxAssaultWaves);
		this.builders.Export(w);
		this.bases.Export(w);
		this.connectors.Export(w);
		this.replicators.Export(w);
		this.turrets.Export(w);
		this.shields.Export(w);
		this.units.Export(w);
	}

	// Token: 0x06000C2A RID: 3114 RVA: 0x000B5408 File Offset: 0x000B3608
	public void Import(BinaryReader r)
	{
		r.ReadInt32();
		this.rtseed = r.ReadInt32();
		this.rtseed_lehmer = r.ReadUInt32();
		this.maxAssaultWaves = r.ReadInt32();
		this.builders.Import(r);
		this.bases.Import(r);
		for (int i = 1; i < this.bases.cursor; i++)
		{
			if (this.bases[i] != null && this.bases[i].id == i)
			{
				this.bases[i].groundSystem = this;
			}
		}
		this.connectors.Import(r);
		this.replicators.Import(r);
		for (int j = 1; j < this.builders.cursor; j++)
		{
			if (this.builders.buffer[j].id == j && this.builders.buffer[j].maxMatter <= 100)
			{
				this.builders.buffer[j].maxMatter = 120;
			}
		}
		this.turrets.Import(r);
		this.AfterTurretsImport();
		this.shields.Import(r);
		this.units.Import(r);
		this.CalcFormsSupply();
		this.CreateTruckSegmentBuffer();
	}

	// Token: 0x06000C2B RID: 3115 RVA: 0x000B5554 File Offset: 0x000B3754
	private void LoadPattern()
	{
		if (EnemyDFGroundSystem.patterns == null)
		{
			TextAsset[] dfGroundGrowthPatterns = Configs.builtin.dfGroundGrowthPatterns;
			EnemyDFGroundSystem.patterns = new GrowthPattern_DFGround[dfGroundGrowthPatterns.Length];
			for (int i = 0; i < dfGroundGrowthPatterns.Length; i++)
			{
				EnemyDFGroundSystem.patterns[i] = new GrowthPattern_DFGround();
				EnemyDFGroundSystem.patterns[i].LoadFromAsset(dfGroundGrowthPatterns[i]);
			}
		}
	}

	// Token: 0x06000C2C RID: 3116 RVA: 0x000B55AC File Offset: 0x000B37AC
	public GrowthPattern_DFGround.Builder[] BakePatternBuilders(int index, int seed, Vector3 lpos, float yaw)
	{
		this.LoadPattern();
		float d = this.planet.realRadius + 0.2f;
		EnemyDFGroundSystem.patterns[index].Bake(seed, lpos.normalized * d, yaw);
		GrowthPattern_DFGround.Builder[] result = EnemyDFGroundSystem.patterns[index].builders;
		EnemyDFGroundSystem.patterns[index].ClearBakedInfos();
		return result;
	}

	// Token: 0x06000C2D RID: 3117 RVA: 0x000B5608 File Offset: 0x000B3808
	public bool CanEraseBase(DFGBaseComponent _base)
	{
		if (_base == null || _base.id == 0)
		{
			return true;
		}
		if (this.builders.buffer[_base.builderId].sp > 0)
		{
			return false;
		}
		GrowthPattern_DFGround.Builder[] pbuilders = _base.pbuilders;
		for (int i = 2; i < pbuilders.Length; i++)
		{
			if (pbuilders[i].instId > 0)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000C2E RID: 3118 RVA: 0x000B566C File Offset: 0x000B386C
	public int NewEnemyBuilderComponent(int enemyId, PrefabDesc desc, int builderIndex)
	{
		ref EnemyBuilderComponent ptr = ref this.builders.Add();
		ref EnemyData ptr2 = ref this.factory.enemyPool[enemyId];
		ptr.enemyId = enemyId;
		ptr.matter = 0;
		ptr.energy = 0;
		ptr.minMatter = desc.enemyMinMatter;
		ptr.minEnergy = desc.enemyMinEnergy;
		ptr.maxMatter = desc.enemyMaxMatter;
		ptr.maxEnergy = desc.enemyMaxEnergy;
		ptr.genMatter = desc.enemyGenMatter;
		ptr.genEnergy = desc.enemyGenEnergy;
		ptr.sp = 0;
		ptr.spMax = desc.enemySpMax;
		ptr.spMatter = desc.enemySpMatter;
		ptr.spEnergy = desc.enemySpEnergy;
		ptr.state = 0;
		ptr.idleEnergy = desc.enemyIdleEnergy;
		ptr.workEnergy = desc.enemyWorkEnergy;
		ptr.builderIndex = builderIndex;
		ptr.buildCursor = 0;
		ptr.buildChance = 0;
		ptr.buildCDTime = 0;
		if (builderIndex > 0)
		{
			DFGBaseComponent dfgbaseComponent = this.bases.buffer[(int)ptr2.owner];
			GrowthPattern_DFGround.Builder[] array = (dfgbaseComponent != null) ? dfgbaseComponent.pbuilders : null;
			if (array[builderIndex].matterProvided == 0)
			{
				ptr.SetPattern(ref array[builderIndex]);
			}
			else
			{
				ptr.SetPatternMatter(ref array[builderIndex]);
			}
			array[builderIndex].matterProvided = 0;
		}
		else
		{
			ptr.sp = ptr.spMax;
		}
		ptr2.builderId = ptr.id;
		return ptr.id;
	}

	// Token: 0x06000C2F RID: 3119 RVA: 0x000B57D4 File Offset: 0x000B39D4
	public void RemoveEnemyBuilderComponent(int id)
	{
		this.builders.Remove(id);
	}

	// Token: 0x06000C30 RID: 3120 RVA: 0x000B57E4 File Offset: 0x000B39E4
	public int NewDFGBaseComponent(int enemyId, PrefabDesc desc, DFRelayComponent relay, GrowthPattern_DFGround.Builder[] pbuilders)
	{
		DFGBaseComponent dfgbaseComponent = this.bases.Add();
		dfgbaseComponent.enemyId = enemyId;
		dfgbaseComponent.builderId = 0;
		dfgbaseComponent.relayId = relay.id;
		dfgbaseComponent.relayEnemyId = relay.enemyId;
		dfgbaseComponent.hiveAstroId = relay.hiveAstroId;
		dfgbaseComponent.groundSystem = this;
		dfgbaseComponent.pbuilders = pbuilders;
		dfgbaseComponent.ticks = relay.baseTicks;
		dfgbaseComponent.InitFormations();
		dfgbaseComponent.evolve = relay.baseEvolve;
		dfgbaseComponent.evolve.threat = 0;
		dfgbaseComponent.evolve.waves = this.maxAssaultWaves;
		dfgbaseComponent.evolve.maxThreat = EvolveData.GetGroundThreatMaxByWaves(dfgbaseComponent.evolve.waves, this.gameData.history.combatSettings.aggressiveLevel);
		dfgbaseComponent.InitIncomingSkills();
		relay.baseId = dfgbaseComponent.id;
		this.factory.enemyPool[enemyId].dfGBaseId = dfgbaseComponent.id;
		return dfgbaseComponent.id;
	}

	// Token: 0x06000C31 RID: 3121 RVA: 0x000B58DD File Offset: 0x000B3ADD
	public void RemoveDFGBaseComponent(int id)
	{
		this.bases.SetNull(id);
	}

	// Token: 0x06000C32 RID: 3122 RVA: 0x000B58EC File Offset: 0x000B3AEC
	public int NewDFGConnectorComponent(int enemyId, PrefabDesc desc)
	{
		ref DFGConnectorComponent ptr = ref this.connectors.Add();
		ptr.enemyId = enemyId;
		ptr.builderId = this.factory.enemyPool[enemyId].builderId;
		int owner = (int)this.factory.enemyPool[enemyId].owner;
		DFGBaseComponent dfgbaseComponent = this.bases.buffer[owner];
		GrowthPattern_DFGround.Builder[] array = (dfgbaseComponent != null) ? dfgbaseComponent.pbuilders : null;
		int builderIndex = this.builders.buffer[ptr.builderId].builderIndex;
		ptr.length = array[builderIndex].length;
		int num = (int)((array[builderIndex].pos - array[1].pos).magnitude / 11f + 0.5f);
		int num2 = (num == 0) ? 4 : ((num == 1) ? 2 : 1);
		ptr.matterSpeed = 5 * num2;
		ptr.baseId = owner;
		ptr.builderAId = array[array[builderIndex].connections[0]].instBuilderId;
		ptr.builderBId = array[array[builderIndex].connections[1]].instBuilderId;
		this.factory.enemyPool[enemyId].dfGConnectorId = ptr.id;
		this.factory.enemyAnimPool[enemyId].working_length = 0f;
		return ptr.id;
	}

	// Token: 0x06000C33 RID: 3123 RVA: 0x000B5A5A File Offset: 0x000B3C5A
	public void RemoveDFGConnectorComponent(int id)
	{
		this.connectors.Remove(id);
	}

	// Token: 0x06000C34 RID: 3124 RVA: 0x000B5A68 File Offset: 0x000B3C68
	public int NewDFGReplicatorComponent(int enemyId, PrefabDesc desc)
	{
		ref DFGReplicatorComponent ptr = ref this.replicators.Add();
		ref EnemyData ptr2 = ref this.factory.enemyPool[enemyId];
		ptr.enemyId = enemyId;
		ptr.builderId = ptr2.builderId;
		ptr.baseId = (int)ptr2.owner;
		ptr.productId = desc.dfReplicatorProductId;
		ptr.productFormId = DFGReplicatorComponent.ProductToForm(ptr.productId);
		ptr.productSpMatter = desc.dfReplicatorProductSpMatter;
		ptr.productSpEnergy = desc.dfReplicatorProductSpEnergy;
		ptr.productSpMax = desc.dfReplicatorProductSpMax;
		ptr.turboSpeed = desc.dfReplicatorTurboSpeed;
		ptr.unitSupply = desc.dfReplicatorUnitSupply;
		ptr.productInitialPos = ptr2.pos + ptr2.rot * desc.dfReplicatorProductInitialPos;
		ptr.productInitialRot = ptr2.rot * desc.dfReplicatorProductInitialRot;
		ptr.productInitialVel = ptr2.rot * desc.dfReplicatorProductInitialVel;
		ptr.productInitialTick = desc.dfReplicatorProductInitialTick;
		DFGBaseComponent dfgbaseComponent = this.bases.buffer[ptr.baseId];
		if (dfgbaseComponent != null)
		{
			int builderIndex = this.builders.buffer[ptr.builderId].builderIndex;
			GrowthPattern_DFGround.Builder[] pbuilders = dfgbaseComponent.pbuilders;
			int num = pbuilders[builderIndex].workTicks;
			int num2 = ptr.unitSupply * ptr.productSpMax;
			if (num > num2)
			{
				num = num2;
			}
			int num3 = num / ptr.productSpMax;
			ptr.productSp = num % ptr.productSpMax;
			pbuilders[builderIndex].workTicks = 0;
			for (int i = 0; i < num3; i++)
			{
				dfgbaseComponent.forms[ptr.productFormId].AddUnit();
			}
		}
		ptr2.dfGReplicatorId = ptr.id;
		return ptr.id;
	}

	// Token: 0x06000C35 RID: 3125 RVA: 0x000B5C2A File Offset: 0x000B3E2A
	public void RemoveDFGReplicatorComponent(int id)
	{
		this.replicators.Remove(id);
	}

	// Token: 0x06000C36 RID: 3126 RVA: 0x000B5C38 File Offset: 0x000B3E38
	public int NewDFGTurretComponent(int enemyId, PrefabDesc desc)
	{
		ref DFGTurretComponent ptr = ref this.turrets.Add();
		ref EnemyData ptr2 = ref this.factory.enemyPool[enemyId];
		ptr.enemyId = enemyId;
		ptr.builderId = ptr2.builderId;
		ptr.baseId = (int)ptr2.owner;
		ptr.state = EDFTurretState.None;
		ptr.type = desc.dfTurretType;
		ptr.attackRange = desc.dfTurretAttackRange;
		ptr.sensorRange = desc.dfTurretSensorRange;
		ptr.rangeInc = desc.dfTurretRangeInc;
		ptr.muzzleY = desc.dfTurretMuzzleY;
		ptr.TurretDataSetup(ref ptr2);
		ptr.localDir = Vector3.forward;
		ptr2.dfGTurretId = ptr.id;
		return ptr.id;
	}

	// Token: 0x06000C37 RID: 3127 RVA: 0x000B5CE9 File Offset: 0x000B3EE9
	public void RemoveDFGTurretComponent(int id)
	{
		this.turrets.Remove(id);
	}

	// Token: 0x06000C38 RID: 3128 RVA: 0x000B5CF8 File Offset: 0x000B3EF8
	public void AfterTurretsImport()
	{
		int cursor = this.turrets.cursor;
		DFGTurretComponent[] buffer = this.turrets.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref DFGTurretComponent ptr = ref buffer[i];
			if (ptr.id == i)
			{
				ptr.TurretDataSetup(ref this.factory.enemyPool[ptr.enemyId]);
			}
		}
	}

	// Token: 0x06000C39 RID: 3129 RVA: 0x000B5D58 File Offset: 0x000B3F58
	public int NewDFGShieldComponent(int enemyId, PrefabDesc desc)
	{
		ref DFGShieldComponent ptr = ref this.shields.Add();
		ptr.enemyId = enemyId;
		ptr.builderId = this.factory.enemyPool[enemyId].builderId;
		ptr.baseId = (int)this.factory.enemyPool[enemyId].owner;
		this.factory.enemyPool[enemyId].dfGShieldId = ptr.id;
		return ptr.id;
	}

	// Token: 0x06000C3A RID: 3130 RVA: 0x000B5DD2 File Offset: 0x000B3FD2
	public void RemoveDFGShieldComponent(int id)
	{
		this.shields.Remove(id);
	}

	// Token: 0x06000C3B RID: 3131 RVA: 0x000B5DE0 File Offset: 0x000B3FE0
	public int NewEnemyUnitComponent(int enemyId)
	{
		ref EnemyUnitComponent ptr = ref this.units.Add();
		ref EnemyData ptr2 = ref this.factory.enemyPool[enemyId];
		ptr.planetId = this.planet.id;
		ptr.enemyId = enemyId;
		ptr.protoId = (int)ptr2.protoId;
		ptr.baseId = (int)ptr2.owner;
		DFGBaseComponent dfgbaseComponent = this.bases.buffer[ptr.baseId];
		if (dfgbaseComponent != null)
		{
			ptr.baseEnemyId = dfgbaseComponent.enemyId;
			ptr.level = dfgbaseComponent.evolve.level;
		}
		ptr2.unitId = ptr.id;
		return ptr.id;
	}

	// Token: 0x06000C3C RID: 3132 RVA: 0x000B5E80 File Offset: 0x000B4080
	public void RemoveEnemyUnitComponent(int id)
	{
		this.units.Remove(id);
	}

	// Token: 0x06000C3D RID: 3133 RVA: 0x000B5E90 File Offset: 0x000B4090
	public void SetupReferenceOnEnemyCreate(int newEnemyId)
	{
		ref EnemyData ptr = ref this.factory.enemyPool[newEnemyId];
		GrowthPattern_DFGround.Builder[] pbuilders = this.bases.buffer[(int)ptr.owner].pbuilders;
		int builderId = ptr.builderId;
		if (builderId > 0)
		{
			int builderIndex = this.builders.buffer[builderId].builderIndex;
			if (builderIndex > 0)
			{
				pbuilders[builderIndex].instId = newEnemyId;
				pbuilders[builderIndex].instBuilderId = builderId;
				this.builders.buffer[builderId].RefreshAnimation_Ground(pbuilders, ref this.factory.enemyAnimPool[newEnemyId], true);
				int[] connections = pbuilders[builderIndex].connections;
				DFGConnectorComponent[] buffer = this.connectors.buffer;
				for (int i = 0; i < connections.Length; i++)
				{
					int instId = pbuilders[connections[i]].instId;
					if (instId > 0)
					{
						ref EnemyData ptr2 = ref this.factory.enemyPool[instId];
						if (ptr2.dfGConnectorId > 0)
						{
							ref DFGConnectorComponent ptr3 = ref buffer[ptr2.dfGConnectorId];
							if (pbuilders[connections[i]].connections[0] == builderIndex)
							{
								ptr3.builderAId = builderId;
							}
							if (pbuilders[connections[i]].connections[1] == builderIndex)
							{
								ptr3.builderBId = builderId;
							}
						}
					}
				}
				if (this.isLocalLoaded)
				{
					this.planet.factoryModel.dfGroundRenderer.SetPBuildersInstId((int)ptr.owner, builderIndex, newEnemyId);
				}
			}
		}
		int dfGBaseId = ptr.dfGBaseId;
		if (dfGBaseId > 0)
		{
			this.AddTruckSegmentsForNewBase(this.bases[dfGBaseId]);
			if (this.isLocalLoaded)
			{
				this.planet.factoryModel.dfGroundRenderer.CollectPBuilderInsts(dfGBaseId);
			}
		}
		int dfGConnectorId = ptr.dfGConnectorId;
		if (dfGConnectorId > 0)
		{
			ref DFGConnectorComponent ptr4 = ref this.connectors.buffer[dfGConnectorId];
			this.factory.enemyAnimPool[newEnemyId].prepare_length = ((ptr4.builderBId > 0 && ptr4.builderAId == 0) ? (-ptr4.length) : ptr4.length);
			ref GrowthPattern_DFGround.Builder ptr5 = ref pbuilders[this.builders.buffer[builderId].builderIndex];
			if (this.truckSegments != null)
			{
				int truckSegOffset = ptr5.truckSegOffset;
				int num = truckSegOffset + (int)(ptr4.length / 3f + 1f);
				for (int j = truckSegOffset; j < num; j++)
				{
					this.truckSegments[j] = 0;
				}
			}
		}
		if (!ptr.dynamic)
		{
			this.factory.powerSystem.SetGeothermalAffectStrength(newEnemyId, 0);
		}
	}

	// Token: 0x06000C3E RID: 3134 RVA: 0x000B6128 File Offset: 0x000B4328
	public void ClearReferencesOnEnemyRemove(int removingEnemyId)
	{
		ref EnemyData ptr = ref this.factory.enemyPool[removingEnemyId];
		GrowthPattern_DFGround.Builder[] pbuilders = this.bases.buffer[(int)ptr.owner].pbuilders;
		int builderId = ptr.builderId;
		if (builderId > 0)
		{
			int builderIndex = this.builders.buffer[builderId].builderIndex;
			if (builderIndex > 0)
			{
				int[] connections = pbuilders[builderIndex].connections;
				DFGConnectorComponent[] buffer = this.connectors.buffer;
				for (int i = 0; i < connections.Length; i++)
				{
					int instId = pbuilders[connections[i]].instId;
					if (instId > 0)
					{
						ref EnemyData ptr2 = ref this.factory.enemyPool[instId];
						if (ptr2.dfGConnectorId > 0)
						{
							ref DFGConnectorComponent ptr3 = ref buffer[ptr2.dfGConnectorId];
							if (ptr3.builderAId == builderId)
							{
								ptr3.builderAId = 0;
								if (ptr3.builderBId == 0)
								{
									this.RemoveEnemyDeferred(instId);
								}
								else
								{
									this.factory.enemyAnimPool[instId].prepare_length = -ptr3.length;
								}
							}
							if (ptr3.builderBId == builderId)
							{
								ptr3.builderBId = 0;
								if (ptr3.builderAId == 0)
								{
									this.RemoveEnemyDeferred(instId);
								}
								else
								{
									this.factory.enemyAnimPool[instId].prepare_length = ptr3.length;
								}
							}
						}
						if (ptr2.builderId > 0)
						{
							ref EnemyBuilderComponent ptr4 = ref this.builders.buffer[ptr2.builderId];
							if (ptr4.id == ptr2.builderId && (ptr4.buildCursor == 0 || ptr4.buildCursor == builderId))
							{
								ptr4.buildCDTime = 20;
							}
						}
					}
				}
				if (this.isLocalLoaded)
				{
					this.planet.factoryModel.dfGroundRenderer.SetPBuildersInstId((int)ptr.owner, builderIndex, 0);
				}
				pbuilders[builderIndex].instId = 0;
				pbuilders[builderIndex].instBuilderId = 0;
			}
		}
		int dfGBaseId = ptr.dfGBaseId;
		if (dfGBaseId > 0)
		{
			this.NotifyBaseRemoving(dfGBaseId);
		}
		if (!ptr.dynamic)
		{
			this.factory.powerSystem.SetGeothermalAffectStrength(0, removingEnemyId);
		}
		this.factory.defenseSystem.RemoveGlobalTargets(ETargetType.Enemy, removingEnemyId);
	}

	// Token: 0x06000C3F RID: 3135 RVA: 0x000B6364 File Offset: 0x000B4564
	public void NotifyBaseKilled(int baseId, int ruinId)
	{
		DFGBaseComponent dfgbaseComponent = this.bases.buffer[baseId];
		if (baseId == 0 || dfgbaseComponent == null)
		{
			return;
		}
		if (dfgbaseComponent.ruinId == 0)
		{
			dfgbaseComponent.ruinId = ruinId;
		}
		if (this.CanEraseBase(dfgbaseComponent))
		{
			return;
		}
		DFRelayComponent relay = dfgbaseComponent.GetRelay();
		this.builders.buffer[dfgbaseComponent.builderId].state = 0;
		this.builders.buffer[dfgbaseComponent.builderId].sp = 0;
		this.builders.buffer[dfgbaseComponent.builderId].matter = 0;
		this.builders.buffer[dfgbaseComponent.builderId].energy = 0;
		dfgbaseComponent.disableAssaultingAlert = 0;
		if (relay != null)
		{
			relay.baseRespawnCD = 120;
		}
		int[] connections = dfgbaseComponent.pbuilders[1].connections;
		for (int i = 0; i < connections.Length; i++)
		{
			int instId = dfgbaseComponent.pbuilders[connections[i]].instId;
			if (instId > 0)
			{
				ref EnemyData ptr = ref this.factory.enemyPool[instId];
				if (ptr.builderId > 0)
				{
					ref EnemyBuilderComponent ptr2 = ref this.builders.buffer[ptr.builderId];
					if (ptr2.id == ptr.builderId && (ptr2.buildCursor == 0 || ptr2.buildCursor == dfgbaseComponent.builderId))
					{
						ptr2.buildCDTime = 20;
					}
				}
			}
		}
	}

	// Token: 0x06000C40 RID: 3136 RVA: 0x000B64D0 File Offset: 0x000B46D0
	public void NotifyBaseRemoving(int baseId)
	{
		DFGBaseComponent dfgbaseComponent = this.bases.buffer[baseId];
		if (baseId == 0 || dfgbaseComponent == null)
		{
			return;
		}
		dfgbaseComponent.disableAssaultingAlert = 0;
		DFRelayComponent relay = dfgbaseComponent.GetRelay();
		if (relay != null)
		{
			relay.LeaveBase();
			relay.hive.relayNeutralizedCounter++;
		}
		for (EnemyDFHiveSystem enemyDFHiveSystem = this.gameData.spaceSector.dfHives[this.factory.planet.star.index]; enemyDFHiveSystem != null; enemyDFHiveSystem = enemyDFHiveSystem.nextSibling)
		{
			for (int i = 1; i < enemyDFHiveSystem.relays.cursor; i++)
			{
				DFRelayComponent dfrelayComponent = enemyDFHiveSystem.relays.buffer[i];
				if (dfrelayComponent != null && dfrelayComponent.targetAstroId == this.factory.planet.astroId && dfrelayComponent.baseId == baseId)
				{
					dfrelayComponent.baseId = 0;
				}
			}
		}
	}

	// Token: 0x06000C41 RID: 3137 RVA: 0x000B65A3 File Offset: 0x000B47A3
	public void NotifyBaseRemoved()
	{
		if (this.onBaseRemoved != null)
		{
			this.onBaseRemoved();
		}
	}

	// Token: 0x06000C42 RID: 3138 RVA: 0x000B65B8 File Offset: 0x000B47B8
	public unsafe void NotifyEnemyKilled(ref EnemyData enemy)
	{
		if (enemy.id != 0)
		{
			int owner = (int)enemy.owner;
			DFGBaseComponent dfgbaseComponent = this.bases.buffer[owner];
			if (dfgbaseComponent == null || dfgbaseComponent.id != owner)
			{
				return;
			}
			float num = (float)RandomTable.Integer(ref this.rtseed, 101) * 0.01f + 1.5f;
			int count = (int)((float)SkillSystem.EnemySandCountByModelIndex[(int)enemy.modelIndex] * num * 0.2f + 0.5f);
			this.gameData.trashSystem.AddTrashFromGroundEnemy(1099, count, 900, enemy.id, this.factory);
			if (enemy.dynamic)
			{
				for (int i = 0; i < 3; i++)
				{
					int num2;
					int num3;
					int num4;
					this.RandomDropItemOnce(dfgbaseComponent.evolve.level, out num2, out num3, out num4);
					if (num2 > 0 && num3 > 0 && num4 > 0)
					{
						this.gameData.trashSystem.AddTrashFromGroundEnemy(num2, num3, num4, enemy.id, this.factory);
					}
				}
			}
			if (dfgbaseComponent.evolve.waveTicks == 0 && dfgbaseComponent.turboTicks > 0)
			{
				double magnitude = (this.factory.enemyPool[dfgbaseComponent.enemyId].pos - enemy.pos).magnitude;
				double num5 = 1.28 - magnitude / (double)dfgbaseComponent.currentSensorRange * 1.6;
				if (num5 > 1.0)
				{
					num5 = 1.0;
				}
				if (num5 > 0.0)
				{
					int num6 = 5 + (int)(20.0 * num5 + 0.5);
					if (dfgbaseComponent.turboRepress < num6)
					{
						dfgbaseComponent.turboRepress = num6;
					}
				}
			}
			if (enemy.dynamic)
			{
				if (this.factory == this.gameData.localLoadedPlanetFactory)
				{
					SkillSFXHolder skillSFXHolder = *this.factory.skillSystem.audio.AddPlanetAudio(169, 2.5f, this.factory.planet.astroId, enemy.pos, 0);
					skillSFXHolder.radius0 = 80f;
					skillSFXHolder.radius1 = 150f;
					skillSFXHolder.falloff = 1f;
				}
				this.NotifyUnitKilled(ref enemy, dfgbaseComponent);
				return;
			}
			if (this.factory == this.gameData.localLoadedPlanetFactory)
			{
				SkillSFXHolder skillSFXHolder2 = *this.factory.skillSystem.audio.AddPlanetAudio(170, 3.5f, this.factory.planet.astroId, enemy.pos, 0);
				skillSFXHolder2.multiplier = 2f;
				skillSFXHolder2.radius0 = 130f;
				skillSFXHolder2.radius1 = 230f;
				skillSFXHolder2.falloff = 0.8f;
			}
		}
	}

	// Token: 0x06000C43 RID: 3139 RVA: 0x000B6884 File Offset: 0x000B4A84
	private void NotifyUnitKilled(ref EnemyData enemy, DFGBaseComponent @base)
	{
		if (enemy.id != 0 && enemy.unitId != 0)
		{
			int port = (int)enemy.port;
			int num = (int)(enemy.protoId - 8128);
			EnemyFormation enemyFormation = @base.forms[num];
			Assert.True(enemyFormation.units[port] > 1);
			enemyFormation.RemoveUnit(port);
		}
	}

	// Token: 0x06000C44 RID: 3140 RVA: 0x000B68D4 File Offset: 0x000B4AD4
	public void RandomDropItemOnce(int enemyLevel, out int itemId, out int count, out int life)
	{
		this.rtseed_lehmer = (uint)((ulong)(this.rtseed_lehmer % 2147483646U + 1U) * 48271UL % 2147483647UL) - 1U;
		double num = this.rtseed_lehmer / 2147483646.0 * 10000.0;
		itemId = ItemProto.enemyDropRangeTable[(int)num];
		count = 0;
		life = 0;
		int num2 = enemyLevel / 3;
		if (num2 > 8)
		{
			num2 = 8;
		}
		double num3 = 1.0;
		if (itemId > 0)
		{
			if (num2 < ItemProto.enemyDropLevelTable[itemId])
			{
				itemId = 0;
			}
			if ((ItemProto.enemyDropMaskTable[itemId] & this.enemyDropMask) != this.enemyDropMask)
			{
				num3 = (double)ItemProto.enemyDropMaskRatioTable[itemId];
				if (num3 < 1E-05)
				{
					itemId = 0;
				}
			}
		}
		double num4 = 0.0;
		if (itemId > 0)
		{
			if (this.gameData.history.ItemUnlocked(itemId))
			{
				num4 = 1.0;
			}
			else if (this.gameData.history.ItemCanDropByEnemy(itemId))
			{
				num4 = 0.4;
			}
			else
			{
				itemId = 0;
			}
		}
		num4 *= (double)this.gameData.history.enemyDropScale;
		double num5 = (itemId >= 5200 && itemId <= 5209) ? (1.0 + (double)this.enemyDropMultiplier * 0.0) : ((double)this.enemyDropMultiplier);
		if (itemId > 0)
		{
			int num6 = ItemProto.enemyDropLevelTable[itemId];
			life = 1800;
			double num7 = (double)ItemProto.enemyDropCountTable[itemId] * num4 * num3 * num5 * 0.8;
			double num8 = num7 * ((double)enemyLevel / 6.0 + 4.0);
			double num9 = num7;
			this.rtseed_lehmer = (uint)((ulong)(this.rtseed_lehmer % 2147483646U + 1U) * 48271UL % 2147483647UL) - 1U;
			double num10 = this.rtseed_lehmer / 2147483646.0 * (num8 - num9) + num9;
			this.rtseed_lehmer = (uint)((ulong)(this.rtseed_lehmer % 2147483646U + 1U) * 48271UL % 2147483647UL) - 1U;
			double num11 = this.rtseed_lehmer / 2147483646.0;
			count = (int)(num10 + num11);
			if (count > 0)
			{
				this.gameData.history.enemyDropItemUnlocked.Add(itemId);
			}
		}
	}

	// Token: 0x06000C45 RID: 3141 RVA: 0x000B6B1C File Offset: 0x000B4D1C
	public bool IsBasePit(int baseId)
	{
		DFGBaseComponent dfgbaseComponent = this.bases[baseId];
		return dfgbaseComponent != null && dfgbaseComponent.id == baseId && this.builders.buffer[dfgbaseComponent.builderId].sp == 0;
	}

	// Token: 0x06000C46 RID: 3142 RVA: 0x000B6B64 File Offset: 0x000B4D64
	public int CheckBaseCanRemoved(int baseId)
	{
		int num = 0;
		if (!this.IsBasePit(baseId))
		{
			num |= 1;
		}
		DFGBaseComponent dfgbaseComponent = this.bases[baseId];
		EnemyBuilderComponent[] buffer = this.builders.buffer;
		int cursor = this.builders.cursor;
		EnemyData[] enemyPool = this.factory.enemyPool;
		bool flag = false;
		for (int i = 1; i < cursor; i++)
		{
			if (buffer[i].id == i)
			{
				ref EnemyData ptr = ref enemyPool[buffer[i].enemyId];
				if ((int)ptr.owner == baseId && ptr.dfGBaseId != baseId)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			num |= 2;
		}
		EnemyFormation[] forms = dfgbaseComponent.forms;
		for (int j = 0; j < 3; j++)
		{
			if (forms[j].unitCount > 0)
			{
				num |= 4;
			}
		}
		return num;
	}

	// Token: 0x06000C47 RID: 3143 RVA: 0x000B6C38 File Offset: 0x000B4E38
	public bool RemoveBase(int baseId, bool removePit = false)
	{
		DFGBaseComponent dfgbaseComponent = this.bases[baseId];
		if (dfgbaseComponent == null || dfgbaseComponent.id != baseId)
		{
			return false;
		}
		if (removePit)
		{
			this.factory.RemoveRuinWithComponet(dfgbaseComponent.ruinId);
		}
		this.factory.RemoveEnemyFinal(dfgbaseComponent.enemyId);
		this.factory.defenseSystem.RefreshTurretSearchPair();
		return true;
	}

	// Token: 0x06000C48 RID: 3144 RVA: 0x000B6C98 File Offset: 0x000B4E98
	public void RemoveBasePit(int pitRuinId)
	{
		this.factory.RemoveRuinWithComponet(pitRuinId);
		DFGBaseComponent[] buffer = this.bases.buffer;
		int cursor = this.bases.cursor;
		for (int i = 1; i < cursor; i++)
		{
			DFGBaseComponent dfgbaseComponent = buffer[i];
			if (dfgbaseComponent != null && dfgbaseComponent.id == i && dfgbaseComponent.ruinId == pitRuinId)
			{
				this.factory.RemoveEnemyFinal(dfgbaseComponent.enemyId);
				this.factory.defenseSystem.RefreshTurretSearchPair();
				return;
			}
		}
	}

	// Token: 0x06000C49 RID: 3145 RVA: 0x000B6D10 File Offset: 0x000B4F10
	public void CreateTruckSegmentBuffer()
	{
		int cursor = this.bases.cursor;
		DFGBaseComponent[] buffer = this.bases.buffer;
		int num = 0;
		for (int i = 1; i < cursor; i++)
		{
			if (buffer[i] != null && buffer[i].pbuilders != null)
			{
				for (int j = 0; j < buffer[i].pbuilders.Length; j++)
				{
					ref GrowthPattern_DFGround.Builder ptr = ref buffer[i].pbuilders[j];
					if (ptr.isConn)
					{
						ptr.truckSegOffset = num;
						num += (int)(ptr.length / 3f + 1f);
					}
				}
			}
		}
		if (num == 0)
		{
			return;
		}
		this.truckSegments = new byte[(num + 3) / 4 * 4];
	}

	// Token: 0x06000C4A RID: 3146 RVA: 0x000B6DBC File Offset: 0x000B4FBC
	public void AddTruckSegmentsForNewBase(DFGBaseComponent newBase)
	{
		if (newBase == null || newBase.pbuilders == null)
		{
			return;
		}
		if (this.truckSegments == null)
		{
			this.CreateTruckSegmentBuffer();
			return;
		}
		int num = this.truckSegments.Length;
		for (int i = 0; i < newBase.pbuilders.Length; i++)
		{
			ref GrowthPattern_DFGround.Builder ptr = ref newBase.pbuilders[i];
			if (ptr.isConn)
			{
				ptr.truckSegOffset = num;
				num += (int)(ptr.length / 3f + 1f);
			}
		}
		byte[] array = this.truckSegments;
		this.truckSegments = new byte[(num + 3) / 4 * 4];
		Array.Copy(array, 0, this.truckSegments, 0, array.Length);
	}

	// Token: 0x06000C4B RID: 3147 RVA: 0x000B6E5C File Offset: 0x000B505C
	public void GameTickLogic_Prepare()
	{
		this.keytick_timer++;
		if (this.isLocalLoaded)
		{
			Player mainPlayer = this.gameData.mainPlayer;
			this.local_player_pos = mainPlayer.position;
			this.local_player_pos_lf = mainPlayer.position;
			this.local_player_exist = true;
			this.local_player_alive = mainPlayer.isAlive;
			this.local_player_exist_alive = (this.local_player_exist && this.local_player_alive);
			this.local_player_grounded_alive = (this.local_player_exist_alive && !mainPlayer.sailing);
			this.local_player_total_energy_consume = (long)(mainPlayer.mecha.totalEnergyConsume + 0.5);
			return;
		}
		Player mainPlayer2 = this.gameData.mainPlayer;
		this.local_player_pos = Vector3.zero;
		this.local_player_pos_lf = VectorLF3.zero;
		this.local_player_exist = false;
		this.local_player_alive = mainPlayer2.isAlive;
		this.local_player_exist_alive = (this.local_player_exist && this.local_player_alive);
		this.local_player_grounded_alive = false;
		this.local_player_total_energy_consume = 0L;
	}

	// Token: 0x06000C4C RID: 3148 RVA: 0x000B6F68 File Offset: 0x000B5168
	public void GameTickLogic_Anim()
	{
		if (!this.isLocalLoaded)
		{
			return;
		}
		AnimData[] enemyAnimPool = this.factory.enemyAnimPool;
		int cursor = this.builders.cursor;
		EnemyBuilderComponent[] buffer = this.builders.buffer;
		for (int i = 1; i < cursor; i++)
		{
			if (buffer[i].buildCDTime == -1)
			{
				ref EnemyBuilderComponent ptr = ref buffer[i];
				ref AnimData ptr2 = ref enemyAnimPool[ptr.enemyId];
				float num = (float)((ptr.sp + 1) * 4 / ptr.spMax + 1) * 0.2f;
				if (ptr2.time < num)
				{
					ptr2.time += 0.001681f;
				}
				if (ptr2.time >= num)
				{
					ptr2.time = num;
					ptr.buildCDTime = 0;
				}
			}
		}
		int cursor2 = this.bases.cursor;
		DFGBaseComponent[] buffer2 = this.bases.buffer;
		for (int j = 1; j < cursor2; j++)
		{
			if (buffer2[j] != null && buffer2[j].builderId > 0 && this.builders.buffer[buffer2[j].builderId].sp == 0)
			{
				enemyAnimPool[buffer2[j].enemyId].time = 0f;
			}
		}
	}

	// Token: 0x06000C4D RID: 3149 RVA: 0x000B70AC File Offset: 0x000B52AC
	public void GameTickLogic_Base(EAggressiveLevel agglv)
	{
		EnemyData[] enemyPool = this.factory.enemyPool;
		if (this.bases.count > 0)
		{
			int cursor = this.bases.cursor;
			DFGBaseComponent[] buffer = this.bases.buffer;
			for (int i = 1; i < cursor; i++)
			{
				DFGBaseComponent dfgbaseComponent = buffer[i];
				if (dfgbaseComponent != null && dfgbaseComponent.id == i)
				{
					dfgbaseComponent.DetermineCurrentSensorRange(agglv);
					dfgbaseComponent.currentIncomingAssaultingUnitCount = 0;
					dfgbaseComponent.currentReadyUnitCount = 0;
					dfgbaseComponent.currentReadyRaiderCount = 0;
					dfgbaseComponent.currentReadyRangerCount = 0;
					dfgbaseComponent.currentActivatedUnitCount = 0;
					dfgbaseComponent.currentIncomingAssaultingUnitPos = Vector3.zero;
					dfgbaseComponent.currentClosetIncomingAssaultingUnitDist2 = 0.0;
				}
			}
		}
	}

	// Token: 0x06000C4E RID: 3150 RVA: 0x000B714C File Offset: 0x000B534C
	public void GameTickLogic_Turret(long gameTick, EAggressiveLevel agglv)
	{
		ref EnemyData[] ptr = ref this.factory.enemyPool;
		if (this.turrets.count > 0)
		{
			int cursor = this.turrets.cursor;
			DFGTurretComponent[] buffer = this.turrets.buffer;
			for (int i = 1; i < cursor; i++)
			{
				ref DFGTurretComponent ptr2 = ref buffer[i];
				if (ptr2.id == i)
				{
					ref EnemyData ptr3 = ref ptr[ptr2.enemyId];
					PrefabDesc pdesc = SpaceSector.PrefabDescByModelIndex[(int)ptr3.modelIndex];
					ptr2.InternalUpdate(pdesc);
					switch (ptr2.state)
					{
					case EDFTurretState.None:
						ptr2.NoneState();
						break;
					case EDFTurretState.Searching:
						if (ptr2.DetermineSearchTarget(gameTick))
						{
							ptr2.SearchTarget(this.factory, this.bases.buffer[buffer[i].baseId], agglv, -1);
						}
						break;
					case EDFTurretState.Aiming:
						if (ptr2.target.id == 0)
						{
							ptr2.state = EDFTurretState.Searching;
						}
						ptr2.Aim(this.factory, this.bases.buffer[buffer[i].baseId], this.builders.buffer, agglv, -1);
						break;
					}
				}
			}
			if (this.isLocalLoaded)
			{
				AnimData[] enemyAnimPool = this.factory.enemyAnimPool;
				for (int j = 1; j < cursor; j++)
				{
					ref DFGTurretComponent ptr4 = ref buffer[j];
					if (ptr4.id == j && this.builders.buffer[ptr4.builderId].state > 0)
					{
						int enemyId = ptr4.enemyId;
						ref EnemyData ptr5 = ref ptr[enemyId];
						PrefabDesc prefabDesc = SpaceSector.PrefabDescByModelIndex[(int)ptr5.modelIndex];
						enemyAnimPool[enemyId].prepare_length = ptr4.localDir.x;
						enemyAnimPool[enemyId].working_length = ptr4.localDir.y;
						enemyAnimPool[enemyId].power = ptr4.localDir.z;
						enemyAnimPool[enemyId].state = (uint)ptr4.state;
						int dfTurretAttackInterval = prefabDesc.dfTurretAttackInterval;
						float num = (float)(-(float)ptr4.fire) / (float)dfTurretAttackInterval;
						if (num < -1f)
						{
							num = -1f;
						}
						float num2 = 2f;
						if (ptr4.heat < 0)
						{
							num2 = (float)ptr4.heat / 3600f + 1f;
							num2 = ((num2 > 1f) ? 1f : ((num2 < 0f) ? 0f : num2));
						}
						else if (ptr4.heat > 0)
						{
							num2 = 0f;
						}
						float num3 = 0f;
						if (ptr4.state == EDFTurretState.Aiming && num >= 0f)
						{
							float x = ptr4.localDir.x;
							float y = ptr4.localDir.y;
							float z = ptr4.localDir.z;
							float num4 = Mathf.Sqrt(x * x + y * y + z * z);
							num3 = (ptr4.sensorRange - num4) / (ptr4.sensorRange - ptr4.attackRange);
							num3 = ((num3 < 0f) ? 0f : ((num3 >= 1f) ? 1f : num3));
						}
						if (num >= 0f)
						{
							num += ((num2 < 2f) ? ((num2 > num3) ? num3 : num2) : num3);
						}
						enemyAnimPool[enemyId].time = num + 10f;
					}
				}
			}
		}
	}

	// Token: 0x06000C4F RID: 3151 RVA: 0x000B74CC File Offset: 0x000B56CC
	public void GameTickLogic_Unit(long gameTick, EAggressiveLevel agglv)
	{
		ref EnemyData[] ptr = ref this.factory.enemyPool;
		if (this.units.count > 0)
		{
			int cursor = this.units.cursor;
			EnemyUnitComponent[] buffer = this.units.buffer;
			DFGBaseComponent[] buffer2 = this.bases.buffer;
			SkillSystem skillSystem = this.factory.skillSystem;
			VectorLF3 playerSkillTargetU = this.factory.skillSystem.playerSkillTargetU;
			VectorLF3 vectorLF = Maths.QInvRotateLF(this.factory.planet.runtimeRotation, playerSkillTargetU - this.factory.planet.uPosition);
			EnemyUnitComponent.gameTick = gameTick;
			int formTick = (int)(((long)this.planet.seed + gameTick) % 151200L);
			float realRadius = this.planet.realRadius;
			int num = (int)(gameTick % 60L);
			float ratio = 0.75f;
			int c = 5;
			int maxHatredGroundTmp = skillSystem.maxHatredGroundTmp;
			int maxHatredGroundBaseTmp = skillSystem.maxHatredGroundBaseTmp;
			if (agglv <= EAggressiveLevel.Torpid)
			{
				if (agglv != EAggressiveLevel.Dummy)
				{
					if (agglv == EAggressiveLevel.Passive || agglv == EAggressiveLevel.Torpid)
					{
						ratio = 0.6f;
						c = 6;
					}
				}
				else
				{
					ratio = 0f;
					c = 0;
				}
			}
			else if (agglv != EAggressiveLevel.Normal)
			{
				if (agglv != EAggressiveLevel.Sharp)
				{
					if (agglv == EAggressiveLevel.Rampage)
					{
						ratio = 0.93f;
						c = 1;
					}
				}
				else
				{
					ratio = 0.86f;
					c = 3;
				}
			}
			else
			{
				ratio = 0.75f;
				c = 5;
			}
			for (int i = 1; i < cursor; i++)
			{
				ref EnemyUnitComponent ptr2 = ref buffer[i];
				if (ptr2.id == i)
				{
					ref EnemyData ptr3 = ref ptr[ptr2.enemyId];
					DFGBaseComponent dfgbaseComponent = this.bases[(int)ptr3.owner];
					if (dfgbaseComponent != null)
					{
						dfgbaseComponent.currentActivatedUnitCount++;
					}
					PrefabDesc pdesc = SpaceSector.PrefabDescByModelIndex[(int)ptr3.modelIndex];
					if (i % 60 == num)
					{
						ptr2.hatredLock.Enter();
						ptr2.hatred.Fade(ratio, c);
						ptr2.hatredLock.Exit();
						ptr2.SensorLogic_Ground(ref ptr3, this.factory, maxHatredGroundTmp, agglv, -1);
						ptr3.hashAddress = this.factory.hashSystemDynamic.UpdateObjectHashAddress(ptr3.hashAddress, ptr3.id, ptr3.pos, EObjectType.Enemy);
						if (agglv > EAggressiveLevel.Dummy && ptr3.willBroadcast)
						{
							ptr3.willBroadcast = false;
							if (ptr2.hatred.max.value > 80 && ptr2.hatred.max.target > 0)
							{
								ptr2.BroadcastHatred(this.factory, buffer, ref ptr3, true, maxHatredGroundTmp, maxHatredGroundBaseTmp, -1);
							}
						}
					}
					int num2 = (int)((1f - ptr2.disturbValue) * 11f + 0.5f);
					if (gameTick * 7L % 11L < (long)num2)
					{
						ptr2.UpdateFireCondition(pdesc);
					}
					switch (ptr2.behavior)
					{
					case EEnemyBehavior.None:
						ptr2.RunBehavior_None(ref ptr3);
						break;
					case EEnemyBehavior.Initial:
						ptr2.RunBehavior_Initial(ref ptr3);
						break;
					case EEnemyBehavior.KeepForm:
						ptr2.ReclaimThreatCarry(this);
						ptr2.RunBehavior_KeepForm(formTick, ptr, buffer2, realRadius, ref ptr3);
						if (ptr3.combatStatId == 0 && ptr2.stateTick == 0 && ptr2.behavior == EEnemyBehavior.KeepForm)
						{
							this.DeactivateUnitDeferred(i);
						}
						break;
					case EEnemyBehavior.SeekForm:
						ptr2.RunBehavior_SeekForm_Ground(formTick, ptr, buffer2, realRadius, ref ptr3);
						break;
					case EEnemyBehavior.SeekTarget:
						ptr2.RunBehavior_SeekTarget_Ground(this.factory, ref ptr3);
						break;
					case EEnemyBehavior.Defense:
						ptr2.RunBehavior_Defense_Ground(formTick, skillSystem, ptr, buffer2, realRadius, ref ptr3);
						break;
					case EEnemyBehavior.Engage:
						ptr2.RunBehavior_Engage_Ground(this.factory, ref ptr3);
						break;
					}
					if (i % 60 == num)
					{
						ptr2.UndergroundRescue(this.planet, ref ptr3);
					}
					if (ptr2.behavior >= EEnemyBehavior.SeekTarget && ptr3.isAssaultingUnit)
					{
						if (dfgbaseComponent != null)
						{
							dfgbaseComponent.currentIncomingAssaultingUnitCount++;
							double num3 = ptr3.pos.x - vectorLF.x;
							double num4 = ptr3.pos.y - vectorLF.y;
							double num5 = ptr3.pos.z - vectorLF.z;
							double num6 = num3 * num3 + num4 * num4 + num5 * num5;
							if (num6 < dfgbaseComponent.currentClosetIncomingAssaultingUnitDist2 || dfgbaseComponent.currentClosetIncomingAssaultingUnitDist2 == 0.0)
							{
								dfgbaseComponent.currentClosetIncomingAssaultingUnitDist2 = num6;
								dfgbaseComponent.currentIncomingAssaultingUnitPos = ptr3.pos;
							}
						}
					}
					else if (ptr2.behavior <= EEnemyBehavior.KeepForm && dfgbaseComponent != null)
					{
						dfgbaseComponent.currentReadyUnitCount++;
						if (ptr3.protoId == 8128)
						{
							dfgbaseComponent.currentReadyRaiderCount++;
						}
						else if (ptr3.protoId == 8129)
						{
							dfgbaseComponent.currentReadyRangerCount++;
						}
					}
					float num7 = 1.25f - ptr2.disturbValue;
					if (num7 < 0.1f)
					{
						num7 = 0.1f;
					}
					ptr2.disturbValue -= num7 / 120f;
					if (ptr2.disturbValue < 0f)
					{
						ptr2.disturbValue = 0f;
					}
				}
			}
			if (this.isLocalLoaded)
			{
				ObjectRenderer[] objectRenderers = this.planet.factoryModel.gpuiManager.objectRenderers;
				if (objectRenderers == null)
				{
					return;
				}
				this.planet.factoryModel.enemyUnitsDirty = true;
				for (int j = 1; j < cursor; j++)
				{
					ref EnemyUnitComponent ptr4 = ref buffer[j];
					if (ptr4.id == j)
					{
						ref EnemyData ptr5 = ref ptr[ptr4.enemyId];
						DynamicRenderer dynamicRenderer = objectRenderers[(int)ptr5.modelIndex] as DynamicRenderer;
						if (dynamicRenderer != null)
						{
							GPUOBJECT[] instPool = dynamicRenderer.instPool;
							int modelId = ptr5.modelId;
							instPool[modelId].posx = (float)ptr5.pos.x;
							instPool[modelId].posy = (float)ptr5.pos.y;
							instPool[modelId].posz = (float)ptr5.pos.z;
							instPool[modelId].rotx = ptr5.rot.x;
							instPool[modelId].roty = ptr5.rot.y;
							instPool[modelId].rotz = ptr5.rot.z;
							instPool[modelId].rotw = ptr5.rot.w;
							Vector4[] extraPool = dynamicRenderer.extraPool;
							int modelId2 = ptr5.modelId;
							extraPool[modelId2].x = ptr4.anim;
							extraPool[modelId2].y = ptr4.disturbValue;
							extraPool[modelId2].z = ptr4.steering;
							extraPool[modelId2].w = ptr4.speed;
						}
					}
				}
			}
		}
	}

	// Token: 0x06000C50 RID: 3152 RVA: 0x000B7B44 File Offset: 0x000B5D44
	public void PostGameTick()
	{
		Player mainPlayer = GameMain.mainPlayer;
		EAggressiveLevel eaggressiveLevel = this.gameData.history.combatSettings.aggressiveLevel;
		if (eaggressiveLevel > EAggressiveLevel.Passive && this.gameData.history.dfTruceTimer > 0L)
		{
			eaggressiveLevel = EAggressiveLevel.Passive;
		}
		if (this.bases.count > 0)
		{
			int cursor = this.bases.cursor;
			DFGBaseComponent[] buffer = this.bases.buffer;
			for (int i = 1; i < cursor; i++)
			{
				DFGBaseComponent dfgbaseComponent = buffer[i];
				if (dfgbaseComponent != null && dfgbaseComponent.id == i)
				{
					dfgbaseComponent.PostGameTick(this.factory, mainPlayer);
				}
			}
		}
		if (this.turrets.count > 0)
		{
			DFGBaseComponent[] buffer2 = this.bases.buffer;
			int cursor2 = this.turrets.cursor;
			DFGTurretComponent[] buffer3 = this.turrets.buffer;
			for (int j = 1; j < cursor2; j++)
			{
				ref DFGTurretComponent ptr = ref buffer3[j];
				if (ptr.id == j)
				{
					ptr.PostGameTick(buffer2[buffer3[j].baseId], this.factory, eaggressiveLevel);
				}
			}
		}
	}

	// Token: 0x1700018A RID: 394
	// (get) Token: 0x06000C51 RID: 3153 RVA: 0x000B7C5D File Offset: 0x000B5E5D
	public double buildSpeed
	{
		get
		{
			return (double)this.gameData.history.combatSettings.growthSpeedFactor / 3.0;
		}
	}

	// Token: 0x1700018B RID: 395
	// (get) Token: 0x06000C52 RID: 3154 RVA: 0x000B7C7F File Offset: 0x000B5E7F
	public double replicateSpeed
	{
		get
		{
			return Math.Pow((double)this.gameData.history.combatSettings.growthSpeedFactor / 2.0, 0.6);
		}
	}

	// Token: 0x06000C53 RID: 3155 RVA: 0x000B7CB0 File Offset: 0x000B5EB0
	public int TicksToBuildChances(int _tick)
	{
		if (_tick < 600)
		{
			double num = this.buildSpeed;
			if (num < 1.0)
			{
				num = 1.0;
			}
			return (int)((double)_tick * num + 0.9999);
		}
		double buildSpeed = this.buildSpeed;
		double num2 = (buildSpeed < 1.0) ? 1.0 : buildSpeed;
		return (int)(600.0 * num2 + (double)(_tick - 600) * buildSpeed + 0.9999);
	}

	// Token: 0x06000C54 RID: 3156 RVA: 0x000B7D34 File Offset: 0x000B5F34
	public int BuildChancesToTicks(int _build_chance)
	{
		double buildSpeed = this.buildSpeed;
		double num = (buildSpeed < 0.01) ? 0.01 : buildSpeed;
		double num2 = (num < 1.0) ? 1.0 : num;
		double num3 = (double)_build_chance / num2;
		if (num3 < 600.0)
		{
			return (int)(num3 + 0.9999);
		}
		_build_chance -= (int)(600.0 * num2 + 0.9999);
		return (int)((double)_build_chance / num + 600.9999);
	}

	// Token: 0x06000C55 RID: 3157 RVA: 0x000B7DC4 File Offset: 0x000B5FC4
	public void KeyTickLogic(long time)
	{
		this.keytick_timer = 0;
		bool isLocalLoaded = this.isLocalLoaded;
		ref CombatSettings ptr = ref this.gameData.history.combatSettings;
		int num = (int)(time / 60L);
		int num2 = (int)((double)num * this.buildSpeed + 0.9999);
		int num3 = (int)((double)(num - 1) * this.buildSpeed + 0.9999);
		bool flag = num2 > num3;
		bool flag2 = num2 % 2 == 0;
		int num4 = (int)((double)num * this.replicateSpeed);
		int num5 = (int)((double)(num - 1) * this.replicateSpeed);
		bool flag3 = num4 > num5;
		this.CalcFormsSupply();
		ref AnimData[] ptr2 = ref this.factory.enemyAnimPool;
		ref EnemyBuilderComponent[] ptr3 = ref this.builders.buffer;
		ref DFGBaseComponent[] ptr4 = ref this.bases.buffer;
		ref EnemyData[] ptr5 = ref this.factory.enemyPool;
		if (isLocalLoaded && this.truckSegments != null)
		{
			Array.Copy(this.truckSegments, 0, this.truckSegments, 1, this.truckSegments.Length - 1);
		}
		int cursor = this.connectors.cursor;
		DFGConnectorComponent[] buffer = this.connectors.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref DFGConnectorComponent ptr6 = ref buffer[i];
			if (ptr6.id == i)
			{
				DFGBaseComponent dfgbaseComponent = ptr4[ptr6.baseId];
				if (dfgbaseComponent != null)
				{
					ptr6.LogicTick(dfgbaseComponent, ptr3, this.truckSegments);
				}
			}
		}
		long dfTruceTimer = this.gameData.history.dfTruceTimer;
		EAggressiveLevel eaggressiveLevel = ptr.aggressiveLevel;
		if (eaggressiveLevel > EAggressiveLevel.Passive && dfTruceTimer > 0L)
		{
			eaggressiveLevel = EAggressiveLevel.Passive;
		}
		SpaceSector spaceSector = this.gameData.spaceSector;
		int cursor2 = this.bases.cursor;
		DFGBaseComponent[] buffer2 = this.bases.buffer;
		int num6 = 0;
		int num7 = 0;
		float num8 = (float)this.local_player_total_energy_consume / 15000f;
		if (num8 > 0f)
		{
			num8 = Mathf.Sqrt(num8);
		}
		float expSharingFactor = this.planet.star.expSharingFactor;
		float num9 = (0.15f + 0.45f * ptr.battleExpFactor) * expSharingFactor;
		float num10 = (0.035f + 0.105f * ptr.battleExpFactor) * expSharingFactor;
		float num11 = (0.5f + 1.5f * ptr.battleExpFactor) * expSharingFactor;
		int hatredTake = spaceSector.skillSystem.maxHatredGroundTmp * 2;
		float ratio = 0.75f;
		int c = 5;
		if (eaggressiveLevel <= EAggressiveLevel.Torpid)
		{
			if (eaggressiveLevel != EAggressiveLevel.Dummy)
			{
				if (eaggressiveLevel == EAggressiveLevel.Passive || eaggressiveLevel == EAggressiveLevel.Torpid)
				{
					ratio = 0.6f;
					c = 6;
				}
			}
			else
			{
				ratio = 0f;
				c = 0;
			}
		}
		else if (eaggressiveLevel != EAggressiveLevel.Normal)
		{
			if (eaggressiveLevel != EAggressiveLevel.Sharp)
			{
				if (eaggressiveLevel == EAggressiveLevel.Rampage)
				{
					ratio = 0.93f;
					c = 1;
				}
			}
			else
			{
				ratio = 0.86f;
				c = 3;
			}
		}
		else
		{
			ratio = 0.75f;
			c = 5;
		}
		int num12 = 0;
		for (int j = 1; j < cursor2; j++)
		{
			DFGBaseComponent dfgbaseComponent2 = buffer2[j];
			if (dfgbaseComponent2 != null && dfgbaseComponent2.id == j && dfgbaseComponent2.evolve.waveTicks > 0)
			{
				num12++;
			}
		}
		for (int k = 1; k < cursor2; k++)
		{
			DFGBaseComponent dfgbaseComponent3 = buffer2[k];
			if (dfgbaseComponent3 != null && dfgbaseComponent3.id == k)
			{
				EnemyDFHiveSystem enemyDFHiveSystem = (dfgbaseComponent3.hiveAstroId == 0) ? null : spaceSector.GetHiveByAstroId(dfgbaseComponent3.hiveAstroId);
				dfgbaseComponent3.UpdateHatred(time, hatredTake, (int)(36f * spaceSector.skillSystem.enemyAggressiveHatredCoefTmp));
				dfgbaseComponent3.hatred.Fade(ratio, c);
				dfgbaseComponent3.LogicTick(ref ptr3[dfgbaseComponent3.builderId], eaggressiveLevel, time);
				dfgbaseComponent3.UpdateFactoryThreat(ref ptr3[dfgbaseComponent3.builderId], eaggressiveLevel, ptr.powerThreatFactor, num8, num12);
				if (ptr3[dfgbaseComponent3.builderId].sp > 0 && ptr5[dfgbaseComponent3.enemyId].isInvincible)
				{
					ptr5[dfgbaseComponent3.enemyId].isInvincible = false;
				}
				if (dfgbaseComponent3.hatred.max.targetType != ETargetType.Player || dfgbaseComponent3.hatred.max.value < spaceSector.skillSystem.maxHatredGroundTmp * 5)
				{
					ptr5[dfgbaseComponent3.enemyId].counterAttack = false;
				}
				if (dfgbaseComponent3.evolve.exppshr > 0 || dfgbaseComponent3.evolve.threatshr > 0)
				{
					num6 += dfgbaseComponent3.evolve.exppshr;
					num7 += dfgbaseComponent3.evolve.threatshr;
					if (dfgbaseComponent3.evolve.waveTicks == 0 && dfgbaseComponent3.evolve.waveAsmTicks == 0)
					{
						DFGBaseComponent dfgbaseComponent4 = dfgbaseComponent3;
						dfgbaseComponent4.evolve.threat = dfgbaseComponent4.evolve.threat + dfgbaseComponent3.evolve.threatshr / 250;
					}
					if (enemyDFHiveSystem != null)
					{
						enemyDFHiveSystem.evolve.AddExpPoint(dfgbaseComponent3.evolve.exppshr / 200);
						if (enemyDFHiveSystem.evolve.waveTicks == 0 && enemyDFHiveSystem.evolve.waveAsmTicks == 0)
						{
							EnemyDFHiveSystem enemyDFHiveSystem2 = enemyDFHiveSystem;
							enemyDFHiveSystem2.evolve.threat = enemyDFHiveSystem2.evolve.threat + dfgbaseComponent3.evolve.threatshr / 5000;
						}
						enemyDFHiveSystem.hatredAstros.HateTarget(ETargetType.Astro, this.planet.astroId, dfgbaseComponent3.evolve.threatshr / 5000, 1000000000, EHatredOperation.Add);
					}
					else
					{
						for (EnemyDFHiveSystem enemyDFHiveSystem3 = spaceSector.dfHives[this.planet.star.index]; enemyDFHiveSystem3 != null; enemyDFHiveSystem3 = enemyDFHiveSystem3.nextSibling)
						{
							if (enemyDFHiveSystem3.evolve.waveTicks == 0 && enemyDFHiveSystem3.evolve.waveAsmTicks == 0)
							{
								EnemyDFHiveSystem enemyDFHiveSystem4 = enemyDFHiveSystem3;
								enemyDFHiveSystem4.evolve.threat = enemyDFHiveSystem4.evolve.threat + dfgbaseComponent3.evolve.threatshr / 5000;
							}
							enemyDFHiveSystem3.hatredAstros.HateTarget(ETargetType.Astro, this.planet.astroId, dfgbaseComponent3.evolve.threatshr / 5000, 1000000000, EHatredOperation.Add);
						}
					}
				}
				if (enemyDFHiveSystem != null)
				{
					int num13 = enemyDFHiveSystem.evolve.exp_total - dfgbaseComponent3.evolve.exp_total;
					if (num13 > 0)
					{
						dfgbaseComponent3.evolve.AddExpPoint((int)((float)num13 * num11));
					}
					else if (num13 < 0)
					{
						enemyDFHiveSystem.evolve.AddExpPoint((int)((float)(-(float)num13) * num10));
					}
				}
			}
		}
		for (int l = 1; l < cursor2; l++)
		{
			DFGBaseComponent dfgbaseComponent5 = buffer2[l];
			if (dfgbaseComponent5 != null && dfgbaseComponent5.id == l)
			{
				dfgbaseComponent5.evolve.AddExpPoint((num6 - dfgbaseComponent5.evolve.exppshr) / 40);
				if (dfgbaseComponent5.evolve.waveTicks == 0 && dfgbaseComponent5.evolve.waveAsmTicks == 0)
				{
					DFGBaseComponent dfgbaseComponent6 = dfgbaseComponent5;
					dfgbaseComponent6.evolve.threat = dfgbaseComponent6.evolve.threat + (num7 - dfgbaseComponent5.evolve.threatshr) / 50;
				}
				dfgbaseComponent5.evolve.exppshr = 0;
				dfgbaseComponent5.evolve.threatshr = 0;
				if (cursor2 > 2)
				{
					int num14 = RandomTable.Integer(ref this.rtseed, this.bases.cursor - 2) + 1;
					if (num14 >= l)
					{
						num14++;
					}
					DFGBaseComponent dfgbaseComponent7 = buffer2[num14];
					if (dfgbaseComponent7 != null && dfgbaseComponent7.id == num14)
					{
						int num15 = dfgbaseComponent7.evolve.exp_total - dfgbaseComponent5.evolve.exp_total;
						if (num15 > 0)
						{
							dfgbaseComponent5.evolve.AddExpPoint((int)((float)num15 * num9));
						}
						else if (num15 < 0)
						{
							dfgbaseComponent7.evolve.AddExpPoint((int)((float)(-(float)num15) * num9));
						}
					}
				}
				if (dfgbaseComponent5.evolve.threat > dfgbaseComponent5.evolve.maxThreat)
				{
					dfgbaseComponent5.evolve.threat = dfgbaseComponent5.evolve.maxThreat;
				}
			}
		}
		int cursor3 = this.replicators.cursor;
		DFGReplicatorComponent[] buffer3 = this.replicators.buffer;
		float turboRatio = Mathf.Pow(ptr.growthSpeedFactor / 2f, 0.4f);
		for (int m = 1; m < cursor3; m++)
		{
			if (buffer3[m].id == m)
			{
				DFGBaseComponent dfgbaseComponent8 = ptr4[buffer3[m].baseId];
				bool flag4 = flag3;
				if (dfgbaseComponent8.turboTicks > 0)
				{
					flag4 = true;
				}
				if (flag4)
				{
					buffer3[m].LogicTick(dfgbaseComponent8, ptr3, eaggressiveLevel, turboRatio, isLocalLoaded);
				}
			}
		}
		int cursor4 = this.turrets.cursor;
		DFGTurretComponent[] buffer4 = this.turrets.buffer;
		for (int n = 1; n < cursor4; n++)
		{
			ref DFGTurretComponent ptr7 = ref buffer4[n];
			if (ptr7.id == n)
			{
				ptr7.LogicTick(ptr3, eaggressiveLevel);
			}
		}
		int cursor5 = this.shields.cursor;
		DFGShieldComponent[] buffer5 = this.shields.buffer;
		for (int num16 = 1; num16 < cursor5; num16++)
		{
			if (buffer5[num16].id == num16)
			{
				buffer5[num16].LogicTick(ptr4[buffer5[num16].baseId], ptr3, ptr2);
			}
		}
		int cursor6 = this.builders.cursor;
		EnemyBuilderComponent[] buffer6 = this.builders.buffer;
		if (flag)
		{
			for (int num17 = 1; num17 < cursor6; num17++)
			{
				ref EnemyBuilderComponent ptr8 = ref buffer6[num17];
				if (ptr8.id == num17)
				{
					int enemyId = ptr8.enemyId;
					DFGBaseComponent dfgbaseComponent9 = ptr4[(int)ptr5[enemyId].owner];
					GrowthPattern_DFGround.Builder[] pbuilders = dfgbaseComponent9.pbuilders;
					ref AnimData anim = ref ptr2[enemyId];
					ptr8.LogicTick();
					if (flag2 && ptr8.state >= 3)
					{
						ptr8.BuildLogic_Ground(this, buffer6, dfgbaseComponent9);
					}
					ptr8.RefreshAnimation_Ground(pbuilders, ref anim, !isLocalLoaded);
				}
			}
		}
	}

	// Token: 0x06000C56 RID: 3158 RVA: 0x000B8750 File Offset: 0x000B6950
	public void PostKeyTick()
	{
		if (this.isLocalLoaded)
		{
			for (int i = 1; i < this.units.cursor; i++)
			{
				ref EnemyUnitComponent ptr = ref this.units.buffer[i];
				if (ptr.id == i)
				{
					ref EnemyData ptr2 = ref this.factory.enemyPool[ptr.enemyId];
					DFGBaseComponent dfgbaseComponent = this.bases[(int)ptr2.owner];
					if (dfgbaseComponent != null)
					{
						dfgbaseComponent.currentActivatedUnitCount++;
					}
				}
			}
			for (int j = 1; j < this.bases.cursor; j++)
			{
				DFGBaseComponent dfgbaseComponent2 = this.bases[j];
				if (dfgbaseComponent2 != null && dfgbaseComponent2.id == j && dfgbaseComponent2.currentActivatedUnitCount == 0)
				{
					for (int k = 0; k < 3; k++)
					{
						EnemyFormation enemyFormation = dfgbaseComponent2.forms[k];
						for (int l = 1; l <= enemyFormation.portCount; l++)
						{
							if (enemyFormation.units[l] > 1)
							{
								Debug.LogError(string.Format("发现一个已删除的单位在form里面的值为{0}", enemyFormation.units[l]));
								enemyFormation.RemoveUnit(l);
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x06000C57 RID: 3159 RVA: 0x000B8880 File Offset: 0x000B6A80
	public void CalcFormsSupply()
	{
		EnemyBuilderComponent[] buffer = this.builders.buffer;
		int cursor = this.bases.cursor;
		DFGBaseComponent[] buffer2 = this.bases.buffer;
		for (int i = 1; i < cursor; i++)
		{
			DFGBaseComponent dfgbaseComponent = buffer2[i];
			if (dfgbaseComponent != null && dfgbaseComponent.id == i)
			{
				for (int j = 0; j < dfgbaseComponent.forms.Length; j++)
				{
					dfgbaseComponent.forms[j].supply = 0;
				}
			}
		}
		int cursor2 = this.replicators.cursor;
		DFGReplicatorComponent[] buffer3 = this.replicators.buffer;
		for (int k = 1; k < cursor2; k++)
		{
			if (buffer3[k].id == k && buffer[buffer3[k].builderId].state > 0)
			{
				DFGBaseComponent dfgbaseComponent2 = buffer2[buffer3[k].baseId];
				if (dfgbaseComponent2 != null && dfgbaseComponent2.id == buffer3[k].baseId)
				{
					dfgbaseComponent2.forms[buffer3[k].productFormId].supply += buffer3[k].unitSupply;
				}
			}
		}
	}

	// Token: 0x06000C58 RID: 3160 RVA: 0x000B89B8 File Offset: 0x000B6BB8
	public void RemoveEnemyDeferred(int enemyId)
	{
		if (this._rmv_id_list == null)
		{
			this._rmv_id_list = new HashSet<int>();
		}
		this._rmv_id_list.Add(enemyId);
	}

	// Token: 0x06000C59 RID: 3161 RVA: 0x000B89DA File Offset: 0x000B6BDA
	public void AddEnemyDeferred(int baseId, int builderIndex)
	{
		if (this._add_bidx_list == null)
		{
			this._add_bidx_list = new HashSet<ValueTuple<int, int>>();
		}
		this._add_bidx_list.Add(new ValueTuple<int, int>(baseId, builderIndex));
	}

	// Token: 0x06000C5A RID: 3162 RVA: 0x000B8A04 File Offset: 0x000B6C04
	public void ExecuteDeferredEnemyChange()
	{
		if (this._rmv_id_list != null && this._rmv_id_list.Count > 0)
		{
			foreach (int id in this._rmv_id_list)
			{
				this.factory.RemoveEnemyFinal(id);
			}
			this._rmv_id_list.Clear();
		}
		if (this._add_bidx_list != null && this._add_bidx_list.Count > 0)
		{
			foreach (ValueTuple<int, int> valueTuple in this._add_bidx_list)
			{
				this.factory.CreateEnemyFinal(valueTuple.Item1, valueTuple.Item2);
			}
			this._add_bidx_list.Clear();
		}
	}

	// Token: 0x06000C5B RID: 3163 RVA: 0x000B8AF4 File Offset: 0x000B6CF4
	public void InitiateUnitDeferred(int baseId, int formId, int port, Vector3 pos, Quaternion rot, Vector3 vel, int tick)
	{
		object initiate_unit_list_lock = this._initiate_unit_list_lock;
		lock (initiate_unit_list_lock)
		{
			if (this._initiate_unit_list == null)
			{
				this._initiate_unit_list = new List<ValueTuple<int, int, int, Vector3, Quaternion, Vector3, int>>();
			}
			this._initiate_unit_list.Add(new ValueTuple<int, int, int, Vector3, Quaternion, Vector3, int>(baseId, formId, port, pos, rot, vel, tick));
		}
	}

	// Token: 0x06000C5C RID: 3164 RVA: 0x000B8B5C File Offset: 0x000B6D5C
	public void ActivateUnitDeferred(int baseId, int formId, int port, long gameTick, EEnemyBehavior behavior, int stateTick)
	{
		object activate_unit_list_lock = this._activate_unit_list_lock;
		lock (activate_unit_list_lock)
		{
			if (this._activate_unit_list == null)
			{
				this._activate_unit_list = new List<ValueTuple<int, int, int, long, EEnemyBehavior, int>>();
			}
			this._activate_unit_list.Add(new ValueTuple<int, int, int, long, EEnemyBehavior, int>(baseId, formId, port, gameTick, behavior, stateTick));
		}
	}

	// Token: 0x06000C5D RID: 3165 RVA: 0x000B8BC4 File Offset: 0x000B6DC4
	public void DeactivateUnitDeferred(int unitId)
	{
		object deactivate_unit_list_lock = this._deactivate_unit_list_lock;
		lock (deactivate_unit_list_lock)
		{
			if (this._deactivate_unit_list == null)
			{
				this._deactivate_unit_list = new List<int>();
			}
			this._deactivate_unit_list.Add(unitId);
		}
	}

	// Token: 0x06000C5E RID: 3166 RVA: 0x000B8C20 File Offset: 0x000B6E20
	public void ExecuteDeferredUnitFormation()
	{
		if (this._initiate_unit_list != null && this._initiate_unit_list.Count > 0)
		{
			foreach (ValueTuple<int, int, int, Vector3, Quaternion, Vector3, int> valueTuple in this._initiate_unit_list)
			{
				int num = this.InitiateUnit(valueTuple.Item1, valueTuple.Item2, valueTuple.Item3, valueTuple.Item4, valueTuple.Item5, valueTuple.Item6);
				if (num > 0)
				{
					ref EnemyUnitComponent ptr = ref this.units.buffer[num];
					if (ptr.id == num)
					{
						if (valueTuple.Item7 > 0)
						{
							ptr.stateTick = valueTuple.Item7;
							ptr.behavior = EEnemyBehavior.Initial;
						}
						else
						{
							ptr.stateTick = 0;
							ptr.behavior = EEnemyBehavior.SeekForm;
						}
					}
				}
			}
			this._initiate_unit_list.Clear();
		}
		if (this._activate_unit_list != null && this._activate_unit_list.Count > 0)
		{
			foreach (ValueTuple<int, int, int, long, EEnemyBehavior, int> valueTuple2 in this._activate_unit_list)
			{
				int num2 = this.ActivateUnit(valueTuple2.Item1, valueTuple2.Item2, valueTuple2.Item3, valueTuple2.Item4);
				if (num2 > 0)
				{
					ref EnemyUnitComponent ptr2 = ref this.units.buffer[num2];
					if (ptr2.id == num2)
					{
						ptr2.behavior = valueTuple2.Item5;
						ptr2.stateTick = valueTuple2.Item6;
					}
				}
			}
			this._activate_unit_list.Clear();
		}
		if (this._deactivate_unit_list != null && this._deactivate_unit_list.Count > 0)
		{
			foreach (int unitId in this._deactivate_unit_list)
			{
				this.DeactivateUnit(unitId);
			}
			this._deactivate_unit_list.Clear();
		}
	}

	// Token: 0x06000C5F RID: 3167 RVA: 0x000B8E3C File Offset: 0x000B703C
	private void _check_null_builder_safety()
	{
		ref EnemyBuilderComponent ptr = ref this.builders.buffer[0];
		if (ptr.id != 0 || ptr.enemyId != 0 || ptr.matter != 0 || ptr.energy != 0 || ptr.minMatter != 0 || ptr.minEnergy != 0 || ptr.maxMatter != 0 || ptr.maxEnergy != 0 || ptr.genMatter != 0 || ptr.genEnergy != 0 || ptr.sp != 0 || ptr.spMax != 0 || ptr.spMatter != 0 || ptr.spEnergy != 0 || ptr.state != 0 || ptr.idleEnergy != 0 || ptr.workEnergy != 0 || ptr.builderIndex != 0 || ptr.buildCursor != 0 || ptr.buildChance != 0 || ptr.buildCDTime != 0)
		{
			Debug.LogError("Error!  builders.buffer[0] has been changed by unknown mistake!");
		}
		foreach (DFGBaseComponent dfgbaseComponent in this.bases.buffer)
		{
			if (dfgbaseComponent != null)
			{
				ref GrowthPattern_DFGround.Builder ptr2 = ref dfgbaseComponent.pbuilders[0];
				if (ptr2.instId != 0 || ptr2.instBuilderId != 0 || ptr2.matterNeeded != 0 || ptr2.matterProvided != 0 || ptr2.matterSpeed != 0)
				{
					Debug.LogError("Error!  pbuilders[0] has been changed by unknown mistake!");
				}
			}
		}
	}

	// Token: 0x06000C60 RID: 3168 RVA: 0x000B8F84 File Offset: 0x000B7184
	public void RefreshPlanetReformState()
	{
		PlatformSystem platformSystem = this.factory.platformSystem;
		platformSystem.ResetStateArea();
		EnemyData[] enemyPool = this.factory.enemyPool;
		for (int i = 1; i < this.bases.cursor; i++)
		{
			DFGBaseComponent dfgbaseComponent = this.bases.buffer[i];
			if (dfgbaseComponent != null && dfgbaseComponent.id == i)
			{
				platformSystem.AddStateArea((uint)(16777216L | (long)dfgbaseComponent.id), enemyPool[dfgbaseComponent.enemyId].pos, 11.25f, 1);
			}
		}
		RuinData[] ruinPool = this.factory.ruinPool;
		int ruinCursor = this.factory.ruinCursor;
		for (int j = 1; j < ruinCursor; j++)
		{
			ref RuinData ptr = ref ruinPool[j];
			if (ptr.id == j && ptr.modelIndex == 406)
			{
				platformSystem.AddStateArea((uint)(33554432L | (long)ptr.id), ptr.pos, 11.25f, 1);
			}
		}
	}

	// Token: 0x06000C61 RID: 3169 RVA: 0x000B9089 File Offset: 0x000B7289
	public void DecisionAI()
	{
	}

	// Token: 0x06000C62 RID: 3170 RVA: 0x000B908C File Offset: 0x000B728C
	public int InitiateUnit(int baseId, int formId, int portId, Vector3 pos, Quaternion rot, Vector3 vel)
	{
		DFGBaseComponent dfgbaseComponent = this.bases.buffer[baseId];
		if (dfgbaseComponent == null || dfgbaseComponent.id != baseId)
		{
			return 0;
		}
		EnemyFormation enemyFormation = dfgbaseComponent.forms[formId];
		if (enemyFormation.units[portId] == 1)
		{
			enemyFormation.units[portId] = 2;
			int protoId = formId + 8128;
			int num = this.factory.CreateEnemyFinal(protoId, baseId, portId, pos, rot, vel);
			return this.factory.enemyPool[num].unitId;
		}
		return 0;
	}

	// Token: 0x06000C63 RID: 3171 RVA: 0x000B9108 File Offset: 0x000B7308
	public int ActivateUnit(int baseId, int formId, int portId, long gameTick)
	{
		DFGBaseComponent dfgbaseComponent = this.bases.buffer[baseId];
		if (dfgbaseComponent == null || dfgbaseComponent.id != baseId)
		{
			return 0;
		}
		EnemyFormation enemyFormation = dfgbaseComponent.forms[formId];
		if (enemyFormation.units[portId] == 1)
		{
			enemyFormation.units[portId] = 2;
			int protoId = formId + 8128;
			long num = ((long)this.planet.seed + gameTick) % 151200L;
			int num2 = this.factory.CreateEnemyFinal(protoId, baseId, portId, (int)num);
			return this.factory.enemyPool[num2].unitId;
		}
		return 0;
	}

	// Token: 0x06000C64 RID: 3172 RVA: 0x000B9198 File Offset: 0x000B7398
	public void DeactivateUnit(int unitId)
	{
		int enemyId = this.units.buffer[unitId].enemyId;
		if (enemyId == 0)
		{
			return;
		}
		ref EnemyData ptr = ref this.factory.enemyPool[enemyId];
		if (ptr.id != 0 && ptr.id == enemyId)
		{
			int owner = (int)ptr.owner;
			int port = (int)ptr.port;
			int num = (int)(ptr.protoId - 8128);
			DFGBaseComponent dfgbaseComponent = this.bases.buffer[owner];
			if (dfgbaseComponent == null || dfgbaseComponent.id != owner)
			{
				return;
			}
			EnemyFormation enemyFormation = dfgbaseComponent.forms[num];
			Assert.True(enemyFormation.units[port] > 1);
			if (enemyFormation.units[port] > 1)
			{
				enemyFormation.units[port] = 1;
			}
			this.factory.RemoveEnemyFinal(enemyId);
		}
	}

	// Token: 0x04000DBB RID: 3515
	public GameData gameData;

	// Token: 0x04000DBC RID: 3516
	public PlanetData planet;

	// Token: 0x04000DBD RID: 3517
	public PlanetFactory factory;

	// Token: 0x04000DBE RID: 3518
	public static GrowthPattern_DFGround[] patterns;

	// Token: 0x04000DBF RID: 3519
	private int enemyDropMask;

	// Token: 0x04000DC0 RID: 3520
	private float enemyDropMultiplier = 1f;

	// Token: 0x04000DC1 RID: 3521
	public int rtseed;

	// Token: 0x04000DC2 RID: 3522
	public uint rtseed_lehmer;

	// Token: 0x04000DC3 RID: 3523
	public int maxAssaultWaves;

	// Token: 0x04000DC4 RID: 3524
	public const float kTruckSpeed = 10f;

	// Token: 0x04000DC5 RID: 3525
	public DataPool<EnemyBuilderComponent> builders;

	// Token: 0x04000DC6 RID: 3526
	public ObjectPool<DFGBaseComponent> bases;

	// Token: 0x04000DC7 RID: 3527
	public DataPool<DFGConnectorComponent> connectors;

	// Token: 0x04000DC8 RID: 3528
	public DataPool<DFGReplicatorComponent> replicators;

	// Token: 0x04000DC9 RID: 3529
	public DataPool<DFGTurretComponent> turrets;

	// Token: 0x04000DCA RID: 3530
	public DataPool<DFGShieldComponent> shields;

	// Token: 0x04000DCB RID: 3531
	public DataPool<EnemyUnitComponent> units;

	// Token: 0x04000DCC RID: 3532
	public byte[] truckSegments;

	// Token: 0x04000DCD RID: 3533
	public const int PROTO_BASE = 8120;

	// Token: 0x04000DCF RID: 3535
	public const float CONN_TRUCK_SEG_LENGTH = 3f;

	// Token: 0x04000DD0 RID: 3536
	public Vector3 local_player_pos = Vector3.zero;

	// Token: 0x04000DD1 RID: 3537
	public VectorLF3 local_player_pos_lf = Vector3.zero;

	// Token: 0x04000DD2 RID: 3538
	public bool local_player_exist;

	// Token: 0x04000DD3 RID: 3539
	public bool local_player_alive = true;

	// Token: 0x04000DD4 RID: 3540
	public bool local_player_exist_alive = true;

	// Token: 0x04000DD5 RID: 3541
	public bool local_player_grounded_alive = true;

	// Token: 0x04000DD6 RID: 3542
	public long local_player_total_energy_consume;

	// Token: 0x04000DD7 RID: 3543
	public int keytick_timer;

	// Token: 0x04000DD8 RID: 3544
	private HashSet<int> _rmv_id_list;

	// Token: 0x04000DD9 RID: 3545
	[TupleElementNames(new string[]
	{
		"baseId",
		"builderIndex"
	})]
	private HashSet<ValueTuple<int, int>> _add_bidx_list;

	// Token: 0x04000DDA RID: 3546
	[TupleElementNames(new string[]
	{
		"baseId",
		"formId",
		"port",
		"pos",
		"rot",
		"vel",
		"tick"
	})]
	private List<ValueTuple<int, int, int, Vector3, Quaternion, Vector3, int>> _initiate_unit_list;

	// Token: 0x04000DDB RID: 3547
	private readonly object _initiate_unit_list_lock = new object();

	// Token: 0x04000DDC RID: 3548
	[TupleElementNames(new string[]
	{
		"baseId",
		"formId",
		"port",
		"gameTick",
		"behavior",
		"stateTick"
	})]
	private List<ValueTuple<int, int, int, long, EEnemyBehavior, int>> _activate_unit_list;

	// Token: 0x04000DDD RID: 3549
	private readonly object _activate_unit_list_lock = new object();

	// Token: 0x04000DDE RID: 3550
	private List<int> _deactivate_unit_list;

	// Token: 0x04000DDF RID: 3551
	private readonly object _deactivate_unit_list_lock = new object();
}
