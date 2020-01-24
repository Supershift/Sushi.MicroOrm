using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Sushi.MicroORM;
using Sushi.MicroORM.Mapping;
using Sushi.MicroORM.Supporting;
using Sushi.MicroORM.Tests.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

public static class CacheExtension
{
    public static void EnableCaching(this TableSetter table, bool applyToAllConnections = false) 
    {
        if (applyToAllConnections)
        {
            table.Map.OnBeforeFetch = Map_BeforeFetch;
            table.Map.OnPostFetch = Map_AfterFetch;
        }
        table.Map.OnPostSave = Map_AfterSave;
        table.Map.OnPostSave = Map_AfterSave;
    }


    public static void EnableCaching<T>(this Connector<T> connection) where T : new()
    {
        connection.Map.OnBeforeFetch = Map_BeforeFetch;
        connection.Map.OnPostFetch = Map_AfterFetch;
    }

    private static void Map_AfterSave(DataMap map)
    {
        var key = $"{map.GetType().Name}";

        Console.WriteLine("AFTERSAVE");

    }

    private static void Map_AfterFetch(QueryData data)
    {
        var key = $"{data.Map.GetType().Name} [{data.Query.UniqueIdentifier}]";

        using (var entry = Cache.CreateEntry(key))
        {
            entry.Value = data.Query.Result;
            entry.AbsoluteExpiration = DateTime.UtcNow.AddDays(1);
        }
    }


    private static void Map_BeforeFetch(QueryData data)
    {
        var key = $"{data.Map.GetType().Name} [{data.Query.UniqueIdentifier}]";

        object result;
        if (Cache.TryGetValue(key, out result))
        {
            Console.WriteLine("CACHE");

            //data.Query.Result = result;
        }
    }

    static IMemoryCache _Cache;
    public static IMemoryCache Cache
    {
        get
        {
            if (_Cache == null)
            {
                var provider = new ServiceCollection()
                               .AddMemoryCache()
                               .BuildServiceProvider();

                _Cache = provider.GetService<IMemoryCache>();
            }
            return _Cache;
        }
    }
}
