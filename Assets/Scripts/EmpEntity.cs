using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmpEntity : MonoBehaviour
{
    public Employee ThisEmp { get { return InfoDetail.emp; } }
    public string EmpName { get { return ThisEmp.Name; } }
    private BehaviorTree selfTree;
    public MeshRenderer Renderer { get; private set; }
    public EmpInfo InfoDetail { get; private set; }
    private Vector3 offset = new Vector3(0, 2.2f, 2.2f);
    private Transform mesh;

    //重要属性
    public Grid StandGrid
    {
        get
        {
            Grid grid;
            if (GridContainer.Instance.GetGrid(transform.position.x, transform.position.z, out grid))
            {
                return grid;
            };
            Debug.LogError("单位没有处在任何一个格子中！");
            return null;
        }
    }               //自身所处的格子
    public Building WorkBuilding
    {
        get
        {
            if (ThisEmp.InfoDetail.emp.CurrentDep != null)
            {
                return ThisEmp.InfoDetail.emp.CurrentDep.building;
            }
            else if (ThisEmp.InfoDetail.emp.CurrentOffice != null)
            {
                return ThisEmp.InfoDetail.emp.CurrentOffice.building;
            }
            return null;
        }
    }        //员工工作的建筑
    public Vector3Value WorkPosition
    {
        get
        {
            if (WorkBuilding)
            {
                if (WorkBuilding.Department)
                {
                    return new Vector3Value(true, WorkBuilding.WorkPos[ThisEmp.CurrentDep.CurrentEmps.IndexOf(ThisEmp)].position);
                }
                if (WorkBuilding.Office)
                {
                    return new Vector3Value(true, WorkBuilding.WorkPos[0].position);
                }
            }
            return new Vector3Value(false);
        }
    }    //员工工作的位置（如果没有工作建筑则没有值）


    //间谍任务
    private int m_SpyTimer = 4;
    public bool IsSpying { get; private set; } = false;      //间谍任务中
    public FOECompany SpyTarget;                             //间谍任务目标
  
    //事件
    public bool NewEventFlag;       //接受到新事件，但还没有开始执行
    public bool Available { get { return CurrentEvent == null && !IsSpying; } }      //员工为可用状态
    public bool SolvingEvent { get { return CurrentEvent != null && CurrentEvent.isSolving; } }     //事件正在执行中
    public Event CurrentEvent { get; private set; } = null;  //当前正在执行的事件
    public EmpEntity EventTarget { get; private set; }       //事件发生的对象
    public int EventStage { get; private set; } = 0;              //事件发生的阶段   0：无事件  1：刚刚接受  2：确定目标  3：发生

    //上下班
    public bool OffWork { get; private set; }                //下班

    //闲逛
    public bool WalkAround { get { return !WorkPosition.HasValue; } }   //没有工作时闲逛
    public WayPoint NextWP { get; private set; }        //目标寻路点
    private bool m_ArriveWP;    //到达目标路点
    private float m_WPTimer;    //到达后计时

    //public List<Event> EventList = new List<Event>();        //存储的待执行事件列表
    //public bool HasEvent { get { return EventList.Count > 0; } }        //事件列表不为空
    //public bool Available { get { return CurrentEvent == null && !IsSpying; } }      //员工为可用状态
    //public bool SolvingEvent { get { return CurrentEvent != null && CurrentEvent.isSolving; } }     //事件正在执行中
    
    public void Init()
    {
        mesh = transform.parent.Find("Mesh");
        Renderer = mesh.GetComponent<MeshRenderer>();
        selfTree = gameObject.AddComponent<BehaviorTree>();
        selfTree.ExternalBehavior = Resources.Load<ExternalBehavior>("BehaviourTree/Emp");
        selfTree.SetVariableValue("EmpEntity", this);
        selfTree.Start();
        CurrentEvent = null;
        NextWP = GridContainer.Instance.AllWayPoint[Random.Range(0, GridContainer.Instance.AllWayPoint.Count)];
    }

    void Update()
    {
        //模型只同步位置，不同步旋转
        mesh.position = this.transform.position + offset;

        if (WalkAround)
        {
            //如果到达路点，则计时
            if (Function.XZDistance(transform, NextWP.transform) < 0.5f) 
                m_WPTimer += Time.deltaTime;
            //计时0.5s前往下一个路点
            if (m_WPTimer > 0.5f)
            {
                m_WPTimer = 0;
                NextWP = NextWP.GetConnect();
            }
        }
    }

    public void SetInfo(EmpInfo detail)
    {
        InfoDetail = detail;
        detail.Entity = this;
        detail.GC.HourEvent.AddListener(TimePass);
        transform.parent.name = EmpName;
    }
    private void TimePass()
    {
        if (IsSpying)
        {
            m_SpyTimer--;
            if (m_SpyTimer <= 0) 
            {
                SpyResult();
            }
        }

        //事件处理中
        if (SolvingEvent)
        {
            //独立事件
            if (!CurrentEvent.HaveTarget)
            {
                CurrentEvent.TimeLeft--;
                if (CurrentEvent.TimeLeft <= 0)
                {
                    CurrentEvent.EventFinish();
                    CurrentEvent = null;
                }
            }
            //主动方控制
            else if (CurrentEvent.SelfEntity == this)
            {
                CurrentEvent.TimeLeft--;
                if (CurrentEvent.TimeLeft <= 0)
                {
                    CurrentEvent.EventFinish();
                    //主被动双方都清理掉
                    CurrentEvent.TargetEntity.CurrentEvent = null;
                    CurrentEvent = null;
                }
            }
        }

        //空闲状态，检查新事件
        //if (Available)
        //    CheckEvents();
    }

    //员工主动发起的执行事件的指令
    public void SolveEvent()
    {
        if (CurrentEvent==null)
            Debug.LogError("无事件");

        if (!EventTarget.Available)
        {
            Debug.Log("对方不可用");
            EventStage = 0;
            CurrentEvent = null;
            return;
        }

        Debug.Log("开始处理事件");
        EventStage = 3;
        EventTarget.CurrentEvent = CurrentEvent;
        CurrentEvent.Target = EventTarget.ThisEmp;
        CurrentEvent.isSolving = true;
    }

    //遍历事件，找下一个可执行的事件
    //public void CheckEvents()
    //{
    //    if (!Available) 
    //    {
    //        Debug.LogWarning("当然仍有在执行的事件");
    //        return;
    //    }

    //    foreach (Event e in EventList)
    //    {
    //        //独立事件
    //        if (!e.HaveTarget)
    //        {
    //            CurrentEvent = e;
    //            CurrentEvent.isSolving = true;
    //            EventList.Remove(CurrentEvent);
    //            break;
    //        }
    //        //有目标的事件，并且是主动发起
    //        else if (e.SelfEntity == this)
    //        {
    //            //对方空闲
    //            if (e.TargetEntity.Available)
    //            {  
    //                //双方做这个事件
    //                CurrentEvent = e;
    //                EventList.Remove(CurrentEvent);
    //                e.TargetEntity.CurrentEvent = e;
    //                e.TargetEntity.EventList.Remove(CurrentEvent);
    //                break;
    //            }
    //        }
    //    }
    //}

    //下班
    public void WorkEnd()
    {
        //Debug.Log("下");
        //OffWork = true;

        ////放弃当前执行的事件，插到第一位
        //if (CurrentEvent != null)
        //{
        //    if (CurrentEvent.SelfEntity == this)
        //        CurrentEvent.isSolving = false;
        //    EventList.Insert(0, CurrentEvent);
        //    CurrentEvent = null;
        //}
    }

    //上班
    public void WorkStart()
    {
        //Debug.Log("上");
        //OffWork = false;
        //FindWorkPos();
    }

    //添加事件
    public void AddEvent(Event e)
    {
        Debug.LogError("这是废弃的方法了");
        //独立事件
        //if (!e.HaveTarget)
        //    Debug.Log(EmpName + "独立事件");
        ////主动事件
        //else
        //    Debug.Log(EmpName + "主动事件");

        ////当前有待执行事件，先加入列表
        //if (CurrentEvent != null)
        //    EventList.Add(e);
        //else
        //{
        //    //有目标的事件
        //    if (e.HaveTarget)
        //    {
        //        if (Available && e.Target.InfoDetail.Entity.Available)
        //        {
        //            CurrentEvent = e;
        //            e.Target.InfoDetail.Entity.CurrentEvent = e;
        //        }
        //        else
        //        {
        //            EventList.Add(e);
        //        }
        //    }
        //    //独立事件
        //    else
        //    {
        //        if (Available)
        //        {
        //            CurrentEvent = e;
        //        }
        //        else
        //        {
        //            EventList.Add(e);
        //        }
        //    }
        //}
    }

    ///寻找目标（事件版本2）
    public void AddEvent(EmpEntity Ee)
    {
        if (Ee != null)
        {
            EventStage = 1;
            EventTarget = Ee;
            print("Added");
        }
    }

    //检查自己和事件对象的位置，是否可以进行事件
    public bool CheckEventTarget()
    {
        if (EventTarget == null) 
        {
            Debug.LogError("没有事件对象");
            return false;
        }

        //对方在游荡
        if (!EventTarget.WorkBuilding)
        {
            //进走廊时为true
            if (StandGrid.Type == Grid.GridType.道路)
            {
                return true;
            }
        }
        //对方在工作
        else
        {
            //进入对方建筑时为true
            if (StandGrid.BelongBuilding == EventTarget.StandGrid.BelongBuilding)
            {
                return true;
            }
        }
        return false;
    }

    public void EventConfirm()
    {
        //当前对象不可用
        if (!EventTarget.Available)
        {
            List<Employee> tempEmployees = ThisEmp.FindAnotherEmp();
            List<Employee> AvailableEmps = new List<Employee>();
            foreach (Employee temp in tempEmployees)
            {
                if (temp.InfoDetail.Entity.Available)
                    AvailableEmps.Add(temp);
            }
            if (AvailableEmps.Count == 0)
            {
                Debug.Log("没有任何可用对象");
                EventStage = 0;
                return;
            }
            EventTarget = AvailableEmps[Random.Range(0, AvailableEmps.Count)].InfoDetail.Entity;
        }

        if(EventTarget == null)
        {
            Debug.Log("没有任何可用对象");
            EventStage = 0;
            return;
        }

        //只给自己添加事件，去找对方
        if (ThisEmp == EventTarget.ThisEmp)
            print("FalseB");
        CurrentEvent = ThisEmp.RandomEvent(EventTarget.InfoDetail.emp);
        CurrentEvent.Self = ThisEmp;
        EventStage = 2;
    }

    //去当间谍
    public void BecomeSpy(FOECompany Target)
    {
        //放弃当前执行的事件，插到第一位
        if (CurrentEvent != null)
        {
            if (CurrentEvent.SelfEntity == this)
                CurrentEvent.isSolving = false;
            //EventList.Insert(0, CurrentEvent);
            CurrentEvent = null;
        }

        IsSpying = true;
        m_SpyTimer = 4;

        //设置目标
        SpyTarget = Target;
    }

    //间谍行动结果
    public void SpyResult()
    {
        IsSpying = false;

        //间谍结果判定
        int Posb = Random.Range(1, 7);
        Posb += (int)(InfoDetail.GC.CurrentEmpInfo.emp.Strategy * 0.2f);
        if (Posb >= 4)
        {
            SpyTarget.Text_Target.gameObject.SetActive(true);
            SpyTarget.Text_SkillName.gameObject.SetActive(true);
            InfoDetail.GC.CreateMessage("内鬼行动成功,获得了" + SpyTarget.Text_CompanyName.text + "的信息");
        }
        else
            InfoDetail.GC.CreateMessage("内鬼行动失败");
        SpyTarget = null;
    }

    //移除员工，清除全部事件
    public void RemoveEntity()
    {
        Destroy(gameObject);
    }

    public void ShowTips(int index)
    {
        if (index < 1 || index > 8)
        {
            Debug.LogError("请输入1-8");
        }
        string str = "";
        switch (index)
        {
            case 1:
                str = "≖‿≖✧";
                break;
            case 2:
                str = "(ง •̀_•́)ง";
                break;
            case 3:
                str = "(:3[▓▓]";
                break;
            case 4:
                str = "╭(●｀∀´●)╯";
                break;
            case 5:
                str = "♪♪♪";
                break;
            case 6:
                str = "♪";
                break;
            case 7:
                str = "。";
                break;
            case 8:
                str = "！！！";
                break;
            default:
                break;
        }
        DynamicWindow.Instance.SetDialogue(transform, str, 3, default, Vector3.up * 10 + Vector3.right * 5);
    }

    public void ShowDetailPanel()
    {
        InfoDetail.ShowPanel();
    }
}
