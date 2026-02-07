namespace LifelikeMotion.IKFootPlacement
{
    using UnityEngine;

    public class BasicCharacterRotation : MonoBehaviour
    {
        [SerializeField] private float mouseSensitivity = 1.5f;
        [SerializeField] private float smoothing = 0;

        private Vector3 rotation;
        private Animator animator;
        private float mouseX;
        private float mouseY;
        private float rotationX = 0;
        private float rotationXTarget = 0;
        private float rotationYTarget = 0;
        private bool receiveInput = true;

        private void Start()
        {
            animator = GetComponent<Animator>();
            rotation.y = transform.eulerAngles.y;
        }

        private void Update()
        {
            GetInputData();
            ApplyRotation();
        }

        private void ApplyRotation()
        {
            if (smoothing <= 0)
            {
                rotation.y += mouseX * mouseSensitivity;

                rotationYTarget = rotation.y;
                rotationXTarget += mouseY * mouseSensitivity;
                rotationXTarget = Mathf.Clamp(rotationXTarget, -90, 90);
                rotationX = rotationXTarget;

                float rotationAngle = rotationXTarget / 90f;
                animator.SetFloat("Rotation_Angle", rotationAngle);

                transform.localEulerAngles = rotation;
            }
            else if (smoothing > 0)
            {
                rotationYTarget += mouseX * mouseSensitivity;

                rotation.y = Mathf.Lerp(rotation.y, rotationYTarget, Time.deltaTime / smoothing);
                rotationXTarget += mouseY * mouseSensitivity;
                rotationXTarget = Mathf.Clamp(rotationXTarget, -90, 90);

                rotationX = Mathf.Lerp(rotationX, rotationXTarget, Time.deltaTime / smoothing);
                float rotationAngle = rotationX / 90f;
                animator.SetFloat("Rotation_Angle", rotationAngle);

                transform.localEulerAngles = rotation;
            }
        }
        private void GetInputData()
        {
            if (receiveInput)
            {
                mouseX = Input.GetAxis("Mouse X");
                mouseY = Input.GetAxis("Mouse Y");
            }
        }
    }

}