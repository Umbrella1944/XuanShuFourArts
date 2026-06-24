using System;
using System.Collections.Generic;
using System.Reflection;
using CoreSkillGrowth.Shared;
using FrameWork;
using GameData.Domains.CombatSkill;
using Game.Views.Combat;
using Game.Views.CharacterMenu;
using Game.Views.MouseTips;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace XuanShuFourArts.Frontend;

internal static class FourSetUiState
{
    private const string Marker = "<!--XuanShuFourArtsSet-->";
    private static int _currentCount = -1;

    internal static void UpdateFrom(ICollection<short> equippedSkills)
    {
        int count = 0;
        if (equippedSkills != null)
        {
            if (equippedSkills.Contains(CoreSkillGrowthConfigPatch.PeiRanJue))
            {
                count++;
            }

            if (equippedSkills.Contains(CoreSkillGrowthConfigPatch.XiaoZongYueGong))
            {
                count++;
            }

            if (equippedSkills.Contains(CoreSkillGrowthConfigPatch.ShuiHuoYingQiGong))
            {
                count++;
            }

            if (equippedSkills.Contains(CoreSkillGrowthConfigPatch.TaiZuChangQuan))
            {
                count++;
            }
        }

        _currentCount = count;
    }

    internal static void UpdateFromPlans(IEnumerable<ICollection<short>> equippedSkillPlans)
    {
        HashSet<short> equippedSkills = new HashSet<short>();
        if (equippedSkillPlans != null)
        {
            foreach (ICollection<short> skillPlan in equippedSkillPlans)
            {
                if (skillPlan == null)
                {
                    continue;
                }

                foreach (short skillId in skillPlan)
                {
                    equippedSkills.Add(skillId);
                }
            }
        }

        UpdateFrom(equippedSkills);
    }

    internal static bool IsSetSkill(short skillId)
    {
        return skillId == CoreSkillGrowthConfigPatch.PeiRanJue ||
            skillId == CoreSkillGrowthConfigPatch.XiaoZongYueGong ||
            skillId == CoreSkillGrowthConfigPatch.ShuiHuoYingQiGong ||
            skillId == CoreSkillGrowthConfigPatch.TaiZuChangQuan;
    }

    internal static string BuildSetHint()
    {
        string title;
        string desc;
        string color = _currentCount >= 4 ? "#D8C06A" : "#8A8F8A";
        bool detailMode = IsDetailMode();
        int displayCount = Math.Max(0, _currentCount);

        if (IsEnglishLanguage())
        {
            title = $"Mystic Pivot Four Arts {displayCount}/4";
            desc = detailMode
                ? "4-piece set effect\nFour Arts Resonance: Attack, Defense, Hit, and Deflection +15%.\nBlood-Rift - Hairline Rift: Normal Attack Power +100%.\nBlood-Rift - Tide of Aftershadows: Normal Attack hits have a 20% chance to gain 2 Blood-Rift stacks and trigger 1 forced-hit, guaranteed-critical Blood-Rift Pursuit; each stack increases pursuit power by 200%, up to 10 stacks for the battle. Blood-Rift Pursuit cannot trigger bounce damage. It consumes all stacks on damage settlement; if interrupted, it loses 1 stack.\nReturning-Frame - Wounds Return to Pivot: Inner Art passive recovery +42.9%.\nShadow Step - Twin Shadows: Shadow-Reflecting Wandering Step's active Movement Speed bonus +40% while active.\nTwofold Qi - Double Rampart: Yin-Yang Form-Restoring Art's active Phy. Defense, Qi Defense, Resistance, Parry, and Dodge bonuses +40% while active."
                : "4pc: Major combat attributes +15%.\nHold Alt for details.";
        }
        else
        {
            title = $"玄枢四诀 {displayCount}/4";
            desc = detailMode
                ? "4件套装效果\n四诀共鸣：攻击、防御、命中、化解 +15%。\n万噬血隙·一线微裂：普通攻击威力 +100%。\n万噬血隙·后影潮复：普通攻击命中时，有20%机率获得2层血隙并触发1次必中、必重创的血隙追击；每层使血隙追击威力+200%，最多10层并持续整场战斗；血隙追击无法被反震，进入伤害结算时消耗全部层数，若被中断则损失1层。\n玄息归骸·百损归枢：内功被动恢复效果 +42.9%。\n照影游身·逐影成双：照影游身步持续期间，施展移动速度提高效果 +40%。\n两仪还形·二气重垣：两仪还形功持续期间，施展提供的御体、御气、卸力、拆招、闪避效果 +40%。"
                : "4件：主战斗属性 +15%。\n按住 Alt 查看详细效果。";
        }

        return
            $"<color={color}><b>{title}</b></color>\n" +
            $"<color=#9EC8C5>{desc}</color>";
    }

