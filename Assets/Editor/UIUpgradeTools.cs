using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

/// <summary>
/// UI 升级辅助工具（纯编辑器工具，不进打包）
/// 菜单：Battle / UI / ...
/// </summary>
public static class UIUpgradeTools
{
    private const string SourceFontPath = "Assets/Fonts/NotoSansSC-VF.ttf";
    private const string FontAssetPath = "Assets/Fonts/NotoSansSC SDF.asset";

    //20482 / 采样 44 / 边距 4：单格约 52px，能放 ~1500 个字形，够 UI 用
    private const int AtlasSize = 2048;
    private const int SamplingSize = 44;
    private const int AtlasPadding = 4;

    // =========================
    // 1. 生成中文 TMP 字体资源
    //
    // 两个坑（都踩过了）：
    //  ① CreateFontAsset 建出来的图集是 new Texture2D(0,0,...)，没有原生显存。
    //     必须「先烘字（TMP 会自己 Resize 成真实尺寸）、再存资源」，
    //     否则存出来的图集引用在运行时是死的 → MissingReferenceException。
    //  ② TryAddCharacters 的返回值不能当成败判据：请求的字符里只要有一个
    //     字体本身没有（生僻字/扫描出来的杂字），它就返回 false，
    //     但能加的其实都加了。所以要改成「校验真正要显示的文案」。
    // =========================
    [MenuItem("Battle/UI/1. 生成中文TMP字体资源（Noto Sans SC）")]
    public static void CreateChineseFontAsset()
    {
        Font source = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);

        if (source == null)
        {
            Debug.LogError($"找不到字体文件：{SourceFontPath}");
            return;
        }

        TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;

