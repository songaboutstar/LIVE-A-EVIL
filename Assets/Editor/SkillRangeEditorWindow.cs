using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SkillRangeEditorWindow : EditorWindow
{

    //编辑器设置

    private const int GridSize = 11;
    private const int Center = GridSize / 2;

    private const float CellSize = 42f;

    //当前编辑数据

    private HashSet<Vector2Int> selectedOffsets = new HashSet<Vector2Int>();
    //当前技能卡
    private SkillCard selectedCard;
    //json文件
    private string jsonFileName = "SkillRange.json";

    //滚动区域
    private Vector2 scrollPosition;


    //打开窗口
    [MenuItem("Battle/Skill Range Editor")]
    public static void ShowWindow()
    {
        SkillRangeEditorWindow window = GetWindow<SkillRangeEditorWindow>("Skill Range Editor");
        window.minSize = new Vector2(600, 650);
    }

    //GUI
    private void OnGUI()
    {
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("技能范围编辑器",EditorStyles.boldLabel);

        EditorGUILayout.Space(5);
        //SkillCard

        selectedCard = (SkillCard)EditorGUILayout.ObjectField("技能卡",selectedCard,typeof(SkillCard),false);
        EditorGUILayout.Space(10);

        //当前状态
        EditorGUILayout.LabelField($"已选择范围：{selectedOffsets.Count}格");
        EditorGUILayout.Space(5);

        //操作按钮
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("清空"))
        {
            ClearRange();
        }
        if (GUILayout.Button("读取技能卡"))
        {
            LoadFromSkillCard();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);

        //绘制棋盘
        DrawRangeGrid();

        EditorGUILayout.Space(10);

        //JSON
        EditorGUILayout.LabelField("JSON文件名");

        jsonFileName = EditorGUILayout.TextField(jsonFileName);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("保存JSON"))
        {
            SaveJson();
        }
        if (GUILayout.Button("加载JSON"))
        {
            LoadJson();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);

        //应用到SkillCard

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("应用当前技能卡", GUILayout.Height(35)))
        {
            ApplyToSkillCard();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.Space(10);

        EditorGUILayout.HelpBox("红色=角色位置（0，0）\n"+
                        "蓝色=技能目标范围\n"+
                        "点击格子可以添加/取消范围\n"+
                        "保存的数据全部使用相对于角色的坐标。",MessageType.Info);

    }


    //绘制棋盘
    private void DrawRangeGrid()
    {
        EditorGUILayout.LabelField("点击格子设置技能范围",EditorStyles.boldLabel);

        float width = GridSize * CellSize;
        float height = GridSize * CellSize;

        Rect gridRect = GUILayoutUtility.GetRect(width,height);

        //背景
        EditorGUI.DrawRect(gridRect, new Color(0.12f, 0.12f, 0.12f));

        for(int row = 0; row < GridSize; row++)
        {
            for(int col = 0; col < GridSize; col++)
            {
                int x = col - Center;
                //unity gui y向下
                //我们这里转换成棋盘Y向上
                int y = Center - row;

                Vector2Int offset = new Vector2Int(x, y);

                Rect cellRect = new Rect(gridRect.x+col*CellSize,gridRect.y+row*CellSize,CellSize,CellSize);

                DrawCell(cellRect,offset);
            }
        }
    }

    //绘制单格
    private void DrawCell(Rect rect,Vector2Int offset)
    {
        //角色中心
        if (offset == Vector2Int.zero)
        {
            EditorGUI.DrawRect(rect, new Color(0.8f, 0.2f, 0.2f));
        }
        //已选择
        else if(selectedOffsets.Contains(offset))
        {
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.45f, 0.95f));
        }
        else
        {
            EditorGUI.DrawRect(rect,new Color(0.22f,0.22f,0.22f));
        }

        //网格线
        Handles.color = new Color(0.45f, 0.45f, 0.45f);

        Handles.DrawLine(new Vector3(rect.x,rect.y),new Vector3(rect.xMax,rect.y));

        Handles.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.x, rect.yMax));

        Handles.DrawLine(new Vector3(rect.xMax, rect.y),new Vector3(rect.xMax,rect.yMax));

        Handles.DrawLine(new Vector3(rect.x,rect.yMax),new Vector3(rect.xMax,rect.yMax));

        //中心显示
        if (offset == Vector2Int.zero)
        {
            GUI.Label(
                rect,
                "角色",
                new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal =
                         {
                                textColor=Color.white
                         }
                }

                );
        }
        else if (selectedOffsets.Contains(offset))
        {
            GUI.Label(
                    rect,
                    $"{offset.x},{offset.y}",
                    new GUIStyle(EditorStyles.boldLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal =
                             {
                                textColor=Color.white
                             }
                           }

                    );
        }

        //鼠标点击
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
        {
            //中心不能选择
            if (offset != Vector2Int.zero)
            {
                ToggleOffset(offset);

                e.Use();

                Repaint();
            }
        }
    }

    //添加/删除范围
    private void ToggleOffset(Vector2Int offset)
    {
        if (selectedOffsets.Contains(offset))
        {
            selectedOffsets.Remove(offset);
        }
        else
        {
            selectedOffsets.Add(offset);
        }
    }


    //清空
    private  void ClearRange()
    {
        if (!EditorUtility.DisplayDialog("清空范围", "确定要清空当前技能范围吗？", "确定", "取消"))
        {
            return;
        }
        selectedOffsets.Clear();
    }

    //应用到SkillCard
    private void ApplyToSkillCard()
    {
        if (selectedCard == null)
        {
            EditorUtility.DisplayDialog("提示","请先选择SkillCard","确定");
            return;
        }

        List<Vector2Int> range = new List<Vector2Int>(selectedOffsets);

        selectedCard.SetCustomRange(range);

        EditorUtility.SetDirty(selectedCard);

        AssetDatabase.SaveAssets();

        Debug.Log($"技能范围已应用到：{selectedCard.GetCardName()},共{range.Count}个格子");

        EditorUtility.DisplayDialog("完成",$"已将{range.Count}个范围格保存到技能卡。","确定");
    }

    //从SkillCard读取
    private void LoadFromSkillCard()
    {
        if (selectedCard == null)
        {
            EditorUtility.DisplayDialog("提示","请先选择SkillCard.","确定");
            return;
        }

        selectedOffsets.Clear();

        List<Vector2Int> range = selectedCard.GetCustomRange();

        if (range == null)
            return;

        foreach(Vector2Int offset in range)
        {
            if (offset != Vector2Int.zero)
            {
                selectedOffsets.Add(offset);
            }
        }
        Debug.Log($"已从技能卡读取{selectedOffsets.Count}个范围格");

        Repaint();

    }


    //JSON数据结构
    [System.Serializable]
    private class SkillRangeData
    {
        public List<OffsetData> offsets = new List<OffsetData>();
    }

    [System.Serializable]
    private class OffsetData
    {
        public int x;
        public int y;

        public OffsetData()
        {

        }
        public OffsetData(Vector2Int offset)
        {
            x = offset.x;
            y = offset.y;
        }

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(x, y);
        }
    }


    //保存JSON
    private void SaveJson()
    {
        SkillRangeData data = new SkillRangeData();
        foreach(Vector2Int offset in selectedOffsets)
        {
            data.offsets.Add(new OffsetData(offset));
        }

        string json = JsonUtility.ToJson(data,true);

        string folder = "Assets/SkillRangeData";

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        string path = Path.Combine(folder,jsonFileName);

        File.WriteAllText(path, json);

        AssetDatabase.Refresh();

        Debug.Log($"技能范围JSON已保存：{path}");

        EditorUtility.DisplayDialog("保存成功",$"已保存到：\n{path}","确定");
    }
    //加载JSON
    private void LoadJson()
    {
        string path = Path.Combine("Assets/SkillRangeData",jsonFileName);

        if (!File.Exists(path))
        {
            EditorUtility.DisplayDialog("加载失败",$"找不到文件：\n{path}","确定");
            return;
        }

        string json = File.ReadAllText(path);
        SkillRangeData data = JsonUtility.FromJson<SkillRangeData>(json);

        selectedOffsets.Clear();

        if (data != null && data.offsets != null)
        {
            foreach(OffsetData offset in data.offsets)
            {
                Vector2Int value = offset.ToVector2Int();
                if (value != Vector2Int.zero)
                {
                    selectedOffsets.Add(value);
                }
            }
        }
        Debug.Log($"已加载技能范围：{selectedOffsets.Count}格");
        Repaint();
    }
}
