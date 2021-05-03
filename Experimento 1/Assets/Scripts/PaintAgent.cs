using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.IO;

public class PaintAgent : Agent
{

  WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();
  public Texture2D goaltexture;
  public GameObject path;
  public RenderTexture renderTexture;
  public Camera cameracritic;

  private Color[] goalpixels, currentpixels;
  private Texture2D currenttexture;
  private Rect rectangle;
  private RenderTexture renderTexturecopy;

  public LineRenderer Bezier;

  private int numPoints = 20;
  private Vector3[] positions = new Vector3[20];
  private Vector3 point1, point2, point3;

  private bool manualcontrol = false;
  private bool clickmouse = false;
  private float limit = 12.5f;


  void Start()
  {
    renderTexturecopy = Instantiate(renderTexture);
    cameracritic.targetTexture = renderTexturecopy;
  }

  public override void OnEpisodeBegin()
  {
    //Delete all points of the previous draw
    int childs = path.transform.childCount;
    for (int i = childs - 1; i > 0; i--)
    {
      GameObject.Destroy(path.transform.GetChild(i).gameObject);
    }

    //Free memory
    Resources.UnloadUnusedAssets();
  }

  public override void CollectObservations(VectorSensor sensor)
  {
    //Bezier control points as a observation
    //9 Vector Size, 3 * 3Vector
    sensor.AddObservation(point1);
    sensor.AddObservation(point2);
    sensor.AddObservation(point3);
  }

  public override void OnActionReceived(float[] vectorAction)
  {
    //just for auto detect default and heuristic mode
    if (clickmouse || !manualcontrol)
    {
      //avoid pass the condition if no click in the next action.
      clickmouse = false;

      //Normalize vectorAction values from -1 to 1 to canvas range (-12.5 to 12.5)
      //to avoid Bezier Curves outside of the Canva area
      var vANormalized_0 = Normalize(vectorAction[0], -1f, 1f, -limit, limit);
      var vANormalized_1 = Normalize(vectorAction[1], -1f, 1f, -limit, limit);
      var vANormalized_2 = Normalize(vectorAction[2], -1f, 1f, -limit, limit);
      var vANormalized_3 = Normalize(vectorAction[3], -1f, 1f, -limit, limit);
      var vANormalized_4 = Normalize(vectorAction[4], -1f, 1f, -limit, limit);
      var vANormalized_5 = Normalize(vectorAction[5], -1f, 1f, -limit, limit);
      var vANormalized_6 = Normalize(vectorAction[6], -1f, 1f, 0, 1);
      vANormalized_6 = Mathf.Round(vANormalized_6);

      //clone and setup Line Renderer by env
      var linerenderercopy = Instantiate(Bezier, path.transform);
      linerenderercopy.startColor = new Color(0, 0, 0, 0);
      linerenderercopy.endColor = new Color(0, 0, 0, 0);
      linerenderercopy.positionCount = numPoints;

      //Define controlpoints of the Bezier with the vector action
      point1 = new Vector3(vANormalized_0, 0.5f, vANormalized_1);
      point2 = new Vector3(vANormalized_2, 0.5f, vANormalized_3);
      point3 = new Vector3(vANormalized_4, 0.5f, vANormalized_5);

      //Render the curve
      if (vANormalized_6 == 1)
      {
        DrawCurve(point1, point2, point3, linerenderercopy);
      }
      //Get reward
      StartCoroutine(printscrn());
    }
  }

  public override void Heuristic(float[] actionsOut)
  {
    //Set manual mode;
    manualcontrol = true;

    //Set vector Actions
    actionsOut[0] = Random.Range(-1f, 1f);
    actionsOut[1] = Random.Range(-1f, 1f);
    actionsOut[2] = Random.Range(-1f, 1f);
    actionsOut[3] = Random.Range(-1f, 1f);
    actionsOut[4] = Random.Range(-1f, 1f);
    actionsOut[5] = Random.Range(-1f, 1f);
    actionsOut[6] = Random.Range(-1f, 1f);

    //if click then draw
    if (Input.GetMouseButton(0))
    {
      clickmouse = true;
    }
  }

  IEnumerator printscrn()
  {
    float reward;

    //Wait until frameRender ends
    yield return frameEnd;

    //Set currenttexture from the camera with render texture associated
    RenderTexture.active = renderTexturecopy;
    currenttexture = new Texture2D(renderTexturecopy.width, renderTexturecopy.height, TextureFormat.RGB24, false);
    rectangle = new Rect(0, 0, renderTexturecopy.width, renderTexturecopy.height);
    currenttexture.ReadPixels(rectangle, 0, 0, false);
    currenttexture.Apply();

    //Get reward
    reward = getComparisionPercent(currenttexture);

    currenttexture = null;
    RenderTexture.active = null;
    SetReward(reward);
  }

  float getComparisionPercent(Texture2D currenttexture)
  {
    //Reset all vars
    float reward = 0.0f;

    //Get pixels from textures
    goalpixels = goaltexture.GetPixels();
    currentpixels = currenttexture.GetPixels();

    //Raw comparision of two textures
    //Compare just painted pixels from each texture and get the average;
    for (int i = 0; i < currentpixels.Length; i++)
    {

      //Simplication of computation
      float goalgrayscale = goalpixels[i].grayscale;
      float currentgrayscale = currentpixels[i].grayscale;

      float absdifference = Mathf.Abs(currentgrayscale - goalgrayscale);

      if (absdifference < 0.2f && currentgrayscale < 1)
      {
        reward += 0.1f;
      }
      else if (absdifference > 0.2)
      {
        reward -= 0.05f;
      }

      // debug and return the normalize rewards ]-i,1]
      // print(this.transform.parent.name + ": " + reward);
    }
    print(this.transform.parent.name + ": " + reward);
    return (reward);
  }


  void DrawCurve(Vector3 point1, Vector3 point2, Vector3 point3, LineRenderer lineRenderer)
  {
    for (int i = 1; i < numPoints + 1; i++)
    {
      float t = i / (float)numPoints;
      positions[i - 1] = CalculateCuadraticBezierPoint(t, point1, point2, point3);
    }
    lineRenderer.SetPositions(positions);
  }

  Vector3 CalculateCuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
  {
    float u = 1 - t;
    float tt = t * t;
    float uu = u * u;

    Vector3 p = uu * p0;
    p += 2 * u * t * p1;
    p += tt * p2;

    return p;
  }

  float Normalize(float val, float valmin, float valmax, float min, float max)
  {
    return (((val - valmin) / (valmax - valmin)) * (max - min)) + min;
  }
}
