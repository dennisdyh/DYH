using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace DYH.Core.Abstract
{
    public interface IDependencyRegistrar
    {
        /// <summary>
        /// 注册类型
        /// </summary>
        /// <param name="builder">实现为组件注册容器</param>
        /// <param name="typeFinder">为所有实现了该接口的类型提供类型信息入口</param>
        void Register(ContainerBuilder builder, ITypeFinder typeFinder);
        /// <summary>
        /// 排序序号，表示那个先注册依赖
        /// </summary>
        int Order { get; }
    }
}
