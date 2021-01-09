using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestControl : MonoBehaviour
{
    public static QuestControl Instance;

    public GameObject MessagePrefab;     //弹窗 预制体
    public GameObject QuestPanel;       //任务引导面板

    private int currentQuest = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartMission(1);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    Finish(1);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    Finish(2);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    Finish(3);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha4))
        //{
        //    Finish(4);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha5))
        //{
        //    Finish(5);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha6))
        //{
        //    Finish(6);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha7))
        //{
        //    Finish(7);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha8))
        //{
        //    Finish(8);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha9))
        //{
        //    Finish(9);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha0))
        //{
        //    Finish(10);
        //}
        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    Finish(11);
        //}
    }
        

    /// <summary>
    /// 确定，确定之后接触暂停继续
    /// </summary>
    /// <param name="txt">文案</param>
    public void Init(string txt)
    {
        MessagePanel message = Instantiate(MessagePrefab, transform).GetComponent<MessagePanel>();
        message.Init(txt);
    }

    /// <summary>
    /// 确定，确定之后执行方法
    /// </summary>
    /// <param name="txt">文案</param>
    /// <param name="onAccept">确定后执行</param>
    public void Init(string txt, Action onAccept)
    {
        MessagePanel message = Instantiate(MessagePrefab, transform).GetComponent<MessagePanel>();
        message.Init(txt, onAccept);
    }

    /// <summary>
    /// 接受或拒绝
    /// </summary>
    /// <param name="txt">文案</param>
    /// <param name="onAccept">接受后执行</param>
    /// <param name="onRefuse">拒绝后执行</param>
    public void Init(string txt, Action onAccept, Action onRefuse)
    {
        MessagePanel message = Instantiate(MessagePrefab, transform).GetComponent<MessagePanel>();
        message.Init(txt, onAccept, onRefuse);
    }

    /// <summary>
    /// 引导任务
    /// </summary>
    /// <param name="step">第几个任务</param>
    public void Finish(int step)
    {
        if (step == currentQuest)
        {
            ClearQuestPanel();
            StartMission(step + 1);
        }
    }

    /// <summary>
    /// 开始任务
    /// </summary>
    /// <param name="step"></param>
    private void StartMission(int step)
    {
        currentQuest = step;
        switch (step)
        {
            case 1:
                ShowQuestPanel("头脑风暴", "你们聚到一块儿，进行了公司的第一次头脑风暴，商量着最近一段时间的工作计划，其中的个别议题却难以达成共识，你究竟该怎么办呢？\n\n选择员工加入头脑风暴会议队列，确定后点击右侧方块可选择员工技能。你所能使用的骰子数量取决于管理能力最高者的管理能力。", "达成条件：完成头脑风暴");
                break;
            case 2:
                ShowQuestPanel("建造部门生产程序迭代", "是时候为两名员工安排工作了，不如让你的团队帮你实现你之前的想法吧。\n\n点击<color=yellow>建造</color>创建技术部门，并点击<color=yellow>员工</color>按钮，将两名员工<color=yellow>转入</color>该部门，技术部门将会开始生产程序迭代，每次生产1个程序迭代，都会消耗1个原型图。记得将技术部门的上级设为CEO办公室，同时不要安排的太远，否则可没法开始工作哦。创建完成点击左下<color=yellow>完成建造</color>。不要频繁的让员工转岗哦，否则<color=yellow>生疏磨合</color>太多，部门信念会严重下降。（转入员工并设置好上级后工作会自动开始）", "达成条件：创建技术部门，并开始生产程序迭代");
                break;
            case 3:
                ShowQuestPanel("培养一名员工获得新技能", "为了下次头脑风暴胜利，你需要更多有能力的人才。查看<color=yellow>员工信息-技能树</color>，找到拥有技术技能树的员工，你会看到获得该技能的要求是<color=yellow>技术等级>5</color>。查看员工信息面板的<color=yellow>技术>5</color>技能，你会看到员工的<color=yellow>Exp</color>经验在增长，从事什么工作就会获得什么经验。", "达成条件：培养一名员工获得技能“发表看法”");
                break;
            case 4:
                ShowQuestPanel("查看技能树，改变CEO办公室的重心为 管理", "开会并不是件容易的事，好好说话可是个稀有的技能。\n查看员工列表并点击技能树，随着工作经验积累，当技能等级超过所需条件员工就会解锁对应的能力，这样下一次头脑风暴才会更加顺利。\n同时，点击CEO办公室或高管办公室的上的<color=yellow>更改模式</color>，并改为<color=yellow>管理</color>，管理者会获得管理经验，而员工也会受到启发继而增加<color=yellow>热情</color>，这样升级就会快得多，热情为5比热情为1的经验多2倍。详情可查看技能列表上方的？查看详情。", "更改CEO办公室模式为管理");
                break; 
            case 5:
                ShowQuestPanel("使用CEO技能+部门成功率", "看到成功率了吗？点击技术部门<color=yellow>详细信息</color>按钮，查看当前<color=yellow>成功率</color>，如果团队成员能力太菜，工作成功率会非常低。比如在技术部门，若管理者的技术等级<6，该部门成功率下降15%，若其中员工技术等级<6，成功率也会下降15%，可想而知如果一个糊涂蛋带着四个外行，成功率接近0%。", "达成条件：\n发动CEO技能“亲自指导”");
                break;
            case 6:
                ShowQuestPanel("使用CEO技能调节情绪、改变文化信仰", "虽然成功率提高了，但是新团队看起来<color=yellow>信念</color>不大坚定啊。\n点击部门<color=yellow>详细信息</color>界面，<color=yellow>鼠标悬停查看信念</color>及<color=yellow>左侧状态栏</color>。看来是因为大家文化不同，信仰不一。特殊时刻，或许有必要使用CEO技能中的“<color=yellow>激怒</color>”和“<color=yellow>改变文化信仰</color>”来保障团队稳定。", "达成条件：发动CEO技能“激怒”、“改变文化信仰”");
                break;
            case 7:
                ShowQuestPanel("商战成功", "每三个月都是你打商战的时刻，和对手抢夺市场吧，每次商战排名越高、<color=yellow>胜利积分</color>越多的公司融资越多。你可以抢夺自由市场，也可以抢夺别人手里的市场，每回合抢夺越多，所消耗的程序迭代越多，传播则可以免除一次被抢夺的结果。按照每场商战获取市场的不同排名，你可能会提高或降低士气。点击界面上的<color=yellow>？</color>查看详情。", "达成条件：完成一次商战");
                break; 
            case 8:
                ShowQuestPanel("造人力部门招聘高管", "商战不那么容易对吗？主要还是人手太少了，不如建造一个人力资源部，为你招聘一位有经营天赋的人来做高管吧。你或许会看到技能那一栏写着“<color=yellow>热情0/5</color>”，意思是天赋是5，当前热情为0，热情越高经验获取越快，同时天赋越高能获得热情就越高。", "达成条件：招聘一名新员工。");
                break;
            case 9:
                ShowQuestPanel("建造高管部门调整上下级", "看看部门<color=yellow>详细信息</color>，最上方显示着管理者所能管理的上限（<color=yellow>1/2意味着能管2个部门，只管了1个</color>），也就是说你恐怕很难创建一个新部门了，或许你可以创建一个高管办公室，并将高管转入其中。至于如何才能让所有办公室都运转起来呢？请你想一想。", "使高管部门可以运作（不再无管理者即可）");
                break;   
            case 10:
                ShowQuestPanel("造产品部门生产原型图", "恭喜你的公司人数提升了33%！下面或许要开始学着继续迭代，毕竟原型图已经消耗了很多，下一步，不如我们来想办法<color=yellow>建造</color>一个产品部门，并招募到合适的员工吧。", "使产品部门开始运转");
                break;
            case 11:
                ShowQuestPanel("CEO组织研究，造建筑", "下面就是最后一步啦！作为一个公司的创始人，除了管理团队、制定战略，你还需要思考应该将公司变成什么样，或许是996地狱？或许是创造力的游乐场？总之，对CEO办公室<color=yellow>更改模式</color>，改成<color=yellow>组织研究</color>试试吧~", "研究一次新办公室");
                break;
            case 12:
                Init("对了，有的时候屏幕上出现“成功”或“失败”的气泡可以点击哦，这样你就会知道到底发生了什么，之后也可以<color=yellow>点击某个员工</color>，查看当事人的<color=yellow>事件历史</color>进一步探索。\n叫你什么好呢？大老板、资本家、创始人？总之我相信你还有很多要做的，加大传播？继续迭代？占领市场？这些都很重要，不过更重要的是照顾好你的团队啊，千万别让他们心力爆炸，否则可能会造成难以挽回的可怕悲剧。当然，你也就可能无法恢复记忆了....");
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 任务引导面板
    /// </summary>
    /// <param name="title"></param>
    /// <param name="info"></param>
    /// <param name="condition"></param>
    private void ShowQuestPanel(string title, string info, string condition)
    {
        QuestPanel.gameObject.SetActive(true);
        QuestPanel.transform.Find("Txt_Title").GetComponent<Text>().text = title;
        QuestPanel.transform.Find("Txt_Condition").GetComponent<Text>().text = condition;
        Button detailBtn = QuestPanel.transform.Find("Btn_Detail").GetComponent<Button>();
        detailBtn.onClick.RemoveAllListeners();
        detailBtn.onClick.AddListener(() =>
        {
            Init(title + "|" + info, ()=> { detailBtn.interactable = true; });
            detailBtn.interactable = false;
        });

        Init(title + "|" + info, () => { detailBtn.interactable = true; });
        detailBtn.interactable = false;
    }

    private void ClearQuestPanel()
    {
        QuestPanel.gameObject.SetActive(false);
    }
}
