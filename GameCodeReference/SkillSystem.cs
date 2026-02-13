using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

// Token: 0x02000189 RID: 393
public class SkillSystem
{
	// Token: 0x06000E5F RID: 3679 RVA: 0x000D7414 File Offset: 0x000D5614
	public SkillSystem(SpaceSector _sector)
	{
		this.sector = _sector;
		this.gameData = this.sector.gameData;
		this.history = this.gameData.history;
		this.astroFactories = this.gameData.galaxy.astrosFactory;
		ModelProto[] dataArray = LDB.models.dataArray;
		SkillSystem.HpMaxByModelIndex = new int[dataArray.Length + 64];
		for (int i = 0; i < SkillSystem.HpMaxByModelIndex.Length; i++)
		{
			SkillSystem.HpMaxByModelIndex[i] = 1;
		}
		for (int j = 0; j < dataArray.Length; j++)
		{
			SkillSystem.HpMaxByModelIndex[dataArray[j].ID] = dataArray[j].HpMax;
		}
		SkillSystem.HpUpgradeByModelIndex = new int[dataArray.Length + 64];
		for (int k = 0; k < SkillSystem.HpUpgradeByModelIndex.Length; k++)
		{
			SkillSystem.HpUpgradeByModelIndex[k] = 1;
		}
		for (int l = 0; l < dataArray.Length; l++)
		{
			SkillSystem.HpUpgradeByModelIndex[dataArray[l].ID] = dataArray[l].HpUpgrade;
		}
		SkillSystem.HpRecoverByModelIndex = new int[dataArray.Length + 64];
		for (int m = 0; m < SkillSystem.HpRecoverByModelIndex.Length; m++)
		{
			SkillSystem.HpRecoverByModelIndex[m] = 0;
		}
		for (int n = 0; n < dataArray.Length; n++)
		{
			SkillSystem.HpRecoverByModelIndex[dataArray[n].ID] = dataArray[n].HpRecover;
		}
		SkillSystem.RoughRadiusByModelIndex = new float[dataArray.Length + 64];
		for (int num = 0; num < SkillSystem.RoughRadiusByModelIndex.Length; num++)
		{
			SkillSystem.RoughRadiusByModelIndex[num] = 1f;
		}
		for (int num2 = 0; num2 < dataArray.Length; num2++)
		{
			SkillSystem.RoughRadiusByModelIndex[dataArray[num2].ID] = dataArray[num2].prefabDesc.roughRadius;
		}
		SkillSystem.RoughHeightByModelIndex = new float[dataArray.Length + 64];
		for (int num3 = 0; num3 < SkillSystem.RoughHeightByModelIndex.Length; num3++)
		{
			SkillSystem.RoughHeightByModelIndex[num3] = 1f;
		}
		for (int num4 = 0; num4 < dataArray.Length; num4++)
		{
			SkillSystem.RoughHeightByModelIndex[dataArray[num4].ID] = dataArray[num4].prefabDesc.roughHeight;
		}
		SkillSystem.RoughWidthByModelIndex = new float[dataArray.Length + 64];
		for (int num5 = 0; num5 < SkillSystem.RoughWidthByModelIndex.Length; num5++)
		{
			SkillSystem.RoughWidthByModelIndex[num5] = 1f;
		}
		for (int num6 = 0; num6 < dataArray.Length; num6++)
		{
			SkillSystem.RoughWidthByModelIndex[dataArray[num6].ID] = dataArray[num6].prefabDesc.roughWidth;
		}
		SkillSystem.BarHeightByModelIndex = new float[dataArray.Length + 64];
		for (int num7 = 0; num7 < SkillSystem.BarHeightByModelIndex.Length; num7++)
		{
			SkillSystem.BarHeightByModelIndex[num7] = 1f;
		}
		for (int num8 = 0; num8 < dataArray.Length; num8++)
		{
			SkillSystem.BarHeightByModelIndex[dataArray[num8].ID] = dataArray[num8].prefabDesc.barHeight;
		}
		SkillSystem.BarWidthByModelIndex = new float[dataArray.Length + 64];
		for (int num9 = 0; num9 < SkillSystem.BarWidthByModelIndex.Length; num9++)
		{
			SkillSystem.BarWidthByModelIndex[num9] = 1f;
		}
		for (int num10 = 0; num10 < dataArray.Length; num10++)
		{
			SkillSystem.BarWidthByModelIndex[dataArray[num10].ID] = dataArray[num10].prefabDesc.barWidth;
		}
		SkillSystem.ColliderComplexityByModelIndex = new int[dataArray.Length + 64];
		for (int num11 = 0; num11 < SkillSystem.ColliderComplexityByModelIndex.Length; num11++)
		{
			SkillSystem.ColliderComplexityByModelIndex[num11] = 0;
		}
		for (int num12 = 0; num12 < dataArray.Length; num12++)
		{
			SkillSystem.ColliderComplexityByModelIndex[dataArray[num12].ID] = dataArray[num12].prefabDesc.colliderComplexity;
		}
		SkillSystem.EnemySandCountByModelIndex = new int[dataArray.Length + 64];
		for (int num13 = 0; num13 < SkillSystem.EnemySandCountByModelIndex.Length; num13++)
		{
			SkillSystem.EnemySandCountByModelIndex[num13] = 0;
		}
		for (int num14 = 0; num14 < dataArray.Length; num14++)
		{
			SkillSystem.EnemySandCountByModelIndex[dataArray[num14].ID] = dataArray[num14].prefabDesc.enemySandCount;
		}
	}

	// Token: 0x06000E60 RID: 3680 RVA: 0x000D7828 File Offset: 0x000D5A28
	public void Init()
	{
		this.combatStats = new DataPool<CombatStat>();
		this.removedSkillTargets = new HashSet<SkillTarget>();
		this.localGeneralProjectiles = new DataPoolRenderer<LocalGeneralProjectile>();
		this.localGeneralProjectiles.InitRenderer(Configs.combat.localGeneralProjectileDesc);
		this.localLaserContinuous = new DataPoolRenderer<LocalLaserContinuous>();
		this.localLaserContinuous.InitRenderer(Configs.combat.localLaserContinuousDesc);
		this.localLaserOneShots = new DataPoolRenderer<LocalLaserOneShot>();
		this.localLaserOneShots.InitRenderer(Configs.combat.localLaserOneShotDesc);
		this.localCannonades = new DataPool<LocalCannonade>();
		this.localCannonades.Reset();
		this.localDisturbingWaves = new DataPoolRenderer<LocalDisturbingWave>();
		this.localDisturbingWaves.InitRenderer(Configs.combat.localDisturbingWaveDesc);
		this.generalProjectiles = new DataPoolRenderer<GeneralProjectile>();
		this.generalProjectiles.InitRenderer(Configs.combat.generalProjectileDesc);
		this.spaceLaserOneShots = new DataPool<SpaceLaserOneShot>();
		this.spaceLaserOneShots.Reset();
		this.spaceLaserOneShotRenderer = new GenericRenderer<SpaceLaserOneShotRenderingData>();
		this.spaceLaserOneShotRenderer.Init(Configs.combat.spaceLaserOneShotDesc, new Action(this.UpdateSpaceLaserOneShotRenderingData));
		this.spaceLaserSweeps = new DataPool<SpaceLaserSweep>();
		this.spaceLaserSweeps.Reset();
		this.spaceLaserSweepRenderer = new GenericRenderer<SpaceLaserSweepRenderingData>();
		this.spaceLaserSweepRenderer.Init(Configs.combat.spaceLaserSweepDesc, new Action(this.UpdateSpaceLaserSweepRenderingData));
		this.explosiveUnitBombs = new DataPool<Bomb_Explosive>();
		this.explosiveUnitBombs.Reset();
		this.explosiveUnitBombRenderer = new GenericRenderer<GeneralBombRenderingData>();
		this.explosiveUnitBombRenderer.Init(Configs.combat.explosiveUnitBombDesc, new Action(this.UpdateExplosiveUnitBombRenderingData));
		this.emCapsuleBombs = new DataPool<Bomb_EMCapsule>();
		this.emCapsuleBombs.Reset();
		this.emCapsuleBombRenderer = new GenericRenderer<GeneralBombRenderingData>();
		this.emCapsuleBombRenderer.Init(Configs.combat.emCapsuleBombDesc, new Action(this.UpdateEMCapsuleBombRenderingData));
		this.liquidBombs = new DataPool<Bomb_Liquid>();
		this.liquidBombs.Reset();
		this.liquidBombRenderer = new GenericRenderer<GeneralBombRenderingData>();
		this.liquidBombRenderer.Init(Configs.combat.liquidBombDesc, new Action(this.UpdateLiquidBombRenderingData));
		this.raiderLasers = new DataPoolRenderer<LocalLaserOneShot>();
		this.raiderLasers.InitRenderer(Configs.combat.raiderLaserDesc);
		this.rangerPlasmas = new DataPoolRenderer<LocalGeneralProjectile>();
		this.rangerPlasmas.InitRenderer(Configs.combat.rangerPlasmaDesc);
		this.guardianPlasmas = new DataPoolRenderer<LocalGeneralProjectile>();
		this.guardianPlasmas.InitRenderer(Configs.combat.guardianPlasmaDesc);
		this.dfgTowerLasers = new DataPoolRenderer<LocalLaserOneShot>();
		this.dfgTowerLasers.InitRenderer(Configs.combat.dfgTowerLaserDesc);
		this.dfgTowerPlasmas = new DataPoolRenderer<LocalGeneralProjectile>();
		this.dfgTowerPlasmas.InitRenderer(Configs.combat.dfgTowerPlasmaDesc);
		this.fighterLasers = new DataPoolRenderer<LocalLaserOneShot>();
		this.fighterLasers.InitRenderer(Configs.combat.fighterLaserDesc);
		this.fighterPlasmas = new DataPoolRenderer<LocalGeneralProjectile>();
		this.fighterPlasmas.InitRenderer(Configs.combat.fighterPlasmaDesc);
		this.fighterShieldPlasmas = new DataPoolRenderer<LocalGeneralProjectile>();
		this.fighterShieldPlasmas.InitRenderer(Configs.combat.fighterShieldPlasmaDesc);
		this.turretGaussProjectiles = new DataPoolRenderer<LocalGeneralProjectile>();
		this.turretGaussProjectiles.InitRenderer(Configs.combat.turretGaussGeneralDesc);
		this.turretLaserContinuous = new DataPoolRenderer<LocalLaserContinuous>();
		this.turretLaserContinuous.InitRenderer(Configs.combat.turretLaserContinuousDesc);
		this.turretCannonades = new DataPool<LocalCannonade>();
		this.turretCannonades.Reset();
		this.turretPlasmas = new DataPoolRenderer<GeneralProjectile>();
		this.turretPlasmas.InitRenderer(Configs.combat.turretPlasmaDesc);
		this.turretLocalPlasmas = new DataPoolRenderer<GeneralProjectile>();
		this.turretLocalPlasmas.InitRenderer(Configs.combat.turretLocalPlasmaDesc);
		this.turretMissiles = new DataPool<GeneralMissile>();
		this.turretMissiles.Reset();
		this.turretMissileRenderer = new GenericRenderer<GeneralMissileRenderingData>();
		this.turretMissileRenderer.Init(Configs.combat.turretMissileDesc, new Action(this.UpdateTurretMissileRenderingData));
		this.turretMissileTrails = new VFTrailRenderer();
		this.turretMissileTrails.InitRenderer(Configs.combat.turretMissileTrailDesc, 50);
		this.turretDisturbingWave = new DataPoolRenderer<LocalDisturbingWave>();
		this.turretDisturbingWave.InitRenderer(Configs.combat.turretDisturbingWaveDesc);
		this.dfsTowerPlasmas = new DataPoolRenderer<GeneralProjectile>();
		this.dfsTowerPlasmas.InitRenderer(Configs.combat.dfsTowerPlasmaDesc);
		this.dfsTowerLasers = new DataPool<SpaceLaserOneShot>();
		this.dfsTowerLasers.Reset();
		this.dfsTowerLaserRenderer = new GenericRenderer<SpaceLaserOneShotRenderingData>();
		this.dfsTowerLaserRenderer.Init(Configs.combat.dfsTowerLaserOneShotDesc, new Action(this.UpdateDFSTowerLaserOneShotRenderingData));
		this.lancerSpacePlasma = new DataPoolRenderer<GeneralProjectile>();
		this.lancerSpacePlasma.InitRenderer(Configs.combat.lancerPlasmaDesc);
		this.lancerLaserOneShots = new DataPool<SpaceLaserOneShot>();
		this.lancerLaserOneShots.Reset();
		this.lancerLaserOneShotRenderer = new GenericRenderer<SpaceLaserOneShotRenderingData>();
		this.lancerLaserOneShotRenderer.Init(Configs.combat.lancerLaserOneShotDesc, new Action(this.UpdateLancerLaserOneShotRenderingData));
		this.lancerLaserSweeps = new DataPool<SpaceLaserSweep>();
		this.lancerLaserSweeps.Reset();
		this.lancerLaserSweepRenderer = new GenericRenderer<SpaceLaserSweepRenderingData>();
		this.lancerLaserSweepRenderer.Init(Configs.combat.lancerLaserSweepDesc, new Action(this.UpdateLancerLaserSweepRenderingData));
		this.humpbackProjectiles = new DataPoolRenderer<GeneralExpImpProjectile>();
		this.humpbackProjectiles.InitRenderer(Configs.combat.humpbackProjectileDesc);
		this.warshipTypeFLasers = new DataPool<SpaceLaserOneShot>();
		this.warshipTypeFLasers.Reset();
		this.warshipTypeFLaserRenderer = new GenericRenderer<SpaceLaserOneShotRenderingData>();
		this.warshipTypeFLaserRenderer.Init(Configs.combat.warshipTypeFLaserDesc, new Action(this.UpdateWarshipTypeFLaserRenderingData));
		this.warshipTypeFPlasmas = new DataPoolRenderer<GeneralProjectile>();
		this.warshipTypeFPlasmas.InitRenderer(Configs.combat.warshipTypeFPlasmaDesc);
		this.warshipTypeAPlasmas = new DataPoolRenderer<GeneralProjectile>();
		this.warshipTypeAPlasmas.InitRenderer(Configs.combat.warshipTypeAPlasmaDesc);
		this.mechaLocalGaussProjectiles = new DataPoolRenderer<LocalGeneralProjectile>();
		this.mechaLocalGaussProjectiles.InitRenderer(Configs.combat.mechaLocalGaussProjectileDesc);
		this.mechaSpaceGaussProjectiles = new DataPoolRenderer<GeneralProjectile>();
		this.mechaSpaceGaussProjectiles.InitRenderer(Configs.combat.mechaSpaceGaussProjectileDesc);
		this.mechaLocalLaserOneShots = new DataPoolRenderer<LocalLaserOneShot>();
		this.mechaLocalLaserOneShots.InitRenderer(Configs.combat.mechaLocalLaserDesc);
		this.mechaSpaceLaserOneShots = new DataPool<SpaceLaserOneShot>();
		this.mechaSpaceLaserOneShots.Reset();
		this.mechaSpaceLaserOneShotRenderer = new GenericRenderer<SpaceLaserOneShotRenderingData>();
		this.mechaSpaceLaserOneShotRenderer.Init(Configs.combat.mechaSpaceLaserDesc, new Action(this.UpdateMechaSpaceLaserRenderingData));
		this.mechaLocalCannonades = new DataPool<LocalCannonade>();
		this.mechaLocalCannonades.Reset();
		this.mechaSpaceCannonades = new DataPool<GeneralCannonade>();
		this.mechaSpaceCannonades.Reset();
		this.mechaPlasmas = new DataPoolRenderer<GeneralProjectile>();
		this.mechaPlasmas.InitRenderer(Configs.combat.mechaPlasmaDesc);
		this.mechaMissiles = new DataPool<GeneralMissile>();
		this.mechaMissiles.Reset();
		this.mechaMissileRenderer = new GenericRenderer<GeneralMissileRenderingData>();
		this.mechaMissileRenderer.Init(Configs.combat.mechaMissileDesc, new Action(this.UpdateMechaMissileRenderingData));
		this.mechaMissileTrails = new VFTrailRenderer();
		this.mechaMissileTrails.InitRenderer(Configs.combat.mechaMissileTrailDesc, 50);
		this.mechaShieldBursts = new DataPoolRendererMultiMat<GeneralShieldBurst>();
		this.mechaShieldBursts.InitRenderer(Configs.combat.mechaShieldBurstDesc);
		this.hitEffects = new DataPoolRenderer<ParticleData>[Configs.combat.skillHitDescs.Length];
		for (int i = 1; i < this.hitEffects.Length; i++)
		{
			RenderableObjectDesc renderableObjectDesc = Configs.combat.skillHitDescs[i];
			if (renderableObjectDesc != null)
			{
				if (i == 10 || i == 18 || i == 38 || i == 40 || i == 41 || i == 42)
				{
					this.hitEffects[i] = new DataPoolRendererMultiMat<ParticleData>();
				}
				else
				{
					this.hitEffects[i] = new DataPoolRenderer<ParticleData>();
				}
				this.hitEffects[i].InitRenderer(renderableObjectDesc);
			}
		}
		this.audio = new SkillAudioLogic();
		this.audio.Init(this.gameData);
	}

	// Token: 0x06000E61 RID: 3681 RVA: 0x000D8024 File Offset: 0x000D6224
	public void Free()
	{
		if (this.combatStats != null)
		{
			this.combatStats.Free();
			this.combatStats = null;
		}
		if (this.removedSkillTargets != null)
		{
			this.removedSkillTargets.Clear();
			this.removedSkillTargets = null;
		}
		if (this.localGeneralProjectiles != null)
		{
			this.localGeneralProjectiles.FreeRenderer();
			this.localGeneralProjectiles = null;
		}
		if (this.localLaserContinuous != null)
		{
			this.localLaserContinuous.FreeRenderer();
			this.localLaserContinuous = null;
		}
		if (this.localLaserOneShots != null)
		{
			this.localLaserOneShots.FreeRenderer();
			this.localLaserOneShots = null;
		}
		if (this.localCannonades != null)
		{
			this.localCannonades.Free();
			this.localCannonades = null;
		}
		if (this.localDisturbingWaves != null)
		{
			this.localDisturbingWaves.FreeRenderer();
			this.localDisturbingWaves = null;
		}
		if (this.generalProjectiles != null)
		{
			this.generalProjectiles.FreeRenderer();
			this.generalProjectiles = null;
		}
		if (this.spaceLaserOneShots != null)
		{
			this.spaceLaserOneShots.Free();
			this.spaceLaserOneShots = null;
		}
		if (this.spaceLaserOneShotRenderer != null)
		{
			this.spaceLaserOneShotRenderer.Free();
			this.spaceLaserOneShotRenderer = null;
		}
		if (this.spaceLaserSweeps != null)
		{
			this.spaceLaserSweeps.Free();
			this.spaceLaserSweeps = null;
		}
		if (this.spaceLaserSweepRenderer != null)
		{
			this.spaceLaserSweepRenderer.Free();
			this.spaceLaserSweepRenderer = null;
		}
		if (this.explosiveUnitBombs != null)
		{
			this.explosiveUnitBombs.Free();
			this.explosiveUnitBombs = null;
		}
		if (this.explosiveUnitBombRenderer != null)
		{
			this.explosiveUnitBombRenderer.Free();
			this.explosiveUnitBombRenderer = null;
		}
		if (this.emCapsuleBombs != null)
		{
			this.emCapsuleBombs.Free();
			this.emCapsuleBombs = null;
		}
		if (this.emCapsuleBombRenderer != null)
		{
			this.emCapsuleBombRenderer.Free();
			this.emCapsuleBombRenderer = null;
		}
		if (this.liquidBombs != null)
		{
			this.liquidBombs.Free();
			this.liquidBombs = null;
		}
		if (this.liquidBombRenderer != null)
		{
			this.liquidBombRenderer.Free();
			this.liquidBombRenderer = null;
		}
		if (this.raiderLasers != null)
		{
			this.raiderLasers.FreeRenderer();
			this.raiderLasers = null;
		}
		if (this.rangerPlasmas != null)
		{
			this.rangerPlasmas.FreeRenderer();
			this.rangerPlasmas = null;
		}
		if (this.guardianPlasmas != null)
		{
			this.guardianPlasmas.FreeRenderer();
			this.guardianPlasmas = null;
		}
		if (this.dfgTowerLasers != null)
		{
			this.dfgTowerLasers.FreeRenderer();
			this.dfgTowerLasers = null;
		}
		if (this.dfgTowerPlasmas != null)
		{
			this.dfgTowerPlasmas.FreeRenderer();
			this.dfgTowerPlasmas = null;
		}
		if (this.fighterLasers != null)
		{
			this.fighterLasers.FreeRenderer();
			this.fighterLasers = null;
		}
		if (this.fighterPlasmas != null)
		{
			this.fighterPlasmas.FreeRenderer();
			this.fighterPlasmas = null;
		}
		if (this.fighterShieldPlasmas != null)
		{
			this.fighterShieldPlasmas.FreeRenderer();
			this.fighterShieldPlasmas = null;
		}
		if (this.turretGaussProjectiles != null)
		{
			this.turretGaussProjectiles.FreeRenderer();
			this.turretGaussProjectiles = null;
		}
		if (this.turretLaserContinuous != null)
		{
			this.turretLaserContinuous.FreeRenderer();
			this.turretLaserContinuous = null;
		}
		if (this.turretCannonades != null)
		{
			this.turretCannonades.Free();
			this.turretCannonades = null;
		}
		if (this.turretPlasmas != null)
		{
			this.turretPlasmas.FreeRenderer();
			this.turretPlasmas = null;
		}
		if (this.turretLocalPlasmas != null)
		{
			this.turretLocalPlasmas.FreeRenderer();
			this.turretLocalPlasmas = null;
		}
		if (this.turretMissiles != null)
		{
			this.turretMissiles.Free();
			this.turretMissiles = null;
		}
		if (this.turretMissileRenderer != null)
		{
			this.turretMissileRenderer.Free();
			this.turretMissileRenderer = null;
		}
		if (this.turretMissileTrails != null)
		{
			this.turretMissileTrails.FreeRenderer();
			this.turretMissileTrails = null;
		}
		if (this.turretDisturbingWave != null)
		{
			this.turretDisturbingWave.FreeRenderer();
			this.turretDisturbingWave = null;
		}
		if (this.dfsTowerPlasmas != null)
		{
			this.dfsTowerPlasmas.FreeRenderer();
			this.dfsTowerPlasmas = null;
		}
		if (this.dfsTowerLasers != null)
		{
			this.dfsTowerLasers.Free();
			this.dfsTowerLasers = null;
		}
		if (this.dfsTowerLaserRenderer != null)
		{
			this.dfsTowerLaserRenderer.Free();
			this.dfsTowerLaserRenderer = null;
		}
		if (this.lancerSpacePlasma != null)
		{
			this.lancerSpacePlasma.FreeRenderer();
			this.lancerSpacePlasma = null;
		}
		if (this.lancerLaserOneShots != null)
		{
			this.lancerLaserOneShots.Free();
			this.lancerLaserOneShots = null;
		}
		if (this.lancerLaserOneShotRenderer != null)
		{
			this.lancerLaserOneShotRenderer.Free();
			this.lancerLaserOneShotRenderer = null;
		}
		if (this.lancerLaserSweeps != null)
		{
			this.lancerLaserSweeps.Free();
			this.lancerLaserSweeps = null;
		}
		if (this.lancerLaserSweepRenderer != null)
		{
			this.lancerLaserSweepRenderer.Free();
			this.lancerLaserSweepRenderer = null;
		}
		if (this.humpbackProjectiles != null)
		{
			this.humpbackProjectiles.FreeRenderer();
			this.humpbackProjectiles = null;
		}
		if (this.warshipTypeFLasers != null)
		{
			this.warshipTypeFLasers.Free();
			this.warshipTypeFLasers = null;
		}
		if (this.warshipTypeFLaserRenderer != null)
		{
			this.warshipTypeFLaserRenderer.Free();
			this.warshipTypeFLaserRenderer = null;
		}
		if (this.warshipTypeFPlasmas != null)
		{
			this.warshipTypeFPlasmas.FreeRenderer();
			this.warshipTypeFPlasmas = null;
		}
		if (this.warshipTypeAPlasmas != null)
		{
			this.warshipTypeAPlasmas.FreeRenderer();
			this.warshipTypeAPlasmas = null;
		}
		if (this.mechaLocalGaussProjectiles != null)
		{
			this.mechaLocalGaussProjectiles.FreeRenderer();
			this.mechaLocalGaussProjectiles = null;
		}
		if (this.mechaSpaceGaussProjectiles != null)
		{
			this.mechaSpaceGaussProjectiles.FreeRenderer();
			this.mechaSpaceGaussProjectiles = null;
		}
		if (this.mechaLocalLaserOneShots != null)
		{
			this.mechaLocalLaserOneShots.FreeRenderer();
			this.mechaLocalLaserOneShots = null;
		}
		if (this.mechaSpaceLaserOneShots != null)
		{
			this.mechaSpaceLaserOneShots.Free();
			this.mechaSpaceLaserOneShots = null;
		}
		if (this.mechaSpaceLaserOneShotRenderer != null)
		{
			this.mechaSpaceLaserOneShotRenderer.Free();
			this.mechaSpaceLaserOneShotRenderer = null;
		}
		if (this.mechaLocalCannonades != null)
		{
			this.mechaLocalCannonades.Free();
			this.mechaLocalCannonades = null;
		}
		if (this.mechaSpaceCannonades != null)
		{
			this.mechaSpaceCannonades.Free();
			this.mechaSpaceCannonades = null;
		}
		if (this.mechaPlasmas != null)
		{
			this.mechaPlasmas.FreeRenderer();
			this.mechaPlasmas = null;
		}
		if (this.mechaMissiles != null)
		{
			this.mechaMissiles.Free();
			this.mechaMissiles = null;
		}
		if (this.mechaMissileRenderer != null)
		{
			this.mechaMissileRenderer.Free();
			this.mechaMissileRenderer = null;
		}
		if (this.mechaMissileTrails != null)
		{
			this.mechaMissileTrails.FreeRenderer();
			this.mechaMissileTrails = null;
		}
		if (this.mechaShieldBursts != null)
		{
			this.mechaShieldBursts.FreeRenderer();
			this.mechaShieldBursts = null;
		}
		if (this.hitEffects != null)
		{
			for (int i = 0; i < this.hitEffects.Length; i++)
			{
				if (this.hitEffects[i] != null)
				{
					this.hitEffects[i].FreeRenderer();
					this.hitEffects[i] = null;
				}
			}
			this.hitEffects = null;
		}
		if (this.audio != null)
		{
			this.audio.Free();
			this.audio = null;
		}
		this.sector = null;
		this.history = null;
		this.gameData = null;
	}

	// Token: 0x06000E62 RID: 3682 RVA: 0x000D86BC File Offset: 0x000D68BC
	public void SetForNewGame()
	{
		this.combatStats.Reset();
		this.removedSkillTargets.Clear();
		this.localGeneralProjectiles.ResetPool();
		this.localLaserContinuous.ResetPool();
		this.localLaserOneShots.ResetPool();
		this.localCannonades.Reset();
		this.localDisturbingWaves.ResetPool();
		this.generalProjectiles.ResetPool();
		this.spaceLaserOneShots.Reset();
		this.spaceLaserOneShotRenderer.ResetInstanceArray();
		this.spaceLaserSweeps.Reset();
		this.spaceLaserSweepRenderer.ResetInstanceArray();
		this.explosiveUnitBombs.Reset();
		this.explosiveUnitBombRenderer.ResetInstanceArray();
		this.emCapsuleBombs.Reset();
		this.emCapsuleBombRenderer.ResetInstanceArray();
		this.liquidBombs.Reset();
		this.liquidBombRenderer.ResetInstanceArray();
		this.raiderLasers.ResetPool();
		this.rangerPlasmas.ResetPool();
		this.guardianPlasmas.ResetPool();
		this.dfgTowerLasers.ResetPool();
		this.dfgTowerPlasmas.ResetPool();
		this.fighterLasers.ResetPool();
		this.fighterPlasmas.ResetPool();
		this.fighterShieldPlasmas.ResetPool();
		this.turretGaussProjectiles.ResetPool();
		this.turretLaserContinuous.ResetPool();
		this.turretCannonades.Reset();
		this.turretPlasmas.ResetPool();
		this.turretLocalPlasmas.ResetPool();
		this.turretMissiles.Reset();
		this.turretMissileRenderer.ResetInstanceArray();
		this.turretMissileTrails.ResetPool();
		this.turretDisturbingWave.ResetPool();
		this.dfsTowerPlasmas.ResetPool();
		this.dfsTowerLasers.Reset();
		this.dfsTowerLaserRenderer.ResetInstanceArray();
		this.lancerSpacePlasma.ResetPool();
		this.lancerLaserOneShots.Reset();
		this.lancerLaserOneShotRenderer.ResetInstanceArray();
		this.lancerLaserSweeps.Reset();
		this.lancerLaserSweepRenderer.ResetInstanceArray();
		this.humpbackProjectiles.ResetPool();
		this.warshipTypeFLasers.Reset();
		this.warshipTypeFLaserRenderer.ResetInstanceArray();
		this.warshipTypeFPlasmas.ResetPool();
		this.warshipTypeAPlasmas.ResetPool();
		this.mechaLocalGaussProjectiles.ResetPool();
		this.mechaSpaceGaussProjectiles.ResetPool();
		this.mechaLocalLaserOneShots.ResetPool();
		this.mechaSpaceLaserOneShots.Reset();
		this.mechaSpaceLaserOneShotRenderer.ResetInstanceArray();
		this.mechaLocalCannonades.Reset();
		this.mechaSpaceCannonades.Reset();
		this.mechaPlasmas.ResetPool();
		this.mechaMissiles.Reset();
		this.mechaMissileRenderer.ResetInstanceArray();
		this.mechaMissileTrails.ResetPool();
		this.mechaShieldBursts.ResetPool();
		for (int i = 1; i < this.hitEffects.Length; i++)
		{
			if (this.hitEffects[i] != null)
			{
				this.hitEffects[i].ResetPool();
			}
		}
	}

	// Token: 0x1400000E RID: 14
	// (add) Token: 0x06000E63 RID: 3683 RVA: 0x000D8988 File Offset: 0x000D6B88
	// (remove) Token: 0x06000E64 RID: 3684 RVA: 0x000D89C0 File Offset: 0x000D6BC0
	public event SkillSystem.DEnemyRefFunc onEnemyKilled;

	// Token: 0x1400000F RID: 15
	// (add) Token: 0x06000E65 RID: 3685 RVA: 0x000D89F8 File Offset: 0x000D6BF8
	// (remove) Token: 0x06000E66 RID: 3686 RVA: 0x000D8A30 File Offset: 0x000D6C30
	public event SkillSystem.DEntityRefFunc onEntityKilled;

	// Token: 0x06000E67 RID: 3687 RVA: 0x000D8A68 File Offset: 0x000D6C68
	public void CollectTempStates()
	{
		this.gameTickTmp = GameMain.gameTick;
		this.combatSettingsTmp = this.history.combatSettings;
		this.isEnemyHostileTmp = this.combatSettingsTmp.isEnemyHostile;
		this.enemyAggressiveLevelTmp = this.combatSettingsTmp.aggressiveLevel;
		EAggressiveLevel eaggressiveLevel = this.enemyAggressiveLevelTmp;
		if (eaggressiveLevel <= EAggressiveLevel.Torpid)
		{
			if (eaggressiveLevel != EAggressiveLevel.Dummy)
			{
				if (eaggressiveLevel == EAggressiveLevel.Passive || eaggressiveLevel == EAggressiveLevel.Torpid)
				{
					this.maxHatredGroundTmp = 225;
					this.maxHatredGroundDamageTmp = 450;
					this.maxHatredSpaceTmp = 2250;
					this.maxHatredSpaceDamageTmp = 4500;
					this.maxHatredGroundBaseTmp = 81000;
					this.maxHatredSpaceHiveTmp = 3240000;
				}
			}
			else
			{
				this.maxHatredGroundTmp = 0;
				this.maxHatredGroundDamageTmp = 0;
				this.maxHatredSpaceTmp = 0;
				this.maxHatredSpaceDamageTmp = 0;
				this.maxHatredGroundBaseTmp = 0;
				this.maxHatredSpaceHiveTmp = 0;
			}
		}
		else if (eaggressiveLevel != EAggressiveLevel.Normal)
		{
			if (eaggressiveLevel != EAggressiveLevel.Sharp)
			{
				if (eaggressiveLevel == EAggressiveLevel.Rampage)
				{
					this.maxHatredGroundTmp = 800;
					this.maxHatredGroundDamageTmp = 1600;
					this.maxHatredSpaceTmp = 8000;
					this.maxHatredSpaceDamageTmp = 16000;
					this.maxHatredGroundBaseTmp = 288000;
					this.maxHatredSpaceHiveTmp = 11520000;
				}
			}
			else
			{
				this.maxHatredGroundTmp = 500;
				this.maxHatredGroundDamageTmp = 1000;
				this.maxHatredSpaceTmp = 5000;
				this.maxHatredSpaceDamageTmp = 10000;
				this.maxHatredGroundBaseTmp = 180000;
				this.maxHatredSpaceHiveTmp = 7200000;
			}
		}
		else
		{
			this.maxHatredGroundTmp = 300;
			this.maxHatredGroundDamageTmp = 600;
			this.maxHatredSpaceTmp = 3000;
			this.maxHatredSpaceDamageTmp = 6000;
			this.maxHatredGroundBaseTmp = 108000;
			this.maxHatredSpaceHiveTmp = 4320000;
		}
		this.enemyAggressiveHatredCoefTmp = this.combatSettingsTmp.aggressiveHatredCoef;
		if (this.enemyAggressiveLevelTmp > EAggressiveLevel.Passive && this.history.dfTruceTimer > 0L)
		{
			this.enemyAggressiveLevelTmp = EAggressiveLevel.Passive;
		}
		this.killStatistics = this.gameData.statistics.kill;
	}

