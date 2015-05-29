using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DYH.Core.Abstract
{
    public abstract class TreeNode<T>
    {
        private readonly List<TreeNode<T>> _list = new List<TreeNode<T>>();
        public virtual T NodeId { get; set; }
        public virtual T ParentId { get; set; }
        public virtual string NodeName { get; set; }
        public virtual int Order { get; set; }
        public virtual bool IsActived { get; set; }
        public virtual List<TreeNode<T>> Children {
            get { return _list; }
        }
    }
}
