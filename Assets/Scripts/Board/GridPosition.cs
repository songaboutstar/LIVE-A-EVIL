using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


/// <summary>
/// 战棋棋盘上的二维坐标。
/// x：横向坐标，范围 0~6
/// y：纵向坐标，范围 0~6
/// </summary>
[Serializable]
public struct GridPosition : IEquatable<GridPosition>
{
    public int x;
    public int y;

    public GridPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    /// <summary>
    /// 判断两个棋盘坐标是否相同
    /// </summary>
    public bool Equals(GridPosition other)
    {
        return x == other.x && y == other.y;
    }

    public override bool Equals(object obj)
    {
        return obj is GridPosition other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, y);
    }

    public static bool operator ==(GridPosition a, GridPosition b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(GridPosition a, GridPosition b)
    {
        return !a.Equals(b);
    }

    public override string ToString()
    {
        return $"({x}, {y})";
    }
}