	// Token: 0x06000E68 RID: 3688 RVA: 0x000D8C78 File Offset: 0x000D6E78
	public void CollectPlayerStates()
	{
		this.mecha = this.gameData.mainPlayer.mecha;
		this.playerAlive = this.mecha.player.isAlive;
		this.playerIsSailing = this.mecha.player.sailing;
		this.playerIsWarping = this.mecha.player.warping;
		if (this.mecha.player.planetData != null && this.mecha.player.planetData.factoryLoaded)
		{
			this.playerAstroId = this.mecha.player.planetData.astroId;
		}
		else
		{
			this.playerAstroId = 0;
		}
		this.playerSkillTargetL = this.mecha.skillTargetLCenter;
		this.playerSkillTargetULast = this.playerSkillTargetU;
		this.playerSkillTargetU = this.mecha.skillTargetUCenter;
		this.playerSkillCastLeftL = this.mecha.skillCastLeftL;
		this.playerSkillCastLeftU = this.mecha.skillCastLeftU;
		this.playerSkillCastRightL = this.mecha.skillCastRightL;
		this.playerSkillCastRightU = this.mecha.skillCastRightU;
		if (this.gameData.localPlanet != null)
		{
			this.playerAltL = this.playerSkillTargetL.magnitude - this.gameData.localPlanet.realRadius;
		}
		else
		{
			this.playerAltL = 0f;
		}
		this.playerVelocityL = this.gameData.mainPlayer.controller.velocity;
		this.playerVelocityU = this.gameData.mainPlayer.uVelocity;
		this.playerSkillColliderL = this.mecha.skillColliderL;
		this.playerSkillColliderU = this.mecha.skillColliderU;
		if (this.mecha.player.isAlive && this.mecha.energyShieldEnergy >= this.mecha.energyShieldEnergyRate)
		{
			this.playerEnergyShieldRadius = this.mecha.energyShieldRadius * this.mecha.energyShieldRadiusMultiplier;
		}
		else
		{
			this.playerEnergyShieldRadius = 0f;
		}
		PlanetData localPlanet = this.gameData.localPlanet;
		this.localAstroId = ((localPlanet != null) ? localPlanet.astroId : 0);
		PlanetData localPlanet2 = this.gameData.localPlanet;
		this.localPlanetAstroId = ((localPlanet2 != null) ? localPlanet2.astroId : 0);
		StarData localStar = this.gameData.localStar;
		this.localStarAstroId = ((localStar != null) ? localStar.astroId : 0);
		this.localPlanetOrStarAstroId = ((this.gameData.localPlanet != null) ? this.gameData.localPlanet.astroId : ((this.gameData.localStar != null) ? this.gameData.localStar.astroId : 0));
	}

	// Token: 0x06000E69 RID: 3689 RVA: 0x000D8F20 File Offset: 0x000D7120
	public void GameTick(long time)
	{
		ref VectorLF3 ptr = ref this.gameData.relativePos;
		ref Quaternion ptr2 = ref this.gameData.relativeRot;
		for (int i = 1; i < this.hitEffects.Length; i++)
		{
			if (this.hitEffects[i] != null)
			{
				int cursor = this.hitEffects[i].cursor;
				ParticleData[] buffer = this.hitEffects[i].buffer;
				for (int j = 1; j < cursor; j++)
				{
					if (buffer[j].id == j)
					{
						ParticleData[] array = buffer;
						int num = j;
						array[num].time = array[num].time + 1;
						if (buffer[j].astroId == 0)
						{
							buffer[j].CalculatePosFromUPos(ref ptr, ref ptr2);
						}
						if (buffer[j].duration > 0 && buffer[j].time >= buffer[j].duration)
						{
							this.hitEffects[i].Remove(j);
						}
					}
				}
				if (this.hitEffects[i].count == 0 && this.hitEffects[i].capacity > 256)
				{
					this.hitEffects[i].Flush();
				}
			}
		}
		PlanetFactory[] factories = this.gameData.factories;
		int cursor2 = this.spaceLaserSweeps.cursor;
		SpaceLaserSweep[] buffer2 = this.spaceLaserSweeps.buffer;
		for (int k = 1; k < cursor2; k++)
		{
			ref SpaceLaserSweep ptr3 = ref buffer2[k];
			if (ptr3.id == k)
			{
				ptr3.BeforeTickSkillLogic(this, ERayTestSkillType.spaceLaserSweep);
			}
		}
		int cursor3 = this.lancerSpacePlasma.cursor;
		GeneralProjectile[] buffer3 = this.lancerSpacePlasma.buffer;
		for (int l = 1; l < cursor3; l++)
		{
			if (buffer3[l].id == l)
			{
				buffer3[l].BeforeTickSkillLogic(this, ERayTestSkillType.lancerSpacePlasma);
			}
		}
		int cursor4 = this.lancerLaserOneShots.cursor;
		SpaceLaserOneShot[] buffer4 = this.lancerLaserOneShots.buffer;
		for (int m = 1; m < cursor4; m++)
		{
			if (buffer4[m].id == m)
			{
				buffer4[m].BeforeTickSkillLogic(this, ERayTestSkillType.lancerLaserOneShot);
			}
		}
		int cursor5 = this.lancerLaserSweeps.cursor;
		SpaceLaserSweep[] buffer5 = this.lancerLaserSweeps.buffer;
		for (int n = 1; n < cursor5; n++)
		{
			ref SpaceLaserSweep ptr4 = ref buffer5[n];
			if (ptr4.id == n)
			{
				ptr4.BeforeTickSkillLogic(this, ERayTestSkillType.lancerLaserSweep);
			}
		}
		int cursor6 = this.humpbackProjectiles.cursor;
		GeneralExpImpProjectile[] buffer6 = this.humpbackProjectiles.buffer;
		for (int num2 = 1; num2 < cursor6; num2++)
		{
			ref GeneralExpImpProjectile ptr5 = ref buffer6[num2];
			if (ptr5.id == num2)
			{
				ptr5.BeforeTickSkillLogic(this, ERayTestSkillType.humpbackPlasma);
			}
		}
		for (int num3 = 0; num3 < this.gameData.factoryCount; num3++)
		{
			PlanetATField planetATField = factories[num3].planetATField;
			if (planetATField.rayTesting)
			{
				planetATField.DoRayTests();
				planetATField.EndRayTests();
			}
		}
		int cursor7 = this.localGeneralProjectiles.cursor;
		LocalGeneralProjectile[] buffer7 = this.localGeneralProjectiles.buffer;
		for (int num4 = 1; num4 < cursor7; num4++)
		{
			if (buffer7[num4].id == num4)
			{
				buffer7[num4].TickSkillLogic(this);
				if (buffer7[num4].life == 0)
				{
					buffer7[num4].HandleRemoving(this);
					this.localGeneralProjectiles.Remove(num4);
				}
			}
		}
		if (this.localGeneralProjectiles.count == 0 && this.localGeneralProjectiles.capacity > 256)
		{
			this.localGeneralProjectiles.Flush();
		}
		int cursor8 = this.localLaserContinuous.cursor;
		LocalLaserContinuous[] buffer8 = this.localLaserContinuous.buffer;
		for (int num5 = 1; num5 < cursor8; num5++)
		{
			if (buffer8[num5].id == num5)
			{
				buffer8[num5].TickSkillLogic(this, factories);
				if (buffer8[num5].fade == 0f)
				{
					this.localLaserContinuous.Remove(num5);
				}
			}
		}
		if (this.localLaserContinuous.count == 0 && this.localLaserContinuous.capacity > 256)
		{
			this.localLaserContinuous.Flush();
		}
		int cursor9 = this.localLaserOneShots.cursor;
		LocalLaserOneShot[] buffer9 = this.localLaserOneShots.buffer;
		for (int num6 = 1; num6 < cursor9; num6++)
		{
			if (buffer9[num6].id == num6)
			{
				buffer9[num6].TickSkillLogic(this, factories);
				if (buffer9[num6].life == 0)
				{
					this.localLaserOneShots.Remove(num6);
				}
			}
		}
		if (this.localLaserOneShots.count == 0 && this.localLaserOneShots.capacity > 256)
		{
			this.localLaserOneShots.Flush();
		}
		int cursor10 = this.localCannonades.cursor;
		LocalCannonade[] buffer10 = this.localCannonades.buffer;
		for (int num7 = 1; num7 < cursor10; num7++)
		{
			if (buffer10[num7].id == num7)
			{
				buffer10[num7].TickSkillLogic(this, factories);
				if (buffer10[num7].life == 0)
				{
					this.localCannonades.Remove(num7);
				}
			}
		}
		if (this.localCannonades.count == 0 && this.localCannonades.capacity > 256)
		{
			this.localCannonades.Flush();
		}
		int cursor11 = this.localDisturbingWaves.cursor;
		LocalDisturbingWave[] buffer11 = this.localDisturbingWaves.buffer;
		for (int num8 = 1; num8 < cursor11; num8++)
		{
			if (buffer11[num8].id == num8)
			{
				buffer11[num8].TickSkillLogic(this);
				if (buffer11[num8].life == 0)
				{
					this.localDisturbingWaves.Remove(num8);
				}
			}
		}
		if (this.localDisturbingWaves.count == 0 && this.localDisturbingWaves.capacity > 256)
		{
			this.localDisturbingWaves.Flush();
		}
		int cursor12 = this.generalProjectiles.cursor;
		GeneralProjectile[] buffer12 = this.generalProjectiles.buffer;
		for (int num9 = 1; num9 < cursor12; num9++)
		{
			if (buffer12[num9].id == num9)
			{
				buffer12[num9].TickSkillLogic(this, this.gameData, ref ptr, ref ptr2);
				if (buffer12[num9].life == 0)
				{
					buffer12[num9].HandleRemoving(this);
					this.generalProjectiles.Remove(num9);
				}
			}
		}
		if (this.generalProjectiles.count == 0 && this.generalProjectiles.capacity > 256)
		{
			this.generalProjectiles.Flush();
		}
		int cursor13 = this.spaceLaserOneShots.cursor;
		SpaceLaserOneShot[] buffer13 = this.spaceLaserOneShots.buffer;
		for (int num10 = 1; num10 < cursor13; num10++)
		{
			if (buffer13[num10].id == num10)
			{
				buffer13[num10].TickSkillLogic(this, ref ptr, ref ptr2);
				if (buffer13[num10].life == 0)
				{
					this.spaceLaserOneShots.Remove(num10);
				}
			}
		}
		if (this.spaceLaserOneShots.count == 0 && this.spaceLaserOneShots.capacity > 256)
		{
			this.spaceLaserOneShots.Flush();
		}
		int cursor14 = this.spaceLaserSweeps.cursor;
		SpaceLaserSweep[] buffer14 = this.spaceLaserSweeps.buffer;
		for (int num11 = 1; num11 < cursor14; num11++)
		{
			if (buffer14[num11].id == num11)
			{
				buffer14[num11].TickSkillLogic(this, ref ptr, ref ptr2);
				if (buffer14[num11].life == 0)
				{
					this.spaceLaserSweeps.Remove(num11);
				}
			}
		}
		if (this.spaceLaserSweeps.count == 0 && this.spaceLaserSweeps.capacity > 256)
		{
			this.spaceLaserSweeps.Flush();
		}
		int cursor15 = this.explosiveUnitBombs.cursor;
		Bomb_Explosive[] buffer15 = this.explosiveUnitBombs.buffer;
		for (int num12 = 1; num12 < cursor15; num12++)
		{
			if (buffer15[num12].id == num12)
			{
				buffer15[num12].TickSkillLogic(this, time);
				if (buffer15[num12].life == 0)
				{
					this.explosiveUnitBombs.Remove(num12);
				}
			}
		}
		if (this.explosiveUnitBombs.count == 0 && this.explosiveUnitBombs.capacity > 256)
		{
			this.explosiveUnitBombs.Flush();
		}
		int cursor16 = this.emCapsuleBombs.cursor;
		Bomb_EMCapsule[] buffer16 = this.emCapsuleBombs.buffer;
		for (int num13 = 1; num13 < cursor16; num13++)
		{
			if (buffer16[num13].id == num13)
			{
				buffer16[num13].TickSkillLogic(this, time);
				if (buffer16[num13].life == 0)
				{
					this.emCapsuleBombs.Remove(num13);
				}
			}
		}
		if (this.emCapsuleBombs.count == 0 && this.emCapsuleBombs.capacity > 256)
		{
			this.emCapsuleBombs.Flush();
		}
		int cursor17 = this.liquidBombs.cursor;
		Bomb_Liquid[] buffer17 = this.liquidBombs.buffer;
		for (int num14 = 1; num14 < cursor17; num14++)
		{
			if (buffer17[num14].id == num14)
			{
				buffer17[num14].TickSkillLogic(this, time);
				if (buffer17[num14].life == 0)
				{
					this.liquidBombs.Remove(num14);
				}
			}
		}
		if (this.liquidBombs.count == 0 && this.liquidBombs.capacity > 256)
		{
			this.liquidBombs.Flush();
		}
		int cursor18 = this.raiderLasers.cursor;
		LocalLaserOneShot[] buffer18 = this.raiderLasers.buffer;
		for (int num15 = 1; num15 < cursor18; num15++)
		{
			if (buffer18[num15].id == num15)
			{
				buffer18[num15].TickSkillLogic(this, factories);
				if (buffer18[num15].life == 0)
				{
					this.raiderLasers.Remove(num15);
				}
			}
		}
		if (this.raiderLasers.count == 0 && this.raiderLasers.capacity > 256)
		{
			this.raiderLasers.Flush();
		}
		int cursor19 = this.rangerPlasmas.cursor;
		LocalGeneralProjectile[] buffer19 = this.rangerPlasmas.buffer;
		for (int num16 = 1; num16 < cursor19; num16++)
		{
			if (buffer19[num16].id == num16)
			{
				buffer19[num16].TickSkillLogic(this);
				if (buffer19[num16].life == 0)
				{
					buffer19[num16].HandleRemoving(this);
					this.rangerPlasmas.Remove(num16);
				}
			}
		}
		if (this.rangerPlasmas.count == 0 && this.rangerPlasmas.capacity > 256)
		{
			this.rangerPlasmas.Flush();
		}
		int cursor20 = this.guardianPlasmas.cursor;
		LocalGeneralProjectile[] buffer20 = this.guardianPlasmas.buffer;
		for (int num17 = 1; num17 < cursor20; num17++)
		{
			if (buffer20[num17].id == num17)
			{
				buffer20[num17].TickSkillLogic(this);
				if (buffer20[num17].life == 0)
				{
					buffer20[num17].HandleRemoving(this);
					this.guardianPlasmas.Remove(num17);
				}
			}
		}
		if (this.guardianPlasmas.count == 0 && this.guardianPlasmas.capacity > 256)
		{
			this.guardianPlasmas.Flush();
		}
		int cursor21 = this.dfgTowerLasers.cursor;
		LocalLaserOneShot[] buffer21 = this.dfgTowerLasers.buffer;
		for (int num18 = 1; num18 < cursor21; num18++)
		{
			if (buffer21[num18].id == num18)
			{
				buffer21[num18].TickSkillLogic(this, factories);
				if (buffer21[num18].life == 0)
				{
					this.dfgTowerLasers.Remove(num18);
				}
			}
		}
		if (this.dfgTowerLasers.count == 0 && this.dfgTowerLasers.capacity > 256)
		{
			this.dfgTowerLasers.Flush();
		}
		int cursor22 = this.dfgTowerPlasmas.cursor;
		LocalGeneralProjectile[] buffer22 = this.dfgTowerPlasmas.buffer;
		for (int num19 = 1; num19 < cursor22; num19++)
		{
			if (buffer22[num19].id == num19)
			{
				buffer22[num19].TickSkillLogic(this);
				if (buffer22[num19].life == 0)
				{
					buffer22[num19].HandleRemoving(this);
					this.dfgTowerPlasmas.Remove(num19);
				}
			}
		}
		if (this.dfgTowerPlasmas.count == 0 && this.dfgTowerPlasmas.capacity > 256)
		{
			this.dfgTowerPlasmas.Flush();
		}
		int cursor23 = this.fighterLasers.cursor;
		LocalLaserOneShot[] buffer23 = this.fighterLasers.buffer;
		for (int num20 = 1; num20 < cursor23; num20++)
		{
			if (buffer23[num20].id == num20)
			{
				buffer23[num20].TickSkillLogic(this, factories);
				if (buffer23[num20].life == 0)
				{
					this.fighterLasers.Remove(num20);
				}
			}
		}
		if (this.fighterLasers.count == 0 && this.fighterLasers.capacity > 256)
		{
			this.fighterLasers.Flush();
		}
		int cursor24 = this.fighterPlasmas.cursor;
		LocalGeneralProjectile[] buffer24 = this.fighterPlasmas.buffer;
		for (int num21 = 1; num21 < cursor24; num21++)
		{
			if (buffer24[num21].id == num21)
			{
				buffer24[num21].TickSkillLogic(this);
				if (buffer24[num21].life == 0)
				{
					buffer24[num21].HandleRemoving(this);
					this.ClearReferencesOnSkillRemove(num21, buffer24[num21].astroId, ESkillType.FighterPlasmas);
					this.fighterPlasmas.Remove(num21);
				}
			}
		}
		if (this.fighterPlasmas.count == 0 && this.fighterPlasmas.capacity > 256)
		{
			this.fighterPlasmas.Flush();
		}
		int cursor25 = this.fighterShieldPlasmas.cursor;
		LocalGeneralProjectile[] buffer25 = this.fighterShieldPlasmas.buffer;
		for (int num22 = 1; num22 < cursor25; num22++)
		{
			if (buffer25[num22].id == num22)
			{
				buffer25[num22].TickSkillLogic(this);
				if (buffer25[num22].life == 0)
				{
					buffer25[num22].HandleRemoving(this);
					this.ClearReferencesOnSkillRemove(num22, buffer25[num22].astroId, ESkillType.FighterShieldPlasmas);
					this.fighterShieldPlasmas.Remove(num22);
				}
			}
		}
		if (this.fighterShieldPlasmas.count == 0 && this.fighterShieldPlasmas.capacity > 256)
		{
			this.fighterShieldPlasmas.Flush();
		}
		int cursor26 = this.dfsTowerPlasmas.cursor;
		GeneralProjectile[] buffer26 = this.dfsTowerPlasmas.buffer;
		for (int num23 = 1; num23 < cursor26; num23++)
		{
			if (buffer26[num23].id == num23)
			{
				buffer26[num23].TickSkillLogic(this, this.gameData, ref ptr, ref ptr2);
				if (buffer26[num23].life == 0)
				{
					buffer26[num23].HandleRemoving(this);
					this.dfsTowerPlasmas.Remove(num23);
				}
			}
		}
		if (this.dfsTowerPlasmas.count == 0 && this.dfsTowerPlasmas.capacity > 256)
		{
			this.dfsTowerPlasmas.Flush();
		}
		int cursor27 = this.dfsTowerLasers.cursor;
		SpaceLaserOneShot[] buffer27 = this.dfsTowerLasers.buffer;
		for (int num24 = 1; num24 < cursor27; num24++)
		{
			if (buffer27[num24].id == num24)
			{
				buffer27[num24].TickSkillLogic(this, ref ptr, ref ptr2);
				if (buffer27[num24].life == 0)
				{
					this.dfsTowerLasers.Remove(num24);
				}
			}
		}
		if (this.dfsTowerLasers.count == 0 && this.dfsTowerLasers.capacity > 256)
		{
			this.dfsTowerLasers.Flush();
		}
		int cursor28 = this.lancerSpacePlasma.cursor;
		GeneralProjectile[] buffer28 = this.lancerSpacePlasma.buffer;
		for (int num25 = 1; num25 < cursor28; num25++)
		{
			if (buffer28[num25].id == num25)
			{
				buffer28[num25].TickSkillLogic(this, this.gameData, ref ptr, ref ptr2);
				if (buffer28[num25].life == 0)
				{
					buffer28[num25].HandleRemoving(this);
					this.lancerSpacePlasma.Remove(num25);
				}
			}
		}
		if (this.lancerSpacePlasma.count == 0 && this.lancerSpacePlasma.capacity > 256)
		{
			this.lancerSpacePlasma.Flush();
		}
		int cursor29 = this.lancerLaserOneShots.cursor;
		SpaceLaserOneShot[] buffer29 = this.lancerLaserOneShots.buffer;
		for (int num26 = 1; num26 < cursor29; num26++)
		{
			if (buffer29[num26].id == num26)
			{
				buffer29[num26].TickSkillLogic(this, ref ptr, ref ptr2);
				if (buffer29[num26].life == 0)
				{
					this.lancerLaserOneShots.Remove(num26);
				}
			}
		}
		if (this.lancerLaserOneShots.count == 0 && this.lancerLaserOneShots.capacity > 256)
		{
			this.lancerLaserOneShots.Flush();
		}
		int cursor30 = this.lancerLaserSweeps.cursor;
		SpaceLaserSweep[] buffer30 = this.lancerLaserSweeps.buffer;
		for (int num27 = 1; num27 < cursor30; num27++)
		{
			ref SpaceLaserSweep ptr6 = ref buffer30[num27];
			if (ptr6.id == num27)
			{
				ptr6.TickSkillLogic(this, ref ptr, ref ptr2);
				if (ptr6.life == 0)
				{
					this.lancerLaserSweeps.Remove(num27);
				}
			}
		}
		if (this.lancerLaserSweeps.count == 0 && this.lancerLaserSweeps.capacity > 256)
		{
			this.lancerLaserSweeps.Flush();
		}
		int cursor31 = this.humpbackProjectiles.cursor;
		GeneralExpImpProjectile[] buffer31 = this.humpbackProjectiles.buffer;
		for (int num28 = 1; num28 < cursor31; num28++)
		{
			if (buffer31[num28].id == num28)
			{
				buffer31[num28].TickSkillLogic(this, factories, this.gameData, ref ptr, ref ptr2);
				if (buffer31[num28].life == 0)
				{
					this.humpbackProjectiles.Remove(num28);
				}
			}
		}
		if (this.humpbackProjectiles.count == 0 && this.humpbackProjectiles.capacity > 256)
		{
			this.humpbackProjectiles.Flush();
		}
		int cursor32 = this.warshipTypeFLasers.cursor;
		SpaceLaserOneShot[] buffer32 = this.warshipTypeFLasers.buffer;
		for (int num29 = 1; num29 < cursor32; num29++)
		{
			if (buffer32[num29].id == num29)
			{
				buffer32[num29].TickSkillLogic(this, ref ptr, ref ptr2);
				if (buffer32[num29].life == 0)
				{
					this.warshipTypeFLasers.Remove(num29);
				}
			}
		}
		if (this.warshipTypeFLasers.count == 0 && this.warshipTypeFLasers.capacity > 256)
		{
			this.warshipTypeFLasers.Flush();
		}
		int cursor33 = this.warshipTypeFPlasmas.cursor;
		GeneralProjectile[] buffer33 = this.warshipTypeFPlasmas.buffer;
		for (int num30 = 1; num30 < cursor33; num30++)
		{
			if (buffer33[num30].id == num30)
			{
				buffer33[num30].TickSkillLogic(this, this.gameData, ref ptr, ref ptr2);
				if (buffer33[num30].life == 0)
				{
					buffer33[num30].HandleRemoving(this);
					this.warshipTypeFPlasmas.Remove(num30);
				}
			}
		}
		if (this.warshipTypeFPlasmas.count == 0 && this.warshipTypeFPlasmas.capacity > 256)
		{
			this.warshipTypeFPlasmas.Flush();
		}
		int cursor34 = this.warshipTypeAPlasmas.cursor;
		GeneralProjectile[] buffer34 = this.warshipTypeAPlasmas.buffer;
		for (int num31 = 1; num31 < cursor34; num31++)
		{
			if (buffer34[num31].id == num31)
			{
				buffer34[num31].TickSkillLogic(this, this.gameData, ref ptr, ref ptr2);
				if (buffer34[num31].life == 0)
				{
					buffer34[num31].HandleRemoving(this);
					this.warshipTypeAPlasmas.Remove(num31);
				}
			}
		}
		if (this.warshipTypeAPlasmas.count == 0 && this.warshipTypeAPlasmas.capacity > 256)
		{
			this.warshipTypeAPlasmas.Flush();
		}
		int cursor35 = this.turretGaussProjectiles.cursor;
		LocalGeneralProjectile[] buffer35 = this.turretGaussProjectiles.buffer;
		for (int num32 = 1; num32 < cursor35; num32++)
		{
			if (buffer35[num32].id == num32)
			{
				buffer35[num32].TickSkillLogic(this);
				if (buffer35[num32].life == 0)
				{
					buffer35[num32].HandleRemoving(this);
					this.ClearReferencesOnSkillRemove(num32, buffer35[num32].astroId, ESkillType.TurretGaussProjectiles);
					this.turretGaussProjectiles.Remove(num32);
				}
			}
		}
		if (this.turretGaussProjectiles.count == 0 && this.turretGaussProjectiles.capacity > 256)
		{
			this.turretGaussProjectiles.Flush();
		}
		int cursor36 = this.turretLaserContinuous.cursor;
		LocalLaserContinuous[] buffer36 = this.turretLaserContinuous.buffer;
		for (int num33 = 1; num33 < cursor36; num33++)
		{
			if (buffer36[num33].id == num33)
			{
				buffer36[num33].TickSkillLogic(this, factories);
				if (buffer36[num33].fade == 0f)
				{
					this.turretLaserContinuous.Remove(num33);
				}
			}
		}
		if (this.turretLaserContinuous.count == 0 && this.turretLaserContinuous.capacity > 256)
		{
			this.turretLaserContinuous.Flush();
		}
		int cursor37 = this.turretCannonades.cursor;
		LocalCannonade[] buffer37 = this.turretCannonades.buffer;
		for (int num34 = 1; num34 < cursor37; num34++)
		{
			if (buffer37[num34].id == num34)
			{
				buffer37[num34].TickSkillLogic(this, factories);
				if (buffer37[num34].life == 0)
				{
					this.turretCannonades.Remove(num34);
				}
			}
		}
		if (this.turretCannonades.count == 0 && this.turretCannonades.capacity > 256)
		{
			this.turretCannonades.Flush();
		}
		int cursor38 = this.turretPlasmas.cursor;
		GeneralProjectile[] buffer38 = this.turretPlasmas.buffer;
		for (int num35 = 1; num35 < cursor38; num35++)
		{
			if (buffer38[num35].id == num35)
			{
				buffer38[num35].TickSkillLogic(this, this.gameData, ref ptr, ref ptr2);
				if (buffer38[num35].life == 0)
				{
					buffer38[num35].HandleRemoving(this);
					this.turretPlasmas.Remove(num35);
				}
			}
		}
		if (this.turretPlasmas.count == 0 && this.turretPlasmas.capacity > 256)
		{
			this.turretPlasmas.Flush();
		}
		int cursor39 = this.turretLocalPlasmas.cursor;
		GeneralProjectile[] buffer39 = this.turretLocalPlasmas.buffer;
		for (int num36 = 1; num36 < cursor39; num36++)
		{
			if (buffer39[num36].id == num36)
			{
				buffer39[num36].TickSkillLogic(this, this.gameData, ref ptr, ref ptr2);
				if (buffer39[num36].life == 0)
				{
					buffer39[num36].HandleRemoving(this);
					this.turretLocalPlasmas.Remove(num36);
				}
			}
		}
		if (this.turretLocalPlasmas.count == 0 && this.turretLocalPlasmas.capacity > 256)
		{
			this.turretLocalPlasmas.Flush();
		}
		int cursor40 = this.turretMissiles.cursor;
		GeneralMissile[] buffer40 = this.turretMissiles.buffer;
		this.turretMissileTrails.SetTrailCapacity(this.turretMissiles.capacity);
		this.turretMissileTrails.trailCursor = cursor40;
		int trailStride = this.turretMissileTrails.trailStride;
		for (int num37 = 1; num37 < cursor40; num37++)
		{
			if (buffer40[num37].id == num37)
			{
				buffer40[num37].TickSkillLogic(this, this.turretMissileTrails, this.sector, ref ptr, ref ptr2, time);
				if (buffer40[num37].life == 0)
				{
					buffer40[num37].HandleRemoving(this);
					this.ClearReferencesOnSkillRemove(num37, buffer40[num37].target.astroId, ESkillType.TurretMissiles);
					this.turretMissiles.Remove(num37);
					Array.Clear(this.turretMissileTrails.smokePool, trailStride * num37, trailStride);
				}
			}
		}
		if (this.turretMissiles.count == 0 && this.turretMissiles.capacity > 256)
		{
			this.turretMissiles.Flush();
		}
		int cursor41 = this.turretDisturbingWave.cursor;
		LocalDisturbingWave[] buffer41 = this.turretDisturbingWave.buffer;
		for (int num38 = 1; num38 < cursor41; num38++)
		{
			if (buffer41[num38].id == num38)
			{
				buffer41[num38].TickSkillLogic(this);
				if (buffer41[num38].life == 0)
				{
					this.turretDisturbingWave.Remove(num38);
				}
			}
		}
		if (this.turretDisturbingWave.count == 0 && this.turretDisturbingWave.capacity > 256)
		{
			this.turretDisturbingWave.Flush();
		}
		int cursor42 = this.mechaLocalGaussProjectiles.cursor;
		LocalGeneralProjectile[] buffer42 = this.mechaLocalGaussProjectiles.buffer;
		for (int num39 = 1; num39 < cursor42; num39++)
		{
			if (buffer42[num39].id == num39)
			{
				buffer42[num39].TickSkillLogic(this);
				if (buffer42[num39].life == 0)
				{
					buffer42[num39].HandleRemoving(this);
					this.ClearReferencesOnSkillRemove(num39, buffer42[num39].astroId, ESkillType.MechaLocalGaussProjectiles);
					this.mechaLocalGaussProjectiles.Remove(num39);
				}
			}
		}
		if (this.mechaLocalGaussProjectiles.count == 0 && this.mechaLocalGaussProjectiles.capacity > 256)
		{
			this.mechaLocalGaussProjectiles.Flush();
		}
		int cursor43 = this.mechaSpaceGaussProjectiles.cursor;
		GeneralProjectile[] buffer43 = this.mechaSpaceGaussProjectiles.buffer;
		for (int num40 = 1; num40 < cursor43; num40++)
		{
			if (buffer43[num40].id == num40)
			{
				buffer43[num40].TickSkillLogic(this, this.gameData, ref ptr, ref ptr2);
				if (buffer43[num40].life == 0)
				{
					buffer43[num40].HandleRemoving(this);
					this.mechaSpaceGaussProjectiles.Remove(num40);
				}
			}
		}
		if (this.mechaSpaceGaussProjectiles.count == 0 && this.mechaSpaceGaussProjectiles.capacity > 256)
		{
			this.mechaSpaceGaussProjectiles.Flush();
		}
		int cursor44 = this.mechaLocalLaserOneShots.cursor;
		LocalLaserOneShot[] buffer44 = this.mechaLocalLaserOneShots.buffer;
		for (int num41 = 1; num41 < cursor44; num41++)
		{
			if (buffer44[num41].id == num41)
			{
				buffer44[num41].TickSkillLogic(this, factories);
				if (buffer44[num41].life == 0)
				{
					this.mechaLocalLaserOneShots.Remove(num41);
				}
			}
		}
		if (this.mechaLocalLaserOneShots.count == 0 && this.mechaLocalLaserOneShots.capacity > 256)
		{
			this.mechaLocalLaserOneShots.Flush();
		}
		int cursor45 = this.mechaSpaceLaserOneShots.cursor;
		SpaceLaserOneShot[] buffer45 = this.mechaSpaceLaserOneShots.buffer;
		for (int num42 = 1; num42 < cursor45; num42++)
		{
			if (buffer45[num42].id == num42)
			{
				buffer45[num42].TickSkillLogic(this, ref ptr, ref ptr2);
				if (buffer45[num42].life == 0)
				{
					this.mechaSpaceLaserOneShots.Remove(num42);
				}
			}
		}
		if (this.mechaSpaceLaserOneShots.count == 0 && this.mechaSpaceLaserOneShots.capacity > 256)
		{
			this.mechaSpaceLaserOneShots.Flush();
		}
		int cursor46 = this.mechaLocalCannonades.cursor;
		LocalCannonade[] buffer46 = this.mechaLocalCannonades.buffer;
		for (int num43 = 1; num43 < cursor46; num43++)
		{
			if (buffer46[num43].id == num43)
			{
				buffer46[num43].TickSkillLogic(this, factories);
				if (buffer46[num43].life == 0)
				{
					this.mechaLocalCannonades.Remove(num43);
				}
			}
		}
		if (this.mechaLocalCannonades.count == 0 && this.mechaLocalCannonades.capacity > 256)
		{
			this.mechaLocalCannonades.Flush();
		}
		int cursor47 = this.mechaSpaceCannonades.cursor;
		GeneralCannonade[] buffer47 = this.mechaSpaceCannonades.buffer;
		for (int num44 = 1; num44 < cursor47; num44++)
		{
			if (buffer47[num44].id == num44)
			{
				buffer47[num44].TickSkillLogic(this, ref ptr, ref ptr2);
				if (buffer47[num44].life == 0)
				{
					this.mechaSpaceCannonades.Remove(num44);
				}
			}
		}
		if (this.mechaSpaceCannonades.count == 0 && this.mechaSpaceCannonades.capacity > 256)
		{
			this.mechaSpaceCannonades.Flush();
		}
		int cursor48 = this.mechaPlasmas.cursor;
		GeneralProjectile[] buffer48 = this.mechaPlasmas.buffer;
		for (int num45 = 1; num45 < cursor48; num45++)
		{
			if (buffer48[num45].id == num45)
			{
				buffer48[num45].TickSkillLogic(this, this.gameData, ref ptr, ref ptr2);
				if (buffer48[num45].life == 0)
				{
					buffer48[num45].HandleRemoving(this);
					this.ClearReferencesOnSkillRemove(num45, buffer48[num45].target.astroId, ESkillType.MechaPlasmas);
					this.mechaPlasmas.Remove(num45);
				}
			}
		}
		if (this.mechaPlasmas.count == 0 && this.mechaPlasmas.capacity > 256)
		{
			this.mechaPlasmas.Flush();
		}
		int cursor49 = this.mechaMissiles.cursor;
		GeneralMissile[] buffer49 = this.mechaMissiles.buffer;
		this.mechaMissileTrails.SetTrailCapacity(this.mechaMissiles.capacity);
		this.mechaMissileTrails.trailCursor = cursor49;
		int trailStride2 = this.mechaMissileTrails.trailStride;
		for (int num46 = 1; num46 < cursor49; num46++)
		{
			if (buffer49[num46].id == num46)
			{
				buffer49[num46].TickSkillLogic(this, this.mechaMissileTrails, this.sector, ref ptr, ref ptr2, time);
				if (buffer49[num46].life == 0)
				{
					buffer49[num46].HandleRemoving(this);
					this.ClearReferencesOnSkillRemove(num46, buffer49[num46].target.astroId, ESkillType.MechaMissiles);
					this.mechaMissiles.Remove(num46);
					Array.Clear(this.mechaMissileTrails.smokePool, trailStride2 * num46, trailStride2);
				}
			}
		}
		if (this.mechaMissiles.count == 0 && this.mechaMissiles.capacity > 256)
		{
			this.mechaMissiles.Flush();
		}
		int cursor50 = this.mechaShieldBursts.cursor;
		GeneralShieldBurst[] buffer50 = this.mechaShieldBursts.buffer;
		for (int num47 = 1; num47 < cursor50; num47++)
		{
			if (buffer50[num47].id == num47)
			{
				buffer50[num47].TickSkillLogic(this);
				if (buffer50[num47].life == 0)
				{
					this.mechaShieldBursts.Remove(num47);
				}
			}
		}
		if (this.mechaShieldBursts.count == 0 && this.mechaShieldBursts.capacity > 256)
		{
			this.mechaShieldBursts.Flush();
		}
		int cursor51 = this.combatStats.cursor;
		CombatStat[] buffer51 = this.combatStats.buffer;
		for (int num48 = 1; num48 < cursor51; num48++)
		{
			if (buffer51[num48].id == num48)
			{
				buffer51[num48].TickSkillLogic(this.gameData, this);
			}
		}
	}

