public class CharacterActionData
{
   //本回合是否已经移动
   public bool HasMoved
    {
        get;
        private set;
    }

    //本回合是否已经攻击
    public bool HasAttacked
    {
        get;
        private set;
    }

    //标记已经移动
    public void SetMoved()
    {
        HasMoved = true;
    }

    //标记已经攻击
    public void SetAttacked()
    {
        HasAttacked = true;
    }
    //可以开始新回合，重置行动状态
    public void Reset()
    {
        HasMoved = false;
        HasAttacked = false;
    }
}
//暂时不继承MonoBehaviour，只是保存数据；