using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BSBossControl : MonoBehaviour
{
    public int DebuffA = 0;//Boss攻击力削弱40%buff（小意思）的层数
    public int BossLevel;//等级
    public int BossHp;//血量
    public int DotValue;//洞察，持续伤害

    private int HpLimit;//血量上限
    private int NextSkillType;//下一个技能的类型
    private int NextSkillValue;//下一个技能的效果

    public GameObject SelectButton, SelectMarker, DeadMarker;
    public BrainStormControl BSC;
    public Text Text_Name, Text_Status, Text_NextSkill;

    List<Employee> Targets = new List<Employee>() { null, null, null};//当前的技能目标

    public void UpdateUI()
    {
        if (BossHp > 0)
        {
            Text_Status.text = "进度 " + (HpLimit - BossHp) + "/" + HpLimit + "\n洞察:" + DotValue;
            if (DebuffA > 0)
                Text_Status.text += "\n攻击力下降(" + DebuffA + "回合)";
        }
        else
        {
            Text_Status.gameObject.SetActive(false);
            Text_NextSkill.gameObject.SetActive(false);
            DeadMarker.SetActive(true);

            //如果当前目标是自己则先取消自身选择，再尝试自动切换至另一目标
            if (BSC.CurrentBoss == this)
            {
                SelectBoss(false);
                foreach (BSBossControl boss in BSC.Bosses)
                {
                    if (boss.BossHp > 0)
                    {
                        boss.SelectBoss();
                        break;
                    }
                }
            }
        }
    }

    //根据等级设置属性
    public void SetLevel(int value)
    {
        BossLevel = value;
        if (value == 1)
        {
            BossHp = 40;
            Text_Name.text = "子议题" + BSC.Bosses.Count;
        }
        else if (value == 2)
        {
            BossHp = 50;
            Text_Name.text = "议题1";
        }
        else if (value == 3)
        {
            BossHp = 70;
            Text_Name.text = "议题1";
        }
        if (value == 4)
        {
            BossHp = 50;
            Text_Name.text = "子议题" + BSC.Bosses.Count;
        }
        else if (value == 5)
        {
            BossHp = 150;
            Text_Name.text = "议题1";
        }
        if (value == 6)
        {
            BossHp = 120;
            Text_Name.text = "子议题" + BSC.Bosses.Count;
        }
        if (value == 7)
        {
            BossHp = 120;
            Text_Name.text = "子议题" + BSC.Bosses.Count;
        }
        HpLimit = BossHp;
        UpdateUI();
        SetNextMove();
    }

    //受到伤害
    public void TakeDamage(int value)
    {
        BossHp -= value + BSC.ExtraAttack;
        if (BossHp < 0)
            BossHp = 0;
        UpdateUI();
    }

    //选择该boss为当前目标
    public void SelectBoss(bool Select = true)
    {
        if (Select == true)
        {
            //已有目标时先取消选择
            if (BSC.CurrentBoss != null && BSC.CurrentBoss != this)
                BSC.CurrentBoss.SelectBoss(false);
            BSC.CurrentBoss = this;
            SelectButton.SetActive(false);
            SelectMarker.SetActive(true);
        }
        //自己取消选择
        else
        {
            if (BossHp > 0)
                SelectButton.SetActive(true);
            else//如果已经死亡就不弹出按钮
                SelectButton.SetActive(false);
            SelectMarker.SetActive(false);
            BSC.CurrentBoss = null;
        }
    }

    //Boss设定下一个目标
    void SetNextMove()
    {

        //检索可用的员工
        List<Employee> PosbTargets = new List<Employee>();
        foreach (Employee emp in BSC.CoreMembers)
        {
            if (emp.Mentality > 0)
                PosbTargets.Add(emp);
        }
        //无目标直接不行动
        if (PosbTargets.Count == 0)
        {
            NextSkillType = 0;
            NextSkillValue = 0;
            Text_NextSkill.text = "不行动";
            return;
        }

        int Posb = Random.Range(1, 7);
        Targets[0] = PosbTargets[Random.Range(0, PosbTargets.Count)];
        if (BossLevel == 1)
        {
            NextSkillType = 2;
            NextSkillValue = 8;
            Text_NextSkill.text = "下一技能:降低" + Targets[0].Name + "8点心力";
        }
        else if (BossLevel == 2)
        {
            if (Posb < 6)
            {
                NextSkillType = 2;
                NextSkillValue = 8;
                Text_NextSkill.text = "下一技能:降低" + Targets[0].Name + "35点心力";
            }
            else
            {
                //旧的禁用3个目标骰子的技能
                ////先清空旧目标防止之后随机出问题
                //Targets[1] = null;
                //Targets[2] = null;
                //Text_NextSkill.text = "下一技能:禁用" + Targets[0].Name;

                //if (BSC.CoreMembers.Count > 1)
                //{
                //    while (Targets[1] == null || Targets[0] == Targets[1])
                //    {
                //        Targets[1] = PosbTargets[Random.Range(0, PosbTargets.Count)];
                //    }
                //    Text_NextSkill.text += "," + Targets[1].Name;
                //}
                //if (BSC.CoreMembers.Count > 2)
                //{
                //    while (Targets[2] == null || Targets[0] == Targets[2] || Targets[1] == Targets[2])
                //    {
                //        Targets[2] = PosbTargets[Random.Range(0, PosbTargets.Count)];
                //    }
                //    Text_NextSkill.text += "," + Targets[2].Name;
                //}
                //Text_NextSkill.text += "的所有骰子1回合";
                NextSkillType = 3;
                NextSkillValue = 1;
                Text_NextSkill.text = "下一技能:禁用" + Targets[0].Name + "的所有骰子1回合";
            }
        }
        else if (BossLevel == 3)
        {
            if (Posb < 6)
            {
                NextSkillType = 2;
                NextSkillValue = 17;
                Text_NextSkill.text = "下一技能:降低" + Targets[0].Name + "17点心力";
            }
            else
            {
                NextSkillType = 7;
                NextSkillValue = 2;
                Text_NextSkill.text = "2回合不会产生带有心形的骰子";
            }
        }
        else if (BossLevel == 4)
        {
            if (Posb < 5)
            {
                NextSkillType = 2;
                NextSkillValue = 12;
                Text_NextSkill.text = "下一技能:降低" + Targets[0].Name + "12点心力";
            }
            else
            {
                NextSkillType = 3;
                NextSkillValue = 1;
                Text_NextSkill.text = "下一技能:禁用" + Targets[0].Name + "的所有骰子1回合";
            }
        }
        else if (BossLevel == 5)
        {
            if (Posb < 5)
            {
                NextSkillType = 2;
                NextSkillValue = 25;
                Text_NextSkill.text = "下一技能:降低" + Targets[0].Name + "25点心力";
            }
            else if (Posb == 5)
            {
                NextSkillType = 4;
                NextSkillValue = 1;
                Text_NextSkill.text = "下一技能:降低想象力1点";
            }
            else
            {
                NextSkillType = 8;
                NextSkillValue = 2;
                Text_NextSkill.text = "2回合内不会产生对话框骰子";
            }
        }
        else if (BossLevel == 6)
        {
            if (Posb < 5)
            {
                NextSkillType = 2;
                NextSkillValue = 25;
                Text_NextSkill.text = "下一技能:降低" + Targets[0].Name + "25点心力";
            }
            else if (Posb == 5)
            {
                NextSkillType = 4;
                NextSkillValue = 1;
                Text_NextSkill.text = "下一技能:降低想象力1点";
            }
            else
            {
                //先清空旧目标防止之后随机出问题
                Targets[1] = null;
                Targets[2] = null;
                Text_NextSkill.text = "下一技能:禁用" + Targets[0].Name;

                if (BSC.CoreMembers.Count > 1)
                {
                    while (Targets[1] == null || Targets[0] == Targets[1])
                    {
                        Targets[1] = PosbTargets[Random.Range(0, PosbTargets.Count)];
                    }
                    Text_NextSkill.text += "," + Targets[1].Name;
                }
                Text_NextSkill.text += "的所有骰子1回合";
                NextSkillType = 6;
                NextSkillValue = 1;
            }
        }
        else if (BossLevel == 7)
        {
            if (Posb < 5)
            {
                NextSkillType = 2;
                NextSkillValue = 17;
                Text_NextSkill.text = "下一技能:降低" + Targets[0].Name + "17点心力";
            }
            else if (Posb == 5)
            {
                NextSkillType = 4;
                NextSkillValue = 1;
                Text_NextSkill.text = "下一技能:降低想象力1点";
            }
            else
            {
                NextSkillType = 3;
                NextSkillValue = 2;
                Text_NextSkill.text = "下一技能:禁用" + Targets[0].Name + "的所有骰子2回合";
            }
        }
    }
    //Boss行动
    public void BossTurn()
    {
        //先计算洞察伤害,先死了就不继续
        if (DotValue > 0)
        {
            TakeDamage(DotValue);
            DotValue -= 1;
        }
        //如果Hp为0或技能无法发动时不继续发动技能
        if (BossHp == 0)
            return;
        if (NextSkillType == 0)
        {
            SetNextMove();
            return;
        }
        BossSkill(NextSkillType, NextSkillValue);
        UpdateUI();
        //确认下一个行动
        SetNextMove();
    }
    //Boss技能
    void BossSkill(int type, int value)
    {
        //先计算伤害抵消
        if (BSC.DebuffB > 0)
        {
            BSC.DebuffB -= 1;
            BSC.Text_Histroy.text += "\n[回合" + BSC.TurnCount + "]利用人脉抵消了一次攻击";
            return;
        }

        //计算Debuff
        if (DebuffA > 0)
        {
            DebuffA -= 1;
            value = (int)(value * 0.4f);
            BSC.Text_Histroy.text += "\n[回合" + BSC.TurnCount + "]受到的伤害被Debuff削弱";
        }
        //全体核心成员心力-n
        if (type == 1)
        {
            if (value > BSC.Shield)
            {
                value -= BSC.Shield;
                BSC.Shield = 0;
            }
            else
            {
                BSC.Shield -= value;
                value = 0;
            }
            foreach (Employee emp in BSC.CoreMembers)
            {
                emp.Mentality -= value;
            }
            BSC.Text_Histroy.text += "\n[回合" + BSC.TurnCount + "]全体员工心力-" + value;
        }
        //某个员工心力-n
        else if (type == 2)
        {
            if (value > BSC.Shield)
            {
                value -= BSC.Shield;
                BSC.Shield = 0;
            }
            else
            {
                BSC.Shield -= value;
                value = 0;
            }
            Targets[0].Mentality -= value;
            BSC.Text_Histroy.text += "\n[回合" + BSC.TurnCount + "]" + Targets[0].Name + "心力-" + value;
        }
        //禁用1名员工骰子n回合
        else if (type == 3)
        {
            Targets[0].SkillLimitTime += value;
            BSC.Text_Histroy.text += "\n[回合" + BSC.TurnCount + "]" + Targets[0].Name + "的骰子被禁用" + value + "回合";
        }
        //想象力-n
        else if (type == 4)
        {
            BSC.ExtraDamage -= value;
            if (BSC.ExtraDamage < 0)
                BSC.ExtraDamage = 0;
            BSC.Text_Histroy.text += "\n[回合" + BSC.TurnCount + "]想象力-" + value;
        }
        //下回合少获得n个骰子
        else if (type == 5)
        {
            BSC.ReduceDiceNum += value;
            BSC.Text_Histroy.text += "\n[回合" + BSC.TurnCount + "]下回合少获得" + value + "个骰子";
        }
        //禁用1-3名员工骰子n回合
        else if (type == 6)
        {
            Targets[0].SkillLimitTime += value;
            BSC.Text_Histroy.text += "\n[回合" + BSC.TurnCount + "]" + Targets[0].Name + "的骰子被禁用" + value + "回合";
            if (Targets[1] != null)
            {
                Targets[1].SkillLimitTime += value;
                BSC.Text_Histroy.text += "\n[回合" + BSC.TurnCount + "]" + Targets[1].Name + "的骰子被禁用" + value + "回合";
            }
            if (Targets[2] != null)
            {
                Targets[2].SkillLimitTime += value;
                BSC.Text_Histroy.text += "\n[回合" + BSC.TurnCount + "]" + Targets[2].Name + "的骰子被禁用" + value + "回合";
            }
        }
        //无法获得心形（己方Buff/0）骰子
        else if (type == 7)
        {
            BSC.NoBuffDice += value;
        }
        //无法获得对话框（防御/4）骰子
        else if (type == 7)
        {
            BSC.NoDefenseDice += value;
        }
    }
}
