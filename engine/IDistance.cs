namespace Groupping.NET
{
    public interface IDistance<T>
    {
        double Measure(T t1, T t2);
    }
}