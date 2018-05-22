using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleSingleton;

public class CCustomGrid : CMonoSingleton<CCustomGrid> {

	#region Fields

	[Header ("Cell")]
	[SerializeField]	protected CCell m_CellPrefab;
	[SerializeField]	protected Color[] m_Colors;
	public Color[] colors {
		get { return this.m_Colors; }
		set { this.m_Colors = value; }
	}

	[Header ("Configs")]
	[SerializeField]	protected int m_Row = 5;
	[SerializeField]	protected int m_MinCellWidth = 3;
	[SerializeField]	protected int m_MaxCellWidth = 4;
	[SerializeField]	protected Vector2 m_CellSize = new Vector2 (1f, 1f);
	[SerializeField]	protected Vector2 m_CellOffset = new Vector2 (0f, 0f);

	[Header ("Grids")]
	[SerializeField]	protected int m_FirstIndex = 0;
	[SerializeField]	protected List<CCell> m_Grid;

	protected Transform m_Transform;

	#endregion

	#region MonoBehaviour Implementation

	protected override void Awake() {
		base.Awake ();
		this.m_Grid = new List<CCell> ();
		this.m_Transform = this.transform;
	}

	protected virtual void Start() {
		this.InitGrid ();
//		var cell = this.CreateCell (0, 4);
//		cell.SetValue (this.GetRandomValue ());
	}

	protected virtual void LateUpdate() {
		
	}

	#endregion

	#region Main methods

	public virtual void InitGrid() {
		if (this.m_CellPrefab == null)
			return;
		for (int y = 0; y < this.m_Row; y++) {
			var isOddRow = y % 2 != 0;
			var nextCellWidth = isOddRow ? this.m_MinCellWidth : this.m_MaxCellWidth;
			for (int x = 0; x < nextCellWidth; x++) {
				var cell = this.CreateCell (x, y);
				var value = this.GetRandomValue ();
				cell.SetValue (value);
				cell.SetColorValue (this.m_Colors [value]);
				cell.SetCellActive (true);
			}
		}
		this.InvokeRepeating ("CheckNotAvailableCell", 0f, 0.25f);
		this.m_FirstIndex = 0;
	}

	public virtual void InitCell (int x, int y, CCell cell) {
		var cellPadding = (this.m_MaxCellWidth - this.m_MinCellWidth) / 2f;
		var center = (float) this.m_MinCellWidth / 2f;
		var cellPosition = cell.transform.position;
		var isOddRow = y % 2 != 0;
		cell.transform.SetParent (this.transform);
		cell.name = this.GetName (x, y);
		cellPosition.x = isOddRow 
			? ((x + cellPadding) - center) * (this.m_CellSize.x + this.m_CellOffset.x)
			: (x - center) * (this.m_CellSize.x + this.m_CellOffset.x);
		cellPosition.y = -y * (this.m_CellSize.y + this.m_CellOffset.y);
		cell.SetXY (x, y);
		cell.SetPosition (cellPosition);
		cell.SetSize (this.m_CellSize);
		// Update anchor 
		this.m_FirstIndex = this.m_FirstIndex > y ? y : this.m_FirstIndex;
	}

	public virtual void CreateNeighborCell (CCell cell, Vector2 contactPoint, int value) {
		var cX = (int)(cell.cellX + contactPoint.x);
		var cY = (int)(cell.cellY + contactPoint.y);
		var isOddRow = cell.cellY % 2 != 0;
		cX = isOddRow && contactPoint.y > 0 ? cX + 1 : cX;
		if (this.ContainCell (cX, cY) == false) {
			var newCell = this.CreateCell (cX, cY);
			newCell.SetValue (value);
			newCell.SetColorValue (this.m_Colors [value]);
			newCell.SetCellActive (true);
			newCell.ExplosionNeighborSameValue ();
		} else {
			Debug.Log ("Duplicate Cell " + cX + "|" + cY);
		}
	}

	public virtual CCell CreateCell (int x, int y) {
		var cellObj = Instantiate<CCell> (this.m_CellPrefab);
		this.AddCell (x, y, cellObj);
		return cellObj;
	}

	public virtual void AddCell (int x, int y, CCell cell) {
		this.InitCell (x, y, cell);
		if (this.m_Grid.Contains (cell) == false) {
			this.m_Grid.Add (cell);
		}
	}

	public virtual void RemoveCell (CCell cell) {
		cell.SetCellActive (false);
		this.m_Grid.Remove (cell);
	}

	public virtual void CheckNotAvailableCell () {
		var cacheCells = new LinkedList <CCell> ();
		for (int i = 0; i < this.m_Grid.Count; i++) {
			var cell = this.m_Grid [i];
			if (cacheCells.Contains (cell))
				continue;
			if (cell != null 
				&& cell.GetCellActive () 
				&& cell.cellY > this.m_FirstIndex) {
				if (cell.IsAvailable ((neightbor) => {
					return neightbor.cellY == this.m_FirstIndex;  
				}) == false) {
					cell.Explosion ();
					cacheCells.AddLast (cell);
					this.RemoveCell (cell);
				}
			}
		}
	}

	#endregion

	#region Getter && Setter

	public virtual CCell GetCell(int x, int y) {
		var childCount = this.m_Transform.childCount;
		for (int i = 0; i < childCount; i++) {
			var child = this.m_Transform.GetChild (i);
			if (child.name == this.GetName (x, y)) {
				return child.GetComponent<CCell> ();
			}
		}
		return null;
	}

	public virtual bool ContainCell(int x, int y) {
		var cellName = this.GetName (x, y);
		var childCount = this.m_Transform.childCount;
		for (int i = 0; i < childCount; i++) {
			var child = this.m_Transform.GetChild (i);
			var cellCtrl = child.GetComponent<CCell> ();
			if (cellCtrl != null 
				&& cellCtrl.cellActive
				&& cellCtrl.name == cellName) {
				return true;
			}
		}
		return false;
	}
		
	public virtual bool ContainCell (CCell value) {
		if (value == null)
			return false;
		var childCount = this.m_Transform.childCount;
		for (int i = 0; i < childCount; i++) {
			var child = this.m_Transform.GetChild (i);
			var cellCtrl = child.GetComponent<CCell> ();
			if (cellCtrl != null 
				&& cellCtrl.cellActive
				&& cellCtrl == value) {
				return true;
			}
		}
		return false;
	}

	public virtual string GetName(int x, int y) {
		return string.Format ("Cell {0}|{1}", x, y);
	}

	public virtual int GetRandomValue() {
		return UnityEngine.Random.Range (0, this.m_Colors.Length);
	}

	#endregion

}
