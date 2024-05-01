using System;
using System.Collections;
using System.Collections.Generic;
using Tac;
using UnityEngine;
using UnityEngine.AI;


[Serializable]
public class UnitPose
{
	public GameObject Stand;
	public GameObject —rouched;
	public GameObject Lying;

	public GameObject WeaponPointStand;
	public GameObject WeaponPoint—rouched;
	public GameObject WeaponPointLying;

	public GameObject WeaponPoint;

	private NavMeshAgent agent;
	private StatusBar StatusBar;
	private Renderer renderer;
	private Color CurrenColor;

	public void Init(NavMeshAgent argAgent, StatusBar argStatusBar)
	{ 
		agent = argAgent;
		StatusBar = argStatusBar;
		WeaponPoint = WeaponPointStand;
		renderer = Stand.GetComponentInChildren<Renderer>();
	}

	public void SetStand()
	{
		—rouched.SetActive(false);
		Lying.SetActive(false);
		Stand.SetActive(true);
		WeaponPoint = WeaponPointStand;
		renderer = Stand.GetComponentInChildren<Renderer>();
		ChangeMaterial(CurrenColor);

		agent.radius = 0.25f;
		agent.height = 2f;
		StatusBar.transform.position.SetY(2f);
	}

	public void Set—rouched()
	{
		Lying.SetActive(false);
		Stand.SetActive(false);
		—rouched.SetActive(true);
		WeaponPoint = WeaponPoint—rouched;
		renderer = —rouched.GetComponentInChildren<Renderer>();
		ChangeMaterial(CurrenColor);

		agent.radius = 0.35f;
		agent.height = 1f;
		StatusBar.transform.position.SetY(1f);
	}

	public void SetLying()
	{
		—rouched.SetActive(false);
		Stand.SetActive(false);
		Lying.SetActive(true);
		WeaponPoint = WeaponPointLying;
		renderer = Lying.GetComponentInChildren<Renderer>();
		ChangeMaterial(CurrenColor);

		agent.radius = 0.25f;
		agent.height = 0.5f;
		StatusBar.transform.position.SetY(0.5f);
	}

	public void ChangeMaterial(Color argColor)
	{
		CurrenColor = argColor;
		if (renderer != null)
		{
			renderer.material.color = argColor;
		}
	}


}
