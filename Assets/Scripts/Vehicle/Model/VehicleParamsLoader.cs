using System;
using System.IO;
using UnityEngine;

namespace Project1028.Vehicle
{
    public static class VehicleParamsLoader
    {
        public static string Path => System.IO.Path.Combine(Application.streamingAssetsPath, "Vehicle", "vehicle_params.json");

        /// <summary>실패 시 기본값을 돌려주고 false (차량은 그래도 동작한다).</summary>
        public static bool TryLoad(out VehicleParams p, out VehicleValidationResult result)
        {
            result = new VehicleValidationResult();
            p = VehicleParams.Default;
            string path = Path;
            if (!File.Exists(path)) { result.Error($"차량 파라미터 파일 없음: {path} → 기본값 사용"); return false; }
            VehicleParams loaded;
            try { loaded = JsonUtility.FromJson<VehicleParams>(File.ReadAllText(path)); }
            catch (Exception ex) { result.Error($"차량 파라미터 파싱 실패: {ex.Message} → 기본값 사용"); return false; }
            result = VehicleParamsValidator.Validate(loaded);
            if (result.IsError) { result.Messages.Add("→ 기본값 사용"); return false; }
            p = loaded;
            return true;
        }
    }
}
