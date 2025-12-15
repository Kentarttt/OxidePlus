using CnControls;
using UltimateSurvival.InputSystem;
using UnityEngine;

namespace UltimateSurvival
{
    public class PlayerInputHandler : PlayerBehaviour
    {
        [SerializeField]
        private bool m_ShowControls;
        private InputManager m_Input;
        private bool attack;
        private bool aim;
        private bool run;
        private float sensitivity = 1f;
        private string playerPrefsKey = "MySliderValue";

        private void Awake()
        {
            if ((bool)GameController.InputManager) m_Input = GameController.InputManager;
            else base.enabled = false;
            base.Player.PlaceObject.AddListener(OnSucceded_PlaceObject);
            sensitivity = PlayerPrefs.GetFloat(playerPrefsKey + "Sensitivity", 1f);
        }

        private void Update()
        {
            float axisRaw = m_Input.GetAxisRaw("Rotate Object");
            if (Mathf.Abs(axisRaw) > 0f) base.Player.RotateObject.Try(axisRaw);

            if (MonoSingleton<InventoryController>.Instance.IsClosed && base.Player.ViewLocked.Is(false))
            {
                // Управление движением - поддержка всех платформ
                Vector2 movementInput = GetMovementInput();
                base.Player.MovementInput.Set(movementInput);

                // Управление камерой - поддержка всех платформ
                Vector2 lookInput = GetLookInput();
                base.Player.LookInput.Set(lookInput);

                // Обработка атаки и прицеливания
                if (attack)
                {
                    base.Player.AttackContinuously.Try();
                    if (aim) base.Player.Aim.TryStart();
                    else base.Player.Aim.ForceStop();
                }
                else if (!aim) base.Player.Aim.ForceStop();

                // Обработка бега
                if (run)
                {
                    bool flag = base.Player.IsGrounded.Get();
                    // Используем полученный ввод для бега
                    base.Player.MovementInput.Set(new Vector2(movementInput.x, 1f));
                    if (!base.Player.Run.Active && flag)
                        base.Player.Run.TryStart();
                }
                else if (base.Player.Run.Active) base.Player.Run.ForceStop();

                // Обработка ввода для ПК (клавиатура и мышь)
                HandlePCInput();
            }
            else
            {
                base.Player.MovementInput.Set(Vector2.zero);
                base.Player.LookInput.Set(Vector2.zero);
            }

            // Прокрутка колесика мыши
            float axis = m_Input.GetAxis("Mouse ScrollWheel");
            base.Player.ScrollValue.Set(axis);
        }

        private Vector2 GetMovementInput()
        {
            Vector2 input = Vector2.zero;

            // Мобильное управление
            if (Application.isMobilePlatform)
            {
                input = new Vector2(CnInputManager.GetAxis("Horizontal"), CnInputManager.GetAxis("Vertical"));
            }
            // Управление для ПК
            else
            {
                input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            }

            return input;
        }

        private Vector2 GetLookInput()
        {
            Vector2 input = Vector2.zero;

            // Мобильное управление
            if (Application.isMobilePlatform)
            {
                input = new Vector2(
                    CnInputManager.GetAxisRaw("Mouse X") * sensitivity,
                    CnInputManager.GetAxisRaw("Mouse Y") * sensitivity
                );
            }
            // Управление для ПК
            else
            {
                input = new Vector2(
                    Input.GetAxisRaw("Mouse X") * sensitivity,
                    Input.GetAxisRaw("Mouse Y") * sensitivity
                );
            }

            return input;
        }

        private void HandlePCInput()
        {
            // Только для ПК платформы
            if (!Application.isMobilePlatform)
            {
                // Бег по Shift
                if (Input.GetKeyDown(KeyCode.LeftShift))
                    RunAction(true);
                if (Input.GetKeyUp(KeyCode.LeftShift))
                    RunAction(false);

                // Прыжок по Space
                if (Input.GetKeyDown(KeyCode.Space))
                    JumpAction();

                // Приседание по Ctrl
                if (Input.GetKeyDown(KeyCode.LeftControl))
                    CrouchAction();

                // Прицеливание по правой кнопке мыши
                if (Input.GetMouseButtonDown(1))
                    AimAction();

                // Атака по левой кнопке мыши
                if (Input.GetMouseButtonDown(0))
                    AttackAction(true);
                if (Input.GetMouseButtonUp(0))
                    AttackAction(false);

                // Взаимодействие по E
                if (Input.GetKeyDown(KeyCode.E))
                    InteractAction(true);
                if (Input.GetKeyUp(KeyCode.E))
                    InteractAction(false);

                // Инвентарь по Tab
                if (Input.GetKeyDown(KeyCode.Tab))
                    InventoryAction();

                // Выход из режима строительства по Q
                if (Input.GetKeyDown(KeyCode.Q))
                    ExitBuildMode();
            }
        }

        private void OnSucceded_PlaceObject()
        {
            base.Player.CanShowObjectPreview.Set(false);
        }

        public void InventoryAction()
        {
            if (MonoSingleton<InventoryController>.Instance.IsClosed)
                MonoSingleton<InventoryController>.Instance.SetState.Try(ET.InventoryState.Normal);
            else
                MonoSingleton<InventoryController>.Instance.SetState.Try(ET.InventoryState.Closed);
        }

        public void InteractAction(bool hold)
        {
            if (hold) base.Player.InteractOnce.Try();
            base.Player.InteractContinuously.Set(hold);
        }

        public void JumpAction()
        {
            base.Player.Jump.TryStart();
        }

        public void RunAction(bool hold)
        {
            run = hold;
        }

        public void CrouchAction()
        {
            if (!base.Player.Crouch.Active)
                base.Player.Crouch.TryStart();
            else
                base.Player.Crouch.TryStop();
        }

        public void AttackAction(bool hold)
        {
            if (hold)
            {
                base.Player.AttackOnce.Try();
                if (base.Player.CanShowObjectPreview.Is(true))
                    base.Player.PlaceObject.Try();
                if (base.Player.SelectBuildable.TryStart())
                    return;
            }
            attack = hold;
        }

        public void PlaceObject()
        {
            if (base.Player.CanShowObjectPreview.Is(true))
                base.Player.PlaceObject.Try();
            base.Player.CanShowObjectPreview.Set(true);
        }

        public void ExitBuildMode()
        {
            if (base.Player.CanShowObjectPreview.Is(true))
                base.Player.CanShowObjectPreview.Set(false);
            base.Player.SelectBuildable.TryStart();
        }

        public void AimAction()
        {
            aim = !aim;

            if (aim)
                base.Player.Aim.TryStart();
            else
                base.Player.Aim.ForceStop();
        }

        // Новый метод для изменения чувствительности
        public void SetSensitivity(float newSensitivity)
        {
            sensitivity = newSensitivity;
            PlayerPrefs.SetFloat(playerPrefsKey + "Sensitivity", sensitivity);
            PlayerPrefs.Save();
        }
    }
}
