using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public class UnitSpawner : MonoBehaviour
    {
        private static UnitSpawner _instance;
        public static UnitSpawner Instance
        {
            get { return _instance; }
            set
            {
                if (_instance != null)
                {
                    Debug.LogWarning("Duplicate singleton created, destroying", value.gameObject);
                    Destroy(value.gameObject);
                    return;
                }

                _instance = value;
            }
        }

        [SerializeField] private Unit _unitPrefab;
        [SerializeField] private UnitData _AIData;
        [SerializeField] [ColorUsage(true, true)] private Color[] _teamColors;
        [SerializeField] private int _targetTeamCount = 50;
        [SerializeField] private float _spawnCooldown = 0.1f;
        [SerializeField] private TextMeshProUGUI _debugText;

        public List<List<Unit>> Teams { get; private set; }
        public List<List<Unit>> NotTeams { get; private set; }

        private List<List<SpawnPoint>> _spawnPoints;
        private float _lastSpawnTime;

        private void Start()
        {
            Instance = this;

            Teams = new List<List<Unit>>();
            NotTeams = new List<List<Unit>>();
            _spawnPoints = new List<List<SpawnPoint>>();

            for (int i = 0; i < _teamColors.Length; i++)
            {
                Teams.Add(new List<Unit>());
                _spawnPoints.Add(new List<SpawnPoint>());
            }

            for (int i = 0; i < _teamColors.Length; i++)
            {
                NotTeams.Add(new List<Unit>());
            }

            SpawnPoint[] foundSpawnPoints = FindObjectsOfType<SpawnPoint>();
            for (int i = 0; i < foundSpawnPoints.Length; i++)
            {
                _spawnPoints[foundSpawnPoints[i].Team].Add(foundSpawnPoints[i]);
            }
        }

        private void Update()
        {
            if (Time.timeSinceLevelLoad < _lastSpawnTime + _spawnCooldown) return;

            for (int i = 0; i < Teams.Count; i++)
            {
                if (Teams[i].Count < _targetTeamCount)
                {
                    Unit spawnedUnit = SpawnUnit(i, _teamColors[i]);
                    if (spawnedUnit != null)
                    {
                        Teams[i].Add(spawnedUnit);

                        for (int j = 0; j < NotTeams.Count; j++)
                        {
                            if(i != j) NotTeams[j].Add(spawnedUnit);
                        }

                        spawnedUnit.KilledEvent += OnUnitKilled;

                        UpdateGUI();
                    }
                }

                _lastSpawnTime = Time.timeSinceLevelLoad;
            }
        }

        private Unit SpawnUnit(int team, Color color)
        {
            Unit newUnit = null;
            SpawnPoint spawnPoint = _spawnPoints[team][Random.Range(0, _spawnPoints[team].Count)];
            if (spawnPoint != null)
            {
                newUnit = Instantiate(_unitPrefab, spawnPoint.GetSpawnPosition(), spawnPoint.transform.rotation);
                newUnit.transform.SetParent(transform);
                newUnit.Initialize(_AIData, team, color);
            }
            else
            {
                Debug.LogError("Failed to find spawn point matching team: " + team);
            }

            return newUnit;
        }

        private void OnUnitKilled(Unit unit)
        {
            Teams[unit.Team].Remove(unit);

            for (int i = 0; i < NotTeams.Count; i++)
            {
                if (unit.Team != i)
                {
                    NotTeams[i].Remove(unit);
                }
            }
            
            UpdateGUI();
        }

        List<Vector3> _patrolPoints = new List<Vector3>();
        public Vector3 GetPatrolDestination(int team)
        {
            _patrolPoints.Clear();
            for (int i = 0; i < _spawnPoints.Count; i++)
            {
                for (int j = 0; j < _spawnPoints[i].Count; j++)
                {
                    if(_spawnPoints[i][j].Team != team) _patrolPoints.Add(_spawnPoints[i][j].transform.position);
                }
            }

            return _patrolPoints[Random.Range(0, _patrolPoints.Count)];
        }

        StringBuilder _sb = new StringBuilder();
        private void UpdateGUI()
        {
            _sb.Clear();
            for (int i = 0; i < Teams.Count; i++)
            {
                _sb.AppendFormat("Team {0}: {1}\r\n", i, Teams[i].Count);
            }
            _debugText.text = _sb.ToString();
        }
    }
}