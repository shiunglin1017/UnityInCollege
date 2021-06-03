using UnityEngine;
//https://docs.unity3d.com/cn/2018.4/Manual/RenderingStatistics.html
//https://answers.unity.com/questions/33369/profiler-fps-vs-stats-fps-vs-timedeltatime.html
//https://forum.unity.com/threads/is-unity-fps-count-wrong-or-am-i-missing-something.150139/?_ga=2.254917896.1879586499.1608475753-1739722695.1600594043
//https://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html

public class showFPS : MonoBehaviour
{
    //更新的時間間隔
    public float UpdateInterval = 0.5F;
    //最後的時間間隔
    private float _lastInterval;
    //幀[中間變數 輔助]
    private int _frames = 0;
    //當前的幀率
    private float _fps;

    void Start()
    {
        Application.targetFrameRate=24;

        UpdateInterval = Time.realtimeSinceStartup;

        _frames = 0;
    }

    void Update()
    {
        ++_frames;

        if (Time.realtimeSinceStartup > _lastInterval + UpdateInterval)
        {
            _fps = _frames / (Time.realtimeSinceStartup - _lastInterval);

            _frames = 0;

            _lastInterval = Time.realtimeSinceStartup;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(100, 100, 200, 200), "FPS:" + _fps.ToString("f2"));
    }
}