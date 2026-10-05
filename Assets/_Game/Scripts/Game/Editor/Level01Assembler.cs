using ClaudeCop.Game.Editor;
using UnityEditor;

namespace ClaudeCop.Game.Editor
{
    /// <summary>
    /// T-403: ghep Scenes/Gameplay/Level_01.unity tu Level_01.prefab (3 Phase x 6 Shot). Loi dung chung: <see cref="LevelAssembler"/>.
    /// </summary>
    public static class Level01Assembler
    {
        const string ScenePath = "Assets/_Game/Scenes/Gameplay/Level_01.unity";

        // Do kho: P1 Calm/Standard -> P2 Standard -> P3 Intense. Pickup: P1/P2 W2 Shotgun, P2 W5 MG, P3 luan phien.
        static readonly LevelAssembler.WaveDef[] Waves =
        {
            new LevelAssembler.WaveDef{phase=1,shot=2,preset="Calm",     pickup="Shotgun"},
            new LevelAssembler.WaveDef{phase=1,shot=3,preset="Calm",     pickup=null, blend=1.0f, fov=58f},
            new LevelAssembler.WaveDef{phase=1,shot=5,preset="Standard", pickup=null},
            new LevelAssembler.WaveDef{phase=1,shot=6,preset="Standard", pickup=null, blend=0.7f, fov=58f},
            new LevelAssembler.WaveDef{phase=2,shot=2,preset="Standard", pickup="Shotgun"},
            new LevelAssembler.WaveDef{phase=2,shot=3,preset="Standard", pickup=null, blend=0.7f, fov=58f},
            new LevelAssembler.WaveDef{phase=2,shot=5,preset="Standard", pickup="MachineGun"},
            new LevelAssembler.WaveDef{phase=2,shot=6,preset="Standard", pickup=null, blend=0.7f, fov=58f},
            new LevelAssembler.WaveDef{phase=3,shot=2,preset="Standard", pickup="Shotgun"},
            new LevelAssembler.WaveDef{phase=3,shot=3,preset="Intense",  pickup="MachineGun", blend=1.0f, fov=58f},
            new LevelAssembler.WaveDef{phase=3,shot=5,preset="Intense",  pickup="Shotgun"},
            new LevelAssembler.WaveDef{phase=3,shot=6,preset="Intense",  pickup="MachineGun", blend=1.0f, fov=58f},
        };

        static readonly string[] Titles = { "STAGE 1-1", "STAGE 1-2", "STAGE 1-3" };

        [MenuItem("ClaudeCop/Game/Assemble Level_01 (T-403)")]
        public static void Assemble()
        {
            LevelAssembler.Run(new LevelAssembler.LevelSpec
            {
                scenePath = ScenePath, rootName = "Level_01", logName = "Level01Assembler",
                waves = Waves, titles = Titles, moveShots = new[] { 1, 4 },
            });
        }
    }
}
