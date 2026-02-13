using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x020006D4 RID: 1748
public class UIBattleBaseWindow : ManualBehaviour
{
	// Token: 0x17000715 RID: 1813
	// (get) Token: 0x0600457D RID: 17789 RVA: 0x003889AC File Offset: 0x00386BAC
	// (set) Token: 0x0600457E RID: 17790 RVA: 0x003889B4 File Offset: 0x00386BB4
	public int battleBaseId
	{
		get
		{
			return this._battleBaseId;
		}
		set
		{
			if (this._battleBaseId != value)
			{
				this._battleBaseId = value;
				this.OnBattleBaseIdChange();
			}
		}
	}

	// Token: 0x17000716 RID: 1814
	// (get) Token: 0x0600457F RID: 17791 RVA: 0x003889CC File Offset: 0x00386BCC
	// (set) Token: 0x06004580 RID: 17792 RVA: 0x003889D4 File Offset: 0x00386BD4
	public int fleetConfigIndex
	{
		get
		{
			return this._fleetConfigIndex;
		}
		set
		{
			int num = value;
			if (!base.active)
			{
				num = -1;
			}
			if (num != this._fleetConfigIndex)
			{
				if (this._fleetConfigIndex >= 0)
				{
					this.CloseConfigPanel();
				}
				this._fleetConfigIndex = num;
				if (this._fleetConfigIndex >= 0)
				{
					this.OpenConfigPanel();
				}
			}
		}
	}

	// Token: 0x06004581 RID: 17793 RVA: 0x00388A1B File Offset: 0x00386C1B
	protected override void _OnCreate()
	{
		this.storageGrid._Create();
		this.fighterEntries = new List<UIBattleBaseFighterEntry>();
		this.fleetTypeButtons = new UIButton[4];
	}

	// Token: 0x06004582 RID: 17794 RVA: 0x00388A3F File Offset: 0x00386C3F
	protected override void _OnDestroy()
	{
		this.storageGrid._Destroy();
		if (this.fighterEntries != null)
		{
			this.fighterEntries = null;
		}
		this.fleetTypeButtons = null;
	}

	// Token: 0x06004583 RID: 17795 RVA: 0x00388A62 File Offset: 0x00386C62
	protected override bool _OnInit()
	{
		this.powerServedSB = new StringBuilder("         W", 12);
		this.powerAccumulatedSB = new StringBuilder("         J", 12);
		return true;
	}

	// Token: 0x06004584 RID: 17796 RVA: 0x00388A89 File Offset: 0x00386C89
	protected override void _OnFree()
	{
	}

	// Token: 0x06004585 RID: 17797 RVA: 0x00388A8C File Offset: 0x00386C8C
	protected override void _OnRegEvent()
	{
		this.bansSlider.onValueChanged.AddListener(new UnityAction<float>(this.OnBansSliderValueChange));
		this.chargePowerSlider.onValueChanged.AddListener(new UnityAction<float>(this.OnMaxChargePowerSliderChange));
		this.filterButton0.onClick += this.OnFilterButton0Click;
		this.filterButton1.onClick += this.OnFilterButton1Click;
		this.filterButton2.onClick += this.OnFilterButton2Click;
		this.filterButton3.onClick += this.OnFilterButton3Click;
		this.loopFilterButton.onClick += this.OnLoopFilterButtonClick;
		this.autoReplenishFleetButton.onClick += this.OnAutoReplenishButtonClick;
		this.configBtn.onMouseDown += this.OnConfigButtonClick;
		this.droneButton.onClick += this.OnDroneButtonClick;
		this.dronePriorityButton.onClick += this.OnDronePriorityButtonClick;
		this.autoReconstructButton.onClick += this.OnAutoReconstructButtonClick;
		this.autoPickButton.onClick += this.OnAutoPickButtonClick;
		this.fleetButton.onClick += this.OnFleetButtonClick;
	}

	// Token: 0x06004586 RID: 17798 RVA: 0x00388BE8 File Offset: 0x00386DE8
	protected override void _OnUnregEvent()
	{
		this.bansSlider.onValueChanged.RemoveAllListeners();
		this.chargePowerSlider.onValueChanged.RemoveAllListeners();
		this.filterButton0.onClick -= this.OnFilterButton0Click;
		this.filterButton1.onClick -= this.OnFilterButton1Click;
		this.filterButton2.onClick -= this.OnFilterButton2Click;
		this.filterButton3.onClick -= this.OnFilterButton3Click;
		this.loopFilterButton.onClick -= this.OnLoopFilterButtonClick;
		this.autoReplenishFleetButton.onClick -= this.OnAutoReplenishButtonClick;
		this.configBtn.onMouseDown -= this.OnConfigButtonClick;
		this.droneButton.onClick -= this.OnDroneButtonClick;
		this.dronePriorityButton.onClick -= this.OnDronePriorityButtonClick;
		this.autoReconstructButton.onClick -= this.OnAutoReconstructButtonClick;
		this.autoPickButton.onClick -= this.OnAutoPickButtonClick;
		this.fleetButton.onClick -= this.OnFleetButtonClick;
	}

