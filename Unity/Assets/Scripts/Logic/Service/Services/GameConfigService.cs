using NetMsg.Common;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Lockstep.Game
{
    public class GameConfigService : BaseGameService, IGameConfigService
    {
        private GameConfig _config;
        public string configPath = "GameConfig";
        private Dictionary<Type, int> entityConfigIdStarts = new Dictionary<Type, int>
        {
            { typeof(Player), 0 }
        };

        public override void DoAwake(IServiceContainer container)
        {
            _config = Resources.Load<GameConfig>(configPath);
            _config.DoAwake();
        }

        public EntityConfig GetEntityConfig(int id)
        {
            if (id >= 1000)
                return _config.GetPlayer2DConfig(id - 1000);
            if (id >= 100)
                return _config.GetSpawnerConfig(id - 100);
            if (id >= 10)
                return _config.GetEnemyConfig(id - 10);
            return _config.GetPlayerConfig(id);
        }

        public AnimatorConfig GetAnimatorConfig(int id)
        {
            return _config.GetAnimatorConfig(id - 1);
        }

        public SkillBoxConfig GetSkillConfig(int id)
        {
            return _config.GetSkillConfig(id - 1);
        }

        public CollisionConfig CollisionConfig => _config.CollisionConfig;
        public string RecorderFilePath => _config.RecorderFilePath;
        public string DumpStrPath => _config.DumpStrPath;
        public Msg_G2C_GameStartInfo ClientModeInfo => _config.ClientModeInfo;

        public WorldConfig GetWorldConfig()
        {
            return _config.worldConfig;
        }
    }
}