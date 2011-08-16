using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectPresenter
{
    public class CueStore
    {
        Dictionary<int, List<string>> store;

        private CueStore()
        {
            store = new Dictionary<int, List<string>>();
        }

        public string Get(int slideId, int step)
        {
            if (step >= store[slideId].Count)
            {
                return null;
            }
            else
            {
                return store[slideId][step];
            }
        }

        public HashSet<string> Flatten()
        {
            List<string> list = new List<string>();

            foreach(KeyValuePair<int, List<string>> pair in store)
            {
                list.AddRange(pair.Value);
            }

            return new HashSet<string>(list);
        }

        public static CueStore Deserialize()
        {
            return new CueStore();
        }

        public void Serialize()
        {
        }
    }
}
