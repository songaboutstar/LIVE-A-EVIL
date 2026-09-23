using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

/// <summary>
/// UI 升级辅助工具（P0 阶段用，纯编辑器工具，不进打包）
/// 菜单：Battle / UI / ...
/// </summary>
public static class UIUpgradeTools
{
    private const string SourceFontPath = "Assets/Fonts/NotoSansSC-VF.ttf";
    private const string FontAssetPath = "Assets/Fonts/NotoSansSC SDF.asset";

    // =========================
    // 1. 生成中文 TMP 字体资源
    //    · 动态图集：用到哪个字才烘哪个字，不用预烘几千个汉字
    //    · 生成后自动挂到默认字体（LiberationSans SDF）的 Fallback 上，
    //      这样场景里已有的 TMP 文本不用逐个改就能显示中文
    //    · Noto Sans SC 是 OFL 授权，可以随游戏发布
    // =========================
    [MenuItem("Battle/UI/1. 生成中文TMP字体资源（Noto Sans SC）")]
    public static void CreateChineseFontAsset()
    {
        Font source = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);

        if (source == null)
        {
            Debug.LogError(
                $"找不到字体文件：{SourceFontPath}\n" +
                $"确认 Assets/Fonts/ 下有 NotoSansSC-VF.ttf"
            );

            return;
        }

        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            source,
            90,                           // 采样大小
            9,                            // padding
            GlyphRenderMode.SDFAA,
            1024,                         // 图集宽
            1024,                         // 图集高
            AtlasPopulationMode.Dynamic,  // 按需烘字
            true                          // 多图集（汉字多，必须开）
        );

        if (fontAsset == null)
        {
            Debug.LogError(
                "生成字体资源失败：FontEngine 读不了这个字体文件。" +
                "如果是因为它是变量字体(VF)，改用静态版 NotoSansSC-Regular.otf 再试"
            );

            return;
        }

        fontAsset.name = "NotoSansSC SDF";

        //先建资源，再把 图集 / 材质 作为子资源存进去，否则重启后引用会丢
        AssetDatabase.CreateAsset(fontAsset, FontAssetPath);

        fontAsset.atlasTextures[0].name = fontAsset.name + " Atlas";
        AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);

        fontAsset.material.name = fontAsset.name + " Material";
        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(FontAssetPath);

        //挂到默认字体的 Fallback：已有 TMP 文本立刻能显示中文
        TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;

        if (defaultFont == null)
        {
            Debug.LogWarning(
                $"中文 TMP 字体已生成：{FontAssetPath}\n" +
                $"但没找到 TMP 默认字体，请手动把它挂到字体的 Fallback 上"
            );

            return;
        }

        if (!defaultFont.fallbackFontAssetTable.Contains(fontAsset))
        {
            defaultFont.fallbackFontAssetTable.Add(fontAsset);
            EditorUtility.SetDirty(defaultFont);
            AssetDatabase.SaveAssets();
        }

        Debug.Log(
            $"中文 TMP 字体已生成：{FontAssetPath}\n" +
            $"已挂到默认字体「{defaultFont.name}」的 Fallback" +
            $"（当前 {defaultFont.fallbackFontAssetTable.Count} 个回退字体）"
        );
    }

    // =========================
    // 2. Canvas 缩放改为 1920x1080 自适应
    //    现在场景里是 Constant Pixel Size，换成锚点布局后
    //    不同分辨率会错位，先把这个改掉
    // =========================
    [MenuItem("Battle/UI/2. Canvas 缩放改为1920x1080（Scale With Screen Size）")]
    public static void SetupCanvasScaler()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();

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
            "Canvas 已改为 Scale With Screen Size（1920×1080，Match 0.5）\n" +
            "场景已标记为已修改 —— 记得按 Ctrl+S 保存"
        );
    }
}
