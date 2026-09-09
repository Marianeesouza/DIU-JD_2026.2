using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class PlayerAnimatorGenerator
{
    [MenuItem("Tools/Generate Player Animator Controller")]
    public static void Generate()
    {
        string folder = "Assets/Animations";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "Animations");

        string path = $"{folder}/PlayerAnimator.controller";
        
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
        {
            if (!EditorUtility.DisplayDialog(
                "Sobrescrever Controller",
                "O PlayerAnimator.controller já existe. Deseja sobrescrevê-lo?",
                "Sim",
                "Não"))
            {
                return;
            }
        }
        
        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        // Parameters
        controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
        controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);

        var rootStateMachine = controller.layers[0].stateMachine;

        // --- Idle State ---
        var idleState = rootStateMachine.AddState("Idle", new Vector3(300, 0, 0));
        idleState.motion = CreateEmptyClip("Idle_Empty");

        // --- Walk State (placeholder - user converts to BlendTree) ---
        var walkState = rootStateMachine.AddState("Walk", new Vector3(600, 0, 0));
        walkState.motion = CreateEmptyClip("Walk_Empty");

        // --- Default State ---
        rootStateMachine.defaultState = idleState;

        // --- Transitions ---
        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.1f;

        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.1f;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Animator Controller criado em: {path}");
        Debug.Log("PRÓXIMO PASSO: Abra o controller no Animator, clique no estado Walk, ");
        Debug.Log("clique com botão direito > Create BlendTree, e configure como 2D Freeform Directional.");
    }

    private static AnimationClip CreateEmptyClip(string clipName)
    {
        var clip = new AnimationClip
        {
            name = clipName,
            frameRate = 60
        };
        return clip;
    }
}
