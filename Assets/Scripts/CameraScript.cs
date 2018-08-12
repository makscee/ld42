using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public static CameraScript Instance;

    private void Awake()
    {
        Instance = this;
    }
}