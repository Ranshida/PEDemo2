using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrainStormControl : MonoBehaviour
{
    public int SkillType;//玩家当前技能类型
    public int BossLevel;//敌人等级
    public int Shield;//玩家护盾值
    public int DebuffB;//人脉，免疫一次攻击
    public int ExtraDamage;//想象力，每次攻击能附加的额外伤害
    public int ReduceDiceNum;//每回合少获得n个骰子的Debuff
    public int ExtraAttack;//技能14的额外攻击力加成
    public int EmptyDiceNum;//空白骰子的数量
    public int StageCount = 0;//已经历关卡数
    public int TurnCount;//回合数
    public bool BSStarted = false;//是否已经开始头脑风暴，用于心力爆炸检测

    private int HpLimit;//Boss血量上限，用于UI显示
    private int FightType = 1;//1正常头脑风暴 2事件组 3融资
    private int UpgradePoint = 0;//当前升级点数
    private int UpgradeProgress = 0;//当前升级点数获取进度

    public int[] AcquiredItem = new int[4];

    private EventGroupInfo CurrentEGI;
    public EmpBSInfo CurrentBSInfo, CEOInfo;
    public BSRouteNode CurrentNode;//当前所处的节点
    public GameControl GC;
    public GameObject MemberSelectPanel, RouteSelectPanel, FightPanel, SkillButton, CloseButton, 
        RouteNoticePanel, NodeSkipButton, DiceUpgradePanel, EscapeButton;
    public BSDiceControl DicePrefab;
    public BSBossControl CurrentBoss, BossPrefab;
    public BSSkillMarker MarkerPrefab;
    public Transform DiceContent, SelectedDiceContent, BossContent, DiceUpgradeContent;
    public Text Text_SkillName, Text_Turn, Text_Histroy, Text_EmptyDice, Text_RouteNotice, Text_Item, Text_Upgrade, Text_CoreTeamButton;
    public Text Text_Shield, Text_ExtraDamage, Text_ExtraStatus;
    public List<BSDiceControl> CurrentDices = new List<BSDiceControl>();
    public List<BSRouteNode> CurrentNodes = new List<BSRouteNode>();
    public List<BSDiceControl> SelectedDices = new List<BSDiceControl>();
    public List<Employee> CoreMembers = new List<Employee>();
    public List<BSBossControl> Bosses = new List<BSBossControl>();
    private List<BSSkillMarker> UpgradeDices = new List<BSSkillMarker>();
    public EmpBSInfo[] EmpInfos = new EmpBSInfo[6];       //头脑风暴战斗面板的info
    public EmpBSInfo[] EmpSelectInfos = new EmpBSInfo[6]; //核心团队选择面板的info

    private void Update()
    {        
        Text_Turn.text = "回合" + TurnCount;
        Text_EmptyDice.text = "本回合空白骰子数:" + EmptyDiceNum;
        Text_Shield.text = "护盾\n" + Shield;
        Text_ExtraDamage.text = "想象力\n" + ExtraDamage;
        Text_ExtraStatus.text = "";
        if (ExtraAttack > 0)
            Text_ExtraStatus.text += "本回合攻击力+3\n";
        if (DebuffB > 0)
            Text_ExtraStatus.text += "抵消" + DebuffB + "次攻击";
    }

    //头脑风暴结束后的一系列设定
    public void EndBS()
    {
        BSStarted = false;
        CloseButton.SetActive(true);
        FightPanel.SetActive(false);
        RouteSelectPanel.SetActive(false);
        MemberSelectPanel.SetActive(true);
        this.GetComponent<WindowBaseControl>().SetWndState(false);
        GC.RemovePause(this);
        //心力+0，用于触发心力爆炸检测
        foreach(Employee emp in CoreMembers)
        {
            emp.Mentality += 0;
        }
    }

    //从人员选择面板开始头脑风暴
    public void StartBS()
    {
        //有未处理事件时不能继续
        if (GC.EC.UnfinishedEvents.Count > 0)
            return;
        GC.AskPause(this);
        BSStarted = true;
        StageCount = 0;
        FightType = 1;
        //重置物品数量
        for(int i = 0; i < AcquiredItem.Length; i++)
        {
            AcquiredItem[i] = 0;
        }
        //重置相关perk
        foreach(DepControl dep in GC.CurrentDeps)
        {
            dep.RemoveSpecialBuffs(1);
        }

        //核心成员信息传递
        for (int i = 0; i < 6; i++)
        {
            if (i < CoreMembers.Count)
            {
                EmpInfos[i].gameObject.SetActive(true);
                EmpInfos[i].EmpJoin(CoreMembers[i]);
            }
            else
                EmpInfos[i].gameObject.SetActive(false);
        }

        //手动设定节点状态
        BSRouteNode StartNode = null;
        foreach(BSRouteNode node in CurrentNodes)
        {
            if (node.isEnd == false && node.NodeType == 0)
                StartNode = node;
            node.SelectButton.interactable = false;
        }
        //开第一波节点
        foreach (BSRouteNode node in StartNode.NextNodes)
        {
            node.SelectButton.interactable = true;
        }

        MemberSelectPanel.SetActive(false);
        RouteSelectPanel.SetActive(true);
        ResetStatus();
    }

    //事件组的头脑风暴战斗
    public void StartEventBossFight(EventGroupInfo e)
    {
        //有未处理事件时不能继续
        if (GC.EC.UnfinishedEvents.Count > 0 || BSStarted == true)
            return;
        GC.AskPause(this);
        CurrentEGI = e;
        BSStarted = true;
        FightType = 2;
        //核心成员信息传递
        for (int i = 0; i < 6; i++)
        {
            if (i < CoreMembers.Count)
            {
                EmpInfos[i].gameObject.SetActive(true);
                EmpInfos[i].EmpJoin(CoreMembers[i]);
            }
            else
                EmpInfos[i].gameObject.SetActive(false);
        }
        SetBossLevel(e.TargetEventGroup.BSBossLevel);
        Text_Item.text = "";
        this.GetComponent<WindowBaseControl>().SetWndState(true);
        MemberSelectPanel.SetActive(false);
        RouteSelectPanel.SetActive(false);
        FightPanel.SetActive(true);
    }

    //融资的头脑风暴战斗
    public void StartInvestBossFight(int level)
    {
        //有未处理事件时不能继续
        if (GC.EC.UnfinishedEvents.Count > 0 || BSStarted == true)
            return;
        GC.AskPause(this);
        BSStarted = true;
        FightType = 3;
        EscapeButton.SetActive(true);
        //核心成员信息传递
        for (int i = 0; i < 6; i++)
        {
            if (i < CoreMembers.Count)
            {
                EmpInfos[i].gameObject.SetActive(true);
                EmpInfos[i].EmpJoin(CoreMembers[i]);
            }
            else
                EmpInfos[i].gameObject.SetActive(false);
        }
        SetBossLevel(level);
        Text_Item.text = "";
        this.GetComponent<WindowBaseControl>().SetWndState(true);
        MemberSelectPanel.SetActive(false);
        RouteSelectPanel.SetActive(false);
        FightPanel.SetActive(true);
    }
    //下一回合
    public void NextTurn()
    {
        //减少员工禁骰子时间
        foreach (Employee emp in CoreMembers)
        {
            if (emp.SkillLimitTime > 0)
                emp.SkillLimitTime -= 1;
        }

        int FinishCount = 0;
        //如果boss还能行动则发动技能
        foreach(BSBossControl boss in Bosses)
        {
            boss.BossTurn();
            //洞察的伤害可能导致boss死亡，此处要额外检查一次
            if (boss.BossHp == 0)
                FinishCount += 1;
        }
        if (FinishCount == Bosses.Count)
            FightFinish(true);

        //失败判定
        FinishCount = 0;
        foreach(Employee emp in CoreMembers)
        {
            if (emp.Mentality == 0)
                FinishCount += 1;
        }
        if(FinishCount == CoreMembers.Count)
        {
            FightFinish(false);
            GC.QC.Init("所有成员心力为0，议题失败");
        }

        //额外伤害归零
        ExtraAttack = 0;
        //回合+1
        TurnCount += 1;
        //重新掷骰子
        RandomDice();
    }

    //重置各种状态
    public void ResetStatus()
    {
        SkillType = 0;
        Shield = 0;//玩家护盾值
        DebuffB = 0;//人脉，免疫一次攻击
        ExtraDamage = 0;//想象力，每次攻击能附加的额外伤害
        ReduceDiceNum = 0;//每回合少获得n个骰子的Debuff
        TurnCount = 1;//回合数重置
        Text_Histroy.text = "";
        foreach (Employee emp in CoreMembers)
        {
            emp.SkillLimitTime = 0;
        }

        //确认是开始战斗的时候再刷新物品UI
        if (CurrentNode == null)
            return;

        Text_Item.text = "关卡" + StageCount;

        //设定UI内容
        if (CurrentNode.NodeType == 1)
            Text_Item.text += "\n击败奖励:《胜利开发者的全新计划》";
        else if (CurrentNode.NodeType == 2)
            Text_Item.text += "\n击败奖励:《预算新编》";
        else if (CurrentNode.NodeType == 3)
            Text_Item.text += "\n击败奖励:《致全体同事的一封信》";
        else if (CurrentNode.NodeType == 4)
            Text_Item.text += "\n击败奖励:《加班技术大全》";
        bool haveItem = false;
        for(int i = 0; i < 4; i++)
        {
            if (AcquiredItem[i] > 0)
            {
                if(haveItem == false)
                {
                    haveItem = true;
                    Text_Item.text += "\n已获奖励:";
                }
                if (i == 0)
                    Text_Item.text += "\n《胜利开发者的全新计划》* " + AcquiredItem[i];
                else if (i == 1)
                    Text_Item.text += "\n《预算新编》* " + AcquiredItem[i];
                else if (i == 2)
                    Text_Item.text += "\n《致全体同事的一封信》* " + AcquiredItem[i];
                else if (i == 3)
                    Text_Item.text += "\n《加班技术大全》* " + AcquiredItem[i];
            }
        }
    }

    //生成一个骰子预制体
    public void InitDice(int[] sides)
    {
        BSDiceControl dice = Instantiate(DicePrefab, DiceContent);
        dice.BSC = this;
        dice.SetSides(sides);
        CurrentDices.Add(dice);
        dice.RandomSide();
    }

    //生成一轮骰子并清除旧骰子
    public void RandomDice()
    {
        EmptyDiceNum = 0;
        foreach (BSDiceControl dice in CurrentDices)
        {
            Destroy(dice.gameObject);
        }
        CurrentDices.Clear();
        SelectedDices.Clear();
        foreach (EmpBSInfo info in EmpInfos)
        {
            if (info.emp == null)
                continue;
            //禁骰子状态或退场时跳过
            if (info.emp.SkillLimitTime > 0 || info.emp.Mentality == 0)
                continue;

            if (info.emp != null)
                info.CreateDices();
        }
        //根据buff移除随机数量的骰子
        if (CurrentDices.Count > 0)//先检测有没有能删的骰子
        {
            for (int i = 0; i < ReduceDiceNum; i++)
            {
                int num = Random.Range(0, CurrentDices.Count);
                Destroy(CurrentDices[num].gameObject);
                CurrentDices.RemoveAt(num);
            }
        }
        CheckSkillType();
    }

    //根据已选的骰子确认技能类型
    public void CheckSkillType()
    {
        if (SelectedDices.Count > 3 || SelectedDices.Count == 0)
        {
            SkillType = 0;
            HighLightDices(new int[6] { 1, 1, 1, 1, 1, 1 });
            Text_SkillName.text = "";
            return;
        }
        int[] TypeCount = new int[6];
        foreach(BSDiceControl dice in SelectedDices)
        {
            TypeCount[dice.CurrentType] += 1;
        }
        if (SelectedDices.Count == 1)
        {
            //表达观点
            if (TypeCount[3] == 1)
            {
                SkillType = 1;
                HighLightDices(new int[6] { 0, 1, 1, 1, 1, 0 });
                Text_SkillName.text = "表达观点:造成3点攻击";
            }
            //解释原理
            else if (TypeCount[4] == 1)
            {
                SkillType = 2;
                HighLightDices(new int[6] { 1, 1, 1, 1, 1, 0 });
                Text_SkillName.text = "解释原理:造成2点护盾";
            }
            //拥抱
            else if (TypeCount[0] == 1)
            {
                SkillType = 3;
                HighLightDices(new int[6] { 1, 1, 1, 0, 1, 1 });
                Text_SkillName.text = "拥抱:选择一名员工，恢复9点心力";
            }
            //听到风声
            else if (TypeCount[5] == 1)
            {
                SkillType = 6;
                HighLightDices(new int[6] { 1, 0, 1, 0, 0, 0 });
                Text_SkillName.text = "听到风声:向议题怪施加“小意思”，减弱议题怪攻击力40%，维持2回合，回合可叠加";
            }
            //只有辅助骰子
            else if (TypeCount[2] == 1)
            {
                SkillType = 0;
                HighLightDices(new int[6] { 1, 1, 1, 1, 1, 1 });
            }
            //只有金钱骰子
            else if (TypeCount[1] == 1)
            {
                SkillType = 0;
                HighLightDices(new int[6] { 1, 1, 1, 1, 1, 0 });
            }
        }
        else if (SelectedDices.Count == 2)
        {
            //场外援助
            if (TypeCount[2] == 2)
            {
                SkillType = 4;
                HighLightDices(new int[6] { 0, 0, 0, 0, 0, 0 });
                Text_SkillName.text = "场外援助:造成3点护盾";
            }
            //增加预算(5)
            else if (TypeCount[1] == 1 && TypeCount[3] == 1)
            {
                SkillType = 5;
                HighLightDices(new int[6] { 0, 0, 1, 0, 0, 0 });
                Text_SkillName.text = "增加预算:造成12点攻击，消耗50金钱";
            }
            //反复强调(8)
            else if (TypeCount[3] == 2)
            {
                SkillType = 8;
                HighLightDices(new int[6] { 0, 0, 0, 1, 0, 0 });
                Text_SkillName.text = "反复强调:造成8点攻击";
            }
            //明确立场(9)
            else if (TypeCount[4] == 2)
            {
                SkillType = 9;
                HighLightDices(new int[6] { 0, 0, 0, 0, 1, 0 });
                Text_SkillName.text = "明确立场:造成5点护盾";
            }
            //鼓舞(10)
            else if (TypeCount[4] == 1 && TypeCount[0] == 1)
            {
                SkillType = 10;
                HighLightDices(new int[6] { 1, 0, 0, 0, 0, 0 });
                Text_SkillName.text = "鼓舞:选择一名员工，恢复20点心力";
            }
            //歪脑筋(13)
            else if (TypeCount[5] == 1 && TypeCount[0] == 1)
            {
                SkillType = 13;
                HighLightDices(new int[6] { 0, 0, 0, 0, 0, 0 });
                Text_SkillName.text = "歪脑筋:施加5层洞察，所有员工心力减少5点";
            }
            //求助(18)
            else if (TypeCount[2] == 1 && TypeCount[0] == 1)
            {
                SkillType = 18;
                HighLightDices(new int[6] { 0, 0, 0, 0, 0, 0 });
                Text_SkillName.text = "求助:恢复15点心力";
            }
            //底气充足(19)
            else if (TypeCount[1] == 1 && TypeCount[4] == 1)
            {
                SkillType = 19;
                HighLightDices(new int[6] { 0, 0, 0, 0, 0, 0 });
                Text_SkillName.text = "底气充足:造成7点护盾，消耗50金钱";
            }
            //攻击+防御
            else if (TypeCount[3] == 1 && TypeCount[4] == 1)
            {
                SkillType = 0;
                HighLightDices(new int[6] { 0, 0, 1, 1, 0, 0 });
            }
            //攻击+辅助
            else if (TypeCount[2] == 1 && TypeCount[3] == 1)
            {
                SkillType = 0;
                HighLightDices(new int[6] { 0, 1, 0, 0, 1, 0 });
            }
            //钱+buff
            else if (TypeCount[0] == 1 && TypeCount[1] == 1)
            {
                SkillType = 0;
                HighLightDices(new int[6] { 0, 1, 0, 0, 0, 0 });
            }
            //辅助+Debuff
            else if (TypeCount[2] == 1 && TypeCount[5] == 1)
            {
                SkillType = 0;
                HighLightDices(new int[6] { 0, 0, 0, 0, 0, 1 });
            }
            //钱2
            else if (TypeCount[1] == 2)
            {
                SkillType = 0;
                HighLightDices(new int[6] { 1, 0, 0, 0, 0, 0 });
            }
            //buff2
            else if (TypeCount[0] == 2)
            {
                SkillType = 0;
                HighLightDices(new int[6] { 1, 0, 0, 0, 1, 0 });
            }
            //Debuff2
            else if (TypeCount[5] == 2)
            {
                SkillType = 0;
                HighLightDices(new int[6] { 0, 0, 1, 0, 0, 0 });
            }
            //其他
            else
            {
                SkillType = 0;
                HighLightDices(new int[6] { 0, 0, 0, 0, 0, 0 });
            }
        }
        else if (SelectedDices.Count == 3)
        {
            //充分讨论(7)
            if (TypeCount[2] == 1 && TypeCount[3] == 1 && TypeCount[4] == 1)
            {
                SkillType = 7;
                Text_SkillName.text = "充分讨论:施加3层洞察";
            }
            //周全准备(11)
            else if (TypeCount[2] == 1 && TypeCount[1] == 1 && TypeCount[3] == 1)
            {
                SkillType = 11;
                Text_SkillName.text = "周全准备:造成12点攻击";
            }
            //物质奖励(12)
            else if (TypeCount[0] == 1 && TypeCount[1] == 2)
            {
                SkillType = 12;
                Text_SkillName.text = "物质奖励:选择一名员工，恢复25点心力，消耗100金钱";
            }
            //勇敢的心(14)
            else if (TypeCount[0] == 2 && TypeCount[4] == 1)
            {
                SkillType = 14;
                Text_SkillName.text = "勇敢的心:本回合所有攻击力+3";
            }
            //深刻论证(15)
            else if (TypeCount[3] == 3)
            {
                SkillType = 15;
                Text_SkillName.text = "深刻论证:造成15点攻击";
            }
            //捍卫原则(16)
            else if (TypeCount[4] == 3)
            {
                SkillType = 16;
                Text_SkillName.text = "捍卫原则:造成10点护盾";
            }
            //倾听(17)
            else if (TypeCount[0] == 3)
            {
                SkillType = 17;
                Text_SkillName.text = "倾听:所有员工，恢复12点心力";
            }
            //动用人脉(20)
            else if (TypeCount[5] == 2 && TypeCount[2] == 1)
            {
                SkillType = 20;
                Text_SkillName.text = "动用人脉:获得“人脉”，免疫本回合议题一次攻击，可叠加";
            }
            //厚积薄发(21)
            else if (TypeCount[3] == 2 && TypeCount[4] == 1)
            {
                SkillType = 21;
                Text_SkillName.text = "厚积薄发:将护盾全部转化为攻击";
            }
            

            //关闭所有高亮
            HighLightDices(new int[6] { 0, 0, 0, 0, 0, 0 });
        }

        if (SkillType <= 0)
        {
            Text_SkillName.text = "";
            SkillButton.SetActive(false);
        }
        else
            SkillButton.SetActive(true);
            
    }

    //设置技能图标和特质信息
    public void CheckAllMarkers()
    {
        for(int i = 0; i < 6; i++)
        {
            if (EmpInfos[i].emp != null)
                EmpInfos[i].CheckMarker();
            if (EmpSelectInfos[i].emp != null)
            {
                EmpSelectInfos[i].CheckMarker();
                EmpSelectInfos[i].CheckPerk();
            }
        }
    }

    //玩家发动技能
    public void UseSkill()
    {
        if (SkillType == 0)
            return;
        if (SkillType == 1)
            CauseDamage(3);
        else if (SkillType == 2)
            Shield += 2;
        else if (SkillType == 3)
        {
            StartSelectEmp();
            return;
        }
        else if (SkillType == 4)
            Shield += 4;
        else if (SkillType == 5)
        {
            if (GC.Money < 50)
            {
                GC.QC.Init("金钱不足");
                return;
            }
            GC.Money -= 50;
            CauseDamage(12);
        }
        else if (SkillType == 6)
            CurrentBoss.DebuffA += 2;
        else if (SkillType == 7)
            CurrentBoss.DotValue += 3;
        else if (SkillType == 8)
            CauseDamage(8);
        else if (SkillType == 9)
            Shield += 5;
        //选择
        else if (SkillType == 10)
        {
            StartSelectEmp();
            return;
        }
        else if (SkillType == 11)
            CauseDamage(12);
        //选择
        else if (SkillType == 12)
        {
            if (GC.Money < 100)
            {
                GC.QC.Init("金钱不足");
                return;
            }
            GC.Money -= 100;
            StartSelectEmp();
        }
        else if (SkillType == 13)
        {
            CurrentBoss.DotValue += 5;
            foreach (Employee emp in CoreMembers)
            {
                emp.Mentality -= 5;
            }
        }
        else if (SkillType == 14)
            ExtraAttack += 3;
        else if (SkillType == 15)
            CauseDamage(15);
        else if (SkillType == 16)
            Shield += 10;
        else if (SkillType == 17)
        {
            foreach (Employee emp in CoreMembers)
            {
                emp.Mentality += 12;
            }
        }
        //也要选择？
        else if (SkillType == 18)
        {
            StartSelectEmp();
            return;
        }
        else if (SkillType == 19)
        {
            if (GC.Money < 50)
            {
                GC.QC.Init("金钱不足");
                return;
            }
            GC.Money -= 50;
            Shield += 7;
        }
        else if (SkillType == 20)
            DebuffB += 1;
        else if (SkillType == 21)
            CauseDamage(Shield);

        //不管有没有取消选择，都把骰子重置一下
        for(int i = 0; i < SelectedDices.Count; i++)
        {
            CurrentDices.Remove(SelectedDices[i]);
            Destroy(SelectedDices[i].gameObject);
        }
        SelectedDices.Clear();
        CheckSkillType();
        if (CurrentBoss != null)
            CurrentBoss.UpdateUI();
    }

    //进入选择员工界面
    public void StartSelectEmp()
    {
        GC.TotalEmpPanel.SetWndState(true);
        GC.SelectMode = 4;
        GC.Text_EmpSelectTip.gameObject.SetActive(true);
        GC.Text_EmpSelectTip.text = "选择一个员工";
        foreach(Employee emp in GC.CurrentEmployees)
        {
            if (CoreMembers.Contains(emp))
                emp.InfoB.gameObject.SetActive(true);
            else
                emp.InfoB.gameObject.SetActive(false);
        }
    }
    public void UseSkillOnSelectedEmp(Employee target)
    {
        if (SkillType == -1)
            target.Mentality += (int)(target.MentalityLimit * 0.2f);
        else if (SkillType == 3)
            target.Mentality += 9;
        else if (SkillType == 10)
            target.Mentality += 20;
        else if (SkillType == 12)
            target.Mentality += 25;
        else if (SkillType == 18)
            target.Mentality += 15;


        //重置已选择骰子
        for (int i = 0; i < SelectedDices.Count; i++)
        {
            CurrentDices.Remove(SelectedDices[i]);
            Destroy(SelectedDices[i].gameObject);
        }
        SelectedDices.Clear();
        CheckSkillType();
        if (CurrentBoss != null)
            CurrentBoss.UpdateUI();
    }

    //高亮可选的骰子
    public void HighLightDices(int[] type)
    {
        foreach(BSDiceControl dice in CurrentDices)
        {
            //已选的跳过
            if (dice.Selected == true)
                continue;
            if (dice.Selected == true || type[dice.CurrentType] == 0)
                dice.SetOutline(false);
            else
                dice.SetOutline(true);
        }
    }


    //设定Boss的等级并开始战斗
    public void SetBossLevel(int level)
    {
        if(level == 1)
        {
            InitBoss(1);
            InitBoss(1);
            InitBoss(1);
        }
        else if (level == 2)
            InitBoss(2);
        else if (level == 3)
            InitBoss(3);
        BossLevel = level;
        
        RandomDice();
        CheckSkillType();
        FightPanel.SetActive(true);
        RouteSelectPanel.SetActive(false);
        ResetStatus();
    }

    //生成boss
    void InitBoss(int level)
    {
        BSBossControl newBoss = Instantiate(BossPrefab, BossContent);
        newBoss.BSC = this;
        newBoss.SelectBoss();
        Bosses.Add(newBoss);
        newBoss.SetLevel(level);
    }

    //玩家造成伤害
    public void CauseDamage(int value)
    {
        if (CurrentBoss != null)
            CurrentBoss.TakeDamage(value);
        int FinishCount = 0;
        foreach(BSBossControl boss in Bosses)
        {
            if (boss.BossHp == 0)
                FinishCount += 1;
        }
        if (FinishCount == Bosses.Count)
            FightFinish(true);
    }

    //战斗结果判定
    public void FightFinish(bool Win)
    {
        //不是事件组头脑风暴战斗的时候
        if (FightType == 1)
        {
            if (Win == true)
            {
                //根据节点类型产生具体结果(暂时只加数)
                AcquiredItem[CurrentNode.NodeType - 1] += 1;
                if (CurrentNode.NodeType == 1)
                    GC.CreateItem(6);
                else if (CurrentNode.NodeType == 2)
                    GC.CreateItem(5);
                else if (CurrentNode.NodeType == 3)
                    GC.CreateItem(4);
                else if (CurrentNode.NodeType == 4)
                    GC.CreateItem(3);

                System.Action AgreeAction = () =>
                {
                    RouteSelectPanel.SetActive(true);
                };
                GC.QC.Init("本阶段议题讨论完毕", AgreeAction);
            }
            else
            {
                GC.Morale -= 5;
                RouteSelectPanel.SetActive(true);
                GC.QC.Init("议题讨论失败，士气-5");
            }
        }
        //是事件组头脑风暴战斗的时候
        else if (FightType == 2)
        {
            if (Win == true)
            {
                GC.QC.Init("事件组议题成功");
                CurrentEGI.ResolveStage(CurrentEGI.TargetEventGroup.BSTurnCorrection);
            }
            else
                GC.QC.Init("事件组议题失败");
            CurrentEGI = null;
            EndBS();
        }
        //融资头脑风暴战斗
        else if (FightType == 3)
        {
            if (Win == true)
            {
                GC.QC.Init("融资议题成功");
                GC.foeControl.InvestComplete();
            }
            else
                GC.QC.Init("融资议题失败");
            EndBS();
        }
        
        CurrentBoss = null;
        for(int i = 0; i < Bosses.Count; i++)
        {
            Destroy(Bosses[i].gameObject);
        }
        Bosses.Clear();
        EscapeButton.SetActive(false);
        FightPanel.SetActive(false);
    }

    //询问玩家是否跳过会议
    public void SkipStage()
    {
        CurrentNode.ConfirmNode();
        FightFinish(false);
        RouteNoticePanel.SetActive(false);
    }

    //显示路径确认面板
    public void ShowRouteNotice()
    {
        if (CurrentNode.NodeType == 0 && CurrentNode.isEnd == true)
        {
            EndBS();
            return;
        }
        Text_RouteNotice.text = "选择进入的议题是:\n" + CurrentNode.Text_Name.text + "议题";
        RouteNoticePanel.SetActive(true);
        if (CurrentNode.NodeType == 5)
            NodeSkipButton.SetActive(false);
        else
            NodeSkipButton.SetActive(true);
    }

    //在路径确认面板中确认选择
    public void ConfirmNodeSelect()
    {
        CurrentNode.ConfirmNode();
        RouteNoticePanel.SetActive(false);
    }

    //升级骰子
    public void UpgradeDice(Employee emp)
    {
        DiceUpgradePanel.SetActive(true);
        for (int i = 0; i < UpgradeDices.Count; i++)
        {
            Destroy(UpgradeDices[i].gameObject);
        }
        UpgradeDices.Clear();
        for(int i = 0; i < emp.CurrentDices.Count; i++)
        {
            int num = 0;
            foreach (int n in emp.CurrentDices[i])
            {
                if (n != -1)
                    num += 1;
                else
                    break;
            }
            if (num == 6)
                continue;
            BSSkillMarker m = Instantiate(GC.BSC.MarkerPrefab, DiceUpgradeContent).GetComponent<BSSkillMarker>();
            m.SetInfo(emp.CurrentDices[i][0], num);
            UpgradeDices.Add(m);
            int numb = i;
            m.GetComponent<Button>().onClick.AddListener(() =>
            {
                emp.UpgradeDice(numb);
                UpgradePointUse();
                DiceUpgradePanel.SetActive(false);
                CheckAllMarkers();
            });
        }

    }

    //成功处理事件后加技能点数进度
    public void EventSolved()
    {
        UpgradeProgress += 1;
        if (UpgradeProgress >= AdjustData.CoreMemberUpgradePointLimit)
        {
            UpgradeProgress -= AdjustData.CoreMemberUpgradePointLimit;
            UpgradePoint += 1;
        }
        UpgradeCheck();
    }

    public void UpgradePointUse()
    {
        UpgradePoint -= 1;
        UpgradeCheck();
    }

    public void UpgradeCheck()
    {
        if (UpgradePoint > 0)
        {
            foreach(EmpBSInfo info in EmpSelectInfos)
            {
                info.AddExpButton.interactable = true;
                info.UpgradeDiceButton.interactable = true;
            }
            Text_CoreTeamButton.color = Color.red;
        }
        else
        {
            foreach (EmpBSInfo info in EmpSelectInfos)
            {
                info.AddExpButton.interactable = false;
                info.UpgradeDiceButton.interactable = false;
            }
            Text_CoreTeamButton.color = Color.black;
        }
        Text_Upgrade.text = "成功处理抉择事件 " + UpgradeProgress + "/" + AdjustData.CoreMemberUpgradePointLimit + " \n当前可用升级点:" + UpgradePoint;
    }
}
