using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 员工类的临时管理器
/// </summary>
public class EmpManager : MonoBehaviour
{
    public static EmpManager Instance { get; private set; }
    private GameObject empPrefabs;
    private Material[] empMaterials;
    private EmpEntity pointEmp;

    public Transform EventPanel;

    public bool SystemPause = false;

    private void Awake()
    {
        Instance = this;
        empPrefabs = ResourcesLoader.LoadPrefab("Prefabs/Employee/Emp");
        empMaterials = ResourcesLoader.LoadAll<Material>("Image/Employee/Materials");
    }

    private void Start()
    {
    
    }

    private void Update()
    {
        pointEmp = null;

        if (CameraController.CharacterHit && !CameraController.IsPointingUI)
        {
            pointEmp = CameraController.CharacterRaycast.collider.transform.parent.parent.GetComponentInChildren<EmpEntity>();
            DynamicWindow.Instance.SetEmpName(pointEmp.EmpName, pointEmp.transform, Vector3.up * 10);
        }
        if (pointEmp && Input.GetMouseButtonDown(1))
            pointEmp.ShowDetailPanel();
    }

    public void EventFinish(Event currentEvent)
    {
        if (currentEvent.IsJudgeEvent)
        {
            SystemPause = true;
            EventPanel.gameObject.SetActive(true);
            Transform childPanel = EventPanel.Find("Panel");
            childPanel.Find("Txt_Title").GetComponent<Text>().text = currentEvent.EventName;
            childPanel.Find("Txt_Description").GetComponent<Text>().text = currentEvent.Self + "发生了事件" + currentEvent.EventName;
            childPanel.Find("Btn_Refuse").GetComponent<Button>().onClick.AddListener(() =>
            {
                SystemPause = false;
                EventPanel.gameObject.SetActive(false);
                (currentEvent as JudgeEvent).Accept = false;
                currentEvent.EventFinish(); 
            });
            childPanel.Find("Btn_Accept").GetComponent<Button>().onClick.AddListener(() =>
            {
                SystemPause = false;
                EventPanel.gameObject.SetActive(false);
                (currentEvent as JudgeEvent).Accept = true;
                currentEvent.EventFinish(); 
            });
        }
        else
        {
            currentEvent.EventFinish();
        }
    }

    public EmpEntity CreateEmp(Vector3 position)
    {
        EmpEntity emp = GameObject.Instantiate(empPrefabs, position, Quaternion.identity).GetComponentInChildren<EmpEntity>();
        emp.Init();
        emp.Renderer.material = empMaterials[Random.Range(0, empMaterials.Length)];
        return emp;
    }


    //查找上级Boss
    public Employee FindBoss(Employee self)
    {
        if (!self.CurrentDep)
            return null;

        if (self.CurrentDep.CommandingOffice != null && self.CurrentDep.CommandingOffice.CurrentManager != null) 
        {
            return self.CurrentDep.CommandingOffice.CurrentManager;
        }
        return null;
    }
    //寻找员工的同事
    public List<Employee> FindColleague(Employee self)
    {
        if (!self.CurrentDep)
            return new List<Employee>();

        List<Employee> colleagues = new List<Employee>();
        for (int i = 0; i < self.CurrentDep.CurrentEmps.Count; i++)
        {
            if (self.CurrentDep.CurrentEmps[i] != self) 
            {
                colleagues.Add(self.CurrentDep.CurrentEmps[i]);
            }
        }
        return colleagues;
    }
    //寻找员工的下属
    public List<Employee> FindMembers(Employee self)
    {
        if (!self.CurrentOffice)
            return new List<Employee>();

        List<Employee> members = new List<Employee>();
        if (self.CurrentOffice != null) 
        {
            if (self.CurrentOffice.building.Type == BuildingType.CEO办公室 || self.CurrentOffice.building.Type == BuildingType.高管办公室) 
            {
                foreach (DepControl dep in self.CurrentOffice.ControledDeps) 
                {
                    foreach (Employee employee in dep.CurrentEmps)
                    {
                        members.Add(employee);
                    }
                }
            }
        }
        return members;
    }


    public EmpEntity RandomEventTarget(Employee self, out int index)
    {
        List<Event> eventList = RandomEventList(self, out index);
        List<Employee> posbTargets = new List<Employee>();     //可能的对象（随机其一）

        //从上下级同事之间选择
        if (index == 0 || index == 1)  
        {
            Employee boss = FindBoss(self);
            List<Employee> colleagues = FindColleague(self);
            List<Employee> members = FindMembers(self);
            posbTargets = Function.MergerList<Employee>(colleagues, members);
            if (boss != null)
                posbTargets.Add(boss);
        }
        if (index == 2)
        {
            posbTargets = self.RelationTargets;
        }

        //乱序排列可用目标，随机找到一个可用的
        if (posbTargets.Count > 0)
        {
            posbTargets = Function.RandomSortList<Employee>(posbTargets);
            foreach (Employee item in posbTargets)
            {
                if (item.InfoDetail.Entity.Available)
                {
                    return item.InfoDetail.Entity;
                }
            }
        }

        //无可用对象
        return null;
    }

