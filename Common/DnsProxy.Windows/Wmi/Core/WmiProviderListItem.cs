using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;

namespace BAG.IT.Core.Wmi.Core
{
    public abstract class WmiProviderListItem : IWmiProviderListItem
    {
        public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();


        public void SetData([NotNull] ManagementObject queryObj)
        {
            if (queryObj == null) throw new ArgumentNullException(nameof(queryObj));
            Data.Clear();
            foreach (PropertyData data in queryObj.Properties)
            {
                Data.Add(data.Name, data.Value);
            }


            var props = from p in GetType().GetProperties()
                        let attr = p.GetCustomAttributes(typeof(WmiNameAttribute), true)
                        where attr.Length == 1
                        select new { Property = p, Attribute = attr.First() as WmiNameAttribute };

            foreach (var prop in props)
            {
                if (prop.Property.PropertyType.IsArray)
                {
                    var val = queryObj[prop.Attribute.Name];
                    prop.Property.SetValue(this, val);
                }
                else
                {
                    // var val = queryObj[prop.Attribute.Name] == null ? "" : queryObj[prop.Attribute.Name].ToString();
                    var targetType = prop.Property.PropertyType;
                    var wmiObject = queryObj[prop.Attribute.Name];
                    var convertObject = ConvertObject(wmiObject, targetType);

                    prop.Property.SetValue(this, convertObject);
                }
            }
        }

        private object ConvertObject(object input, Type targetType)
        {
            object convertObject = null;
            if (input == null)
            {
                if (targetType.IsValueType)
                {
                    convertObject = Activator.CreateInstance(targetType);
                }
                else if (targetType == typeof(string))
                {
                    convertObject = string.Empty;
                }

            }
            else if (targetType == typeof(DateTime))
            {
                convertObject = ManagementDateTimeConverter.ToDateTime(input.ToString());
            }
            else if (targetType == typeof(bool))
            {
                convertObject = Convert.ChangeType(input, targetType, CultureInfo.InvariantCulture);
            }
            else
            {
                convertObject = Convert.ChangeType(input, targetType, CultureInfo.InvariantCulture);
            }

            return convertObject;

        }
    }
}