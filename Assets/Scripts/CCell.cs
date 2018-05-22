﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent (typeof (CircleCollider2D))]
public class CCell : MonoBehaviour {

	#region Fields

	[Header ("Configs")]
	[SerializeField]	protected bool m_CellActive = false; // Cell is active 
	public bool cellActive {
		get { return this.m_CellActive; }
		set { this.m_CellActive = value; }
	}
	[SerializeField]	protected CircleCollider2D m_Collider; // Detect collider
	protected float radius {
		get { return this.m_Collider == null ? 0f : this.m_Collider.radius; }
		set { if (this.m_Collider != null) this.m_Collider.radius = value; }
	}
	[SerializeField]	protected Rigidbody2D m_Rigidbody2D;
	[SerializeField]	protected SpriteRenderer m_SpriteRenderer;
	[SerializeField]	protected int m_CellX = 0; // Cell X axis  in grid
	public int cellX {
		get { return this.m_CellX; }
		set { this.m_CellX = value; }
	}
	[SerializeField]	protected int m_CellY = 0; // Cell Y axis in grid
	public int cellY{
		get { return this.m_CellY; }
		set { this.m_CellY = value; }
	}
	[SerializeField]	protected CValue m_CellValue; // Cell value to comparable
	public int cellValue {
		get { return this.m_CellValue.intValue; }
		set { this.m_CellValue.intValue = value; }
	}

	[Header ("Detect points")]
	[SerializeField]	protected GameObject[] m_DetectPoints; // Detect point to calcualte to add neighbor/
	public GameObject[] detectPoints {
		get { return this.m_DetectPoints; }
	}

	// Event triggers
	[Header ("Events")]
	public UnityEvent OnExplosion;
	public UnityEventCell OnDetected;

	[Serializable]
	public class UnityEventCell: UnityEvent<CCell> {}

	// Internal valuable
	protected Transform m_Transform;

	#endregion

	#region MonoBehavour Implementation

	protected virtual void Awake() {
		this.m_Transform = this.transform;
	}

	protected virtual void Start() {
		if (this.m_DetectPoints.Length == 0) {
			this.CreatePoints ();
		}
	}

	protected virtual void LateUpdate() {

	}

	protected virtual void OnCollisionEnter2D(Collision2D value) {
		if (this.m_CellActive == false)
			return;
		var cell = value.collider.GetComponent <CCell> ();
		if (cell != null) {
			if (this.OnDetected != null) {
				this.OnDetected.Invoke (cell);
			}
		}
	}

	protected virtual void OnCollisionStay2D(Collision2D value) {
		if (this.m_CellActive == false)
			return;
//		Debug.Log (value.collider.name);
	}

	protected virtual void OnCollisionExit2D(Collision2D value) {
		if (this.m_CellActive == false)
			return;
//		Debug.Log (value.collider.name);
	}

	#endregion

	#region Main methods

	// INIT POINTS
	protected virtual void CreatePoints(float radius = 0.5f) {
		var points = new GameObject ("Points");
		var segment = 6;
		var segmentAngle = Mathf.PI * 2 / segment;
		this.m_DetectPoints = new GameObject [segment];
		for (int i = 0; i < segment; i++) {
			var index = i - 0.5f;
			var x = Mathf.Sin (index * segmentAngle) * radius;
			var y = Mathf.Cos (index * segmentAngle) * radius;
			var detectPoint = new GameObject("Point" + i);
			detectPoint.transform.SetParent (points.transform);
			detectPoint.transform.position = new Vector3 (x, y);
			this.m_DetectPoints [i] = detectPoint;
		}
		points.transform.SetParent (this.m_Transform);
		points.transform.localPosition = Vector3.zero;
	}

	// Explosion
	public virtual void Explosion () {
		// FALL OUT
		this.m_Collider.isTrigger = true;
		this.m_Rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
		this.m_Rigidbody2D.AddExplosionForce (500f, Vector3.up, 1f);
		// EVENT TRIGGER
		if (this.OnExplosion != null) {
			this.OnExplosion.Invoke ();
		}
	}