	// Token: 0x06000E6A RID: 3690 RVA: 0x000DAEAF File Offset: 0x000D90AF
	public void OnRemovingSkillTarget(SkillTarget removingSkillTarget)
	{
		if (!this.removedSkillTargets.Contains(removingSkillTarget))
		{
			this.removedSkillTargets.Add(removingSkillTarget);
		}
	}

	// Token: 0x06000E6B RID: 3691 RVA: 0x000DAECC File Offset: 0x000D90CC
	public void OnRemovingSkillTarget(int removingId, int removingAstroId, ETargetType removingTargetType)
	{
		SkillTarget item;
		item.id = removingId;
		item.astroId = removingAstroId;
		item.type = removingTargetType;
		if (!this.removedSkillTargets.Contains(item))
		{
			this.removedSkillTargets.Add(item);
		}
	}

	// Token: 0x06000E6C RID: 3692 RVA: 0x000DAF0C File Offset: 0x000D910C
	public void PrepareTick()
	{
		this.ClearRemovedSkillTargetsReferences();
		this.removedSkillTargets.Clear();
	}

	// Token: 0x06000E6D RID: 3693 RVA: 0x000DAF1F File Offset: 0x000D911F
	public void AfterTick()
	{
		this.ClearRemovedSkillTargetsReferences();
		this.removedSkillTargets.Clear();
	}

