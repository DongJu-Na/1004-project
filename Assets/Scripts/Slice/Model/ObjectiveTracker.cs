using System.Collections.Generic;

namespace Project1028.Slice
{
    /// <summary>목표 단계 진행. 현재 단계의 조건만 전진시킨다. 순수 로직.</summary>
    public sealed class ObjectiveTracker
    {
        private readonly IReadOnlyList<ObjectiveStep> steps;

        public int Index { get; private set; }
        public bool IsComplete => Index >= steps.Count;
        public ObjectiveStep Current => IsComplete ? null : steps[Index];
        public int Count => steps.Count;

        public ObjectiveTracker(IReadOnlyList<ObjectiveStep> steps) { this.steps = steps ?? new List<ObjectiveStep>(); }

        public bool Satisfy(string condition)
        {
            if (IsComplete || string.IsNullOrEmpty(condition)) return false;
            if (steps[Index].condition != condition) return false;
            Index++;
            return true;
        }
    }
}
