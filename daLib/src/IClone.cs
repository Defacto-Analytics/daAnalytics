



namespace daLib
{
    public interface IDeepClone
    {
        object DeepClone();
    }
    public interface IDeepClone<T> : IDeepClone
    {
        new T DeepClone();
    }

    public interface IShallowClone
    {
        object ShallowClone();
    }
    public interface IShallowCloneable<T> : IShallowClone
    {
        new T ShallowClone();
    }
}
