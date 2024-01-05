using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters
{
    public abstract class FilterBase<T> where T : IBase
    {
        public int SkipCount { get; set; }

        public int TakeCount { get; set; }

        public List<CheckPermissionOutput<T>> CheckPermissionResuts { get; set; }

        protected IQueryable<T> FilteredObjects(IQueryable<T> objects, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null)
        {
            if (CheckPermissionResuts != null)
            {
                objects =
                    CheckPermissionResuts.Where(
                        checkPermissionResut =>
                            checkPermissionResut != null && !checkPermissionResut.HaveAccessForAllObjects)
                        .Aggregate(objects,
                            (current, checkPermissionResut) => current.Where(checkPermissionResut.Filter));
            }

            if (orderBy != null)
            {
                objects = orderBy(objects);
                if (TakeCount != 0)
                {
                    TakeCount = Math.Min(TakeCount, 100000);
                    objects = objects.Skip(SkipCount * TakeCount).Take(TakeCount);
                }
            }
            return objects;
        }

        protected abstract IQueryable<T> CreateQuery(IQueryable<T> objects, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null);

        public bool? OrderBy { get; set; }

        public string FieldNameToOrderBy { get; set; }

        public void FilterByValue<T1>(ref IQueryable<T1> objects, FiltersOperation filtersOperation, string fieldName, string additionalFieldName = null)
        {
            if (filtersOperation != null && filtersOperation.OperationTypeList != null && filtersOperation.OperationTypeList.Any())
            {
                objects = objects.Where(BuildExpression<T1>(filtersOperation, fieldName, additionalFieldName));
            }
        }

        private Expression<Func<T1, bool>> BuildExpression<T1>(FiltersOperation filtersOperation, string fieldName, string additionalFieldName)
        {
            var type = typeof(T1).GetProperty(fieldName).PropertyType;
            var typeCode = Type.GetTypeCode(type);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                typeCode = Type.GetTypeCode(type.GetGenericArguments()[0]);

            var parameter = Expression.Parameter(typeof(T1), "x");
            var member = Expression.Property(parameter, fieldName);
            Expression leftExpression = member;
            if (additionalFieldName != null)
            {
                var additionalMember = Expression.Property(parameter, additionalFieldName);
                var additionalMethod = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
                leftExpression = Expression.Add(member, Expression.Constant(" "), additionalMethod);
                leftExpression = Expression.Add(leftExpression, additionalMember, additionalMethod);
            }

            Expression finalExpression = Expression.Constant(filtersOperation.IsAnd);
            Expression expression = null;
            MethodInfo method = null;
            ConstantExpression valueExpression = null;
            foreach (var item in filtersOperation.OperationTypeList)
            {
                switch (typeCode)
                {
                    case TypeCode.Int64:
                        if (item.OperationTypeId != (int)FilterOperations.InSet && item.OperationTypeId != (int)FilterOperations.OutOfSet)
                            valueExpression = Expression.Constant(item.IntValue);
                        else
                            valueExpression = Expression.Constant(item.StringValue.Split(',').Select(Int64.Parse).ToList());
                        break;
                    case TypeCode.Int32:
                        if (item.OperationTypeId != (int)FilterOperations.InSet && item.OperationTypeId != (int)FilterOperations.OutOfSet)
                            valueExpression = Expression.Constant(item.IntValue);
                        else
                            valueExpression = Expression.Constant(item.StringValue.Split(',').Select(Int32.Parse).ToList());
                        break;
                    case TypeCode.Decimal:
                        if (item.OperationTypeId != (int)FilterOperations.InSet && item.OperationTypeId != (int)FilterOperations.OutOfSet)
                            valueExpression = Expression.Constant(item.DecimalValue);
                        else
                            valueExpression = Expression.Constant(item.StringValue.Split(',').Select(Decimal.Parse).ToList());
                        break;
                    case TypeCode.String:
                        if (item.OperationTypeId != (int)FilterOperations.InSet && item.OperationTypeId != (int)FilterOperations.OutOfSet)
                            valueExpression = Expression.Constant(item.StringValue?.Replace("'", string.Empty));
                        else
                            valueExpression = Expression.Constant(item.StringValue.Split(',').Select(x => x.Replace("'", string.Empty)).ToList());
                        break;
                    case TypeCode.DateTime:
                        if (item.OperationTypeId != (int)FilterOperations.InSet && item.OperationTypeId != (int)FilterOperations.OutOfSet)
                            valueExpression = Expression.Constant(item.DateTimeValue);
                        else
                            valueExpression = Expression.Constant(item.StringValue.Split(',').Select(DateTime.Parse).ToList());
                        break;
                    case TypeCode.Boolean:
                        if (item.OperationTypeId != (int)FilterOperations.InSet && item.OperationTypeId != (int)FilterOperations.OutOfSet)
                            valueExpression = Expression.Constant(item.IntValue == 1);
                        else
                            valueExpression = Expression.Constant(item.StringValue.Split(',').Select(Boolean.Parse).ToList());
                        break;
                }
                UnaryExpression convertedValue = null;
                if (item.OperationTypeId != (int)FilterOperations.InSet && item.OperationTypeId != (int)FilterOperations.OutOfSet)
                    convertedValue = Expression.Convert(valueExpression, type);

                switch (item.OperationTypeId)
                {
                    case (int)FilterOperations.IsEqualTo:
                        expression = Expression.Equal(leftExpression, convertedValue);
                        break;
                    case (int)FilterOperations.IsGreaterThenOrEqualTo:
                        expression = Expression.GreaterThanOrEqual(leftExpression, convertedValue);
                        break;
                    case (int)FilterOperations.IsGreaterThen:
                        expression = Expression.GreaterThan(leftExpression, convertedValue);
                        break;
                    case (int)FilterOperations.IsLessThenOrEqualTo:
                        expression = Expression.LessThanOrEqual(leftExpression, convertedValue);
                        break;
                    case (int)FilterOperations.IsLessThen:
                        expression = Expression.LessThan(leftExpression, convertedValue);
                        break;
                    case (int)FilterOperations.IsNotEqualTo:
                        expression = Expression.NotEqual(leftExpression, convertedValue);
                        break;
                    case (int)FilterOperations.Contains:
                        method = type.GetMethod("Contains", new[] { typeof(string) });
                        expression = Expression.Call(leftExpression, method, convertedValue);
                        break;
                    case (int)FilterOperations.StartsWith:
                        method = type.GetMethod("StartsWith", new[] { typeof(string) });
                        expression = Expression.Call(leftExpression, method, convertedValue);
                        break;
                    case (int)FilterOperations.EndsWith:
                        method = type.GetMethod("EndsWith", new[] { typeof(string) });
                        expression = Expression.Call(leftExpression, method, convertedValue);
                        break;
                    case (int)FilterOperations.DoesNotContain:
                        method = type.GetMethod("Contains", new[] { typeof(string) });
                        expression = Expression.Not(Expression.Call(leftExpression, method, valueExpression));
                        break;
                    case (int)FilterOperations.IsNull:
                        expression = Expression.Equal(leftExpression, Expression.Convert(Expression.Constant(null), type));
                        break;
                    case (int)FilterOperations.InSet:
                        var t = type;
                        var typesArray = type.GetGenericArguments();
                        if (typesArray.Count() > 0)
                            t = typesArray[0];
                        method = valueExpression.Type.GetMethod("Contains", new[] { t });
                        expression = Expression.Call(valueExpression, method, Expression.Convert(leftExpression, t));
                        break;
                    case (int)FilterOperations.OutOfSet:
                        t = type.GetGenericArguments()[0];
                        method = valueExpression.Type.GetMethod("Contains", new[] { t });
                        expression = Expression.Not(Expression.Call(valueExpression, method, Expression.Convert(leftExpression, t)));
                        break;
                }
                if (filtersOperation.IsAnd)
                    finalExpression = Expression.AndAlso(finalExpression, expression);
                else
                    finalExpression = Expression.OrElse(finalExpression, expression);
            }
            return Expression.Lambda<Func<T1, bool>>(finalExpression, parameter);
        }
    }
}