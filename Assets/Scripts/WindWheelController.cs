using UnityEngine;

public class WindWheelController : MonoBehaviour
{
    float[] childrenRotations;
    Transform[] children;

    void OnEnable()
    {
        if (children != null)
            return;

        childrenRotations = new float[5];
        children = new Transform[5];

        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i).GetChild(0);
            childrenRotations[i] = children[i].rotation.z;
        }
    }
    public void Rotate(float amount)
    {

        transform.rotation = Quaternion.Euler(0f, 0f, amount);

        for (int i = 0; i < transform.childCount; i++)
        {
            children[i].rotation = Quaternion.Euler(0f, 0f, childrenRotations[i]);
        }
    }
}
