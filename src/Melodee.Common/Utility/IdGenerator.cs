namespace Melodee.Common.Utility;

public static class IdGenerator
{
    private static readonly Lazy<IdGen.IdGenerator> Lazy = new Lazy<IdGen.IdGenerator>(new IdGen.IdGenerator(0));
    
    public static long CreateId => Lazy.Value.CreateId();
}
