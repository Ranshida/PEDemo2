using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmpEntity : MonoBehaviour
{
    public bool canMove = false, WorkShift = false, canChangePos = true;
    static public float MoveSpeed = 50.0f;

    public EmpInfo InfoDetail;
    public RectTransform Rect;
    public BuildingManage BM;
    public EmpEntity TargetEmp;
    public Text Text_Name;

    public List<EmpEvent> CurrentEvent = new List<EmpEvent>();
    public List<EmpEvent> ReceivedEvent = new List<EmpEvent>();
    public GameObject[] Markers = new GameObject[5];

    int WaitHour = 0;

    Vector2 Destination = new Vector2(0,0);
    private void Start()
    {
        
    }

    void Update()
    {
        if (TargetEmp != null && WorkShift == false)
            Destination = TargetEmp.transform.position;
        if (canMove == true)
        {
            //移动
            if (Vector2.Distance((Vector2)transform.position, Destination) > 2 * InfoDetail.GC.TimeMultiply)
            {
                Vector2 Dir = (Destination - (Vector2)transform.position).normalized;
                transform.Translate(Dir * Time.deltaTime * InfoDetail.GC.TimeMultiply * MoveSpeed);
            }
            //到达目的地
            else
            {
                canMove = false;
                transform.position = Destination;

                if (WorkShift == true)
                {
                    //下班
                    if (canChangePos == false)
                        InfoDetail.GC.WorkEndCheck();
                    //上班后回到工位
                    else
                    {
                        WorkShift = false;
                        SetTarget();
                    }
                }
                //待命乱逛
                else if (InfoDetail.emp.CurrentDep == null && InfoDetail.emp.CurrentOffice == null && TargetEmp == null)
                {
                    canMove = false;
                    WaitHour = Random.Range(1, 3);
                }
            }
        }
    }

    public void SetInfo(EmpInfo detail)
    {
        InfoDetail = detail;
        detail.Entity = this;
        detail.GC.HourEvent.AddListener(TimePass);
        detail.GC.HourEvent.AddListener(detail.emp.EventCheck);
        BM = detail.GC.BM;
        FindWorkPos();
        Text_Name.text = detail.emp.Name;
    }

    public void TimePass()
    {
        if(WaitHour > 0)
        {
            WaitHour -= 1;
            if (WaitHour == 0 && EventCheck() == true)
                FindWorkPos();
        }
        EventTimePass();
    }

    public void WorkEnd()
    {
        WaitHour = 0;
        canMove = true;
        WorkShift = true;
        canChangePos = false;
        Destination = BM.ExitPos.position;
        for(int i = 0; i < CurrentEvent.Count; i++)
        {
            CurrentEvent[i].EventActive = false;
        }
    }

    public void WorkStart()
    {
        canChangePos = true;
        FindWorkPos();
    }

    public bool EventCheck()
    {
        for(int i = 0; i < CurrentEvent.Count; i++)
        {
            if (CurrentEvent[i].EventActive == true)
                return false;
        }
        for(int i = 0; i < ReceivedEvent.Count; i++)
        {
            if (ReceivedEvent[i].EventActive == true)
                return false;
        }
        return true;
    }

    public void EventTimePass()
    {

        for (int i = 0; i < CurrentEvent.Count; i++)
        {
            if (CurrentEvent[i].EventActive == true)
            {
                CurrentEvent[i].Time -= 1;
                if (CurrentEvent[i].Time <= 0)
                {
                    CurrentEvent[i].FinishEvent();
                    if (EventCheck() == true)
                    {
                        SetTarget();
                    }
                }
            }
        }
        
    }

    public void AddEvent(int type)
    {

        EmpEvent e = new EmpEvent();
        Employee m = InfoDetail.emp;
        e.Self = this;
        float[] r = InfoDetail.emp.Character;
        e.Type = type;
        if (type == 1)
        {
            //此处策划没写完所以只有两种情况
            //体力负， 心力正
            if (m.Mentality >= m.Stamina)
            {
                if (m.Stamina >= 70)
                    e.Value = -1;
                else if (m.Stamina >= 40)
                    e.Value = -1;
                else
                    e.Value = -1;
                //目前仅有喝咖啡一种情况所以不需要目标
                e.HaveTarget = false;
            }
            else
            {
                if (m.Mentality >= 70)
                    e.Value = 1;
                else if (m.Mentality >= 40)
                    e.Value = 1;
                else
                    e.Value = 1;
            }
        }
        //-3、3都是强 -1、1为弱
        else if (type == 2)
        {
            //此处策划没写完所以只有两种情况
            if (r[0] < -2)
                e.Value = -1;
            else if (r[0] < -1)
                e.Value = -1;
            else if (r[0] < 0)
                e.Value = -1;
            else if (r[0] < 1)
                e.Value = 1;
            else if (r[0] < 2)
                e.Value = 1;
            else
                e.Value = 1;
        }
        else if (type == 3)
        {
            if (r[2] < -2)
                e.Value = -3;
            else if (r[2] < -1)
                e.Value = -2;
            else if (r[2] < 0)
                e.Value = -1;
            else if (r[2] < 1)
                e.Value = 1;
            else if (r[2] < 2)
                e.Value = 2;
            else
                e.Value = 3;
        }
        else if (type == 4)
        {
            //此处策划没写完所以只有两种情况
            if (r[1] < -2)
                e.Value = -1;
            else if (r[1] < -1)
                e.Value = -1;
            else if (r[1] < 0)
                e.Value = -1;
            else if (r[1] < 1)
                e.Value = 1;
            else if (r[1] < 2)
                e.Value = 1;
            else
                e.Value = 1;
        }
        if (e.HaveTarget == true)
        {
            if (e.FindTarget() == true)
            {
                CurrentEvent.Add(e);
                print(Text_Name.text + "添加了" + type + "类事件" + e.Value);
                e.Target.ReceivedEvent.Add(e);
                if (EventCheck() == true)
                    SetTarget();
            }
        }
        else
        {
            e.EventActive = true;
            CurrentEvent.Add(e);
            print(Text_Name.text + "添加了" + type + "类事件" + e.Value);
        }

    }

    public bool SetTarget()
    {
        for (int i = 0; i < CurrentEvent.Count; i++)
        {
            //二次检测能否移动以防万一
            if (CurrentEvent[i].EventActive == true)
                return false;
            //再次检测是否需要移动
            else if (CurrentEvent[i].HaveTarget == false)
            {
                CurrentEvent[i].EventActive = true;
                canMove = false;
                return false;
            }
            else if (CurrentEvent[i].Target != null || CurrentEvent[i].FindTarget() == true)
            {
                TargetEmp = CurrentEvent[i].Target;
                Destination = TargetEmp.transform.position;
                canMove = true;
                return false;
            }
        }
        FindWorkPos();
        return true;
    }

    public void FindWorkPos()
    {
        //从当前部门转为待命或者从待命移动到某部门时都要调用
        //此处检测主要为了防止在下班时安排工作导致移动
        if (canChangePos == true)
        {
            if (InfoDetail.emp.CurrentDep != null)
            {
                Destination = InfoDetail.emp.CurrentDep.building.WorkPos[InfoDetail.emp.CurrentDep.CurrentEmps.IndexOf(InfoDetail.emp)].position;
            }
            else if (InfoDetail.emp.CurrentOffice != null)
            {
                Destination = InfoDetail.emp.CurrentOffice.building.WorkPos[0].position;
            }
            else
            {
                float x = Random.Range(BM.MinPos.position.x, BM.MaxPos.position.x);
                float y = Random.Range(BM.MinPos.position.y, BM.MaxPos.position.y);
                Destination = new Vector2(x, y);
            }
        }
        canMove = true;
        TargetEmp = null;
    }

    public void ShowMarker(int type)
    {
        Markers[type].SetActive(true);
        StartCoroutine(ResetMarker(type));
    }

    public void ShowName()
    {
        Text_Name.transform.parent.gameObject.SetActive(true);
    }

    public void HideName()
    {
        Text_Name.transform.parent.gameObject.SetActive(false);
    }

    public void ShowDetailPanel()
    {
        InfoDetail.ShowPanel();
    }

    public void RemoveEntity()
    {
        for (int i = 0; i < CurrentEvent.Count; i++)
        {
            if (CurrentEvent[i].Target != null && CurrentEvent[i].Target.ReceivedEvent.Contains(CurrentEvent[i]))
            {
                //遇到有在进行中的且有目标的事件，先停止其激活，再检查目标是否可以动，最后从目标的ReceiveEvent中删除该事件
                if(CurrentEvent[i].EventActive == true)
                {
                    CurrentEvent[i].EventActive = false;
                    if (CurrentEvent[i].Target.EventCheck() == true)
                        CurrentEvent[i].Target.SetTarget();
                }
                CurrentEvent[i].Target.ReceivedEvent.Remove(CurrentEvent[i]);
            }
        }
        for(int i = 0; i < ReceivedEvent.Count; i++)
        {
            if(ReceivedEvent[i].EventActive == true)
            {
                ReceivedEvent[i].EventActive = false;
                if (ReceivedEvent[i].Self.EventCheck() == true)
                    ReceivedEvent[i].Self.SetTarget();
            }
            ReceivedEvent[i].Self.CurrentEvent.Remove(ReceivedEvent[i]);
        }
        Destroy(this.gameObject);
    }

    IEnumerator ResetMarker(int type)
    {
        yield return new WaitForSeconds(3.5f / InfoDetail.GC.TimeMultiply);
        Markers[type].SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "EmpEntity")
        {
            EmpEntity Et = collision.gameObject.GetComponent<EmpEntity>();
            float posb = Random.Range(0.0f, 1.0f);
            if (posb < 0.3f)
            {
                ShowMarker(0);
                Employee Es = InfoDetail.emp;

                Es.ChangeRelation(Et.InfoDetail.emp, 1);
                Et.InfoDetail.emp.ChangeRelation(Es, 1);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        //接触目标
        if (TargetEmp != null && WorkShift == false)
        {
            EmpEntity Et = collision.gameObject.GetComponent<EmpEntity>();
            if (TargetEmp == Et)
            {
                for (int i = 0; i < CurrentEvent.Count; i++)
                {
                    if (CurrentEvent[i].Target == TargetEmp)
                    {
                        CurrentEvent[i].EventActive = true;
                        canMove = false;
                        TargetEmp.canMove = false;
                    }
                }
            }
        }
    }
}

public class EmpEvent
{
    public bool EventActive = false, HaveTarget = true;
    public int Type, Value, Time = 2;
    public EmpEntity Target = null, Self;

    public void FinishEvent()
    {
        //加战略进度
        Self.InfoDetail.GC.StrC.SolveStrRequest(6, 1);

        int a = Random.Range(2, 13);
        string Ns = Self.InfoDetail.emp.Name;
        string Nt = "";
        Employee Es = Self.InfoDetail.emp;
        Employee Et = null;
        if (HaveTarget == true)
        {
            Et = Target.InfoDetail.emp;
            Nt = Et.Name;
            Relation Rt = Et.FindRelation(Self.InfoDetail.emp);
            if (Rt.FriendValue > 0)
                a += 1;
            else if (Rt.FriendValue < 0)
                a -= 2;
            if (Rt.LoveValue > 0)
                a += 2;
            if (Rt.MasterValue > 0)
                a += 1;

            float c1 = Et.Character[0];
            float c2 = Es.Character[0];
            float b1 = Et.Character[1];
            float b2 = Es.Character[1];

            if ((c1 * c2) >= 0)
                a += 1;
            else
                a -= 1;

            if ((b1 * b2) >= 0)
                a += 1;
            else
                a -= 1;
        }
        int m = Self.InfoDetail.GC.Morale;
        if (m >= 80)
            a += 2;
        else if (m >= 60)
            a += 1;
        else if (m < 40)
            a -= 1;
        else if (m < 20)
            a -= 2;

        if (Type == 1)
        {
            if (Value == -1)
            {
                if (a <= 2)
                {
                    Self.InfoDetail.AddHistory(Ns + "将咖啡倒入口中的一瞬间，眼前仿佛看到了幻觉，体内一些奇怪的改变发生了");
                    EventResult(1);
                }
                else if (a <= 6)
                {
                    Es.Mentality -= 5;
                    Self.InfoDetail.AddHistory(Ns + "咖啡杯跌落，咖啡洒到了衣服上，并将手烫了个大泡");
                    EventResult(2);
                }
                else if (a < 11)
                {
                    Es.Stamina += 1;
                    Self.InfoDetail.AddHistory(Ns + "悠然地喝了一口咖啡，体力也有所恢复");
                    EventResult(3);
                }
                else
                {
                    Es.Stamina += 3;
                    Self.InfoDetail.AddHistory(Ns + "喝到咖啡之后，一股电流忽从脑部流向全身，突然大喊着“我神功大成了！”");
                    EventResult(4);
                }
            }
            else if(Value == 1)
            {
                float v = Et.HR * 0.5f;
                a += (int)v;
                if (a <= 2)
                {
                    Es.Mentality -= 5;
                    Self.InfoDetail.AddHistory(Ns + "向HR" + Nt + "倾诉时，HR睡着了，于是十分愤怒的摔门而去，可是HR依然没有醒来，心力-5");
                    Target.InfoDetail.AddHistory(Nt + "睡着了，没有听到" + Ns + "的倾诉");
                    EventResult(1);
                }
                else if (a <= 5)
                {
                    Self.InfoDetail.AddHistory(Ns + "向HR" + Nt + "讲述了自己如何从意识流角度进行自我后现代性剖析，HR表示没有听懂");
                    Target.InfoDetail.AddHistory(Nt + "对" + Ns + "的倾诉表示没有听懂");
                    EventResult(2);
                }
                else if (a < 11)
                {
                    Es.Mentality += 10;
                    Self.InfoDetail.AddHistory(Ns + "向HR" + Nt + "寻求了一些工作上的建议，心力+10");
                    Target.InfoDetail.AddHistory(Nt + "给了" + Ns + "一些工作上的建议");
                    EventResult(3);
                }
                else
                {
                    Es.Mentality += 20;
                    Self.InfoDetail.AddHistory(Ns + "向HR" + Nt + "寻求答案，并从HR的话里感受到了智者的禅意，颇有开悟之感，心力+20");
                    Target.InfoDetail.AddHistory(Nt + "给了" + Ns + "一些建议，使他颇有开悟之感");
                    EventResult(4);
                }
            }
        }
        else if(Type == 2)
        {
            if (Value == -1)
            {
                float v = Es.Charm * 0.5f;
                a += (int)v;
                if (a <= 4)
                {
                    Et.Mentality -= 10;
                    Es.ChangeRelation(Et, -5);
                    Et.ChangeRelation(Es, -5);
                    Self.InfoDetail.AddHistory(Ns + "向" + Nt + "指出应该多向自己学习，认为对方太年轻，太幼稚，应该提高知识水平，好感度-5");
                    Target.InfoDetail.AddHistory(Nt + "对" + Ns + "的指点表示无语，心力-10，好感度-5");
                    EventResult(1);
                }
                else if (a <= 8)
                {
                    Es.ChangeRelation(Et, -5);
                    Et.ChangeRelation(Es, -5);
                    Self.InfoDetail.AddHistory(Ns + "向" + Nt + "讲述了自己当年的英雄往事，对方不以为然的感慨“幸亏你的膝盖中了一箭”，好感度-5");
                    Target.InfoDetail.AddHistory(Nt + "对" + Ns + "讲的故事感慨：“幸亏你的膝盖中了一剑”，好感度-5");
                    EventResult(2);
                }
                else if (a <= 12)
                {
                    Es.ChangeRelation(Et, 5);
                    Et.ChangeRelation(Es, 5);
                    Self.InfoDetail.AddHistory(Ns + "向" + Nt + "展示自己与大佬们的合影，对方表示感到惊奇，好感度+5");
                    Target.InfoDetail.AddHistory(Nt + "对" + Ns + "与大佬们的合影感到惊奇，好感度+5");
                    EventResult(3);
                }
                else
                {
                    Es.ChangeRelation(Et, 10);
                    Et.ChangeRelation(Es, 10);
                    Self.InfoDetail.AddHistory(Ns + "向" + Nt + "展示自己对于前沿领域的应用和深刻理解，使对方惊呼大佬，好感度+10");
                    Target.InfoDetail.AddHistory(Nt + "对" + Ns + "的展示惊呼大佬，好感度+10");
                    EventResult(4);
                }
            }
            else if (Value == 1)
            {
                float v = Es.Charm * 0.5f;
                a += (int)v;
                if (a <= 2)
                {
                    Et.Mentality -= 10;
                    Es.ChangeRelation(Et, -5);
                    Et.ChangeRelation(Es, -5);
                    Self.InfoDetail.AddHistory(Ns + "与" + Nt + "聊天时，不小心把口水喷到了对方脸上，场面一度非常尴尬，好感度-5");
                    Target.InfoDetail.AddHistory(Nt + "被" + Ns + "的口水喷到了脸上，场面一度非常尴尬，心力-10，好感度-5");
                    EventResult(1);
                }
                else if (a <= 5)
                {
                    Es.ChangeRelation(Et, -5);
                    Et.ChangeRelation(Es, -5);
                    Self.InfoDetail.AddHistory(Ns + "与" + Nt + "聊天时，紧张的说不出话来，哆哆嗦嗦的寒暄了半天，好感度-5");
                    Target.InfoDetail.AddHistory(Nt + "对" + Ns + "紧张得说不出话的表现感到无语，好感度-5");
                    EventResult(2);
                }
                else if (a < 11)
                {
                    Es.ChangeRelation(Et, 5);
                    Et.ChangeRelation(Es, 5);
                    Self.InfoDetail.AddHistory(Ns + "与" + Nt + "聊起了很多感兴趣的话题，彼此感到非常愉快，好感度+5");
                    Target.InfoDetail.AddHistory(Nt + "与" + Ns + "聊天非常愉快，好感度+5");
                    EventResult(3);
                }
                else
                {
                    Es.ChangeRelation(Et, 10);
                    Et.ChangeRelation(Es, 10);
                    Self.InfoDetail.AddHistory(Ns + "与" + Nt + "聊起了很多童年往事，隐隐感到彼此心有灵犀，好感度+10");
                    Target.InfoDetail.AddHistory(Nt + "感到与" + Ns + "心有灵犀，好感度+10");
                    EventResult(4);
                }
            }
        }
        else if(Type == 3)
        {
            if (Value == -1)
            {
                int Check1 = Es.Skill1 + Es.Skill2 + Es.Skill3;
                int Check2 = Es.Manage + Es.HR + Es.Finance;
                int Check3 = Es.Decision + Es.Forecast + Es.Strategy;
                if (Check1 > 10)
                    a += 2;
                if (Check2 > 10)
                    a += 2;
                if (Check3 > 10)
                    a += 2;

                if (Self.InfoDetail.CalcSalary() > 15)
                    a -= 4;
                else if (Self.InfoDetail.CalcSalary() > 10)
                    a -= 2;

                float v = Es.Convince * 0.5f;
                a += (int)v;

                if (a <= 2)
                {
                    Es.Mentality -= 20;
                    Es.Character[4] -= 1;
                    if (Es.Character[4] < 0)
                        Es.Character[4] = 0;

                    Self.InfoDetail.AddHistory(Ns + "试图与" + Nt + "要求上涨工资，却被上司羞辱其能力低下，心力-20，信念-1");
                    Target.InfoDetail.AddHistory(Nt + "羞辱了" + Ns + "能力低下");
                    EventResult(1);
                }
                else if (a <= 6)
                {
                    Es.Character[2] -= 0.15f;
                    if (Es.Character[2] < -3)
                        Es.Character[2] = -3;
                    Self.InfoDetail.AddHistory(Ns + "试图与" + Nt + "要求上涨工资，但是以能力不足为由被上司婉言拒绝，功利主义+5%");
                    Target.InfoDetail.AddHistory(Nt + "认为" + Ns + "能力不足");
                    EventResult(2);
                }
                else if (a < 11)
                {
                    Es.SalaryExtra += (int)(Self.InfoDetail.CalcSalary() * 0.2f);
                    Self.InfoDetail.AddHistory(Ns + "试图与" + Nt + "要求上涨工资，上司对其成绩表示认可并同意上报，工资+20%");
                    Target.InfoDetail.AddHistory(Nt + "认可" + Ns + "的工作能力，并同意上报涨工资请求");
                    EventResult(3);
                }
                else
                {
                    Es.SalaryExtra += (int)(Self.InfoDetail.CalcSalary() * 0.4f);
                    Self.InfoDetail.AddHistory(Ns + "试图与" + Nt + "要求上涨工资，上司对其最近表现大加赞赏，表示其工资要求太低，工资+40%");
                    Target.InfoDetail.AddHistory(Nt + "对" + Ns + "最近的表现大加赞赏，表示其要求的工资太低");
                    EventResult(4);
                }
            }
            else if (Value == -2)
            {
                int Check2 = Es.Manage + Es.HR + Es.Finance;
                int Check3 = Es.Decision + Es.Forecast + Es.Strategy;
                if (Check2 > 15)
                    a += 2;
                else if (Check2 > 10)
                    a += 1;
                if (Check3 > 10)
                    a += 2;

                float v = Es.Convince * 0.5f;
                a += (int)v;
                if (a <= 3)
                {
                    Es.Character[4] -= 1;
                    if (Es.Character[4] < 0)
                        Es.Character[4] = 0;
                    Es.Character[2] -= 0.6f;
                    if (Es.Character[2] < -3)
                        Es.Character[2] = -3;

                    Self.InfoDetail.AddHistory(Ns + "试图向上司" + Nt + "展示自己卓越的领导才能，结果上司却笑出了声，信念-1，功利主义+20%");
                    Target.InfoDetail.AddHistory(Nt + "对" + Ns + "的表现笑出了声");
                    EventResult(1);
                }
                else if (a <= 8)
                {
                    Es.Character[0] -= 0.3f;
                    if (Es.Character[0] < -3)
                        Es.Character[0] = -3;
                    Self.InfoDetail.AddHistory(Ns + "试图向上司" + Nt + "争取更高的管理职位，却被认为是好高骛远，独裁+10%");
                    Target.InfoDetail.AddHistory(Nt + "认为" + Ns + "好高骛远");
                    EventResult(2);
                }
                else if (a < 12)
                {
                    Self.InfoDetail.AddHistory(Ns + "有条不紊地向上司" + Nt + "展示了自己最近的进展，并提出可以进一步谋求更大的发展机会，得到上司认可(目前无效！！)");
                    Target.InfoDetail.AddHistory(Nt + "认可了" + Ns + "最近的进展");
                    EventResult(3);
                }
                else
                {
                    Self.InfoDetail.AddHistory(Ns + "向上司" + Nt + "展示了自己强劲的领导才能，并表达了无论什么岗位始终维护上级的决心，被大加赞赏(目前无效！！)");
                    Target.InfoDetail.AddHistory(Nt + "对" + Ns + "的表现大加赞赏，并调整了他的工作岗位");
                    EventResult(4);
                }
            }
            else if (Value == -3)
            {
                float v = Es.Convince * 0.5f;
                a += (int)v;
                v = Es.Charm * 0.5f;
                a += (int)v;
                v = Es.Gossip * 0.5f;
                a += (int)v;

                if (a <= 3)
                {
                    Es.Character[4] -= 1;
                    if (Es.Character[4] < 0)
                        Es.Character[4] = 0;
                    Es.Character[0] -= 0.6f;
                    if (Es.Character[0] < -3)
                        Es.Character[0] = -3;

                    Self.InfoDetail.AddHistory(Ns + "试图与" + Nt + "沟通建立一个公司内部派系，却遭到对方的疯狂嘲讽，被称为痴人说梦的野心家，信念-1，独裁+20%");
                    Target.InfoDetail.AddHistory(Nt + "嘲讽" + Ns + "就是一个痴人说梦的野心家");
                    EventResult(1);
                }
                else if (a <= 8)
                {
                    Self.InfoDetail.AddHistory(Ns + "试图与" + Nt + "建立一个公司内部派系，对方对这种想法似乎不以为然(没效果??)");
                    Target.InfoDetail.AddHistory(Nt + "对" + Ns + "建立派系的想法不以为然");
                    EventResult(2);
                }
                else if (a < 12)
                {
                    Self.InfoDetail.AddHistory(Ns + "试图与" + Nt + "建立一个公司内部派系，并获得了认同，从此二人将会共建派系彼此照应(目前无效！！)");
                    Target.InfoDetail.AddHistory(Nt + "与" + Ns + "两人将会共建派系彼此照应");
                    EventResult(3);
                }
                else
                {
                    Self.InfoDetail.AddHistory(Ns + "说出了与" + Nt + "建立一个公司内部派系，令对方十分惊异其雄图大志，并同意共建派系(目前无效！！)");
                    Target.InfoDetail.AddHistory(Nt + "十分惊诧于" + Ns + "的雄图大志，并同意与之共建派系");
                    EventResult(4);
                }
            }
            else if (Value == 1)
            {
                float v = Es.Convince * 0.5f;
                a += (int)v;
                if (a <= 2)
                {
                    Et.Mentality -= 10;
                    Self.InfoDetail.AddHistory(Ns + "对" + Nt + "的工作指手画脚，横加干涉，使对方难以忍耐");
                    Target.InfoDetail.AddHistory(Nt + "难以忍受" + Ns + "，心力-10");
                    EventResult(1);
                }
                else if (a <= 7)
                {
                    Es.Mentality -= 5;
                    Self.InfoDetail.AddHistory(Ns + "对" + Nt + "的工作主动提出了一些自己看法，对方表示不以为然，心力-5");
                    Target.InfoDetail.AddHistory(Nt + "对" + Ns + "的看法不以为然");
                    EventResult(2);
                }
                else if (a < 12)
                {
                    Es.Character[0] += 0.3f;
                    if (Es.Character[0] > 3)
                        Es.Character[0] = 3;
                    Self.InfoDetail.AddHistory(Ns + "对" + Nt + "的工作提出了一些建议，对方欣然接受，开源+10%");
                    Target.InfoDetail.AddHistory(Nt + "欣然接受了" + Ns + "的建议");
                    EventResult(3);
                }
                else
                {
                    Self.InfoDetail.AddHistory(Ns + "帮助" + Nt + "捋清了整个业务流程，使对方通体舒爽，如沐春风(目前无效！！)");
                    Target.InfoDetail.AddHistory(Nt + "对" + Ns + "的帮助感到通体舒爽，如沐春风");
                    EventResult(4);
                }
            }
            else if (Value == 2)
            {
                float v = Es.Convince * 0.5f;
                a += (int)v;
                if (a <= 2)
                {
                    Es.Character[3] -= 0.6f;
                    if (Es.Character[3] < -3)
                        Es.Character[3] = -3;
                    Self.InfoDetail.AddHistory(Ns + "试图用来自热带的土著舞蹈引起" + Nt + "的注意，并给他一些工作上的启发，对方不知道说什么好，随心所欲+20%");
                    Target.InfoDetail.AddHistory(Nt + "对" + Ns + "的表演不知道说什么好");
                    EventResult(1);
                }
                else if (a <= 7)
                {
                    Es.Character[0] -= 0.3f;
                    if (Es.Character[0] < -3)
                        Es.Character[0] = -3;
                    Self.InfoDetail.AddHistory(Ns + "滔滔不绝地提出了十条帮助" + Nt + "的建议，对方表示其根本没明白自己在讲什么，独裁+10%");
                    Target.InfoDetail.AddHistory(Nt + "根本没听明白" + Ns + "在讲什么");
                    EventResult(2);
                }
                else if (a < 12)
                {
                    Self.InfoDetail.AddHistory(Ns + "在" + Nt + "提到工作的困境时，使对方突然获得了巨大灵感(目前无效！！)");
                    Target.InfoDetail.AddHistory(Nt + "从" + Ns + "的话语中获得了巨大的灵感");
                    EventResult(3);
                }
                else
                {
                    Self.InfoDetail.AddHistory(Ns + "帮助" + Nt + "用更高维度视角对当前的困难降维打击，一个神奇的清晰图景立即浮现(目前无效！！)");
                    Target.InfoDetail.AddHistory(Nt + "在" + Ns + "的帮助下，脑中浮现出一个神奇的清晰图景");
                    EventResult(4);
                }
            }
            else if (Value == 3)
            {
                if (a <= 2)
                {
                    Et.Mentality -= 10;
                    Et.Character[3] -= 0.3f;
                    if (Et.Character[3] < -3)
                        Et.Character[3] = -3;
                    Self.InfoDetail.AddHistory(Ns + "向" + Nt + "阐述了热力学第二定律与当前工作问题的关系和解决方法，使对方感到根本听不懂");
                    Target.InfoDetail.AddHistory(Nt + "对" + Ns + "的讲述表示完全听不懂，心力-10，随心所欲+10%");
                    EventResult(1);
                }
                else if (a <= 7)
                {
                    Self.InfoDetail.AddHistory(Ns + "向" + Nt + "提出一起来通过倒立的方式头脑风暴，对方表示这样容易脑溢血并拒绝了他(没效果??)");
                    Target.InfoDetail.AddHistory(Nt + "对" + Ns + "的建议表示这样容易脑溢血，并拒绝了他");
                    EventResult(2);
                }
                else if (a < 12)
                {
                    Self.InfoDetail.AddHistory(Ns + "向" + Nt + "提出一起头脑风暴来解决问题，二人非常创造性的讨论了很多问题(目前无效！！)");
                    Target.InfoDetail.AddHistory(Nt + "对" + Ns + "创造性地讨论了许多问题");
                    EventResult(3);
                }
                else
                {
                    Self.InfoDetail.AddHistory(Ns + "在与" + Nt + "头脑风暴时感到彼此灵魂激荡，“等价交换！”，一扇真理之门似乎在他们面前显现(目前无效！！)");
                    Target.InfoDetail.AddHistory(Nt + "对" + Ns + "感到一扇真理之门似乎在他们面前显现");
                    EventResult(4);
                }
            }
        }
        else
        {
            if (Value == 1)
            {

                float v = Es.HR * 0.5f;
                a += (int)v;

                if (a <= 4)
                {
                    Et.Mentality -= 30;
                    Self.InfoDetail.AddHistory(Ns + "试图与" + Nt + "大力宣扬人类中心主义，并称非人类的一切都毫无价值，引起对方的剧烈反感");
                    Target.InfoDetail.AddHistory(Nt + "的价值观宣传引起了" + Ns + "的剧烈反感，心力-30");
                    EventResult(1);
                }
                else if (a <= 8)
                {
                    Et.Mentality -= 10;
                    Self.InfoDetail.AddHistory(Ns + "就人类的神圣地位和人工弱智能的关系向" + Nt + "发表论断，引发二人的不快争执");
                    Target.InfoDetail.AddHistory(Nt + "与" + Ns + "产生争执，心力-10");
                    EventResult(2);
                }
                else if (a < 12)
                {
                    Et.Character[1] += 0.3f;
                    if (Et.Character[1] > 3)
                        Et.Character[1] = 3;
                    Self.InfoDetail.AddHistory(Ns + "向" + Nt + "讲解了人类如何与机械共同进化的话题，引起对方的强烈兴趣");
                    Target.InfoDetail.AddHistory(Nt + "的价值观宣传引起了" + Ns + "的强烈兴趣，人文主义+10%");
                    EventResult(3);
                }
                else
                {
                    Et.Mentality += 20;
                    Et.Character[1] += 0.3f;
                    if (Et.Character[1] > 3)
                        Et.Character[1] = 3;
                    Es.SalaryExtra += (int)(Self.InfoDetail.CalcSalary() * 0.4f);
                    Self.InfoDetail.AddHistory(Ns + "向" + Nt + "分享了人性的美好与光辉，使对方因人类的演化之美和混沌性，流下感动的热泪");
                    Target.InfoDetail.AddHistory(Nt + "因" + Ns + "的宣传流下了感动的热泪，心力+20，人文主义+10%");
                    EventResult(4);
                }
            }
            else if (Value == -1)
            {

                float v = Es.HR * 0.5f;
                a += (int)v;

                if (a <= 4)
                {
                    Et.Mentality -= 30;
                    Self.InfoDetail.AddHistory(Ns + "向" + Nt + "大力宣扬机械飞升主义，并称人类终将毁灭，世界属于数据，引起对方的剧烈反感");
                    Target.InfoDetail.AddHistory(Nt + "的价值观宣传引起了" + Ns + "的剧烈方案，心力-30");
                    EventResult(1);
                }
                else if (a <= 8)
                {
                    Et.Mentality -= 10;
                    Self.InfoDetail.AddHistory(Ns + "向" + Nt + "发表了人类本质邪恶而脆弱的见解，导致二人谈话气氛十分尴尬");
                    Target.InfoDetail.AddHistory(Nt + "与" + Ns + "的谈话氛围十分尴尬，心力-10");
                    EventResult(2);
                }
                else if (a < 12)
                {
                    Et.Character[1] -= 0.3f;
                    if (Et.Character[1] < -3)
                        Et.Character[1] = -3;
                    Self.InfoDetail.AddHistory(Ns + "向" + Nt + "讲解了人工智能对于生命意义的新的诠释，引起对方的强烈兴趣");
                    Target.InfoDetail.AddHistory(Nt + "的价值观宣传引起了" + Ns + "的强烈兴趣，机械飞升+10%");
                    EventResult(3);
                }
                else
                {
                    Et.Mentality += 20;
                    Et.Character[1] -= 0.3f;
                    if (Et.Character[1] < -3)
                        Et.Character[1] = -3;
                    Es.SalaryExtra += (int)(Self.InfoDetail.CalcSalary() * 0.4f);
                    Self.InfoDetail.AddHistory(Ns + "向" + Nt + "描绘了一个由数据和理性构成的完美乌托邦的新图景，使对方流连于对未来都市的想象并振奋不已");
                    Target.InfoDetail.AddHistory(Nt + "被" + Ns + "的价值观宣传打动，振奋不已，心力+20，机械飞升+10%");
                    EventResult(4);
                }
            }
        }

        Self.CurrentEvent.Remove(this);
        if (Target != null)
        {
            Target.ReceivedEvent.Remove(this);
            if (Target.EventCheck() == true)
                Target.SetTarget();
        }
    }
    public bool FindTarget()
    {
        if (Self.InfoDetail.GC.CurrentEmployees.Count > 1)
        {
            while (Target == null || Target == Self)
            {
                int r = Random.Range(0, Self.InfoDetail.GC.CurrentEmployees.Count);
                Target = Self.InfoDetail.GC.CurrentEmployees[r].InfoDetail.Entity;
            }
            return true;
        }
        return false;
    }

    public void EventResult(int value)
    {
        if (value == 1)
            MonoBehaviour.print("大失败");
        else if (value == 2)
            MonoBehaviour.print("失败");
        else if (value == 3)
            MonoBehaviour.print("成功");
        else if (value == 4)
            MonoBehaviour.print("大成功");
        Self.ShowMarker(value);
    }
}

