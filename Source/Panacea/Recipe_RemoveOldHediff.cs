using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace Panacea
{
    public class Recipe_RemoveOldHediff : Recipe_Surgery
    {
        private const float BaseSeverityReduction = 6f; // The base value for scar reduction
        private const float reductionVariance = 2f;     // Reduction is randomized by +- this value

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill)) return;
            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);

            // Calculate how much damage we reduced
            float amountReduced = Mathf.Max(0, BaseSeverityReduction + Rand.Range(-reductionVariance, reductionVariance)) * billDoer.GetStatValue(StatDefOf.MedicalTendQuality);

            // Find all scars
            var partsToApplyTo = GetPartsToApplyOn(pawn, bill.recipe).ToArray();
            
            List<Hediff> scars = pawn.health.hediffSet.hediffs.FindAll(h => h.IsPermanent() && partsToApplyTo.Contains(h.Part));
            foreach (Hediff scar in scars)
            {
                // Reduce scars until we run out of reduction
                if (scar.Severity <= amountReduced)
                {
                    amountReduced -= scar.Severity;
                    pawn.health.RemoveHediff(scar);
                    if (amountReduced <= 0) break;
                }
                else
                {
                    scar.Severity -= amountReduced;
                    break;
                }
            }
        }

        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            if (pawn.RaceProps.IsFlesh && pawn.health.hediffSet.hediffs.Any(h => h.IsPermanent() && h.Part.depth == BodyPartDepth.Outside && !h.Part.def.delicate && !pawn.health.hediffSet.PartIsMissing(h.Part)))
            {
                yield return pawn.RaceProps.body.corePart;
            }
        }
    }
}
