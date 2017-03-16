using UnityEngine;
using System.Collections;

public class Recoil : MonoBehaviour
{
    private bool once = false;
    private static float recoil = 0.0f;
    private static float maxRecoil_x = -10f;
    private static float maxRecoil_y = 4f;
    private static float recoilSpeed = 2f;
    private Vector3 startPoint;
    private GameObject obj;

    public void start()
    {
    }

    public void StartRecoil(float recoilParam, float maxRecoil_xParam, float recoilSpeedParam)
    {
        // in seconds
        recoil = recoilParam;
        maxRecoil_x = maxRecoil_xParam;
        recoilSpeed = recoilSpeedParam;
        maxRecoil_y = Random.Range(-3, 3);
    }

    void recoiling()
    {
        if (Input.GetMouseButton(1) && Input.GetMouseButton(0) && PlayerController.getBulletCount() != 0)
        {
            if (recoil > 0f)
            {
                Quaternion maxRecoil = Quaternion.Euler(-(maxRecoil_x + Input.GetAxis("Mouse X")), (maxRecoil_y + Input.GetAxis("Mouse Y")), 0f);

                // Dampen towards the target rotation
                transform.localRotation = Quaternion.Slerp(transform.localRotation, maxRecoil, Time.deltaTime * recoilSpeed);

                // Dampen towards the target rotation
                obj.transform.localRotation = Quaternion.Slerp(transform.localRotation, maxRecoil, Time.deltaTime * recoilSpeed);
                obj.transform.localPosition = Vector3.Lerp(new Vector3(0, 0, Mathf.Clamp(obj.transform.localPosition.z, -0.2f, 0f)), -(new Vector3(0, 0, 5f)), Time.deltaTime * 2f);

                recoil -= Time.deltaTime;

            }
            else {
                recoil = 0f;
                // Dampen towards the target rotation
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime * recoilSpeed / 2);

                obj.transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime * recoilSpeed / 2);
                obj.transform.localPosition = Vector3.Lerp(startPoint, new Vector3(startPoint.x, startPoint.y, obj.transform.localPosition.z), Time.deltaTime * 2f);
            }
        }
        else if (!Input.GetMouseButton(0))
        {
            recoil = 0f;
            // Dampen towards the target rotation
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime * recoilSpeed / 2);
            obj.transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime * recoilSpeed / 2);
            obj.transform.localPosition = Vector3.Lerp(startPoint, new Vector3(startPoint.x, startPoint.y, obj.transform.localPosition.z), Time.deltaTime * 2f);
        }else if (PlayerController.getBulletCount() > 0)
        {
            if (recoil > 0f)
            {
                obj.transform.localPosition = Vector3.Lerp(new Vector3(0, 0, Mathf.Clamp(obj.transform.localPosition.z, -0.2f, 0f)), -(new Vector3(0, 0, 5f)), Time.deltaTime * 2f);
                recoil -= Time.deltaTime;
            }
            else
            {
                obj.transform.localPosition = Vector3.Lerp(startPoint, new Vector3(startPoint.x, startPoint.y, obj.transform.localPosition.z), Time.deltaTime * 2f);
            }
        }
        else
        {
            recoil = 0f;
            // Dampen towards the target rotation
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime * recoilSpeed / 2);
            obj.transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime * recoilSpeed / 2);
            obj.transform.localPosition = Vector3.Lerp(startPoint, new Vector3(startPoint.x, startPoint.y, obj.transform.localPosition.z), Time.deltaTime * 2f);
        }
    }

    public void setObj(GameObject Obj)
    {
        obj = Obj;
        startPoint = obj.transform.localPosition;

    }

    public GameObject getObj()
    {
       return obj;
    }

    // Update is called once per frame
    void Update()
    {
        if (obj != null)
        {
            if (!once)
            {
                start();
                once = true;
            }
            recoiling();
        }
    }
}