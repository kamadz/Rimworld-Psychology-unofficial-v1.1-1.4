﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
namespace Psychology;

public class Pawn_SexualityTracker : IExposable
{
  public int kinseyRating;
  public float sexDrive;
  public float romanticDrive;
  public List<Pawn> knownSexualitiesWorkingKeys;
  public List<int> knownSexualitiesWorkingValues;
  public Dictionary<Pawn, int> knownSexualities = new Dictionary<Pawn, int>();
  public Pawn pawn;
  public static readonly float[] onesArray = new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f };
  public const float asexualCutoff = 0.1f;
  public static readonly int[] bisexualRatings = new int[] { 2, 3, 4 };
  public static readonly int[] homosexualRatings = new int[] { 5, 6 };
  public bool IsAsexual => sexDrive < asexualCutoff;

  public float AdjustedRomanticDrive => AdjustedDrive(true);
  public float AdjustedSexDrive => AdjustedDrive(false);

  public static readonly SimpleCurve FemaleSexDriveCurve = new SimpleCurve
    {
        new CurvePoint(13f, 0f),
        new CurvePoint(15f, 1f),
        new CurvePoint(35f, 1.6f),
        new CurvePoint(50f, 1f),
        new CurvePoint(80f, 0.6f),
    };

  public static readonly SimpleCurve MaleSexDriveCurve = new SimpleCurve
    {
        new CurvePoint(13f, 0f),
        new CurvePoint(15f, 1f),
        new CurvePoint(20f, 1.6f),
        new CurvePoint(50f, 1f),
        new CurvePoint(80f, 0.6f),
    };

  public static readonly SimpleCurve RomanticDriveCurve = new SimpleCurve
    {
        new CurvePoint(10f, 0f),
        new CurvePoint(15f, 1.3f),
        new CurvePoint(25f, 1.6f),
        new CurvePoint(35f, 1.3f),
        new CurvePoint(50f, 1f),
        new CurvePoint(80f, 0.8f),
    };

  public Pawn_SexualityTracker(Pawn p)
  {
    this.pawn = p;
  }

  private float AdjustedDrive(bool isDating)
  {
    // Pawns can still have drive even if they are underage from the user species settings. Hence no check on species settings.
    if (!SpeciesHelper.RomanceLifestageAgeCheck(pawn, true))
    {
      //Log.Message("AdjustedDrive, !RomanceLifestageAgeCheck");
      return 0f;
    }
    float age = pawn.ageTracker.AgeBiologicalYearsFloat;
    SpeciesSettings settings = PsychologySettings.speciesDict[pawn.def.defName];
    float minSettingsAge = isDating ? settings.minDatingAge : settings.minLovinAge;
    float drive = isDating ? romanticDrive : sexDrive;
    if (!settings.enablePsyche || minSettingsAge < 0f)
    {
      //Log.Message("AdjustedDrive, !settings.enablePsyche || minLovinAge < 0f, pawn: " + pawn + ", minSettingsAge: " + minSettingsAge);
      return 0f;
    }
    if (!settings.enableAgeGap)
    {
      //Log.Message("AdjustedDrive, !RomanceLifestageAgeCheck, pawn: " + pawn + ", minSettingsAge: " + minSettingsAge);
      // settings.minDatingAge here is intentional for both romantic and sex drive.
      return age > settings.minDatingAge ? drive : 0f;
    }
    if (minSettingsAge == 0f)
    {
      Log.Message("AdjustedDrive, minLovinAge == 0f, pawn: " + pawn + ", minSettingsAge: " + minSettingsAge);
      return drive;
    }
    float scaledAge = isDating ? PsycheHelper.DatingBioAgeToVanilla(age, minSettingsAge) : PsycheHelper.LovinBioAgeToVanilla(age, minSettingsAge);
    float ageFactor = 1f;
    if (isDating)
    {
      ageFactor = RomanticDriveCurve.Evaluate(scaledAge);
    }
    else
    {
      switch (pawn.gender)
      {
        case Gender.Female:
          ageFactor = FemaleSexDriveCurve.Evaluate(scaledAge);
          break;
        case Gender.Male:
          ageFactor = MaleSexDriveCurve.Evaluate(scaledAge);
          break;
        default:
          // Maybe one day other genders will come to the Rim...
          ageFactor = 0.5f * (FemaleSexDriveCurve.Evaluate(scaledAge) + MaleSexDriveCurve.Evaluate(scaledAge));
          break;
      }
    }
    if (ageFactor == 0f)
    {
      Log.Message("AdjustedDrive, ageFactor == 0f, pawn: " + pawn + ", minSettingsAge: " + minSettingsAge);
    }
    return ageFactor * drive;

  }

  public virtual bool IncompatibleSexualityKnown(Pawn recipient)
  {
    if (this.knownSexualities.ContainsKey(recipient))
    {
      return ((knownSexualities[recipient] - 4) >= 0) != (recipient.gender == this.pawn.gender);
    }
    return false;
  }

  public virtual void LearnSexuality(Pawn p)
  {
    if (p != null && PsycheHelper.PsychologyEnabled(pawn) && !knownSexualities.Keys.Contains(p))
    {
      knownSexualities.Add(p, PsycheHelper.Comp(p).Sexuality.kinseyRating);
    }
  }

  public virtual void ExposeData()
  {
    Scribe_Values.Look(ref this.kinseyRating, "kinseyRating", 0, false);
    Scribe_Values.Look(ref this.sexDrive, "sexDrive", 1, false);
    Scribe_Values.Look(ref this.romanticDrive, "romanticDrive", 1, false);
    Scribe_Collections.Look(ref this.knownSexualities, "knownSexualities", LookMode.Reference, LookMode.Value, ref this.knownSexualitiesWorkingKeys, ref this.knownSexualitiesWorkingValues);
  }

  public virtual void GenerateSexuality(int inputSeed = 0)
  {
    GenerateSexuality(onesArray, inputSeed);
  }

  public virtual void GenerateSexuality(float[] overrideKinseyArray, int inputSeed = 0)
  {
    //kinseyRating = RandKinsey(b0, b1, b2, b3, b4, b5, b6, inputSeed);
    //sexDrive = GenerateSexDrive(inputSeed);
    //romanticDrive = GenerateRomanticDrive(inputSeed);
    GenerateKinsey(overrideKinseyArray, inputSeed);
    GenerateSexDrive(inputSeed);
    GenerateRomanticDrive(inputSeed);
  }

  public virtual void GenerateKinsey(float[] overrideKinseyArray, int inputSeed = 0)
  {
    kinseyRating = RandKinsey(overrideKinseyArray, inputSeed);
  }

  public virtual void GenerateSexDrive(int inputSeed = 0)
  {
    sexDrive = RandSexDrive(inputSeed);
  }

  public virtual void GenerateRomanticDrive(int inputSeed = 0)
  {
    romanticDrive = RandRomanticDrive(inputSeed);
  }

  /*
   * Average roll: 0.989779
   * Percent chance of rolling each number:
   * 0: 62.4949 %
   * 1: 11.3289 %
   * 2: 9.2658 %
   * 3: 6.8466 %
   * 4: 4.522 %
   * 5: 2.7806 %
   * 6: 2.7612 %
   * Percent chance of being predominantly straight:  83.0896 %
   * Percent chance of being predominantly gay:       10.0638 %
   * Percent chance of being more or less straight:   73.8238 %
   * Percent chance of being more or less bisexual:   20.6344 %
   * Percent chance of being more or less gay:         5.5418 %
   */

  public virtual int RandKinsey(float[] oList, int inputSeed = 0)
  {
    float[] wArray = new float[7];
    if (PsychologySettings.kinseyFormula != KinseyMode.Custom)
    {
      wArray = PsycheHelper.KinseyModeWeightDict[PsychologySettings.kinseyFormula];
    }
    else
    {
      wArray = PsychologySettings.kinseyWeightCustom.ToArray();
      if (wArray.Sum() == 0f)
      {
        wArray = onesArray;
      }
    }
    float wCumSum = 0f;
    float woCumSum = 0f;
    float[] wCumSumArray = new float[7];
    float[] woCumSumArray = new float[7];
    for (int i = 0; i < 7; i++)
    {
      wCumSum += wArray[i];
      wCumSumArray[i] = wCumSum;
      woCumSum += wArray[i] * oList[i];
      woCumSumArray[i] = woCumSum;
    }
    if (woCumSumArray[6] > 0f)
    {
      return RandKinseyByWeight(woCumSumArray, inputSeed);
    }
    return RandKinseyByWeight(wCumSumArray, inputSeed);
  }

  public virtual int RandKinseyByWeight(float[] wCumSumArray, int inputSeed = 0)
  {
    float randValue = Rand.ValueSeeded(17 * PsycheHelper.PawnSeed(this.pawn) + 11 * inputSeed + 31) * wCumSumArray[6];
    for (int s = 0; s < 6; s++)
    {
      if (randValue <= wCumSumArray[s])
      {
        return s;
      }
    }
    return 6;
  }

  public virtual float RandSexDrive(int inputSeed = 0)
  {
    float drive = -1f;
    int kill = 0;
    int pawnSeed = PsycheHelper.PawnSeed(this.pawn);
    int seed1 = 11 * pawnSeed + 2 * inputSeed + 131;
    int seed2 = 13 * pawnSeed + 7 * inputSeed + 89;
    while ((drive < 0f || 1f < drive) && kill < 500)
    {
      //drive = Rand.Gaussian(1.1f, 0.26f);
      drive = PsycheHelper.RandGaussianSeeded(seed1, seed2, 1.1f, 0.26f);
      seed1 += 43;
      seed2 += 67;
      kill++;
    }
    return Mathf.Clamp01(drive);
  }

  public virtual float RandRomanticDrive(int inputSeed = 0)
  {
    return RandSexDrive(859456 + 3 * inputSeed);
  }

  public void AsexualTraitReroll()
  {
    if (this.sexDrive < asexualCutoff)
    {
      return;
    }
    this.sexDrive = asexualCutoff * Rand.ValueSeeded(5 * PsycheHelper.PawnSeed(this.pawn) + 8);
  }

  public void BisexualTraitReroll()
  {
    if (bisexualRatings.Contains(this.kinseyRating))
    {
      return;
    }
    GenerateKinsey(new float[] { 0f, 0f, 1f, 3f, 1f, 0f, 0f });
    PsycheHelper.Comp(this.pawn).Psyche.CalculateAdjustedRatings();
  }

  public void GayTraitReroll()
  {
    if (homosexualRatings.Contains(this.kinseyRating))
    {
      return;
    }
    GenerateKinsey(new float[] { 0f, 0f, 0f, 0f, 0f, 1f, 3f });
    PsycheHelper.Comp(pawn).Psyche.CalculateAdjustedRatings();
  }

  public void DeepCopyFromOtherTracker(Pawn_SexualityTracker otherTracker)
  {
    //Log.Message("Pawn_SexualityTracker.DeepCopyFromOtherTracker step 0");
    this.kinseyRating = otherTracker.kinseyRating;
    //Log.Message("Pawn_SexualityTracker.DeepCopyFromOtherTracker step 1, otherTracker.kinseyRating = " + otherTracker.kinseyRating);
    this.sexDrive = otherTracker.sexDrive;
    //Log.Message("Pawn_SexualityTracker.DeepCopyFromOtherTracker step 2, otherTracker.sexDrive = " + otherTracker.sexDrive);
    this.romanticDrive = otherTracker.romanticDrive;
    //Log.Message("Pawn_SexualityTracker.DeepCopyFromOtherTracker step 3, otherTracker.romanticDrive " + otherTracker.romanticDrive);
    if (otherTracker.knownSexualitiesWorkingKeys != null)
    {
      this.knownSexualitiesWorkingKeys = new List<Pawn>();
      foreach (Pawn p in otherTracker.knownSexualitiesWorkingKeys)
      {
        this.knownSexualitiesWorkingKeys.Add(p);
      }
    }
    //Log.Message("Pawn_SexualityTracker.DeepCopyFromOtherTracker step 4");
    if (otherTracker.knownSexualitiesWorkingValues != null)
    {
      this.knownSexualitiesWorkingValues = new List<int>();
      foreach (int i in otherTracker.knownSexualitiesWorkingValues)
      {
        this.knownSexualitiesWorkingValues.Add(i);
      }
    }
    //Log.Message("Pawn_SexualityTracker.DeepCopyFromOtherTracker step 5");
    this.knownSexualities = new Dictionary<Pawn, int>();
    foreach (KeyValuePair<Pawn, int> kvp in otherTracker.knownSexualities)
    {
      this.knownSexualities.Add(kvp.Key, kvp.Value);
    }
  }
}


