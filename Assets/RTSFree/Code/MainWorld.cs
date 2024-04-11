using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.FilePathAttribute;
using UnityEngine.UIElements;

public class MainWorld : MonoBehaviour
{
	[HideInInspector] public Terrain Terrain;

	public int DebugUnit;

	void Awake()
	{
		Terrain = GetComponentInChildren<Terrain>();
	}

	public GameObject Create(GameObject argModel, Vector3 argPosition, Quaternion argRotation)
	{
		return Instantiate(argModel, argPosition, argRotation);
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

	public static GameObject Create(GameObject argModel, Vector3 argPosition, Quaternion argRotation) 
		=> MainWorld.Create(argModel, argPosition, argRotation);

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

}