using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CBullet : MonoBehaviour {

	#region Fields

	[SerializeField]	protected float m_Speed = 5f;
	public float speed {
		get { return this.m_Speed; }
		set { this.m_Speed = value; }
	}
	[SerializeField]	protected CValue m_BulletValue;
	public int value {
		get { return this.m_BulletValue.intValue; }
		set { this.m_BulletValue.intValue = value; }
	}
	[SerializeField]	protected SpriteRenderer m_SpriteRenderer;
	[SerializeField]	protected Vector3 m_StartPosition;
	[SerializeField]	protected Vector3 m_MovePosition;

	[Header ("Events")]
	public UnityEvent OnShoot;
	public UnityEventPoint OnDetected;

	[Serializable]
	public class UnityEventPoint: UnityEvent<CCell> {}

	protected Transform m_Transform;
	protected CCustomGrid m_GameManager;
	protected bool m_IsMoving = false;
	protected bool m_Freeze = false;

	#endregion

	#region MonoBehavour Implementation

	protected virtual void Awake() {
		this.m_Transform = this.transform;
	}

	protected virtual void Start() {
		this.m_GameManager = CCustomGrid.GetInstance ();
		this.Restart ();
	}

	protected virtual void Update() {
		if (Input.GetMouseButtonDown (0) && this.m_IsMoving == false) {
			this.Shoot ();
		} 
		if (this.m_IsMoving) {
			this.Move ();
		}
	}

	protected virtual void OnCollisionEnter2D(Collision2D value) {
		if (this.m_Freeze)
			return;
		if (value.gameObject.CompareTag ("Ball")) { 
			var cell = value.collider.GetComponent <CCell> ();
			if (cell != null) {
				if (this.OnDetected != null) {
					this.OnDetected.Invoke (cell);
				}
				var face = this.GetContactPointToV2 (cell);
				this.m_GameManager.CreateNeighborCell (cell, face, this.m_BulletValue.intValue);
				this.Stop ();
				this.Restart ();
			}
			this.m_Freeze = true;
			Invoke ("Defreeze", 0.5f);
		} else if (value.gameObject.CompareTag ("Wall")) {
			var pointContact = value.contacts [0];
			this.m_MovePosition = Vector2.Reflect (this.m_MovePosition, pointContact.normal);
			this.m_MovePosition.z = 0f;
		}
	}

	private void Defreeze() {
		this.m_Freeze = false;
	}

	protected virtual void OnCollisionStay2D(Collision2D value) {
		
	}

	protected virtual void OnCollisionExit2D(Collision2D value) {
		
	}

	#endregion

	#region Main methods

	public virtual void Restart() {
		this.m_Transform.position = this.m_StartPosition;
		this.m_IsMoving = false;
		var value = this.m_GameManager.GetRandomValue ();
		this.SetValue (value);
		this.SetColorValue (this.m_GameManager.colors[value]);
	}

	public virtual void Shoot() {
		if (this.m_Freeze)
			return;
		this.m_IsMoving = true;
		this.m_MovePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition) - this.m_StartPosition;
		this.m_MovePosition.z = 0f;
		if (this.OnShoot != null) {
			this.OnShoot.Invoke ();
		}
	}

	public virtual void Stop() {
		this.m_IsMoving = false;
	}

	public virtual void Move () {
		var position = this.m_Transform.position;
		position += this.m_MovePosition.normalized * Time.deltaTime * this.m_Speed;
		position.z = 0f;
		this.m_Transform.position = position;
	}

	#endregion

	#region Getter && Setter

	public virtual Vector2 GetContactPointToV2(CCell value) {
		// GET DETECT POINT NEAREST
		var contactPoints = value.detectPoints;
		var anglePoint = -1;
		var minDistance = 9999f;
		for (int i = 0; i < contactPoints.Length; i++) {
			var direction = this.m_Transform.position - contactPoints [i].transform.position;
			var currentDistance = direction.sqrMagnitude;
			if (direction.sqrMagnitude < minDistance) {
				anglePoint = i;
				minDistance = direction.sqrMagnitude;
			}
		}
		// CALCULATE POINT
		switch (anglePoint) {
		default:
		case 0:
			return new Vector2 (-1f, -1f);
		case 1:
			return new Vector2 (0f, -1f);
		case 2:
			return new Vector2 (1f, 0f);
		case 3:
			return new Vector2 (0f, 1f);
		case 4:
			return new Vector2 (-1f, 1f);
		case 5:
			return new Vector2 (-1f, 0f);
		}
	}

	public virtual void SetValue(int value) {
		this.m_BulletValue.intValue = value;
	}

	public virtual void SetColorValue (Color color) {
		this.m_SpriteRenderer.color = color;
	}

	public virtual int GetValue() {
		return this.m_BulletValue.intValue;
	}

	#endregion

}
