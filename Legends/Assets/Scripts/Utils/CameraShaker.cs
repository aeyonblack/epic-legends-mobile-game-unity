using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShaker : Singleton<CameraShaker>
{
    private CinemachineVirtualCamera virtualCamera;

    private float shakeTimer;

    protected override void Awake()
    {
        base.Awake();
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    public void Shake(float intensity, float time)
    {
        CinemachineBasicMultiChannelPerlin c = virtualCamera
            .GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        c.m_AmplitudeGain = intensity;
        shakeTimer = time;
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0)
            {
                CinemachineBasicMultiChannelPerlin c = virtualCamera
                    .GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                c.m_AmplitudeGain = 0;
            }
        }
    }
}
