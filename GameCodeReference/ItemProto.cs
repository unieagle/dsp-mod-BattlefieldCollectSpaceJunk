using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Token: 0x02000338 RID: 824
[Serializable]
public class ItemProto : Proto
{
	// Token: 0x170004EA RID: 1258
	// (get) Token: 0x060022EE RID: 8942 RVA: 0x00245F97 File Offset: 0x00244197
	// (set) Token: 0x060022EF RID: 8943 RVA: 0x00245F9F File Offset: 0x0024419F
	public string miningFrom { get; set; }

	// Token: 0x170004EB RID: 1259
	// (get) Token: 0x060022F0 RID: 8944 RVA: 0x00245FA8 File Offset: 0x002441A8
	// (set) Token: 0x060022F1 RID: 8945 RVA: 0x00245FB0 File Offset: 0x002441B0
	public string produceFrom { get; set; }

	// Token: 0x170004EC RID: 1260
	// (get) Token: 0x060022F2 RID: 8946 RVA: 0x00245FB9 File Offset: 0x002441B9
	// (set) Token: 0x060022F3 RID: 8947 RVA: 0x00245FC1 File Offset: 0x002441C1
	public string description { get; set; }

	// Token: 0x170004ED RID: 1261
	// (get) Token: 0x060022F4 RID: 8948 RVA: 0x00245FCA File Offset: 0x002441CA
	// (set) Token: 0x060022F5 RID: 8949 RVA: 0x00245FD2 File Offset: 0x002441D2
	public int index { get; private set; }

	// Token: 0x170004EE RID: 1262
	// (get) Token: 0x060022F6 RID: 8950 RVA: 0x00245FDB File Offset: 0x002441DB
	public Sprite iconSprite
	{
		get
		{
			return this._iconSprite;
		}
	}

	// Token: 0x170004EF RID: 1263
	// (get) Token: 0x060022F7 RID: 8951 RVA: 0x00245FE3 File Offset: 0x002441E3
	public Sprite propertyIconSprite
	{
		get
		{
			return this._propertyIconSprite;
		}
	}

	// Token: 0x170004F0 RID: 1264
	// (get) Token: 0x060022F8 RID: 8952 RVA: 0x00245FEB File Offset: 0x002441EB
	public Sprite propertyIconSpriteSmall
	{
		get
		{
			return this._propertyIconSpriteSmall;
		}
	}

	// Token: 0x170004F1 RID: 1265
	// (get) Token: 0x060022F9 RID: 8953 RVA: 0x00245FF3 File Offset: 0x002441F3
	// (set) Token: 0x060022FA RID: 8954 RVA: 0x00245FFB File Offset: 0x002441FB
	public string propertyName { get; private set; }

	// Token: 0x060022FB RID: 8955 RVA: 0x00246004 File Offset: 0x00244204
	public void RefreshTranslation()
	{
		base.name = this.Name.Translate();
		this.miningFrom = this.MiningFrom.Translate();
		this.produceFrom = this.ProduceFrom.Translate();
		this.description = this.Description.Translate();
		if (this.ID > 6000 && this.ID < 6100)
		{
			for (int i = 0; i < PropertySystem.itemIds.Length; i++)
			{
				if (PropertySystem.itemIds[i] == this.ID)
				{
					this.propertyName = PropertySystem.itemNames[i];
					this.propertyName = this.propertyName.Translate();
					return;
				}
			}
		}
	}

	// Token: 0x060022FC RID: 8956 RVA: 0x002460B0 File Offset: 0x002442B0
	public void Preload(int _index)
	{
		this.index = _index;
		if (this.ID >= 12000)
		{
			Debug.LogError("物品ID不能大于 " + 12000.ToString());
		}
		if (this.SubID >= 256)
		{
			Debug.LogError("物品SubID不能大于 " + 256.ToString());
		}
		base.name = this.Name.Translate();
		this.miningFrom = this.MiningFrom.Translate();
		this.produceFrom = this.ProduceFrom.Translate();
		this.description = this.Description.Translate();
		this.iconTagString = "\\" + this.IconTag + ";";
		this.iconNameTagString = "\\" + this.IconTag + "-;";
		if (!string.IsNullOrEmpty(this.IconPath))
		{
			this._iconSprite = Resources.Load<Sprite>(this.IconPath);
		}
		int num = this.ModelIndex + this.ModelCount;
		for (int i = this.ModelIndex; i < num; i++)
		{
			ModelProto modelProto = LDB.models.modelArray[i];
			if (modelProto != null && modelProto.iconSprite == null && this.iconSprite != null)
			{
				modelProto.OverrideIconSprite(this.iconSprite);
			}
		}
		if (this.ID > 6000 && this.ID < 6100)
		{
			this._propertyIconSprite = Resources.Load<Sprite>(string.Format("Icons/Property/property-icon-{0}", this.ID));
			this._propertyIconSpriteSmall = Resources.Load<Sprite>(string.Format("Icons/Property/property-icon-40px-{0}", this.ID));
			this.propertyName = "";
			for (int j = 0; j < PropertySystem.itemIds.Length; j++)
			{
				if (PropertySystem.itemIds[j] == this.ID)
				{
					this.propertyName = PropertySystem.itemNames[j];
					this.propertyName = this.propertyName.Translate();
					break;
				}
			}
		}
		ModelProto modelProto2 = LDB.models.modelArray[this.ModelIndex];
		if (modelProto2 != null)
		{
			this.prefabDesc = modelProto2.prefabDesc;
		}
		else
		{
			this.prefabDesc = PrefabDesc.none;
		}
		if (this.prefabDesc.isCollectStation)
		{
			ItemProto.stationCollectorId = this.ID;
		}
		ItemProto.itemProtoById[this.ID] = this;
		this.FindRecipes();
		this.ComputeRawMats();
		this.FindPreTech();
	}

	// Token: 0x060022FD RID: 8957 RVA: 0x00246320 File Offset: 0x00244520
	public int GetUpgradeID(int upgrade)
	{
		if (this.Grade == 0 || this.Upgrades.Length == 0)
		{
			return this.ID;
		}
		int num = this.Grade + upgrade;
		if (num < 1)
		{
			num = 1;
		}
		else if (num > this.Upgrades.Length)
		{
			num = this.Upgrades.Length;
		}
		return this.Upgrades[num - 1];
	}

	// Token: 0x060022FE RID: 8958 RVA: 0x00246374 File Offset: 0x00244574
	public int GetGradeID(int grade)
	{
		if (this.Grade == 0 || this.Upgrades.Length == 0)
		{
			return this.ID;
		}
		if (grade < 1)
		{
			grade = 1;
		}
		else if (grade > this.Upgrades.Length)
		{
			grade = this.Upgrades.Length;
		}
		return this.Upgrades[grade - 1];
	}

	// Token: 0x060022FF RID: 8959 RVA: 0x002463C4 File Offset: 0x002445C4
	public ItemProto GetUpgradeItem(int upgrade)
	{
		if (this.Grade == 0 || this.Upgrades.Length == 0)
		{
			return this;
		}
		int num = this.Grade + upgrade;
		if (num < 1)
		{
			num = 1;
		}
		else if (num > this.Upgrades.Length)
		{
			num = this.Upgrades.Length;
		}
		return LDB.items.Select(this.Upgrades[num - 1]);
	}

	// Token: 0x06002300 RID: 8960 RVA: 0x00246420 File Offset: 0x00244620
	public ItemProto GetUpgradeItem(GameHistoryData history, int upgrade)
	{
		if (this.Grade == 0 || this.Upgrades.Length == 0)
		{
			return this;
		}
		int num = this.Grade + upgrade;
		if (num < 1)
		{
			num = 1;
		}
		else if (num > this.Upgrades.Length)
		{
			num = this.Upgrades.Length;
		}
		ItemProto itemProto = LDB.items.Select(this.Upgrades[num - 1]);
		if (itemProto != null && itemProto != this && (itemProto.ID == 2318 || itemProto.ID == 2319 || itemProto.ID == 2902) && !history.ItemUnlocked(itemProto.ID))
		{
			itemProto = LDB.items.Select(this.Upgrades[num - 2]);
		}
		if (itemProto != null)
		{
			return itemProto;
		}
		return this;
	}

	// Token: 0x06002301 RID: 8961 RVA: 0x002464D4 File Offset: 0x002446D4
	public ItemProto GetGradeItem(int grade)
	{
		if (this.Grade == 0 || this.Upgrades.Length == 0)
		{
			return this;
		}
		if (grade < 1)
		{
			grade = 1;
		}
		else if (grade > this.Upgrades.Length)
		{
			grade = this.Upgrades.Length;
		}
		return LDB.items.Select(this.Upgrades[grade - 1]);
	}

