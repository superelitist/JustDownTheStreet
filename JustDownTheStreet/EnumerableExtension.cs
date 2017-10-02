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

    public static T PickRandom<T>( this IEnumerable<T> source )
    {
      return source.PickRandom( 1 ).Single();
    }

    public static IEnumerable<T> PickRandom<T>( this IEnumerable<T> source, int count )
    {
      return source.Shuffle().Take( count );
    }

    public static IEnumerable<T> Shuffle<T>( this IEnumerable<T> source )
    {
      return source.OrderBy( x => Rng );
    }
  }
}
