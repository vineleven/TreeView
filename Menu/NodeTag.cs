using System;
using System.Collections;
using System.Text;

namespace TreeMenu
{
    class NodeTag
    {
        public string Name;
        public string Account;
        public int ChildCount = 0;
        public int TotalChileCount = 0;



        public NodeTag()
        {
        }


        public NodeTag(string name, string account)
        {
            this.Name = name;
            this.Account = account;
        }


        public Hashtable encode()
        {
            Hashtable t = new Hashtable();
            t["n"] = Name;
            t["a"] = Account;
            t["c"] = ChildCount;
            t["t"] = TotalChileCount;

            return t;
        }


        public void decode(Hashtable data)
        {
            Name = data["n"] as string;
            Account = data["a"] as string;
            //ChildCount = (int)data["c"];
            //TotalChileCount = (int)data["t"];
        }
    }
}
