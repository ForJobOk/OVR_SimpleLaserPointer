using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;
#endif

namespace OVRTouchSample
{
    [RequireComponent(typeof(OVRGrabber))]
    [RequireComponent(typeof(HandPose))]
    public class HandAction : MonoBehaviour
    {
        public const string ANIM_LAYER_NAME_POINT = "Point Layer";
        public const string ANIM_LAYER_NAME_THUMB = "Thumb Layer";
        public const string ANIM_PARAM_NAME_FLEX = "Flex";
        public const float THRESH_COLLISION_FLEX = 0.9f;

        public const float INPUT_RATE_CHANGE = 20.0f;

        [SerializeField]
        private OVRInput.Controller m_controller;
        [SerializeField]
        private Animator m_animator = null;

        private Collider[] m_colliders = null;
        private bool m_collisionEnabled = true;
        private OVRGrabber m_grabber;
        private HandPose grabPose;

        private int m_animLayerIndexThumb = -1;
        private int m_animLayerIndexPoint = -1;
        private int m_animParamIndexFlex = -1;

        private bool m_isPointing = false;
        private bool m_isGivingThumbsUp = false;
        private float m_pointBlend = 0.0f;
        private float m_thumbsUpBlend = 0.0f;

        private void Start()
        {
            m_grabber = GetComponent<OVRGrabber>();
            grabPose = this.gameObject.GetComponent<HandPose>();

            // Collision starts disabled. We'll enable it for certain cases such as making a fist.
            m_colliders = this.GetComponentsInChildren<Collider>().Where(childCollider => !childCollider.isTrigger).ToArray();

            // Get animator layer indices by name, for later use switching between hand visuals
            m_animLayerIndexPoint = m_animator.GetLayerIndex(ANIM_LAYER_NAME_POINT);
            m_animLayerIndexThumb = m_animator.GetLayerIndex(ANIM_LAYER_NAME_THUMB);
            m_animParamIndexFlex = Animator.StringToHash(ANIM_PARAM_NAME_FLEX);

#if UNITY_EDITOR
            OVRPlugin.SendEvent("custom_hand", (SceneManager.GetActiveScene().name == "CustomHands").ToString(), "sample_framework");
#endif
        }

        private void Update()
        {
            UpdateCapTouchStates();

            m_pointBlend = InputValueRateChange(m_isPointing, m_pointBlend);
            m_thumbsUpBlend = InputValueRateChange(m_isGivingThumbsUp, m_thumbsUpBlend);

            float flex = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, m_controller);

            bool collisionEnabled = m_grabber.grabbedObject == null && flex >= THRESH_COLLISION_FLEX;

            UpdateAnimStates();
        }

        // Just checking the state of the index and thumb cap touch sensors, but with a little bit of
        // debouncing.
        private void UpdateCapTouchStates()
        {
            m_isPointing = !OVRInput.Get(OVRInput.NearTouch.PrimaryIndexTrigger, m_controller);
            m_isGivingThumbsUp = !OVRInput.Get(OVRInput.NearTouch.PrimaryThumbButtons, m_controller);
        }

        private float InputValueRateChange(bool isDown, float value)
        {
            float rateDelta = Time.deltaTime * INPUT_RATE_CHANGE;
            float sign = isDown ? 1.0f : -1.0f;
            return Mathf.Clamp01(value + rateDelta * sign);
        }

        private void UpdateAnimStates()
        {
            bool grabbing = m_grabber.grabbedObject != null;

            // Flex
            float flex = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, m_controller);
            m_animator.SetFloat(m_animParamIndexFlex, flex);

            // Point
            bool canPoint = grabPose.AllowPointing && !grabbing;
            float point = canPoint ? m_pointBlend : 0.0f;
            m_animator.SetLayerWeight(m_animLayerIndexPoint, point);

            // Thumbs up
            bool canThumbsUp = grabPose.AllowThumbsUp && !grabbing;
            float thumbsUp = canThumbsUp ? m_thumbsUpBlend : 0.0f;
            m_animator.SetLayerWeight(m_animLayerIndexThumb, thumbsUp);

        }
    }
}