using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DYH.Core.Basic
{
    /// <summary>
    /// 提供一个便捷方式来实现多线程读取，单线程写
    /// </summary>
    public class WriteLockDisposable : IDisposable
    {
        /// <summary>
        /// 表示用于管理资源访问的锁定状态，可实现多线程读取或进行独占式写入访问
        /// </summary>
        private readonly ReaderWriterLockSlim _rwLock;
        /// <summary>
        /// 初始化一个锁定访问资源的实例 <see cref="WriteLockDisposable"/> 
        /// </summary>
        /// <param name="rwLock">表示用于管理资源访问的锁定状态，可实现多线程读取或进行独占式写入访问</param>
        public WriteLockDisposable(ReaderWriterLockSlim rwLock)
        {
            _rwLock = rwLock;
            _rwLock.EnterWriteLock();
        }

        public void Dispose()
        {
            _rwLock.ExitWriteLock();
        }
    }
}
