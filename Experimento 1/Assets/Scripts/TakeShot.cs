using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TakeShot : MonoBehaviour
{

  WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

  void Update()
  {
    if (Input.GetKey(KeyCode.Space))
    {
      StartCoroutine(printscrn());
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

    // Encode texture into PNG
    byte[] bytes = texture.EncodeToPNG();

    // For testing purposes, also write to a file in the project folder
    File.WriteAllBytes(Application.dataPath + "/SavedScreen.png", bytes);
    print("guardado");
  }
}
