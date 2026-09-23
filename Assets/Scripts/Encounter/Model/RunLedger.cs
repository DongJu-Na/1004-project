using System.Collections.Generic;
using System.Linq;

namespace Project1028.Encounter
{
    /// <summary>런 공유 기록: 결과 플래그·발생 이력. 저장은 후속(Export/Import). 순수 로직.</summary>
    public sealed class RunLedger
    {
        private readonly HashSet<string> flags = new HashSet<string>();
        private readonly HashSet<string> history = new HashSet<string>();

        public IReadOnlyCollection<string> Flags => flags;
        public IReadOnlyCollection<string> History => history;

        public bool Record(string encounterId) => !string.IsNullOrEmpty(encounterId) && history.Add(encounterId);

        public int AddFlags(IEnumerable<string> ids)
        {
            int added = 0;
            if (ids == null) return 0;
            foreach (var f in ids) if (!string.IsNullOrEmpty(f) && flags.Add(f)) added++;
            return added;
        }

        public void Import(IEnumerable<string> priorHistory)
        {
            if (priorHistory == null) return;
            foreach (var h in priorHistory) if (!string.IsNullOrEmpty(h)) history.Add(h);
        }

        public (string[] flags, string[] history) Export() => (flags.OrderBy(x => x).ToArray(), history.OrderBy(x => x).ToArray());

        public static List<CallbackEntry> CallbacksFor(string flag, CallbackEntry[] table)
        {
            var list = new List<CallbackEntry>();
            if (table == null || string.IsNullOrEmpty(flag)) return list;
            foreach (var c in table) if (c != null && c.flag == flag) list.Add(c);
            return list;
        }
    }
}
