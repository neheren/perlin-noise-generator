using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perlin : MonoBehaviour
{

    int size = 64;
    int gridEach = 16;

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

        Application.targetFrameRate = 60;
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
                        nearestGradient * gradientSize,
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

                Vector2 PixelPosition = new Vector2(x + 0.5f, y + 0.5f) / size;
                
                Vector2 CornerA = new Vector2(gX,     gY    ) / size * gridEach;
                Vector2 CornerB = new Vector2(gX + 1, gY    ) / size * gridEach;
                Vector2 CornerC = new Vector2(gX,     gY + 1) / size * gridEach;
                Vector2 CornerD = new Vector2(gX + 1, gY + 1) / size * gridEach;

                Vector2 directionA = PixelPosition - CornerA;
                Vector2 directionB = PixelPosition - CornerB;
                Vector2 directionC = PixelPosition - CornerC;
                Vector2 directionD = PixelPosition - CornerD;
                
                
                // draw2dRay(new Vector2(gX,     gY    ) / size * gridEach, directionA, Color.green);
                // draw2dRay(new Vector2(gX + 1, gY    ) / size * gridEach, directionB, Color.magenta);
                // draw2dRay(new Vector2(gX,     gY + 1) / size * gridEach, directionC, Color.cyan);
                // draw2dRay(new Vector2(gX + 1, gY + 1) / size * gridEach, directionD, Color.yellow);
                

                Vector2[] directionsFromCorners = {
                    directionA,
                    directionB,
                    directionC,
                    directionD
                };

                PixelPosition -= new Vector2(0.5f, 0.5f) / size;
                float[] dotProducts = dotDirectionWithGradient(directionsFromCorners, cornerGradients);
                float dotSum = 0f;
                foreach (var dotProduct in dotProducts) {
                    dotSum += dotProduct;
                }

                float Iab = (CornerB - PixelPosition).x * gradientSize;
                // print("x: " + x + ", y: " + y + ", iABx " + Iab);
                // draw2dLine(PixelPosition, CornerA, Color.blue * Iab);
                float Icd = (CornerD - PixelPosition).x * gradientSize;
                // draw2dLine(PixelPosition, CornerD, Color.green * Icd);
                // print("x: " + x + ", y: " + y + ", iCDx " + Icd);
                float ix0 = lerp(dotProducts[0], dotProducts[1], Iab);
                float ix1 = lerp(dotProducts[2], dotProducts[3], Icd);



                dotSum = ix0 + ix1;

                float Iac = (CornerC - PixelPosition).y * gradientSize;
                dotSum = lerp(ix1, ix0, Iac);
                // print(Iac);
                // draw2dLine(PixelPosition, CornerC, Color.blue * Iac);

                logOnce("iab: " + Iab);
                logOnce("icd: " + Icd);
                logOnce("Iac: " + Iac);

                logOnce("ix0: " + ix0);
                logOnce("ix0: " + ix1);
                logOnce("dotSum: " + dotSum);
                
                once = true;

                floatMap[x, y] = (dotSum + 1f) / 2f;
            }
        }

        Texture2D texture = convertFloatToTexture(floatMap);
        GetComponent<Renderer>().material.mainTexture = texture;
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        // Mathf.Lerp(, )
    }
    bool once = false;
    void logOnce(string input) {
        if(!once){
            print(input);
        }
    }


    float[] dotDirectionWithGradient(Vector2[] cornerDirection, Vector2[] cornerGradients) { 
        float[] dots = new float[cornerGradients.Length];
        for (int i = 0; i < cornerDirection.Length; i++){
            dots[i] = Vector2.Dot(cornerDirection[i].normalized, cornerGradients[i].normalized);
        }
        
        // draw2dRay(CornerPos, cornerGradients[0].normalized, Color.magenta);
        // draw2dRay(PixelPosition, cornerDirection[0].normalized, Color.blue);
        // draw2dLine(cornerGradients[0], cornerDirection[0].normalized, Color.green);
        // return (Vector2.Dot(cornerDirection[0].normalized, cornerGradients[0].normalized) + 1) / 2;
        return dots;
    }


    float lerp(float a0, float a1, float w) {
        // w = (w + 1f) / 2f;
        return (1.0f - w) * a0 + w * a1;
        return a0 + w * (a1 - a0);
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
