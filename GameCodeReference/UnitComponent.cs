using System;
using System.IO;
using UnityEngine;

// Token: 0x0200019A RID: 410
public struct UnitComponent : IPoolElement
{
	// Token: 0x170001CB RID: 459
	// (get) Token: 0x06000F52 RID: 3922 RVA: 0x000F5FE6 File Offset: 0x000F41E6
	// (set) Token: 0x06000F53 RID: 3923 RVA: 0x000F5FEE File Offset: 0x000F41EE
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

	// Token: 0x06000F54 RID: 3924 RVA: 0x000F5FF8 File Offset: 0x000F41F8
	public void Export(BinaryWriter w)
	{
		w.Write(0);
		w.Write(this.id);
		w.Write(this.craftId);
		w.Write(this.protoId);
		w.Write(this.isShooting0);
		w.Write(this.isShooting1);
		w.Write(this.fire0);
		w.Write(this.fire1);
		w.Write(this.muzzleIndex0);
		w.Write(this.muzzleIndex1);
		w.Write(this.isRetreating);
		w.Write(this.isCharging);
		w.Write(this.currentInitializeValue);
		w.Write(this.hpShortage);
		w.Write(this.adjustEngageRange);
		w.Write((byte)this.behavior);
		this.hatred.Export(w);
	}

	// Token: 0x06000F55 RID: 3925 RVA: 0x000F60CC File Offset: 0x000F42CC
	public void Import(BinaryReader r)
	{
		r.ReadInt32();
		this.id = r.ReadInt32();
		this.craftId = r.ReadInt32();
		this.protoId = r.ReadInt16();
		this.isShooting0 = r.ReadBoolean();
		this.isShooting1 = r.ReadBoolean();
		this.fire0 = r.ReadInt32();
		this.fire1 = r.ReadInt32();
		this.muzzleIndex0 = r.ReadByte();
		this.muzzleIndex1 = r.ReadByte();
		this.isRetreating = r.ReadBoolean();
		this.isCharging = r.ReadBoolean();
		this.currentInitializeValue = r.ReadInt32();
		this.hpShortage = r.ReadInt32();
		this.adjustEngageRange = r.ReadBoolean();
		this.behavior = (EUnitBehavior)r.ReadByte();
		this.hatred.Import(r);
	}

	// Token: 0x06000F56 RID: 3926 RVA: 0x000F61A0 File Offset: 0x000F43A0
	public void Reset()
	{
		this.id = 0;
		this.craftId = 0;
		this.protoId = 0;
		this.isShooting0 = false;
		this.isShooting1 = false;
		this.fire0 = 0;
		this.fire1 = 0;
		this.muzzleIndex0 = 0;
		this.muzzleIndex1 = 0;
		this.isRetreating = false;
		this.isCharging = false;
		this.currentInitializeValue = 0;
		this.hpShortage = 0;
		this.adjustEngageRange = false;
		this.behavior = EUnitBehavior.None;
		this.hatred.Reset();
	}

	// Token: 0x06000F57 RID: 3927 RVA: 0x000F6224 File Offset: 0x000F4424
	public void PostGameTick_Ground(PlanetFactory factory, PrefabDesc pdesc, ref CraftData craft)
	{
		if (this.behavior == EUnitBehavior.Engage)
		{
			ref CraftData ptr = ref factory.craftPool[craft.owner];
			int fleetId = ptr.fleetId;
			ref FleetComponent ptr2 = ref factory.combatGroundSystem.fleets.buffer[fleetId];
			float num = 35f;
			if (fleetId > 0 && ptr2.id == fleetId)
			{
				num = PlanetFactory.PrefabDescByModelIndex[(int)ptr.modelIndex].fleetMaxActiveArea + pdesc.craftUnitAttackRange0;
			}
			float num2 = num * num;
			float craftUnitAttackRange = pdesc.craftUnitAttackRange0;
			float craftUnitAttackRange2 = pdesc.craftUnitAttackRange0;
			bool flag = false;
			ref HatredTarget ptr3 = ref this.hatred.max;
			bool flag2 = true;
			if (ptr.owner == -1)
			{
				flag2 = GameMain.mainPlayer.mecha.groundCombatModule.attackBuilding;
			}
			if (ptr3.objectType == EObjectType.Enemy)
			{
				ref EnemyData ptr4 = ref factory.enemyPool[ptr3.objectId];
				if (ptr4.isInvincible || ptr4.id == 0)
				{
					flag = true;
					this.hatred.max = default(HatredTarget);
				}
				if ((ptr4.pos - ptr.pos).sqrMagnitude > (double)num2)
				{
					flag = true;
					this.hatred.max = default(HatredTarget);
				}
				if (ptr4.builderId > 0 && !flag2)
				{
					flag = true;
					this.hatred.max = default(HatredTarget);
				}
			}
			ptr3 = ref this.hatred.h1;
			if (ptr3.value > 0 && ptr3.objectType == EObjectType.Enemy)
			{
				ref EnemyData ptr5 = ref factory.enemyPool[ptr3.objectId];
				if (ptr5.isInvincible || ptr5.id == 0)
				{
					flag = true;
					this.hatred.h1 = default(HatredTarget);
				}
				if ((ptr5.pos - ptr.pos).sqrMagnitude > (double)num2)
				{
					flag = true;
					this.hatred.h1 = default(HatredTarget);
				}
				if (ptr5.builderId > 0 && !flag2)
				{
					flag = true;
					this.hatred.h1 = default(HatredTarget);
				}
			}
			ptr3 = ref this.hatred.h2;
			if (ptr3.value > 0 && ptr3.objectType == EObjectType.Enemy)
			{
				ref EnemyData ptr6 = ref factory.enemyPool[ptr3.objectId];
				if (ptr6.isInvincible || ptr6.id == 0)
				{
					flag = true;
					this.hatred.h2 = default(HatredTarget);
				}
				if ((ptr6.pos - ptr.pos).sqrMagnitude > (double)num2)
				{
					flag = true;
					this.hatred.h2 = default(HatredTarget);
				}
				if (ptr6.builderId > 0 && !flag2)
				{
					flag = true;
					this.hatred.h2 = default(HatredTarget);
				}
			}
			ptr3 = ref this.hatred.h3;
			if (ptr3.value > 0 && ptr3.objectType == EObjectType.Enemy)
			{
				ref EnemyData ptr7 = ref factory.enemyPool[ptr3.objectId];
				if (ptr7.isInvincible || ptr7.id == 0)
				{
					flag = true;
					this.hatred.h3 = default(HatredTarget);
				}
				if ((ptr7.pos - ptr.pos).sqrMagnitude > (double)num2)
				{
					flag = true;
					this.hatred.h3 = default(HatredTarget);
				}
				if (ptr7.builderId > 0 && !flag2)
				{
					flag = true;
					this.hatred.h3 = default(HatredTarget);
				}
			}
			ptr3 = ref this.hatred.h4;
			if (ptr3.value > 0 && ptr3.objectType == EObjectType.Enemy)
			{
				ref EnemyData ptr8 = ref factory.enemyPool[ptr3.objectId];
				if (ptr8.isInvincible || ptr8.id == 0)
				{
					flag = true;
					this.hatred.h4 = default(HatredTarget);
				}
				if ((ptr8.pos - ptr.pos).sqrMagnitude > (double)num2)
				{
					flag = true;
					this.hatred.h4 = default(HatredTarget);
				}
				if (ptr8.builderId > 0 && !flag2)
				{
					flag = true;
					this.hatred.h4 = default(HatredTarget);
				}
			}
			ptr3 = ref this.hatred.h5;
			if (ptr3.value > 0 && ptr3.objectType == EObjectType.Enemy)
			{
				ref EnemyData ptr9 = ref factory.enemyPool[ptr3.objectId];
				if (ptr9.isInvincible || ptr9.id == 0)
				{
					flag = true;
					this.hatred.h5 = default(HatredTarget);
				}
				if ((ptr9.pos - ptr.pos).sqrMagnitude > (double)num2)
				{
					flag = true;
					this.hatred.h5 = default(HatredTarget);
				}
				if (ptr9.builderId > 0 && !flag2)
				{
					flag = true;
					this.hatred.h5 = default(HatredTarget);
				}
			}
			ptr3 = ref this.hatred.h6;
			if (ptr3.value > 0 && ptr3.objectType == EObjectType.Enemy)
			{
				ref EnemyData ptr10 = ref factory.enemyPool[ptr3.objectId];
				if (ptr10.isInvincible || ptr10.id == 0)
				{
					flag = true;
					this.hatred.h6 = default(HatredTarget);
				}
				if ((ptr10.pos - ptr.pos).sqrMagnitude > (double)num2)
				{
					flag = true;
					this.hatred.h6 = default(HatredTarget);
				}
				if (ptr10.builderId > 0 && !flag2)
				{
					flag = true;
					this.hatred.h6 = default(HatredTarget);
				}
			}
			ptr3 = ref this.hatred.min;
			if (ptr3.value > 0 && ptr3.objectType == EObjectType.Enemy)
			{
				ref EnemyData ptr11 = ref factory.enemyPool[ptr3.objectId];
				if (ptr11.isInvincible || ptr11.id == 0)
				{
					flag = true;
					this.hatred.min = default(HatredTarget);
				}
				if ((ptr11.pos - ptr.pos).sqrMagnitude > (double)num2)
				{
					flag = true;
					this.hatred.min = default(HatredTarget);
				}
				if (ptr11.builderId > 0 && !flag2)
				{
					flag = true;
					this.hatred.min = default(HatredTarget);
				}
			}
			if (flag)
			{
				this.hatred.Arrange();
				if (this.hatred.max.value == 0)
				{
					this.RunBehavior_Engage_EmptyHatred(ref craft);
				}
			}
		}
	}

	// Token: 0x06000F58 RID: 3928 RVA: 0x000F68A4 File Offset: 0x000F4AA4
	public void PostGameTick_Space(SpaceSector sector, PrefabDesc pdesc, ref CraftData craft)
	{
		if (this.behavior == EUnitBehavior.Engage)
		{
			bool flag = false;
			ref HatredTarget ptr = ref this.hatred.max;
			bool flag2 = false;
			bool flag3 = true;
			if (sector.craftPool[craft.owner].owner == -1)
			{
				flag2 = GameMain.mainPlayer.mecha.spaceCombatModule.attackRelay;
				flag3 = GameMain.mainPlayer.mecha.spaceCombatModule.attackBuilding;
			}
			if (ptr.objectType == EObjectType.Enemy)
			{
				ref EnemyData ptr2 = ref sector.enemyPool[ptr.objectId];
				if (ptr2.isInvincible || ptr2.id == 0)
				{
					flag = true;
					this.hatred.max = default(HatredTarget);
				}
				ref CombatStat ptr3 = ref sector.skillSystem.combatStats.buffer[ptr2.combatStatId];
				if (((ptr2.combatStatId == 0) ? 1f : ((float)(ptr3.hp + ptr3.hpIncoming) / (float)ptr3.hpMax)) < -0.01f)
				{
					flag = true;
					this.hatred.max = default(HatredTarget);
				}
				if (ptr2.dfRelayId > 0 && !flag2)
				{
					flag = true;
					this.hatred.max = default(HatredTarget);
				}
				if (ptr2.builderId > 0 && !flag3)
				{
					flag = true;
					this.hatred.max = default(HatredTarget);
				}
			}
			ptr = ref this.hatred.h1;
			if (ptr.value > 0 && ptr.objectType == EObjectType.Enemy)
			{
				ref EnemyData ptr4 = ref sector.enemyPool[ptr.objectId];
				if (ptr4.isInvincible || ptr4.id == 0)
				{
					flag = true;
					this.hatred.h1 = default(HatredTarget);
				}
				ref CombatStat ptr5 = ref sector.skillSystem.combatStats.buffer[ptr4.combatStatId];
				if (((ptr4.combatStatId == 0) ? 1f : ((float)(ptr5.hp + ptr5.hpIncoming) / (float)ptr5.hpMax)) < -0.01f)
				{
					flag = true;
					this.hatred.h1 = default(HatredTarget);
				}
				if (ptr4.dfRelayId > 0 && !flag2)
				{
					flag = true;
					this.hatred.h1 = default(HatredTarget);
				}
				if (ptr4.builderId > 0 && !flag3)
				{
					flag = true;
					this.hatred.h1 = default(HatredTarget);
				}
			}
			ptr = ref this.hatred.h2;
			if (ptr.value > 0 && ptr.objectType == EObjectType.Enemy)
			{
				ref EnemyData ptr6 = ref sector.enemyPool[ptr.objectId];
				if (ptr6.isInvincible || ptr6.id == 0)
				{
					flag = true;
					this.hatred.h2 = default(HatredTarget);
				}
				ref CombatStat ptr7 = ref sector.skillSystem.combatStats.buffer[ptr6.combatStatId];
				if (((ptr6.combatStatId == 0) ? 1f : ((float)(ptr7.hp + ptr7.hpIncoming) / (float)ptr7.hpMax)) < -0.01f)
				{
					flag = true;
					this.hatred.h2 = default(HatredTarget);
				}
				if (ptr6.dfRelayId > 0 && !flag2)
				{
					flag = true;
					this.hatred.h2 = default(HatredTarget);
				}
				if (ptr6.builderId > 0 && !flag3)
				{
					flag = true;
					this.hatred.h2 = default(HatredTarget);
				}
			}
			ptr = ref this.hatred.h3;
			if (ptr.value > 0 && ptr.objectType == EObjectType.Enemy)
			{
				ref EnemyData ptr8 = ref sector.enemyPool[ptr.objectId];
				if (ptr8.isInvincible || ptr8.id == 0)
				{
					flag = true;
					this.hatred.h3 = default(HatredTarget);
				}
				ref CombatStat ptr9 = ref sector.skillSystem.combatStats.buffer[ptr8.combatStatId];
				if (((ptr8.combatStatId == 0) ? 1f : ((float)(ptr9.hp + ptr9.hpIncoming) / (float)ptr9.hpMax)) < -0.01f)
				{
					flag = true;
					this.hatred.h3 = default(HatredTarget);
				}
				if (ptr8.dfRelayId > 0 && !flag2)
				{
					flag = true;
					this.hatred.h3 = default(HatredTarget);
				}
				if (ptr8.builderId > 0 && !flag3)
				{
					flag = true;
					this.hatred.h3 = default(HatredTarget);
				}
			}
			ptr = ref this.hatred.h4;
			if (ptr.value > 0 && ptr.objectType == EObjectType.Enemy)
			{
				ref EnemyData ptr10 = ref sector.enemyPool[ptr.objectId];
				if (ptr10.isInvincible || ptr10.id == 0)
				{
					flag = true;
					this.hatred.h4 = default(HatredTarget);
				}
				ref CombatStat ptr11 = ref sector.skillSystem.combatStats.buffer[ptr10.combatStatId];
				if (((ptr10.combatStatId == 0) ? 1f : ((float)(ptr11.hp + ptr11.hpIncoming) / (float)ptr11.hpMax)) < -0.01f)
				{
					flag = true;
					this.hatred.h4 = default(HatredTarget);
				}
				if (ptr10.dfRelayId > 0 && !flag2)
				{
					flag = true;
					this.hatred.h4 = default(HatredTarget);
				}
				if (ptr10.builderId > 0 && !flag3)
				{
					flag = true;
					this.hatred.h4 = default(HatredTarget);
				}
			}
			ptr = ref this.hatred.h5;
			if (ptr.value > 0 && ptr.objectType == EObjectType.Enemy)
			{
				ref EnemyData ptr12 = ref sector.enemyPool[ptr.objectId];
				if (ptr12.isInvincible || ptr12.id == 0)
				{
					flag = true;
					this.hatred.h5 = default(HatredTarget);
				}
				ref CombatStat ptr13 = ref sector.skillSystem.combatStats.buffer[ptr12.combatStatId];
				if (((ptr12.combatStatId == 0) ? 1f : ((float)(ptr13.hp + ptr13.hpIncoming) / (float)ptr13.hpMax)) < -0.01f)
				{
					flag = true;
					this.hatred.h5 = default(HatredTarget);
				}
				if (ptr12.dfRelayId > 0 && !flag2)
				{
					flag = true;
					this.hatred.h5 = default(HatredTarget);
				}
				if (ptr12.builderId > 0 && !flag3)
				{
					flag = true;
					this.hatred.h5 = default(HatredTarget);
				}
			}
			ptr = ref this.hatred.h6;
			if (ptr.value > 0 && ptr.objectType == EObjectType.Enemy)
			{
				ref EnemyData ptr14 = ref sector.enemyPool[ptr.objectId];
				if (ptr14.isInvincible || ptr14.id == 0)
				{
					flag = true;
					this.hatred.h6 = default(HatredTarget);
				}
				ref CombatStat ptr15 = ref sector.skillSystem.combatStats.buffer[ptr14.combatStatId];
				if (((ptr14.combatStatId == 0) ? 1f : ((float)(ptr15.hp + ptr15.hpIncoming) / (float)ptr15.hpMax)) < -0.01f)
				{
					flag = true;
					this.hatred.h6 = default(HatredTarget);
				}
				if (ptr14.dfRelayId > 0 && !flag2)
				{
					flag = true;
					this.hatred.h6 = default(HatredTarget);
				}
				if (ptr14.builderId > 0 && !flag3)
				{
					flag = true;
					this.hatred.h6 = default(HatredTarget);
				}
			}
			ptr = ref this.hatred.min;
			if (ptr.value > 0 && ptr.objectType == EObjectType.Enemy)
			{
				ref EnemyData ptr16 = ref sector.enemyPool[ptr.objectId];
				if (ptr16.isInvincible || ptr16.id == 0)
				{
					flag = true;
					this.hatred.min = default(HatredTarget);
				}
				ref CombatStat ptr17 = ref sector.skillSystem.combatStats.buffer[ptr16.combatStatId];
				if (((ptr16.combatStatId == 0) ? 1f : ((float)(ptr17.hp + ptr17.hpIncoming) / (float)ptr17.hpMax)) < -0.01f)
				{
					flag = true;
					this.hatred.min = default(HatredTarget);
				}
				if (ptr16.dfRelayId > 0 && !flag2)
				{
					flag = true;
					this.hatred.min = default(HatredTarget);
				}
				if (ptr16.builderId > 0 && !flag3)
				{
					flag = true;
					this.hatred.min = default(HatredTarget);
				}
			}
			if (flag)
			{
				this.hatred.Arrange();
				if (this.hatred.max.value == 0)
				{
					this.RunBehavior_Engage_EmptyHatred(ref craft);
				}
			}
		}
	}

	// Token: 0x06000F59 RID: 3929 RVA: 0x000F710C File Offset: 0x000F530C
	public void DetermineBehavior(ref SkillTarget target, ref VectorLF3 targetPos, int orbitAstroId, bool dispatch)
	{
		if (this.behavior == EUnitBehavior.Initialize || this.behavior == EUnitBehavior.Recycled)
		{
			return;
		}
		if (!dispatch)
		{
			if (this.behavior != EUnitBehavior.Recycled)
			{
				this.behavior = ((orbitAstroId > 0) ? EUnitBehavior.Orbiting : EUnitBehavior.SeekForm);
				return;
			}
		}
		else if (target.type != ETargetType.None)
		{
			if (target.id == 0)
			{
				if (this.hatred.max.target == 0)
				{
					this.behavior = EUnitBehavior.SeekForm;
					return;
				}
				if (this.behavior != EUnitBehavior.Engage)
				{
					this.behavior = EUnitBehavior.Engage;
					this.isRetreating = false;
					return;
				}
			}
			else
			{
				int num = (target.astroId > 100 && target.astroId <= 204899 && target.astroId % 100 > 0) ? 1000 : 10000;
				Mecha mecha = GameMain.mainPlayer.mecha;
				if (target.astroId <= 204899 && target.astroId % 100 > 0)
				{
					PlanetFactory planetFactory = GameMain.data.spaceSector.skillSystem.astroFactories[target.astroId];
					if (planetFactory != null)
					{
						ref EnemyData ptr = ref planetFactory.enemyPool[target.id];
						if (ptr.id == target.id && (ptr.builderId <= 0 || mecha.groundCombatModule.attackBuilding))
						{
							this.hatred.HateTarget(target.type, target.id, num, num, EHatredOperation.Set);
						}
					}
				}
				else if (target.astroId > 1000000)
				{
					ref EnemyData ptr2 = ref GameMain.data.spaceSector.enemyPool[target.id];
					if (ptr2.id == target.id && (ptr2.builderId <= 0 || mecha.spaceCombatModule.attackBuilding))
					{
						this.hatred.HateTarget(target.type, target.id, num, num, EHatredOperation.Set);
					}
				}
				if (this.behavior != EUnitBehavior.Engage)
				{
					this.behavior = EUnitBehavior.Engage;
					this.isRetreating = false;
					return;
				}
			}
		}
		else if (orbitAstroId > 0)
		{
			if (this.hatred.max.target > 0)
			{
				this.behavior = EUnitBehavior.Engage;
				this.isRetreating = false;
			}
			if (this.behavior != EUnitBehavior.Engage)
			{
				this.behavior = EUnitBehavior.Orbiting;
				return;
			}
		}
		else
		{
			if (this.hatred.max.target == 0)
			{
				this.behavior = EUnitBehavior.SeekForm;
				return;
			}
			if (this.behavior != EUnitBehavior.Engage)
			{
				this.behavior = EUnitBehavior.Engage;
				this.isRetreating = false;
			}
		}
	}

	// Token: 0x06000F5A RID: 3930 RVA: 0x000F734C File Offset: 0x000F554C
	public void DetermineCraftAstroId(SpaceSector sector, ref CraftData craft)
	{
		if (craft.astroId == 0)
		{
			double num;
			StarData nearestStar = sector.GetNearestStar(ref craft.pos, out num);
			if (num < 1619999.9570846558)
			{
				sector.AlterCraftAstroId(nearestStar.astroId, ref craft);
				return;
			}
		}
		else
		{
			double sqrMagnitude = craft.pos.sqrMagnitude;
			double num2 = 3240000000000.0;
			if (sqrMagnitude > num2)
			{
				sector.AlterCraftAstroId(0, ref craft);
			}
		}
	}

	// Token: 0x06000F5B RID: 3931 RVA: 0x000F73AA File Offset: 0x000F55AA
	public void RunBehavior_None()
	{
	}

	// Token: 0x06000F5C RID: 3932 RVA: 0x000F73AC File Offset: 0x000F55AC
	public void RunBehavior_Initialize_Ground(PlanetFactory factory, Mecha mecha, PrefabDesc pdesc, ref CraftData craft, ref CombatUpgradeData combatUpgradeData, float fighterInitializeSpeedScale)
	{
		CraftData[] craftPool = factory.craftPool;
		ref CraftData ptr = ref craftPool[craft.owner];
		if (this.currentInitializeValue < 600000)
		{
			factory.craftAnimPool[this.craftId].time = (float)this.currentInitializeValue / 600000f;
			float fleetInitializeUnitSpeedScale = PlanetFactory.PrefabDescByModelIndex[(int)ptr.modelIndex].fleetInitializeUnitSpeedScale;
			int num = (int)((float)pdesc.craftUnitInitializeSpeed * fighterInitializeSpeedScale * fleetInitializeUnitSpeedScale + 0.5f);
			int num2 = (600000 - this.currentInitializeValue) / num;
			if (num2 <= 0)
			{
				num2 = 1;
			}
			int num3 = this.hpShortage / num2;
			this.hpShortage -= num3;
			CombatStat[] buffer = factory.skillSystem.combatStats.buffer;
			int combatStatId = craft.combatStatId;
			buffer[combatStatId].hp = buffer[combatStatId].hp + num3;
			this.currentInitializeValue += num;
		}
		else
		{
			factory.craftAnimPool[this.craftId].time = 1f;
			this.currentInitializeValue = 600000;
			this.behavior = EUnitBehavior.SeekForm;
			FleetComponent[] buffer2 = factory.combatGroundSystem.fleets.buffer;
			FleetComponent[] obj = buffer2;
			lock (obj)
			{
				FleetComponent[] array = buffer2;
				int fleetId = craftPool[craft.owner].fleetId;
				array[fleetId].currentAssembleUnitsCount = array[fleetId].currentAssembleUnitsCount - 1;
			}
			craft.vel = ptr.vel;
		}
		if (ptr.owner < 0)
		{
			factory.combatGroundSystem.fleets.buffer[ptr.fleetId].GetUnitPort_Ground(UnitComponent.gameTick, mecha, ref ptr, ref craft, ref craft.pos, ref craft.rot, factory, ref combatUpgradeData);
		}
	}

	// Token: 0x06000F5D RID: 3933 RVA: 0x000F7574 File Offset: 0x000F5774
	public void RunBehavior_Initialize_Space(SpaceSector sector, Mecha mecha, PrefabDesc pdesc, ref CraftData craft, ref CombatUpgradeData combatUpgradeData, int orbitAstroId, float fighterInitializeSpeedScale)
	{
		CraftData[] craftPool = sector.craftPool;
		int owner = craft.owner;
		ref CraftData ptr = ref craftPool[owner];
		if (this.currentInitializeValue < 600000)
		{
			sector.craftAnimPool[this.craftId].time = (float)this.currentInitializeValue / 600000f;
			float fleetInitializeUnitSpeedScale = PlanetFactory.PrefabDescByModelIndex[(int)ptr.modelIndex].fleetInitializeUnitSpeedScale;
			int num = (int)((float)pdesc.craftUnitInitializeSpeed * fighterInitializeSpeedScale * fleetInitializeUnitSpeedScale + 0.5f);
			int num2 = (600000 - this.currentInitializeValue) / num;
			if (num2 <= 0)
			{
				num2 = 1;
			}
			int num3 = this.hpShortage / num2;
			this.hpShortage -= num3;
			CombatStat[] buffer = sector.skillSystem.combatStats.buffer;
			int combatStatId = craft.combatStatId;
			buffer[combatStatId].hp = buffer[combatStatId].hp + num3;
			this.currentInitializeValue += num;
		}
		else
		{
			sector.craftAnimPool[this.craftId].time = 0.999f;
			this.currentInitializeValue = 600000;
			this.behavior = EUnitBehavior.SeekForm;
			FleetComponent[] buffer2 = sector.combatSpaceSystem.fleets.buffer;
			FleetComponent[] obj = buffer2;
			lock (obj)
			{
				FleetComponent[] array = buffer2;
				int fleetId = craftPool[owner].fleetId;
				array[fleetId].currentAssembleUnitsCount = array[fleetId].currentAssembleUnitsCount - 1;
			}
			if (orbitAstroId == 0)
			{
				craft.vel = ptr.vel;
			}
		}
		if (ptr.owner < 0)
		{
			ref FleetComponent ptr2 = ref sector.combatSpaceSystem.fleets.buffer[ptr.fleetId];
			if (orbitAstroId > 0)
			{
				this.OrbitingFlightLogic(sector, mecha, pdesc, ref craft, ref combatUpgradeData, orbitAstroId);
				return;
			}
			ptr2.GetUnitPort_Space(mecha, ref ptr, ref craft, ref craft.pos, ref craft.rot, ref craft.vel, ref combatUpgradeData);
		}
	}

