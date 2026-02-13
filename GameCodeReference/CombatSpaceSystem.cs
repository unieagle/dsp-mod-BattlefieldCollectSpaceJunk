using System;
using System.IO;
using UnityEngine;

// Token: 0x02000168 RID: 360
public class CombatSpaceSystem
{
	// Token: 0x06000B56 RID: 2902 RVA: 0x000AB520 File Offset: 0x000A9720
	public CombatSpaceSystem(SpaceSector _sector)
	{
		this.spaceSector = _sector;
		this.gameData = this.spaceSector.gameData;
		this.mecha = this.gameData.mainPlayer.mecha;
		this.fleets = new DataPool<FleetComponent>();
		this.units = new DataPool<UnitComponent>();
		this.fleets.Reset();
		this.units.Reset();
	}

	// Token: 0x06000B57 RID: 2903 RVA: 0x000AB590 File Offset: 0x000A9790
	public CombatSpaceSystem(SpaceSector _sector, bool import)
	{
		this.spaceSector = _sector;
		this.gameData = this.spaceSector.gameData;
		this.mecha = this.gameData.mainPlayer.mecha;
		this.fleets = new DataPool<FleetComponent>();
		this.units = new DataPool<UnitComponent>();
	}

	// Token: 0x06000B58 RID: 2904 RVA: 0x000AB5E8 File Offset: 0x000A97E8
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
		this.spaceSector = null;
		this.gameData = null;
		this.mecha = null;
	}

	// Token: 0x17000185 RID: 389
	// (get) Token: 0x06000B59 RID: 2905 RVA: 0x000AB63E File Offset: 0x000A983E
	public bool inStarmap
	{
		get
		{
			return UIGame.viewMode == EViewMode.Starmap;
		}
	}

	// Token: 0x06000B5A RID: 2906 RVA: 0x000AB648 File Offset: 0x000A9848
	public void Export(BinaryWriter w)
	{
		w.Write(0);
		this.units.Export(w);
		this.fleets.Export(w);
	}

	// Token: 0x06000B5B RID: 2907 RVA: 0x000AB66C File Offset: 0x000A986C
	public void Import(BinaryReader r)
	{
		r.ReadInt32();
		this.units.Import(r);
		this.fleets.Import(r);
		if (this.gameData.patch < 11)
		{
			for (int i = 1; i < this.fleets.cursor; i++)
			{
				ref FleetComponent ptr = ref this.fleets.buffer[i];
				if (ptr.id == i)
				{
					ref CraftData ptr2 = ref this.spaceSector.craftPool[ptr.craftId];
					if (ptr2.id == ptr.craftId)
					{
						if (ptr2.owner == -1)
						{
							ref ModuleFleet ptr3 = ref this.mecha.spaceCombatModule.moduleFleets[(int)ptr2.port];
							bool flag = true;
							for (int j = 0; j < ptr3.fighters.Length; j++)
							{
								if (ptr3.fighters[j].craftId > 0)
								{
									flag = false;
									break;
								}
							}
							if (flag)
							{
								this.spaceSector.RemoveCraftWithComponents(ptr2.id);
								ptr3.OnFleetComponentRemoved();
							}
						}
						else
						{
							int owner = ptr2.owner;
						}
					}
				}
			}
		}
		if (this.gameData.patch < 13)
		{
			for (int k = 1; k < this.units.cursor; k++)
			{
				ref UnitComponent ptr4 = ref this.units.buffer[k];
				if (ptr4.id == k)
				{
					ref CraftData ptr5 = ref this.spaceSector.craftPool[ptr4.craftId];
					if (ptr5.id == ptr4.craftId)
					{
						if (ptr5.owner == 0)
						{
							this.spaceSector.RemoveCraftWithComponents(ptr5.id);
						}
						else if (this.spaceSector.craftPool[ptr5.owner].id != ptr5.owner)
						{
							this.spaceSector.RemoveCraftWithComponents(ptr5.id);
						}
					}
				}
			}
		}
		int patch = this.gameData.patch;
		for (int l = 1; l < this.units.cursor; l++)
		{
			ref UnitComponent ptr6 = ref this.units.buffer[l];
			if (ptr6.id == l)
			{
				ref CraftData ptr7 = ref this.spaceSector.craftPool[ptr6.craftId];
				if (ptr7.id == ptr6.craftId)
				{
					if (double.IsNaN(ptr7.pos.x) || double.IsNaN(ptr7.pos.y) || double.IsNaN(ptr7.pos.z))
					{
						this.spaceSector.RemoveCraftWithComponents(ptr7.id);
					}
					else if (ptr7.owner <= 0)
					{
						this.spaceSector.RemoveCraftWithComponents(ptr7.id);
					}
					else if (this.spaceSector.craftPool[ptr7.owner].id != ptr7.owner)
					{
						this.spaceSector.RemoveCraftWithComponents(ptr7.id);
					}
				}
			}
		}
		for (int m = 1; m < this.fleets.cursor; m++)
		{
			ref FleetComponent ptr8 = ref this.fleets.buffer[m];
			if (ptr8.id == m)
			{
				ref CraftData ptr9 = ref this.spaceSector.craftPool[ptr8.craftId];
				if (ptr9.id == ptr8.craftId && !ptr8.CheckOwnerExist(ref ptr9, null, this.gameData.mainPlayer.mecha))
				{
					this.spaceSector.RemoveCraftWithComponents(ptr8.craftId);
				}
			}
		}
	}

	// Token: 0x06000B5C RID: 2908 RVA: 0x000ABA08 File Offset: 0x000A9C08
	public int NewFleetComponent(int craftId, PrefabDesc desc)
	{
		ref FleetComponent ptr = ref this.fleets.Add();
		ptr.craftId = craftId;
		ptr.owner = this.spaceSector.craftPool[craftId].owner;
		ptr.dispatch = true;
		ref CraftData ptr2 = ref this.spaceSector.craftPool[craftId];
		ptr2.fleetId = ptr.id;
		VectorLF3 upos;
		this.spaceSector.TransformFromAstro_ref(ptr2.astroId, out upos, ref ptr2.pos);
		this.spaceSector.skillSystem.audio.AddSpaceAudio(desc.audioProtoId0, 2f, upos, 0);
		return ptr.id;
	}

	// Token: 0x06000B5D RID: 2909 RVA: 0x000ABAAB File Offset: 0x000A9CAB
	public void RemoveFleetComponent(int id)
	{
		this.fleets.Remove(id);
	}

	// Token: 0x06000B5E RID: 2910 RVA: 0x000ABABC File Offset: 0x000A9CBC
	public int NewUnitComponent(int craftId, PrefabDesc desc)
	{
		ref UnitComponent ptr = ref this.units.Add();
		ref CraftData ptr2 = ref this.spaceSector.craftPool[craftId];
		ptr.craftId = craftId;
		ptr.protoId = ptr2.protoId;
		ptr.behavior = EUnitBehavior.Initialize;
		ptr2.unitId = ptr.id;
		int modelIndex = (int)ptr2.modelIndex;
		ref CombatStat ptr3 = ref this.spaceSector.skillSystem.combatStats.Add();
		ptr3.hpMax = (int)((float)SkillSystem.HpMaxByModelIndex[modelIndex] * this.gameData.history.combatShipDurabilityRatio * (1f + this.gameData.history.globalHpEnhancement) + 0.5f);
		ptr3.hp = (int)((float)ptr3.hpMax * 0.2f + 0.5f);
		ptr3.hpRecover = SkillSystem.HpRecoverByModelIndex[modelIndex];
		ptr3.astroId = (ptr3.originAstroId = ptr2.astroId);
		ptr3.objectType = 6;
		ptr3.objectId = craftId;
		ptr3.dynamic = (ptr2.dynamic ? 1 : 0);
		ptr3.localPos = ptr2.pos;
		ptr3.size = SkillSystem.BarWidthByModelIndex[modelIndex];
		ptr2.combatStatId = ptr3.id;
		ptr.hpShortage = ptr3.hpMax - ptr3.hp;
		return ptr.id;
	}

	// Token: 0x06000B5F RID: 2911 RVA: 0x000ABC08 File Offset: 0x000A9E08
	public void RemoveUnitComponent(int id)
	{
		this.units.Remove(id);
	}

	// Token: 0x06000B60 RID: 2912 RVA: 0x000ABC18 File Offset: 0x000A9E18
	public void GameTick(long tick)
	{
		GameHistoryData history = this.gameData.history;
		EnemyData[] enemyPool = this.spaceSector.enemyPool;
		ref CombatSettings combatSettings = ref this.gameData.history.combatSettings;
		bool flag = this.spaceSector.model == null || this.spaceSector.model.disableFleet;
		SpaceObjectRenderer[] objectRenderers = this.spaceSector.model.gpuiManager.objectRenderers;
		this.spaceSector.model.craftDirty = true;
		int num = (int)(tick % 60L);
		UnitComponent.gameTick = tick;
		CombatUpgradeData combatUpgradeData = default(CombatUpgradeData);
		history.GetCombatUpgradeData(ref combatUpgradeData);
		ref VectorLF3 ptr = ref this.gameData.relativePos;
		ref Quaternion ptr2 = ref this.gameData.relativeRot;
		Vector3 vector = new Vector3(0f, 0f, 0f);
		Quaternion identity = Quaternion.identity;
		bool inStarmap = this.inStarmap;
		if (this.fleets.count > 0)
		{
			DeepProfiler.BeginSample(DPEntry.CBSCraft, -1, 0L);
			FleetComponent[] buffer = this.fleets.buffer;
			int cursor = this.fleets.cursor;
			for (int i = 1; i < cursor; i++)
			{
				ref FleetComponent ptr3 = ref buffer[i];
				if (ptr3.id == i)
				{
					ref CraftData ptr4 = ref this.spaceSector.craftPool[ptr3.craftId];
					PrefabDesc pdesc = SpaceSector.PrefabDescByModelIndex[(int)ptr4.modelIndex];
					if (i % 60 == num)
					{
						ptr3.SensorLogic_Space(ref ptr4, pdesc, this.spaceSector, tick);
					}
					ptr3.InternalUpdate_Space(ref ptr4, ref this.spaceSector.craftAnimPool[ptr4.id], pdesc, this.spaceSector, this.mecha);
					ptr3.AssembleUnits_Space(ref ptr4, ref combatUpgradeData, pdesc, this.mecha, this.spaceSector, tick);
					ptr3.DetermineCraftAstroId(this.spaceSector, ref ptr4);
					if (ptr3.DeterminActiveEnemyUnits(true, tick))
					{
						ptr3.ActiveEnemyUnits_Space(this.spaceSector, pdesc);
					}
					if (flag)
					{
						this.spaceSector.craftAnimPool[ptr4.id].state = 0U;
					}
					SpaceDynamicRenderer spaceDynamicRenderer = objectRenderers[(int)ptr4.modelIndex] as SpaceDynamicRenderer;
					if (spaceDynamicRenderer != null)
					{
						ref SPACEOBJECT ptr5 = ref spaceDynamicRenderer.instPool[ptr4.modelId];
						ptr5.astroId = (uint)ptr4.astroId;
						if (ptr5.astroId == 0U)
						{
							if (inStarmap && UIGame.viewModeReady)
							{
								ref VectorLF3 ptr6 = ref this.gameData.starmapViewPos;
								ptr5.posx = (float)((ptr4.pos.x - ptr6.x) * 0.00025);
								ptr5.posy = (float)((ptr4.pos.y - ptr6.y) * 0.00025);
								ptr5.posz = (float)((ptr4.pos.z - ptr6.z) * 0.00025);
								ptr5.rotx = ptr4.rot.x;
								ptr5.roty = ptr4.rot.y;
								ptr5.rotz = ptr4.rot.z;
								ptr5.rotw = ptr4.rot.w;
							}
							else
							{
								VectorLF3 vectorLF;
								vectorLF.x = ptr4.pos.x - ptr.x;
								vectorLF.y = ptr4.pos.y - ptr.y;
								vectorLF.z = ptr4.pos.z - ptr.z;
								Maths.QInvRotateLF_ref(ref ptr2, ref vectorLF, ref vector);
								ptr5.posx = vector.x;
								ptr5.posy = vector.y;
								ptr5.posz = vector.z;
								Maths.QInvMultiply_ref(ref ptr2, ref ptr4.rot, out identity);
								ptr5.rotx = identity.x;
								ptr5.roty = identity.y;
								ptr5.rotz = identity.z;
								ptr5.rotw = identity.w;
							}
						}
						else
						{
							ptr5.posx = (float)ptr4.pos.x;
							ptr5.posy = (float)ptr4.pos.y;
							ptr5.posz = (float)ptr4.pos.z;
							ptr5.rotx = ptr4.rot.x;
							ptr5.roty = ptr4.rot.y;
							ptr5.rotz = ptr4.rot.z;
							ptr5.rotw = ptr4.rot.w;
						}
					}
				}
			}
			DeepProfiler.EndSample(-1, -2L);
		}
		if (this.units.count > 0)
		{
			DeepProfiler.BeginSample(DPEntry.CBSCraft, -1, 1L);
			UnitComponent[] buffer2 = this.units.buffer;
			int cursor2 = this.units.cursor;
			for (int j = 1; j < cursor2; j++)
			{
				ref UnitComponent ptr7 = ref buffer2[j];
				if (ptr7.id == j)
				{
					ref CraftData ptr8 = ref this.spaceSector.craftPool[ptr7.craftId];
					PrefabDesc pdesc2 = SpaceSector.PrefabDescByModelIndex[(int)ptr8.modelIndex];
					if (j % 60 == num)
					{
						ptr7.hatred.Fade(0.75f, 5);
						ptr7.SensorLogic_Space(ref ptr8, pdesc2, this.spaceSector);
					}
					ptr7.AssistTeammates_Space(ref ptr8, this.spaceSector, this.mecha);
					ptr7.UpdateFireCondition(ptr8.isSpace, pdesc2, ref combatUpgradeData);
					int orbitAstroId = 0;
					bool flag2 = false;
					bool flag3 = false;
					if (ptr8.owner > 0)
					{
						ref CraftData ptr9 = ref this.spaceSector.craftPool[ptr8.owner];
						if (ptr9.id != 0 && ptr9.fleetId > 0)
						{
							ref FleetComponent ptr10 = ref this.fleets.buffer[ptr9.fleetId];
							orbitAstroId = FleetComponent.DetermineOrbitingAstro(this.spaceSector, ref ptr9);
							ptr7.DetermineBehavior(ref ptr10.target, ref ptr10.targetPos, orbitAstroId, ptr10.dispatch);
							flag2 = true;
							if (ptr9.owner < 0 && this.mecha.player.isAlive)
							{
								flag3 = !ptr7.UpdateMechaEnergy(this.mecha, pdesc2, ptr8.isSpace);
								if (flag3)
								{
									ptr7.behavior = EUnitBehavior.Recycled;
								}
							}
						}
					}
					if (flag2)
					{
						switch (ptr7.behavior)
						{
						case EUnitBehavior.None:
							ptr7.RunBehavior_None();
							break;
						case EUnitBehavior.Initialize:
							ptr7.RunBehavior_Initialize_Space(this.spaceSector, this.mecha, pdesc2, ref ptr8, ref combatUpgradeData, orbitAstroId, history.fighterInitializeSpeedScale);
							break;
						case EUnitBehavior.Recycled:
							ptr7.RunBehavior_Recycled_Space(this.spaceSector, this.mecha, pdesc2, ref ptr8, ref combatSettings, ref combatUpgradeData, orbitAstroId, flag3);
							break;
						case EUnitBehavior.KeepForm:
							ptr7.RunBehavior_KeepForm(ref ptr8);
							break;
						case EUnitBehavior.SeekForm:
							ptr7.RunBehavior_SeekForm_Space(this.spaceSector, this.mecha, pdesc2, ref ptr8, ref combatUpgradeData);
							break;
						case EUnitBehavior.Engage:
							ptr7.RunBehavior_Engage_Space(this.spaceSector, this.mecha, pdesc2, ref ptr8, ref combatSettings, ref combatUpgradeData);
							break;
						case EUnitBehavior.Orbiting:
							ptr7.RunBehavior_Orbiting(this.spaceSector, this.mecha, pdesc2, ref ptr8, ref combatUpgradeData, orbitAstroId);
							break;
						}
					}
					else
					{
						this.spaceSector.RemoveCraftDeferred(ptr7.craftId);
					}
					ptr7.DetermineCraftAstroId(this.spaceSector, ref ptr8);
					SpaceDynamicRenderer spaceDynamicRenderer2 = objectRenderers[(int)ptr8.modelIndex] as SpaceDynamicRenderer;
					if (spaceDynamicRenderer2 != null)
					{
						ref SPACEOBJECT ptr11 = ref spaceDynamicRenderer2.instPool[ptr8.modelId];
						ptr11.astroId = (uint)ptr8.astroId;
						if (ptr11.astroId == 0U)
						{
							if (inStarmap && UIGame.viewModeReady)
							{
								ref VectorLF3 ptr12 = ref this.gameData.starmapViewPos;
								ptr11.posx = (float)((ptr8.pos.x - ptr12.x) * 0.00025);
								ptr11.posy = (float)((ptr8.pos.y - ptr12.y) * 0.00025);
								ptr11.posz = (float)((ptr8.pos.z - ptr12.z) * 0.00025);
								ptr11.rotx = ptr8.rot.x;
								ptr11.roty = ptr8.rot.y;
								ptr11.rotz = ptr8.rot.z;
								ptr11.rotw = ptr8.rot.w;
							}
							else
							{
								VectorLF3 vectorLF2;
								vectorLF2.x = ptr8.pos.x - ptr.x;
								vectorLF2.y = ptr8.pos.y - ptr.y;
								vectorLF2.z = ptr8.pos.z - ptr.z;
								Maths.QInvRotateLF_ref(ref ptr2, ref vectorLF2, ref vector);
								ptr11.posx = vector.x;
								ptr11.posy = vector.y;
								ptr11.posz = vector.z;
								Maths.QInvMultiply_ref(ref ptr2, ref ptr8.rot, out identity);
								ptr11.rotx = identity.x;
								ptr11.roty = identity.y;
								ptr11.rotz = identity.z;
								ptr11.rotw = identity.w;
							}
						}
						else
						{
							ptr11.posx = (float)ptr8.pos.x;
							ptr11.posy = (float)ptr8.pos.y;
							ptr11.posz = (float)ptr8.pos.z;
							ptr11.rotx = ptr8.rot.x;
							ptr11.roty = ptr8.rot.y;
							ptr11.rotz = ptr8.rot.z;
							ptr11.rotw = ptr8.rot.w;
							Vector4[] extraPool = spaceDynamicRenderer2.extraPool;
							int modelId = ptr8.modelId;
							extraPool[modelId].x = ptr7.anim;
							extraPool[modelId].z = ptr7.steering;
							extraPool[modelId].w = ptr7.speed;
						}
					}
				}
			}
			DeepProfiler.EndSample(-1, -2L);
		}
	}

	// Token: 0x06000B61 RID: 2913 RVA: 0x000AC640 File Offset: 0x000AA840
	public void PostGameTick(long tick)
	{
		if (this.units.count > 0)
		{
			DeepProfiler.BeginSample(DPEntry.CBSCraft, -1, 1L);
			CraftData[] craftPool = this.spaceSector.craftPool;
			int cursor = this.units.cursor;
			UnitComponent[] buffer = this.units.buffer;
			for (int i = 1; i < cursor; i++)
			{
				ref UnitComponent ptr = ref buffer[i];
				if (ptr.id == i)
				{
					ref CraftData ptr2 = ref craftPool[ptr.craftId];
					PrefabDesc pdesc = SpaceSector.PrefabDescByModelIndex[(int)ptr2.modelIndex];
					ptr.PostGameTick_Space(this.spaceSector, pdesc, ref ptr2);
				}
			}
			DeepProfiler.EndSample(-1, -2L);
		}
	}

	// Token: 0x04000D30 RID: 3376
	public SpaceSector spaceSector;

	// Token: 0x04000D31 RID: 3377
	public GameData gameData;

	// Token: 0x04000D32 RID: 3378
	public Mecha mecha;

	// Token: 0x04000D33 RID: 3379
	public DataPool<FleetComponent> fleets;

	// Token: 0x04000D34 RID: 3380
	public DataPool<UnitComponent> units;
}
