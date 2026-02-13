using System;
using System.IO;
using UnityEngine;

// Token: 0x02000167 RID: 359
public class CombatGroundSystem
{
	// Token: 0x06000B48 RID: 2888 RVA: 0x000AA420 File Offset: 0x000A8620
	public CombatGroundSystem(PlanetData _planet)
	{
		this.planet = _planet;
		this.factory = this.planet.factory;
		this.spaceSector = this.factory.sector;
		this.gameData = this.factory.gameData;
		this.mecha = this.gameData.mainPlayer.mecha;
		this.fleets = new DataPool<FleetComponent>();
		this.units = new DataPool<UnitComponent>();
		this.combatModules = new ObjectPool<CombatModuleComponent>();
		this.units.Reset();
		this.fleets.Reset();
		this.combatModules.Reset();
	}

	// Token: 0x06000B49 RID: 2889 RVA: 0x000AA4DC File Offset: 0x000A86DC
	public CombatGroundSystem(PlanetData _planet, bool import)
	{
		this.planet = _planet;
		this.factory = this.planet.factory;
		this.spaceSector = this.factory.sector;
		this.gameData = this.factory.gameData;
		this.mecha = this.gameData.mainPlayer.mecha;
		this.units = new DataPool<UnitComponent>();
		this.fleets = new DataPool<FleetComponent>();
		this.combatModules = new ObjectPool<CombatModuleComponent>();
	}

	// Token: 0x06000B4A RID: 2890 RVA: 0x000AA578 File Offset: 0x000A8778
	public void Free()
	{
		if (this.fleets != null)
		{
			this.fleets.Free();
			this.fleets = null;
		}
		if (this.units != null)
		{
			this.units.Free();
			this.units = null;
		}
		if (this.combatModules != null)
		{
			this.combatModules.Free();
			this.combatModules = null;
		}
		this.planet = null;
		this.factory = null;
		this.spaceSector = null;
		this.gameData = null;
		this.mecha = null;
	}

	// Token: 0x17000184 RID: 388
	// (get) Token: 0x06000B4B RID: 2891 RVA: 0x000AA5F6 File Offset: 0x000A87F6
	public bool isLocalLoaded
	{
		get
		{
			return this.gameData.localLoadedPlanetFactory == this.factory;
		}
	}

	// Token: 0x06000B4C RID: 2892 RVA: 0x000AA60B File Offset: 0x000A880B
	public void Export(BinaryWriter w)
	{
		w.Write(0);
		this.units.Export(w);
		this.fleets.Export(w);
		this.combatModules.Export(w);
	}

	// Token: 0x06000B4D RID: 2893 RVA: 0x000AA638 File Offset: 0x000A8838
	public void Import(BinaryReader r)
	{
		r.ReadInt32();
		this.units.Import(r);
		this.fleets.Import(r);
		this.combatModules.Import(r);
		int cursor = this.combatModules.cursor;
		for (int i = 1; i < cursor; i++)
		{
			CombatModuleComponent combatModuleComponent = this.combatModules.buffer[i];
			if (combatModuleComponent != null && combatModuleComponent.id == i)
			{
				combatModuleComponent.AfterImport(this.gameData);
			}
		}
		if (this.gameData.patch < 11)
		{
			for (int j = 1; j < this.fleets.cursor; j++)
			{
				ref FleetComponent ptr = ref this.fleets.buffer[j];
				if (ptr.id == j)
				{
					ref CraftData ptr2 = ref this.factory.craftPool[ptr.craftId];
					if (ptr2.id == ptr.craftId)
					{
						if (ptr2.owner == -1)
						{
							ref ModuleFleet ptr3 = ref this.mecha.groundCombatModule.moduleFleets[(int)ptr2.port];
							bool flag = true;
							for (int k = 0; k < ptr3.fighters.Length; k++)
							{
								if (ptr3.fighters[k].craftId > 0)
								{
									flag = false;
									break;
								}
							}
							if (flag)
							{
								this.factory.RemoveCraftWithComponents(ptr2.id);
								ptr3.OnFleetComponentRemoved();
							}
						}
						else if (ptr2.owner > 0)
						{
							ref EntityData ptr4 = ref this.factory.entityPool[ptr2.owner];
							if (ptr4.id == ptr2.owner && ptr4.combatModuleId > 0)
							{
								ref CombatModuleComponent ptr5 = ref this.combatModules.buffer[ptr4.combatModuleId];
								if (ptr5.id == ptr4.combatModuleId)
								{
									ref ModuleFleet ptr6 = ref ptr5.moduleFleets[(int)ptr2.port];
									bool flag2 = true;
									for (int l = 0; l < ptr6.fighters.Length; l++)
									{
										if (ptr6.fighters[l].craftId > 0)
										{
											flag2 = false;
											break;
										}
									}
									if (flag2)
									{
										this.factory.RemoveCraftWithComponents(ptr2.id);
										ptr6.OnFleetComponentRemoved();
									}
								}
							}
						}
					}
				}
			}
		}
		if (this.gameData.patch < 13)
		{
			for (int m = 1; m < this.units.cursor; m++)
			{
				ref UnitComponent ptr7 = ref this.units.buffer[m];
				if (ptr7.id == m)
				{
					ref CraftData ptr8 = ref this.factory.craftPool[ptr7.craftId];
					if (ptr8.id == ptr7.craftId)
					{
						if (ptr8.owner == 0)
						{
							this.factory.RemoveCraftWithComponents(ptr8.id);
						}
						else if (this.factory.craftPool[ptr8.owner].id != ptr8.owner)
						{
							this.factory.RemoveCraftWithComponents(ptr8.id);
						}
					}
				}
			}
		}
	}

