using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compare : MonoBehaviour
{
  public Texture2D img1, img2;
  private int score = 0, total = 0;

  void Start()
  {

  }

  void Update()
  {
    if (Input.GetMouseButtonDown(0))
    {
      print(getComparisionPercent());
    }

    if (Input.GetMouseButtonDown(1))
      Debug.Log("Pressed secondary button.");

    if (Input.GetMouseButtonDown(2))
      Debug.Log("Pressed middle click.");
  }

  double getComparisionPercent()
  {
    score = 0;
    total = 0;
    Color[] img1pixels = img1.GetPixels();
    Color[] img2pixels = img2.GetPixels();

    for (int i = 0; i < img2pixels.Length; i++)
    {
      float grayscale1 = img1pixels[i].grayscale;
      float grayscale2 = img2pixels[i].grayscale;

      if (grayscale2 < 1)
      {
        float difference = grayscale1 - grayscale2;
        float absdifference = Mathf.Abs(difference);
        if (absdifference == 0)
        {
          score += 10;
        }
        else
        {
          score = score - (int)(absdifference * 10);
        }
        total +=1;
      }
    }
    return ((double)score / (double)(total * 10));
  }

}
