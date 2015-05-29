using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace DYH.Core.Extensions
{
    public static class EnumExtension
    {
        /// <summary>
        /// 返回指定枚举值的描述（通过 
        /// <see cref="System.ComponentModel.DescriptionAttribute"/> 指定）。
        /// 如果没有指定描述，则返回枚举常数的名称，没有找到枚举常数则返回枚举值。
        /// </summary>
        /// <param name="value">要获取描述的枚举值。</param>
        /// <returns>指定枚举值的描述。</returns>
        public static string GetDescription(this Enum value)
        {
            var description = string.Empty;
            var name = Enum.GetName(value.GetType(), value);
            if (!string.IsNullOrEmpty(name))
            {
                var info = value.GetType().GetField(name);
                if (info != null)
                {
                    var attr = info.GetCustomAttribute(typeof (DescriptionAttribute), false) as DescriptionAttribute;
                    if (attr != null)
                    {
                        description = attr.Description;
                    }
                }
            }

            return description;
        }
    }
}