	// Explosion neighbor
	public virtual void ExplosionNeighborSameValue () {
		var samevalues = this.DetectSameValue ();
		if (samevalues.Count < 3)
			return;
		for (int i = 0; i < samevalues.Count; i++) {
			var cell = samevalues [i];
			cell.Explosion ();
			CCustomGrid.Instance.RemoveCell (cell);
		}
	}

	private RaycastHit2D[] m_CacheColliders = new RaycastHit2D[99];
	public virtual List<CCell> DetectedNeighbors () {
		var results = new List<CCell> ();
		var countCollider = Physics2D.CircleCastNonAlloc (
			this.GetPosition (), 
			this.radius * 2f, 
			Vector2.zero,
			this.m_CacheColliders);
		for (int i = 0; i < countCollider; i++) {
			var coll = this.m_CacheColliders [i];
			var neighbor = coll.collider.GetComponent <CCell> ();
			if (neighbor != null) {
				results.Add (neighbor);
			}
		}
		return results;
	}

	public virtual bool IsAvailable (Func <CCell, bool> condition) {
		var cacheCells = new LinkedList <CCell> ();
		var queueCells = new Queue<CCell> ();
		cacheCells.AddLast (this);
		queueCells.Enqueue (this);
		while (queueCells.Count > 0) {
			var curCell = queueCells.Dequeue ();
			var neighbors = curCell.DetectedNeighbors ();
			for (int i = 0; i < neighbors.Count; i++) {
				var neiCell = neighbors [i];
				if (cacheCells.Contains (neiCell) == false) {
					if (condition (neiCell)) {
						return true;
					} else {
						queueCells.Enqueue (neiCell);
					}
					cacheCells.AddLast (neiCell);
				}
			}
		}
		return false;
	}

	public virtual List<CCell> DetectSameValue () {
		var result = new List<CCell> ();
		var queueCells = new Queue<CCell> ();
		queueCells.Enqueue (this);
		while (queueCells.Count > 0) {
			var curCell = queueCells.Dequeue ();
			var neighbors = curCell.DetectedNeighbors ();
			for (int i = 0; i < neighbors.Count; i++) {
				var neiCell = neighbors [i];
				if (this.cellValue == neiCell.cellValue 
					&& result.Contains (neiCell) == false) {
					result.Add (neiCell);
					queueCells.Enqueue (neiCell);
				}
			}
		}
		return result;
	}

	#endregion

	#region Getter && Setter

	public virtual void SetValue(int value) {
		this.m_CellValue.intValue = value;
	}

	public virtual void SetColorValue(Color color) {
		this.m_SpriteRenderer.color = color;
	}

	public virtual int GetValue() {
		return this.m_CellValue.intValue;
	}

	public virtual void SetXY(int x, int y) {
		this.m_CellX = x;
		this.m_CellY = y;
	}

	public virtual void SetSize(Vector2 value) {
//		this.m_Collider.radius = Mathf.Min (value.x, value.y);
	}

	public virtual void SetPosition(Vector3 position) {
		this.m_Transform.localPosition = position;
	}

	public virtual Vector3 GetPosition () {
		return this.m_Transform.position;
	}

	public virtual void SetCellActive(bool value) {
		this.m_CellActive = value;
	}

	public virtual bool GetCellActive() {
		return this.m_CellActive;
	}

	#endregion

	#region Object Implementation

	public override bool Equals (object other)
	{
		return base.Equals (other);
	}

	public override int GetHashCode ()
	{
		unchecked
		{
			int hash = 17;
			hash = hash * 23 + this.gameObject.GetHashCode();
			hash = hash * 23 + this.m_CellX.GetHashCode();
			hash = hash * 23 + this.m_CellY.GetHashCode();
			return hash;
		}
	}

	#endregion

}