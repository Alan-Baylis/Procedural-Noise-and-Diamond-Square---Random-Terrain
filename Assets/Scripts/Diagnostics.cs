using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Diagnostics : MonoBehaviour 
{	
	void Update () 
    {
        Text m_text = gameObject.GetComponent<Text>();

        m_text.text = "FPS: " + ( 1 / Time.deltaTime ).ToString();
	}
}
