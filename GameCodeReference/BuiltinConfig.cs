using System;
using UnityEngine;
using UnityEngine.UI.Youthcat;

// Token: 0x0200019D RID: 413
public class BuiltinConfig : ScriptableObject
{
	// Token: 0x0400112F RID: 4399
	[Header("Universe")]
	public UniverseSimulator universeSimulatorPrefab;

	// Token: 0x04001130 RID: 4400
	public BlackHoleHandler blackHolePrefab;

	// Token: 0x04001131 RID: 4401
	public NeutronStarHandler neutronStarPrefab;

	// Token: 0x04001132 RID: 4402
	[Header("Common Materials")]
	public Material planetSurfaceMatProto;

	// Token: 0x04001133 RID: 4403
	public Material planetHeightmapProto;

	// Token: 0x04001134 RID: 4404
	public Material planetMinimapDefault;

	// Token: 0x04001135 RID: 4405
	public Material planetReformMatProto0;

	// Token: 0x04001136 RID: 4406
	public Material planetReformMatProto1;

	// Token: 0x04001137 RID: 4407
	public Material prebuildReadyMat;

	// Token: 0x04001138 RID: 4408
	public Material prebuildReadyMat_Inserter;

	// Token: 0x04001139 RID: 4409
	public Material previewOkMat;

	// Token: 0x0400113A RID: 4410
	public Material previewOkMat_Inserter;

	// Token: 0x0400113B RID: 4411
	public Material previewGizmoMat;

	// Token: 0x0400113C RID: 4412
	public Material previewGizmoMat_Inserter;

	// Token: 0x0400113D RID: 4413
	public Material previewUpgradeMat;

	// Token: 0x0400113E RID: 4414
	public Material previewUpgradeMat_Inserter;

	// Token: 0x0400113F RID: 4415
	public Material previewDowngradeMat;

	// Token: 0x04001140 RID: 4416
	public Material previewDowngradeMat_Inserter;

	// Token: 0x04001141 RID: 4417
	public Material previewDismantleMat;

	// Token: 0x04001142 RID: 4418
	public Material previewDismantleMat_Inserter;

	// Token: 0x04001143 RID: 4419
	public Material previewErrorMat;

	// Token: 0x04001144 RID: 4420
	public Material previewGPUIMat;

	// Token: 0x04001145 RID: 4421
	public Material previewGPUIMat_Inserter;

	// Token: 0x04001146 RID: 4422
	public Material previewBPPreSelectMat;

	// Token: 0x04001147 RID: 4423
	public Material previewBPSelectMat;

	// Token: 0x04001148 RID: 4424
	public Material previewBPDeleteSelectMat;

	// Token: 0x04001149 RID: 4425
	public Material previewBPInserterPreSelectMat;

	// Token: 0x0400114A RID: 4426
	public Material previewBPInserterSelectMat;

	// Token: 0x0400114B RID: 4427
	public Material previewBPInserterDeleteSelectMat;

	// Token: 0x0400114C RID: 4428
	public Material previewErrorMat_Inserter;

	// Token: 0x0400114D RID: 4429
	public Material previewIgnoreMat_Inserter;

	// Token: 0x0400114E RID: 4430
	public Material powerSystemConnMat;

	// Token: 0x0400114F RID: 4431
	public Material powerSystemConnMat2;

	// Token: 0x04001150 RID: 4432
	public Material powerSystemNodeMat;

	// Token: 0x04001151 RID: 4433
	public Material powerSystemConsumerMat;

	// Token: 0x04001152 RID: 4434
	public Material powerSystemGenMat;

	// Token: 0x04001153 RID: 4435
	public Material powerSystemExcMat;

	// Token: 0x04001154 RID: 4436
	public Material defenseSystemTurretMat0;

	// Token: 0x04001155 RID: 4437
	public Material defenseSystemTurretMat1;

	// Token: 0x04001156 RID: 4438
	public Material entitySignMat;

	// Token: 0x04001157 RID: 4439
	public Material warningBatchMat;

	// Token: 0x04001158 RID: 4440
	public Material combatStatMat;

	// Token: 0x04001159 RID: 4441
	public Material milkyWayClusterMat;

	// Token: 0x0400115A RID: 4442
	public Material milkyWayClusterNebuleEffectMat;

