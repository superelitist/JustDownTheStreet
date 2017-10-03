using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustDownTheStreet
{
  public static class EnumerableExtension
  {
    private static readonly Random Rng = new Random();

    public static T PickRandom<T>( this IEnumerable<T> source ) {
      return source.PickRandom( 1 ).Single();
    }

    public static IEnumerable<T> PickRandom<T>( this IEnumerable<T> source, int count ) {
      return source.Shuffle().Take( count );
    }

    public static IEnumerable<T> Shuffle<T>( this IEnumerable<T> source ) {
      //return source.OrderBy( x => Rng );
      if (source == null) throw new ArgumentNullException("source");
      if (Rng == null) throw new ArgumentNullException("rng");
      return source.ShuffleIterator(Rng);
    }

    private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, Random rng) {
      var buffer = source.ToList();
      for (int i = 0; i < buffer.Count; i++) {
        int j = rng.Next(i, buffer.Count);
        yield return buffer[j];
        buffer[j] = buffer[i];
      }
    }
  }
}