        ClearOldFallback(defaultFont);

        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath) != null)
        {
            AssetDatabase.DeleteAsset(FontAssetPath);
            AssetDatabase.Refresh();
        }

        Debug.Log("开始生成中文字体资源（要烘几百个字形，稍等几秒）...");

        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            source, SamplingSize, AtlasPadding, GlyphRenderMode.SDFAA,
            AtlasSize, AtlasSize, AtlasPopulationMode.Dynamic, true);

        if (fontAsset == null)
        {
            Debug.LogError("生成字体资源失败：FontEngine 读不了这个字体文件");
            return;
        }

        fontAsset.name = "NotoSansSC SDF";

        //先烘字：这一步里 TMP 会把 0x0 的图集 Resize 成真实尺寸（才有显存）
        string characters = CollectCharacters();
        string missing;

        fontAsset.TryAddCharacters(characters, out missing);

        Texture2D atlas = fontAsset.atlasTextures[0];

        if (atlas.width == 0 || atlas.height == 0)
        {
            atlas.Reinitialize(AtlasSize, AtlasSize);
            atlas.Apply(false, false);

            Debug.LogWarning("图集仍是 0x0，已手动初始化");
        }

        atlas.name = fontAsset.name + " Atlas";

        Debug.Log(
            $"烘焙完成：请求 {characters.Length} 个字符，已烘入 {fontAsset.characterTable.Count} 个\n" +
            $"· 图集 {atlas.width}x{atlas.height}（可读={atlas.isReadable}）\n" +
            $"· 字体里没有的字：{(string.IsNullOrEmpty(missing) ? "无" : missing.Length + " 个（不影响 UI，下面的校验才作数）")}"
        );

        //★真正的判据：UI 上会显示的文案，字够不够
        List<string> criticalTexts = CollectCriticalTexts();
        List<string> problems = new List<string>();

        bool allTextOk = VerifyTexts(fontAsset, criticalTexts, problems);

        if (allTextOk)
        {
            //切成 Static：运行时不再动态加字，也就不会触发资源重导入和
            //「图集引用失效」那个异常
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;

            Debug.Log($"文案校验：{criticalTexts.Count} 条全部有字形 ? → 已切成 Static（运行时不再动态加字）");
        }
        else
        {
            Debug.LogWarning(
                $"文案校验没全过（{problems.Count} 条缺字），保持 Dynamic：\n  " +
                string.Join("\n  ", problems.ToArray())
            );
        }

        //存资源 + 子资源（顺序：烘完再存）
        AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
        AssetDatabase.AddObjectToAsset(atlas, fontAsset);

        fontAsset.material.name = fontAsset.name + " Material";
        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        //重新读一次：挂 Fallback 必须用持久化后的实例
        TMP_FontAsset persisted = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);

        if (persisted == null)
        {
            Debug.LogError($"字体资源保存后读不回来：{FontAssetPath}");
            return;
        }

        if (defaultFont != null && !defaultFont.fallbackFontAssetTable.Contains(persisted))
        {
            defaultFont.fallbackFontAssetTable.Add(persisted);
            EditorUtility.SetDirty(defaultFont);
            AssetDatabase.SaveAssets();
        }

        Debug.Log(
            $"中文 TMP 字体已完成：{FontAssetPath}\n" +
            $"· 图集 {persisted.atlasTextures[0].width}x{persisted.atlasTextures[0].height} / " +
            $"模式 {persisted.atlasPopulationMode} / 字符 {persisted.characterTable.Count} 个\n" +
            $"· 已挂到「{defaultFont?.name}」的 Fallback\n" +
            "· 以后加了新文案再跑一次这个菜单（会自动重新扫字符并重烘）"
        );
    }

    // =========================
    // 校验：这些文案必须每个字都有字形
    // 卡名 / 角色名直接从数据资源取，改数据不用改这里；
    // UI 文案是代码里写死的，改了 UI 记得往这里补一行
    // =========================
    private static List<string> CollectCriticalTexts()
    {
        List<string> texts = new List<string>();

        //--- 手牌 / 信息栏 ---
        texts.Add("角色");
        texts.Add("技能卡");
        texts.Add("伤害");
        texts.Add("计时");
        texts.Add("移动");
        texts.Add("移动卡");
        texts.Add("已使用");
        texts.Add("射程");
        texts.Add("效果范围");
        texts.Add("特殊效果");
        texts.Add("预览");
        texts.Add("移动范围");
        texts.Add("技能射程");
        texts.Add("手牌");
        texts.Add("能力低下");
        texts.Add("已移动");
        texts.Add("未移动");
        texts.Add("已攻击");
        texts.Add("未攻击");
        texts.Add("未选中");
        texts.Add("点一张技能卡开始");
        texts.Add("隐藏");
        texts.Add("详细");
        texts.Add("紧凑");
        texts.Add("切预览");
        texts.Add("请点一个己方角色");

        //--- 选秀 / 部署 ---
        texts.Add("次选择角色");
        texts.Add("玩家方");
        texts.Add("敌方");
        texts.Add("角色摆放");
        texts.Add("当前要摆");
        texts.Add("已摆放");
        texts.Add("点棋盘上高亮的格子");
        texts.Add("放下该角色");

        //--- 特效 / 技能说明 ---
        texts.Add("单点");
        texts.Add("十字");
        texts.Add("横向");
        texts.Add("纵向");
        texts.Add("方形");
        texts.Add("米字形");
        texts.Add("直线");
        texts.Add("无限延伸");
        texts.Add("击飞");
        texts.Add("取消计时");
        texts.Add("回复");
        texts.Add("直线攻击");

        //--- 卡名 / 角色名（从资源里取）---
        foreach (string guid in AssetDatabase.FindAssets(
            "t:ScriptableObject", new[] { "Assets/Cards", "Assets/Characters" }))
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(
                AssetDatabase.GUIDToAssetPath(guid));

            if (asset == null)
            {
                continue;
            }

            ActionCard card = asset as ActionCard;

            if (card != null && !string.IsNullOrEmpty(card.GetCardName()))
            {
                texts.Add(card.GetCardName());
            }

            CharacterData data = asset as CharacterData;

            if (data != null && !string.IsNullOrEmpty(data.GetCharacterName()))
            {
                texts.Add(data.GetCharacterName());
            }
        }

        return texts;
    }

    private static bool VerifyTexts(
        TMP_FontAsset fontAsset,
        List<string> texts,
        List<string> problems)
    {
        bool allOk = true;

        foreach (string text in texts)
        {
            if (string.IsNullOrEmpty(text))
            {
                continue;
            }

            uint[] missingCodes;

            //searchFallbacks=false / tryAddCharacter=false：只查这个资源自己有没有
            if (fontAsset.HasCharacters(text, out missingCodes, false, false))
            {
                continue;
            }

            if (missingCodes == null || missingCodes.Length == 0)
            {
                continue;
            }

            StringBuilder builder = new StringBuilder();

            foreach (uint code in missingCodes)
            {
                builder.Append((char)code);
            }

            allOk = false;
            problems.Add($"「{text}」缺少：{builder}");
        }

        return allOk;
    }

    // =========================
    // 收集要预烘的字符（宁多勿少：多出来的杂字没有也不影响校验）
    // =========================
    private static string CollectCharacters()
    {
        HashSet<char> set = new HashSet<char>();

        for (char c = (char)0x20; c <= (char)0x7E; c++)
        {
            set.Add(c);
        }

        //CJK 标点 + 平假名 + 片假名 + 全角
        for (int c = 0x3000; c <= 0x30FF; c++)
        {
            set.Add((char)c);
        }

        for (int c = 0xFF01; c <= 0xFF5E; c++)
        {
            set.Add((char)c);
        }

        //脚本里出现过的中日文（UI 文案都在代码里）
        if (Directory.Exists("Assets/Scripts"))
        {
            foreach (string path in Directory.GetFiles(
                "Assets/Scripts", "*.cs", SearchOption.AllDirectories))
            {
                foreach (char c in ReadFileAutoEncoding(path))
                {
                    if (IsCjk(c))
                    {
                        set.Add(c);
                    }
                }
            }
        }

        List<char> sorted = new List<char>(set);
        sorted.Sort();

        StringBuilder builder = new StringBuilder(sorted.Count);

        foreach (char c in sorted)
        {
            builder.Append(c);
        }

        return builder.ToString();
    }

    private static bool IsCjk(char c)
    {
        return (c >= 0x3000 && c <= 0x303F) ||
               (c >= 0x3040 && c <= 0x30FF) ||
               (c >= 0x4E00 && c <= 0x9FFF) ||
               (c >= 0xFF00 && c <= 0xFFEF);
    }

    //项目里的 .cs 是 GBK：先按严格 UTF-8 解，解不开就按系统 ANSI
    private static string ReadFileAutoEncoding(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);

        try
        {
            return new UTF8Encoding(false, true).GetString(bytes);
        }
        catch (Exception)
        {
            return Encoding.Default.GetString(bytes);
        }
    }

    private static void ClearOldFallback(TMP_FontAsset defaultFont)
    {
        if (defaultFont == null || defaultFont.fallbackFontAssetTable == null)
        {
            return;
        }

        for (int i = defaultFont.fallbackFontAssetTable.Count - 1; i >= 0; i--)
        {
            TMP_FontAsset entry = defaultFont.fallbackFontAssetTable[i];

            if (entry == null || AssetDatabase.GetAssetPath(entry) == FontAssetPath)
            {
                defaultFont.fallbackFontAssetTable.RemoveAt(i);
            }
        }

        EditorUtility.SetDirty(defaultFont);
        AssetDatabase.SaveAssets();
    }

    // =========================
    // 2. Canvas 缩放改为 1920x1080 自适应
    // =========================
    [MenuItem("Battle/UI/2. Canvas 缩放改为1920x1080（Scale With Screen Size）")]
    public static void SetupCanvasScaler()
    {
        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("场景里没找到 Canvas！");
            return;
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();

        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        EditorUtility.SetDirty(scaler);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            canvas.gameObject.scene
        );

        Debug.Log(
            "Canvas 已改为 Scale With Screen Size（1920×1080，Match 0.5）——记得 Ctrl+S"
        );
    }
}