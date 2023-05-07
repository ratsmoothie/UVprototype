using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyShooting : MonoBehaviour
{
    [Header("Enemy Weapon Stats")] //same stats as playerShooting
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

    public bool continuousFire;
    public bool readyToShoot = true;
    public bool shooting = false;
    public bool reloading;

    [Header("FX")]
    public GameObject muzzleFlash;
    public float muzzleFlashTime;
    public TrailRenderer bulletTrail;
    public ParticleSystem ImpactParticleSystem;
    //sounds
    private AudioSource gunshotSound;
    private AudioSource reloadSound;
    public AudioClip rifleGunshotSound; //rifle gunshot Sound Effect from <a href="https://pixabay.com/sound-effects/?utm_source=link-attribution&amp;utm_medium=referral&amp;utm_campaign=music&amp;utm_content=47258">Pixabay</a>
    public AudioClip rifleReloadSound; //reload Sound Effect from<a href="https://pixabay.com/?utm_source=link-attribution&amp;utm_medium=referral&amp;utm_campaign=music&amp;utm_content=92134"> Pixabay</a>

    [Header("References")]
    public EnemyAI EnemyAI;
    public Transform muzzle;
    public RaycastHit hitscan;
    public LayerMask Mask;
    public LayerMask isPlayer;
    public LayerMask isWall;
    private Transform playerTransform;
    private GameObject playerObject;
    private Transform enemyTransform;

    public bool targetVisible = false;
    public Vector3 lastSeenPosition;

    // Start is called before the first frame update
    void Start()
    {
        remainingAmmo = magazineCapacity;
        readyToShoot = true;

        reloadSound = gameObject.AddComponent<AudioSource>();
        reloadSound.clip = rifleReloadSound;

        gunshotSound = gameObject.AddComponent<AudioSource>();
        gunshotSound.clip = rifleGunshotSound;

        //find this enemy and get its transform
        enemyTransform = GetComponent<Transform>();

        //get bullet trail and assign it


        StartCoroutine(WaitForPlayerSpawn());

    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject != null)
        {
            shouldIReload();
            isTargetVisible();
        }      
    }

    IEnumerator WaitForPlayerSpawn()
    {
        while (playerObject == null)
        {
            yield return new WaitForSeconds(0.01f); // short wait

            // Find the player object
            playerObject = GameObject.Find("PlayerObj");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
                Debug.Log("Player object found!");
            }
            else
            {
                Debug.Log("Player object not found!");
            }
        }
    }

    public void shouldIReload()
    {
        if (remainingAmmo <= 0)
        {
            Reload();
        }
        /*if (reloading)
        {
            PlayReloadSound();
        }*/
    }

    public void isTargetVisible()
    {
        if (playerObject == null)
        {
            return;
        }
        RaycastHit wallCheck;
        if (Physics.Linecast(transform.position, playerTransform.position, out wallCheck, isWall))
        {
            targetVisible = false;
        }
        else
        {
            targetVisible = true;

            //Store the player's last known position
            lastSeenPosition = playerTransform.position;
        }
    }

    public void Shoot()
    {
        if (!shooting && targetVisible)
        {
            shooting = true;
            StartCoroutine(ShootWithDelay(rateOfFire));
        }      
    }

    public IEnumerator ShootWithDelay(float rateOfFire)
    {
        if (!readyToShoot) yield break;

        readyToShoot = false;

        //shooting
        //muzzle flash off the gun
        GameObject muzzleFlashInstance = Instantiate(muzzleFlash, muzzle.position, muzzle.rotation);
        Destroy(muzzleFlashInstance, muzzleFlashTime);

        //weapon spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        float z = Random.Range(-spread, spread);

        //spread direction
        Vector3 weaponSpread = new Vector3(x, y, z);

        // Calculate the direction towards the player
        Vector3 direction = playerTransform.position - muzzle.transform.position + weaponSpread;

        //Hitscan shooting, using a raycast
        if (Physics.Raycast(muzzle.transform.position, direction.normalized, out hitscan, range, Mask))
        {
            //Debug.Log(hitscan.collider.name);
            TrailRenderer trail = Instantiate(bulletTrail, muzzle.position, Quaternion.identity);

            StartCoroutine(SpawnBulletTrail(trail, hitscan));

            gunshotSound.PlayOneShot(rifleGunshotSound, 0.4f);

            hitscan.collider.gameObject.GetComponent<PlayerStats>()?.TakeDamage(damage);
            Debug.Log("Enemy dealt " + damage + " damage to " + hitscan.collider.name + ".");

        }

        remainingAmmo--;
        spentAmmo--;

        // Wait for the rate of fire delay before setting readyToShoot to true again.
        yield return new WaitForSeconds(rateOfFire);

        readyToShoot = true;
        shooting = false;

    }

    void OnDestroy()
    {
        shooting = false;
    }

    public void AttackPlayer()
    {

        if (remainingAmmo > 0 && !reloading)
        {          
            EnemyAI.readyToAttack = true;

            bulletsShot = projectilesPerShot;
            Shoot();

        }
        if (remainingAmmo <= 0)
        {
            EnemyAI.readyToAttack = false;           
        }
    }

    private void ResetReadyToShoot()
    {
        readyToShoot = true;
    }

    public void Reload()
    {
        if (reloading) return; // don't start reloading if already reloading

        reloading = true;
        reloadSound.PlayOneShot(rifleReloadSound, 0.4f);

        Invoke("ReloadOver", reloadTime);
    }

    public void PlayReloadSound()
    {
        //reloadSound.Play();       
    }

    private void ReloadOver()
    {
        remainingAmmo = magazineCapacity;
        reloading = false;
    }

    private IEnumerator SpawnBulletTrail(TrailRenderer Trail, RaycastHit Hit)
    {
        float time = 0;
        Vector3 startPosition = Trail.transform.position;

        while (time < 1)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, Hit.point, time);
            time += Time.deltaTime / Trail.time;

            yield return null;
        }
        //Animator.SetBool("shooting", false);
        Trail.transform.position = Hit.point;
        Instantiate(ImpactParticleSystem, Hit.point, Quaternion.LookRotation(Hit.normal));

        Destroy(Trail.gameObject, Trail.time);
    }
}
