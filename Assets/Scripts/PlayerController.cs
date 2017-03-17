using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{

    //Movement Variables

    //Other
    private CharacterController charControl;
    private PlayerHealth playerHealth;
    private Camera cam;
    private GameObject weaponHolder;
    private NetworkCharacter netChar;
    private Recoil recoil;
    private float DamageTime = 2f;

    //GUI Stuff
    public Text Ammo;
    public bool tookDamage = false;
    public Text HealthText;
    public Image HealthBar;
    public Image HealthBarCenter;

    //Paused Menu (eventually)
    private bool paused = false;

    // Use this for initialization
    void Start()
    {

        Cursor.lockState = CursorLockMode.Locked;
        addGuns();

        charControl = GetComponent<CharacterController>();
        playerHealth = GetComponent<PlayerHealth>();
        cam = transform.FindChild("RecoilHolder").transform.FindChild("PlayerCamera").GetComponent<Camera>();
        weaponHolder = transform.FindChild("weaponHolder").gameObject;
        netChar = GetComponent<NetworkCharacter>();
        recoil = GetComponentInChildren<Recoil>();
        Ammo.enabled = false;

    }

    void OnGUI()
    {
        if (!paused && !playerHealth.getDead()) {
            if (currentGun != null && Ammo != null)
            {
                Ammo.enabled = true;
                Ammo.text = bulletCount + "/" + currentGun.getTotalBullets();
            }

            if (HealthText != null && HealthBar != null) {
                HealthText.text = ((int)playerHealth.getHealth()).ToString();
                HealthBar.fillAmount = playerHealth.getHealth();
            }

            if (tookDamage)
            {
                HealthBarCenter.color = Color.red;
                DamageTime -= 1 * Time.deltaTime;
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {

        if(DamageTime <= 0 && tookDamage)
        {
            tookDamage = false;
            DamageTime = 2f;
            HealthBarCenter.color = Color.white;
        }
        movement();
        if (Input.GetMouseButton(0) && timeRemaining <= 0 && currentGun != null && bulletCount != 0)
        {
            shooting();
        }
        else {
            timeRemaining -= 1 * Time.deltaTime;
            firing -= 1 * Time.deltaTime;
            singleShotTime -= Time.deltaTime;
            if (hasFired && singleShotTime <= 0)
            {
                hasFired = false;
                singleShotTime = 0.5f;
            }
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
    public bool hasFired = false;
    public float singleShotTime = 0.5f;
    private float timeRemaining = 0;
    private float damage = 0;

    void shooting()
    {
        firing = Mathf.Clamp(firing, 0, currentGun.getMagSize() + 1);

        bulletCount--;
        timeRemaining = 120 / currentGun.getRpm();

        if (!Input.GetMouseButton(1) && hasFired)
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
            hasFired = true;
            firing += 1;
            ray = new Ray(cam.transform.position, cam.transform.forward);
        }

        Transform hitInfo;
        Vector3 hitPoint;
        float distance;

        Fire();

        hitInfo = FindClosestHitInfo(ray, out hitPoint, out distance);

        PhotonNetwork.Instantiate("TestBullet", hitPoint, Quaternion.identity, 0);

        if (hitInfo != null)
        {
            Debug.Log("We hit: " + hitInfo.name);
            if (distance < currentGun.getRange())
            {
                damage = currentGun.getDamage();
            }
            else
            {
                damage = currentGun.getDamage() * (1 - ((distance - currentGun.getRange()) / 100));
            }
            Debug.Log("Damage: " + damage + " Distance: " + distance + " Weapon Starting Damage: " + currentGun.getDamage() + " Multiplied by: " + (1 - ((distance - currentGun.getRange()) / 100)));
            PlayerHealth h = hitInfo.GetComponent<PlayerHealth>();
            while (h == null && hitInfo.parent)
            {
                hitInfo = hitInfo.parent;
                h = hitInfo.GetComponent<PlayerHealth>();
            }
            if (h != null)
            {
                Debug.Log(hitInfo.name);
                Debug.Log(h);
                h.GetComponent<PhotonView>().RPC("damage", PhotonTargets.AllBuffered, damage);
            }
        }

    }

    IEnumerator Fire()
    {
        recoil.StartRecoil(currentGun.getRpm() / 120, (currentGun.getMagSize() / currentGun.getDamage()) * 10, 5f);
        // other firing code;
        return null;
    }

    Transform FindClosestHitInfo(Ray ray, out Vector3 hitPoint, out float distance)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray);
        Transform closest = null;
        distance = 0;
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
            new gunClass("AK47", 850, 30, 44, 50, 10),
            new gunClass("G36C", 780, 30, 38, 50, 5),
            new gunClass("Vector", 1200, 25, 23, 20, 2),
            new gunClass("552", 690, 30, 47, 50, 5)};
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
        Selected = selected;
        currentGun = guns[Selected];
        int id = GetComponent<PhotonView>().viewID;
        netChar.setParent(currentGun.getGun(), id);
        bulletCount = (currentGun.getMagSize() + 1);
        netChar.loadedModel = loadedModel;
        recoil.setObj(loadedModel);
        //wr.setStartPoint();
    }

    [PunRPC]
    void setWeaponParent(string Name, int id)
    {
        Destroy(loadedModel);
        GameObject parent = PhotonView.Find(id).gameObject;
        Debug.Log("THIS IS BEING CALLED RIGHT");
        Debug.Log(parent.name);
        loadedModel = (GameObject)Instantiate(Resources.Load(Name), Vector3.zero, Quaternion.identity);
        loadedModel.transform.parent = parent.transform.FindChild("weaponHolder").transform;
        loadedModel.transform.position = loadedModel.transform.parent.transform.position;
        loadedModel.transform.rotation = loadedModel.transform.parent.transform.rotation;
    }

    public GameObject getLoadedModel()
    {
        return loadedModel;
    }
    public void setLoadedModel(GameObject model)
    {
        loadedModel = model;
    }

    [PunRPC]
    void setPlayerName(string name)
    {
        transform.name = name;
    }

}