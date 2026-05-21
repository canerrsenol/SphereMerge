public interface IPool<T>
{
    int CountActive { get; }
    int CountInactive { get; }
    T Get();
    void Release(T item);
}
