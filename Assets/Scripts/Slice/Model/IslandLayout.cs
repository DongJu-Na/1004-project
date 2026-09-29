using System;
using System.IO;
using UnityEngine;

namespace Project1028.Slice
{
    /// <summary>island_layout.json 모델(에디터·런타임 공용). 스키마: contracts/island-layout-schema.json</summary>
    [Serializable]
    public sealed class IslandLayout
    {
        public string version;
        public Facility[] facilities;
        public NpcPlacement[] npcs;
        public ZoneDef[] restrictedZones;
        public string dockFacilityId;
        public string warehouseFacilityId;
        public string evidenceFacilityId;

        public Facility Find(string id)
        {
            if (facilities == null || string.IsNullOrEmpty(id)) return null;
            foreach (var f in facilities) if (f != null && f.id == id) return f;
            return null;
        }
    }

    [Serializable] public sealed class LayoutVec3 { public float x; public float y; public float z; public Vector3 V => new Vector3(x, y, z); } // Encounter.Vec3와 이름 충돌 방지
    [Serializable] public sealed class Facility { public string id; public string kind; public LayoutVec3 position; public LayoutVec3 size; public float rotationY; public string label; }
    [Serializable] public sealed class NpcPlacement { public string npcId; public string displayName; public LayoutVec3 position; public LayoutVec3 facing; }
    [Serializable] public sealed class ZoneDef { public string id; public LayoutVec3 center; public LayoutVec3 size; }

    public static class IslandLayoutLoader
    {
        public static string Path => System.IO.Path.Combine(Application.streamingAssetsPath, "Slice", "island_layout.json");

        public static bool TryLoad(out IslandLayout layout, out string error)
        {
            layout = null; error = null;
            string path = Path;
            if (!File.Exists(path)) { error = $"섬 배치 파일 없음: {path}"; return false; }
            try { layout = JsonUtility.FromJson<IslandLayout>(File.ReadAllText(path)); }
            catch (Exception ex) { error = $"섬 배치 파싱 실패: {ex.Message}"; return false; }
            if (layout == null || layout.facilities == null || layout.facilities.Length == 0) { error = "facilities가 비어 있습니다."; return false; }
            if (layout.Find(layout.dockFacilityId) == null) { error = $"dockFacilityId '{layout.dockFacilityId}' 없음"; return false; }
            if (layout.Find(layout.warehouseFacilityId) == null) { error = $"warehouseFacilityId '{layout.warehouseFacilityId}' 없음"; return false; }
            if (layout.Find(layout.evidenceFacilityId) == null) { error = $"evidenceFacilityId '{layout.evidenceFacilityId}' 없음"; return false; }
            return true;
        }
    }
}
