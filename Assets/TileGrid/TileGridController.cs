using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TileGrid
{
    [RequireComponent(typeof(TilePopulator))]
    public class TileGridController : MonoBehaviour
    {
        public GameObject TilePrefab;
        private MeshRenderer _renderer;
        private TilePopulator _populator;
        private HashSet<CubeCoord> _edgeSet = new HashSet<CubeCoord>();
        private Dictionary<CubeCoord, GameObject> _knownTiles = new Dictionary<CubeCoord, GameObject>();
    
        void Start()
        {
            _renderer = TilePrefab.GetComponentInChildren<MeshRenderer>();
            _populator = GetComponent<TilePopulator>();
            spawnOrigin();
            StartCoroutine(SpawnEdgeTiles());
        }

        IEnumerator SpawnEdgeTiles()
        {
            for (var i = 0; i < 10; ++i)
            {
                yield return StartCoroutine(spawnTilesAroundEdge());
            }
        }

        private void spawnOrigin()
        {
            spawnTile(CubeCoord.ORIGIN);
            _edgeSet.Add(CubeCoord.ORIGIN);
        }

        public List<GameObject> GetNeighborTiles(CubeCoord coord)
        {
            return CubeCoord.Neighbors.Select(c => _knownTiles[c + coord]).ToList();
        }

        IEnumerator spawnTilesAroundEdge()
        {
            var newEdge = new HashSet<CubeCoord>();
            foreach (var cubeCoord in _edgeSet.SelectMany(edgeCoord => CubeCoord.Neighbors.Select(offset => edgeCoord + offset).Where(newCoord => !_knownTiles.ContainsKey(newCoord))).Distinct().OrderBy(e => Random.value))
            {
                yield return new WaitForSeconds(0.05f);
                spawnAndPopulateTile(cubeCoord);
                newEdge.Add(cubeCoord);
            }

            _edgeSet = newEdge;
        }

        private GameObject spawnTile(CubeCoord pos)
        {
            var objectScale = TilePrefab.transform.localScale;
            var tileSize = _renderer.bounds.size;
            tileSize.Scale(objectScale);
            
            var instance = Instantiate(TilePrefab, transform, true);
            instance.name = $"{pos}";
            var worldPos = pos.ToWorld(0, tileSize);
            instance.transform.position = worldPos;
            _knownTiles[pos] = instance;
            return instance;
        }

        private GameObject spawnAndPopulateTile(CubeCoord pos)
        {
            var go = spawnTile(pos);
            _populator.Populate(pos, go);
            return go;
        }
    }
}
