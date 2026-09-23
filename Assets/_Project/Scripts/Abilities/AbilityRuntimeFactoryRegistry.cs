using System;
using System.Collections.Generic;

namespace ProjectFirstRun.Abilities
{
    public sealed class AbilityRuntimeFactoryRegistry
    {
        private readonly List<IAbilityRuntimeFactory>
            _factories =
                new List<IAbilityRuntimeFactory>();

        public int FactoryCount =>
            _factories.Count;

        public void BindEnemyRegistry(ProjectFirstRun.Enemies.EnemyRegistry registry)
        {
            foreach (var factory in _factories)
                if (factory is IMapEnemyRegistryBinding binding) binding.BindEnemyRegistry(registry);
        }

        public void Register(
            IAbilityRuntimeFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(
                    nameof(factory));
            }

            if (_factories.Contains(factory))
            {
                throw new InvalidOperationException(
                    "The same ability runtime factory " +
                    "has already been registered.");
            }

            _factories.Add(
                factory);
        }

        public IAbilityRuntimeFactory Resolve(
            AbilityDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            IAbilityRuntimeFactory matchedFactory =
                null;

            for (int index = 0;
                 index < _factories.Count;
                 index++)
            {
                IAbilityRuntimeFactory factory =
                    _factories[index];

                if (!factory.Supports(
                        definition))
                {
                    continue;
                }

                if (matchedFactory != null)
                {
                    throw new InvalidOperationException(
                        $"Multiple ability runtime factories " +
                        $"support definition '{definition.name}'.");
                }

                matchedFactory =
                    factory;
            }

            if (matchedFactory == null)
            {
                throw new InvalidOperationException(
                    $"No ability runtime factory supports " +
                    $"definition '{definition.name}'.");
            }

            return matchedFactory;
        }
    }
}
