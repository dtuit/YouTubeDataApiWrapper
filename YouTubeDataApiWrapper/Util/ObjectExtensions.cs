using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeDataRetrievalWrapper.Util
{
    public static class ObjectExtensions
    {
        public static TProperty GetPropertyValue<TProperty>(this object obj, string propertyName)
        {
            var prop = obj.GetType().GetProperty(propertyName, typeof (TProperty));
            if (prop == null)
            {
                return default(TProperty);
            }
            return (TProperty) prop.GetValue(obj, null);
        }

        public static object GetPropertyValue(this object obj, string propertyName)
        {
            var prop = obj.GetType().GetProperty(propertyName);
            return prop?.GetValue(obj, null);
        }
    }
}
