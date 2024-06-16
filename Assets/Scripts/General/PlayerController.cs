using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Gestiona las animaciones y movimiento del personaje jugable
public class PlayerController : MonoBehaviour
{
    Animator animator;
    CharacterController characterController;

    // Sistema inputs
    PlayerInput playerInput;
    bool moveKey;
    bool runKey;
    bool crouchKey;
    bool getPunchedKey;
    bool attackKey;
    bool talkKey;

    // Movimiento
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    [SerializeField] float walkMultiplier = 1.5f;
    [SerializeField] float runMultiplier = 3.0f;
    readonly float groundedGravity = -0.05f;
    readonly float gravity = -9.8f;

    // Rotacion
    Vector3 positionToLookAt;
    Quaternion currentRotation;
    Quaternion targetRotation;
    [SerializeField] float rotationFactorPerFrame;

    // Estados de animaciones
    bool isWalking;
    bool isRunning;
    bool isCrouched;
    bool getPunched;
    bool attack;
    bool talk;
    [SerializeField] AudioSource talking; // Audio

    private void Awake()
    {
        animator = GetComponent<Animator>();

        playerInput = new();
        characterController = GetComponent<CharacterController>();

        // Movimiento
        playerInput.CharacterControls.Move.started += OnMove;
        playerInput.CharacterControls.Move.performed += OnMove; // Valores medios
        playerInput.CharacterControls.Move.canceled += OnMove;

        // Correr
        playerInput.CharacterControls.Run.started += OnRun;
        playerInput.CharacterControls.Run.canceled += OnRun;

        // Agacharse
        playerInput.CharacterControls.Crouch.started += OnCrouch;
        playerInput.CharacterControls.Crouch.canceled += OnCrouch;

        // Recibe puñetazo
        playerInput.CharacterControls.GetPunched.started += OnGetPunch;
        playerInput.CharacterControls.GetPunched.canceled += OnGetPunch;

        // Ataca
        playerInput.CharacterControls.Attack.started += OnAttack;
        playerInput.CharacterControls.Attack.canceled += OnAttack;

        // Habla
        playerInput.CharacterControls.Talk.started += OnTalk;
        playerInput.CharacterControls.Talk.canceled += OnTalk;
    }

    void OnMove(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();

        // Anda
        currentMovement.x = currentMovementInput.x * walkMultiplier;
        currentMovement.z = currentMovementInput.y * walkMultiplier;

        // Corre
        currentRunMovement.x = currentMovementInput.x * runMultiplier;
        currentRunMovement.z = currentMovementInput.y * runMultiplier;

        moveKey = currentMovementInput.x != 0 || currentMovementInput.y != 0; // Se presiona alguna tecla
    }

    void OnRun(InputAction.CallbackContext context)
    {
        runKey = context.ReadValueAsButton();
    }

    void OnCrouch(InputAction.CallbackContext context)
    {
        crouchKey = context.ReadValueAsButton();
    }

    void OnGetPunch(InputAction.CallbackContext context)
    {
        getPunchedKey = context.ReadValueAsButton();
    }

    void OnAttack(InputAction.CallbackContext context)
    {
        attackKey = context.ReadValueAsButton();
    }

    void OnTalk(InputAction.CallbackContext context)
    {
        talkKey = context.ReadValueAsButton();
    }

    // Update is called once per frame
    void Update()
    {
        // Movimiento
        if (runKey) // Tecla de correr
        {
            // Corre
            characterController.Move(currentRunMovement * Time.deltaTime);
        }
        else
        {
            // Anda
            characterController.Move(currentMovement * Time.deltaTime);
        }
        

        // Rotación
        HandleRotation();

        // Gravedad
        HandleGravity();

        // Animaciones
        HandleAnimation();
    }

    private void HandleRotation()
    {
        // Cambio de posicion a la que debería apuntar según inputs
        positionToLookAt = new(currentMovement.x, 0.0f, currentMovement.z);

        // Rotación actual es la del objeto
        currentRotation = transform.rotation;

        // Si se pulsa una tecla de movimiento
        if (moveKey)
        {
            // Nueva rotación hacia donde apunta
            targetRotation = Quaternion.LookRotation(positionToLookAt);

            // Interpola las rotaciones
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
        }
            
    }

    // Gestiona el movimiento en Y con la gravedad
    private void HandleGravity()
    {
        // Si está en el suelo
        if (characterController.isGrounded)
        {
            currentMovement.y = groundedGravity;
            currentRunMovement.y = groundedGravity;
        }
        else // Sino
        {
            currentMovement.y += gravity;
            currentRunMovement.y += gravity;
        }
    }

    // Gestiona las animaciones
    private void HandleAnimation()
    {
        isWalking = animator.GetBool("isWalking");
        isRunning = animator.GetBool("isRunning");
        isCrouched = animator.GetBool("isCrouched");
        getPunched = animator.GetBool("getPunched");
        attack = animator.GetBool("attack");
        talk = animator.GetBool("talk");

        // Tecla de moverse presionada
        if (moveKey)
        {
            // No está andando
            if (!isWalking)
                animator.SetBool("isWalking", true); // Anda
            
            // Tecla de correr presionada y no está corriendo
            if (runKey && !isRunning)
                animator.SetBool("isRunning", true); // Corre
            
            // No presiona tecla de correr y está corriendo
            if (!runKey && isRunning)
                animator.SetBool("isRunning", false); // Deja de correr
            
            if (crouchKey && !isCrouched)
                animator.SetBool("isCrouched", true); // Se agacha
            
            // No presiona tecla de agacharse y está agachado
            if (!crouchKey && isCrouched)
                animator.SetBool("isCrouched", false); // Deja de agacharse
            
        }
        else // No se mueve
        {
            if (isWalking)
                animator.SetBool("isWalking", false);

            if (isRunning)
                animator.SetBool("isRunning", false);

            if (isCrouched)
                animator.SetBool("isCrouched", false);

            // Tecla de recibir puñetazo
            if (getPunchedKey)
            {
                // Si no está recibiendolo
                if (!getPunched)
                    animator.SetBool("getPunched", true); // Le dan un puñetazo
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("GetPunched"))
            {
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                    animator.SetBool("getPunched", false); // La animación de atacar ha terminado
            }

            // Tecla de atacar
            if (attackKey)
            {
                // Si no está atacando
                if (!attack)
                    animator.SetBool("attack", true); // Ataca
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            { 
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                    animator.SetBool("attack", false); // La animación de atacar ha terminado
            }

            // Tecla de hablar
            if (talkKey)
            {
                // Si no está hablando
                if (!talk)
                    talking.Play(); // Dice la frase
                    animator.SetBool("talk", true); // Habla
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Talk"))
            {
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                    animator.SetBool("talk", false); // La animación de hablar ha terminado
            }
        }
    }

    private void OnEnable()
    {
        // Activa el mapa de acciones del control del personaje
        playerInput.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        // Desactiva el mapa de acciones del control del personaje
        playerInput.CharacterControls.Disable();
    }
}