    internal static bool IsDetailMode()
    {
        return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
    }

    internal static string BuildSetDetail()
    {
        int displayCount = Math.Max(0, _currentCount);
        string activeColor = _currentCount >= 4 ? "#D8C06A" : "#8A8F8A";

        if (IsEnglishLanguage())
        {
            return
                $"<color={activeColor}><b>Mystic Pivot Four Arts</b></color>\n" +
                $"Progress: {displayCount}/4\n" +
                "Four Arts Resonance: Attack, Defense, Hit, and Deflection +15%.\n" +
                "Blood-Rift - Hairline Rift: Normal Attack Power +100%.\n" +
                "Blood-Rift - Tide of Aftershadows: Normal Attack hits have a 20% chance to gain 2 Blood-Rift stacks and trigger 1 forced-hit, guaranteed-critical Blood-Rift Pursuit; each stack increases pursuit power by 200%, up to 10 stacks for the battle. Blood-Rift Pursuit cannot trigger bounce damage. It consumes all stacks on damage settlement; if interrupted, it loses 1 stack.\n" +
                "Returning-Frame - Wounds Return to Pivot: Inner Art passive recovery +42.9%.\n" +
                "Shadow Step - Twin Shadows: Shadow-Reflecting Wandering Step's active Movement Speed bonus +40% while active.\n" +
                "Twofold Qi - Double Rampart: Yin-Yang Form-Restoring Art's active Phy. Defense, Qi Defense, Resistance, Parry, and Dodge bonuses +40% while active.";
        }

        return
            $"<color={activeColor}><b>玄枢四诀 套装效果</b></color>\n" +
            $"当前进度：{displayCount}/4\n" +
            "四诀共鸣：攻击、防御、命中、化解 +15%。\n" +
            "万噬血隙·一线微裂：普通攻击威力 +100%。\n" +
            "万噬血隙·后影潮复：普通攻击命中时，有20%机率获得2层血隙并触发1次必中、必重创的血隙追击；每层使血隙追击威力+200%，最多10层并持续整场战斗；血隙追击无法被反震，进入伤害结算时消耗全部层数，若被中断则损失1层。\n" +
            "玄息归骸·百损归枢：内功被动恢复效果 +42.9%。\n" +
            "照影游身·逐影成双：照影游身步持续期间，施展移动速度提高效果 +40%。\n" +
            "两仪还形·二气重垣：两仪还形功持续期间，施展提供的御体、御气、卸力、拆招、闪避效果 +40%。";
    }

    internal static string AppendSetHint(short skillId, string originalText)
    {
        if (!IsSetSkill(skillId) || string.IsNullOrEmpty(originalText) || originalText.Contains(Marker))
        {
            return originalText;
        }

        return originalText + "\n\n" + Marker + BuildSetHint();
    }

    internal static string RemoveSetHint(string originalText)
    {
        if (string.IsNullOrEmpty(originalText))
        {
            return originalText;
        }

        int markerIndex = originalText.IndexOf(Marker, StringComparison.Ordinal);
        return markerIndex >= 0
            ? originalText.Substring(0, markerIndex).TrimEnd()
            : originalText;
    }

