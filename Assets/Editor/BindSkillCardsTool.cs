using UnityEngine;
using UnityEditor;

/// <summary>
/// 一键把 Assets/Cards/&lt;角色名&gt;/SkillCard1~3.asset
/// 绑定到 Assets/Characters/&lt;角色名&gt;.asset 的 skillCards 三个槽。
///
/// 用法：Unity 菜单栏 → Battle → 自动绑定技能卡到角色数据
/// 只读校验：Battle → 检查技能卡绑定情况
/// </summary>
public static class BindSkillCardsTool
{
    private static readonly string[] CharacterNames =
    {
        "Akira", "Cube", "Masaru", "Oboromaru",
        "Oersted", "Pogo", "Shifu", "SundownKid"
    };

    private const string CharacterFolder = "Assets/Characters";
    private const string CardFolder = "Assets/Cards";

    [MenuItem("Battle/自动绑定技能卡到角色数据")]
    public static void BindAll()
    {
        int okChars = 0;
        int problemCount = 0;

        foreach (string name in CharacterNames)
        {
            string charPath = $"{CharacterFolder}/{name}.asset";
            CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>(charPath);

            if (data == null)
            {
                Debug.LogError($"[绑定] 找不到角色数据：{charPath}");
                problemCount++;
                continue;
            }

            SkillCard[] cards = new SkillCard[3];

            for (int i = 0; i < 3; i++)
            {
                string cardPath = $"{CardFolder}/{name}/SkillCard{i + 1}.asset";
                cards[i] = AssetDatabase.LoadAssetAtPath<SkillCard>(cardPath);

                if (cards[i] == null)
                {
                    Debug.LogWarning($"[绑定] 找不到技能卡：{cardPath}");
                    problemCount++;
                }
            }

            SerializedObject so = new SerializedObject(data);
            SerializedProperty arrayProp = so.FindProperty("skillCards");

            if (arrayProp == null)
            {
                Debug.LogError($"[绑定] {name} 上找不到 skillCards 字段（CharacterData.cs 被改过？）");
                problemCount++;
                continue;
            }

            arrayProp.arraySize = 3;

            for (int i = 0; i < 3; i++)
            {
                arrayProp.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(data);

            okChars++;

            Debug.Log(
                $"[绑定] {name} ← " +
                $"{CardName(cards[0])} / {CardName(cards[1])} / {CardName(cards[2])}"
            );
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[绑定] 完成：成功绑定 {okChars} 个角色，异常 {problemCount} 处");
    }

    [MenuItem("Battle/检查技能卡绑定情况")]
    public static void VerifyAll()
    {
        int okChars = 0;

        foreach (string name in CharacterNames)
        {
            string charPath = $"{CharacterFolder}/{name}.asset";
            CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>(charPath);

            if (data == null)
            {
                Debug.LogWarning($"[检查] 找不到 {charPath}");
                continue;
            }

            SkillCard[] cards = data.GetSkillCards();
            int bound = 0;

            if (cards != null)
            {
                foreach (SkillCard c in cards)
                {
                    if (c != null)
                    {
                        bound++;
                    }
                }
            }

            if (bound == 3)
            {
                okChars++;
            }

            Debug.Log($"[检查] {name}：已绑定 {bound} / 3 张");
        }

        Debug.Log($"[检查] 结果：{okChars} / {CharacterNames.Length} 个角色绑定完整");
    }

    private static string CardName(SkillCard card)
    {
        if (card == null)
        {
            return "(未找到)";
        }

        return card.GetCardName();
    }
}
