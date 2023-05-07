using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//credit to Dave / GameDevelopment and Brackeys on youtube for their gun tutorials, camera shake, etc.
//their videos were used to create this.
public class playerShooting : MonoBehaviour
{
    [Header("Weapon Stats")]
    public int damage;
    public float rateOfFire;
    public float burstInterval; //burst guns
    public float spread;
    public float range;
    public float reloadTime;
    public float impactForce;
    public int bulletsShot;
    public int projectilesPerShot; //in case I want to make a shotgun or something
    public int magazineCapacity;
    public int remainingAmmo;
    private int spentAmmo;
    public int spareAmmo;
    public int maxAmmo;
    public int roundsShot;

    public bool continuousFire;
    public bool readyToShoot;
    public bool shooting;
    public bool reloading;

    [Header("Sights and Sounds")]
    // ADD CAMERA SHAKE HERE
    public GameObject muzzleFlash;
    public float muzzleFlashTime;
    //public GameObject bulletImpact;
    //public float bulletImpactTime;
    public TrailRenderer bulletTrail;
    public ParticleSystem ImpactParticleSystem;
    public float trailSpeed;

    //public Animator Animator;

    public AudioSource gunshotSound;
    public AudioSource reloadSound;

    //Sound Effect from <a href="https://pixabay.com/sound-effects/?utm_source=link-attribution&amp;utm_medium=referral&amp;utm_campaign=music&amp;utm_content=82365">Pixabay</a>
    //shooting sound
    public AudioClip handgunShot;
    //Sound Effect from<a href="https://pixabay.com/sound-effects/?utm_source=link-attribution&amp;utm_medium=referral&amp;utm_campaign=music&amp;utm_content=6248"> Pixabay</a>
    //reload sound
    //Sound Effect from <a href="https://pixabay.com/sound-effects/?utm_source=link-attribution&amp;utm_medium=referral&amp;utm_campaign=music&amp;utm_content=7132">Pixabay</a>
    //holster sound if I want it

    [Header("References")]
    public new PlayerCam camera;
    public Transform muzzle;
    public RaycastHit hitscan;
    public LayerMask Mask;
    public LayerMask isEnemy;

    private void playerInput()
    {
        //determines if the current weapon allows for multiple shots to be fired without additonal input
        if (continuousFire)
        {
            shooting = Input.GetKey(KeyCode.Mouse0);
        }
        else
        {
            shooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        //if it makes sense to reload allow it with a button press
        if(Input.GetKeyDown(KeyCode.R) && remainingAmmo < magazineCapacity && !reloading)
        {
            Reload();           
        }

        //shooting
        if(readyToShoot && shooting && !reloading && remainingAmmo > 0)
        {
            bulletsShot = projectilesPerShot;
            Shoot();

            roundsShot++;

            //PlayOneShot doesnt cut itself off like Play() did :)
            gunshotSound.PlayOneShot(handgunShot, 0.4f);
        }
            
    }

    private void Shoot()
    {
        readyToShoot = false;
        //Animator.SetBool("shooting", true);

        //weapon spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        float z = Random.Range(-spread, spread);


        //spread direction
        Vector3 spreadDirection = camera.transform.forward + new Vector3(x,y,z);

        //Hitscan shooting, using a raycast
        if (Physics.Raycast(muzzle.transform.position, spreadDirection, out hitscan, range, Mask))
        {
            Debug.Log("Shot hit: " + hitscan.collider.name);
            if (hitscan.collider.gameObject.TryGetComponent(out EnemyStats enemyStats))
            {
                Debug.Log("You dealt " + damage + " damage to " + hitscan.collider.name + ".");
            }
            TrailRenderer trail = Instantiate(bulletTrail, muzzle.position, Quaternion.identity);

            StartCoroutine(SpawnBulletTrail(trail, hitscan, trailSpeed));
                /*
                if(hitscan.rigidbody != null)
                {
                hitscan.rigidbody.AddForce(-hitscan.normal * impactForce);
                }*/

            //deal damage if it has a health component
            hitscan.collider.gameObject.GetComponent<EnemyStats>()?.TakeDamage(damage);
        }

        GameObject muzzleFlashInstance = Instantiate(muzzleFlash, muzzle.position, muzzle.rotation);
        Destroy(muzzleFlashInstance, muzzleFlashTime);

        remainingAmmo--;
        spentAmmo--;

        //using the Invoke function to repeat shots at a set delay, in this case our rate of fire
        Invoke("ResetReadyToShoot", rateOfFire);

        if(remainingAmmo > 0 && spentAmmo > 0)
        {
           Invoke("Shoot", burstInterval);
        }

     //call camera shake function
        
    }

    private void ResetReadyToShoot()
    {
        readyToShoot = true;
    }

    private void Reload()
    {
        if (spareAmmo > 0)
        {
            reloadSound.Play();
            reloading = true;
            Invoke("ReloadOver", reloadTime);
        }
        else
        {
            return;
        }
    }

    private void ReloadOver()
    {
        if (spareAmmo >= roundsShot)
        {           
            remainingAmmo += roundsShot;
            spareAmmo -= roundsShot;
        }
        if (spareAmmo < magazineCapacity)
        {
            remainingAmmo += spareAmmo;
            spareAmmo = 0;
        }
        //stops overcapped mags
        if(remainingAmmo > magazineCapacity)
        {
            remainingAmmo = magazineCapacity;
        }
        //Stops Negative Ammo Count
        if(spareAmmo < 0)
        {
            spareAmmo = 0;
        }

        roundsShot = 0;
        reloading = false;
    }

    private IEnumerator SpawnBulletTrail(TrailRenderer Trail, RaycastHit Hit, float speed)
    {
        Vector3 startPosition = Trail.transform.position;
        Vector3 endPosition = Hit.point;
        float distance = Vector3.Distance(startPosition, endPosition);
        float time = distance / speed;
        float currentTime = 0f;

        while (currentTime < time)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, endPosition, currentTime / time);
            currentTime += Time.deltaTime;
            yield return null;
        }
        //Animator.SetBool("shooting", false);
        Trail.transform.position = Hit.point;
        Instantiate(ImpactParticleSystem, Hit.point, Quaternion.LookRotation(Hit.normal));

        Destroy(Trail.gameObject, Trail.time);
    }

    // Start is called before the first frame update
    void Start()
    {
        spareAmmo = maxAmmo;
        remainingAmmo = magazineCapacity;
        readyToShoot = true;

        //Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        playerInput();
    }
}
