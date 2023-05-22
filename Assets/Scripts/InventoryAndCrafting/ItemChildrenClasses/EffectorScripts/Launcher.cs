using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    public GameObject testBulletPrefab;
    public Transform initialTransform;
    public Transform angleTransform;
    public float ammoThrust = 0;
    public bool isCatapult = false;

    [Header("Catapult Vars")]
    public int gizmoIterations;
    public List<Vector3> gizmoSpheres = new List<Vector3>();
    public Vector3 initialVelocity;
    public bool hasVelocity = false;
    private GameObject testBullet;
    private float previousAmmoThrust = 0;
    [HideInInspector] public float originalAmmoThrust = 0;
    private Vector3 previousAngleTransformPosition;

    private void Awake()
    {
        previousAmmoThrust = ammoThrust;
        originalAmmoThrust = ammoThrust;
        previousAngleTransformPosition = angleTransform.position;

        testBullet = Instantiate(testBulletPrefab);
        testBullet.transform.position = testBullet.transform.position - new Vector3(0, 1000, 0);
        testBullet.GetComponent<MeshRenderer>().enabled = false;
        testBullet.transform.parent = this.gameObject.transform;
    }

    private void OnDrawGizmos()
    {
        if (isCatapult)
        {
            Gizmos.color = Color.magenta;
            gizmoSpheres.Clear();
            if (hasVelocity && initialVelocity != Vector3.zero)
            {
                Vector3 trajectoryPoint;
                float t = (-1f * initialVelocity.y) / Physics.gravity.y;
                t = 2f * t;
                for (int i = 0; i < gizmoIterations; i++)
                {
                    float time = t * i / (float)gizmoIterations;
                    trajectoryPoint = initialTransform.position + initialVelocity * time + 0.5f * Physics.gravity * time * time;
                    gizmoSpheres.Add(trajectoryPoint);
                }

                for (int i = 0; i < gizmoSpheres.Count; i++)
                {
                    Gizmos.DrawSphere(gizmoSpheres[i], .2f);
                }
            }
        }
        else
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(initialTransform.position, initialTransform.position + (angleTransform.position - initialTransform.position).normalized * ammoThrust);
        }
    }

    void GetInitialVelocity()
    {
        if ((previousAmmoThrust != ammoThrust && hasVelocity && initialVelocity != Vector3.zero)
            || (previousAngleTransformPosition != angleTransform.position && hasVelocity && initialVelocity != Vector3.zero))
        {
            hasVelocity = false;
            testBullet.transform.position = Vector3.zero;
            testBullet.transform.rotation = Quaternion.identity;
            testBullet.SetActive(true);
            testBullet.GetComponent<Rigidbody>().velocity = Vector3.zero;
            testBullet.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            testBullet.GetComponent<Rigidbody>().isKinematic = true;
            testBullet.GetComponent<Rigidbody>().isKinematic = false;
            initialVelocity = Vector3.zero;

            previousAmmoThrust = ammoThrust;
            previousAngleTransformPosition = angleTransform.position;
            return;
        }

        if (!hasVelocity)
        {
            testBullet.GetComponent<MeshRenderer>().enabled = false;
            Vector3 finalDir = (angleTransform.position - initialTransform.position).normalized;
            testBullet.GetComponent<Rigidbody>().AddForce(finalDir * ammoThrust);
            hasVelocity = true;
        }

        if (hasVelocity && initialVelocity == Vector3.zero)
        {
            initialVelocity = testBullet.GetComponent<Rigidbody>().velocity;
        }
        else if (hasVelocity && initialVelocity != Vector3.zero && testBullet.activeSelf)
        {
            testBullet.GetComponent<Rigidbody>().velocity = Vector3.zero;
            testBullet.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            testBullet.GetComponent<Rigidbody>().isKinematic = true;
            testBullet.GetComponent<Rigidbody>().isKinematic = false;
            testBullet.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        GetInitialVelocity();
    }

    public void Shoot(ObjectPooler.Key ammoObjectPoolerKey)
    {
        GameObject bullet = ObjectPooler.GetPooler(ammoObjectPoolerKey).GetPooledObject();
        if (bullet == null)
        {
            return;
        }
        
        bullet.transform.position = initialTransform.position;
        bullet.GetComponent<Rigidbody>().velocity = Vector3.zero;
        bullet.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        bullet.GetComponent<Rigidbody>().isKinematic = true;
        bullet.GetComponent<Rigidbody>().isKinematic = false;
        bullet.SetActive(true);

        Vector3 finalDir = (angleTransform.position - initialTransform.position).normalized;
        bullet.GetComponent<Rigidbody>().AddForce(finalDir * ammoThrust);
    }

    public void Shoot(GameObject objectToShoot)
    {
        GameObject bullet = objectToShoot;
        if (bullet == null)
        {
            return;
        }

        bullet.transform.position = initialTransform.position;
        bullet.GetComponent<Rigidbody>().velocity = Vector3.zero;
        bullet.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        bullet.GetComponent<Rigidbody>().isKinematic = true;
        bullet.GetComponent<Rigidbody>().isKinematic = false;
        bullet.SetActive(true);

        Vector3 finalDir = (angleTransform.position - initialTransform.position).normalized;
        bullet.GetComponent<Rigidbody>().AddForce(finalDir * ammoThrust);
    }
}
