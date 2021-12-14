using Lockstep.Math;

namespace Lockstep.Game
{
    public class EnemySystem : BaseSystem
    {
        private Spawner[] Spawners => _gameStateService.GetSpawners();
        private Enemy[] AllEnemy => _gameStateService.GetEnemies();

        public override void DoStart()
        {
            foreach (var configId in _gameConfigService.GetWorldConfig().spawnerIds)
            {
                var config = _gameConfigService.GetEntityConfig(configId) as SpawnerConfig;
                _gameStateService.CreateEntity<Spawner>(configId, config.entity.Info.spawnPoint);
            }

            foreach (var spawner in Spawners)
            {
                spawner.ServiceContainer = _serviceContainer;
                spawner.GameStateService = _gameStateService;
                spawner.DebugService = _debugService;
                spawner.DoStart();
            }
        }

        public override void DoUpdate(LFloat deltaTime)
        {
            foreach (var spawner in Spawners)
            {
                spawner.DoUpdate(deltaTime);
            }

            foreach (var enemy in AllEnemy)
            {
                enemy.DoUpdate(deltaTime);
            }
        }
    }
}