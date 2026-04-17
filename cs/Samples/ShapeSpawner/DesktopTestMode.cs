using Microsoft.MixedReality.Volumetric;

namespace CsShapeSpawner;

internal static class DesktopTestMode
{
    public static void Start(VolumetricApp app, Func<Volume?> getVolume, Func<ShapeManager?> getShapeManager)
    {
        Console.WriteLine("=== Desktop Test Mode ===");
        Console.WriteLine("  Space  = Spawn random shape");
        Console.WriteLine("  1-9    = Poke (destroy) shape #N");
        Console.WriteLine("  D      = Dump active shapes");
        Console.WriteLine("  Q      = Quit");
        Console.WriteLine("=========================");

        Task.Run(() =>
        {
            while (!app.IsStopped)
            {
                var key = Console.ReadKey(true);
                var volume = getVolume();
                var shapeManager = getShapeManager();

                if (volume == null || shapeManager == null) continue;

                volume.DispatchToNextUpdate(() =>
                {
                    switch (key.Key)
                    {
                        case ConsoleKey.Spacebar:
                            shapeManager.SpawnAtRandom();
                            break;
                        case ConsoleKey.D:
                            shapeManager.DumpState();
                            break;
                        case ConsoleKey.Q:
                            app.RequestExit();
                            break;
                        default:
                            if (key.KeyChar >= '1' && key.KeyChar <= '9')
                                shapeManager.PokeShape(key.KeyChar - '1');
                            break;
                    }
                });
            }
        });
    }
}
