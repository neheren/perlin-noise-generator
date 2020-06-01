using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class Perlin : MonoBehaviour
{

	int size = 512;
	int gridEach = 16;

	public Color[] terrainColors;

	Vector2[,] generateGradients (int size) {
		Vector2[,] gradients = new Vector2[size, size];
		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {
				gradients[x, y] =  new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
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
				Color col = new Color(currentPixelValue, currentPixelValue, currentPixelValue);
				if(currentPixelValue < 0.3f) {
					texture.SetPixel(y, x, terrainColors[0] * (currentPixelValue + 0.8f));  
				} else if(currentPixelValue < 0.4f) {
					texture.SetPixel(y, x, terrainColors[1] * (currentPixelValue + 0.2f));  
				} else if(currentPixelValue < 0.5f) {
					texture.SetPixel(y, x, terrainColors[2] * (currentPixelValue + 0.2f));
				} else if(currentPixelValue < 0.9f) {
					texture.SetPixel(y, x, terrainColors[3] * (currentPixelValue));
				} else { 
					texture.SetPixel(y, x, col);  
				}
				// texture.SetPixel(y, x, col);  
			}
		}
		texture.SetPixel(0, 0, Color.blue);
		texture.SetPixel(2, 0, Color.red);
		texture.SetPixel(0, 2, Color.green);
		return texture;
	}

	public AnimationCurve gradientSmoothStep;

	Vector2[,] gradients;
	int gradientSize;
	void Start() {
		Application.targetFrameRate = 60;
		gradientSize = size / gridEach;

		float[,] floatMap4 = gaussian(iteratePerlin(16, createEmptyFloatMap(size)), 3);
		float[,] floatMap8 = gaussian(iteratePerlin(32, createEmptyFloatMap(size)), 2);
		float[,] floatMap32 = gaussian(iteratePerlin(64, createEmptyFloatMap(size)), 4);
		float[,] floatMap64 = gaussian(iteratePerlin(128, createEmptyFloatMap(size)), 4);
		float[,] floatMap16 = gaussian(iteratePerlin(128*2, createEmptyFloatMap(size)), 2);

		float[,] floatMap = createEmptyFloatMap(size);
		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {
				floatMap[x, y] = gradientSmoothStep.Evaluate((floatMap4[x, y] +  floatMap8[x, y] + floatMap16[x, y] + floatMap32[x, y] + floatMap64[x, y]) / 5f);
				// floatMap[x, y] = gradientSmoothStep.Evaluate((floatMap32[x, y]));
			}
		}


		if(this.GetComponent<Renderer>() != null){
			Texture2D texture = convertFloatToTexture(floatMap);

			GetComponent<Renderer>().material.mainTexture = texture;
			
			texture.filterMode = FilterMode.Point;
			texture.Apply();
		}


		if(this.GetComponent<Terrain>() != null){
			Texture2D texture = convertFloatToTexture(floatMap);
			texture.filterMode = FilterMode.Point;
			texture.Apply();
			print("terrain on object");
			Terrain terrain = GetComponent<Terrain>();
			// terrain.terrainData.terrainLayers = 
			//terrain.GetComponent<Renderer>().material.mainTexture = convertFloatToTexture(floatMap);
			TerrainData terrainData = terrain.terrainData;
			terrainData.SetHeights(0,0, floatMap);

			terrainData.terrainLayers[0].diffuseTexture = texture;
		    // TerrainLayer[] splatPrototype = new TerrainLayer[1];
			// splatPrototype[0].diffuseTexture = convertFloatToTexture(floatMap);    //Sets the texture
			// for (int i = 0; i < 1; i++)
			// {
			// 	splatPrototype[i] = new TerrainLayer();
				// splatPrototype[i].tileSize = new Vector2(terrainData.splatPrototypes[i].tileSize.x, terrainData.splatPrototypes[i].tileSize.y);    //Sets the size of the texture
				// splatPrototype[i].tileOffset = new Vector2(terrainData.splatPrototypes[i].tileOffset.x, terrainData.splatPrototypes[i].tileOffset.y);    //Sets the size of the texture
			// }

		}
	}

	float[,] iteratePerlin (int g_size, float[,] inp) {
		size = 512;
		gridEach = g_size;
		gradientSize = size / gridEach;
		gradients = generateGradients(gradientSize);


		for (int y = 0; y < size; y++) {
			if(y % gridEach == 0) { 
				// draw2dRay(new Vector2(0, y / (float)size), Vector2.right, Color.black);
			}
			for (int x = 0; x < size; x++) {
				float point = PerlinNoise(x, y);
				inp[x, y] = point;
			}
		}

		return inp;

	}

	public float[,] gaussian (float[,] heightMap, int smoothing) {
		for (int k = 0; k < smoothing; k++) {
			for (int i = 1; i < heightMap.GetLength (0) - 1; i++) {
				for (int j = 1; j < heightMap.GetLength (1) - 1; j++) {
					float blur = (
									 heightMap [i, j]

									 + heightMap [i + 1, j + 1] * 2
									 + heightMap [i + 1, j] * 2
									 + heightMap [i, j + 1] * 2

									 + heightMap [i - 1, j - 1] * 2
									 + heightMap [i - 1, j] * 2
									 + heightMap [i, j - 1] * 2

									 + heightMap [i + 1, j - 1] * 2
									 + heightMap [i - 1, j + 1] * 2
								 )
								 / (9 + 8);
					heightMap [i, j] = blur;	
				}
			}
		}
		return heightMap;
	}


	


	float PerlinNoise(float x, float y) {
		//debugging:
		if(x % gridEach == 0) { 
			// draw2dRay(new Vector2(x / (float)size, 0), Vector2.up, Color.black);
		}
		if(x % gridEach == 0 && y % gridEach == 0) {
			Vector2 nearestGradient = gradients[(int)(x / gridEach), (int)(y / gridEach)];
			draw2dRay(new Vector2(x, y) / size,
				nearestGradient * gradientSize,
				Color.red
			);
		}

		int gX = (int)(x / gridEach);
		int gY = (int)(y / gridEach);
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

		Vector2[] directionsFromCorners = {
			directionA,
			directionB,
			directionC,
			directionD
		};

		PixelPosition -= new Vector2(0.5f, 0.5f) / size;
		float[] dotProducts = dotDirectionWithGradient(directionsFromCorners, cornerGradients);
		float dotSum = 0f;

		float fractionX = (CornerB - PixelPosition).x * gradientSize;
		float ix0 = lerp(dotProducts[0], dotProducts[1], 1 - fractionX);
		float ix1 = lerp(dotProducts[2], dotProducts[3], 1 - fractionX);

		float fractionY = (CornerC - PixelPosition).y * gradientSize;
		dotSum = lerp(ix1, ix0, fractionY);
		return gradientSmoothStep.Evaluate((dotSum + 1) / 2);
	}

	float[] dotDirectionWithGradient(Vector2[] cornerDirection, Vector2[] cornerGradients) { 
		float[] dots = new float[cornerGradients.Length];
		for (int i = 0; i < cornerDirection.Length; i++){
			dots[i] = Vector2.Dot(cornerDirection[i].normalized, cornerGradients[i].normalized);
		}
		return dots;
	}


	float lerp(float a0, float a1, float w) {
		return (Mathf.Lerp(a0, a1, w));
	}

	float perlinBlendingFunction (float t) {
		// return t * t * t * (t * (t * 6 - 15) + 10);	
		return Mathf.SmoothStep(0, 1, t);
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

	void Update()
	{
		
	}


}
