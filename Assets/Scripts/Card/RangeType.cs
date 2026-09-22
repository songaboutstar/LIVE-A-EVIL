public enum RangeType
{
    Single,//单点
    Cross,//十字
    Horizontal,//横向
    Vertical,//纵向
    Square,//方形
    CrossWithDiagonal,//十字+斜向
    HorizontalInfinite,//横向无限
    VerticalInfinite,//纵向无限
    DirectLine,//直线攻击
    DiagonalFourAtTwo,//斜向单点四个

    Custom,//自定义

    CrossInfinite,//十字无限延伸（叉形：上下左右各到棋盘边缘）
    CrossWithDiagonalInfinite//米字形无限延伸（十字 + 四条斜线各到棋盘边缘）
}