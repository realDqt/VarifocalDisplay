using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Brick : MonoBehaviour
{
    public Vector3Int offset;

    public void UpdateOffset(Axis axis, bool cw)
    {
        switch (axis)
        {
            case Axis.None:
                break;
            case Axis.X:
                UpdateOffsetX(cw);
                break;
            case Axis.Y:
                UpdateOffsetY(cw);
                break;
            case Axis.Z:
                UpdateOffsetZ(cw);
                break;
            default:
                break;
        }
    }

    private void UpdateOffsetZ(bool cw)
    {
        if (offset.x == 0 && offset.y == 0) return;
        if (offset.x == 0)
        {
            if (cw)
            {
                offset.x += offset.y;
            }
            else
            {
                offset.x -= offset.y;
            }
            offset.y = 0;
        }
        else if (offset.y == 0)
        {
            if (cw)
            {
                offset.y -= offset.x;
            }
            else
            {
                offset.y += offset.x;
            }
            offset.x = 0;
        }
        else
        {
            if (cw)
            {
                if (offset.x == offset.y)
                {
                    offset.y = -offset.x;
                }
                else
                {
                    offset.x = offset.y;
                }
            }
            else
            {
                if (offset.x == offset.y)
                {
                    offset.x = -offset.y;
                }
                else
                {
                    offset.y = offset.x;
                }
            }
        }
    }

    private void UpdateOffsetY(bool cw)
    {
        if (offset.x == 0 && offset.z == 0) return;
        if (offset.x == 0)
        {
            if (cw)
            {
                offset.x += offset.z;
            }
            else
            {
                offset.x -= offset.z;
            }
            offset.z = 0;
        }
        else if (offset.z == 0)
        {
            if (cw)
            {
                offset.z -= offset.x;
            }
            else
            {
                offset.z += offset.x;
            }
            offset.x = 0;
        }
        else
        {
            if (cw)
            {
                if (offset.x == offset.z)
                {
                    offset.z = -offset.x;
                }
                else
                {
                    offset.x = offset.z;
                }
            }
            else
            {
                if (offset.x == offset.z)
                {
                    offset.x = -offset.z;
                }
                else
                {
                    offset.z = offset.x;
                }
            }
        }
    }

    private void UpdateOffsetX(bool cw)
    {
        if (offset.y == 0 && offset.z == 0) return;
        if (offset.y == 0)
        {
            if (cw)
            {
                offset.y -= offset.z;
            }
            else
            {
                offset.y += offset.z;
            }
            offset.z = 0;
        }
        else if (offset.z == 0)
        {
            if (cw)
            {
                offset.z += offset.y;
            }
            else
            {
                offset.z -= offset.y;
            }
            offset.y = 0;
        }
        else
        {
            if (cw)
            {
                if (offset.y == offset.z)
                {
                    offset.y = -offset.z;
                }
                else
                {
                    offset.z = offset.y;
                }
            }
            else
            {
                if (offset.y == offset.z)
                {
                    offset.z = -offset.y;
                }
                else
                {
                    offset.y = offset.z;
                }
            }
        }
    }
}
