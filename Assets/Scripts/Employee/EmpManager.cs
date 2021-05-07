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
    private EmpEntity pointEmp;
    private GameObject eventBubble;

    private Material[] empMaterials;
    public Sprite[] EmpFaces = new Sprite[10];

    private void Awake()
    {
        Instance = this;
        empPrefabs = ResourcesLoader.LoadPrefab("Prefabs/Employee/Emp");
        empMaterials = ResourcesLoader.LoadAll<Material>("Image/Employee/Materials");
        eventBubble = ResourcesLoader.LoadPrefab("Prefabs/Employee/Bubble");
    }

    private void Update()
    {
        pointEmp = null;

        if (CameraController.CharacterHit && !UISvc.IsPointingUI)
        {
            pointEmp = CameraController.CharacterRaycast.collider.transform.parent.parent.GetComponentInChildren<EmpEntity>();
            DynamicWindow.Instance.ShowName(pointEmp.EmpName, pointEmp.transform, Vector3.up * 10);
        }
        if (pointEmp && !pointEmp.Fired && Input.GetMouseButtonDown(0))
            pointEmp.ShowDetailPanel();
    }

    public EmpEntity CreateEmp(Vector3 position)
    {
        EmpEntity emp = GameObject.Instantiate(empPrefabs, position, Quaternion.identity).GetComponentInChildren<EmpEntity>();
        emp.Init();
        emp.FaceType = Random.Range(0, empMaterials.Length);
        emp.Renderer.material = empMaterials[emp.FaceType];
        return emp;
    }

    //查找上级Boss
    public Employee FindBoss(Employee self)
    {
        if (!self.CurrentDep)
            return null;

        if (self.CurrentDep.CurrentDivision.Manager != null) 
        {
            Employee boss = self.CurrentDep.CurrentDivision.Manager;
            if (boss != self) 
            {
                return boss;
            }
        }
        return null;
    }
    //寻找员工的同事
    public List<Employee> FindColleague(Employee self)
    {
        List<Employee> colleagues = new List<Employee>();
        if (!self.CurrentDep)
        {
            foreach(Employee e in GameControl.Instance.CurrentEmployees)
            {
                if (e != self && e.CurrentDep == null && e.CurrentDivision == null)
                    colleagues.Add(e);
            }
            return colleagues;
        }
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
        if (self.CurrentDivision == null)
            return new List<Employee>();

        List<Employee> members = new List<Employee>();
        foreach (DepControl dep in self.CurrentDivision.CurrentDeps)
        {
            foreach (Employee employee in dep.CurrentEmps)
            {
                members.Add(employee);
            }
        }
        return members;
    }
    //寻找上级下属和同事
    public List<Employee> FindAll(Employee emp)
    {
        List<Employee> TargetEmps = new List<Employee>();
        if (emp.CurrentDep != null)
        {
            foreach (Employee e in emp.CurrentDep.CurrentEmps)
            {
                if (e != emp)
                    TargetEmps.Add(e);
            }
            if (emp.CurrentDep.CurrentDivision.Manager != null)
                TargetEmps.Add(emp.CurrentDep.CurrentDivision.Manager);
        }
        else if (emp.CurrentDivision != null)
        {
            foreach (DepControl dep in emp.CurrentDivision.CurrentDeps)
            {
                foreach (Employee e in dep.CurrentEmps)
                {
                    TargetEmps.Add(e);
                }
            }
        }
        else
        {
            foreach (Employee e in GameControl.Instance.CurrentEmployees)
            {
                if (e.CurrentDep != null && e.CurrentDivision != null && e != emp)
                    TargetEmps.Add(e);
            }
        }
        return TargetEmps;
    }

    private void CheckRelation(Employee self)
    {
        Employee boss = FindBoss(self);
        if (boss != null) 
        {
            self.FindRelation(boss).KnowTarget();
            boss.FindRelation(self).KnowTarget();
        }

        List<Employee> colleagues = FindColleague(self);
        foreach (Employee item in colleagues)
        {
            self.FindRelation(item).KnowTarget();
            item.FindRelation(self).KnowTarget();
        }
    }

    //根据条件和权重选择一个事件序列
    public void AddEvent(Employee emp)
    {
        int TotalWeight = 10;
        bool HaveBlack = false, HaveOrange = false, HaveColleague = false;
        //判断有没有对应颜色的事件状态
        foreach (EventCondition c in emp.EventConditions)
        {
            if ((int)c <= 4)
                HaveBlack = true;
            else
                HaveOrange = true;
        }

        //是否有上司、下属或同事(待命时所有待命员工互相为同事)
        if (emp.CurrentDep != null)
        {
            if (emp.CurrentDep.CurrentEmps.Count > 1 || emp.CurrentDep.CurrentDivision.Manager != null)
                HaveColleague = true;
        }
        else if (emp.CurrentDivision != null)
        {
            foreach(DepControl dep in emp.CurrentDivision.CurrentDeps)
            {
                if (dep.CurrentEmps.Count > 0)
                {
                    HaveColleague = true;
                    break;
                }
            }
        }
        else
        {
            int count = 0;
            foreach(Employee e in GameControl.Instance.CurrentEmployees)
            {
                if (e.CurrentDep == null && e.CurrentDivision == null)
                {
                    count += 1;
                    if (count > 1)
                    {
                        HaveColleague = true;
                        break;
                    }
                }
            }
        }

        int ColleagueWeight = 0, BlackWeight1 = 0, BlackWeight2 = 0, OrangeWeight = 0;
        if (HaveColleague == true)
        {
            ColleagueWeight = 10;
            TotalWeight += 10;
        }
        if (HaveBlack == true)
        {
            BlackWeight1 = ColleagueWeight + 10;
            BlackWeight2 = ColleagueWeight + 40;
            TotalWeight += 40;
        }
        if (HaveOrange == true)
        {
            OrangeWeight += (ColleagueWeight + BlackWeight1 + BlackWeight2);
            TotalWeight += 50;
        }
        int Posb = Random.Range(1, TotalWeight + 1);
        if (Posb <= ColleagueWeight)
            StartEvent(emp, 1);
        else if (Posb <= BlackWeight1)
            StartEvent(emp, 2);
        else if (Posb <= BlackWeight2)
            StartEvent(emp, 3);
        else if (Posb <= OrangeWeight)
            StartEvent(emp, 4);
    }

    //根据序列选择一个事件
    private void StartEvent(Employee emp, int Type)
    {
        //1公司日常行为树 2公司一般事件行为树 3个人港口行为树 4个人行为树
        if (Type == 1)
        {
            //先判断是进同事事件还是状态事件

            //同事事件时，先确认所有可行目标
            List<Employee> TargetEmps = FindAll(emp);
            if (TargetEmps.Count == 0)
            {
                Debug.LogError("公司日常事件没有找到目标");
                return;
            }
            Employee target = TargetEmps[Random.Range(0, TargetEmps.Count)];

            //确认所有可用事件
            List<Event> PosbEvents = new List<Event>();
            foreach(Event e in EventData.CompanyRoutineEventA)
            {
                if (e.ConditionCheck(emp, target) == true)
                    PosbEvents.Add(e);
            }

            //直接随机一个事件结算效果
            PosbEvents[Random.Range(0, PosbEvents.Count)].StartEvent(emp, 0, target);
        }
        else if (Type == 2)
        {
            List<Event> PosbEvents = new List<Event>();
            foreach(Event e in EventData.CompanyNormalEvent)
            {
                if (e.ConditionCheck(emp) == true)
                    PosbEvents.Add(e);
            }
            PosbEvents[Random.Range(0, PosbEvents.Count)].StartEvent(emp);
        }
        else if (Type == 3)
        {
            List<Event> PosbEvents = new List<Event>();
            foreach (Event e in EventData.EmpPortEvent)
            {
                if (e.ConditionCheck(emp) == true)
                    PosbEvents.Add(e);
            }
            PosbEvents[Random.Range(0, PosbEvents.Count)].StartEvent(emp);
        }
        else if (Type == 4)
        {
            List<Employee> TargetEmps = FindAll(emp);
            foreach(Employee e in emp.RelationTargets)
            {
                if (TargetEmps.Contains(e) == false)
                    TargetEmps.Add(e);
            }

            //没有任何目标时直接执行认识新人事件
            if (TargetEmps.Count == 0)
            {
                if (GameControl.Instance.CurrentEmployees.Count > 1)
                {
                    Employee rTarget = GameControl.Instance.CurrentEmployees[Random.Range(0, GameControl.Instance.CurrentEmployees.Count)];
                    while(rTarget == emp)
                    {
                        rTarget = GameControl.Instance.CurrentEmployees[Random.Range(0, GameControl.Instance.CurrentEmployees.Count)];
                    }
                    new Event14().StartEvent(emp, 0, rTarget);
                }
                return;
            }

            Employee target = TargetEmps[Random.Range(0, TargetEmps.Count)];
            List<Event> PosbEvents = new List<Event>();
            foreach (Event e in EventData.EmpPersonalEvent)
            {
                if (e.ConditionCheck(emp, target) == true)
                    PosbEvents.Add(e);
            }
            PosbEvents[Random.Range(0, PosbEvents.Count)].StartEvent(emp, 0, target);
        }
    }
}