	// Token: 0x0400115B RID: 4443
	public Material mechaVoxelArmorMat;

	// Token: 0x0400115C RID: 4444
	public Material mechaSelectedArmorMat;

	// Token: 0x0400115D RID: 4445
	public MechaMaterialSetting[] mechaArmorMaterials;

	// Token: 0x0400115E RID: 4446
	public Vector3[] mechaArmorProperties;

	// Token: 0x0400115F RID: 4447
	[Header("Common Meshes")]
	public Mesh quadMesh;

	// Token: 0x04001160 RID: 4448
	public Mesh planetReformMesh;

	// Token: 0x04001161 RID: 4449
	[Header("Common Texture")]
	public Texture noise256;

	// Token: 0x04001162 RID: 4450
	[Header("Cargo Logistics")]
	public Material cargoMat;

	// Token: 0x04001163 RID: 4451
	public Mesh cargoMesh;

	// Token: 0x04001164 RID: 4452
	public Material trashMat;

	// Token: 0x04001165 RID: 4453
	public Material trashMatHighlight;

	// Token: 0x04001166 RID: 4454
	public Mesh trashMesh;

	// Token: 0x04001167 RID: 4455
	public Material[] beltMat;

	// Token: 0x04001168 RID: 4456
	public Mesh[] beltMesh;

	// Token: 0x04001169 RID: 4457
	public Material[] pathMat;

	// Token: 0x0400116A RID: 4458
	public Mesh[] pathMesh;

	// Token: 0x0400116B RID: 4459
	[Header("Dyson Sphere")]
	public Material solarSailMaterialFar;

	// Token: 0x0400116C RID: 4460
	public Material solarSailMaterialNear;

	// Token: 0x0400116D RID: 4461
	public Material sailBulletMaterial;

	// Token: 0x0400116E RID: 4462
	public Material dysonLayerPaintingOverlayMaterial;

	// Token: 0x0400116F RID: 4463
	public Mesh solarSailMesh;

	// Token: 0x04001170 RID: 4464
	public Gradient dysonSphereSunColors;

	// Token: 0x04001171 RID: 4465
	public Gradient dysonSphereEmissionColors;

	// Token: 0x04001172 RID: 4466
	public Color dysonSphereNeutronSunColor;

	// Token: 0x04001173 RID: 4467
	public Color dysonSphereNeutronEmissionColor;

	// Token: 0x04001174 RID: 4468
	[Header("Other GPU Batching")]
	public Material mechaDroneMat;

	// Token: 0x04001175 RID: 4469
	public Mesh mechaDroneMesh;

	// Token: 0x04001176 RID: 4470
	public Material miniBlockMat;

	// Token: 0x04001177 RID: 4471
	public Mesh miniBlockMesh;

	// Token: 0x04001178 RID: 4472
	public Material[] shipUIMats;

	// Token: 0x04001179 RID: 4473
	public Mesh shipUIMesh;

	// Token: 0x0400117A RID: 4474
	public Material starmapInstancingDefaultMat;

	// Token: 0x0400117B RID: 4475
	[Header("Common Objects")]
	public GameObject[] oceanSpheres;

	// Token: 0x0400117C RID: 4476
	public GameObject nephogramSphere;

	// Token: 0x0400117D RID: 4477
	public GameObject atFieldSphere;

	// Token: 0x0400117E RID: 4478
	public BoxCollider boxColliderPrefab;

	// Token: 0x0400117F RID: 4479
	public CapsuleCollider capsuleColliderPrefab;

	// Token: 0x04001180 RID: 4480
	public SphereCollider sphereColliderPrefab;

	// Token: 0x04001181 RID: 4481
	public BoxCollider spaceBoxColliderPrefab;

	// Token: 0x04001182 RID: 4482
	public CapsuleCollider spaceCapsuleColliderPrefab;

	// Token: 0x04001183 RID: 4483
	public SphereCollider spaceSphereColliderPrefab;

	// Token: 0x04001184 RID: 4484
	public MeshCollider spaceMeshColliderPrefab;

	// Token: 0x04001185 RID: 4485
	public MeshRenderer meshRendererPrefab;

