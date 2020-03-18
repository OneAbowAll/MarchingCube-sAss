using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    public World dataOrigin;
    [Space]
    public Text chunksOnScreen;
    public Text chunksCreated;
    public Text renderDistance;
    public Text fps;
    public Slider renderDistanceSlider;

    private void Start()
    {
        renderDistanceSlider.value = dataOrigin.viewDistance;
    }

    // Update is called once per frame
    void Update()
    {
        chunksOnScreen.text = dataOrigin.GetChunksOnScreen().ToString();
        chunksCreated.text = dataOrigin.GetChunksCreated().ToString();
        renderDistance.text = dataOrigin.viewDistance.ToString();
        fps.text = ((int)(1f/Time.smoothDeltaTime)).ToString();
    }

    public void UpdateRenderDistance()
    {
        dataOrigin.viewDistance = (int)renderDistanceSlider.value;
    }
}
