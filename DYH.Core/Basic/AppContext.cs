using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DYH.Core.Abstract;

namespace DYH.Core.Basic
{
    public class AppContext
    {
        /// <summary>
        /// <para>初始化一个NOP工厂的静态实例,该方法是线程同步的</para>
        /// </summary>
        /// <param name="forceRecreate">
        /// <para>如果是真，创建一个新的工厂实例即使该实例在之前已经被初始化过</para>
        /// </param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IApp Initialize(bool forceRecreate)
        {

            if (Singleton<IApp>.Instance == null //判断引擎是否已经初始化过
                || forceRecreate //强制初始化
                )
            {
                Singleton<IApp>.Instance = new AppRuntime();
                Singleton<IApp>.Instance.Initialize();
            }

            return Singleton<IApp>.Instance;
        }

        /// <summary>
        /// <para>获取一个单例引擎NopEngine用来访问各种服务</para>
        /// </summary>
        public static IApp Current
        {
            get
            {
                if (Singleton<IApp>.Instance == null)
                {
                    return Initialize(false);
                }
                return Singleton<IApp>.Instance;
            }
        }
    }
}