    private List<Event> RandomEventList(Employee self, out int listIndex)
    {
        float initalValue = 0.3f;
        float companyValue = 0.3f;
        float relationValue = 0.4f;
        for (int i = 0; i < self.InfoDetail.PerksInfo.Count; i++)
        {
            if (self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 42)
            {
                initalValue += 0.1f;
                companyValue += 0.1f;
            }
            if (self.InfoDetail.PerksInfo[i].CurrentPerk.Num == 43)
            {
                companyValue -= 0.1f;
                relationValue += 0.1f;
            }
        }

        float value = Random.Range(0f, initalValue + companyValue + relationValue) ;
        if (value < initalValue)
        {
            listIndex = 0;
            return EventData.InitialList;
        }
        else if (value < initalValue + companyValue)
        {
            listIndex = 1;
            return EventData.CompanyList;
        }
        else 
        {
            listIndex = 2;
            return EventData.RelationList;
        }
    }
    public Event RandomEvent(Employee self, Employee target,int index)
    {
        Event newEvent = null;
        List<Event> AddEvents = new List<Event>();
        List<Event> PosbEvents = new List<Event>();

        List<Event> weight_5 = new List<Event>();
        List<Event> weight_4 = new List<Event>();
        List<Event> weight_3 = new List<Event>();
        List<Event> weight_2 = new List<Event>();
        List<Event> weight_1 = new List<Event>();

        if (index == 0) 
            PosbEvents = EventData.InitialList;
        else if (index == 1)
            PosbEvents = EventData.CompanyList;
        else if (index == 2)
            PosbEvents = EventData.RelationList;

        //没有目标
        if (target == null)
        {
            foreach (Event item in PosbEvents)
            {
                if (item.HaveTarget)
                {
                    continue;
                }
                int weight = item.Weight;
                item.Self = self;
                item.Target = null;
                if (item.ConditionCheck(-1) == true)
                {
                    item.SetWeight();
                    if (item.Weight >= 5)
                        weight_5.Add(item.Clone());
                    if (item.Weight == 4)
                        weight_4.Add(item.Clone());
                    if (item.Weight == 3)
                        weight_3.Add(item.Clone());
                    if (item.Weight == 2)
                        weight_2.Add(item.Clone());
                    if (item.Weight == 1)
                        weight_1.Add(item.Clone());
                }
                item.Weight = weight;
                item.Self = null;
                item.Target = null;
            }
        }
        //有目标
        else
        {
            foreach (Event item in PosbEvents)
            {
                if (!item.HaveTarget)
                {
                    continue;
                }
                int weight = item.Weight;
                item.Self = self;
                item.Target = target;
                if (item.ConditionCheck(-1) == true)
                {
                    item.SetWeight();
                    if (item.Weight >= 5)
                        weight_5.Add(item.Clone());
                    if (item.Weight == 4)
                        weight_4.Add(item.Clone());
                    if (item.Weight == 3)
                        weight_3.Add(item.Clone());
                    if (item.Weight == 2)
                        weight_2.Add(item.Clone());
                    if (item.Weight == 1)
                        weight_1.Add(item.Clone());
                }
                item.Weight = weight;
                item.Self = null;
                item.Target = null;
            }
        }

        //找到5个AddEvents，按42211的权重排序
        for (int i = 0; i < 5; i++)
        {
            if (weight_5.Count > 0) 
            {
                Event temp = weight_5[Random.Range(0, weight_5.Count)];
                AddEvents.Add(temp);
                weight_5.Remove(temp);
                continue;
            }
            if (weight_4.Count > 0) 
            {
                Event temp = weight_4[Random.Range(0, weight_4.Count)];
                AddEvents.Add(temp);
                weight_4.Remove(temp);
                continue;
            }
            if (weight_3.Count > 0) 
            {
                Event temp = weight_3[Random.Range(0, weight_3.Count)];
                AddEvents.Add(temp);
                weight_3.Remove(temp);
                continue;
            }
            if (weight_2.Count > 0) 
            {
                Event temp = weight_2[Random.Range(0, weight_2.Count)];
                AddEvents.Add(temp);
                weight_2.Remove(temp);
                continue;
            }
            if (weight_1.Count > 0) 
            {
                Event temp = weight_1[Random.Range(0, weight_1.Count)];
                AddEvents.Add(temp);
                weight_1.Remove(temp);
                continue;
            }
        }

        if (AddEvents.Count > 0)
        {
            float MaxPosb = 0, Posb = 0;
            if (AddEvents.Count == 1)
            {
                newEvent = AddEvents[0];
                return newEvent;
            }
            else if (AddEvents.Count == 2)
                MaxPosb = 0.6f;
            else if (AddEvents.Count == 3)
                MaxPosb = 0.8f;
            else if (AddEvents.Count == 4)
                MaxPosb = 0.9f;
            else if (AddEvents.Count == 5)
                MaxPosb = 1.0f;
            Posb = Random.Range(0.0f, MaxPosb);
            if (Posb < 0.4f)
                newEvent = AddEvents[0];
            else if (Posb < 0.6f)
                newEvent = AddEvents[1];
            else if (Posb < 0.8f)
                newEvent = AddEvents[2];
            else if (Posb < 0.9f)
                newEvent = AddEvents[3];
            else if (Posb < 1.0f)
                newEvent = AddEvents[4];
        }

        //子事件检测
        if (newEvent != null)
        {
            Event TempEvent = newEvent.SubEventCheck();
            if (TempEvent != null)
                newEvent = TempEvent.Clone();
        }
        if (newEvent == null)
            MonoBehaviour.print("无合适事件");
        return newEvent;
    }
}
