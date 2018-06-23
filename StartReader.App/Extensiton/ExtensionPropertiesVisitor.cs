using System;
using System.Buffers;
using System.Linq;
using Windows.Foundation.Collections;

namespace StartReader.App.Extensiton
{
    static class ExtensionPropertiesVisitor
    {
        public static string GetProperty(this IPropertySet prop, string propName)
        {
            if (prop.TryGetValue("@" + propName, out var attr) && attr is string s0)
                return s0;
            if (!prop.TryGetValue(propName, out var ele))
                return null;
            if (ele is IPropertySet ps && ps.TryGetValue("#text", out var cont) && cont is string s1)
                return s1;
            if (ele is object[])
                return GetProperties(prop, propName).FirstOrDefault();
            return null;
        }

        public static string[] GetProperties(this IPropertySet prop, string propName)
        {
            var resultTemp = ArrayPool<string>.Shared.Rent(16);
            var propCount = 0;
            if (prop.TryGetValue("@" + propName, out var attr) && attr is string s0)
                resultTemp[propCount++] = s0;
            if (prop.TryGetValue(propName, out var ele))
            {
                if (ele is IPropertySet ps && ps.TryGetValue("#text", out var cont) && cont is string s1)
                    resultTemp[propCount++] = s1;
                else if (ele is object[] pss)
                {
                    foreach (var item in pss.OfType<IPropertySet>())
                    {
                        if (item.TryGetValue("#text", out var cont2) && cont2 is string s2)
                        {
                            resultTemp[propCount++] = s2;
                            if (resultTemp.Length <= propCount)
                            {
                                var newTemp = ArrayPool<string>.Shared.Rent(resultTemp.Length * 2);
                                Array.Copy(resultTemp, 0, newTemp, 0, propCount);
                                ArrayPool<string>.Shared.Return(resultTemp);
                                resultTemp = newTemp;
                            }
                        }
                    }
                }
            }

            if (propCount == 0)
                return Array.Empty<string>();
            var r = new string[propCount];
            Array.Copy(resultTemp, 0, r, 0, propCount);
            ArrayPool<string>.Shared.Return(resultTemp);
            return r;
        }

        public static IPropertySet GetChild(this IPropertySet prop, string childName)
        {
            if (!prop.TryGetValue(childName, out var value))
                return null;
            return value as IPropertySet;
        }

        public static IPropertySet[] GetChildren(this IPropertySet prop, string childName)
        {
            if (!prop.TryGetValue(childName, out var value))
                return Array.Empty<IPropertySet>();
            switch (value)
            {
            case object[] array:
                var r = new IPropertySet[array.Length];
                array.CopyTo(r, 0);
                return r;
            case IPropertySet p:
                return new[] { p };
            default:
                return Array.Empty<IPropertySet>();
            }
        }
    }
}
