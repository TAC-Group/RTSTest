using System.Collections;
using Tac;
using Unity.VisualScripting;
using UnityEngine;

namespace RTSToolkitFree
{
    public class SpawnPoint : MonoBehaviour
    {
        public GameObject objectToSpawn;
        public float TimeStep = 0.01f;
        public int numberOfObjects = 10000;
        public float size = 1.0f;

        public bool randomizeRotation = true;

        public int Nation;
        SpawnWeapon SpawnWeapon;
		DiscreteMap<Unit> map;

		void Start()
        {
            SpawnWeapon = World.GetSpawnWeapon(Nation);
            for (int i = 0; i < SpawnWeapon.Assortment.Count; i++)
            {
                SpawnWeapon.Assortment[i].LeftCount = SpawnWeapon.Assortment[i].MaxCount;
			}

			map = new DiscreteMap<Unit>(transform.position, new Vector2Int(20, 20), 0.5f);
			StartCoroutine(Spawn());
		}



		IEnumerator Spawn()
        {
            while(numberOfObjects > 0)
            {

                Vector2 randPos = Random.insideUnitCircle * size;

                Vector3 position = transform.position + new Vector3(randPos.x, 0f, randPos.y);

                position = Discrete.Get2D(position, 0.5f);

				Vector3? p = map.GetNearestEmpty(position);

                if (p != null)
                {
                    position = World.GetTerrainPosition(p.Value + map.Center);

                    GameObject instance = World.Create(objectToSpawn, position);

                    Unit unit = instance.GetComponent<Unit>();
                    if (unit != null)
                    {
                        map[p.Value.To2()] = unit;

                        if (unit.EnemyNation >= BattleSystem.active.numberNations)
                        {
                            BattleSystem.active.AddNation();
                        }

                        unit.Init();
                        unit.Id = World.GetId();
                        unit.Pose.ChangeMaterial(Color.white);

                        SpawnWeapon.AddWeapon(unit.WeaponPoint.transform);
                        BattleSystem.active.AddUnit(unit);
                    }
                }
                else
                {
                    int a = 1;
                }

				numberOfObjects--;
			    yield return new WaitForSeconds(TimeStep);
			}
		}

    }
}
