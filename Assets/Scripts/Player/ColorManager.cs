using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorManager : MonoBehaviour
{
    [SerializeField] private Transform _transform;

    // Gets the shape colors depending on the block info, then sets it
    public void GetShapeColor(BlockTypes type)
    {
        switch (type)
        {
            case BlockTypes.l_shape:
                SetShapeColor(new Color(1, 0.7f, 0, 1));
                break;
            case BlockTypes.j_shape:
                SetShapeColor(Color.blue);
                break;
            case BlockTypes.z_shape:
                SetShapeColor(Color.red);
                break;
            case BlockTypes.s_shape:
                SetShapeColor(Color.green);
                break;
            case BlockTypes.i_shape:
                SetShapeColor(Color.cyan);
                break;
            case BlockTypes.t_shape:
                SetShapeColor(Color.magenta);
                break;
            case BlockTypes.o_shape:
                SetShapeColor(Color.yellow);
                break;
            default:
                break;

        }
    }

    private void SetShapeColor(Color color)
    {
        Transform newTransform = _transform;
        if (_transform == null) { newTransform = transform; }

        foreach (Transform child in newTransform)
        {
            if (child.TryGetComponent<SpriteRenderer>(out SpriteRenderer renderer))
            {
                renderer.color = color;
            }
            else if (child.TryGetComponent<Image>(out Image image))
            {
                image.color = color;
            }
        }
    }
}
