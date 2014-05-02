using System;
using System.Collections.Generic;
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
            Name = name;
        }

        #endregion

        /// <summary>
        /// The table name used in the database
        /// </summary>
        public string Name { get; set; }

        #region Static

        private static Dictionary<Type, string> TableNames = new Dictionary<Type, string>();

        /// <summary>
        /// Get the table name for a particular object type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTableName(Type type)
        {
            if (!TableNames.ContainsKey(type))
            {
                lock (TableNames)
                {
                    if (!TableNames.ContainsKey(type))
                    {
                        TableNames[type] = type.Name;

                        var attrs = type.GetCustomAttributes(typeof(TableAttribute), false);
                        if (attrs != null && attrs.Length > 0)
                        {
                            var tableAttr = attrs[0] as TableAttribute;
                            if (tableAttr != null && !string.IsNullOrWhiteSpace(tableAttr.Name))
                            {
                                TableNames[type] = tableAttr.Name;
                            }
                        }
                    }
                }
            }

            return TableNames[type];
        }

        #endregion
    }
}
