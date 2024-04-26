using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.FilePathAttribute;
using UnityEngine.UIElements;

using Tac;

public class MainWorld : MonoBehaviour
{
	[HideInInspector] public Terrain Terrain;

	public int DebugUnit;

	void Awake()
	{
		Terrain = GetComponentInChildren<Terrain>();


		//Id id = WeaponType.CombatKnife.Id();

	}

	public GameObject Create(GameObject argModel, Vector3 argPosition, Quaternion argRotation)
	{
		return Instantiate(argModel, argPosition, argRotation);
	}

	public GameObject Create(GameObject argModel, Transform argParent)
	{
		return Instantiate(argModel, argParent);
	}

}

public static class World
{

	private static MainWorld world;
	public static MainWorld MainWorld
	{
		get
		{
			if (world == null)
			{
				world = GameObject.Find("World").GetComponent<MainWorld>();
			}
			return world;
		}
	}

	public static Terrain Terrain
	{
		get { return MainWorld.Terrain; }
	}

	public static System.Random rnd = new System.Random();


	public static GameObject Create(GameObject argModel, Vector3 argPosition)
		=> MainWorld.Create(argModel, argPosition, Quaternion.identity);

	public static GameObject Create(GameObject argModel, Vector3 argPosition, Quaternion argRotation) 
		=> MainWorld.Create(argModel, argPosition, argRotation);

	public static GameObject Create(GameObject argModel, Transform argParent)
		=> MainWorld.Create(argModel, argParent);

	public static Vector3 GetTerrainPosition(Vector3 origin)
	{
		return new Vector3(origin.x, Terrain.SampleHeight(origin), origin.z);
	}

	private static int IdCount;
	public static int GetId()
	{
		IdCount++;
		return IdCount;
	}

	public static SpawnWeapon GetSpawnWeapon(int argNation)
	{
		SpawnWeapon ret = null;
		SpawnWeapon[] spawn = MainWorld.GetComponentsInChildren<SpawnWeapon>();
		foreach (SpawnWeapon item in spawn) 
		{
			if (item.Nation == argNation)
			{ 
				ret = item; break;
			}
		}
		return ret;
	}

}