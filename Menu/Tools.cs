using System;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using System.Security.Policy;

namespace TreeMenu
{
    class Tools
    {
        public static void refreshNodes( TreeNodeCollection nodes )
        {
            foreach( TreeNode node in nodes )
            {
                int i = 0;
                refreshNode(node, ref i);
            }
        }


        public volatile static bool refreshed = false;
        public volatile static int redLimit = 15;
        static void refreshNode(TreeNode node, ref int totalCount)
        {
            int myCount = node.Nodes.Count;
            int allChildCount = 0;

            if (myCount > 0)
            {
                foreach ( TreeNode cNode in node.Nodes )
                {
                    refreshNode(cNode, ref allChildCount);
                }
            }

            totalCount++;
            totalCount += allChildCount;

            NodeTag tag = node.Tag as NodeTag;
            tag.ChildCount = myCount;
            tag.TotalChileCount = allChildCount;

            if (myCount >= redLimit)
            {
                node.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                node.ForeColor = System.Drawing.Color.Black;
            }

            node.Text = string.Format("{0} {1}({2}-{3})", tag.Name, tag.Account, myCount, allChildCount);

            refreshed = true;
        }


        public static string convertToAscii(string str)
        {
            //byte[] array = System.Text.Encoding.ASCII.GetBytes(str);
            //string ASCIIstr2 = "";
            //for (int i = 0; i < array.Length; i++)
            //{
            //    int asciicode = (int)(array[i]);
            //    ASCIIstr2 += Convert.ToString(asciicode);
            //}
            //return ASCIIstr2;


            return Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(str.ToCharArray()));
        }
    }
}
