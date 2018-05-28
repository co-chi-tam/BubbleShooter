using System;
using UnityEngine;

[Serializable]
public class CValue {

	[SerializeField]	protected int m_IntValue = 0;
	public int intValue {
		get { return this.m_IntValue; }
		set { this.m_IntValue = value; }
	}
	[SerializeField]	protected Color m_ColorValue = Color.white;
	public Color colorValue {
		get { return this.m_ColorValue; }
		set { this.m_ColorValue = new Color (value.r, value.g, value.b, value.a); }
	}
	[SerializeField]	protected Sprite m_SpriteValue;
	public Sprite spiteValue {
		get { return this.m_SpriteValue; }
		set { this.m_SpriteValue = value; }
	}
	[SerializeField]	protected GameObject m_GObjectValue;
	public GameObject gobjectValue {
		get { return this.m_GObjectValue; }
		set { 
			// May be get error
			this.m_GObjectValue = value; 
		}
	}

	public CValue ()
	{
		this.m_IntValue = 0;
		this.m_ColorValue = Color.white;
		this.m_GObjectValue = null;
	}

	public CValue (CValue clone)
	{
		this.m_IntValue = clone.intValue;
		this.m_ColorValue = new Color (clone.colorValue.r, clone.colorValue.g, clone.colorValue.b, clone.colorValue.a);
		// May be get error
		this.m_GObjectValue = clone.gobjectValue;
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
			hash = hash * 23 + this.m_ColorValue.r.GetHashCode();
			hash = hash * 23 + this.m_ColorValue.g.GetHashCode();
			hash = hash * 23 + this.m_ColorValue.b.GetHashCode();
			hash = hash * 23 + this.m_ColorValue.a.GetHashCode();
			hash = hash * 23 + this.m_GObjectValue.name.GetHashCode();
			return hash;
		}
	}

	#endregion

}
