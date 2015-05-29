using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DYH.Core.Abstract;

namespace DYH.Core.Utils
{
    public class TreeUtils
    {
        public static TreeNode<T> GetTree<T>(TreeNode<T> root, IEnumerable<TreeNode<T>> list, T currentId)
        {
            return GetSubItem<T>(list, root, currentId);
        }

        private static TreeNode<T> GetSubItem<T>(IEnumerable<TreeNode<T>> source, TreeNode<T> parentNode, T currentId)
        {
            if (source == null || parentNode == null)
            {
                return null;
            }
            var list = source.Where(x => x.ParentId.Equals(parentNode.NodeId)).OrderBy(x => x.Order);

            foreach (var item in list)
            {
                var child = Serialize.Dereference(item);
                child.IsActived = child.NodeId.Equals(currentId);

                parentNode.Children.Add(GetSubItem(source, child, currentId));

                if (child.Children.Any(x => x.IsActived))
                {
                    child.IsActived = true;
                }
            }

            return parentNode;
        }
    }
}
