using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.Transform)]
    [Tooltip("Tween the local Y and Z position of a GameObject towards a Vector3. Allows you to ignore any combination of X, Y, or Z coordinates.")]
    public class TweenLocalPosition : FsmStateAction
    {
        [RequiredField]
        [Tooltip("The GameObject to tween.")]
        public FsmOwnerDefault gameObject;

        [RequiredField]
        [Tooltip("The target position to tween towards.")]
        public FsmVector3 targetPosition;

        [RequiredField]
        [Tooltip("The duration of the tween in seconds.")]
        public FsmFloat duration;

        [Tooltip("Should the X coordinate be ignored?")]
        public bool ignoreXCoordinate;

        [Tooltip("Should the Y coordinate be ignored?")]
        public bool ignoreYCoordinate;

        [Tooltip("Should the Z coordinate be ignored?")]
        public bool ignoreZCoordinate;

        private GameObject cachedGameObject;
        private Transform cachedTransform;
        private Vector3 initialPosition;
        private Vector3 targetLocalPosition;
        private float elapsedTime;

        public override void Reset()
        {
            gameObject = null;
            targetPosition = null;
            duration = 1.0f;
            ignoreXCoordinate = false;
            ignoreYCoordinate = false;
            ignoreZCoordinate = false;
        }

        public override void OnEnter()
        {
            cachedGameObject = Fsm.GetOwnerDefaultTarget(gameObject);
            if (cachedGameObject == null)
            {
                Finish();
                return;
            }

            cachedTransform = cachedGameObject.transform;
            initialPosition = cachedTransform.localPosition;

            // Calculate the target local position based on the ignore flags
            float x = ignoreXCoordinate ? initialPosition.x : targetPosition.Value.x;
            float y = ignoreYCoordinate ? initialPosition.y : targetPosition.Value.y;
            float z = ignoreZCoordinate ? initialPosition.z : targetPosition.Value.z;

            targetLocalPosition = new Vector3(x, y, z);

            elapsedTime = 0f;
        }

        public override void OnUpdate()
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= duration.Value)
            {
                cachedTransform.localPosition = targetLocalPosition;
                Finish();
                return;
            }

            float t = elapsedTime / duration.Value;
            cachedTransform.localPosition = Vector3.Lerp(initialPosition, targetLocalPosition, t);
        }
    }
}
