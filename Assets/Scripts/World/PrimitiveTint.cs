using UnityEngine;

namespace Project1028.PlayFoundation
{
    /// <summary>
    /// 프리미티브 색 구분용. 에디터에서 만든 비에셋 Material은 씬 저장 시 유실되므로 런타임 Awake에서 생성한다.
    /// 신규 에셋 없음 (헌장 원칙 II).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrimitiveTint : MonoBehaviour
    {
        [SerializeField] private Color color = Color.white;

        public void SetColor(Color c) => color = c;

        private void Awake()
        {
            var renderer = GetComponent<Renderer>();
            if (renderer == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null) return;
            var mat = new Material(shader) { name = $"Mat_{gameObject.name}" };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            renderer.material = mat;
        }
    }
}
