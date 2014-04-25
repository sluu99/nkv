using System;
using System.Reflection;

namespace Nkv.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        #region Constructors

        public TableAttribute() : this(string.Empty) { }

        public TableAttribute(string name)
        {
            this.Name = name;
        }

        #endregion

        /// <summary>
        /// The table name used in the database
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Get the table name for a particular object type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTableName(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(TableAttribute), false);
            if (attrs != null && attrs.Length > 0)
            {
                var tableAttr = attrs[0] as TableAttribute;
                if (tableAttr != null && !string.IsNullOrWhiteSpace(tableAttr.Name))
                {
                    return tableAttr.Name;
                }
            }
            
            return type.Name;
        }
    }
}
