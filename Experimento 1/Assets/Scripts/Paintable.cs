using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paintable : MonoBehaviour
{

  public GameObject Brush;

  public Renderer m_Display;

  WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();
  // Start is called before the first frame update
  void Start()
  {
  }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKey(KeyCode.Space))
    {
      StartCoroutine(printscrn());
    }
    if (Input.GetMouseButton(0))
    {
      var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit;
      if (Physics.Raycast(Ray, out hit))
      {
        var go = Instantiate(Brush, hit.point + Vector3.up * 0.1f, Quaternion.identity, transform);
      }
    }
  }

  IEnumerator printscrn()
  {
    yield return frameEnd;
    //Create a new texture with the width and height of the screen
    Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
    //Read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
    texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
    texture.Apply();

    print(texture.GetPixel(0, 0));
  }
}