	// Token: 0x06004587 RID: 17799 RVA: 0x00388D2C File Offset: 0x00386F2C
	protected override void _OnOpen()
	{
		if (GameMain.localPlanet != null && GameMain.localPlanet.factory != null)
		{
			this.factory = GameMain.localPlanet.factory;
			this.powerSystem = this.factory.powerSystem;
			this.defenseSystem = this.factory.defenseSystem;
			this.player = GameMain.mainPlayer;
			this.history = GameMain.history;
			this.OnBattleBaseIdChange();
			this.eventLock = true;
		}
		else
		{
			base._Close();
		}
		base.transform.SetAsLastSibling();
	}

	// Token: 0x06004588 RID: 17800 RVA: 0x00388DB4 File Offset: 0x00386FB4
	protected override void _OnClose()
	{
		this.eventLock = true;
		if (this.battleBaseId != 0)
		{
			PlayerAction_Inspect actionInspect = this.player.controller.actionInspect;
			if (actionInspect.inspectId > 0 && actionInspect.inspectType == EObjectType.None && this.factory.entityPool[actionInspect.inspectId].battleBaseId == this.battleBaseId)
			{
				actionInspect.InspectNothing();
			}
		}
		this.filterSettingsGroup.gameObject.SetActive(false);
		if (UILootFilter.isOpened)
		{
			UILootFilter.Close();
		}
		for (int i = 0; i < this.fighterEntries.Count; i++)
		{
			this.fighterEntries[i]._Free();
			this.fighterEntries[i]._Destroy();
		}
		this.fighterEntries.Clear();
		this.battleBaseId = 0;
		this.factory = null;
		this.powerSystem = null;
		this.defenseSystem = null;
		this.player = null;
		this.history = null;
		this.storageGrid._Free();
		this.battleBase = null;
		this.constructionModule = null;
		this.combatModule = null;
		this.storage = null;
		this.fleetConfigIndex = -1;
	}

	// Token: 0x06004589 RID: 17801 RVA: 0x00388ED3 File Offset: 0x003870D3
	protected override void _OnUpdate()
	{
		if (this.battleBaseId == 0 || this.factory == null)
		{
			base._Close();
			return;
		}
		if (this.battleBase.id != this.battleBaseId)
		{
			base._Close();
			return;
		}
		this.RefreshWindowComplete();
	}

