using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Ce script gère le feedback visuel (Hover/Grab) sur les armes en utilisant MaterialPropertyBlock
// pour optimiser les performances GPU et éviter l'instanciation de matériaux.
// Il écoute les événements XR Interaction Toolkit et anime les propriétés du Shader.
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(XRGrabInteractable))]
public class WeaponFeedback : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Vitesse de transition de la couleur (Lerp)")]
    [SerializeField] private float transitionSpeed = 10f;

    // Références internes
    private Renderer _renderer;
    private XRGrabInteractable _interactable;
    private MaterialPropertyBlock _propBlock;

    // IDs des propriétés du Shader (pour la performance)
    private static readonly int HoverPropId = Shader.PropertyToID("_Hover");
    private static readonly int GrabPropId = Shader.PropertyToID("_Grab");

    // Valeurs cibles et actuelles pour l'animation
    private float _targetHover = 0f;
    private float _currentHover = 0f;
    private float _targetGrab = 0f;
    private float _currentGrab = 0f;

    // Initialisation des composants et du MaterialPropertyBlock
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _interactable = GetComponent<XRGrabInteractable>();
        _propBlock = new MaterialPropertyBlock();
    }

    // Abonnement aux événements XR Interactable lors de l'activation
    private void OnEnable()
    {
        if (_interactable != null)
        {
            _interactable.hoverEntered.AddListener(OnHoverEnter);
            _interactable.hoverExited.AddListener(OnHoverExit);
            _interactable.selectEntered.AddListener(OnSelectEnter);
            _interactable.selectExited.AddListener(OnSelectExit);
        }
    }

    // Désabonnement des événements XR Interactable pour éviter les fuites de mémoire
    private void OnDisable()
    {
        if (_interactable != null)
        {
            _interactable.hoverEntered.RemoveListener(OnHoverEnter);
            _interactable.hoverExited.RemoveListener(OnHoverExit);
            _interactable.selectEntered.RemoveListener(OnSelectEnter);
            _interactable.selectExited.RemoveListener(OnSelectExit);
        }
    }

    // Boucle principale pour animer les valeurs et mettre à jour le rendu via PropertyBlock
    private void Update()
    {
        // Vérification si une mise à jour visuelle est nécessaire
        bool isDirty = false;

        // Lissage de la valeur Hover
        if (!Mathf.Approximately(_currentHover, _targetHover))
        {
            _currentHover = Mathf.MoveTowards(_currentHover, _targetHover, Time.deltaTime * transitionSpeed);
            isDirty = true;
        }

        // Lissage de la valeur Grab
        if (!Mathf.Approximately(_currentGrab, _targetGrab))
        {
            _currentGrab = Mathf.MoveTowards(_currentGrab, _targetGrab, Time.deltaTime * transitionSpeed);
            isDirty = true;
        }

        // Application au GPU uniquement si les valeurs ont changé
        if (isDirty)
        {
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(HoverPropId, _currentHover);
            _propBlock.SetFloat(GrabPropId, _currentGrab);
            _renderer.SetPropertyBlock(_propBlock);
        }
    }

    // Callback déclenché quand la main survole l'objet
    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        _targetHover = 1f;
    }

    // Callback déclenché quand la main quitte l'objet
    private void OnHoverExit(HoverExitEventArgs args)
    {
        _targetHover = 0f;
    }

    // Callback déclenché quand l'objet est saisi (Grab)
    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        _targetGrab = 1f;
        // Optionnel : on peut forcer le hover à 0 si on ne veut pas cumuler les couleurs
        // _targetHover = 0f; 
    }

    // Callback déclenché quand l'objet est relâché
    private void OnSelectExit(SelectExitEventArgs args)
    {
        _targetGrab = 0f;
    }
}