using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OD {
    public class SimpleBlit : ScriptableRendererFeature {
        class CustomRenderPass : ScriptableRenderPass{
            public RenderTargetIdentifier source;
            public Settings settings = null;

            RenderTargetHandle tempTexture;
            Material material = null;

            public CustomRenderPass(){}

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor){
                cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData){
                if (settings == null) return;
                if(renderingData.cameraData.cameraType != CameraType.Game && renderingData.cameraData.cameraType != CameraType.SceneView) return;
                
                material = settings.material;

                if(material == null) return;

                CommandBuffer cmd = CommandBufferPool.Get();
                cmd.Clear();

                if(settings.setGlobalInverseView) Shader.SetGlobalMatrix(settings.inverseViewPropertyName, renderingData.cameraData.camera.cameraToWorldMatrix);

                cmd.Blit(source, tempTexture.Identifier(), material, 0);
                cmd.Blit(tempTexture.Identifier(), source);

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            public override void FrameCleanup(CommandBuffer cmd){}
        }
        //

        CustomRenderPass m_ScriptablePass;

        [System.Serializable]
        public class Settings{
            public bool setGlobalInverseView = true;
            public string inverseViewPropertyName = "_InverseView";

            public Material material;
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public Settings settings = new Settings();

        public override void Create(){
            m_ScriptablePass = new CustomRenderPass();
            m_ScriptablePass.settings = settings;
            m_ScriptablePass.renderPassEvent = settings.renderPassEvent;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData){
            m_ScriptablePass.source = renderer.cameraColorTarget;
            m_ScriptablePass.settings = settings;

            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}