	// Token: 0x06000B4E RID: 2894 RVA: 0x000AA954 File Offset: 0x000A8B54
	public int NewFleetComponent(int craftId, PrefabDesc desc)
	{
		ref FleetComponent ptr = ref this.fleets.Add();
		ref CraftData ptr2 = ref this.factory.craftPool[craftId];
		ptr.craftId = craftId;
		ptr.owner = ptr2.owner;
		ptr.dispatch = true;
		ptr2.fleetId = ptr.id;
		this.factory.skillSystem.audio.AddPlanetAudio(desc.audioProtoId0, 2f, ptr2.astroId, ptr2.pos, 0);
		return ptr.id;
	}

	// Token: 0x06000B4F RID: 2895 RVA: 0x000AA9DE File Offset: 0x000A8BDE
	public void RemoveFleetComponent(int id)
	{
		this.fleets.Remove(id);
	}

	// Token: 0x06000B50 RID: 2896 RVA: 0x000AA9EC File Offset: 0x000A8BEC
	public int NewUnitComponent(int craftId, PrefabDesc desc)
	{
		ref UnitComponent ptr = ref this.units.Add();
		ref CraftData ptr2 = ref this.factory.craftPool[craftId];
		ptr.craftId = craftId;
		ptr.protoId = ptr2.protoId;
		ptr.behavior = EUnitBehavior.Initialize;
		ptr2.unitId = ptr.id;
		int modelIndex = (int)ptr2.modelIndex;
		ref CombatStat ptr3 = ref this.factory.skillSystem.combatStats.Add();
		ptr3.hpMax = (int)((float)SkillSystem.HpMaxByModelIndex[modelIndex] * this.gameData.history.combatDroneDurabilityRatio * (1f + this.gameData.history.globalHpEnhancement) + 0.5f);
		ptr3.hp = (int)((float)ptr3.hpMax * 0.2f + 0.5f);
		ptr3.hpRecover = SkillSystem.HpRecoverByModelIndex[modelIndex];
		ptr3.astroId = (ptr3.originAstroId = this.factory.planetId);
		ptr3.objectType = 6;
		ptr3.objectId = craftId;
		ptr3.dynamic = (ptr2.dynamic ? 1 : 0);
		ptr3.localPos = ptr2.pos;
		ptr3.size = SkillSystem.BarWidthByModelIndex[modelIndex];
		ptr2.combatStatId = ptr3.id;
		ptr.hpShortage = ptr3.hpMax - ptr3.hp;
		return ptr.id;
	}

	// Token: 0x06000B51 RID: 2897 RVA: 0x000AAB3D File Offset: 0x000A8D3D
	public void RemoveUnitComponent(int id)
	{
		this.units.Remove(id);
	}

	// Token: 0x06000B52 RID: 2898 RVA: 0x000AAB4B File Offset: 0x000A8D4B
	public int NewCombatModuleComponent(int entityId, PrefabDesc desc)
	{
		CombatModuleComponent combatModuleComponent = this.combatModules.Add();
		combatModuleComponent.entityId = entityId;
		combatModuleComponent.Init(this.gameData);
		combatModuleComponent.Setup(desc, this.gameData);
		return combatModuleComponent.id;
	}

