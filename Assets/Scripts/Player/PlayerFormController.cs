using UnityEngine;

/// <summary>
/// Manages the visual forms of the player (child GameObjects).
/// Swaps active form by enabling/disabling child GOs and updating the Animator reference.
/// Forms: 0=Human, 1=Bat, 2=Warg.
/// </summary>
public class PlayerFormController : MonoBehaviour
{
    [Header("Form GameObjects (children)")]
    [SerializeField] private GameObject formHuman;
    [SerializeField] private GameObject formBat;
    [SerializeField] private GameObject formWarg;

    [Header("Transformation Data (ScriptableObjects)")]
    [SerializeField] private TransformationData formDataBat;
    [SerializeField] private TransformationData formDataWarg;

    private int currentFormIndex;
    private GameObject[] formObjects;
    private TransformationData[] formsData;
    private Animator currentAnimator;

    public int CurrentFormIndex => currentFormIndex;
    public TransformationData CurrentFormData => GetFormData(currentFormIndex);
    public Animator CurrentAnimator => currentAnimator;

    private void Awake()
    {
        formObjects = new GameObject[3];
        formObjects[0] = formHuman;
        formObjects[1] = formBat;
        formObjects[2] = formWarg;

        formsData = new TransformationData[3];
        formsData[0] = null; // Human has no TransformationData (base form)
        formsData[1] = formDataBat;
        formsData[2] = formDataWarg;

        // Ensure only human is active at start
        ActivateForm(0);
    }

    /// <summary>
    /// Activates the form at the given index and deactivates all others.
    /// Index: 0=Human, 1=Bat, 2=Warg.
    /// </summary>
    public void ActivateForm(int index)
    {
        if (index < 0 || index >= formObjects.Length) return;

        for (int i = 0; i < formObjects.Length; i++)
        {
            if (formObjects[i] != null)
                formObjects[i].SetActive(i == index);
        }

        currentFormIndex = index;
        UpdateCurrentAnimator();
    }

    /// <summary>
    /// Deactivates all forms and activates the human form.
    /// </summary>
    public void DeactivateAllForms()
    {
        ActivateForm(0);
    }

    /// <summary>
    /// Returns the currently active form GameObject.
    /// </summary>
    public GameObject GetActiveForm()
    {
        if (currentFormIndex >= 0 && currentFormIndex < formObjects.Length)
            return formObjects[currentFormIndex];
        return formHuman;
    }

    /// <summary>
    /// Returns the TransformationData for the given form index.
    /// Returns null for Human (base form).
    /// </summary>
    public TransformationData GetFormData(int index)
    {
        if (index >= 0 && index < formsData.Length)
            return formsData[index];
        return null;
    }

    /// <summary>
    /// Maps form index to EnemyType.
    /// </summary>
    public EnemyType IndexToType(int index)
    {
        switch (index)
        {
            case 1: return EnemyType.Bat;
            case 2: return EnemyType.Orc; // Warg
            default: return EnemyType.None;
        }
    }

    private void UpdateCurrentAnimator()
    {
        GameObject active = GetActiveForm();
        if (active != null)
            currentAnimator = active.GetComponent<Animator>();
        else
            currentAnimator = null;
    }
}
