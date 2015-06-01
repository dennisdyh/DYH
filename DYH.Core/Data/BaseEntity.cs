using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYH.Core.Data
{
    /// <summary>
    /// 抽象实体基类，所有实体都继承自该实体
    /// </summary>
    /// <typeparam name="T">ID类型</typeparam>
    /// <typeparam name="TPrimaryKey"></typeparam>
    public abstract class BaseEntity<TPrimaryKey> where TPrimaryKey : struct
    {
        /// <summary>
        /// 设置或者获取实体类型的唯一ID
        /// </summary>
        public TPrimaryKey Id { get; set; }
        /// <summary>
        /// 重写Equals方法用来对比对象是否是同一个对象
        /// </summary>
        /// <param name="obj">传入对象----当前对象 与 传入对象 对比</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as BaseEntity<TPrimaryKey>);
        }

        private static bool IsTransient(BaseEntity<TPrimaryKey> obj)
        {

            return
                //对象是否为空
                obj != null &&
                //对象Id 是否为0
                Equals(obj.Id, default(int));
        }

        /// <summary>
        /// 获取当前实例的类型
        /// </summary>
        /// <returns></returns>
        private Type GetUnproxiedType()
        {
            return GetType();
        }

        /// <summary>
        /// 自定义的Equals方法，指定的类型是否与当前类型是否是同一个对象
        /// </summary>
        /// <param name="other">传入对象</param>
        /// <returns></returns>
        public virtual bool Equals(BaseEntity<TPrimaryKey> other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (!IsTransient(this) &&
                !IsTransient(other) &&
                Equals(Id, other.Id))
            {
                var otherType = other.GetUnproxiedType();
                var thisType = GetUnproxiedType();
                return thisType.IsAssignableFrom(otherType) ||
                        otherType.IsAssignableFrom(thisType);
            }

            return false;
        }
        /// <summary>
        /// 通过该类的Id来返回该实例的哈希值
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (Equals(Id, default(int)))
                return base.GetHashCode();
            return Id.GetHashCode();
        }
        /// <summary>
        /// 重载==操作符
        /// </summary>
        /// <param name="x">对象A</param>
        /// <param name="y">对象B</param>
        /// <returns></returns>
        public static bool operator ==(BaseEntity<TPrimaryKey> x, BaseEntity<TPrimaryKey> y)
        {
            return Equals(x, y);
        }
        /// <summary>
        /// 操作符重载！=
        /// </summary>
        /// <param name="x">对象A</param>
        /// <param name="y">对象B</param>
        /// <returns></returns>
        public static bool operator !=(BaseEntity<TPrimaryKey> x, BaseEntity<TPrimaryKey> y)
        {
            return !(x == y);
        }
    }
}
