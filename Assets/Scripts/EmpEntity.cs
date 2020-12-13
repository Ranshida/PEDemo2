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


    //忙碌任务
    public bool OutCompany { get; private set; } = false;      //间谍任务中
    public bool IsSpying { get; private set; }

    //事件
    public bool NewEventFlag;       //接受到新事件，但还没有开始执行
    public bool Available { get { return CurrentEvent == null && !OutCompany; } }      //员工为可用状态
    public bool SolvingEvent { get { return CurrentEvent != null && CurrentEvent.isSolving; } }     //事件正在执行中
    public Event CurrentEvent { get; private set; } = null;  //当前正在执行的事件
    public EmpEntity EventTarget { get; private set; }       //事件发生的对象
    public int EventStage { get; private set; } = 0;         //事件发生的阶段   0：无事件  1：确定目标  2：发生中

    //上下班
    public bool OffWork { get; private set; }                //下班

    //闲逛
    public bool WalkAround { get { return !WorkPosition.HasValue; } }   //没有工作时闲逛
    public WayPoint NextWP { get; private set; }        //目标寻路点
    private bool m_ArriveWP;    //到达目标路点
    private float m_WPTimer;    //到达后计时

    public string StandGridName()
    {
        if (StandGrid != null) 
        {
            if (StandGrid.BelongBuilding != null) 
            {
                return StandGrid.BelongBuilding.Type.ToString();
            }
        }
        return "走廊";
    }
    
    public void Init()
    {
        mesh = transform.parent.Find("Mesh");
        Renderer = mesh.GetComponent<MeshRenderer>();
        selfTree = gameObject.AddComponent<BehaviorTree>();
        selfTree.ExternalBehavior = Resources.Load<ExternalBehavior>("BehaviourTree/Emp");
        selfTree.SetVariableValue("EmpEntity", this);
        selfTree.Start();
        EventStage = 0;
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
            if (Function.XZDistance(transform, NextWP.transform) < 2f) 
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
    public void TimePass()
    {
        //事件处理中
        if (SolvingEvent)
        {
            //独立事件
            if (!CurrentEvent.HaveTarget)
            {
                CurrentEvent.TimeLeft--;
                if (CurrentEvent.TimeLeft <= 0)
                {
                    EventFinish();
                }
            }
            //主动方控制
            else if (CurrentEvent.SelfEntity == this)
            {
                CurrentEvent.TimeLeft--;
                if (CurrentEvent.TimeLeft <= 0)
                {
                    EventFinish();
                }
            }
        }
    }

    void EventFinish()
    {
        CurrentEvent.EventFinish();
        EventStage = 0;
        EventTarget = null;
        if (CurrentEvent.HaveTarget)
        {
            CurrentEvent.TargetEntity.EventStage = 0;
            CurrentEvent.TargetEntity.EventTarget = null;
            CurrentEvent.TargetEntity.CurrentEvent = null;
        }
        CurrentEvent = null;
    }

    //员工主动发起的执行事件的指令
    public void SolveEvent()
    {
        CurrentEvent.isSolving = true;
        EventStage = 2;

        //对方也开始处理
        if (CurrentEvent.HaveTarget)
        {
            EventTarget.EventStage = 2;
        }
    }

    //下班
    public void WorkEnd()
    {
        if (OutCompany)
        {
            return;
        }

        //Debug.Log("下");
        OffWork = true;

        //事件直接判定为完成
        if (CurrentEvent != null)
        {
            EventFinish();
        }
    }

    //上班
    public void WorkStart()
    {
        //Debug.Log("上");
        OffWork = false;
        //FindWorkPos();
    }

    ///寻找目标（事件版本2）
    public void AddEvent(EmpEntity Ee, int index)  //场景序列
    {
        if (Ee != null)
        {
            if (!EventTarget || !EventTarget.Available)
            {
                CurrentEvent = EmpManager.Instance.RandomEvent(ThisEmp, null, index);
                if (CurrentEvent!=null)
                {
                    EventStage = 1;
                    EventTarget = Ee;
                }
            }
            else
            {
                CurrentEvent = EmpManager.Instance.RandomEvent(ThisEmp, EventTarget.ThisEmp, index);
                if (CurrentEvent != null)
                {
                    EventStage = 1;
                    EventTarget = Ee;
                    EventTarget.EventStage = 1;
                    EventTarget.EventTarget = this;
                    EventTarget.CurrentEvent = CurrentEvent;
                }
            }
            if (CurrentEvent != null) 
            {
                CurrentEvent.PerkRemoveCheck();

                if (CurrentEvent.HaveTarget && CurrentEvent.Target == null)
                {
                    Debug.LogError("有目标的事件，但是没有目标对象" + CurrentEvent.EventName);
                }
            }
        }
    }

    public void AddEvent(Event e)  //废弃
    {
        return;
    }

    //进入忙碌状态
    public void SetBusy()
    {
        OutCompany = true;
        //当前事件直接成功
        if (CurrentEvent != null)
        {
            EventFinish();
        }

    }

    //解除忙碌状态
    public void SetFree()
    {
        OutCompany = false;
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
