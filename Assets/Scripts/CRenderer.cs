using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CRenderer : MonoBehaviour {

	[Header ("Events")]
	public UnityEvent OnInvisible;
	public UnityEvent OnVisible;

	protected virtual void OnBecameInvisible() {
		if (this.OnInvisible != null) {
			this.OnInvisible.Invoke ();
		}
	}

	protected virtual void OnBecameVisible() {
		if (this.OnVisible != null) {
			this.OnVisible.Invoke ();
		}
	}

}
