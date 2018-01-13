namespace Groupping.NET.Algorithms
{
    public interface IDistance<T>
    {
        double Measure(T item1, T item2);
    }
}