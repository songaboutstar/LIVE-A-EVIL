using UnityEngine;

public class TileSelector : MonoBehaviour
{
    private Tile selectedTile;


    /// <summary>
    /// 选择一个 Tile
    /// </summary>
    public void SelectTile(Tile tile)
    {
        if (tile == null)
        {
            return;
        }


        // =========================
        // 清除之前选中的 Tile
        // =========================

        if (selectedTile != null)
        {
            selectedTile.SetVisualState(
                TileVisualState.Normal
            );
        }


        // =========================
        // 记录新的 Tile
        // =========================

        selectedTile = tile;


        // =========================
        // 设置新的 Tile 为选中状态
        // =========================

        selectedTile.SetVisualState(
            TileVisualState.Selected
        );


        Debug.Log(
            $"选中格子：{selectedTile.GetGridPosition()}"
        );
    }


    /// <summary>
    /// 获取当前选中的 Tile
    /// </summary>
    public Tile GetSelectedTile()
    {
        return selectedTile;
    }
}