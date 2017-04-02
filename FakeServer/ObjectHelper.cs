﻿using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace FakeServer
{
    public static class ObjectHelper
    {
        /// <summary>
        /// Find property from ExpandoObject and compare it to provided value
        /// </summary>
        /// <param name="current"></param>
        /// <param name="propertyName"></param>
        /// <param name="valueToCompare"></param>
        /// <returns>True is object matches valueToCompare</returns>
        public static bool GetPropertyAndCompare(ExpandoObject current, string propertyName, string valueToCompare)
        {
            var currentProperty = propertyName.Contains('.') ? propertyName.Split('.').First() : propertyName;
            var tail = propertyName.Contains('.') ? propertyName.Substring(propertyName.IndexOf('.') + 1) : string.Empty;

            var currentValue = ((IDictionary<string, object>)current)[currentProperty];

            if (string.IsNullOrEmpty(tail))
                return ((dynamic)currentValue).ToString() == valueToCompare;

            if (currentValue is IEnumerable<dynamic> valueEnumerable)
                return valueEnumerable.Any(e => GetPropertyAndCompare(e, tail, valueToCompare));
            else
                return GetPropertyAndCompare(currentValue as ExpandoObject, tail, valueToCompare);
        }

        /// <summary>
        /// Find property from ExpandoObject
        /// 
        /// Split nested properties with backslash, e.g. child/grandchild/grandgrandchild
        /// 
        /// If path contains integers, those are used as id field comparisons
        /// </summary>
        /// <param name="current"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static ExpandoObject GetNestedProperty(ExpandoObject current, string propertyName)
        {
            var propertyNameCurrent = propertyName.Contains('/') ? propertyName.Split('/').First() : propertyName;
            var tail = propertyName.Contains('/') ? propertyName.Substring(propertyName.IndexOf('/') + 1) : string.Empty;
            var peekProperty = tail.Contains('/') ? tail.Split('/').FirstOrDefault() : string.Empty;

            var currentValue = ((IDictionary<string, object>)current)[propertyNameCurrent];

            dynamic returnValue;

            if (int.TryParse(peekProperty, out int parsedInteger))
            {
                tail = tail.Contains('/') ? tail.Substring(tail.IndexOf('/') + 1) : string.Empty;

                if (currentValue is IEnumerable<dynamic> valueEnumerable)
                    returnValue = valueEnumerable.FirstOrDefault(e => e.id == parsedInteger);
                else
                    returnValue = ((dynamic)currentValue).id == parsedInteger ? currentValue as ExpandoObject : null;
            }
            else
            {
                returnValue = currentValue;
            }

            if (string.IsNullOrEmpty(tail))
                return returnValue;
            else
                return GetNestedProperty(returnValue, tail);
        }
    }
}