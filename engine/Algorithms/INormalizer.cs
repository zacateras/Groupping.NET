using System.Collections.Generic;

namespace Groupping.NET.Algorithms
{
    public interface INormalizer<T>
    {
        IEnumerable<T> Normalize(IEnumerable<T> items);
    }
}