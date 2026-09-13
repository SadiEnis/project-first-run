using System;
using ProjectFirstRun.Rewards;
using UnityEngine;

namespace ProjectFirstRun.Chests.Spawning
{
    [CreateAssetMenu(fileName = "ChestDropProfile", menuName = "Project First Run/Chests/Enemy Drop Profile")]
    public sealed class EnemyChestDropProfile : ScriptableObject
    {
        [SerializeField, Range(0, 10000)] private int _chanceBasisPoints;
        [SerializeField] private ChestDropTable _dropTable;

        public int ChanceBasisPoints => _chanceBasisPoints;
        public ChestDropTable DropTable => _dropTable;

        public int Validate()
        {
            if (_chanceBasisPoints < 0 || _chanceBasisPoints > 10000)
                throw new InvalidOperationException("Chest drop chance must be between 0 and 10000 basis points.");
            if (_chanceBasisPoints == 0) return 0;
            if (_dropTable == null)
                throw new InvalidOperationException("An enabled chest drop profile requires a drop table.");
            return _dropTable.Validate();
        }

        public bool TryRoll(IRandomSource random, out ChestDefinition definition)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            definition = null;
            Validate();
            if (_chanceBasisPoints == 0) return false;
            if (_chanceBasisPoints < 10000 && Next(random, 10000) >= _chanceBasisPoints) return false;
            definition = _dropTable.Select(random);
            return true;
        }

        private static int Next(IRandomSource random, int maximum)
        {
            int value = random.Next(0, maximum);
            if (value < 0 || value >= maximum)
                throw new InvalidOperationException("Random source returned a value outside the requested range.");
            return value;
        }
    }
}
