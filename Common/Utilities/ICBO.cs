using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Wongs.Common.Utilities
{
    public interface ICBO
    {
        List<TObject> FillCollection<TObject>(IDataReader dr) where TObject : new();

        TObject FillObject<TObject>(IDataReader dr) where TObject : new();

        //SortedList<TKey, TValue> FillSortedList<TKey, TValue>(string keyField, IDataReader dr);

        TObject GetCachedObject<TObject>(CacheItemArgs cacheItemArgs, CacheItemExpiredCallback cacheItemExpired, bool saveInDictionary);
    }
}
