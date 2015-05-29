using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYH.Core.Basic
{
    public class Singleton
    {
        private static volatile IDictionary<Type, object> instance = null;
        private static readonly object SyncRoot = new object();

        static Singleton()
        {

        }

        /// <summary>
        /// 获取所有的单例实例类型的数据字典
        /// </summary>
        public static IDictionary<Type, object> AllInstance
        {
            get
            {
                if (instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (instance == null)
                            instance = new Dictionary<Type, object>();
                    }
                }

                return instance;
            }
        }
    }

    /// <summary>
    /// 一个静态编译的单例用来存储整个应用程序域生命周期中的对象
    /// </summary>
    /// <typeparam name="T">存储对象的类型</typeparam>
    /// <remarks>对实例的访问时同步的</remarks>
    public class Singleton<T> : Singleton
    {
        static T _instance;

        /// <summary>
        /// 获取或设置给定类型的单例实例，在当前实例列表中每一个类型只有一个实例
        /// </summary>
        public static T Instance
        {
            get { return _instance; }
            set
            {
                _instance = value;
                AllInstance[typeof(T)] = value;
            }
        }
    }
}
