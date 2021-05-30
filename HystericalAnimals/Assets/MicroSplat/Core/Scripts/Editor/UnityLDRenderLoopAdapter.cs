using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Text;
using JBooth.MicroSplat;
using System.Collections.Generic;
using System.Linq;

namespace JBooth.MicroSplat
{
   public class UnityLDRenderLoopAdapter : IRenderLoopAdapter 
   {
      static TextAsset vertexFunc;
      static TextAsset fragmentFunc;
      static TextAsset sharedInc;
      static TextAsset terrainBody;
      //static TextAsset terrainBlendBody;
      static TextAsset shadowPass;
      static TextAsset depthPass;
      static TextAsset metaPass;

      public string GetDisplayName() 
      { 
         return "Unity LD"; 
      }

      public string GetRenderLoopKeyword() 
      {
         return "_MSRENDERLOOP_UNITYLD";
      }

      public int GetNumPasses() { return 4; }

      public void WriteShaderHeader(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, bool blend)
      {
         sb.AppendLine("   SubShader {");

         sb.AppendLine("      Tags{\"RenderType\" = \"Opaque\" \"RenderPipeline\" = \"LightweightPipeline\" \"Queue\"=\"Geometry+100\"}");


      }

      public void WritePassHeader(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
      {
         if (pass == 0)
         {
            sb.AppendLine("      Pass");
            sb.AppendLine("      {");

            sb.AppendLine("      HLSLPROGRAM");

            sb.AppendLine("      #pragma target " + compiler.GetShaderModel(features));

            sb.AppendLine("      #pragma multi_compile _ _ADDITIONAL_LIGHTS");
            sb.AppendLine("      #pragma multi_compile _ _VERTEX_LIGHTS");
            sb.AppendLine("      #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE");
            sb.AppendLine("      #pragma multi_compile _ FOG_LINEAR FOG_EXP2");
            sb.AppendLine("      #pragma multi_compile _ LIGHTMAP_ON");
            sb.AppendLine("      #pragma multi_compile _ DIRLIGHTMAP_COMBINED");
            sb.AppendLine("      #pragma multi_compile_instancing");


            sb.AppendLine("      #pragma vertex vert");
            sb.AppendLine("      #pragma fragment frag");

            if (features.Contains("_TESSDISTANCE"))
            {
               sb.AppendLine("      #pragma hull hull");
               sb.AppendLine("      #pragma domain domain");
            }

            sb.AppendLine("      #include \"HLSLSupport.cginc\"");
            sb.AppendLine("      #include \"LWRP/ShaderLibrary/Core.hlsl\"");
            sb.AppendLine("      #include \"LWRP/ShaderLibrary/Lighting.hlsl\"");
            sb.AppendLine("      #include \"CoreRP/ShaderLibrary/Color.hlsl\"");



         }
         else if (pass == 1)
         {
            sb.AppendLine(shadowPass.text);
         }
         else if (pass == 2)
         {
            sb.AppendLine(depthPass.text);
         }
         else if (pass == 3)
         {
            sb.AppendLine(metaPass.text);
         }
      }


      public void WriteVertexFunction(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
      {
         if (pass == 0)
         {
            sb.AppendLine(vertexFunc.text);
         }
      }

      public void WriteFragmentFunction(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
      {
         if (pass == 0)
         {
            sb.AppendLine(fragmentFunc.text);
         }
         else
         {
            sb.AppendLine("       ENDHLSL }");
        
         }

         if (pass == 3)
         {
            sb.AppendLine("   }");
         }

         /*
         if (blend && terrainBlendBody != null)
         {
            sb.AppendLine(terrainBlendBody.text);
         }
         */
      }


      public void WriteShaderFooter(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, bool blend, string baseName)
      {
         if (blend)
         {
            sb.AppendLine("   CustomEditor \"MicroSplatBlendableMaterialEditor\"");
         }
         else if (baseName != null)
         {
            sb.AppendLine("   Dependency \"AddPassShader\" = \"Hidden/MicroSplat/AddPass\"");
            sb.AppendLine("   Dependency \"BaseMapShader\" = \"" + baseName + "\"");
            sb.AppendLine("   CustomEditor \"MicroSplatShaderGUI\"");
         }
         sb.AppendLine("   Fallback \"Nature/Terrain/Diffuse\"");
         sb.Append("}");
      }

      public void Init(string[] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths[i];
            if (p.EndsWith("microsplat_terrain_unityld_vertex.txt"))
            {
               vertexFunc = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_terrain_unityld_fragment.txt"))
            {
               fragmentFunc = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            //if (p.EndsWith("microsplat_terrainblend_body.txt"))
            //{
            //   terrainBlendBody = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            //}
            if (p.EndsWith("microsplat_terrain_unityld_fragment_meta.txt"))
            {
               metaPass = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_terrain_unityld_fragment_shadow.txt"))
            {
               shadowPass = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_terrain_unityld_fragment_depth.txt"))
            {
               depthPass = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_terrain_body.txt"))
            {
               terrainBody = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_shared.txt"))
            {
               sharedInc = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
         }
      }

      public void PostProcessShader(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, bool blend)
      {
         sb.Replace("INTERNAL_DATA", "");
      }

      public void WriteSharedCode(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
      {
         if (pass == 0) 
         {
            sb.AppendLine(sharedInc.text);
         }
      }

      public void WriteTerrainBody(string[] features, StringBuilder sb, MicroSplatShaderGUI.MicroSplatCompiler compiler, int pass, bool blend)
      {
         if (pass == 0)
         {
            sb.AppendLine(terrainBody.text);
         }
      }
   }
}
