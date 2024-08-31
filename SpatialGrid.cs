using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ddotb
{
    public class SpatialGrid
    {
        private SpatialCell[,] m_Cells;

        private int m_CellsLength;

        private float m_CellSize;
        private float m_HalfGridSize;
        private float m_CellSizeReciprocal;

        private List<SpatialCellReference> m_CellsList;
        private SpatialObjectCollection m_SpatialObjectCollection;
        
        private Dictionary<GameObject, SpatialCellReference> m_ObjectCellDictionary;

        private const int CELLS_LIST_PREALLOCATE = 100;

        public SpatialGrid(float cellSize, float gridSize)
        {
            m_CellSize = cellSize;
            m_CellsLength = Round(gridSize / m_CellSize) + 1;
            
            m_Cells = new SpatialCell[m_CellsLength, m_CellsLength];

            m_CellsList = new List<SpatialCellReference>(CELLS_LIST_PREALLOCATE);
            m_SpatialObjectCollection = new SpatialObjectCollection();
            
            m_ObjectCellDictionary = new Dictionary<GameObject, SpatialCellReference>();
            
            m_HalfGridSize = (gridSize * 0.5f);
            m_CellSizeReciprocal = 1 / m_CellSize;
            
            for (int i = 0; i < m_CellsLength; i++)
            {
                for (int j = 0; j < m_CellsLength; j++)
                {
                    Vector3 position = new Vector3((i * m_CellSize) - m_HalfGridSize, 0, (j * m_CellSize) - m_HalfGridSize);
                    m_Cells[i, j] = new SpatialCell(position, m_CellSize);
                }
            }
        }
        
        public void Insert(GameObject objectToInsert)
        {
            SpatialCellReference cell = GetCell(objectToInsert.transform.position);
            
            m_ObjectCellDictionary.Add(objectToInsert, cell);
            
            m_Cells[cell.X, cell.Z].Insert(objectToInsert);
        }

        public void Remove(GameObject objectToRemove)
        {
            SpatialCellReference cell = m_ObjectCellDictionary[objectToRemove];
            
            m_ObjectCellDictionary.Remove(objectToRemove);
            
            m_Cells[cell.X, cell.Z].Remove(objectToRemove);
        }

        public SpatialObjectCollection GetObjectsInRadius(Vector3 position, float radius)
        {
            m_SpatialObjectCollection.Clear();

            List<SpatialCellReference> cells = GetCells(position, radius);

            for (int i = 0; i < cells.Count; i++)
            {
                SpatialObjectCollection objectsInCell = m_Cells[cells[i].X, cells[i].Z].Objects;
                m_SpatialObjectCollection.UnionWith(objectsInCell);
            }

            return m_SpatialObjectCollection;
        }

        private SpatialCellReference GetCell(Vector3 position)
        {
            int posX = Mathf.Clamp(Round((position.x + m_HalfGridSize) * m_CellSizeReciprocal), 0, m_CellsLength - 1);
            int posZ = Mathf.Clamp(Round((position.z + m_HalfGridSize) * m_CellSizeReciprocal), 0, m_CellsLength - 1);
            
            return new SpatialCellReference(posX, posZ);
        }

        private List<SpatialCellReference> GetCells(Vector3 position, float radius)
        {
            m_CellsList.Clear();

            Vector3 offsetUp = new Vector3(0.0f, 0.0f, radius);
            Vector3 offsetRight = new Vector3(radius, 0.0f, 0.0f);

            int minX = GetCell(position - offsetRight).X;
            int minZ = GetCell(position - offsetUp).Z;
            int maxX = GetCell(position + offsetRight).X;
            int maxZ = GetCell(position + offsetUp).Z;

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    m_CellsList.Add(new SpatialCellReference(x, z));
                }
            }
            
            return m_CellsList;
        }

        private int Round(float toRound)
        {
            return (int)(toRound + 0.5f);
        }
        
        public void DrawCells()
        {
            for (int i = 0; i < m_CellsLength; i++)
            {
                for (int j = 0; j < m_CellsLength; j++)
                {
                    m_Cells[i, j].DrawCell();
                }
            }
        }
        
        public class SpatialCell
        {
            public Vector3 Position;
            public SpatialObjectCollection Objects;

            private Color m_DebugColour;
            
            private float m_CellSize;

            public SpatialCell(Vector3 position, float cellSize)
            {
                Position = position;
                Objects = new SpatialObjectCollection();

                m_CellSize = cellSize;
                
                m_DebugColour = Color.blue;
            }

            public void Insert(GameObject objectToAdd)
            {
                Objects.Add(objectToAdd);
            }

            public void Remove(GameObject objectToRemove)
            {
                Objects.Remove(objectToRemove);
            }

            public void DrawCell()
            {
                Gizmos.color = m_DebugColour;
                Gizmos.DrawWireCube(Position, new Vector3(m_CellSize, 0.0f, m_CellSize));
                
                IEnumerator enumerator = Objects.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    GameObject current = (GameObject)enumerator.Current;
                    Gizmos.DrawLine(Position, current.transform.position );
                    Gizmos.DrawSphere(current.transform.position, 1.0f);
                }
            }
        }

        public struct SpatialCellReference
        {
            public int X;
            public int Z;

            public SpatialCellReference(int posX, int posZ)
            {
                X = posX;
                Z = posZ;
            }
        }

        public class SpatialObjectCollection : IEnumerable<GameObject>
        {
            public HashSet<GameObject> HashSet;
            public List<GameObject> BackerList;

            private const int PREALLOCATE_COUNT = 100;
            
            public SpatialObjectCollection()
            {
                HashSet = new HashSet<GameObject>(PREALLOCATE_COUNT);
                BackerList = new List<GameObject>(PREALLOCATE_COUNT);
            }

            public void Add(GameObject objectToAdd)
            {
                HashSet.Add(objectToAdd);
                BackerList.Add(objectToAdd);
            }

            public void Remove(GameObject objectToRemove)
            {
                HashSet.Remove(objectToRemove);
                BackerList.Remove(objectToRemove);
            }

            public void UnionWith(SpatialObjectCollection other)
            {
                List<GameObject> otherList = other.BackerList;
                for (int i = 0; i < otherList.Count; i++)
                {
                    Add(otherList[i]);
                }
            }

            public void Clear()
            {
                HashSet.Clear();
                BackerList.Clear();
            }

            public IEnumerator<GameObject> GetEnumerator()
            {
                return BackerList.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