	// Token: 0x04001186 RID: 4486
	public BuildPreviewModel buildModelPrefab;

	// Token: 0x04001187 RID: 4487
	public AudioSource generalAudioSourcePrefab;

	// Token: 0x04001188 RID: 4488
	public AudioSource spaceAudioSourcePrefab;

	// Token: 0x04001189 RID: 4489
	[Header("Gizmo Settings")]
	public RTSTargetGizmo rtsTargetGizmoPrefab;

	// Token: 0x0400118A RID: 4490
	public LineGizmo lineGizmoPrefab;

	// Token: 0x0400118B RID: 4491
	public Material lineGizmoMat;

	// Token: 0x0400118C RID: 4492
	public Texture2D[] lineTextures;

	// Token: 0x0400118D RID: 4493
	public TrackerGizmo trackerGizmoPrefab;

	// Token: 0x0400118E RID: 4494
	public Material[] trackerGizmoMats;

	// Token: 0x0400118F RID: 4495
	public CircleGizmo circleGizmoPrefab;

	// Token: 0x04001190 RID: 4496
	public Material circleGizmoMat;

	// Token: 0x04001191 RID: 4497
	public Texture2D[] circleTextures;

	// Token: 0x04001192 RID: 4498
	public string[] circleAudio;

	// Token: 0x04001193 RID: 4499
	public BoxGizmo boxGizmoPrefab;

	// Token: 0x04001194 RID: 4500
	public Material boxGizmoMat;

	// Token: 0x04001195 RID: 4501
	public BombCurveGizmo bombCurveGizmoPrefab;

	// Token: 0x04001196 RID: 4502
	public Material bombCurveGizmoMat;

	// Token: 0x04001197 RID: 4503
	public BombTargetGizmo bombTargetGizmoPrefab;

	// Token: 0x04001198 RID: 4504
	public Material bombTargetBoxGizmoMat;

	// Token: 0x04001199 RID: 4505
	public Material bombTargetCircleGizmoMat;

	// Token: 0x0400119A RID: 4506
	public Mesh bombTargetBoxGizmoMesh;

	// Token: 0x0400119B RID: 4507
	public Mesh bombTargetCircleGizmoMesh;

	// Token: 0x0400119C RID: 4508
	public LogisticsPairGizmo logisticsPairingGizmoPrefab;

	// Token: 0x0400119D RID: 4509
	public Material logisticsPairingGizmoMat;

	// Token: 0x0400119E RID: 4510
	public Material logisticsPairingBillboardMat;

	// Token: 0x0400119F RID: 4511
	public Color[] gizmoColors;

	// Token: 0x040011A0 RID: 4512
	public GeneralGizmo[] generalGizmoPrefabs;

	// Token: 0x040011A1 RID: 4513
	public BuildingGizmo buildingGizmoPrefab;

	// Token: 0x040011A2 RID: 4514
	public ModelGizmo modelGizmoPrefab;

	// Token: 0x040011A3 RID: 4515
	public Material[] solidGizmoMats;

	// Token: 0x040011A4 RID: 4516
	public Mesh[] solidGizmoMeshes;

	// Token: 0x040011A5 RID: 4517
	public VFAudio vfAudioPrefab;

	// Token: 0x040011A6 RID: 4518
	public VFAudio vfAudioPrefabSkill;

	// Token: 0x040011A7 RID: 4519
	public UIButtonTip uiButtonTipPrefab;

	// Token: 0x040011A8 RID: 4520
	public UIItemTip uiItemTipPrefab;

	// Token: 0x040011A9 RID: 4521
	public LabMatrixEffect labMatrixEffect;

	// Token: 0x040011AA RID: 4522
	[Header("Reform Settings")]
	public Color[] reformColors;

	// Token: 0x040011AB RID: 4523
	[Header("Particle Effect")]
	public VFSailEffect sailEffectPrefab;

	// Token: 0x040011AC RID: 4524
	public VFWarpEffect warpEffectPrefab;

	// Token: 0x040011AD RID: 4525
	public VFParticleEffect[] particleEffectPrefabs;

	// Token: 0x040011AE RID: 4526
	public Mesh rocketMesh;

	// Token: 0x040011AF RID: 4527
	[Header("Shaders")]
	public Shader heightmapShader;

