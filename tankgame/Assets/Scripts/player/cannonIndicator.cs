using UnityEngine;

public class cannonIndicatorUpdater : MonoBehaviour
{
    public Transform cannonIndicator;
    public LayerMask layerMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, 200f, layerMask))
        {
            cannonIndicator.position = hit.point;
        }
        else
        {
            cannonIndicator.position = origin + direction * 1000f;
        }
    }
}
