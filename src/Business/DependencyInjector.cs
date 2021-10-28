using IsometricGame.Logic.ScriptHelpers;
using IsometricGame.Logic.ScriptHelpers.Abilities;
using IsometricGame.Logic.ScriptHelpers.Effects;
using IsometricGame.Logic.ScriptHelpers.Skills;
using IsometricGame.Repository;
using System.Collections.Generic;

namespace IsometricGame.Logic.Utils
{
    public static class DependencyInjector
    {
        static DependencyInjector()
        {
            gamesRepository = new GamesRepository();
            accountRepository = new AccountRepository();
            mapRepository = new MapRepository();
            teamsRepository = new TeamsRepository();
            pluginUtils = new PluginUtils();
            unitUtils = new UnitUtils(pluginUtils);
            gameLogic = new GameLogic(mapRepository, pluginUtils, unitUtils);
            serverLogic = new ServerLogic(gameLogic, accountRepository, gamesRepository);

            pluginUtils.Initialize(
                new List<IUnitType>
                {
                    new AmazonUnitType(),
                    new GoatmanUnitType(),
                    new WitchUnitType()
                },
                new List<IEffect>
                {
                    new BurnEffect(),
                    new HasteEffect()
                },
                new List<IAbility>
                {
                    new RangedAttackAbility(),
                    new MeleeAttackAbility(),
                    new HealAbility(),
                    new FireballAbility(pluginUtils),
                    new HasteAbility(pluginUtils),
                    new MoveAbility(),
                    new SkipTurnAbility()
                },
                new List<ISkill>
                {
                    new ArcherySkill(),
                    new BallisticsSkill(),
                    new EagleEyeSkill(),
                    new LogisticsSkill(),
                    new WisdomSkill(),
                    new FireMagicSkill(pluginUtils),
                    new AirMagicSkill(pluginUtils),
                    new OffenceSkill(),
                    new ArmourerSkill(),
                    new FirstAidSkill(pluginUtils),
                },
                new List<IBot>
                {
                    new EasyBot(gameLogic)
                });
        }

        public static PluginUtils pluginUtils { get; }
        public static UnitUtils unitUtils { get; }
        public static GamesRepository gamesRepository { get; }
        public static AccountRepository accountRepository { get; }
        public static MapRepository mapRepository { get; }
        public static GameLogic gameLogic { get; }
        public static ServerLogic serverLogic { get; }
        public static TeamsRepository teamsRepository { get; }
    }
}
