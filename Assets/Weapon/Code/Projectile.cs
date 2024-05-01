// AliyerEdon@mail.com Christmas 2022
// Use this component to manage the projectiles or bullets items

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTSToolkitFree
{

    public class Projectile : MonoBehaviour
    {
        // Damage value to apply to the target after collision
        public int damageValue;

        // Instantiate the damage particle after collision
        public GameObject damageParticle;

        // Destroy the projectile after 5 seconds with out collision
        public float lifeTime = 5f;

        IEnumerator Start()
        {
            // Destroy the projectile after lifeTime value with out collision
            yield return new WaitForSeconds(lifeTime);
            Destroy(gameObject);
        }

        void OnCollisionEnter(Collision collision)
        {
            /*IAgentState health = collision.gameObject.GetComponent<IAgentState>();
            if (health != null ) 
            { 
                health.ApplyDamage(damageValue);
            }*/


            // Instantiate the collision particle
            if (damageParticle)
                Instantiate(damageParticle, transform.position, transform.rotation);

            // Destroy the projectile (or bullet)
            Destroy(gameObject);

        }
    }
}