	// Token: 0x06002302 RID: 8962 RVA: 0x00246528 File Offset: 0x00244728
	public bool IsUpgradeOf(ItemProto other)
	{
		if (other == null)
		{
			return false;
		}
		if (this.ID == other.ID)
		{
			return true;
		}
		for (int i = 0; i < this.Upgrades.Length; i++)
		{
			if (this.Upgrades[i] == other.ID)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06002303 RID: 8963 RVA: 0x00246570 File Offset: 0x00244770
	public bool IsUpgradeOf(int other)
	{
		if (other == 0)
		{
			return false;
		}
		if (this.ID == other)
		{
			return true;
		}
		for (int i = 0; i < this.Upgrades.Length; i++)
		{
			if (this.Upgrades[i] == other)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06002304 RID: 8964 RVA: 0x002465B0 File Offset: 0x002447B0
	public static bool IsUpgradeOf(int itemProtoIdA, int itemProtoIdB)
	{
		ItemProto itemProto = LDB.items.Select(itemProtoIdA);
		ItemProto other = LDB.items.Select(itemProtoIdB);
		return itemProto != null && itemProto.IsUpgradeOf(other);
	}

	// Token: 0x06002305 RID: 8965 RVA: 0x002465E0 File Offset: 0x002447E0
	public bool IsSimilar(ItemProto other)
	{
		if (other == null)
		{
			return false;
		}
		if (this.ID == other.ID)
		{
			return true;
		}
		for (int i = 0; i < this.Upgrades.Length; i++)
		{
			if (this.Upgrades[i] == other.ID)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x170004F2 RID: 1266
	// (get) Token: 0x06002306 RID: 8966 RVA: 0x00246628 File Offset: 0x00244828
	public bool canUpgrade
	{
		get
		{
			return this.Grade > 0 && this.Upgrades.Length != 0;
		}
	}

	// Token: 0x170004F3 RID: 1267
	// (get) Token: 0x06002307 RID: 8967 RVA: 0x0024663F File Offset: 0x0024483F
	public bool isAmmo
	{
		get
		{
			return this.AmmoType > EAmmoType.None;
		}
	}

	// Token: 0x170004F4 RID: 1268
	// (get) Token: 0x06002308 RID: 8968 RVA: 0x0024664A File Offset: 0x0024484A
	public bool isBomb
	{
		get
		{
			return this.BombType > EBombType.None;
		}
	}

	// Token: 0x170004F5 RID: 1269
	// (get) Token: 0x06002309 RID: 8969 RVA: 0x00246655 File Offset: 0x00244855
	public bool isCraft
	{
		get
		{
			return this.CraftType > 0;
		}
	}

	// Token: 0x170004F6 RID: 1270
	// (get) Token: 0x0600230A RID: 8970 RVA: 0x00246660 File Offset: 0x00244860
	public bool isDynamicCraft
	{
		get
		{
			return (this.CraftType & 1) > 0;
		}
	}

	// Token: 0x170004F7 RID: 1271
	// (get) Token: 0x0600230B RID: 8971 RVA: 0x0024666D File Offset: 0x0024486D
	public bool isSpaceCraft
	{
		get
		{
			return (this.CraftType & 2) > 0;
		}
	}

	// Token: 0x170004F8 RID: 1272
	// (get) Token: 0x0600230C RID: 8972 RVA: 0x0024667A File Offset: 0x0024487A
	public bool isLargeCraft
	{
		get
		{
			return (this.CraftType & 4) > 0;
		}
	}

	// Token: 0x170004F9 RID: 1273
	// (get) Token: 0x0600230D RID: 8973 RVA: 0x00246687 File Offset: 0x00244887
	public bool isFighter
	{
		get
		{
			return 5100 <= this.ID && this.ID <= 5199;
		}
	}

	// Token: 0x170004FA RID: 1274
	// (get) Token: 0x0600230E RID: 8974 RVA: 0x002466A8 File Offset: 0x002448A8
	public bool isGroundFighter
	{
		get
		{
			return this.isFighter && !this.isSpaceCraft;
		}
	}

	// Token: 0x170004FB RID: 1275
	// (get) Token: 0x0600230F RID: 8975 RVA: 0x002466BD File Offset: 0x002448BD
	public bool isSpaceFighter
	{
		get
		{
			return this.isFighter && this.isSpaceCraft;
		}
	}

	// Token: 0x170004FC RID: 1276
	// (get) Token: 0x06002310 RID: 8976 RVA: 0x002466CF File Offset: 0x002448CF
	public bool isSmallSpaceFighter
	{
		get
		{
			return this.isFighter && this.isSpaceCraft && !this.isLargeCraft;
		}
	}

	// Token: 0x170004FD RID: 1277
	// (get) Token: 0x06002311 RID: 8977 RVA: 0x002466EC File Offset: 0x002448EC
	public bool isLargeSpaceFighter
	{
		get
		{
			return this.isFighter && this.isSpaceCraft && this.isLargeCraft;
		}
	}

	// Token: 0x06002312 RID: 8978 RVA: 0x00246708 File Offset: 0x00244908
	private void FindRecipes()
	{
		if (this.recipes != null)
		{
			return;
		}
		this.isRaw = true;
		this.recipes = new List<RecipeProto>(4);
		this.handcrafts = new List<RecipeProto>(4);
		this.makes = new List<RecipeProto>(4);
		this.handcraft = null;
		this.maincraft = null;
		RecipeProto[] dataArray = LDB.recipes.dataArray;
		int num = dataArray.Length;
		int num2 = 100;
		int num3 = 100;
		for (int i = 0; i < num; i++)
		{
			RecipeProto recipeProto = dataArray[i];
			int[] results = recipeProto.Results;
			int j = 0;
			while (j < results.Length)
			{
				if (results[j] == this.ID)
				{
					this.recipes.Add(recipeProto);
					if (j < num2)
					{
						num2 = j;
						this.maincraft = recipeProto;
						this.maincraftProductCount = recipeProto.ResultCounts[j];
					}
					if (!recipeProto.Handcraft)
					{
						break;
					}
					this.handcrafts.Add(recipeProto);
					this.isRaw = false;
					if (j < num3)
					{
						num3 = j;
						this.handcraft = recipeProto;
						this.handcraftProductCount = recipeProto.ResultCounts[j];
						break;
					}
					break;
				}
				else
				{
					j++;
				}
			}
			int[] items = recipeProto.Items;
			for (int k = 0; k < items.Length; k++)
			{
				if (items[k] == this.ID)
				{
					this.makes.Add(recipeProto);
					break;
				}
			}
		}
		this.ComputeRawMats();
	}

	// Token: 0x06002313 RID: 8979 RVA: 0x00246860 File Offset: 0x00244A60
	private void ComputeRawMats()
	{
		if (this.rawMats != null)
		{
			return;
		}
		this.rawMats = new List<IDCNTINC>();
		int num = 0;
		while (this.maincraft != null && num < this.maincraft.Items.Length)
		{
			ItemProto itemProto = LDB.items.Select(this.maincraft.Items[num]);
			if (itemProto != null)
			{
				if (itemProto.recipes == null)
				{
					itemProto.FindRecipes();
				}
				if (itemProto.isRaw)
				{
					this._add_raw_mat(this.maincraft.Items[num], this.maincraft.ItemCounts[num]);
				}
				else
				{
					List<IDCNTINC> list = itemProto.rawMats;
					for (int i = 0; i < list.Count; i++)
					{
						this._add_raw_mat(list[i].id, list[i].count * this.maincraft.ItemCounts[num]);
					}
				}
			}
			num++;
		}
	}

	// Token: 0x06002314 RID: 8980 RVA: 0x00246940 File Offset: 0x00244B40
	private void _add_raw_mat(int id, int cnt)
	{
		for (int i = 0; i < this.rawMats.Count; i++)
		{
			if (id == this.rawMats[i].id)
			{
				this.rawMats[i] = new IDCNTINC(id, cnt + this.rawMats[i].count, 0);
				return;
			}
		}
		this.rawMats.Add(new IDCNTINC(id, cnt, 0));
	}

	// Token: 0x06002315 RID: 8981 RVA: 0x002469B4 File Offset: 0x00244BB4
	public static void InitFuelNeeds()
	{
		ItemProto[] dataArray = LDB.items.dataArray;
		List<int> list = new List<int>();
		for (int i = 0; i < ItemProto.fuelNeeds.Length; i++)
		{
			list.Clear();
			foreach (ItemProto itemProto in dataArray)
			{
				if ((i & itemProto.FuelType) != 0)
				{
					list.Add(itemProto.ID);
				}
			}
			ItemProto.fuelNeeds[i] = list.ToArray();
		}
		list.Clear();
	}

	// Token: 0x06002316 RID: 8982 RVA: 0x00246A30 File Offset: 0x00244C30
	public static void InitTurretNeeds()
	{
		ItemProto[] dataArray = LDB.items.dataArray;
		List<int> list = new List<int>();
		for (int i = 1; i < ItemProto.turretNeeds.Length; i++)
		{
			list.Clear();
			foreach (ItemProto itemProto in dataArray)
			{
				if (i == (int)itemProto.AmmoType)
				{
					list.Add(itemProto.ID);
				}
			}
			if (list.Count > 0)
			{
				for (int k = list.Count; k < 6; k++)
				{
					list.Add(0);
				}
			}
			ItemProto.turretNeeds[i] = list.ToArray();
		}
		list.Clear();
	}

	// Token: 0x06002317 RID: 8983 RVA: 0x00246AD0 File Offset: 0x00244CD0
	public static bool isFluid(int itemId)
	{
		int num = ItemProto.fluids.Length;
		for (int i = 0; i < num; i++)
		{
			if (ItemProto.fluids[i] == itemId)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06002318 RID: 8984 RVA: 0x00246B00 File Offset: 0x00244D00
	public static void InitFluids()
	{
		ItemProto[] dataArray = LDB.items.dataArray;
		List<int> list = new List<int>();
		for (int i = 0; i < dataArray.Length; i++)
		{
			if (dataArray[i].IsFluid)
			{
				list.Add(dataArray[i].ID);
			}
		}
		ItemProto.fluids = list.ToArray();
	}

	// Token: 0x06002319 RID: 8985 RVA: 0x00246B50 File Offset: 0x00244D50
	public static void InitTurrets()
	{
		ItemProto[] dataArray = LDB.items.dataArray;
		List<int> list = new List<int>();
		for (int i = 0; i < dataArray.Length; i++)
		{
			if (dataArray[i].Type == EItemType.Turret)
			{
				list.Add(dataArray[i].ID);
			}
		}
		ItemProto.turrets = list.ToArray();
	}

	// Token: 0x0600231A RID: 8986 RVA: 0x00246BA0 File Offset: 0x00244DA0
	public static void InitEnemyDropTables()
	{
		ItemProto.enemyDropRangeTable = new int[10001];
		ItemProto.enemyDropLevelTable = new int[12000];
		ItemProto.enemyDropCountTable = new float[12000];
		ItemProto.enemyDropMaskTable = new int[12000];
		ItemProto.enemyDropMaskRatioTable = new float[12000];
		for (int i = 0; i < 12000; i++)
		{
			ItemProto.enemyDropLevelTable[i] = -1;
			ItemProto.enemyDropCountTable[i] = 0f;
			ItemProto.enemyDropMaskTable[i] = 0;
			ItemProto.enemyDropMaskRatioTable[i] = 1f;
		}
		ItemProto[] dataArray = LDB.items.dataArray;
		for (int j = 0; j < dataArray.Length; j++)
		{
			int id = dataArray[j].ID;
			if (dataArray[j].EnemyDropRange.y > 5E-05f)
			{
				ItemProto.enemyDropLevelTable[id] = dataArray[j].EnemyDropLevel;
				ItemProto.enemyDropCountTable[id] = dataArray[j].EnemyDropCount;
				ItemProto.enemyDropMaskTable[id] = dataArray[j].EnemyDropMask;
				ItemProto.enemyDropMaskRatioTable[id] = dataArray[j].EnemyDropMaskRatio;
				int num = (int)(dataArray[j].EnemyDropRange.x * 10000f + 0.5f);
				int num2 = (int)((dataArray[j].EnemyDropRange.x + dataArray[j].EnemyDropRange.y) * 10000f + 0.5f);
				for (int k = num; k < num2; k++)
				{
					ItemProto.enemyDropRangeTable[k] = id;
				}
			}
		}
	}

	// Token: 0x0600231B RID: 8987 RVA: 0x00246D08 File Offset: 0x00244F08
	public static void InitConstructableItems()
	{
		ItemProto[] dataArray = LDB.items.dataArray;
		ItemProto.constructableIdHash = new HashSet<int>();
		ItemProto.constructableIndiceById = new int[12000];
		for (int i = 0; i < 12000; i++)
		{
			ItemProto.constructableIndiceById[i] = -1;
		}
		int num = 0;
		List<int> list = new List<int>();
		for (int j = 0; j < dataArray.Length; j++)
		{
			if (dataArray[j].CanBuild && dataArray[j].ID != 1131)
			{
				list.Add(dataArray[j].ID);
				ItemProto.constructableIdHash.Add(dataArray[j].ID);
				ItemProto.constructableIndiceById[dataArray[j].ID] = num++;
			}
		}
		ItemProto.constructableCount = num;
		ItemProto.constructableIds = list.ToArray();
	}

	// Token: 0x0600231C RID: 8988 RVA: 0x00246DD0 File Offset: 0x00244FD0
	public static void InitItemIds()
	{
		ItemProto[] dataArray = LDB.items.dataArray;
		List<int> list = new List<int>();
		for (int i = 0; i < dataArray.Length; i++)
		{
			list.Add(dataArray[i].ID);
		}
		list.Add(11901);
		list.Add(11902);
		list.Add(11903);
		ItemProto.itemIds = list.ToArray();
	}

	// Token: 0x0600231D RID: 8989 RVA: 0x00246E38 File Offset: 0x00245038
	public static void InitItemIndices()
	{
		ItemProto[] dataArray = LDB.items.dataArray;
		ItemProto.itemIndices = new int[12000];
		for (int i = 0; i < 12000; i++)
		{
			ItemProto.itemIndices[i] = 9999;
		}
		for (int j = 0; j < dataArray.Length; j++)
		{
			ItemProto.itemIndices[dataArray[j].ID] = dataArray[j].index;
		}
	}

	// Token: 0x0600231E RID: 8990 RVA: 0x00246EA0 File Offset: 0x002450A0
	public static void InitMechaMaterials()
	{
		int[] array = new int[128];
		int num = 0;
		ItemProto[] dataArray = LDB.items.dataArray;
		for (int i = 0; i < dataArray.Length; i++)
		{
			if (dataArray[i].ID > 0 && dataArray[i].MechaMaterialID > 0)
			{
				array[dataArray[i].MechaMaterialID] = dataArray[i].ID;
				num++;
			}
		}
		ItemProto.mechaMaterials = new int[num];
		int num2 = 0;
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] > 0)
			{
				ItemProto.mechaMaterials[num2++] = array[j];
			}
		}
	}

	// Token: 0x0600231F RID: 8991 RVA: 0x00246F3C File Offset: 0x0024513C
	public static void InitFighterIndices()
	{
		ItemProto[] dataArray = LDB.items.dataArray;
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		List<int> list4 = new List<int>();
		List<int> list5 = new List<int>();
		for (int i = 0; i < dataArray.Length; i++)
		{
			PrefabDesc prefabDesc = dataArray[i].prefabDesc;
			int id = dataArray[i].ID;
			if (prefabDesc.isCraftUnit)
			{
				list.Add(id);
				if (dataArray[i].isSpaceCraft)
				{
					list3.Add(id);
				}
				else
				{
					list2.Add(id);
				}
				if (dataArray[i].isSmallSpaceFighter)
				{
					list4.Add(id);
				}
				else if (dataArray[i].isLargeSpaceFighter)
				{
					list5.Add(id);
				}
			}
		}
		ItemProto.kFighterIds = list.ToArray();
		ItemProto.kFighterGroundIds = list2.ToArray();
		ItemProto.kFighterSpaceIds = list3.ToArray();
		ItemProto.kFighterSpaceSmallIds = list4.ToArray();
		ItemProto.kFighterSpaceLargeIds = list5.ToArray();
	}

	// Token: 0x06002320 RID: 8992 RVA: 0x0024702C File Offset: 0x0024522C
	public static void InitPowerFacilityIndices()
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		list.Add(0);
		list2.Add(0);
		ItemProto.powerGenId2Index = new int[12000];
		ItemProto.powerConId2Index = new int[12000];
		foreach (ItemProto itemProto in LDB.items.dataArray)
		{
			if (itemProto != null)
			{
				PrefabDesc prefabDesc = itemProto.prefabDesc;
				if (prefabDesc.isPowerGen && !list.Contains(itemProto.ID))
				{
					list.Add(itemProto.ID);
					ItemProto.powerGenId2Index[itemProto.ID] = list.Count - 1;
				}
				if (prefabDesc.isPowerConsumer && !list2.Contains(itemProto.ID))
				{
					list2.Add(itemProto.ID);
					ItemProto.powerConId2Index[itemProto.ID] = list2.Count - 1;
				}
				if (prefabDesc.isPowerCharger && !list2.Contains(itemProto.ID))
				{
					list2.Add(itemProto.ID);
					ItemProto.powerConId2Index[itemProto.ID] = list2.Count - 1;
				}
				if (prefabDesc.isAccumulator)
				{
					if (!list.Contains(itemProto.ID))
					{
						list.Add(itemProto.ID);
						ItemProto.powerGenId2Index[itemProto.ID] = list.Count - 1;
					}
					if (!list2.Contains(itemProto.ID))
					{
						list2.Add(itemProto.ID);
						ItemProto.powerConId2Index[itemProto.ID] = list2.Count - 1;
					}
				}
				if (prefabDesc.isPowerExchanger)
				{
					if (!list.Contains(itemProto.ID))
					{
						list.Add(itemProto.ID);
						ItemProto.powerGenId2Index[itemProto.ID] = list.Count - 1;
					}
					if (!list2.Contains(itemProto.ID))
					{
						list2.Add(itemProto.ID);
						ItemProto.powerConId2Index[itemProto.ID] = list2.Count - 1;
					}
				}
			}
		}
		ItemProto.powerGenIndex2Id = list.ToArray();
		ItemProto.powerConIndex2Id = list2.ToArray();
	}

	// Token: 0x06002321 RID: 8993 RVA: 0x0024723C File Offset: 0x0024543C
	public void FindPreTech()
	{
		if (this.PreTechOverride > 0)
		{
			this.missingTech = false;
			this.preTech = LDB.techs.Select(this.PreTechOverride);
			return;
		}
		if (this.UnlockKey == -2)
		{
			this.missingTech = false;
			this.preTech = null;
			return;
		}
		if (this.maincraft != null)
		{
			for (int i = 0; i < LDB.techs.Length; i++)
			{
				TechProto techProto = LDB.techs.dataArray[i];
				for (int j = 0; j < techProto.UnlockRecipes.Length; j++)
				{
					if (techProto.UnlockRecipes[j] == this.maincraft.ID)
					{
						this.missingTech = false;
						this.preTech = techProto;
						return;
					}
				}
			}
			this.missingTech = true;
			this.preTech = null;
			return;
		}
		if (this.Type == EItemType.Resource)
		{
			this.missingTech = false;
			this.preTech = null;
			return;
		}
		this.missingTech = true;
		this.preTech = null;
	}

	// Token: 0x06002322 RID: 8994 RVA: 0x00247320 File Offset: 0x00245520
	public static void InitProductionMask()
	{
		foreach (RecipeProto recipeProto in LDB.recipes.dataArray)
		{
			if (recipeProto.Type != ERecipeType.None)
			{
				foreach (int id in recipeProto.Items)
				{
					ItemProto itemProto = LDB.items.Select(id);
					if (itemProto != null)
					{
						if (recipeProto.Type <= ERecipeType.Particle)
						{
							itemProto.consumptionMask |= 1;
						}
						else if (recipeProto.Type == ERecipeType.Fractionate)
						{
							itemProto.consumptionMask |= 8;
						}
						else if (recipeProto.Type == ERecipeType.Research)
						{
							itemProto.consumptionMask |= 2;
						}
					}
				}
				foreach (int id2 in recipeProto.Results)
				{
					ItemProto itemProto2 = LDB.items.Select(id2);
					if (itemProto2 != null)
					{
						if (recipeProto.Type <= ERecipeType.Particle)
						{
							itemProto2.productionMask |= 1;
						}
						else if (recipeProto.Type == ERecipeType.Fractionate)
						{
							itemProto2.productionMask |= 8;
						}
						else if (recipeProto.Type == ERecipeType.Research)
						{
							itemProto2.productionMask |= 2;
						}
					}
				}
			}
		}
		for (int l = 0; l < LabComponent.matrixIds.Length; l++)
		{
			int id3 = LabComponent.matrixIds[l];
			ItemProto itemProto3 = LDB.items.Select(id3);
			if (itemProto3 != null)
			{
				itemProto3.consumptionMask |= 2;
			}
		}
		VeinProto[] dataArray2 = LDB.veins.dataArray;
		for (int m = 0; m < dataArray2.Length; m++)
		{
			int miningItem = dataArray2[m].MiningItem;
			ItemProto itemProto4 = LDB.items.Select(miningItem);
			if (itemProto4 != null)
			{
				itemProto4.productionMask |= 4;
			}
		}
		ItemProto[] dataArray3 = LDB.items.dataArray;
		for (int n = 0; n < dataArray3.Length; n++)
		{
			PrefabDesc prefabDesc = dataArray3[n].prefabDesc;
			if (prefabDesc.isEjector)
			{
				int ejectorBulletId = prefabDesc.ejectorBulletId;
				ItemProto itemProto5 = LDB.items.Select(ejectorBulletId);
				if (itemProto5 != null)
				{
					itemProto5.consumptionMask |= 16;
				}
			}
			if (prefabDesc.isSilo)
			{
				int siloBulletId = prefabDesc.siloBulletId;
				ItemProto itemProto6 = LDB.items.Select(siloBulletId);
				if (itemProto6 != null)
				{
					itemProto6.consumptionMask |= 32;
				}
			}
			if (prefabDesc.isPowerGen)
			{
				if (prefabDesc.fuelMask > 0)
				{
					foreach (int id4 in ItemProto.fuelNeeds[prefabDesc.fuelMask])
					{
						ItemProto itemProto7 = LDB.items.Select(id4);
						if (itemProto7 != null)
						{
							itemProto7.consumptionMask |= 64;
						}
					}
				}
				if (prefabDesc.powerProductId > 0)
				{
					int powerProductId = prefabDesc.powerProductId;
					ItemProto itemProto8 = LDB.items.Select(powerProductId);
					if (itemProto8 != null)
					{
						itemProto8.productionMask |= 64;
					}
				}
				if (prefabDesc.powerCatalystId > 0)
				{
					int powerCatalystId = prefabDesc.powerCatalystId;
					ItemProto itemProto9 = LDB.items.Select(powerCatalystId);
					if (itemProto9 != null)
					{
						itemProto9.consumptionMask |= 64;
					}
				}
			}
			if (prefabDesc.isSpraycoster)
			{
				foreach (int id5 in prefabDesc.incItemId)
				{
					ItemProto itemProto10 = LDB.items.Select(id5);
					if (itemProto10 != null)
					{
						itemProto10.consumptionMask |= 256;
					}
				}
			}
		}
		ThemeProto[] dataArray4 = LDB.themes.dataArray;
		for (int num3 = 0; num3 < dataArray4.Length; num3++)
		{
			if (dataArray4[num3].PlanetType == EPlanetType.Gas)
			{
				foreach (int id6 in dataArray4[num3].GasItems)
				{
					ItemProto itemProto11 = LDB.items.Select(id6);
					if (itemProto11 != null)
					{
						itemProto11.productionMask |= 128;
					}
				}
			}
			if (dataArray4[num3].WaterItemId > 0)
			{
				int waterItemId = dataArray4[num3].WaterItemId;
				ItemProto itemProto12 = LDB.items.Select(waterItemId);
				if (itemProto12 != null)
				{
					itemProto12.productionMask |= 4;
				}
			}
		}
	}

	// Token: 0x170004FE RID: 1278
	// (get) Token: 0x06002323 RID: 8995 RVA: 0x0024775C File Offset: 0x0024595C
	public string typeString
	{
		get
		{
			switch (this.Type)
			{
			case EItemType.Unknown:
				return "未知分类".Translate();
			case EItemType.Resource:
				return "自然资源".Translate();
			case EItemType.Material:
				return "材料".Translate();
			case EItemType.Component:
				return "组件".Translate();
			case EItemType.Product:
				return "成品".Translate();
			case EItemType.Logistics:
				if (this.prefabDesc.isAccumulator)
				{
					return "电力储存".Translate();
				}
				if (this.prefabDesc.isPowerNode)
				{
					return "电力运输".Translate();
				}
				if (this.prefabDesc.isPowerExchanger)
				{
					return "电力交换".Translate();
				}
				return "物流运输".Translate();
			case EItemType.Production:
				if (this.prefabDesc.isPowerGen)
				{
					return "电力设备".Translate();
				}
				if (this.prefabDesc.isLab)
				{
					return "科研设备".Translate();
				}
				switch (this.prefabDesc.assemblerRecipeType)
				{
				case ERecipeType.Smelt:
					return "冶炼设备".Translate();
				case ERecipeType.Chemical:
					return "化工设备".Translate();
				case ERecipeType.Refine:
					return "精炼设备".Translate();
				case ERecipeType.Assemble:
					return "制造台".Translate();
				case ERecipeType.Particle:
					return "粒子对撞机".Translate();
				case ERecipeType.Exchange:
					return "能量交换器".Translate();
				case ERecipeType.PhotonStore:
					return "射线接收站".Translate();
				case ERecipeType.Fractionate:
					return "分馏设备".Translate();
				case ERecipeType.Research:
					return "科研设备".Translate();
				}
				switch (this.prefabDesc.minerType)
				{
				case EMinerType.Water:
					return "抽水设备".Translate();
				case EMinerType.Vein:
					return "采矿设备".Translate();
				case EMinerType.Oil:
					return "抽油设备".Translate();
				default:
					return "生产设备".Translate();
				}
				break;
			case EItemType.Decoration:
				return "装饰物".Translate();
			case EItemType.Turret:
				return "武器".Translate();
			case EItemType.Defense:
				return "防御设施".Translate();
			case EItemType.DarkFog:
				return "黑雾物品".Translate();
			case EItemType.Matrix:
				return "科学矩阵".Translate();
			default:
				return "其他分类".Translate();
			}
		}
	}

	// Token: 0x170004FF RID: 1279
	// (get) Token: 0x06002324 RID: 8996 RVA: 0x002479AC File Offset: 0x00245BAC
	public string fuelTypeString
	{
		get
		{
			string text = "";
			if ((this.FuelType & 1) == 1)
			{
				text += "化学".Translate();
			}
			if ((this.FuelType & 2) == 2)
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += " / ";
				}
				text += "核能".Translate();
			}
			if ((this.FuelType & 4) == 4)
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += " / ";
				}
				text += "质能".Translate();
			}
			if ((this.FuelType & 8) == 8)
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += " / ";
				}
				text += "储存".Translate();
			}
			if ((this.FuelType & 16) == 16)
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += " / ";
				}
				text += "X";
			}
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
			return "-";
		}
	}

	// Token: 0x06002325 RID: 8997 RVA: 0x00247AA8 File Offset: 0x00245CA8
	public string GetPropName(int index)
	{
		if ((ulong)index >= (ulong)((long)this.DescFields.Length))
		{
			return "";
		}
		switch (this.DescFields[index])
		{
		case 0:
			return "采集自".Translate();
		case 1:
			return "制造于".Translate();
		case 2:
			return "燃料类型".Translate();
		case 3:
			return "能量".Translate();
		case 4:
			return "发电类型".Translate();
		case 5:
			return "发电功率".Translate();
		case 6:
			return "热效率".Translate();
		case 7:
			return "流体消耗".Translate();
		case 8:
			return "输入功率".Translate();
		case 9:
			return "输出功率".Translate();
		case 10:
			return "蓄电量".Translate();
		case 11:
			return "工作功率".Translate();
		case 12:
			return "待机功率".Translate();
		case 13:
			return "连接长度".Translate();
		case 14:
			return "覆盖范围".Translate();
		case 15:
			return "运载速度".Translate();
		case 16:
			return "接口数量".Translate();
		case 17:
			return "仓储空间".Translate();
		case 18:
			return "开采对象".Translate();
		case 19:
			return "开采速度".Translate();
		case 20:
			if (this.prefabDesc.inserterGrade != 4)
			{
				return "运送速度".Translate();
			}
			if (!GameMain.history.inserterBidirectional)
			{
				return "单程耗时".Translate();
			}
			return "传输速度".Translate();
		case 21:
			return "单次拾取货物".Translate();
		case 22:
			return "制造速度".Translate();
		case 23:
			return "血量".Translate();
		case 24:
			return "仓储物品".Translate();
		case 25:
			return "船运载量".Translate();
		case 26:
			return "船运载量".Translate();
		case 27:
			return "飞行速度".Translate();
		case 28:
			return "制造加速".Translate();
		case 29:
			return "喷涂次数".Translate();
		case 30:
			return "流体容量".Translate();
		case 31:
			return "机甲功率提升".Translate();
		case 32:
			return "采集速度".Translate();
		case 33:
			return "研究速度".Translate();
		case 34:
			return "弹射速度".Translate();
		case 35:
			return "发射速度".Translate();
		case 36:
			return "使用寿命".Translate();
		case 37:
			return "潜在能量".Translate();
		case 38:
			return "最大充能功率".Translate();
		case 39:
			return "基础发电功率".Translate();
		case 40:
			return "增产剂效果".Translate();
		case 41:
			return "喷涂增产效果".Translate();
		case 42:
			return "喷涂加速效果".Translate();
		case 43:
			return "额外电力消耗".Translate();
		case 44:
			return "配送范围".Translate();
		case 45:
			return "船运载量".Translate();
		case 46:
			return "飞行速度".Translate();
		case 47:
			return "弹药数量".Translate();
		case 48:
			return "伤害".Translate();
		case 49:
			return "射击速度".Translate();
		case 50:
			return "每秒伤害".Translate();
		case 51:
			return "防御塔类型".Translate();
		case 52:
			return "弹药类型".Translate();
		case 53:
			return "最大耐久度".Translate();
		case 54:
			return "手动制造速度".Translate();
		case 55:
			return "伤害类型".Translate();
		case 56:
			return "目标类型".Translate();
		case 57:
			return "对地防御范围".Translate();
		case 58:
			return "对天防御范围".Translate();
		case 59:
			return "飞行速度".Translate();
		case 60:
			return "射击距离".Translate();
		case 61:
			return "爆炸半径".Translate();
		case 62:
			return "主炮伤害".Translate();
		case 63:
			return "主炮射速".Translate();
		case 64:
			return "主炮射程".Translate();
		case 65:
			return "舰炮伤害".Translate();
		case 66:
			return "舰炮射速".Translate();
		case 67:
			return "舰炮射程".Translate();
		case 68:
			return "干扰单位".Translate();
		case 69:
			return "干扰效果".Translate();
		case 70:
			return "干扰时间".Translate();
		case 71:
			return "可投掷".Translate();
		case 72:
			return "单次拾取货物".Translate();
		case 73:
			return "输出堆叠层数".Translate();
		default:
			return "??";
		}
	}

	// Token: 0x06002326 RID: 8998 RVA: 0x00247F68 File Offset: 0x00246168
	public string GetPropValue(int index, StringBuilder sb, int incLevel)
	{
		incLevel = ((incLevel > 0) ? ((incLevel > 10) ? 10 : incLevel) : 0);
		double num = Cargo.incTableMilli[incLevel] + 1.0;
		double num2 = Cargo.accTableMilli[incLevel] + 1.0;
		if ((ulong)index >= (ulong)((long)this.DescFields.Length))
		{
			return "";
		}
		switch (this.DescFields[index])
		{
		case 0:
			if (!string.IsNullOrEmpty(this.miningFrom))
			{
				return this.miningFrom;
			}
			return "-";
		case 1:
			if (this.maincraft != null)
			{
				return this.maincraft.madeFromString;
			}
			if (!string.IsNullOrEmpty(this.produceFrom))
			{
				return this.produceFrom;
			}
			return "-";
		case 2:
			return this.fuelTypeString;
		case 3:
		{
			long valuel = (long)((double)this.HeatValue * (this.Productive ? num : 1.0) + 0.1);
			StringBuilderUtility.WriteKMG(sb, 8, valuel, true, '\u2009', ' ');
			if (this.Productive && incLevel > 0)
			{
				return "<color=#61D8FFB8>" + sb.ToString().TrimStart() + "J</color>";
			}
			return sb.ToString().TrimStart() + "J";
		}
		case 4:
			if (!this.prefabDesc.isPowerGen)
			{
				return "-";
			}
			if (this.prefabDesc.windForcedPower)
			{
				return "风能".Translate();
			}
			if (this.prefabDesc.photovoltaic)
			{
				return "光伏".Translate();
			}
			if (this.prefabDesc.gammaRayReceiver)
			{
				return "离子流".Translate();
			}
			if (this.prefabDesc.fuelMask <= 1 && this.prefabDesc.fuelMask > 0)
			{
				return "火力".Translate();
			}
			if (this.prefabDesc.geothermal)
			{
				return "地热".Translate();
			}
			return "离子流".Translate();
		case 5:
		case 39:
			StringBuilderUtility.WriteKMG(sb, 8, this.prefabDesc.genEnergyPerTick * 60L, true, '\u2009', ' ');
			return sb.ToString().TrimStart() + "W";
		case 6:
			if (this.prefabDesc.useFuelPerTick > 0L)
			{
				return ((float)this.prefabDesc.genEnergyPerTick / (float)this.prefabDesc.useFuelPerTick).ToString("0.# %");
			}
			return "0";
		case 7:
			return "-";
		case 8:
			if (this.prefabDesc.exchangeEnergyPerTick > 0L)
			{
				StringBuilderUtility.WriteKMG(sb, 8, this.prefabDesc.exchangeEnergyPerTick * 60L, true, '\u2009', ' ');
			}
			else
			{
				StringBuilderUtility.WriteKMG(sb, 8, this.prefabDesc.inputEnergyPerTick * 60L, true, '\u2009', ' ');
			}
			return sb.ToString().TrimStart() + "W";
		case 9:
			if (this.prefabDesc.exchangeEnergyPerTick > 0L)
			{
				StringBuilderUtility.WriteKMG(sb, 8, this.prefabDesc.exchangeEnergyPerTick * 60L, true, '\u2009', ' ');
			}
			else
			{
				StringBuilderUtility.WriteKMG(sb, 8, this.prefabDesc.outputEnergyPerTick * 60L, true, '\u2009', ' ');
			}
			return sb.ToString().TrimStart() + "W";
		case 10:
			StringBuilderUtility.WriteKMG(sb, 8, this.prefabDesc.maxAcuEnergy + this.prefabDesc.stationMaxEnergyAcc + this.prefabDesc.dispenserMaxEnergyAcc, true, '\u2009', ' ');
			return sb.ToString().TrimStart() + "J";
		case 11:
			StringBuilderUtility.WriteKMG(sb, 8, this.prefabDesc.workEnergyPerTick * 60L, true, '\u2009', ' ');
			return sb.ToString().TrimStart() + "W";
		case 12:
			StringBuilderUtility.WriteKMG(sb, 8, this.prefabDesc.idleEnergyPerTick * 60L, true, '\u2009', ' ');
			return sb.ToString().TrimStart() + "W";
		case 13:
			if (this.prefabDesc.powerConnectDistance > 0f)
			{
				return (this.prefabDesc.powerConnectDistance - 0.5f).ToString("0.##") + " m";
			}
			return "0";
		case 14:
			if (this.prefabDesc.powerCoverRadius > 0f && this.prefabDesc.beaconSignalRadius > 0f)
			{
				return string.Concat(new string[]
				{
					"电力".Translate(),
					" ",
					(this.prefabDesc.powerCoverRadius - 0.5f).ToString("0.##"),
					" m  /  ",
					"信号".Translate(),
					" ",
					this.prefabDesc.beaconSignalRadius.ToString("0.##"),
					" m"
				});
			}
			if (this.prefabDesc.powerCoverRadius > 0f)
			{
				return (this.prefabDesc.powerCoverRadius - 0.5f).ToString("0.##") + " m";
			}
			if (this.prefabDesc.battleBasePickRange > 0f && this.prefabDesc.constructionRange > 0f)
			{
				return string.Concat(new string[]
				{
					"建造".Translate(),
					" ",
					this.prefabDesc.constructionRange.ToString("0.##"),
					" m  /  ",
					"拾取".Translate(),
					" ",
					this.prefabDesc.battleBasePickRange.ToString("0.##"),
					" m"
				});
			}
			return "0";
		case 15:
			if (this.prefabDesc.isBelt)
			{
				return ((double)this.prefabDesc.beltSpeed * 60.0 / 10.0).ToString("0.##") + "/s";
			}
			return "0";
		case 16:
			if (this.prefabDesc.portPoses.Length != 0)
			{
				return this.prefabDesc.portPoses.Length.ToString("0");
			}
			return "0";
		case 17:
			if (this.prefabDesc.isStorage)
			{
				return (this.prefabDesc.storageRow * this.prefabDesc.storageCol).ToString() + "仓储空间的后缀".Translate();
			}
			return "0";
		case 18:
			if (this.prefabDesc.minerType == EMinerType.Water)
			{
				return "水源".Translate();
			}
			if (this.prefabDesc.minerType == EMinerType.Vein)
			{
				return "矿脉".Translate();
			}
			if (this.prefabDesc.minerType == EMinerType.Oil)
			{
				return "油田".Translate();
			}
			if (this.prefabDesc.isCollectStation)
			{
				return "气态行星".Translate();
			}
			return "-";
		case 19:
			if (this.prefabDesc.minerType == EMinerType.Vein)
			{
				return (60.0 / ((double)this.prefabDesc.minerPeriod / 600000.0) * (double)GameMain.history.miningSpeedScale).ToString("0.#") + "每分每矿脉".Translate();
			}
			if (this.prefabDesc.minerType == EMinerType.Oil)
			{
				return GameMain.history.miningSpeedScale.ToString("0.##") + "x";
			}
			if (this.prefabDesc.minerType == EMinerType.Water)
			{
				return (60.0 / ((double)this.prefabDesc.minerPeriod / 600000.0) * (double)GameMain.history.miningSpeedScale).ToString("0.#") + "/min";
			}
			return "-";
		case 20:
			if (!this.prefabDesc.isInserter)
			{
				return "";
			}
			if (this.prefabDesc.inserterGrade != 4)
			{
				return (300000.0 / (double)this.prefabDesc.inserterSTT).ToString("0.0") + "往返每秒每格".Translate();
			}
			if (!GameMain.history.inserterBidirectional)
			{
				return ((double)this.prefabDesc.inserterSTT / 600000.0).ToString("0.00") + "单程每格耗时".Translate();
			}
			return "<color=#61D8FFB8>120" + "每秒运送货物".Translate() + "</color>";
		case 21:
		{
			int num3 = (this.prefabDesc.inserterGrade == 3) ? GameMain.history.inserterStackCountObsolete : 1;
			if (num3 <= 1)
			{
				return "不支持".Translate();
			}
			return num3.ToString();
		}
		case 22:
		case 34:
		case 35:
			if (this.prefabDesc.isAssembler)
			{
				return ((double)this.prefabDesc.assemblerSpeed / 10000.0).ToString("0.###") + "x";
			}
			if (this.prefabDesc.isLab)
			{
				return ((double)this.prefabDesc.labAssembleSpeed / 10000.0).ToString("0.###") + "x";
			}
			if (this.prefabDesc.isEjector)
			{
				return (3600.0 / (double)(this.prefabDesc.ejectorChargeFrame + this.prefabDesc.ejectorColdFrame)).ToString("0.##") + "/min";
			}
			if (this.prefabDesc.isSilo)
			{
				return (3600.0 / (double)(this.prefabDesc.siloChargeFrame + this.prefabDesc.siloColdFrame)).ToString("0.##") + "/min";
			}
			return "-";
		case 23:
			return this.HpMax.ToString("0");
		case 24:
			return this.prefabDesc.stationMaxItemKinds.ToString() + "仓储物品种类后缀".Translate();
		case 25:
			if (GameMain.history == null)
			{
				return "-";
			}
			return GameMain.history.logisticDroneCarries.ToString();
		case 26:
			if (GameMain.history == null)
			{
				return "-";
			}
			return GameMain.history.logisticShipCarries.ToString();
		case 27:
			if (GameMain.history == null)
			{
				return "-";
			}
			return GameMain.history.logisticDroneSpeedModified.ToString("0.###") + " m/s";
		case 28:
			return this.Ability.ToString() + " %";
		case 29:
		{
			int num4 = (int)((double)this.HpMax * num + 0.1);
			if (incLevel > 0)
			{
				return "<color=#61D8FFB8>" + num4.ToString() + "</color>" + "喷涂次数的后缀".Translate();
			}
			return this.HpMax.ToString() + "喷涂次数的后缀".Translate();
		}
		case 30:
			return this.prefabDesc.fluidStorageCount.ToString("#,##0");
		case 31:
		{
			float num5 = this.ReactorInc + 1f;
			num5 *= (float)(this.Productive ? num : num2);
			num5 -= 1f;
			if (incLevel > 0)
			{
				if (this.Productive)
				{
					return "<color=#61D8FFB8>" + ((num5 > 0f) ? num5.ToString("+0.###%") : num5.ToString("0.###%")) + "</color>";
				}
				return "<color=#FD965EB8>" + ((num5 > 0f) ? num5.ToString("+0.###%") : num5.ToString("0.###%")) + "</color>";
			}
			else
			{
				if (num5 <= 0f)
				{
					return num5.ToString("0%");
				}
				return num5.ToString("+0%");
			}
			break;
		}
		case 32:
			return ((float)this.prefabDesc.stationCollectSpeed * GameMain.history.miningSpeedScale).ToString("0.0###") + "x";
		case 33:
			return (this.prefabDesc.labResearchSpeed * 60f * (float)GameMain.history.techSpeed).ToString() + " Hash/s";
		case 36:
			return GameMain.history.solarSailLife.ToString() + "空格秒".Translate();
		case 37:
			StringBuilderUtility.WriteKMG(sb, 8, this.Potential, true, '\u2009', ' ');
			return sb.ToString().TrimStart() + "J";
		case 38:
			StringBuilderUtility.WriteKMG(sb, 8, this.prefabDesc.workEnergyPerTick * 60L * 5L, true, '\u2009', ' ');
			return sb.ToString().TrimStart() + "W";
		case 40:
			if (this.Productive)
			{
				return "额外产出".Translate();
			}
			return "加速生产".Translate();
		case 41:
			return "+" + ((float)Cargo.incTable[this.Ability] * 0.1f).ToString("0.0") + "%";
		case 42:
			return "+" + ((float)Cargo.accTable[this.Ability] * 0.1f).ToString("0.0") + "%";
		case 43:
			return "+" + ((float)Cargo.powerTable[this.Ability] * 0.1f).ToString("0.0") + "%";
		case 44:
			if (GameMain.history == null)
			{
				return "-";
			}
			return GameMain.history.dispenserDeliveryMaxAngle.ToString("0.##") + "°";
		case 45:
			if (GameMain.history == null)
			{
				return "-";
			}
			return GameMain.history.logisticCourierCarries.ToString();
		case 46:
			if (GameMain.history == null)
			{
				return "-";
			}
			return GameMain.history.logisticCourierSpeedModified.ToString("0.0#") + " m/s";
		case 47:
		{
			int num6 = (int)((double)this.HpMax * num + ((this.HpMax < 12) ? 0.51 : 0.1));
			string text = (this.AmmoType > EAmmoType.None) ? "弹药数量单位".Translate() : "";
			if (incLevel > 0)
			{
				return "<color=#61D8FFB8>" + num6.ToString() + "</color>" + text;
			}
			return this.HpMax.ToString() + text;
		}
		case 48:
			if (this.isAmmo)
			{
				int num7 = this.Ability;
				GameHistoryData history = GameMain.history;
				if (history == null)
				{
					return (num7 / 100).ToString("0.0") + " hp";
				}
				double num8;
				switch (this.AmmoType)
				{
				case EAmmoType.Bullet:
					num8 = (double)((float)num7 * history.kineticDamageScale - (float)num7);
					goto IL_FC4;
				case EAmmoType.Cannon:
					num8 = (double)((float)num7 * (history.kineticDamageScale * 0.5f + history.blastDamageScale * 0.5f) - (float)num7);
					goto IL_FC4;
				case EAmmoType.Plasma:
					num8 = (double)((float)num7 * history.energyDamageScale - (float)num7);
					goto IL_FC4;
				case EAmmoType.Missile:
					num8 = (double)((float)num7 * history.blastDamageScale - (float)num7);
					goto IL_FC4;
				}
				num8 = 0.0;
				IL_FC4:
				num7 /= 100;
				num8 /= 100.0;
				if (num8 != 0.0)
				{
					return num7.ToString("0.0") + "<color=#61D8FFB8> + " + num8.ToString("0.0#") + "</color> hp";
				}
				return num7.ToString("0.0") + " hp";
			}
			else
			{
				if (!this.prefabDesc.isCraftUnit)
				{
					return "";
				}
				int num9 = this.prefabDesc.craftUnitAttackDamage0;
				GameHistoryData history2 = GameMain.history;
				if (history2 == null)
				{
					return (num9 / 100).ToString("0.0") + " hp";
				}
				float num10 = (float)num9 * history2.energyDamageScale;
				num10 = (this.isSpaceCraft ? (num10 * history2.combatShipDamageRatio - (float)num9) : (num10 * history2.combatDroneDamageRatio - (float)num9));
				num9 /= 100;
				num10 /= 100f;
				if (num10 != 0f)
				{
					return num9.ToString("0.0") + "<color=#61D8FFB8> + " + num10.ToString("0.0#") + "</color> hp";
				}
				return num9.ToString("0.0") + " hp";
			}
			break;
		case 49:
			if (this.prefabDesc.isTurret)
			{
				if (this.prefabDesc.turretMuzzleCount <= 1)
				{
					return ((float)this.prefabDesc.turretROF * 60f / (float)this.prefabDesc.turretRoundInterval).ToString("0.##") + "发每秒".Translate();
				}
				return ((float)this.prefabDesc.turretROF * 60f * (float)this.prefabDesc.turretMuzzleCount / (float)(this.prefabDesc.turretRoundInterval + this.prefabDesc.turretMuzzleInterval * (int)(this.prefabDesc.turretMuzzleCount - 1))).ToString("0.##") + "发每秒".Translate();
			}
			else
			{
				if (!this.prefabDesc.isCraftUnit)
				{
					return "";
				}
				float num11 = (this.prefabDesc.craftUnitMuzzleCount0 > 1) ? ((float)this.prefabDesc.craftUnitROF0 * 60f * (float)this.prefabDesc.craftUnitMuzzleCount0 / (float)(this.prefabDesc.craftUnitRoundInterval0 + this.prefabDesc.craftUnitMuzzleInterval0 * (this.prefabDesc.craftUnitMuzzleCount0 - 1))) : ((float)this.prefabDesc.craftUnitROF0 * 60f / (float)this.prefabDesc.craftUnitRoundInterval0);
				GameHistoryData history3 = GameMain.history;
				if (history3 == null)
				{
					return num11.ToString("0.##") + "发每秒".Translate();
				}
				float num12 = this.isSpaceCraft ? (num11 * history3.combatShipROFRatio - num11) : (num11 * history3.combatDroneROFRatio - num11);
				if (num12 != 0f)
				{
					return string.Concat(new string[]
					{
						num11.ToString("0.##"),
						"<color=#61D8FFB8> + ",
						num12.ToString("0.##"),
						"</color>",
						"发每秒".Translate()
					});
				}
				return num11.ToString("0.##") + "发每秒".Translate();
			}
			break;
		case 50:
			return (100f * this.prefabDesc.turretDamageScale * GameMain.history.energyDamageScale * 0.6f).ToString("0.0#") + " hp";
		case 51:
			switch (this.prefabDesc.turretType)
			{
			case ETurretType.Gauss:
				return "动能武器".Translate();
			case ETurretType.Laser:
				return "能量武器".Translate();
			case ETurretType.Cannon:
				return "动能加爆破武器".Translate();
			case ETurretType.Plasma:
				return "能量武器".Translate();
			case ETurretType.Missile:
				return "爆破武器".Translate();
			case ETurretType.LocalPlasma:
				return "能量武器".Translate();
			case ETurretType.Disturb:
				return "电磁武器".Translate();
			default:
				return "-";
			}
			break;
		case 52:
			switch (this.prefabDesc.isTurret ? this.prefabDesc.turretAmmoType : this.AmmoType)
			{
			case EAmmoType.Bullet:
				return "子弹".Translate();
			case EAmmoType.Cannon:
				return "炮弹".Translate();
			case EAmmoType.Plasma:
				return "能量胶囊".Translate();
			case EAmmoType.Missile:
				return "导弹".Translate();
			case EAmmoType.EMCapsule:
				return "电磁胶囊".Translate();
			}
			return "-";
		case 53:
		{
			ModelProto modelProto = LDB.models.modelArray[this.ModelIndex];
			if (modelProto == null)
			{
				return "-";
			}
			int num13 = modelProto.HpMax;
			GameHistoryData history4 = GameMain.history;
			if (history4 == null)
			{
				return (num13 / 100).ToString() + " hp";
			}
			double num14 = 1.0 + (double)history4.globalHpEnhancement;
			num14 = (double)((int)(num14 * 1000.0 + 0.5)) / 1000.0;
			double num16;
			if (this.prefabDesc.isCraftUnit)
			{
				double num15 = this.isSpaceCraft ? ((double)history4.combatShipDurabilityRatio * num14) : ((double)history4.combatDroneDurabilityRatio * num14);
				num16 = (this.isSpaceCraft ? ((double)num13 * num15 - (double)num13) : ((double)num13 * num15 - (double)num13));
			}
			else
			{
				num16 = (double)num13 * num14 - (double)num13;
			}
			num13 /= 100;
			num16 /= 100.0;
			if (num16 != 0.0)
			{
				return num13.ToString("0.#") + "<color=#61D8FFB8> + " + num16.ToString("0.##") + "</color> hp";
			}
			return num13.ToString("0.#") + " hp";
		}
		case 54:
			if (incLevel == 0)
			{
				return "+" + this.Ability.ToString() + "%";
			}
			return "<color=#FD965EB8>+" + ((double)this.Ability * num2).ToString() + "%</color>";
		case 55:
			if (this.isAmmo)
			{
				switch (this.AmmoType)
				{
				case EAmmoType.Bullet:
					return "子弹伤害类型".Translate();
				case EAmmoType.Cannon:
					return "炮弹伤害类型".Translate();
				case EAmmoType.Plasma:
					return "能量胶囊伤害类型".Translate();
				case EAmmoType.Missile:
					return "导弹伤害类型".Translate();
				}
				return "-";
			}
			if (this.isBomb)
			{
				if (this.BombType == EBombType.ExplosiveUnit)
				{
					return "炸药伤害类型".Translate();
				}
				return "-";
			}
			else
			{
				if (this.prefabDesc.isCraftUnit)
				{
					return "战斗无人机伤害类型".Translate();
				}
				return "-";
			}
			break;
		case 56:
			if ((this.prefabDesc.turretVSCaps & VSLayerMask.GroundAndAirAndSpace) == VSLayerMask.GroundAndAirAndSpace)
			{
				return "对地且对天".Translate();
			}
			if ((this.prefabDesc.turretVSCaps & VSLayerMask.SpaceHigh) <= VSLayerMask.None)
			{
				return "对地".Translate();
			}
			return "对天".Translate();
		case 57:
			if (this.prefabDesc.turretMinAttackRange != 0f)
			{
				return this.prefabDesc.turretMinAttackRange.ToString() + " ~ " + this.prefabDesc.turretMaxAttackRange.ToString() + " m";
			}
			return this.prefabDesc.turretMaxAttackRange.ToString() + " m";
		case 58:
			return this.prefabDesc.turretSpaceAttackRange.ToString() + " m";
		case 59:
			return this.prefabDesc.craftUnitMaxMovementSpeed.ToString() + " m/s";
		case 60:
			return this.prefabDesc.craftUnitAttackRange0.ToString() + " m";
		case 61:
			if (this.AmmoType == EAmmoType.Cannon)
			{
				return this.prefabDesc.AmmoBlastRadius1.ToString() + " m";
			}
			if (this.AmmoType == EAmmoType.Plasma)
			{
				if (this.ID == 1607)
				{
					return SkillSystem.plasmaDamageRadius1.ToString() + " m";
				}
				if (this.ID == 1608)
				{
					return SkillSystem.antimatterDamageRadius1.ToString() + " m";
				}
				return "-";
			}
			else
			{
				if (this.AmmoType == EAmmoType.Missile)
				{
					return this.prefabDesc.AmmoBlastRadius1.ToString() + " m";
				}
				if (this.BombType == EBombType.ExplosiveUnit)
				{
					return this.prefabDesc.AmmoBlastRadius1.ToString() + " m";
				}
				return "-";
			}
			break;
		case 62:
		{
			if (!this.prefabDesc.isCraftUnit)
			{
				return "";
			}
			int num17 = this.prefabDesc.craftUnitAttackDamage1;
			GameHistoryData history5 = GameMain.history;
			if (history5 == null)
			{
				return (num17 / 100).ToString("0.0") + " hp";
			}
			double num18 = (double)((float)num17 * history5.energyDamageScale);
			num18 = (this.isSpaceCraft ? (num18 * (double)history5.combatShipDamageRatio - (double)num17) : (num18 * (double)history5.combatDroneDamageRatio - (double)num17));
			num17 /= 100;
			num18 /= 100.0;
			if (num18 != 0.0)
			{
				return num17.ToString("0.0") + "<color=#61D8FFB8> + " + num18.ToString("0.0#") + "</color> hp";
			}
			return num17.ToString("0.0") + " hp";
		}
		case 63:
		{
			if (!this.prefabDesc.isCraftUnit)
			{
				return "";
			}
			float num19 = (this.prefabDesc.craftUnitMuzzleCount1 > 1) ? ((float)this.prefabDesc.craftUnitROF1 * 60f * (float)this.prefabDesc.craftUnitMuzzleCount1 / (float)(this.prefabDesc.craftUnitRoundInterval1 + this.prefabDesc.craftUnitMuzzleInterval1 * (this.prefabDesc.craftUnitMuzzleCount1 - 1))) : ((float)this.prefabDesc.craftUnitROF1 * 60f / (float)this.prefabDesc.craftUnitRoundInterval1);
			GameHistoryData history6 = GameMain.history;
			if (history6 == null)
			{
				return num19.ToString("0.##") + "发每秒".Translate();
			}
			float num20 = this.isSpaceCraft ? (num19 * history6.combatShipROFRatio - num19) : (num19 * history6.combatDroneROFRatio - num19);
			if (num20 != 0f)
			{
				return string.Concat(new string[]
				{
					num19.ToString("0.##"),
					"<color=#61D8FFB8> + ",
					num20.ToString("0.##"),
					"</color>",
					"发每秒".Translate()
				});
			}
			return num19.ToString("0.##") + "发每秒".Translate();
		}
		case 64:
			return this.prefabDesc.craftUnitAttackRange1.ToString() + " m";
		case 65:
		{
			if (!this.prefabDesc.isCraftUnit)
			{
				return "";
			}
			int num21 = this.prefabDesc.craftUnitAttackDamage0;
			GameHistoryData history7 = GameMain.history;
			if (history7 == null)
			{
				return (num21 / 100).ToString("0.0") + " hp";
			}
			double num22 = (double)((float)num21 * history7.energyDamageScale);
			num22 = (this.isSpaceCraft ? (num22 * (double)history7.combatShipDamageRatio - (double)num21) : (num22 * (double)history7.combatDroneDamageRatio - (double)num21));
			num21 /= 100;
			num22 /= 100.0;
			if (num22 != 0.0)
			{
				return num21.ToString("0.0") + "<color=#61D8FFB8> + " + num22.ToString("0.0#") + "</color> hp";
			}
			return num21.ToString("0.0") + " hp";
		}
		case 66:
		{
			if (!this.prefabDesc.isCraftUnit)
			{
				return "";
			}
			float num23 = (this.prefabDesc.craftUnitMuzzleCount0 > 1) ? ((float)this.prefabDesc.craftUnitROF0 * 60f * (float)this.prefabDesc.craftUnitMuzzleCount0 / (float)(this.prefabDesc.craftUnitRoundInterval0 + this.prefabDesc.craftUnitMuzzleInterval0 * (this.prefabDesc.craftUnitMuzzleCount0 - 1))) : ((float)this.prefabDesc.craftUnitROF0 * 60f / (float)this.prefabDesc.craftUnitRoundInterval0);
			GameHistoryData history8 = GameMain.history;
			if (history8 == null)
			{
				return num23.ToString("0.##") + "发每秒".Translate();
			}
			float num24 = this.isSpaceCraft ? (num23 * history8.combatShipROFRatio - num23) : (num23 * history8.combatDroneROFRatio - num23);
			if (num24 != 0f)
			{
				return string.Concat(new string[]
				{
					num23.ToString("0.##"),
					"<color=#61D8FFB8> + ",
					num24.ToString("0.##"),
					"</color>",
					"发每秒".Translate()
				});
			}
			return num23.ToString("0.##") + "发每秒".Translate();
		}
		case 67:
			return this.prefabDesc.craftUnitAttackRange0.ToString() + " m";
		case 68:
		{
			int num25 = (int)((double)this.HpMax * num + 0.1);
			string text2 = (this.AmmoType > EAmmoType.None) ? "干扰胶囊数量单位".Translate() : "";
			if (incLevel > 0)
			{
				return "<color=#61D8FFB8>" + num25.ToString() + "</color>" + text2;
			}
			return this.HpMax.ToString() + text2;
		}
		case 69:
		{
			int ability = this.Ability;
			GameHistoryData history9 = GameMain.history;
			if (history9 == null)
			{
				return "干扰胶囊效果".Translate() + " " + ability.ToString() + "%";
			}
			double num26 = (double)((float)ability * history9.magneticDamageScale - (float)ability);
			if (num26 != 0.0)
			{
				return string.Concat(new string[]
				{
					"干扰胶囊效果".Translate(),
					" ",
					ability.ToString(),
					"%<color=#61D8FFB8> + ",
					num26.ToString("0.##"),
					"%</color>"
				});
			}
			return "干扰胶囊效果".Translate() + " " + ability.ToString() + "%";
		}
		case 70:
		{
			double num27 = (double)this.Ability;
			GameHistoryData history10 = GameMain.history;
			if (history10 == null)
			{
				num27 /= 100.0;
				return "干扰胶囊持续时间".Translate() + " " + num27.ToString("0.##") + "s";
			}
			double num28 = num27 * (double)history10.magneticDamageScale - num27;
			num27 /= 50.0;
			num28 /= 50.0;
			if (num28 != 0.0)
			{
				return string.Concat(new string[]
				{
					"干扰胶囊持续时间".Translate(),
					" ",
					num27.ToString("0.##"),
					"s<color=#61D8FFB8> + ",
					num28.ToString("0.##"),
					"s</color>"
				});
			}
			return "干扰胶囊持续时间".Translate() + " " + num27.ToString("0.##") + "s";
		}
		case 71:
			if (this.BombType == EBombType.EMCapusle)
			{
				int ability2 = this.Ability;
				GameHistoryData history11 = GameMain.history;
				if (history11 == null)
				{
					return "干扰胶囊效果".Translate() + " " + ability2.ToString() + "%";
				}
				double num29 = (double)((float)ability2 * history11.magneticDamageScale - (float)ability2);
				if (num29 != 0.0)
				{
					return string.Concat(new string[]
					{
						"干扰胶囊效果".Translate(),
						" ",
						ability2.ToString(),
						"%<color=#61D8FFB8> + ",
						num29.ToString("0.##"),
						"%</color>"
					});
				}
				return "干扰胶囊效果".Translate() + " " + ability2.ToString() + "%";
			}
			else if (this.BombType == EBombType.ExplosiveUnit)
			{
				int num30 = this.Ability;
				GameHistoryData history12 = GameMain.history;
				if (history12 == null)
				{
					num30 /= 100;
					return "伤害".Translate() + " " + num30.ToString("0.0") + " hp";
				}
				double num31 = (double)((float)num30 * history12.blastDamageScale - (float)num30);
				num30 /= 100;
				num31 /= 100.0;
				if (num31 != 0.0)
				{
					return string.Concat(new string[]
					{
						"伤害".Translate(),
						" ",
						num30.ToString("0.0"),
						"<color=#61D8FFB8> + ",
						num31.ToString("0.0#"),
						"</color> hp"
					});
				}
				return "伤害".Translate() + " " + num30.ToString("0.0") + " hp";
			}
			else
			{
				if (this.BombType == EBombType.Liquid)
				{
					return "投掷无效果".Translate();
				}
				return "-";
			}
			break;
		case 72:
		{
			int num32 = (this.prefabDesc.inserterGrade == 4) ? GameMain.history.inserterStackInput : 1;
			if (num32 <= 1)
			{
				return "不支持".Translate();
			}
			if (num32 >= 4)
			{
				return "<color=#61D8FFB8>" + num32.ToString() + "</color>";
			}
			return "<color=#61D8FFB8>" + num32.ToString() + "</color>" + "可升级".Translate();
		}
		case 73:
		{
			int num33 = (this.prefabDesc.inserterGrade == 4) ? GameMain.history.inserterStackOutput : 0;
			if (num33 <= 0)
			{
				return "不支持".Translate();
			}
			if (num33 >= 4)
			{
				return "<color=#61D8FFB8>" + num33.ToString() + "</color>";
			}
			return "<color=#61D8FFB8>" + num33.ToString() + "</color>" + "可升级".Translate();
		}
		default:
			return "-";
		}
	}

	// Token: 0x04002A3C RID: 10812
	public EItemType Type;

	// Token: 0x04002A3D RID: 10813
	public int SubID;

	// Token: 0x04002A3E RID: 10814
	public string MiningFrom;

	// Token: 0x04002A3F RID: 10815
	public string ProduceFrom;

	// Token: 0x04002A40 RID: 10816
	public int StackSize;

	// Token: 0x04002A41 RID: 10817
	public int Grade;

	// Token: 0x04002A42 RID: 10818
	public int[] Upgrades;

	// Token: 0x04002A43 RID: 10819
	public bool IsFluid;

	// Token: 0x04002A44 RID: 10820
	public bool IsEntity;

	// Token: 0x04002A45 RID: 10821
	public bool CanBuild;

	// Token: 0x04002A46 RID: 10822
	public bool BuildInGas;

	// Token: 0x04002A47 RID: 10823
	public string IconPath;

	// Token: 0x04002A48 RID: 10824
	public string IconTag;

	// Token: 0x04002A49 RID: 10825
	public int ModelIndex;

	// Token: 0x04002A4A RID: 10826
	public int ModelCount;

	// Token: 0x04002A4B RID: 10827
	public int HpMax;

	// Token: 0x04002A4C RID: 10828
	public int Ability;

	// Token: 0x04002A4D RID: 10829
	public long HeatValue;

	// Token: 0x04002A4E RID: 10830
	public long Potential;

	// Token: 0x04002A4F RID: 10831
	public float ReactorInc;

	// Token: 0x04002A50 RID: 10832
	public int FuelType;

	// Token: 0x04002A51 RID: 10833
	public EAmmoType AmmoType;

	// Token: 0x04002A52 RID: 10834
	public EBombType BombType;

	// Token: 0x04002A53 RID: 10835
	public int CraftType;

	// Token: 0x04002A54 RID: 10836
	public int BuildIndex;

	// Token: 0x04002A55 RID: 10837
	public int BuildMode;

	// Token: 0x04002A56 RID: 10838
	public int GridIndex;

	// Token: 0x04002A57 RID: 10839
	public int UnlockKey;

	// Token: 0x04002A58 RID: 10840
	public int PreTechOverride;

	// Token: 0x04002A59 RID: 10841
	public bool Productive;

	// Token: 0x04002A5A RID: 10842
	public int MechaMaterialID;

	// Token: 0x04002A5B RID: 10843
	public float DropRate;

	// Token: 0x04002A5C RID: 10844
	public int EnemyDropLevel;

	// Token: 0x04002A5D RID: 10845
	public Vector2 EnemyDropRange;

	// Token: 0x04002A5E RID: 10846
	public float EnemyDropCount;

	// Token: 0x04002A5F RID: 10847
	public int EnemyDropMask;

	// Token: 0x04002A60 RID: 10848
	public float EnemyDropMaskRatio;

	// Token: 0x04002A61 RID: 10849
	public int[] DescFields;

	// Token: 0x04002A62 RID: 10850
	public string Description;

	// Token: 0x04002A66 RID: 10854
	public string iconTagString;

	// Token: 0x04002A67 RID: 10855
	public string iconNameTagString;

	// Token: 0x04002A69 RID: 10857
	[NonSerialized]
	public bool isRaw;

	// Token: 0x04002A6A RID: 10858
	[NonSerialized]
	public RecipeProto handcraft;

	// Token: 0x04002A6B RID: 10859
	[NonSerialized]
	public RecipeProto maincraft;

	// Token: 0x04002A6C RID: 10860
	[NonSerialized]
	public int handcraftProductCount;

	// Token: 0x04002A6D RID: 10861
	[NonSerialized]
	public int maincraftProductCount;

	// Token: 0x04002A6E RID: 10862
	[NonSerialized]
	public List<RecipeProto> handcrafts;

	// Token: 0x04002A6F RID: 10863
	[NonSerialized]
	public List<RecipeProto> recipes;

	// Token: 0x04002A70 RID: 10864
	[NonSerialized]
	public List<RecipeProto> makes;

	// Token: 0x04002A71 RID: 10865
	[NonSerialized]
	public List<IDCNTINC> rawMats;

	// Token: 0x04002A72 RID: 10866
	[NonSerialized]
	public TechProto preTech;

	// Token: 0x04002A73 RID: 10867
	[NonSerialized]
	public bool missingTech;

	// Token: 0x04002A74 RID: 10868
	[NonSerialized]
	public int productionMask;

	// Token: 0x04002A75 RID: 10869
	[NonSerialized]
	public int consumptionMask;

	// Token: 0x04002A76 RID: 10870
	public const int kMaxProtoId = 12000;

	// Token: 0x04002A77 RID: 10871
	public const int kMaxSubId = 256;

	// Token: 0x04002A78 RID: 10872
	public const int kMaxProtoIndex = 9999;

	// Token: 0x04002A79 RID: 10873
	public const int kWaterId = 1000;

	// Token: 0x04002A7A RID: 10874
	public const int kDroneId = 5001;

	// Token: 0x04002A7B RID: 10875
	public const int kShipId = 5002;

	// Token: 0x04002A7C RID: 10876
	public const int kCourierId = 5003;

	// Token: 0x04002A7D RID: 10877
	public const int kConstructionDroneModelIndex = 454;

	// Token: 0x04002A7E RID: 10878
	public const int kRocketId = 1503;

	// Token: 0x04002A7F RID: 10879
	public const int kWarperId = 1210;

	// Token: 0x04002A80 RID: 10880
	public const int kVeinMinerId = 2301;

	// Token: 0x04002A81 RID: 10881
	public const int kOilMinerId = 2307;

	// Token: 0x04002A82 RID: 10882
	public const int kSpraycoaterId = 2313;

	// Token: 0x04002A83 RID: 10883
	public const int kAccumulator = 2206;

	// Token: 0x04002A84 RID: 10884
	public const int kAccumulatorFull = 2207;

	// Token: 0x04002A85 RID: 10885
	public const int kSoilPileId = 1099;

	// Token: 0x04002A86 RID: 10886
	public const int kTerrainId = 1131;

	// Token: 0x04002A87 RID: 10887
	public const int kMarkerId = 2401;

	// Token: 0x04002A88 RID: 10888
	public const int kAmmoPlasma = 1607;

	// Token: 0x04002A89 RID: 10889
	public const int kAmmoAntimatter = 1608;

	// Token: 0x04002A8A RID: 10890
	public const int kDFMemoryUnitId = 5201;

	// Token: 0x04002A8B RID: 10891
	public const int kDFSiliconNeuronId = 5202;

	// Token: 0x04002A8C RID: 10892
	public const int kDFReassemblerId = 5203;

	// Token: 0x04002A8D RID: 10893
	public const int kDFSingularityId = 5204;

	// Token: 0x04002A8E RID: 10894
	public const int kDFVirtualParticleId = 5205;

	// Token: 0x04002A8F RID: 10895
	public const int kDFEnergyFragment = 5206;

	// Token: 0x04002A90 RID: 10896
	public static int stationCollectorId;

	// Token: 0x04002A91 RID: 10897
	public static int[] kFuelAutoReplenishIds = new int[]
	{
		1804,
		1803,
		1802,
		1801,
		1130,
		1129,
		1128,
		1121,
		1120,
		1109,
		1011,
		1114,
		1007,
		1006,
		1117,
		1030,
		1031
	};

	// Token: 0x04002A92 RID: 10898
	private Sprite _iconSprite;

	// Token: 0x04002A93 RID: 10899
	private Sprite _propertyIconSprite;

	// Token: 0x04002A94 RID: 10900
	private Sprite _propertyIconSpriteSmall;

	// Token: 0x04002A96 RID: 10902
	[NonSerialized]
	public PrefabDesc prefabDesc;

	// Token: 0x04002A97 RID: 10903
	public static int[][] fuelNeeds = new int[64][];

	// Token: 0x04002A98 RID: 10904
	public static int[][] turretNeeds = new int[16][];

	// Token: 0x04002A99 RID: 10905
	public static int[] fluids;

	// Token: 0x04002A9A RID: 10906
	public static int[] turrets;

	// Token: 0x04002A9B RID: 10907
	public static int[] enemyDropRangeTable;

	// Token: 0x04002A9C RID: 10908
	public static int[] enemyDropLevelTable;

	// Token: 0x04002A9D RID: 10909
	public static float[] enemyDropCountTable;

	// Token: 0x04002A9E RID: 10910
	public static int[] enemyDropMaskTable;

	// Token: 0x04002A9F RID: 10911
	public static float[] enemyDropMaskRatioTable;

	// Token: 0x04002AA0 RID: 10912
	public static int constructableCount;

	// Token: 0x04002AA1 RID: 10913
	public static HashSet<int> constructableIdHash;

	// Token: 0x04002AA2 RID: 10914
	public static int[] constructableIds;

	// Token: 0x04002AA3 RID: 10915
	public static int[] constructableIndiceById;

	// Token: 0x04002AA4 RID: 10916
	public static ItemProto[] itemProtoById;

	// Token: 0x04002AA5 RID: 10917
	public static int[] itemIds;

	// Token: 0x04002AA6 RID: 10918
	public static int[] itemIndices;

	// Token: 0x04002AA7 RID: 10919
	public static int[] mechaMaterials;

	// Token: 0x04002AA8 RID: 10920
	public const int kFighterIdRangeMin = 5100;

	// Token: 0x04002AA9 RID: 10921
	public const int kFighterIdRangeMax = 5199;

	// Token: 0x04002AAA RID: 10922
	public static int[] kFighterIds;

	// Token: 0x04002AAB RID: 10923
	public static int[] kFighterGroundIds;

	// Token: 0x04002AAC RID: 10924
	public static int[] kFighterSpaceIds;

	// Token: 0x04002AAD RID: 10925
	public static int[] kFighterSpaceSmallIds;

	// Token: 0x04002AAE RID: 10926
	public static int[] kFighterSpaceLargeIds;

	// Token: 0x04002AAF RID: 10927
	public static int[] powerGenIndex2Id;

	// Token: 0x04002AB0 RID: 10928
	public static int[] powerConIndex2Id;

	// Token: 0x04002AB1 RID: 10929
	public static int[] powerGenId2Index;

	// Token: 0x04002AB2 RID: 10930
	public static int[] powerConId2Index;
}
