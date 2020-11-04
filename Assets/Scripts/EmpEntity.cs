using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmpEntity : MonoBehaviour
{
    public string EmpName { get; private set; }
    private BehaviorTree selfTree;
    public MeshRenderer Renderer { get; private set; }
    public EmpInfo InfoDetail;
    private Vector3 offset = new Vector3(0, 2.2f, 2.2f);
    private Transform mesh;

    public bool SpecialWork { get { return IsSpying; } }
    public bool IsSpying { get; private set; } = false;
    private int SpyTimer = 4;
    public bool OffWork { get; private set; }  //下班
    public Event CurrentEvent { get; private set; } = null;  //当前正在执行的事件
    public List<Event> EventList = new List<Event>();  //存储的待执行事件列表
    public FOECompany SpyTarget;

    public bool Available { get { return CurrentEvent == null && !IsSpying; } }      //员工为可用状态
    public bool SolvingEvent { get { return CurrentEvent != null && CurrentEvent.isSolving; } }     //事件正在执行中
    public bool HasEvent { get { return EventList.Count > 0; } }        //事件列表不为空

    //debug
    public string AvailableDebug;      //员工为可用状态
    public int EventCount;        //事件列表不为空
    public int EventTimer;

    public void Init()
    {
        mesh = transform.parent.Find("Mesh");
        Renderer = mesh.GetComponent<MeshRenderer>();
        selfTree = gameObject.AddComponent<BehaviorTree>();
        selfTree.ExternalBehavior = Resources.Load<ExternalBehavior>("BehaviourTree/Emp");
        selfTree.SetVariableValue("EmpEntity", this);
        selfTree.Start();
        CurrentEvent = null;
    }

    void Update()
    {
        //模型只同步位置，不同步旋转
        mesh.position = this.transform.position + offset;
        EventCount = EventList.Count;

        if (CurrentEvent== null)
            AvailableDebug = "空闲";
        else if (!CurrentEvent.HaveTarget)
            AvailableDebug = "独立事件";
        else if (CurrentEvent.SelfEntity == this)
            AvailableDebug = "主动事件";
        else
            AvailableDebug = "被动事件";

        if (CurrentEvent == null)
            EventTimer = 0;
        else
            EventTimer = CurrentEvent.TimeLeft;

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
        EmpName = detail.emp.Name;
        transform.parent.name = EmpName;
    }
    private void TimePass()
    {
        if (IsSpying)
        {
            SpyTimer--;
            if (SpyTimer <= 0) 
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
        if (Available)
            CheckEvents();
    }

    //员工主动发起的执行事件的指令
    public void DealtEvent()
    {
        if (CurrentEvent==null)
            Debug.LogError("无事件");

        Debug.Log("开始处理事件");
        CurrentEvent.isSolving = true;
    }

    //遍历事件，找下一个可执行的事件
    public void CheckEvents()
    {
        if (!Available) 
        {
            Debug.LogWarning("当然仍有在执行的事件");
            return;
        }

        foreach (Event e in EventList)
        {
            //独立事件
            if (!e.HaveTarget)
            {
                CurrentEvent = e;
                CurrentEvent.isSolving = true;
                EventList.Remove(CurrentEvent);
                break;
            }
            //有目标的事件，并且是主动发起
            else if (e.SelfEntity == this)
            {
                //对方空闲
                if (e.TargetEntity.Available)
                {  
                    //双方做这个事件
                    CurrentEvent = e;
                    EventList.Remove(CurrentEvent);
                    e.TargetEntity.CurrentEvent = e;
                    e.TargetEntity.EventList.Remove(CurrentEvent);
                    break;
                }
            }
        }
    }

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
        //独立事件
        if (!e.HaveTarget)
            Debug.Log(EmpName + "独立事件");
        //主动事件
        else
            Debug.Log(EmpName + "主动事件");

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
            //独立事件
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
    //寻找目标（事件版本2）
    public void AddEvent(EmpEntity Ee)
    {

    }

    //去当间谍
    public void BecomeSpy(FOECompany Target)
    {
        //放弃当前执行的事件，插到第一位
        if (CurrentEvent != null)
        {
            if (CurrentEvent.SelfEntity == this)
                CurrentEvent.isSolving = false;
            EventList.Insert(0, CurrentEvent);
            CurrentEvent = null;
        }

        IsSpying = true;
        SpyTimer = 4;

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
