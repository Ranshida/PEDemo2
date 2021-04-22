using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmpEntity : MonoBehaviour
{
    public string EmpName;
    public int FaceType = 0;
    public Employee ThisEmp
    {
        get
        {
            if (InfoDetail == null)
            {
                return null;
            } 
            return InfoDetail.emp;
        } 
    }
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
            if (ThisEmp == null)  
                return null;
            
            if (ThisEmp.InfoDetail.emp.CurrentDep != null)
            {
                return ThisEmp.InfoDetail.emp.CurrentDep.building;
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
            }
            return new Vector3Value(false);
        }
    }    //员工工作的位置（如果没有工作建筑则没有值）

    public bool Fired { get; private set; }                   //已经被开除
    public bool OutCompany { get; private set; } = false;      //离开公司

    //事件
    public bool NewEventFlag;       //接受到新事件，但还没有开始执行
    public bool Available { get { return CurrentEvent == null && !OutCompany; } }      //员工为可用状态
    public Event CurrentEvent { get; private set; } = null;  //当前正在执行的事件
    public EmpEntity EventTarget { get; private set; }       //事件发生的对象
    public int EventStage { get; private set; } = 0;         //事件发生的阶段   0：无事件  1：确定目标  2：发生中

    public int LastHour;

    //上下班
    public bool OffWork { get; private set; }                //下班

    //闲逛
    public bool WalkAround { get { return !WorkPosition.HasValue && ThisEmp != null; } }   //没有工作时闲逛
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
        EmpName = ThisEmp.Name;
        transform.parent.name = EmpName;
    }
    
    public void TimePass()
    {
        //原先事件时间在这里减少
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
        //if (CurrentEvent != null)
        //{
        //    EventFinish();
        //}
    }

    //上班
    public void WorkStart()
    {
        //Debug.Log("上");
        OffWork = false;
        //FindWorkPos();
    }


    //进入忙碌状态
    public void SetBusy()
    {
        OutCompany = true;
        //当前事件直接成功
        //if (CurrentEvent != null)
        //{
        //    EventFinish();
        //}

    }

    //解除忙碌状态
    public void SetFree()
    {
        OutCompany = false;
    }

    //移除员工，清除全部事件
    public void RemoveEntity()
    {
        InfoDetail = null;
        Fired = true;
        OffWork = true;
    }
    public void Remove()
    {
        Destroy(transform.parent.gameObject);
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
        if (InfoDetail)
        {
            InfoDetail.ShowPanel();
        }
    }


}