namespace LifelikeMotion.IKFootPlacement
{
    using UnityEngine;

    public class BasicCharacterMovement : MonoBehaviour
    {
        private CharacterController cc;
        [SerializeField] private Packages.Lifelike_Motion.IKFootPlacement___Free_IK_Script.Tool.IKFootPlacement iKFootPlacement;
        [SerializeField] private float movementSpeed = 2;
        [SerializeField] private float jumpSpeed = 5;
        [SerializeField] private float gravity = 15;

        private bool receiveInput = true;
        private bool isMoving = true;
        private float horizontal;
        private float vertical;
        [HideInInspector] public bool jumped;

        private Vector3 velocity;
        private Vector3 ccPosition;
        private Animator animator;

        void Start()
        {
            cc = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();

            //Optional cursor lock and disabled visability
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
            GetInputData();
            CalculateMovement();
        }

        public void CalculateMovement()
        {
            Vector3 velocity = Vector3.zero;
            velocity.z = vertical;
            velocity.x = horizontal;

            animator.SetFloat("Z", vertical);
            animator.SetFloat("X", horizontal);

            velocity = Vector3.ClampMagnitude(velocity, 1);

            this.velocity.z = velocity.z * movementSpeed;
            this.velocity.x = velocity.x * movementSpeed;

            if (cc.isGrounded && !jumped) { this.velocity.y = -2; }

            else if (cc.isGrounded && jumped)
            {
                this.velocity.y = jumpSpeed;
                if (iKFootPlacement != null)
                {
                    iKFootPlacement.isGrounded = false;
                    iKFootPlacement.jumped = true;
                }
                isMoving = true;
                jumped = false;
            }

            else
            {
                this.velocity.y -= gravity * Time.deltaTime;
                isMoving = true;
            }

            cc.Move(transform.TransformVector(this.velocity) * Time.deltaTime);

            if (!isMoving) { cc.transform.position = new Vector3(ccPosition.x, cc.transform.position.y, ccPosition.z); }
            else { ccPosition = cc.transform.position; }
        }

        private void GetInputData()
        {
            if (receiveInput)
            {
                vertical = Input.GetAxis("Vertical");
                horizontal = Input.GetAxis("Horizontal");

                if (iKFootPlacement != null) { iKFootPlacement.isGrounded = cc.isGrounded; }

                if (vertical != 0 || horizontal != 0)
                {
                    isMoving = true;
                    if (iKFootPlacement != null) iKFootPlacement.isMoving = true;
                }
                else
                {
                    isMoving = false;
                    if (iKFootPlacement != null) iKFootPlacement.isMoving = false;
                }

                if (Input.GetAxis("Jump") > 0 && !jumped) { jumped = true; }
                else { jumped = false; }
            }
        }
    }
}