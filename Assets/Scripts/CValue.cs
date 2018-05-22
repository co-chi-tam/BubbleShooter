using System;
using UnityEngine;

[Serializable]
public class CValue {

	[SerializeField]	protected int m_IntValue = 0;
	public int intValue {
		get { return this.m_IntValue; }
		set { this.m_IntValue = value; }
	}

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
			hash = hash * 23 + this.m_IntValue.GetHashCode();
			return hash;
		}
	}

	#endregion

}
