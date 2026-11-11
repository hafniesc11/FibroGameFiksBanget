using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Custom BaseInput yang mencegah Unity memblokir tombol WASD dan Spasi saat mengetik teks,
/// sekaligus mencegah terjadinya ArgumentException dari Input.GetButtonDown.
/// </summary>
public class PCQuizInput : BaseInput
{
    public override bool GetButtonDown(string buttonName)
    {
        // Cegah StandaloneInputModule memicu tombol navigasi / submit saat menekan spasi/enter
        return false;
    }

    public override float GetAxisRaw(string axisName)
    {
        // Cegah StandaloneInputModule mencegat tombol WASD sebagai tombol arah
        return 0f;
    }

    public override bool GetMouseButtonDown(int button)
    {
        return Input.GetMouseButtonDown(button);
    }

    public override bool GetMouseButtonUp(int button)
    {
        return Input.GetMouseButtonUp(button);
    }

    public override bool GetMouseButton(int button)
    {
        return Input.GetMouseButton(button);
    }

    public override Vector2 mousePosition
    {
        get { return Input.mousePosition; }
    }

    public override Vector2 mouseScrollDelta
    {
        get { return Input.mouseScrollDelta; }
    }

    public override string compositionString
    {
        get { return Input.compositionString; }
    }

    public override IMECompositionMode imeCompositionMode
    {
        get { return Input.imeCompositionMode; }
        set { Input.imeCompositionMode = value; }
    }
}