	// Token: 0x06000F5E RID: 3934 RVA: 0x000F775C File Offset: 0x000F595C
	public void RunBehavior_KeepForm(ref CraftData craft)
	{
	}

	// Token: 0x06000F5F RID: 3935 RVA: 0x000F7760 File Offset: 0x000F5960
	public void BroadcastHatred(PlanetFactory factory, UnitComponent[] unitPool, ref CraftData craft, int max_hatred)
	{
		int num = (this.hatred.max.value - 40) / 4;
		Vector3 vector = craft.pos;
		float x = vector.x;
		float y = vector.y;
		float z = vector.z;
		HashSystem hashSystemDynamic = factory.hashSystemDynamic;
		int[] hashPool = hashSystemDynamic.hashPool;
		int[] bucketOffsets = hashSystemDynamic.bucketOffsets;
		hashSystemDynamic.GetBucketIdxesInArea(vector, 16f);
		int activeBucketsCount = hashSystemDynamic.activeBucketsCount;
		CraftData[] craftPool = factory.craftPool;
		for (int i = 0; i < activeBucketsCount; i++)
		{
			int num2 = hashSystemDynamic.activeBuckets[i];
			int num3 = bucketOffsets[num2];
			int num4 = hashSystemDynamic.bucketCursors[num2];
			for (int j = 0; j < num4; j++)
			{
				int num5 = num3 + j;
				int num6 = hashPool[num5];
				if (num6 != 0 && num6 >> 28 == 6)
				{
					int num7 = num6 & 268435455;
					if (num7 != 0)
					{
						ref CraftData ptr = ref craftPool[num7];
						if (ptr.id == num7 && ptr.dynamic && ptr.unitId != 0)
						{
							ref UnitComponent ptr2 = ref unitPool[ptr.unitId];
							if (ptr2.id == ptr.unitId)
							{
								float num8 = (float)ptr.pos.x - x;
								float num9 = (float)ptr.pos.y - y;
								float num10 = (float)ptr.pos.z - z;
								float num11 = num8 * num8 + num9 * num9 + num10 * num10;
								int num12 = num - (int)(num11 / 10f);
								if (num11 < 256f && num12 > 10)
								{
									ptr2.hatred.HateTarget(this.hatred.max.targetType, this.hatred.max.objectId, num12, max_hatred, EHatredOperation.Add);
								}
							}
						}
					}
				}
			}
		}
		hashSystemDynamic.ClearActiveBuckets();
	}

	// Token: 0x06000F60 RID: 3936 RVA: 0x000F7944 File Offset: 0x000F5B44
	public bool UpdateMechaEnergy(Mecha mecha, PrefabDesc pdesc, bool isSpace)
	{
		bool result;
		lock (mecha)
		{
			if (mecha.coreEnergy >= (double)pdesc.craftUnitEnergyPerTick)
			{
				mecha.coreEnergy -= (double)pdesc.craftUnitEnergyPerTick;
				mecha.MarkEnergyChange(isSpace ? 13 : 12, (double)(-(double)pdesc.craftUnitEnergyPerTick));
				result = true;
			}
			else
			{
				result = false;
			}
		}
		return result;
	}

	// Token: 0x06000F61 RID: 3937 RVA: 0x000F79BC File Offset: 0x000F5BBC
	public bool UpdateBattleBaseEnergy(BattleBaseComponent battleBase, PrefabDesc pdesc)
	{
		if (battleBase.energy >= (long)pdesc.craftUnitEnergyPerTick)
		{
			battleBase.energy -= (long)pdesc.craftUnitEnergyPerTick;
			return true;
		}
		return false;
	}

	// Token: 0x06000F62 RID: 3938 RVA: 0x000F79E4 File Offset: 0x000F5BE4
	public void UpdateFireCondition(bool isSpace, PrefabDesc pdesc, ref CombatUpgradeData combatUpgradeData)
	{
		if (this.behavior != EUnitBehavior.Engage)
		{
			this.adjustEngageRange = false;
			this.isShooting0 = false;
			this.isShooting1 = false;
			this.muzzleIndex0 = 0;
			this.muzzleIndex1 = 0;
		}
		float num = isSpace ? combatUpgradeData.combatShipROFRatio : combatUpgradeData.combatDroneROFRatio;
		this.fire0 -= (int)((float)pdesc.craftUnitROF0 * num + 0.5f);
		this.fire1 -= (int)((float)pdesc.craftUnitROF1 * num + 0.5f);
		if (this.fire0 < 0 && !this.isShooting0)
		{
			this.fire0 = 0;
		}
		if (this.fire1 < 0 && !this.isShooting1)
		{
			this.fire1 = 0;
		}
	}

	// Token: 0x06000F63 RID: 3939 RVA: 0x000F7A9C File Offset: 0x000F5C9C
	public bool GetAstroAvoidanceTargetPos(SpaceSector sector, float speed, int targetAstroId, ref VectorLF3 targetUPos, ref CraftData craft)
	{
		if (craft.astroId == 0)
		{
			return false;
		}
		int astroId = craft.astroId;
		int planetCount = sector.galaxy.StarById(astroId / 100).planetCount;
		float num = 5000f + speed;
		AstroData[] galaxyAstros = sector.galaxyAstros;
		VectorLF3 vectorLF;
		sector.TransformFromAstro_ref(astroId, out vectorLF, ref craft.pos);
		Vector3 vector = default(Vector3);
		Vector3 b;
		galaxyAstros[targetAstroId].VelocityU(ref vector, out b);
		VectorLF3 vectorLF2 = targetUPos - vectorLF + (craft.vel - b);
		double magnitude = vectorLF2.magnitude;
		if (magnitude < 1E-34)
		{
			return false;
		}
		vectorLF2 /= magnitude;
		int num2 = -1;
		double rhs = 0.0;
		double num3 = double.MaxValue;
		VectorLF3 vectorLF3 = default(VectorLF3);
		for (int i = astroId; i <= astroId + planetCount; i++)
		{
			float num4 = galaxyAstros[i].uRadius;
			if (num4 >= 1f)
			{
				float num5 = num4 + num;
				num4 = ((targetAstroId == i) ? (num4 + 20f) : (num4 + 50f));
				if (i == astroId)
				{
					num4 += 100f;
				}
				VectorLF3 vectorLF4 = galaxyAstros[i].uPos - vectorLF;
				double num6 = vectorLF4.x * vectorLF2.x + vectorLF4.y * vectorLF2.y + vectorLF4.z * vectorLF2.z;
				if (num6 < magnitude && num6 >= 0.0)
				{
					double magnitude2 = vectorLF4.magnitude;
					if (magnitude2 <= (double)num5)
					{
						double num7 = vectorLF4.x * vectorLF4.x + vectorLF4.y * vectorLF4.y + vectorLF4.z * vectorLF4.z;
						if (num7 - num6 * num6 < (double)(num4 * num4) && magnitude2 < num3)
						{
							num2 = i;
							num3 = magnitude2;
							double num8 = Math.Sqrt(num7);
							vectorLF3 = vectorLF4 / num8 * (num8 - (double)num4);
							rhs = vectorLF3.x * vectorLF2.x + vectorLF3.y * vectorLF2.y + vectorLF3.z * vectorLF2.z;
						}
					}
				}
			}
		}
		if (num2 > 0)
		{
			VectorLF3 lhs = vectorLF2 * rhs + vectorLF;
			float uRadius = galaxyAstros[num2].uRadius;
			float num9 = (targetAstroId == num2) ? (uRadius + 70f) : (uRadius + 100f);
			if (num2 == astroId)
			{
				num9 += 100f;
			}
			float num10 = galaxyAstros[num2].uRadius + num9;
			VectorLF3 vectorLF5 = vectorLF + vectorLF3;
			VectorLF3 lhs2 = vectorLF5 + (lhs - vectorLF5).normalized * (double)num10;
			targetUPos = vectorLF + (lhs2 - vectorLF).normalized * magnitude;
			return true;
		}
		return false;
	}

