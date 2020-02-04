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

public static class ExtendOnEntension
{
    public static DataMapItemSetter ExtendOn(this DataMapItemSetter map, string key, bool isNullable = false)
    {
        //  Sign up to event to alter the select query.
        map.DataItem.Sender.OnApplyFilter = Sender_SelectQueryCreation;

        //  Remove "myself" from the query creation process.
        map.DataItem.Sender.DatabaseColumns.Remove(map.DataItem);
        //  Extract the other Nested ORM entity type.
        var property = map.DataItem.MemberInfoTree;
        var nestedType = ReflectionHelper.GetMemberType(property);
        if (nestedType.IsGenericType)
            nestedType = nestedType.GetGenericArguments().Single();
        //  Get the ORM map from the entity type.
        var nestedMap = DatabaseConfiguration.DataMapProvider.GetMapForType(nestedType);

        List<JoinedMap> nestedMaps = map.DataItem.Sender["nested_maps"] as List<JoinedMap>;
        if (nestedMaps == null)
            nestedMaps = new List<JoinedMap>();

        JoinedMap join = new JoinedMap();
        join.Map = nestedMap;
        join.Primary_Key = map.DataItem.Column;
        join.Foreign_Key = key;
        join.Info = property;

        if (isNullable)
            join.JoinType = " left ";

        nestedMaps.Add(join);
        map.DataItem.Sender["nested_maps"] = nestedMaps;
        return map;
    }

    public class JoinedMap
    { 
        public DataMap Map { get; set; }
        public string Primary_Key { get; set; }
        public string Foreign_Key { get; set; }
        public string JoinType { get; set; }
        public List<MemberInfo> Info { get; set; }
    }


    private static void Sender_SelectQueryCreation(QueryData data)
    {
        var nestedMaps = data.Map["nested_maps"] as List<JoinedMap>;
        if (nestedMaps != null)
        {
            foreach(var join in nestedMaps)
            {
                foreach (var col in join.Map.DatabaseColumns)
                {
                    //  Important to identify the property!
                    col.OnReflection = Col_Reflection1;
                    col.Instance = join.Info;
                    data.Query.Select.Add(col);
                }

                data.Query.From = ($" {data.Query.From} {join.JoinType} join {join.Map.TableName} on {join.Primary_Key} = {join.Foreign_Key} ").Trim();
            }
        }
    }

    private static void Col_Reflection1(QueryDataOutput data)
    {
        var nestedinstance = ReflectionHelper.GetMemberValue(data.DatabaseColumn.Instance,data.Instance);
        if (nestedinstance == null)
        {

            //// For instance within a IList
            //if (nestedinstance.GetType().IsGenericType)
            //{
            //    var baseinstance = System.Activator.CreateInstance(param.Info.PropertyType.GetGenericArguments().Single());
            //    param.Info.PropertyType.GetMethod("Add").Invoke(nestedinstance, new[] { baseinstance });
            //    ReflectionHelper.SetPropertyValue(param.Info, nestedinstance, instance);
            //    SetResultValuesToObject(param.Map, reader, baseinstance);
            //    continue;
            //}

            var memberType = ReflectionHelper.GetMemberType(data.DatabaseColumn.Instance);
            nestedinstance = System.Activator.CreateInstance(memberType);
            ReflectionHelper.SetMemberValue(data.DatabaseColumn.Instance, nestedinstance, data.Instance);
        }
        ReflectionHelper.SetMemberValue(data.DatabaseColumn.MemberInfoTree, data.Value, nestedinstance);
    }
}