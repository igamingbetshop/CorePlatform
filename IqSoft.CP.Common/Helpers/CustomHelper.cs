using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IqSoft.CP.Common.Helpers
{
    public static class CustomHelper
    {
        public static Dictionary<int, int> MapUserStateToClient = new Dictionary<int, int>
        {
            { (int)UserStates.Active, (int)ClientStates.Active },
            { (int)UserStates.Closed, (int)ClientStates.FullBlocked },
            { (int)UserStates.InactivityClosed, (int)ClientStates.FullBlocked }, //??
            { (int)UserStates.Disabled, (int)ClientStates.Disabled },
            { (int)UserStates.Suspended, (int)ClientStates.Suspended },
            { (int)UserStates.ForceBlock, (int)ClientStates.ForceBlock }
        };

        public static bool Greater(UserStates leftValue, UserStates rightValue)
        {
            switch (leftValue)
            {
                case UserStates.Active:
                    return false;
                case UserStates.Suspended:
                    return rightValue == UserStates.Active || rightValue == UserStates.Suspended;
                case UserStates.Closed:
                case UserStates.InactivityClosed:
                    return (rightValue == UserStates.Active || rightValue == UserStates.Suspended ||
                        rightValue == UserStates.ForceBlock || rightValue == UserStates.ForceBlockBySecurityCode);
                case UserStates.ForceBlock:
                case UserStates.ForceBlockBySecurityCode:
                    return (rightValue == UserStates.Active || rightValue == UserStates.Suspended);
                case UserStates.Disabled:
                    return true;
            }
            return false;
        }
        public static bool Greater(ClientStates leftValue, ClientStates rightValue)
        {
            switch (leftValue)
            {
                case ClientStates.Active:
                    return false;
                case ClientStates.Suspended:
                    return rightValue == ClientStates.Active || rightValue == ClientStates.Suspended;
                case ClientStates.FullBlocked:
                    return (rightValue == ClientStates.Active || rightValue == ClientStates.Suspended || rightValue == ClientStates.ForceBlock);
                case ClientStates.ForceBlock:
                    return (rightValue == ClientStates.Active || rightValue == ClientStates.Suspended);
                case ClientStates.Disabled:
                    return true;
            }
            return false;
        }
        public static string ParseOperationType(int operationTypes)
        {
            switch (operationTypes)
            {
                case (int)FilterOperations.IsEqualTo:
                    return "=";
                case (int)FilterOperations.IsGreaterThenOrEqualTo:
                    return ">=";
                case (int)FilterOperations.IsGreaterThen:
                    return ">";
                case (int)FilterOperations.IsLessThenOrEqualTo:
                    return "<=";
                case (int)FilterOperations.IsLessThen:
                    return "<";
                case (int)FilterOperations.IsNotEqualTo:
                    return "!=";
                case (int)FilterOperations.InSet:
                    return "in";
                default:
                    return string.Empty;
            }
        }

        public static IQueryable<T1> FilterByCondition<T1>(this IQueryable<T1> objects, Condition filtersOperation, string fieldName,
                                                           string additionalFieldName = null, bool isAndCondition = true)
        {
            if (objects != null && filtersOperation != null && filtersOperation.ConditionItems != null && 
                filtersOperation.ConditionItems.Any(x => x.StringValue != null))
            {
                objects = objects.Where(BuildExpression<T1>(filtersOperation, fieldName, additionalFieldName, isAndCondition));
            }
            return objects;
        }

        public static IEnumerable<T1> FilterByCondition<T1>(this IEnumerable<T1> objects, Condition filtersOperation, string fieldName,
                                                            string additionalFieldName = null, bool isAndCondition = true)
        {
            if (filtersOperation != null && filtersOperation.ConditionItems != null && filtersOperation.ConditionItems.Any())
            {
                objects = objects.Where(BuildExpression<T1>(filtersOperation, fieldName, additionalFieldName, isAndCondition).Compile());
            }
            return objects;
        }

        private static Expression<Func<T1, bool>> BuildExpression<T1>(Condition filtersOperation, string fieldName,
                                                                      string additionalFieldName, bool isAndCondition = true)
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

            Expression finalExpression = Expression.Constant(true);
            Expression expression = null;
            MethodInfo method;
            ConstantExpression valueExpression = null;
            foreach (var item in filtersOperation.ConditionItems)
            {
                switch (typeCode)
                {
                    case TypeCode.Int64:
                        if (item.OperationTypeId != (int)FilterOperations.InSet && item.OperationTypeId != (int)FilterOperations.OutOfSet)
                            valueExpression = Expression.Constant(Convert.ToInt64(item.StringValue));
                        else
                            valueExpression = Expression.Constant(item.StringValue.Split(',').Select(Int64.Parse).ToList());
                        break;
                    case TypeCode.Int32:
                        if (item.OperationTypeId != (int)FilterOperations.InSet && item.OperationTypeId != (int)FilterOperations.OutOfSet)
                            valueExpression = Expression.Constant(Convert.ToInt32(item.StringValue));
                        else
                            valueExpression = Expression.Constant(item.StringValue.Split(',').Select(Int32.Parse).ToList());
                        break;
                    case TypeCode.Decimal:
                        if (item.OperationTypeId != (int)FilterOperations.InSet && item.OperationTypeId != (int)FilterOperations.OutOfSet)
                            valueExpression = Expression.Constant(Convert.ToDecimal(item.StringValue));
                        else
                            valueExpression = Expression.Constant(item.StringValue.Split(',').Select(Decimal.Parse).ToList());
                        break;
                    case TypeCode.String:
                        if (item.OperationTypeId != (int)FilterOperations.InSet && item.OperationTypeId != (int)FilterOperations.OutOfSet)
                            valueExpression = Expression.Constant(item.StringValue);
                        else
                            valueExpression = Expression.Constant(item.StringValue.Split(',').ToList());
                        break;
                    case TypeCode.DateTime:
                        if (item.OperationTypeId != (int)FilterOperations.InSet && item.OperationTypeId != (int)FilterOperations.OutOfSet)
                            valueExpression = Expression.Constant(Convert.ToDateTime(item.StringValue));
                        else
                            valueExpression = Expression.Constant(item.StringValue.Split(',').Select(DateTime.Parse).ToList());
                        break;
                    case TypeCode.Boolean:
                        if (item.OperationTypeId != (int)FilterOperations.InSet && item.OperationTypeId != (int)FilterOperations.OutOfSet)
                            valueExpression = Expression.Constant(item.StringValue != "0");
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
                        method = valueExpression.Type.GetMethod("Contains", new[] { type });
                        expression = Expression.Call(valueExpression, method, leftExpression);
                        break;
                    case (int)FilterOperations.OutOfSet:
                        method = valueExpression.Type.GetMethod("Contains", new[] { type });
                        expression = Expression.Not(Expression.Call(valueExpression, method, leftExpression));
                        break;
                }
                if (isAndCondition)
                    finalExpression = Expression.AndAlso(finalExpression, expression);
                else
                    finalExpression = Expression.OrElse(finalExpression, expression);
            }
            return Expression.Lambda<Func<T1, bool>>(finalExpression, parameter);
        }
    }
}