using UnityEngine;

namespace Project1028.PlayFoundation
{
    /// <summary>씬 내 목적지 마커. 프리미티브(Cylinder + Sphere)만 사용한다 (헌장 원칙 II).</summary>
    public sealed class DestinationMarker : MonoBehaviour
    {
        public Destination Destination { get; private set; }

        public static DestinationMarker Spawn(Destination destination)
        {
            var root = new GameObject($"Marker_{destination.Name}_{destination.Receiver.Id}");
            root.transform.position = destination.Position;

            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Pillar";
            pillar.transform.SetParent(root.transform, false);
            pillar.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            pillar.transform.localScale = new Vector3(0.6f, 0.75f, 0.6f);
            Object.Destroy(pillar.GetComponent<Collider>());
            Tint(pillar, new Color(0.2f, 0.9f, 0.4f));

            var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "Orb";
            orb.transform.SetParent(root.transform, false);
            orb.transform.localPosition = new Vector3(0f, 1.9f, 0f);
            orb.transform.localScale = Vector3.one * 0.5f;
            Object.Destroy(orb.GetComponent<Collider>());
            Tint(orb, new Color(1f, 0.95f, 0.3f));

            var marker = root.AddComponent<DestinationMarker>();
            marker.Destination = destination;
            destination.Marker = marker;
            return marker;
        }

        private void Update()
        {
            var orb = transform.Find("Orb");
            if (orb != null)
            {
                orb.localPosition = new Vector3(0f, 1.9f + Mathf.Sin(Time.time * 2f) * 0.15f, 0f);
            }
        }

        private static void Tint(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null) return;
            var mat = new Material(renderer.sharedMaterial);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            renderer.material = mat;
        }
    }
}
