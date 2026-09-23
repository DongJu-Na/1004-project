namespace Project1028.Encounter
{
    /// <summary>§6.2 C 유형 이지선다: 선택 또는 타임아웃(기본 옵션). 순수 로직.</summary>
    public sealed class ChoiceResolver
    {
        private readonly string defaultOption;
        private readonly float timeout;
        private float elapsed;

        public bool Resolved { get; private set; }
        public string Option { get; private set; }
        public bool ByTimeout { get; private set; }

        public ChoiceResolver(string defaultOption, float timeoutSeconds)
        {
            this.defaultOption = defaultOption == "A" ? "A" : "B";
            timeout = timeoutSeconds <= 0f ? float.PositiveInfinity : timeoutSeconds;
        }

        public bool Choose(string option)
        {
            if (Resolved || (option != "A" && option != "B")) return false;
            Resolved = true; Option = option; ByTimeout = false;
            return true;
        }

        /// <summary>타임아웃으로 해결되는 순간 true.</summary>
        public bool Tick(float dt)
        {
            if (Resolved) return false;
            elapsed += dt < 0f ? 0f : dt;
            if (elapsed < timeout) return false;
            Resolved = true; Option = defaultOption; ByTimeout = true;
            return true;
        }

        public void ForceDefault()
        {
            if (Resolved) return;
            Resolved = true; Option = defaultOption; ByTimeout = true;
        }
    }
}
