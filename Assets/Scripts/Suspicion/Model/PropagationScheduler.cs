using System;
using System.Collections.Generic;

namespace Project1028.Suspicion
{
    /// <summary>
    /// §2.6 전파 예약. 순수 로직(string 키). 만남(거리 안) 동안 (from,to,player)는 1회만 예약되고,
    /// 만남이 끝나면 초기화되어 다음 만남에 다시 예약할 수 있다. 예약은 원인 NPC의 의심이 내려가도 실행된다.
    /// </summary>
    public sealed class PropagationScheduler
    {
        public struct Reservation
        {
            public string From;
            public string To;
            public string PlayerId;
            public float ExecuteAt;
        }

        private sealed class Meeting
        {
            public bool Active;
            public readonly HashSet<(string, string, string)> Reserved = new HashSet<(string, string, string)>();
        }

        private readonly Dictionary<(string, string), Meeting> meetings = new Dictionary<(string, string), Meeting>();
        private readonly List<Reservation> pending = new List<Reservation>();

        public IReadOnlyList<Reservation> Pending => pending;

        private static (string, string) Key(string a, string b) =>
            string.CompareOrdinal(a, b) <= 0 ? (a, b) : (b, a);

        public bool IsMeeting(string a, string b) => meetings.TryGetValue(Key(a, b), out var m) && m.Active;

        public void SetMeeting(string a, string b, bool inRange, float now)
        {
            var key = Key(a, b);
            if (!meetings.TryGetValue(key, out var m))
            {
                if (!inRange) return;
                m = new Meeting();
                meetings[key] = m;
            }
            if (m.Active && !inRange)
            {
                m.Active = false;
                m.Reserved.Clear();
            }
            else if (!m.Active && inRange)
            {
                m.Active = true;
            }
        }

        public bool TryReserve(string from, string to, string playerId, float executeAt)
        {
            if (!meetings.TryGetValue(Key(from, to), out var m) || !m.Active) return false;
            if (!m.Reserved.Add((from, to, playerId))) return false;
            pending.Add(new Reservation { From = from, To = to, PlayerId = playerId, ExecuteAt = executeAt });
            return true;
        }

        public IEnumerable<Reservation> Drain(float now)
        {
            var due = new List<Reservation>();
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                if (pending[i].ExecuteAt <= now)
                {
                    due.Add(pending[i]);
                    pending.RemoveAt(i);
                }
            }
            due.Reverse();
            return due;
        }

        public void Clear()
        {
            meetings.Clear();
            pending.Clear();
        }
    }
}
