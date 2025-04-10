using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
public enum AnimationControllerParamType
{
    NUMBER,
    BOOL,
    TRIGGER
}

public class FrameAction
{
    public double videoTimeInSeconds;
    public AnimationControllerParamType paramType;
    public string animationControllerParamName;
    public JSONNode animationControllerParamValue;

    public FrameAction() { }
    public FrameAction(JSONNode node)
    {
        videoTimeInSeconds = node["videoTimeInSeconds"];
        paramType = (AnimationControllerParamType)Enum.Parse(typeof(AnimationControllerParamType), node["paramType"]);

        Debug.Log(paramType + "," + node["paramType"]);
        animationControllerParamName = node["animationControllerParamName"];
        animationControllerParamValue = node["animationControllerParamValue"];
    }
}

public class VideoClipControlsAnimationController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public List<FrameAction> frameActions;

    private int currentActionIdx = 0;

    private Animator animatorController;
    private bool waitForLoop;
    private bool isInit = false;

    public TextAsset mockData;


    // Start is called before the first frame update
    void Start()
    {
        animatorController = GetComponent<Animator>();
        videoPlayer.loopPointReached += _ => StartCoroutine(LoopPointReached());

        SetFrameActions(JSON.Parse(mockData.text)["frameActions"]);
    }

    public void SetFrameActions(JSONNode node)
    {
        frameActions = new List<FrameAction>();
        foreach (JSONNode el in node)
        {
            frameActions.Add(new FrameAction(el));
        }

        isInit = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInit) return;
        if (waitForLoop) return;

        FrameAction frameAction = frameActions[currentActionIdx];
        if (frameActions[currentActionIdx].videoTimeInSeconds <= videoPlayer.time)
        {
            SetParameter(animatorController, frameAction);
            currentActionIdx++;
            if (currentActionIdx >= frameActions.Count)
            {
                waitForLoop = true;
            }
        }
    }

    private void SetParameter(Animator animatorController, FrameAction frameAction)
    {
        Debug.Log("Setting parameter " + frameAction.animationControllerParamName + " to " + frameAction.animationControllerParamValue);
        if (frameAction.paramType == AnimationControllerParamType.NUMBER)
        {
            animatorController.SetInteger(frameAction.animationControllerParamName, (int)frameAction.animationControllerParamValue);
        }
        else if (frameAction.paramType == AnimationControllerParamType.BOOL)
        {
            bool val = frameAction.animationControllerParamValue;
            animatorController.SetBool(frameAction.animationControllerParamName, val);
        }
        else if (frameAction.paramType == AnimationControllerParamType.TRIGGER)
        {
            animatorController.SetTrigger(frameAction.animationControllerParamName);
        }
    }

    private IEnumerator LoopPointReached()
    {
        yield return new WaitForSeconds(0.5f);
        currentActionIdx = 0;
        waitForLoop = false;
    }
}