	// Token: 0x06000B53 RID: 2899 RVA: 0x000AAB7D File Offset: 0x000A8D7D
	public void RemoveCombatModuleComponent(int id)
	{
		CombatModuleComponent combatModuleComponent = this.combatModules.buffer[id];
		combatModuleComponent.KillAllCraftsDirectly();
		combatModuleComponent.Free();
		this.combatModules.Remove(id);
	}

	// Token: 0x06000B54 RID: 2900 RVA: 0x000AABA4 File Offset: 0x000A8DA4
	public void GameTick(long tick, bool isActive)
	{
		bool isLocalLoaded = this.isLocalLoaded;
		GameHistoryData history = this.gameData.history;
		EnemyData[] enemyPool = this.factory.enemyPool;
		ref CombatSettings combatSettings = ref this.gameData.history.combatSettings;
		bool flag = this.factory.planet.factoryModel == null || this.factory.planet.factoryModel.disableFleet;
		bool flag2 = isLocalLoaded;
		ObjectRenderer[] array = isLocalLoaded ? this.planet.factoryModel.gpuiManager.objectRenderers : null;
		if (array == null)
		{
			flag2 = false;
		}
		else
		{
			this.planet.factoryModel.craftDirty = (this.planet.factoryModel.craftDirty || this.fleets.count + this.units.count > 0);
		}
		int num = (int)(tick % 60L);
		UnitComponent.gameTick = tick;
		CombatUpgradeData combatUpgradeData = default(CombatUpgradeData);
		history.GetCombatUpgradeData(ref combatUpgradeData);
		bool flag3 = false;
		Pose pose;
		if (isLocalLoaded)
		{
			if (flag3)
			{
				pose = this.multithreadPlayerPose;
			}
			else
			{
				pose.position = this.mecha.player.position;
				VectorLF3 vectorLF;
				this.spaceSector.InverseTransformToAstro_ref(this.factory.planetId, ref this.mecha.player.uPosition, ref this.mecha.player.uRotation, out vectorLF, out pose.rotation);
			}
		}
		else
		{
			pose = new Pose(Vector3.zero, Quaternion.identity);
		}
		if (this.combatModules.count > 0)
		{
			DeepProfiler.BeginSample(DPEntry.CBGCombatModule, -1, -1L);
			int cursor = this.combatModules.cursor;
			CombatModuleComponent[] buffer = this.combatModules.buffer;
			for (int i = 1; i < cursor; i++)
			{
				CombatModuleComponent combatModuleComponent = buffer[i];
				if (combatModuleComponent != null && combatModuleComponent.id == i)
				{
					combatModuleComponent.GameTick(tick, this.factory, null, ref combatUpgradeData);
				}
			}
			DeepProfiler.EndSample(-1, -2L);
		}
		if (this.fleets.count > 0)
		{
			DeepProfiler.BeginSample(DPEntry.CBGCraft, -1, 0L);
			float realRadius = this.planet.realRadius;
			FleetComponent[] buffer2 = this.fleets.buffer;
			int cursor2 = this.fleets.cursor;
			for (int j = 1; j < cursor2; j++)
			{
				ref FleetComponent ptr = ref buffer2[j];
				if (ptr.id == j)
				{
					ref CraftData ptr2 = ref this.factory.craftPool[ptr.craftId];
					PrefabDesc pdesc = SpaceSector.PrefabDescByModelIndex[(int)ptr2.modelIndex];
					bool flag4 = true;
					if (!ptr.CheckOwnerExist(ref ptr2, this.factory, this.mecha))
					{
						this.factory.RemoveCraftDeferred(ptr.craftId);
						flag4 = false;
					}
					if (flag4)
					{
						if (j % 60 == num)
						{
							ptr.SensorLogic_Ground(ref ptr2, pdesc, this.factory, tick);
							ptr2.hashAddress = this.factory.hashSystemDynamic.UpdateObjectHashAddress(ptr2.hashAddress, ptr2.id, ptr2.pos, EObjectType.Craft);
						}
						this.factory.craftAnimPool[ptr2.id].state = 1U;
						ptr.InternalUpdate_Ground(ref ptr2, ref pose, pdesc, enemyPool, this.mecha, realRadius, this.factory);
						ptr.AssembleUnits_Ground(ref ptr2, ref combatUpgradeData, pdesc, this.mecha, this.factory, tick);
						if (ptr.DeterminActiveEnemyUnits(false, tick))
						{
							ptr.ActiveEnemyUnits_Ground(this.factory, pdesc);
						}
						if (flag || ptr.owner > 0)
						{
							this.factory.craftAnimPool[ptr2.id].state = 0U;
						}
						if (flag2)
						{
							DynamicRenderer dynamicRenderer = array[(int)ptr2.modelIndex] as DynamicRenderer;
							if (dynamicRenderer != null)
							{
								GPUOBJECT[] instPool = dynamicRenderer.instPool;
								int modelId = ptr2.modelId;
								instPool[modelId].posx = (float)ptr2.pos.x;
								instPool[modelId].posy = (float)ptr2.pos.y;
								instPool[modelId].posz = (float)ptr2.pos.z;
								instPool[modelId].rotx = ptr2.rot.x;
								instPool[modelId].roty = ptr2.rot.y;
								instPool[modelId].rotz = ptr2.rot.z;
								instPool[modelId].rotw = ptr2.rot.w;
							}
						}
					}
				}
			}
			DeepProfiler.EndSample(-1, -2L);
		}
		if (this.units.count > 0)
		{
			DeepProfiler.BeginSample(DPEntry.CBGCraft, -1, 1L);
			UnitComponent[] buffer3 = this.units.buffer;
			int cursor3 = this.units.cursor;
			for (int k = 1; k < cursor3; k++)
			{
				ref UnitComponent ptr3 = ref buffer3[k];
				if (ptr3.id == k)
				{
					ref CraftData ptr4 = ref this.factory.craftPool[ptr3.craftId];
					PrefabDesc pdesc2 = SpaceSector.PrefabDescByModelIndex[(int)ptr4.modelIndex];
					if (k % 60 == num)
					{
						ptr3.hatred.Fade(0.75f, 5);
						ptr3.SensorLogic_Groud(ref ptr4, pdesc2, this.factory);
						ptr4.hashAddress = this.factory.hashSystemDynamic.UpdateObjectHashAddress(ptr4.hashAddress, ptr4.id, ptr4.pos, EObjectType.Craft);
						if (ptr4.willBroadcast)
						{
							ptr4.willBroadcast = false;
							if (ptr3.hatred.max.value > 80 && ptr3.hatred.max.target > 0)
							{
								ptr3.BroadcastHatred(this.factory, buffer3, ref ptr4, 300);
							}
						}
					}
					ptr3.AssistTeammates_Ground(ref ptr4, this.factory, this.mecha);
					ptr3.UpdateFireCondition(ptr4.isSpace, pdesc2, ref combatUpgradeData);
					bool flag5 = false;
					bool flag6 = false;
					bool flag7 = true;
					if (ptr4.owner > 0)
					{
						ref CraftData ptr5 = ref this.factory.craftPool[ptr4.owner];
						if (ptr5.id != 0 && ptr5.fleetId > 0)
						{
							ref FleetComponent ptr6 = ref this.fleets.buffer[ptr5.fleetId];
							ptr3.DetermineBehavior(ref ptr6.target, ref ptr6.targetPos, 0, ptr6.dispatch);
							flag5 = true;
							if (ptr5.owner < 0)
							{
								bool flag8 = !ptr3.UpdateMechaEnergy(this.mecha, pdesc2, ptr4.isSpace);
								flag6 = flag8;
								if (flag6)
								{
									ptr3.behavior = EUnitBehavior.Recycled;
								}
								else if (flag8)
								{
									flag7 = false;
								}
							}
							else if (ptr5.owner > 0)
							{
								ref EntityData ptr7 = ref this.factory.entityPool[ptr5.owner];
								if (ptr7.battleBaseId > 0)
								{
									BattleBaseComponent battleBaseComponent = this.factory.defenseSystem.battleBases.buffer[ptr7.battleBaseId];
									if (battleBaseComponent != null && battleBaseComponent.id == ptr7.battleBaseId)
									{
										bool flag9 = !ptr3.UpdateBattleBaseEnergy(battleBaseComponent, pdesc2);
										flag6 = flag9;
										if (flag6)
										{
											ptr3.behavior = EUnitBehavior.Recycled;
										}
										else if (flag9)
										{
											flag7 = false;
										}
									}
								}
							}
						}
					}
					if (flag7)
					{
						if (flag5)
						{
							switch (ptr3.behavior)
							{
							case EUnitBehavior.None:
								ptr3.RunBehavior_None();
								break;
							case EUnitBehavior.Initialize:
								ptr3.RunBehavior_Initialize_Ground(this.factory, this.mecha, pdesc2, ref ptr4, ref combatUpgradeData, history.fighterInitializeSpeedScale);
								break;
							case EUnitBehavior.Recycled:
								ptr3.RunBehavior_Recycled_Ground(this.factory, this.mecha, pdesc2, ref ptr4, ref combatSettings, ref combatUpgradeData, flag6);
								break;
							case EUnitBehavior.KeepForm:
								ptr3.RunBehavior_KeepForm(ref ptr4);
								break;
							case EUnitBehavior.SeekForm:
								ptr3.RunBehavior_SeekForm_Ground(this.factory, this.mecha, pdesc2, ref ptr4, ref combatUpgradeData);
								break;
							case EUnitBehavior.Engage:
								ptr3.RunBehavior_Engage_Ground(this.factory, this.mecha, pdesc2, ref ptr4, ref combatSettings, ref combatUpgradeData);
								break;
							}
						}
						else
						{
							this.factory.RemoveCraftDeferred(ptr3.craftId);
						}
						if (flag2)
						{
							DynamicRenderer dynamicRenderer2 = array[(int)ptr4.modelIndex] as DynamicRenderer;
							if (dynamicRenderer2 != null)
							{
								GPUOBJECT[] instPool2 = dynamicRenderer2.instPool;
								int modelId2 = ptr4.modelId;
								instPool2[modelId2].posx = (float)ptr4.pos.x;
								instPool2[modelId2].posy = (float)ptr4.pos.y;
								instPool2[modelId2].posz = (float)ptr4.pos.z;
								instPool2[modelId2].rotx = ptr4.rot.x;
								instPool2[modelId2].roty = ptr4.rot.y;
								instPool2[modelId2].rotz = ptr4.rot.z;
								instPool2[modelId2].rotw = ptr4.rot.w;
								Vector4[] extraPool = dynamicRenderer2.extraPool;
								int modelId3 = ptr4.modelId;
								extraPool[modelId3].x = ptr3.anim;
								extraPool[modelId3].z = ptr3.steering;
								extraPool[modelId3].w = ptr3.speed;
							}
						}
					}
				}
			}
			DeepProfiler.EndSample(-1, -2L);
		}
	}

