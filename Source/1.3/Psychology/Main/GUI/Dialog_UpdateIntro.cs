﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using UnityEngine;
//using RimWorld;
using Verse;
            return new Vector2(Dialog_UpdateIntro_SCOS.windowRectWidth, Dialog_UpdateIntro_SCOS.windowRectHeight);
        }
        doCloseButton = false;

        GameFont oldFont = Text.Font;
        Text.Font = GameFont.Medium;
        Widgets.Label(Dialog_UpdateIntro_SCOS.IntroTitleRect, Dialog_UpdateIntro_SCOS.IntroTitleText);

        Text.Font = GameFont.Small;
        if (Widgets.ButtonText(Dialog_UpdateIntro_SCOS.UpdateButtonRect, Dialog_UpdateIntro_SCOS.UpdateButtonText))
        {
            this.Close();
            Find.WindowStack.Add(new Dialog_UpdateYesNo());
        }
        {
            this.Close();
        }
    }
{
    public static float windowRectWidth = 500f;
    public static float windowRectHeight;

    public static string IntroTitleText = "UpdateIntroMessageTitle".Translate();
    public static Rect IntroTitleRect;

    public static string IntroDescText = "UpdateIntroMessage".Translate();
    public static Rect IntroDescRect;

    public static string UpdateButtonText = "ApplyUpdateButton".Translate();
    public static Rect UpdateButtonRect;

    public static string CloseButtonText = "CloseButton".Translate();
    public static Rect CloseButtonRect;

    static Dialog_UpdateIntro_SCOS()
    {
        GameFont oldFont = Text.Font;
        Text.Font = GameFont.Medium;

        float introHeight = Text.CalcSize(IntroTitleText).y;
        IntroTitleRect = new Rect(0f, 0f, inRectWidth, 40f);
        Text.Font = GameFont.Small;

        float introDescHeight = Text.CalcHeight(IntroDescText, inRectWidth) + 10f;
        //float introDescHeight = 140f;
        IntroDescRect = new Rect(0f, IntroTitleRect.yMax, inRectWidth, introDescHeight);

        float buttonWidth = Mathf.Max(Text.CalcSize(UpdateButtonText).x + 20f, Text.CalcSize(CloseButtonText).x + 20f, Window.CloseButSize.x);
        float buttonY = IntroDescRect.yMax + Window.StandardMargin;
        UpdateButtonRect = new Rect(-Window.StandardMargin + windowRectWidth / 3f - 0.5f * buttonWidth, buttonY, buttonWidth, Window.CloseButSize.y);
        CloseButtonRect = new Rect(-Window.StandardMargin + 2f * windowRectWidth / 3f - 0.5f * buttonWidth, buttonY, buttonWidth, Window.CloseButSize.y);

        inRectHeight = UpdateButtonRect.yMax - IntroTitleRect.y;
        windowRectHeight = inRectHeight + 2f * Window.StandardMargin;

        Text.Font = oldFont;
    }
}