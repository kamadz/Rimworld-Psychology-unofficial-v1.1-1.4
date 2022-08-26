﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Reflection;
//using UnityEngine;
//using Verse;
//using RimWorld;
//using HarmonyLib;

//namespace Psychology.Harmony
//{
//    [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
//    public static class Game_LoadGame_Patch
//    {
//        public static void LoadGame()
//        {
//            if (!ModIsActive || !PsychologySettings.enableKinsey)
//            {
//                return;
//            }
//            foreach (Pawn pawn in )
//            {
//                if (pawn.story == null || !PsycheHelper.PsychologyEnabled(pawn))
//                {
//                    continue;
//                }
//                if (pawn.story.traits.HasTrait(TraitDefOf.Gay))
//                {
//                    RemoveTrait(pawn, TraitDefOf.Gay);
//                    PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality(0f, 0f, 0f, 0f, 0f, 1f, 2f);
//                }
//                if (pawn.story.traits.HasTrait(TraitDefOf.Bisexual))
//                {
//                    RemoveTrait(pawn, TraitDefOf.Bisexual);
//                    PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality(0f, 0f, 1f, 2f, 1f, 0f, 0f);
//                }
//                if (pawn.story.traits.HasTrait(TraitDefOf.Asexual))
//                {
//                    RemoveTrait(pawn, TraitDefOf.Asexual);
//                    PsycheHelper.Comp(pawn).Sexuality.sexDrive = 0.10f * Rand.ValueSeeded(11 * pawn.HashOffset() + 8);
//                }
//            }

//        }
//    }
//}

