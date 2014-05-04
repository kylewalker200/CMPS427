﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FrozenOrb : Ability
{
    public FrozenOrb(AttackType attackType, DamageType damageType, float range, float angle, float cooldown, float damageMod, float resourceCost, string id, string readable, GameObject particles)
        : base(attackType, damageType, range, angle, cooldown, damageMod, resourceCost, id, readable, particles)
    {

    }


    public override void SpawnProjectile(GameObject source, GameObject owner, Vector3 forward, string abilityID, bool isPlayer)
    {
        GameObject projectile = (GameObject)GameObject.Instantiate(particleSystem, source.transform.position, source.transform.rotation);


        projectile.GetComponent<ProjectileBehaviour>().owner = owner;
        projectile.GetComponent<ProjectileBehaviour>().timeToActivate = 4.0f;
        projectile.GetComponent<ProjectileBehaviour>().abilityID = abilityID;
        projectile.GetComponent<ProjectileBehaviour>().ExplodesOnTimeout = false;
        projectile.GetComponent<ProjectileBehaviour>().hasCollided = true;


        projectile.rigidbody.velocity = forward.normalized * 5.0f;


        int tempindex = 10;
        while (owner.GetComponent<Entity>().abilityManager.abilities[tempindex] != null && owner.GetComponent<Entity>().abilityManager.abilities[tempindex].ID != "icebolt")
        {
            tempindex++;
        }
        if (owner.GetComponent<Entity>().abilityManager.abilities[tempindex] == null)
        {
            owner.GetComponent<Entity>().abilityManager.AddAbility(GameManager.Abilities["icebolt"], tempindex);
            owner.GetComponent<Entity>().abilityIndexDict["icebolt"] = tempindex;

        }
        /*
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out rayCastTarget, Mathf.Infinity);
        vectorToMouse = rayCastTarget.point - SourceEntity.transform.position;
        forward = new Vector3(vectorToMouse.x, SourceEntity.transform.forward.y, vectorToMouse.z).normalized;
        */


        GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().RunCoroutine(launch(projectile, owner, tempindex, isPlayer));

    }


    public IEnumerator launch(GameObject source, GameObject owner, int tempindex, bool isplayer)
    {
        for (int i = 0; i < 10; i++)
        {

            Vector3 forward = new Vector3(Random.Range(-1.0f, 1.0f), owner.GetComponent<Entity>().transform.forward.y, Random.Range(-1.0f, 1.0f)).normalized;
            owner.GetComponent<Entity>().abilityManager.abilities[tempindex].SpawnProjectile(source, owner, forward, owner.GetComponent<Entity>().abilityManager.abilities[tempindex].ID, isplayer);
            //SpawnProjectile(SourceEntity.gameObject, rayCastTarget.point, SourceEntity.gameObject, forward, SourceEntity.abilityManager.abilities[tempindex].ID, true);
            yield return new WaitForSeconds(0.25f);
        }


        yield return null;
    }

    public override void AttackHandler(GameObject source, GameObject target, Entity attacker, bool isPlayer)
    {
        /*
        List<GameObject> attacked = OnAttack(target, isPlayer);

        if (isPlayer == true)
        {
            Debug.Log(attacked.Count);
            foreach (GameObject enemy in attacked)
            {
                if (enemy.GetComponent<AIController>().IsResetting() == false
                    && enemy.GetComponent<AIController>().IsDead() == false)
                {
                    Entity defender = enemy.GetComponent<Entity>();
                    DoDamage(source, enemy, attacker, defender, isPlayer);
                    DoPhysics(target, enemy);
                    if (enemy.GetComponent<AIController>().IsInCombat() == false)
                    {
                        enemy.GetComponent<AIController>().BeenAttacked(source);
                    }
                }
            }
        }

        else
        {
            foreach (GameObject enemy in attacked)
            {
                Entity defender = enemy.GetComponent<Entity>();
                DoDamage(source, enemy, attacker, defender, isPlayer);
                DoPhysics(target, enemy);

            }
        }
         * */
    }

    public override void DoDamage(GameObject source, GameObject target, Entity attacker, Entity defender, bool isPlayer)
    {
    }

    public override List<GameObject> OnAttack(GameObject source, bool isPlayer)
    {

        List<GameObject> enemiesToAttack = new List<GameObject>();

        Vector3 forward = new Vector3();

        // this is a player attack, forward attack vector will be based on cursor position
        if (isPlayer == true)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit target;
            Physics.Raycast(ray, out target, Mathf.Infinity);
            Vector3 vectorToMouse = target.point - source.transform.position;
            forward = new Vector3(vectorToMouse.x, source.transform.forward.y, vectorToMouse.z).normalized;
        }

        int enemyMask = LayerMask.NameToLayer("Enemy");
        int playerMask = LayerMask.NameToLayer("Player");

        Collider[] colliders;

        if (isPlayer == true)
        {
            colliders = Physics.OverlapSphere(source.transform.position, range, 1 << enemyMask);
        }

        else
        {
            colliders = Physics.OverlapSphere(source.transform.position, range, 1 << playerMask);
        }


        foreach (Collider collider in colliders)
        {
            //Debug.Log(collider.ToString());

            // create a vector from the possible enemy to the source.transform

            Vector3 enemyVector = collider.transform.position - source.transform.position;
            Vector3 enemyVector2 = source.transform.position - collider.transform.position;

            // this is an enemy attack, forward attack vector will be based on target position
            if (isPlayer == false)
            {
                forward = enemyVector;
            }

            // if the angle between the forward vector of the attacker and the enemy vector is less than the angle of attack, the enemy is within the attack angle
            if (Vector3.Angle(forward, enemyVector) < angle)
            {
                RaycastHit hit = new RaycastHit();



                if (isPlayer == true)
                {
                    // try to cast a ray from the enemy to the player
                    bool rayCastHit = Physics.Raycast(new Ray(collider.transform.position, enemyVector2), out hit, range);

                    if (!rayCastHit)
                    {

                    }
                    // if the ray hits, the enemy is in line of sight of the player, this is a successful attack hit
                    else
                    {
                        //if (hit.collider.gameObject.tag == "Player")
                        //{
                        Debug.DrawRay(collider.transform.position, enemyVector, Color.green, 0.5f);
                        Debug.DrawRay(collider.transform.position, enemyVector2, Color.red, 0.5f);
                        enemiesToAttack.Add(collider.gameObject);
                        //}
                    }
                }

                else
                {
                    // try to cast a ray from the player to the enemy
                    bool rayCastHit = Physics.Raycast(new Ray(collider.transform.position, enemyVector2), out hit, range);

                    if (!rayCastHit)
                    {

                    }
                    // if the ray hits, the player is in line of sight of the enemy, this is a successful attack hit
                    else
                    {
                        if (hit.collider.gameObject.tag == "Enemy")
                        {
                            //Debug.DrawRay(collider.transform.position, enemyVector, Color.green, 0.5f);
                            //Debug.DrawRay(collider.transform.position, enemyVector2, Color.red, 0.5f);
                            enemiesToAttack.Add(collider.gameObject);
                        }
                    }
                }
            }
        }
        enemiesToAttack.Add(source);
        return enemiesToAttack;
    }

    public override void DoPhysics(GameObject source, GameObject target)
    {
    }

}
