namespace Project1028.Subdue
{
    /// <summary>§4.1 정면 밀치기 실패 확률 판정. 난수는 주입한다(테스트 가능).</summary>
    public static class ShoveRoll
    {
        public static bool Succeeds(float failChance, float roll01) => roll01 >= failChance;
    }
}
