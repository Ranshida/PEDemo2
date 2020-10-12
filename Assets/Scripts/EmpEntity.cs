using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmpEntity : MonoBehaviour
{
    public string EmpName { get; private set; }

    public bool Available { get { return CurrentEvent == null; } }      //员工为可用状态
    public bool SolvingEvent { get { return !Available && CurrentEvent.isSolving; } }     //事件正在执行中
    public bool HasEvent { get { return EventList.Count > 0; } }        //事件列表不为空
    public bool Movable;        //可移动
    public Event CurrentEvent;
    public List<Event> EventList = new List<Event>();

    private Vector3 offset = new Vector3(0, 2.2f, 2.2f);
    private Transform mesh;
    public MeshRenderer Renderer { get; private set; }
    private BehaviorTree selfTree;
    public EmpInfo InfoDetail;

    public bool OffWork { get; private set; }  //下班

    public BuildingManage BM;

    //int WaitHour = 0;
    //移动目标位置
    public Vector3 Destination;

    public void Init()
    {
        mesh = transform.parent.Find("Mesh");
        Renderer = mesh.GetComponent<MeshRenderer>();
        selfTree = gameObject.AddComponent<BehaviorTree>();
        selfTree.ExternalBehavior = Resources.Load<ExternalBehavior>("BehaviourTree/Emp");
        selfTree.SetVariableValue("EmpEntity", this);
        selfTree.Start();
    }

    void Update()
    {
        //模型只同步位置，不同步旋转
        mesh.position = this.transform.position + offset;


        //if (TargetEmp != null && WorkShift == false)
        //    Destination = TargetEmp.transform.position;

        //if (canMove == true)
        //{
        //    //移动
        //    if (Vector2.Distance((Vector2)transform.position, Destination) > 2 * InfoDetail.GC.TimeMultiply)
        //    {
        //        Vector2 Dir = (Destination - (Vector2)transform.position).normalized;
        //        transform.Translate(Dir * Time.deltaTime * InfoDetail.GC.TimeMultiply * MoveSpeed);
        //    }
        //    //到达目的地
        //    else
        //    {
        //        canMove = false;
        //        transform.position = Destination;

        //        if (WorkShift == true)
        //        {
        //            //下班
        //            if (canChangePos == false)
        //                InfoDetail.GC.WorkEndCheck();
        //            //上班后回到工位
        //            else
        //            {
        //                WorkShift = false;
        //                SetTarget();
        //            }
        //        }
        //        //待命乱逛
        //        else if (InfoDetail.emp.CurrentDep == null && InfoDetail.emp.CurrentOffice == null && TargetEmp == null)
        //        {
        //            canMove = false;
        //            WaitHour = Random.Range(1, 3);
        //        }
        //    }
        //}
    }

    public void SetInfo(EmpInfo detail)
    {
        InfoDetail = detail;
        detail.Entity = this;
        detail.GC.HourEvent.AddListener(TimePass);
        BM = detail.GC.BM;
        EmpName = detail.emp.Name;
    }
    private void TimePass()
    {

    }

    public void CheckEvents()
    {
        if (CurrentEvent != null) 
        {
            Debug.Log("当然仍有在执行的事件");
            return;
        }

        foreach (Event e in EventList)
        {
            //有目标的事件
            if (e.HaveTarget)
            {
                if (e.Target.InfoDetail.Entity.Available)
                {
                    CurrentEvent = e;
                    e.Target.InfoDetail.Entity.CurrentEvent = e;
                    break;
                }
            }
            //独立事件
            else
            {
                CurrentEvent = e;
                break;
            }
        }
    }

    //private void EventTimePass()
    //{
    //    for (int i = 0; i < CurrentEvent.Count; i++)
    //    {
    //        if (CurrentEvent[i].EventActive == true)
    //        {
    //            CurrentEvent[i].Time -= 1;
    //            if (CurrentEvent[i].Time <= 0)
    //            {
    //                CurrentEvent[i].FinishEvent();
    //                if (EventCheck() == true)
    //                {
    //                    SetTarget();
    //                }
    //            }
    //        }
    //    }
    //}
    //下班

    public void WorkEnd()
    {
        OffWork = true;
    }

    //上班
    public void WorkStart()
    {
        OffWork = false;
        //FindWorkPos();
    }

    //添加事件
    public void AddEvent(Event e)
    {
        //当前有待执行事件，先加入列表
        if (CurrentEvent != null)
            EventList.Add(e);
        else
        {
            //有目标的事件
            if (e.HaveTarget)
            {
                if (Available && e.Target.InfoDetail.Entity.Available)
                {
                    CurrentEvent = e;
                    e.Target.InfoDetail.Entity.CurrentEvent = e;
                }
                else
                {
                    EventList.Add(e);
                }
            }
            else
            {
                if (Available)
                {
                    CurrentEvent = e;
                }
                else
                {
                    EventList.Add(e);
                }
            }
        }
    }

    //去当间谍
    public void BecomeSpy()
    {

    }

    //移除员工，清除全部事件
    public void RemoveEntity()
    {

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

    private void OnTriggerStay2D(Collider2D collision)
    {
        //接触目标
        //if (TargetEmp != null && WorkShift == false)
        //{
        //    EmpEntity Et = collision.gameObject.GetComponent<EmpEntity>();
        //    if (TargetEmp == Et)
        //    {
        //        for (int i = 0; i < CurrentEvent.Count; i++)
        //        {
        //            if (CurrentEvent[i].Target == TargetEmp)
        //            {
        //                CurrentEvent[i].EventActive = true;
        //                canMove = false;
        //                TargetEmp.canMove = false;
        //            }
        //        }
        //    }
        //}
    }

    public void ShowDetailPanel()
    {
        InfoDetail.ShowPanel();
    }

    //public void ShowName()
    //{
    //    Text_Name.transform.parent.gameObject.SetActive(true);
    //}

    //public void HideName()
    //{
    //    Text_Name.transform.parent.gameObject.SetActive(false);
    //}
}
