using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{

    //Movement Variables

    //Other
    private CharacterController charControl;
    private Camera cam;
    private GameObject weaponHolder;
    private NetworkCharacter netChar;
    private Recoil recoil;
    private GUI Ammo;
    private GUIStyle ammoStyle;
    private GUI Health;
    private GUIStyle healthStyle;
    private bool paused = false;

    // Use this for initialization
    void Start()
    {

        Screen.lockCursor = true;
        addGuns();

        charControl = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        weaponHolder = GameObject.Find("weaponHolder");
        netChar = GetComponent<NetworkCharacter>();
        recoil = GetComponentInChildren<Recoil>();

        
            GUI.Label(new Rect(Screen.width - 100, Screen.height - 50, 100, 50), bulletCount + "/" + currentGun.getTotalBullets(), ammoStyle);

        ammoStyle = new GUIStyle();
        ammoStyle.fontStyle = FontStyle.Bold;
        ammoStyle.fontSize = 28;
        ammoStyle.normal.textColor = Color.white;
        

    }

    void OnGUI()
    {
        if (currentGun != null && !paused)
        {
           
        }
        
    }

    // Update is called once per frame
    void Update()
    {

        movement();
        if (Input.GetMouseButton(0) && timeRemaining <= 0 && currentGun != null && bulletCount != 0)
        {
            shooting();
        }
        else {
            timeRemaining -= 1 * Time.deltaTime;
            firing -= 1 * Time.deltaTime;
        }
        selectWeapon();
        if (currentGun != null)
        {
            if (Input.GetKeyDown(KeyCode.R) && (bulletCount < (currentGun.getMagSize() + 1)) && (!Input.GetMouseButton(0) || !Input.GetMouseButton(1)))
            {
                Debug.Log("Bullet count: " + bulletCount + " currentGun.getMagSize() + 1: " + (currentGun.getMagSize() + 1));
                reloadWeapon();
            }
        }

    }

    /*
	** 
	** Shooting Functions & Variables
	**
	*/
    private Ray ray;
    private float firing = 0;
    private float timeRemaining = 0;

    void shooting()
    {
        firing = Mathf.Clamp(firing, 0, 10);

        bulletCount--;
        timeRemaining = 120 / currentGun.getRpm();

        if (!Input.GetMouseButton(1) && firing != 0)
        {
            firing += 1;
            float randomRadius = Random.Range(0, firing / 10);
            float randomAngle = Random.Range(0, 2f * Mathf.PI);

            //Calculating the raycast direction
            Vector3 direction = new Vector3(randomRadius * Mathf.Cos(randomAngle), randomRadius * Mathf.Sin(randomAngle), 10f);
            direction = cam.transform.TransformDirection(direction.normalized);
            ray = new Ray(cam.transform.position, direction);
        }
        else {
            firing += 1;
            ray = new Ray(cam.transform.position, cam.transform.forward);
        }

        Transform hitInfo;
        Vector3 hitPoint;

        Fire();

        hitInfo = FindClosestHitInfo(ray, out hitPoint);

        PhotonNetwork.Instantiate("TestBullet", hitPoint, Quaternion.identity, 0);

        if (hitInfo != null)
        {
            Debug.Log("We hit: " + hitInfo.name);
            PlayerHealth h = hitInfo.GetComponent<PlayerHealth>();
            while (h == null && hitInfo.parent)
            {
                hitInfo = hitInfo.parent;
                h = hitInfo.GetComponent<PlayerHealth>();
            }
            if (h != null)
            {
                float damage = 50;
                h.GetComponent<PhotonView>().RPC("damage", PhotonTargets.All, damage);
            }
        }

    }

    IEnumerator Fire()
    {
        recoil.StartRecoil(currentGun.getRpm() / 120, (currentGun.getMagSize() / currentGun.getDamage()) * 10, 5f);
        // other firing code;
        return null;
    }

    Transform FindClosestHitInfo(Ray ray, out Vector3 hitPoint)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray);
        Transform closest = null;
        float distance = 0;
        hitPoint = Vector3.zero;
        foreach (RaycastHit Hit in hits)
        {
            if (Hit.transform != this.transform && (closest == null || Hit.distance < distance))
            {
                closest = Hit.transform;
                distance = Hit.distance;
                hitPoint = Hit.point;
            }
        }
        return closest;
    }

    /*
	**
	** Movement Functions & Variables
	**
	*/
    private float speedMult = 5.0f;
    private float sensMult = 5.0f;
    private float lookRange = 60.0f;
    private float jump = 5.0f;
    private float vertRotation = 0f;
    private float verticalVel = 0f;

    void movement()
    {
        float rotX = Input.GetAxis("Mouse X") * sensMult;
        float rotY = (Input.GetAxis("Mouse Y") * sensMult);

        transform.Rotate(0, rotX, 0);

        vertRotation -= rotY;
        vertRotation = Mathf.Clamp(vertRotation, -lookRange, lookRange);
        cam.transform.localRotation = Quaternion.Euler(vertRotation, 0, 0);
        weaponHolder.transform.localRotation = cam.transform.localRotation;

        float forwardSpeed = Input.GetAxis("Vertical") * speedMult;
        float strafeSpeed = Input.GetAxis("Horizontal") * speedMult;

        verticalVel += Physics.gravity.y * Time.deltaTime;

        if (charControl.isGrounded && Input.GetButtonDown("Jump"))
        {
            verticalVel = jump;
        }

        Vector3 speed = new Vector3(strafeSpeed, verticalVel, forwardSpeed);

        speed = transform.rotation * speed;

        charControl.Move(speed * Time.deltaTime);
    }

    /*
	**
	** Weapon Functions & Variables
	**
	*/
    private gunClass[] guns;
    private gunClass currentGun;
    private GameObject loadedModel;
    private static int Selected = -1;
    private static int bulletCount;

    private void addGuns()
    {
        guns = new gunClass[]{
            new gunClass("AK47", 850, 30, 44, 1000, 10),
            new gunClass("G36C", 780, 30, 38, 1000, 5),
            new gunClass("Vector", 1200, 25, 23, 200, 2),
            new gunClass("552", 690, 30, 47, 1000, 5)};
    }

    public static int getBulletCount()
    {
        return bulletCount;
    }

    void reloadWeapon()
    {

        Debug.Log("reloading");
        if ((bulletCount + ((currentGun.getMagSize() + 1) - bulletCount)) < currentGun.getTotalBullets() && bulletCount != 0)
        {
            currentGun.setTotalBullets(currentGun.getTotalBullets() - ((currentGun.getMagSize() + 1) - bulletCount));
            Debug.Log(currentGun.getTotalBullets());
            bulletCount = currentGun.getMagSize() + 1;
            Debug.Log(bulletCount);
        }else if ((bulletCount + ((currentGun.getMagSize() + 1) - bulletCount)) < currentGun.getTotalBullets() && bulletCount == 0)
        {
            currentGun.setTotalBullets(currentGun.getTotalBullets() - ((currentGun.getMagSize() + 1) - bulletCount));
            Debug.Log(currentGun.getTotalBullets());
            bulletCount = currentGun.getMagSize();
            Debug.Log(bulletCount);
        }
        else
        {
            if (currentGun.getTotalBullets() != 0)
            {
                bulletCount = currentGun.getTotalBullets();
                currentGun.setTotalBullets(0);
            }
        }
        timeRemaining = 3;
    }

    void selectWeapon()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            weapon(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            weapon(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            weapon(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            weapon(3);
        }
    }

    void weapon(int selected)
    {
        if (Selected != -1 && this.loadedModel != null)
        {
            PhotonNetwork.Destroy(this.loadedModel);
        }
        Selected = selected;
        currentGun = guns[Selected];
        loadedModel = (GameObject)PhotonNetwork.Instantiate(currentGun.getGun(), transform.FindChild("weaponHolder").position, transform.FindChild("weaponHolder").rotation, 0);
        loadedModel.transform.parent = transform.FindChild("weaponHolder").transform;
        bulletCount = (currentGun.getMagSize() + 1);
        netChar.loadedModel = loadedModel;
        Debug.Log("Recoil: " + recoil.name);
        Debug.Log("loadedModel: " + loadedModel);
        recoil.setObj(loadedModel);
        Debug.Log(recoil.getObj());
        //wr.setStartPoint();
    }


    [PunRPC]
    void setPlayerName(string name)
    {
        transform.name = name;
    }

}