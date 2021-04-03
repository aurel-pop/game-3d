using UnityEngine;

namespace MonsterFlow
{
    /// <summary>
    ///    Handles the player movement.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        /// <summary> Sets the movement speed of the player for Keyboard. 10-200 % </summary>
        [Range(0.1f, 2)]
        public float moveSpeedMultiplier;
        [SerializeField]
        public float deadZoneAngle;

        private Controls controls;
        private int      moveDirection;
        private float    rotationAmount;

        private int      turnValue;
        private Vector2  halfScreenResolution;
        private bool     overwriteLeft;
        private bool     overwriteRight;

        private void Awake()
        {
            this.controls = new Controls();
        }

        private void Start()
        {
            this.moveSpeedMultiplier *= 360f;

            this.halfScreenResolution = new Vector2(Screen.width * 0.5f, Screen.height);

#if UNITY_EDITOR || UNITY_STANDALONE
            SetupKeyboardInput();
#endif
        }

#if PLATFORM_IOS || PLATFORM_ANDROID
        private void HandleTouchInput()
        {
            // Handle screen touches smoothly.
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; ++i)
                {
                    if (Input.GetTouch(i).phase == TouchPhase.Began)
                    {
                        if (Input.GetTouch(i).position.x > halfScreenResolution.x)
                        {
                            // A: Right screen part input = clockwise turn (-1)
                            overwriteRight = true;
                        }
                        else if (Input.GetTouch(i).position.x <= halfScreenResolution.x)
                        {
                            // B: Left screen part input = counter-clockwise turn (1)
                            overwriteLeft = true;
                        }
                    }

                    if (Input.GetTouch(i).phase == TouchPhase.Ended || Input.GetTouch(i).phase == TouchPhase.Canceled)
                    {
                        if (Input.GetTouch(i).position.x > halfScreenResolution.x)
                        {
                            // A: Right screen part input = clockwise turn (-1)
                            overwriteRight = false;
                        }
                        else if (Input.GetTouch(i).position.x <= halfScreenResolution.x)
                        {
                            // B: Left screen part input = counter-clockwise turn (1)
                            overwriteLeft = false;
                        }
                    }

                    if (Input.GetTouch(i).position.x > halfScreenResolution.x)
                    {
                        // A: Right screen part input = clockwise turn (-1)
                        turnValue = -1;

                        if (overwriteLeft)
                            turnValue = 1;
                    }
                    else if (Input.GetTouch(i).position.x <= halfScreenResolution.x)
                    {
                        // B: Left screen part input = counter-clockwise turn (1)
                        turnValue = 1;

                        if (overwriteRight)
                            turnValue = -1;
                    }
                }
            }
            else
            {
                turnValue = 0;
                overwriteRight = false;
                overwriteLeft = false;
            }

            Rotate(turnValue);
        }

        private void Rotate(int direction)
        {
            Vector3 targetRotation = new Vector3(0f, 0f, direction * moveSpeedMultiplier * Time.deltaTime);
            transform.Rotate(targetRotation);
        }
#endif

#if UNITY_EDITOR || UNITY_STANDALONE
        private void SetupKeyboardInput()
        {
            this.controls.Standard.Move.performed += context =>
            {
                // x = RightArrow, y = LeftArrow
                Vector2 value = context.ReadValue<Vector2>();

                // If both now pressed invert current direction...
                if (value.x + value.y > 1f) this.moveDirection *= -1;

                // ...otherwise turn in the direction that's still pressed.
                else this.moveDirection = value.x > 0 ? -1 : 1;
            };

            // Stop moving when both keys unpressed.
            this.controls.Standard.Move.canceled += context => { this.moveDirection = 0; };
        }

        private void HandleKeyboardInput()
        {
            float zAngle = 0f;
            if (this.moveDirection != 0) zAngle = this.moveSpeedMultiplier * this.moveDirection * Time.deltaTime;
            else if (Mathf.Abs(this.rotationAmount) > this.deadZoneAngle)
            {
                zAngle = (this.rotationAmount > 0 ? 1f : -1f) * this.moveSpeedMultiplier * Time.deltaTime;
                this.rotationAmount -= zAngle;
            }

            this.transform.Rotate(0f, 0f, zAngle);
        }
#endif

        private void Update()
        {
#if PLATFORM_IOS || PLATFORM_ANDROID
            HandleTouchInput();
#endif
#if UNITY_EDITOR || UNITY_STANDALONE
            HandleKeyboardInput();
#endif
        }

        private void OnEnable()
        {
            this.controls.Enable();
        }

        private void OnDisable()
        {
            this.controls.Disable();
        }
    }
}