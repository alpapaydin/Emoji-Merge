Shader "Custom/MergeGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1,0.7,0.3,1)
        _ParticleColor ("Particle Color", Color) = (1,0.9,0.5,1)
        _GlowIntensity ("Glow Intensity", Range(0,10)) = 2.0
        _PulseSpeed ("Pulse Speed", Range(0.1,5)) = 1.0
        _InnerRadius ("Inner Radius", Range(0,0.5)) = 0.2
        _OuterRadius ("Outer Radius", Range(0.01,1)) = 0.5
        _Softness ("Edge Softness", Range(0.001,0.5)) = 0.1
        _ParticleCount ("Particle Count", Range(1,50)) = 20
        _ParticleSize ("Particle Size", Range(0.001,0.1)) = 0.02
        _ParticleSpeed ("Particle Speed", Range(0.1,5)) = 1.0
        _EmissionRate ("Emission Rate", Range(0.1,5)) = 1.0
        _ParticleLifetime ("Particle Lifetime", Range(0.5,5)) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _GlowColor;
            float4 _ParticleColor;
            float _GlowIntensity;
            float _PulseSpeed;
            float _InnerRadius;
            float _OuterRadius;
            float _Softness;
            float _ParticleCount;
            float _ParticleSize;
            float _ParticleSpeed;
            float _EmissionRate;
            float _ParticleLifetime;
            
            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }
            
            float createParticle(float2 uv, float2 center, float size)
            {
                float dist = length(uv - center);
                return smoothstep(size, size * 0.1, dist);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float mask = tex2D(_MaskTex, i.uv).a;
                
                float2 centeredUV = i.uv - 0.5;
                
                float dist = length(centeredUV);
                
                float pulseTime = _Time.y * _PulseSpeed;
                float pulse = 0.15 * sin(pulseTime) + 0.85;
                
                float innerRadius = _InnerRadius * pulse;
                float outerRadius = _OuterRadius * pulse;
                
                float glowFactor = 1.0 - smoothstep(innerRadius, outerRadius, dist);
                
                float waveFactor = sin((dist * 10.0 - pulseTime * 2.0) * 3.14159) * 0.1 + 0.9;
                glowFactor *= waveFactor;
                
                float angle = atan2(centeredUV.y, centeredUV.x);
                float angleVariation = (sin(angle * 6.0 + pulseTime) * 0.05) + 0.95;
                glowFactor *= angleVariation;
                
                float4 glowColor = _GlowColor * _GlowIntensity * glowFactor;
                
                float particleValue = 0;
                
                for (int j = 0; j < _ParticleCount; j++)
                {
                    float birthOffset = hash(float2(j, 0.42)) * _ParticleLifetime;
                    float particleTime = fmod(_Time.y * _EmissionRate + birthOffset, _ParticleLifetime) / _ParticleLifetime;
                    
                    if (particleTime <= 0.01) continue;
                    float particleAngle = j * (6.28318 / _ParticleCount) + hash(float2(j, 0.87)) * 0.5;
                    float radialPosition = particleTime * _OuterRadius * 1.2;

                    float wobble = sin(particleTime * 6.0 + j) * 0.1 * particleTime;
                    float wobbleAngle = particleAngle + wobble;

                    float2 particlePos;
                    particlePos.x = cos(wobbleAngle) * radialPosition;
                    particlePos.y = sin(wobbleAngle) * radialPosition;
                    
                    float sizeCurve = sin(particleTime * 3.14159);
                    float particleSize = _ParticleSize * sizeCurve * (0.5 + 0.5 * hash(float2(j, 0.35)));
                    
                    float pval = createParticle(centeredUV, particlePos, particleSize);
                    
                    float fadeIn = smoothstep(0, 0.1, particleTime);
                    float fadeOut = 1.0 - smoothstep(0.7, 1.0, particleTime);
                    pval *= fadeIn * fadeOut;
                    
                    float edgeFade = 1.0 - smoothstep(0.8 * _OuterRadius, 1.0 * _OuterRadius, radialPosition);
                    pval *= edgeFade;
                    
                    particleValue = max(particleValue, pval);
                }
                
                fixed4 texColor = tex2D(_MainTex, i.uv) * _Color;
                
                float softGlow = smoothstep(0.0, _Softness, glowFactor);
                float alpha = softGlow * _GlowColor.a;
                
                float4 particleColor = _ParticleColor * particleValue;
                float4 combinedGlow = glowColor + particleColor;
                
                alpha = max(alpha, particleValue * _ParticleColor.a);
                alpha *= mask * i.color.a;
                texColor.a *= mask * i.color.a;
                
                float4 finalColor = float4(combinedGlow.rgb, alpha);
                
                return lerp(texColor, finalColor, alpha);
            }
            ENDCG
        }
    }
}