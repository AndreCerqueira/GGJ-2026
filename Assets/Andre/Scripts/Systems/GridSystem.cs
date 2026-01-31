using System.Collections.Generic;
using UnityEngine;

namespace Andre.Scripts.Systems
{
    public class GridSystem : MonoBehaviour
    {
        public static GridSystem Instance { get; private set; }

        private Dictionary<Vector2Int, AreaView> _grid = new Dictionary<Vector2Int, AreaView>();

        private void Awake()
        {
            Instance = this;
        }

        public void RegisterArea(Vector2Int coord, AreaView area)
        {
            if (!_grid.ContainsKey(coord))
            {
                _grid.Add(coord, area);
            }
        }

        public bool TryGetArea(Vector2Int coord, out AreaView area)
        {
            return _grid.TryGetValue(coord, out area);
        }

        public IEnumerable<AreaView> GetAllAreas()
        {
            return _grid.Values;
        }
    }
}