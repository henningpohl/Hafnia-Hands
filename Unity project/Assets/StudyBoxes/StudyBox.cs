using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;

[RequireComponent(typeof(AudioSource))]
public class StudyBox : MonoBehaviour {
    public OVRHand Hand;
    public OVRHand.Hand HandType;
    private OVRSkeleton skeleton;
    private Vector3 handBaseSize;
    public Renderer MeshRenderer;
    public Bounds MovementBounds;
    public AudioClip FadeInSound;
    public AudioClip FadeOutSound;

    private Color ActiveColor, InactiveColor;
    private Color currentColor = Color.clear;
    private float alpha;
    private bool inTrial = false;
    private List<Vector3> trialPath;
    private AudioSource audioSource;

    public bool IsHandTracked { 
        get { return Hand.IsTracked; }
    }

    public bool IsActive { 
        get { return MeshRenderer.enabled; }
    }

    public IList<OVRBone> HandBones {
        get {            
            return skeleton.Bones; 
        }
    }

    public bool IsHandInside {
        get {
            if(!Hand.IsTracked) {
                return false;
            }

            var middleFingerBasePos = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Middle1].Transform.position;
            var distance = Vector3.SqrMagnitude(transform.position - middleFingerBasePos);
            if(distance > 0.015f * Hand.HandScale) {
                return false;
            }

            var handForward = HandType == OVRHand.Hand.HandLeft ? Hand.transform.right : -Hand.transform.right;
            var handUp = HandType == OVRHand.Hand.HandLeft ? -Hand.transform.up : Hand.transform.up;
            var angleBetweenBoxForwardAndHandForward = Vector3.Angle(handForward, transform.right);
            var angleBetweenBoxUpAndHandUp = Vector3.Angle(handUp, transform.up);

            //Debug.LogError(HandType + "  " + angleBetweenBoxForwardAndHandForward + "  " + angleBetweenBoxUpAndHandUp);          
            if(angleBetweenBoxForwardAndHandForward > 45f || angleBetweenBoxUpAndHandUp > 45f) {
                return false;
            }

            return true;
        }
    }

    void Awake() {
        skeleton = Hand.GetComponent<OVRSkeleton>();
        audioSource = GetComponent<AudioSource>();
        handBaseSize = transform.localScale;
        MeshRenderer.material.color = currentColor;
    }

    void Update() {
        if(HandType == OVRHand.Hand.HandLeft) {
            Debug.DrawRay(transform.position, 5 * transform.right, Color.red);
            Debug.DrawRay(transform.position, 5 * transform.up, Color.green);
            Debug.DrawRay(transform.position, 5 * transform.forward, Color.blue);
            Debug.DrawRay(Hand.transform.position, 5 * Hand.transform.right, Color.red);
            Debug.DrawRay(Hand.transform.position, 5 * -Hand.transform.up, Color.green);
            Debug.DrawRay(Hand.transform.position, 5 * -Hand.transform.forward, Color.blue);
        }
        if(HandType == OVRHand.Hand.HandRight) {
            Debug.DrawRay(transform.position, 5 * transform.right, Color.red);
            Debug.DrawRay(transform.position, 5 * transform.up, Color.green);
            Debug.DrawRay(transform.position, 5 * transform.forward, Color.blue);
            Debug.DrawRay(Hand.transform.position, 5 * -Hand.transform.right, Color.red);
            Debug.DrawRay(Hand.transform.position, 5 * Hand.transform.up, Color.green);
            Debug.DrawRay(Hand.transform.position, 5 * -Hand.transform.forward, Color.blue);
        }


        if(Hand.IsTracked) {
            transform.localScale = handBaseSize * Hand.HandScale;
            if(IsHandInside) {
                DOTween.To(x => currentColor.r = x, currentColor.r, ActiveColor.r, 0.5f);
                DOTween.To(x => currentColor.g = x, currentColor.g, ActiveColor.g, 0.5f);
                DOTween.To(x => currentColor.b = x, currentColor.b, ActiveColor.b, 0.5f);
            } else {
                DOTween.To(x => currentColor.r = x, currentColor.r, InactiveColor.r, 0.5f);
                DOTween.To(x => currentColor.g = x, currentColor.g, InactiveColor.g, 0.5f);
                DOTween.To(x => currentColor.b = x, currentColor.b, InactiveColor.b, 0.5f);
            }
        } else {
            DOTween.To(x => currentColor.r = x, currentColor.r, InactiveColor.r, 0.5f);
            DOTween.To(x => currentColor.g = x, currentColor.g, InactiveColor.g, 0.5f);
            DOTween.To(x => currentColor.b = x, currentColor.b, InactiveColor.b, 0.5f);
        }

        MeshRenderer.material.color = currentColor;
    }

    public async Task StartMovement(int durationInMs, float averageSpeedInMetersPerSecond = 0.6f) {
        Sequence seq = DOTween.Sequence();
        seq.AppendCallback(() => {
            audioSource.clip = FadeInSound;
            audioSource.Play();
        });
        seq.Append(DOTween.ToAlpha(() => currentColor, x => currentColor = x, InactiveColor.a, 1));
        seq.AppendCallback(() => {
            inTrial = true;
        });
        seq.AppendInterval(0.5f);

        int totalDuration = 2000;
        Vector3 lastPosition = transform.position;
        trialPath = new List<Vector3>() { transform.localPosition };
        while(totalDuration < (durationInMs - 250)) {
            var newPos = SampleBounds(lastPosition, 0.2f); // next point should be at lest 20cm away
            trialPath.Add(newPos);
            var duration = Vector3.Distance(lastPosition, newPos) / averageSpeedInMetersPerSecond;
            seq.Append(transform.DOLocalMove(newPos, duration).SetEase(Ease.InOutSine));
            seq.Insert(totalDuration * 0.001f, transform.DOLocalRotate(SampleRotation(), duration));
            seq.AppendInterval(0.5f);
            totalDuration += (int)(duration * 1000) + 1500;
        }
        seq.AppendCallback(() => {
            inTrial = false;
            audioSource.clip = FadeOutSound;
            audioSource.Play();
        });
        seq.Append(DOTween.ToAlpha(() => currentColor, x => currentColor = x, 0, 1));

        await seq.AsyncWaitForCompletion();
    }

    private Vector3 SampleBounds(Vector3 lastPosition, float minDist = 0f, int maxTries = 10) {
        int attempt = 0;
        float distance = 0f;
        Vector3 sample;
        do {
            sample = new Vector3(
                Random.Range(MovementBounds.center.x - MovementBounds.extents.x, MovementBounds.center.x + MovementBounds.extents.x),
                Random.Range(MovementBounds.center.y - MovementBounds.extents.y, MovementBounds.center.y + MovementBounds.extents.y),
                Random.Range(MovementBounds.center.z - MovementBounds.extents.z, MovementBounds.center.z + MovementBounds.extents.z)
            );
            distance = Vector3.SqrMagnitude(lastPosition - sample);
            attempt++;
        } while(distance < minDist * minDist && attempt < maxTries);
        return sample;
    }

    private Vector3 SampleRotation() {
        if(HandType == OVRHand.Hand.HandLeft) {
            return new Vector3(Random.Range(-40f, 90f), 0f, 0f);
        } else {
            return new Vector3(Random.Range(-90f, 40f), 0f, 0f);
        }
    }

    public void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube((transform.position - transform.localPosition) + MovementBounds.center, MovementBounds.size);
        if(inTrial) {
            Gizmos.color = Color.black;
            for(int i = 1; i < trialPath.Count; ++i) {
                Gizmos.DrawLine(transform.position - transform.localPosition + trialPath[i - 1], transform.position - transform.localPosition + trialPath[i]);
            }
        }
    }

    public void SetActive(bool active) {
        MeshRenderer.enabled = active;
    }

    public void SetColors(Color activeColor, Color inactiveColor) {
        ActiveColor = activeColor;
        InactiveColor = inactiveColor;
    }
}
