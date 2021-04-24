using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class AnimatedTexture : MonoBehaviour
{
    [SerializeField]
    Vector2 direction = Vector2.down;

    Vector2 offset = Vector2.zero;
    Vector2 scale;

    Renderer renderer;
    MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        scale = renderer.material.mainTextureScale;
        propertyBlock = new MaterialPropertyBlock();
    }

    private void LateUpdate()
    {
        offset += direction * GameManager.Instance.MoveSpeed * Time.deltaTime;

        renderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetVector("_MainTex_ST", new Vector4(
            scale.x,
            scale.y,
            offset.x,
            offset.y
        ));
        renderer.SetPropertyBlock(propertyBlock);
    }
}
