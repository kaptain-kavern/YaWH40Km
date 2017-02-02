using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    [StaticConstructorOnStartup]
    public class CorruptionStoryTrackerUtilities
    {

        public static Texture2D ButtonIG = ContentFinder<Texture2D>.Get("UI/Buttons/ButtonIG", true);
        public static Texture2D ButtonAM = ContentFinder<Texture2D>.Get("UI/Buttons/ButtonAM", true);
        public static Texture2D ButtonAS = ContentFinder<Texture2D>.Get("UI/Buttons/ButtonAS", true);
        public static Texture2D PlanetMedium = ContentFinder<Texture2D>.Get("UI/SectorMap/Planet_Medium", true);
        public static Texture2D PlanetSmall = ContentFinder<Texture2D>.Get("UI/SectorMap/Planet_Small", true);
        public static Texture2D Moon = ContentFinder<Texture2D>.Get("UI/SectorMap/Moon", true);

        public static CorruptionStoryTracker currentStoryTracker
        {
            get
            {
                return Find.WorldObjects.AllWorldObjects.FirstOrDefault(x => x.def == CorruptionDefOfs.CorruptionStoryTracker) as CorruptionStoryTracker;
            }
        }

        public static string ReturnImperialFactionDescription(Faction faction)
        {

            return "IG_CCM_ContactFaction".Translate(new object[]
            {
                    faction.Name
            });

        }

        public static void DrawCorruptionStoryTrackerTab(CorruptionStoryTracker tracker, Rect rect)
        {
            GUI.BeginGroup(rect);
            Rect rect2 = new Rect(rect.x, rect.y + 20f, rect.width, 55f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect2, Faction.OfPlayer.Name);
            Text.Font = GameFont.Small;

            Rect rect3 = rect2;
            rect3.y = rect2.yMax + 30f;
            rect3.width = 200f;
            rect3.height = 25f;
            
            float num = rect.y + 30f;
            CorruptionStoryTrackerUtilities.ListSeparatorBig(ref num, rect.width, "ImperialInstitutions".Translate());
            foreach (Faction current in tracker.ImperialFactions)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.2f);
                Widgets.DrawLineHorizontal(0f, num, rect.width);
                GUI.color = Color.white;
                num += CorruptionStoryTrackerUtilities.DrawImperialFactionRow(current, num, rect);
            }
            GUI.EndGroup();
        }
        public static void ListSeparatorBig(ref float curY, float width, string label)
        {
            Color color = GUI.color;
            curY += 3f;
            GUI.color = Widgets.SeparatorLabelColor;
            Rect rect = new Rect(0f, curY, width, 50f);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Small;
            Widgets.Label(rect, label);
            curY += 20f;
            GUI.color = new Color(0.3f, 0.3f, 0.3f, 1f); ;
            Widgets.DrawLineHorizontal(0f, curY+5f, width);
            curY += 5f;
            GUI.color = color;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private static float DrawImperialFactionRow(Faction faction, float rowY, Rect fillRect)
        {
            Rect rect = new Rect(35f, rowY, 200f, 80f);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Faction current in Find.FactionManager.AllFactionsVisible)
            {
                if (current != faction && !current.IsPlayer && !current.def.hidden)
                {
                    if (faction.HostileTo(current))
                    {
                        stringBuilder.AppendLine("HostileTo".Translate(new object[]
                        {
                            current.Name
                        }));
                    }
                }
            }
            string text = stringBuilder.ToString();
            float width = fillRect.width - rect.xMax;
            float num = Text.CalcHeight(text, width);
            float num2 = Mathf.Max(80f, num);
            Rect position = new Rect(10f, rowY + 10f, 15f, 15f);
            Rect rect2 = new Rect(0f, rowY, fillRect.width, num2);
            if (Mouse.IsOver(rect2))
            {
                GUI.DrawTexture(rect2, TexUI.HighlightTex);
            }
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
   //         Widgets.DrawRectFast(position, faction.Color, null);
            string label = string.Concat(new string[]
            {
                faction.Name,
                "\n",
                faction.def.LabelCap,
                "\n",
                (faction.leader == null) ? string.Empty : (faction.def.leaderTitle + ": " + faction.leader.Name.ToStringFull)
            });
            Widgets.Label(rect, label);
            Rect rect3 = new Rect(rect.xMax, rowY, 60f, 80f);
            Widgets.InfoCardButton(rect3.x, rect3.y, faction.def);
            Rect rect4 = new Rect(rect3.xMax, rowY, 200f, 80f);
            string text2 = Mathf.RoundToInt(faction.GoodwillWith(Faction.OfPlayer)).ToStringCached();
            if (Faction.OfPlayer.HostileTo(faction))
            {
                text2 = text2 + "\n" + "Hostile".Translate();
            }
            if (faction.defeated)
            {
                text2 = text2 + "\n(" + "DefeatedLower".Translate() + ")";
            }
            if (faction.PlayerGoodwill < 0f)
            {
                GUI.color = Color.red;
            }
            else if (faction.PlayerGoodwill == 0f)
            {
                GUI.color = Color.yellow;
            }
            else
            {
                GUI.color = Color.green;
            }
            Widgets.Label(rect4, text2);
            GUI.color = Color.white;
            TooltipHandler.TipRegion(rect4, "CurrentGoodwill".Translate());
            Rect rect5 = new Rect(rect4.xMax, rowY, width, num);
            Widgets.Label(rect5, text);
            Text.Anchor = TextAnchor.UpperLeft;
            return num2;
        }

        public static Texture2D XenoFactionCrypticLabel(Faction faction)
        {
            switch (faction.def.defName)
            {
                case ("EldarWarhost"):
                    {
                        return ContentFinder<Texture2D>.Get("UI/Images/LabelEldar", true);
                    }
                case ("TauVanguard"):
                    {
                        return ContentFinder<Texture2D>.Get("UI/Images/LabelTau", true);
                    }
                case ("ChaosCult"):
                    {
                        return ContentFinder<Texture2D>.Get("UI/Images/LabelChaos", true);
                    }
                default:
                    {
                        return ContentFinder<Texture2D>.Get("UI/Images/LabelChaos", true);
                    }
            }
        }

    }
}