	// Token: 0x06000E6E RID: 3694 RVA: 0x000DAF34 File Offset: 0x000D9134
	public void ClearRemovedSkillTargetsReferences()
	{
		int cursor = this.localGeneralProjectiles.cursor;
		LocalGeneralProjectile[] buffer = this.localGeneralProjectiles.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref LocalGeneralProjectile ptr = ref buffer[i];
			if (ptr.id == i)
			{
				ptr.HandleSkillTargetRemove(this);
			}
		}
		int cursor2 = this.localLaserOneShots.cursor;
		LocalLaserOneShot[] buffer2 = this.localLaserOneShots.buffer;
		for (int j = 1; j < cursor2; j++)
		{
			ref LocalLaserOneShot ptr2 = ref buffer2[j];
			if (ptr2.id == j)
			{
				ptr2.HandleSkillTargetRemove(this);
			}
		}
		int cursor3 = this.localCannonades.cursor;
		LocalCannonade[] buffer3 = this.localCannonades.buffer;
		for (int k = 1; k < cursor3; k++)
		{
			ref LocalCannonade ptr3 = ref buffer3[k];
			if (ptr3.id == k)
			{
				ptr3.HandleSkillTargetRemove(this);
			}
		}
		int cursor4 = this.localDisturbingWaves.cursor;
		LocalDisturbingWave[] buffer4 = this.localDisturbingWaves.buffer;
		for (int l = 1; l < cursor4; l++)
		{
			ref LocalDisturbingWave ptr4 = ref buffer4[l];
			if (ptr4.id == l)
			{
				ptr4.HandleSkillTargetRemove(this);
			}
		}
		int cursor5 = this.generalProjectiles.cursor;
		GeneralProjectile[] buffer5 = this.generalProjectiles.buffer;
		for (int m = 1; m < cursor5; m++)
		{
			ref GeneralProjectile ptr5 = ref buffer5[m];
			if (ptr5.id == m)
			{
				ptr5.HandleSkillTargetRemove(this);
			}
		}
		int cursor6 = this.spaceLaserOneShots.cursor;
		SpaceLaserOneShot[] buffer6 = this.spaceLaserOneShots.buffer;
		for (int n = 1; n < cursor6; n++)
		{
			ref SpaceLaserOneShot ptr6 = ref buffer6[n];
			if (ptr6.id == n)
			{
				ptr6.HandleSkillTargetRemove(this);
			}
		}
		int cursor7 = this.explosiveUnitBombs.cursor;
		Bomb_Explosive[] buffer7 = this.explosiveUnitBombs.buffer;
		for (int num = 1; num < cursor7; num++)
		{
			ref Bomb_Explosive ptr7 = ref buffer7[num];
			if (ptr7.id == num)
			{
				ptr7.HandleSkillTargetRemove(this);
			}
		}
		int cursor8 = this.emCapsuleBombs.cursor;
		Bomb_EMCapsule[] buffer8 = this.emCapsuleBombs.buffer;
		for (int num2 = 1; num2 < cursor8; num2++)
		{
			ref Bomb_EMCapsule ptr8 = ref buffer8[num2];
			if (ptr8.id == num2)
			{
				ptr8.HandleSkillTargetRemove(this);
			}
		}
		int cursor9 = this.liquidBombs.cursor;
		Bomb_Liquid[] buffer9 = this.liquidBombs.buffer;
		for (int num3 = 1; num3 < cursor9; num3++)
		{
			ref Bomb_Liquid ptr9 = ref buffer9[num3];
			if (ptr9.id == num3)
			{
				ptr9.HandleSkillTargetRemove(this);
			}
		}
		int cursor10 = this.raiderLasers.cursor;
		LocalLaserOneShot[] buffer10 = this.raiderLasers.buffer;
		for (int num4 = 1; num4 < cursor10; num4++)
		{
			ref LocalLaserOneShot ptr10 = ref buffer10[num4];
			if (ptr10.id == num4)
			{
				ptr10.HandleSkillTargetRemove(this);
			}
		}
		int cursor11 = this.rangerPlasmas.cursor;
		LocalGeneralProjectile[] buffer11 = this.rangerPlasmas.buffer;
		for (int num5 = 1; num5 < cursor11; num5++)
		{
			ref LocalGeneralProjectile ptr11 = ref buffer11[num5];
			if (ptr11.id == num5)
			{
				ptr11.HandleSkillTargetRemove(this);
			}
		}
		int cursor12 = this.guardianPlasmas.cursor;
		LocalGeneralProjectile[] buffer12 = this.guardianPlasmas.buffer;
		for (int num6 = 1; num6 < cursor12; num6++)
		{
			ref LocalGeneralProjectile ptr12 = ref buffer12[num6];
			if (ptr12.id == num6)
			{
				ptr12.HandleSkillTargetRemove(this);
			}
		}
		int cursor13 = this.dfgTowerLasers.cursor;
		LocalLaserOneShot[] buffer13 = this.dfgTowerLasers.buffer;
		for (int num7 = 1; num7 < cursor13; num7++)
		{
			ref LocalLaserOneShot ptr13 = ref buffer13[num7];
			if (ptr13.id == num7)
			{
				ptr13.HandleSkillTargetRemove(this);
			}
		}
		int cursor14 = this.dfgTowerPlasmas.cursor;
		LocalGeneralProjectile[] buffer14 = this.dfgTowerPlasmas.buffer;
		for (int num8 = 1; num8 < cursor14; num8++)
		{
			ref LocalGeneralProjectile ptr14 = ref buffer14[num8];
			if (ptr14.id == num8)
			{
				ptr14.HandleSkillTargetRemove(this);
			}
		}
		int cursor15 = this.fighterLasers.cursor;
		LocalLaserOneShot[] buffer15 = this.fighterLasers.buffer;
		for (int num9 = 1; num9 < cursor15; num9++)
		{
			ref LocalLaserOneShot ptr15 = ref buffer15[num9];
			if (ptr15.id == num9)
			{
				ptr15.HandleSkillTargetRemove(this);
			}
		}
		int cursor16 = this.fighterPlasmas.cursor;
		LocalGeneralProjectile[] buffer16 = this.fighterPlasmas.buffer;
		for (int num10 = 1; num10 < cursor16; num10++)
		{
			ref LocalGeneralProjectile ptr16 = ref buffer16[num10];
			if (ptr16.id == num10)
			{
				ptr16.HandleSkillTargetRemove(this);
			}
		}
		int cursor17 = this.fighterShieldPlasmas.cursor;
		LocalGeneralProjectile[] buffer17 = this.fighterShieldPlasmas.buffer;
		for (int num11 = 1; num11 < cursor17; num11++)
		{
			ref LocalGeneralProjectile ptr17 = ref buffer17[num11];
			if (ptr17.id == num11)
			{
				ptr17.HandleSkillTargetRemove(this);
			}
		}
		int cursor18 = this.turretGaussProjectiles.cursor;
		LocalGeneralProjectile[] buffer18 = this.turretGaussProjectiles.buffer;
		for (int num12 = 1; num12 < cursor18; num12++)
		{
			ref LocalGeneralProjectile ptr18 = ref buffer18[num12];
			if (ptr18.id == num12)
			{
				ptr18.HandleSkillTargetRemove(this);
			}
		}
		int cursor19 = this.turretCannonades.cursor;
		LocalCannonade[] buffer19 = this.turretCannonades.buffer;
		for (int num13 = 1; num13 < cursor19; num13++)
		{
			ref LocalCannonade ptr19 = ref buffer19[num13];
			if (ptr19.id == num13)
			{
				ptr19.HandleSkillTargetRemove(this);
			}
		}
		int cursor20 = this.turretPlasmas.cursor;
		GeneralProjectile[] buffer20 = this.turretPlasmas.buffer;
		for (int num14 = 1; num14 < cursor20; num14++)
		{
			ref GeneralProjectile ptr20 = ref buffer20[num14];
			if (ptr20.id == num14)
			{
				ptr20.HandleSkillTargetRemove(this);
			}
		}
		int cursor21 = this.turretLocalPlasmas.cursor;
		GeneralProjectile[] buffer21 = this.turretLocalPlasmas.buffer;
		for (int num15 = 1; num15 < cursor21; num15++)
		{
			ref GeneralProjectile ptr21 = ref buffer21[num15];
			if (ptr21.id == num15)
			{
				ptr21.HandleSkillTargetRemove(this);
			}
		}
		int cursor22 = this.turretMissiles.cursor;
		GeneralMissile[] buffer22 = this.turretMissiles.buffer;
		for (int num16 = 1; num16 < cursor22; num16++)
		{
			ref GeneralMissile ptr22 = ref buffer22[num16];
			if (ptr22.id == num16)
			{
				ptr22.HandleSkillTargetRemove(this);
			}
		}
		int cursor23 = this.turretDisturbingWave.cursor;
		LocalDisturbingWave[] buffer23 = this.turretDisturbingWave.buffer;
		for (int num17 = 1; num17 < cursor23; num17++)
		{
			ref LocalDisturbingWave ptr23 = ref buffer23[num17];
			if (ptr23.id == num17)
			{
				ptr23.HandleSkillTargetRemove(this);
			}
		}
		int cursor24 = this.dfsTowerPlasmas.cursor;
		GeneralProjectile[] buffer24 = this.dfsTowerPlasmas.buffer;
		for (int num18 = 1; num18 < cursor24; num18++)
		{
			ref GeneralProjectile ptr24 = ref buffer24[num18];
			if (ptr24.id == num18)
			{
				ptr24.HandleSkillTargetRemove(this);
			}
		}
		int cursor25 = this.dfsTowerLasers.cursor;
		SpaceLaserOneShot[] buffer25 = this.dfsTowerLasers.buffer;
		for (int num19 = 1; num19 < cursor25; num19++)
		{
			ref SpaceLaserOneShot ptr25 = ref buffer25[num19];
			if (ptr25.id == num19)
			{
				ptr25.HandleSkillTargetRemove(this);
			}
		}
		int cursor26 = this.lancerSpacePlasma.cursor;
		GeneralProjectile[] buffer26 = this.lancerSpacePlasma.buffer;
		for (int num20 = 1; num20 < cursor26; num20++)
		{
			ref GeneralProjectile ptr26 = ref buffer26[num20];
			if (ptr26.id == num20)
			{
				ptr26.HandleSkillTargetRemove(this);
			}
		}
		int cursor27 = this.lancerLaserOneShots.cursor;
		SpaceLaserOneShot[] buffer27 = this.lancerLaserOneShots.buffer;
		for (int num21 = 1; num21 < cursor27; num21++)
		{
			ref SpaceLaserOneShot ptr27 = ref buffer27[num21];
			if (ptr27.id == num21)
			{
				ptr27.HandleSkillTargetRemove(this);
			}
		}
		int cursor28 = this.humpbackProjectiles.cursor;
		GeneralExpImpProjectile[] buffer28 = this.humpbackProjectiles.buffer;
		for (int num22 = 1; num22 < cursor28; num22++)
		{
			ref GeneralExpImpProjectile ptr28 = ref buffer28[num22];
			if (ptr28.id == num22)
			{
				ptr28.HandleSkillTargetRemove(this);
			}
		}
		int cursor29 = this.warshipTypeFLasers.cursor;
		SpaceLaserOneShot[] buffer29 = this.warshipTypeFLasers.buffer;
		for (int num23 = 1; num23 < cursor29; num23++)
		{
			ref SpaceLaserOneShot ptr29 = ref buffer29[num23];
			if (ptr29.id == num23)
			{
				ptr29.HandleSkillTargetRemove(this);
			}
		}
		int cursor30 = this.warshipTypeFPlasmas.cursor;
		GeneralProjectile[] buffer30 = this.warshipTypeFPlasmas.buffer;
		for (int num24 = 1; num24 < cursor30; num24++)
		{
			ref GeneralProjectile ptr30 = ref buffer30[num24];
			if (ptr30.id == num24)
			{
				ptr30.HandleSkillTargetRemove(this);
			}
		}
		int cursor31 = this.warshipTypeAPlasmas.cursor;
		GeneralProjectile[] buffer31 = this.warshipTypeAPlasmas.buffer;
		for (int num25 = 1; num25 < cursor31; num25++)
		{
			ref GeneralProjectile ptr31 = ref buffer31[num25];
			if (ptr31.id == num25)
			{
				ptr31.HandleSkillTargetRemove(this);
			}
		}
		int cursor32 = this.mechaLocalGaussProjectiles.cursor;
		LocalGeneralProjectile[] buffer32 = this.mechaLocalGaussProjectiles.buffer;
		for (int num26 = 1; num26 < cursor32; num26++)
		{
			ref LocalGeneralProjectile ptr32 = ref buffer32[num26];
			if (ptr32.id == num26)
			{
				ptr32.HandleSkillTargetRemove(this);
			}
		}
		int cursor33 = this.mechaSpaceGaussProjectiles.cursor;
		GeneralProjectile[] buffer33 = this.mechaSpaceGaussProjectiles.buffer;
		for (int num27 = 1; num27 < cursor33; num27++)
		{
			ref GeneralProjectile ptr33 = ref buffer33[num27];
			if (ptr33.id == num27)
			{
				ptr33.HandleSkillTargetRemove(this);
			}
		}
		int cursor34 = this.mechaLocalLaserOneShots.cursor;
		LocalLaserOneShot[] buffer34 = this.mechaLocalLaserOneShots.buffer;
		for (int num28 = 1; num28 < cursor34; num28++)
		{
			ref LocalLaserOneShot ptr34 = ref buffer34[num28];
			if (ptr34.id == num28)
			{
				ptr34.HandleSkillTargetRemove(this);
			}
		}
		int cursor35 = this.mechaSpaceLaserOneShots.cursor;
		SpaceLaserOneShot[] buffer35 = this.mechaSpaceLaserOneShots.buffer;
		for (int num29 = 1; num29 < cursor35; num29++)
		{
			ref SpaceLaserOneShot ptr35 = ref buffer35[num29];
			if (ptr35.id == num29)
			{
				ptr35.HandleSkillTargetRemove(this);
			}
		}
		int cursor36 = this.mechaLocalCannonades.cursor;
		LocalCannonade[] buffer36 = this.mechaLocalCannonades.buffer;
		for (int num30 = 1; num30 < cursor36; num30++)
		{
			ref LocalCannonade ptr36 = ref buffer36[num30];
			if (ptr36.id == num30)
			{
				ptr36.HandleSkillTargetRemove(this);
			}
		}
		int cursor37 = this.mechaPlasmas.cursor;
		GeneralProjectile[] buffer37 = this.mechaPlasmas.buffer;
		for (int num31 = 1; num31 < cursor37; num31++)
		{
			ref GeneralProjectile ptr37 = ref buffer37[num31];
			if (ptr37.id == num31)
			{
				ptr37.HandleSkillTargetRemove(this);
			}
		}
		int cursor38 = this.mechaMissiles.cursor;
		GeneralMissile[] buffer38 = this.mechaMissiles.buffer;
		for (int num32 = 1; num32 < cursor38; num32++)
		{
			ref GeneralMissile ptr38 = ref buffer38[num32];
			if (ptr38.id == num32)
			{
				ptr38.HandleSkillTargetRemove(this);
			}
		}
	}

	// Token: 0x06000E6F RID: 3695 RVA: 0x000DBA0C File Offset: 0x000D9C0C
	public int DeterminePlanetATFieldRaytestInStar(int starAstroId, ERayTestSkillType skillType, int skillId, ref VectorLF3 beginPosU, ref VectorLF3 endPosU, int targetAstroId = 0)
	{
		int result = 0;
		AstroData[] galaxyAstros = this.sector.galaxyAstros;
		int num;
		int num2;
		if (targetAstroId == 0)
		{
			num = starAstroId + 1;
			StarData starData = this.sector.galaxy.StarById(starAstroId / 100);
			num2 = (starAstroId + ((starData != null) ? new int?(starData.planetCount) : null)).GetValueOrDefault();
		}
		else
		{
			num2 = targetAstroId;
			num = targetAstroId;
		}
		VectorLF3 vectorLF = endPosU - beginPosU;
		double num3 = Math.Sqrt(vectorLF.x * vectorLF.x + vectorLF.y * vectorLF.y + vectorLF.z * vectorLF.z);
		vectorLF.x /= num3;
		vectorLF.y /= num3;
		vectorLF.z /= num3;
		for (int i = num; i <= num2; i++)
		{
			float num4 = galaxyAstros[i].uRadius + 60.8f + 10f;
			PlanetFactory planetFactory = this.astroFactories[i];
			PlanetATField planetATField = ((planetFactory != null) ? planetFactory.planetATField : null) ?? null;
			if (planetATField != null && !planetATField.isEmpty && planetATField.energy > 0L && Phys.RayCastSphereLF(ref beginPosU, ref vectorLF, num3, ref galaxyAstros[i].uPos, (double)num4))
			{
				if (!planetATField.rayTesting)
				{
					planetATField.BeginRayTests(900);
					planetATField.SetColliderHot(900);
					planetATField.SetPhysicsChangeSensitivity(120f);
				}
				VectorLF3 vec;
				this.sector.InverseTransformToAstro_ref(i, ref beginPosU, out vec);
				VectorLF3 vec2;
				this.sector.InverseTransformToAstro_ref(i, ref endPosU, out vec2);
				Vector3 vector = vec;
				Vector3 vector2 = vec2;
				float extendDist = (skillType == ERayTestSkillType.lancerLaserSweep || skillType == ERayTestSkillType.spaceLaserSweep) ? 500f : 0f;
				planetATField.AddRaycastTest(skillType, skillId, ref vector, ref vector2, extendDist);
				result = i;
			}
		}
		return result;
	}

	// Token: 0x06000E70 RID: 3696 RVA: 0x000DBC24 File Offset: 0x000D9E24
	public int DeterminePlanetATFieldRaytestInStar(int starAstroId, ERayTestSkillType skillType, int skillId, ref VectorLF3 uPos, ref Vector3 uVel, int targetAstroId = 0)
	{
		int result = 0;
		AstroData[] galaxyAstros = this.sector.galaxyAstros;
		int num;
		int num2;
		if (targetAstroId == 0)
		{
			num = starAstroId + 1;
			StarData starData = this.sector.galaxy.StarById(starAstroId / 100);
			num2 = (starAstroId + ((starData != null) ? new int?(starData.planetCount) : null)).GetValueOrDefault();
		}
		else
		{
			num2 = targetAstroId;
			num = targetAstroId;
		}
		VectorLF3 vectorLF = uVel;
		double num3 = Math.Sqrt(vectorLF.x * vectorLF.x + vectorLF.y * vectorLF.y + vectorLF.z * vectorLF.z);
		vectorLF.x /= num3;
		vectorLF.y /= num3;
		vectorLF.z /= num3;
		VectorLF3 rhs = uVel * 0.016666666666666666;
		VectorLF3 vectorLF2 = uPos + rhs;
		for (int i = num; i <= num2; i++)
		{
			float num4 = galaxyAstros[i].uRadius + 60.8f + 10f;
			PlanetFactory planetFactory = this.astroFactories[i];
			PlanetATField planetATField = ((planetFactory != null) ? planetFactory.planetATField : null) ?? null;
			if (planetATField != null && !planetATField.isEmpty && planetATField.energy > 0L && Phys.RayCastSphereLF(ref uPos, ref vectorLF, num3, ref galaxyAstros[i].uPos, (double)num4))
			{
				if (!planetATField.rayTesting)
				{
					planetATField.BeginRayTests(900);
					planetATField.SetColliderHot(900);
					planetATField.SetPhysicsChangeSensitivity(120f);
				}
				VectorLF3 vec;
				this.sector.InverseTransformToAstro_ref(i, ref vectorLF2, out vec);
				VectorLF3 vec2;
				vec2.x = uPos.x - planetATField.planetPrevUPos.x;
				vec2.y = uPos.y - planetATField.planetPrevUPos.y;
				vec2.z = uPos.z - planetATField.planetPrevUPos.z;
				Maths.QInvRotateLF_ref(ref planetATField.planetPrevURot, ref vec2, ref vec2);
				Vector3 vector = vec2;
				Vector3 vector2 = vec;
				float extendDist = (float)rhs.magnitude * 0.5f;
				planetATField.AddRaycastTest(skillType, skillId, ref vector, ref vector2, extendDist);
				result = i;
			}
		}
		return result;
	}

	// Token: 0x06000E71 RID: 3697 RVA: 0x000DBEA8 File Offset: 0x000DA0A8
	public void PlanetATFieldRaycastHitCallback(int factoryAstroId, int rayId, ref PlanetATField.RayTest ray)
	{
		switch (ray.skillType)
		{
		case ERayTestSkillType.generalProjectile:
			this.generalProjectiles.buffer[ray.skillId].atfRayId = rayId;
			return;
		case ERayTestSkillType.spaceLaserOneShot:
		{
			ref SpaceLaserOneShot ptr = ref this.spaceLaserOneShots.buffer[ray.skillId];
			if (ptr.atfRayId == 0)
			{
				ptr.atfAstroId = factoryAstroId;
				ptr.atfRayId = rayId;
				return;
			}
			float planetATFieldRaycastHitDist = this.GetPlanetATFieldRaycastHitDist(ptr.atfAstroId, ptr.atfRayId);
			if (ray.dist < planetATFieldRaycastHitDist)
			{
				ptr.atfAstroId = factoryAstroId;
				ptr.atfRayId = rayId;
				return;
			}
			break;
		}
		case ERayTestSkillType.spaceLaserSweep:
		{
			ref SpaceLaserSweep ptr2 = ref this.spaceLaserSweeps.buffer[ray.skillId];
			if (ptr2.atfRayId == 0)
			{
				ptr2.atfAstroId = factoryAstroId;
				ptr2.atfRayId = rayId;
				return;
			}
			float planetATFieldRaycastHitDist2 = this.GetPlanetATFieldRaycastHitDist(ptr2.atfAstroId, ptr2.atfRayId);
			if (ray.dist < planetATFieldRaycastHitDist2)
			{
				ptr2.atfAstroId = factoryAstroId;
				ptr2.atfRayId = rayId;
				return;
			}
			break;
		}
		case ERayTestSkillType.lancerSpacePlasma:
		{
			ref GeneralProjectile ptr3 = ref this.lancerSpacePlasma.buffer[ray.skillId];
			if (ptr3.atfAstroId == factoryAstroId)
			{
				ptr3.atfRayId = rayId;
				return;
			}
			break;
		}
		case ERayTestSkillType.lancerLaserOneShot:
		{
			ref SpaceLaserOneShot ptr4 = ref this.lancerLaserOneShots.buffer[ray.skillId];
			if (ptr4.atfRayId == 0)
			{
				ptr4.atfAstroId = factoryAstroId;
				ptr4.atfRayId = rayId;
				return;
			}
			float planetATFieldRaycastHitDist3 = this.GetPlanetATFieldRaycastHitDist(ptr4.atfAstroId, ptr4.atfRayId);
			if (ray.dist < planetATFieldRaycastHitDist3)
			{
				ptr4.atfAstroId = factoryAstroId;
				ptr4.atfRayId = rayId;
				return;
			}
			break;
		}
		case ERayTestSkillType.lancerLaserSweep:
		{
			ref SpaceLaserSweep ptr5 = ref this.lancerLaserSweeps.buffer[ray.skillId];
			if (ptr5.atfRayId == 0)
			{
				ptr5.atfAstroId = factoryAstroId;
				ptr5.atfRayId = rayId;
				return;
			}
			float planetATFieldRaycastHitDist4 = this.GetPlanetATFieldRaycastHitDist(ptr5.atfAstroId, ptr5.atfRayId);
			if (ray.dist < planetATFieldRaycastHitDist4)
			{
				ptr5.atfAstroId = factoryAstroId;
				ptr5.atfRayId = rayId;
			}
			break;
		}
		case ERayTestSkillType.humpbackPlasma:
		{
			ref GeneralExpImpProjectile ptr6 = ref this.humpbackProjectiles.buffer[ray.skillId];
			if (ptr6.atfAstroId == factoryAstroId)
			{
				ptr6.atfRayId = rayId;
				return;
			}
			break;
		}
		default:
			return;
		}
	}

	// Token: 0x06000E72 RID: 3698 RVA: 0x000DC0DC File Offset: 0x000DA2DC
	private float GetPlanetATFieldRaycastHitDist(int atfAstroId, int atfRayId)
	{
		PlanetFactory planetFactory = this.astroFactories[atfAstroId];
		if (planetFactory != null && atfRayId > 0)
		{
			PlanetATField planetATField = planetFactory.planetATField;
			if (planetATField.rayTestCursor > atfRayId && planetATField.rayTests[atfRayId].testState > 0)
			{
				return planetATField.rayTests[atfRayId].dist;
			}
		}
		return float.MaxValue;
	}

	// Token: 0x06000E73 RID: 3699 RVA: 0x000DC134 File Offset: 0x000DA334
	public void UpdateDFSTowerLaserOneShotRenderingData()
	{
		if (this.dfsTowerLasers == null || this.dfsTowerLasers.count == 0)
		{
			this.dfsTowerLaserRenderer.instCursor = 0;
			return;
		}
		while (this.dfsTowerLaserRenderer.capacity < this.dfsTowerLasers.count)
		{
			this.dfsTowerLaserRenderer.ExpandInstanceArray();
		}
		VectorLF3 relativePos = this.gameData.relativePos;
		Quaternion relativeRot = this.gameData.relativeRot;
		int cursor = this.dfsTowerLasers.cursor;
		SpaceLaserOneShot[] buffer = this.dfsTowerLasers.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref SpaceLaserOneShot ptr = ref buffer[i];
			if (ptr.id == i)
			{
				ref SpaceLaserOneShotRenderingData ptr2 = ref this.dfsTowerLaserRenderer.instArr[this.dfsTowerLaserRenderer.instCursor];
				ptr2.id = ptr.id;
				ptr2.life = ptr.life;
				VectorLF3 vectorLF = ptr.beginPosU - relativePos;
				VectorLF3 vectorLF2 = ptr.endPosU + ptr.deltaPos - relativePos;
				Maths.QInvRotateLF_ref(ref relativeRot, ref vectorLF, ref ptr2.beginPos);
				Maths.QInvRotateLF_ref(ref relativeRot, ref vectorLF2, ref ptr2.finalPos);
				this.dfsTowerLaserRenderer.instCursor++;
			}
		}
	}

	// Token: 0x06000E74 RID: 3700 RVA: 0x000DC280 File Offset: 0x000DA480
	public void UpdateLancerLaserOneShotRenderingData()
	{
		if (this.lancerLaserOneShots == null || this.lancerLaserOneShots.count == 0)
		{
			this.lancerLaserOneShotRenderer.instCursor = 0;
			return;
		}
		while (this.lancerLaserOneShotRenderer.capacity < this.lancerLaserOneShots.count)
		{
			this.lancerLaserOneShotRenderer.ExpandInstanceArray();
		}
		VectorLF3 relativePos = this.gameData.relativePos;
		Quaternion relativeRot = this.gameData.relativeRot;
		int cursor = this.lancerLaserOneShots.cursor;
		SpaceLaserOneShot[] buffer = this.lancerLaserOneShots.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref SpaceLaserOneShot ptr = ref buffer[i];
			if (ptr.id == i)
			{
				ref SpaceLaserOneShotRenderingData ptr2 = ref this.lancerLaserOneShotRenderer.instArr[this.lancerLaserOneShotRenderer.instCursor];
				ptr2.id = ptr.id;
				ptr2.life = ptr.life;
				VectorLF3 vectorLF = ptr.beginPosU - relativePos;
				VectorLF3 vectorLF2 = ptr.endPosU + ptr.deltaPos - relativePos;
				Maths.QInvRotateLF_ref(ref relativeRot, ref vectorLF, ref ptr2.beginPos);
				Maths.QInvRotateLF_ref(ref relativeRot, ref vectorLF2, ref ptr2.finalPos);
				this.lancerLaserOneShotRenderer.instCursor++;
			}
		}
	}

	// Token: 0x06000E75 RID: 3701 RVA: 0x000DC3CC File Offset: 0x000DA5CC
	public void UpdateSpaceLaserOneShotRenderingData()
	{
		if (this.spaceLaserOneShots == null || this.spaceLaserOneShots.count == 0)
		{
			this.spaceLaserOneShotRenderer.instCursor = 0;
			return;
		}
		while (this.spaceLaserOneShotRenderer.capacity < this.spaceLaserOneShots.count)
		{
			this.spaceLaserOneShotRenderer.ExpandInstanceArray();
		}
		VectorLF3 relativePos = this.gameData.relativePos;
		Quaternion relativeRot = this.gameData.relativeRot;
		int cursor = this.spaceLaserOneShots.cursor;
		SpaceLaserOneShot[] buffer = this.spaceLaserOneShots.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref SpaceLaserOneShot ptr = ref buffer[i];
			if (ptr.id == i)
			{
				ref SpaceLaserOneShotRenderingData ptr2 = ref this.spaceLaserOneShotRenderer.instArr[this.spaceLaserOneShotRenderer.instCursor];
				ptr2.id = ptr.id;
				ptr2.life = ptr.life;
				VectorLF3 vectorLF = ptr.beginPosU - relativePos;
				VectorLF3 vectorLF2 = ptr.endPosU + ptr.deltaPos - relativePos;
				Maths.QInvRotateLF_ref(ref relativeRot, ref vectorLF, ref ptr2.beginPos);
				Maths.QInvRotateLF_ref(ref relativeRot, ref vectorLF2, ref ptr2.finalPos);
				this.spaceLaserOneShotRenderer.instCursor++;
			}
		}
	}

	// Token: 0x06000E76 RID: 3702 RVA: 0x000DC518 File Offset: 0x000DA718
	public void UpdateSpaceLaserSweepRenderingData()
	{
		if (this.spaceLaserSweeps == null || this.spaceLaserSweeps.count == 0)
		{
			this.spaceLaserSweepRenderer.instCursor = 0;
			return;
		}
		while (this.spaceLaserSweepRenderer.capacity < this.spaceLaserSweeps.count)
		{
			this.spaceLaserSweepRenderer.ExpandInstanceArray();
		}
		VectorLF3 relativePos = this.gameData.relativePos;
		Quaternion relativeRot = this.gameData.relativeRot;
		int cursor = this.spaceLaserSweeps.cursor;
		SpaceLaserSweep[] buffer = this.spaceLaserSweeps.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref SpaceLaserSweep ptr = ref buffer[i];
			if (ptr.id == i)
			{
				ref SpaceLaserSweepRenderingData ptr2 = ref this.spaceLaserSweepRenderer.instArr[this.spaceLaserSweepRenderer.instCursor];
				ptr2.id = ptr.id;
				ptr2.life = ptr.life;
				VectorLF3 lhs;
				this.sector.TransformFromAstro(ptr.astroId, out lhs, ptr.beginPos);
				VectorLF3 lhs2;
				this.sector.TransformFromAstro(ptr.astroId, out lhs2, ptr.endPos);
				Vector3 vector = lhs - relativePos;
				Vector3 vector2 = lhs2 - relativePos;
				Maths.QInvRotate_ref(ref relativeRot, ref vector, ref ptr2.beginPos);
				Maths.QInvRotate_ref(ref relativeRot, ref vector2, ref ptr2.endPos);
				this.spaceLaserSweepRenderer.instCursor++;
			}
		}
	}

	// Token: 0x06000E77 RID: 3703 RVA: 0x000DC688 File Offset: 0x000DA888
	public void UpdateLancerLaserSweepRenderingData()
	{
		if (this.lancerLaserSweeps == null || this.lancerLaserSweeps.count == 0)
		{
			this.lancerLaserSweepRenderer.instCursor = 0;
			return;
		}
		while (this.lancerLaserSweepRenderer.capacity < this.lancerLaserSweeps.count)
		{
			this.lancerLaserSweepRenderer.ExpandInstanceArray();
		}
		VectorLF3 relativePos = this.gameData.relativePos;
		Quaternion relativeRot = this.gameData.relativeRot;
		int cursor = this.lancerLaserSweeps.cursor;
		SpaceLaserSweep[] buffer = this.lancerLaserSweeps.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref SpaceLaserSweep ptr = ref buffer[i];
			if (ptr.id == i)
			{
				ref SpaceLaserSweepRenderingData ptr2 = ref this.lancerLaserSweepRenderer.instArr[this.lancerLaserSweepRenderer.instCursor];
				ptr2.id = ptr.id;
				ptr2.life = ptr.life;
				VectorLF3 lhs;
				this.sector.TransformFromAstro(ptr.astroId, out lhs, ptr.beginPos);
				VectorLF3 lhs2;
				this.sector.TransformFromAstro(ptr.astroId, out lhs2, ptr.endPos);
				Vector3 vector = lhs - relativePos;
				Vector3 vector2 = lhs2 - relativePos;
				Maths.QInvRotate_ref(ref relativeRot, ref vector, ref ptr2.beginPos);
				Maths.QInvRotate_ref(ref relativeRot, ref vector2, ref ptr2.endPos);
				this.lancerLaserSweepRenderer.instCursor++;
			}
		}
	}

	// Token: 0x06000E78 RID: 3704 RVA: 0x000DC7F8 File Offset: 0x000DA9F8
	public void UpdateMechaSpaceLaserRenderingData()
	{
		if (this.mechaSpaceLaserOneShots == null || this.mechaSpaceLaserOneShots.count == 0)
		{
			this.mechaSpaceLaserOneShotRenderer.instCursor = 0;
			return;
		}
		while (this.mechaSpaceLaserOneShotRenderer.capacity < this.mechaSpaceLaserOneShots.count)
		{
			this.mechaSpaceLaserOneShotRenderer.ExpandInstanceArray();
		}
		VectorLF3 relativePos = this.gameData.relativePos;
		Quaternion relativeRot = this.gameData.relativeRot;
		int cursor = this.mechaSpaceLaserOneShots.cursor;
		SpaceLaserOneShot[] buffer = this.mechaSpaceLaserOneShots.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref SpaceLaserOneShot ptr = ref buffer[i];
			if (ptr.id == i)
			{
				ref SpaceLaserOneShotRenderingData ptr2 = ref this.mechaSpaceLaserOneShotRenderer.instArr[this.mechaSpaceLaserOneShotRenderer.instCursor];
				ptr2.id = ptr.id;
				ptr2.life = ptr.life;
				VectorLF3 vectorLF = ptr.beginPosU - relativePos;
				VectorLF3 vectorLF2 = ptr.endPosU + ptr.deltaPos - relativePos;
				Maths.QInvRotateLF_ref(ref relativeRot, ref vectorLF, ref ptr2.beginPos);
				Maths.QInvRotateLF_ref(ref relativeRot, ref vectorLF2, ref ptr2.finalPos);
				this.mechaSpaceLaserOneShotRenderer.instCursor++;
			}
		}
	}

	// Token: 0x06000E79 RID: 3705 RVA: 0x000DC944 File Offset: 0x000DAB44
	public void UpdateExplosiveUnitBombRenderingData()
	{
		if (this.explosiveUnitBombs == null || this.explosiveUnitBombs.count == 0)
		{
			this.explosiveUnitBombRenderer.instCursor = 0;
			return;
		}
		while (this.explosiveUnitBombRenderer.capacity < this.explosiveUnitBombs.count)
		{
			this.explosiveUnitBombRenderer.ExpandInstanceArray();
		}
		VectorLF3 relativePos = this.gameData.relativePos;
		Quaternion relativeRot = this.gameData.relativeRot;
		int cursor = this.explosiveUnitBombs.cursor;
		Bomb_Explosive[] buffer = this.explosiveUnitBombs.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref Bomb_Explosive ptr = ref buffer[i];
			if (ptr.id == i)
			{
				ref GeneralBombRenderingData ptr2 = ref this.explosiveUnitBombRenderer.instArr[this.explosiveUnitBombRenderer.instCursor];
				ptr2.id = ptr.id;
				ptr2.type = ptr.bombModelId;
				ptr2.life = ptr.life;
				Vector3 pos = ptr2.pos;
				VectorLF3 vectorLF = ptr.uPos - relativePos;
				Maths.QInvRotateLF_ref(ref relativeRot, ref vectorLF, ref ptr2.pos);
				ptr2.vel = ptr2.pos - pos;
				ptr2.rot = Quaternion.Inverse(relativeRot) * ptr.uRot;
				this.explosiveUnitBombRenderer.instCursor++;
			}
		}
	}

	// Token: 0x06000E7A RID: 3706 RVA: 0x000DCAA4 File Offset: 0x000DACA4
	public void UpdateEMCapsuleBombRenderingData()
	{
		if (this.emCapsuleBombs == null || this.emCapsuleBombs.count == 0)
		{
			this.emCapsuleBombRenderer.instCursor = 0;
			return;
		}
		while (this.emCapsuleBombRenderer.capacity < this.emCapsuleBombs.count)
		{
			this.emCapsuleBombRenderer.ExpandInstanceArray();
		}
		VectorLF3 relativePos = this.gameData.relativePos;
		Quaternion relativeRot = this.gameData.relativeRot;
		int cursor = this.emCapsuleBombs.cursor;
		Bomb_EMCapsule[] buffer = this.emCapsuleBombs.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref Bomb_EMCapsule ptr = ref buffer[i];
			if (ptr.id == i)
			{
				ref GeneralBombRenderingData ptr2 = ref this.emCapsuleBombRenderer.instArr[this.emCapsuleBombRenderer.instCursor];
				ptr2.id = ptr.id;
				ptr2.type = ptr.bombModelId;
				ptr2.life = ptr.life;
				Vector3 pos = ptr2.pos;
				VectorLF3 vectorLF = ptr.uPos - relativePos;
				Maths.QInvRotateLF_ref(ref relativeRot, ref vectorLF, ref ptr2.pos);
				ptr2.vel = ptr2.pos - pos;
				ptr2.rot = Quaternion.Inverse(relativeRot) * ptr.uRot;
				this.emCapsuleBombRenderer.instCursor++;
			}
		}
	}

	// Token: 0x06000E7B RID: 3707 RVA: 0x000DCC04 File Offset: 0x000DAE04
	public void UpdateLiquidBombRenderingData()
	{
		if (this.liquidBombs == null || this.liquidBombs.count == 0)
		{
			this.liquidBombRenderer.instCursor = 0;
			return;
		}
		while (this.liquidBombRenderer.capacity < this.liquidBombs.count)
		{
			this.liquidBombRenderer.ExpandInstanceArray();
		}
		VectorLF3 relativePos = this.gameData.relativePos;
		Quaternion relativeRot = this.gameData.relativeRot;
		int cursor = this.liquidBombs.cursor;
		Bomb_Liquid[] buffer = this.liquidBombs.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref Bomb_Liquid ptr = ref buffer[i];
			if (ptr.id == i)
			{
				ref GeneralBombRenderingData ptr2 = ref this.liquidBombRenderer.instArr[this.liquidBombRenderer.instCursor];
				ptr2.id = ptr.id;
				ptr2.type = ptr.bombModelId;
				ptr2.life = ptr.life;
				Vector3 pos = ptr2.pos;
				VectorLF3 vectorLF = ptr.uPos - relativePos;
				Maths.QInvRotateLF_ref(ref relativeRot, ref vectorLF, ref ptr2.pos);
				ptr2.vel = ptr2.pos - pos;
				ptr2.rot = Quaternion.Inverse(relativeRot) * ptr.uRot;
				this.liquidBombRenderer.instCursor++;
			}
		}
	}

	// Token: 0x06000E7C RID: 3708 RVA: 0x000DCD64 File Offset: 0x000DAF64
	public void UpdateTurretMissileRenderingData()
	{
		if (this.turretMissiles == null || this.turretMissiles.count == 0)
		{
			this.turretMissileRenderer.instCursor = 0;
			return;
		}
		while (this.turretMissileRenderer.capacity < this.turretMissiles.count)
		{
			this.turretMissileRenderer.ExpandInstanceArray();
		}
		VectorLF3 relativePos = this.gameData.relativePos;
		Quaternion relativeRot = this.gameData.relativeRot;
		int cursor = this.turretMissiles.cursor;
		GeneralMissile[] buffer = this.turretMissiles.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref GeneralMissile ptr = ref buffer[i];
			if (ptr.id == i)
			{
				ref GeneralMissileRenderingData ptr2 = ref this.turretMissileRenderer.instArr[this.turretMissileRenderer.instCursor];
				ptr2.id = ptr.id;
				ptr2.type = ptr.modelIndex - 431;
				ptr2.life = ptr.life;
				ptr2.vel = ptr.vel;
				VectorLF3 vectorLF = ptr.uPos - relativePos;
				Maths.QInvRotateLF_ref(ref relativeRot, ref vectorLF, ref ptr2.pos);
				ptr2.rot = Quaternion.Inverse(relativeRot) * ptr.uRot;
				this.turretMissileRenderer.instCursor++;
			}
		}
	}

	// Token: 0x06000E7D RID: 3709 RVA: 0x000DCEBC File Offset: 0x000DB0BC
	public void UpdateMechaMissileRenderingData()
	{
		if (this.mechaMissiles == null || this.mechaMissiles.count == 0)
		{
			this.mechaMissileRenderer.instCursor = 0;
			return;
		}
		while (this.mechaMissileRenderer.capacity < this.mechaMissiles.count)
		{
			this.mechaMissileRenderer.ExpandInstanceArray();
		}
		VectorLF3 relativePos = this.gameData.relativePos;
		Quaternion relativeRot = this.gameData.relativeRot;
		int cursor = this.mechaMissiles.cursor;
		GeneralMissile[] buffer = this.mechaMissiles.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref GeneralMissile ptr = ref buffer[i];
			if (ptr.id == i)
			{
				ref GeneralMissileRenderingData ptr2 = ref this.mechaMissileRenderer.instArr[this.mechaMissileRenderer.instCursor];
				ptr2.id = ptr.id;
				ptr2.type = ptr.modelIndex - 431;
				ptr2.life = ptr.life;
				ptr2.vel = ptr.vel;
				VectorLF3 vectorLF = ptr.uPos - relativePos;
				Maths.QInvRotateLF_ref(ref relativeRot, ref vectorLF, ref ptr2.pos);
				ptr2.rot = Quaternion.Inverse(relativeRot) * ptr.uRot;
				this.mechaMissileRenderer.instCursor++;
			}
		}
	}

	// Token: 0x06000E7E RID: 3710 RVA: 0x000DD014 File Offset: 0x000DB214
	public void UpdateWarshipTypeFLaserRenderingData()
	{
		if (this.warshipTypeFLasers == null || this.warshipTypeFLasers.count == 0)
		{
			this.warshipTypeFLaserRenderer.instCursor = 0;
			return;
		}
		while (this.warshipTypeFLaserRenderer.capacity < this.warshipTypeFLasers.count)
		{
			this.warshipTypeFLaserRenderer.ExpandInstanceArray();
		}
		VectorLF3 relativePos = this.gameData.relativePos;
		Quaternion relativeRot = this.gameData.relativeRot;
		int cursor = this.warshipTypeFLasers.cursor;
		SpaceLaserOneShot[] buffer = this.warshipTypeFLasers.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref SpaceLaserOneShot ptr = ref buffer[i];
			if (ptr.id == i)
			{
				ref SpaceLaserOneShotRenderingData ptr2 = ref this.warshipTypeFLaserRenderer.instArr[this.warshipTypeFLaserRenderer.instCursor];
				ptr2.id = ptr.id;
				ptr2.life = ptr.life;
				VectorLF3 vectorLF = ptr.beginPosU - relativePos;
				VectorLF3 vectorLF2 = ptr.endPosU + ptr.deltaPos - relativePos;
				Maths.QInvRotateLF_ref(ref relativeRot, ref vectorLF, ref ptr2.beginPos);
				Maths.QInvRotateLF_ref(ref relativeRot, ref vectorLF2, ref ptr2.finalPos);
				this.warshipTypeFLaserRenderer.instCursor++;
			}
		}
	}

	// Token: 0x06000E7F RID: 3711 RVA: 0x000DD160 File Offset: 0x000DB360
	public void NotifyObjectKilled(SkillTarget killer, int objectType, int objectAstroId, int objectId)
	{
		int astroId = killer.astroId;
		if (astroId <= 1000000 && astroId > 100 && astroId <= 204899 && astroId % 100 > 0)
		{
			PlanetFactory planetFactory = this.astroFactories[astroId];
			if (killer.type == ETargetType.None && objectType == 4)
			{
				int turretId = planetFactory.entityPool[killer.id].turretId;
				if (turretId > 0)
				{
					ref TurretComponent ptr = ref planetFactory.defenseSystem.turrets.buffer[turretId];
					if (ptr.id == turretId)
					{
						ptr.totalKillCount++;
					}
				}
			}
		}
		if (objectAstroId > 1000000)
		{
			if (objectType == 4)
			{
				ref EnemyData ptr2 = ref this.sector.enemyPool[objectId];
				if (ptr2.id != 0 && ptr2.id == objectId && this.onEnemyKilled != null)
				{
					this.onEnemyKilled(killer, ref ptr2);
					return;
				}
			}
		}
		else if (objectAstroId > 100 && objectAstroId <= 204899 && objectAstroId % 100 > 0)
		{
			if (objectType == 0)
			{
				ref EntityData entity = ref this.astroFactories[objectAstroId].entityPool[objectId];
				if (this.onEntityKilled != null)
				{
					this.onEntityKilled(killer, ref entity);
					return;
				}
			}
			else if (objectType == 4)
			{
				PlanetFactory planetFactory2 = this.astroFactories[objectAstroId];
				if (planetFactory2 != null)
				{
					ref EnemyData ptr3 = ref planetFactory2.enemyPool[objectId];
					if (ptr3.id != 0 && ptr3.id == objectId && this.onEnemyKilled != null)
					{
						this.onEnemyKilled(killer, ref ptr3);
					}
				}
			}
		}
	}

	// Token: 0x06000E80 RID: 3712 RVA: 0x000DD2D8 File Offset: 0x000DB4D8
	public void ClearReferencesOnSkillRemove(int id, int astroId, ESkillType type)
	{
		if (type <= ESkillType.TurretMissiles)
		{
			if (type != ESkillType.None)
			{
				switch (type)
				{
				case ESkillType.LocalGeneralProjectiles:
				case ESkillType.LocalLaserContinuous:
				case ESkillType.LocalLaserOneShots:
				case ESkillType.LocalCannonades:
				case ESkillType.GeneralProjectiles:
				case ESkillType.SpaceLaserOneShots:
				case ESkillType.SpaceLaserSweeps:
					break;
				default:
					switch (type)
					{
					case ESkillType.FighterPlasmas:
						this.ClearEnemyBaseIncomingSkillReferences(id, astroId, type);
						this.ClearEnemyUnitBlockSkillReferences(id, astroId, type);
						return;
					case ESkillType.FighterShieldPlasmas:
						this.ClearEnemyBaseIncomingSkillReferences(id, astroId, type);
						this.ClearEnemyUnitBlockSkillReferences(id, astroId, type);
						return;
					case ESkillType.TurretGaussProjectiles:
						this.ClearEnemyBaseIncomingSkillReferences(id, astroId, type);
						this.ClearEnemyUnitBlockSkillReferences(id, astroId, type);
						return;
					case ESkillType.TurretLaserContinuous:
					case ESkillType.TurretCannonades:
					case ESkillType.TurretPlasmas:
						break;
					case ESkillType.TurretMissiles:
						this.ClearEnemyBaseIncomingSkillReferences(id, astroId, type);
						this.ClearEnemyUnitBlockSkillReferences(id, astroId, type);
						return;
					default:
						return;
					}
					break;
				}
			}
		}
		else
		{
			if (type == ESkillType.MechaLocalGaussProjectiles)
			{
				this.ClearEnemyBaseIncomingSkillReferences(id, astroId, type);
				this.ClearEnemyUnitBlockSkillReferences(id, astroId, type);
				return;
			}
			if (type == ESkillType.MechaPlasmas)
			{
				this.ClearEnemyBaseIncomingSkillReferences(id, astroId, type);
				this.ClearEnemyUnitBlockSkillReferences(id, astroId, type);
				return;
			}
			if (type != ESkillType.MechaMissiles)
			{
				return;
			}
			this.ClearEnemyBaseIncomingSkillReferences(id, astroId, type);
			this.ClearEnemyUnitBlockSkillReferences(id, astroId, type);
		}
	}

	// Token: 0x06000E81 RID: 3713 RVA: 0x000DD3D8 File Offset: 0x000DB5D8
	public void ClearEnemyBaseIncomingSkillReferences(int id, int astroId, ESkillType type)
	{
		if (astroId > 100 && astroId <= 204899 && astroId % 100 > 0)
		{
			EnemyDFGroundSystem enemySystem = this.astroFactories[astroId].enemySystem;
			DFGBaseComponent[] buffer = enemySystem.bases.buffer;
			int cursor = enemySystem.bases.cursor;
			for (int i = 1; i < cursor; i++)
			{
				if (buffer[i] != null && buffer[i].id == i)
				{
					buffer[i].RemoveIncomingSkill(id, type);
				}
			}
		}
	}

	// Token: 0x06000E82 RID: 3714 RVA: 0x000DD444 File Offset: 0x000DB644
	public void ClearEnemyUnitBlockSkillReferences(int id, int astroId, ESkillType type)
	{
		if (astroId > 100 && astroId <= 204899 && astroId % 100 > 0)
		{
			EnemyDFGroundSystem enemySystem = this.astroFactories[astroId].enemySystem;
			EnemyUnitComponent[] buffer = enemySystem.units.buffer;
			int cursor = enemySystem.units.cursor;
			for (int i = 1; i < cursor; i++)
			{
				if (buffer[i].id == i && buffer[i].behavior == EEnemyBehavior.Defense)
				{
					buffer[i].ClearBlockSkill(id, type);
				}
			}
		}
	}

	// Token: 0x06000E83 RID: 3715 RVA: 0x000DD4C4 File Offset: 0x000DB6C4
	public void MissileSearchSpaceTarget(ref GeneralMissile missile, float searchRange)
	{
		int lastTargetCurrentAstroId = missile.lastTargetCurrentAstroId;
		int num = -1;
		if (lastTargetCurrentAstroId > 1000000)
		{
			EnemyDFHiveSystem enemyDFHiveSystem = this.sector.dfHivesByAstro[lastTargetCurrentAstroId - 1000000];
			if (enemyDFHiveSystem != null)
			{
				num = enemyDFHiveSystem.starData.index;
			}
		}
		else if (lastTargetCurrentAstroId >= 100 && lastTargetCurrentAstroId <= 204899)
		{
			num = lastTargetCurrentAstroId / 100 - 1;
		}
		if (num >= 0)
		{
			EnemyDFHiveSystem enemyDFHiveSystem2 = this.sector.dfHives[num];
			EnemyData[] enemyPool = this.sector.enemyPool;
			float num2 = searchRange * searchRange;
			VectorLF3 vectorLF;
			this.sector.TransformFromAstro_ref(lastTargetCurrentAstroId, out vectorLF, ref missile.lastTargetPos);
			double x = vectorLF.x;
			double y = vectorLF.y;
			double z = vectorLF.z;
			while (enemyDFHiveSystem2 != null)
			{
				EnemyUnitComponent[] buffer = enemyDFHiveSystem2.units.buffer;
				int cursor = enemyDFHiveSystem2.units.cursor;
				for (int i = 1; i < cursor; i++)
				{
					ref EnemyUnitComponent ptr = ref buffer[i];
					if (ptr.id == i && ptr.enemyId > 0)
					{
						ref EnemyData ptr2 = ref enemyPool[ptr.enemyId];
						if (ptr2.id == ptr.enemyId)
						{
							VectorLF3 vectorLF2;
							this.sector.TransformFromAstro_ref(ptr2.astroId, out vectorLF2, ref ptr2.pos);
							float num3 = (float)(vectorLF2.x - x);
							float num4 = num3 * num3;
							if (num4 <= num2)
							{
								float num5 = (float)(vectorLF2.y - y);
								float num6 = num5 * num5;
								if (num6 <= num2)
								{
									float num7 = (float)(vectorLF2.z - z);
									float num8 = num7 * num7;
									if (num8 <= num2 && num4 + num6 + num8 <= num2)
									{
										missile.target.type = ETargetType.Enemy;
										missile.target.id = ptr2.id;
										missile.target.astroId = ptr2.originAstroId;
										return;
									}
								}
							}
						}
					}
				}
				enemyDFHiveSystem2 = enemyDFHiveSystem2.nextSibling;
			}
		}
	}

	// Token: 0x06000E84 RID: 3716 RVA: 0x000DD694 File Offset: 0x000DB894
	public void MissileSearchGroundTarget(ref GeneralMissile missile, float searchRange)
	{
		PlanetFactory planetFactory = this.astroFactories[missile.lastTargetCurrentAstroId];
		if (planetFactory != null)
		{
			EnemyData[] enemyPool = planetFactory.enemyPool;
			Vector3 vector = missile.lastTargetPos;
			float num = searchRange * searchRange;
			float x = vector.x;
			float y = vector.y;
			float z = vector.z;
			HashSystem hashSystemDynamic = planetFactory.hashSystemDynamic;
			hashSystemDynamic.ClearActiveBuckets();
			int[] hashPool = hashSystemDynamic.hashPool;
			int[] bucketOffsets = hashSystemDynamic.bucketOffsets;
			int[] bucketCursors = hashSystemDynamic.bucketCursors;
			hashSystemDynamic.GetBucketIdxesInArea(vector, searchRange);
			int[] activeBuckets = hashSystemDynamic.activeBuckets;
			int activeBucketsCount = hashSystemDynamic.activeBucketsCount;
			for (int i = 0; i < activeBucketsCount; i++)
			{
				int num2 = activeBuckets[i];
				int num3 = bucketOffsets[num2];
				int num4 = num3 + bucketCursors[num2];
				for (int j = num3; j < num4; j++)
				{
					int num5 = hashPool[j];
					if (num5 != 0 && num5 >> 28 == 4)
					{
						int num6 = num5 & 268435455;
						if (num6 != 0)
						{
							ref EnemyData ptr = ref enemyPool[num6];
							if (ptr.id == num6 && !ptr.isInvincible)
							{
								float num7 = (float)ptr.pos.x - x;
								float num8 = (float)ptr.pos.y - y;
								float num9 = (float)ptr.pos.z - z;
								if (num7 * num7 + num8 * num8 + num9 * num9 <= num)
								{
									missile.target.type = ETargetType.Enemy;
									missile.target.id = ptr.id;
									missile.target.astroId = ptr.originAstroId;
									hashSystemDynamic.ClearActiveBuckets();
									return;
								}
							}
						}
					}
				}
			}
			hashSystemDynamic.ClearActiveBuckets();
			if (missile.target.id == 0)
			{
				HashSystem hashSystemStatic = planetFactory.hashSystemStatic;
				hashSystemStatic.ClearActiveBuckets();
				int[] hashPool2 = hashSystemStatic.hashPool;
				int[] bucketOffsets2 = hashSystemStatic.bucketOffsets;
				int[] bucketCursors2 = hashSystemStatic.bucketCursors;
				hashSystemStatic.GetBucketIdxesInArea(vector, searchRange);
				int[] activeBuckets2 = hashSystemStatic.activeBuckets;
				int activeBucketsCount2 = hashSystemStatic.activeBucketsCount;
				for (int k = 0; k < activeBucketsCount2; k++)
				{
					int num10 = activeBuckets2[k];
					int num11 = bucketOffsets2[num10];
					int num12 = num11 + bucketCursors2[num10];
					for (int l = num11; l < num12; l++)
					{
						int num13 = hashPool2[l];
						if (num13 != 0 && num13 >> 28 == 4)
						{
							int num14 = num13 & 268435455;
							if (num14 != 0)
							{
								ref EnemyData ptr2 = ref enemyPool[num14];
								if (ptr2.id == num14 && !ptr2.isInvincible)
								{
									float num15 = (float)ptr2.pos.x - x;
									float num16 = (float)ptr2.pos.y - y;
									float num17 = (float)ptr2.pos.z - z;
									if (num15 * num15 + num16 * num16 + num17 * num17 <= num)
									{
										missile.target.type = ETargetType.Enemy;
										missile.target.id = ptr2.id;
										missile.target.astroId = ptr2.originAstroId;
										hashSystemStatic.ClearActiveBuckets();
										return;
									}
								}
							}
						}
					}
				}
				hashSystemStatic.ClearActiveBuckets();
			}
		}
	}

	// Token: 0x06000E85 RID: 3717 RVA: 0x000DD9A0 File Offset: 0x000DBBA0
	public bool GetObjectUPosition(ref SkillTarget obj, out VectorLF3 upos)
	{
		if (obj.id == 0)
		{
			upos.x = 0.0;
			upos.y = 0.0;
			upos.z = 0.0;
			return false;
		}
		if (obj.type == ETargetType.Enemy)
		{
			int astroId = obj.astroId;
			if (astroId > 1000000 || astroId == 0)
			{
				ref EnemyData ptr = ref this.sector.enemyPool[obj.id];
				if (ptr.id == 0)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					return false;
				}
				obj.astroId = ptr.originAstroId;
				astroId = ptr.astroId;
				if (astroId > 1000000)
				{
					this.sector.astros[astroId - 1000000].PositionU(ref ptr.pos, out upos);
				}
				else if (astroId >= 100 && astroId <= 204899)
				{
					this.sector.galaxyAstros[astroId].PositionU(ref ptr.pos, out upos);
				}
				else
				{
					upos = ptr.pos;
				}
			}
			else
			{
				if (astroId <= 100 || astroId > 204899 || astroId % 100 <= 0)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					return false;
				}
				ref EnemyData ptr2 = ref this.astroFactories[astroId].enemyPool[obj.id];
				this.sector.galaxyAstros[astroId].PositionU(ref ptr2.pos, out upos);
			}
		}
		else if (obj.type == ETargetType.Craft)
		{
			int astroId2 = obj.astroId;
			if (astroId2 > 100 && astroId2 <= 204899 && astroId2 % 100 > 0)
			{
				ref CraftData ptr3 = ref this.astroFactories[astroId2].craftPool[obj.id];
				this.sector.galaxyAstros[astroId2].PositionU(ref ptr3.pos, out upos);
			}
			else
			{
				if (astroId2 % 100 != 0 && astroId2 <= 1000000)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					return false;
				}
				ref CraftData ptr4 = ref this.sector.craftPool[obj.id];
				if (ptr4.id == 0)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					return false;
				}
				obj.astroId = ptr4.astroId;
				astroId2 = ptr4.astroId;
				if (astroId2 > 1000000)
				{
					this.sector.astros[astroId2 - 1000000].PositionU(ref ptr4.pos, out upos);
				}
				else if (astroId2 >= 100 && astroId2 <= 204899)
				{
					this.sector.galaxyAstros[astroId2].PositionU(ref ptr4.pos, out upos);
				}
				else
				{
					upos = ptr4.pos;
				}
			}
		}
		else if (obj.type == ETargetType.None)
		{
			int astroId3 = obj.astroId;
			if (astroId3 <= 100 || astroId3 > 204899 || astroId3 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				return false;
			}
			ref EntityData ptr5 = ref this.astroFactories[astroId3].entityPool[obj.id];
			this.sector.galaxyAstros[astroId3].PositionU(ref ptr5.pos, out upos);
		}
		else if (obj.type == ETargetType.Vegetable)
		{
			int astroId4 = obj.astroId;
			if (astroId4 <= 100 || astroId4 > 204899 || astroId4 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				return false;
			}
			ref VegeData ptr6 = ref this.astroFactories[astroId4].vegePool[obj.id];
			this.sector.galaxyAstros[astroId4].PositionU(ref ptr6.pos, out upos);
		}
		else if (obj.type == ETargetType.Vein)
		{
			int astroId5 = obj.astroId;
			if (astroId5 <= 100 || astroId5 > 204899 || astroId5 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				return false;
			}
			ref VeinData ptr7 = ref this.astroFactories[astroId5].veinPool[obj.id];
			this.sector.galaxyAstros[astroId5].PositionU(ref ptr7.pos, out upos);
		}
		else if (obj.type == ETargetType.Prebuild)
		{
			int astroId6 = obj.astroId;
			if (astroId6 <= 100 || astroId6 > 204899 || astroId6 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				return false;
			}
			ref PrebuildData ptr8 = ref this.astroFactories[astroId6].prebuildPool[obj.id];
			this.sector.galaxyAstros[astroId6].PositionU(ref ptr8.pos, out upos);
		}
		else if (obj.type == ETargetType.Ruin)
		{
			int astroId7 = obj.astroId;
			if (astroId7 <= 100 || astroId7 > 204899 || astroId7 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				return false;
			}
			ref RuinData ptr9 = ref this.astroFactories[astroId7].ruinPool[obj.id];
			this.sector.galaxyAstros[astroId7].PositionU(ref ptr9.pos, out upos);
		}
		else
		{
			if (obj.type != ETargetType.Player)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				return false;
			}
			upos = this.playerSkillTargetU;
		}
		return true;
	}

	// Token: 0x06000E86 RID: 3718 RVA: 0x000DE014 File Offset: 0x000DC214
	public bool GetObjectUPose(ref SkillTarget obj, out VectorLF3 upos, out Quaternion urot)
	{
		if (obj.type == ETargetType.Enemy)
		{
			int astroId = obj.astroId;
			if (astroId > 1000000 || astroId == 0)
			{
				ref EnemyData ptr = ref this.sector.enemyPool[obj.id];
				if (ptr.id == 0)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					urot.x = 0f;
					urot.y = 0f;
					urot.z = 0f;
					urot.w = 1f;
					return false;
				}
				obj.astroId = ptr.originAstroId;
				astroId = ptr.astroId;
				this.sector.TransformFromAstro(astroId, out upos, out urot, ptr.pos, ptr.rot);
			}
			else
			{
				if (astroId <= 100 || astroId > 204899 || astroId % 100 <= 0)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					urot.x = 0f;
					urot.y = 0f;
					urot.z = 0f;
					urot.w = 1f;
					return false;
				}
				ref EnemyData ptr2 = ref this.astroFactories[astroId].enemyPool[obj.id];
				this.sector.TransformFromAstro(astroId, out upos, out urot, ptr2.pos, ptr2.rot);
			}
		}
		else if (obj.type == ETargetType.Craft)
		{
			int astroId2 = obj.astroId;
			if (astroId2 > 100 && astroId2 <= 204899 && astroId2 % 100 > 0)
			{
				ref CraftData ptr3 = ref this.astroFactories[astroId2].craftPool[obj.id];
				this.sector.TransformFromAstro(astroId2, out upos, out urot, ptr3.pos, ptr3.rot);
			}
			else
			{
				if (astroId2 % 100 != 0 && astroId2 <= 1000000)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					urot.x = 0f;
					urot.y = 0f;
					urot.z = 0f;
					urot.w = 1f;
					return false;
				}
				ref CraftData ptr4 = ref this.sector.craftPool[obj.id];
				if (ptr4.id == 0)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					urot.x = 0f;
					urot.y = 0f;
					urot.z = 0f;
					urot.w = 1f;
					return false;
				}
				obj.astroId = ptr4.astroId;
				astroId2 = ptr4.astroId;
				this.sector.TransformFromAstro(astroId2, out upos, out urot, ptr4.pos, ptr4.rot);
			}
		}
		else if (obj.type == ETargetType.None)
		{
			int astroId3 = obj.astroId;
			if (astroId3 <= 100 || astroId3 > 204899 || astroId3 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				urot.x = 0f;
				urot.y = 0f;
				urot.z = 0f;
				urot.w = 1f;
				return false;
			}
			ref EntityData ptr5 = ref this.astroFactories[astroId3].entityPool[obj.id];
			this.sector.TransformFromAstro(astroId3, out upos, out urot, ptr5.pos, ptr5.rot);
		}
		else if (obj.type == ETargetType.Vegetable)
		{
			int astroId4 = obj.astroId;
			if (astroId4 <= 100 || astroId4 > 204899 || astroId4 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				urot.x = 0f;
				urot.y = 0f;
				urot.z = 0f;
				urot.w = 1f;
				return false;
			}
			ref VegeData ptr6 = ref this.astroFactories[astroId4].vegePool[obj.id];
			this.sector.TransformFromAstro(astroId4, out upos, out urot, ptr6.pos, ptr6.rot);
		}
		else if (obj.type == ETargetType.Vein)
		{
			int astroId5 = obj.astroId;
			if (astroId5 <= 100 || astroId5 > 204899 || astroId5 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				urot.x = 0f;
				urot.y = 0f;
				urot.z = 0f;
				urot.w = 1f;
				return false;
			}
			ref VeinData ptr7 = ref this.astroFactories[astroId5].veinPool[obj.id];
			this.sector.galaxyAstros[astroId5].PositionU(ref ptr7.pos, out upos);
			urot.x = 0f;
			urot.y = 0f;
			urot.z = 0f;
			urot.w = 1f;
		}
		else if (obj.type == ETargetType.Prebuild)
		{
			int astroId6 = obj.astroId;
			if (astroId6 <= 100 || astroId6 > 204899 || astroId6 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				urot.x = 0f;
				urot.y = 0f;
				urot.z = 0f;
				urot.w = 1f;
				return false;
			}
			ref PrebuildData ptr8 = ref this.astroFactories[astroId6].prebuildPool[obj.id];
			this.sector.TransformFromAstro(astroId6, out upos, out urot, ptr8.pos, ptr8.rot);
		}
		else if (obj.type == ETargetType.Ruin)
		{
			int astroId7 = obj.astroId;
			if (astroId7 <= 100 || astroId7 > 204899 || astroId7 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				urot.x = 0f;
				urot.y = 0f;
				urot.z = 0f;
				urot.w = 1f;
				return false;
			}
			ref RuinData ptr9 = ref this.astroFactories[astroId7].ruinPool[obj.id];
			this.sector.TransformFromAstro(astroId7, out upos, out urot, ptr9.pos, ptr9.rot);
		}
		else
		{
			if (obj.type != ETargetType.Player)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				urot.x = 0f;
				urot.y = 0f;
				urot.z = 0f;
				urot.w = 1f;
				return false;
			}
			upos = this.playerSkillTargetU;
			urot = GameMain.mainPlayer.uRotation;
		}
		return true;
	}

	// Token: 0x06000E87 RID: 3719 RVA: 0x000DE7AC File Offset: 0x000DC9AC
	public void GetObjectLVelocity(ref SkillTarget obj, out Vector3 lvel)
	{
		if (obj.id == 0)
		{
			lvel.x = 0f;
			lvel.y = 0f;
			lvel.z = 0f;
		}
		if (obj.type == ETargetType.Enemy)
		{
			if (obj.astroId > 1000000 || obj.astroId == 0)
			{
				ref EnemyData ptr = ref this.sector.enemyPool[obj.id];
				if (ptr.id == 0)
				{
					lvel.x = 0f;
					lvel.y = 0f;
					lvel.z = 0f;
					return;
				}
				obj.astroId = ptr.originAstroId;
				lvel = ptr.vel;
				return;
			}
			else
			{
				if (obj.astroId > 100 && obj.astroId <= 204899 && obj.astroId % 100 > 0)
				{
					lvel = this.astroFactories[obj.astroId].enemyPool[obj.id].vel;
					return;
				}
				lvel.x = 0f;
				lvel.y = 0f;
				lvel.z = 0f;
				return;
			}
		}
		else if (obj.type == ETargetType.Craft)
		{
			if (obj.astroId % 100 == 0 || obj.astroId > 1000000)
			{
				ref CraftData ptr2 = ref this.sector.craftPool[obj.id];
				if (ptr2.id == 0)
				{
					lvel.x = 0f;
					lvel.y = 0f;
					lvel.z = 0f;
					return;
				}
				obj.astroId = ptr2.astroId;
				lvel = ptr2.vel;
				return;
			}
			else
			{
				if (obj.astroId > 100 && obj.astroId <= 204899 && obj.astroId % 100 > 0)
				{
					lvel = this.astroFactories[obj.astroId].craftPool[obj.id].vel;
					return;
				}
				lvel.x = 0f;
				lvel.y = 0f;
				lvel.z = 0f;
				return;
			}
		}
		else
		{
			if (obj.type == ETargetType.Player)
			{
				lvel = this.playerVelocityL;
				return;
			}
			lvel.x = 0f;
			lvel.y = 0f;
			lvel.z = 0f;
			return;
		}
	}

	// Token: 0x06000E88 RID: 3720 RVA: 0x000DE9F0 File Offset: 0x000DCBF0
	public void GetObjectLVelocity(ref SkillTargetLocal obj, PlanetFactory factory, out Vector3 lvel)
	{
		if (obj.type == ETargetType.Enemy && obj.id != 0)
		{
			lvel = factory.enemyPool[obj.id].vel;
		}
		if (obj.type == ETargetType.Craft && obj.id != 0)
		{
			lvel = factory.craftPool[obj.id].vel;
			return;
		}
		if (obj.type == ETargetType.Player)
		{
			lvel = this.playerVelocityL;
			return;
		}
		lvel.x = 0f;
		lvel.y = 0f;
		lvel.z = 0f;
	}

	// Token: 0x06000E89 RID: 3721 RVA: 0x000DEA90 File Offset: 0x000DCC90
	public void GetObjectUVelocity(ref SkillTarget obj, out Vector3 uvel)
	{
		if (obj.type == ETargetType.Enemy)
		{
			int astroId = obj.astroId;
			if (astroId > 1000000 || astroId == 0)
			{
				ref EnemyData ptr = ref this.sector.enemyPool[obj.id];
				if (ptr.id == 0)
				{
					uvel.x = 0f;
					uvel.y = 0f;
					uvel.z = 0f;
					return;
				}
				obj.astroId = ptr.originAstroId;
				astroId = ptr.astroId;
				if (astroId > 1000000)
				{
					this.sector.astros[astroId - 1000000].VelocityL2U(ref ptr.pos, ref ptr.vel, out uvel);
					return;
				}
				if (astroId >= 100 && astroId <= 204899)
				{
					this.sector.galaxyAstros[astroId].VelocityL2U(ref ptr.pos, ref ptr.vel, out uvel);
					return;
				}
				uvel = ptr.vel;
				return;
			}
			else
			{
				if (astroId > 100 && astroId <= 204899 && astroId % 100 > 0)
				{
					ref EnemyData ptr2 = ref this.astroFactories[astroId].enemyPool[obj.id];
					this.sector.galaxyAstros[astroId].VelocityL2U(ref ptr2.pos, ref ptr2.vel, out uvel);
					return;
				}
				uvel.x = 0f;
				uvel.y = 0f;
				uvel.z = 0f;
				return;
			}
		}
		else if (obj.type == ETargetType.Craft)
		{
			int astroId2 = obj.astroId;
			if (astroId2 > 100 && astroId2 <= 204899 && astroId2 % 100 > 0)
			{
				ref CraftData ptr3 = ref this.astroFactories[astroId2].craftPool[obj.id];
				this.sector.galaxyAstros[astroId2].VelocityL2U(ref ptr3.pos, ref ptr3.vel, out uvel);
				return;
			}
			if (astroId2 % 100 != 0 && astroId2 <= 1000000)
			{
				uvel.x = 0f;
				uvel.y = 0f;
				uvel.z = 0f;
				return;
			}
			ref CraftData ptr4 = ref this.sector.craftPool[obj.id];
			if (ptr4.id == 0)
			{
				uvel.x = 0f;
				uvel.y = 0f;
				uvel.z = 0f;
				return;
			}
			obj.astroId = ptr4.astroId;
			astroId2 = ptr4.astroId;
			if (astroId2 > 1000000)
			{
				this.sector.astros[astroId2 - 1000000].VelocityL2U(ref ptr4.pos, ref ptr4.vel, out uvel);
				return;
			}
			if (astroId2 >= 100 && astroId2 <= 204899)
			{
				this.sector.galaxyAstros[astroId2].VelocityL2U(ref ptr4.pos, ref ptr4.vel, out uvel);
				return;
			}
			uvel = ptr4.vel;
			return;
		}
		else if (obj.type == ETargetType.None)
		{
			int astroId3 = obj.astroId;
			if (astroId3 > 100 && astroId3 <= 204899 && astroId3 % 100 > 0)
			{
				ref EntityData ptr5 = ref this.astroFactories[astroId3].entityPool[obj.id];
				this.sector.galaxyAstros[astroId3].VelocityU(ref ptr5.pos, out uvel);
				return;
			}
			uvel.x = 0f;
			uvel.y = 0f;
			uvel.z = 0f;
			return;
		}
		else if (obj.type == ETargetType.Vegetable)
		{
			int astroId4 = obj.astroId;
			if (astroId4 > 100 && astroId4 <= 204899 && astroId4 % 100 > 0)
			{
				ref VegeData ptr6 = ref this.astroFactories[astroId4].vegePool[obj.id];
				this.sector.galaxyAstros[astroId4].VelocityU(ref ptr6.pos, out uvel);
				return;
			}
			uvel.x = 0f;
			uvel.y = 0f;
			uvel.z = 0f;
			return;
		}
		else if (obj.type == ETargetType.Vein)
		{
			int astroId5 = obj.astroId;
			if (astroId5 > 100 && astroId5 <= 204899 && astroId5 % 100 > 0)
			{
				ref VeinData ptr7 = ref this.astroFactories[astroId5].veinPool[obj.id];
				this.sector.galaxyAstros[astroId5].VelocityU(ref ptr7.pos, out uvel);
				return;
			}
			uvel.x = 0f;
			uvel.y = 0f;
			uvel.z = 0f;
			return;
		}
		else if (obj.type == ETargetType.Prebuild)
		{
			int astroId6 = obj.astroId;
			if (astroId6 > 100 && astroId6 <= 204899 && astroId6 % 100 > 0)
			{
				ref PrebuildData ptr8 = ref this.astroFactories[astroId6].prebuildPool[obj.id];
				this.sector.galaxyAstros[astroId6].VelocityU(ref ptr8.pos, out uvel);
				return;
			}
			uvel.x = 0f;
			uvel.y = 0f;
			uvel.z = 0f;
			return;
		}
		else if (obj.type == ETargetType.Ruin)
		{
			int astroId7 = obj.astroId;
			if (astroId7 > 100 && astroId7 <= 204899 && astroId7 % 100 > 0)
			{
				ref RuinData ptr9 = ref this.astroFactories[astroId7].ruinPool[obj.id];
				this.sector.galaxyAstros[astroId7].VelocityU(ref ptr9.pos, out uvel);
				return;
			}
			uvel.x = 0f;
			uvel.y = 0f;
			uvel.z = 0f;
			return;
		}
		else
		{
			if (obj.type == ETargetType.Player)
			{
				uvel = this.playerVelocityU;
				return;
			}
			uvel.x = 0f;
			uvel.y = 0f;
			uvel.z = 0f;
			return;
		}
	}

	// Token: 0x06000E8A RID: 3722 RVA: 0x000DF02C File Offset: 0x000DD22C
	public bool GetObjectUPositionAndVelocity(ref SkillTarget obj, out VectorLF3 upos, out Vector3 uvel)
	{
		if (obj.type == ETargetType.Enemy)
		{
			int astroId = obj.astroId;
			if (astroId > 1000000 || astroId == 0)
			{
				ref EnemyData ptr = ref this.sector.enemyPool[obj.id];
				if (ptr.id == 0)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					uvel.x = 0f;
					uvel.y = 0f;
					uvel.z = 0f;
					return false;
				}
				obj.astroId = ptr.originAstroId;
				astroId = ptr.astroId;
				if (astroId > 1000000)
				{
					AstroData[] astros = this.sector.astros;
					int num = astroId - 1000000;
					astros[num].PositionU(ref ptr.pos, out upos);
					astros[num].VelocityL2U(ref ptr.pos, ref ptr.vel, out uvel);
				}
				else if (astroId >= 100 && astroId <= 204899)
				{
					AstroData[] galaxyAstros = this.sector.galaxyAstros;
					int num2 = astroId;
					galaxyAstros[num2].PositionU(ref ptr.pos, out upos);
					galaxyAstros[num2].VelocityL2U(ref ptr.pos, ref ptr.vel, out uvel);
				}
				else
				{
					upos = ptr.pos;
					uvel = ptr.vel;
				}
			}
			else
			{
				if (astroId <= 100 || astroId > 204899 || astroId % 100 <= 0)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					uvel.x = 0f;
					uvel.y = 0f;
					uvel.z = 0f;
					return false;
				}
				ref EnemyData ptr2 = ref this.astroFactories[astroId].enemyPool[obj.id];
				if (ptr2.id == 0)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					uvel.x = 0f;
					uvel.y = 0f;
					uvel.z = 0f;
					return false;
				}
				AstroData[] galaxyAstros2 = this.sector.galaxyAstros;
				int num3 = astroId;
				galaxyAstros2[num3].PositionU(ref ptr2.pos, out upos);
				galaxyAstros2[num3].VelocityL2U(ref ptr2.pos, ref ptr2.vel, out uvel);
			}
		}
		else if (obj.type == ETargetType.Craft)
		{
			int astroId2 = obj.astroId;
			if (astroId2 > 100 && astroId2 <= 204899 && astroId2 % 100 > 0)
			{
				ref CraftData ptr3 = ref this.astroFactories[astroId2].craftPool[obj.id];
				if (ptr3.id == 0)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					uvel.x = 0f;
					uvel.y = 0f;
					uvel.z = 0f;
					return false;
				}
				AstroData[] galaxyAstros3 = this.sector.galaxyAstros;
				int num4 = astroId2;
				galaxyAstros3[num4].PositionU(ref ptr3.pos, out upos);
				galaxyAstros3[num4].VelocityL2U(ref ptr3.pos, ref ptr3.vel, out uvel);
			}
			else
			{
				if (astroId2 % 100 != 0 && astroId2 <= 1000000)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					uvel.x = 0f;
					uvel.y = 0f;
					uvel.z = 0f;
					return false;
				}
				ref CraftData ptr4 = ref this.sector.craftPool[obj.id];
				if (ptr4.id == 0)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					uvel.x = 0f;
					uvel.y = 0f;
					uvel.z = 0f;
					return false;
				}
				obj.astroId = ptr4.astroId;
				astroId2 = ptr4.astroId;
				if (astroId2 > 1000000)
				{
					AstroData[] astros2 = this.sector.astros;
					int num5 = astroId2 - 1000000;
					astros2[num5].PositionU(ref ptr4.pos, out upos);
					astros2[num5].VelocityL2U(ref ptr4.pos, ref ptr4.vel, out uvel);
				}
				else if (astroId2 >= 100 && astroId2 <= 204899)
				{
					AstroData[] galaxyAstros4 = this.sector.galaxyAstros;
					int num6 = astroId2;
					galaxyAstros4[num6].PositionU(ref ptr4.pos, out upos);
					galaxyAstros4[num6].VelocityL2U(ref ptr4.pos, ref ptr4.vel, out uvel);
				}
				else
				{
					upos = ptr4.pos;
					uvel = ptr4.vel;
				}
			}
		}
		else if (obj.type == ETargetType.None)
		{
			int astroId3 = obj.astroId;
			if (astroId3 <= 100 || astroId3 > 204899 || astroId3 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				uvel.x = 0f;
				uvel.y = 0f;
				uvel.z = 0f;
				return false;
			}
			ref EntityData ptr5 = ref this.astroFactories[astroId3].entityPool[obj.id];
			AstroData[] galaxyAstros5 = this.sector.galaxyAstros;
			int num7 = astroId3;
			galaxyAstros5[num7].PositionU(ref ptr5.pos, out upos);
			galaxyAstros5[num7].VelocityU(ref ptr5.pos, out uvel);
		}
		else if (obj.type == ETargetType.Vegetable)
		{
			int astroId4 = obj.astroId;
			if (astroId4 <= 100 || astroId4 > 204899 || astroId4 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				uvel.x = 0f;
				uvel.y = 0f;
				uvel.z = 0f;
				return false;
			}
			ref VegeData ptr6 = ref this.astroFactories[astroId4].vegePool[obj.id];
			AstroData[] galaxyAstros6 = this.sector.galaxyAstros;
			int num8 = astroId4;
			galaxyAstros6[num8].PositionU(ref ptr6.pos, out upos);
			galaxyAstros6[num8].VelocityU(ref ptr6.pos, out uvel);
		}
		else if (obj.type == ETargetType.Vein)
		{
			int astroId5 = obj.astroId;
			if (astroId5 <= 100 || astroId5 > 204899 || astroId5 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				uvel.x = 0f;
				uvel.y = 0f;
				uvel.z = 0f;
				return false;
			}
			ref VeinData ptr7 = ref this.astroFactories[astroId5].veinPool[obj.id];
			AstroData[] galaxyAstros7 = this.sector.galaxyAstros;
			int num9 = astroId5;
			galaxyAstros7[num9].PositionU(ref ptr7.pos, out upos);
			galaxyAstros7[num9].VelocityU(ref ptr7.pos, out uvel);
		}
		else if (obj.type == ETargetType.Prebuild)
		{
			int astroId6 = obj.astroId;
			if (astroId6 <= 100 || astroId6 > 204899 || astroId6 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				uvel.x = 0f;
				uvel.y = 0f;
				uvel.z = 0f;
				return false;
			}
			ref PrebuildData ptr8 = ref this.astroFactories[astroId6].prebuildPool[obj.id];
			AstroData[] galaxyAstros8 = this.sector.galaxyAstros;
			int num10 = astroId6;
			galaxyAstros8[num10].PositionU(ref ptr8.pos, out upos);
			galaxyAstros8[num10].VelocityU(ref ptr8.pos, out uvel);
		}
		else if (obj.type == ETargetType.Ruin)
		{
			int astroId7 = obj.astroId;
			if (astroId7 <= 100 || astroId7 > 204899 || astroId7 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				uvel.x = 0f;
				uvel.y = 0f;
				uvel.z = 0f;
				return false;
			}
			ref RuinData ptr9 = ref this.astroFactories[astroId7].ruinPool[obj.id];
			AstroData[] galaxyAstros9 = this.sector.galaxyAstros;
			int num11 = astroId7;
			galaxyAstros9[num11].PositionU(ref ptr9.pos, out upos);
			galaxyAstros9[num11].VelocityU(ref ptr9.pos, out uvel);
		}
		else
		{
			if (obj.type != ETargetType.Player)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				uvel.x = 0f;
				uvel.y = 0f;
				uvel.z = 0f;
				return false;
			}
			upos = this.playerSkillTargetU;
			uvel = this.playerVelocityU;
		}
		return true;
	}

	// Token: 0x06000E8B RID: 3723 RVA: 0x000DF95C File Offset: 0x000DDB5C
	public bool GetObjectUPositionAndVelocity(ref SkillTarget obj, out VectorLF3 upos, out Vector3 uvel_astro, out Vector3 uvel_obj)
	{
		if (obj.type == ETargetType.Enemy)
		{
			int astroId = obj.astroId;
			if (astroId > 1000000 || astroId == 0)
			{
				ref EnemyData ptr = ref this.sector.enemyPool[obj.id];
				if (ptr.id == 0)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					uvel_astro.x = 0f;
					uvel_astro.y = 0f;
					uvel_astro.z = 0f;
					uvel_obj.x = 0f;
					uvel_obj.y = 0f;
					uvel_obj.z = 0f;
					return false;
				}
				obj.astroId = ptr.originAstroId;
				astroId = ptr.astroId;
				if (astroId > 1000000)
				{
					AstroData[] astros = this.sector.astros;
					int num = astroId - 1000000;
					astros[num].PositionU(ref ptr.pos, out upos);
					astros[num].VelocityL2U(ref ptr.pos, ref ptr.vel, out uvel_astro, out uvel_obj);
				}
				else if (astroId >= 100 && astroId <= 204899)
				{
					AstroData[] galaxyAstros = this.sector.galaxyAstros;
					int num2 = astroId;
					galaxyAstros[num2].PositionU(ref ptr.pos, out upos);
					galaxyAstros[num2].VelocityL2U(ref ptr.pos, ref ptr.vel, out uvel_astro, out uvel_obj);
				}
				else
				{
					upos = ptr.pos;
					uvel_obj = ptr.vel;
					uvel_astro.x = 0f;
					uvel_astro.y = 0f;
					uvel_astro.z = 0f;
				}
			}
			else
			{
				if (astroId <= 100 || astroId > 204899 || astroId % 100 <= 0)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					uvel_astro.x = 0f;
					uvel_astro.y = 0f;
					uvel_astro.z = 0f;
					uvel_obj.x = 0f;
					uvel_obj.y = 0f;
					uvel_obj.z = 0f;
					return false;
				}
				ref EnemyData ptr2 = ref this.astroFactories[astroId].enemyPool[obj.id];
				if (ptr2.id == 0)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					uvel_astro.x = 0f;
					uvel_astro.y = 0f;
					uvel_astro.z = 0f;
					uvel_obj.x = 0f;
					uvel_obj.y = 0f;
					uvel_obj.z = 0f;
					return false;
				}
				AstroData[] galaxyAstros2 = this.sector.galaxyAstros;
				int num3 = astroId;
				galaxyAstros2[num3].PositionU(ref ptr2.pos, out upos);
				galaxyAstros2[num3].VelocityL2U(ref ptr2.pos, ref ptr2.vel, out uvel_astro, out uvel_obj);
			}
		}
		else if (obj.type == ETargetType.Craft)
		{
			int astroId2 = obj.astroId;
			if (astroId2 > 100 && astroId2 <= 204899 && astroId2 % 100 > 0)
			{
				ref CraftData ptr3 = ref this.astroFactories[astroId2].craftPool[obj.id];
				if (ptr3.id == 0)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					uvel_astro.x = 0f;
					uvel_astro.y = 0f;
					uvel_astro.z = 0f;
					uvel_obj.x = 0f;
					uvel_obj.y = 0f;
					uvel_obj.z = 0f;
					return false;
				}
				AstroData[] galaxyAstros3 = this.sector.galaxyAstros;
				int num4 = astroId2;
				galaxyAstros3[num4].PositionU(ref ptr3.pos, out upos);
				galaxyAstros3[num4].VelocityL2U(ref ptr3.pos, ref ptr3.vel, out uvel_astro, out uvel_obj);
			}
			else
			{
				if (astroId2 % 100 != 0 && astroId2 <= 1000000)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					uvel_astro.x = 0f;
					uvel_astro.y = 0f;
					uvel_astro.z = 0f;
					uvel_obj.x = 0f;
					uvel_obj.y = 0f;
					uvel_obj.z = 0f;
					return false;
				}
				ref CraftData ptr4 = ref this.sector.craftPool[obj.id];
				if (ptr4.id == 0)
				{
					upos.x = 0.0;
					upos.y = 0.0;
					upos.z = 0.0;
					uvel_astro.x = 0f;
					uvel_astro.y = 0f;
					uvel_astro.z = 0f;
					uvel_obj.x = 0f;
					uvel_obj.y = 0f;
					uvel_obj.z = 0f;
					return false;
				}
				obj.astroId = ptr4.astroId;
				astroId2 = ptr4.astroId;
				if (astroId2 > 1000000)
				{
					AstroData[] astros2 = this.sector.astros;
					int num5 = astroId2 - 1000000;
					astros2[num5].PositionU(ref ptr4.pos, out upos);
					astros2[num5].VelocityL2U(ref ptr4.pos, ref ptr4.vel, out uvel_astro, out uvel_obj);
				}
				else if (astroId2 >= 100 && astroId2 <= 204899)
				{
					AstroData[] galaxyAstros4 = this.sector.galaxyAstros;
					int num6 = astroId2;
					galaxyAstros4[num6].PositionU(ref ptr4.pos, out upos);
					galaxyAstros4[num6].VelocityL2U(ref ptr4.pos, ref ptr4.vel, out uvel_astro, out uvel_obj);
				}
				else
				{
					upos = ptr4.pos;
					uvel_obj = ptr4.vel;
					uvel_astro.x = 0f;
					uvel_astro.y = 0f;
					uvel_astro.z = 0f;
				}
			}
		}
		else if (obj.type == ETargetType.None)
		{
			int astroId3 = obj.astroId;
			if (astroId3 <= 100 || astroId3 > 204899 || astroId3 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				uvel_astro.x = 0f;
				uvel_astro.y = 0f;
				uvel_astro.z = 0f;
				uvel_obj.x = 0f;
				uvel_obj.y = 0f;
				uvel_obj.z = 0f;
				return false;
			}
			ref EntityData ptr5 = ref this.astroFactories[astroId3].entityPool[obj.id];
			AstroData[] galaxyAstros5 = this.sector.galaxyAstros;
			int num7 = astroId3;
			galaxyAstros5[num7].PositionU(ref ptr5.pos, out upos);
			galaxyAstros5[num7].VelocityU(ref ptr5.pos, out uvel_astro);
			uvel_obj.x = 0f;
			uvel_obj.y = 0f;
			uvel_obj.z = 0f;
		}
		else if (obj.type == ETargetType.Vegetable)
		{
			int astroId4 = obj.astroId;
			if (astroId4 <= 100 || astroId4 > 204899 || astroId4 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				uvel_astro.x = 0f;
				uvel_astro.y = 0f;
				uvel_astro.z = 0f;
				uvel_obj.x = 0f;
				uvel_obj.y = 0f;
				uvel_obj.z = 0f;
				return false;
			}
			ref VegeData ptr6 = ref this.astroFactories[astroId4].vegePool[obj.id];
			AstroData[] galaxyAstros6 = this.sector.galaxyAstros;
			int num8 = astroId4;
			galaxyAstros6[num8].PositionU(ref ptr6.pos, out upos);
			galaxyAstros6[num8].VelocityU(ref ptr6.pos, out uvel_astro);
			uvel_obj.x = 0f;
			uvel_obj.y = 0f;
			uvel_obj.z = 0f;
		}
		else if (obj.type == ETargetType.Vein)
		{
			int astroId5 = obj.astroId;
			if (astroId5 <= 100 || astroId5 > 204899 || astroId5 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				uvel_astro.x = 0f;
				uvel_astro.y = 0f;
				uvel_astro.z = 0f;
				uvel_obj.x = 0f;
				uvel_obj.y = 0f;
				uvel_obj.z = 0f;
				return false;
			}
			ref VeinData ptr7 = ref this.astroFactories[astroId5].veinPool[obj.id];
			AstroData[] galaxyAstros7 = this.sector.galaxyAstros;
			int num9 = astroId5;
			galaxyAstros7[num9].PositionU(ref ptr7.pos, out upos);
			galaxyAstros7[num9].VelocityU(ref ptr7.pos, out uvel_astro);
			uvel_obj.x = 0f;
			uvel_obj.y = 0f;
			uvel_obj.z = 0f;
		}
		else if (obj.type == ETargetType.Prebuild)
		{
			int astroId6 = obj.astroId;
			if (astroId6 <= 100 || astroId6 > 204899 || astroId6 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				uvel_astro.x = 0f;
				uvel_astro.y = 0f;
				uvel_astro.z = 0f;
				uvel_obj.x = 0f;
				uvel_obj.y = 0f;
				uvel_obj.z = 0f;
				return false;
			}
			ref PrebuildData ptr8 = ref this.astroFactories[astroId6].prebuildPool[obj.id];
			AstroData[] galaxyAstros8 = this.sector.galaxyAstros;
			int num10 = astroId6;
			galaxyAstros8[num10].PositionU(ref ptr8.pos, out upos);
			galaxyAstros8[num10].VelocityU(ref ptr8.pos, out uvel_astro);
			uvel_obj.x = 0f;
			uvel_obj.y = 0f;
			uvel_obj.z = 0f;
		}
		else if (obj.type == ETargetType.Ruin)
		{
			int astroId7 = obj.astroId;
			if (astroId7 <= 100 || astroId7 > 204899 || astroId7 % 100 <= 0)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				uvel_astro.x = 0f;
				uvel_astro.y = 0f;
				uvel_astro.z = 0f;
				uvel_obj.x = 0f;
				uvel_obj.y = 0f;
				uvel_obj.z = 0f;
				return false;
			}
			ref RuinData ptr9 = ref this.astroFactories[astroId7].ruinPool[obj.id];
			AstroData[] galaxyAstros9 = this.sector.galaxyAstros;
			int num11 = astroId7;
			galaxyAstros9[num11].PositionU(ref ptr9.pos, out upos);
			galaxyAstros9[num11].VelocityU(ref ptr9.pos, out uvel_astro);
			uvel_obj.x = 0f;
			uvel_obj.y = 0f;
			uvel_obj.z = 0f;
		}
		else
		{
			if (obj.type != ETargetType.Player)
			{
				upos.x = 0.0;
				upos.y = 0.0;
				upos.z = 0.0;
				uvel_astro.x = 0f;
				uvel_astro.y = 0f;
				uvel_astro.z = 0f;
				uvel_obj.x = 0f;
				uvel_obj.y = 0f;
				uvel_obj.z = 0f;
				return false;
			}
			upos = this.playerSkillTargetU;
			uvel_astro.x = 0f;
			uvel_astro.y = 0f;
			uvel_astro.z = 0f;
			uvel_obj = this.playerVelocityU;
		}
		return true;
	}

	// Token: 0x06000E8C RID: 3724 RVA: 0x000E0570 File Offset: 0x000DE770
	public void GetMissileObjVel(int astroId, ref VectorLF3 upos, ref Vector3 uvel, out Vector3 uvel_obj)
	{
		if (astroId >= 100 && astroId <= 204899 && astroId % 100 > 0)
		{
			ref AstroData ptr = ref this.sector.galaxyAstros[astroId];
			Quaternion uRot = ptr.uRot;
			VectorLF3 vectorLF;
			vectorLF.x = upos.x - ptr.uPos.x;
			vectorLF.y = upos.y - ptr.uPos.y;
			vectorLF.z = upos.z - ptr.uPos.z;
			Maths.QInvRotateLF_ref(ref uRot, ref vectorLF, ref vectorLF);
			Vector3 vector;
			ptr.VelocityU(ref vectorLF, out vector);
			uvel_obj.x = uvel.x - vector.x;
			uvel_obj.y = uvel.y - vector.y;
			uvel_obj.z = uvel.z - vector.z;
			return;
		}
		uvel_obj.x = uvel.x;
		uvel_obj.y = uvel.y;
		uvel_obj.z = uvel.z;
	}

	// Token: 0x06000E8D RID: 3725 RVA: 0x000E0678 File Offset: 0x000DE878
	public ref CombatStat DamageObject(int damage, int slice, ref SkillTarget target, ref SkillTarget caster)
	{
		ref CombatStat ptr = ref CombatStat.temp;
		ptr.Reset();
		int astroId = target.astroId;
		if (target.type == ETargetType.Player)
		{
			if (target.id == 1)
			{
				this.mecha.TakeDamage(damage);
				this.AddMechaHatred(caster.astroId, caster.id, damage);
			}
		}
		else if (astroId > 1000000)
		{
			if (target.type == ETargetType.Enemy)
			{
				EnemyDFHiveSystem enemyDFHiveSystem = this.sector.dfHivesByAstro[astroId - 1000000];
				int num = 0;
				if (enemyDFHiveSystem != null)
				{
					num = enemyDFHiveSystem.evolve.level;
					int num2 = 100 / slice;
					int num3 = num * num2 / 2;
					damage -= num3;
					if (damage < num2)
					{
						damage = num2;
					}
				}
				ref EnemyData ptr2 = ref this.sector.enemyPool[target.id];
				if (ptr2.id == 0 || ptr2.isInvincible)
				{
					return ref CombatStat.temp;
				}
				if (ptr2.combatStatId > 0)
				{
					CombatStat[] buffer = this.combatStats.buffer;
					int combatStatId = ptr2.combatStatId;
					buffer[combatStatId].hp = buffer[combatStatId].hp - damage;
					buffer[combatStatId].lastCaster.type = caster.type;
					buffer[combatStatId].lastCaster.id = caster.id;
					buffer[combatStatId].lastCaster.astroId = caster.astroId;
					ptr = ref buffer[combatStatId];
				}
				else
				{
					ref CombatStat ptr3 = ref this.combatStats.Add();
					ptr3.hpMax = SkillSystem.HpMaxByModelIndex[(int)ptr2.modelIndex] + num * SkillSystem.HpUpgradeByModelIndex[(int)ptr2.modelIndex];
					ptr3.hp = ptr3.hpMax - damage;
					ptr3.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr2.modelIndex];
					ptr3.astroId = (ptr3.originAstroId = astroId);
					ptr3.objectType = (int)target.type;
					ptr3.objectId = target.id;
					ptr3.dynamic = (ptr2.dynamic ? 1 : 0);
					ptr3.localPos = (ptr2.dynamic ? ptr2.pos : (ptr2.pos + ptr2.pos.normalized * (double)SkillSystem.BarHeightByModelIndex[(int)ptr2.modelIndex]));
					ptr3.size = SkillSystem.BarWidthByModelIndex[(int)ptr2.modelIndex];
					ptr3.lastCaster.type = caster.type;
					ptr3.lastCaster.id = caster.id;
					ptr3.lastCaster.astroId = caster.astroId;
					ptr2.combatStatId = ptr3.id;
					ptr = ref ptr3;
				}
				PlanetFactory planetFactory = (caster.id > 0) ? this.astroFactories[caster.astroId] : null;
				if (planetFactory != null && caster.type == ETargetType.None)
				{
					ref EntityData ptr4 = ref planetFactory.entityPool[caster.id];
					if (ptr4.id == caster.id)
					{
						int turretId = ptr4.turretId;
						if (turretId > 0)
						{
							ref TurretComponent ptr5 = ref planetFactory.defenseSystem.turrets.buffer[turretId];
							if (ptr5.id == turretId)
							{
								ptr5.totalDamage += (long)damage;
							}
						}
					}
				}
				if (enemyDFHiveSystem != null)
				{
					this.AddSpaceEnemyExp(enemyDFHiveSystem, (float)damage, slice);
				}
				if (caster.id > 0 && this.isEnemyHostileTmp)
				{
					if (planetFactory != null && caster.type == ETargetType.None && planetFactory.entityPool[caster.id].id == caster.id)
					{
						this.AddSpaceEnemyHatred(enemyDFHiveSystem, ref ptr2, caster.type, caster.astroId, caster.id, damage);
					}
					else if (planetFactory == null && caster.type == ETargetType.Craft && this.sector.craftPool[caster.id].id == caster.id)
					{
						this.AddSpaceEnemyHatred(enemyDFHiveSystem, ref ptr2, caster.type, caster.astroId, caster.id, damage);
					}
					else if (caster.type == ETargetType.Player && this.playerAlive)
					{
						this.AddSpaceEnemyHatred(enemyDFHiveSystem, ref ptr2, ETargetType.Player, 0, caster.id, damage);
					}
				}
			}
			else if (target.type == ETargetType.Craft)
			{
				ref CraftData ptr6 = ref this.sector.craftPool[target.id];
				if (ptr6.id == 0 || ptr6.isInvincible)
				{
					return ref CombatStat.temp;
				}
				if (ptr6.combatStatId > 0)
				{
					CombatStat[] buffer2 = this.combatStats.buffer;
					int combatStatId2 = ptr6.combatStatId;
					buffer2[combatStatId2].hp = buffer2[combatStatId2].hp - damage;
					buffer2[combatStatId2].lastCaster.type = caster.type;
					buffer2[combatStatId2].lastCaster.id = caster.id;
					buffer2[combatStatId2].lastCaster.astroId = caster.astroId;
					ptr = ref buffer2[combatStatId2];
				}
				else
				{
					ref CombatStat ptr7 = ref this.combatStats.Add();
					ptr7.hpMax = (int)((float)SkillSystem.HpMaxByModelIndex[(int)ptr6.modelIndex] * this.history.combatDroneDurabilityRatio * (1f + this.history.globalHpEnhancement) + 0.5f);
					ptr7.hp = ptr7.hpMax - damage;
					ptr7.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr6.modelIndex];
					ptr7.astroId = (ptr7.originAstroId = astroId);
					ptr7.objectType = (int)target.type;
					ptr7.objectId = target.id;
					ptr7.dynamic = (ptr6.dynamic ? 1 : 0);
					ptr7.localPos = (ptr6.dynamic ? ptr6.pos : (ptr6.pos + ptr6.pos.normalized * (double)SkillSystem.BarHeightByModelIndex[(int)ptr6.modelIndex]));
					ptr7.size = SkillSystem.BarWidthByModelIndex[(int)ptr6.modelIndex];
					ptr7.lastCaster.type = caster.type;
					ptr7.lastCaster.id = caster.id;
					ptr7.lastCaster.astroId = caster.astroId;
					ptr6.combatStatId = ptr7.id;
					ptr = ref ptr7;
				}
				int unitId = ptr6.unitId;
				ref UnitComponent ptr8 = ref this.sector.combatSpaceSystem.units.buffer[unitId];
				if (unitId > 0 && ptr8.id == unitId && caster.id > 0 && caster.type == ETargetType.Enemy && this.sector.enemyPool[caster.id].id == caster.id && (this.sector.enemyPool[caster.id].builderId <= 0 || this.mecha.spaceCombatModule.attackBuilding))
				{
					this.AddSpaceCraftHatred(ref ptr6, ref ptr8, ref this.sector.enemyPool[caster.id], damage);
				}
			}
		}
		else if (astroId > 100 && astroId <= 204899 && astroId % 100 > 0)
		{
			PlanetFactory factory = this.astroFactories[astroId];
			if (caster.astroId == astroId)
			{
				SkillTargetLocal skillTargetLocal = default(SkillTargetLocal);
				SkillTargetLocal skillTargetLocal2 = default(SkillTargetLocal);
				skillTargetLocal.id = target.id;
				skillTargetLocal.type = target.type;
				skillTargetLocal2.id = caster.id;
				skillTargetLocal2.type = caster.type;
				ref CombatStat ptr9 = ref this.DamageGroundObjectByLocalCaster(factory, damage, slice, ref skillTargetLocal, ref skillTargetLocal2);
				if (ptr9.id > 0)
				{
					ptr = ref this.combatStats.buffer[ptr9.id];
				}
			}
			else
			{
				SkillTargetLocal skillTargetLocal3 = default(SkillTargetLocal);
				SkillTarget skillTarget = default(SkillTarget);
				skillTargetLocal3.id = target.id;
				skillTargetLocal3.type = target.type;
				skillTarget.id = caster.id;
				skillTarget.type = caster.type;
				skillTarget.astroId = caster.astroId;
				ref CombatStat ptr10 = ref this.DamageGroundObjectByRemoteCaster(factory, damage, slice, ref skillTargetLocal3, ref skillTarget);
				if (ptr10.id > 0)
				{
					ptr = ref this.combatStats.buffer[ptr10.id];
				}
			}
		}
		else if (astroId % 100 == 0 && target.type == ETargetType.Craft)
		{
			ref CraftData ptr11 = ref this.sector.craftPool[target.id];
			if (ptr11.id == 0 || ptr11.isInvincible)
			{
				return ref CombatStat.temp;
			}
			if (ptr11.combatStatId > 0)
			{
				CombatStat[] buffer3 = this.combatStats.buffer;
				int combatStatId3 = ptr11.combatStatId;
				buffer3[combatStatId3].hp = buffer3[combatStatId3].hp - damage;
				buffer3[combatStatId3].lastCaster.type = caster.type;
				buffer3[combatStatId3].lastCaster.id = caster.id;
				buffer3[combatStatId3].lastCaster.astroId = caster.astroId;
				ptr = ref buffer3[combatStatId3];
			}
			else
			{
				ref CombatStat ptr12 = ref this.combatStats.Add();
				ptr12.hpMax = (int)((float)SkillSystem.HpMaxByModelIndex[(int)ptr11.modelIndex] * this.history.combatDroneDurabilityRatio * (1f + this.history.globalHpEnhancement) + 0.5f);
				ptr12.hp = ptr12.hpMax - damage;
				ptr12.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr11.modelIndex];
				ptr12.astroId = (ptr12.originAstroId = astroId);
				ptr12.objectType = (int)target.type;
				ptr12.objectId = target.id;
				ptr12.dynamic = (ptr11.dynamic ? 1 : 0);
				ptr12.localPos = (ptr11.dynamic ? ptr11.pos : (ptr11.pos + ptr11.pos.normalized * (double)SkillSystem.BarHeightByModelIndex[(int)ptr11.modelIndex]));
				ptr12.size = SkillSystem.BarWidthByModelIndex[(int)ptr11.modelIndex];
				ptr12.lastCaster.type = caster.type;
				ptr12.lastCaster.id = caster.id;
				ptr12.lastCaster.astroId = caster.astroId;
				ptr11.combatStatId = ptr12.id;
				ptr = ref ptr12;
			}
			int unitId2 = ptr11.unitId;
			ref UnitComponent ptr13 = ref this.sector.combatSpaceSystem.units.buffer[unitId2];
			if (unitId2 > 0 && ptr13.id == unitId2 && caster.id > 0 && caster.type == ETargetType.Enemy && this.sector.enemyPool[caster.id].id == caster.id && (this.sector.enemyPool[caster.id].builderId <= 0 || this.mecha.spaceCombatModule.attackBuilding))
			{
				this.AddSpaceCraftHatred(ref ptr11, ref ptr13, ref this.sector.enemyPool[caster.id], damage);
			}
		}
		return ref ptr;
	}

	// Token: 0x06000E8E RID: 3726 RVA: 0x000E1174 File Offset: 0x000DF374
	public ref CombatStat DamageGroundObjectByLocalCaster(PlanetFactory factory, int damage, int slice, ref SkillTargetLocal target, ref SkillTargetLocal caster)
	{
		ref CombatStat ptr = ref CombatStat.temp;
		ptr.Reset();
		if (target.id <= 0)
		{
			return ref CombatStat.temp;
		}
		if (target.type == ETargetType.Enemy)
		{
			ref EnemyData ptr2 = ref factory.enemyPool[target.id];
			if (ptr2.id != target.id || ptr2.isInvincible)
			{
				return ref CombatStat.temp;
			}
			DFGBaseComponent dfgbaseComponent = null;
			if (ptr2.owner > 0)
			{
				dfgbaseComponent = factory.enemySystem.bases[(int)ptr2.owner];
				if (dfgbaseComponent.id != (int)ptr2.owner)
				{
					dfgbaseComponent = null;
				}
			}
			int num = 0;
			if (dfgbaseComponent != null)
			{
				num = dfgbaseComponent.evolve.level;
				int num2 = 100 / slice;
				int num3 = num * num2 / 5;
				damage -= num3;
				if (damage < num2)
				{
					damage = num2;
				}
			}
			if (ptr2.combatStatId > 0)
			{
				CombatStat[] buffer = this.combatStats.buffer;
				int combatStatId = ptr2.combatStatId;
				buffer[combatStatId].hp = buffer[combatStatId].hp - damage;
				buffer[combatStatId].lastCaster.type = caster.type;
				buffer[combatStatId].lastCaster.id = caster.id;
				buffer[combatStatId].lastCaster.astroId = factory.planetId;
				ptr = ref buffer[combatStatId];
			}
			else
			{
				ref CombatStat ptr3 = ref this.combatStats.Add();
				ptr3.hpMax = SkillSystem.HpMaxByModelIndex[(int)ptr2.modelIndex] + num * SkillSystem.HpUpgradeByModelIndex[(int)ptr2.modelIndex];
				ptr3.hp = ptr3.hpMax - damage;
				ptr3.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr2.modelIndex];
				ptr3.astroId = (ptr3.originAstroId = factory.planetId);
				ptr3.objectType = (int)target.type;
				ptr3.objectId = target.id;
				ptr3.dynamic = (ptr2.dynamic ? 1 : 0);
				ptr3.localPos = (ptr2.dynamic ? ptr2.pos : (ptr2.pos + ptr2.pos.normalized * (double)SkillSystem.BarHeightByModelIndex[(int)ptr2.modelIndex]));
				ptr3.size = SkillSystem.BarWidthByModelIndex[(int)ptr2.modelIndex];
				ptr3.lastCaster.type = caster.type;
				ptr3.lastCaster.id = caster.id;
				ptr3.lastCaster.astroId = factory.planetId;
				ptr2.combatStatId = ptr3.id;
				ptr = ref ptr3;
			}
			EntityData[] entityPool = factory.entityPool;
			CraftData[] craftPool = factory.craftPool;
			if (caster.id > 0 && caster.type == ETargetType.None)
			{
				ref EntityData ptr4 = ref entityPool[caster.id];
				if (ptr4.id == caster.id)
				{
					int turretId = ptr4.turretId;
					if (turretId > 0)
					{
						ref TurretComponent ptr5 = ref factory.defenseSystem.turrets.buffer[turretId];
						if (ptr5.id == turretId)
						{
							ptr5.totalDamage += (long)damage;
						}
					}
				}
			}
			if (dfgbaseComponent != null)
			{
				this.AddGroundEnemyExp(dfgbaseComponent, (float)damage, slice);
			}
			if (caster.id > 0 && this.isEnemyHostileTmp)
			{
				if (caster.type == ETargetType.None && entityPool[caster.id].id == caster.id)
				{
					this.AddGroundEnemyHatred(dfgbaseComponent, ref ptr2, caster.type, caster.id, damage);
				}
				else if (caster.type == ETargetType.Craft && craftPool[caster.id].id == caster.id)
				{
					this.AddGroundEnemyHatred(dfgbaseComponent, ref ptr2, caster.type, caster.id, damage);
				}
				else if (caster.type == ETargetType.Player && this.playerAlive)
				{
					this.AddGroundEnemyHatred(dfgbaseComponent, ref ptr2, caster.type, caster.id, damage);
				}
			}
		}
		else if (target.type == ETargetType.Craft)
		{
			ref CraftData ptr6 = ref factory.craftPool[target.id];
			if (ptr6.id != target.id || ptr6.isInvincible)
			{
				return ref CombatStat.temp;
			}
			if (ptr6.combatStatId > 0)
			{
				CombatStat[] buffer2 = this.combatStats.buffer;
				int combatStatId2 = ptr6.combatStatId;
				buffer2[combatStatId2].hp = buffer2[combatStatId2].hp - damage;
				buffer2[combatStatId2].lastCaster.type = caster.type;
				buffer2[combatStatId2].lastCaster.id = caster.id;
				buffer2[combatStatId2].lastCaster.astroId = factory.planetId;
				ptr = ref buffer2[combatStatId2];
			}
			else
			{
				ref CombatStat ptr7 = ref this.combatStats.Add();
				ptr7.hpMax = (int)((float)SkillSystem.HpMaxByModelIndex[(int)ptr6.modelIndex] * this.history.combatDroneDurabilityRatio * (1f + this.history.globalHpEnhancement) + 0.5f);
				ptr7.hp = ptr7.hpMax - damage;
				ptr7.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr6.modelIndex];
				ptr7.astroId = (ptr7.originAstroId = factory.planetId);
				ptr7.objectType = (int)target.type;
				ptr7.objectId = target.id;
				ptr7.dynamic = (ptr6.dynamic ? 1 : 0);
				ptr7.localPos = (ptr6.dynamic ? ptr6.pos : (ptr6.pos + ptr6.pos.normalized * (double)SkillSystem.BarHeightByModelIndex[(int)ptr6.modelIndex]));
				ptr7.size = SkillSystem.BarWidthByModelIndex[(int)ptr6.modelIndex];
				ptr7.lastCaster.type = caster.type;
				ptr7.lastCaster.id = caster.id;
				ptr7.lastCaster.astroId = factory.planetId;
				ptr6.combatStatId = ptr7.id;
				ptr = ref ptr7;
			}
			int unitId = ptr6.unitId;
			ref UnitComponent ptr8 = ref factory.combatGroundSystem.units.buffer[unitId];
			if (unitId > 0 && ptr8.id == unitId && caster.id > 0 && caster.type == ETargetType.Enemy && factory.enemyPool[caster.id].id == caster.id && (factory.enemyPool[caster.id].builderId <= 0 || this.mecha.groundCombatModule.attackBuilding))
			{
				this.AddGroundCraftHatred(ref ptr6, ref ptr8, ETargetType.Enemy, caster.id, damage);
			}
		}
		else if (target.type == ETargetType.None)
		{
			ref EntityData ptr9 = ref factory.entityPool[target.id];
			if (ptr9.id != target.id)
			{
				return ref CombatStat.temp;
			}
			if (ptr9.combatStatId > 0)
			{
				CombatStat[] buffer3 = this.combatStats.buffer;
				int combatStatId3 = ptr9.combatStatId;
				buffer3[combatStatId3].hp = buffer3[combatStatId3].hp - damage;
				buffer3[combatStatId3].lastCaster.type = caster.type;
				buffer3[combatStatId3].lastCaster.id = caster.id;
				buffer3[combatStatId3].lastCaster.astroId = factory.planetId;
				ptr = ref buffer3[combatStatId3];
			}
			else
			{
				ref CombatStat ptr10 = ref this.combatStats.Add();
				ptr10.hpMax = (int)((float)SkillSystem.HpMaxByModelIndex[(int)ptr9.modelIndex] * (1f + this.history.globalHpEnhancement) + 0.5f);
				ptr10.hp = ptr10.hpMax - damage;
				ptr10.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr9.modelIndex];
				ptr10.astroId = (ptr10.originAstroId = factory.planetId);
				ptr10.objectType = (int)target.type;
				ptr10.objectId = target.id;
				ptr10.dynamic = 0;
				ptr10.localPos = ptr9.pos + ptr9.pos.normalized * SkillSystem.BarHeightByModelIndex[(int)ptr9.modelIndex];
				ptr10.size = SkillSystem.BarWidthByModelIndex[(int)ptr9.modelIndex];
				ptr10.lastCaster.type = caster.type;
				ptr10.lastCaster.id = caster.id;
				ptr10.lastCaster.astroId = factory.planetId;
				ptr9.combatStatId = ptr10.id;
				ptr = ref ptr10;
			}
			if (ptr9.constructStatId > 0)
			{
				ConstructStat[] buffer4 = factory.constructionSystem.constructStats.buffer;
				int constructStatId = ptr9.constructStatId;
				buffer4[constructStatId].damageRegister = buffer4[constructStatId].damageRegister + damage;
			}
			else
			{
				int constructStatId2 = factory.constructionSystem.AddConstructStat(target.id, damage);
				ptr9.constructStatId = constructStatId2;
			}
			if (ptr.warningId == 0)
			{
				ref WarningData ptr11 = ref this.gameData.warningSystem.NewWarningData(-4, ptr.ID, 512);
				ptr11.astroId = ptr.originAstroId;
				ptr11.localPos = ptr9.pos;
				ptr11.localPos += ptr9.pos.normalized * SpaceSector.PrefabDescByModelIndex[(int)ptr9.modelIndex].signHeight;
				ptr11.state = ((ptr.hp > 0 && (EntitySignRenderer.buildingWarningMask & 2048) > 0) ? 1 : 0);
				ptr11.detailId1 = (int)ptr9.protoId;
				ptr.warningId = ptr11.id;
				if (ptr9.ignoreRepairWarning)
				{
					ptr11.state = 0;
				}
			}
			if (ptr9.turretId > 0)
			{
				ref EnemyData ptr12 = ref factory.enemyPool[caster.id];
				if (caster.type == ETargetType.Enemy && ptr12.id > 0 && ptr12.dfGTurretId > 0)
				{
					ref HatredTarget ptr13 = ref factory.defenseSystem.turrets.buffer[ptr9.turretId].hatred0;
					if (damage > ptr13.value)
					{
						ptr13.target = (ptr12.id | 268435456);
						ptr13.value = damage;
					}
				}
			}
		}
		else if (target.type == ETargetType.Vegetable)
		{
			ref VegeData ptr14 = ref factory.vegePool[target.id];
			if (ptr14.id != target.id)
			{
				return ref CombatStat.temp;
			}
			if (ptr14.combatStatId > 0)
			{
				CombatStat[] buffer5 = this.combatStats.buffer;
				int combatStatId4 = ptr14.combatStatId;
				buffer5[combatStatId4].hp = buffer5[combatStatId4].hp - damage;
				buffer5[combatStatId4].lastCaster.type = caster.type;
				buffer5[combatStatId4].lastCaster.id = caster.id;
				buffer5[combatStatId4].lastCaster.astroId = factory.planetId;
				ptr = ref buffer5[combatStatId4];
			}
			else
			{
				ref CombatStat ptr15 = ref this.combatStats.Add();
				ptr15.hpMax = SkillSystem.HpMaxByModelIndex[(int)ptr14.modelIndex];
				ptr15.hp = ptr15.hpMax - damage;
				ptr15.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr14.modelIndex];
				ptr15.astroId = (ptr15.originAstroId = factory.planetId);
				ptr15.objectType = (int)target.type;
				ptr15.objectId = target.id;
				ptr15.dynamic = 0;
				ptr15.localPos = ptr14.pos + ptr14.pos.normalized * SkillSystem.BarHeightByModelIndex[(int)ptr14.modelIndex];
				ptr15.size = SkillSystem.BarWidthByModelIndex[(int)ptr14.modelIndex];
				ptr15.lastCaster.type = caster.type;
				ptr15.lastCaster.id = caster.id;
				ptr15.lastCaster.astroId = factory.planetId;
				ptr14.combatStatId = ptr15.id;
				ptr = ref ptr15;
			}
		}
		else if (target.type == ETargetType.Vein)
		{
			ref VeinData ptr16 = ref factory.veinPool[target.id];
			if (ptr16.id != target.id)
			{
				return ref CombatStat.temp;
			}
			if (ptr16.combatStatId > 0)
			{
				CombatStat[] buffer6 = this.combatStats.buffer;
				int combatStatId5 = ptr16.combatStatId;
				buffer6[combatStatId5].hp = buffer6[combatStatId5].hp - damage;
				buffer6[combatStatId5].lastCaster.type = caster.type;
				buffer6[combatStatId5].lastCaster.id = caster.id;
				buffer6[combatStatId5].lastCaster.astroId = factory.planetId;
				ptr = ref buffer6[combatStatId5];
			}
			else
			{
				ref CombatStat ptr17 = ref this.combatStats.Add();
				ptr17.hpMax = SkillSystem.HpMaxByModelIndex[(int)ptr16.modelIndex];
				ptr17.hp = ptr17.hpMax - damage;
				ptr17.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr16.modelIndex];
				ptr17.astroId = (ptr17.originAstroId = factory.planetId);
				ptr17.objectType = (int)target.type;
				ptr17.objectId = target.id;
				ptr17.dynamic = 0;
				ptr17.localPos = ptr16.pos + ptr16.pos.normalized * SkillSystem.BarHeightByModelIndex[(int)ptr16.modelIndex];
				ptr17.size = SkillSystem.BarWidthByModelIndex[(int)ptr16.modelIndex];
				ptr17.lastCaster.type = caster.type;
				ptr17.lastCaster.id = caster.id;
				ptr17.lastCaster.astroId = factory.planetId;
				ptr16.combatStatId = ptr17.id;
				ptr = ref ptr17;
			}
		}
		else if (target.type == ETargetType.Player && target.id == 1)
		{
			this.mecha.TakeDamage(damage);
			this.AddMechaHatred(factory.planetId, caster.id, damage);
		}
		return ref ptr;
	}

	// Token: 0x06000E8F RID: 3727 RVA: 0x000E1F0C File Offset: 0x000E010C
	public ref CombatStat DamageGroundObjectByRemoteCaster(PlanetFactory factory, int damage, int slice, ref SkillTargetLocal target, ref SkillTarget caster)
	{
		ref CombatStat ptr = ref CombatStat.temp;
		ptr.Reset();
		if (target.id <= 0)
		{
			return ref CombatStat.temp;
		}
		if (target.type == ETargetType.Enemy)
		{
			ref EnemyData ptr2 = ref factory.enemyPool[target.id];
			if (ptr2.id != target.id || ptr2.isInvincible)
			{
				return ref CombatStat.temp;
			}
			DFGBaseComponent dfgbaseComponent = null;
			if (ptr2.owner > 0)
			{
				dfgbaseComponent = factory.enemySystem.bases[(int)ptr2.owner];
				if (dfgbaseComponent.id != (int)ptr2.owner)
				{
					dfgbaseComponent = null;
				}
			}
			int num = 0;
			if (dfgbaseComponent != null)
			{
				num = dfgbaseComponent.evolve.level;
				int num2 = 100 / slice;
				int num3 = num * num2 / 5;
				damage -= num3;
				if (damage < num2)
				{
					damage = num2;
				}
			}
			if (ptr2.combatStatId > 0)
			{
				CombatStat[] buffer = this.combatStats.buffer;
				int combatStatId = ptr2.combatStatId;
				buffer[combatStatId].hp = buffer[combatStatId].hp - damage;
				buffer[combatStatId].lastCaster.type = caster.type;
				buffer[combatStatId].lastCaster.id = caster.id;
				buffer[combatStatId].lastCaster.astroId = caster.astroId;
				ptr = ref buffer[combatStatId];
			}
			else
			{
				ref CombatStat ptr3 = ref this.combatStats.Add();
				ptr3.hpMax = SkillSystem.HpMaxByModelIndex[(int)ptr2.modelIndex] + num * SkillSystem.HpUpgradeByModelIndex[(int)ptr2.modelIndex];
				ptr3.hp = ptr3.hpMax - damage;
				ptr3.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr2.modelIndex];
				ptr3.astroId = (ptr3.originAstroId = factory.planetId);
				ptr3.objectType = (int)target.type;
				ptr3.objectId = target.id;
				ptr3.dynamic = (ptr2.dynamic ? 1 : 0);
				ptr3.localPos = (ptr2.dynamic ? ptr2.pos : (ptr2.pos + ptr2.pos.normalized * (double)SkillSystem.BarHeightByModelIndex[(int)ptr2.modelIndex]));
				ptr3.size = SkillSystem.BarWidthByModelIndex[(int)ptr2.modelIndex];
				ptr3.lastCaster.type = caster.type;
				ptr3.lastCaster.id = caster.id;
				ptr3.lastCaster.astroId = caster.astroId;
				ptr2.combatStatId = ptr3.id;
				ptr = ref ptr3;
			}
			if (caster.type == ETargetType.None)
			{
				PlanetFactory planetFactory = (caster.id > 0 && caster.astroId < this.astroFactories.Length) ? this.astroFactories[caster.astroId] : null;
				if (planetFactory != null)
				{
					ref EntityData ptr4 = ref planetFactory.entityPool[caster.id];
					if (ptr4.id == caster.id)
					{
						int turretId = ptr4.turretId;
						if (turretId > 0)
						{
							ref TurretComponent ptr5 = ref planetFactory.defenseSystem.turrets.buffer[turretId];
							if (ptr5.id == turretId)
							{
								ptr5.totalDamage += (long)damage;
							}
						}
					}
				}
			}
			if (dfgbaseComponent != null)
			{
				this.AddGroundEnemyExp(dfgbaseComponent, (float)damage, slice);
			}
			if (caster.id > 0 && this.isEnemyHostileTmp)
			{
				if (caster.type == ETargetType.Player && this.playerAlive)
				{
					this.AddGroundEnemyHatred(dfgbaseComponent, ref ptr2, caster.type, caster.id, damage);
				}
				else
				{
					this.AddGroundEnemyThreat(dfgbaseComponent, ref ptr2, damage);
				}
			}
		}
		else if (target.type == ETargetType.Craft)
		{
			ref CraftData ptr6 = ref factory.craftPool[target.id];
			if (ptr6.id != target.id || ptr6.isInvincible)
			{
				return ref CombatStat.temp;
			}
			if (ptr6.combatStatId > 0)
			{
				CombatStat[] buffer2 = this.combatStats.buffer;
				int combatStatId2 = ptr6.combatStatId;
				buffer2[combatStatId2].hp = buffer2[combatStatId2].hp - damage;
				buffer2[combatStatId2].lastCaster.type = caster.type;
				buffer2[combatStatId2].lastCaster.id = caster.id;
				buffer2[combatStatId2].lastCaster.astroId = caster.astroId;
				ptr = ref buffer2[combatStatId2];
			}
			else
			{
				ref CombatStat ptr7 = ref this.combatStats.Add();
				ptr7.hpMax = (int)((float)SkillSystem.HpMaxByModelIndex[(int)ptr6.modelIndex] * this.history.combatDroneDurabilityRatio * (1f + this.history.globalHpEnhancement) + 0.5f);
				ptr7.hp = ptr7.hpMax - damage;
				ptr7.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr6.modelIndex];
				ptr7.astroId = (ptr7.originAstroId = factory.planetId);
				ptr7.objectType = (int)target.type;
				ptr7.objectId = target.id;
				ptr7.dynamic = (ptr6.dynamic ? 1 : 0);
				ptr7.localPos = (ptr6.dynamic ? ptr6.pos : (ptr6.pos + ptr6.pos.normalized * (double)SkillSystem.BarHeightByModelIndex[(int)ptr6.modelIndex]));
				ptr7.size = SkillSystem.BarWidthByModelIndex[(int)ptr6.modelIndex];
				ptr7.lastCaster.type = caster.type;
				ptr7.lastCaster.id = caster.id;
				ptr7.lastCaster.astroId = caster.astroId;
				ptr6.combatStatId = ptr7.id;
				ptr = ref ptr7;
			}
		}
		else if (target.type == ETargetType.None)
		{
			ref EntityData ptr8 = ref factory.entityPool[target.id];
			if (ptr8.id != target.id)
			{
				return ref CombatStat.temp;
			}
			if (ptr8.combatStatId > 0)
			{
				CombatStat[] buffer3 = this.combatStats.buffer;
				int combatStatId3 = ptr8.combatStatId;
				buffer3[combatStatId3].hp = buffer3[combatStatId3].hp - damage;
				buffer3[combatStatId3].lastCaster.type = caster.type;
				buffer3[combatStatId3].lastCaster.id = caster.id;
				buffer3[combatStatId3].lastCaster.astroId = caster.astroId;
				ptr = ref buffer3[combatStatId3];
			}
			else
			{
				ref CombatStat ptr9 = ref this.combatStats.Add();
				ptr9.hpMax = (int)((float)SkillSystem.HpMaxByModelIndex[(int)ptr8.modelIndex] * (1f + this.history.globalHpEnhancement) + 0.5f);
				ptr9.hp = ptr9.hpMax - damage;
				ptr9.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr8.modelIndex];
				ptr9.astroId = (ptr9.originAstroId = factory.planetId);
				ptr9.objectType = (int)target.type;
				ptr9.objectId = target.id;
				ptr9.dynamic = 0;
				ptr9.localPos = ptr8.pos + ptr8.pos.normalized * SkillSystem.BarHeightByModelIndex[(int)ptr8.modelIndex];
				ptr9.size = SkillSystem.BarWidthByModelIndex[(int)ptr8.modelIndex];
				ptr9.lastCaster.type = caster.type;
				ptr9.lastCaster.id = caster.id;
				ptr9.lastCaster.astroId = caster.astroId;
				ptr8.combatStatId = ptr9.id;
				ptr = ref ptr9;
				ref WarningData ptr10 = ref this.gameData.warningSystem.NewWarningData(-4, ptr9.ID, 512);
				ptr10.astroId = ptr9.originAstroId;
				ptr10.localPos = ptr8.pos;
				ptr10.localPos += ptr8.pos.normalized * SpaceSector.PrefabDescByModelIndex[(int)ptr8.modelIndex].signHeight;
				ptr10.state = ((ptr9.hp > 0 && (EntitySignRenderer.buildingWarningMask & 2048) > 0) ? 1 : 0);
				ptr10.detailId1 = (int)ptr8.protoId;
				if (ptr8.ignoreRepairWarning)
				{
					ptr10.state = 0;
				}
				ptr9.warningId = ptr10.id;
			}
			if (ptr8.constructStatId > 0)
			{
				ConstructStat[] buffer4 = factory.constructionSystem.constructStats.buffer;
				int constructStatId = ptr8.constructStatId;
				buffer4[constructStatId].damageRegister = buffer4[constructStatId].damageRegister + damage;
			}
			else
			{
				int constructStatId2 = factory.constructionSystem.AddConstructStat(target.id, damage);
				ptr8.constructStatId = constructStatId2;
			}
		}
		else if (target.type == ETargetType.Vegetable)
		{
			ref VegeData ptr11 = ref factory.vegePool[target.id];
			if (ptr11.id != target.id)
			{
				return ref CombatStat.temp;
			}
			if (ptr11.combatStatId > 0)
			{
				CombatStat[] buffer5 = this.combatStats.buffer;
				int combatStatId4 = ptr11.combatStatId;
				buffer5[combatStatId4].hp = buffer5[combatStatId4].hp - damage;
				buffer5[combatStatId4].lastCaster.type = caster.type;
				buffer5[combatStatId4].lastCaster.id = caster.id;
				buffer5[combatStatId4].lastCaster.astroId = caster.astroId;
				ptr = ref buffer5[combatStatId4];
			}
			else
			{
				ref CombatStat ptr12 = ref this.combatStats.Add();
				ptr12.hpMax = SkillSystem.HpMaxByModelIndex[(int)ptr11.modelIndex];
				ptr12.hp = ptr12.hpMax - damage;
				ptr12.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr11.modelIndex];
				ptr12.astroId = (ptr12.originAstroId = factory.planetId);
				ptr12.objectType = (int)target.type;
				ptr12.objectId = target.id;
				ptr12.dynamic = 0;
				ptr12.localPos = ptr11.pos + ptr11.pos.normalized * SkillSystem.BarHeightByModelIndex[(int)ptr11.modelIndex];
				ptr12.size = SkillSystem.BarWidthByModelIndex[(int)ptr11.modelIndex];
				ptr12.lastCaster.type = caster.type;
				ptr12.lastCaster.id = caster.id;
				ptr12.lastCaster.astroId = caster.astroId;
				ptr11.combatStatId = ptr12.id;
				ptr = ref ptr12;
			}
		}
		else if (target.type == ETargetType.Vein)
		{
			ref VeinData ptr13 = ref factory.veinPool[target.id];
			if (ptr13.id != target.id)
			{
				return ref CombatStat.temp;
			}
			if (ptr13.combatStatId > 0)
			{
				CombatStat[] buffer6 = this.combatStats.buffer;
				int combatStatId5 = ptr13.combatStatId;
				buffer6[combatStatId5].hp = buffer6[combatStatId5].hp - damage;
				buffer6[combatStatId5].lastCaster.type = caster.type;
				buffer6[combatStatId5].lastCaster.id = caster.id;
				buffer6[combatStatId5].lastCaster.astroId = caster.astroId;
				ptr = ref buffer6[combatStatId5];
			}
			else
			{
				ref CombatStat ptr14 = ref this.combatStats.Add();
				ptr14.hpMax = SkillSystem.HpMaxByModelIndex[(int)ptr13.modelIndex];
				ptr14.hp = ptr14.hpMax - damage;
				ptr14.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr13.modelIndex];
				ptr14.astroId = (ptr14.originAstroId = factory.planetId);
				ptr14.objectType = (int)target.type;
				ptr14.objectId = target.id;
				ptr14.dynamic = 0;
				ptr14.localPos = ptr13.pos + ptr13.pos.normalized * SkillSystem.BarHeightByModelIndex[(int)ptr13.modelIndex];
				ptr14.size = SkillSystem.BarWidthByModelIndex[(int)ptr13.modelIndex];
				ptr14.lastCaster.type = caster.type;
				ptr14.lastCaster.id = caster.id;
				ptr14.lastCaster.astroId = caster.astroId;
				ptr13.combatStatId = ptr14.id;
				ptr = ref ptr14;
			}
		}
		else if (target.type == ETargetType.Player && target.id == 1)
		{
			this.mecha.TakeDamage(damage);
			this.AddMechaHatred(caster.astroId, caster.id, damage);
		}
		return ref ptr;
	}

	// Token: 0x06000E90 RID: 3728 RVA: 0x000E2B04 File Offset: 0x000E0D04
	public bool MechaEnergyShieldResist(SkillTarget caster, ref int damage)
	{
		this.AddMechaHatred(caster.astroId, caster.id, damage);
		return this.mecha.EnergyShieldResist(ref damage);
	}

	// Token: 0x06000E91 RID: 3729 RVA: 0x000E2B26 File Offset: 0x000E0D26
	public bool MechaEnergyShieldResist(SkillTargetLocal caster, int astroId, ref int damage)
	{
		this.AddMechaHatred(astroId, caster.id, damage);
		return this.mecha.EnergyShieldResist(ref damage);
	}

	// Token: 0x06000E92 RID: 3730 RVA: 0x000E2B44 File Offset: 0x000E0D44
	public ref readonly CombatStat GetCombatStat(ref SkillTarget obj)
	{
		CombatStat.temp.Reset();
		if (obj.id == 0)
		{
			return ref CombatStat.temp;
		}
		if (obj.type == ETargetType.Enemy)
		{
			int astroId = obj.astroId;
			if (astroId > 1000000)
			{
				ref EnemyData ptr = ref this.sector.enemyPool[obj.id];
				if (ptr.isInvincible)
				{
					return ref CombatStat.temp;
				}
				object obj2 = this.combat_stat_rw_lock;
				lock (obj2)
				{
					int combatStatId = ptr.combatStatId;
					if (combatStatId > 0)
					{
						return ref this.combatStats.buffer[combatStatId];
					}
					EnemyDFHiveSystem enemyDFHiveSystem = this.sector.dfHivesByAstro[astroId - 1000000];
					int num = 0;
					if (enemyDFHiveSystem != null)
					{
						num = enemyDFHiveSystem.evolve.level;
					}
					int num2;
					using (this.combatStats.Add(out num2))
					{
						ref CombatStat ptr2 = ref this.combatStats.buffer[num2];
						ptr2.hpMax = SkillSystem.HpMaxByModelIndex[(int)ptr.modelIndex] + num * SkillSystem.HpUpgradeByModelIndex[(int)ptr.modelIndex];
						ptr2.hp = ptr2.hpMax;
						ptr2.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr.modelIndex];
						ptr2.astroId = (ptr2.originAstroId = astroId);
						ptr2.objectType = (int)obj.type;
						ptr2.objectId = obj.id;
						ptr2.dynamic = (ptr.dynamic ? 1 : 0);
						ptr2.localPos = (ptr.dynamic ? ptr.pos : (ptr.pos + new VectorLF3(0f, Mathf.Sign((float)ptr.pos.y) * SkillSystem.BarHeightByModelIndex[(int)ptr.modelIndex], 0f)));
						ptr2.size = SkillSystem.BarWidthByModelIndex[(int)ptr.modelIndex];
						ptr.combatStatId = ptr2.id;
						return ref ptr2;
					}
				}
			}
			if (astroId > 100 && astroId <= 204899 && astroId % 100 > 0)
			{
				PlanetFactory planetFactory = this.astroFactories[astroId];
				ref EnemyData ptr3 = ref planetFactory.enemyPool[obj.id];
				if (ptr3.isInvincible)
				{
					return ref CombatStat.temp;
				}
				object obj2 = this.combat_stat_rw_lock;
				lock (obj2)
				{
					int combatStatId2 = ptr3.combatStatId;
					if (combatStatId2 > 0)
					{
						return ref this.combatStats.buffer[combatStatId2];
					}
					DFGBaseComponent dfgbaseComponent = (ptr3.owner > 0) ? planetFactory.enemySystem.bases.buffer[(int)ptr3.owner] : null;
					if (dfgbaseComponent != null && dfgbaseComponent.id != (int)ptr3.owner)
					{
						dfgbaseComponent = null;
					}
					int num3 = 0;
					if (dfgbaseComponent != null)
					{
						num3 = dfgbaseComponent.evolve.level;
					}
					int num4;
					using (this.combatStats.Add(out num4))
					{
						ref CombatStat ptr4 = ref this.combatStats.buffer[num4];
						ptr4.hpMax = SkillSystem.HpMaxByModelIndex[(int)ptr3.modelIndex] + num3 * SkillSystem.HpUpgradeByModelIndex[(int)ptr3.modelIndex];
						ptr4.hp = ptr4.hpMax;
						ptr4.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr3.modelIndex];
						ptr4.astroId = (ptr4.originAstroId = planetFactory.planetId);
						ptr4.objectType = (int)obj.type;
						ptr4.objectId = obj.id;
						ptr4.dynamic = (ptr3.dynamic ? 1 : 0);
						ptr4.localPos = (ptr3.dynamic ? ptr3.pos : (ptr3.pos + ptr3.pos.normalized * (double)SkillSystem.BarHeightByModelIndex[(int)ptr3.modelIndex]));
						ptr4.size = SkillSystem.BarWidthByModelIndex[(int)ptr3.modelIndex];
						ptr3.combatStatId = ptr4.id;
						return ref ptr4;
					}
				}
			}
			return ref CombatStat.temp;
		}
		if (obj.type == ETargetType.Craft)
		{
			int astroId2 = obj.astroId;
			if (astroId2 > 1000000 || astroId2 == 0)
			{
				ref CraftData ptr5 = ref this.sector.craftPool[obj.id];
				if (ptr5.isInvincible)
				{
					return ref CombatStat.temp;
				}
				object obj2 = this.combat_stat_rw_lock;
				lock (obj2)
				{
					int combatStatId3 = ptr5.combatStatId;
					if (combatStatId3 > 0)
					{
						return ref this.combatStats.buffer[combatStatId3];
					}
					int num5;
					using (this.combatStats.Add(out num5))
					{
						ref CombatStat ptr6 = ref this.combatStats.buffer[num5];
						ptr6.hpMax = (int)((float)SkillSystem.HpMaxByModelIndex[(int)ptr5.modelIndex] * this.history.combatDroneDurabilityRatio * (1f + this.history.globalHpEnhancement) + 0.5f);
						ptr6.hp = ptr6.hpMax;
						ptr6.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr5.modelIndex];
						ptr6.astroId = (ptr6.originAstroId = astroId2);
						ptr6.objectType = (int)obj.type;
						ptr6.objectId = obj.id;
						ptr6.dynamic = (ptr5.dynamic ? 1 : 0);
						ptr6.localPos = (ptr5.dynamic ? ptr5.pos : (ptr5.pos + ptr5.pos.normalized * (double)SkillSystem.BarHeightByModelIndex[(int)ptr5.modelIndex]));
						ptr6.size = SkillSystem.BarWidthByModelIndex[(int)ptr5.modelIndex];
						ptr5.combatStatId = ptr6.id;
						return ref ptr6;
					}
				}
			}
			if (astroId2 > 100 && astroId2 <= 204899 && astroId2 % 100 > 0)
			{
				PlanetFactory planetFactory2 = this.astroFactories[astroId2];
				ref CraftData ptr7 = ref planetFactory2.craftPool[obj.id];
				if (ptr7.isInvincible)
				{
					return ref CombatStat.temp;
				}
				object obj2 = this.combat_stat_rw_lock;
				lock (obj2)
				{
					int combatStatId4 = ptr7.combatStatId;
					if (combatStatId4 > 0)
					{
						return ref this.combatStats.buffer[combatStatId4];
					}
					int num6;
					using (this.combatStats.Add(out num6))
					{
						ref CombatStat ptr8 = ref this.combatStats.buffer[num6];
						ptr8.hpMax = (int)((float)SkillSystem.HpMaxByModelIndex[(int)ptr7.modelIndex] * this.history.combatDroneDurabilityRatio * (1f + this.history.globalHpEnhancement) + 0.5f);
						ptr8.hp = ptr8.hpMax;
						ptr8.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr7.modelIndex];
						ptr8.astroId = (ptr8.originAstroId = planetFactory2.planetId);
						ptr8.objectType = (int)obj.type;
						ptr8.objectId = obj.id;
						ptr8.dynamic = (ptr7.dynamic ? 1 : 0);
						ptr8.localPos = (ptr7.dynamic ? ptr7.pos : (ptr7.pos + ptr7.pos.normalized * (double)SkillSystem.BarHeightByModelIndex[(int)ptr7.modelIndex]));
						ptr8.size = SkillSystem.BarWidthByModelIndex[(int)ptr7.modelIndex];
						ptr7.combatStatId = ptr8.id;
						return ref ptr8;
					}
				}
			}
			return ref CombatStat.temp;
		}
		if (obj.type == ETargetType.None)
		{
			int astroId3 = obj.astroId;
			if (astroId3 > 100 && astroId3 <= 204899 && astroId3 % 100 > 0)
			{
				PlanetFactory planetFactory3 = this.astroFactories[astroId3];
				ref EntityData ptr9 = ref planetFactory3.entityPool[obj.id];
				object obj2 = this.combat_stat_rw_lock;
				lock (obj2)
				{
					int combatStatId5 = ptr9.combatStatId;
					if (combatStatId5 > 0)
					{
						return ref this.combatStats.buffer[combatStatId5];
					}
					int num7;
					using (this.combatStats.Add(out num7))
					{
						ref CombatStat ptr10 = ref this.combatStats.buffer[num7];
						ptr10.hpMax = (int)((float)SkillSystem.HpMaxByModelIndex[(int)ptr9.modelIndex] * (1f + this.history.globalHpEnhancement) + 0.5f);
						ptr10.hp = ptr10.hpMax;
						ptr10.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr9.modelIndex];
						ptr10.astroId = (ptr10.originAstroId = planetFactory3.planetId);
						ptr10.objectType = (int)obj.type;
						ptr10.objectId = obj.id;
						ptr10.dynamic = 0;
						ptr10.localPos = ptr9.pos + ptr9.pos.normalized * SkillSystem.BarHeightByModelIndex[(int)ptr9.modelIndex];
						ptr10.size = SkillSystem.BarWidthByModelIndex[(int)ptr9.modelIndex];
						ptr9.combatStatId = ptr10.id;
						return ref ptr10;
					}
				}
			}
			return ref CombatStat.temp;
		}
		if (obj.type == ETargetType.Vegetable)
		{
			int astroId4 = obj.astroId;
			if (astroId4 > 100 && astroId4 <= 204899 && astroId4 % 100 > 0)
			{
				PlanetFactory planetFactory4 = this.astroFactories[astroId4];
				ref VegeData ptr11 = ref planetFactory4.vegePool[obj.id];
				object obj2 = this.combat_stat_rw_lock;
				lock (obj2)
				{
					int combatStatId6 = ptr11.combatStatId;
					if (combatStatId6 > 0)
					{
						return ref this.combatStats.buffer[combatStatId6];
					}
					int num8;
					using (this.combatStats.Add(out num8))
					{
						ref CombatStat ptr12 = ref this.combatStats.buffer[num8];
						ptr12.hpMax = SkillSystem.HpMaxByModelIndex[(int)ptr11.modelIndex];
						ptr12.hp = ptr12.hpMax;
						ptr12.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr11.modelIndex];
						ptr12.astroId = (ptr12.originAstroId = planetFactory4.planetId);
						ptr12.objectType = (int)obj.type;
						ptr12.objectId = obj.id;
						ptr12.dynamic = 0;
						ptr12.localPos = ptr11.pos + ptr11.pos.normalized * SkillSystem.BarHeightByModelIndex[(int)ptr11.modelIndex];
						ptr12.size = SkillSystem.BarWidthByModelIndex[(int)ptr11.modelIndex];
						ptr11.combatStatId = ptr12.id;
						return ref ptr12;
					}
				}
			}
			return ref CombatStat.temp;
		}
		if (obj.type == ETargetType.Vein)
		{
			int astroId5 = obj.astroId;
			if (astroId5 > 100 && astroId5 <= 204899 && astroId5 % 100 > 0)
			{
				PlanetFactory planetFactory5 = this.astroFactories[astroId5];
				ref VeinData ptr13 = ref planetFactory5.veinPool[obj.id];
				object obj2 = this.combat_stat_rw_lock;
				lock (obj2)
				{
					int combatStatId7 = ptr13.combatStatId;
					if (combatStatId7 > 0)
					{
						return ref this.combatStats.buffer[combatStatId7];
					}
					int num9;
					using (this.combatStats.Add(out num9))
					{
						ref CombatStat ptr14 = ref this.combatStats.buffer[num9];
						ptr14.hpMax = SkillSystem.HpMaxByModelIndex[(int)ptr13.modelIndex];
						ptr14.hp = ptr14.hpMax;
						ptr14.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr13.modelIndex];
						ptr14.astroId = (ptr14.originAstroId = planetFactory5.planetId);
						ptr14.objectType = (int)obj.type;
						ptr14.objectId = obj.id;
						ptr14.dynamic = 0;
						ptr14.localPos = ptr13.pos + ptr13.pos.normalized * SkillSystem.BarHeightByModelIndex[(int)ptr13.modelIndex];
						ptr14.size = SkillSystem.BarWidthByModelIndex[(int)ptr13.modelIndex];
						ptr13.combatStatId = ptr14.id;
						return ref ptr14;
					}
				}
			}
			return ref CombatStat.temp;
		}
		return ref CombatStat.temp;
	}

	// Token: 0x06000E93 RID: 3731 RVA: 0x000E3948 File Offset: 0x000E1B48
	public int AddCombatStatHPIncoming(ref SkillTarget obj, int addValue)
	{
		if (obj.id == 0)
		{
			return 0;
		}
		if (obj.type == ETargetType.Enemy)
		{
			int astroId = obj.astroId;
			if (astroId > 1000000)
			{
				ref EnemyData ptr = ref this.sector.enemyPool[obj.id];
				if (ptr.isInvincible)
				{
					return 0;
				}
				object obj2 = this.combat_stat_rw_lock;
				lock (obj2)
				{
					int num = ptr.combatStatId;
					if (num > 0)
					{
						CombatStat[] buffer = this.combatStats.buffer;
						int num2 = num;
						buffer[num2].hpIncoming = buffer[num2].hpIncoming + addValue;
					}
					else
					{
						EnemyDFHiveSystem enemyDFHiveSystem = this.sector.dfHivesByAstro[astroId - 1000000];
						int num3 = 0;
						if (enemyDFHiveSystem != null)
						{
							num3 = enemyDFHiveSystem.evolve.level;
						}
						int num4;
						using (this.combatStats.Add(out num4))
						{
							ref CombatStat ptr2 = ref this.combatStats.buffer[num4];
							ptr2.hpMax = SkillSystem.HpMaxByModelIndex[(int)ptr.modelIndex] + num3 * SkillSystem.HpUpgradeByModelIndex[(int)ptr.modelIndex];
							ptr2.hp = ptr2.hpMax;
							ptr2.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr.modelIndex];
							ref CombatStat ptr3 = ref ptr2;
							int num5 = ptr2.originAstroId = astroId;
							ptr3.astroId = num5;
							ptr2.objectType = (int)obj.type;
							ptr2.objectId = obj.id;
							ptr2.dynamic = (ptr.dynamic ? 1 : 0);
							ptr2.localPos = (ptr.dynamic ? ptr.pos : (ptr.pos + new VectorLF3(0f, Mathf.Sign((float)ptr.pos.y) * SkillSystem.BarHeightByModelIndex[(int)ptr.modelIndex], 0f)));
							ptr2.size = SkillSystem.BarWidthByModelIndex[(int)ptr.modelIndex];
							ptr2.hpIncoming = addValue;
							num5 = (ptr.combatStatId = ptr2.id);
							num = num5;
						}
					}
					return num;
				}
			}
			if (astroId > 100 && astroId <= 204899 && astroId % 100 > 0)
			{
				PlanetFactory planetFactory = this.astroFactories[astroId];
				ref EnemyData ptr4 = ref planetFactory.enemyPool[obj.id];
				if (ptr4.isInvincible)
				{
					return 0;
				}
				object obj2 = this.combat_stat_rw_lock;
				lock (obj2)
				{
					int num6 = ptr4.combatStatId;
					if (num6 > 0)
					{
						CombatStat[] buffer2 = this.combatStats.buffer;
						int num7 = num6;
						buffer2[num7].hpIncoming = buffer2[num7].hpIncoming + addValue;
					}
					else
					{
						DFGBaseComponent dfgbaseComponent = (ptr4.owner > 0) ? planetFactory.enemySystem.bases.buffer[(int)ptr4.owner] : null;
						if (dfgbaseComponent != null && dfgbaseComponent.id != (int)ptr4.owner)
						{
							dfgbaseComponent = null;
						}
						int num8 = 0;
						if (dfgbaseComponent != null)
						{
							num8 = dfgbaseComponent.evolve.level;
						}
						int num9;
						using (this.combatStats.Add(out num9))
						{
							ref CombatStat ptr5 = ref this.combatStats.buffer[num9];
							ptr5.hpMax = SkillSystem.HpMaxByModelIndex[(int)ptr4.modelIndex] + num8 * SkillSystem.HpUpgradeByModelIndex[(int)ptr4.modelIndex];
							ptr5.hp = ptr5.hpMax;
							ptr5.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr4.modelIndex];
							ptr5.astroId = (ptr5.originAstroId = planetFactory.planetId);
							ptr5.objectType = (int)obj.type;
							ptr5.objectId = obj.id;
							ptr5.dynamic = (ptr4.dynamic ? 1 : 0);
							ptr5.localPos = (ptr4.dynamic ? ptr4.pos : (ptr4.pos + ptr4.pos.normalized * (double)SkillSystem.BarHeightByModelIndex[(int)ptr4.modelIndex]));
							ptr5.size = SkillSystem.BarWidthByModelIndex[(int)ptr4.modelIndex];
							ptr5.hpIncoming = addValue;
							num6 = (ptr4.combatStatId = ptr5.id);
						}
					}
					return num6;
				}
			}
			return 0;
		}
		if (obj.type == ETargetType.Craft)
		{
			int astroId2 = obj.astroId;
			if (astroId2 > 1000000 || astroId2 == 0)
			{
				ref CraftData ptr6 = ref this.sector.craftPool[obj.id];
				if (ptr6.isInvincible)
				{
					return 0;
				}
				object obj2 = this.combat_stat_rw_lock;
				lock (obj2)
				{
					int num10 = ptr6.combatStatId;
					if (num10 > 0)
					{
						CombatStat[] buffer3 = this.combatStats.buffer;
						int num11 = num10;
						buffer3[num11].hpIncoming = buffer3[num11].hpIncoming + addValue;
					}
					else
					{
						int num12;
						using (this.combatStats.Add(out num12))
						{
							ref CombatStat ptr7 = ref this.combatStats.buffer[num12];
							ptr7.hpMax = (int)((float)SkillSystem.HpMaxByModelIndex[(int)ptr6.modelIndex] * this.history.combatDroneDurabilityRatio * (1f + this.history.globalHpEnhancement) + 0.5f);
							ptr7.hp = ptr7.hpMax;
							ptr7.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr6.modelIndex];
							ptr7.astroId = (ptr7.originAstroId = astroId2);
							ptr7.objectType = (int)obj.type;
							ptr7.objectId = obj.id;
							ptr7.dynamic = (ptr6.dynamic ? 1 : 0);
							ptr7.localPos = (ptr6.dynamic ? ptr6.pos : (ptr6.pos + ptr6.pos.normalized * (double)SkillSystem.BarHeightByModelIndex[(int)ptr6.modelIndex]));
							ptr7.size = SkillSystem.BarWidthByModelIndex[(int)ptr6.modelIndex];
							ptr7.hpIncoming = addValue;
							num10 = (ptr6.combatStatId = ptr7.id);
						}
					}
					return num10;
				}
			}
			if (astroId2 > 100 && astroId2 <= 204899 && astroId2 % 100 > 0)
			{
				PlanetFactory planetFactory2 = this.astroFactories[astroId2];
				ref CraftData ptr8 = ref planetFactory2.craftPool[obj.id];
				if (ptr8.isInvincible)
				{
					return 0;
				}
				object obj2 = this.combat_stat_rw_lock;
				lock (obj2)
				{
					int num13 = ptr8.combatStatId;
					if (num13 > 0)
					{
						CombatStat[] buffer4 = this.combatStats.buffer;
						int num14 = num13;
						buffer4[num14].hpIncoming = buffer4[num14].hpIncoming + addValue;
					}
					else
					{
						int num15;
						using (this.combatStats.Add(out num15))
						{
							ref CombatStat ptr9 = ref this.combatStats.buffer[num15];
							ptr9.hpMax = (int)((float)SkillSystem.HpMaxByModelIndex[(int)ptr8.modelIndex] * this.history.combatDroneDurabilityRatio * (1f + this.history.globalHpEnhancement) + 0.5f);
							ptr9.hp = ptr9.hpMax;
							ptr9.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr8.modelIndex];
							ptr9.astroId = (ptr9.originAstroId = planetFactory2.planetId);
							ptr9.objectType = (int)obj.type;
							ptr9.objectId = obj.id;
							ptr9.dynamic = (ptr8.dynamic ? 1 : 0);
							ptr9.localPos = (ptr8.dynamic ? ptr8.pos : (ptr8.pos + ptr8.pos.normalized * (double)SkillSystem.BarHeightByModelIndex[(int)ptr8.modelIndex]));
							ptr9.size = SkillSystem.BarWidthByModelIndex[(int)ptr8.modelIndex];
							ptr9.hpIncoming = addValue;
							num13 = (ptr8.combatStatId = ptr9.id);
						}
					}
					return num13;
				}
			}
			return 0;
		}
		if (obj.type == ETargetType.None)
		{
			int astroId3 = obj.astroId;
			if (astroId3 > 100 && astroId3 <= 204899 && astroId3 % 100 > 0)
			{
				PlanetFactory planetFactory3 = this.astroFactories[astroId3];
				ref EntityData ptr10 = ref planetFactory3.entityPool[obj.id];
				object obj2 = this.combat_stat_rw_lock;
				lock (obj2)
				{
					int num16 = ptr10.combatStatId;
					if (num16 > 0)
					{
						CombatStat[] buffer5 = this.combatStats.buffer;
						int num17 = num16;
						buffer5[num17].hpIncoming = buffer5[num17].hpIncoming + addValue;
					}
					else
					{
						int num18;
						using (this.combatStats.Add(out num18))
						{
							ref CombatStat ptr11 = ref this.combatStats.buffer[num18];
							ptr11.hpMax = (int)((float)SkillSystem.HpMaxByModelIndex[(int)ptr10.modelIndex] * (1f + this.history.globalHpEnhancement) + 0.5f);
							ptr11.hp = ptr11.hpMax;
							ptr11.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr10.modelIndex];
							ptr11.astroId = (ptr11.originAstroId = planetFactory3.planetId);
							ptr11.objectType = (int)obj.type;
							ptr11.objectId = obj.id;
							ptr11.dynamic = 0;
							ptr11.localPos = ptr10.pos + ptr10.pos.normalized * SkillSystem.BarHeightByModelIndex[(int)ptr10.modelIndex];
							ptr11.size = SkillSystem.BarWidthByModelIndex[(int)ptr10.modelIndex];
							ptr11.hpIncoming = addValue;
							num16 = (ptr10.combatStatId = ptr11.id);
						}
					}
					return num16;
				}
			}
			return 0;
		}
		if (obj.type == ETargetType.Vegetable)
		{
			int astroId4 = obj.astroId;
			if (astroId4 > 100 && astroId4 <= 204899 && astroId4 % 100 > 0)
			{
				PlanetFactory planetFactory4 = this.astroFactories[astroId4];
				ref VegeData ptr12 = ref planetFactory4.vegePool[obj.id];
				object obj2 = this.combat_stat_rw_lock;
				lock (obj2)
				{
					int num19 = ptr12.combatStatId;
					if (num19 > 0)
					{
						CombatStat[] buffer6 = this.combatStats.buffer;
						int num20 = num19;
						buffer6[num20].hpIncoming = buffer6[num20].hpIncoming + addValue;
					}
					else
					{
						int num21;
						using (this.combatStats.Add(out num21))
						{
							ref CombatStat ptr13 = ref this.combatStats.buffer[num21];
							ptr13.hpMax = SkillSystem.HpMaxByModelIndex[(int)ptr12.modelIndex];
							ptr13.hp = ptr13.hpMax;
							ptr13.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr12.modelIndex];
							ptr13.astroId = (ptr13.originAstroId = planetFactory4.planetId);
							ptr13.objectType = (int)obj.type;
							ptr13.objectId = obj.id;
							ptr13.dynamic = 0;
							ptr13.localPos = ptr12.pos + ptr12.pos.normalized * SkillSystem.BarHeightByModelIndex[(int)ptr12.modelIndex];
							ptr13.size = SkillSystem.BarWidthByModelIndex[(int)ptr12.modelIndex];
							ptr13.hpIncoming = addValue;
							num19 = (ptr12.combatStatId = ptr13.id);
						}
					}
					return num19;
				}
			}
			return 0;
		}
		if (obj.type == ETargetType.Vein)
		{
			int astroId5 = obj.astroId;
			if (astroId5 > 100 && astroId5 <= 204899 && astroId5 % 100 > 0)
			{
				PlanetFactory planetFactory5 = this.astroFactories[astroId5];
				ref VeinData ptr14 = ref planetFactory5.veinPool[obj.id];
				object obj2 = this.combat_stat_rw_lock;
				lock (obj2)
				{
					int num22 = ptr14.combatStatId;
					if (num22 > 0)
					{
						CombatStat[] buffer7 = this.combatStats.buffer;
						int num23 = num22;
						buffer7[num23].hpIncoming = buffer7[num23].hpIncoming + addValue;
					}
					else
					{
						int num24;
						using (this.combatStats.Add(out num24))
						{
							ref CombatStat ptr15 = ref this.combatStats.buffer[num24];
							ptr15.hpMax = SkillSystem.HpMaxByModelIndex[(int)ptr14.modelIndex];
							ptr15.hp = ptr15.hpMax;
							ptr15.hpRecover = SkillSystem.HpRecoverByModelIndex[(int)ptr14.modelIndex];
							ptr15.astroId = (ptr15.originAstroId = planetFactory5.planetId);
							ptr15.objectType = (int)obj.type;
							ptr15.objectId = obj.id;
							ptr15.dynamic = 0;
							ptr15.localPos = ptr14.pos + ptr14.pos.normalized * SkillSystem.BarHeightByModelIndex[(int)ptr14.modelIndex];
							ptr15.size = SkillSystem.BarWidthByModelIndex[(int)ptr14.modelIndex];
							ptr15.hpIncoming = addValue;
							num22 = (ptr14.combatStatId = ptr15.id);
						}
					}
					return num22;
				}
			}
			return 0;
		}
		return 0;
	}

	// Token: 0x06000E94 RID: 3732 RVA: 0x000E47C4 File Offset: 0x000E29C4
	public int CalculateDamageIncoming(ref SkillTarget target, int damage, int slice = 1)
	{
		int num = damage;
		if (target.type == ETargetType.Enemy)
		{
			int astroId = target.astroId;
			if (astroId > 1000000)
			{
				EnemyDFHiveSystem enemyDFHiveSystem = this.sector.dfHivesByAstro[astroId - 1000000];
				if (enemyDFHiveSystem != null)
				{
					int level = enemyDFHiveSystem.evolve.level;
					int num2 = 100 / slice;
					int num3 = level * num2 / 2;
					num -= num3;
					if (num < num2)
					{
						num = num2;
					}
				}
			}
			else
			{
				PlanetFactory planetFactory = this.astroFactories[astroId];
				ref EnemyData ptr = ref planetFactory.enemyPool[target.id];
				if (ptr.id != target.id || ptr.isInvincible)
				{
					return num;
				}
				DFGBaseComponent dfgbaseComponent = null;
				if (ptr.owner > 0)
				{
					dfgbaseComponent = planetFactory.enemySystem.bases[(int)ptr.owner];
					if (dfgbaseComponent.id != (int)ptr.owner)
					{
						dfgbaseComponent = null;
					}
				}
				if (dfgbaseComponent != null)
				{
					int level2 = dfgbaseComponent.evolve.level;
					int num4 = 100 / slice;
					int num5 = level2 * num4 / 5;
					num -= num5;
					if (num < num4)
					{
						num = num4;
					}
				}
			}
		}
		return num;
	}

	// Token: 0x06000E95 RID: 3733 RVA: 0x000E48D0 File Offset: 0x000E2AD0
	public void AddGroundEnemyExp(DFGBaseComponent @base, float damage, int slice)
	{
		lock (@base)
		{
			if (damage > 100000f)
			{
				damage = 100000f;
			}
			float num = damage * (float)slice / (float)(1200 + @base.evolve.level * 500);
			if (num > 10f)
			{
				num = 10f;
			}
			int num2 = (int)(num * damage * 4f * this.combatSettingsTmp.battleExpFactor + 0.5f);
			@base.evolve.AddExpPoint(num2);
			@base.evolve.exppshr = @base.evolve.exppshr + num2;
		}
	}

	// Token: 0x06000E96 RID: 3734 RVA: 0x000E497C File Offset: 0x000E2B7C
	public void AddSpaceEnemyExp(EnemyDFHiveSystem hive, float damage, int slice)
	{
		lock (hive)
		{
			if (damage > 5000000f)
			{
				damage = 5000000f;
			}
			float num = damage * (float)slice * 0.1f / (float)(1000 + hive.evolve.level * 300);
			if (num > 15f)
			{
				num = 15f;
			}
			int num2 = (int)(num * damage * 0.4f * this.combatSettingsTmp.battleExpFactor + 0.5f);
			hive.evolve.AddExpPoint(num2);
			hive.evolve.exppshr = hive.evolve.exppshr + num2;
		}
	}

	// Token: 0x06000E97 RID: 3735 RVA: 0x000E4A2C File Offset: 0x000E2C2C
	public void AddGroundEnemyHatred(DFGBaseComponent @base, ref EnemyData enemy, ETargetType casterType, int casterId)
	{
		if (this.isEnemyHostileTmp && @base != null)
		{
			int num = (int)(80f * this.enemyAggressiveHatredCoefTmp + 0.5f);
			if (!enemy.isAssaultingUnit && this.enemyAggressiveLevelTmp > EAggressiveLevel.Passive)
			{
				Interlocked.Add(ref @base.evolve.threatshr, (int)(50f * this.combatSettingsTmp.battleThreatFactor + 0.5f));
			}
			if (enemy.dynamic)
			{
				if (enemy.unitId > 0)
				{
					ref EnemyUnitComponent ptr = ref @base.groundSystem.units.buffer[enemy.unitId];
					if (ptr.id == enemy.unitId)
					{
						enemy.willBroadcast = true;
						ptr.hatredLock.Enter();
						ptr.hatred.HateTarget(casterType, casterId, num, this.maxHatredGroundDamageTmp, EHatredOperation.Add);
						ptr.hatredLock.Exit();
						return;
					}
				}
			}
			else
			{
				@base.hatredLock.Enter();
				@base.hatred.HateTarget(casterType, casterId, num * 10, this.maxHatredGroundBaseTmp, EHatredOperation.Add);
				@base.hatredLock.Exit();
			}
		}
	}

	// Token: 0x06000E98 RID: 3736 RVA: 0x000E4B3C File Offset: 0x000E2D3C
	public void AddGroundEnemyHatred(DFGBaseComponent @base, ref EnemyData enemy, ETargetType casterType, int casterId, int damage)
	{
		if (this.isEnemyHostileTmp && @base != null)
		{
			if (damage > 50000)
			{
				damage = 50000;
			}
			int num = (int)((float)damage * this.enemyAggressiveHatredCoefTmp / 50f + 0.5f);
			if (num < 1)
			{
				num = 1;
			}
			if (!enemy.isAssaultingUnit && this.enemyAggressiveLevelTmp > EAggressiveLevel.Passive)
			{
				Interlocked.Add(ref @base.evolve.threatshr, (int)((float)damage * 0.2f * this.combatSettingsTmp.battleThreatFactor + 0.5f));
			}
			if (enemy.dynamic)
			{
				if (enemy.unitId > 0)
				{
					ref EnemyUnitComponent ptr = ref @base.groundSystem.units.buffer[enemy.unitId];
					if (ptr.id == enemy.unitId)
					{
						ptr.hatredLock.Enter();
						ptr.hatred.HateTarget(casterType, casterId, num, this.maxHatredGroundDamageTmp, EHatredOperation.Add);
						ptr.hatredLock.Exit();
						return;
					}
				}
			}
			else
			{
				lock (@base)
				{
					if (@base.enemyId == enemy.id)
					{
						num *= 10;
					}
					@base.hatredLock.Enter();
					@base.hatred.HateTarget(casterType, casterId, num * 10, this.maxHatredGroundBaseTmp, EHatredOperation.Add);
					@base.hatredLock.Exit();
					@base.TrySetTurbo(60);
					if (@base.turboTicks > 0 && @base.turboRepress < 10)
					{
						@base.turboRepress = 10;
					}
					if (casterType == ETargetType.Player)
					{
						@base.groundSystem.factory.enemyPool[@base.enemyId].counterAttack = true;
					}
				}
			}
		}
	}

	// Token: 0x06000E99 RID: 3737 RVA: 0x000E4CE8 File Offset: 0x000E2EE8
	public void AddGroundEnemyThreat(DFGBaseComponent @base, ref EnemyData enemy)
	{
		if (this.isEnemyHostileTmp && @base != null && !enemy.isAssaultingUnit && this.enemyAggressiveLevelTmp > EAggressiveLevel.Passive)
		{
			Interlocked.Add(ref @base.evolve.threatshr, (int)(50f * this.combatSettingsTmp.battleThreatFactor + 0.5f));
		}
	}

	// Token: 0x06000E9A RID: 3738 RVA: 0x000E4D3C File Offset: 0x000E2F3C
	public void AddGroundEnemyThreat(DFGBaseComponent @base, ref EnemyData enemy, int damage)
	{
		if (this.isEnemyHostileTmp && @base != null)
		{
			if (damage > 50000)
			{
				damage = 50000;
			}
			if (!enemy.isAssaultingUnit && this.enemyAggressiveLevelTmp > EAggressiveLevel.Passive)
			{
				Interlocked.Add(ref @base.evolve.threatshr, (int)((float)damage * 0.2f * this.combatSettingsTmp.battleThreatFactor + 0.5f));
			}
		}
	}

	// Token: 0x06000E9B RID: 3739 RVA: 0x000E4DA4 File Offset: 0x000E2FA4
	public void AddSpaceEnemyHatred(EnemyDFHiveSystem hive, ref EnemyData enemy, ETargetType casterType, int casterAstroId, int casterId)
	{
		if (this.isEnemyHostileTmp && hive != null)
		{
			int num = (int)(500f * this.enemyAggressiveHatredCoefTmp + 0.5f);
			if (!enemy.isAssaultingUnit && this.enemyAggressiveLevelTmp > EAggressiveLevel.Passive)
			{
				Interlocked.Add(ref hive.evolve.threatshr, (int)(200f * this.combatSettingsTmp.battleThreatFactor + 0.5f));
			}
			if ((casterType == ETargetType.Player && this.playerAlive) || casterType == ETargetType.Craft)
			{
				if (enemy.dynamic)
				{
					if (enemy.unitId <= 0)
					{
						return;
					}
					ref EnemyUnitComponent ptr = ref hive.units.buffer[enemy.unitId];
					if (ptr.id == enemy.unitId)
					{
						ptr.hatredLock.Enter();
						ptr.hatred.HateTarget(casterType, casterId, num, this.maxHatredGroundDamageTmp, EHatredOperation.Add);
						ptr.hatredLock.Exit();
						return;
					}
					return;
				}
				else
				{
					EnemyDFHiveSystem obj = hive;
					lock (obj)
					{
						hive.hatred.HateTarget(casterType, casterId, num * 10, this.maxHatredSpaceHiveTmp, EHatredOperation.Add);
						hive.TrySetTurbo(60);
						if (hive.turboTicks > 0 && hive.turboRepress < 80)
						{
							hive.turboRepress = 80;
						}
						return;
					}
				}
			}
			if (casterType == ETargetType.None && !enemy.isAssaultingUnit)
			{
				EnemyDFHiveSystem obj = hive;
				lock (obj)
				{
					hive.hatredAstros.HateTarget(ETargetType.Astro, casterAstroId, 40, 1000000000, EHatredOperation.Add);
				}
			}
		}
	}

	// Token: 0x06000E9C RID: 3740 RVA: 0x000E4F38 File Offset: 0x000E3138
	public void AddSpaceEnemyHatred(EnemyDFHiveSystem hive, ref EnemyData enemy, ETargetType casterType, int casterAstroId, int casterId, int damage)
	{
		if (this.isEnemyHostileTmp && hive != null)
		{
			if (damage > 1000000)
			{
				damage = 1000000;
			}
			int num = (int)((float)damage * this.enemyAggressiveHatredCoefTmp / 50f + 0.5f);
			if (num < 1)
			{
				num = 1;
			}
			if (!enemy.isAssaultingUnit && this.enemyAggressiveLevelTmp > EAggressiveLevel.Passive)
			{
				Interlocked.Add(ref hive.evolve.threatshr, (int)((float)damage * 0.05f * this.combatSettingsTmp.battleThreatFactor + 0.5f));
			}
			if (enemy.dfRelayId > 0 && this.enemyAggressiveLevelTmp >= EAggressiveLevel.Passive && hive.hiveAstroId == enemy.originAstroId)
			{
				DFRelayComponent dfrelayComponent = hive.relays.buffer[enemy.dfRelayId];
				if (dfrelayComponent.id == enemy.dfRelayId && dfrelayComponent.stage == 2 && dfrelayComponent.baseState > 0)
				{
					if (this.enemyAggressiveLevelTmp > EAggressiveLevel.Passive)
					{
						EnemyDFHiveSystem obj = hive;
						lock (obj)
						{
							if (hive.evolve.waveTicks == 0 && hive.evolve.waveAsmTicks == 0)
							{
								hive.evolve.threat = hive.evolve.threat + (int)((float)damage * 0.0648f * this.combatSettingsTmp.battleThreatFactor + 0.5f);
							}
							hive.evolve.threatshr = hive.evolve.threatshr + (int)((float)damage * 1.8f * this.combatSettingsTmp.battleThreatFactor + 0.5f);
						}
					}
					if (dfrelayComponent.baseId > 0)
					{
						PlanetFactory planetFactory = this.astroFactories[dfrelayComponent.targetAstroId];
						if (dfrelayComponent.baseState == 2 && planetFactory != null)
						{
							EnemyDFHiveSystem obj = hive;
							lock (obj)
							{
								hive.hatredAstros.HateTarget(ETargetType.Astro, dfrelayComponent.targetAstroId, damage / 500, 1000000000, EHatredOperation.Add);
							}
							if (casterType == ETargetType.None && casterAstroId == dfrelayComponent.targetAstroId)
							{
								DFGBaseComponent dfgbaseComponent = planetFactory.enemySystem.bases.buffer[dfrelayComponent.baseId];
								if (dfgbaseComponent != null && dfgbaseComponent.id == dfrelayComponent.baseId)
								{
									this.AddGroundEnemyHatred(dfgbaseComponent, ref planetFactory.enemyPool[dfgbaseComponent.enemyId], ETargetType.None, casterId, damage);
								}
							}
							else if (casterType == ETargetType.Player && this.playerAlive && !this.playerIsSailing)
							{
								DFGBaseComponent dfgbaseComponent2 = planetFactory.enemySystem.bases.buffer[dfrelayComponent.baseId];
								if (dfgbaseComponent2 != null && dfgbaseComponent2.id == dfrelayComponent.baseId)
								{
									this.AddGroundEnemyHatred(dfgbaseComponent2, ref planetFactory.enemyPool[dfgbaseComponent2.enemyId], ETargetType.Player, casterId, damage);
									planetFactory.enemyPool[dfgbaseComponent2.enemyId].counterAttack = true;
								}
							}
						}
					}
				}
			}
			if ((casterType == ETargetType.Player && this.playerAlive) || casterType == ETargetType.Craft)
			{
				if (enemy.dynamic)
				{
					if (enemy.unitId <= 0)
					{
						goto IL_46F;
					}
					ref EnemyUnitComponent ptr = ref hive.units.buffer[enemy.unitId];
					if (ptr.id == enemy.unitId)
					{
						ptr.hatredLock.Enter();
						ptr.hatred.HateTarget(casterType, casterId, num, this.maxHatredSpaceDamageTmp, EHatredOperation.Add);
						ptr.hatredLock.Exit();
						enemy.counterAttack = true;
						goto IL_46F;
					}
					goto IL_46F;
				}
				else
				{
					EnemyDFHiveSystem obj = hive;
					lock (obj)
					{
						hive.hatred.HateTarget(casterType, casterId, num * 10, this.maxHatredSpaceHiveTmp, EHatredOperation.Add);
						goto IL_46F;
					}
				}
			}
			if (casterType == ETargetType.None && !enemy.isAssaultingUnit)
			{
				EnemyDFHiveSystem obj = hive;
				lock (obj)
				{
					hive.hatredAstros.HateTarget(ETargetType.Astro, casterAstroId, damage / 100, 1000000000, EHatredOperation.Add);
				}
				if (enemy.unitId > 0)
				{
					ref EnemyUnitComponent ptr2 = ref hive.units.buffer[enemy.unitId];
					if (ptr2.id == enemy.unitId)
					{
						ptr2.hatredLock.Enter();
						ptr2.hatred.HateTarget(ETargetType.Astro, casterAstroId, num, this.maxHatredSpaceDamageTmp, EHatredOperation.Add);
						ptr2.hatredLock.Exit();
						ptr2.SnitchHatredToHive(this, ETargetType.Astro, casterAstroId, num);
						enemy.counterAttack = true;
					}
				}
				else if (enemy.dfRelayId == 0)
				{
					obj = hive;
					lock (obj)
					{
						hive.hatred.HateTarget(ETargetType.Astro, casterAstroId, num, this.maxHatredSpaceHiveTmp, EHatredOperation.Add);
					}
				}
			}
		}
		IL_46F:
		hive.turboTicks = 20;
	}

	// Token: 0x06000E9D RID: 3741 RVA: 0x000E53FC File Offset: 0x000E35FC
	public void AddGroundCraftHatred(ref CraftData craft, ref UnitComponent unit, ETargetType casterType, int casterId, int damage)
	{
		if (craft.dynamic && !craft.isSpace)
		{
			craft.willBroadcast = true;
			unit.hatred.HateTarget(casterType, casterId, 30 + damage / 50, 1600, EHatredOperation.Add);
		}
	}

	// Token: 0x06000E9E RID: 3742 RVA: 0x000E5431 File Offset: 0x000E3631
	public void AddSpaceCraftHatred(ref CraftData craft, ref UnitComponent unit, ref EnemyData enemy, int damage)
	{
		if (craft.dynamic && craft.isSpace && enemy.isSpace)
		{
			unit.hatred.HateTarget(ETargetType.Enemy, enemy.id, 300 + damage / 50, 16000, EHatredOperation.Add);
		}
	}

	// Token: 0x06000E9F RID: 3743 RVA: 0x000E5470 File Offset: 0x000E3670
	public void AddMechaHatred(int casterAstroId, int casterId, int damage)
	{
		int num = damage / 100;
		this.mecha.HateAmmoTarget(casterAstroId, casterId, num, num, 0.667f);
		this.mecha.HateLaserTarget(casterAstroId, casterId, num, num, 0.667f);
	}

	// Token: 0x06000EA0 RID: 3744 RVA: 0x000E54AC File Offset: 0x000E36AC
	public void RendererUpdate()
	{
		this.spaceLaserOneShotRenderer.Update();
		this.spaceLaserSweepRenderer.Update();
		this.explosiveUnitBombRenderer.Update();
		this.emCapsuleBombRenderer.Update();
		this.liquidBombRenderer.Update();
		this.turretMissileRenderer.Update();
		this.dfsTowerLaserRenderer.Update();
		this.lancerLaserOneShotRenderer.Update();
		this.lancerLaserSweepRenderer.Update();
		this.warshipTypeFLaserRenderer.Update();
		this.mechaSpaceLaserOneShotRenderer.Update();
		this.mechaMissileRenderer.Update();
	}

	// Token: 0x06000EA1 RID: 3745 RVA: 0x000E5540 File Offset: 0x000E3740
	public void RendererDraw()
	{
		if (this.gameData.localPlanet != null)
		{
			this.localGeneralProjectiles.Render();
			this.localLaserContinuous.Render();
			this.localLaserOneShots.Render();
			this.localDisturbingWaves.Render();
			this.raiderLasers.Render();
			this.rangerPlasmas.Render();
			this.guardianPlasmas.Render();
			this.dfgTowerLasers.Render();
			this.dfgTowerPlasmas.Render();
			this.fighterLasers.Render();
			this.fighterPlasmas.Render();
			this.fighterShieldPlasmas.Render();
			this.turretGaussProjectiles.Render();
			this.turretLaserContinuous.Render();
			this.turretDisturbingWave.Render();
			this.mechaLocalGaussProjectiles.Render();
			this.mechaLocalLaserOneShots.Render();
		}
		this.generalProjectiles.Render();
		this.spaceLaserOneShotRenderer.Render();
		this.spaceLaserSweepRenderer.Render();
		this.explosiveUnitBombRenderer.Render();
		this.emCapsuleBombRenderer.Render();
		this.liquidBombRenderer.Render();
		this.turretPlasmas.Render();
		this.turretLocalPlasmas.Render();
		this.turretMissileRenderer.Render();
		this.turretMissileTrails.instMat.SetBuffer("_MissileBuffer", this.turretMissileRenderer.instBuffer);
		this.turretMissileTrails.Render();
		this.dfsTowerPlasmas.Render();
		this.dfsTowerLaserRenderer.Render();
		this.lancerSpacePlasma.Render();
		this.lancerLaserOneShotRenderer.Render();
		this.lancerLaserSweepRenderer.Render();
		this.humpbackProjectiles.Render();
		this.warshipTypeFLaserRenderer.Render();
		this.warshipTypeFPlasmas.Render();
		this.warshipTypeAPlasmas.Render();
		this.mechaSpaceGaussProjectiles.Render();
		this.mechaSpaceLaserOneShotRenderer.Render();
		this.mechaPlasmas.Render();
		this.mechaShieldBursts.Render();
		this.mechaMissileRenderer.Render();
		this.mechaMissileTrails.instMat.SetBuffer("_MissileBuffer", this.mechaMissileRenderer.instBuffer);
		this.mechaMissileTrails.Render();
		for (int i = 1; i < this.hitEffects.Length; i++)
		{
			if (this.hitEffects[i] != null)
			{
				this.hitEffects[i].Render();
			}
		}
	}

	// Token: 0x06000EA2 RID: 3746 RVA: 0x000E5798 File Offset: 0x000E3998
	public void Export(BinaryWriter w)
	{
		w.Write(3);
		this.combatStats.Export(w);
		w.Write(this.removedSkillTargets.Count);
		foreach (SkillTarget skillTarget in this.removedSkillTargets)
		{
			w.Write(skillTarget.id);
			w.Write(skillTarget.astroId);
			w.Write((int)skillTarget.type);
		}
		this.localGeneralProjectiles.Export(w);
		this.localLaserContinuous.Export(w);
		this.localLaserOneShots.Export(w);
		this.localCannonades.Export(w);
		this.localDisturbingWaves.Export(w);
		this.generalProjectiles.Export(w);
		this.spaceLaserOneShots.Export(w);
		this.spaceLaserSweeps.Export(w);
		this.explosiveUnitBombs.Export(w);
		this.emCapsuleBombs.Export(w);
		this.liquidBombs.Export(w);
		this.raiderLasers.Export(w);
		this.rangerPlasmas.Export(w);
		this.guardianPlasmas.Export(w);
		this.dfgTowerLasers.Export(w);
		this.dfgTowerPlasmas.Export(w);
		this.fighterLasers.Export(w);
		this.fighterPlasmas.Export(w);
		this.fighterShieldPlasmas.Export(w);
		this.turretGaussProjectiles.Export(w);
		this.turretLaserContinuous.Export(w);
		this.turretCannonades.Export(w);
		this.turretPlasmas.Export(w);
		this.turretLocalPlasmas.Export(w);
		this.turretMissiles.Export(w);
		this.turretMissileTrails.Export(w);
		this.turretDisturbingWave.Export(w);
		this.dfsTowerPlasmas.Export(w);
		this.dfsTowerLasers.Export(w);
		this.lancerSpacePlasma.Export(w);
		this.lancerLaserOneShots.Export(w);
		this.lancerLaserSweeps.Export(w);
		this.humpbackProjectiles.Export(w);
		this.warshipTypeFLasers.Export(w);
		this.warshipTypeFPlasmas.Export(w);
		this.warshipTypeAPlasmas.Export(w);
		this.mechaLocalGaussProjectiles.Export(w);
		this.mechaSpaceGaussProjectiles.Export(w);
		this.mechaLocalLaserOneShots.Export(w);
		this.mechaSpaceLaserOneShots.Export(w);
		this.mechaLocalCannonades.Export(w);
		this.mechaSpaceCannonades.Export(w);
		this.mechaPlasmas.Export(w);
		this.mechaMissiles.Export(w);
		this.mechaMissileTrails.Export(w);
		this.mechaShieldBursts.Export(w);
		w.Write((short)this.hitEffects.Length);
		for (int i = 0; i < this.hitEffects.Length; i++)
		{
			if (this.hitEffects[i] != null)
			{
				w.Write((short)i);
				this.hitEffects[i].Export(w);
			}
			else
			{
				w.Write(0);
			}
		}
	}

	// Token: 0x06000EA3 RID: 3747 RVA: 0x000E5AA0 File Offset: 0x000E3CA0
	public void Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		this.combatStats.Import(r);
		if (num >= 1)
		{
			int num2 = r.ReadInt32();
			for (int i = 0; i < num2; i++)
			{
				SkillTarget item;
				item.id = r.ReadInt32();
				item.astroId = r.ReadInt32();
				item.type = (ETargetType)r.ReadInt32();
				this.removedSkillTargets.Add(item);
			}
		}
		this.localGeneralProjectiles.Import(r);
		this.localLaserContinuous.Import(r);
		this.localLaserOneShots.Import(r);
		this.localCannonades.Import(r);
		if (num >= 3)
		{
			this.localDisturbingWaves.Import(r);
		}
		this.generalProjectiles.Import(r);
		this.spaceLaserOneShots.Import(r);
		this.spaceLaserSweeps.Import(r);
		if (num >= 3)
		{
			this.explosiveUnitBombs.Import(r);
			this.emCapsuleBombs.Import(r);
			this.liquidBombs.Import(r);
		}
		this.raiderLasers.Import(r);
		this.rangerPlasmas.Import(r);
		this.guardianPlasmas.Import(r);
		this.dfgTowerLasers.Import(r);
		this.dfgTowerPlasmas.Import(r);
		this.fighterLasers.Import(r);
		this.fighterPlasmas.Import(r);
		this.fighterShieldPlasmas.Import(r);
		this.turretGaussProjectiles.Import(r);
		this.turretLaserContinuous.Import(r);
		this.turretCannonades.Import(r);
		this.turretPlasmas.Import(r);
		if (num >= 2)
		{
			this.turretLocalPlasmas.Import(r);
		}
		this.turretMissiles.Import(r);
		this.turretMissileTrails.Import(r);
		if (num >= 3)
		{
			this.turretDisturbingWave.Import(r);
		}
		this.dfsTowerPlasmas.Import(r);
		this.dfsTowerLasers.Import(r);
		this.lancerSpacePlasma.Import(r);
		this.lancerLaserOneShots.Import(r);
		this.lancerLaserSweeps.Import(r);
		this.humpbackProjectiles.Import(r);
		this.warshipTypeFLasers.Import(r);
		this.warshipTypeFPlasmas.Import(r);
		this.warshipTypeAPlasmas.Import(r);
		this.mechaLocalGaussProjectiles.Import(r);
		this.mechaSpaceGaussProjectiles.Import(r);
		this.mechaLocalLaserOneShots.Import(r);
		this.mechaSpaceLaserOneShots.Import(r);
		this.mechaLocalCannonades.Import(r);
		this.mechaSpaceCannonades.Import(r);
		this.mechaPlasmas.Import(r);
		this.mechaMissiles.Import(r);
		this.mechaMissileTrails.Import(r);
		if (num >= 3)
		{
			this.mechaShieldBursts.Import(r);
		}
		int num3 = (int)r.ReadInt16();
		for (int j = 0; j < num3; j++)
		{
			int num4 = (int)r.ReadInt16();
			if (num4 > 0)
			{
				Assert.True(num4 == j);
				if (j < this.hitEffects.Length)
				{
					this.hitEffects[j].Import(r);
				}
				else
				{
					this.hitEffects[0] = new DataPoolRenderer<ParticleData>();
					this.hitEffects[0].Import(r);
					this.hitEffects[0].ResetPool();
					this.hitEffects[0] = null;
				}
			}
		}
		this.playerSkillTargetULast = this.gameData.mainPlayer.mecha.skillTargetUCenter;
		if (this.gameData.patch < 17)
		{
			ConstructionModuleComponent constructionModule = this.gameData.mainPlayer.mecha.constructionModule;
			PlanetFactory[] factories = this.gameData.factories;
			int factoryCount = this.gameData.factoryCount;
			for (int k = 0; k < factoryCount; k++)
			{
				PlanetFactory planetFactory = factories[k];
				if (planetFactory != null)
				{
					EntityData[] entityPool = planetFactory.entityPool;
					int entityCursor = planetFactory.entityCursor;
					for (int l = 1; l < entityCursor; l++)
					{
						ref EntityData ptr = ref entityPool[l];
						if (ptr.id == l && ptr.combatStatId > 0 && ptr.constructStatId == 0)
						{
							int constructStatId = planetFactory.constructionSystem.AddConstructStat(l, 0);
							ptr.constructStatId = constructStatId;
							constructionModule.repairTargetTotalCount++;
						}
					}
				}
			}
		}
	}

	// Token: 0x04000F98 RID: 3992
	public SpaceSector sector;

	// Token: 0x04000F99 RID: 3993
	public GameData gameData;

	// Token: 0x04000F9A RID: 3994
	public GameHistoryData history;

	// Token: 0x04000F9B RID: 3995
	public DataPool<CombatStat> combatStats;

	// Token: 0x04000F9C RID: 3996
	public HashSet<SkillTarget> removedSkillTargets;

	// Token: 0x04000F9D RID: 3997
	public DataPoolRenderer<LocalGeneralProjectile> localGeneralProjectiles;

	// Token: 0x04000F9E RID: 3998
	public DataPoolRenderer<LocalLaserContinuous> localLaserContinuous;

	// Token: 0x04000F9F RID: 3999
	public DataPoolRenderer<LocalLaserOneShot> localLaserOneShots;

	// Token: 0x04000FA0 RID: 4000
	public DataPool<LocalCannonade> localCannonades;

	// Token: 0x04000FA1 RID: 4001
	public DataPoolRenderer<LocalDisturbingWave> localDisturbingWaves;

	// Token: 0x04000FA2 RID: 4002
	public DataPoolRenderer<GeneralProjectile> generalProjectiles;

	// Token: 0x04000FA3 RID: 4003
	public DataPool<SpaceLaserOneShot> spaceLaserOneShots;

	// Token: 0x04000FA4 RID: 4004
	public GenericRenderer<SpaceLaserOneShotRenderingData> spaceLaserOneShotRenderer;

	// Token: 0x04000FA5 RID: 4005
	public DataPool<SpaceLaserSweep> spaceLaserSweeps;

	// Token: 0x04000FA6 RID: 4006
	public GenericRenderer<SpaceLaserSweepRenderingData> spaceLaserSweepRenderer;

	// Token: 0x04000FA7 RID: 4007
	public DataPool<Bomb_Explosive> explosiveUnitBombs;

	// Token: 0x04000FA8 RID: 4008
	public GenericRenderer<GeneralBombRenderingData> explosiveUnitBombRenderer;

	// Token: 0x04000FA9 RID: 4009
	public DataPool<Bomb_EMCapsule> emCapsuleBombs;

	// Token: 0x04000FAA RID: 4010
	public GenericRenderer<GeneralBombRenderingData> emCapsuleBombRenderer;

	// Token: 0x04000FAB RID: 4011
	public DataPool<Bomb_Liquid> liquidBombs;

	// Token: 0x04000FAC RID: 4012
	public GenericRenderer<GeneralBombRenderingData> liquidBombRenderer;

	// Token: 0x04000FAD RID: 4013
	public DataPoolRenderer<LocalLaserOneShot> raiderLasers;

	// Token: 0x04000FAE RID: 4014
	public DataPoolRenderer<LocalGeneralProjectile> rangerPlasmas;

	// Token: 0x04000FAF RID: 4015
	public DataPoolRenderer<LocalGeneralProjectile> guardianPlasmas;

	// Token: 0x04000FB0 RID: 4016
	public DataPoolRenderer<LocalLaserOneShot> dfgTowerLasers;

	// Token: 0x04000FB1 RID: 4017
	public DataPoolRenderer<LocalGeneralProjectile> dfgTowerPlasmas;

	// Token: 0x04000FB2 RID: 4018
	public DataPoolRenderer<LocalLaserOneShot> fighterLasers;

	// Token: 0x04000FB3 RID: 4019
	public DataPoolRenderer<LocalGeneralProjectile> fighterPlasmas;

	// Token: 0x04000FB4 RID: 4020
	public DataPoolRenderer<LocalGeneralProjectile> fighterShieldPlasmas;

	// Token: 0x04000FB5 RID: 4021
	public DataPoolRenderer<LocalGeneralProjectile> turretGaussProjectiles;

	// Token: 0x04000FB6 RID: 4022
	public DataPoolRenderer<LocalLaserContinuous> turretLaserContinuous;

	// Token: 0x04000FB7 RID: 4023
	public DataPool<LocalCannonade> turretCannonades;

	// Token: 0x04000FB8 RID: 4024
	public DataPoolRenderer<GeneralProjectile> turretPlasmas;

	// Token: 0x04000FB9 RID: 4025
	public DataPoolRenderer<GeneralProjectile> turretLocalPlasmas;

	// Token: 0x04000FBA RID: 4026
	public DataPool<GeneralMissile> turretMissiles;

	// Token: 0x04000FBB RID: 4027
	public GenericRenderer<GeneralMissileRenderingData> turretMissileRenderer;

	// Token: 0x04000FBC RID: 4028
	public VFTrailRenderer turretMissileTrails;

	// Token: 0x04000FBD RID: 4029
	public DataPoolRenderer<LocalDisturbingWave> turretDisturbingWave;

	// Token: 0x04000FBE RID: 4030
	public DataPoolRenderer<GeneralProjectile> dfsTowerPlasmas;

	// Token: 0x04000FBF RID: 4031
	public DataPool<SpaceLaserOneShot> dfsTowerLasers;

	// Token: 0x04000FC0 RID: 4032
	public GenericRenderer<SpaceLaserOneShotRenderingData> dfsTowerLaserRenderer;

	// Token: 0x04000FC1 RID: 4033
	public DataPoolRenderer<GeneralProjectile> lancerSpacePlasma;

	// Token: 0x04000FC2 RID: 4034
	public DataPool<SpaceLaserOneShot> lancerLaserOneShots;

	// Token: 0x04000FC3 RID: 4035
	public GenericRenderer<SpaceLaserOneShotRenderingData> lancerLaserOneShotRenderer;

	// Token: 0x04000FC4 RID: 4036
	public DataPool<SpaceLaserSweep> lancerLaserSweeps;

	// Token: 0x04000FC5 RID: 4037
	public GenericRenderer<SpaceLaserSweepRenderingData> lancerLaserSweepRenderer;

	// Token: 0x04000FC6 RID: 4038
	public DataPoolRenderer<GeneralExpImpProjectile> humpbackProjectiles;

	// Token: 0x04000FC7 RID: 4039
	public DataPool<SpaceLaserOneShot> warshipTypeFLasers;

	// Token: 0x04000FC8 RID: 4040
	public GenericRenderer<SpaceLaserOneShotRenderingData> warshipTypeFLaserRenderer;

	// Token: 0x04000FC9 RID: 4041
	public DataPoolRenderer<GeneralProjectile> warshipTypeFPlasmas;

	// Token: 0x04000FCA RID: 4042
	public DataPoolRenderer<GeneralProjectile> warshipTypeAPlasmas;

	// Token: 0x04000FCB RID: 4043
	public DataPoolRenderer<LocalGeneralProjectile> mechaLocalGaussProjectiles;

	// Token: 0x04000FCC RID: 4044
	public DataPoolRenderer<GeneralProjectile> mechaSpaceGaussProjectiles;

	// Token: 0x04000FCD RID: 4045
	public DataPoolRenderer<LocalLaserOneShot> mechaLocalLaserOneShots;

	// Token: 0x04000FCE RID: 4046
	public DataPool<SpaceLaserOneShot> mechaSpaceLaserOneShots;

	// Token: 0x04000FCF RID: 4047
	public GenericRenderer<SpaceLaserOneShotRenderingData> mechaSpaceLaserOneShotRenderer;

	// Token: 0x04000FD0 RID: 4048
	public DataPool<LocalCannonade> mechaLocalCannonades;

	// Token: 0x04000FD1 RID: 4049
	public DataPool<GeneralCannonade> mechaSpaceCannonades;

	// Token: 0x04000FD2 RID: 4050
	public DataPoolRenderer<GeneralProjectile> mechaPlasmas;

	// Token: 0x04000FD3 RID: 4051
	public DataPool<GeneralMissile> mechaMissiles;

	// Token: 0x04000FD4 RID: 4052
	public GenericRenderer<GeneralMissileRenderingData> mechaMissileRenderer;

	// Token: 0x04000FD5 RID: 4053
	public DataPoolRendererMultiMat<GeneralShieldBurst> mechaShieldBursts;

	// Token: 0x04000FD6 RID: 4054
	public VFTrailRenderer mechaMissileTrails;

	// Token: 0x04000FD7 RID: 4055
	public DataPoolRenderer<ParticleData>[] hitEffects;

	// Token: 0x04000FD8 RID: 4056
	public SkillAudioLogic audio;

	// Token: 0x04000FD9 RID: 4057
	public PlanetFactory[] astroFactories;

	// Token: 0x04000FDA RID: 4058
	public readonly object fire_lock = new object();

	// Token: 0x04000FDB RID: 4059
	public readonly object combat_stat_rw_lock = new object();

	// Token: 0x04000FDC RID: 4060
	public static int[] HpMaxByModelIndex;

	// Token: 0x04000FDD RID: 4061
	public static int[] HpUpgradeByModelIndex;

	// Token: 0x04000FDE RID: 4062
	public static int[] HpRecoverByModelIndex;

	// Token: 0x04000FDF RID: 4063
	public static float[] RoughRadiusByModelIndex;

	// Token: 0x04000FE0 RID: 4064
	public static float[] RoughHeightByModelIndex;

	// Token: 0x04000FE1 RID: 4065
	public static float[] RoughWidthByModelIndex;

	// Token: 0x04000FE2 RID: 4066
	public static float[] BarHeightByModelIndex;

	// Token: 0x04000FE3 RID: 4067
	public static float[] BarWidthByModelIndex;

	// Token: 0x04000FE4 RID: 4068
	public static int[] ColliderComplexityByModelIndex;

	// Token: 0x04000FE5 RID: 4069
	public static int[] EnemySandCountByModelIndex;

	// Token: 0x04000FE6 RID: 4070
	private long gameTickTmp;

	// Token: 0x04000FE7 RID: 4071
	private CombatSettings combatSettingsTmp;

	// Token: 0x04000FE8 RID: 4072
	private bool isEnemyHostileTmp;

	// Token: 0x04000FE9 RID: 4073
	private EAggressiveLevel enemyAggressiveLevelTmp;

	// Token: 0x04000FEA RID: 4074
	public float enemyAggressiveHatredCoefTmp;

	// Token: 0x04000FEB RID: 4075
	public int maxHatredGroundTmp;

	// Token: 0x04000FEC RID: 4076
	private int maxHatredGroundDamageTmp;

	// Token: 0x04000FED RID: 4077
	public int maxHatredSpaceTmp;

	// Token: 0x04000FEE RID: 4078
	private int maxHatredSpaceDamageTmp;

	// Token: 0x04000FEF RID: 4079
	public int maxHatredGroundBaseTmp;

	// Token: 0x04000FF0 RID: 4080
	public int maxHatredSpaceHiveTmp;

	// Token: 0x04000FF1 RID: 4081
	public KillStatistics killStatistics;

	// Token: 0x04000FF2 RID: 4082
	public bool playerAlive;

	// Token: 0x04000FF3 RID: 4083
	public bool playerIsSailing;

	// Token: 0x04000FF4 RID: 4084
	public bool playerIsWarping;

	// Token: 0x04000FF5 RID: 4085
	public int playerAstroId;

	// Token: 0x04000FF6 RID: 4086
	public Vector3 playerSkillTargetL;

	// Token: 0x04000FF7 RID: 4087
	public VectorLF3 playerSkillTargetULast;

	// Token: 0x04000FF8 RID: 4088
	public VectorLF3 playerSkillTargetU;

	// Token: 0x04000FF9 RID: 4089
	public Vector3 playerSkillCastLeftL;

	// Token: 0x04000FFA RID: 4090
	public Vector3 playerSkillCastRightL;

	// Token: 0x04000FFB RID: 4091
	public VectorLF3 playerSkillCastLeftU;

	// Token: 0x04000FFC RID: 4092
	public VectorLF3 playerSkillCastRightU;

	// Token: 0x04000FFD RID: 4093
	public float playerAltL;

	// Token: 0x04000FFE RID: 4094
	public Vector3 playerVelocityL;

	// Token: 0x04000FFF RID: 4095
	public Vector3 playerVelocityU;

	// Token: 0x04001000 RID: 4096
	public ColliderData playerSkillColliderL;

	// Token: 0x04001001 RID: 4097
	public ColliderDataLF playerSkillColliderU;

	// Token: 0x04001002 RID: 4098
	public float playerEnergyShieldRadius;

	// Token: 0x04001003 RID: 4099
	public Mecha mecha;

	// Token: 0x04001004 RID: 4100
	public int localAstroId;

	// Token: 0x04001005 RID: 4101
	public int localPlanetAstroId;

	// Token: 0x04001006 RID: 4102
	public int localStarAstroId;

	// Token: 0x04001007 RID: 4103
	public int localPlanetOrStarAstroId;

	// Token: 0x04001008 RID: 4104
	public static float localCannonadeBlastRadius0 = 7f;

	// Token: 0x04001009 RID: 4105
	public static float localCannonadeBlastRadius1 = 13f;

	// Token: 0x0400100A RID: 4106
	public static float localCannonadeBlastFalloff = 0.2f;

	// Token: 0x0400100B RID: 4107
	public static float spaceCannonadeBlastRadius0 = 50f;

	// Token: 0x0400100C RID: 4108
	public static float spaceCannonadeBlastRadius1 = 150f;

	// Token: 0x0400100D RID: 4109
	public static float spaceCannonadeBlastFalloff = 0.3f;

	// Token: 0x0400100E RID: 4110
	public static float plasmaDamageRadius0 = 0f;

	// Token: 0x0400100F RID: 4111
	public static float plasmaDamageRadius1 = 8f;

	// Token: 0x04001010 RID: 4112
	public static float plasmaDamageFalloff = 0.2f;

	// Token: 0x04001011 RID: 4113
	public static float antimatterDamageRadius0 = 0f;

	// Token: 0x04001012 RID: 4114
	public static float antimatterDamageRadius1 = 20f;

	// Token: 0x04001013 RID: 4115
	public static float antimatterDamageFalloff = 0.2f;

	// Token: 0x04001014 RID: 4116
	public static float shieldBurstDamageFalloff = 0.2f;

	// Token: 0x02000C04 RID: 3076
	// (Invoke) Token: 0x06007D7B RID: 32123
	public delegate void DEnemyRefFunc(SkillTarget killer, ref EnemyData enemy);

	// Token: 0x02000C05 RID: 3077
	// (Invoke) Token: 0x06007D7F RID: 32127
	public delegate void DEntityRefFunc(SkillTarget killer, ref EntityData entity);
}
