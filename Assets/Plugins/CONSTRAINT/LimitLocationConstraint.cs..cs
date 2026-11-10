using UnityEngine;

[ExecuteAlways] // Membuat script berjalan di Scene View tanpa Play Mode
public class LimitLocationConstraint : MonoBehaviour
{
    public enum SpaceOwner { World, Local }

    [Header("Constraint Settings")]
    public bool affectTransform = true; 
    public SpaceOwner owner = SpaceOwner.Local;
    [Range(0f, 1f)] public float influence = 1f;

    [Header("Limit Bounds")]
    public bool limitX = true;
    public float minX = -1f;
    public float maxX = 1f;
    
    public bool limitY = true;
    public float minY = -1f;
    public float maxY = 1f;

    [Header("Current Output")]
    public Vector2 joystickPos;

    // Fungsi LateUpdate paling cocok untuk Constraint agar tidak bentrok dengan animasi
    void LateUpdate()
    {
        // Ambil posisi saat ini
        Vector3 currentPos = (owner == SpaceOwner.Local) ? transform.localPosition : transform.position;
        Vector3 limitedPos = currentPos;

        // Logika Batasan (Limit Location)
        if (limitX) limitedPos.x = Mathf.Clamp(currentPos.x, minX, maxX);
        if (limitY) limitedPos.y = Mathf.Clamp(currentPos.y, minY, maxY);

        // Terapkan Influence (Blending)
        Vector3 finalPos = Vector3.Lerp(currentPos, limitedPos, influence);

        // Update nilai untuk Joystick Visual
        joystickPos.x = finalPos.x;
        joystickPos.y = finalPos.y;

        // Terapkan ke Transform jika opsi aktif
        if (affectTransform)
        {
            if (owner == SpaceOwner.Local)
            {
                transform.localPosition = new Vector3(finalPos.x, finalPos.y, transform.localPosition.z);
            }
            else
            {
                transform.position = new Vector3(finalPos.x, finalPos.y, transform.position.z);
            }
        }
    }
}