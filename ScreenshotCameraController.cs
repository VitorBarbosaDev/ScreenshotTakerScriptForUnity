using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotCameraController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform; // reference to the camera transform
    [SerializeField] private Transform subjectPostion; // reference to the camera pivot
    [SerializeField] private LayerMask cullingMask; // culling mask for the camera
    [SerializeField] private int screenshotWidth = 256; // width of the screenshots
    [SerializeField] private int screenshotHeight = 256; // height of the screenshots
    [SerializeField] private string savePath = "/Screenshots/"; // path to save the screenshots
    [SerializeField] private string tag = "Plant"; // tag of the objects to take screenshots of
    private void Start()
    {
        // create the save folder if it doesn't exist
        System.IO.Directory.CreateDirectory(Application.dataPath + savePath);

        // find all plants in the scene and take screenshots of them
        GameObject[] plants = GameObject.FindGameObjectsWithTag(tag);

        foreach (GameObject plant in plants)
        {
            // set the camera position and rotation
            plant.transform.position= subjectPostion.position  ;
              plant.transform.rotation=subjectPostion.rotation;

            // take the screenshot
            Sprite screenshot = TakeScreenshot(plant);

            plant.SetActive(false);
            
            // save the screenshot
            SaveScreenshot(screenshot, plant.name);
            
           
        }
    }

    private Sprite TakeScreenshot(GameObject objectToCapture)
{
    // Set the camera culling mask
    Camera camera = cameraTransform.GetComponent<Camera>();
    camera.cullingMask = cullingMask;
    
  
    // If the camera is too close to the object, move the object back a certain distance
    Renderer renderer = objectToCapture.GetComponent<Renderer>();
    Bounds bounds = renderer.bounds;

    float objectSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
   float  distance = objectSize / (2 * Mathf.Tan(camera.fieldOfView / 4 * Mathf.Deg2Rad));


     Vector3  objectPosition = bounds.center;
     Vector3  cameraPosition = objectPosition - cameraTransform.forward * distance;
    cameraPosition.y = objectPosition.y;
    cameraTransform.position = cameraPosition;
    
    Vector3 direction = (objectPosition - cameraPosition).normalized;
    Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
    cameraTransform.rotation = rotation;

    
    // Create a new render texture
    RenderTexture rt = new RenderTexture(screenshotWidth, screenshotHeight, 24);
    RenderTexture.active = rt;

    camera.clearFlags = CameraClearFlags.SolidColor;
    camera.backgroundColor = new Color(0, 0, 0, 0);

    // Render the game object to the render texture
    camera.targetTexture = rt;
    camera.Render();
    camera.targetTexture = null;

    // Create a new texture and read the pixels from the render texture into it
    Texture2D texture = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);
    texture.ReadPixels(new Rect(0, 0, screenshotWidth, screenshotHeight), 0, 0);
    texture.Apply();

    // Convert the texture to a sprite and return it
    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2f);
    Debug.Log("Screenshot taken and converted to sprite and its called " + sprite.name + ".");
    RenderTexture.active = null;
    return sprite;
}




    private void SaveScreenshot(Sprite screenshot, string plantName)
    {
        // Create a new texture and read the pixels from the screenshot sprite into it
        Texture2D texture = screenshot.texture;
        Color[] pixels = texture.GetPixels();
        Texture2D newTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        newTexture.SetPixels(pixels);

        // Save the texture as a PNG file
        byte[] bytes = newTexture.EncodeToPNG();
        string filePath = Application.dataPath + savePath + plantName + ".png";
        System.IO.File.WriteAllBytes(filePath, bytes);

        Debug.Log("Screenshot saved: " + filePath);
    }
}