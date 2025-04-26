using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRay.Utility
{
    internal class GF256
    {
        private ResourceDatabase database = ResourceDatabase.Instance;
        public Dictionary<int, int> Log = new Dictionary<int, int>();
        public Dictionary<int, int> AntiLog = new Dictionary<int, int>();

        public GF256()
        {
            Log = database.LogTable;
            AntiLog = database.AntiLogTable;
        }
    }
}
