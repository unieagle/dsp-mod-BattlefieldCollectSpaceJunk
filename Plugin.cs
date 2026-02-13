using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace BattlefieldAnalysisBaseCollectSpaceJunk
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource? Log;
        internal static Plugin? Instance;

        /// <summary>太空舰船掉落：抽选次数（地面为 3）</summary>
        internal const int SpaceDropRollCount = 12;

        /// <summary>太空舰船掉落：数量倍数（地面为基础值）</summary>
        internal const int SpaceDropCountMultiplier = 100;

        /// <summary>太空舰船等级（用于掉落等级判定，等效地面 level=18）</summary>
        internal const int SpaceCraftEnemyLevel = 18;

        /// <summary>落点相对基站的横向偏移范围（米），8-22 避免与基站模型重叠</summary>
        private const float LandOffsetMin = 8f;
        private const float LandOffsetMax = 22f;

        /// <summary>坠落物抵达落点的目标时长（秒），据此计算速度大小以保证一致的坠落体验</summary>
        internal const double FallDurationSeconds = 10.0;

        /// <summary>最小下落速度（m/s），避免近处时过慢</summary>
        internal const double MinFallSpeed = 30.0;

        /// <summary>距离地表此高度（米）以内不再控制速度，交由游戏逻辑处理</summary>
        internal const double HandoffHeightAboveSurface = 10.0;

        /// <summary>落地时目标速度（m/s），先快后慢的缓动确保平稳着陆</summary>
        internal const double LandingSpeed = 30.0;

        /// <summary>轨道与地表的最小间隙（米），确保绕背时不会与星球碰撞（Bezier 曲线会略向内弯曲，控制点需留足余量）</summary>
        internal const double CurveClearanceMargin = 1200.0;

        /// <summary>高于此高度时不叠加行星速度，避免远处 GetUniversalVelocityAtLocalPoint 返回过大值导致抖动</summary>
        internal const double PlanetVelBlendHeight = 500.0;

        /// <summary>太空掉落 life 基础值（tick）。游戏每 tick 减 1，10 秒约 600 tick；按原生公式缩短后至少保留 1/3，故 2400 可保证至少约 10 秒。</summary>
        private const int SpaceDropBaseLifeTicks = 2400;

        /// <summary>仅当击毁位置与当前星球距离小于此值（米，与 uPosition/日志距离一致）时生成尾迹粒子。约 2000 km，覆盖同轨道“星球附近”场景。</summary>
        private const double TrailMaxDistFromLocalPlanet = 2_000_000.0;

        /// <summary>每个目标星球最多允许的太空掉落堆数（未拾取/未过期），超出后本事件内不再生成，避免单星堆积过多。</summary>
        internal const int MaxTrashPerPlanet = 1000;

        /// <summary>当前击毁事件剩余可生成堆数（由 Patch 在每次击毁时设置，SpawnTrashAtDeathPosition 扣减）。-1 表示不限制。</summary>
        internal static int _currentPlanetSpawnQuota = -1;

        private static uint _spaceDropSeed = 12345u;
        private bool _hasLoggedEnemyDropTable;

        /// <summary>本 Mod 生成的坠落物：尾焰粒子 + 目标落点 + 生成时间与位置（用于平滑轨道）</summary>
        internal readonly List<(int trashIndex, GameObject trailGo, float landedAt, Vector3 targetLandPosLocal, double spawnGameTime, VectorLF3 spawnPos)> _trashTrails = new List<(int, GameObject, float, Vector3, double, VectorLF3)>();
        private const float TrailDestroyDelayAfterLand = 2.5f;

        private void Awake()
        {
            Log = Logger;
            Instance = this;
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll(typeof(SpaceCraftDeathPatch));
            harmony.PatchAll(typeof(SpaceEnemyDeathPatch));
            harmony.PatchAll(typeof(TrashSystemGravityPatch));
            Log.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 加载中 (GUID: {PluginInfo.PLUGIN_GUID})");
            Log.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 击毁黑雾太空飞船时，按地面掉落表抽选 {SpaceDropRollCount} 次、数量×{SpaceDropCountMultiplier}，从飞船位置坠向战场基站。");
        }

        private void Update()
        {
            if (!_hasLoggedEnemyDropTable && LDB.items?.dataArray != null)
            {
                LogEnemyDropTable();
                _hasLoggedEnemyDropTable = true;
            }

            UpdateTrashTrails();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _trashTrails.Count; i++)
                DestroyTrailGameObject(_trashTrails[i].trailGo);
            _trashTrails.Clear();
        }

        /// <summary>一次性打印地面黑雾敌人掉落表（LDB.items 中 EnemyDropRange.y &gt; 0 的物品）到 BepInEx 日志。</summary>
        private void LogEnemyDropTable()
        {
            try
            {
                var dataArray = LDB.items.dataArray;
                var list = new List<string>();
                for (int j = 0; j < dataArray.Length; j++)
                {
                    var item = dataArray[j];
                    if (item == null || item.EnemyDropRange.y <= 5E-05f)
                        continue;
                    string name = item.name ?? $"id={item.ID}";
                    list.Add($"{item.ID}|{name}|Range=({item.EnemyDropRange.x:F4},{item.EnemyDropRange.y:F4})|Count={item.EnemyDropCount:F2}|Level={item.EnemyDropLevel}|Mask={item.EnemyDropMask}|MaskRatio={item.EnemyDropMaskRatio:F2}");
                }
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ====== 地面黑雾敌人掉落表 (EnemyDropRange.y>0) 共 {list.Count} 项 ======");
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 格式: itemId|名称|Range(起点,宽度)|基础数量|等级|Mask|MaskRatio");
                foreach (var s in list)
                    Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] {s}");
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ====== 掉落表结束 ======");
            }
            catch (Exception ex)
            {
                Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] LogEnemyDropTable 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>每帧将尾焰位置同步到对应 Trash 的 uPos（相对坐标），回收或无效的则销毁并移除。</summary>
        private void UpdateTrashTrails()
        {
            if (GameMain.data?.trashSystem?.container == null)
                return;

            var container = GameMain.data.trashSystem.container;
            var objPool = container.trashObjPool;
            var dataPool = container.trashDataPool;
            var gd = GameMain.data;

            for (int i = _trashTrails.Count - 1; i >= 0; i--)
            {
                int index = _trashTrails[i].trashIndex;
                GameObject go = _trashTrails[i].trailGo;
                float landedAt = _trashTrails[i].landedAt;

                if (go == null || index < 0 || index >= objPool.Length)
                {
                    DestroyTrailGameObject(go);
                    _trashTrails.RemoveAt(i);
                    continue;
                }

                if (objPool[index].item == 0)
                {
                    DestroyTrailGameObject(go);
                    _trashTrails.RemoveAt(i);
                    continue;
                }

                // 落地后停止发射粒子，延迟一定时间后销毁尾焰以释放资源（未收集的物资不会永久占用）
                if (dataPool[index].landPlanetId != 0)
                {
                    if (landedAt < 0f)
                    {
                        landedAt = Time.time;
                        _trashTrails[i] = (index, go, landedAt, _trashTrails[i].targetLandPosLocal, _trashTrails[i].spawnGameTime, _trashTrails[i].spawnPos);
                        var ps = go.GetComponent<ParticleSystem>();
                        if (ps != null)
                        {
                            var emission = ps.emission;
                            if (emission.enabled) emission.enabled = false;
                        }
                    }
                    if (Time.time - landedAt >= TrailDestroyDelayAfterLand)
                    {
                        DestroyTrailGameObject(go);
                        _trashTrails.RemoveAt(i);
                        continue;
                    }
                }
                else
                {
                    VectorLF3 uPos = dataPool[index].uPos;
                    Vector3 relPos = (Vector3)Maths.QInvRotateLF(gd.relativeRot, uPos - gd.relativePos);
                    go.transform.position = relPos;
                }
            }
        }

        /// <summary>销毁尾迹 GameObject 并释放其材质（CreateTrailParticle 中 new 的 Material 需显式 Destroy，否则会泄漏）。</summary>
        private void DestroyTrailGameObject(GameObject? go)
        {
            if (go == null) return;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            if (renderer != null && renderer.material != null)
                UnityEngine.Object.Destroy(renderer.material);
            UnityEngine.Object.Destroy(go);
        }

        /// <summary>创建一个拖尾火焰粒子：World 空间、小粒子、沿路径遗留形成拖尾。</summary>
        private GameObject CreateTrailParticle()
        {
            var go = new GameObject("TrashTrail");
            if (VFEffect.effectGroup != null)
                go.transform.SetParent(VFEffect.effectGroup, false);
            go.transform.localPosition = Vector3.zero;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = 0.55f;
            main.startSpeed = 0f;  // 粒子留在发射点，物体移动形成拖尾
            main.startSize = 0.28f;  // 更小的粒子
            main.simulationSpace = ParticleSystemSimulationSpace.World;  // 粒子留在发射点，物体移动即形成拖尾
            main.maxParticles = 1500;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 400f;  // 更密的拖尾

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            shape.radius = 0.15f;


            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 0.7f, 0.2f), 0f), new GradientColorKey(new Color(1f, 0.4f, 0f), 0.4f), new GradientColorKey(new Color(0.3f, 0.1f, 0f), 1f) },
                new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0.3f, 0.6f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.5f, 0.6f), new Keyframe(1f, 0f)));

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
                renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));

            ps.Play();
            return go;
        }

        /// <summary>是否应为该掉落生成尾迹粒子。仅当前星系且击毁位置在“当前星球附近”时生成，避免遥远处大量粒子。</summary>
        private static bool ShouldSpawnTrailForDrop(PlanetData planet, VectorLF3 deathPos)
        {
            if (GameMain.localStar == null || planet.star != GameMain.localStar)
                return false;
            PlanetData? local = GameMain.localPlanet;
            if (local == null)
                return true;
            if (planet.id == local.id)
                return true;
            double dx = deathPos.x - local.uPosition.x, dy = deathPos.y - local.uPosition.y, dz = deathPos.z - local.uPosition.z;
            double distSq = dx * dx + dy * dy + dz * dz;
            return distSq <= TrailMaxDistFromLocalPlanet * TrailMaxDistFromLocalPlanet;
        }

        /// <summary>统计当前存活垃圾中目标为该星球的数量（nearPlanetId 或 landPlanetId 等于该星），用于每星上限。</summary>
        internal static int CountTrashForPlanet(int planetId)
        {
            var container = GameMain.data?.trashSystem?.container;
            if (container?.trashDataPool == null || container.trashObjPool == null)
                return 0;
            int n = 0;
            int cursor = container.trashCursor;
            var dataPool = container.trashDataPool;
            var objPool = container.trashObjPool;
            for (int i = 0; i < cursor; i++)
            {
                if (objPool[i].item == 0)
                    continue;
                int p = dataPool[i].nearPlanetId != 0 ? dataPool[i].nearPlanetId : dataPool[i].landPlanetId;
                if (p == planetId)
                    n++;
            }
            return n;
        }

        /// <summary>从飞船击毁位置生成 1 个 Trash，初速度指向目标星球。不依赖 factoryLoaded：有基站且工厂已加载则落基站附近，否则落行星表面（与地面黑雾掉落一致，未加载星球也可坠落并被范围内基站拾取）。</summary>
        internal static void SpawnTrashAtDeathPosition(VectorLF3 deathPos, int itemId, int count)
        {
            if (GameMain.data?.trashSystem?.container == null || itemId <= 0 || count <= 0)
                return;
            if (GameMain.data.trashSystem.enemyDropBans != null && GameMain.data.trashSystem.enemyDropBans.Contains(itemId))
                return;

            PlanetData? planet = GetPlanetWithBattleBase(deathPos);
            if (planet == null)
                return;
            if (_currentPlanetSpawnQuota == 0)
                return;

            TrashSystem trashSystem = GameMain.data.trashSystem;
            Vector3 landPosLocal;
            VectorLF3 landWorld;
            PlanetFactory? factory = GetFactoryForPlanet(planet);
            DefenseSystem? defense = factory?.defenseSystem;
            if (defense?.battleBases != null && defense.battleBases.count > 0 && factory != null)
            {
                var list = new List<BattleBaseComponent>();
                int cursor = defense.battleBases.cursor;
                var buffer = defense.battleBases.buffer;
                for (int i = 1; i < cursor; i++)
                {
                    if (buffer[i] != null && buffer[i].id == i && buffer[i].entityId > 0)
                        list.Add(buffer[i]);
                }
                if (list.Count == 0)
                {
                    if (!TryGetFallbackLandPosLocal(planet, deathPos, out landPosLocal))
                        return;
                    VectorLF3 landPosLocalLF = new VectorLF3(landPosLocal.x, landPosLocal.y, landPosLocal.z);
                    landWorld = Maths.QRotateLF(planet.runtimeRotation, landPosLocalLF) + planet.uPosition;
                }
                else
                {
                    BattleBaseComponent chosen = list[UnityEngine.Random.Range(0, list.Count)];
                    ref EntityData entity = ref factory.entityPool[chosen.entityId];
                    if (entity.id != chosen.entityId)
                        return;
                    Vector3 basePosLocal = entity.pos;
                    float baseMag = basePosLocal.magnitude;
                    if (baseMag < 1f)
                        return;
                    Vector3 baseDir = basePosLocal / baseMag;
                    Vector3 tangent = Vector3.Cross(baseDir, UnityEngine.Random.insideUnitSphere).normalized;
                    if (tangent.sqrMagnitude < 0.01f)
                        tangent = Vector3.Cross(baseDir, Vector3.up).normalized;
                    float offset = UnityEngine.Random.Range(LandOffsetMin, LandOffsetMax);
                    landPosLocal = (basePosLocal + tangent * offset).normalized * baseMag;
                    VectorLF3 landPosLocalLF = new VectorLF3(landPosLocal.x, landPosLocal.y, landPosLocal.z);
                    landWorld = Maths.QRotateLF(planet.runtimeRotation, landPosLocalLF) + planet.uPosition;
                }
            }
            else
            {
                if (!TryGetFallbackLandPosLocal(planet, deathPos, out landPosLocal))
                    return;
                VectorLF3 landPosLocalLF = new VectorLF3(landPosLocal.x, landPosLocal.y, landPosLocal.z);
                landWorld = Maths.QRotateLF(planet.runtimeRotation, landPosLocalLF) + planet.uPosition;
            }

            VectorLF3 deathPosLocal = Maths.QInvRotateLF(planet.runtimeRotation, deathPos - planet.uPosition);
            VectorLF3 planetVel = planet.GetUniversalVelocityAtLocalPoint(GameMain.gameTime, deathPosLocal);

            VectorLF3 toLand = landWorld - deathPos;
            double dist = Math.Sqrt(toLand.x * toLand.x + toLand.y * toLand.y + toLand.z * toLand.z);
            if (dist < 1.0)
                return;
            toLand.x /= dist;
            toLand.y /= dist;
            toLand.z /= dist;
            double speed = dist / FallDurationSeconds;
            if (speed < MinFallSpeed)
                speed = MinFallSpeed;
            VectorLF3 uVel = new VectorLF3(
                planetVel.x + toLand.x * speed,
                planetVel.y + toLand.y * speed,
                planetVel.z + toLand.z * speed);

            int stackSize = LDB.items.Select(itemId)?.StackSize ?? 100;
            int chunk = count > stackSize ? stackSize : count;
            // 与地面掉落一致：按当前垃圾数量缩短 life，游戏会在 GameTick 中每 tick 减 1，life 归零时 RemoveTrash
            int trashCount = trashSystem.trashCount;
            float lifeFactor = (float)(500.0 / (trashCount + 100));
            int life = SpaceDropBaseLifeTicks / 3 + (int)((double)SpaceDropBaseLifeTicks * (double)lifeFactor * 2.0 / 3.0 + 0.5);
            var trashData = new TrashData
            {
                warningId = 0,
                landPlanetId = 0,
                nearPlanetId = planet.id,
                nearStarId = planet.star.astroId,
                nearStarGravity = trashSystem.GetStarGravity(planet.star.id),
                life = life,
                lPos = Vector3.zero,
                lRot = Quaternion.identity,
                uPos = deathPos,
                uRot = Quaternion.LookRotation(UnityEngine.Random.insideUnitSphere.normalized),
                uVel = uVel,
                uAgl = UnityEngine.Random.insideUnitSphere * 0.03f
            };

            GameData gd = GameMain.data;
            Vector3 rPos = (Vector3)Maths.QInvRotateLF(gd.relativeRot, deathPos - gd.relativePos);
            Quaternion rRot = Quaternion.Inverse(gd.relativeRot) * trashData.uRot;
            var trashObj = new TrashObject(itemId, chunk, 0, rPos, rRot);

            int trashIndex = trashSystem.container.NewTrash(trashObj, trashData);
            if (_currentPlanetSpawnQuota > 0)
                _currentPlanetSpawnQuota--;
            try
            {
                if (Instance != null && ShouldSpawnTrailForDrop(planet, deathPos))
                {
                    GameObject trailGo = Instance.CreateTrailParticle();
                    Instance._trashTrails.Add((trashIndex, trailGo, -1f, landPosLocal, GameMain.gameTime, deathPos));
                }
            }
            catch { }
        }

        /// <summary>用于日志：返回行星显示名称（恒星名 + 行星序号/名）及 pos 到该行星的距离（米）。</summary>
        internal static (string planetName, double distanceM) GetPlanetNameAndDistance(PlanetData planet, VectorLF3 pos)
        {
            double dx = planet.uPosition.x - pos.x, dy = planet.uPosition.y - pos.y, dz = planet.uPosition.z - pos.z;
            double dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            string starName = planet.star?.name ?? "";
            string planetName = planet.displayName ?? $"{starName} 行星{planet.index + 1}";
            if (string.IsNullOrEmpty(planetName) && !string.IsNullOrEmpty(starName))
                planetName = $"{starName} 行星{planet.index + 1}";
            return (planetName, dist);
        }

        /// <summary>用于日志：根据 protoId 返回敌人显示名称。</summary>
        internal static string GetEnemyName(int protoId)
        {
            var proto = LDB.enemies?.Select(protoId);
            if (proto == null) return $"id{protoId}";
            if (proto.name != null && proto.name.Length > 0) return proto.name;
            if (proto.Name != null) return proto.Name.Translate();
            return $"id{protoId}";
        }

        /// <summary>不带任何条件：全星图中离 pos 最近的行星（仅按距离）。与游戏 SpaceSector.GetNearestPlanet 一致：用 star.planetCount 限定循环，避免遍历 planets 数组中的空槽。</summary>
        internal static PlanetData? GetNearestPlanetUnconditional(VectorLF3 pos)
        {
            var sector = GameMain.data?.spaceSector;
            if (sector == null) return FallbackGetNearestPlanetUnconditional(pos);
            VectorLF3 upos = pos;
            var planet = sector.GetNearestPlanet(ref upos, out _);
            return planet;
        }

        /// <summary>无 spaceSector 时的回退：按 galaxy.stars + planetCount 遍历，与游戏逻辑一致。</summary>
        private static PlanetData? FallbackGetNearestPlanetUnconditional(VectorLF3 pos)
        {
            var galaxy = GameMain.data?.galaxy;
            if (galaxy?.stars == null) return null;
            PlanetData? best = null;
            double bestDistSq = double.MaxValue;
            for (int i = 0; i < galaxy.starCount; i++)
            {
                var star = galaxy.stars[i];
                if (star?.planets == null) continue;
                int planetCount = star.planetCount;
                for (int j = 0; j < planetCount; j++)
                {
                    var p = star.planets[j];
                    if (p == null) continue;
                    double dx = p.uPosition.x - pos.x, dy = p.uPosition.y - pos.y, dz = p.uPosition.z - pos.z;
                    double distSq = dx * dx + dy * dy + dz * dz;
                    if (distSq < bestDistSq)
                    {
                        bestDistSq = distSq;
                        best = p;
                    }
                }
            }
            return best;
        }

        /// <summary>选落点行星：本星系内优先选离 searchPos 最近且带基站的星（GetFactoryForPlanet 不依赖 factoryLoaded）；若无则选最近任意行星（表面落点）。</summary>
        internal static PlanetData? GetPlanetWithBattleBase(VectorLF3? nearPosition = null)
        {
            StarData? star;
            if (nearPosition.HasValue && GameMain.data?.spaceSector != null)
            {
                VectorLF3 pos = nearPosition.Value;
                star = GameMain.data.spaceSector.GetNearestStar(ref pos, out _);
                if (star?.planets == null)
                    return null;
            }
            else
            {
                star = GameMain.localStar;
            }
            if (star?.planets == null)
                return null;

            VectorLF3 searchPos = nearPosition ?? GameMain.localPlanet?.uPosition ?? default;
            PlanetData? bestWithBase = null;
            PlanetData? nearestAny = null;
            double bestWithBaseDistSq = double.MaxValue;
            double nearestAnyDistSq = double.MaxValue;

            var local = GameMain.localPlanet;
            if (local != null && local.star == star)
            {
                double dx = local.uPosition.x - searchPos.x, dy = local.uPosition.y - searchPos.y, dz = local.uPosition.z - searchPos.z;
                double distSq = dx * dx + dy * dy + dz * dz;
                if (distSq < nearestAnyDistSq) { nearestAnyDistSq = distSq; nearestAny = local; }
                var localFactory = GetFactoryForPlanet(local);
                if (localFactory?.defenseSystem?.battleBases != null && localFactory.defenseSystem.battleBases.count > 0 && distSq < bestWithBaseDistSq)
                { bestWithBaseDistSq = distSq; bestWithBase = local; }
            }

            int planetCount = star.planetCount;
            for (int j = 0; j < planetCount; j++)
            {
                var p = star.planets[j];
                if (p == null) continue;
                double dx = p.uPosition.x - searchPos.x, dy = p.uPosition.y - searchPos.y, dz = p.uPosition.z - searchPos.z;
                double distSq = dx * dx + dy * dy + dz * dz;
                if (distSq < nearestAnyDistSq) { nearestAnyDistSq = distSq; nearestAny = p; }
                var pFactory = GetFactoryForPlanet(p);
                if (pFactory?.defenseSystem?.battleBases != null && pFactory.defenseSystem.battleBases.count > 0 && distSq < bestWithBaseDistSq)
                { bestWithBaseDistSq = distSq; bestWithBase = p; }
            }

            // 有击毁位置时也优先选带基站的最近星；仅当本星系内无任何基站时才退化为最近任意行星（表面落点）
            return bestWithBase ?? nearestAny;
        }

        /// <summary>获取行星对应的工厂引用。优先 planet.factory；否则用 GameData.galaxy.astrosFactory[planet.astroId]（与 SkillSystem.astroFactories 同一引用，未加载时游戏也用它处理战斗/掉落）。</summary>
        internal static PlanetFactory? GetFactoryForPlanet(PlanetData planet)
        {
            if (planet == null) return null;
            if (planet.factory != null) return planet.factory;
            // SkillSystem.astroFactories = gameData.galaxy.astrosFactory（见 SkillSystem 构造函数）
            PlanetFactory[]? astrosFactory = GameMain.data?.galaxy?.astrosFactory;
            if (astrosFactory == null) return null;
            int astroId = planet.astroId;
            if (astroId < 0 || astroId >= astrosFactory.Length) return null;
            var factory = astrosFactory[astroId];
            return factory != null && factory.planetId == planet.astroId ? factory : null;
        }

        /// <summary>当无法取得基站位置时，用行星表面朝向击毁方向的一点作为落点（用于轨迹与重力）。</summary>
        internal static bool TryGetFallbackLandPosLocal(PlanetData planet, VectorLF3 deathPos, out Vector3 landPosLocal)
        {
            landPosLocal = default;
            double rx = deathPos.x - planet.uPosition.x, ry = deathPos.y - planet.uPosition.y, rz = deathPos.z - planet.uPosition.z;
            double len = Math.Sqrt(rx * rx + ry * ry + rz * rz);
            if (len < 1e-6) return false;
            double radius = planet.realRadius > 1e-6 ? planet.realRadius : 200.0;
            VectorLF3 surfaceWorld = new VectorLF3(
                planet.uPosition.x + rx / len * radius,
                planet.uPosition.y + ry / len * radius,
                planet.uPosition.z + rz / len * radius);
            VectorLF3 localLF = Maths.QInvRotateLF(planet.runtimeRotation, surfaceWorld - planet.uPosition);
            landPosLocal = new Vector3((float)localLF.x, (float)localLF.y, (float)localLF.z);
            return true;
        }

        /// <summary>太空舰船掉落：复用地面掉落表逻辑，12 次抽选，数量×100。</summary>
        internal static void RandomDropItemForSpace(int enemyLevel, out int itemId, out int count, out int life)
        {
            _spaceDropSeed = (uint)((ulong)(_spaceDropSeed % 2147483646U + 1U) * 48271UL % 2147483647UL) - 1U;
            double num = _spaceDropSeed / 2147483646.0 * 10000.0;
            itemId = ItemProto.enemyDropRangeTable[(int)num];
            count = 0;
            life = 0;
            int num2 = enemyLevel / 3;
            if (num2 > 8) num2 = 8;
            double num3 = 1.0;
            int enemyDropMask = 2147483647;
            if (itemId > 0)
            {
                if (num2 < ItemProto.enemyDropLevelTable[itemId])
                    itemId = 0;
                if ((ItemProto.enemyDropMaskTable[itemId] & enemyDropMask) != enemyDropMask)
                {
                    num3 = (double)ItemProto.enemyDropMaskRatioTable[itemId];
                    if (num3 < 1E-05) itemId = 0;
                }
            }
            double num4 = 0.0;
            if (itemId > 0)
            {
                if (GameMain.data.history.ItemUnlocked(itemId))
                    num4 = 1.0;
                else if (GameMain.data.history.ItemCanDropByEnemy(itemId))
                    num4 = 0.4;
                else
                    itemId = 0;
            }
            num4 *= (double)GameMain.data.history.enemyDropScale;
            float enemyDropMultiplier = GameMain.data.gameDesc.enemyDropMultiplier;
            double num5 = (itemId >= 5200 && itemId <= 5209) ? 1.0 : (double)enemyDropMultiplier;
            if (itemId > 0)
            {
                life = 1800;
                double num7 = (double)ItemProto.enemyDropCountTable[itemId] * num4 * num3 * num5 * 0.8;
                double num8 = num7 * ((double)enemyLevel / 6.0 + 4.0);
                double num9 = num7;
                _spaceDropSeed = (uint)((ulong)(_spaceDropSeed % 2147483646U + 1U) * 48271UL % 2147483647UL) - 1U;
                double num10 = _spaceDropSeed / 2147483646.0 * (num8 - num9) + num9;
                _spaceDropSeed = (uint)((ulong)(_spaceDropSeed % 2147483646U + 1U) * 48271UL % 2147483647UL) - 1U;
                double num11 = _spaceDropSeed / 2147483646.0;
                count = (int)(num10 + num11);
                count *= SpaceDropCountMultiplier;
                if (count > 0)
                    GameMain.data.history.enemyDropItemUnlocked.Add(itemId);
            }
        }
    }

    [HarmonyPatch(typeof(SpaceSector), nameof(SpaceSector.RemoveCraftWithComponents))]
    public static class SpaceCraftDeathPatch
    {
        static void Prefix(SpaceSector __instance, int id)
        {
            try
            {
                if (id <= 0 || __instance.craftPool == null || id >= __instance.craftPool.Length)
                    return;
                ref CraftData craft = ref __instance.craftPool[id];
                if (craft.id != id)
                    return;

                int rootOwner = craft.owner;
                while (rootOwner > 0 && rootOwner < __instance.craftPool.Length)
                {
                    ref CraftData parent = ref __instance.craftPool[rootOwner];
                    if (parent.id != rootOwner) break;
                    rootOwner = parent.owner;
                }
                if (rootOwner == -1)
                    return;

                __instance.TransformFromAstro(craft.astroId, out VectorLF3 deathPos, craft.pos);

                var gd = GameMain.data;
                if (gd?.trashSystem == null || ItemProto.enemyDropRangeTable == null)
                    return;

                var planet = Plugin.GetPlanetWithBattleBase(deathPos);
                Plugin._currentPlanetSpawnQuota = planet != null ? Math.Max(0, Plugin.MaxTrashPerPlanet - Plugin.CountTrashForPlanet(planet.id)) : -1;

                if (planet != null && Plugin._currentPlanetSpawnQuota == 0)
                {
                    string pName = Plugin.GetPlanetNameAndDistance(planet, deathPos).planetName;
                    Plugin.Log?.LogInfo($"[BattlefieldAnalysisBaseCollectSpaceJunk] 落点行星（{pName}）已达垃圾堆数上限（{Plugin.MaxTrashPerPlanet}），本击毁不生成战利品。");
                }

                int totalSpawned = 0;
                for (int i = 0; i < Plugin.SpaceDropRollCount; i++)
                {
                    if (Plugin._currentPlanetSpawnQuota == 0)
                        break;
                    Plugin.RandomDropItemForSpace(Plugin.SpaceCraftEnemyLevel, out int itemId, out int count, out int life);
                    if (itemId <= 0 || count <= 0)
                        continue;
                    while (count > 0)
                    {
                        if (Plugin._currentPlanetSpawnQuota == 0)
                            break;
                        Plugin.SpawnTrashAtDeathPosition(deathPos, itemId, count);
                        totalSpawned++;
                        count -= LDB.items.Select(itemId)?.StackSize ?? 100;
                        if (count < 0) count = 0;
                    }
                }
                if (totalSpawned > 0 && planet != null && Plugin._currentPlanetSpawnQuota == 0)
                {
                    string pName = Plugin.GetPlanetNameAndDistance(planet, deathPos).planetName;
                    Plugin.Log?.LogInfo($"[BattlefieldAnalysisBaseCollectSpaceJunk] 落点行星（{pName}）已达垃圾堆数上限，本击毁仅生成 {totalSpawned} 批战利品。");
                }
                if (totalSpawned > 0)
                {
                    var nearest = Plugin.GetNearestPlanetUnconditional(deathPos);
                    var landing = Plugin.GetPlanetWithBattleBase(deathPos);
                    string nearestLine = nearest != null ? Plugin.GetPlanetNameAndDistance(nearest, deathPos) is var (nName, nDist) ? $"离击毁位置最近的行星（无条件）: {nName}，{nDist:F0} m" : "" : "";
                    string landingLine = "";
                    if (landing != null && Plugin.GetPlanetNameAndDistance(landing, deathPos) is var (lName, lDist))
                    {
                        var lf = Plugin.GetFactoryForPlanet(landing);
                        bool hasBase = lf?.defenseSystem?.battleBases != null && lf.defenseSystem.battleBases.count > 0;
                        landingLine = hasBase ? $"落点行星（带基站）: {lName}，{lDist:F0} m" : $"落点行星（无基站，表面落点）: {lName}，{lDist:F0} m";
                    }
                    Plugin.Log?.LogInfo($"[BattlefieldAnalysisBaseCollectSpaceJunk] 太空舰船击毁，已生成 {totalSpawned} 批战利品。{nearestLine}{(string.IsNullOrEmpty(nearestLine) ? "" : "；")}{landingLine}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[BattlefieldAnalysisBaseCollectSpaceJunk] SpaceCraftDeathPatch 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    /// <summary>仅太空黑雾：若方法由基类定义，PlanetFactory 与 SpaceSector 会走同一 MethodInfo，需用运行时类型排除地面。</summary>
    [HarmonyPatch(typeof(SpaceSector), nameof(SpaceSector.RemoveEnemyWithComponents))]
    public static class SpaceEnemyDeathPatch
    {
        static void Prefix(object __instance, int id)
        {
            try
            {
                if (__instance is PlanetFactory)
                    return;
                if (__instance is not SpaceSector sector)
                    return;
                if (id <= 0 || sector.enemyPool == null || id >= sector.enemyPool.Length)
                    return;
                ref EnemyData enemy = ref sector.enemyPool[id];
                if (enemy.id != id)
                    return;
                if (!enemy.isSpace)
                    return;

                sector.TransformFromAstro(enemy.astroId, out VectorLF3 deathPos, enemy.pos);

                var gd = GameMain.data;
                if (gd?.trashSystem == null || ItemProto.enemyDropRangeTable == null)
                    return;

                var planet = Plugin.GetPlanetWithBattleBase(deathPos);
                Plugin._currentPlanetSpawnQuota = planet != null ? Math.Max(0, Plugin.MaxTrashPerPlanet - Plugin.CountTrashForPlanet(planet.id)) : -1;

                if (planet != null && Plugin._currentPlanetSpawnQuota == 0)
                {
                    string pName = Plugin.GetPlanetNameAndDistance(planet, deathPos).planetName;
                    Plugin.Log?.LogInfo($"[BattlefieldAnalysisBaseCollectSpaceJunk] 落点行星（{pName}）已达垃圾堆数上限（{Plugin.MaxTrashPerPlanet}），本击毁不生成战利品。");
                }

                int totalSpawned = 0;
                for (int i = 0; i < Plugin.SpaceDropRollCount; i++)
                {
                    if (Plugin._currentPlanetSpawnQuota == 0)
                        break;
                    Plugin.RandomDropItemForSpace(Plugin.SpaceCraftEnemyLevel, out int itemId, out int count, out int life);
                    if (itemId <= 0 || count <= 0)
                        continue;
                    while (count > 0)
                    {
                        if (Plugin._currentPlanetSpawnQuota == 0)
                            break;
                        Plugin.SpawnTrashAtDeathPosition(deathPos, itemId, count);
                        totalSpawned++;
                        count -= LDB.items.Select(itemId)?.StackSize ?? 100;
                        if (count < 0) count = 0;
                    }
                }
                if (totalSpawned > 0 && planet != null && Plugin._currentPlanetSpawnQuota == 0)
                {
                    string pName = Plugin.GetPlanetNameAndDistance(planet, deathPos).planetName;
                    Plugin.Log?.LogInfo($"[BattlefieldAnalysisBaseCollectSpaceJunk] 落点行星（{pName}）已达垃圾堆数上限，本击毁仅生成 {totalSpawned} 批战利品。");
                }
                if (totalSpawned > 0)
                {
                    string enemyName = Plugin.GetEnemyName(enemy.protoId);
                    var nearest = Plugin.GetNearestPlanetUnconditional(deathPos);
                    var landing = Plugin.GetPlanetWithBattleBase(deathPos);
                    string nearestLine = nearest != null ? Plugin.GetPlanetNameAndDistance(nearest, deathPos) is var (nName, nDist) ? $"离击毁位置最近的行星（无条件）: {nName}，{nDist:F0} m" : "" : "";
                    string landingLine = "";
                    if (landing != null && Plugin.GetPlanetNameAndDistance(landing, deathPos) is var (lName, lDist))
                    {
                        var lf = Plugin.GetFactoryForPlanet(landing);
                        bool hasBase = lf?.defenseSystem?.battleBases != null && lf.defenseSystem.battleBases.count > 0;
                        landingLine = hasBase ? $"落点行星（带基站）: {lName}，{lDist:F0} m" : $"落点行星（无基站，表面落点）: {lName}，{lDist:F0} m";
                    }
                    Plugin.Log?.LogInfo($"[BattlefieldAnalysisBaseCollectSpaceJunk] 黑雾太空敌舰击毁，已生成 {totalSpawned} 批战利品。被击毁: {enemyName}。{nearestLine}{(string.IsNullOrEmpty(nearestLine) ? "" : "；")}{landingLine}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[BattlefieldAnalysisBaseCollectSpaceJunk] SpaceEnemyDeathPatch 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    /// <summary>每帧沿预计算平滑轨道设置坠落物位置与速度，10 秒从起点到终点，无横向抖动</summary>
    [HarmonyPatch(typeof(TrashSystem), nameof(TrashSystem.GameTick))]
    public static class TrashSystemGravityPatch
    {
        static void Postfix(TrashSystem __instance)
        {
            try
            {
                if (Plugin.Instance == null || __instance.gameData?.galaxy?.astrosData == null)
                    return;

                var astrosData = __instance.gameData.galaxy.astrosData;
                var container = __instance.container;
                var dataPool = container.trashDataPool;
                var trails = Plugin.Instance._trashTrails;
                double gameTimeSec = GameMain.gameTime;

                for (int tr = 0; tr < trails.Count; tr++)
                {
                    int index = trails[tr].trashIndex;
                    if (index < 0 || index >= dataPool.Length)
                        continue;
                    if (dataPool[index].landPlanetId != 0)
                        continue;

                    PlanetData? planet = Plugin.GetPlanetWithBattleBase(trails[tr].spawnPos);
                    if (planet == null)
                        continue;
                    int astroId = planet.astroId;
                    if (astroId <= 0 || astroId >= astrosData.Length)
                        continue;
                    double planetRadius = astrosData[astroId].uRadius;
                    VectorLF3 planetPos = astrosData[astroId].uPos;

                    double spawnTime = trails[tr].spawnGameTime;
                    double elapsed = gameTimeSec - spawnTime;
                    double t = elapsed / Plugin.FallDurationSeconds;
                    if (t >= 1.0)
                        continue;
                    if (t < 0)
                        continue;

                    VectorLF3 P0 = trails[tr].spawnPos;
                    Vector3 targetLocal = trails[tr].targetLandPosLocal;
                    VectorLF3 landPosLF = new VectorLF3(targetLocal.x, targetLocal.y, targetLocal.z);
                    VectorLF3 P3 = Maths.QRotateLF(planet.runtimeRotation, landPosLF) + planet.uPosition;

                    double p0x = P0.x - planetPos.x, p0y = P0.y - planetPos.y, p0z = P0.z - planetPos.z;
                    double p3x = P3.x - planetPos.x, p3y = P3.y - planetPos.y, p3z = P3.z - planetPos.z;
                    double r0 = Math.Sqrt(p0x * p0x + p0y * p0y + p0z * p0z);
                    double r3 = Math.Sqrt(p3x * p3x + p3y * p3y + p3z * p3z);
                    if (r0 < 1e-6 || r3 < 1e-6)
                        continue;
                    double u0x = p0x / r0, u0y = p0y / r0, u0z = p0z / r0;
                    double u1x = p3x / r3, u1y = p3y / r3, u1z = p3z / r3;
                    double dotU = u0x * u1x + u0y * u1y + u0z * u1z;

                    double px, py, pz, vx, vy, vz;
                    bool useArcPath = dotU < -0.2;

                    if (useArcPath)
                    {
                        double crossX = u0y * u1z - u0z * u1y;
                        double crossY = u0z * u1x - u0x * u1z;
                        double crossZ = u0x * u1y - u0y * u1x;
                        double crossLen = Math.Sqrt(crossX * crossX + crossY * crossY + crossZ * crossZ);
                        if (crossLen < 1e-6)
                        {
                            double refX = 0, refY = 0, refZ = 0;
                            if (Math.Abs(u0x) <= Math.Abs(u0y) && Math.Abs(u0x) <= Math.Abs(u0z))
                                refX = 1;
                            else if (Math.Abs(u0y) <= Math.Abs(u0z))
                                refY = 1;
                            else
                                refZ = 1;
                            crossX = u0y * refZ - u0z * refY;
                            crossY = u0z * refX - u0x * refZ;
                            crossZ = u0x * refY - u0y * refX;
                            crossLen = Math.Sqrt(crossX * crossX + crossY * crossY + crossZ * crossZ);
                            if (crossLen < 1e-6)
                                continue;
                        }
                        double umx = crossX / crossLen, umy = crossY / crossLen, umz = crossZ / crossLen;

                        double alpha = Plugin.LandingSpeed / (r3 > 1e-6 ? r3 * 0.5 : 50.0);
                        alpha = alpha < 0.1 ? 0.1 : (alpha > 1.0 ? 1.0 : alpha);
                        double sParam = Math.Pow(t, alpha);

                        double ux, uy, uz;
                        double rCur;
                        if (sParam < 0.5)
                        {
                            double s2 = sParam * 2.0;
                            double dot0m = u0x * umx + u0y * umy + u0z * umz;
                            double th = Math.Acos(Math.Max(-1, Math.Min(1, dot0m)));
                            double sinTh = th > 1e-8 ? Math.Sin(th) : 1e-8;
                            double w0 = Math.Sin((1 - s2) * th) / sinTh;
                            double w1 = Math.Sin(s2 * th) / sinTh;
                            ux = w0 * u0x + w1 * umx;
                            uy = w0 * u0y + w1 * umy;
                            uz = w0 * u0z + w1 * umz;
                        }
                        else
                        {
                            double s2 = (sParam - 0.5) * 2.0;
                            double dotm1 = umx * u1x + umy * u1y + umz * u1z;
                            double th = Math.Acos(Math.Max(-1, Math.Min(1, dotm1)));
                            double sinTh = th > 1e-8 ? Math.Sin(th) : 1e-8;
                            double w0 = Math.Sin((1 - s2) * th) / sinTh;
                            double w1 = Math.Sin(s2 * th) / sinTh;
                            ux = w0 * umx + w1 * u1x;
                            uy = w0 * umy + w1 * u1y;
                            uz = w0 * umz + w1 * u1z;
                        }
                        double ulen = Math.Sqrt(ux * ux + uy * uy + uz * uz);
                        if (ulen > 1e-6) { ux /= ulen; uy /= ulen; uz /= ulen; }

                        double minR = planetRadius + Plugin.CurveClearanceMargin;
                        if (sParam < 0.8)
                            rCur = Math.Max(r0, minR);
                        else
                        {
                            double f = (sParam - 0.8) / 0.2;
                            f = f * f * (3 - 2 * f);
                            rCur = r0 + (r3 - r0) * f;
                        }
                        px = planetPos.x + ux * rCur;
                        py = planetPos.y + uy * rCur;
                        pz = planetPos.z + uz * rCur;

                        double dt = 0.01;
                        double t1 = Math.Min(1.0, t + dt / Plugin.FallDurationSeconds);
                        double s1 = Math.Pow(t1, alpha);
                        double ux1, uy1, uz1;
                        double r1;
                        if (s1 < 0.5)
                        {
                            double s2 = s1 * 2.0;
                            double dot0m = u0x * umx + u0y * umy + u0z * umz;
                            double th = Math.Acos(Math.Max(-1, Math.Min(1, dot0m)));
                            double sinTh = th > 1e-8 ? Math.Sin(th) : 1e-8;
                            ux1 = (Math.Sin((1 - s2) * th) * u0x + Math.Sin(s2 * th) * umx) / sinTh;
                            uy1 = (Math.Sin((1 - s2) * th) * u0y + Math.Sin(s2 * th) * umy) / sinTh;
                            uz1 = (Math.Sin((1 - s2) * th) * u0z + Math.Sin(s2 * th) * umz) / sinTh;
                        }
                        else
                        {
                            double s2 = (s1 - 0.5) * 2.0;
                            double dotm1 = umx * u1x + umy * u1y + umz * u1z;
                            double th = Math.Acos(Math.Max(-1, Math.Min(1, dotm1)));
                            double sinTh = th > 1e-8 ? Math.Sin(th) : 1e-8;
                            ux1 = (Math.Sin((1 - s2) * th) * umx + Math.Sin(s2 * th) * u1x) / sinTh;
                            uy1 = (Math.Sin((1 - s2) * th) * umy + Math.Sin(s2 * th) * u1y) / sinTh;
                            uz1 = (Math.Sin((1 - s2) * th) * umz + Math.Sin(s2 * th) * u1z) / sinTh;
                        }
                        double ul1 = Math.Sqrt(ux1 * ux1 + uy1 * uy1 + uz1 * uz1);
                        if (ul1 > 1e-6) { ux1 /= ul1; uy1 /= ul1; uz1 /= ul1; }
                        if (s1 < 0.8) r1 = Math.Max(r0, minR); else { double f = (s1 - 0.8) / 0.2; f = f * f * (3 - 2 * f); r1 = r0 + (r3 - r0) * f; }
                        vx = (planetPos.x + ux1 * r1 - px) / dt;
                        vy = (planetPos.y + uy1 * r1 - py) / dt;
                        vz = (planetPos.z + uz1 * r1 - pz) / dt;
                    }
                    else
                    {
                        double d0x = P3.x - P0.x, d0y = P3.y - P0.y, d0z = P3.z - P0.z;
                        double cx = planetPos.x - P0.x, cy = planetPos.y - P0.y, cz = planetPos.z - P0.z;
                        double d2 = d0x * d0x + d0y * d0y + d0z * d0z;
                        if (d2 < 1e-10)
                            continue;
                        double sClosest = (d0x * cx + d0y * cy + d0z * cz) / d2;
                        sClosest = sClosest < 0 ? 0 : (sClosest > 1 ? 1 : sClosest);
                        double qx = P0.x + sClosest * d0x - planetPos.x;
                        double qy = P0.y + sClosest * d0y - planetPos.y;
                        double qz = P0.z + sClosest * d0z - planetPos.z;
                        double distToLine = Math.Sqrt(qx * qx + qy * qy + qz * qz);
                        double minDist = planetRadius + Plugin.CurveClearanceMargin;
                        double bulge = 0, bx = 0, by = 0, bz = 0;
                        if (distToLine < minDist)
                        {
                            bulge = Math.Max(0, minDist - distToLine);
                            double p0cx = P0.x - planetPos.x, p0cy = P0.y - planetPos.y, p0cz = P0.z - planetPos.z;
                            double p3cx = P3.x - planetPos.x, p3cy = P3.y - planetPos.y, p3cz = P3.z - planetPos.z;
                            bx = p0cx + p3cx; by = p0cy + p3cy; bz = p0cz + p3cz;
                            double bl = Math.Sqrt(bx * bx + by * by + bz * bz);
                            if (bl > 1e-6) { bx /= bl; by /= bl; bz /= bl; }
                            else { double p0l = Math.Sqrt(p0cx * p0cx + p0cy * p0cy + p0cz * p0cz); if (p0l > 1e-6) { bx = p0cx / p0l; by = p0cy / p0l; bz = p0cz / p0l; } else bulge = 0; }
                        }
                        double p1x = P0.x + 0.333 * (P3.x - P0.x) + bx * bulge;
                        double p1y = P0.y + 0.333 * (P3.y - P0.y) + by * bulge;
                        double p1z = P0.z + 0.333 * (P3.z - P0.z) + bz * bulge;
                        double p2x = P0.x + 0.667 * (P3.x - P0.x) + bx * bulge;
                        double p2y = P0.y + 0.667 * (P3.y - P0.y) + by * bulge;
                        double p2z = P0.z + 0.667 * (P3.z - P0.z) + bz * bulge;
                        double r1x = p1x - planetPos.x, r1y = p1y - planetPos.y, r1z = p1z - planetPos.z;
                        double dist1 = Math.Sqrt(r1x * r1x + r1y * r1y + r1z * r1z);
                        if (dist1 < minDist && dist1 > 1e-6) { double sc = minDist / dist1; p1x = planetPos.x + r1x * sc; p1y = planetPos.y + r1y * sc; p1z = planetPos.z + r1z * sc; }
                        double r2x = p2x - planetPos.x, r2y = p2y - planetPos.y, r2z = p2z - planetPos.z;
                        double dist2 = Math.Sqrt(r2x * r2x + r2y * r2y + r2z * r2z);
                        if (dist2 < minDist && dist2 > 1e-6) { double sc = minDist / dist2; p2x = planetPos.x + r2x * sc; p2y = planetPos.y + r2y * sc; p2z = planetPos.z + r2z * sc; }
                        double dp1x = 3 * (p1x - P0.x), dp1y = 3 * (p1y - P0.y), dp1z = 3 * (p1z - P0.z);
                        double dp2x = 3 * (p2x - p1x), dp2y = 3 * (p2y - p1y), dp2z = 3 * (p2z - p1z);
                        double dp3x = 3 * (P3.x - p2x), dp3y = 3 * (P3.y - p2y), dp3z = 3 * (P3.z - p2z);
                        double dPds1 = Math.Sqrt(dp3x * dp3x + dp3y * dp3y + dp3z * dp3z);
                        double alpha = dPds1 > 1e-6 ? Plugin.LandingSpeed / dPds1 : 1.0;
                        alpha = alpha < 0.1 ? 0.1 : (alpha > 1.0 ? 1.0 : alpha);
                        double sParam = Math.Pow(t, alpha);
                        double sm = 1 - sParam, sm2 = sm * sm, sm3 = sm2 * sm, s2 = sParam * sParam, s3 = s2 * sParam;
                        px = sm3 * P0.x + 3 * sm2 * sParam * p1x + 3 * sm * s2 * p2x + s3 * P3.x;
                        py = sm3 * P0.y + 3 * sm2 * sParam * p1y + 3 * sm * s2 * p2y + s3 * P3.y;
                        pz = sm3 * P0.z + 3 * sm2 * sParam * p1z + 3 * sm * s2 * p2z + s3 * P3.z;
                        double dPdsx = sm2 * dp1x + 2 * sm * sParam * dp2x + s2 * dp3x;
                        double dPdsy = sm2 * dp1y + 2 * sm * sParam * dp2y + s2 * dp3y;
                        double dPdsz = sm2 * dp1z + 2 * sm * sParam * dp2z + s2 * dp3z;
                        double tSafe = t < 0.001 ? 0.001 : t;
                        double dsdt = alpha * Math.Pow(tSafe, alpha - 1.0) / Plugin.FallDurationSeconds;
                        vx = dPdsx * dsdt;
                        vy = dPdsy * dsdt;
                        vz = dPdsz * dsdt;
                    }

                    double dx = px - planetPos.x, dy = py - planetPos.y, dz = pz - planetPos.z;
                    double heightAboveSurface = Math.Sqrt(dx * dx + dy * dy + dz * dz) - planetRadius;
                    if (heightAboveSurface < Plugin.HandoffHeightAboveSurface)
                        continue;

                    ref TrashData trash = ref dataPool[index];
                    trash.uPos.x = px;
                    trash.uPos.y = py;
                    trash.uPos.z = pz;
                    trash.uVel.x = vx;
                    trash.uVel.y = vy;
                    trash.uVel.z = vz;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[BattlefieldAnalysisBaseCollectSpaceJunk] TrashSystemGravityPatch 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
