using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perlin : MonoBehaviour
{

    int size = 32;
    int gridEach = 8;

    Vector2[,] generateGradients (int size) {
        Vector2[,] gradients = new Vector2[size, size];
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                gradients[x, y] =  new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            }
        }
        return gradients;
    }

    float [,] createEmptyFloatMap(int size) { 
        float[,] gradients = new float[size, size];
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                gradients[x, y] = 1f;
            }
        }
        return gradients;
    }

    Texture2D convertFloatToTexture (float [,] floatMap) {
        Texture2D texture = new Texture2D(floatMap.GetLength(0), floatMap.GetLength(0));
        for (int y = 0; y < floatMap.GetLength(0); y++) {
            for (int x = 0; x < floatMap.GetLength(1); x++) {
                float currentPixelValue = floatMap[x,y];
                texture.SetPixel(x, y, new Color(currentPixelValue, currentPixelValue, currentPixelValue));  
            }
        }
        texture.SetPixel(0,0, Color.red);
        return texture;
    }

    Vector2[,] gradients;
    void Start() {
        int gradientSize = size / gridEach;
        gradients = generateGradients(gradientSize);

        float[,] floatMap = createEmptyFloatMap(size);

        for (int y = 0; y < size; y++) {
            if(y % gridEach == 0) { 
                draw2dRay(new Vector2(0, y / (float)size), Vector2.right, Color.black);
            }
            for (int x = 0; x < size; x++) {
                //debugging:
                if(x % gridEach == 0) { 
                    draw2dRay(new Vector2(x / (float)size, 0), Vector2.up, Color.black);
                }
                if(x % gridEach == 0 && y % gridEach == 0) {
                    Vector2 nearestGradient = gradients[x / gridEach, y / gridEach];
                    draw2dRay(new Vector2(x, y) / size,
                        nearestGradient / gridEach / 2,
                        Color.red
                    );
                }

                int gX = x / gridEach;
                int gY = y / gridEach;
                Vector2[] cornerGradients = {
                    gradients[gX, gY],
                    gradients[(gX + 1) % gradientSize, gY],
                    gradients[gX, (gY + 1) % gradientSize],
                    gradients[(gX + 1) % gradientSize, (gY + 1) % gradientSize]
                };


                // draw2dLine(new Vector2(gX, gY) / size * gridEach, new Vector2(x, y) / size, Color.green);
                Vector2 PixelPosition = new Vector2(x + 0.5f, y + 0.5f) / size;

                Vector2 directionA = PixelPosition - new Vector2(gX,     gY    ) / size * gridEach;
                Vector2 directionB = PixelPosition - new Vector2(gX + 1, gY    ) / size * gridEach;
                Vector2 directionC = PixelPosition - new Vector2(gX,     gY + 1) / size * gridEach;
                Vector2 directionD = PixelPosition - new Vector2(gX + 1, gY + 1) / size * gridEach;
                
                // draw2dRay(new Vector2(gX,     gY    ) / size * gridEach, directionA, Color.green);
                // draw2dRay(new Vector2(gX + 1, gY    ) / size * gridEach, directionB, Color.magenta);
                // draw2dRay(new Vector2(gX,     gY + 1) / size * gridEach, directionC, Color.cyan);
                // draw2dRay(new Vector2(gX + 1, gY + 1) / size * gridEach, directionD, Color.yellow);




                // if(x % gridEach == 0 && y % gridEach == 0) {
                //     draw2dRay(new Vector2(gX,     gY    ) / size * gridEach,
                //         cornerGradients[0] * gridEach / 2,
                //         Color.red
                //     );
                // }

                Vector2[] directionsFromCorners = {
                    directionA,
                    directionB,
                    directionC,
                    directionD
                };
                
                // x & y is flipped...
                floatMap[x, y] = dotDirectionWithGradient(directionsFromCorners, cornerGradients, PixelPosition, new Vector2(gX, gY) / size * gridEach);
            }
        }
        floatMap[0,0] = 0f;
        // floatMap[size-1,0] = 0f;

        Texture2D texture = convertFloatToTexture(floatMap);
        GetComponent<Renderer>().material.mainTexture = texture;
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        // Mathf.Lerp(, )
    }


    float dotDirectionWithGradient(Vector2[] cornerDirection, Vector2[] cornerGradients, Vector2 PixelPosition, Vector2 CornerPos) { 
        float sum = 0;
        for (int i = 0; i < cornerDirection.Length; i++){
            sum += Vector2.Dot(cornerDirection[i].normalized, cornerGradients[i].normalized);
        }
        sum /= 4;
        // draw2dRay(CornerPos, cornerGradients[0].normalized, Color.magenta);
        // draw2dRay(PixelPosition, cornerDirection[0].normalized, Color.blue);
        // draw2dLine(cornerGradients[0], cornerDirection[0].normalized, Color.green);
        // return (Vector2.Dot(cornerDirection[0].normalized, cornerGradients[0].normalized) + 1) / 2;
        return (sum + 1) / 2;
    }

    Color defColor = Color.red;
    void draw2dRay (Vector2 start, Vector2 dir, Color col) {
        Debug.DrawRay(
            new Vector3(start.x, start.y, 0),
            new Vector3(dir.x, dir.y, 0),
            col, 
            1000f
        );
    }
    void draw2dLine (Vector2 start, Vector2 dir, Color col) {
        Debug.DrawLine(
            new Vector3(start.x, start.y, 0),
            new Vector3(dir.x, dir.y, 0),
            col, 
            1000f
        );
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
