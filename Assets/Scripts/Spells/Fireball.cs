using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : Entity
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only Red Player sends data to server.
        if (!LoginScreen.IAmRed) return;

        if (other.tag == "Player" && casterID != -1 && casterID != other.GetComponent<Player>().id)
            other.GetComponent<Player>().GotHitBy(LoginScreen.EntityTypes.Fireball, id); // Send server that someone got rekt by an entity in 2k19 LUL
        else if (other.tag == "Entity" && casterID != -1 && casterID != other.GetComponent<Entity>().casterID)
            LoginScreen.EntitiesHit(id, other.gameObject.GetComponent<Entity>().id); // Send server the entity-entity collision
    }
}
