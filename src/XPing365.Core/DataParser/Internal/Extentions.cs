using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HtmlAgilityPack;
using XPing365.Core.DataParser.Converters;
using XPing365.Core.DataSource;

namespace XPing365.Core.DataParser.Internal
{
    internal static class Extentions
    {
        private static readonly Lazy<IValueConverter> defaultConverter = new(() => new DefaultValueConverter());

        private class PropertyComparer : IEqualityComparer<PropertyInfo>
        {
            public bool Equals(PropertyInfo? x, PropertyInfo? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }
                else if (x == null || y == null)
                {
                    return false;
                }
                else if (string.Compare(x.Name, y.Name) == 0)
                {
                    return true;
                }

                return false;
            }

            public int GetHashCode([DisallowNull] PropertyInfo obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        public static bool IsList(this Type type) => type.IsGenericType &&
            (type.GetGenericTypeDefinition() == typeof(List<>) ||
             type.GetGenericTypeDefinition() == typeof(IList<>));

        public static IList? CreateList(this Type t)
        {
            if (!t.IsList())
            {
                return null;
            }

            var listType = typeof(List<>);
            var typeParameter = t.GetGenericArguments().First();
            var constructedListType = listType.MakeGenericType(typeParameter);
            var instance = Activator.CreateInstance(constructedListType);

            return instance as IList;
        }

        public static bool HasDefaultConstructor(this Type t)
        {
            return t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null;
        }

        public static object? CreateListItem(this Type t)
        {
            if (!t.IsList())
            {
                return null;
            }

            var typeParameter = t.GetGenericArguments().First();

            if (typeParameter == typeof(string))
            {
                return string.Empty;
            }
            else if (typeParameter.HasDefaultConstructor())
            {
                return Activator.CreateInstance(typeParameter);
            }
            else
            {
                throw new InvalidOperationException($"Type {typeParameter.Name} does not have default constuctor");
            }
        }

        /// <summary>
        /// Gets properties from type that meets criteria for data parser.
        /// </summary>
        /// <param name="subject">Type for which the properties should be retrieved.</param>
        /// <returns>List of properties</returns>
        /// <exception cref="ArgumentException">When property is decorated with XPath and does not have default ctor or write access.</exception>
        public static IList<PropertyInfo> GetPropertiesToVisit(this Type subject)
        {
            List<PropertyInfo> results = new();

            // Filtering criteria for properties:
            // - public and instance type
            // - exclued properties from base class HtmlDataSource
            // - property type is a class type or has an XPath attribute
            // - property type has default ctor (including value types) or is string
            // - property has write access
            var properties = subject.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    .Except(typeof(HtmlSource).GetProperties(), new PropertyComparer());

            foreach (var p in properties)
            {
                if (p.GetAttribute<XPathAttribute>() != null && 
                   !(p.PropertyType.HasDefaultConstructor() || p.PropertyType == typeof(string) || p.PropertyType.IsList()))
                {
                    throw new ArgumentException($"Property: \"{p.Name}\" of class: \"{subject.Name}\" is decorated with XPathAttribute " + 
                                                "but does not have default constructor");
                }

                if (p.GetAttribute<XPathAttribute>() != null &&
                   !p.CanWrite)
                {
                    throw new ArgumentException($"Property: \"{p.Name}\" of class: \"{subject.Name}\" is decorated with XPathAttribute " +
                                                "but does not have access write");
                }

                if (p.CanWrite && (p.GetAttribute<XPathAttribute>() != null || (p.PropertyType.IsClass && p.PropertyType.HasDefaultConstructor())))
                {
                    results.Add(p);
                }
            }

            return results;
        }

        public static T? GetAttribute<T>(this PropertyInfo property) where T : Attribute
        {
            var attribute = property.GetCustomAttributes(typeof(T), false)
                                    .Select(i => (T)i)
                                    .FirstOrDefault();

            return attribute;
        }

        public static IValueConverter GetConverter(this PropertyInfo propertyInfo)
        {
            var converter = propertyInfo.GetCustomAttributes()
                                        .Where(i => i.GetType().IsAssignableTo(typeof(IValueConverter)))
                                        .Select(i => (IValueConverter)i)
                                        .FirstOrDefault();

            if (converter == null)
            {
                converter = defaultConverter.Value;
            }

            return converter;
        }
    }
}
