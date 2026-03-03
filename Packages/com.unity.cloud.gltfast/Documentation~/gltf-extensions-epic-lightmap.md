# GLTF Extensions
In order to promote assets that are self-contained and define all attributes that relate to their usage and behaviour, we seek to define a set of extensions to the [GLTF 2.0](https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html) specification.

## Baked Lighting
To facilitate creators generating high-fidelity assets that can look great on any platform, we support embedding baked lighting data within GLTF assets via extensions to the spec.

### EPIC_lightmap_textures
We support the Epic GLTF extension that embeds baked lighting within the asset. The main body of additional schema introduced by the extension can be found under the `extensions` attribute of the GLTF. 

The data contained within enumerates the array of lightmap textures and their corresponding shader parameters to be used at runtime to decode the textures within a shader.

Each lightmap element within this array follows the schema:

- **name** - _string_ - a name identifying the initial mesh associated with the lightmap when exported
- **texture**
  - **index** - _integer_ - the index of the lightmap texture within the array of textures in the GLTF asset
  - **texCoord** - _integer_ - the texture coordinate/UV set to be used to sample from the lightmap for all nodes that reference this lightmap
- **lightmapScale** - _4-float array_ - used in the shader to remap the lightmap texture data to the correct range
- **lightmapAdd** - _4-float array_ - used in the shader to remap the lightmap texture data to the correct range
- **coordinateScaleBias** - _4-float array_ - remaps the UV in the shader to sample from the appropriate texel(s)

**Example**
Wherein we see a GLTF asset define two textures within the GLTF's array of textures to be lightmap textures, along with the corresponding parameters to be used at runtime when decoding the lightmap.
```
   "extensions":{
      "EPIC_lightmap_textures":{
         "lightmaps":[
            {
               "name":"StaticMeshComponent0",
               "texture":{
                  "index":0,
                  "texCoord":2
               },
               "lightmapScale":[
                  0.208726227, 0.110468388, 0.0472461581, 2.40995216
               ],
               "lightmapAdd":[
                  0.919579387, 0.965532422, 0.799155474, -1.64603257
               ],
               "coordinateScaleBias":[
                  0.498046875, 0.99609375, 0.0009765625, 0.001953125
               ]
            },
            {
               "name":"StaticMeshComponent0",
               "texture":{
                  "index":1,
                  "texCoord":2
               },
               "lightmapScale":[
                  0.0950857401, 0.0519743562, 0.0390427709, 1.54381776
               ],
               "lightmapAdd":[
                  1.03355002, 0.964888871, 0.806874275, -0.779717863
               ],
               "coordinateScaleBias":[
                  0.498046875, 0.99609375, 0.0009765625, 0.001953125
               ]
            }
         ]
      }
   }
```
**Example - Parsing Unreal Lightmaps In The Shader**
Whilst parsing the data above is relatively straightforward, understanding it's intended application in the shader sampling the lightmap is not. Here, we show an implementation within Unreal that parses the lightmap.

![lightmap_shader](lightmap_shader.png)

At a high level, the shader must first parse the following inputs, provided to it via the rendering API. The following must be accessible to the shader:
* lightmapScale
* lightmapAdd
* coordinateScaleBias

Texture coordinates themselves comprise of two separate sets of UVs, that themselves are transformed by a 4-float uniform passed to the shader. The transformation happens in the screenshot above within the `MF_GetTexCoords` node, which functions as follows.

![lightmap_shader_get_tex_coords](lightmap_shader_get_tex_coords.png)

In Unreal, this is all then fed into the the `Unpack lightmaps` node, which is where the real magic happens. The following is the snippet of code that is executed by this node.

```
half2 scale = coordinateScaleBias.xy;
half2 bias = coordinateScaleBias.zw;

float2 LightmapUV0 = (UV * scale + bias) * float2(1.0, 0.5);
float2 LightmapUV1 = LightmapUV0 + float2(0.0, 0.5);

// We do not currently output this - this only gets set using LM_Directionality, which we don't currently solve for..
half3 OutSubsurfaceLighting = 0;

half4 Lightmap0 = Texture2DSample(lightmapTexture, lightmapTextureSampler, LightmapUV0);
half4 Lightmap1 = Texture2DSample(lightmapTexture, lightmapTextureSampler, LightmapUV1);

half LogL = Lightmap0.w;

// Add residual
LogL += Lightmap1.w * (1.0 / 255) - (0.5 / 255);

// Range scale LogL
LogL = LogL * lightmapScale.w + lightmapAdd.w;
	
// Range scale UVW
half3 UVW = Lightmap0.rgb * Lightmap0.rgb * lightmapScale.rgb + lightmapAdd.rgb;

// LogL -> L
const half LogBlackPoint = 0.01858136;
half L = exp2( LogL ) - LogBlackPoint;

// Let's ignore directionality for now..
half Directionality = 0.6;

half Luma = L * Directionality;
half3 Color = Luma * UVW;

half3 OutDiffuseLighting = Color;

return OutDiffuseLighting;
```

Material Blueprint Pseudocode
```
LightmapVal = UnpackLightmap(UI, lightmapTexture, coordinateScaleBiase, lightmapScale, lightmapAdd);
OutEmissiveColor = (LightmapVal * BaseColor) + EmissiveColor;
OutAO = 0.0;
Intentionally set to 0.0f so the ambient/diffuse lighting contribution of the AO is removed from all lightmapped materials, otherwise they will end up being double-lit
```

#### Nodes extension
The array of elements under the GLTF node attribute gain an attribute under the extension named `lightmap`. This attribute is an integer which defines the index of the lightmap texture contained within the array of lightmaps to be used for this particular node.

**Example:**
```
   "nodes":[
      {
         "name":"Terrain_00_mdl2_StaticMeshComponent0",
         "translation":[8, 1.5, 0],
         "rotation":[-0, -0.819152534, 0, 0.573575795],
         "scale":[3.25, 2.75, 3.25],
         "mesh":0,
         "extensions":{
            "EPIC_lightmap_textures":{
               "lightmap":0
            }
         }
      }

```