	// Token: 0x0600458A RID: 17802 RVA: 0x00388F0C File Offset: 0x0038710C
	private void RefreshWindowComplete()
	{
		this.autoPickButton.highlighted = this.battleBase.autoPickEnabled;
		bool flag = this.history.autoReconstructSpeed > 0;
		if (flag)
		{
			this.autoReconstructButton.gameObject.SetActive(true);
			this.autoReconstructPlaceholder.gameObject.SetActive(false);
		}
		else
		{
			this.autoReconstructButton.gameObject.SetActive(false);
			this.autoReconstructPlaceholder.gameObject.SetActive(true);
		}
		if (flag)
		{
			this.autoReconstructButton.highlighted = this.constructionModule.autoReconstruct;
		}
		if (this.autoReconstructButton.highlighted || this.autoPickButton.highlighted)
		{
			this.ruinBg.color = this.fleetBgNormalColor;
		}
		else
		{
			this.ruinBg.color = this.fleetBgDisabledColor;
		}
		this.droneButton.highlighted = this.constructionModule.droneEnabled;
		this.dronePriorityButton.gameObject.SetActive(this.constructionModule.droneEnabled);
		this.RefreshDronePriorityButton();
		bool flag2 = false;
		for (int i = 0; i < ItemProto.kFighterIds.Length; i++)
		{
			if (this.history.ItemUnlocked(ItemProto.kFighterIds[i]))
			{
				flag2 = true;
				break;
			}
		}
		if (flag2)
		{
			this.fleetGroupPlaceholder.SetActive(false);
			this.fleetGroup.SetActive(true);
		}
		else
		{
			this.fleetGroupPlaceholder.SetActive(true);
			this.fleetGroup.SetActive(false);
		}
		if (flag2)
		{
			bool moduleEnabled = this.combatModule.moduleEnabled;
			if (this.combatModule.moduleFleets[0].fleetId > 0)
			{
				this.fleetBg.color = (moduleEnabled ? this.fleetBgEngagingColor : this.fleetBgRetreatingColor);
				this.fleetStateText.text = (moduleEnabled ? "基站战斗机出击中".Translate() : "基站战斗机召回中".Translate());
				this.fleetStateText.color = (moduleEnabled ? this.fleetStateEngagingColor : this.fleetStateRetreatingColor);
			}
			else
			{
				this.fleetBg.color = (moduleEnabled ? this.fleetBgNormalColor : this.fleetBgDisabledColor);
				this.fleetStateText.text = "";
			}
			this.fleetButton.highlighted = moduleEnabled;
			int groundFleetPortCount = this.history.groundFleetPortCount;
			this.fighterCountText.text = string.Format("{0} / {1}", this.combatModule.moduleFleets[0].fighterCount, groundFleetPortCount);
			this.autoReplenishFleetButton.highlighted = this.combatModule.autoReplenishFleet;
			for (int j = 0; j < this.fighterEntries.Count; j++)
			{
				this.fighterEntries[j].unlockedFighterCount = groundFleetPortCount;
				this.fighterEntries[j]._Update();
			}
			if (this.combatModule.moduleFleets[0].fleetId > 0)
			{
				this.configBtn.button.interactable = false;
				if (this.fleetConfigIndex == 0)
				{
					this.fleetConfigIndex = -1;
				}
			}
			else
			{
				this.configBtn.button.interactable = true;
			}
			if (this.fleetConfigGroupRectTrans.gameObject.activeSelf && this.fleetConfigFrame > 0 && (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Mouse2)))
			{
				Camera worldCamera = UIRoot.instance.overlayCanvas.worldCamera;
				if (!RectTransformUtility.RectangleContainsScreenPoint(this.fleetConfigGroupRectTrans, Input.mousePosition, worldCamera))
				{
					this.fleetConfigIndex = -1;
				}
			}
			if (this.fleetConfigGroupRectTrans.gameObject.activeSelf)
			{
				this.fleetConfigFrame++;
			}
			else
			{
				this.fleetConfigFrame = 0;
			}
		}
		bool activeSelf = this.filterSettingsGroup.gameObject.activeSelf;
		this.loopFilterRt.anchoredPosition = (activeSelf ? new Vector2(154f, 0f) : new Vector2(46f, 0f));
		this.loopFilterBgRt.anchoredPosition = (activeSelf ? new Vector2(154f, 0f) : new Vector2(46f, 0f));
		PowerConsumerComponent[] consumerPool = this.powerSystem.consumerPool;
		int pcId = this.battleBase.pcId;
		int networkId = consumerPool[pcId].networkId;
		PowerNetwork powerNetwork = this.powerSystem.netPool[networkId];
		float num = (powerNetwork != null && networkId > 0) ? ((float)powerNetwork.consumerRatio) : 0f;
		float num2 = (float)((double)this.battleBase.energy / (double)this.battleBase.energyMax);
		double num3 = (double)(consumerPool[pcId].requiredEnergy * 60L);
		long valuel = (long)(num3 * (double)num + 0.5);
		if (num > 0f)
		{
			StringBuilderUtility.WriteKMG(this.powerServedSB, 8, valuel, false, '\u2009', ' ');
			this.powerText.text = this.powerServedSB.ToString();
			if (num2 == 1f)
			{
				this.stateText.text = "已充满".Translate();
				this.stateText.color = this.idleColor;
			}
			else
			{
				this.stateText.text = ((num3 > 0.0) ? "充电中".Translate() : "待机中".Translate());
				this.stateText.color = ((num3 > 0.0) ? this.powerNormalColor : this.idleColor);
			}
		}
		else
		{
			this.powerText.text = "断电".Translate();
			this.stateText.text = "无法充电".Translate();
			this.stateText.color = this.powerOffColor;
		}
		StringBuilderUtility.WriteKMG(this.powerAccumulatedSB, 8, this.battleBase.energy, false, '\u2009', ' ');
		this.energyText.text = this.powerAccumulatedSB.ToString().TrimStart();
		this.powerIcon.color = ((num > 0f || num2 > 0.1f) ? this.powerNormalIconColor : this.powerOffIconColor);
		this.powerText.color = ((num > 0f) ? this.powerNormalColor : this.powerOffColor);
		this.energyBar.fillAmount = num2;
		float width = this.energyBar.rectTransform.rect.width;
		if ((double)num2 > 0.7)
		{
			this.energyText.rectTransform.anchoredPosition = new Vector2(Mathf.Round(width * num2 - 30f), 0f);
			this.energyText.color = this.energyTextColor1;
			this.energyBar.color = this.energyBarColorIn;
		}
		else
		{
			this.energyText.rectTransform.anchoredPosition = new Vector2(Mathf.Round(width * num2 + 30f), 0f);
			this.energyText.color = ((num2 < 0.12f) ? this.energyTextColor2 : this.energyTextColor0);
			this.energyBar.color = ((num2 < 0.12f) ? this.energyBarColorOut : this.energyBarColorIn);
		}
		string str = this.constructionModule.droneIdleCount.ToString();
		string str2 = this.constructionModule.droneCount.ToString();
		this.droneCountText.text = str + " / " + str2;
		this.eventLock = true;
		this.bansSlider.maxValue = (float)this.storage.size;
		this.bansSlider.value = (float)(this.storage.size - this.storage.bans);
		this.bansValueText.text = this.bansSlider.value.ToString();
		this.bansButton.tips.tipText = (this.history.TechUnlocked(1608) ? "自动化容量分隔提示1".Translate() : "自动化容量分隔提示".Translate());
		this.eventLock = false;
	}

	// Token: 0x0600458B RID: 17803 RVA: 0x00389714 File Offset: 0x00387914
	private void OnMaxChargePowerSliderChange(float arg0)
	{
		if (this.battleBaseId == 0 || this.factory == null)
		{
			return;
		}
		if (this.battleBase.id != this.battleBaseId)
		{
			return;
		}
		this.factory.powerSystem.consumerPool[this.battleBase.pcId].workEnergyPerTick = (long)(5000.0 * (double)arg0 + 0.5);
		StringBuilderUtility.WriteKMG(this.powerServedSB, 8, (long)(300000.0 * (double)arg0 + 0.5), false, '\u2009', ' ');
		this.chargePowerValue.text = this.powerServedSB.ToString();
	}

	// Token: 0x0600458C RID: 17804 RVA: 0x003897C4 File Offset: 0x003879C4
	private void OnBattleBaseIdChange()
	{
		if (base.active)
		{
			if (this.battleBaseId == 0 || this.factory == null)
			{
				base._Close();
				return;
			}
			this.battleBase = this.defenseSystem.battleBases.buffer[this.battleBaseId];
			this.constructionModule = this.battleBase.constructionModule;
			this.combatModule = this.battleBase.combatModule;
			this.storage = this.battleBase.storage;
			if (this.battleBase.id != this.battleBaseId)
			{
				base._Close();
				return;
			}
			this.menuButton.SetInfo(UIGenericMenuButton.EInfoType.Entity, this.factory.index, this.battleBase.entityId, 0, 0, 0);
			this.eventLock = true;
			ItemProto itemProto = LDB.items.Select((int)this.factory.entityPool[this.battleBase.entityId].protoId);
			this.titleText.text = itemProto.name;
			long workEnergyPerTick = itemProto.prefabDesc.workEnergyPerTick;
			long num = workEnergyPerTick * 5L;
			long num2 = workEnergyPerTick / 2L;
			long workEnergyPerTick2 = this.factory.powerSystem.consumerPool[this.battleBase.pcId].workEnergyPerTick;
			this.chargePowerSlider.maxValue = (float)(num / 5000L);
			this.chargePowerSlider.minValue = (float)(num2 / 5000L);
			this.chargePowerSlider.value = (float)(workEnergyPerTick2 / 5000L);
			StringBuilderUtility.WriteKMG(this.powerServedSB, 8, workEnergyPerTick2 * 60L, true, '\u2009', ' ');
			this.chargePowerValue.text = this.powerServedSB.ToString();
			this.storageGrid._Free();
			this.storageGrid._Init(this.storage);
			this.storageGrid._Open();
			this.storageGrid.OnStorageDataChanged();
			this.bansSlider.maxValue = (float)this.storage.size;
			this.bansSlider.value = (float)(this.storage.size - this.storage.bans);
			this.bansValueText.text = this.bansSlider.value.ToString();
			for (int i = 0; i < this.fighterEntries.Count; i++)
			{
				this.fighterEntries[i]._Free();
				this.fighterEntries[i]._Destroy();
			}
			this.fighterEntries.Clear();
			ModuleFighter[] fighters = this.combatModule.moduleFleets[0].fighters;
			for (int j = 0; j < fighters.Length; j++)
			{
				UIBattleBaseFighterEntry uibattleBaseFighterEntry = Object.Instantiate<UIBattleBaseFighterEntry>(this.fighterEntry, this.fighterEntry.transform.parent);
				uibattleBaseFighterEntry._Create();
				uibattleBaseFighterEntry.fighterIndex = j;
				uibattleBaseFighterEntry.battleBaseId = this.battleBaseId;
				uibattleBaseFighterEntry._Init(this.factory);
				uibattleBaseFighterEntry.SetTrans();
				uibattleBaseFighterEntry._Open();
				this.fighterEntries.Add(uibattleBaseFighterEntry);
			}
			this.RefreshWindowComplete();
			this.eventLock = false;
		}
	}

	// Token: 0x0600458D RID: 17805 RVA: 0x00389AD8 File Offset: 0x00387CD8
	public void OnDroneButtonClick(int obj)
	{
		this.constructionModule.droneEnabled = !this.constructionModule.droneEnabled;
		if (this.constructionModule.droneEnabled)
		{
			this.constructionModule.SearchBuildTargets(this.factory, this.player, true);
			this.constructionModule.SearchRepairTargets(this.factory, this.player, true);
		}
	}

	// Token: 0x0600458E RID: 17806 RVA: 0x00389B3C File Offset: 0x00387D3C
	public void OnDronePriorityButtonClick(int obj)
	{
		if (this.constructionModule.dronePriority == 0)
		{
			this.constructionModule.ChangeDronesPriority(this.factory, 1);
			return;
		}
		if (this.constructionModule.dronePriority == 1)
		{
			this.constructionModule.ChangeDronesPriority(this.factory, 2);
			return;
		}
		this.constructionModule.ChangeDronesPriority(this.factory, 0);
	}

	// Token: 0x0600458F RID: 17807 RVA: 0x00389B9C File Offset: 0x00387D9C
	public void OnFleetButtonClick(int obj)
	{
		this.combatModule.moduleEnabled = !this.combatModule.moduleEnabled;
	}

	// Token: 0x06004590 RID: 17808 RVA: 0x00389BB8 File Offset: 0x00387DB8
	private void RefreshDronePriorityButton()
	{
		if (this.constructionModule.dronePriority == 2)
		{
			this.dronePriorityButtonImage.sprite = this.priorRepair;
			this.dronePriorityButton.tips.tipTitle = "建设机优先修理标题".Translate();
			this.dronePriorityButton.tips.tipText = "建设机优先修理描述".Translate();
			this.dronePriorityButton.UpdateTip();
			return;
		}
		if (this.constructionModule.dronePriority == 0)
		{
			this.dronePriorityButtonImage.sprite = this.priorBalance;
			this.dronePriorityButton.tips.tipTitle = "建设机均衡建造标题".Translate();
			this.dronePriorityButton.tips.tipText = "建设机均衡建造描述".Translate();
			this.dronePriorityButton.UpdateTip();
			return;
		}
		this.dronePriorityButtonImage.sprite = this.priorConstruct;
		this.dronePriorityButton.tips.tipTitle = "建设机优先建造标题".Translate();
		this.dronePriorityButton.tips.tipText = "建设机优先建造描述".Translate();
		this.dronePriorityButton.UpdateTip();
	}

	// Token: 0x06004591 RID: 17809 RVA: 0x00389CD4 File Offset: 0x00387ED4
	public void OnAutoReconstructButtonClick(int obj)
	{
		this.constructionModule.autoReconstruct = !this.constructionModule.autoReconstruct;
		if (this.constructionModule.autoReconstruct)
		{
			PowerConsumerComponent[] consumerPool = this.powerSystem.consumerPool;
			int pcId = this.battleBase.pcId;
			int networkId = consumerPool[pcId].networkId;
			PowerNetwork powerNetwork = this.powerSystem.netPool[networkId];
			if (((powerNetwork != null && networkId > 0) ? ((float)powerNetwork.consumerRatio) : 0f) > 0f)
			{
				this.constructionModule.SearchAutoReconstructTargets(this.factory, this.player, true);
			}
		}
	}

	// Token: 0x06004592 RID: 17810 RVA: 0x00389D6C File Offset: 0x00387F6C
	public void OnAutoPickButtonClick(int obj)
	{
		this.battleBase.autoPickEnabled = !this.battleBase.autoPickEnabled;
	}

	// Token: 0x06004593 RID: 17811 RVA: 0x00389D87 File Offset: 0x00387F87
	public void OnConfigButtonClick(int obj, PointerEventData.InputButton mouseButton)
	{
		if (this.fleetConfigIndex == 0)
		{
			this.fleetConfigIndex = -1;
			return;
		}
		this.fleetConfigIndex = 0;
	}

	// Token: 0x06004594 RID: 17812 RVA: 0x00389DA0 File Offset: 0x00387FA0
	public void OnFleetTypeButtonClick(int obj)
	{
		this.combatModule.ChangeFleetConfig(0, ItemProto.kFighterGroundIds[obj], this.battleBase.storage, this.player);
		this.fleetConfigIndex = -1;
	}

	// Token: 0x06004595 RID: 17813 RVA: 0x00389DD0 File Offset: 0x00387FD0
	private void OpenConfigPanel()
	{
		int num = ItemProto.kFighterGroundIds.Length;
		for (int i = 0; i < num; i++)
		{
			UIButton uibutton = Object.Instantiate<UIButton>(this.groundFleetTypeButton, this.groundFleetTypeButton.transform.parent);
			uibutton.data = i;
			uibutton.tips.itemId = ItemProto.kFighterGroundIds[i];
			RectTransform rectTransform = uibutton.transform as RectTransform;
			rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x + (float)(i * 40), rectTransform.anchoredPosition.y);
			uibutton.gameObject.SetActive(true);
			uibutton.onClick += this.OnFleetTypeButtonClick;
			this.fleetTypeButtons[i] = uibutton;
			ItemProto itemProto = LDB.items.Select(ItemProto.kFighterGroundIds[i]);
			if (itemProto != null)
			{
				Sprite iconSprite = itemProto.iconSprite;
				Image image = uibutton.GetComponentsInChildren<Image>()[1];
				image.sprite = iconSprite;
				image.enabled = (iconSprite != null);
			}
		}
		this.fleetConfigFrame = 0;
		this.fleetConfigGroupRectTrans.gameObject.SetActive(true);
		this.fleetConfigGroupRectTrans.transform.SetAsLastSibling();
	}

	// Token: 0x06004596 RID: 17814 RVA: 0x00389EEC File Offset: 0x003880EC
	private void CloseConfigPanel()
	{
		for (int i = 0; i < this.fleetTypeButtons.Length; i++)
		{
			UIButton uibutton = this.fleetTypeButtons[i];
			if (uibutton != null)
			{
				uibutton.onClick -= this.OnFleetTypeButtonClick;
				Object.Destroy(uibutton.gameObject);
			}
		}
		this.fleetConfigGroupRectTrans.gameObject.SetActive(false);
		this.fleetConfigFrame = 0;
	}

	// Token: 0x06004597 RID: 17815 RVA: 0x00389F55 File Offset: 0x00388155
	public void OnAutoReplenishButtonClick(int obj)
	{
		this.combatModule.autoReplenishFleet = !this.combatModule.autoReplenishFleet;
	}

	// Token: 0x06004598 RID: 17816 RVA: 0x00389F70 File Offset: 0x00388170
	public void OnSortClick()
	{
		if (this.battleBaseId == 0 || this.factory == null)
		{
			return;
		}
		if (this.storage == null)
		{
			return;
		}
		if (this.storage.type == EStorageType.Default || this.storageGrid.sortableFilter)
		{
			this.storage.Sort(true);
		}
	}

	// Token: 0x06004599 RID: 17817 RVA: 0x00389FC0 File Offset: 0x003881C0
	public void OnBansSliderValueChange(float value)
	{
		if (this.eventLock)
		{
			return;
		}
		if (this.battleBaseId == 0 || this.factory == null)
		{
			return;
		}
		if (this.storage == null)
		{
			return;
		}
		if ((this.storage.type == EStorageType.Default || (this.storage.type == EStorageType.Filtered && this.storageGrid.editableFilter)) && this.storage.id > 0)
		{
			int bans = this.storage.bans;
			int num = Mathf.RoundToInt(this.bansSlider.maxValue - this.bansSlider.value);
			if (bans != num)
			{
				this.storage.SetBans(num);
			}
		}
	}

	// Token: 0x0600459A RID: 17818 RVA: 0x0038A05E File Offset: 0x0038825E
	private void OnFilterButton0Click(int obj)
	{
		this.filterSettingsGroup.gameObject.SetActive(!this.filterSettingsGroup.gameObject.activeSelf);
	}

	// Token: 0x0600459B RID: 17819 RVA: 0x0038A084 File Offset: 0x00388284
	private void OnFilterItemPickerReturn(ItemProto itemProto)
	{
		if (itemProto != null)
		{
			if (this.battleBaseId == 0 || this.factory == null)
			{
				return;
			}
			if (this.storage == null)
			{
				return;
			}
			this.storage.type = EStorageType.Filtered;
			for (int i = 0; i < this.storage.grids.Length; i++)
			{
				int itemId = this.storage.grids[i].itemId;
				if (this.storage.grids[i].count <= 0 || itemId <= 0 || itemId == itemProto.ID)
				{
					this.storage.SetFilter(i, itemProto.ID);
				}
			}
			this.storageGrid.OnStorageContentChanged();
		}
	}

	// Token: 0x0600459C RID: 17820 RVA: 0x0038A130 File Offset: 0x00388330
	private void OnFilterButton1Click(int obj)
	{
		UIItemPicker.Popup(this.windowTrans.anchoredPosition + new Vector2(-350f, 180f), new Action<ItemProto>(this.OnFilterItemPickerReturn));
		this.filterSettingsGroup.gameObject.SetActive(false);
	}

	// Token: 0x0600459D RID: 17821 RVA: 0x0038A180 File Offset: 0x00388380
	private void OnFilterButton2Click(int obj)
	{
		if (this.battleBaseId == 0 || this.factory == null)
		{
			return;
		}
		if (this.storage == null)
		{
			return;
		}
		this.storage.type = EStorageType.Filtered;
		for (int i = 0; i < this.storage.grids.Length; i++)
		{
			int itemId = this.storage.grids[i].itemId;
			if (itemId != 0)
			{
				this.storage.SetFilter(i, itemId);
			}
		}
		this.storageGrid.OnStorageContentChanged();
		this.filterSettingsGroup.gameObject.SetActive(false);
	}

	// Token: 0x0600459E RID: 17822 RVA: 0x0038A210 File Offset: 0x00388410
	private void OnFilterButton3Click(int obj)
	{
		if (this.battleBaseId == 0 || this.factory == null)
		{
			return;
		}
		if (this.storage == null)
		{
			return;
		}
		for (int i = 0; i < this.storage.grids.Length; i++)
		{
			this.storage.SetFilter(i, 0);
		}
		this.storage.type = EStorageType.Default;
		this.storageGrid.OnStorageContentChanged();
		this.filterSettingsGroup.gameObject.SetActive(false);
	}

	// Token: 0x0600459F RID: 17823 RVA: 0x0038A284 File Offset: 0x00388484
	public void OnLoopFilterButtonClick(int obj)
	{
		if (!UILootFilter.isOpened)
		{
			UILootFilter.Popup(this.windowTrans.anchoredPosition + new Vector2(-350f, 180f), true);
			return;
		}
		UILootFilter.Close();
	}

	// Token: 0x040052AD RID: 21165
	[SerializeField]
	public RectTransform windowTrans;

	// Token: 0x040052AE RID: 21166
	[SerializeField]
	public RectTransform fleetConfigGroupRectTrans;

	// Token: 0x040052AF RID: 21167
	[SerializeField]
	public UIGenericMenuButton menuButton;

	// Token: 0x040052B0 RID: 21168
	[SerializeField]
	public Text titleText;

	// Token: 0x040052B1 RID: 21169
	[Header("Modules Group")]
	[Header(" * ruin analysis")]
	[SerializeField]
	public Image ruinBg;

	// Token: 0x040052B2 RID: 21170
	[SerializeField]
	public UIButton autoReconstructButton;

	// Token: 0x040052B3 RID: 21171
	[SerializeField]
	public UIButton autoPickButton;

	// Token: 0x040052B4 RID: 21172
	[SerializeField]
	public GameObject autoReconstructPlaceholder;

	// Token: 0x040052B5 RID: 21173
	[Header(" * drone")]
	[SerializeField]
	public Text droneCountText;

	// Token: 0x040052B6 RID: 21174
	[SerializeField]
	public Image droneBg;

	// Token: 0x040052B7 RID: 21175
	[SerializeField]
	public Image droneIcon;

	// Token: 0x040052B8 RID: 21176
	[SerializeField]
	public UIButton droneButton;

	// Token: 0x040052B9 RID: 21177
	[SerializeField]
	public UIButton dronePriorityButton;

	// Token: 0x040052BA RID: 21178
	[SerializeField]
	public Image dronePriorityButtonImage;

	// Token: 0x040052BB RID: 21179
	[SerializeField]
	public Sprite priorConstruct;

	// Token: 0x040052BC RID: 21180
	[SerializeField]
	public Sprite priorRepair;

	// Token: 0x040052BD RID: 21181
	[SerializeField]
	public Sprite priorBalance;

	// Token: 0x040052BE RID: 21182
	[Header(" * fleet")]
	[SerializeField]
	public Text fighterCountText;

	// Token: 0x040052BF RID: 21183
	[SerializeField]
	public Text fleetStateText;

	// Token: 0x040052C0 RID: 21184
	[SerializeField]
	public Image fleetBg;

	// Token: 0x040052C1 RID: 21185
	[SerializeField]
	public GameObject fleetGroup;

	// Token: 0x040052C2 RID: 21186
	[SerializeField]
	public GameObject fleetGroupPlaceholder;

	// Token: 0x040052C3 RID: 21187
	[SerializeField]
	public UIBattleBaseFighterEntry fighterEntry;

	// Token: 0x040052C4 RID: 21188
	[SerializeField]
	public UIButton fleetButton;

	// Token: 0x040052C5 RID: 21189
	[SerializeField]
	public UIButton groundFleetTypeButton;

	// Token: 0x040052C6 RID: 21190
	[SerializeField]
	public UIButton configBtn;

	// Token: 0x040052C7 RID: 21191
	[SerializeField]
	public UIButton autoReplenishFleetButton;

	// Token: 0x040052C8 RID: 21192
	[SerializeField]
	public Color fleetBgNormalColor;

	// Token: 0x040052C9 RID: 21193
	[SerializeField]
	public Color fleetBgEngagingColor;

	// Token: 0x040052CA RID: 21194
	[SerializeField]
	public Color fleetBgRetreatingColor;

	// Token: 0x040052CB RID: 21195
	[SerializeField]
	public Color fleetBgDisabledColor;

	// Token: 0x040052CC RID: 21196
	[SerializeField]
	public Color fleetStateEngagingColor;

	// Token: 0x040052CD RID: 21197
	[SerializeField]
	public Color fleetStateRetreatingColor;

	// Token: 0x040052CE RID: 21198
	[Header("Power Group")]
	[SerializeField]
	public Text energyText;

	// Token: 0x040052CF RID: 21199
	[SerializeField]
	public Image energyBar;

	// Token: 0x040052D0 RID: 21200
	[SerializeField]
	public Image powerIcon;

	// Token: 0x040052D1 RID: 21201
	[SerializeField]
	public Text powerText;

	// Token: 0x040052D2 RID: 21202
	[SerializeField]
	public Text stateText;

	// Token: 0x040052D3 RID: 21203
	[Header("Storage Group")]
	[SerializeField]
	public UIStorageGrid storageGrid;

	// Token: 0x040052D4 RID: 21204
	[SerializeField]
	public Slider bansSlider;

	// Token: 0x040052D5 RID: 21205
	[SerializeField]
	public Text bansValueText;

	// Token: 0x040052D6 RID: 21206
	[SerializeField]
	public UIButton bansButton;

	// Token: 0x040052D7 RID: 21207
	[SerializeField]
	public UIButton filterButton0;

	// Token: 0x040052D8 RID: 21208
	[SerializeField]
	public UIButton filterButton1;

	// Token: 0x040052D9 RID: 21209
	[SerializeField]
	public UIButton filterButton2;

	// Token: 0x040052DA RID: 21210
	[SerializeField]
	public UIButton filterButton3;

	// Token: 0x040052DB RID: 21211
	[SerializeField]
	public UIButton loopFilterButton;

	// Token: 0x040052DC RID: 21212
	[SerializeField]
	public RectTransform loopFilterRt;

	// Token: 0x040052DD RID: 21213
	[SerializeField]
	public RectTransform loopFilterBgRt;

	// Token: 0x040052DE RID: 21214
	[SerializeField]
	public RectTransform filterSettingsGroup;

	// Token: 0x040052DF RID: 21215
	[Header("Config Group")]
	[SerializeField]
	public Slider chargePowerSlider;

	// Token: 0x040052E0 RID: 21216
	[SerializeField]
	public Text chargePowerValue;

	// Token: 0x040052E1 RID: 21217
	[Header("Colors & Settings")]
	[SerializeField]
	public Color powerNormalColor;

	// Token: 0x040052E2 RID: 21218
	[SerializeField]
	public Color powerNormalIconColor;

	// Token: 0x040052E3 RID: 21219
	[SerializeField]
	public Color powerOffColor;

	// Token: 0x040052E4 RID: 21220
	[SerializeField]
	public Color powerOffIconColor;

	// Token: 0x040052E5 RID: 21221
	[SerializeField]
	public Color energyBarColorIn;

	// Token: 0x040052E6 RID: 21222
	[SerializeField]
	public Color energyBarColorOut;

	// Token: 0x040052E7 RID: 21223
	[SerializeField]
	public Color energyTextColor0;

	// Token: 0x040052E8 RID: 21224
	[SerializeField]
	public Color energyTextColor1;

	// Token: 0x040052E9 RID: 21225
	[SerializeField]
	public Color energyTextColor2;

	// Token: 0x040052EA RID: 21226
	[SerializeField]
	public Color idleColor;

	// Token: 0x040052EB RID: 21227
	private List<UIBattleBaseFighterEntry> fighterEntries;

	// Token: 0x040052EC RID: 21228
	private UIButton[] fleetTypeButtons;

	// Token: 0x040052ED RID: 21229
	public PlanetFactory factory;

	// Token: 0x040052EE RID: 21230
	public PowerSystem powerSystem;

	// Token: 0x040052EF RID: 21231
	public DefenseSystem defenseSystem;

	// Token: 0x040052F0 RID: 21232
	public Player player;

	// Token: 0x040052F1 RID: 21233
	public GameHistoryData history;

	// Token: 0x040052F2 RID: 21234
	public BattleBaseComponent battleBase;

	// Token: 0x040052F3 RID: 21235
	public ConstructionModuleComponent constructionModule;

	// Token: 0x040052F4 RID: 21236
	public CombatModuleComponent combatModule;

	// Token: 0x040052F5 RID: 21237
	public StorageComponent storage;

	// Token: 0x040052F6 RID: 21238
	private int _battleBaseId;

	// Token: 0x040052F7 RID: 21239
	public int fleetConfigFrame;

	// Token: 0x040052F8 RID: 21240
	private int _fleetConfigIndex = -1;

	// Token: 0x040052F9 RID: 21241
	private StringBuilder powerServedSB;

	// Token: 0x040052FA RID: 21242
	private StringBuilder powerAccumulatedSB;

	// Token: 0x040052FB RID: 21243
	private bool eventLock;
}
