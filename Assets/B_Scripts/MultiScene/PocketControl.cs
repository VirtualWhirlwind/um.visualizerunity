using UnityEngine;
using System.Collections;

public class PocketControl : MonoBehaviour
{
    #region Fields
    protected float _TargetScale = 1f;
    protected float _Size = 1f;
    #endregion

    #region Properties
    public float TargetScale
    {
        get
        {
            return _TargetScale;
        }
        set
        {
            if (_TargetScale != 0f)
            {
                _TargetScale = value >= 0f ? value : 1f;
            }
        }
    }

    public float Size
    {
        get
        {
            return _Size;
        }
        set
        {
            _Size = value >= 0f ? value : 1f;
        }
    }

    protected Transform trans { get; set; }
    #endregion

    #region Unity Events
    void Start()
    {
        trans = transform;

        trans.localScale = new Vector3(0f, 0f, 0f);
    }

    void Update()
    {
        if (trans.localScale.x != TargetScale)
        {
            if (Mathf.Abs(trans.localScale.x - TargetScale) < 0.025f) // Jump right to target
            {
                trans.localScale = new Vector3(TargetScale, TargetScale, TargetScale);

                if (TargetScale == 0f)
                {
                    Destroy(gameObject);
                }
            }
            else // Get closer
            {
                float NewScale = Mathf.Lerp(trans.localScale.x, TargetScale, Time.deltaTime);
                trans.localScale = new Vector3(NewScale, NewScale, NewScale);
            }
        }
    }
    #endregion
}
