using UnityEngine;

namespace ProjectFirstRun.Progression
{
    [CreateAssetMenu(
        fileName = "XP_NewExperience",
        menuName = "Project First Run/Progression/Experience Definition")]
    public sealed class ExperienceDefinition : ScriptableObject
    {
        [SerializeField, Min(1)]
        private int _firstLevelCost = 100;

        [SerializeField, Min(1)]
        private int _costIncreasePerLevel = 50;

        public ExperienceCurve CreateRuntimeCurve()
        {
            return new ExperienceCurve(_firstLevelCost, _costIncreasePerLevel);
        }
    }
}