	// Token: 0x06000B55 RID: 2901 RVA: 0x000AB47C File Offset: 0x000A967C
	public void PostGameTick(long tick)
	{
		if (this.units.count > 0)
		{
			DeepProfiler.BeginSample(DPEntry.CBGCraft, -1, 1L);
			CraftData[] craftPool = this.factory.craftPool;
			int cursor = this.units.cursor;
			UnitComponent[] buffer = this.units.buffer;
			for (int i = 1; i < cursor; i++)
			{
				ref UnitComponent ptr = ref buffer[i];
				if (ptr.id == i)
				{
					ref CraftData ptr2 = ref craftPool[ptr.craftId];
					PrefabDesc pdesc = SpaceSector.PrefabDescByModelIndex[(int)ptr2.modelIndex];
					ptr.PostGameTick_Ground(this.factory, pdesc, ref ptr2);
				}
			}
			DeepProfiler.EndSample(-1, -2L);
		}
	}

	// Token: 0x04000D27 RID: 3367
	public PlanetData planet;

	// Token: 0x04000D28 RID: 3368
	public PlanetFactory factory;

	// Token: 0x04000D29 RID: 3369
	public SpaceSector spaceSector;

	// Token: 0x04000D2A RID: 3370
	public GameData gameData;

	// Token: 0x04000D2B RID: 3371
	public Mecha mecha;

	// Token: 0x04000D2C RID: 3372
	public DataPool<FleetComponent> fleets;

	// Token: 0x04000D2D RID: 3373
	public DataPool<UnitComponent> units;

	// Token: 0x04000D2E RID: 3374
	public ObjectPool<CombatModuleComponent> combatModules;

	// Token: 0x04000D2F RID: 3375
	public Pose multithreadPlayerPose = new Pose(Vector3.zero, Quaternion.identity);
}
