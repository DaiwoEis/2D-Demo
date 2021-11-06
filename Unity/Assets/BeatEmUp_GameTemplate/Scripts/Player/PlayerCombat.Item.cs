using UnityEngine;

public partial class PlayerCombat
{
	// item
	public Transform weaponBone; //the bone were weapon will be parented on
	[HideInInspector] public Item currentWeapon; //the current weapon the player is holding
	[HideInInspector] public GameObject itemInRange; //an item that is currently in interactable range

	//starts the throw weapon attack
	void StartThrowAttack()
	{
		playerState.SetState(PLAYERSTATE.THROWKNIFE);
		animator.Throw();
		Invoke("ThrowKnife", .08f);
		Destroy(weaponBone.GetChild(0).gameObject);
	}

	//spawns a throwing knife projectile
	public void ThrowKnife()
	{
		GameObject knife = GameObject.Instantiate(Resources.Load("ThrowingKnife")) as GameObject;
		int dir = (int)GetComponent<PlayerMovement>().getCurrentDirection();
		knife.transform.position = transform.position + Vector3.up * 1.5f + Vector3.right * dir * .7f;
		knife.GetComponent<ThrowingKnife>().ThrowKnife(dir);
		resetWeapon();
	}

	//equips a weapon
	public void EquipWeapon(Item weapon)
	{
		currentWeapon = weapon;

		if (weapon.itemName == "Knife")
		{
			GameObject knife = GameObject.Instantiate(Resources.Load("KnifeHandWeapon"), weaponBone.position, Quaternion.identity) as GameObject;
			knife.transform.parent = weaponBone;
			knife.transform.localPosition = Vector3.zero;
			knife.transform.localRotation = Quaternion.identity;
		}
	}

	//resetWeapon
	public void resetWeapon()
	{
		currentWeapon = null;
	}

	//interact with an item in range
	public void InteractWithItem()
	{
		if (itemInRange != null)
		{
			Item item = itemInRange.GetComponent<ItemInteractable>().item;
			if (item != null && item.isPickup)
			{
				itemInRange.GetComponent<ItemInteractable>().ActivateItem(gameObject);
				animator.PickUpItem();
				playerState.SetState(PLAYERSTATE.PICKUPITEM);
			}
		}
	}
}
