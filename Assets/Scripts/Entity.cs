using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public byte id;
    public float Speed = 3f;
    public float ExpireTime = 3f;
    public int HitDamage = 17;
    private float currentTime = 0;
    public Vector3 targetDirection = Vector3.zero;
    public int casterID = -1;
    public Vector3 SelectThisForGODSAKE = Vector3.zero;

    void FixedUpdate()
    {
        if (currentTime > ExpireTime) Destroy(gameObject);
        else currentTime += Time.deltaTime;
        transform.Translate(targetDirection * Speed * Time.deltaTime);
    }
    public void SetCasterTo(byte id)
    {
        casterID = id;
    }
    public void RotateTo(Vector3 point)
    {
        float x = point.x - transform.position.x;
        float y = point.y - transform.position.y;
        float angle = -Mathf.Atan2(x, y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        transform.Rotate(SelectThisForGODSAKE);
    }
}
