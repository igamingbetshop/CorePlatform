using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.ComponentModel;
using Newtonsoft.Json;
using IqSoft.CP.Common.Attributes;
using IqSoft.CP.Common.Models.UserModels;

namespace IqSoft.CP.Common.Helpers
{
    public static class ExportExcelHelper
    {
        public static void AddObjectToLine<T>(List<T> reportData, List<UserMenuState> menuColumns, List<string> lines, bool moveRight, bool ignoreMoving = false)
        {
            if (reportData.Count == 0)
                return;
            IEnumerable<PropertyDescriptor> properties = TypeDescriptor.GetProperties(typeof(T)).OfType<PropertyDescriptor>();
            var headerProperties = properties.Where(x => !(x.PropertyType.IsGenericType &&
                         x.PropertyType.GetGenericTypeDefinition() == typeof(List<>))).ToList();
            var header = string.Join(",", properties.ToList().Where(x => !(x.PropertyType.IsGenericType &&
                                          x.PropertyType.GetGenericTypeDefinition() == typeof(List<>)))
                              .Where(x => !x.Attributes.OfType<NotExcelPropertyAttribute>().Any() &&
                                         (menuColumns == null || menuColumns.Any(y => y.ColumnId == x.Name && !y.Hide) ||
                                         (x.Attributes.OfType<JsonPropertyAttribute>().Any() &&
                                          menuColumns.Any(y => y.ColumnId == x.Attributes.OfType<JsonPropertyAttribute>().First().PropertyName && !y.Hide))
                                         ))
                              .Select(x => x.Attributes.OfType<JsonPropertyAttribute>().Any() ?
                              x.Attributes.OfType<JsonPropertyAttribute>().First().PropertyName : x.Name));
            if (!ignoreMoving && moveRight)
                header = header.Insert(0, " ,");
            if (!string.IsNullOrEmpty(header.Replace(",", string.Empty)))
                lines.Add(header);
            foreach (var item in reportData)
            {
                if (item == null)
                    continue;
                var valueLine = new StringBuilder();
                if (!ignoreMoving && moveRight)
                    valueLine.Append(",");
                foreach (var p in properties)
                {
                    if (!(p.PropertyType.IsGenericType &&
                         p.PropertyType.GetGenericTypeDefinition() == typeof(List<>)) && !p.Attributes.OfType<NotExcelPropertyAttribute>().Any() &&
                         (menuColumns == null || menuColumns.Any(y => y.ColumnId == p.Name && !y.Hide) ||
                         (p.Attributes.OfType<JsonPropertyAttribute>().Any() &&
                          menuColumns.Any(y => y.ColumnId == p.Attributes.OfType<JsonPropertyAttribute>().First().PropertyName && !y.Hide))
                         ))
                    {
                        var val = item.GetType().GetProperty(p.Name).GetValue(item, null);
                        if (val != null)
                            valueLine.Append(val.ToString().Replace(",", string.Empty) + ",");
                        else
                            valueLine.Append(",");
                    }
                }
                if (!string.IsNullOrEmpty(valueLine.ToString()) && !string.IsNullOrEmpty(valueLine.ToString().Replace(",", string.Empty)))
                    lines.Add(valueLine.ToString());
                valueLine.Clear();
                if (!ignoreMoving && moveRight)
                    valueLine.Append(",");
                foreach (var p in properties)
                {
                    if (p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) && !p.Attributes.OfType<NotExcelPropertyAttribute>().Any() &&
                        (menuColumns == null || menuColumns.Any(y => y.ColumnId == p.Name && !y.Hide) ||
                        (p.Attributes.OfType<JsonPropertyAttribute>().Any() &&
                         menuColumns.Any(y => y.ColumnId == p.Attributes.OfType<JsonPropertyAttribute>().First().PropertyName && !y.Hide))
                        ))
                    {
                        var val = item.GetType().GetProperty(p.Name).GetValue(item, null);
                        if (val == null)
                        {
                            valueLine.Append(null + ",");
                            continue;
                        }
                        var methodInfo = typeof(ExportExcelHelper).GetMethod(nameof(AddObjectToLine));
                        var generic = methodInfo.MakeGenericMethod(p.PropertyType.GetGenericArguments().Single());
                        generic.Invoke(null, new object[] { val, null, lines, true, true });
                    }
                }
                if (!string.IsNullOrEmpty(valueLine.ToString()) && !string.IsNullOrEmpty(valueLine.ToString().Replace(",", string.Empty)))
                    lines.Add(valueLine.ToString());
            }
        }

        public static List<string> SaveToCSV<T>(List<T> list, DateTime? fromDate, DateTime? endDate, DateTime currentDate, double timeZone,
                                         List<UserMenuState> menuColumns, List<string> lines)
        {
            if (lines == null)
                lines = new List<string>
            {
                "DATE:," + string.Format("{0:dd.MM.yyyy HH:mm:ss}", currentDate.GetGMTDateFromUTC(timeZone)),
                "FromDate:, " + string.Format("{0:dd.MM.yyyy HH:mm:ss}", fromDate.GetGMTDateFromUTC(timeZone)),
                "UntilDate:, " + string.Format("{0:dd.MM.yyyy HH:mm:ss}", endDate.GetGMTDateFromUTC(timeZone)),
                "TimeZone:, GMT +" + timeZone.ToString()
            };
            AddObjectToLine(list, menuColumns, lines, false, false);
            return lines;
        }
    }
}