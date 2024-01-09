using Patchwork.Attributes;

namespace V1ldDetectHiddenObjectsWithoutStealth
{
    [ModifiesType]
    class V1ld_CharacterStatsDHOWS: CharacterStats
    {
		[ModifiesMember("DetectionRange")]
        new public float DetectionRange(Detectable d)
        {
            int detectionRange = CalculateSkill(SkillType.Mechanics);
            //if (!Stealth.IsInStealthMode(base.gameObject))
            //{
            //    detectionRange -= 4;
            //}
            detectionRange += 1;
            if (d != null)
            {
                detectionRange -= d.GetDifficulty();
            }
            return detectionRange;
        }
    }
}