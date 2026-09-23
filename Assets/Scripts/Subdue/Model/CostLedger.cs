using System.Collections.Generic;

namespace Project1028.Subdue
{
    /// <summary>§4.4 대가 원장: 제압 목격은 제압 1회당 1회, 발견은 (기절자, 발견자)당 1회, 목격자는 발견 대가 없음. 순수 로직.</summary>
    public sealed class CostLedger
    {
        private readonly Dictionary<string, HashSet<string>> witnesses = new Dictionary<string, HashSet<string>>();
        private readonly Dictionary<string, HashSet<string>> discoverers = new Dictionary<string, HashSet<string>>();
        private readonly HashSet<string> witnessRecorded = new HashSet<string>();

        /// <summary>제압 순간의 목격자 집합을 기록한다. 같은 기절 건에 대해 처음이면 true.</summary>
        public bool RecordWitnesses(string bodyId, IEnumerable<string> observerIds)
        {
            if (!witnesses.TryGetValue(bodyId, out var set)) { set = new HashSet<string>(); witnesses[bodyId] = set; }
            if (observerIds != null) foreach (var id in observerIds) set.Add(id);
            return witnessRecorded.Add(bodyId);
        }

        public bool IsWitness(string bodyId, string observerId) =>
            witnesses.TryGetValue(bodyId, out var set) && set.Contains(observerId);

        /// <summary>발견을 기록한다. 목격자이거나 이미 발견했으면 false.</summary>
        public bool TryRecordDiscovery(string bodyId, string observerId)
        {
            if (IsWitness(bodyId, observerId)) return false;
            if (!discoverers.TryGetValue(bodyId, out var set)) { set = new HashSet<string>(); discoverers[bodyId] = set; }
            return set.Add(observerId);
        }

        public int DiscovererCount(string bodyId) => discoverers.TryGetValue(bodyId, out var s) ? s.Count : 0;
        public int WitnessCount(string bodyId) => witnesses.TryGetValue(bodyId, out var s) ? s.Count : 0;

        public void Clear(string bodyId)
        {
            witnesses.Remove(bodyId);
            discoverers.Remove(bodyId);
            witnessRecorded.Remove(bodyId);
        }
    }
}