	// Token: 0x06000F64 RID: 3940 RVA: 0x000F7DAC File Offset: 0x000F5FAC
	public bool IsPlanetaryOcclusion(SpaceSector sector, int starAstroId, ref VectorLF3 targetUPos, ref CraftData craft)
	{
		if (starAstroId == 0)
		{
			return false;
		}
		VectorLF3 rhs;
		sector.TransformFromAstro_ref(starAstroId, out rhs, ref craft.pos);
		VectorLF3 vectorLF = targetUPos - rhs;
		double magnitude = vectorLF.magnitude;
		vectorLF /= magnitude;
		int planetCount = sector.galaxy.StarById(starAstroId / 100).planetCount;
		for (int i = starAstroId; i <= starAstroId + planetCount; i++)
		{
			float uRadius = sector.galaxyAstros[i].uRadius;
			if (uRadius < 1f)
			{
				break;
			}
			VectorLF3 vectorLF2 = sector.galaxyAstros[i].uPos - rhs;
			double num = vectorLF2.x * vectorLF.x + vectorLF2.y * vectorLF.y + vectorLF2.z * vectorLF.z;
			if (num < magnitude && num >= 0.0 && vectorLF2.x * vectorLF2.x + vectorLF2.y * vectorLF2.y + vectorLF2.z * vectorLF2.z - num * num < (double)(uRadius * uRadius))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000F65 RID: 3941 RVA: 0x000F7ED4 File Offset: 0x000F60D4
	public void RunBehavior_Engage_Ground(PlanetFactory factory, Mecha mecha, PrefabDesc pdesc, ref CraftData craft, ref CombatSettings combatSettings, ref CombatUpgradeData combatUpgradeData)
	{
		if (this.hatred.max.target == 0)
		{
			this.RunBehavior_Engage_EmptyHatred(ref craft);
			return;
		}
		if (!mecha.groundCombatModule.attackBuilding && this.ClearLocalHatredBuilding(factory))
		{
			return;
		}
		switch (this.protoId)
		{
		case 5101:
			this.RunBehavior_Engage_DefenseShield_Ground(factory, mecha, pdesc, ref craft, ref combatSettings, ref combatUpgradeData);
			return;
		case 5102:
			this.RunBehavior_Engage_AttackPlasma_Ground(factory, mecha, pdesc, ref craft, ref combatSettings, ref combatUpgradeData);
			return;
		case 5103:
			this.RunBehavior_Engage_AttackLaser_Ground(factory, mecha, pdesc, ref craft, ref combatSettings, ref combatUpgradeData);
			return;
		default:
			return;
		}
	}

	// Token: 0x06000F66 RID: 3942 RVA: 0x000F7F64 File Offset: 0x000F6164
	public void RunBehavior_Engage_Space(SpaceSector sector, Mecha mecha, PrefabDesc pdesc, ref CraftData craft, ref CombatSettings combatSettings, ref CombatUpgradeData combatUpgradeData)
	{
		if (this.hatred.max.target == 0)
		{
			this.RunBehavior_Engage_EmptyHatred(ref craft);
			return;
		}
		if (!mecha.spaceCombatModule.attackBuilding && this.ClearSpaceHatredBuilding(sector))
		{
			return;
		}
		short num = this.protoId;
		if (num == 5111)
		{
			this.RunBehavior_Engage_SAttackPlasma_Small(sector, mecha, pdesc, ref craft, ref combatSettings, ref combatUpgradeData);
			return;
		}
		if (num != 5112)
		{
			return;
		}
		this.RunBehavior_Engage_SAttackLaser_Large(sector, mecha, pdesc, ref craft, ref combatSettings, ref combatUpgradeData);
	}

	// Token: 0x06000F67 RID: 3943 RVA: 0x000F7FDC File Offset: 0x000F61DC
	public void RunBehavior_Engage_EmptyHatred(ref CraftData craft)
	{
		this.hatred.ClearMax();
		ref Vector3 ptr = ref craft.vel;
		craft.pos.x = craft.pos.x + (double)ptr.x * 0.016666666666666666;
		craft.pos.y = craft.pos.y + (double)ptr.y * 0.016666666666666666;
		craft.pos.z = craft.pos.z + (double)ptr.z * 0.016666666666666666;
		this.anim = 0f;
		this.steering = 0f;
		this.speed = ptr.magnitude;
		this.behavior = EUnitBehavior.SeekForm;
	}

	// Token: 0x06000F68 RID: 3944 RVA: 0x000F807C File Offset: 0x000F627C
	public bool ClearLocalHatredBuilding(PlanetFactory factory)
	{
		ref HatredTarget ptr = ref this.hatred.max;
		ref EnemyData ptr2 = ref factory.enemyPool[ptr.objectId];
		if (ptr2.id == ptr.objectId && ptr2.id > 0 && ptr2.builderId > 0)
		{
			this.hatred.ClearMax();
			this.hatred.Arrange();
			return true;
		}
		return false;
	}

	// Token: 0x06000F69 RID: 3945 RVA: 0x000F80E0 File Offset: 0x000F62E0
	public bool ClearSpaceHatredBuilding(SpaceSector sector)
	{
		ref HatredTarget ptr = ref this.hatred.max;
		ref EnemyData ptr2 = ref sector.enemyPool[ptr.objectId];
		if (ptr2.id == ptr.objectId && ptr2.id > 0 && ptr2.builderId > 0)
		{
			this.hatred.ClearMax();
			this.hatred.Arrange();
			return true;
		}
		return false;
	}

	// Token: 0x06000F6A RID: 3946 RVA: 0x000F8144 File Offset: 0x000F6344
	public void RunBehavior_Engage_AttackLaser_Ground(PlanetFactory factory, Mecha mecha, PrefabDesc pdesc, ref CraftData craft, ref CombatSettings combatSettings, ref CombatUpgradeData combatUpgradeData)
	{
		Vector3 vector;
		if (this.GetTargetPosition_Ground(factory, this.hatred.max.target, out vector))
		{
			float num = 25f;
			float num2 = 35f;
			EnemyData[] enemyPool = factory.enemyPool;
			CraftData[] craftPool = factory.craftPool;
			int fleetId = craftPool[craft.owner].fleetId;
			ref CraftData ptr = ref craftPool[craft.owner];
			if (fleetId > 0)
			{
				PrefabDesc prefabDesc = PlanetFactory.PrefabDescByModelIndex[(int)ptr.modelIndex];
				num = prefabDesc.fleetSensorRange;
				num2 = prefabDesc.fleetMaxActiveArea;
				if (this.hatred.max.value > 800)
				{
					float num3 = (float)(enemyPool[this.hatred.max.objectId].pos - ptr.pos).magnitude;
					num += num3;
					num2 += num3;
				}
			}
			Vector3 b;
			Vector3 rhs;
			craft.rot.ForwardRight(out b, out rhs);
			float craftUnitMaxMovementSpeed = pdesc.craftUnitMaxMovementSpeed;
			float num4 = pdesc.craftUnitMaxMovementAcceleration;
			float craftUnitAttackRange = pdesc.craftUnitAttackRange0;
			VectorLF3 vectorLF = craft.pos - ptr.pos;
			float num5 = (float)vectorLF.magnitude;
			bool flag = num5 > num2;
			if (!flag && !this.isRetreating)
			{
				short port = craft.port;
				int num6 = (int)(port / 12);
				int num7 = (int)(port % 12);
				float num8 = 225f;
				float num9 = 212f;
				ref VectorLF3 ptr2 = ref craft.pos;
				ref Quaternion result = ref craft.rot;
				ref Vector3 ptr3 = ref craft.vel;
				float num10 = vector.x - (float)ptr2.x;
				float num11 = vector.y - (float)ptr2.y;
				float num12 = vector.z - (float)ptr2.z;
				float num13 = (float)(ptr2.x * ptr2.x + ptr2.y * ptr2.y + ptr2.z * ptr2.z);
				float num14 = vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
				float num15 = Mathf.Sqrt(num13);
				float num16 = (num14 - num13) / (num15 * 2f);
				bool flag2 = num16 > craftUnitAttackRange * 0.9f;
				if (num15 + num16 > num8)
				{
					num16 = num8 - num15;
				}
				else if (num15 + num16 < num9)
				{
					num16 = num9 - num15;
				}
				num16 += (float)(num6 % 3) + (float)(num7 % 3) * 0.7f - 1.7f;
				float num17 = (float)ptr2.x / num15;
				float num18 = (float)ptr2.y / num15;
				float num19 = (float)ptr2.z / num15;
				float num20 = num10 * num17 + num11 * num18 + num12 * num19;
				num20 -= num16 / 1.2f;
				num10 -= num17 * num20;
				num11 -= num18 * num20;
				num12 -= num19 * num20;
				float num21 = Mathf.Sqrt(num10 * num10 + num11 * num11 + num12 * num12);
				float num22 = num21;
				if (num22 < 0f)
				{
					num22 = 0f;
				}
				float num23 = (float)(num7 % 2 * 2 - 1) * ((float)(num7 / 2) * 0.5f + 1f) + (float)num6 * 0.05f;
				float num24 = num18 * num12 - num11 * num19;
				float num25 = num19 * num10 - num12 * num17;
				float num26 = num17 * num11 - num10 * num18;
				float num27 = craftUnitAttackRange;
				float num28 = num22 / num27;
				float num29 = (num28 > 1f) ? (1f / num28) : (2f - num28);
				float num30 = num29 * num29;
				float num31 = (num10 * ptr3.x + num11 * ptr3.y + num12 * ptr3.z) / num21;
				if (num31 < 0f)
				{
					num31 = -num31;
				}
				num31 += 0.2f;
				num31 /= craftUnitMaxMovementSpeed;
				float num32 = num30 * num31 * num27 * num23 / Mathf.Sqrt(num24 * num24 + num25 * num25 + num26 * num26);
				num24 *= num32;
				num25 *= num32;
				num26 *= num32;
				num10 += num24;
				num11 += num25;
				num12 += num26;
				float num33 = Mathf.Sqrt(num10 * num10 + num11 * num11 + num12 * num12);
				float num34 = craftUnitMaxMovementSpeed / num33;
				num10 *= num34;
				num11 *= num34;
				num12 *= num34;
				float num35 = (float)(360L - (UnitComponent.gameTick + 35L * (long)num7) % 420L);
				if (num35 < 0f)
				{
					num35 = -num35;
				}
				num35 = 60f - num35;
				if (num35 < 0f)
				{
					num35 = 0f;
				}
				num35 /= 5f;
				float num36 = num35;
				float num37 = (num21 - 5f) / (num36 * 1.2f + 5f);
				num37 = ((num37 > 0.28f) ? ((num37 > 1f) ? 1f : num37) : 0.28f);
				num37 *= num37;
				float num38 = num21 / craftUnitMaxMovementSpeed;
				float num39 = (num38 > 2f) ? ((num38 > 16f) ? 16f : num38) : 2f;
				num39 /= 2f;
				num4 /= num39;
				num4 *= num37;
				float num40 = num10 - ptr3.x;
				float num41 = num11 - ptr3.y;
				float num42 = num12 - ptr3.z;
				float num43 = Mathf.Sqrt(num40 * num40 + num41 * num41 + num42 * num42) / (num4 * 0.016666668f);
				if (num43 > 1f)
				{
					num40 /= num43;
					num41 /= num43;
					num42 /= num43;
				}
				ptr3.x += num40;
				ptr3.y += num41;
				ptr3.z += num42;
				VectorLF3 vectorLF2 = -vectorLF.normalized;
				float num44 = num5 / num2;
				if (num5 > num)
				{
					num44 *= 10f;
				}
				ptr3.x += (float)vectorLF2.x * num44 * 0.15f;
				ptr3.y += (float)vectorLF2.y * num44 * 0.15f;
				ptr3.z += (float)vectorLF2.z * num44 * 0.15f;
				this.speed = Mathf.Sqrt(ptr3.x * ptr3.x + ptr3.y * ptr3.y + ptr3.z * ptr3.z);
				if (this.speed < craftUnitMaxMovementSpeed + 4f)
				{
					float num45 = num35 * (1.45f + (float)num7 * 0.05f) * 0.016666668f / this.speed;
					ptr3.x += ptr3.x * num45;
					ptr3.y += ptr3.y * num45;
					ptr3.z += ptr3.z * num45;
				}
				ptr2.x += (double)ptr3.x * 0.016666666666666666;
				ptr2.y += (double)ptr3.y * 0.016666666666666666;
				ptr2.z += (double)ptr3.z * 0.016666666666666666;
				if (this.speed > 0.1f)
				{
					Maths.LookRotation(ptr3.x / this.speed, ptr3.y / this.speed, ptr3.z / this.speed, num17, num18, num19, out result);
				}
				this.anim = 0f;
				this.isShooting0 = (!flag2 && num22 < craftUnitAttackRange);
				if (this.isShooting0)
				{
					this.anim = 1f - (float)this.fire0 / (float)pdesc.craftUnitRoundInterval0 * 2f;
					if (this.anim != 0f && this.fire0 <= pdesc.craftUnitROF0 * 4)
					{
						SkillTargetLocal skillTargetLocal = this.hatred.max.skillTargetLocal;
						Vector3 vector2 = vector;
						if (!enemyPool[skillTargetLocal.id].dynamic)
						{
							vector2 += vector2.normalized * (SkillSystem.RoughHeightByModelIndex[(int)enemyPool[skillTargetLocal.id].modelIndex] * 0.4f);
						}
						bool flag3 = true;
						int craftUnitFireEnergy = pdesc.craftUnitFireEnergy0;
						if (ptr.owner < 0)
						{
							Mecha obj = mecha;
							lock (obj)
							{
								if (mecha.coreEnergy < (double)craftUnitFireEnergy)
								{
									flag3 = false;
								}
								goto IL_895;
							}
						}
						if (ptr.owner > 0)
						{
							ref EntityData ptr4 = ref factory.entityPool[ptr.owner];
							if (factory.defenseSystem.battleBases.buffer[ptr4.battleBaseId].energy < (long)craftUnitFireEnergy)
							{
								flag3 = false;
							}
						}
						IL_895:
						if (this.fire0 <= 0 && flag3)
						{
							ref LocalLaserOneShot ptr5 = ref factory.skillSystem.fighterLasers.Add();
							ptr5.astroId = factory.planetId;
							ptr5.hitIndex = 16;
							ptr5.beginPos = craft.pos + craft.vel * 0.03333f;
							ptr5.target = skillTargetLocal;
							ptr5.endPos = vector2;
							ptr5.damage = (int)((float)pdesc.craftUnitAttackDamage0 * combatUpgradeData.combatDroneDamageRatio * combatUpgradeData.energyDamageScale + 0.5f);
							ptr5.mask = ETargetTypeMask.Enemy;
							ptr5.caster.type = ETargetType.Craft;
							ptr5.caster.id = this.craftId;
							ptr5.life = 15;
							this.fire0 += pdesc.craftUnitRoundInterval0;
							this.hatred.HateTarget(skillTargetLocal.type, skillTargetLocal.id, 40, 300, EHatredOperation.Add);
							if (ptr.owner < 0)
							{
								Mecha obj = mecha;
								lock (obj)
								{
									mecha.coreEnergy -= (double)craftUnitFireEnergy;
									mecha.MarkEnergyChange(12, (double)(-(double)craftUnitFireEnergy));
									goto IL_A11;
								}
							}
							if (ptr.owner > 0)
							{
								ref EntityData ptr6 = ref factory.entityPool[ptr.owner];
								factory.defenseSystem.battleBases.buffer[ptr6.battleBaseId].energy -= (long)craftUnitFireEnergy;
							}
							IL_A11:
							ref EnemyData ptr7 = ref enemyPool[skillTargetLocal.id];
							DFGBaseComponent dfgbaseComponent = factory.enemySystem.bases[(int)ptr7.owner];
							if (dfgbaseComponent != null && dfgbaseComponent.id == (int)ptr7.owner)
							{
								factory.skillSystem.AddGroundEnemyHatred(dfgbaseComponent, ref ptr7, ETargetType.Craft, this.craftId);
							}
						}
					}
				}
			}
			else
			{
				if (flag && !this.isRetreating)
				{
					this.hatred.ClearMax();
				}
				this.isRetreating = true;
				VectorLF3 pos = craftPool[craft.owner].pos;
				VectorLF3 normalized = (craft.pos - pos).normalized;
				VectorLF3 vectorLF3 = normalized * (double)num + pos;
				Quaternion quaternion = Quaternion.LookRotation(-normalized, craft.pos);
				Vector3 vector3 = default(Vector3);
				float craftUnitMaxMovementSpeed2 = pdesc.craftUnitMaxMovementSpeed;
				float num46 = pdesc.craftUnitMaxMovementAcceleration;
				ref FleetComponent ptr8 = ref factory.combatGroundSystem.fleets.buffer[fleetId];
				ref VectorLF3 ptr9 = ref craft.pos;
				ref Quaternion ptr10 = ref craft.rot;
				ref Vector3 ptr11 = ref craft.vel;
				Vector3 vector4 = new Vector3((float)(vectorLF3.x - ptr9.x), (float)(vectorLF3.y - ptr9.y), (float)(vectorLF3.z - ptr9.z));
				float num47 = Mathf.Sqrt(vector4.x * vector4.x + vector4.y * vector4.y + vector4.z * vector4.z);
				float magnitude = ptr11.magnitude;
				float num48 = (magnitude > 2f) ? magnitude : 2f;
				float num49 = num47 / num48;
				num49 -= 0.016666668f;
				if (num49 < 0f)
				{
					num49 = 0f;
				}
				float num50 = ((num49 > 1f) ? 1f : num49) * 0.3f;
				Vector3 vector5 = new Vector3((float)vectorLF3.x - vector3.x * num50, (float)vectorLF3.y - vector3.y * num50, (float)vectorLF3.z - vector3.z * num50);
				Vector3 vector6 = new Vector3(vector5.x - (float)ptr9.x, vector5.y - (float)ptr9.y, vector5.z - (float)ptr9.z);
				float num51 = Mathf.Sqrt(vector6.x * vector6.x + vector6.y * vector6.y + vector6.z * vector6.z);
				if (num51 > 0f)
				{
					vector6.x /= num51;
					vector6.y /= num51;
					vector6.z /= num51;
				}
				float num52 = (num49 > 2f) ? ((num49 > 6f) ? 6f : num49) : 2f;
				num52 /= 2f;
				num46 /= num52;
				float num53 = 0f;
				float num54 = num49 / 0.5f - 0.02f;
				if (num54 <= 0f)
				{
					ptr11.x = vector3.x;
					ptr11.y = vector3.y;
					ptr11.z = vector3.z;
				}
				else
				{
					Vector3 vector7 = new Vector3(vector6.x * craftUnitMaxMovementSpeed2, vector6.y * craftUnitMaxMovementSpeed2, vector6.z * craftUnitMaxMovementSpeed2);
					if (num54 < 1f)
					{
						float num55 = 1f - num54;
						vector7.x = vector7.x * num54 + vector3.x * num55;
						vector7.y = vector7.y * num54 + vector3.y * num55;
						vector7.z = vector7.z * num54 + vector3.z * num55;
					}
					Vector3 vector8 = new Vector3(vector7.x - ptr11.x, vector7.y - ptr11.y, vector7.z - ptr11.z);
					num53 = Mathf.Sqrt(vector8.x * vector8.x + vector8.y * vector8.y + vector8.z * vector8.z);
					float num56 = num53 / (num46 * 0.016666668f);
					if (num56 > 1f)
					{
						vector8.x /= num56;
						vector8.y /= num56;
						vector8.z /= num56;
					}
					ptr11.x += vector8.x;
					ptr11.y += vector8.y;
					ptr11.z += vector8.z;
					float num57 = (num51 - 2f) / 15f;
					if (num57 > 0f)
					{
						float num58 = (float)(ptr9.x * ptr9.x + ptr9.y * ptr9.y + ptr9.z * ptr9.z);
						float num59 = vector5.x * vector5.x + vector5.y * vector5.y + vector5.z * vector5.z - num58;
						float num60 = Mathf.Sqrt(num58);
						Vector3 vector9 = new Vector3((float)ptr9.x / num60, (float)ptr9.y / num60, (float)ptr9.z / num60);
						float num61 = ptr11.x * vector9.x + ptr11.y * vector9.y + ptr11.z * vector9.z;
						if (num61 * num59 < 0f)
						{
							if (num57 >= 1f)
							{
								ptr11.x -= vector9.x * num61;
								ptr11.y -= vector9.y * num61;
								ptr11.z -= vector9.z * num61;
							}
							else
							{
								num57 *= num57;
								num61 *= num57;
								ptr11.x -= vector9.x * num61;
								ptr11.y -= vector9.y * num61;
								ptr11.z -= vector9.z * num61;
							}
						}
					}
				}
				ptr9.x += (double)ptr11.x * 0.016666666666666666;
				ptr9.y += (double)ptr11.y * 0.016666666666666666;
				ptr9.z += (double)ptr11.z * 0.016666666666666666;
				bool flag5 = false;
				bool flag6 = false;
				float num62 = num49 + num53 / num46;
				float num63 = num62 / 0.15f - 0.04f;
				if (num63 < 1f)
				{
					if (num63 <= 0f)
					{
						ptr9.x = vectorLF3.x;
						ptr9.y = vectorLF3.y;
						ptr9.z = vectorLF3.z;
						if (ptr8.owner > 0)
						{
							this.behavior = EUnitBehavior.KeepForm;
						}
						flag5 = true;
					}
					else
					{
						float num64 = 1f - num63;
						ptr9.x = ptr9.x * (double)num63 + vectorLF3.x * (double)num64;
						ptr9.y = ptr9.y * (double)num63 + vectorLF3.y * (double)num64;
						ptr9.z = ptr9.z * (double)num63 + vectorLF3.z * (double)num64;
					}
				}
				float num65 = num62 / 0.65f - 0.04f;
				if (num65 <= 0f)
				{
					ptr10 = quaternion;
					flag6 = true;
				}
				else
				{
					if (ptr11.x * ptr11.x + ptr11.y * ptr11.y + ptr11.z * ptr11.z > 0.01f)
					{
						Quaternion b2 = Quaternion.LookRotation(ptr11, ptr9);
						ptr10 = Quaternion.Slerp(ptr10, b2, 0.1f);
					}
					if (num65 < 1f)
					{
						ptr10 = Quaternion.Slerp(quaternion, ptr10, num65 * num65);
					}
				}
				if (flag5 && flag6)
				{
					this.isRetreating = false;
				}
				this.anim = 0f;
				this.steering = 0f;
				this.speed = magnitude;
				Vector3 vector10 = craft.pos;
				float x = vector10.x;
				float y = vector10.y;
				float z = vector10.z;
				float num66 = craftUnitAttackRange * craftUnitAttackRange;
				ref HatredTarget ptr12 = ref this.hatred.max;
				this.isShooting0 = false;
				for (int i = 0; i < 8; i++)
				{
					if (i == 0)
					{
						ptr12 = ref this.hatred.max;
					}
					else if (i == 1)
					{
						ptr12 = ref this.hatred.h1;
					}
					else if (i == 2)
					{
						ptr12 = ref this.hatred.h2;
					}
					else if (i == 3)
					{
						ptr12 = ref this.hatred.h3;
					}
					else if (i == 4)
					{
						ptr12 = ref this.hatred.h4;
					}
					else if (i == 5)
					{
						ptr12 = ref this.hatred.h5;
					}
					else if (i == 6)
					{
						ptr12 = ref this.hatred.h6;
					}
					else if (i == 7)
					{
						ptr12 = ref this.hatred.min;
					}
					if (ptr12.targetType == ETargetType.Enemy)
					{
						int objectId = ptr12.objectId;
						ref EnemyData ptr13 = ref enemyPool[objectId];
						if (ptr13.id == objectId && !ptr13.isInvincible)
						{
							float num67 = (float)ptr13.pos.x;
							float num68 = (float)ptr13.pos.y;
							float num69 = (float)ptr13.pos.z;
							float num70 = num67 - x;
							float num71 = num68 - y;
							float num72 = num69 - z;
							if (num70 * num70 + num71 * num71 + num72 * num72 <= num66)
							{
								this.isShooting0 = true;
								break;
							}
						}
					}
				}
				if (this.isShooting0)
				{
					this.anim = 1f - (float)this.fire0 / (float)pdesc.craftUnitRoundInterval0 * 2f;
					if (this.anim != 0f && this.fire0 <= pdesc.craftUnitROF0 * 4)
					{
						SkillTargetLocal skillTargetLocal2 = ptr12.skillTargetLocal;
						vectorLF3 = enemyPool[skillTargetLocal2.id].pos;
						Vector3 vector11 = vectorLF3;
						if (!enemyPool[skillTargetLocal2.id].dynamic)
						{
							vector11 += vector11.normalized * (SkillSystem.RoughHeightByModelIndex[(int)enemyPool[skillTargetLocal2.id].modelIndex] * 0.4f);
						}
						bool flag7 = true;
						int craftUnitFireEnergy2 = pdesc.craftUnitFireEnergy0;
						if (ptr.owner < 0)
						{
							Mecha obj = mecha;
							lock (obj)
							{
								if (mecha.coreEnergy < (double)craftUnitFireEnergy2)
								{
									flag7 = false;
								}
								goto IL_1518;
							}
						}
						if (ptr.owner > 0)
						{
							ref EntityData ptr14 = ref factory.entityPool[ptr.owner];
							if (factory.defenseSystem.battleBases.buffer[ptr14.battleBaseId].energy < (long)craftUnitFireEnergy2)
							{
								flag7 = false;
							}
						}
						IL_1518:
						if (this.fire0 <= 0 && flag7)
						{
							ref LocalLaserOneShot ptr15 = ref factory.skillSystem.fighterLasers.Add();
							ptr15.astroId = factory.planetId;
							ptr15.hitIndex = 16;
							ptr15.beginPos = craft.pos + craft.vel * 0.03333f;
							ptr15.target = skillTargetLocal2;
							ptr15.endPos = vector11;
							ptr15.damage = (int)((float)pdesc.craftUnitAttackDamage0 * combatUpgradeData.combatDroneDamageRatio * combatUpgradeData.energyDamageScale + 0.5f);
							ptr15.mask = ETargetTypeMask.Enemy;
							ptr15.caster.type = ETargetType.Craft;
							ptr15.caster.id = this.craftId;
							ptr15.life = 15;
							this.fire0 += pdesc.craftUnitRoundInterval0;
							if (ptr.owner < 0)
							{
								Mecha obj = mecha;
								lock (obj)
								{
									mecha.coreEnergy -= (double)craftUnitFireEnergy2;
									mecha.MarkEnergyChange(12, (double)(-(double)craftUnitFireEnergy2));
									goto IL_1673;
								}
							}
							if (ptr.owner > 0)
							{
								ref EntityData ptr16 = ref factory.entityPool[ptr.owner];
								factory.defenseSystem.battleBases.buffer[ptr16.battleBaseId].energy -= (long)craftUnitFireEnergy2;
							}
							IL_1673:
							ref EnemyData ptr17 = ref enemyPool[skillTargetLocal2.id];
							DFGBaseComponent dfgbaseComponent2 = factory.enemySystem.bases[(int)ptr17.owner];
							if (dfgbaseComponent2 != null && dfgbaseComponent2.id == (int)ptr17.owner)
							{
								factory.skillSystem.AddGroundEnemyHatred(dfgbaseComponent2, ref ptr17, ETargetType.Craft, this.craftId);
							}
						}
					}
				}
			}
			this.steering = Vector3.Dot(craft.rot.Forward() - b, rhs);
			return;
		}
		this.RunBehavior_Engage_EmptyHatred(ref craft);
	}

	// Token: 0x06000F6B RID: 3947 RVA: 0x000F9874 File Offset: 0x000F7A74
	public void RunBehavior_Engage_AttackPlasma_Ground(PlanetFactory factory, Mecha mecha, PrefabDesc pdesc, ref CraftData craft, ref CombatSettings combatSettings, ref CombatUpgradeData combatUpgradeData)
	{
		Vector3 vector;
		if (this.GetTargetPosition_Ground(factory, this.hatred.max.target, out vector))
		{
			float num = 25f;
			float num2 = 35f;
			EnemyData[] enemyPool = factory.enemyPool;
			CraftData[] craftPool = factory.craftPool;
			int fleetId = craftPool[craft.owner].fleetId;
			ref CraftData ptr = ref craftPool[craft.owner];
			if (fleetId > 0)
			{
				PrefabDesc prefabDesc = PlanetFactory.PrefabDescByModelIndex[(int)ptr.modelIndex];
				num = prefabDesc.fleetSensorRange;
				num2 = prefabDesc.fleetMaxActiveArea;
				if (this.hatred.max.value > 800)
				{
					float num3 = (float)(enemyPool[this.hatred.max.objectId].pos - ptr.pos).magnitude;
					num += num3;
					num2 += num3;
				}
			}
			Vector3 b;
			Vector3 rhs;
			craft.rot.ForwardRight(out b, out rhs);
			float craftUnitMaxMovementSpeed = pdesc.craftUnitMaxMovementSpeed;
			float num4 = pdesc.craftUnitMaxMovementAcceleration;
			float craftUnitAttackRange = pdesc.craftUnitAttackRange0;
			VectorLF3 vectorLF = craft.pos - ptr.pos;
			float num5 = (float)vectorLF.magnitude;
			bool flag = num5 > num2;
			if (!flag && !this.isRetreating)
			{
				short port = craft.port;
				int num6 = (int)(port / 12);
				int num7 = (int)(port % 12);
				float num8 = 225f;
				float num9 = 212f;
				ref VectorLF3 ptr2 = ref craft.pos;
				ref Quaternion result = ref craft.rot;
				ref Vector3 ptr3 = ref craft.vel;
				float num10 = vector.x - (float)ptr2.x;
				float num11 = vector.y - (float)ptr2.y;
				float num12 = vector.z - (float)ptr2.z;
				float num13 = (float)(ptr2.x * ptr2.x + ptr2.y * ptr2.y + ptr2.z * ptr2.z);
				float num14 = vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
				float num15 = Mathf.Sqrt(num13);
				float num16 = (num14 - num13) / (num15 * 2f);
				bool flag2 = num16 > craftUnitAttackRange * 0.9f;
				if (num15 + num16 > num8)
				{
					num16 = num8 - num15;
				}
				else if (num15 + num16 < num9)
				{
					num16 = num9 - num15;
				}
				num16 += (float)(num6 % 3) + (float)(num7 % 3) * 0.7f - 1.7f;
				float num17 = (float)ptr2.x / num15;
				float num18 = (float)ptr2.y / num15;
				float num19 = (float)ptr2.z / num15;
				float num20 = num10 * num17 + num11 * num18 + num12 * num19;
				num20 -= num16 / 1.2f;
				num10 -= num17 * num20;
				num11 -= num18 * num20;
				num12 -= num19 * num20;
				float num21 = Mathf.Sqrt(num10 * num10 + num11 * num11 + num12 * num12);
				float num22 = num21;
				if (num22 < 0f)
				{
					num22 = 0f;
				}
				float num23 = (float)(num7 % 2 * 2 - 1) * ((float)(num7 / 2) * 0.5f + 1f) + (float)num6 * 0.05f;
				float num24 = num18 * num12 - num11 * num19;
				float num25 = num19 * num10 - num12 * num17;
				float num26 = num17 * num11 - num10 * num18;
				float num27 = craftUnitAttackRange;
				float num28 = num22 / num27;
				float num29 = (num28 > 1f) ? (1f / num28) : (2f - num28);
				float num30 = num29 * num29;
				float num31 = (num10 * ptr3.x + num11 * ptr3.y + num12 * ptr3.z) / num21;
				if (num31 < 0f)
				{
					num31 = -num31;
				}
				num31 += 0.2f;
				num31 /= craftUnitMaxMovementSpeed;
				float num32 = num30 * num31 * num27 * num23 / Mathf.Sqrt(num24 * num24 + num25 * num25 + num26 * num26);
				num24 *= num32;
				num25 *= num32;
				num26 *= num32;
				num10 += num24;
				num11 += num25;
				num12 += num26;
				float num33 = Mathf.Sqrt(num10 * num10 + num11 * num11 + num12 * num12);
				float num34 = craftUnitMaxMovementSpeed / num33;
				num10 *= num34;
				num11 *= num34;
				num12 *= num34;
				float num35 = (float)(360L - (UnitComponent.gameTick + 35L * (long)num7) % 420L);
				if (num35 < 0f)
				{
					num35 = -num35;
				}
				num35 = 60f - num35;
				if (num35 < 0f)
				{
					num35 = 0f;
				}
				num35 /= 5f;
				float num36 = num35;
				float num37 = (num21 - 5f) / (num36 * 1.2f + 5f);
				num37 = ((num37 > 0.28f) ? ((num37 > 1f) ? 1f : num37) : 0.28f);
				num37 *= num37;
				float num38 = num21 / craftUnitMaxMovementSpeed;
				float num39 = (num38 > 2f) ? ((num38 > 16f) ? 16f : num38) : 2f;
				num39 /= 2f;
				num4 /= num39;
				num4 *= num37;
				float num40 = num10 - ptr3.x;
				float num41 = num11 - ptr3.y;
				float num42 = num12 - ptr3.z;
				float num43 = Mathf.Sqrt(num40 * num40 + num41 * num41 + num42 * num42) / (num4 * 0.016666668f);
				if (num43 > 1f)
				{
					num40 /= num43;
					num41 /= num43;
					num42 /= num43;
				}
				ptr3.x += num40;
				ptr3.y += num41;
				ptr3.z += num42;
				VectorLF3 vectorLF2 = -vectorLF.normalized;
				float num44 = num5 / num2;
				if (num5 > num)
				{
					num44 *= 10f;
				}
				ptr3.x += (float)vectorLF2.x * num44 * 0.15f;
				ptr3.y += (float)vectorLF2.y * num44 * 0.15f;
				ptr3.z += (float)vectorLF2.z * num44 * 0.15f;
				this.speed = Mathf.Sqrt(ptr3.x * ptr3.x + ptr3.y * ptr3.y + ptr3.z * ptr3.z);
				if (this.speed < craftUnitMaxMovementSpeed + 4f)
				{
					float num45 = num35 * (1.45f + (float)num7 * 0.05f) * 0.016666668f / this.speed;
					ptr3.x += ptr3.x * num45;
					ptr3.y += ptr3.y * num45;
					ptr3.z += ptr3.z * num45;
				}
				ptr2.x += (double)ptr3.x * 0.016666666666666666;
				ptr2.y += (double)ptr3.y * 0.016666666666666666;
				ptr2.z += (double)ptr3.z * 0.016666666666666666;
				if (this.speed > 0.1f)
				{
					Maths.LookRotation(ptr3.x / this.speed, ptr3.y / this.speed, ptr3.z / this.speed, num17, num18, num19, out result);
				}
				this.anim = 0f;
				this.isShooting0 = (!flag2 && num22 < craftUnitAttackRange);
				if (this.isShooting0)
				{
					this.anim = 1f - (float)this.fire0 / (float)pdesc.craftUnitRoundInterval0 * 2f;
					if (this.anim != 0f && this.fire0 <= pdesc.craftUnitROF0 * 4)
					{
						SkillTargetLocal skillTargetLocal = this.hatred.max.skillTargetLocal;
						Vector3 vector2 = craft.pos + craft.vel * 0.03333f;
						Vector3 vector3 = Vector3.zero;
						float num46 = 2.7f;
						if (skillTargetLocal.type == ETargetType.Enemy)
						{
							float d = (vector - vector2).magnitude / (num46 * 60f);
							vector3 = vector + enemyPool[skillTargetLocal.id].vel * d - vector2;
						}
						AnimData[] craftAnimPool = factory.craftAnimPool;
						int num47 = craft.id;
						craftAnimPool[num47].prepare_length = vector3.x;
						craftAnimPool[num47].working_length = vector3.y;
						craftAnimPool[num47].power = vector3.z;
						bool flag3 = true;
						int craftUnitFireEnergy = pdesc.craftUnitFireEnergy0;
						if (ptr.owner < 0)
						{
							Mecha obj = mecha;
							lock (obj)
							{
								if (mecha.coreEnergy < (double)craftUnitFireEnergy)
								{
									flag3 = false;
								}
								goto IL_903;
							}
						}
						if (ptr.owner > 0)
						{
							ref EntityData ptr4 = ref factory.entityPool[ptr.owner];
							if (factory.defenseSystem.battleBases.buffer[ptr4.battleBaseId].energy < (long)craftUnitFireEnergy)
							{
								flag3 = false;
							}
						}
						IL_903:
						if (this.fire0 <= 0 && flag3)
						{
							if ((int)this.muzzleIndex0 == pdesc.craftUnitMuzzleCount0)
							{
								this.muzzleIndex0 = 0;
							}
							SkillSystem skillSystem = factory.skillSystem;
							ref LocalGeneralProjectile ptr5 = ref skillSystem.fighterPlasmas.Add();
							ptr5.astroId = factory.planet.astroId;
							ptr5.hitIndex = 15;
							ptr5.pos = vector2;
							ptr5.target = skillTargetLocal;
							ptr5.dir = vector3.normalized;
							ptr5.speed = num46;
							ptr5.damage = (int)((float)pdesc.craftUnitAttackDamage0 * combatUpgradeData.combatDroneDamageRatio * combatUpgradeData.energyDamageScale + 0.5f);
							ptr5.mask = ETargetTypeMask.Enemy;
							ptr5.caster.type = ETargetType.Craft;
							ptr5.caster.id = this.craftId;
							ptr5.life = 18;
							ptr5.lifemax = 18;
							SkillTarget skillTarget;
							skillTarget.id = skillTargetLocal.id;
							skillTarget.astroId = ptr5.astroId;
							skillTarget.type = skillTargetLocal.type;
							ptr5.damageIncoming = skillSystem.CalculateDamageIncoming(ref skillTarget, ptr5.damage, 1);
							int targetCombatStatId = skillSystem.AddCombatStatHPIncoming(ref skillTarget, -ptr5.damageIncoming);
							ptr5.targetCombatStatId = targetCombatStatId;
							this.muzzleIndex0 += 1;
							if ((int)this.muzzleIndex0 >= pdesc.craftUnitMuzzleCount0)
							{
								this.fire0 += pdesc.craftUnitRoundInterval0;
							}
							else
							{
								this.fire0 += pdesc.craftUnitMuzzleInterval0;
							}
							this.hatred.HateTarget(skillTargetLocal.type, skillTargetLocal.id, 30, 300, EHatredOperation.Add);
							if (ptr.owner < 0)
							{
								Mecha obj = mecha;
								lock (obj)
								{
									mecha.coreEnergy -= (double)craftUnitFireEnergy;
									mecha.MarkEnergyChange(12, (double)(-(double)craftUnitFireEnergy));
									goto IL_B31;
								}
							}
							if (ptr.owner > 0)
							{
								ref EntityData ptr6 = ref factory.entityPool[ptr.owner];
								factory.defenseSystem.battleBases.buffer[ptr6.battleBaseId].energy -= (long)craftUnitFireEnergy;
							}
							IL_B31:
							ref EnemyData ptr7 = ref enemyPool[skillTargetLocal.id];
							DFGBaseComponent dfgbaseComponent = factory.enemySystem.bases[(int)ptr7.owner];
							if (dfgbaseComponent != null && dfgbaseComponent.id == (int)ptr7.owner)
							{
								skillSystem.AddGroundEnemyHatred(dfgbaseComponent, ref ptr7, ETargetType.Craft, this.craftId);
								if (!ptr7.dynamic)
								{
									dfgbaseComponent.AddIncomingSkill(ptr5.id, ESkillType.FighterPlasmas);
								}
							}
						}
					}
				}
			}
			else
			{
				if (flag && !this.isRetreating)
				{
					this.hatred.ClearMax();
				}
				this.isRetreating = true;
				VectorLF3 pos = craftPool[craft.owner].pos;
				VectorLF3 normalized = (craft.pos - pos).normalized;
				VectorLF3 vectorLF3 = normalized * (double)num + pos;
				Quaternion quaternion = Quaternion.LookRotation(-normalized, craft.pos);
				Vector3 vector4 = default(Vector3);
				float craftUnitMaxMovementSpeed2 = pdesc.craftUnitMaxMovementSpeed;
				float num48 = pdesc.craftUnitMaxMovementAcceleration;
				ref FleetComponent ptr8 = ref factory.combatGroundSystem.fleets.buffer[fleetId];
				ref VectorLF3 ptr9 = ref craft.pos;
				ref Quaternion ptr10 = ref craft.rot;
				ref Vector3 ptr11 = ref craft.vel;
				Vector3 vector5 = new Vector3((float)(vectorLF3.x - ptr9.x), (float)(vectorLF3.y - ptr9.y), (float)(vectorLF3.z - ptr9.z));
				float num49 = Mathf.Sqrt(vector5.x * vector5.x + vector5.y * vector5.y + vector5.z * vector5.z);
				float magnitude = ptr11.magnitude;
				float num50 = (magnitude > 2f) ? magnitude : 2f;
				float num51 = num49 / num50;
				num51 -= 0.016666668f;
				if (num51 < 0f)
				{
					num51 = 0f;
				}
				float num52 = ((num51 > 1f) ? 1f : num51) * 0.3f;
				Vector3 vector6 = new Vector3((float)vectorLF3.x - vector4.x * num52, (float)vectorLF3.y - vector4.y * num52, (float)vectorLF3.z - vector4.z * num52);
				Vector3 vector7 = new Vector3(vector6.x - (float)ptr9.x, vector6.y - (float)ptr9.y, vector6.z - (float)ptr9.z);
				float num53 = Mathf.Sqrt(vector7.x * vector7.x + vector7.y * vector7.y + vector7.z * vector7.z);
				if (num53 > 0f)
				{
					vector7.x /= num53;
					vector7.y /= num53;
					vector7.z /= num53;
				}
				float num54 = (num51 > 2f) ? ((num51 > 6f) ? 6f : num51) : 2f;
				num54 /= 2f;
				num48 /= num54;
				float num55 = 0f;
				float num56 = num51 / 0.5f - 0.02f;
				if (num56 <= 0f)
				{
					ptr11.x = vector4.x;
					ptr11.y = vector4.y;
					ptr11.z = vector4.z;
				}
				else
				{
					Vector3 vector8 = new Vector3(vector7.x * craftUnitMaxMovementSpeed2, vector7.y * craftUnitMaxMovementSpeed2, vector7.z * craftUnitMaxMovementSpeed2);
					if (num56 < 1f)
					{
						float num57 = 1f - num56;
						vector8.x = vector8.x * num56 + vector4.x * num57;
						vector8.y = vector8.y * num56 + vector4.y * num57;
						vector8.z = vector8.z * num56 + vector4.z * num57;
					}
					Vector3 vector9 = new Vector3(vector8.x - ptr11.x, vector8.y - ptr11.y, vector8.z - ptr11.z);
					num55 = Mathf.Sqrt(vector9.x * vector9.x + vector9.y * vector9.y + vector9.z * vector9.z);
					float num58 = num55 / (num48 * 0.016666668f);
					if (num58 > 1f)
					{
						vector9.x /= num58;
						vector9.y /= num58;
						vector9.z /= num58;
					}
					ptr11.x += vector9.x;
					ptr11.y += vector9.y;
					ptr11.z += vector9.z;
					float num59 = (num53 - 2f) / 15f;
					if (num59 > 0f)
					{
						float num60 = (float)(ptr9.x * ptr9.x + ptr9.y * ptr9.y + ptr9.z * ptr9.z);
						float num61 = vector6.x * vector6.x + vector6.y * vector6.y + vector6.z * vector6.z - num60;
						float num62 = Mathf.Sqrt(num60);
						Vector3 vector10 = new Vector3((float)ptr9.x / num62, (float)ptr9.y / num62, (float)ptr9.z / num62);
						float num63 = ptr11.x * vector10.x + ptr11.y * vector10.y + ptr11.z * vector10.z;
						if (num63 * num61 < 0f)
						{
							if (num59 >= 1f)
							{
								ptr11.x -= vector10.x * num63;
								ptr11.y -= vector10.y * num63;
								ptr11.z -= vector10.z * num63;
							}
							else
							{
								num59 *= num59;
								num63 *= num59;
								ptr11.x -= vector10.x * num63;
								ptr11.y -= vector10.y * num63;
								ptr11.z -= vector10.z * num63;
							}
						}
					}
				}
				ptr9.x += (double)ptr11.x * 0.016666666666666666;
				ptr9.y += (double)ptr11.y * 0.016666666666666666;
				ptr9.z += (double)ptr11.z * 0.016666666666666666;
				bool flag5 = false;
				bool flag6 = false;
				float num64 = num51 + num55 / num48;
				float num65 = num64 / 0.15f - 0.04f;
				if (num65 < 1f)
				{
					if (num65 <= 0f)
					{
						ptr9.x = vectorLF3.x;
						ptr9.y = vectorLF3.y;
						ptr9.z = vectorLF3.z;
						if (ptr8.owner > 0)
						{
							this.behavior = EUnitBehavior.KeepForm;
						}
						flag5 = true;
					}
					else
					{
						float num66 = 1f - num65;
						ptr9.x = ptr9.x * (double)num65 + vectorLF3.x * (double)num66;
						ptr9.y = ptr9.y * (double)num65 + vectorLF3.y * (double)num66;
						ptr9.z = ptr9.z * (double)num65 + vectorLF3.z * (double)num66;
					}
				}
				float num67 = num64 / 0.65f - 0.04f;
				if (num67 <= 0f)
				{
					ptr10 = quaternion;
					flag6 = true;
				}
				else
				{
					if (ptr11.x * ptr11.x + ptr11.y * ptr11.y + ptr11.z * ptr11.z > 0.01f)
					{
						Quaternion b2 = Quaternion.LookRotation(ptr11, ptr9);
						ptr10 = Quaternion.Slerp(ptr10, b2, 0.1f);
					}
					if (num67 < 1f)
					{
						ptr10 = Quaternion.Slerp(quaternion, ptr10, num67 * num67);
					}
				}
				if (flag5 && flag6)
				{
					this.hatred.Reset();
					this.isRetreating = false;
				}
				this.anim = 0f;
				this.steering = 0f;
				this.speed = magnitude;
				Vector3 vector11 = craft.pos;
				float x = vector11.x;
				float y = vector11.y;
				float z = vector11.z;
				float num68 = craftUnitAttackRange * craftUnitAttackRange;
				ref HatredTarget ptr12 = ref this.hatred.max;
				this.isShooting0 = false;
				for (int i = 0; i < 8; i++)
				{
					if (i == 0)
					{
						ptr12 = ref this.hatred.max;
					}
					else if (i == 1)
					{
						ptr12 = ref this.hatred.h1;
					}
					else if (i == 2)
					{
						ptr12 = ref this.hatred.h2;
					}
					else if (i == 3)
					{
						ptr12 = ref this.hatred.h3;
					}
					else if (i == 4)
					{
						ptr12 = ref this.hatred.h4;
					}
					else if (i == 5)
					{
						ptr12 = ref this.hatred.h5;
					}
					else if (i == 6)
					{
						ptr12 = ref this.hatred.h6;
					}
					else if (i == 7)
					{
						ptr12 = ref this.hatred.min;
					}
					if (ptr12.targetType == ETargetType.Enemy)
					{
						int objectId = ptr12.objectId;
						ref EnemyData ptr13 = ref enemyPool[objectId];
						if (ptr13.id == objectId && !ptr13.isInvincible)
						{
							float num69 = (float)ptr13.pos.x;
							float num70 = (float)ptr13.pos.y;
							float num71 = (float)ptr13.pos.z;
							float num72 = num69 - x;
							float num73 = num70 - y;
							float num74 = num71 - z;
							if (num72 * num72 + num73 * num73 + num74 * num74 <= num68)
							{
								this.isShooting0 = true;
								break;
							}
						}
					}
				}
				if (this.isShooting0)
				{
					this.anim = 1f - (float)this.fire0 / (float)pdesc.craftUnitRoundInterval0 * 2f;
					if (this.anim != 0f && this.fire0 <= pdesc.craftUnitROF0 * 4)
					{
						SkillTargetLocal skillTargetLocal2 = ptr12.skillTargetLocal;
						VectorLF3 pos2 = craft.pos;
						Vector3 vector12 = Vector3.zero;
						float num75 = 2.7f;
						if (skillTargetLocal2.type == ETargetType.Enemy)
						{
							vectorLF3 = enemyPool[skillTargetLocal2.id].pos;
							Vector3 a = vectorLF3 - pos2;
							float d2 = a.magnitude / (num75 * 60f);
							vector12 = a + enemyPool[skillTargetLocal2.id].vel * d2;
						}
						AnimData[] craftAnimPool2 = factory.craftAnimPool;
						int num76 = craft.id;
						craftAnimPool2[num76].prepare_length = vector12.x;
						craftAnimPool2[num76].working_length = vector12.y;
						craftAnimPool2[num76].power = vector12.z;
						bool flag7 = true;
						int craftUnitFireEnergy2 = pdesc.craftUnitFireEnergy0;
						if (ptr.owner < 0)
						{
							Mecha obj = mecha;
							lock (obj)
							{
								if (mecha.coreEnergy < (double)craftUnitFireEnergy2)
								{
									flag7 = false;
								}
								goto IL_16A8;
							}
						}
						if (ptr.owner > 0)
						{
							ref EntityData ptr14 = ref factory.entityPool[ptr.owner];
							if (factory.defenseSystem.battleBases.buffer[ptr14.battleBaseId].energy < (long)craftUnitFireEnergy2)
							{
								flag7 = false;
							}
						}
						IL_16A8:
						if (this.fire0 <= 0 && flag7)
						{
							if ((int)this.muzzleIndex0 == pdesc.craftUnitMuzzleCount0)
							{
								this.muzzleIndex0 = 0;
							}
							SkillSystem skillSystem2 = factory.skillSystem;
							ref LocalGeneralProjectile ptr15 = ref skillSystem2.fighterPlasmas.Add();
							ptr15.astroId = factory.planetId;
							ptr15.hitIndex = 15;
							ptr15.pos = pos2;
							ptr15.target = skillTargetLocal2;
							ptr15.dir = vector12.normalized;
							ptr15.speed = num75;
							ptr15.damage = (int)((float)pdesc.craftUnitAttackDamage0 * combatUpgradeData.combatDroneDamageRatio * combatUpgradeData.energyDamageScale + 0.5f);
							ptr15.mask = ETargetTypeMask.Enemy;
							ptr15.caster.type = ETargetType.Craft;
							ptr15.caster.id = this.craftId;
							ptr15.life = 18;
							ptr15.lifemax = 18;
							SkillTarget skillTarget2;
							skillTarget2.id = skillTargetLocal2.id;
							skillTarget2.astroId = ptr15.astroId;
							skillTarget2.type = skillTargetLocal2.type;
							ptr15.damageIncoming = skillSystem2.CalculateDamageIncoming(ref skillTarget2, ptr15.damage, 1);
							int targetCombatStatId2 = skillSystem2.AddCombatStatHPIncoming(ref skillTarget2, -ptr15.damageIncoming);
							ptr15.targetCombatStatId = targetCombatStatId2;
							this.muzzleIndex0 += 1;
							if ((int)this.muzzleIndex0 >= pdesc.craftUnitMuzzleCount0)
							{
								this.fire0 += pdesc.craftUnitRoundInterval0;
							}
							else
							{
								this.fire0 += pdesc.craftUnitMuzzleInterval0;
							}
							if (ptr.owner < 0)
							{
								Mecha obj = mecha;
								lock (obj)
								{
									mecha.coreEnergy -= (double)craftUnitFireEnergy2;
									mecha.MarkEnergyChange(12, (double)(-(double)craftUnitFireEnergy2));
									goto IL_18B5;
								}
							}
							if (ptr.owner > 0)
							{
								ref EntityData ptr16 = ref factory.entityPool[ptr.owner];
								factory.defenseSystem.battleBases.buffer[ptr16.battleBaseId].energy -= (long)craftUnitFireEnergy2;
							}
							IL_18B5:
							ref EnemyData ptr17 = ref enemyPool[skillTargetLocal2.id];
							DFGBaseComponent dfgbaseComponent2 = factory.enemySystem.bases[(int)ptr17.owner];
							if (dfgbaseComponent2 != null && dfgbaseComponent2.id == (int)ptr17.owner)
							{
								skillSystem2.AddGroundEnemyHatred(dfgbaseComponent2, ref ptr17, ETargetType.Craft, this.craftId);
								if (!ptr17.dynamic)
								{
									dfgbaseComponent2.AddIncomingSkill(ptr15.id, ESkillType.FighterPlasmas);
								}
							}
						}
					}
				}
			}
			this.steering = Vector3.Dot(craft.rot.Forward() - b, rhs);
			return;
		}
		this.RunBehavior_Engage_EmptyHatred(ref craft);
	}

	// Token: 0x06000F6C RID: 3948 RVA: 0x000FB1FC File Offset: 0x000F93FC
	public void RunBehavior_Engage_DefenseShield_Ground(PlanetFactory factory, Mecha mecha, PrefabDesc pdesc, ref CraftData craft, ref CombatSettings combatSettings, ref CombatUpgradeData combatUpgradeData)
	{
		Vector3 vector;
		if (this.GetTargetPosition_Ground(factory, this.hatred.max.target, out vector))
		{
			float num = 25f;
			float num2 = 35f;
			EnemyData[] enemyPool = factory.enemyPool;
			CraftData[] craftPool = factory.craftPool;
			int fleetId = craftPool[craft.owner].fleetId;
			ref CraftData ptr = ref craftPool[craft.owner];
			if (fleetId > 0)
			{
				PrefabDesc prefabDesc = PlanetFactory.PrefabDescByModelIndex[(int)ptr.modelIndex];
				num = prefabDesc.fleetSensorRange;
				num2 = prefabDesc.fleetMaxActiveArea;
				if (this.hatred.max.value > 800)
				{
					float num3 = (float)(enemyPool[this.hatred.max.objectId].pos - ptr.pos).magnitude;
					num += num3;
					num2 += num3;
				}
			}
			Vector3 b;
			Vector3 rhs;
			craft.rot.ForwardRight(out b, out rhs);
			float craftUnitMaxMovementSpeed = pdesc.craftUnitMaxMovementSpeed;
			float num4 = pdesc.craftUnitMaxMovementAcceleration;
			float craftUnitAttackRange = pdesc.craftUnitAttackRange0;
			VectorLF3 vectorLF = craft.pos - ptr.pos;
			float num5 = (float)vectorLF.magnitude;
			bool flag = num5 > num2;
			if (!flag && !this.isRetreating)
			{
				short port = craft.port;
				int num6 = (int)(port / 12);
				int num7 = (int)(port % 12);
				float num8 = 225f;
				float num9 = 212f;
				ref VectorLF3 ptr2 = ref craft.pos;
				ref Quaternion result = ref craft.rot;
				ref Vector3 ptr3 = ref craft.vel;
				float num10 = vector.x - (float)ptr2.x;
				float num11 = vector.y - (float)ptr2.y;
				float num12 = vector.z - (float)ptr2.z;
				float num13 = (float)(ptr2.x * ptr2.x + ptr2.y * ptr2.y + ptr2.z * ptr2.z);
				float num14 = vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
				float num15 = Mathf.Sqrt(num13);
				float num16 = (num14 - num13) / (num15 * 2f);
				bool flag2 = num16 > craftUnitAttackRange * 0.9f;
				if (num15 + num16 > num8)
				{
					num16 = num8 - num15;
				}
				else if (num15 + num16 < num9)
				{
					num16 = num9 - num15;
				}
				num16 += (float)(num6 % 3) + (float)(num7 % 3) * 0.7f - 1.7f;
				float num17 = (float)ptr2.x / num15;
				float num18 = (float)ptr2.y / num15;
				float num19 = (float)ptr2.z / num15;
				float num20 = num10 * num17 + num11 * num18 + num12 * num19;
				num20 -= num16 / 1.2f;
				num10 -= num17 * num20;
				num11 -= num18 * num20;
				num12 -= num19 * num20;
				float num21 = Mathf.Sqrt(num10 * num10 + num11 * num11 + num12 * num12);
				float num22 = num21;
				if (num22 < 0f)
				{
					num22 = 0f;
				}
				float num23 = (float)(num7 % 2 * 2 - 1) * ((float)(num7 / 2) * 0.5f + 1f) + (float)num6 * 0.05f;
				float num24 = num18 * num12 - num11 * num19;
				float num25 = num19 * num10 - num12 * num17;
				float num26 = num17 * num11 - num10 * num18;
				float num27 = craftUnitAttackRange;
				float num28 = num22 / num27;
				float num29 = (num28 > 1f) ? (1f / num28) : (2f - num28);
				float num30 = num29 * num29;
				float num31 = (num10 * ptr3.x + num11 * ptr3.y + num12 * ptr3.z) / num21;
				if (num31 < 0f)
				{
					num31 = -num31;
				}
				num31 += 0.2f;
				num31 /= craftUnitMaxMovementSpeed;
				float num32 = num30 * num31 * num27 * num23 / Mathf.Sqrt(num24 * num24 + num25 * num25 + num26 * num26);
				num24 *= num32;
				num25 *= num32;
				num26 *= num32;
				num10 += num24;
				num11 += num25;
				num12 += num26;
				float num33 = Mathf.Sqrt(num10 * num10 + num11 * num11 + num12 * num12);
				float num34 = craftUnitMaxMovementSpeed / num33;
				num10 *= num34;
				num11 *= num34;
				num12 *= num34;
				float num35 = (float)(360L - (UnitComponent.gameTick + 35L * (long)num7) % 420L);
				if (num35 < 0f)
				{
					num35 = -num35;
				}
				num35 = 60f - num35;
				if (num35 < 0f)
				{
					num35 = 0f;
				}
				num35 /= 5f;
				float num36 = num35;
				float num37 = (num21 - 5f) / (num36 * 1.2f + 5f);
				num37 = ((num37 > 0.28f) ? ((num37 > 1f) ? 1f : num37) : 0.28f);
				num37 *= num37;
				float num38 = num21 / craftUnitMaxMovementSpeed;
				float num39 = (num38 > 2f) ? ((num38 > 16f) ? 16f : num38) : 2f;
				num39 /= 2f;
				num4 /= num39;
				num4 *= num37;
				float num40 = num10 - ptr3.x;
				float num41 = num11 - ptr3.y;
				float num42 = num12 - ptr3.z;
				float num43 = Mathf.Sqrt(num40 * num40 + num41 * num41 + num42 * num42) / (num4 * 0.016666668f);
				if (num43 > 1f)
				{
					num40 /= num43;
					num41 /= num43;
					num42 /= num43;
				}
				ptr3.x += num40;
				ptr3.y += num41;
				ptr3.z += num42;
				VectorLF3 vectorLF2 = -vectorLF.normalized;
				float num44 = num5 / num2;
				if (num5 > num)
				{
					num44 *= 10f;
				}
				ptr3.x += (float)vectorLF2.x * num44 * 0.15f;
				ptr3.y += (float)vectorLF2.y * num44 * 0.15f;
				ptr3.z += (float)vectorLF2.z * num44 * 0.15f;
				this.speed = Mathf.Sqrt(ptr3.x * ptr3.x + ptr3.y * ptr3.y + ptr3.z * ptr3.z);
				if (this.speed < craftUnitMaxMovementSpeed + 4f)
				{
					float num45 = num35 * (1.45f + (float)num7 * 0.05f) * 0.016666668f / this.speed;
					ptr3.x += ptr3.x * num45;
					ptr3.y += ptr3.y * num45;
					ptr3.z += ptr3.z * num45;
				}
				ptr2.x += (double)ptr3.x * 0.016666666666666666;
				ptr2.y += (double)ptr3.y * 0.016666666666666666;
				ptr2.z += (double)ptr3.z * 0.016666666666666666;
				if (this.speed > 0.1f)
				{
					Maths.LookRotation(ptr3.x / this.speed, ptr3.y / this.speed, ptr3.z / this.speed, num17, num18, num19, out result);
				}
				this.anim = 0f;
				this.isShooting0 = (!flag2 && num22 < craftUnitAttackRange);
				if (this.isShooting0)
				{
					this.anim = 1f - (float)this.fire0 / (float)pdesc.craftUnitRoundInterval0 * 2f;
					if (this.anim != 0f && this.fire0 <= pdesc.craftUnitROF0 * 4)
					{
						SkillTargetLocal skillTargetLocal = this.hatred.max.skillTargetLocal;
						Vector3 vector2 = craft.pos + craft.vel * 0.03333f;
						Vector3 vector3 = Vector3.zero;
						float num46 = 1.8f;
						if (skillTargetLocal.type == ETargetType.Enemy)
						{
							float d = (vector - vector2).magnitude / (num46 * 60f);
							vector3 = vector + enemyPool[skillTargetLocal.id].vel * d - vector2;
						}
						AnimData[] craftAnimPool = factory.craftAnimPool;
						int num47 = craft.id;
						craftAnimPool[num47].prepare_length = vector3.x;
						craftAnimPool[num47].working_length = vector3.y;
						craftAnimPool[num47].power = vector3.z;
						bool flag3 = true;
						int craftUnitFireEnergy = pdesc.craftUnitFireEnergy0;
						if (ptr.owner < 0)
						{
							Mecha obj = mecha;
							lock (obj)
							{
								if (mecha.coreEnergy < (double)craftUnitFireEnergy)
								{
									flag3 = false;
								}
								goto IL_903;
							}
						}
						if (ptr.owner > 0)
						{
							ref EntityData ptr4 = ref factory.entityPool[ptr.owner];
							if (factory.defenseSystem.battleBases.buffer[ptr4.battleBaseId].energy < (long)craftUnitFireEnergy)
							{
								flag3 = false;
							}
						}
						IL_903:
						if (this.fire0 <= 0 && flag3)
						{
							SkillSystem skillSystem = factory.skillSystem;
							ref LocalGeneralProjectile ptr5 = ref skillSystem.fighterShieldPlasmas.Add();
							ptr5.astroId = factory.planetId;
							ptr5.hitIndex = 30;
							ptr5.pos = vector2;
							ptr5.target = skillTargetLocal;
							ptr5.dir = vector3.normalized;
							ptr5.speed = num46;
							ptr5.damage = (int)((float)pdesc.craftUnitAttackDamage0 * combatUpgradeData.combatDroneDamageRatio * combatUpgradeData.energyDamageScale + 0.5f);
							ptr5.mask = ETargetTypeMask.Enemy;
							ptr5.caster.type = ETargetType.Craft;
							ptr5.caster.id = this.craftId;
							ptr5.life = 10;
							ptr5.lifemax = 10;
							SkillTarget skillTarget;
							skillTarget.id = skillTargetLocal.id;
							skillTarget.astroId = ptr5.astroId;
							skillTarget.type = skillTargetLocal.type;
							ptr5.damageIncoming = skillSystem.CalculateDamageIncoming(ref skillTarget, ptr5.damage, 1);
							int targetCombatStatId = skillSystem.AddCombatStatHPIncoming(ref skillTarget, -ptr5.damageIncoming);
							ptr5.targetCombatStatId = targetCombatStatId;
							this.fire0 += pdesc.craftUnitRoundInterval0;
							this.hatred.HateTarget(skillTargetLocal.type, skillTargetLocal.id, 30, 300, EHatredOperation.Add);
							if (ptr.owner < 0)
							{
								Mecha obj = mecha;
								lock (obj)
								{
									mecha.coreEnergy -= (double)craftUnitFireEnergy;
									mecha.MarkEnergyChange(12, (double)(-(double)craftUnitFireEnergy));
									goto IL_AE5;
								}
							}
							if (ptr.owner > 0)
							{
								ref EntityData ptr6 = ref factory.entityPool[ptr.owner];
								factory.defenseSystem.battleBases.buffer[ptr6.battleBaseId].energy -= (long)craftUnitFireEnergy;
							}
							IL_AE5:
							ref EnemyData ptr7 = ref enemyPool[skillTargetLocal.id];
							DFGBaseComponent dfgbaseComponent = factory.enemySystem.bases[(int)ptr7.owner];
							if (dfgbaseComponent != null && dfgbaseComponent.id == (int)ptr7.owner)
							{
								skillSystem.AddGroundEnemyHatred(dfgbaseComponent, ref ptr7, ETargetType.Craft, this.craftId);
								if (!ptr7.dynamic)
								{
									dfgbaseComponent.AddIncomingSkill(ptr5.id, ESkillType.FighterShieldPlasmas);
								}
							}
						}
					}
				}
			}
			else
			{
				if (flag && !this.isRetreating)
				{
					this.hatred.ClearMax();
				}
				this.isRetreating = true;
				VectorLF3 pos = craftPool[craft.owner].pos;
				VectorLF3 normalized = (craft.pos - pos).normalized;
				VectorLF3 vectorLF3 = normalized * (double)num + pos;
				Quaternion quaternion = Quaternion.LookRotation(-normalized, craft.pos);
				Vector3 vector4 = default(Vector3);
				float craftUnitMaxMovementSpeed2 = pdesc.craftUnitMaxMovementSpeed;
				float num48 = pdesc.craftUnitMaxMovementAcceleration;
				ref FleetComponent ptr8 = ref factory.combatGroundSystem.fleets.buffer[fleetId];
				ref VectorLF3 ptr9 = ref craft.pos;
				ref Quaternion ptr10 = ref craft.rot;
				ref Vector3 ptr11 = ref craft.vel;
				Vector3 vector5 = new Vector3((float)(vectorLF3.x - ptr9.x), (float)(vectorLF3.y - ptr9.y), (float)(vectorLF3.z - ptr9.z));
				float num49 = Mathf.Sqrt(vector5.x * vector5.x + vector5.y * vector5.y + vector5.z * vector5.z);
				float magnitude = ptr11.magnitude;
				float num50 = (magnitude > 2f) ? magnitude : 2f;
				float num51 = num49 / num50;
				num51 -= 0.016666668f;
				if (num51 < 0f)
				{
					num51 = 0f;
				}
				float num52 = ((num51 > 1f) ? 1f : num51) * 0.3f;
				Vector3 vector6 = new Vector3((float)vectorLF3.x - vector4.x * num52, (float)vectorLF3.y - vector4.y * num52, (float)vectorLF3.z - vector4.z * num52);
				Vector3 vector7 = new Vector3(vector6.x - (float)ptr9.x, vector6.y - (float)ptr9.y, vector6.z - (float)ptr9.z);
				float num53 = Mathf.Sqrt(vector7.x * vector7.x + vector7.y * vector7.y + vector7.z * vector7.z);
				if (num53 > 0f)
				{
					vector7.x /= num53;
					vector7.y /= num53;
					vector7.z /= num53;
				}
				float num54 = (num51 > 2f) ? ((num51 > 6f) ? 6f : num51) : 2f;
				num54 /= 2f;
				num48 /= num54;
				float num55 = 0f;
				float num56 = num51 / 0.5f - 0.02f;
				if (num56 <= 0f)
				{
					ptr11.x = vector4.x;
					ptr11.y = vector4.y;
					ptr11.z = vector4.z;
				}
				else
				{
					Vector3 vector8 = new Vector3(vector7.x * craftUnitMaxMovementSpeed2, vector7.y * craftUnitMaxMovementSpeed2, vector7.z * craftUnitMaxMovementSpeed2);
					if (num56 < 1f)
					{
						float num57 = 1f - num56;
						vector8.x = vector8.x * num56 + vector4.x * num57;
						vector8.y = vector8.y * num56 + vector4.y * num57;
						vector8.z = vector8.z * num56 + vector4.z * num57;
					}
					Vector3 vector9 = new Vector3(vector8.x - ptr11.x, vector8.y - ptr11.y, vector8.z - ptr11.z);
					num55 = Mathf.Sqrt(vector9.x * vector9.x + vector9.y * vector9.y + vector9.z * vector9.z);
					float num58 = num55 / (num48 * 0.016666668f);
					if (num58 > 1f)
					{
						vector9.x /= num58;
						vector9.y /= num58;
						vector9.z /= num58;
					}
					ptr11.x += vector9.x;
					ptr11.y += vector9.y;
					ptr11.z += vector9.z;
					float num59 = (num53 - 2f) / 15f;
					if (num59 > 0f)
					{
						float num60 = (float)(ptr9.x * ptr9.x + ptr9.y * ptr9.y + ptr9.z * ptr9.z);
						float num61 = vector6.x * vector6.x + vector6.y * vector6.y + vector6.z * vector6.z - num60;
						float num62 = Mathf.Sqrt(num60);
						Vector3 vector10 = new Vector3((float)ptr9.x / num62, (float)ptr9.y / num62, (float)ptr9.z / num62);
						float num63 = ptr11.x * vector10.x + ptr11.y * vector10.y + ptr11.z * vector10.z;
						if (num63 * num61 < 0f)
						{
							if (num59 >= 1f)
							{
								ptr11.x -= vector10.x * num63;
								ptr11.y -= vector10.y * num63;
								ptr11.z -= vector10.z * num63;
							}
							else
							{
								num59 *= num59;
								num63 *= num59;
								ptr11.x -= vector10.x * num63;
								ptr11.y -= vector10.y * num63;
								ptr11.z -= vector10.z * num63;
							}
						}
					}
				}
				ptr9.x += (double)ptr11.x * 0.016666666666666666;
				ptr9.y += (double)ptr11.y * 0.016666666666666666;
				ptr9.z += (double)ptr11.z * 0.016666666666666666;
				bool flag5 = false;
				bool flag6 = false;
				float num64 = num51 + num55 / num48;
				float num65 = num64 / 0.15f - 0.04f;
				if (num65 < 1f)
				{
					if (num65 <= 0f)
					{
						ptr9.x = vectorLF3.x;
						ptr9.y = vectorLF3.y;
						ptr9.z = vectorLF3.z;
						if (ptr8.owner > 0)
						{
							this.behavior = EUnitBehavior.KeepForm;
						}
						flag5 = true;
					}
					else
					{
						float num66 = 1f - num65;
						ptr9.x = ptr9.x * (double)num65 + vectorLF3.x * (double)num66;
						ptr9.y = ptr9.y * (double)num65 + vectorLF3.y * (double)num66;
						ptr9.z = ptr9.z * (double)num65 + vectorLF3.z * (double)num66;
					}
				}
				float num67 = num64 / 0.65f - 0.04f;
				if (num67 <= 0f)
				{
					ptr10 = quaternion;
					flag6 = true;
				}
				else
				{
					if (ptr11.x * ptr11.x + ptr11.y * ptr11.y + ptr11.z * ptr11.z > 0.01f)
					{
						Quaternion b2 = Quaternion.LookRotation(ptr11, ptr9);
						ptr10 = Quaternion.Slerp(ptr10, b2, 0.1f);
					}
					if (num67 < 1f)
					{
						ptr10 = Quaternion.Slerp(quaternion, ptr10, num67 * num67);
					}
				}
				if (flag5 && flag6)
				{
					this.hatred.Reset();
					this.isRetreating = false;
				}
				this.anim = 0f;
				this.steering = 0f;
				this.speed = magnitude;
				Vector3 vector11 = craft.pos;
				float x = vector11.x;
				float y = vector11.y;
				float z = vector11.z;
				float num68 = craftUnitAttackRange * craftUnitAttackRange;
				ref HatredTarget ptr12 = ref this.hatred.max;
				this.isShooting0 = false;
				for (int i = 0; i < 8; i++)
				{
					if (i == 0)
					{
						ptr12 = ref this.hatred.max;
					}
					else if (i == 1)
					{
						ptr12 = ref this.hatred.h1;
					}
					else if (i == 2)
					{
						ptr12 = ref this.hatred.h2;
					}
					else if (i == 3)
					{
						ptr12 = ref this.hatred.h3;
					}
					else if (i == 4)
					{
						ptr12 = ref this.hatred.h4;
					}
					else if (i == 5)
					{
						ptr12 = ref this.hatred.h5;
					}
					else if (i == 6)
					{
						ptr12 = ref this.hatred.h6;
					}
					else if (i == 7)
					{
						ptr12 = ref this.hatred.min;
					}
					if (ptr12.targetType == ETargetType.Enemy)
					{
						int objectId = ptr12.objectId;
						ref EnemyData ptr13 = ref enemyPool[objectId];
						if (ptr13.id == objectId && !ptr13.isInvincible)
						{
							float num69 = (float)ptr13.pos.x;
							float num70 = (float)ptr13.pos.y;
							float num71 = (float)ptr13.pos.z;
							float num72 = num69 - x;
							float num73 = num70 - y;
							float num74 = num71 - z;
							if (num72 * num72 + num73 * num73 + num74 * num74 <= num68)
							{
								this.isShooting0 = true;
								break;
							}
						}
					}
				}
				if (this.isShooting0)
				{
					this.anim = 1f - (float)this.fire0 / (float)pdesc.craftUnitRoundInterval0 * 2f;
					if (this.anim != 0f && this.fire0 <= pdesc.craftUnitROF0 * 4)
					{
						SkillTargetLocal skillTargetLocal2 = ptr12.skillTargetLocal;
						VectorLF3 pos2 = craft.pos;
						Vector3 vector12 = Vector3.zero;
						float num75 = 1.8f;
						if (skillTargetLocal2.type == ETargetType.Enemy)
						{
							vectorLF3 = enemyPool[skillTargetLocal2.id].pos;
							Vector3 a = vectorLF3 - pos2;
							float d2 = a.magnitude / (num75 * 60f);
							vector12 = a + enemyPool[skillTargetLocal2.id].vel * d2;
						}
						AnimData[] craftAnimPool2 = factory.craftAnimPool;
						int num76 = craft.id;
						craftAnimPool2[num76].prepare_length = vector12.x;
						craftAnimPool2[num76].working_length = vector12.y;
						craftAnimPool2[num76].power = vector12.z;
						bool flag7 = true;
						int craftUnitFireEnergy2 = pdesc.craftUnitFireEnergy0;
						if (ptr.owner < 0)
						{
							Mecha obj = mecha;
							lock (obj)
							{
								if (mecha.coreEnergy < (double)craftUnitFireEnergy2)
								{
									flag7 = false;
								}
								goto IL_165C;
							}
						}
						if (ptr.owner > 0)
						{
							ref EntityData ptr14 = ref factory.entityPool[ptr.owner];
							if (factory.defenseSystem.battleBases.buffer[ptr14.battleBaseId].energy < (long)craftUnitFireEnergy2)
							{
								flag7 = false;
							}
						}
						IL_165C:
						if (this.fire0 <= 0 && flag7)
						{
							SkillSystem skillSystem2 = factory.skillSystem;
							ref LocalGeneralProjectile ptr15 = ref skillSystem2.fighterShieldPlasmas.Add();
							ptr15.astroId = factory.planetId;
							ptr15.hitIndex = 30;
							ptr15.pos = pos2;
							ptr15.target = skillTargetLocal2;
							ptr15.dir = vector12.normalized;
							ptr15.speed = num75;
							ptr15.damage = (int)((float)pdesc.craftUnitAttackDamage0 * combatUpgradeData.combatDroneDamageRatio * combatUpgradeData.energyDamageScale + 0.5f);
							ptr15.mask = ETargetTypeMask.Enemy;
							ptr15.caster.type = ETargetType.Craft;
							ptr15.caster.id = this.craftId;
							ptr15.life = 10;
							ptr15.lifemax = 10;
							SkillTarget skillTarget2;
							skillTarget2.id = skillTargetLocal2.id;
							skillTarget2.astroId = ptr15.astroId;
							skillTarget2.type = skillTargetLocal2.type;
							ptr15.damageIncoming = skillSystem2.CalculateDamageIncoming(ref skillTarget2, ptr15.damage, 1);
							int targetCombatStatId2 = skillSystem2.AddCombatStatHPIncoming(ref skillTarget2, -ptr15.damageIncoming);
							ptr15.targetCombatStatId = targetCombatStatId2;
							this.fire0 += pdesc.craftUnitRoundInterval0;
							if (ptr.owner < 0)
							{
								Mecha obj = mecha;
								lock (obj)
								{
									mecha.coreEnergy -= (double)craftUnitFireEnergy2;
									mecha.MarkEnergyChange(12, (double)(-(double)craftUnitFireEnergy2));
									goto IL_1822;
								}
							}
							if (ptr.owner > 0)
							{
								ref EntityData ptr16 = ref factory.entityPool[ptr.owner];
								factory.defenseSystem.battleBases.buffer[ptr16.battleBaseId].energy -= (long)craftUnitFireEnergy2;
							}
							IL_1822:
							ref EnemyData ptr17 = ref enemyPool[skillTargetLocal2.id];
							DFGBaseComponent dfgbaseComponent2 = factory.enemySystem.bases[(int)ptr17.owner];
							if (dfgbaseComponent2 != null && dfgbaseComponent2.id == (int)ptr17.owner)
							{
								skillSystem2.AddGroundEnemyHatred(dfgbaseComponent2, ref ptr17, ETargetType.Craft, this.craftId);
								if (!ptr17.dynamic)
								{
									dfgbaseComponent2.AddIncomingSkill(ptr15.id, ESkillType.FighterShieldPlasmas);
								}
							}
						}
					}
				}
			}
			this.steering = Vector3.Dot(craft.rot.Forward() - b, rhs);
			return;
		}
		this.RunBehavior_Engage_EmptyHatred(ref craft);
	}

	// Token: 0x06000F6D RID: 3949 RVA: 0x000FCAF0 File Offset: 0x000FACF0
	public void RunBehavior_Engage_SAttackLaser_Large(SpaceSector sector, Mecha mecha, PrefabDesc pdesc, ref CraftData craft, ref CombatSettings combatSettings, ref CombatUpgradeData combatUpgradeData)
	{
		VectorLF3 vectorLF = new VectorLF3(0f, 0f, 0f);
		Quaternion quaternion = new Quaternion(0f, 0f, 0f, 1f);
		Vector3 vector = new VectorLF3(0f, 0f, 0f);
		Vector3 vector2 = default(Vector3);
		int astroId;
		if (this.GetTargetPosition_Space(sector, this.hatred.max.target, ref vectorLF, ref vector2, out astroId))
		{
			VectorLF3 endPosU = vectorLF;
			int astroId2 = craft.astroId;
			sector.InverseTransformToAstro_ref(astroId2, ref endPosU, out vectorLF);
			float num = 35f;
			EnemyData[] enemyPool = sector.enemyPool;
			CraftData[] craftPool = sector.craftPool;
			int fleetId = craftPool[craft.owner].fleetId;
			ref CraftData ptr = ref craftPool[craft.owner];
			if (fleetId > 0)
			{
				PrefabDesc prefabDesc = PlanetFactory.PrefabDescByModelIndex[(int)ptr.modelIndex];
				float num2 = prefabDesc.fleetSensorRange;
				num = prefabDesc.fleetMaxActiveArea;
				if (this.hatred.max.value > 8000)
				{
					float num3 = (float)(enemyPool[this.hatred.max.objectId].pos - ptr.pos).magnitude;
					num2 += num3;
					num += num3;
				}
			}
			float craftUnitMaxMovementSpeed = pdesc.craftUnitMaxMovementSpeed;
			float craftUnitMaxMovementAcceleration = pdesc.craftUnitMaxMovementAcceleration;
			float craftUnitMaxRotateAcceleration = pdesc.craftUnitMaxRotateAcceleration;
			float num4 = craftUnitMaxMovementAcceleration * craftUnitMaxMovementAcceleration;
			float num5 = craftUnitMaxMovementSpeed * craftUnitMaxMovementSpeed;
			float craftUnitAttackRange = pdesc.craftUnitAttackRange0;
			float craftUnitAttackRange2 = pdesc.craftUnitAttackRange1;
			bool flag = (float)(craft.pos - ptr.pos).magnitude > num;
			if (!flag && !this.isRetreating)
			{
				VectorLF3 vectorLF2 = craft.pos;
				Quaternion quaternion2 = craft.rot;
				Vector3 vector3 = craft.vel;
				VectorLF3 vectorLF3 = new VectorLF3(vectorLF.x - vectorLF2.x, vectorLF.y - vectorLF2.y, vectorLF.z - vectorLF2.z);
				VectorLF3 normalized = vectorLF3.normalized;
				VectorLF3 vectorLF4 = vectorLF3 - normalized * (double)craftUnitAttackRange * 0.800000011920929;
				quaternion = Quaternion.LookRotation(normalized, quaternion2.Up());
				float num6 = Mathf.Sqrt((float)(vectorLF4.x * vectorLF4.x) + (float)(vectorLF4.y * vectorLF4.y) + (float)(vectorLF4.z * vectorLF4.z));
				float num7 = 0.016666668f;
				float magnitude = vector3.magnitude;
				ref FleetComponent ptr2 = ref sector.combatSpaceSystem.fleets.buffer[fleetId];
				Vector3 point = default(Vector3);
				if (fleetId > 0)
				{
					ptr2.GetUnitFormOffset_Space((int)craft.port, mecha, ref craftPool[ptr2.craftId], ref point, ref combatUpgradeData);
				}
				vectorLF += quaternion * point;
				float num8 = 0.1f;
				if (num8 < 0.1f)
				{
					num8 = 0.1f;
				}
				if (num6 > num8)
				{
					Vector3 rhs = quaternion2.Forward();
					float num9 = Vector3.Dot(quaternion.Forward(), rhs);
					bool astroAvoidanceTargetPos;
					Vector3 a;
					if (Vector3.Dot(normalized, rhs) > 0f && num9 < -0.95f)
					{
						VectorLF3 vectorLF5 = new VectorLF3(vectorLF.x - vectorLF2.x, vectorLF.y - vectorLF2.y, vectorLF.z - vectorLF2.z);
						vectorLF5 = vectorLF - vectorLF5.normalized * (double)craftUnitAttackRange * 0.800000011920929;
						VectorLF3 vectorLF6;
						sector.TransformFromAstro_ref(astroId2, out vectorLF6, ref vectorLF5);
						astroAvoidanceTargetPos = this.GetAstroAvoidanceTargetPos(sector, craftUnitMaxMovementSpeed, 0, ref vectorLF6, ref craft);
						sector.InverseTransformToAstro_ref(astroId2, ref vectorLF6, out vectorLF5);
						a = this.Arrive(ref vectorLF5, ref craft, ref vector, craftUnitMaxMovementSpeed, astroAvoidanceTargetPos, 3000f);
					}
					else
					{
						Vector3 a2 = vector2;
						float d = num6 / (craftUnitMaxMovementSpeed + a2.magnitude);
						VectorLF3 vectorLF7 = vectorLF + a2 * d;
						VectorLF3 vectorLF8 = new VectorLF3(vectorLF7.x - vectorLF2.x, vectorLF7.y - vectorLF2.y, vectorLF7.z - vectorLF2.z);
						vectorLF8 = vectorLF7 - vectorLF8.normalized * (double)craftUnitAttackRange * 0.800000011920929;
						VectorLF3 vectorLF9;
						sector.TransformFromAstro_ref(astroId2, out vectorLF9, ref vectorLF8);
						astroAvoidanceTargetPos = this.GetAstroAvoidanceTargetPos(sector, craftUnitMaxMovementSpeed, 0, ref vectorLF9, ref craft);
						sector.InverseTransformToAstro_ref(astroId2, ref vectorLF9, out vectorLF8);
						a = this.Arrive(ref vectorLF8, ref craft, ref vector, craftUnitMaxMovementSpeed, astroAvoidanceTargetPos, 3000f);
					}
					if (a.sqrMagnitude > num4 || astroAvoidanceTargetPos)
					{
						a = a.normalized * craftUnitMaxMovementAcceleration;
					}
					vector3 += a * num7;
				}
				else
				{
					vector3 = vector2;
				}
				Vector3 normalized2 = vector3.normalized;
				if (vector3.sqrMagnitude > num5)
				{
					vector3 = normalized2 * craftUnitMaxMovementSpeed;
				}
				float num10 = Quaternion.Angle(quaternion2, quaternion) / 180f;
				float num11 = craftUnitMaxRotateAcceleration * (num10 * num10);
				if (num11 < 0.1f)
				{
					num11 = 0.1f;
				}
				quaternion2 = Quaternion.RotateTowards(quaternion2, quaternion, num11);
				vectorLF2 += vector3 * (double)num7;
				craft.pos = vectorLF2;
				craft.rot = quaternion2;
				craft.vel = vector3;
				this.anim = 0f;
				this.steering = 0f;
				this.speed = magnitude;
				SkillTarget skillTarget = this.hatred.max.GetSkillTarget(0);
				int craftUnitMuzzleCount = pdesc.craftUnitMuzzleCount0;
				VectorLF3 vectorLF10 = ((int)this.muzzleIndex0 == craftUnitMuzzleCount) ? pdesc.portPoses[0].position : pdesc.portPoses[(int)this.muzzleIndex0].position;
				vectorLF10 = Maths.QRotate(quaternion2, vectorLF10);
				vectorLF10 += vectorLF2;
				VectorLF3 vectorLF11;
				sector.TransformFromAstro_ref(astroId2, out vectorLF11, ref vectorLF10);
				Vector3 b;
				((astroId2 > 1000000) ? sector.astros : sector.galaxyAstros)[(astroId2 > 1000000) ? (astroId2 - 1000000) : astroId2].VelocityL2U(ref vectorLF2, ref vector3, out b);
				SkillSystem skillSystem = sector.skillSystem;
				VectorLF3 vectorLF12;
				Vector3 a3;
				skillSystem.GetObjectUPositionAndVelocity(ref skillTarget, out vectorLF12, out a3);
				Vector3 vector4 = vectorLF12 - vectorLF11;
				ref EnemyData ptr3 = ref enemyPool[skillTarget.id];
				double num12 = (double)vector4.x;
				double num13 = (double)vector4.y;
				double num14 = (double)vector4.z;
				double d2 = num12 * num12 + num13 * num13 + num14 * num14;
				double num15 = Math.Sqrt(d2) * 5.0;
				if (num15 < 200.0)
				{
					num15 = 200.0;
				}
				else if (num15 > 20000.0)
				{
					num15 = 20000.0;
				}
				float num16 = (float)(Math.Sqrt(d2) / num15);
				if (ptr3.astroId > 1000000)
				{
					EnemyDFHiveSystem enemyDFHiveSystem = sector.dfHivesByAstro[ptr3.astroId - 1000000];
					Vector3 a4 = (enemyDFHiveSystem.starData.uPosition - vectorLF12).normalized;
					vector4 += a4 * enemyDFHiveSystem.hiveAstroOrbit.GetEstimatePointOffset(num16);
				}
				vector4 += a3 * num16;
				Vector3 normalized3 = vector4.normalized;
				if (!this.IsPlanetaryOcclusion(sector, astroId2, ref vectorLF12, ref craft))
				{
					if (this.CheckTargetValidBeforeShoot_Space(sector, skillTarget.id))
					{
						bool flag2 = true;
						int num17 = pdesc.craftUnitFireEnergy0;
						Mecha obj;
						if (ptr.owner < 0)
						{
							obj = mecha;
							lock (obj)
							{
								if (mecha.coreEnergy < (double)num17)
								{
									flag2 = false;
								}
							}
						}
						this.isShooting0 = ((this.isShooting0 ? (num6 < craftUnitAttackRange) : (num6 < craftUnitAttackRange * 0.8f)) && flag2);
						if (this.isShooting0)
						{
							this.anim = 1f - (float)this.fire0 / (float)pdesc.craftUnitRoundInterval0 * 2f;
							if (this.fire0 <= 0)
							{
								if ((int)this.muzzleIndex0 == craftUnitMuzzleCount)
								{
									this.muzzleIndex0 = 0;
								}
								ref GeneralProjectile ptr4 = ref skillSystem.warshipTypeFPlasmas.Add();
								ptr4.astroId = astroId2;
								ptr4.hitIndex = 26;
								ptr4.target.type = skillTarget.type;
								ptr4.target.id = skillTarget.id;
								ptr4.target.astroId = astroId;
								ptr4.caster.type = ETargetType.Craft;
								ptr4.caster.id = this.craftId;
								ptr4.caster.astroId = astroId2;
								ptr4.uPos = vectorLF11;
								ptr4.uVel = vector4 / num16;
								ptr4.uVelObj = ptr4.uVel - b;
								ptr4.damage = (int)((float)pdesc.craftUnitAttackDamage0 * combatUpgradeData.combatDroneDamageRatio * combatUpgradeData.energyDamageScale + 0.5f);
								ptr4.mask = ETargetTypeMask.Enemy;
								ptr4.life = 120;
								ptr4.lifemax = 120;
								ptr4.damageIncoming = skillSystem.CalculateDamageIncoming(ref ptr4.target, ptr4.damage, 1);
								int targetCombatStatId = skillSystem.AddCombatStatHPIncoming(ref ptr4.target, -ptr4.damageIncoming);
								ptr4.targetCombatStatId = targetCombatStatId;
								this.muzzleIndex0 += 1;
								if ((int)this.muzzleIndex0 >= craftUnitMuzzleCount)
								{
									this.fire0 += pdesc.craftUnitRoundInterval0;
								}
								else
								{
									this.fire0 += pdesc.craftUnitMuzzleInterval0;
								}
								EnemyDFHiveSystem hiveByAstroId = sector.GetHiveByAstroId(ptr3.originAstroId);
								if (hiveByAstroId != null && hiveByAstroId.hiveAstroId == ptr3.originAstroId)
								{
									skillSystem.AddSpaceEnemyHatred(hiveByAstroId, ref ptr3, ETargetType.Craft, astroId2, this.craftId);
								}
								if (ptr.owner < 0)
								{
									obj = mecha;
									lock (obj)
									{
										mecha.coreEnergy -= (double)num17;
										mecha.MarkEnergyChange(13, (double)(-(double)num17));
									}
								}
							}
						}
						flag2 = true;
						num17 = pdesc.craftUnitFireEnergy1;
						if (ptr.owner < 0)
						{
							obj = mecha;
							lock (obj)
							{
								if (mecha.coreEnergy < (double)num17)
								{
									flag2 = false;
								}
							}
						}
						this.isShooting1 = ((this.isShooting1 ? (num6 < craftUnitAttackRange2) : (num6 < craftUnitAttackRange2 * 0.8f)) && flag2 && Vector3.Dot(craft.rot.Forward(), vectorLF3) > 0.9961947f);
						if (!this.isShooting1)
						{
							goto IL_1063;
						}
						int craftUnitMuzzleCount2 = pdesc.craftUnitMuzzleCount1;
						this.anim = 1f - (float)this.fire1 / (float)pdesc.craftUnitRoundInterval1 * 2f;
						SkillTargetLocal skillTargetLocal = this.hatred.max.skillTargetLocal;
						if (this.fire1 > 0 || !flag2)
						{
							goto IL_1063;
						}
						if ((int)this.muzzleIndex1 == craftUnitMuzzleCount2)
						{
							this.muzzleIndex1 = 0;
						}
						VectorLF3 vectorLF13 = new VectorLF3(0f, 2f, -5f);
						vectorLF13 = Maths.QRotate(quaternion2, vectorLF13);
						vectorLF13 += vectorLF2;
						if (vectorLF11.sqrMagnitude == 0.0)
						{
							sector.TransformFromAstro_ref(astroId2, out vectorLF11, ref vectorLF13);
						}
						ref SpaceLaserOneShot ptr5 = ref sector.skillSystem.warshipTypeFLasers.Add();
						ptr5.astroId = astroId2;
						ptr5.hitIndex = 25;
						ptr5.beginPosU = vectorLF11;
						ptr5.target = default(SkillTarget);
						ptr5.target.id = skillTargetLocal.id;
						ptr5.target.type = skillTargetLocal.type;
						ptr5.target.astroId = astroId;
						ptr5.endPosU = endPosU;
						ptr5.damage = (int)((float)pdesc.craftUnitAttackDamage1 * combatUpgradeData.combatDroneDamageRatio * combatUpgradeData.energyDamageScale + 0.5f);
						ptr5.mask = ETargetTypeMask.Enemy;
						ptr5.caster.type = ETargetType.Craft;
						ptr5.caster.id = this.craftId;
						ptr5.caster.astroId = astroId2;
						ptr5.life = 15;
						ptr5.extendedDistWhenMiss = 10;
						this.muzzleIndex1 += 1;
						if ((int)this.muzzleIndex1 >= craftUnitMuzzleCount2)
						{
							this.fire1 += pdesc.craftUnitRoundInterval1;
						}
						else
						{
							this.fire1 += pdesc.craftUnitMuzzleInterval1;
						}
						EnemyDFHiveSystem hiveByAstroId2 = sector.GetHiveByAstroId(ptr3.originAstroId);
						if (hiveByAstroId2 != null && hiveByAstroId2.hiveAstroId == ptr3.originAstroId)
						{
							skillSystem.AddSpaceEnemyHatred(hiveByAstroId2, ref ptr3, ETargetType.Craft, astroId2, this.craftId);
						}
						if (ptr.owner >= 0)
						{
							goto IL_1063;
						}
						obj = mecha;
						lock (obj)
						{
							mecha.coreEnergy -= (double)num17;
							mecha.MarkEnergyChange(13, (double)(-(double)num17));
							goto IL_1063;
						}
					}
					this.RunBehavior_Engage_EmptyHatred(ref craft);
				}
			}
			else
			{
				if (flag && !this.isRetreating)
				{
					this.hatred.ClearMax();
				}
				this.isRetreating = true;
				VectorLF3 pos = craftPool[craft.owner].pos;
				VectorLF3 normalized4 = (craft.pos - pos).normalized;
				vectorLF = normalized4 * (double)num * 0.5 + pos;
				quaternion = Quaternion.LookRotation(-normalized4, craft.pos);
				vector = craft.vel;
				ref VectorLF3 ptr6 = ref craft.pos;
				ref Quaternion ptr7 = ref craft.rot;
				ref Vector3 ptr8 = ref craft.vel;
				Vector3 vector5 = new Vector3((float)(vectorLF.x - ptr6.x), (float)(vectorLF.y - ptr6.y), (float)(vectorLF.z - ptr6.z));
				float num18 = Mathf.Sqrt(vector5.x * vector5.x + vector5.y * vector5.y + vector5.z * vector5.z);
				float num19 = 0.016666668f;
				float magnitude2 = ptr8.magnitude;
				bool flag4 = false;
				float num20 = 0.1f;
				if (num20 < 0.1f)
				{
					num20 = 0.1f;
				}
				if (num18 > num20)
				{
					Vector3 rhs2 = ptr7.Forward();
					float num21 = Vector3.Dot(quaternion.Forward(), rhs2);
					bool astroAvoidanceTargetPos2;
					Vector3 a5;
					if (Vector3.Dot(vector5, rhs2) > 0f && num21 < -0.95f)
					{
						VectorLF3 vectorLF14;
						sector.TransformFromAstro_ref(astroId2, out vectorLF14, ref vectorLF);
						astroAvoidanceTargetPos2 = this.GetAstroAvoidanceTargetPos(sector, craftUnitMaxMovementSpeed, 0, ref vectorLF14, ref craft);
						sector.InverseTransformToAstro_ref(astroId2, ref vectorLF14, out vectorLF);
						a5 = this.Arrive(ref vectorLF, ref craft, ref vector, craftUnitMaxMovementSpeed, astroAvoidanceTargetPos2, 3000f);
					}
					else
					{
						Vector3 vec = vector;
						float num22 = vector5.magnitude / (craftUnitMaxMovementSpeed + vec.magnitude);
						VectorLF3 vectorLF15 = vectorLF + vec * (double)num22;
						VectorLF3 vectorLF16;
						sector.TransformFromAstro_ref(astroId2, out vectorLF16, ref vectorLF15);
						astroAvoidanceTargetPos2 = this.GetAstroAvoidanceTargetPos(sector, craftUnitMaxMovementSpeed, 0, ref vectorLF16, ref craft);
						sector.InverseTransformToAstro_ref(astroId2, ref vectorLF16, out vectorLF15);
						a5 = this.Arrive(ref vectorLF15, ref craft, ref vector, craftUnitMaxMovementSpeed, astroAvoidanceTargetPos2, 3000f);
					}
					if (a5.sqrMagnitude > num4 || astroAvoidanceTargetPos2)
					{
						a5 = a5.normalized * craftUnitMaxMovementAcceleration;
					}
					ptr8 += a5 * num19;
				}
				else
				{
					ptr8 = vector;
					flag4 = true;
				}
				if (ptr8.sqrMagnitude > num5)
				{
					ptr8 = ptr8.normalized * craftUnitMaxMovementSpeed;
				}
				float num23 = Quaternion.Angle(ptr7, quaternion) / 180f;
				float num24 = craftUnitMaxRotateAcceleration * (num23 * num23);
				if (num24 < 0.1f)
				{
					num24 = 0.1f;
				}
				ptr7 = Quaternion.RotateTowards(ptr7, quaternion, num24);
				ptr6 += ptr8 * (double)num19;
				craft.pos = ptr6;
				craft.rot = ptr7;
				craft.vel = ptr8;
				if (flag4)
				{
					this.hatred.Reset();
					this.isRetreating = false;
				}
				this.anim = 0f;
				this.steering = 0f;
				this.speed = magnitude2;
			}
			IL_1063:
			this.steering = 0f;
			return;
		}
		this.RunBehavior_Engage_EmptyHatred(ref craft);
	}

	// Token: 0x06000F6E RID: 3950 RVA: 0x000FDBA8 File Offset: 0x000FBDA8
	public void RunBehavior_Engage_SAttackPlasma_Small(SpaceSector sector, Mecha mecha, PrefabDesc pdesc, ref CraftData craft, ref CombatSettings combatSettings, ref CombatUpgradeData combatUpgradeData)
	{
		VectorLF3 vectorLF = new VectorLF3(0f, 0f, 0f);
		Quaternion quaternion = new Quaternion(0f, 0f, 0f, 1f);
		Vector3 vector = new VectorLF3(0f, 0f, 0f);
		Vector3 vector2 = default(Vector3);
		int astroId;
		if (this.GetTargetPosition_Space(sector, this.hatred.max.target, ref vectorLF, ref vector2, out astroId))
		{
			VectorLF3 vectorLF2 = vectorLF;
			int astroId2 = craft.astroId;
			sector.InverseTransformToAstro_ref(astroId2, ref vectorLF2, out vectorLF);
			float num = 35f;
			EnemyData[] enemyPool = sector.enemyPool;
			CraftData[] craftPool = sector.craftPool;
			int fleetId = craftPool[craft.owner].fleetId;
			ref CraftData ptr = ref craftPool[craft.owner];
			if (fleetId > 0)
			{
				PrefabDesc prefabDesc = PlanetFactory.PrefabDescByModelIndex[(int)ptr.modelIndex];
				float num2 = prefabDesc.fleetSensorRange;
				num = prefabDesc.fleetMaxActiveArea;
				if (this.hatred.max.value > 8000)
				{
					float num3 = (float)(enemyPool[this.hatred.max.objectId].pos - ptr.pos).magnitude;
					num2 += num3;
					num += num3;
				}
			}
			float craftUnitMaxMovementSpeed = pdesc.craftUnitMaxMovementSpeed;
			float craftUnitMaxMovementAcceleration = pdesc.craftUnitMaxMovementAcceleration;
			float craftUnitMaxRotateAcceleration = pdesc.craftUnitMaxRotateAcceleration;
			float num4 = craftUnitMaxMovementAcceleration * craftUnitMaxMovementAcceleration;
			float num5 = craftUnitMaxMovementSpeed * craftUnitMaxMovementSpeed;
			float craftUnitAttackRange = pdesc.craftUnitAttackRange0;
			float craftUnitAttackRange2 = pdesc.craftUnitAttackRange1;
			bool flag = (float)(craft.pos - ptr.pos).magnitude > num;
			if (!flag && !this.isRetreating)
			{
				VectorLF3 vectorLF3 = craft.pos;
				Quaternion quaternion2 = craft.rot;
				Vector3 vector3 = craft.vel;
				VectorLF3 vectorLF4 = new VectorLF3(vectorLF.x - vectorLF3.x, vectorLF.y - vectorLF3.y, vectorLF.z - vectorLF3.z);
				double magnitude = vectorLF4.magnitude;
				quaternion = Quaternion.LookRotation(vectorLF4 / magnitude, quaternion2.Up());
				float num6 = Mathf.Sqrt((float)(vectorLF4.x * vectorLF4.x) + (float)(vectorLF4.y * vectorLF4.y) + (float)(vectorLF4.z * vectorLF4.z));
				float num7 = 0.016666668f;
				float magnitude2 = vector3.magnitude;
				ref FleetComponent ptr2 = ref sector.combatSpaceSystem.fleets.buffer[fleetId];
				Vector3 point = default(Vector3);
				if (fleetId > 0)
				{
					ptr2.GetUnitFormOffset_Space((int)craft.port, mecha, ref craftPool[ptr2.craftId], ref point, ref combatUpgradeData);
				}
				vectorLF += quaternion * point;
				Vector3 rhs = quaternion2.Forward();
				Vector3.Dot(quaternion.Forward(), rhs);
				VectorLF3 vectorLF5 = new VectorLF3(vectorLF.x - vectorLF3.x, vectorLF.y - vectorLF3.y, vectorLF.z - vectorLF3.z);
				float num8 = craftUnitAttackRange * 0.2f;
				if (magnitude < (double)num8 && !this.adjustEngageRange)
				{
					this.adjustEngageRange = true;
				}
				else if (magnitude > (double)(craftUnitAttackRange * 0.8f))
				{
					this.adjustEngageRange = false;
				}
				if (this.adjustEngageRange)
				{
					if (fleetId > 0)
					{
						VectorLF3 lhs = default(VectorLF3);
						Quaternion quaternion3 = default(Quaternion);
						Vector3 vector4 = default(Vector3);
						ptr2.GetUnitPort_Space(mecha, ref ptr, ref craft, ref lhs, ref quaternion3, ref vector4, ref combatUpgradeData);
						VectorLF3 vectorLF6 = lhs - vectorLF;
						vectorLF5 = vectorLF + vectorLF6.normalized * (double)(craftUnitAttackRange + 200f);
					}
				}
				else
				{
					vectorLF5 = vectorLF;
					vector = vector2;
				}
				VectorLF3 vectorLF7;
				sector.TransformFromAstro_ref(astroId2, out vectorLF7, ref vectorLF5);
				bool astroAvoidanceTargetPos = this.GetAstroAvoidanceTargetPos(sector, craftUnitMaxMovementSpeed, 0, ref vectorLF7, ref craft);
				sector.InverseTransformToAstro_ref(astroId2, ref vectorLF7, out vectorLF5);
				Vector3 a = this.Arrive(ref vectorLF5, ref craft, ref vector, (magnitude < (double)craftUnitAttackRange && !this.adjustEngageRange) ? 500f : craftUnitMaxMovementSpeed, astroAvoidanceTargetPos, 3000f);
				if (a.sqrMagnitude > num4 || astroAvoidanceTargetPos)
				{
					a = a.normalized * craftUnitMaxMovementAcceleration;
				}
				vector3 += a * num7;
				Vector3 normalized = vector3.normalized;
				if (vector3.sqrMagnitude > num5)
				{
					vector3 = normalized * craftUnitMaxMovementSpeed;
				}
				if (vector3.sqrMagnitude > 2500f)
				{
					quaternion2 = Quaternion.Lerp(quaternion2, Quaternion.LookRotation(normalized, quaternion.Up()), num7 * craftUnitMaxRotateAcceleration * 0.2f);
				}
				else
				{
					float num9 = Quaternion.Angle(quaternion2, quaternion) / 180f;
					float num10 = (num6 < craftUnitAttackRange) ? (craftUnitMaxRotateAcceleration * num9 * 2f) : (craftUnitMaxRotateAcceleration * (num9 * num9));
					if (num10 < 0.1f)
					{
						num10 = 0.1f;
					}
					quaternion2 = Quaternion.RotateTowards(quaternion2, quaternion, num10);
				}
				vectorLF3 += vector3 * (double)num7;
				craft.pos = vectorLF3;
				craft.rot = quaternion2;
				craft.vel = vector3;
				this.anim = 0f;
				this.steering = 0f;
				this.speed = magnitude2;
				SkillTarget skillTarget = this.hatred.max.GetSkillTarget(0);
				int craftUnitMuzzleCount = pdesc.craftUnitMuzzleCount0;
				VectorLF3 vectorLF8 = ((int)this.muzzleIndex0 == craftUnitMuzzleCount) ? pdesc.portPoses[0].position : pdesc.portPoses[(int)this.muzzleIndex0].position;
				vectorLF8 = Maths.QRotate(quaternion2, vectorLF8);
				vectorLF8 += vectorLF3;
				VectorLF3 vectorLF9;
				sector.TransformFromAstro_ref(astroId2, out vectorLF9, ref vectorLF8);
				Vector3 b;
				((astroId2 > 1000000) ? sector.astros : sector.galaxyAstros)[(astroId2 > 1000000) ? (astroId2 - 1000000) : astroId2].VelocityL2U(ref vectorLF3, ref vector3, out b);
				SkillSystem skillSystem = sector.skillSystem;
				VectorLF3 vectorLF10;
				Vector3 a2;
				skillSystem.GetObjectUPositionAndVelocity(ref skillTarget, out vectorLF10, out a2);
				Vector3 vector5 = vectorLF10 - vectorLF9;
				ref EnemyData ptr3 = ref enemyPool[skillTarget.id];
				double num11 = (double)vector5.x;
				double num12 = (double)vector5.y;
				double num13 = (double)vector5.z;
				double d = num11 * num11 + num12 * num12 + num13 * num13;
				double num14 = Math.Sqrt(d) * 5.0;
				if (num14 < 200.0)
				{
					num14 = 200.0;
				}
				else if (this.speed > 20000f)
				{
					num14 = 20000.0;
				}
				float num15 = (float)(Math.Sqrt(d) / num14);
				if (ptr3.astroId > 1000000)
				{
					EnemyDFHiveSystem enemyDFHiveSystem = sector.dfHivesByAstro[ptr3.astroId - 1000000];
					Vector3 a3 = (enemyDFHiveSystem.starData.uPosition - vectorLF10).normalized;
					vector5 += a3 * enemyDFHiveSystem.hiveAstroOrbit.GetEstimatePointOffset(num15);
				}
				vector5 += a2 * num15;
				Vector3 normalized2 = vector5.normalized;
				if (this.IsPlanetaryOcclusion(sector, astroId2, ref vectorLF10, ref craft) || !this.CheckTargetValidBeforeShoot_Space(sector, skillTarget.id))
				{
					goto IL_D45;
				}
				bool flag2 = true;
				int craftUnitFireEnergy = pdesc.craftUnitFireEnergy0;
				Mecha obj;
				if (ptr.owner < 0)
				{
					obj = mecha;
					lock (obj)
					{
						if (mecha.coreEnergy < (double)craftUnitFireEnergy)
						{
							flag2 = false;
						}
					}
				}
				this.isShooting0 = ((this.isShooting0 ? (num6 < craftUnitAttackRange) : (num6 < craftUnitAttackRange * 0.8f)) && flag2 && Vector3.Dot(craft.rot.Forward(), normalized2) > 0.5f);
				if (!this.isShooting0)
				{
					goto IL_D45;
				}
				this.anim = 1f - (float)this.fire0 / (float)pdesc.craftUnitRoundInterval0 * 2f;
				if (this.fire0 > 0)
				{
					goto IL_D45;
				}
				if ((int)this.muzzleIndex0 == craftUnitMuzzleCount)
				{
					this.muzzleIndex0 = 0;
				}
				ref GeneralProjectile ptr4 = ref skillSystem.warshipTypeAPlasmas.Add();
				ptr4.astroId = astroId2;
				ptr4.hitIndex = 35;
				ptr4.target.type = skillTarget.type;
				ptr4.target.id = skillTarget.id;
				ptr4.target.astroId = astroId;
				ptr4.caster.type = ETargetType.Craft;
				ptr4.caster.id = this.craftId;
				ptr4.caster.astroId = astroId2;
				ptr4.uPos = vectorLF9;
				ptr4.uVel = vector5 / num15;
				ptr4.uVelObj = ptr4.uVel - b;
				ptr4.damage = (int)((float)pdesc.craftUnitAttackDamage0 * combatUpgradeData.combatDroneDamageRatio * combatUpgradeData.energyDamageScale + 0.5f);
				ptr4.mask = ETargetTypeMask.Enemy;
				ptr4.life = 120;
				ptr4.lifemax = 120;
				ptr4.damageIncoming = skillSystem.CalculateDamageIncoming(ref ptr4.target, ptr4.damage, 1);
				int targetCombatStatId = skillSystem.AddCombatStatHPIncoming(ref ptr4.target, -ptr4.damageIncoming);
				ptr4.targetCombatStatId = targetCombatStatId;
				this.muzzleIndex0 += 1;
				if ((int)this.muzzleIndex0 >= craftUnitMuzzleCount)
				{
					this.fire0 += pdesc.craftUnitRoundInterval0;
				}
				else
				{
					this.fire0 += pdesc.craftUnitMuzzleInterval0;
				}
				EnemyDFHiveSystem hiveByAstroId = sector.GetHiveByAstroId(ptr3.originAstroId);
				if (hiveByAstroId != null && hiveByAstroId.hiveAstroId == ptr3.originAstroId)
				{
					skillSystem.AddSpaceEnemyHatred(hiveByAstroId, ref ptr3, ETargetType.Craft, astroId2, this.craftId);
				}
				if (ptr.owner >= 0)
				{
					goto IL_D45;
				}
				obj = mecha;
				lock (obj)
				{
					mecha.coreEnergy -= (double)craftUnitFireEnergy;
					mecha.MarkEnergyChange(13, (double)(-(double)craftUnitFireEnergy));
					goto IL_D45;
				}
			}
			if (flag && !this.isRetreating)
			{
				this.hatred.ClearMax();
			}
			this.isRetreating = true;
			VectorLF3 pos = craftPool[craft.owner].pos;
			VectorLF3 normalized3 = (craft.pos - pos).normalized;
			vectorLF = normalized3 * (double)num * 0.5 + pos;
			quaternion = Quaternion.LookRotation(-normalized3, craft.pos);
			vector = craft.vel;
			ref VectorLF3 ptr5 = ref craft.pos;
			ref Quaternion ptr6 = ref craft.rot;
			ref Vector3 ptr7 = ref craft.vel;
			Vector3 vector6 = new Vector3((float)(vectorLF.x - ptr5.x), (float)(vectorLF.y - ptr5.y), (float)(vectorLF.z - ptr5.z));
			float num16 = Mathf.Sqrt(vector6.x * vector6.x + vector6.y * vector6.y + vector6.z * vector6.z);
			float num17 = 0.016666668f;
			float magnitude3 = ptr7.magnitude;
			bool flag4 = false;
			float num18 = 0.1f;
			if (num18 < 0.1f)
			{
				num18 = 0.1f;
			}
			if (num16 > num18)
			{
				Vector3 rhs2 = ptr6.Forward();
				float num19 = Vector3.Dot(quaternion.Forward(), rhs2);
				bool astroAvoidanceTargetPos2;
				Vector3 a4;
				if (Vector3.Dot(vector6, rhs2) > 0f && num19 < -0.95f)
				{
					VectorLF3 vectorLF11;
					sector.TransformFromAstro_ref(astroId2, out vectorLF11, ref vectorLF);
					astroAvoidanceTargetPos2 = this.GetAstroAvoidanceTargetPos(sector, craftUnitMaxMovementSpeed, 0, ref vectorLF11, ref craft);
					sector.InverseTransformToAstro_ref(astroId2, ref vectorLF11, out vectorLF);
					a4 = this.Arrive(ref vectorLF, ref craft, ref vector, craftUnitMaxMovementSpeed, astroAvoidanceTargetPos2, 3000f);
				}
				else
				{
					Vector3 vec = vector;
					float num20 = vector6.magnitude / (craftUnitMaxMovementSpeed + vec.magnitude);
					VectorLF3 vectorLF12 = vectorLF + vec * (double)num20;
					VectorLF3 vectorLF13;
					sector.TransformFromAstro_ref(astroId2, out vectorLF13, ref vectorLF12);
					astroAvoidanceTargetPos2 = this.GetAstroAvoidanceTargetPos(sector, craftUnitMaxMovementSpeed, 0, ref vectorLF13, ref craft);
					sector.InverseTransformToAstro_ref(astroId2, ref vectorLF13, out vectorLF12);
					a4 = this.Arrive(ref vectorLF12, ref craft, ref vector, craftUnitMaxMovementSpeed, astroAvoidanceTargetPos2, 3000f);
				}
				if (a4.sqrMagnitude > num4 || astroAvoidanceTargetPos2)
				{
					a4 = a4.normalized * craftUnitMaxMovementAcceleration;
				}
				ptr7 += a4 * num17;
			}
			else
			{
				ptr7 = vector;
				flag4 = true;
			}
			if (ptr7.sqrMagnitude > num5)
			{
				ptr7 = ptr7.normalized * craftUnitMaxMovementSpeed;
			}
			float num21 = Quaternion.Angle(ptr6, quaternion) / 180f;
			float num22 = craftUnitMaxRotateAcceleration * (num21 * num21);
			if (num22 < 0.1f)
			{
				num22 = 0.1f;
			}
			ptr6 = Quaternion.RotateTowards(ptr6, quaternion, num22);
			ptr5 += ptr7 * (double)num17;
			craft.pos = ptr5;
			craft.rot = ptr6;
			craft.vel = ptr7;
			if (flag4)
			{
				this.hatred.Reset();
				this.isRetreating = false;
			}
			this.anim = 0f;
			this.steering = 0f;
			this.speed = magnitude3;
			IL_D45:
			this.steering = 0f;
			return;
		}
		this.RunBehavior_Engage_EmptyHatred(ref craft);
	}

	// Token: 0x06000F6F RID: 3951 RVA: 0x000FE92C File Offset: 0x000FCB2C
	public void RunBehavior_Orbiting(SpaceSector sector, Mecha mecha, PrefabDesc pdesc, ref CraftData craft, ref CombatUpgradeData combatUpgradeData, int orbitAstroId)
	{
		if (this.OrbitingFlightLogic(sector, mecha, pdesc, ref craft, ref combatUpgradeData, orbitAstroId))
		{
			CraftData[] craftPool = sector.craftPool;
			int fleetId = craftPool[craft.owner].fleetId;
			ref FleetComponent ptr = ref sector.combatSpaceSystem.fleets.buffer[fleetId];
			ref CombatStat ptr2 = ref sector.skillSystem.combatStats.buffer[craft.combatStatId];
			if (craft.combatStatId == 0 || ptr2.hp == ptr2.hpMax)
			{
				if (this.isCharging)
				{
					this.isCharging = false;
					ref FleetComponent ptr3 = ref ptr;
					ptr3.currentChargingUnitsCount -= 1;
				}
				if (!ptr.dispatch)
				{
					this.behavior = EUnitBehavior.Recycled;
					return;
				}
			}
			else if (ptr2.hp > 0)
			{
				if (this.isCharging)
				{
					int num = (int)((float)pdesc.craftUnitRepairHPPerTick * (1f + sector.gameData.history.globalHpEnhancement) + 0.1f);
					ptr2.hp += num;
					if (ptr.owner < 0)
					{
						Mecha obj = mecha;
						lock (obj)
						{
							int num2 = num * pdesc.craftUnitRepairEnergyPerHP;
							mecha.coreEnergy -= (double)num2;
							mecha.MarkEnergyChange(13, (double)(-(double)num2));
						}
						if (mecha.coreEnergy <= 0.0)
						{
							mecha.coreEnergy = 0.0;
							return;
						}
					}
				}
				else if ((int)ptr.currentChargingUnitsCount < PlanetFactory.PrefabDescByModelIndex[(int)craftPool[craft.owner].modelIndex].fleetMaxChargingCount)
				{
					ref FleetComponent ptr4 = ref ptr;
					ptr4.currentChargingUnitsCount += 1;
					this.isCharging = true;
					int num3 = (int)((float)pdesc.craftUnitRepairHPPerTick * (1f + sector.gameData.history.globalHpEnhancement) + 0.1f);
					ptr2.hp += num3;
					if (ptr.owner < 0)
					{
						Mecha obj = mecha;
						lock (obj)
						{
							int num4 = num3 * pdesc.craftUnitRepairEnergyPerHP;
							mecha.coreEnergy -= (double)num4;
							mecha.MarkEnergyChange(13, (double)(-(double)num4));
						}
						if (mecha.coreEnergy <= 0.0)
						{
							mecha.coreEnergy = 0.0;
						}
					}
				}
			}
		}
	}

	// Token: 0x06000F70 RID: 3952 RVA: 0x000FEB9C File Offset: 0x000FCD9C
	public bool OrbitingFlightLogic(SpaceSector sector, Mecha mecha, PrefabDesc pdesc, ref CraftData craft, ref CombatUpgradeData combatUpgradeData, int orbitAstroId)
	{
		VectorLF3 vectorLF = new VectorLF3(0.0, 0.0, 0.0);
		Quaternion quaternion = new Quaternion(0f, 0f, 0f, 1f);
		Vector3 vector = new Vector3(0f, 0f, 0f);
		float craftUnitMaxMovementSpeed = pdesc.craftUnitMaxMovementSpeed;
		float craftUnitMaxMovementAcceleration = pdesc.craftUnitMaxMovementAcceleration;
		float craftUnitMaxRotateAcceleration = pdesc.craftUnitMaxRotateAcceleration;
		float num = craftUnitMaxMovementAcceleration * craftUnitMaxMovementAcceleration;
		float num2 = craftUnitMaxMovementSpeed * craftUnitMaxMovementSpeed;
		CraftData[] craftPool = sector.craftPool;
		int fleetId = craftPool[craft.owner].fleetId;
		ref FleetComponent ptr = ref sector.combatSpaceSystem.fleets.buffer[fleetId];
		if (fleetId > 0 && fleetId == ptr.id)
		{
			ref CraftData ptr2 = ref craftPool[ptr.craftId];
			ptr.GetUnitOrbitingAstroPose(sector, mecha, orbitAstroId, (int)ptr2.port, (int)craft.port, UnitComponent.gameTick, ref vectorLF, ref quaternion, ref combatUpgradeData);
			sector.galaxyAstros[orbitAstroId].VelocityU(ref vectorLF, out vector);
			VectorLF3 vectorLF2;
			Quaternion quaternion2;
			sector.TransformFromAstro_ref(orbitAstroId, out vectorLF2, out quaternion2, ref vectorLF, ref quaternion);
			bool astroAvoidanceTargetPos = this.GetAstroAvoidanceTargetPos(sector, craftUnitMaxMovementSpeed, orbitAstroId, ref vectorLF2, ref craft);
			sector.InverseTransformToAstro_ref(craft.astroId, ref vectorLF2, ref quaternion2, out vectorLF, out quaternion);
			VectorLF3 vectorLF3 = craft.pos;
			Quaternion quaternion3 = craft.rot;
			Vector3 vector2 = craft.vel;
			Vector3 vector3 = new Vector3((float)(vectorLF.x - vectorLF3.x), (float)(vectorLF.y - vectorLF3.y), (float)(vectorLF.z - vectorLF3.z));
			float num3 = Mathf.Sqrt(vector3.x * vector3.x + vector3.y * vector3.y + vector3.z * vector3.z);
			float num4 = 0.016666668f;
			float magnitude = vector2.magnitude;
			float magnitude2 = vector.magnitude;
			bool flag = false;
			bool flag2 = false;
			float num5 = 0.1f;
			if (num5 < 0.1f)
			{
				num5 = 0.1f;
			}
			if (num3 > num5)
			{
				Vector3 a = (num3 < 1000f && magnitude - magnitude2 < 100f && !astroAvoidanceTargetPos) ? this.Arrive_Orbiting(ref vectorLF, ref craft, ref vector, craftUnitMaxMovementSpeed, astroAvoidanceTargetPos) : this.Arrive(ref vectorLF, ref craft, ref vector, craftUnitMaxMovementSpeed, astroAvoidanceTargetPos, 3000f);
				if (a.sqrMagnitude > num || astroAvoidanceTargetPos)
				{
					a = a.normalized * craftUnitMaxMovementAcceleration;
				}
				vector2 += a * num4;
			}
			else
			{
				vector2 = vector;
			}
			if (num3 < 150f && vector2.magnitude - magnitude2 < 100f)
			{
				flag = true;
			}
			if (vector2.sqrMagnitude > num2)
			{
				vector2 = vector2.normalized * craftUnitMaxMovementSpeed;
			}
			float num6 = Quaternion.Angle(quaternion3, quaternion) / 180f;
			float num7 = craftUnitMaxRotateAcceleration * num6;
			if (num7 < 0.1f)
			{
				num7 = 0.1f;
			}
			quaternion3 = Quaternion.RotateTowards(quaternion3, quaternion, num7);
			if (Quaternion.Angle(quaternion3, quaternion) < 5f && num7 < 1.5f)
			{
				flag2 = true;
			}
			vectorLF3 += vector2 * (double)num4;
			craft.pos = vectorLF3;
			craft.rot = quaternion3;
			craft.vel = vector2;
			this.anim = 0f;
			this.steering = 0f;
			this.speed = magnitude;
			return flag && flag2;
		}
		return false;
	}

	// Token: 0x06000F71 RID: 3953 RVA: 0x000FEF0C File Offset: 0x000FD10C
	public void RunBehavior_Recycled_Ground(PlanetFactory factory, Mecha mecha, PrefabDesc pdesc, ref CraftData craft, ref CombatSettings combatSettings, ref CombatUpgradeData combatUpgradeData, bool isCompulsory)
	{
		if (!isCompulsory)
		{
			VectorLF3 vectorLF = new VectorLF3(0.0, 0.0, 0.0);
			Quaternion quaternion = new Quaternion(0f, 0f, 0f, 1f);
			Vector3 vector = new Vector3(0f, 0f, 0f);
			float craftUnitMaxMovementSpeed = pdesc.craftUnitMaxMovementSpeed;
			float num = pdesc.craftUnitMaxMovementAcceleration;
			CraftData[] craftPool = factory.craftPool;
			ref CraftData ptr = ref craftPool[craft.owner];
			int fleetId = ptr.fleetId;
			ref FleetComponent ptr2 = ref factory.combatGroundSystem.fleets.buffer[fleetId];
			if (fleetId > 0)
			{
				ptr2.GetUnitPort_Ground(UnitComponent.gameTick, mecha, ref craftPool[ptr2.craftId], ref craft, ref vectorLF, ref quaternion, factory, ref combatUpgradeData);
			}
			ref VectorLF3 ptr3 = ref craft.pos;
			ref Quaternion ptr4 = ref craft.rot;
			ref Vector3 ptr5 = ref craft.vel;
			Vector3 vector2 = new Vector3((float)(vectorLF.x - ptr3.x), (float)(vectorLF.y - ptr3.y), (float)(vectorLF.z - ptr3.z));
			float num2 = Mathf.Sqrt(vector2.x * vector2.x + vector2.y * vector2.y + vector2.z * vector2.z);
			float magnitude = ptr5.magnitude;
			float num3 = (magnitude > 2f) ? magnitude : 2f;
			float num4 = num2 / num3;
			num4 -= 0.016666668f;
			if (num4 < 0f)
			{
				num4 = 0f;
			}
			float num5 = ((num4 > 1f) ? 1f : num4) * 0.3f;
			Vector3 vector3 = new Vector3((float)vectorLF.x - vector.x * num5, (float)vectorLF.y - vector.y * num5, (float)vectorLF.z - vector.z * num5);
			Vector3 vector4 = new Vector3(vector3.x - (float)ptr3.x, vector3.y - (float)ptr3.y, vector3.z - (float)ptr3.z);
			float num6 = Mathf.Sqrt(vector4.x * vector4.x + vector4.y * vector4.y + vector4.z * vector4.z);
			if (num6 > 0f)
			{
				vector4.x /= num6;
				vector4.y /= num6;
				vector4.z /= num6;
			}
			float num7 = (num4 > 2f) ? ((num4 > 6f) ? 6f : num4) : 2f;
			num7 /= 2f;
			num /= num7;
			float num8 = 0f;
			float num9 = num4 / 0.5f - 0.02f;
			if (num9 <= 0f)
			{
				ptr5.x = vector.x;
				ptr5.y = vector.y;
				ptr5.z = vector.z;
			}
			else
			{
				Vector3 vector5 = new Vector3(vector4.x * craftUnitMaxMovementSpeed, vector4.y * craftUnitMaxMovementSpeed, vector4.z * craftUnitMaxMovementSpeed);
				if (num9 < 1f)
				{
					float num10 = 1f - num9;
					vector5.x = vector5.x * num9 + vector.x * num10;
					vector5.y = vector5.y * num9 + vector.y * num10;
					vector5.z = vector5.z * num9 + vector.z * num10;
				}
				Vector3 vector6 = new Vector3(vector5.x - ptr5.x, vector5.y - ptr5.y, vector5.z - ptr5.z);
				num8 = Mathf.Sqrt(vector6.x * vector6.x + vector6.y * vector6.y + vector6.z * vector6.z);
				float num11 = num8 / (num * 0.016666668f);
				if (num11 > 1f)
				{
					vector6.x /= num11;
					vector6.y /= num11;
					vector6.z /= num11;
				}
				ptr5.x += vector6.x;
				ptr5.y += vector6.y;
				ptr5.z += vector6.z;
				float num12 = (num6 - 2f) / 15f;
				if (num12 > 0f)
				{
					float num13 = (float)(ptr3.x * ptr3.x + ptr3.y * ptr3.y + ptr3.z * ptr3.z);
					float num14 = vector3.x * vector3.x + vector3.y * vector3.y + vector3.z * vector3.z - num13;
					float num15 = Mathf.Sqrt(num13);
					Vector3 vector7 = new Vector3((float)ptr3.x / num15, (float)ptr3.y / num15, (float)ptr3.z / num15);
					float num16 = ptr5.x * vector7.x + ptr5.y * vector7.y + ptr5.z * vector7.z;
					if (num16 * num14 < 0f)
					{
						if (num12 >= 1f)
						{
							ptr5.x -= vector7.x * num16;
							ptr5.y -= vector7.y * num16;
							ptr5.z -= vector7.z * num16;
						}
						else
						{
							num12 *= num12;
							num16 *= num12;
							ptr5.x -= vector7.x * num16;
							ptr5.y -= vector7.y * num16;
							ptr5.z -= vector7.z * num16;
						}
					}
				}
			}
			ptr3.x += (double)ptr5.x * 0.016666666666666666;
			ptr3.y += (double)ptr5.y * 0.016666666666666666;
			ptr3.z += (double)ptr5.z * 0.016666666666666666;
			bool flag = false;
			bool flag2 = false;
			float num17 = num4 + num8 / num;
			float num18 = num17 / 0.15f - 0.04f;
			if (num18 < 1f)
			{
				if (num18 <= 0f)
				{
					ptr3.x = vectorLF.x;
					ptr3.y = vectorLF.y;
					ptr3.z = vectorLF.z;
					flag = true;
				}
				else
				{
					float num19 = 1f - num18;
					ptr3.x = ptr3.x * (double)num18 + vectorLF.x * (double)num19;
					ptr3.y = ptr3.y * (double)num18 + vectorLF.y * (double)num19;
					ptr3.z = ptr3.z * (double)num18 + vectorLF.z * (double)num19;
				}
			}
			float num20 = num17 / 0.65f - 0.04f;
			if (num20 <= 0f)
			{
				ptr4 = quaternion;
				flag2 = true;
			}
			else
			{
				if (ptr5.x * ptr5.x + ptr5.y * ptr5.y + ptr5.z * ptr5.z > 0.01f)
				{
					Quaternion b = Quaternion.LookRotation(ptr5, ptr3);
					ptr4 = Quaternion.Slerp(ptr4, b, 0.1f);
				}
				if (num20 < 1f)
				{
					ptr4 = Quaternion.Slerp(quaternion, ptr4, num20 * num20);
				}
			}
			if (flag && flag2)
			{
				ref CombatStat ptr6 = ref factory.skillSystem.combatStats.buffer[craft.combatStatId];
				if (craft.combatStatId == 0 || ptr6.hp >= ptr6.hpMax)
				{
					if (this.currentInitializeValue > 0)
					{
						this.currentInitializeValue -= pdesc.craftUnitInitializeSpeed;
						factory.craftAnimPool[this.craftId].time = (float)this.currentInitializeValue / 600000f;
					}
					else
					{
						factory.RemoveCraftDeferred(this.craftId);
						this.currentInitializeValue = 0;
						factory.craftAnimPool[this.craftId].time = 0f;
					}
					if (this.isCharging)
					{
						this.isCharging = false;
						ref FleetComponent ptr7 = ref ptr2;
						ptr7.currentChargingUnitsCount -= 1;
					}
				}
				else if (ptr6.hp > 0)
				{
					if (this.isCharging)
					{
						int num21 = (int)((float)pdesc.craftUnitRepairHPPerTick * (1f + factory.gameData.history.globalHpEnhancement) + 0.1f);
						ptr6.hp += num21;
						if (ptr2.owner < 0)
						{
							Mecha obj = mecha;
							lock (obj)
							{
								int num22 = num21 * pdesc.craftUnitRepairEnergyPerHP;
								mecha.coreEnergy -= (double)num22;
								mecha.MarkEnergyChange(12, (double)(-(double)num22));
							}
							if (mecha.coreEnergy <= 0.0)
							{
								mecha.coreEnergy = 0.0;
							}
						}
						else if (ptr2.owner > 0)
						{
							ref EntityData ptr8 = ref factory.entityPool[ptr2.owner];
							if (ptr8.battleBaseId > 0)
							{
								BattleBaseComponent battleBaseComponent = factory.defenseSystem.battleBases.buffer[ptr8.battleBaseId];
								if (battleBaseComponent != null && battleBaseComponent.id == ptr8.battleBaseId)
								{
									int num23 = num21 * pdesc.craftUnitRepairEnergyPerHP;
									battleBaseComponent.energy -= (long)num23;
									if (battleBaseComponent.energy <= 0L)
									{
										battleBaseComponent.energy = 0L;
									}
								}
							}
						}
					}
					else if ((int)ptr2.currentChargingUnitsCount < PlanetFactory.PrefabDescByModelIndex[(int)ptr.modelIndex].fleetMaxChargingCount)
					{
						ref FleetComponent ptr9 = ref ptr2;
						ptr9.currentChargingUnitsCount += 1;
						this.isCharging = true;
						int num24 = (int)((float)pdesc.craftUnitRepairHPPerTick * (1f + factory.gameData.history.globalHpEnhancement) + 0.1f);
						ptr6.hp += num24;
						if (ptr2.owner < 0)
						{
							Mecha obj = mecha;
							lock (obj)
							{
								int num25 = num24 * pdesc.craftUnitRepairEnergyPerHP;
								mecha.coreEnergy -= (double)num25;
								mecha.MarkEnergyChange(12, (double)(-(double)num25));
							}
							if (mecha.coreEnergy <= 0.0)
							{
								mecha.coreEnergy = 0.0;
							}
						}
						else if (ptr2.owner > 0)
						{
							ref EntityData ptr10 = ref factory.entityPool[ptr2.owner];
							if (ptr10.battleBaseId > 0)
							{
								BattleBaseComponent battleBaseComponent2 = factory.defenseSystem.battleBases.buffer[ptr10.battleBaseId];
								if (battleBaseComponent2 != null && battleBaseComponent2.id == ptr10.battleBaseId)
								{
									int num26 = num24 * pdesc.craftUnitRepairEnergyPerHP;
									battleBaseComponent2.energy -= (long)num26;
									if (battleBaseComponent2.energy <= 0L)
									{
										battleBaseComponent2.energy = 0L;
									}
								}
							}
						}
					}
				}
			}
			this.anim = 0f;
			this.steering = 0f;
			this.speed = magnitude;
			return;
		}
		if (this.currentInitializeValue > 0)
		{
			this.currentInitializeValue -= pdesc.craftUnitInitializeSpeed;
			factory.craftAnimPool[this.craftId].time = (float)this.currentInitializeValue / 600000f;
		}
		else
		{
			factory.RemoveCraftDeferred(this.craftId);
			this.currentInitializeValue = 0;
			factory.craftAnimPool[this.craftId].time = 0f;
		}
		if (this.isCharging)
		{
			this.isCharging = false;
			int fleetId2 = factory.craftPool[craft.owner].fleetId;
			FleetComponent[] buffer = factory.combatGroundSystem.fleets.buffer;
			int num27 = fleetId2;
			buffer[num27].currentChargingUnitsCount = buffer[num27].currentChargingUnitsCount - 1;
		}
	}

	// Token: 0x06000F72 RID: 3954 RVA: 0x000FFB60 File Offset: 0x000FDD60
	public void RunBehavior_Recycled_Space(SpaceSector sector, Mecha mecha, PrefabDesc pdesc, ref CraftData craft, ref CombatSettings combatSettings, ref CombatUpgradeData combatUpgradeData, int orbitAstroId, bool isCompulsory)
	{
		VectorLF3 vectorLF = new VectorLF3(0.0, 0.0, 0.0);
		Quaternion quaternion = new Quaternion(0f, 0f, 0f, 1f);
		Vector3 vector = new Vector3(0f, 0f, 0f);
		float craftUnitMaxMovementSpeed = pdesc.craftUnitMaxMovementSpeed;
		float craftUnitMaxMovementAcceleration = pdesc.craftUnitMaxMovementAcceleration;
		float craftUnitMaxRotateAcceleration = pdesc.craftUnitMaxRotateAcceleration;
		float num = craftUnitMaxMovementAcceleration * craftUnitMaxMovementAcceleration;
		float num2 = craftUnitMaxMovementSpeed * craftUnitMaxMovementSpeed;
		ref CraftData ptr = ref sector.craftPool[craft.owner];
		int fleetId = ptr.fleetId;
		ref FleetComponent ptr2 = ref sector.combatSpaceSystem.fleets.buffer[fleetId];
		bool flag = orbitAstroId > 0;
		bool flag2 = false;
		if (fleetId > 0)
		{
			if (flag)
			{
				ptr2.GetUnitOrbitingAstroPose(sector, mecha, orbitAstroId, (int)ptr.port, (int)craft.port, UnitComponent.gameTick, ref vectorLF, ref quaternion, ref combatUpgradeData);
				sector.galaxyAstros[orbitAstroId].VelocityU(ref vectorLF, out vector);
				VectorLF3 vectorLF2;
				Quaternion quaternion2;
				sector.TransformFromAstro_ref(orbitAstroId, out vectorLF2, out quaternion2, ref vectorLF, ref quaternion);
				flag2 = this.GetAstroAvoidanceTargetPos(sector, craftUnitMaxMovementSpeed, orbitAstroId, ref vectorLF2, ref craft);
				sector.InverseTransformToAstro_ref(craft.astroId, ref vectorLF2, ref quaternion2, out vectorLF, out quaternion);
			}
			else
			{
				ptr2.GetUnitPort_Space(mecha, ref ptr, ref craft, ref vectorLF, ref quaternion, ref vector, ref combatUpgradeData);
			}
		}
		VectorLF3 vectorLF3 = craft.pos;
		Quaternion quaternion3 = craft.rot;
		Vector3 vector2 = craft.vel;
		Vector3 vector3 = new Vector3((float)(vectorLF.x - vectorLF3.x), (float)(vectorLF.y - vectorLF3.y), (float)(vectorLF.z - vectorLF3.z));
		float num3 = Mathf.Sqrt(vector3.x * vector3.x + vector3.y * vector3.y + vector3.z * vector3.z);
		float num4 = 0.016666668f;
		float magnitude = vector2.magnitude;
		float magnitude2 = vector.magnitude;
		bool flag3 = false;
		bool flag4 = false;
		float num5 = 0.1f;
		if (num5 < 0.1f)
		{
			num5 = 0.1f;
		}
		if (num3 > num5)
		{
			if (flag)
			{
				Vector3 a = (num3 < 1000f && magnitude - magnitude2 < 100f && !flag2) ? this.Arrive_Orbiting(ref vectorLF, ref craft, ref vector, craftUnitMaxMovementSpeed, flag2) : this.Arrive(ref vectorLF, ref craft, ref vector, craftUnitMaxMovementSpeed, flag2, 3000f);
				if (a.sqrMagnitude > num || flag2)
				{
					a = a.normalized * craftUnitMaxMovementAcceleration;
				}
				vector2 += a * num4;
			}
			else
			{
				Vector3 rhs = quaternion3.Forward();
				float num6 = Vector3.Dot(quaternion.Forward(), rhs);
				Vector3 a2;
				if (Vector3.Dot(vector3, rhs) > 0f && num6 < -0.95f)
				{
					VectorLF3 vectorLF4;
					Quaternion quaternion4;
					sector.TransformFromAstro_ref(craft.astroId, out vectorLF4, out quaternion4, ref vectorLF, ref quaternion);
					flag2 = this.GetAstroAvoidanceTargetPos(sector, craftUnitMaxMovementSpeed, orbitAstroId, ref vectorLF4, ref craft);
					sector.InverseTransformToAstro_ref(craft.astroId, ref vectorLF4, ref quaternion4, out vectorLF, out quaternion);
					a2 = this.Arrive(ref vectorLF, ref craft, ref vector, craftUnitMaxMovementSpeed, flag2, 3000f);
				}
				else
				{
					Vector3 vec = vector;
					float num7 = vector3.magnitude / (craftUnitMaxMovementSpeed + vec.magnitude);
					VectorLF3 vectorLF5 = vectorLF + vec * (double)num7;
					VectorLF3 vectorLF6;
					Quaternion quaternion5;
					sector.TransformFromAstro_ref(craft.astroId, out vectorLF6, out quaternion5, ref vectorLF5, ref quaternion);
					flag2 = this.GetAstroAvoidanceTargetPos(sector, craftUnitMaxMovementSpeed, orbitAstroId, ref vectorLF6, ref craft);
					sector.InverseTransformToAstro_ref(craft.astroId, ref vectorLF6, ref quaternion5, out vectorLF5, out quaternion);
					a2 = this.Arrive(ref vectorLF5, ref craft, ref vector, craftUnitMaxMovementSpeed, flag2, 3000f);
				}
				if (a2.sqrMagnitude > num || flag2)
				{
					a2 = a2.normalized * craftUnitMaxMovementAcceleration;
				}
				vector2 += a2 * num4;
			}
		}
		else
		{
			vector2 = vector;
		}
		if (flag)
		{
			if (num3 < 150f && vector2.magnitude - magnitude2 < 100f)
			{
				flag3 = true;
			}
		}
		else if (num3 < 20f && (ptr.vel - vector2).sqrMagnitude < 400f)
		{
			flag3 = true;
		}
		if (vector2.sqrMagnitude > num2)
		{
			vector2 = vector2.normalized * craftUnitMaxMovementSpeed;
		}
		float num8 = Quaternion.Angle(quaternion3, quaternion) / 180f;
		float num9 = flag ? (craftUnitMaxRotateAcceleration * num8) : (craftUnitMaxRotateAcceleration * (num8 * num8));
		if (num9 < 0.1f)
		{
			num9 = 0.1f;
		}
		quaternion3 = Quaternion.RotateTowards(quaternion3, quaternion, num9);
		if (flag)
		{
			if (Quaternion.Angle(quaternion3, quaternion) < 5f && num9 < 1.5f)
			{
				flag4 = true;
			}
		}
		else if (Quaternion.Angle(quaternion3, quaternion) < 1f && num9 < 1f)
		{
			flag4 = true;
		}
		vectorLF3 += vector2 * (double)num4;
		craft.pos = vectorLF3;
		craft.rot = quaternion3;
		craft.vel = vector2;
		if (isCompulsory)
		{
			if (this.currentInitializeValue > 0)
			{
				this.currentInitializeValue -= pdesc.craftUnitInitializeSpeed;
				sector.craftAnimPool[this.craftId].time = (float)this.currentInitializeValue / 600000f;
			}
			else
			{
				sector.RemoveCraftDeferred(this.craftId);
				this.currentInitializeValue = 0;
				sector.craftAnimPool[this.craftId].time = 0f;
			}
			if (this.isCharging)
			{
				this.isCharging = false;
				ref FleetComponent ptr3 = ref ptr2;
				ptr3.currentChargingUnitsCount -= 1;
			}
		}
		else if ((flag3 && flag4) || this.currentInitializeValue < 600000)
		{
			ref CombatStat ptr4 = ref sector.skillSystem.combatStats.buffer[craft.combatStatId];
			if (craft.combatStatId == 0 || ptr4.hp >= ptr4.hpMax)
			{
				if (this.currentInitializeValue > 0)
				{
					this.currentInitializeValue -= pdesc.craftUnitInitializeSpeed;
					sector.craftAnimPool[this.craftId].time = (float)this.currentInitializeValue / 600000f;
				}
				else
				{
					sector.RemoveCraftDeferred(this.craftId);
					this.currentInitializeValue = 0;
					sector.craftAnimPool[this.craftId].time = 0f;
				}
				if (this.isCharging)
				{
					this.isCharging = false;
					ref FleetComponent ptr5 = ref ptr2;
					ptr5.currentChargingUnitsCount -= 1;
				}
			}
			else if (ptr4.hp > 0)
			{
				if (this.isCharging)
				{
					int num10 = (int)((float)pdesc.craftUnitRepairHPPerTick * (1f + sector.gameData.history.globalHpEnhancement) + 0.1f);
					ptr4.hp += num10;
					if (ptr2.owner < 0)
					{
						Mecha obj = mecha;
						lock (obj)
						{
							int num11 = num10 * pdesc.craftUnitRepairEnergyPerHP;
							mecha.coreEnergy -= (double)num11;
							mecha.MarkEnergyChange(13, (double)(-(double)num11));
						}
						if (mecha.coreEnergy <= 0.0)
						{
							mecha.coreEnergy = 0.0;
						}
					}
				}
				else if ((int)ptr2.currentChargingUnitsCount < SpaceSector.PrefabDescByModelIndex[(int)ptr.modelIndex].fleetMaxChargingCount)
				{
					ref FleetComponent ptr6 = ref ptr2;
					ptr6.currentChargingUnitsCount += 1;
					this.isCharging = true;
					int num12 = (int)((float)pdesc.craftUnitRepairHPPerTick * (1f + sector.gameData.history.globalHpEnhancement) + 0.1f);
					ptr4.hp += num12;
					if (ptr2.owner < 0)
					{
						Mecha obj = mecha;
						lock (obj)
						{
							int num13 = num12 * pdesc.craftUnitRepairEnergyPerHP;
							mecha.coreEnergy -= (double)num13;
							mecha.MarkEnergyChange(13, (double)(-(double)num13));
						}
						if (mecha.coreEnergy <= 0.0)
						{
							mecha.coreEnergy = 0.0;
						}
					}
				}
			}
		}
		this.anim = 0f;
		this.steering = 0f;
		this.speed = magnitude;
	}

	// Token: 0x06000F73 RID: 3955 RVA: 0x001003A0 File Offset: 0x000FE5A0
	public void RunBehavior_SeekForm_Ground(PlanetFactory factory, Mecha mecha, PrefabDesc pdesc, ref CraftData craft, ref CombatUpgradeData combatUpgradeData)
	{
		VectorLF3 vectorLF = new VectorLF3(0.0, 0.0, 0.0);
		Quaternion quaternion = new Quaternion(0f, 0f, 0f, 1f);
		Vector3 vector = new Vector3(0f, 0f, 0f);
		float craftUnitMaxMovementSpeed = pdesc.craftUnitMaxMovementSpeed;
		float num = pdesc.craftUnitMaxMovementAcceleration;
		CraftData[] craftPool = factory.craftPool;
		int fleetId = craftPool[craft.owner].fleetId;
		ref FleetComponent ptr = ref factory.combatGroundSystem.fleets.buffer[fleetId];
		if (fleetId > 0)
		{
			ptr.GetUnitPort_Ground(UnitComponent.gameTick, mecha, ref craftPool[ptr.craftId], ref craft, ref vectorLF, ref quaternion, factory, ref combatUpgradeData);
		}
		ref VectorLF3 ptr2 = ref craft.pos;
		ref Quaternion ptr3 = ref craft.rot;
		ref Vector3 ptr4 = ref craft.vel;
		Vector3 b;
		Vector3 rhs;
		ptr3.ForwardRight(out b, out rhs);
		Vector3 vector2 = new Vector3((float)(vectorLF.x - ptr2.x), (float)(vectorLF.y - ptr2.y), (float)(vectorLF.z - ptr2.z));
		float num2 = Mathf.Sqrt(vector2.x * vector2.x + vector2.y * vector2.y + vector2.z * vector2.z);
		float magnitude = ptr4.magnitude;
		float num3 = (magnitude > 2f) ? magnitude : 2f;
		float num4 = num2 / num3;
		num4 -= 0.016666668f;
		if (num4 < 0f)
		{
			num4 = 0f;
		}
		float num5 = ((num4 > 1f) ? 1f : num4) * 0.3f;
		Vector3 vector3 = new Vector3((float)vectorLF.x - vector.x * num5, (float)vectorLF.y - vector.y * num5, (float)vectorLF.z - vector.z * num5);
		Vector3 vector4 = new Vector3(vector3.x - (float)ptr2.x, vector3.y - (float)ptr2.y, vector3.z - (float)ptr2.z);
		float num6 = Mathf.Sqrt(vector4.x * vector4.x + vector4.y * vector4.y + vector4.z * vector4.z);
		if (num6 > 0f)
		{
			vector4.x /= num6;
			vector4.y /= num6;
			vector4.z /= num6;
		}
		float num7 = (num4 > 2f) ? ((num4 > 6f) ? 6f : num4) : 2f;
		num7 /= 2f;
		num /= num7;
		float num8 = 0f;
		float num9 = num4 / 0.5f - 0.02f;
		if (num9 <= 0f)
		{
			ptr4.x = vector.x;
			ptr4.y = vector.y;
			ptr4.z = vector.z;
		}
		else
		{
			Vector3 vector5 = new Vector3(vector4.x * craftUnitMaxMovementSpeed, vector4.y * craftUnitMaxMovementSpeed, vector4.z * craftUnitMaxMovementSpeed);
			if (num9 < 1f)
			{
				float num10 = 1f - num9;
				vector5.x = vector5.x * num9 + vector.x * num10;
				vector5.y = vector5.y * num9 + vector.y * num10;
				vector5.z = vector5.z * num9 + vector.z * num10;
			}
			Vector3 vector6 = new Vector3(vector5.x - ptr4.x, vector5.y - ptr4.y, vector5.z - ptr4.z);
			num8 = Mathf.Sqrt(vector6.x * vector6.x + vector6.y * vector6.y + vector6.z * vector6.z);
			float num11 = num8 / (num * 0.016666668f);
			if (num11 > 1f)
			{
				vector6.x /= num11;
				vector6.y /= num11;
				vector6.z /= num11;
			}
			ptr4.x += vector6.x;
			ptr4.y += vector6.y;
			ptr4.z += vector6.z;
			float num12 = (num6 - 2f) / 15f;
			if (num12 > 0f)
			{
				float num13 = (float)(ptr2.x * ptr2.x + ptr2.y * ptr2.y + ptr2.z * ptr2.z);
				float num14 = vector3.x * vector3.x + vector3.y * vector3.y + vector3.z * vector3.z - num13;
				float num15 = Mathf.Sqrt(num13);
				Vector3 vector7 = new Vector3((float)ptr2.x / num15, (float)ptr2.y / num15, (float)ptr2.z / num15);
				float num16 = ptr4.x * vector7.x + ptr4.y * vector7.y + ptr4.z * vector7.z;
				if (num16 * num14 < 0f)
				{
					if (num12 >= 1f)
					{
						ptr4.x -= vector7.x * num16;
						ptr4.y -= vector7.y * num16;
						ptr4.z -= vector7.z * num16;
					}
					else
					{
						num12 *= num12;
						num16 *= num12;
						ptr4.x -= vector7.x * num16;
						ptr4.y -= vector7.y * num16;
						ptr4.z -= vector7.z * num16;
					}
				}
			}
		}
		ptr2.x += (double)ptr4.x * 0.016666666666666666;
		ptr2.y += (double)ptr4.y * 0.016666666666666666;
		ptr2.z += (double)ptr4.z * 0.016666666666666666;
		bool flag = false;
		bool flag2 = false;
		float num17 = num4 + num8 / num;
		float num18 = num17 / 0.15f - 0.04f;
		if (num18 < 1f)
		{
			if (num18 <= 0f)
			{
				ptr2.x = vectorLF.x;
				ptr2.y = vectorLF.y;
				ptr2.z = vectorLF.z;
				if (ptr.owner > 0)
				{
					this.behavior = EUnitBehavior.KeepForm;
				}
				flag = true;
			}
			else
			{
				float num19 = 1f - num18;
				ptr2.x = ptr2.x * (double)num18 + vectorLF.x * (double)num19;
				ptr2.y = ptr2.y * (double)num18 + vectorLF.y * (double)num19;
				ptr2.z = ptr2.z * (double)num18 + vectorLF.z * (double)num19;
			}
		}
		float num20 = num17 / 0.65f - 0.04f;
		if (num20 <= 0f)
		{
			ptr3 = quaternion;
			flag2 = true;
		}
		else
		{
			if (ptr4.x * ptr4.x + ptr4.y * ptr4.y + ptr4.z * ptr4.z > 0.01f)
			{
				Quaternion b2 = Quaternion.LookRotation(ptr4, ptr2);
				ptr3 = Quaternion.Slerp(ptr3, b2, 0.1f);
			}
			if (num20 < 1f)
			{
				ptr3 = Quaternion.Slerp(quaternion, ptr3, num20 * num20);
			}
		}
		if (flag && flag2)
		{
			ref CombatStat ptr5 = ref factory.skillSystem.combatStats.buffer[craft.combatStatId];
			if (craft.combatStatId == 0 || ptr5.hp >= ptr5.hpMax)
			{
				if (this.isCharging)
				{
					this.isCharging = false;
					ref FleetComponent ptr6 = ref ptr;
					ptr6.currentChargingUnitsCount -= 1;
				}
				if (!ptr.dispatch)
				{
					this.behavior = EUnitBehavior.Recycled;
				}
			}
			else if (ptr5.hp > 0)
			{
				if (this.isCharging)
				{
					int num21 = (int)((float)pdesc.craftUnitRepairHPPerTick * (1f + factory.gameData.history.globalHpEnhancement) + 0.1f);
					ptr5.hp += num21;
					if (ptr.owner < 0)
					{
						Mecha obj = mecha;
						lock (obj)
						{
							int num22 = num21 * pdesc.craftUnitRepairEnergyPerHP;
							mecha.coreEnergy -= (double)num22;
							mecha.MarkEnergyChange(12, (double)(-(double)num22));
						}
						if (mecha.coreEnergy <= 0.0)
						{
							mecha.coreEnergy = 0.0;
						}
					}
					else if (ptr.owner > 0)
					{
						ref EntityData ptr7 = ref factory.entityPool[ptr.owner];
						if (ptr7.battleBaseId > 0)
						{
							BattleBaseComponent battleBaseComponent = factory.defenseSystem.battleBases.buffer[ptr7.battleBaseId];
							if (battleBaseComponent != null && battleBaseComponent.id == ptr7.battleBaseId)
							{
								int num23 = num21 * pdesc.craftUnitRepairEnergyPerHP;
								battleBaseComponent.energy -= (long)num23;
								if (battleBaseComponent.energy <= 0L)
								{
									battleBaseComponent.energy = 0L;
								}
							}
						}
					}
				}
				else if ((int)ptr.currentChargingUnitsCount < PlanetFactory.PrefabDescByModelIndex[(int)craftPool[craft.owner].modelIndex].fleetMaxChargingCount)
				{
					ref FleetComponent ptr8 = ref ptr;
					ptr8.currentChargingUnitsCount += 1;
					this.isCharging = true;
					int num24 = (int)((float)pdesc.craftUnitRepairHPPerTick * (1f + factory.gameData.history.globalHpEnhancement) + 0.1f);
					ptr5.hp += num24;
					if (ptr.owner < 0)
					{
						Mecha obj = mecha;
						lock (obj)
						{
							int num25 = num24 * pdesc.craftUnitRepairEnergyPerHP;
							mecha.coreEnergy -= (double)num25;
							mecha.MarkEnergyChange(12, (double)(-(double)num25));
						}
						if (mecha.coreEnergy <= 0.0)
						{
							mecha.coreEnergy = 0.0;
						}
					}
					else if (ptr.owner > 0)
					{
						ref EntityData ptr9 = ref factory.entityPool[ptr.owner];
						if (ptr9.battleBaseId > 0)
						{
							BattleBaseComponent battleBaseComponent2 = factory.defenseSystem.battleBases.buffer[ptr9.battleBaseId];
							if (battleBaseComponent2 != null && battleBaseComponent2.id == ptr9.battleBaseId)
							{
								int num26 = num24 * pdesc.craftUnitRepairEnergyPerHP;
								battleBaseComponent2.energy -= (long)num26;
								if (battleBaseComponent2.energy <= 0L)
								{
									battleBaseComponent2.energy = 0L;
								}
							}
						}
					}
				}
			}
		}
		this.anim = 0f;
		this.steering = Vector3.Dot(craft.rot.Forward() - b, rhs);
		this.speed = magnitude;
	}

	// Token: 0x06000F74 RID: 3956 RVA: 0x00100F0C File Offset: 0x000FF10C
	public void RunBehavior_SeekForm_Space(SpaceSector sector, Mecha mecha, PrefabDesc pdesc, ref CraftData craft, ref CombatUpgradeData combatUpgradeData)
	{
		VectorLF3 vectorLF = new VectorLF3(0.0, 0.0, 0.0);
		Quaternion quaternion = new Quaternion(0f, 0f, 0f, 1f);
		Vector3 vector = new Vector3(0f, 0f, 0f);
		float num = pdesc.craftUnitMaxMovementSpeed + 500f;
		float craftUnitMaxMovementAcceleration = pdesc.craftUnitMaxMovementAcceleration;
		float craftUnitMaxRotateAcceleration = pdesc.craftUnitMaxRotateAcceleration;
		float num2 = craftUnitMaxMovementAcceleration * craftUnitMaxMovementAcceleration;
		float num3 = num * num;
		CraftData[] craftPool = sector.craftPool;
		int fleetId = craftPool[craft.owner].fleetId;
		ref FleetComponent ptr = ref sector.combatSpaceSystem.fleets.buffer[fleetId];
		ref CraftData ptr2 = ref craftPool[ptr.craftId];
		if (fleetId > 0)
		{
			ptr.GetUnitPort_Space(mecha, ref ptr2, ref craft, ref vectorLF, ref quaternion, ref vector, ref combatUpgradeData);
		}
		VectorLF3 vectorLF2 = craft.pos;
		Quaternion quaternion2 = craft.rot;
		Vector3 vector2 = craft.vel;
		Vector3 vector3 = new Vector3((float)(vectorLF.x - vectorLF2.x), (float)(vectorLF.y - vectorLF2.y), (float)(vectorLF.z - vectorLF2.z));
		float num4 = Mathf.Sqrt(vector3.x * vector3.x + vector3.y * vector3.y + vector3.z * vector3.z);
		float num5 = 0.016666668f;
		float magnitude = vector2.magnitude;
		bool flag = false;
		bool flag2 = false;
		float num6 = 0.1f;
		if (num6 < 0.1f)
		{
			num6 = 0.1f;
		}
		if (num4 > num6)
		{
			Vector3 rhs = quaternion2.Forward();
			float num7 = Vector3.Dot(quaternion.Forward(), rhs);
			bool astroAvoidanceTargetPos;
			Vector3 a;
			if (Vector3.Dot(vector3, rhs) > 0f && num7 < -0.95f)
			{
				VectorLF3 vectorLF3;
				sector.TransformFromAstro_ref(craft.astroId, out vectorLF3, ref vectorLF);
				astroAvoidanceTargetPos = this.GetAstroAvoidanceTargetPos(sector, num, 0, ref vectorLF3, ref craft);
				sector.InverseTransformToAstro_ref(craft.astroId, ref vectorLF3, out vectorLF);
				a = this.Arrive(ref vectorLF, ref craft, ref vector, num, astroAvoidanceTargetPos, 3000f);
			}
			else
			{
				Vector3 vec = vector;
				float num8 = vector3.magnitude / (num + vec.magnitude);
				VectorLF3 vectorLF4 = vectorLF + vec * (double)num8;
				VectorLF3 vectorLF5;
				sector.TransformFromAstro_ref(craft.astroId, out vectorLF5, ref vectorLF4);
				astroAvoidanceTargetPos = this.GetAstroAvoidanceTargetPos(sector, num, 0, ref vectorLF5, ref craft);
				sector.InverseTransformToAstro_ref(craft.astroId, ref vectorLF5, out vectorLF4);
				a = this.Arrive(ref vectorLF4, ref craft, ref vector, num, astroAvoidanceTargetPos, 3000f);
			}
			if (a.sqrMagnitude > num2 || astroAvoidanceTargetPos)
			{
				a = a.normalized * craftUnitMaxMovementAcceleration;
			}
			vector2 += a * num5;
		}
		else
		{
			vector2 = vector;
		}
		if (num4 < 20f && (ptr2.vel - vector2).sqrMagnitude < 400f)
		{
			flag = true;
		}
		if (vector2.sqrMagnitude > num3)
		{
			vector2 = vector2.normalized * num;
		}
		float num9 = Quaternion.Angle(quaternion2, quaternion) / 180f;
		float num10 = craftUnitMaxRotateAcceleration * (num9 * num9);
		if (num10 < 0.1f)
		{
			num10 = 0.1f;
		}
		quaternion2 = Quaternion.RotateTowards(quaternion2, quaternion, num10);
		if (Quaternion.Angle(quaternion2, quaternion) < 1f && num10 < 1f)
		{
			flag2 = true;
		}
		vectorLF2 += vector2 * (double)num5;
		craft.pos = vectorLF2;
		craft.rot = quaternion2;
		craft.vel = vector2;
		if (flag && flag2)
		{
			ref CombatStat ptr3 = ref sector.skillSystem.combatStats.buffer[craft.combatStatId];
			if (craft.combatStatId == 0 || ptr3.hp >= ptr3.hpMax)
			{
				if (this.isCharging)
				{
					this.isCharging = false;
					ref FleetComponent ptr4 = ref ptr;
					ptr4.currentChargingUnitsCount -= 1;
				}
				if (!ptr.dispatch)
				{
					this.behavior = EUnitBehavior.Recycled;
				}
			}
			else if (ptr3.hp > 0)
			{
				if (this.isCharging)
				{
					int num11 = (int)((float)pdesc.craftUnitRepairHPPerTick * (1f + sector.gameData.history.globalHpEnhancement) + 0.1f);
					ptr3.hp += num11;
					if (ptr.owner < 0)
					{
						Mecha obj = mecha;
						lock (obj)
						{
							int num12 = num11 * pdesc.craftUnitRepairEnergyPerHP;
							mecha.coreEnergy -= (double)num12;
							mecha.MarkEnergyChange(13, (double)(-(double)num12));
						}
						if (mecha.coreEnergy <= 0.0)
						{
							mecha.coreEnergy = 0.0;
						}
					}
				}
				else if ((int)ptr.currentChargingUnitsCount < PlanetFactory.PrefabDescByModelIndex[(int)craftPool[craft.owner].modelIndex].fleetMaxChargingCount)
				{
					ref FleetComponent ptr5 = ref ptr;
					ptr5.currentChargingUnitsCount += 1;
					this.isCharging = true;
					int num13 = (int)((float)pdesc.craftUnitRepairHPPerTick * (1f + sector.gameData.history.globalHpEnhancement) + 0.1f);
					ptr3.hp += num13;
					if (ptr.owner < 0)
					{
						Mecha obj = mecha;
						lock (obj)
						{
							int num14 = num13 * pdesc.craftUnitRepairEnergyPerHP;
							mecha.coreEnergy -= (double)num14;
							mecha.MarkEnergyChange(13, (double)(-(double)num14));
						}
						if (mecha.coreEnergy <= 0.0)
						{
							mecha.coreEnergy = 0.0;
						}
					}
				}
			}
		}
		this.anim = 0f;
		this.steering = 0f;
		this.speed = magnitude;
	}

	// Token: 0x06000F75 RID: 3957 RVA: 0x001014F4 File Offset: 0x000FF6F4
	public void SensorLogic_Groud(ref CraftData craft, PrefabDesc pdesc, PlanetFactory factory)
	{
		EnemyData[] enemyPool = factory.enemyPool;
		ref CraftData ptr = ref factory.craftPool[craft.owner];
		int fleetId = ptr.fleetId;
		ref FleetComponent ptr2 = ref factory.combatGroundSystem.fleets.buffer[fleetId];
		float num = 35f;
		bool flag = true;
		if (ptr.owner == -1)
		{
			flag = GameMain.mainPlayer.mecha.groundCombatModule.attackBuilding;
		}
		if (fleetId > 0 && ptr2.id == fleetId && this.behavior != EUnitBehavior.Engage)
		{
			this.hatred.Merge(ref ptr2.hatred, 300);
			num = PlanetFactory.PrefabDescByModelIndex[(int)ptr.modelIndex].fleetMaxActiveArea;
		}
		float craftUnitSensorRange = pdesc.craftUnitSensorRange;
		float num2 = craftUnitSensorRange * craftUnitSensorRange;
		float num3 = num * num;
		HashSystem hashSystemDynamic = factory.hashSystemDynamic;
		int[] hashPool = hashSystemDynamic.hashPool;
		int[] bucketOffsets = hashSystemDynamic.bucketOffsets;
		hashSystemDynamic.GetBucketIdxesInArea(craft.pos, craftUnitSensorRange);
		int activeBucketsCount = hashSystemDynamic.activeBucketsCount;
		for (int i = 0; i < activeBucketsCount; i++)
		{
			int num4 = hashSystemDynamic.activeBuckets[i];
			int num5 = bucketOffsets[num4];
			int num6 = hashSystemDynamic.bucketCursors[num4];
			for (int j = 0; j < num6; j++)
			{
				int num7 = num5 + j;
				int num8 = hashPool[num7];
				if (num8 != 0 && num8 >> 28 == 4)
				{
					int num9 = num8 & 268435455;
					ref EnemyData ptr3 = ref enemyPool[num9];
					if (ptr3.id == num9 && !ptr3.isInvincible && (ptr3.builderId <= 0 || flag))
					{
						float num10 = (float)(ptr3.pos - craft.pos).sqrMagnitude;
						float num11 = (float)(ptr3.pos - ptr.pos).sqrMagnitude;
						if (num10 < num2 && num11 < num3)
						{
							this.hatred.HateTarget(EObjectType.Enemy, num9, (int)((craftUnitSensorRange - Mathf.Sqrt(num10)) * 2f + 30f + 0.5f), 300, EHatredOperation.Add);
						}
					}
				}
			}
		}
		hashSystemDynamic.ClearActiveBuckets();
		HashSystem hashSystemStatic = factory.hashSystemStatic;
		hashPool = hashSystemStatic.hashPool;
		bucketOffsets = hashSystemStatic.bucketOffsets;
		hashSystemStatic.GetBucketIdxesInArea(craft.pos, craftUnitSensorRange);
		activeBucketsCount = hashSystemStatic.activeBucketsCount;
		for (int k = 0; k < activeBucketsCount; k++)
		{
			int num12 = hashSystemStatic.activeBuckets[k];
			int num13 = bucketOffsets[num12];
			int num14 = hashSystemStatic.bucketCursors[num12];
			for (int l = 0; l < num14; l++)
			{
				int num15 = num13 + l;
				int num16 = hashPool[num15];
				if (num16 != 0 && num16 >> 28 == 4)
				{
					int num17 = num16 & 268435455;
					ref EnemyData ptr4 = ref enemyPool[num17];
					if (ptr4.id == num17 && !ptr4.isInvincible && (ptr4.builderId <= 0 || flag))
					{
						float num18 = (float)(ptr4.pos - craft.pos).sqrMagnitude;
						float num19 = (float)(ptr4.pos - ptr.pos).sqrMagnitude;
						if (num18 < num2 && num19 < num3)
						{
							this.hatred.HateTarget(EObjectType.Enemy, num17, (int)((craftUnitSensorRange - Mathf.Sqrt(num18)) * 2f + 30f + 0.5f), 300, EHatredOperation.Add);
						}
					}
				}
			}
		}
		hashSystemStatic.ClearActiveBuckets();
	}

	// Token: 0x06000F76 RID: 3958 RVA: 0x00101878 File Offset: 0x000FFA78
	public void AssistTeammates_Ground(ref CraftData craft, PlanetFactory factory, Mecha mecha)
	{
		CraftData[] craftPool = factory.craftPool;
		ref CraftData ptr = ref craftPool[craft.owner];
		int fleetId = ptr.fleetId;
		ref FleetComponent ptr2 = ref factory.combatGroundSystem.fleets.buffer[fleetId];
		if (this.hatred.max.value == 0)
		{
			ModuleFleet[] array = null;
			UnitComponent[] buffer = factory.combatGroundSystem.units.buffer;
			if (ptr2.owner == -1)
			{
				array = mecha.groundCombatModule.moduleFleets;
			}
			else
			{
				if (ptr2.owner <= 0)
				{
					return;
				}
				ref EntityData ptr3 = ref factory.entityPool[ptr2.owner];
				if (ptr3.id == ptr2.owner)
				{
					int combatModuleId = ptr3.combatModuleId;
					if (combatModuleId > 0)
					{
						CombatModuleComponent combatModuleComponent = factory.combatGroundSystem.combatModules.buffer[combatModuleId];
						if (combatModuleComponent != null && combatModuleComponent.id == combatModuleId)
						{
							array = combatModuleComponent.moduleFleets;
						}
					}
				}
			}
			ModuleFleet[] obj = array;
			lock (obj)
			{
				ModuleFighter[] fighters = array[(int)((ptr2.owner == -1) ? ptr.port : 0)].fighters;
				int num = fighters.Length;
				for (int i = 0; i < num; i++)
				{
					int num2 = fighters[i].craftId;
					if (num2 != 0 && num2 != this.craftId && craftPool[num2].id == num2)
					{
						int unitId = craftPool[num2].unitId;
						ref UnitComponent ptr4 = ref buffer[unitId];
						if (ptr4.id == unitId && ptr4.hatred.max.value != 0)
						{
							ref HatredTarget ptr5 = ref ptr4.hatred.max;
							this.hatred.HateTarget(ptr5.targetType, ptr5.objectId, ptr5.value, 300, EHatredOperation.Set);
						}
					}
				}
			}
		}
	}

	// Token: 0x06000F77 RID: 3959 RVA: 0x00101A6C File Offset: 0x000FFC6C
	public void SensorLogic_Space(ref CraftData craft, PrefabDesc pdesc, SpaceSector sector)
	{
		EnemyData[] enemyPool = sector.enemyPool;
		int enemyCursor = sector.enemyCursor;
		CraftData[] craftPool = sector.craftPool;
		int fleetId = craftPool[craft.owner].fleetId;
		bool flag = false;
		bool flag2 = true;
		if (craftPool[craft.owner].owner == -1)
		{
			flag = GameMain.mainPlayer.mecha.spaceCombatModule.attackRelay;
			flag2 = GameMain.mainPlayer.mecha.spaceCombatModule.attackBuilding;
		}
		if (fleetId > 0)
		{
			this.hatred.Merge(ref sector.combatSpaceSystem.fleets.buffer[fleetId].hatred, 3000);
		}
		float craftUnitSensorRange = pdesc.craftUnitSensorRange;
		float num = craftUnitSensorRange * craftUnitSensorRange;
		VectorLF3 pos = craft.pos;
		VectorLF3 lhs = default(VectorLF3);
		EnemyDFHiveSystem[] dfHivesByAstro = sector.dfHivesByAstro;
		int num2 = 1000000;
		CombatStat[] buffer = sector.skillSystem.combatStats.buffer;
		int astroId = craft.astroId;
		for (int i = 1; i < enemyCursor; i++)
		{
			if (enemyPool[i].id == i)
			{
				ref EnemyData ptr = ref enemyPool[i];
				if (!ptr.isInvincible && (ptr.dfRelayId <= 0 || flag) && (ptr.builderId <= 0 || flag2))
				{
					int originAstroId = ptr.originAstroId;
					if (dfHivesByAstro[originAstroId - num2].starData.astroId == craft.astroId || ptr.dfTinderId != 0)
					{
						int astroId2 = ptr.astroId;
						if (astroId != astroId2)
						{
							if (astroId == 0)
							{
								sector.InverseTransformToAstro_ref(astroId2, ref pos, out lhs);
							}
							else if (astroId2 == 0)
							{
								sector.TransformFromAstro_ref(astroId, out lhs, ref pos);
							}
							else
							{
								VectorLF3 vectorLF;
								sector.TransformFromAstro_ref(astroId, out vectorLF, ref pos);
								sector.InverseTransformToAstro_ref(astroId2, ref vectorLF, out lhs);
							}
						}
						else
						{
							lhs = pos;
						}
						float num3 = (float)(lhs - ptr.pos).sqrMagnitude;
						if (num3 <= num)
						{
							ref CombatStat combatStat = ref buffer[ptr.combatStatId];
							float sqrRangeRatio = num3 / num;
							float num4;
							float num5;
							this.CalculateDamageOverflow_Space(ref combatStat, sqrRangeRatio, out num4, out num5);
							if (num4 >= num5)
							{
								this.hatred.HateTarget(EObjectType.Enemy, i, (int)((craftUnitSensorRange - Mathf.Sqrt(num3)) * 0.1f + 0.5f), 3000, EHatredOperation.Add);
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x06000F78 RID: 3960 RVA: 0x00101CB0 File Offset: 0x000FFEB0
	public void AssistTeammates_Space(ref CraftData craft, SpaceSector sector, Mecha mecha)
	{
		CraftData[] craftPool = sector.craftPool;
		ref CraftData ptr = ref craftPool[craft.owner];
		int fleetId = ptr.fleetId;
		ref FleetComponent ptr2 = ref sector.combatSpaceSystem.fleets.buffer[fleetId];
		if (this.hatred.max.value == 0)
		{
			UnitComponent[] buffer = sector.combatSpaceSystem.units.buffer;
			if (ptr2.owner != -1)
			{
				return;
			}
			ModuleFleet[] moduleFleets = mecha.spaceCombatModule.moduleFleets;
			ModuleFleet[] obj = moduleFleets;
			lock (obj)
			{
				ModuleFighter[] fighters = moduleFleets[(int)((ptr2.owner == -1) ? ptr.port : 0)].fighters;
				int num = fighters.Length;
				for (int i = 0; i < num; i++)
				{
					int num2 = fighters[i].craftId;
					if (num2 != 0 && num2 != this.craftId && craftPool[num2].id == num2)
					{
						int unitId = craftPool[num2].unitId;
						ref UnitComponent ptr3 = ref buffer[unitId];
						if (ptr3.id == unitId && ptr3.hatred.max.value != 0)
						{
							ref HatredTarget ptr4 = ref ptr3.hatred.max;
							this.hatred.HateTarget(ptr4.targetType, ptr4.objectId, ptr4.value, 3000, EHatredOperation.Set);
						}
					}
				}
			}
		}
	}

	// Token: 0x06000F79 RID: 3961 RVA: 0x00101E3C File Offset: 0x0010003C
	public void CalculateDamageOverflow_Space(ref CombatStat combatStat, float sqrRangeRatio, out float hpBudgetProp, out float hpThresholdMin)
	{
		hpBudgetProp = 1f;
		hpThresholdMin = 0f;
		if (combatStat.id == 0)
		{
			return;
		}
		hpBudgetProp = (float)(combatStat.hp + combatStat.hpIncoming) / (float)combatStat.hpMax;
		if (sqrRangeRatio >= 0.85f)
		{
			hpThresholdMin = -0.01f;
			return;
		}
		if (sqrRangeRatio >= 0.2f)
		{
			hpThresholdMin = -0.1f;
			return;
		}
		hpBudgetProp = 1f;
	}

	// Token: 0x06000F7A RID: 3962 RVA: 0x00101EA4 File Offset: 0x001000A4
	public bool GetTargetPosition_Ground(PlanetFactory factory, int idType, out Vector3 target)
	{
		int num = idType >> 26;
		int num2 = idType & 67108863;
		if (num2 <= 0)
		{
			target.x = 0f;
			target.y = 0f;
			target.z = 0f;
			return false;
		}
		if (num == 4)
		{
			ref EnemyData ptr = ref factory.enemyPool[num2];
			if (ptr.isInvincible)
			{
				target.x = 0f;
				target.y = 0f;
				target.z = 0f;
				return false;
			}
			target = ptr.pos;
			return ptr.id == num2;
		}
		else
		{
			if (num == 0)
			{
				ref EntityData ptr2 = ref factory.entityPool[num2];
				target = ptr2.pos;
				return ptr2.id == num2;
			}
			if (num == 6)
			{
				ref CraftData ptr3 = ref factory.craftPool[num2];
				if (ptr3.isInvincible)
				{
					target.x = 0f;
					target.y = 0f;
					target.z = 0f;
					return false;
				}
				target = ptr3.pos;
				return ptr3.id == num2;
			}
			else
			{
				if (num == 1)
				{
					ref VegeData ptr4 = ref factory.vegePool[num2];
					target = ptr4.pos;
					return ptr4.id == num2;
				}
				if (num == 2)
				{
					ref VeinData ptr5 = ref factory.veinPool[num2];
					target = ptr5.pos;
					return ptr5.id == num2;
				}
				target.x = 0f;
				target.y = 0f;
				target.z = 0f;
				return false;
			}
		}
	}

	// Token: 0x06000F7B RID: 3963 RVA: 0x00102034 File Offset: 0x00100234
	public bool GetTargetPosition_Space(SpaceSector sector, int idType, ref VectorLF3 targetUPos, ref Vector3 targetUVel, out int objAstroId)
	{
		int num = idType >> 26;
		int num2 = idType & 67108863;
		objAstroId = 0;
		if (num2 <= 0)
		{
			return false;
		}
		if (num == 4)
		{
			ref EnemyData ptr = ref sector.enemyPool[num2];
			if (ptr.isInvincible)
			{
				return false;
			}
			objAstroId = ptr.astroId;
			sector.TransformFromAstro_ref(objAstroId, out targetUPos, ref ptr.pos);
			SkillTarget skillTarget = default(SkillTarget);
			skillTarget.astroId = ptr.originAstroId;
			skillTarget.id = num2;
			skillTarget.type = ETargetType.Enemy;
			sector.skillSystem.GetObjectUVelocity(ref skillTarget, out targetUVel);
			objAstroId = ptr.originAstroId;
			return ptr.id == num2;
		}
		else
		{
			if (num != 6)
			{
				return false;
			}
			ref CraftData ptr2 = ref sector.craftPool[num2];
			if (ptr2.isInvincible)
			{
				return false;
			}
			objAstroId = ptr2.astroId;
			sector.TransformFromAstro_ref(objAstroId, out targetUPos, ref ptr2.pos);
			Vector3 vel = ptr2.vel;
			if (objAstroId > 1000000)
			{
				sector.astros[objAstroId - 1000000].VelocityL2U(ref ptr2.pos, ref vel, out targetUVel);
			}
			else if (objAstroId >= 100 && objAstroId <= 204899)
			{
				sector.galaxyAstros[objAstroId].VelocityL2U(ref ptr2.pos, ref vel, out targetUVel);
			}
			else
			{
				targetUVel = ptr2.pos;
			}
			return ptr2.id == num2;
		}
	}

	// Token: 0x06000F7C RID: 3964 RVA: 0x00102198 File Offset: 0x00100398
	private Vector3 Arrive(ref VectorLF3 tarPos, ref CraftData craft, ref Vector3 tarVel, float maxMovementSpeed, bool needAvoidanceAstro, float decelerateRange = 3000f)
	{
		Vector3 a = tarPos - craft.pos;
		float magnitude = a.magnitude;
		if (magnitude > 0f)
		{
			float d = (magnitude > decelerateRange) ? 1f : (magnitude / decelerateRange);
			float d2 = needAvoidanceAstro ? (maxMovementSpeed * 1f) : maxMovementSpeed;
			if (needAvoidanceAstro)
			{
				tarVel = default(Vector3);
			}
			return a / magnitude * d2 * d + tarVel - craft.vel;
		}
		return -craft.vel;
	}

	// Token: 0x06000F7D RID: 3965 RVA: 0x00102230 File Offset: 0x00100430
	private Vector3 Arrive_Orbiting(ref VectorLF3 tarPos, ref CraftData craft, ref Vector3 tarVel, float maxMovementSpeed, bool needAvoidanceAstro)
	{
		VectorLF3 lhs = tarPos - craft.pos;
		double magnitude = lhs.magnitude;
		if (magnitude > 0.0)
		{
			double num = magnitude * 5.0;
			if (num > (double)maxMovementSpeed)
			{
				num = (double)maxMovementSpeed;
			}
			if (needAvoidanceAstro)
			{
				tarVel = default(Vector3);
			}
			return lhs / magnitude * (float)num + tarVel - craft.vel;
		}
		return -craft.vel;
	}

	// Token: 0x06000F7E RID: 3966 RVA: 0x001022BC File Offset: 0x001004BC
	public bool CheckTargetValidBeforeShoot_Space(SpaceSector sector, int targetId)
	{
		ref EnemyData ptr = ref sector.enemyPool[targetId];
		return ptr.id == targetId && !ptr.isInvincible;
	}

	// Token: 0x06000F7F RID: 3967 RVA: 0x001022EC File Offset: 0x001004EC
	private void RotateVector(ref Vector3 vec, ref Quaternion rotation, out Vector3 result)
	{
		float num = rotation.x * 2f;
		float num2 = rotation.y * 2f;
		float num3 = rotation.z * 2f;
		float num4 = rotation.x * num;
		float num5 = rotation.y * num2;
		float num6 = rotation.z * num3;
		float num7 = rotation.x * num2;
		float num8 = rotation.x * num3;
		float num9 = rotation.y * num3;
		float num10 = rotation.w * num;
		float num11 = rotation.w * num2;
		float num12 = rotation.w * num3;
		result.x = (1f - (num5 + num6)) * vec.x + (num7 - num12) * vec.y + (num8 + num11) * vec.z;
		result.y = (num7 + num12) * vec.x + (1f - (num4 + num6)) * vec.y + (num9 - num10) * vec.z;
		result.z = (num8 - num11) * vec.x + (num9 + num10) * vec.y + (1f - (num4 + num5)) * vec.z;
	}

	// Token: 0x06000F80 RID: 3968 RVA: 0x00102410 File Offset: 0x00100610
	private void RotateVector(ref Vector3 vec, ref Quaternion rotation, out VectorLF3 result)
	{
		float num = rotation.x * 2f;
		float num2 = rotation.y * 2f;
		float num3 = rotation.z * 2f;
		float num4 = rotation.x * num;
		float num5 = rotation.y * num2;
		float num6 = rotation.z * num3;
		float num7 = rotation.x * num2;
		float num8 = rotation.x * num3;
		float num9 = rotation.y * num3;
		float num10 = rotation.w * num;
		float num11 = rotation.w * num2;
		float num12 = rotation.w * num3;
		result.x = (double)((1f - (num5 + num6)) * vec.x + (num7 - num12) * vec.y + (num8 + num11) * vec.z);
		result.y = (double)((num7 + num12) * vec.x + (1f - (num4 + num6)) * vec.y + (num9 - num10) * vec.z);
		result.z = (double)((num8 - num11) * vec.x + (num9 + num10) * vec.y + (1f - (num4 + num5)) * vec.z);
	}

	// Token: 0x040010F3 RID: 4339
	public int id;

	// Token: 0x040010F4 RID: 4340
	public int craftId;

	// Token: 0x040010F5 RID: 4341
	public short protoId;

	// Token: 0x040010F6 RID: 4342
	public bool isShooting0;

	// Token: 0x040010F7 RID: 4343
	public bool isShooting1;

	// Token: 0x040010F8 RID: 4344
	public int fire0;

	// Token: 0x040010F9 RID: 4345
	public int fire1;

	// Token: 0x040010FA RID: 4346
	public byte muzzleIndex0;

	// Token: 0x040010FB RID: 4347
	public byte muzzleIndex1;

	// Token: 0x040010FC RID: 4348
	public bool isRetreating;

	// Token: 0x040010FD RID: 4349
	public bool isCharging;

	// Token: 0x040010FE RID: 4350
	public int currentInitializeValue;

	// Token: 0x040010FF RID: 4351
	public int hpShortage;

	// Token: 0x04001100 RID: 4352
	public bool adjustEngageRange;

	// Token: 0x04001101 RID: 4353
	public EUnitBehavior behavior;

	// Token: 0x04001102 RID: 4354
	public HatredList hatred;

	// Token: 0x04001103 RID: 4355
	public float speed;

	// Token: 0x04001104 RID: 4356
	public float anim;

	// Token: 0x04001105 RID: 4357
	public float steering;

	// Token: 0x04001106 RID: 4358
	private const int KMaxInitializeValue = 600000;

	// Token: 0x04001107 RID: 4359
	private const double kDeltaTime = 0.016666666666666666;

	// Token: 0x04001108 RID: 4360
	private const double kEnterStarDistance = 3240000000000.0;

	// Token: 0x04001109 RID: 4361
	public static long gameTick;
}
