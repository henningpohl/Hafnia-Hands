using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormsRoot : MonoBehaviour {
    public Transform User;
    public OVRHand LeftHand;
    private OVRSkeleton LeftSkeleton;
    public OVRHand RightHand;
    private OVRSkeleton RightSkeleton;

    public SurveyController surveyController;
    public StudyControllerFrontiers studyController;

    public float SmoothTime = 0.5f;
    public float FreezeDistance = 100f;
    public float SlowdownZone = 0.2f;
    public float AngularSlowdownThreshold = 0.3f;
    public float PlayerDistance = 0.4f;
    public float PlayerOffsetY = -0.2f;

    private List<RectTransform> forms = new List<RectTransform>();
    private Vector3 velocity = new Vector3();

    private void Start() {
        GetComponentsInChildren<RectTransform>(forms);
        LeftSkeleton = LeftHand.GetComponent<OVRSkeleton>();
        RightSkeleton = RightHand.GetComponent<OVRSkeleton>();
    }

    void Update() {
        var minDist = GetMinIndexFormDistance();

        float maxSpeed = 10f;
        if(minDist < FreezeDistance) {
            maxSpeed = 0f;
        }

        var newPos = User.transform.position + User.transform.forward * PlayerDistance;
        newPos.y += PlayerOffsetY;

        var panelDistance = Vector3.Distance(transform.position, newPos);
        //var angularVelocity = OVRManager.display.angularVelocity.magnitude;
        if(panelDistance < SlowdownZone && velocity.magnitude < 0.1f) {
            maxSpeed = 0f;
        }

        transform.position = new Vector3(
            Mathf.SmoothDamp(transform.position.x, newPos.x, ref velocity.x, SmoothTime, maxSpeed),
            Mathf.SmoothDamp(transform.position.y, newPos.y, ref velocity.y, SmoothTime, maxSpeed),
            Mathf.SmoothDamp(transform.position.z, newPos.z, ref velocity.z, SmoothTime, maxSpeed));

        var toUser = User.transform.position - transform.position;
        //toUser = Vector3.ProjectOnPlane(toUser, Vector3.up).normalized;
        if(minDist >= FreezeDistance) {
            transform.rotation = Quaternion.LookRotation(toUser);
        }
    }

    private RectTransform GetActiveForm() {
        foreach(var form in forms) {
            if(form.gameObject.activeInHierarchy) {
                return form;
            }
        }
        return null;
    }

    private float GetMinIndexFormDistance() {
        var form = GetActiveForm();
        if(form == null) {
            return float.PositiveInfinity;
        }

        /*
        Vector3[] corners = new Vector3[4];
        form.GetWorldCorners(corners);
        Plane plane = new Plane(corners[0], corners[1], corners[2]);

        var leftOnPlane = plane.ClosestPointOnPlane(LeftHand.transform.position);
        var rightOnPlane = plane.ClosestPointOnPlane(RightHand.transform.position);
        */

        float leftDist = float.PositiveInfinity;
        float rightDist = float.PositiveInfinity;
        if(LeftHand.IsTracked && LeftSkeleton.IsDataValid) {
            var leftIndexTipPos = LeftSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.position;
            leftDist = Mathf.Abs(form.InverseTransformPoint(leftIndexTipPos).z);
        }
        if(RightHand.IsTracked && RightSkeleton.IsDataValid) {
            var rightIndexTipPos = RightSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.position;
            rightDist = Mathf.Abs(form.InverseTransformPoint(rightIndexTipPos).z);
        }

        return Mathf.Min(leftDist, rightDist);
    }

    public void OnClick(int button) {
        // TODO: we could stop click events at this point, but it looks like
        // that might not even be necessary with the panel staying in place

        if(button == 1) {
            surveyController.button1Pressed();
        } else if(button == 2) {
            surveyController.button2Pressed();
        } else if(button == 3) {
            surveyController.button3Pressed();
        } else if(button == 4) {
            surveyController.button4Pressed();
        } else if(button == 5) {
            surveyController.button5Pressed();
        } else if(button == 6) {
            surveyController.button6Pressed();
        } else if(button == 7) {
            surveyController.button7Pressed();
        } else if(button == 8) {
            studyController.ClickedFinalDone();
        }
    }
}