    private static bool IsEnglishLanguage()
    {
        try
        {
            string languageKey = LocalStringManager.CurLanguageKey;
            return string.Equals(languageKey, "EN", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(languageKey, "English", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}

[HarmonyPatch(typeof(ViewCombat), "OnEnable")]
internal static class FourSetCombatViewEnablePatch
{
    [HarmonyPostfix]
    private static void MarkCombatViewActive(ViewCombat __instance)
    {
        FourSetCombatViewState.MarkActive(__instance);
    }
}

[HarmonyPatch(typeof(ViewCombat), "OnCombatEnd")]
internal static class FourSetCombatViewEndPatch
{
    [HarmonyPostfix]
    private static void MarkCombatViewInactive(ViewCombat __instance)
    {
        FourSetCombatViewState.MarkInactive(__instance);
    }
}

[HarmonyPatch(typeof(ViewCombat), "OnDestroy")]
internal static class FourSetCombatViewDestroyPatch
{
    [HarmonyPostfix]
    private static void MarkCombatViewDestroyed(ViewCombat __instance)
    {
        FourSetCombatViewState.MarkInactive(__instance);
    }
}

internal static class FourSetCombatViewState
{
    private static readonly HashSet<int> ActiveCombatViews = new HashSet<int>();

    internal static bool ShouldSuppressSetHint => ActiveCombatViews.Count > 0;

    internal static void MarkActive(ViewCombat view)
    {
        if (view != null)
        {
            ActiveCombatViews.Add(view.GetInstanceID());
        }

        FourSetNewTooltipCombatSkillPatch.HideFloatingHint();
    }

    internal static void MarkInactive(ViewCombat view)
    {
        if (view != null)
        {
            ActiveCombatViews.Remove(view.GetInstanceID());
        }

        FourSetNewTooltipCombatSkillPatch.HideFloatingHint();
    }
}

[HarmonyPatch(typeof(ViewCharacterMenuEquipCombatSkill))]
internal static class FourSetNewEquipSkillCachePatch
{
    private static readonly FieldInfo EquippedSkillsField =
        AccessTools.Field(typeof(ViewCharacterMenuEquipCombatSkill), "EquippedSkills");

    [HarmonyPostfix]
    [HarmonyPatch("Refresh", new Type[] { typeof(bool) })]
    private static void CacheAfterRefresh(ViewCharacterMenuEquipCombatSkill __instance)
    {
        CacheEquippedSkills(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("UpdateDisplayData")]
    private static void CacheAfterUpdateDisplayData(ViewCharacterMenuEquipCombatSkill __instance)
    {
        CacheEquippedSkills(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("AddEquippedCombatSkill", new Type[] { typeof(CombatSkillDisplayDataCharacterMenuListItem) })]
    private static void CacheAfterAdd(ViewCharacterMenuEquipCombatSkill __instance)
    {
        CacheEquippedSkills(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("RemoveEquippedCombatSkill", new Type[] { typeof(short) })]
    private static void CacheAfterRemove(ViewCharacterMenuEquipCombatSkill __instance)
    {
        CacheEquippedSkills(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("ResetEquippedSkills")]
    private static void CacheAfterReset(ViewCharacterMenuEquipCombatSkill __instance)
    {
        CacheEquippedSkills(__instance);
    }

    private static void CacheEquippedSkills(ViewCharacterMenuEquipCombatSkill instance)
    {
        try
        {
            FourSetUiState.UpdateFromPlans(
                EquippedSkillsField?.GetValue(instance) as IEnumerable<ICollection<short>>);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] four-set new equip cache failed: {ex}");
        }
    }
}

[HarmonyPatch(typeof(UI_CharacterMenuEquipCombatSkill), "RefreshEquippedCombatSkills")]
internal static class FourSetEquippedSkillCachePatch
{
    private static readonly FieldInfo EquippedCombatSkillListField =
        AccessTools.Field(typeof(UI_CharacterMenuEquipCombatSkill), "_equippedCombatSkillList");

    [HarmonyPostfix]
    private static void CacheEquippedSkills(UI_CharacterMenuEquipCombatSkill __instance)
    {
        try
        {
            FourSetUiState.UpdateFrom(
                EquippedCombatSkillListField?.GetValue(__instance) as ICollection<short>);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] four-set UI cache failed: {ex}");
        }
    }
}

[HarmonyPatch(typeof(ViewCombat), "OnGetProactiveSkillList")]
internal static class FourSetCombatSkillCachePatch
{
    [HarmonyPostfix]
    private static void CacheCombatSkillList(int charId)
    {
        try
        {
            CombatModel model = SingletonObject.getInstance<CombatModel>();
            if (model == null || charId != model.SelfCharId)
            {
                return;
            }

            if (!model.ProactiveSkillData.TryGetValue(charId, out IReadOnlyList<CombatSkillDisplayData> skills))
            {
                return;
            }

            List<short> equippedSkills = new List<short>(skills.Count);
            for (int i = 0; i < skills.Count; i++)
            {
                CombatSkillDisplayData skill = skills[i];
                if (skill != null)
                {
                    equippedSkills.Add(skill.TemplateId);
                }
            }

            FourSetUiState.UpdateFrom(equippedSkills);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] four-set combat cache failed: {ex}");
        }
    }
}

[HarmonyPatch(typeof(MouseTipCombatSkill), "RefreshCombatSkillPanel")]
internal static class FourSetMouseTipPanelPatch
{
    private const string HintNodeName = "XuanShuFourArtsSetHint";
    private static readonly FieldInfo CombatSkillTemplateIdField =
        AccessTools.Field(typeof(MouseTipCombatSkill), "_combatSkillTemplateId");

    [HarmonyPostfix]
    internal static void RefreshSetHint(MouseTipCombatSkill __instance)
    {
        try
        {
            TextMeshProUGUI hint = FindHint(__instance);
            if (FourSetCombatViewState.ShouldSuppressSetHint)
            {
                if (hint != null)
                {
                    hint.gameObject.SetActive(false);
                }

                return;
            }

            short skillId = Convert.ToInt16(CombatSkillTemplateIdField.GetValue(__instance));
            if (!FourSetUiState.IsSetSkill(skillId))
            {
                if (hint != null)
                {
                    hint.gameObject.SetActive(false);
                }

                return;
            }

            hint ??= CreateHint(__instance);
            if (hint == null)
            {
                return;
            }

            hint.text = FourSetUiState.BuildSetHint();
            ApplyHintLayout(hint);
            hint.gameObject.SetActive(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] four-set mouse tip failed: {ex}");
        }
    }

    private static TextMeshProUGUI FindHint(MouseTipCombatSkill mouseTip)
    {
        Transform existing = mouseTip.transform.Find(HintNodeName);
        if (existing != null)
        {
            return existing.GetComponent<TextMeshProUGUI>();
        }

        TextMeshProUGUI[] texts = mouseTip.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].gameObject.name == HintNodeName)
            {
                return texts[i];
            }
        }

        return null;
    }

    private static void ApplyHintLayout(TextMeshProUGUI hint)
    {
        LayoutElement layoutElement = hint.GetComponent<LayoutElement>() ??
            hint.gameObject.AddComponent<LayoutElement>();

        bool detailMode = FourSetUiState.IsDetailMode();
        float preferredWidth = detailMode ? 720f : 560f;
        float preferredHeight = Mathf.Clamp(
            hint.GetPreferredValues(hint.text ?? string.Empty, preferredWidth - 24f, 0f).y + 18f,
            detailMode ? 144f : 72f,
            detailMode ? 280f : 190f);

        layoutElement.minWidth = 520f;
        layoutElement.preferredWidth = preferredWidth;
        layoutElement.minHeight = preferredHeight;
        layoutElement.preferredHeight = preferredHeight;
    }

    private static TextMeshProUGUI CreateHint(MouseTipCombatSkill mouseTip)
    {
        TextMeshProUGUI anchor =
            mouseTip.CGet<TextMeshProUGUI>("ReverseEffectDesc") ??
            mouseTip.CGet<TextMeshProUGUI>("DirectEffectDesc") ??
            mouseTip.CGet<TextMeshProUGUI>("Description") ??
            FindLastText(mouseTip);

        Transform parent = anchor != null ? anchor.transform.parent : mouseTip.transform;
        GameObject hintObject;
        if (anchor != null)
        {
            hintObject = UnityEngine.Object.Instantiate(anchor.gameObject, parent, false);
        }
        else
        {
            hintObject = new GameObject(HintNodeName, typeof(RectTransform), typeof(TextMeshProUGUI));
            hintObject.transform.SetParent(parent, false);
        }

        hintObject.name = HintNodeName;
        hintObject.transform.SetAsLastSibling();

        TextMeshProUGUI hint = hintObject.GetComponent<TextMeshProUGUI>() ??
            hintObject.AddComponent<TextMeshProUGUI>();

        hint.raycastTarget = false;
        hint.richText = true;
        hint.enableWordWrapping = true;
        hint.alignment = TextAlignmentOptions.TopLeft;
        hint.color = new Color32(158, 200, 197, 255);

        if (anchor != null)
        {
            hint.font = anchor.font;
            hint.fontSize = Mathf.Max(18f, anchor.fontSize * 0.88f);
            hint.lineSpacing = anchor.lineSpacing;
        }
        else
        {
            hint.fontSize = 22f;
        }

        RectTransform rect = hintObject.GetComponent<RectTransform>();
        LayoutGroup layoutGroup = parent.GetComponent<LayoutGroup>();
        if (layoutGroup != null)
        {
            LayoutElement layoutElement = hintObject.GetComponent<LayoutElement>() ??
                hintObject.AddComponent<LayoutElement>();

            layoutElement.minHeight = 56f;
            layoutElement.preferredHeight = 70f;

            if (anchor != null)
            {
                hintObject.transform.SetSiblingIndex(anchor.transform.GetSiblingIndex() + 1);
            }
        }
        else if (rect != null)
        {
            rect.anchorMin = new Vector2(0.04f, 0.02f);
            rect.anchorMax = new Vector2(0.96f, 0.02f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 78f);
            rect.sizeDelta = new Vector2(0f, 72f);
        }

        return hint;
    }

    private static TextMeshProUGUI FindLastText(MouseTipCombatSkill mouseTip)
    {
        TextMeshProUGUI[] texts = mouseTip.GetComponentsInChildren<TextMeshProUGUI>(true);
        return texts.Length > 0 ? texts[texts.Length - 1] : null;
    }
}

[HarmonyPatch(typeof(MouseTipCombatSkill), "Refresh", new Type[] { typeof(FrameWork.ArgumentBox) })]
internal static class FourSetMouseTipRefreshWithArgsPatch
{
    [HarmonyPostfix]
    private static void RefreshSetHint(MouseTipCombatSkill __instance)
    {
        FourSetMouseTipPanelPatch.RefreshSetHint(__instance);
    }
}

[HarmonyPatch(typeof(MouseTipCombatSkill), "Refresh", new Type[] { })]
internal static class FourSetMouseTipRefreshPatch
{
    [HarmonyPostfix]
    private static void RefreshSetHint(MouseTipCombatSkill __instance)
    {
        FourSetMouseTipPanelPatch.RefreshSetHint(__instance);
    }
}

[HarmonyPatch(typeof(MouseTipCombatSkill), "Update")]
internal static class FourSetMouseTipUpdatePatch
{
    [HarmonyPostfix]
    private static void RefreshSetHint(MouseTipCombatSkill __instance)
    {
        FourSetMouseTipPanelPatch.RefreshSetHint(__instance);
    }
}

[HarmonyPatch(typeof(TooltipCombatSkill), "Refresh", new Type[] { typeof(FrameWork.ArgumentBox) })]
internal static class FourSetNewTooltipCombatSkillRefreshPatch
{
    [HarmonyPostfix]
    private static void RefreshSetHint(TooltipCombatSkill __instance)
    {
        FourSetNewTooltipCombatSkillPatch.RefreshSetHint(__instance);
    }
}

[HarmonyPatch(typeof(TooltipCombatSkill), "Update")]
internal static class FourSetNewTooltipCombatSkillUpdatePatch
{
    [HarmonyPostfix]
    private static void RefreshSetHint(TooltipCombatSkill __instance)
    {
        FourSetNewTooltipCombatSkillPatch.RefreshSetHint(__instance);
    }
}

[HarmonyPatch(typeof(MouseTipBase), "OnDisable")]
internal static class FourSetTooltipDisablePatch
{
    [HarmonyPostfix]
    private static void HideSetHint(MouseTipBase __instance)
    {
        if (__instance is TooltipCombatSkill)
        {
            FourSetNewTooltipCombatSkillPatch.HideFloatingHint();
        }
    }
}

internal static class FourSetNewTooltipCombatSkillPatch
{
    private const string HintNodeName = "XuanShuFourArtsSetHintNew";
    private const string PanelNodeName = "XuanShuFourArtsSetHintPanelNew";
    private const string OverlayCanvasName = "XuanShuFourArtsOverlayCanvas";
    private static RectTransform _floatingPanelRect;
    private static TextMeshProUGUI _floatingHint;
    private static RectTransform _overlayCanvasRect;

    private static readonly FieldInfo CombatSkillTemplateIdField =
        AccessTools.Field(typeof(TooltipCombatSkill), "_combatSkillTemplateId");

    private static readonly FieldInfo ReverseEffectDescTextField =
        AccessTools.Field(typeof(TooltipCombatSkill), "reverseEffectDescText");

    private static readonly FieldInfo DirectEffectDescTextField =
        AccessTools.Field(typeof(TooltipCombatSkill), "directEffectDescText");

    private static readonly FieldInfo DescTextField =
        AccessTools.Field(typeof(TooltipCombatSkill), "descText");

    internal static void RefreshSetHint(TooltipCombatSkill tooltip)
    {
        try
        {
            TextMeshProUGUI hint = FindHint();
            if (FourSetCombatViewState.ShouldSuppressSetHint)
            {
                SetHintVisible(hint, false);
                return;
            }

            short skillId = Convert.ToInt16(CombatSkillTemplateIdField.GetValue(tooltip));
            bool isSetSkill = FourSetUiState.IsSetSkill(skillId);
            if (!isSetSkill)
            {
                SetHintVisible(hint, false);

                return;
            }

            hint ??= CreateHint(tooltip);
            if (hint == null)
            {
                return;
            }

            hint.text = FourSetUiState.BuildSetHint();
            ApplyHintLayout(tooltip, hint);
            SetHintVisible(hint, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] four-set new tooltip failed: {ex}");
        }
    }

    private static TextMeshProUGUI FindHint()
    {
        return _floatingHint != null ? _floatingHint : null;
    }

    private static void ApplyHintLayout(TooltipCombatSkill tooltip, TextMeshProUGUI hint)
    {
        bool detailMode = FourSetUiState.IsDetailMode();
        RectTransform panelRect = _floatingPanelRect;
        if (panelRect != null)
        {
            float width = detailMode ? 640f : 430f;
            float preferredHeight = Mathf.Clamp(
                hint.GetPreferredValues(hint.text ?? string.Empty, width - 32f, 0f).y + 28f,
                detailMode ? 170f : 90f,
                detailMode ? 420f : 210f);

            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(width, preferredHeight);
            UpdatePanelPosition(tooltip, panelRect);
        }

        RectTransform textRect = hint.GetComponent<RectTransform>();
        if (textRect != null)
        {
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.offsetMin = new Vector2(16f, 10f);
            textRect.offsetMax = new Vector2(-16f, -10f);
        }
    }

    private static TextMeshProUGUI CreateHint(TooltipCombatSkill tooltip)
    {
        Canvas canvas = EnsureOverlayCanvas();
        if (canvas == null)
        {
            return null;
        }
        if (_floatingHint != null && _floatingPanelRect != null)
        {
            return _floatingHint;
        }

        TextMeshProUGUI anchor =
            ReverseEffectDescTextField.GetValue(tooltip) as TextMeshProUGUI ??
            DirectEffectDescTextField.GetValue(tooltip) as TextMeshProUGUI ??
            DescTextField.GetValue(tooltip) as TextMeshProUGUI ??
            FindLastText(tooltip);

        GameObject panelObject = new GameObject(
            PanelNodeName,
            typeof(RectTransform),
            typeof(Image));
        panelObject.transform.SetParent(canvas.transform, false);
        panelObject.transform.SetAsLastSibling();
        panelObject.layer = canvas.gameObject.layer;

        Image background = panelObject.GetComponent<Image>();
        background.color = new Color32(8, 18, 18, 226);
        background.raycastTarget = false;

        GameObject hintObject = new GameObject(
            HintNodeName,
            typeof(RectTransform),
            typeof(TextMeshProUGUI));
        hintObject.transform.SetParent(panelObject.transform, false);
        hintObject.name = HintNodeName;
        hintObject.layer = canvas.gameObject.layer;

        TextMeshProUGUI hint = hintObject.GetComponent<TextMeshProUGUI>();

        hint.raycastTarget = false;
        hint.richText = true;
        hint.enableWordWrapping = true;
        hint.alignment = TextAlignmentOptions.TopLeft;
        hint.color = new Color32(158, 200, 197, 255);

        if (anchor != null)
        {
            hint.font = anchor.font;
            hint.fontSize = Mathf.Max(18f, anchor.fontSize * 0.88f);
            hint.lineSpacing = anchor.lineSpacing;
        }
        else
        {
            hint.fontSize = 22f;
        }

        _floatingPanelRect = panelObject.GetComponent<RectTransform>();
        _floatingHint = hint;

        ApplyHintLayout(tooltip, hint);

        return hint;
    }

    private static Canvas EnsureOverlayCanvas()
    {
        if (_overlayCanvasRect != null)
        {
            return _overlayCanvasRect.GetComponent<Canvas>();
        }

        GameObject existing = GameObject.Find(OverlayCanvasName);
        GameObject canvasObject = existing != null
            ? existing
            : new GameObject(
                OverlayCanvasName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

        UnityEngine.Object.DontDestroyOnLoad(canvasObject);
        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer >= 0)
        {
            canvasObject.layer = uiLayer;
        }

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 32760;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GraphicRaycaster raycaster = canvasObject.GetComponent<GraphicRaycaster>();
        raycaster.enabled = false;

        _overlayCanvasRect = canvasObject.GetComponent<RectTransform>();
        _overlayCanvasRect.anchorMin = Vector2.zero;
        _overlayCanvasRect.anchorMax = Vector2.one;
        _overlayCanvasRect.pivot = new Vector2(0.5f, 0.5f);
        _overlayCanvasRect.anchoredPosition = Vector2.zero;
        _overlayCanvasRect.sizeDelta = Vector2.zero;

        return canvas;
    }

    private static void UpdatePanelPosition(TooltipCombatSkill tooltip, RectTransform panelRect)
    {
        Canvas canvas = EnsureOverlayCanvas();
        RectTransform canvasRect = _overlayCanvasRect;
        RectTransform tooltipRect = tooltip.GetComponent<RectTransform>();
        if (canvas == null || canvasRect == null || tooltipRect == null)
        {
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, 0f);
            return;
        }

        Vector2 size = panelRect.sizeDelta;
        if (FourSetUiState.IsDetailMode())
        {
            panelRect.pivot = new Vector2(1f, 0f);
            float x = Mathf.Clamp(
                canvasRect.rect.xMax - 28f,
                canvasRect.rect.xMin + size.x + 16f,
                canvasRect.rect.xMax - 16f);
            float y = Mathf.Clamp(
                canvasRect.rect.yMin + 122f,
                canvasRect.rect.yMin + 16f,
                canvasRect.rect.yMax - size.y - 16f);
            panelRect.anchoredPosition = new Vector2(x, y);
            return;
        }

        Vector3[] corners = new Vector3[4];
        tooltipRect.GetWorldCorners(corners);

        Canvas sourceCanvas = tooltip.GetComponentInParent<Canvas>();
        Camera sourceCamera = sourceCanvas != null && sourceCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? sourceCanvas.worldCamera
            : null;

        Vector2 leftBottomLocal;
        Vector2 leftTopLocal;
        Vector2 rightTopLocal;
        Vector2 rightBottomLocal;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(sourceCamera, corners[0]),
            null,
            out leftBottomLocal);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(sourceCamera, corners[1]),
            null,
            out leftTopLocal);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(sourceCamera, corners[2]),
            null,
            out rightTopLocal);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(sourceCamera, corners[3]),
            null,
            out rightBottomLocal);

        float tipLeft = Mathf.Min(leftBottomLocal.x, leftTopLocal.x, rightTopLocal.x, rightBottomLocal.x);
        float tipRight = Mathf.Max(leftBottomLocal.x, leftTopLocal.x, rightTopLocal.x, rightBottomLocal.x);
        float tipBottom = Mathf.Min(leftBottomLocal.y, leftTopLocal.y, rightTopLocal.y, rightBottomLocal.y);
        float tipTop = Mathf.Max(leftBottomLocal.y, leftTopLocal.y, rightTopLocal.y, rightBottomLocal.y);
        float tipCenterY = (tipBottom + tipTop) * 0.5f;

        const float gap = 18f;
        bool canPlaceRight = tipRight + gap + size.x <= canvasRect.rect.xMax - 16f;
        bool canPlaceLeft = tipLeft - gap - size.x >= canvasRect.rect.xMin + 16f;
        bool placeRight = canPlaceRight || !canPlaceLeft;
        if (FourSetUiState.IsDetailMode() && canPlaceLeft)
        {
            // In the practice screen, the right side is usually the attribute panel
            // players compare while holding Alt, so detailed set text avoids that area.
            float rightAnalysisAreaStart = canvasRect.rect.xMax - canvasRect.rect.width * 0.28f;
            bool rightPlacementBlocksAnalysisArea = tipRight + gap + size.x > rightAnalysisAreaStart;
            if (rightPlacementBlocksAnalysisArea)
            {
                placeRight = false;
            }
        }

        panelRect.pivot = placeRight ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
        Vector2 position = placeRight
            ? new Vector2(tipRight + gap, tipCenterY)
            : new Vector2(tipLeft - gap, tipCenterY);

        float minX = placeRight ? canvasRect.rect.xMin + 16f : canvasRect.rect.xMin + size.x + 16f;
        float maxX = placeRight ? canvasRect.rect.xMax - size.x - 16f : canvasRect.rect.xMax - 16f;
        float minY = canvasRect.rect.yMin + size.y * 0.5f + 16f;
        float maxY = canvasRect.rect.yMax - size.y * 0.5f - 16f;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        panelRect.anchoredPosition = position;
    }

    internal static void HideFloatingHint()
    {
        if (_floatingPanelRect != null)
        {
            _floatingPanelRect.gameObject.SetActive(false);
        }
    }

    private static void SetHintVisible(TextMeshProUGUI hint, bool visible)
    {
        if (hint == null)
        {
            return;
        }

        GameObject target = _floatingPanelRect != null ? _floatingPanelRect.gameObject : hint.gameObject;
        target.SetActive(visible);
        if (visible && _floatingPanelRect != null)
        {
            _floatingPanelRect.SetAsLastSibling();
        }
    }

    private static TextMeshProUGUI FindLastText(TooltipCombatSkill tooltip)
    {
        TextMeshProUGUI[] texts = tooltip.GetComponentsInChildren<TextMeshProUGUI>(true);
        return texts.Length > 0 ? texts[texts.Length - 1] : null;
    }
}

[HarmonyPatch(typeof(CombatSkillIntro), "Refresh")]
internal static class FourSetCombatSkillIntroPatch
{
    [HarmonyPostfix]
    private static void AppendSetHint(CombatSkillIntro __instance, CombatSkillDisplayData displayData)
    {
        try
        {
            TextMeshProUGUI desc = __instance.CGet<TextMeshProUGUI>("Description");
            if (desc == null)
            {
                return;
            }

            if (FourSetCombatViewState.ShouldSuppressSetHint)
            {
                desc.text = FourSetUiState.RemoveSetHint(desc.text);
                return;
            }

            if (displayData == null || !FourSetUiState.IsSetSkill(displayData.TemplateId))
            {
                return;
            }

            desc.text = FourSetUiState.AppendSetHint(displayData.TemplateId, desc.text);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[XuanShuFourArts] four-set intro failed: {ex}");
        }
    }
}