	// Token: 0x040011B0 RID: 4528
	[Header("Other Objects")]
	public SpaceCapsule spaceCapsulePrefab;

	// Token: 0x040011B1 RID: 4529
	[Header("Prebuild GPUI Colors")]
	public Color32 copyPreselectColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	// Token: 0x040011B2 RID: 4530
	public Color32 copySelectedColor = new Color32(44, 123, 250, byte.MaxValue);

	// Token: 0x040011B3 RID: 4531
	public Color32 copyDeleteColor = new Color32(13, 13, 13, byte.MaxValue);

	// Token: 0x040011B4 RID: 4532
	public Color32 copyErrorColor = new Color32(250, 44, 44, byte.MaxValue);

	// Token: 0x040011B5 RID: 4533
	public Color32 blueprintHighlightColor1 = new Color32(0, 0, 0, 0);

	// Token: 0x040011B6 RID: 4534
	public Color32 blueprintHighlightColor2 = new Color32(0, 0, 0, 0);

	// Token: 0x040011B7 RID: 4535
	public Color32 blueprintHighlightColor3 = new Color32(0, 0, 0, 0);

	// Token: 0x040011B8 RID: 4536
	public Color32 pasteErrorColor = new Color32(250, 44, 44, byte.MaxValue);

	// Token: 0x040011B9 RID: 4537
	public Color32 pasteErrorHighlightColor = new Color32(0, 0, 0, 0);

	// Token: 0x040011BA RID: 4538
	public Color32 pastePrestageOkColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 180);

	// Token: 0x040011BB RID: 4539
	public Color32 pasteConfirmOkColor = new Color32(44, 123, 250, byte.MaxValue);

	// Token: 0x040011BC RID: 4540
	public Color32 pasteConfirmNeedConnColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 180);

	// Token: 0x040011BD RID: 4541
	public Color32 pasteNotEnoughItemColor = new Color32(40, 40, 40, byte.MaxValue);

	// Token: 0x040011BE RID: 4542
	[Header("Color Palette")]
	public Color[] colorPalette256;

	// Token: 0x040011BF RID: 4543
	public Color[] colorPalette249;

	// Token: 0x040011C0 RID: 4544
	public Color[] colorPaletteText;

	// Token: 0x040011C1 RID: 4545
	public int[] colorPalette249To256Mapping;

	// Token: 0x040011C2 RID: 4546
	public int[] colorPalette256To249Mapping;

	// Token: 0x040011C3 RID: 4547
	[Header("Blueprint Anchor")]
	public GameObject blueprintAnchor;

	// Token: 0x040011C4 RID: 4548
	[Header("Enemy")]
	public TextAsset[] dfSpaceGrowthPatterns;

	// Token: 0x040011C5 RID: 4549
	public TextAsset[] dfGroundGrowthPatterns;

	// Token: 0x040011C6 RID: 4550
	[Header("Bucket Map")]
	public TextAsset bucketMap;

	// Token: 0x040011C7 RID: 4551
	[Header("Dashboard")]
	public ChartStyleConfig[] chartStyleConfigs;

	// Token: 0x040011C8 RID: 4552
	public UISimpleGeneralTip uiDashboardSimpleTipPrefab;

	// Token: 0x040011C9 RID: 4553
	public UIPopupMenu uiStatPlanBtnPopupMenu;

	// Token: 0x040011CA RID: 4554
	[Header("Marker")]
	public Mesh markerEffectMesh;

	// Token: 0x040011CB RID: 4555
	public Material[] markerEffectMats;

	// Token: 0x040011CC RID: 4556
	public Material[] markerStarmapMats;

	// Token: 0x040011CD RID: 4557
	public Material[] markerSceneUIMats;

	// Token: 0x040011CE RID: 4558
	public Font markerBreifInfoWordFont;

	// Token: 0x040011CF RID: 4559
	[Header("Generic Menu Button")]
	public Sprite[] genericMenuButtonSprites;

	// Token: 0x040011D0 RID: 4560
	[Header("TextIconMapping")]
	public TextIconMapping generalIconMapping;

	// Token: 0x040011D1 RID: 4561
	[Header("TextLayoutLocale")]
	public TextLayoutLocale generalTextLayoutLocale;
}
